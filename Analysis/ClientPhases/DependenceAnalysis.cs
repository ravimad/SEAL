using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;
using SafetyAnalysis.Purity.Summaries;
using SafetyAnalysis.TypeUtil;

namespace SafetyAnalysis.Purity
{
    using depentry = Pair<AccessPathRegexp, AccessPathRegexp>;
    public class DependenceAnalysisPhase : Phx.Phases.Phase
    {                    

#if (PHX_DEBUG_SUPPORT)
        private static Phx.Controls.ComponentControl StaticAnalysisPhaseCtrl;
#endif
        private static string OutputSuffix = "-dependence-output.txt";
        private StreamWriter outputFileWriter;

        private DependenceAnalysisPhase()
        {            
        }
       
        public static void Initialize()
        {
#if PHX_DEBUG_SUPPORT
            DependenceAnalysisPhase.StaticAnalysisPhaseCtrl = 
                Phx.Controls.ComponentControl.New("DependenceAnalysis",
                "Perform a dependence analysis", "DependenceAnalysisPhase.cs");            
#endif
        }

        public static DependenceAnalysisPhase New(Phx.Phases.PhaseConfiguration config)
        {
            DependenceAnalysisPhase initPhase = new DependenceAnalysisPhase();
            initPhase.Initialize(config, "DependenceAnalyisPhase");
            return initPhase;
        }

        protected override void Execute(Phx.Unit unit)
        {            
            if (!unit.IsPEModuleUnit)
            {
                return;
            }
            Phx.PEModuleUnit moduleUnit = unit.AsPEModuleUnit;

            //initialize output
            var assemblyname = moduleUnit.Manifest.Name.NameString;
            var dirname = PurityAnalysisPhase.outputdir;
            outputFileWriter = new StreamWriter(new FileStream(dirname + assemblyname + OutputSuffix, 
                FileMode.Create, FileAccess.Write, FileShare.Read));

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Generating dependence analysis reports...");
            Console.ResetColor();

            var th = CombinedTypeHierarchy.GetInstance(moduleUnit);
            foreach (var childUnit in moduleUnit.ChildUnits)
            {
                if (childUnit.IsFunctionUnit)
                {
                    var funit = childUnit.AsFunctionUnit;

                    //get summary 
                    var summaryRecord = moduleUnit.CallGraph.SummaryManager.RetrieveSummary(
                        funit.FunctionSymbol.CallNode,
                        PurityAnalysisSummary.Type);
                    if (summaryRecord == null)
                        continue;
                    var data = (summaryRecord as PurityAnalysisSummary).PurityData.Copy();

                    //check if there is a need to perform Eager evaluation (i.e, if the method returns IEnumerable)
                    Phx.Types.AggregateType rettype;
                    if (PhxUtil.TryGetAggregateType(funit.FunctionSymbol.FunctionType.ReturnType,
                        out rettype))
                    {
                        rettype = PhxUtil.NormalizedAggregateType(rettype);                        

                        //check if the return type is an enumerable
                        if (PhxUtil.GetTypeName(rettype).Equals("[mscorlib]System.Collections.Generic.IEnumerable`1"))
                        {
                            data = EagerlyEvaluate(data, funit, moduleUnit);
                        }
                    }
                    this.outputFileWriter.WriteLine("Dependencies for Method: " +
                        PhxUtil.GetQualifiedFunctionName(funit.FunctionSymbol) + "\n");

                    var inLabels = LabelInputNodes(data);
                    var outLabels = LabelOutputNodes(data);
                    //DumpAllDependences(inLabels,outLabels,data);
                    DumpColumnDependencesOnly(inLabels, outLabels, data);
                }
            }
            
            outputFileWriter.Flush();
            outputFileWriter.Close();            
        }

        /// <summary>
        /// Eagerly evaluates an enumerator using a most general client.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private PurityAnalysisData EagerlyEvaluate(PurityAnalysisData data, 
            Phx.FunctionUnit funit, Phx.PEModuleUnit moduleUnit)
        {
            //read the return vertices (this corresponds to the arguments)
            var retEdges = data.OutHeapGraph.OutEdges(ReturnVertex.GetInstance());
            var retVertices = retEdges.Select((HeapEdgeBase e) => e.Target).ToList();

            //create a variable (correponding to the return value) and make it point to the 'retVertices'
            var argvar = VariableHeapVertex.New("EagerEval", 43, Context.EmptyContext);
            data.OutHeapGraph.AddVertex(argvar);
            var newedges = from retv in retVertices
                           select new InternalHeapEdge(argvar, retv, NullField.Instance);
            foreach (var e in newedges)
                data.OutHeapGraph.AddEdge(e);

            //remove all the out edges from return vertex
            data.OutHeapGraph.RemoveAllOutEdges(ReturnVertex.GetInstance());

            //Sythesise a call to the 'Rowset.Create' method with 'argvar' as argument.
            var typename = "[ScopeStubs]ScopeStubs.RowSet";
            var methodname = "Create";
            var methodsig = "([mscorlib]System.Collections.Generic.IEnumerable`1)[ScopeStubs]ScopeStubs.RowSet";
            var call = new StaticCall(37, methodname, typename, methodsig, new List<string>());
            call.AddParam(0, argvar);
            var callRetvar = VariableHeapVertex.New("EagerEval",47,Context.EmptyContext);
            call.AddReturnValue(callRetvar);
            
            //read callee summary
            var summary = (new CalleeSummaryReader(funit, moduleUnit)).GetCalleeData(call, data);
            if (summary == null)
                throw new NotSupportedException("Callee summary not found: " + typename + "::" + methodname + "/" + methodsig);           

            //compose the callee summary
            var builder = new HigherOrderHeapGraphBuilder(funit);
            builder.ComposeCalleeSummary(call,data,summary);
            builder.ComposeResolvableSkippedCalls(data);

            //make the return variable point to targets of 'callretvar'
            var callRetVertices = data.OutHeapGraph.OutEdges(callRetvar).Select((HeapEdgeBase e)=> e.Target).ToList();
            foreach (var callRetV in callRetVertices)
                data.OutHeapGraph.AddEdge(new InternalHeapEdge(ReturnVertex.GetInstance(), callRetV, NullField.Instance));

            return data;
        }
        
        //inputs are nodes that are reachable only via external edges (prestate objects).
        private ExtendedMap<HeapVertexBase, AccessPathRegexp>
            LabelInputNodes(PurityAnalysisData data)
        {
            var labelMap = new ExtendedMap<HeapVertexBase, AccessPathRegexp>();
            var pathFinder  = new AcyclicPathFinder(data,
                (HeapEdgeBase edge) => (edge is ExternalHeapEdge),
                (HeapVertexBase v, AccessPathRegexp ap) => {
                    labelMap.Add(v, ap);
                    return true;
                }
            );                    
            
            //create a set of roots, ap pairs
            var roots = from v in data.OutHeapGraph.Vertices
                        where (v is ParameterHeapVertex) || (v is GlobalLoadVertex)
                        select v;                
            pathFinder.FindPaths(roots);                      
            return labelMap;                
        }

        //outputs are nodes that are reachable in the output graph        
        private ExtendedMap<HeapVertexBase, AccessPathRegexp>
            LabelOutputNodes(PurityAnalysisData data)
        {
            var labelMap = new ExtendedMap<HeapVertexBase, AccessPathRegexp>();
            var pathFinder = new AcyclicPathFinder(data,
                (HeapEdgeBase edge) => (true),
                (HeapVertexBase v, AccessPathRegexp ap) =>
                {
                    labelMap.Add(v, ap);
                    return true;
                }
            );
            var roots = from v in data.OutHeapGraph.Vertices
                         where ((v is ParameterHeapVertex)
                              || (v is GlobalLoadVertex)
                              || (v is ReturnVertex)
                              || (v is ExceptionVertex)
                              )
                         select v;
            pathFinder.FindPaths(roots);            
            return labelMap;                
        }        
                  
        private void DumpAllDependences(ExtendedMap<HeapVertexBase, AccessPathRegexp> inputs,
            ExtendedMap<HeapVertexBase, AccessPathRegexp> outputs,
            PurityAnalysisData data)
        {
            foreach (var intedge in data.OutHeapGraph.Edges.OfType<InternalHeapEdge>())
            {
                if (inputs.ContainsKey(intedge.Target))
                {                    
                    if (outputs.ContainsKey(intedge.Source))
                    {
                        foreach (var srcap in outputs[intedge.Source])
                        {
                            AccessPathRegexp newap = null;
                            if (intedge.Source is ReturnVertex
                                || intedge.Source is ExceptionVertex
                                || intedge.Source is VariableHeapVertex)
                            {
                                newap = srcap;
                            }
                            else
                            {
                                //append edges field to the source ap
                                newap = srcap.Clone();
                                newap.AppendField(intedge.Field);
                            }                         
   
                            this.outputFileWriter.WriteLine(newap + "---->");
                            foreach (var tgtap in inputs[intedge.Target])
                            {
                                this.outputFileWriter.WriteLine("\t" + tgtap.ToString());
                            }
                        }
                    }
                }
            }
        }
        
        private void DumpColumnDependencesOnly(ExtendedMap<HeapVertexBase, AccessPathRegexp> inputs,
            ExtendedMap<HeapVertexBase, AccessPathRegexp> outputs,
            PurityAnalysisData data)
        {
            var depset = new HashSet<depentry>();
            foreach (var intedge in data.OutHeapGraph.Edges.OfType<InternalHeapEdge>())
            {
                if (inputs.ContainsKey(intedge.Target))
                {
                    if (outputs.ContainsKey(intedge.Source))
                    {
                        foreach (var srcap in outputs[intedge.Source])
                        {
                            AccessPathRegexp newap = null;
                            if (intedge.Source is ReturnVertex
                                || intedge.Source is ExceptionVertex
                                || intedge.Source is VariableHeapVertex)
                            {
                                newap = srcap;
                            }
                            else
                            {
                                //append edges field to the source ap
                                newap = srcap.Clone();
                                newap.AppendField(intedge.Field);
                            }                            

                            var srcCol = GetColumnIfAny(newap);
                            if (srcCol == null)
                                continue;
                            foreach (var tgtap in inputs[intedge.Target])
                            {
                                var tgtCol = GetColumnIfAny(tgtap);
                                if (tgtCol == null)
                                    continue;

                                if (!depset.Contains(new depentry(srcCol, tgtCol)))
                                {
                                    depset.Add(new depentry(srcCol, tgtCol));
                                    this.outputFileWriter.WriteLine(srcCol + "---->");
                                    this.outputFileWriter.WriteLine("\t" + tgtCol);
                                }
                            }
                        }
                    }
                }
            }
        }

        private AccessPathRegexp GetColumnIfAny(AccessPathRegexp ap)
        {
            var newap = new AccessPathRegexp(ap.root,null);
            if (ap.fields.Any())
            {
                foreach (var col in ap.fields)
                {
                    newap.AppendField(col);
                    if (col is NamedField)
                    {                        
                        var colname = (col as NamedField).Name;                        
                        if (StringConstantVertex.ExistsStrConst(colname))
                        {                            
                            return newap;
                        }
                    }
                }
                return null;
            }
            return newap;
        }       
    }

    class AcyclicPathFinder
    {
        System.Func<HeapVertexBase, AccessPathRegexp, bool> vertexAction;        
        System.Func<HeapEdgeBase, bool> edgeFilter;
        PurityAnalysisData data;
        ExtendedMap<HeapVertexBase, Call> recvrMap = new ExtendedMap<HeapVertexBase, Call>();
        
        public AcyclicPathFinder(PurityAnalysisData d, System.Func<HeapEdgeBase, bool> ef,
            System.Func<HeapVertexBase, AccessPathRegexp, bool> va)
        {
            this.vertexAction = va;
            this.edgeFilter = ef;
            data = d;
            InitSkCalls();
        }

        public void InitSkCalls()
        {
            //compute a map from receiver vertices to skipped calls
            foreach (var skcall in data.SkippedCalls)
            {
                if (skcall.GetReturnValue() == null)
                    continue;                
                IEnumerable<HeapVertexBase> targets = null;
                if (skcall is VirtualCall)
                {
                    var vcall = skcall as VirtualCall;
                    targets = AnalysisUtil.GetReceiverVertices(vcall, data);                    
                }
                else if (skcall is DelegateCall)
                {
                    targets = from e in data.OutHeapGraph.OutEdges((skcall as DelegateCall).GetTarget())
                              select e.Target;                    
                }
                foreach (var t in targets)
                {
                    recvrMap.Add(t, skcall);
                }
            }
        }

        public void FindPaths(IEnumerable<HeapVertexBase> roots)
        {
            var stk = new Stack<HeapVertexBase>();                       
            foreach (var root in roots)
            {
                var ap = new AccessPathRegexp(root, null);
                stk.Push(root);
                DoDFS(root, stk, ap);
                stk.Pop();                             
            }
        }

        private void DoDFS(HeapVertexBase v, Stack<HeapVertexBase> stk, AccessPathRegexp ap)
        {            
            vertexAction(v,ap);

            //find successors (and their access-paths) and invoke  DoDFS on them recursively
            var succs = new List<Object[]>();                    
            if (recvrMap.ContainsKey(v))
            {
                foreach (var skcall in recvrMap[v])
                {
                    //return vertices are successors v is the receiver of skcall
                    var retVertices = from e in data.OutHeapGraph.OutEdges(skcall.GetReturnValue())
                                      where e.Target is ReturnedValueVertex
                                      select e.Target;
                    if (!retVertices.Any())
                        continue;

                    string fieldname = null;
                    if (skcall is VirtualCall)
                        fieldname = (skcall as VirtualCall).GetMethodName() + "()";
                    else if (skcall is DelegateCall)
                        fieldname = "()";

                    //create a dummy named field representing the skcall
                    var mfield = NamedField.New(fieldname, null);
                    var newap = ap.Clone();
                    newap.AppendField(mfield);

                    //add the  returned vertex and the accesspath to the succs list
                    foreach (var retv in retVertices)
                        succs.Add(new Object[] { retv, newap });
                }
            }
            foreach (var edge in data.OutHeapGraph.OutEdges(v).Where(edgeFilter))
            {
                var newap = ap.Clone();
                newap.AppendField(edge.Field);
                succs.Add(new Object[] { edge.Target, newap });
            }

            foreach(var succ in succs)
            {
                var succv = succ[0] as HeapVertexBase;
                var succap = succ[1] as AccessPathRegexp;
                if(!stk.Contains(succv))
                {   
                    stk.Push(succv);
                    DoDFS(succv, stk, succap);
                    stk.Pop();
                }
            }
        }
    }
}
