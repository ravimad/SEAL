using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{
    public class PurityReportUtil
    {
        public static HashSet<AccessPathRegexp> IsPure(PurityAnalysisData data, bool isConstructor)
        {
            var witnesses = new HashSet<AccessPathRegexp>();                        
            foreach (var vertex in data.OutHeapGraph.Vertices)
            {                
                if ((vertex is GlobalLoadVertex) || (vertex is ParameterHeapVertex))
                {
                    witnesses.UnionWith(
                        CheckPurityRecursive(data, vertex, isConstructor, 
                        new HeapVertexSet() ,new LinkedList<HeapEdgeBase>())
                        );
                }
            }
            return witnesses;
        }

        //Gets one access path from the 'vertex' to every modified nodes.
        //there might exist multiple access paths (if there is load node sharing).        
        private static IEnumerable<AccessPathRegexp> CheckPurityRecursive(
            PurityAnalysisData data,
            HeapVertexBase vertex,
            bool isConstructor,
            HeapVertexSet visited,
            LinkedList<HeapEdgeBase> stack          
            )
        {            
            if (visited.Contains(vertex))
                yield break;

            visited.Add(vertex);

            HashSet<Field> processedFields = new HashSet<Field>();
            foreach (var edge in data.OutHeapGraph.OutEdges(vertex).OfType<InternalHeapEdge>())
            {
                processedFields.Add(edge.Field);
                if (!(vertex is VariableHeapVertex))
                {
                    //this implies that the vertex is written
                    if (!(isConstructor && HeapVertexUtil.IsThis(vertex)))
                    {                                                
                        AccessPathRegexp ap;
                        if (stack.Any())
                            ap = new AccessPathRegexp(stack.First().Source, null);
                        else
                            ap = new AccessPathRegexp(vertex, null);

                        foreach (var e in stack)                       
                            ap.AppendField(e.Field);

                        ap.AppendField(edge.Field);                        
                        yield return ap;                        
                    }
                }
            }
            //these fields stand for primitive values            
            foreach (Field f in data.GetMayWriteFields(vertex).Except(processedFields))
            {
                if (!(isConstructor && HeapVertexUtil.IsThis(vertex)))
                {                                                                                                               
                    AccessPathRegexp ap;
                    if (stack.Any())
                        ap = new AccessPathRegexp(stack.First().Source, null);
                    else
                        ap = new AccessPathRegexp(vertex, null);
                        
                    foreach (var e in stack)
                        ap.AppendField(e.Field);

                    ap.AppendField(f);                    
                    yield return ap;
                }
            }

            foreach (var edge in data.OutHeapGraph.OutEdges(vertex))
            {
                if (edge is InternalHeapEdge)
                    continue;

                stack.AddLast(edge);
                foreach (var witness in CheckPurityRecursive(data, edge.Target, isConstructor, visited, stack))
                    yield return witness;
                stack.RemoveLast();
            }
        }

        public static bool IsStaticImpure(PurityAnalysisData data)
        {
            var glv = GlobalLoadVertex.GetInstance();
            Queue<HeapVertexBase> bfsQueue = new Queue<HeapVertexBase>();
            HeapVertexSet visited = new HeapVertexSet();
            bfsQueue.Enqueue(glv);
            visited.Add(glv);

            while (bfsQueue.Any())
            {
                var v = bfsQueue.Dequeue();
                if (data.OutHeapGraph.OutEdges(v).OfType<InternalHeapEdge>().Any())
                    return true;
                if (data.GetMayWriteFields(v).Any())
                    return true;
                foreach (var succEdge in data.OutHeapGraph.OutEdges(v).OfType<ExternalHeapEdge>())
                {
                    if (!visited.Contains(succEdge.Target))
                    {
                        bfsQueue.Enqueue(succEdge.Target);
                        visited.Add(succEdge.Target);
                    }
                }
            }
            return false;
        }

        public static PurityReport GetPurityReport(           
           string funcName,
            string signature,
           PurityAnalysisData endPurityData,
            bool isConstructor)
        {            
            var witnesses = IsPure(endPurityData, isConstructor).ToList();
            bool isPure = !witnesses.Any();
            PurityReport report = new PurityReport(funcName, signature, isPure);
            if (!isPure)
            {
                foreach (var witness in witnesses)                                    
                    report.addWitnesses(witness);                
            }
            //add the skipped calls to the purity report
            foreach (var skcall in endPurityData.SkippedCalls)
                report.AddSkippedCalls(skcall);
            return report;
        }
         
    }
}
