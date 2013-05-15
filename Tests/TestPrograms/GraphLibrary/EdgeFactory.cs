/*
 * 
 * User: Gavin Mead
 * Date: 11/30/2009
 * Time: 2:59 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{
    internal enum EdgeType {
        UndirectedEdge = 1,
        DirectedEdge = 2,
        WeightedEdge = UndirectedEdge | 4,
        DirectedWeightedEdge = DirectedEdge | 8
        
    }
    /// <summary>
    /// Description of EdgeFactory.
    /// </summary>
    internal static class EdgeFactory<U>
    {
        public static IEdge<U> CreateEdge(EdgeType edgeType) {
            if(edgeType == EdgeType.UndirectedEdge)
            {
                return new BaseEdge<U>();
            }
            else if(edgeType == EdgeType.DirectedEdge)
            {
                return new DirectedEdge<U>();
            }
            else {
                return null;
            }
        }
    }
}
