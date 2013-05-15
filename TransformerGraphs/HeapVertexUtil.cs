using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Framework.Graphs
{
    public class HeapVertexUtil
    {
        public static bool IsThis(HeapVertexBase vertex)
        {
            if (!(vertex is ParameterHeapVertex))
                return false;

            if ((vertex as ParameterHeapVertex).name.Equals("this"))
                return true;

            return false;
        }

        public static ParameterHeapVertex CreateThisVertex()
        {
            return ParameterHeapVertex.New(1, "this");
        }

        public static ParameterHeapVertex GetThisVertex(HeapGraphBase hg)
        {
            var thisvertex = from v in hg.Vertices.OfType<ParameterHeapVertex>()
                             where v.name.Equals("this")
                             select v;
            if (thisvertex.Any())
                return thisvertex.First();
            else
                return null;
        }                
    }
}
