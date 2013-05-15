using System;
using System.Collections.Generic;
using System.Text;

using SafetyAnalysis.Framework.Graphs;

namespace SafetyAnalysis.Purity
{
    public class PurityAnalysisSummary : Phx.Graphs.SummaryRecord
    {

        /// <summary>
        /// 
        /// </summary>
        public PurityAnalysisData PurityData { get; protected set; }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="summaryManager"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static PurityAnalysisSummary New(
            Phx.Graphs.SummaryManager summaryManager, 
            PurityAnalysisData purityData)
        {
            PurityAnalysisSummary summary = new PurityAnalysisSummary();
            summary.Initialize(summaryManager, summaryManager.Lifetime);
            summary.PurityData = purityData.Copy();
            
            return summary;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Delete()
        {
            base.Delete();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override bool Copy(Phx.Graphs.SummaryRecord record)
        {
            //Contract.Assert(this.IsEqual(record));

            PurityAnalysisSummary summary = record as PurityAnalysisSummary;
            this.PurityData = summary.PurityData.Copy();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Phx.Graphs.SummaryRecord Retrieve()
        {
            return this;
        }

        public static Type Type
        {
            get { return typeof(PurityAnalysisSummary); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Type SummaryType
        {
            get { return typeof(PurityAnalysisSummary); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dump()
        {
        }
    }
}
