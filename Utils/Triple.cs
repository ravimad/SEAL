using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Util
{
    [Serializable]
    public class Triple<U,V,W>
    {
        readonly public U t1;
        readonly public V t2;
        readonly public W t3;

        public Triple(U u, V v, W w)
        {
            //Contract.Assert(u != null && v != null && w != null);
            t1 = u;
            t2 = v;
            t3 = w;
        }

        public override bool Equals(object obj)
        {
            Triple<U, V, W> triple = obj as Triple<U, V, W>;
            if (triple != null && t1.Equals(triple.t1) && t2.Equals(triple.t2)
                && t3.Equals(triple.t3))
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return ((t1.GetHashCode() << 15) ^ (t2.GetHashCode() << 9) ^ t3.GetHashCode());
        }

        public override string ToString()
        {
            return "(" + t1.ToString() + "," + t2.ToString() + "," + t3.ToString() + ")";
        }
    }
}
