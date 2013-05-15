using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
using SafetyAnalysis.TypeUtil;
using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.Summaries;

namespace SafetyAnalysis.Purity
{
    public class CallUtil
    {
        public static CallType GetCallType(Call call, PurityAnalysisData data)
        {
            var calltype = new CallType();
            CallStubManager stubman;
            if (CallStubManager.TryGetCallStubManager(call, out stubman))
            {
                calltype.hasTargets = true;
                calltype.isResolvable = true;
                calltype.isCallback = false;
                calltype.stubbed = true;
                return calltype;
            }

            if (call is StaticCall)
            {
                calltype.hasTargets = true;
                calltype.isResolvable = true;
                calltype.isCallback = false;
                return calltype;
            }
            else if (call is DelegateCall)
            {
                return GetDelagateCallType(call as DelegateCall, data);
            }
            else if (call is VirtualCall)
            {
                return GetVirtualCallType(call as VirtualCall, data);
            }
            return null;
        }        

        private static CallType GetVirtualCallType(VirtualCall vcall, PurityAnalysisData data)
        {
            var calltype = new CallType();            
            var recvrVertices = AnalysisUtil.GetReceiverVertices(vcall, data);
            if (!recvrVertices.Any())
                calltype.isResolvable = false;
            else calltype.isResolvable = true;

            if (recvrVertices.Any((HeapVertexBase v) => (v is InternalHeapVertex)))
            {
                calltype.hasTargets = true;                
                if (recvrVertices.All((HeapVertexBase v) => (v is InternalHeapVertex)))
                    calltype.isCallback = false;
                else
                    calltype.isCallback = true;
            }
            else
            {
                calltype.hasTargets = false;
                calltype.isCallback = true;
            }
            return calltype;
        }

        private static CallType GetDelagateCallType(DelegateCall dcall, PurityAnalysisData data)
        {
            CallType calltype = new CallType();
            var targetVertices = AnalysisUtil.GetTargetVertices(dcall, data);
            var methodVertices = AnalysisUtil.GetMethodVertices(data, targetVertices);

            if (!targetVertices.Any())
            {
                calltype.isResolvable = false;
                //other fields values are null here.
                return calltype;
            }

            calltype.isResolvable = true;
            if (targetVertices.Any((HeapVertexBase v) => (v is InternalHeapVertex))
                && methodVertices.Any())
            {
                calltype.hasTargets = true;
                if (targetVertices.All((HeapVertexBase v) => (v is InternalHeapVertex)))
                {
                    //check if any of the methods is virtual
                    if (methodVertices.Any((MethodHeapVertex m) => m.IsVirtual))
                    {
                        var recvrVertices = AnalysisUtil.GetReceiverVertices(data, targetVertices);
                        //for virtual methods all receiver vertices should be internal
                        //Assumption: no receiver vertex => call is captured
                        if (recvrVertices.All((HeapVertexBase v) => (v is InternalHeapVertex)))
                            calltype.isCallback = false;
                        else
                            calltype.isCallback = true;
                    }
                    else
                        calltype.isCallback = false;
                }
                else
                    calltype.isCallback = true;
            }
            else
            {
                calltype.hasTargets = false;
                calltype.isCallback = true;
            }
            return calltype;
        }
    }
}
