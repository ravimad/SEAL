using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using QuickGraph;
using QuickGraph.Algorithms.Search;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.Framework.Graphs
{    
    /// <summary>
    /// A simple,efficient heap graph without partitions
    /// </summary>     
    [Serializable]
    public partial class HeapGraph : HeapGraphBase, ISerializable
    {
        private BidirectionalGraph<HeapVertexBase, HeapEdgeBase> bigraph;
        private HashSet<HeapEdgeBase> alledges = new HashSet<HeapEdgeBase>();

        public override IEnumerable<HeapVertexBase> Vertices { get { return bigraph.Vertices; } }

        public override IEnumerable<HeapEdgeBase> Edges { get { return bigraph.Edges; } }

        public override int VertexCount { get { return bigraph.VertexCount; } }

        public override int EdgeCount { get { return bigraph.EdgeCount; } }        

        public override bool IsVerticesEmpty
        {
            get { return bigraph.IsVerticesEmpty; }
        }

        private uint version = 0;
        //tracks the changes done to this reference
        public  uint Version
        {
            get { return version; }
        }
                
        public HeapGraph()
        {
            this.bigraph = new BidirectionalGraph<HeapVertexBase, HeapEdgeBase>();

            //initialize the edge added/removed events
            this.bigraph.EdgeAdded += (HeapEdgeBase edge) => {
                if (alledges.Contains(edge))
                    throw new NotSupportedException("Trying to add duplicate edge: "+edge);
                alledges.Add(edge); 
            };
            this.bigraph.EdgeRemoved += (HeapEdgeBase edge) => { alledges.Remove(edge); };

            //intialize versioning
            VertexAction<HeapVertexBase> vact = (HeapVertexBase v) => { version++; };
            EdgeAction<HeapVertexBase, HeapEdgeBase> eact = (HeapEdgeBase e) => { version++; };
            this.bigraph.VertexAdded += vact;
            this.bigraph.VertexRemoved += vact;
            this.bigraph.EdgeAdded += eact;
            this.bigraph.EdgeRemoved += eact;        
        }

        public HeapGraph(SerializationInfo info, StreamingContext context) : this()
        {            
            int vertex_count = (int)info.GetValue("vertexcount", typeof(int));
            for (int i = 0; i < vertex_count; i++)
            {
                var nodeWrap = (HeapNodeWrapper)info.GetValue("vertex_" + i, typeof(HeapNodeWrapper));                
                this.AddVertex(nodeWrap.GetNode());                
            }
            int edge_count = (int)info.GetValue("edgecount", typeof(int));
            for (int i = 0; i < edge_count; i++)
            {
                var edgeWrap = (HeapEdgeWrapper)info.GetValue("edge_" + i, typeof(HeapEdgeWrapper));                
                this.AddEdge(edgeWrap.GetEdge());                
            }
        }
        
        /// <summary>
        /// This is an optimized version of contains edge that uses a hashset.        
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public override bool ContainsHeapEdge(HeapEdgeBase edge)
        {
            return alledges.Contains(edge);
        }

        public override void RemoveAllOutEdges(HeapVertexBase vertex)
        {
            this.bigraph.ClearEdges(vertex);              
        }                

        public override bool Equals(object obj)
        {            
            if (!(obj is HeapGraphBase))
                return false;

            HeapGraph graph = obj as HeapGraph;

            if (graph.VertexCount != this.VertexCount)
                return false;

            if (graph.EdgeCount != this.EdgeCount)
                return false;           

            if (this.ContainedIn(graph))
                return true;

            return false;
        }
        
        //use a much better hashcode creator
        public override int GetHashCode()
        {
            return (this.VertexCount ^ this.EdgeCount);
        }        

        public override HeapGraphBase Copy()
        {
            return DeepCopy();
        }

        public HeapGraph DeepCopy()
        {
            var graph = new HeapGraph();
            foreach (HeapVertexBase v in this.Vertices)                
                    graph.AddVertex(v);

            foreach (HeapEdgeBase edge in this.Edges)                            
                    graph.AddEdge(edge);                     
            return graph;
        }        
        
        public override string ToString()
        {
            StringWriter writer = new StringWriter();
            writer.WriteLine("HeapGraph - Vertices[{0}], Edges[{1}]", this.VertexCount, this.EdgeCount);

            foreach (HeapVertexBase vertex in this.Vertices)
            {
                // print vertex
                writer.WriteLine(
                    "#{0}: Type[{1}],Contents[{2}]",
                    vertex.Id,
                    vertex.GetType().Name,
                    vertex.ToString());

                // print edges
                if (!this.IsOutEdgesEmpty(vertex))
                {
                    foreach (HeapEdgeBase edge in this.OutEdges(vertex))
                    {
                        writer.WriteLine(
                            "\t{0}: Field[{1}],Target[{2}]",
                            edge.GetType().Name,
                            edge.Field == null ? "" : edge.Field.ToString(),
                            edge.Target.Id);
                    }
                }
            }
            return writer.GetStringBuilder().ToString();
        }       

        public override void Dump()
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("HeapGraph - Vertices[{0}], Edges[{1}]", this.VertexCount, this.EdgeCount);

            foreach (HeapVertexBase vertex in this.Vertices)
            {
                // print vertex
                Console.ForegroundColor = (vertex is ParameterHeapVertex) ? ConsoleColor.Green : (
                    (vertex is VariableHeapVertex) ? ConsoleColor.Blue : ConsoleColor.Cyan);
                Console.WriteLine(
                    "#{0}: Type[{1}],Contents[{2}]",
                    vertex.Id,
                    vertex.GetType().Name,
                    vertex.ToString());
                
                // print edges
                Console.ForegroundColor = ConsoleColor.Gray;
                if (!this.IsOutEdgesEmpty(vertex))
                {
                    foreach (HeapEdgeBase edge in this.OutEdges(vertex))
                    {
                        Console.WriteLine(
                            "\t{0}: Field[{1}],Target[{2}]",
                            edge.GetType().Name,
                            edge.Field == null ? "" : edge.Field.ToString(),
                            edge.Target.Id);
                    }
                }
            }

            Console.ForegroundColor = defaultColor;
        }

        public override void RemoveVertices(IEnumerable<HeapVertexBase> vrs)
        {
            foreach (var v in vrs.ToList())
                this.RemoveVertex(v);
        }

        public override void RemoveEdges(IEnumerable<HeapEdgeBase> edges)
        {
            foreach (var e in edges.ToList())
                this.RemoveEdge(e);
        }

        public override void AddEdge(HeapEdgeBase edge)
        {
            this.bigraph.AddEdge(edge);
        }

        public override void AddVertex(HeapVertexBase v)
        {
            this.bigraph.AddVertex(v);
        }

        public override void RemoveVertex(HeapVertexBase v)
        {
            this.bigraph.RemoveVertex(v);
        }

        public override void RemoveEdge(HeapEdgeBase e)
        {
            this.bigraph.RemoveEdge(e);
        }

        public override bool ContainsVertex(HeapVertexBase v)
        {
            return this.bigraph.ContainsVertex(v);
        }

        public override IEnumerable<HeapEdgeBase> OutEdges(HeapVertexBase v)
        {
            return this.bigraph.OutEdges(v);
        }

        public override IEnumerable<HeapEdgeBase> InEdges(HeapVertexBase v)
        {
            return this.bigraph.InEdges(v);
        }

        public override bool IsOutEdgesEmpty(HeapVertexBase v)
        {
            return bigraph.IsOutEdgesEmpty(v);
        }
        
        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //just store the vertices and edges            
            info.AddValue("vertexcount", this.Vertices.Count(), typeof(int));
            int i = 0;
            foreach (var vertex in this.Vertices)
            {
                info.AddValue("vertex_" + i, new HeapNodeWrapper(vertex), typeof(HeapNodeWrapper));
                i++;
            }

            info.AddValue("edgecount", this.Edges.Count(), typeof(int));
            i = 0;
            foreach (var edge in this.Edges)
            {
                info.AddValue("edge_" + i, new HeapEdgeWrapper(edge), typeof(HeapEdgeWrapper));
                i++;
            }           
        }

        #endregion                   
           
    }
}
