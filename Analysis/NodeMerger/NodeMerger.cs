using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.Summaries;
using SafetyAnalysis.Util;
using QuickGraph.Collections;

namespace SafetyAnalysis.Purity
{    
    public class NodeMerger
    {                                     
        protected Func<PurityAnalysisData,List<HeapVertexSet>, bool> initializer;
        protected Func<PurityAnalysisData,HeapVertexBase, List<HeapVertexSet>, bool> targetSelector;

        public NodeMerger(Func<PurityAnalysisData, List<HeapVertexSet>, bool> init,
            Func<PurityAnalysisData, HeapVertexBase, List<HeapVertexSet>, bool> tarSel)
        {            
            initializer = init;
            targetSelector = tarSel;
        }        

        public void MergeNodes(PurityAnalysisData data)
        {            
            List<HeapVertexSet> mergableVertices = new List<HeapVertexSet>();

            //initialize nodes
            initializer(data,mergableVertices);

            //Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.WriteLine("Before reduce: [" + purityData.OutHeapGraph.VertexCount
            //    + "," + purityData.OutHeapGraph.EdgeCount + "]");                        
            int old_edges_cnt;
            int old_vertices_cnt;
            HeapVertexSet repNodes = new HeapVertexSet();
            do
            {
                old_edges_cnt = data.OutHeapGraph.EdgeCount;
                old_vertices_cnt = data.OutHeapGraph.VertexCount;

                repNodes.Clear();                
                               
                HeapVertexSet loadVertices = new HeapVertexSet();
                HeapVertexSet internalVertices = new HeapVertexSet();
                HeapVertexSet returnValueVertices = new HeapVertexSet();

                foreach (var vertices in mergableVertices)
                {
                    loadVertices.Clear();
                    internalVertices.Clear();
                    returnValueVertices.Clear();

                    foreach (var v in vertices)
                    {
                        //note that some vertices may not be in the outHeapGraph 
                        //because of preceeding collapses
                        if (!data.OutHeapGraph.ContainsVertex(v))
                            continue;
                        if (v is GlobalLoadVertex)                     
                            continue;
                        if (v is LoadHeapVertex)
                            loadVertices.Add(v);
                        else if (v is InternalHeapVertex)
                            internalVertices.Add(v);
                        else if (v is ReturnedValueVertex)
                            returnValueVertices.Add(v);
                    }
                    if (loadVertices.Count() > 1)
                    {                        
                        var rep = PurityDataUtil.CollapseVertices(data, loadVertices);
                        repNodes.Add(rep);
                    }
                    if (internalVertices.Count() > 1)
                    {                                             
                        var rep = PurityDataUtil.CollapseVertices(data, internalVertices);
                        repNodes.Add(rep);
                    }
                    if (returnValueVertices.Count() > 1)
                    {                        
                        var rep = PurityDataUtil.CollapseVertices(data, returnValueVertices);
                        repNodes.Add(rep);
                    }
                }
               
                mergableVertices.Clear();                
                foreach (var vertex in repNodes)
                {
                    if (data.OutHeapGraph.ContainsVertex(vertex))
                    {
                        targetSelector(data, vertex, mergableVertices);                        
                    }
                }

            } while (data.OutHeapGraph.EdgeCount != old_edges_cnt
                || data.OutHeapGraph.VertexCount != old_vertices_cnt);

            //Console.WriteLine("After reduce: [" + purityData.OutHeapGraph.VertexCount
            //    + "," + purityData.OutHeapGraph.EdgeCount + "]");
            //Console.ResetColor();
        }

        /// <summary>
        /// Collapses all expections into a single. Could be lossy but losses are rare.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mergableVertices"></param>
        public static void PopulateMergableExceptionNodes(PurityAnalysisData data,
            List<HeapVertexSet> mergableVertices)
        {
            var exceptionVertex = ExceptionVertex.GetInstance();
            var reachableVertices = data.OutHeapGraph.GetReachableVertices(
                new List<HeapVertexBase> { exceptionVertex }).OfType<InternalHeapVertex>();
            HeapVertexSet toMergeVertices = new HeapVertexSet();
            foreach (var v in reachableVertices)
            {
                foreach (var typename in data.GetTypes(v))
                {
                    if (typename.Contains("Exception"))
                    {
                        toMergeVertices.Add(v);
                        break;
                    }
                }
            }
            mergableVertices.Add(toMergeVertices);
        }

        /// <summary>
        /// Collapse all nodes that correspond to strings. Non-lossy
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mergableVertices"></param>
        public static void PopulateStringNodes(PurityAnalysisData data, List<HeapVertexSet> mergableVertices)
        {
            HeapVertexSet toMergeVertices = new HeapVertexSet();
            foreach (var v in data.OutHeapGraph.Vertices)
            {
                if (AnalysisUtil.IsString(data, v))
                    toMergeVertices.Add(v);
            }
            mergableVertices.Add(toMergeVertices);
        }

    }
}
