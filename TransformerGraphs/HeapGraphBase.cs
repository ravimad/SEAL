using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Framework.Graphs
{
    [Serializable]
    public abstract class HeapGraphBase
    {
        public abstract IEnumerable<HeapVertexBase> Vertices { get;  }

        public abstract  IEnumerable<HeapEdgeBase> Edges { get; }

        public abstract int VertexCount { get; }

        public abstract int EdgeCount { get; }

        public abstract bool IsVerticesEmpty { get; }
        
        public abstract void AddEdge(HeapEdgeBase edge);

        public abstract void AddVertex(HeapVertexBase v);

        public abstract void RemoveVertex(HeapVertexBase v);

        public abstract void RemoveEdge(HeapEdgeBase e);

        public abstract void RemoveVertices(IEnumerable<HeapVertexBase> vrs);

        public abstract void RemoveEdges(IEnumerable<HeapEdgeBase> edges);

        public abstract bool ContainsVertex(HeapVertexBase v);

        public abstract bool ContainsHeapEdge(HeapEdgeBase edge);

        public abstract IEnumerable<HeapEdgeBase> OutEdges(HeapVertexBase v);

        public abstract IEnumerable<HeapEdgeBase> InEdges(HeapVertexBase v);        

        //this is used in strong updates
        public abstract void RemoveAllOutEdges(HeapVertexBase vertex);

        /// <summary>
        /// Will not copy vertex state
        /// </summary>
        /// <returns></returns>
        public abstract HeapGraphBase Copy();

        public abstract void Dump();

        public virtual bool ContainedIn(HeapGraphBase hg)
        {
            foreach (var v in this.Vertices)
            {
                if (!hg.ContainsVertex(v))
                    return false;
            }

            foreach (var e in this.Edges)
            {
                if (!hg.ContainsHeapEdge(e))
                    return false;
            }
            return true;
        }

        public virtual void Union(HeapGraphBase graph)
        {
            foreach (HeapVertexBase vertex in graph.Vertices)
            {
                if (!this.ContainsVertex(vertex))
                    this.AddVertex(vertex);
            }

            foreach (HeapEdgeBase edge in graph.Edges)
            {
                if (!this.ContainsHeapEdge(edge))
                    this.AddEdge(edge);
            }
        }

        public abstract bool IsOutEdgesEmpty(HeapVertexBase v);        
    }
}
