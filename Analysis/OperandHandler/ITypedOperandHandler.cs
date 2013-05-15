using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Purity
{
    internal abstract class ITypedOperandHandler : IOperandHandler
    {
        internal abstract System.Type GetSelectionType();
    }
}
