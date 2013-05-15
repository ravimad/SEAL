/*
 * 
 * User: Gavin Mead
 * Date: 12/1/2009
 * Time: 9:20 PM
 * 
 * 
 */
using System;
using System.Collections.Generic;

namespace GraphLibrary
{
    /// <summary>
    /// Description of BreadthFirstTraverser.
    /// </summary>
    public class BreadthFirstTraverser<T,U> : IGraphTraverser<T, U>
    {
        IGraph<T, U> graph;
        IList<IVertexId> visitedVertices = null;
        Stack<IVertexId> dfsStack = null;
        IList<T> dfsResults = null;
        
        public BreadthFirstTraverser()
        {
            visitedVertices = new List<IVertexId>();
            dfsStack = new Stack<IVertexId>();
            dfsResults = new List<T>();
        }
        
        public IGraph<T, U> Graph {
            set {
                throw new NotImplementedException();
            }
        }
        
        public System.Collections.Generic.IList<T> Traverse(IVertexId startVertex)
        {
            throw new NotImplementedException();
        }
    }
}
