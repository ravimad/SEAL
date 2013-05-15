/*
 * 
 * User: Gavin Mead
 * Date: 11/21/2009
 * Time: 3:53 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{
    
    
    /// <summary>
    /// Defines how to find a path between vertices.
    /// </summary>
    public interface IGraphPathFinder<T,U>
    {
        IGraph<T, U> FindPath(IVertexId startVertex, IVertexId endVertex);
    }
}
