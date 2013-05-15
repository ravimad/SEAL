using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.TypeUtil;
using SafetyAnalysis.Purity.Summaries;
using SafetyAnalysis.Framework.Graphs;

namespace SafetyAnalysis.Purity
{
    class FlowInsensitivePurityAnalysis
    {
        private static PredicatedHandlerProvider<Phx.FunctionUnit, System.Func<Phx.FunctionUnit, PurityAnalysisData,bool>> MethodHandlers = null;
        private static PurityAnalysisTransformers _transformers = new PurityAnalysisTransformers();

        static FlowInsensitivePurityAnalysis()
        {
            MethodHandlers = new PredicatedHandlerProvider<Phx.FunctionUnit, Func<Phx.FunctionUnit, PurityAnalysisData, bool>>();
            //MethodHandlers.RegisterHandler(IsMainMethod, MainMethodHandler);
            MethodHandlers.RegisterHandler(IsDefaultEventMethod, EventMethodHandler);                
        }  

        public PurityAnalysisData Compute(Phx.FunctionUnit functionUnit)
        {
            PurityAnalysisData newData = new PurityAnalysisData(new HeapGraph());

            System.Func<Phx.FunctionUnit,PurityAnalysisData, bool> methodHandler;
            if (MethodHandlers.TryGetHandler(functionUnit, out methodHandler))
            {
                methodHandler(functionUnit, newData);
            }
            else
            {
                InstructionFixPointComputation(functionUnit, newData);
            }            
            return newData;
        }     
   
        public static bool IsDefaultEventMethod(Phx.FunctionUnit funit)
        {
            var funname = PhxUtil.GetFunctionName(funit.FunctionSymbol);
            if (funname.StartsWith("add_")
                || funname.StartsWith("remove_"))
            {
                var linenumber = funit.DebugInfo.GetLineNumber(funit.FirstEnterInstruction.DebugTag);
                if (linenumber == 0)
                {
                    //compiler generated
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Caution: lots of handcrafting and magic numbers
        /// </summary>
        /// <param name="functionUnit"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool EventMethodHandler(Phx.FunctionUnit functionUnit, PurityAnalysisData data)
        {                                   
            var funname = PhxUtil.GetFunctionName(functionUnit.FunctionSymbol);
            var enclTypename = PhxUtil.GetTypeName(functionUnit.FunctionSymbol.EnclosingAggregateType);

            if (funname.StartsWith("add_"))
            {
                var fieldname = funname.Substring(4);
                //var transformer = new PurityAnalysisTransformers();
                _transformers.Apply(functionUnit.FirstEnterInstruction, data);

                if (functionUnit.FunctionSymbol.IsInstanceMethod)
                {
                    //make the this.fieldname point-to value parameter (2nd parameter)
                    HeapVertexBase secondParam = null, thisParam = null;
                    foreach (var v in data.OutHeapGraph.Vertices.OfType<ParameterHeapVertex>())
                    {
                        if (v.Index == 2 && v.name.Equals("value"))
                            secondParam = v;
                        else if (v.Index == 1 && v.name.Equals("this"))
                            thisParam = v;
                    }
                    var edge = new InternalHeapEdge(thisParam, secondParam, NamedField.New(fieldname, enclTypename));
                    if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                        data.OutHeapGraph.AddEdge(edge);
                }
                else
                {
                    //make the globalobject.fieldname point-to value parameter (2nd parameter)
                    HeapVertexBase firstParam = null;
                    foreach (var v in data.OutHeapGraph.Vertices.OfType<ParameterHeapVertex>())
                    {                        
                        if (v.Index == 1 && v.name.Equals("value"))
                            firstParam = v;
                    }
                    var edge = new InternalHeapEdge(GlobalLoadVertex.GetInstance(), firstParam, NamedField.New(fieldname, enclTypename));
                    if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                        data.OutHeapGraph.AddEdge(edge);
                }
            }
            else
            {
                //handle remove_ here
                //add Global load vertex, return and exception vertices to the data
                data.OutHeapGraph.AddVertex(GlobalLoadVertex.GetInstance());
                data.OutHeapGraph.AddVertex(ExceptionVertex.GetInstance());
                data.OutHeapGraph.AddVertex(ReturnVertex.GetInstance());

            }
            return true;
        }

        public static bool IsMainMethod(Phx.FunctionUnit funit)
        {
            return funit.FunctionSymbol.NameString.Equals("Main");
        }

        public static bool MainMethodHandler(Phx.FunctionUnit functionUnit, PurityAnalysisData data)
        {            
            //invoke the fixpoint computation
            InstructionFixPointComputation(functionUnit,data);

            PurityAnalysisData oldData;
            do
            {

                oldData = data;
                data.CopyInto(MergeStaticConstructorSummaries(functionUnit, oldData));

                var builder = new HigherOrderHeapGraphBuilder(functionUnit);
                builder.ComposeResolvableSkippedCalls(data);
                data.skippedCallTargets.UnionWith(builder.GetMergedTargets());

            } while ((oldData.OutHeapGraph.VertexCount != data.OutHeapGraph.VertexCount)
            || (oldData.OutHeapGraph.EdgeCount != data.OutHeapGraph.EdgeCount)
            || (oldData.SkippedCalls.Count() != data.SkippedCalls.Count()));
           
            return true;
        }

        /// <summary>
        /// Ugly: Lost of hand crafting.
        /// </summary>
        /// <param name="functionUnit"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static PurityAnalysisData MergeStaticConstructorSummaries(Phx.FunctionUnit functionUnit, PurityAnalysisData data)
        {
            //now create a new data
            //add a global load vertex, exception vertex, return vertex to the data
            var callerData = new PurityAnalysisData(new HeapGraph());
            callerData.OutHeapGraph.AddVertex(GlobalLoadVertex.GetInstance());
            callerData.OutHeapGraph.AddVertex(ExceptionVertex.GetInstance());
            callerData.OutHeapGraph.AddVertex(ReturnVertex.GetInstance());

            //create parameter vertices (which are parameters of main) in the callerData
            var parameterVertices = from paramv in data.OutHeapGraph.Vertices.OfType<ParameterHeapVertex>()
                                    select paramv;
            var paramCount = parameterVertices.Count();
            var paramVars = new VariableHeapVertex[paramCount];
            foreach (var paramv in parameterVertices)
            {
                //create new variable vetex
                var varVertex = NodeEquivalenceRelation.CreateVariableHeapVertex("DummyRoot", paramv.Index, Context.EmptyContext);

                //add a edge from varVertex to paramv
                //reusing the parameter vertes (assumption: paramname is not used in equality comparison)
                if (!callerData.OutHeapGraph.ContainsVertex(paramv))
                    callerData.OutHeapGraph.AddVertex(paramv);
                if (!callerData.OutHeapGraph.ContainsVertex(varVertex))
                    callerData.OutHeapGraph.AddVertex(varVertex);
                var edge = new InternalHeapEdge(varVertex, paramv, null);
                if (!callerData.OutHeapGraph.ContainsHeapEdge(edge))
                    callerData.OutHeapGraph.AddEdge(edge);

                paramVars[paramv.Index - 1] = varVertex;
            }


            //find all the static fields referenced.
            var staticTypes = from edge in data.OutHeapGraph.OutEdges(GlobalLoadVertex.GetInstance())
                              where !String.IsNullOrEmpty(edge.Field.EnclosingTypename)
                              select edge.Field.EnclosingTypename;

            ////call the static constructor of all the referred static types
            var moduleunit = functionUnit.ParentPEModuleUnit;
            var th = CombinedTypeHierarchy.GetInstance(moduleunit);

            foreach (var typename in staticTypes)
            {
                var typeinfo = th.LookupTypeInfo(typename);

                //synthesise an artificial call to the static constructor                       
                var methodname = "cctor";
                var methodsig = "()void";
                var call = new StaticCall(23, methodname, typename, methodsig, new List<string>());
                var summary = (new CalleeSummaryReader(functionUnit, moduleunit)).GetCalleeData(call, callerData);
                if (summary == null)
                    continue;
                callerData.Union(summary);
                //(new HigherOrderHeapGraphBuilder(functionUnit)).ComposeCalleeSummary(call, callerData, summary);
            }

            //synthesise a dummy call instruction to main
            var mainname = PhxUtil.GetFunctionName(functionUnit.FunctionSymbol);
            var mainsig = PhxUtil.GetFunctionTypeSignature(functionUnit.FunctionSymbol.FunctionType);
            var containingType = PhxUtil.GetTypeName(functionUnit.FunctionSymbol.EnclosingAggregateType);
            var maincall = new StaticCall(29, mainname, containingType, mainsig, new List<string>());
            //add parameters to main call
            for (int i = 0; i < paramVars.Length; i++)
            {
                maincall.AddParam(i, paramVars[i]);
            }

            (new HigherOrderHeapGraphBuilder(functionUnit)).ComposeCalleeSummary(maincall, callerData, data);
            return callerData;
        }

        public static void InstructionFixPointComputation(Phx.FunctionUnit functionUnit, PurityAnalysisData initData)
        {
            Phx.Graphs.NodeFlowOrder nodeOrder = Phx.Graphs.NodeFlowOrder.New(functionUnit.Lifetime);            
            nodeOrder.Build(functionUnit.FlowGraph, Phx.Graphs.Order.PreOrder);
                       
            var newData = initData;
            int oldVertexCount;
            int oldEdgeCount;
            int oldSkippedCallCount;
            int oldWriteSetCount;
            do
            {
                //initialize old state
                oldVertexCount = newData.OutHeapGraph.VertexCount;
                oldEdgeCount = newData.OutHeapGraph.EdgeCount;
                oldSkippedCallCount = newData.SkippedCalls.Count();
                oldWriteSetCount = newData.MayWriteSet.GetCount();

                //traverse the function once.
                for (uint i = 1; i <= nodeOrder.NodeCount; i++)
                {
                    Phx.Graphs.BasicBlock block = nodeOrder.Node(i).AsBasicBlock;
                    foreach (Phx.IR.Instruction instruction in block.Instructions)
                    {
                        //Console.WriteLine("Processing Instruction: " + instruction);
                        //PhxUtil.GetQualifiedFunctionName(functionUnit.FunctionSymbol).Equals(
                        //    "[TestPrimitiveValues]TestPrimitiveValues.TestPrimitiveValues::ResBug/(mp32->f64,mp32->f64)void")
                        //if (functionUnit.FunctionSymbol.QualifiedName.Equals("System.Linq.OrderedEnumerable`1::<GetEnumerator>d__0::MoveNext"))
                        //{
                        //    if (instruction.IsCallInstruction)                                
                        //    Console.WriteLine("Processing Instruction: " + instruction
                        //        + " Id: " + instruction.InstructionId);
                        //    else
                        //        Console.WriteLine("Processing Instruction: " + instruction
                        //        + " Id: " + instruction.InstructionId);                            
                        //}
                        _transformers.Apply(instruction, newData);
                        //if (functionUnit.FunctionSymbol.QualifiedName.Equals("System.Linq.OrderedEnumerable`1::<GetEnumerator>d__0::MoveNext"))
                        //{
                        //    //PurityDataUtil.DumpAsDGML(newData, null, null);
                        //    //Console.WriteLine("[{0},{1},{2},{3}]",
                        //    //    newData.OutHeapGraph.VertexCount,
                        //    //    newData.OutHeapGraph.EdgeCount,
                        //    //    newData.SkippedCalls.Count(),
                        //    //    newData.MayWriteSet.GetCount());
                        //    if (!instruction.IsCallInstruction)
                        //        continue;
                        //    string input = Console.ReadLine();
                        //    if (input.Equals("Dumpgraph"))
                        //    {
                        //        newData.Dump();
                        //        PurityDataUtil.DumpAsDGML(newData,"temp.dgml",null);
                        //        string msg = Console.ReadLine();
                        //        if (msg.Equals("dumpskinfo"))
                        //        {
                        //            foreach (var call in newData.SkippedCalls)
                        //            {
                        //                Console.WriteLine("Skipped call: " + call);
                        //                Console.WriteLine("Params: ");
                        //                foreach (var param in call.GetAllParams())
                        //                {
                        //                    Console.WriteLine("\t" + param);
                        //                }
                        //            }
                        //        }
                        //    }
                        //    //var witnesses = Util.IsPure(newData,
                        //    //    PhxUtil.IsConstructor(functionUnit));
                        //    //Console.WriteLine("IsPure: " + !witnesses.Any());
                        //    //if (witnesses.Any())
                        //    //{
                        //    //    foreach (var witness in witnesses)
                        //    //    {
                        //    //        var puritywitness = new PurityWitness(witness);
                        //    //        Console.WriteLine(puritywitness.ToAccessPath());
                        //    //    }
                        //    //}
                        //}                        
                    }                       
                }                
            }while ( (newData.OutHeapGraph.VertexCount != oldVertexCount)               
                || (newData.OutHeapGraph.EdgeCount != oldEdgeCount)
                || (newData.SkippedCalls.Count() != oldSkippedCallCount)
                || (newData.MayWriteSet.GetCount() != oldWriteSetCount));
        }
    }
}
