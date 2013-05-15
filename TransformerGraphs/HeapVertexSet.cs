using System;
using System.Collections.Generic;
using System.Text;
using QuickGraph.Collections;
using System.Linq;

namespace SafetyAnalysis.Framework.Graphs
{
    public class HeapVertexSet : 
        System.Collections.Generic.HashSet<HeapVertexBase>,
        IDisposable
    {
        public HeapVertexSet()
        {
        }        

        public HeapVertexSet(IEnumerable<HeapVertexBase> vertices)
            : base(vertices)
        {                        
        }

        public static HeapVertexSet Create(IEnumerable<HeapVertexBase> vertices)
        {
            var list = vertices.ToList();
            return new HeapVertexSet(list);
        }

        public override string ToString()
        {
            string str = "[";
            bool firstTime = true;
            foreach (HeapVertexBase v in this)
            {
                if (!firstTime)
                    str += " , ";
                else
                    firstTime = false;
                str += v.ToString();
            }
            str += "]";
            return str;
        }

        public void Dispose()
        {
        }        
    }
}
