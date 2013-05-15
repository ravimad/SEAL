using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.Summaries;
using SafetyAnalysis.Util;
using QuickGraph.Collections;

namespace SafetyAnalysis.Purity
{    
    public class NodeSkippedCallMerger
    {                                     
        protected NodeMerger nm;
        protected Func<Call, PurityAnalysisData, SkippedCallHash> hashSelector;
        private PurityAnalysisData data;

        public NodeSkippedCallMerger(NodeMerger nodeMerger, 
            Func<Call,PurityAnalysisData,SkippedCallHash> hashsel)
        {            
            nm = nodeMerger;
            hashSelector = hashsel;
        }

        public void MergeWithSkippedCalls(PurityAnalysisData purityData)
        {
            //storing it in a field for easy access.
            data = purityData;

            int oldSkcallsCount;
            int iter = 0;
            do
            {
                oldSkcallsCount = data.SkippedCalls.Count();

                nm.MergeNodes(data);
                this.MergeAllSkippedCalls();

                iter++;
            } while (oldSkcallsCount != data.SkippedCalls.Count());
        }

        private void MergeAllSkippedCalls()
        {
            var mergeableSkcallsList = new List<List<Call>>();
            PopulateMergeableSkippedCalls(mergeableSkcallsList);

            foreach (var sklist in mergeableSkcallsList)
            {
                MergeSkippedCalls(sklist);
            }
        }

        private void PopulateMergeableSkippedCalls(List<List<Call>> mergeableSkcallsList)
        {
            var dic = new Dictionary<SkippedCallHash, List<Call>>(data.SkippedCalls.Count());
            foreach (var skcall in data.SkippedCalls)
            {
                List<Call> calllist;
                var skhash = hashSelector(skcall, data);
                if (dic.TryGetValue(skhash, out calllist))
                {
                    calllist.Add(skcall);
                }
                else
                {
                    calllist = new List<Call> { skcall };
                    dic.Add(skhash, calllist);
                }
            }
            foreach(var pair in dic)
            {
                if (pair.Value.Count > 1)
                    mergeableSkcallsList.Add(pair.Value);
            }
        }        

        private void MergeSkippedCalls(List<Call> sklist)
        {            
            var firstcall = sklist.First();
            Call newCall = null;
            uint cid = firstcall.contextid;

            //compute calling methods list
            var callingmethods = sklist.SelectMany((Call skcall) => (skcall.callingMethodnames)).Distinct().ToList();                                 

            if (firstcall is DelegateCall)
            {
                //compute a representative for the receiver                
                var equivVars = new HeapVertexSet();
                foreach (var skcall in sklist)
                {
                    var dcall = skcall as DelegateCall;
                    equivVars.Add(dcall.GetTarget());
                }
                HeapVertexBase rep = null;
                if (equivVars.Count > 1)
                {
                    rep = NodeEquivalenceRelation.ChooseRepresentative(equivVars);
                    equivVars.Remove(rep);
                    data.OutHeapGraph.MergeEdges(equivVars, rep);
                }
                else
                    rep = equivVars.First();
                //create a new delegate call
                newCall = new DelegateCall(cid, rep as VariableHeapVertex,(firstcall as DelegateCall).GetSignature(),callingmethods);
            }
            else
            {
                var vcall = firstcall as VirtualCall;
                //create a new virtual call
                newCall = new VirtualCall(cid, vcall.methodname, vcall.signature, vcall.declaringtype, callingmethods);
            }
            for (int paramCount = 0; paramCount < firstcall.GetParamCount(); paramCount++)
            {
                //compute a representative for the parameter     
                var equivVars = new HeapVertexSet();
                foreach (var skcall in sklist)
                {
                    equivVars.Add(skcall.GetParam(paramCount));
                }
                HeapVertexBase rep = null;
                if (equivVars.Count > 1)
                {
                    rep = NodeEquivalenceRelation.ChooseRepresentative(equivVars);
                    equivVars.Remove(rep);
                    data.OutHeapGraph.MergeEdges(equivVars, rep);
                }
                else
                    rep = equivVars.First();

                //add parameter to the new call
                newCall.AddParam(paramCount, rep as VariableHeapVertex);
            }
            //compute a representative for the return variable
            {                                
                var equivVars = new HeapVertexSet();
                foreach (var skcall in sklist)
                {
                    if (skcall.HasReturnValue())
                        equivVars.Add(skcall.GetReturnValue());
                }
                if (equivVars.Any())
                {
                     HeapVertexBase rep = null;
                     if (equivVars.Count > 1)
                     {
                         rep = NodeEquivalenceRelation.ChooseRepresentative(equivVars);
                         equivVars.Remove(rep);
                         data.OutHeapGraph.MergeEdges(equivVars, rep);
                     }
                     else
                         rep = equivVars.First();

                    //add a return value to the newcall
                    newCall.AddReturnValue(rep as VariableHeapVertex);
                }                
            }
            
            //remove all skipped calls
            var newcallTargets = new HashSet<string>();
            foreach (var skcall in sklist)
            {
                newcallTargets.UnionWith(data.GetTargets(skcall));
                data.RemoveSkippedCall(skcall);
            }
            data.AddSkippedCall(newCall);
            data.skippedCallTargets.Add(newCall, newcallTargets);
        }                
    }
}
