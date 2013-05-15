using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Framework.Graphs
{
    public static class HeapGraphExtensions
    {
        /// <summary>
        /// This method will make all the predecessors and successors of the elements of the
        /// input vertices as the preds and succs of rep. Finally it will remove all the input 
        /// vertices from the graph
        /// Precondition: 
        /// (a) 'rep' does not belong to 'vertices'
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="rep"></param>
        public static void CollapseVertices(this HeapGraphBase hg, ICollection<HeapVertexBase> vertices, HeapVertexBase rep)
        {
            MergeEdges(hg, vertices, rep);
            //remove old vertices
            hg.RemoveVertices(vertices);
        }

        /// <summary>      
        /// This method will make all the predecessors and successors of the elements of the
        /// input vertices as the preds and succs of rep.
        /// </summary>
        /// <param name="hg"></param>
        /// <param name="vertices"></param>
        /// <param name="rep"></param>
        public static void MergeEdges(this HeapGraphBase hg, ICollection<HeapVertexBase> vertices, HeapVertexBase rep)
        {
            if (!hg.ContainsVertex(rep))
                hg.AddVertex(rep);

            var inedges = from vertex in vertices
                          from inEdge in hg.InEdges(vertex)
                          select inEdge;

            var outedges = from vertex in vertices
                           from outEdge in hg.OutEdges(vertex)
                           select outEdge;

            //add all the predecessors
            foreach (var inedge in inedges)
            {
                HeapEdgeBase newedge = null;
                if (inedge is ExternalHeapEdge)
                    newedge = new ExternalHeapEdge(inedge.Source, rep, inedge.Field);
                else if (inedge is InternalHeapEdge)
                    newedge = new InternalHeapEdge(inedge.Source, rep, inedge.Field);

                if (!hg.ContainsHeapEdge(newedge))
                    hg.AddEdge(newedge);
            }

            //add all the sucessors
            foreach (var outedge in outedges)
            {
                HeapEdgeBase newedge = null;
                if (outedge is ExternalHeapEdge)
                    newedge = new ExternalHeapEdge(rep, outedge.Target, outedge.Field);
                else if (outedge is InternalHeapEdge)
                    newedge = new InternalHeapEdge(rep, outedge.Target, outedge.Field);

                if (!hg.ContainsHeapEdge(newedge))
                    hg.AddEdge(newedge);
            }
        }

        /// <summary>
        /// VertexVistor - the return value = false means continue search;
        ///  true means terminate search.
        /// </summary>
        /// <param name="roots"></param>
        /// <param name="vertexVisitor"></param>
        /// <param name="EdgeVisitor"></param>
        public static void VisitBfs(
            this HeapGraphBase hg,
            IEnumerable<HeapVertexBase> roots,
            System.Func<HeapVertexBase, bool> vertexVisitor,
            System.Func<HeapEdgeBase, bool> edgeVisitor
            )
        {
            Queue<HeapVertexBase> Q = new Queue<HeapVertexBase>();
            HeapVertexSet visited = new HeapVertexSet();

            foreach (var v in roots)
            {
                if (!hg.ContainsVertex(v))
                    throw new SystemException("root: " + v + " not in the graph");

                visited.Add(v);
                Q.Enqueue(v);
            }

            while (Q.Count != 0)
            {
                HeapVertexBase v = Q.Dequeue();
                if (vertexVisitor != null)
                    vertexVisitor(v);

                foreach (HeapEdgeBase edge in hg.OutEdges(v))
                {
                    if (edgeVisitor != null)
                        edgeVisitor(edge);

                    if (!visited.Contains(edge.Target))
                    {
                        visited.Add(edge.Target);
                        Q.Enqueue(edge.Target);
                    }
                }
            }
        }

        public static HeapVertexSet GetReachableVertices(
            this HeapGraphBase hg,
            IEnumerable<HeapVertexBase> roots)
        {
            HeapVertexSet reachSet = new HeapVertexSet();
            VisitBfs(hg, roots,
                (HeapVertexBase v) =>
                {
                    reachSet.Add(v);
                    return false;
                },
                null);
            return reachSet;
        }
    }
}
