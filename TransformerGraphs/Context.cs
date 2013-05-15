using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;

namespace SafetyAnalysis.Framework.Graphs
{    
    using TrieEntry = Dictionary<Object,Object>;
    public class Context
    {                
        //Trie data structure
        private static TrieEntry trieRoot = new TrieEntry();
        //public static Dictionary<List<Object>, Context> Table
        //    = new Dictionary<List<object>, Context>(new ContextListEqualityComparer());

        public static Context EmptyContext = new Context(new List<object>());
        public static int GUID = 13;

        List<Object> list;
        int id;

        public int Count {
            get { return list.Count; }            
        }

        private Context(List<Object> ctstring)
        {
            list = new List<object>(ctstring);
            id = GUID++;
        }
        
        public static Context New(List<Object> ctstring)
        {
            Context currContext = EmptyContext;                               
            TrieEntry nextEntry = trieRoot;
            TrieEntry currEntry = null;     
            //tracks the no. of objects traversed
            List<Object> currList = new List<object>();

            foreach (var ctobject in ctstring)
            {
                currList.Add(ctobject);
                currEntry = nextEntry; 
              
                if (currEntry.ContainsKey(ctobject))
                {
                    var pair = currEntry[ctobject] as Pair<Context, TrieEntry>;
                    currContext = pair.Key;                    
                    nextEntry = pair.Value;
                }
                else
                {
                    currContext = new Context(currList);
                    nextEntry = new TrieEntry();
                    var pair = new Pair<Context, TrieEntry>(currContext, nextEntry);
                    currEntry.Add(ctobject,pair);                    
                }                
            }
            return currContext;
        }

        public List<object> GetContextString()
        {           
            return list;
        }

        public override bool Equals(object obj)
        {
            return (this == obj);
        }        

        public override int GetHashCode()
        {
            return id;
        }

        public override string ToString()
        {
            string str = "[";
            int count = 1;
            foreach (var elem in list)
            {
                str += elem;
                if (count < list.Count)
                    str += ",";
                count++;
            }
            str += "]";
            return str;
        }
    }

    class ContextListEqualityComparer : IEqualityComparer<List<Object>>
    {
        #region IEqualityComparer<List<object>> Members

        public bool Equals(List<object> x, List<object> y)
        {
            if (x.Count == y.Count)
            {
                for (int i = 0; i < x.Count; i++)
                {
                    if (!x[i].Equals(y[i]))
                        return false;
                }
                return true;
            }
            return false;
        }

        public int GetHashCode(List<object> list)
        {
            return list.Count;
        }

        #endregion
    }
}
