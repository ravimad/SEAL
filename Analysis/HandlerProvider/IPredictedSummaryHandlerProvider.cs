using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafetyAnalysis.Purity.Summaries;

namespace SafetyAnalysis.Purity.HandlerProvider
{
    internal abstract class IPredictedSummaryHandlerProvider<T> where T : IPredicatedSummaryHandler
    {
        internal abstract void RegisterHandler(T handler);

        internal abstract bool TryGetHandler(object obj, out T handler);
    }
}
