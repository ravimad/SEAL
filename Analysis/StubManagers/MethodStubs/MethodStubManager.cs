using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.Util;
using SafetyAnalysis.Purity;

namespace SafetyAnalysis.Purity.Summaries
{
    public abstract class MethodStubManager
    {    
        /// <summary>
        /// First priority to synthetic and second to stub
        /// </summary>
        /// <param name="methodinfo"></param>
        /// <param name="sumManager"></param>
        /// <returns></returns>
        public static bool TryGetMethodStubManager(TypeUtil.MethodInfo methodinfo, out MethodStubManager sumManager)
        {
            var syntheticMan = SyntheticMethodStub.GetInstance();
            if (syntheticMan.HasSummary(methodinfo))
            {
                sumManager = syntheticMan;
                return true;
            }
            var stubman = DBMethodStub.GetInstancce();
            if (stubman.HasStub(methodinfo))
            {
                sumManager = stubman;
                return true;
            }
            
            sumManager = null;
            return false;           
        }

        public abstract PurityAnalysisData GetSummary(TypeUtil.MethodInfo methodinfo);        
    }
}