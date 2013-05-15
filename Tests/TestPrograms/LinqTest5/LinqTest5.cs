using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqTest5
{
    public class LinqTest
    {
        public int f = 0;
        public class Pair<U, V>
        {
            public U Key;
            public V Value;

            public Pair(U u, V v)
            {            
                Key = u;
                Value = v;
            }

            public override bool Equals(object obj)
            {
                Pair<U, V> pair = obj as Pair<U, V>;
                if (pair != null && Key.Equals(pair.Key) && Value.Equals(pair.Value))
                    return true;
                return false;
            }

            public override int GetHashCode()
            {
                return ((Key.GetHashCode() << 15) ^ Value.GetHashCode());
            }            
        }

        public void Client(LinqTest rep)
        {
            var col = new HashSet<Pair<LinqTest, int>>();
            LinqTest[] linqArray = { new LinqTest(), new LinqTest() };
            col.Add(new Pair<LinqTest, int>(linqArray[0], 0));
            col.Add(new Pair<LinqTest, int>(linqArray[1], 1));
            var linqSet = new HashSet<LinqTest>(linqArray);

            UnionSetOfPairs<LinqTest, int>(col, linqSet, rep);
            var first = col.First();
            first.Key.f = 1;
        }

        public void UnionSetOfPairs<K, V>(HashSet<Pair<K, V>> setOfPairs,
            HashSet<K> vertices, K rep) 
        {
            IEnumerable<V> values = from pair in setOfPairs
                                    where vertices.Contains(pair.Key)
                                    select pair.Value;
            foreach (var value in values.ToList())
            {
                setOfPairs.Add(new Pair<K, V>(rep, value));                
            }

            //remove all the old entries
            setOfPairs.RemoveWhere((Pair<K, V> pair) => vertices.Contains(pair.Key));            
        }
    }
}
