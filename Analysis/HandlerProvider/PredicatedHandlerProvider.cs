using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Purity.HandlerProvider
{
    class PredicatedHandlerProvider<P,T> : IDisposable         
    {
        private Dictionary<Predicate<P>,T> _handlers;

        internal PredicatedHandlerProvider()
        {
            _handlers = new Dictionary<Predicate<P>, T>();
        }

        internal void RegisterHandler(Predicate<P> pred, T handler)
        {
            _handlers.Add(pred,handler);
        }

        internal bool TryGetHandler(P obj, out T _handler)
        {
            foreach (var pair in _handlers)
            {
                if (pair.Key(obj))
                {
                    _handler = pair.Value;
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
