using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

using SafetyAnalysis.Util;
using SafetyAnalysis.TypeUtil;
using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity;

namespace SafetyAnalysis.Purity.Summaries
{
    public class DBCallStub : CallStubManager
    {
        private static DBCallStub instance = null;
        private DBStubManager stubman = new DBStubManager("vcallsstubmappingfilename");

        public static DBCallStub GetInstancce()
        {
            if (instance == null)
                instance = new DBCallStub();
            return instance;
        }

        /// <summary>
        /// cannnot stub delegate call as of now.
        /// </summary>
        /// <param name="call"></param>
        /// <returns></returns>
        public bool HasStub(Call call)
        {
            if(call is CallWithMethodName)
            {
                var mcall = call as CallWithMethodName;
                return stubman.ExistsStub(mcall.GetDeclaringType(), mcall.GetMethodName(), mcall.GetSignature());
            }
            return false;            
        }

        public override PurityAnalysisData GetSummary(Call call, PurityAnalysisData data, CombinedTypeHierarchy th)
        {
            if (!(call is CallWithMethodName))
                throw new ArgumentException("Delegate call cannot be stubbed: " + call);

            var mcall = call as CallWithMethodName;
            return stubman.RetrieveSummary(mcall.GetDeclaringType(), mcall.GetMethodName(), 
                mcall.GetSignature(),th);
        }
    }
}
