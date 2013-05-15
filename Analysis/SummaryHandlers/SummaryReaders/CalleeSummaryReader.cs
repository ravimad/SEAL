using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;

using SafetyAnalysis.Util;
using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity;
using SafetyAnalysis.TypeUtil;
using System.IO;
using System.Xml;

namespace SafetyAnalysis.Purity.Summaries
{
    public class CalleeSummaryReader
    {
        private Phx.FunctionUnit callerUnit;
        private Phx.PEModuleUnit moduleUnit;
        private ExtendedMap<Call,string> processedCallTargets = new ExtendedMap<Call,string>();

        public CalleeSummaryReader(Phx.FunctionUnit cunit, Phx.PEModuleUnit munit)
        {
            callerUnit = cunit;

            if (munit == null)
                throw new NotSupportedException("module unit null");
            moduleUnit = munit;
        }

        public ExtendedMap<Call, string> GetFoundTargets()
        {
            return processedCallTargets;
        }

        /// <summary>        
        /// Determines if any of the targets of the call is resolvable if yes returns its summary. 
        /// Ignores calls that have already been resolved (in the case of skipped calls)
        /// a virtual call can be resolved in 3 ways 
        /// (a) using stubs (that is applicable to all overriden implementations)
        /// (b) using the type hierarchy, assuming it is complete
        /// (c) lazily by storing it in the skipped calls list
        /// This code checks how to resolve a virtual call and suitably constructs its summary
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="callerData"></param>
        /// <returns></returns>
        public PurityAnalysisData
            GetCalleeData(            
            Call call,
            PurityAnalysisData callerData)
        {              
            var calltype = CallUtil.GetCallType(call, callerData);
            if (calltype.stubbed == true)
            {
                CallStubManager summan;
                if (CallStubManager.TryGetCallStubManager(call, out summan))
                {
                    var th = CombinedTypeHierarchy.GetInstance(moduleUnit);
                    return summan.GetSummary(call, callerData, th);
                }
                else
                    throw new ArgumentException("Cannot find stub managaer for call: " + call);
            }

            List<PurityAnalysisData> calleeSummaries = null;
            if(call is StaticCall)
            {
                calleeSummaries = GetTargetSummaries(call as StaticCall);                
            }
            else if (call is VirtualCall)
            {
                if (calltype.typehierarchy == true)
                    calleeSummaries = GetTargetSummariesUsingTypeHierarchy(call as VirtualCall, callerData);
                else
                    calleeSummaries = GetTargetSummaries(call as VirtualCall, callerData);
            }            
            else if (call is DelegateCall)
            {
                calleeSummaries = GetTargetSummaries(call as DelegateCall, callerData);
            }
            else
                throw new NotSupportedException("Unknown call type encountered in InternalSummaryManager");                    

            if (!calleeSummaries.Any())
            {
                //better to drop this call (this is very likely infeasible)
                return null;                       
            }

            //collapse all data
            return AnalysisUtil.CollapsePurityData(calleeSummaries);            
        }

        public List<PurityAnalysisData> GetTargetSummaries(StaticCall scall)
        {
            var decTypename = scall.GetDeclaringType();
            var methodname = scall.GetMethodName();
            var sig = scall.GetSignature();
            var qualifiedName = decTypename + "::" + methodname + "/" + sig;
            var summaries = new List<PurityAnalysisData>();

            if (PurityAnalysisPhase.TraceSummaryApplication)
                Trace.TraceInformation("Getting summary for static call : " + qualifiedName);

            //update the wholeprogram call graph
            if (this.callerUnit != null)
            {
                wholecg.AddEdge(PhxUtil.GetQualifiedFunctionName(this.callerUnit.FunctionSymbol), qualifiedName);
            }

            var typeinfo = CombinedTypeHierarchy.GetInstance(moduleUnit).LookupTypeInfo(decTypename);
            //if (typeinfo == null)
            //{
            //    //type hierarhcy is not downward closed here.
            //    //log error msg
            //    Trace.TraceWarning("Cannot find the receiver type of " + qualifiedName);
            //    MethodLevelAnalysis.unknownTargetCalls.Add(qualifiedName);           
  
            //    //use unanalyzable call summary here
            //    var calleeSum = SummaryTemplates.GetUnanalyzableCallSummary(methodname, decTypename);
            //    summaries.Add(calleeSum);
            //    return summaries;
            //}

            var methodInfos = typeinfo.GetMethodInfos(methodname, sig);
            foreach (var methodinfo in methodInfos)
            {
                //on the fly call graph construction
                //add edges from caller function to the callee node
                if (methodinfo is InternalMethodInfo && this.callerUnit != null)
                {
                    var callGraph = this.callerUnit.ParentPEModuleUnit.CallGraph;
                    var callerNode = this.callerUnit.FunctionSymbol.CallNode;
                    var calleeNode = (methodinfo as InternalMethodInfo).GetFunctionSymbol().CallNode;
                    if (callGraph.FindCallEdge(callerNode, calleeNode) == null)
                    {
                        callGraph.CreateUniqueCallEdge(callerNode, calleeNode.FunctionSymbol);
                    }
                }                

                var calleeSum = methodinfo.GetSummary();
                if (calleeSum == null)
                {
                    //callees are not downward closed here and we do not have stubs
                    //log error msg
                    Trace.TraceWarning("Cannot find the summary for: " + qualifiedName);
                    MethodLevelAnalysis.unknownTargetCalls.Add(qualifiedName);

                    //use unanalyzable call summary here
                    var calleeData = SummaryTemplates.GetUnanalyzableCallSummary(methodname, decTypename);
                    summaries.Add(calleeData);
                    continue;
                }

                summaries.Add(calleeSum);                
            }            
            //if (!summaries.Any())
            //{                
            //    //type hierarhcy is not downward closed here.
            //    //log error msg
            //    Trace.TraceWarning("Cannot find the receiver type of " + qualifiedName);
            //    MethodLevelAnalysis.unknownTargetCalls.Add(qualifiedName);

            //    //use unanalyzable call summary here
            //    var calleeData = SummaryTemplates.GetUnanalyzableCallSummary(methodname, decTypename);
            //    summaries.Add(calleeData);
            //}
            return summaries;
        }        
        
        public List<PurityAnalysisData> GetTargetSummaries(            
            DelegateCall dcall,
            PurityAnalysisData callerData)
        {
            var calleeSummaries = new List<PurityAnalysisData>();

            var targetVertices = AnalysisUtil.GetTargetVertices(dcall, callerData);
            var targetMethods = AnalysisUtil.GetMethodVertices(callerData, targetVertices);
            IEnumerable<string> recvrTypenames = null;

            if (targetMethods.Any())
            {
                foreach (var m in targetMethods)
                {
                    if (m.IsVirtual)
                    {
                        if (recvrTypenames == null)
                        {
                            var recvrVertices = AnalysisUtil.GetReceiverVertices(callerData, targetVertices).OfType<InternalHeapVertex>();
                            recvrTypenames = from r in recvrVertices
                                             from typeName in callerData.GetTypes(r)
                                             select typeName;
                        }
                        foreach (var typename in recvrTypenames)
                        {
                            ResolveIfNotProcessed(callerData, dcall, typename, m.methodname, m.signature, calleeSummaries);
                        }
                    }
                    else
                    {
                        ResolveIfNotProcessed(callerData, dcall, m.typename, m.methodname, m.signature, calleeSummaries);
                    }
                }
            }
            else
            {
                throw new NotImplementedException("No targets for delegate call: " + dcall);                
            }
            return calleeSummaries;
        }               

        private List<PurityAnalysisData> GetTargetSummaries(
            VirtualCall vcall,
            PurityAnalysisData callerData)
        {
            var summaries = new List<PurityAnalysisData>();
            //try getting the receiver vertices
            IEnumerable<HeapVertexBase> receiverVertices = null;
            receiverVertices = AnalysisUtil.GetReceiverVertices(vcall, callerData);

            //check if this is a resolvable call.
            if (receiverVertices != null &&
                receiverVertices.Any((HeapVertexBase v) => (v is InternalHeapVertex)))
            {
                //known receiver types
                IEnumerable<string> concreteTypenames =
                    from r in receiverVertices.OfType<InternalHeapVertex>()
                    from typeName in callerData.GetTypes(r as InternalHeapVertex)
                    select typeName;

                string methodname = vcall.GetMethodName();
                string sig = vcall.GetSignature();

                foreach (var typename in concreteTypenames)
                {
                    ResolveIfNotProcessed(callerData, vcall, typename, methodname, sig, summaries);
                }
                return summaries;
            }
            else
                throw new NotSupportedException(
                    "Cannot resolve virtual call: " + vcall + " no internal receiver vertices");           
        }

        private List<PurityAnalysisData> GetTargetSummariesUsingTypeHierarchy(VirtualCall vcall, 
            PurityAnalysisData callerData)
        {
            var summaries = new List<PurityAnalysisData>();

            var th = CombinedTypeHierarchy.GetInstance(moduleUnit);
            var methodinfos = new HashSet<TypeUtil.MethodInfo>();

            //get the names of all the subtypes and supertypes
            //handle virtual calls here                                
            var decTypeinfo = th.LookupTypeInfo(vcall.declaringtype);

            //find all the inherited methods with the same signature                                            
            var inheritedMethods = th.GetInheritedMethods(decTypeinfo, vcall.methodname, vcall.signature);
            methodinfos.UnionWith(inheritedMethods);

            //find all the sub-types of the receiver type.                                                     
            var subTypeMethods = new List<TypeUtil.MethodInfo>();
            foreach (var subtypeinfo in th.GetSubTypesFromTypeHierarchy(decTypeinfo))
            {
                subTypeMethods.AddRange(subtypeinfo.GetMethodInfos(vcall.methodname, vcall.signature));
            }
            methodinfos.UnionWith(subTypeMethods);
            
            foreach (var minfo in methodinfos)
            {                
                var summary = minfo.GetSummary();
                if (summary != null)
                {                    
                    summaries.Add(summary);
                }
            }
            return summaries;
        }

        private void ResolveIfNotProcessed(
            PurityAnalysisData callerData,
            Call call,
            string typename,
            string methodname,
            string sig,
            List<PurityAnalysisData> calleeSummaries)
        {            
            string qualifiedName = typename + "::" + methodname + "/" + sig;
            if (callerData.GetProcessedTargets(call).Contains(qualifiedName))
                return;

            processedCallTargets.Add(call, qualifiedName);

            if (PurityAnalysisPhase.TraceSummaryApplication)
                Trace.TraceInformation("Getting summary for virtual call : " + qualifiedName);
            
            //Get all the possible method infos
            var th = CombinedTypeHierarchy.GetInstance(moduleUnit);
            var typeinfo = th.LookupTypeInfo(typename);
            //if (!th.IsHierarhcyKnown(typeinfo) == null)
            //{
            //    //type hierarhcy is not downward close here.
            //    //log error msg
            //    Trace.TraceWarning("Cannot find the receiver type of " + qualifiedName);
            //    MethodLevelAnalysis.unknownTargetCalls.Add(qualifiedName);

            //    //use unanalyzable call summary here
            //    var calleeSum = SummaryTemplates.GetUnanalyzableCallSummary(methodname, typename);
            //    calleeSummaries.Add(calleeSum);

            //    //update the wholeprogram call graph                
            //    if (call.callingMethodnames != null)
            //    {
            //        foreach(var mname in call.callingMethodnames)
            //            wholecg.AddEdge(mname, qualifiedName);
            //    }
            //    return;            
            //}

            var inheritedMethods = th.GetInheritedMethods(typeinfo, methodname, sig);
            if (inheritedMethods.Any())
            {
                foreach (var methodinfo in inheritedMethods)
                {
                    //on the fly call graph construction
                    //add edges from caller function to the callee node
                    if (methodinfo is InternalMethodInfo && this.callerUnit != null)
                    {
                        var callGraph = this.callerUnit.ParentPEModuleUnit.CallGraph;
                        var callerNode = this.callerUnit.FunctionSymbol.CallNode;
                        var calleeNode = (methodinfo as InternalMethodInfo).GetFunctionSymbol().CallNode;
                        if (callGraph.FindCallEdge(callerNode, calleeNode) == null)
                        {
                            callGraph.CreateUniqueCallEdge(callerNode, calleeNode.FunctionSymbol);
                        }
                    }

                    //update the wholeprogram call graph
                    if (call.callingMethodnames != null)
                    {
                        foreach (var mname in call.callingMethodnames)
                            wholecg.AddEdge(mname, qualifiedName);
                    }

                    var calleeSum = methodinfo.GetSummary();
                    if (calleeSum == null)
                    {                        
                        //The callees are not downward closed here and we do not have stubs
                        //log error msg
                        Trace.TraceWarning("Cannot find summary for the method: " + qualifiedName);
                        MethodLevelAnalysis.unknownTargetCalls.Add(qualifiedName);

                        //use unanalyzable call summary here
                        calleeSum = SummaryTemplates.GetUnanalyzableCallSummary(methodname, typename);
                        calleeSummaries.Add(calleeSum);
                        continue;
                    }
                    //if (call is VirtualCall && (call as VirtualCall).methodname.Equals("MoveNext"))
                    //{
                    //    Console.WriteLine("\t >> Getting summary of method: " + methodinfo.GetQualifiedMethodName());
                    //    Console.WriteLine("[{0},{1},{2}]", calleeSum.OutHeapGraph.VertexCount,
                    //            calleeSum.OutHeapGraph.EdgeCount,
                    //            calleeSum.skippedCalls.Count);
                    //}
                    calleeSummaries.Add(calleeSum);                    
                }
            }
            //There are no methods here
            //else
            //{
            //    if (!typeinfo.HasInfo())
            //    {
            //        //The type hierarhcy is not downward closed here.
            //        //log error msg
            //        Trace.TraceWarning("Cannot find the receiver type of " + qualifiedName);
            //        MethodLevelAnalysis.unknownTargetCalls.Add(qualifiedName);

            //        //use unanalyzable call summary here
            //        var calleeSum = SummaryTemplates.GetUnanalyzableCallSummary(methodname, typename);
            //        calleeSummaries.Add(calleeSum);
            //        return; 
            //    }                               
            //}            
        }

        //static whole program call graph
        public static WholeProgramCG wholecg = new WholeProgramCG();        
    }

    #region wholeprogramCG

    public class WholeProgramCG : QuickGraph.BidirectionalGraph<WholeCGNode, WholeCGEdge>
    {
        public void AddEdge(string src, string dest)
        {
            var srcnode = WholeCGNode.New(src);
            var destnode = WholeCGNode.New(dest); 

            if (!this.ContainsVertex(srcnode))
                this.AddVertex(srcnode);
            if (!this.ContainsVertex(destnode))
                this.AddVertex(destnode);
            var edge = new WholeCGEdge(srcnode, destnode);
            if (!this.ContainsEdge(edge))
                this.AddEdge(edge);
        }

        public void DumpToText(StreamWriter writer)
        {
            GraphUtil.DumpAsText<WholeCGNode, WholeCGEdge>(writer, this, (WholeCGNode v) => (v.Name));
            writer.Flush();
        }        
    }

    public class WholeCGNode
    {
        private static Dictionary<string,WholeCGNode> NodeTable = new Dictionary<string,WholeCGNode>();
        public static int GUID = 37;
        public string Name { get; private set; }        
        public int id;

        private WholeCGNode(string name)
        {            
            this.Name = name;
            id = GUID++;
        }

        public static WholeCGNode New(string name)
        {
            if (NodeTable.ContainsKey(name))
                return NodeTable[name];
            else
            {
                var node = new WholeCGNode(name);
                NodeTable.Add(name, node);
                return node;
            }
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override bool Equals(object obj)
        {
            return this == obj;
        }
    }

    public class WholeCGEdge : QuickGraph.Edge<WholeCGNode>
    {        
        public WholeCGEdge(WholeCGNode srcnode, WholeCGNode destnode)
            : base(srcnode,destnode)
        {            
        }
       
        public override bool Equals(object obj)
        {
            if (obj is WholeCGEdge)
            {
                var edge = obj as WholeCGEdge;
                return (edge.Source.Equals(this.Source) && edge.Target.Equals(this.Target));                    
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (this.Source.GetHashCode() << 3) ^ this.Target.GetHashCode();
        }
    }

    #endregion wholeprogramCG
}
