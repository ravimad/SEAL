/*
 * 
 * User: Gavin Mead
 * Date: 11/20/2009
 * Time: 12:00 AM
 * 
 * 
 */
using System;
using System.Collections.Generic;

namespace GraphLibrary
{
    
    /// <summary>
    /// Traverses a graph using a documented algorithm.   
    /// </summary>
    public interface IGraphTraverser<T,U> 
    {
        /// <summary>
        /// Traverses the graph starting at a given vertex.  No guarantee is made
        /// that all vertices are returned as the result of the traversal.
        /// </summary>
        /// <param name="startVertex">Starting vertex.  Cannot be null.</param>
        /// <returns>An ordered list of the vertices within the graph that can be
        /// accessed from the startVertex.</returns>
        /// <exception cref="ArgumentNullException">If startVertex is null.</exception>
        /// <exception cref="InvalidOperationException">If Graph property is not
        /// set prior to calling this method.</exception>
        /// <exception cref="ArgumentException">If the IVertexId does not exist 
        /// in the graph.</exception>
        IList<T> Traverse(IVertexId startVertex);
        
        /// <summary>
        /// The Graph to Traverse.  This must be set prior to calling Traverse.
        /// </summary>
        IGraph<T, U> Graph { set; }
    }
}
