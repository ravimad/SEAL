using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.Purity;
using SafetyAnalysis.Util;
using SafetyAnalysis.Purity.statistics;
using SafetyAnalysis.TypeUtil;

namespace SafetyAnalysis.Purity.Summaries
{    
    public class CallBackResolver : CallResolver
    {       
        //for statistics
        List<Pair<Phx.Graphs.CallNode, int>> calleeEdgeCounts;        

        private Phx.FunctionUnit functionUnit;

        internal CallBackResolver(Phx.FunctionUnit funit)
        {            
            calleeEdgeCounts = new List<Pair<Phx.Graphs.CallNode, int>>();
            functionUnit = funit;            
        }

        public override void ApplySummary(
            Call call,
            PurityAnalysisData data,
            HeapGraphBuilder builder)
        {
            var moduleUnit = functionUnit.ParentPEModuleUnit;
            CallType calltype = CallUtil.GetCallType(call, data);

            string methodname = null, typename = null;
            if (call is CallWithMethodName)
            {
                methodname = (call as CallWithMethodName).GetMethodName();
                typename = (call as CallWithMethodName).GetDeclaringType();
            }
            else
                methodname = "Delegate";

            if (calltype.isResolvable == false)
            {
                var calleeSummary = SummaryTemplates.GetUnanalyzableCallSummary(methodname, typename);
                ApplyTargetsSummary(call, data, calleeSummary, builder as HigherOrderHeapGraphBuilder);
            }
            else
            {
                //check if the call is a thread creator (special handling for thread creators)
                if (calltype.threadstart == true)
                {
                    if (!(call is StaticCall))
                        throw new NotSupportedException("Thread start is not a static call: " + call);

                    //add the receivers to live thread objs list
                    data.AddThreadObjects(AnalysisUtil.GetReceiverVertices(call as StaticCall,
                        data));
                    //make the call a call back (though technically this may not be a call back
                    //this mechanism will let us handle threads soundly)
                    HandleCallback(call, data);
                }
                else if (calltype.threadjoin == true)
                {
                    if (!(call is StaticCall))
                        throw new NotSupportedException("Thread Join is not a static call: " + call);
                    
                    var recvs = AnalysisUtil.GetReceiverVertices(call as StaticCall, data);
                    if (recvs.Count() == 1)
                    {
                        //assuming that the abstract object represents only one concrete object
                        //and that the variable must point to the object (need to be fixed later)
                        data.RemoveFromThreadObjs(recvs.First());
                    }
                }
                else
                {
                    if (calltype.isCallback == true)
                    {
                        HandleCallback(call, data);
                    }
                    //try to merge the summaries of the known targets
                    if (calltype.hasTargets == true)
                    {
                        var calleeSummary =
                            (new CalleeSummaryReader(functionUnit, moduleUnit)).GetCalleeData(call, data);
                        ApplyTargetsSummary(call, data, calleeSummary, builder as HigherOrderHeapGraphBuilder);
                    }
                }
            }

            //int new_edges = _callerData.OutHeapGraph.EdgeCount;         
            //if new_edges is more than (1+alpha) times the old_edges. Here alpha is tentatively fixed at 0.8
            //if ((new_edges - old_edges) > (0.8 * old_edges))
            //{
            //    bool found_reason = false;
            //    foreach (var pair in calleeEdgeCounts)
            //    {
            //        //if the edges in a callee summary is alpha times the new edge count. Here alpha is
            //        //tentatively fixed at 0.6
            //        if (pair.Value > (0.6 * new_edges))
            //        {
            //            PurityAnalysisPhase.explosionReasons.Add(new CalleeSummary(old_edges,new_edges,pair.Key));
            //            found_reason = true;
            //        }
            //    }
            //    if(!found_reason)
            //    {
            //        if (calleeEdgeCounts.Count > 1)
            //            PurityAnalysisPhase.explosionReasons.Add(new VirtualMethodCall(old_edges, new_edges, calleeEdgeCounts.Count, callInstruction));
            //        else
            //            PurityAnalysisPhase.explosionReasons.Add(new UnknownReason(old_edges,new_edges));
            //    }
            //}            
        }                

        private void HandleCallback(Call call, PurityAnalysisData data)
        {            
            //Contract.Assert(call is DelegateCall);
            data.AddSkippedCall(call);

            //add all the parameters to the graph (if not present)
            foreach (var param in call.GetReferredVertices())
            {
                if (!data.OutHeapGraph.ContainsVertex(param))
                    data.OutHeapGraph.AddVertex(param);                
            }

            if (call.HasReturnValue())
            {
                //make the return vertex point to an external node (ReturnedValueVertex).
                var retvar = call.GetReturnValue();
                ReturnedValueVertex rv = NodeEquivalenceRelation.CreateReturnedValueVertex(call.contextid, Context.EmptyContext);                

                if (!data.OutHeapGraph.ContainsVertex(rv))
                    data.OutHeapGraph.AddVertex(rv);

                if (!data.OutHeapGraph.ContainsVertex(retvar))
                    data.OutHeapGraph.AddVertex(retvar);

                var edge = new InternalHeapEdge(retvar, rv, NullField.Instance);
                if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                    data.OutHeapGraph.AddEdge(edge);

                //add type info to the returned value vertex                
                //AnalysisUtil.AddApproximateType(data, rv, callinst.DestinationOperand.Type);                
            }

            //get all the referred vertices and remove them from strong update set
            if (PurityAnalysisPhase.FlowSensitivity)
            {
                foreach (var refvar in call.GetReferredVertices())
                {
                    data.RemoveStrongUpdates(refvar);
                }
            }
        }

        //private void HandleVirtualCallback(Call call, PurityAnalysisData data)
        //{            
        //    //Contract.Assert(call is VirtualCall);
        //    data.skippedCalls.Add(call);

        //    //add all the parameters to the graph (if not present)
        //    foreach (var param in call.GetAllParams())
        //    {
        //        if (!data.OutHeapGraph.ContainsVertex(param))
        //            data.OutHeapGraph.AddVertex(param);
        //    }

        //    if (call.HasReturnValue())
        //    {                
        //        //make the return vertex point to an external node (ReturnedValueVertex).
        //        var retvar = call.GetReturnValue();
        //        ReturnedValueVertex rv = NodeEquivalenceRelation.CreateReturnedValueVertex(call.contextid, Context.EmptyContext);

        //        if (!data.OutHeapGraph.ContainsVertex(rv))
        //            data.OutHeapGraph.AddVertex(rv);

        //        if (!data.OutHeapGraph.ContainsVertex(retvar))
        //            data.OutHeapGraph.AddVertex(retvar);

        //        var edge = new InternalHeapEdge(retvar, rv, NullField.Instance);
        //        if (!data.OutHeapGraph.ContainsHeapEdge(edge))
        //            data.OutHeapGraph.AddEdge(edge);

        //        //add type info to the returned value vertex                
        //        //AnalysisUtil.AddApproximateType(data, rv, callinst.DestinationOperand.Type);
        //    }
        //}       
    }
}
