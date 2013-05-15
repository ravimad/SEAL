using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Purity
{
    internal abstract class ITypedOperandHandlerProvider
    {
        internal abstract void RegisterHandler(ITypedOperandHandler handler);

        internal abstract bool TryGetHandler(System.Type type, out ITypedOperandHandler handler);
    }
}
