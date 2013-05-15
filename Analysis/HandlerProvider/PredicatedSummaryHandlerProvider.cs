using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafetyAnalysis.Purity.Summaries;

namespace SafetyAnalysis.Purity.HandlerProvider
{
    class PredicatedSummaryHandlerProvider<T> :
        IPredictedSummaryHandlerProvider<T>,
        IDisposable
        where T : IPredicatedSummaryHandler
    {
        private List<T> _handlers;

        internal PredicatedSummaryHandlerProvider()
        {
            _handlers = new List<T>();
        }

        internal override void RegisterHandler(T handler)
        {
            _handlers.Add(handler);
        }

        internal override bool TryGetHandler(object obj, out T _handler)
        {
            foreach (var handler in _handlers)
            {
                if (handler.GetPredicate()(obj))
                {
                    _handler = handler;
                    return true;
                }
            }

            _handler = default(T);
            return false;
        }

        public void Dispose()
        {
            _handlers.Clear();
            _handlers = null;
        }
    }
}
