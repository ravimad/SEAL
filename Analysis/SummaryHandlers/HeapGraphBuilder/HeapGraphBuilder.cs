using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Purity;
using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity.Summaries
{
    public abstract class HeapGraphBuilder
    {
        public abstract void ComposeCalleeSummary(
            Call call,
            PurityAnalysisData callerData,
            PurityAnalysisData calleeData);
    }
}
