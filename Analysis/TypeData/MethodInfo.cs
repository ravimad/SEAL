using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using Phx.Symbols;
using SafetyAnalysis.Util;
using SafetyAnalysis.Purity;
using SafetyAnalysis.Purity.Summaries;

namespace SafetyAnalysis.TypeUtil
{
    using CacheKey = Triple<string, string, string>;

    public abstract class MethodInfo
    {
        private static int GUID = 23;        
        private int Id;        
        
        //containing type 
        public TypeInfo Typeinfo { get; private set; }
        public string Methodname { get; private set; }
        public string DTypename {  get; private set; }
        public string Sig {  get; private set; }        

        protected MethodInfo(string typename, string methodname, string sig,TypeInfo tinfo)
        {
            this.Id = GUID++;

            this.DTypename = typename;
            this.Methodname = methodname;
            this.Sig = sig;
            this.Typeinfo = tinfo;
        }
       
        public override bool Equals(object obj)
        {
            return (this == obj);
        }
        
        public override int GetHashCode()
        {
            return this.Id;
        }

        public string GetQualifiedMethodName()
        {
            return DTypename + "::" + Methodname + "/" + Sig;
        }

        public abstract bool IsVirtual();
        public abstract bool IsAbstract();
        public abstract bool IsInstance();
        public abstract PurityAnalysisData GetSummary();
        public abstract PurityAnalysisData ReadSummaryFromStore();
    }

    public class InternalMethodInfo : MethodInfo
    {                
        FunctionSymbol fsym;
        PurityAnalysisData summary = null;
        bool ReadSummary = false;

        public static InternalMethodInfo New(FunctionSymbol funcsym, TypeInfo tinfo)
        {
            //if (MethodTable.ContainsKey(funcsym))
            //    return MethodTable[funcsym];
            //else
            //{
                var typename = PhxUtil.GetTypeName(funcsym.EnclosingAggregateType);
                var methodname = PhxUtil.GetFunctionName(funcsym);
                var sig = PhxUtil.GetFunctionTypeSignature(funcsym.FunctionType);

                var info = new InternalMethodInfo(funcsym, typename, methodname, sig, tinfo);
                //MethodTable.Add(funcsym, info);
                return info;
            //}
        }

        private InternalMethodInfo(FunctionSymbol funcsym, string typename, string methodname, string sig, TypeInfo tinfo)
            : base(typename, methodname, sig, tinfo)
        {
            fsym = funcsym;        
        }

        public FunctionSymbol GetFunctionSymbol()
        {
            return fsym;
        }

        public override bool IsVirtual()
        {
            return fsym.IsVirtual;
        }

        public override bool IsAbstract()
        {
            return fsym.IsAbstract;
        }

        public override bool IsInstance()
        {
            return fsym.IsInstanceMethod;
        }

        public override PurityAnalysisData GetSummary()
        {
            if (ReadSummary)
                return summary;
            
            MethodStubManager knownMethodMan;
            if (MethodStubManager.TryGetMethodStubManager(this, out knownMethodMan))
            {
                //read the summary
                this.ReadSummary = true;
                summary =  knownMethodMan.GetSummary(this);
                return summary;
            }
            //do not cache here (this is the output of ReadActualSummary is susceptible to change)
            return ReadSummaryFromStore();
        }

        public override PurityAnalysisData ReadSummaryFromStore()
        {
            if (fsym.FunctionUnit != null && fsym.CallNode != null)
            {
                var moduleUnit = fsym.FunctionUnit.ParentPEModuleUnit;
                var summaryRecord = moduleUnit.CallGraph.SummaryManager.RetrieveSummary(
                    fsym.CallNode, PurityAnalysisSummary.Type);

                if (summaryRecord != null)
                {
                    PurityAnalysisSummary puritySummary = summaryRecord as PurityAnalysisSummary;
                    
                    if (PurityAnalysisPhase.EnableStats)
                    {
                        MethodLevelAnalysis.callee_summaries_count++;
                    }

                    //return a copy (so that the caller does not mutate the summary)
                    return puritySummary.PurityData.Copy();
                }
            }
            return null;
        }

        public void Serialize(Phx.PEModuleUnit moduleUnit, PurityDBDataContext dbContext)
        {            
            string dllname = moduleUnit.Manifest.Name.NameString;            
            
            if (Sig.Length >= 4000 || Methodname.Length >= 200)
            {
                throw new NotSupportedException("Cannot serialize: Signature length: " + 
                    Sig.Length + " Mehtodname length: " + Methodname.Length);
            }

            Console.WriteLine("serializing method: " + fsym.QualifiedName);

            //serialize method information
            var methodinfo = new SafetyAnalysis.Purity.MethodInfo();
            methodinfo.dllname = dllname;
            methodinfo.typename = DTypename;
            methodinfo.methodname = Methodname;
            methodinfo.methodSignature = Sig;
            methodinfo.IsVirtual = this.IsVirtual();
            methodinfo.IsInstance = this.IsInstance();
            methodinfo.IsAbstract = this.IsAbstract();

            dbContext.MethodInfos.InsertOnSubmit(methodinfo);

            //need not serialize summaries for known methods or unanalyzable methods
            MethodStubManager knownSummaryMan;
            if (MethodStubManager.TryGetMethodStubManager(this, out knownSummaryMan)
                    || !AnalyzableMethods.IsAnalyzable(fsym))
                return;
            
            //serailize summaries
            var summary = this.GetSummary();
            
            puritysummary puritySummary = new puritysummary();
            puritySummary.methodname = Methodname;
            puritySummary.methodSignature = Sig;
            puritySummary.typename = DTypename;
            puritySummary.dllname = dllname;

            //create a memory stream for the summary                             
            BinaryFormatter serializer = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, summary);           
            puritySummary.purityData = new System.Data.Linq.Binary(ms.ToArray());
            ms.Close();

            dbContext.puritysummaries.InsertOnSubmit(puritySummary);                       
        }
    }

    public class ExternalMethodInfo : MethodInfo
    {        
        bool isVirtual;
        bool isAbstract;
        bool isInstance;
        PurityAnalysisData summary = null;
        bool ReadSummary = false;

        public static ExternalMethodInfo New(string type, string mname, string msig,
            bool isVir, bool isAbs, bool isInst, TypeInfo tinfo)
        {
            //var key = new CacheKey(type, mname, msig);
            //if (MethodTable.ContainsKey(key))
            //    return MethodTable[key];
            //else
            //{
                var info = new ExternalMethodInfo(type, mname, msig, isVir, isAbs, isInst, tinfo);
                //MethodTable.Add(key, info);
                return info;
            //}
        }

        private ExternalMethodInfo(string type, string mname, string msig, bool isVir, bool isAbs, bool isInst, TypeInfo tinfo)
            : base(type, mname, msig, tinfo)
        {
            isVirtual = isVir;
            isAbstract = isAbs;
            isInstance = isInst;
        }
        
        public override bool IsVirtual()
        {
            return isVirtual;
        }

        public override bool IsAbstract()
        {
            return isAbstract;
        }

        public override bool IsInstance()
        {
            return isInstance;
        }

        public override PurityAnalysisData GetSummary()
        {
            if (PurityAnalysisPhase.EnableStats)
                MethodLevelAnalysis.dbcacheLookups++;

            if (ReadSummary)
            {              
                if (PurityAnalysisPhase.EnableStats)                
                    MethodLevelAnalysis.dbcacheHits++;                
                return summary;
            }

            //read the summary
            ReadSummary = true;

            MethodStubManager knownMethodMan;
            if (MethodStubManager.TryGetMethodStubManager(this, out knownMethodMan))
            {
                summary = knownMethodMan.GetSummary(this);
                return summary;
            }
            //read summary from database
            summary =  this.ReadSummaryFromStore();
            return summary;
        }

        public override PurityAnalysisData ReadSummaryFromStore()
        {                                  
            //read summary from the database
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();
            PurityDBDataContext dbcontext = PurityAnalysisPhase.DataContext;
            
            var reports = (from report in dbcontext.puritysummaries
                           where report.typename.Equals(DTypename)
                                     && report.methodname.Equals(Methodname)
                                     && report.methodSignature.Equals(Sig)
                           select report).ToList();
            int summaryCount = 0;
            var sumlist = new List<PurityAnalysisData>();
            foreach (var report in reports)
            {
                summaryCount++;
                //deserialize the summary
                if (report.purityData != null)
                {
                    MemoryStream ms = new MemoryStream(report.purityData.ToArray());
                    BinaryFormatter deserializer = new BinaryFormatter();
                    var sum = (PurityAnalysisData)deserializer.Deserialize(ms);
                    sumlist.Add(sum);
                    ms.Close();
                }
                else
                    Trace.TraceWarning("DB data for {0} is null", (report.typename + "::" + report.methodname + "/" + report.methodSignature));
            }
            dbcontext.Dispose();
            sw.Stop();

            if (PurityAnalysisPhase.EnableStats)
            {
                MethodLevelAnalysis.dbaccessTime += sw.ElapsedMilliseconds;
                MethodLevelAnalysis.callee_summaries_count += summaryCount;
            }
            if (sumlist.Any())
            {
                summary = AnalysisUtil.CollapsePurityData(sumlist);
                return summary;
            }
            return null;
        }        

        /// <summary>
        /// Copies the summary. Used by ExternalTypeInfo
        /// </summary>
        /// <param name="minfo"></param>
        public void CopySummary(ExternalMethodInfo minfo)
        {
            minfo.summary = this.summary;
            minfo.ReadSummary = this.ReadSummary;
        }
    }   
}
