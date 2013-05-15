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
    public class DBMethodStub : MethodStubManager
    {
        private static DBMethodStub instance = null;     
        private DBStubManager stubman = new DBStubManager("stubmappingfilename");
                          
        public static DBMethodStub GetInstancce()
        {
            if(instance == null)
                instance = new DBMethodStub();
            return instance;
        }
                
        public bool HasStub(TypeUtil.MethodInfo methodinfo)
        {
            return stubman.ExistsStub(methodinfo.DTypename, methodinfo.Methodname, methodinfo.Sig);
        }

        public override PurityAnalysisData GetSummary(TypeUtil.MethodInfo methodinfo)
        {
            return stubman.RetrieveSummary(methodinfo.DTypename, methodinfo.Methodname, 
                methodinfo.Sig, methodinfo.Typeinfo.Their);
        }        
    }
}
