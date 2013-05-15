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
    public class LosslessNodeMerger 
    {        
        //IEnumerable<HeapVertexBase> globallyEscapingVertices;        
        public static NodeMerger CreateNodeMerger()             
        {
            return new NodeMerger(LosslessNodeMerger.InitialMergeableNodes, LosslessNodeMerger.MergeableTargets);
        }        

        public static bool InitialMergeableNodes(PurityAnalysisData data,
            List<HeapVertexSet> mergeableVertices)
        {
            //populate the nodes that can be merged.
            NodeMerger.PopulateMergableExceptionNodes(data, mergeableVertices);
            NodeMerger.PopulateStringNodes(data, mergeableVertices);
            foreach (var vertex in data.OutHeapGraph.Vertices)
            {
                LosslessNodeMerger.PopulateMergableReadNodes(data, vertex, mergeableVertices);
                LosslessNodeMerger.PopulateMergableWriteNodes(data, vertex, mergeableVertices);
            }
            return true;
        }

        public static bool MergeableTargets(PurityAnalysisData data, 
            HeapVertexBase vertex, 
            List<HeapVertexSet> mergeableVertices)
        {
            LosslessNodeMerger.PopulateMergableReadNodes(data, vertex, mergeableVertices);
            LosslessNodeMerger.PopulateMergableWriteNodes(data, vertex, mergeableVertices);
            return true;
        }
        
        /// <summary>
        /// Read nodes could be only external nodes
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="mergableVertices"></param>
        public static void PopulateMergableReadNodes(PurityAnalysisData data, 
            HeapVertexBase vertex,
            List<HeapVertexSet> mergableVertices)
        {
            var edgeGroups =
                    from outEdge in data.OutHeapGraph.OutEdges(vertex).OfType<ExternalHeapEdge>()                    
                    group outEdge by outEdge.Field;

            //Console.ForegroundColor = ConsoleColor.Green;
            foreach (var group in edgeGroups)
            {
                //Console.WriteLine("Read Group: ");
                HeapVertexSet toMergeVertices = new HeapVertexSet();
                foreach (var edge in group)
                {                                        
                    //check if the target is not the dest. of other external edges
                    var extedges = data.OutHeapGraph.InEdges(edge.Target).OfType<ExternalHeapEdge>();
                    if (extedges.Count() > 1)
                        continue;

                    //Console.WriteLine("\tCollapsing Read edges: " + edge);
                    toMergeVertices.Add(edge.Target);
                }
                //if there are atleast two vertices
                if (toMergeVertices.Count > 1)
                {
                    //Console.WriteLine(toMergeVertices);
                    mergableVertices.Add(toMergeVertices);
                }
            }
            //Console.ResetColor();
        }

        /// <summary>
        /// Write nodes could be any node (load, internal or parameter).
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="mergableVertices"></param>
        public static void PopulateMergableWriteNodes(PurityAnalysisData data, HeapVertexBase vertex,
            List<HeapVertexSet> mergableVertices)
        {                                   
            var edgeGroups =
                    from outEdge in data.OutHeapGraph.OutEdges(vertex).OfType<InternalHeapEdge>()                    
                    group outEdge by outEdge.Field;

            //Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var group in edgeGroups)
            {
                //Console.WriteLine("Write Group: ");
                HeapVertexSet toMergeVertices = new HeapVertexSet();
                foreach (var edge in group)
                {
                    if (edge.Target is GlobalLoadVertex)
                        continue;                                       

                    if (edge.Target is InternalHeapVertex)
                    {
                        //check if the target does not have any other internal edge
                        //check if the target is not the dest. of other external edges
                        var otheredges = data.OutHeapGraph.InEdges(edge.Target);
                        if (otheredges.Count() > 1)
                            continue;

                        toMergeVertices.Add(edge.Target);
                    }
                    ////else if ((edge.Target is LoadHeapVertex) &&
                    ////    globallyEscapingVertices.Contains(edge.Target))
                    //else if (edge.Target is LoadHeapVertex)
                    //{
                    //    toMergeVertices.Add(edge.Target);
                    //}                    
                }
                //if there are atleast two vertices to merge
                if (toMergeVertices.Count > 1)
                {
                    //Console.WriteLine(toMergeVertices);
                    mergableVertices.Add(toMergeVertices);
                }
            }
            //Console.ResetColor();
        }    
    }
}
