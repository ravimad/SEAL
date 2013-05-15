using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Purity.Summaries
{
    public  abstract class IPredicatedSummaryHandler : ISummaryHandler
    {
        internal abstract Predicate<object> GetPredicate();
    }
}
