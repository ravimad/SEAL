using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafetyAnalysis.Framework.Graphs;

namespace SafetyAnalysis.Purity.Summaries
{
    public abstract class IPuritySummaryHandler : IPredicatedSummaryHandler
    {
        internal abstract void ApplySummary(
            Phx.IR.Instruction instruction,
            PurityAnalysisData data);            
    }
}
