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
    public class LossyNodeMerger 
    {        
        //IEnumerable<HeapVertexBase> globallyEscapingVertices;        
        public static NodeMerger CreateNodeMerger()             
        {
            return new NodeMerger(LossyNodeMerger.InitialMergeableNodes, LossyNodeMerger.MergeableTargets);
        }        

        public static bool InitialMergeableNodes(PurityAnalysisData data,
            List<HeapVertexSet> mergeableVertices)
        {
            //populate the nodes that can be merged.
            NodeMerger.PopulateMergableExceptionNodes(data, mergeableVertices);
            NodeMerger.PopulateStringNodes(data, mergeableVertices);
            foreach (var vertex in data.OutHeapGraph.Vertices)
            {
                LossyNodeMerger.PopulateMergableReadNodes(data, vertex, mergeableVertices);
                LossyNodeMerger.PopulateMergableWriteNodes(data, vertex, mergeableVertices);
            }
            return true;
        }

        public static bool MergeableTargets(PurityAnalysisData data, 
            HeapVertexBase vertex, 
            List<HeapVertexSet> mergeableVertices)
        {
            LossyNodeMerger.PopulateMergableReadNodes(data, vertex, mergeableVertices);
            LossyNodeMerger.PopulateMergableWriteNodes(data, vertex, mergeableVertices);
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

                    toMergeVertices.Add(edge.Target);

                    //if (edge.Target is InternalHeapVertex)
                    //{
                    //    toMergeVertices.Add(edge.Target);
                    //}
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
