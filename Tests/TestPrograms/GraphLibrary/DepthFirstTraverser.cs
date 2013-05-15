/*
 * 
 * User: Gavin Mead
 * Date: 12/1/2009
 * Time: 2:45 PM
 * 
 * 
 */
using System;
using System.Collections.Generic;

namespace GraphLibrary
{
    /// <summary>
    /// Description of DepthFirstTraverser.
    /// </summary>
    public class DepthFirstTraverser<T,U> : IGraphTraverser<T, U>
    {
        IGraph<T, U> graph;
        IList<IVertexId> visitedVertices = null;
        Stack<IVertexId> dfsStack = null;
        IList<T> dfsResults = null;
        
        public DepthFirstTraverser()
        {
            visitedVertices = new List<IVertexId>();
            dfsStack = new Stack<IVertexId>();
            dfsResults = new List<T>();
        }
        
        public IGraph<T, U> Graph {
            set 
            {
                graph = value;
            }
        }
        
        /// <summary>
        /// Performs a depth first traversal on the Graph.  An important note:
        /// The traverser will retrieve the edge list of the vertex using
        /// IGraph.GetEdgeList(IVertexId) and the neighbors will be added in the order
        /// they were received via the edge list.
        /// </summary>
        /// <param name="startVertex">initial vertex to be traversed.</param>
        /// <returns>An ordered list of vertex data.</returns>
        /// <example>
        ///     <code>
        ///     IGraph{int, int} graph = GraphFactory{int, int}.CreateGraph(GraphTypes.Undirected);
        ///    
        ///     IVertexId v1 = graph.AddVertex(1);
        ///     IVertexId v2 = graph.AddVertex(2);
        ///     IVertexId v3 = graph.AddVertex(3);
        ///     IVertexId v4 = graph.AddVertex(4);
        ///        
        ///     IEdgeId v1Tov2 = graph.AddEdge(1, v1, v2);
        ///     IEdgeId v1Tov3 = graph.AddEdge(2, v1, v3);
        ///     IEdgeId v2Tov4 = graph.AddEdge(3, v2, v4);
        ///     IEdgeId v3Tov4 = graph.AddEdge(4, v3, v4);
        /// 
        ///     IGraphTraverser{int,int} dfs = new DepthFirstTraverser{int,int}();
        ///     dfs.Graph = graph;
        ///     IList{int} results = dfs.Traverse(v1);
        ///
        ///     //Results = [1, 3, 4, 2] because v1's edge list 
        ///     //returns v1Tov2, v1Tov3;  v1Tov3 will be processed first.
        ///      
        ///     
        ///     </code>
        /// </example>
        public IList<T> Traverse(IVertexId startVertex)
        {
            visitedVertices.Clear();
            dfsStack.Clear();
            dfsResults.Clear();
            
            if(startVertex == null)
            {
                throw new ArgumentNullException("startVertex cannot be null");
            }
            
            if(graph == null)
            {
                throw new InvalidOperationException("Graph property has not be set.");
            }
            
            //Make sure vertex exists, IGraph.GetVertexData(IVertexId) throws the exception.
            T vertex = graph.GetVertexData(startVertex);
  
            DoDfs(startVertex);
            
            return dfsResults;
        }
        
        private void DoDfs(IVertexId startVertex)
        {
            IVertexId currentVertexId = null;
            dfsResults.Add(graph.GetVertexData(startVertex));
            visitedVertices.Add(startVertex);
            
            //Get the start vertex edge list.
            IList<IEdgeId> edgeList = graph.GetEdgeList(startVertex);
            ProcessEdgeList(edgeList, startVertex);
            
            while(dfsStack.Count != 0)
            {
                currentVertexId = dfsStack.Pop();
                #if(DEBUG)
                T vertexVal = graph.GetVertexData(currentVertexId);
                #endif
                //If it hasn't been visited yet, mark as visited.
                if(!visitedVertices.Contains(currentVertexId))
                {
                    //Get the vertex data and to visited vertices.
                    dfsResults.Add(graph.GetVertexData(currentVertexId));
                    visitedVertices.Add(currentVertexId);
                    
                    //Process it's edge list.
                    edgeList.Clear();
                    edgeList = graph.GetEdgeList(currentVertexId);
                    ProcessEdgeList(edgeList, currentVertexId);
                }
                
            }
        }
        
        private void ProcessEdgeList(IList<IEdgeId> vertexEdgeList, IVertexId currentVertex)
        {
            foreach(IEdgeId edgeId in vertexEdgeList)
            {
                #if(DEBUG)
                U edgeVal = graph.GetEdgeData(edgeId);
                #endif
                //Don't re-add the current vertex.
                if(edgeId.Vertex1Id.Equals(currentVertex))
                {
                    dfsStack.Push(edgeId.Vertex2Id);
                }
                else 
                {
                    dfsStack.Push(edgeId.Vertex1Id);
                }
            }
        }
        
        
    }
}
