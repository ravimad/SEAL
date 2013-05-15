/*
 * 
 * User: Gavin Mead
 * Date: 11/30/2009
 * Time: 3:12 PM
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphLibrary
{
    /// <summary>
    /// Description of EdgeManager.
    /// </summary>
    internal class DirectedEdgeManager<U> : IEdgeManager<U>
    {
        
        private IList<IEdge<U>> edgeList;
        
        public DirectedEdgeManager()
        {
        }
        
        public IEdge<U> CreateEdge(U edgeData, IVertexId vertexId1, IVertexId vertexId2)
        {
            IDirectedEdge<U> newEdge = EdgeFactory<U>.CreateEdge(EdgeType.DirectedEdge) as IDirectedEdge<U>;
            if(vertexId1 == null)
            {
                throw new ArgumentNullException("vertexId1 cannot be null.");
            }
            
            if(vertexId2 == null)
            {
                throw new ArgumentNullException("vertexId2 cannot be null.");
            }
            
            //See if an edge exists between these two vertices.
            IList<IEdgeId> existingEdges = GetEdgeList(vertexId1);
            foreach(IEdgeId edgeId in existingEdges)
            {
                if(edgeId.Vertex2Id == vertexId2)
                {
                    //The edge already exists, throw an exception.
                    throw new GraphException("A directed graph can only have one directed edge going " +
                                             "from vertex 1 to vertex 2.");
                }
            }
            
            newEdge.EdgeId.Vertex1Id = vertexId1;
            newEdge.EdgeId.Vertex2Id = vertexId2;
            newEdge.OutboundVertex = vertexId1;
            newEdge.EdgeData = edgeData;
            return newEdge;
        }
        
        public IList<IEdgeId> GetEdgeList(IVertexId vertexId)
        {
            IList<IEdgeId> vertexEdgeList = null;
            
            if(vertexId == null)
            {
                throw new ArgumentNullException("vertexId cannot be null");
            }
            
            if(edgeList == null)
            {
                throw new ArgumentNullException("edgeList cannot be null");
            }
            
                        //Get the edges where vertex is the outbound vertex.
            vertexEdgeList = new List<IEdgeId>();
            foreach(IEdge<U> edge in edgeList)
            {
                IDirectedEdge<U> directedEdge = edge as IDirectedEdge<U>;
                
                if(directedEdge.OutboundVertex.Equals(vertexId))
                {
                    vertexEdgeList.Add(directedEdge.EdgeId);
                }
            }
            
            return vertexEdgeList;
        }
        
        public IList<IEdge<U>> EdgeList {
            set {
                if(value is IList<IEdge<U>>)
                {
                    edgeList = value;
                }
                else 
                {
                    throw new InvalidOperationException("Cannot set non IEdge<U> list for this property");
                }
            }
        }
    }
}
