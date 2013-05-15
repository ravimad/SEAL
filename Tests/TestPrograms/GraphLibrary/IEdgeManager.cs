/*
 * 
 * User: Gavin Mead
 * Date: 11/30/2009
 * Time: 1:47 PM
 * 
 * 
 */
using System;
using System.Collections.Generic;

namespace GraphLibrary
{
    /// <summary>
    /// Description of IEdgeManager.
    /// </summary>
    internal interface IEdgeManager<U>
    {
        /// <summary>
        /// Creates an edge based on the EdgeType specified at graph initialization.  The caller is
        /// responsible for verifying that the IVertexId belong to the graph.
        /// </summary>
        /// <param name="edgeData">The edge data associated with the graph.  Can be null.</param>
        /// <param name="vertexId1">An IVertexId of an existing vertex.  Cannot be null.</param>
        /// <param name="vertexId2">An IVertexId of an existing vertex.  Cannot be null.</param>
        /// <returns>An edge which can be added to the IGraph's edge list.</returns>
        /// <exception cref="ArgumentNullException">If vertexId1 or vertexId2 are null.</exception>
        IEdge<U> CreateEdge(U edgeData, IVertexId vertexId1, IVertexId vertexId2);
        
        /// <summary>
        /// Returns the edge list for a vertex based on the type of edge specified
        /// at graph initialization.
        /// </summary>
        /// <param name="vertexId">The vertex whose edge list is request.  Cannot be null.</param>
        /// <returns>The edge list for the vertex, or an empty list if edge relate to this
        /// vertex.</returns>
        /// <exception cref="ArgumentNullException">If either parameter is null.</exception>
        IList<IEdgeId> GetEdgeList(IVertexId vertexId);
        
        IList<IEdge<U>> EdgeList { set;}
        
    }
}
