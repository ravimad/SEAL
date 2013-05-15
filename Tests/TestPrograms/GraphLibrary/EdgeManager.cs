/*
 * 
 * User: Gavin Mead
 * Date: 11/30/2009
 * Time: 3:29 PM
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
    internal class EdgeManager<U> : IEdgeManager<U>
    {
        private IList<IEdge<U>> edgeList;
        
        public EdgeManager()
        {
        }
        
        public IEdge<U> CreateEdge(U edgeData, IVertexId vertexId1, IVertexId vertexId2)
        {
            IEdge<U> newEdge = EdgeFactory<U>.CreateEdge(EdgeType.UndirectedEdge);
            
            if(vertexId1 == null)
            {
                throw new ArgumentNullException("vertexId1 cannot be null.");
            }
            
            if(vertexId2 == null)
            {
                throw new ArgumentNullException("vertexId2 cannot be null.");
            }
            
            //Make an edge doesn't exit between these two vertices.  Get the intersection
            //of these lists.  If the intersection contains the edge, then an edge
            //already exists between them.
            IList<IEdgeId> vertex1List = GetEdgeList(vertexId1);
            IList<IEdgeId> vertex2List = GetEdgeList(vertexId2);
            
            IEnumerable<IEdgeId> intersection = vertex1List.Intersect(vertex2List);
            
            //No edge exists, so create one.
            if(intersection.Count() == 0)
            {
                newEdge.EdgeData = edgeData;
                newEdge.EdgeId.Vertex1Id = vertexId1;
                newEdge.EdgeId.Vertex2Id = vertexId2;
            
                return newEdge;
            }
            else 
            {
                throw new GraphException("An undirected graph can have at most 1 edge between a pair of " +
                                         "vertices.");
            }
        }
        
        public System.Collections.Generic.IList<IEdgeId> GetEdgeList(IVertexId vertexId)
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
            
            vertexEdgeList = new List<IEdgeId>();
            
            //Get the edges where vertex is referenced
            foreach(IEdge<U> edge in edgeList)
            {
                if(edge.EdgeId.Vertex1Id.Equals(vertexId)
                   || edge.EdgeId.Vertex2Id.Equals(vertexId))
                {
                    vertexEdgeList.Add(edge.EdgeId);
                }
            }
            
            return vertexEdgeList;
        }
        
        public IList<IEdge<U>> EdgeList {
            set {
                edgeList = value;
            }
        }
        
    }
}
