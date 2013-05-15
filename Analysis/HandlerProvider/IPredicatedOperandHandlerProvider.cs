using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Purity.HandlerProvider
{
    public abstract class IPredicatedOperandHandlerProvider<T> where T: IPredicatedOperandHandler
    {
        internal abstract void RegisterHandler(T handler);

        internal abstract bool TryGetHandler(object obj, out T handler);
    }
}
