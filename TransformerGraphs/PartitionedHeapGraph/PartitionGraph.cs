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
    /// This is a graph with two partitions : one partition is non-shared and is deeply copied,
    /// the other partition is shared and is shallow copied. Elements can be  moved from non-shared to shared.    
    /// </summary>     
    [Serializable]
    public partial class PartitionGraph : HeapGraphBase
    {
        HeapGraph shared;
        HeapGraph unique;

        public override IEnumerable<HeapVertexBase> Vertices
        {
            get
            {
                var list = new List<HeapVertexBase>();
                list.AddRange(this.shared.Vertices);

                list.AddRange(this.unique.Vertices.Where(
                    (HeapVertexBase v) => (!this.shared.Vertices.Contains(v))));
                return list;
            }
        }

        public override IEnumerable<HeapEdgeBase> Edges
        {
            get
            {
                var list = new List<HeapEdgeBase>();
                list.AddRange(this.shared.Edges);

                list.AddRange(this.unique.Edges.Where(
                    (HeapEdgeBase e) => (!this.shared.Edges.Contains(e))));
                return list;
            }
        }

        public override int VertexCount
        {
            get
            {
                var count = this.shared.VertexCount;
                count += this.unique.Vertices.Where(
                    (HeapVertexBase v) => (!this.shared.Vertices.Contains(v))).Count();
                return count;
            }
        }

        public override int EdgeCount
        {
            get
            {
                var count = this.shared.EdgeCount;
                count += this.unique.Edges.Where(
                    (HeapEdgeBase e) => (!this.shared.Edges.Contains(e))).Count();
                return count;
            }
        }

        public override bool IsVerticesEmpty
        {
            get { return (this.shared.IsVerticesEmpty && this.unique.IsVerticesEmpty); }
        }

        //version number for the shared state
        private uint sharedVersion = 0;
        public  uint SharedVersion
        {
            get { return sharedVersion; }
        }

        public static PartitionGraph New()
        {
            var pgraph = new PartitionGraph();
            pgraph.shared = new HeapGraph();
            pgraph.unique = new HeapGraph();
            return pgraph;
        }

        private PartitionGraph()
        {
        }

        public override bool ContainsHeapEdge(HeapEdgeBase edge)
        {
            if (this.shared.ContainsHeapEdge(edge))
                return true;
            if (this.unique.ContainsHeapEdge(edge))
                return true;
            return false;
        }

        public override bool ContainsVertex(HeapVertexBase v)
        {
            if (this.shared.ContainsVertex(v))
                return true;
            if (this.unique.ContainsVertex(v))
                return true;
            return false;
        }

        public override IEnumerable<HeapEdgeBase> OutEdges(HeapVertexBase v)
        {
            IEnumerable<HeapEdgeBase> e = null;
            if (this.shared.ContainsVertex(v))
                e = this.shared.OutEdges(v);

            if (this.unique.ContainsVertex(v))
            {
                e = (e == null) ?
                    this.unique.OutEdges(v) : e.Union(this.unique.OutEdges(v));
            }
            return e;
        }

        public override HeapGraphBase Copy()
        {
            var pgraph = new PartitionGraph();
            pgraph.shared = this.shared.DeepCopy();
            pgraph.unique = this.unique.DeepCopy();
            return pgraph;
        }

        public override int GetHashCode()
        {
            return this.unique.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PartitionGraph))
                return false;
            var pgraph = obj as PartitionGraph;
            return (this.shared.Equals(pgraph.shared)) && this.unique.Equals(pgraph.unique);
        }

        public override string ToString()
        {
            var str = "SharedGraph: \n";
            str += shared.ToString();
            str += "LocalGraph: \n";
            str += unique.ToString();
            return str;
        }

        public override void Dump()
        {
            Console.WriteLine("SharedGraph: \n");
            shared.Dump();
            Console.WriteLine("LocalGraph: \n");
            unique.Dump();
        }

        /// <summary>
        /// All edges on shared vertices are added to shared edge list
        /// </summary>
        /// <param name="edge"></param>
        public override void AddEdge(HeapEdgeBase edge)
        {
            if (this.shared.ContainsVertex(edge.Source))
            {
                if (!this.shared.ContainsVertex(edge.Target))
                    this.shared.AddVertex(edge.Target);

                this.shared.AddEdge(edge);
                this.sharedVersion++;
            }
            else
            {
                if (!this.unique.ContainsVertex(edge.Target))
                    this.unique.AddVertex(edge.Target);

                this.unique.AddEdge(edge);
            }
        }

        //By default adds vertices to shared partition
        public override void AddVertex(HeapVertexBase v)
        {
            this.shared.AddVertex(v);
            this.sharedVersion++;
        }

        public void AddVertexUnique(HeapVertexBase v)
        {
            this.unique.AddVertex(v);
        }

        public override void RemoveVertex(HeapVertexBase v)
        {
            if (this.shared.ContainsVertex(v))
            {
                this.shared.RemoveVertex(v);
                this.sharedVersion++;
            }

            if (this.unique.ContainsVertex(v))
            {
                this.unique.RemoveVertex(v);
            }
        }

        public override void RemoveEdge(HeapEdgeBase e)
        {
            if (this.shared.ContainsHeapEdge(e))
            {
                this.shared.RemoveEdge(e);
                this.sharedVersion++;
            }

            if (this.unique.ContainsHeapEdge(e))
                this.unique.RemoveEdge(e);
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

        public override IEnumerable<HeapEdgeBase> InEdges(HeapVertexBase v)
        {
            IEnumerable<HeapEdgeBase> e = null;
            if (this.shared.ContainsVertex(v))
                e = this.shared.InEdges(v);

            if (this.unique.ContainsVertex(v))
            {
                e = (e == null) ?
                    this.unique.InEdges(v) : e.Union(this.unique.InEdges(v));
            }
            return e;
        }

        public override void RemoveAllOutEdges(HeapVertexBase v)
        {
            if (this.shared.ContainsVertex(v))
            {
                this.shared.RemoveAllOutEdges(v);
                this.sharedVersion++;
            }

            if (this.unique.ContainsVertex(v))
                this.unique.RemoveAllOutEdges(v);
        }

        /// <summary>
        /// Implements an efficient way to check containment
        /// </summary>
        /// <param name="hg"></param>
        /// <returns></returns>
        public override bool ContainedIn(HeapGraphBase hg)
        {
            return this.shared.ContainedIn(hg) && this.unique.ContainedIn(hg);
        }

        public override bool IsOutEdgesEmpty(HeapVertexBase v)
        {
            return ((!this.shared.ContainsVertex(v) || this.shared.IsOutEdgesEmpty(v))
                        && (!this.unique.ContainsVertex(v) || this.unique.IsOutEdgesEmpty(v)));
        }        

        public PartitionGraph CopyNonShared()
        {
            var pgraph = new PartitionGraph();
            //shallow copy
            pgraph.shared = this.shared;
            //deep copy
            pgraph.unique = this.unique.DeepCopy();
            //copy the version number
            pgraph.sharedVersion = this.sharedVersion;
            return pgraph;
        }

        public void UnionNonShared(PartitionGraph pg)
        {
            //merge unique graphs
            //as a side effect we go to a more efficient representation
            foreach (var edge in pg.unique.Edges)
            {
                if (!this.shared.ContainsHeapEdge(edge))
                {
                    if (!this.unique.ContainsHeapEdge(edge))
                    {
                        if (!this.unique.ContainsVertex(edge.Source))
                            this.unique.AddVertex(edge.Source);

                        if (!this.unique.ContainsVertex(edge.Target))
                            this.unique.AddVertex(edge.Target);

                        this.unique.AddEdge(edge);
                    }
                }
            }
        }

        public bool EqualsNonShared(PartitionGraph pg)
        {
            return this.unique.Equals(pg.unique);
        }
        
        public void MoveOutEdgesToShared(HeapVertexBase v)
        {
            //old shared version
            var sharedVer = this.shared.Version;

            if (!this.shared.ContainsVertex(v))
                this.shared.AddVertex(v);

            if (this.unique.ContainsVertex(v))
            {
                foreach (var oute in this.unique.OutEdges(v))
                {
                    if (!this.shared.ContainsVertex(oute.Target))
                        this.shared.AddVertex(oute.Target);

                    if (!this.shared.ContainsHeapEdge(oute))
                        this.shared.AddEdge(oute);
                }
                this.unique.RemoveAllOutEdges(v);
            }

            //check if the shared state has changed
            if (this.shared.Version != sharedVer)
                this.sharedVersion++;
        }

        //public HeapGraph ConvertToPlainGraph()
        //{
        //    var graph = new HeapGraph();
        //    graph.Union(this.shared);
        //    graph.Union(this.unique);
        //    return graph;
        //}
    }
}
