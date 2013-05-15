/*
 * 
 * User: Gavin Mead
 * Date: 11/24/2009
 * Time: 3:33 PM
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;


namespace GraphLibrary
{
    /// <summary>
    /// BaseGraph performs all the operations that occur on a graph, regardless of the 
    /// edges used.
    /// </summary>
    [Serializable]
    internal class BaseGraph<T,U> : IGraph<T, U>
    {
        protected IList<IEdge<U>> edgeList;
        protected IList<IVertex<T>> vertexList;
        protected IEdgeManager<U> edgeManager;
        
        public BaseGraph(GraphTypes graphType)
        {
            edgeManager = EdgeManagerFactory<U>.CreateEdgeManager(graphType);
            edgeList = new List<IEdge<U>>();
            vertexList = new List<IVertex<T>>();
            
            edgeManager.EdgeList = edgeList;
        }
        
        public IVertexId AddVertex(T vertexData)
        {
            IVertexId vertexId = new BaseVertexId();
            IVertex<T> vertex = new BaseVertex<T>();
            vertex.VertexData = vertexData;
            vertexList.Add(vertex);
            
            return vertex.VertexId;
        }
        
        public IEdgeId AddEdge(U edgeData, T vertex1Data, T vertex2Data)
        {
           
            IVertexId vertex1 = null;
            IVertexId vertex2 = null;
            
            vertex1 = this.AddVertex(vertex1Data);
            vertex2 = this.AddVertex(vertex2Data);
            
            return AddEdge(edgeData, vertex1, vertex2);  
        }
        
        public IEdgeId AddEdge(U edgeData, IVertexId vertexId1, IVertexId vertexId2)
        {
            IVertex<T> vertex1;
            IVertex<T> vertex2;
            IEdge<U> newEdge = null;
            
            if(vertexId1 == null)
            {
                throw new ArgumentNullException("vertexId1 cannot be null");
            }
            
            if(vertexId2 == null)
            {
                throw new ArgumentNullException("vertexId2 cannot be null");
            }
            
            vertex1 = GetVertex(vertexId1);
            vertex2 = GetVertex(vertexId2);
            if(vertex1 == null || vertex2 == null)
            {
                throw new ArgumentException("One or more vertices could not be found.");
            }
            
            newEdge = edgeManager.CreateEdge(edgeData, vertexId1, vertexId2);
            edgeList.Add(newEdge);
            
            return newEdge.EdgeId;        
        }
        
        public bool DeleteVertex(IVertexId vertexId) 
        {
            IVertex<T> vertex = null;
            IList<IEdge<U>> edges = null;
            
            if(vertexId == null)
            {
                throw new ArgumentNullException("vertexId cannot be null");
            }
            
            vertex = GetVertex(vertexId);
            if(vertex == null)
            {
                return false;
            }
            
            //Get the list of edges in the edge list which contain this vertexId.
            edges = edgeList.Where(edge => edge.EdgeId.Vertex1Id.Equals(vertexId)
                                                         || edge.EdgeId.Vertex2Id.Equals(vertexId)).ToList();
            
            //Remove these edges from the edgeList.
            foreach(IEdge<U> edge in edges)
            {
                edgeList.Remove(edge);
            }
            
            //Remove vertexId from the vertexList.
            return vertexList.Remove(vertex);;
        }
        
        public bool DeleteEdge(IEdgeId edgeId)
        {
            IEdge<U> edge = null;
            
            if(edgeId == null)
            {
                throw new ArgumentNullException("edgeId cannot be null.");
            }
            
            edge = GetEdge(edgeId);
            if(edge == null)
            {
                return false;
            }
            
            return edgeList.Remove(edge);            
        }
        
        public bool ReplaceVertex(IVertexId vertexId, T vertexData)
        {
            IVertex<T> vertex = null;
            
            if(vertexId == null)
            {
                throw new ArgumentNullException("vertexId cannot be null.");
            }
            
            vertex = GetVertex(vertexId);
            if(vertex == null)
            {
                return false;
            }

            vertex.VertexData = vertexData;           
            
            return true;
        }
        
        public virtual bool ReplaceEdge(IEdgeId edgeId, U edgeData)
        {
            IEdge<U> edge = null;
            
            if(edgeId == null)
            {
                throw new ArgumentNullException("edgeId cannot be null");
            }
            
            edge = GetEdge(edgeId);
            
            if(edge != null)
            {
                edge.EdgeData = edgeData;
                return true;
            }
            else 
            {
                return false;
            }
            
        }
        
        public IList<IEdgeId> GetEdgeList(IVertexId vertexId)
        {
            IVertex<T> vertex = null;
            
            if(vertexId == null)
            {
                throw new ArgumentNullException("vertexId cannot be null");
            }
            
            vertex = GetVertex(vertexId);
            
            //Vertex was not found.
            if(vertex == null)
            {
                List<IEdgeId> emptyList = new List<IEdgeId>();
                return emptyList;
            }
            
            return edgeManager.GetEdgeList(vertexId);    
        }
        
        public T GetVertexData(IVertexId vertexId)
        {
            if(vertexId == null)
            {
                throw new ArgumentNullException("vertexId cannot be null.");
            }
            
            IVertex<T> vertex = GetVertex(vertexId);
            
            
            if(vertex != null)
            {
                return vertex.VertexData;
            }
            else
            {
                throw new ArgumentException("vertexId could not be found");
            }
               
        }
        
        public virtual U GetEdgeData(IEdgeId edgeId)
        {
            IEdge<U> edge = null;
            
            if(edgeId == null)
            {
                throw new ArgumentNullException("edgeId cannot be null");
            }
            
            edge = GetEdge(edgeId);
            if(edge == null)
            {
                return default(U);
            }
            else {
                return edge.EdgeData;
            }
           
        }
        
        protected IVertex<T> GetVertex(IVertexId vertexId)
        {
            IVertex<T> vertex;
            
            try {
                vertex = vertexList.Single(v => v.VertexId.Equals(vertexId));
            }
            catch(InvalidOperationException)
            {
                return null;
            }
            
            return vertex;
        }
        
        protected IEdge<U> GetEdge(IEdgeId edgeId)
        {
            IEdge<U> edge;
            
            try
            {
                edge = edgeList.Single(e => e.EdgeId.Equals(edgeId));
            }
            catch(InvalidOperationException)
            {
                return null;
            }
            
            return edge;
        }
    }
}
