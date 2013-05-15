/*
 * 
 * User: Gavin Mead
 * Date: 11/19/2009
 * Time: 11:59 PM
 * 
 * 
 */
using System;
using System.Collections.Generic;

namespace GraphLibrary
{
   
    /// <summary>
    /// Defines operations which occur on all graph implementations.  The reason for T and U
    /// types is that the vertex data can be different from the edge data.  Although it is certainly
    /// allowed if T=U.  Note. that mentions to allowing null for both T and U implies reference types.
    /// </summary>
    public interface IGraph<T,U>
    {
        /// <summary>
        /// Creates a new vertex in the graph with the specified vertexData.  vertexData
        /// can be null.
        /// </summary>
        /// <param name="vertexData">Data to be placed in the vertex.</param>
        /// <returns>A IVertexId which can uniquely identify the vertex within
        /// the graph independent of the data.</returns>
        IVertexId AddVertex(T vertexData);

        /// <summary>
        /// Creates two vertices and puts an edge between them.  The implementation
        /// is responsible for determining the constraints between vertices.
        /// </summary>
        /// <param name="edgeData">Data to be placed in the edge.  Can be null.</param>
        /// <param name="vertex1Data">Data to be placed in the vertex.  Can be null.</param>
        /// <param name="vertex2Data">Data to be placed in the vertex.  Can be null.</param>
        /// <returns>An IEdgeId which uniquely identifies the edge.</returns>
        IEdgeId AddEdge(U edgeData, T vertex1Data, T vertex2Data);

        /// <summary>
        /// Creates an edge between two existing vertices.  The implementation
        /// is responsible for determining the constraints between vertices.
        /// </summary>
        /// <param name="edgeData">Data to be placed in the edge.  Can be null.</param>
        /// <param name="vertexId1">Existing vertexId.  Cannot be null.</param>
        /// <param name="vertexId2">Existing vertexId.  Cannot be null.</param>
        /// <returns>An IEdge with associated IVertexIds.</returns>
        /// <exception cref="ArgumentException">If either of vertexId's cannot be found.</exception>
        /// <exception cref="ArgumentNullException">If either of the vertexId's are null.</exception>
        /// <exception cref="GraphException">If adding the edge violates the
        /// property of a graph.  For example if the implementation is a directed graph, there
        /// can only be 1 edge going from vertex1 to vertex2.</exception>
        IEdgeId AddEdge(U edgeData, IVertexId vertexId1, IVertexId vertexId2);
		
        /// <summary>
        /// Removes the vertex from the graph and any edges associated with it.
        /// </summary>
        /// <param name="vertexId">The vertexId of the vertex.  Cannot be null.</param>
        /// <returns>True, if the deletion was successful.  False, if the vertexId
        /// could not be found.</returns>
        /// <exception cref="ArgumentNullException">If vertexId is null.</exception>
        bool DeleteVertex(IVertexId vertexId);

        /// <summary>
        /// Removes an edge from the graph.  This does not remove the associated vertices.
        /// </summary>
        /// <param name="edgeId">The edgeId of the edge.  Cannot be null.</param>
        /// <returns>True, if the deletion was successful.  False, if the vertexId
        /// could not be found.</returns>
        /// <exception cref="ArgumentNullException">If edgeId is null.</exception>
        bool DeleteEdge(IEdgeId edgeId);

        /// <summary>
        /// Replaces the contents of the vertex with the provided data.  This method can also
        /// act as an 'update' of the vertex data.
        /// </summary>
        /// <param name="vertexId">The vertexId of the vertex.  Cannot be null.</param>
        /// <param name="vertexData">The vertex data to use.  Can be null.</param>
        /// <returns>True, if the vertexId was found.  False, if the vertexId
        /// could not be found.</returns>
        bool ReplaceVertex(IVertexId vertexId, T vertexData);

        /// <summary>
        /// Replaces the contents of the edge with the provided data.  This method can also
        /// act an as 'update' of the edge data.
        /// </summary>
        /// <param name="edgeId">The edgeId of the edge.  Cannot be null.</param>
        /// <param name="edgeData">The edge data to use.  Can be null.</param>
        /// <returns>True if th edgeId was found.  False, if the edgeId was not
        /// found.</returns>
        bool ReplaceEdge(IEdgeId edgeId, U edgeData);

        /// <summary>
        /// Retrieves all the edges associated with this vertex.
        /// </summary>
        /// <param name="vertexId">id associated with the vertex.  Cannot be null.</param>
        /// <returns>A list of IEdgeId's related to this vertex, or null if no
        /// edges are associated with this vertex.</returns>
        IList<IEdgeId> GetEdgeList(IVertexId vertexId);

        /// <summary>
        /// Returns the data associated with a given vertex.
        /// </summary>
        /// <param name="vertexId">id associated with the vertex.  Cannot be null.</param>
        /// <returns>Associated vertex data. If the data was null to begin with, default(T) is returned.</returns>
        /// <exception cref="ArgumentNullException">vertexId is null.</exception>
        /// <exception cref="ArgumentException">vertexId cannot be found.</exception>
        T GetVertexData(IVertexId vertexId);

        /// <summary>
        /// Returns the data associated with a given edge.
        /// </summary>
        /// <param name="edgeId">id associated with the edge.  Cannot be null.</param>
        /// <returns>Associated edge data.  If the edgeId does not exist
        /// null will be returned.  If the data was null to begin with, null
        /// will be returned.</returns>
        /// <exception cref="ArgumentNullException">edgeId is null.</exception>
        U GetEdgeData(IEdgeId edgeId);
        
    }
}
