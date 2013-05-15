using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.Purity;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity.Summaries
{
    /// <summary>
    /// This uses two maps a map (keys-values) and a backward map form (value to keys).
    /// </summary>
    public class VertexMap : ExtendedMap<HeapVertexBase,HeapVertexBase>
    {
        //int size = 0;
        internal VertexMap()
        {
        }

        /// <summary>
        /// returns \mu(secondVertex)
        /// </summary>
        /// <param name="secondVertex"></param>
        /// <returns></returns>
        internal HashSet<HeapVertexBase> GetMappedSet(HeapVertexBase secondVertex)
        {            
            if (this.ContainsKey(secondVertex))
                return this[secondVertex];
            else return new HeapVertexSet();
        }

        internal HeapVertexSet Range()
        {
            var range = new HeapVertexSet();
            foreach (var key in this.Keys)
            {
                range.UnionWith(this[key]);
            }
            return range;
        }

        internal VertexMap Union(VertexMap map)
        {
            var newmap = new VertexMap();
            newmap.UnionWith(this);
            newmap.UnionWith(map);
            return newmap;
        }
    }
}
