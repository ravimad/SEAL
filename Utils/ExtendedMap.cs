using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Util
{
    /// <summary>
    /// This is a specialized map that can be treated as a hashset set of pairs as well.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    [Serializable]
    public class ExtendedMap<K,V> : Dictionary<K,HashSet<V>>
    {        
        public ExtendedMap()
        {
        }

        public ExtendedMap(IEnumerable<Pair<K, V>> initlist)
        {
            foreach (var elem in initlist)
                this.Add(elem.Key, elem.Value);
        }

        public bool Contains(K key, V value)
        {
            if (!this.ContainsKey(key))
                return false;
            return this[key].Contains(value);                
        }

        public void Add(K key, V value)
        {
            if (!this.ContainsKey(key))
            {
                var set = new HashSet<V>();
                set.Add(value);
                this[key] = set;                
            }
            else 
                this[key].Add(value);                        
        }

        public void Add(K key, IEnumerable<V> values)
        {
            if (!this.ContainsKey(key))
            {
                var set = new HashSet<V>(values);
                this[key] = set;
            }
            else
                this[key].UnionWith(values);            
        }

        public void UnionWith(ExtendedMap<K, V> map)
        {
            foreach (var key in map.Keys)
            {
                this.Add(key, map[key]);                
            }
        }

        public void RemoveAll(ExtendedMap<K, V> map)
        {
            foreach (var key in map.Keys)
            {
                if (this.ContainsKey(key))
                    this[key].RemoveWhere((V val) => (map[key].Contains(val)));
            }
        }

        public List<Pair<K,V>> GetListOfPairs()
        {
            var list = new List<Pair<K,V>>();
            foreach (var key in this.Keys)
            {
                foreach(var value in this[key])
                {
                    var pair = new Pair<K, V>(key, value);
                    list.Add(pair);
                }
            }
            return list;
        }

        public bool SetEquals(ExtendedMap<K, V> map)
        {
            if (this.Keys.Count != map.Keys.Count)
                return false;

            foreach (var key in this.Keys)
            {
                if (!map.ContainsKey(key))
                    return false;
                if (!this[key].SetEquals(map[key]))
                    return false;
            }
            return true;
        }

        public int GetCount()
        {
            int count = 0;
            foreach (var key in this.Keys)
            {
                count += this[key].Count;
            }
            return count;
        }

        public ExtendedMap<K, V> Copy()
        {
            var newmap = new ExtendedMap<K, V>();
            newmap.UnionWith(this);
            return newmap;
        }
    }
}
