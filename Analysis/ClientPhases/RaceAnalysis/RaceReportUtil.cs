using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{
    public class RaceReportUtil
    {
        public static void GetReadWriteSet(PurityAnalysisData data,
            HashSet<AccessPathRegexp> readSet,
            HashSet<AccessPathRegexp> writeSet)
        {            
            foreach (var vertex in data.OutHeapGraph.Vertices)
            {                
                if ((vertex is GlobalLoadVertex) || (vertex is ParameterHeapVertex))
                {
                    GetReadWriteSet(data, vertex, new HeapVertexSet(),
                        new LinkedList<HeapEdgeBase>(), readSet, writeSet);
                }
            }            
        }
        
        private static void GetReadWriteSet(
            PurityAnalysisData data,
            HeapVertexBase vertex,            
            HeapVertexSet visited,
            LinkedList<HeapEdgeBase> stack,
            HashSet<AccessPathRegexp> readSet,
            HashSet<AccessPathRegexp> writeSet)
        {
            if (visited.Contains(vertex))
                return;

            visited.Add(vertex);
            
            System.Func<Field, AccessPathRegexp> stkToAP =
                (Field field) =>
                {
                    AccessPathRegexp ap;
                    if (stack.Any())
                        ap = new AccessPathRegexp(stack.First().Source, null);
                    else
                        ap = new AccessPathRegexp(vertex, null);

                    foreach (var e in stack)
                        ap.AppendField(e.Field);

                    ap.AppendField(field);
                    return ap;
                };

            //compute write set
            HashSet<Field> processedFields = new HashSet<Field>();
            foreach (var edge in data.OutHeapGraph.OutEdges(vertex).OfType<InternalHeapEdge>())
            {
                processedFields.Add(edge.Field);
                if (!(vertex is VariableHeapVertex))
                {                    
                     writeSet.Add(stkToAP(edge.Field));
                }
            }
            //these fields stand for primitive values            
            foreach (Field f in data.GetMayWriteFields(vertex).Except(processedFields))
            {
                writeSet.Add(stkToAP(f));
            }

            //compute readset set
            processedFields = new HashSet<Field>();
            foreach (var edge in data.OutHeapGraph.OutEdges(vertex).OfType<ExternalHeapEdge>())
            {
                processedFields.Add(edge.Field);
                readSet.Add(stkToAP(edge.Field));
            }

            //these fields stand for primitive values            
            foreach (Field f in data.GetReadFields(vertex).Except(processedFields))
            {
                readSet.Add(stkToAP(f));
            }

            //only recursing over external edges
            foreach (var edge in data.OutHeapGraph.OutEdges(vertex))
            {
                if (edge is InternalHeapEdge)
                    continue;

                stack.AddLast(edge);
                GetReadWriteSet(data, edge.Target, visited, stack, readSet, writeSet);                
                stack.RemoveLast();
            }
        }

        public static RaceReport FindRaces(           
           string funcName,
            string signature,
           PurityAnalysisData endPurityData)
        {
            var readset = new HashSet<AccessPathRegexp>();
            var writeset = new HashSet<AccessPathRegexp>();
            GetReadWriteSet(endPurityData, readset, writeset);
                                               
            RaceReport report = new RaceReport(funcName, signature, readset, writeset);
            //add the skipped calls to the purity report
            foreach (var skcall in endPurityData.SkippedCalls)
                report.AddSkippedCalls(skcall);
            return report;
        }
         
    }
}
