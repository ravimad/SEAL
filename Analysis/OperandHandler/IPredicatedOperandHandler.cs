using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Purity
{
    public abstract class IPredicatedOperandHandler : IOperandHandler
    {
        internal abstract Predicate<object> GetPredicate();
    }
}
