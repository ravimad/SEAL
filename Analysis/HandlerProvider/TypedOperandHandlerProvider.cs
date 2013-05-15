using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Purity
{
    internal class TypedOperandHandlerProvider : ITypedOperandHandlerProvider
    {
        private Dictionary<System.Type, ITypedOperandHandler> _readers;

        internal TypedOperandHandlerProvider()
        {
            _readers = new Dictionary<Type, ITypedOperandHandler>();
        }

        internal override void RegisterHandler(ITypedOperandHandler reader)
        {
            if (!_readers.ContainsKey(reader.GetType()))
            {
                _readers.Add(reader.GetType(), reader);
            }
        }

        internal override bool TryGetHandler(System.Type type, out ITypedOperandHandler typedReader)        
        {
            if (_readers.ContainsKey(type))
            {
                typedReader = _readers[type];
                return true;
            }

            typedReader = null;
            return false;
        }
    }
}
