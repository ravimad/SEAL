using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.Util;
using SafetyAnalysis.Purity;
using SafetyAnalysis.TypeUtil;

namespace SafetyAnalysis.Purity.Summaries
{
    public abstract class CallStubManager
    {
        /// <summary>
        /// First priority to synthetic and second to stub.        
        /// </summary>
        /// <param name="call"></param>
        /// <param name="sumManager"></param>
        /// <returns></returns>
        public static bool TryGetCallStubManager(Call call, out CallStubManager sumManager)
        {            
            var syntheticMan = SyntheticCallStub.GetInstance();
            if (syntheticMan.HasSummary(call))
            {
                sumManager = syntheticMan;
                return true;
            }
            var stubman = DBCallStub.GetInstancce();
            if (stubman.HasStub(call))
            {
                sumManager = stubman;
                return true;
            }
            sumManager = null;
            return false;           
        }

        public abstract PurityAnalysisData GetSummary(Call call, PurityAnalysisData data, CombinedTypeHierarchy th);        
    }
}