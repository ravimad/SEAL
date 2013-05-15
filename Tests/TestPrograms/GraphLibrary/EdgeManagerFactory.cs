/*
 * 
 * User: Gavin Mead
 * Date: 11/30/2009
 * Time: 3:54 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{
    /// <summary>
    /// Description of EdgeManagerFactory.
    /// </summary>
    internal static class EdgeManagerFactory<U>
    {
        public static IEdgeManager<U> CreateEdgeManager(GraphTypes type)
        {
            if(type == GraphTypes.Directed)
            {
                return new DirectedEdgeManager<U>();
            }
            else if(type == GraphTypes.Undirected)
            {
                return new EdgeManager<U>();
            }
            else if(type == (GraphTypes.Directed & GraphTypes.WithWeight))
            {
                throw new NotImplementedException();
            }
            else if(type == (GraphTypes.Undirected & GraphTypes.WithWeight))
            {
                throw new NotImplementedException();
            }
            else {
                throw new ArgumentException("invalid GraphType flag combination");
            }
        }
    }
}
