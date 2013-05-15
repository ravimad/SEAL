using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Util
{
    [Serializable]
    public class Pair<U,V>
    {
        readonly public U Key;
        readonly public V Value;

        public Pair(U u, V v)
        {
            //Contract.Assert(u != null && v != null);
            Key = u;
            Value = v;
        }

        public override bool Equals(object obj)
        {
            Pair<U, V> pair = obj as Pair<U, V>;

            if (pair == null)
                return false;
            
            if (Key.Equals(pair.Key) && Value.Equals(pair.Value))
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return ((Key.GetHashCode() << 15) ^ Value.GetHashCode());
        }

        public override string ToString()
        {
            return "(" + Key.ToString() + "," + Value.ToString() + ")";
        }
    }
}
