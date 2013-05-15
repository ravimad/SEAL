using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SafetyAnalysis.Purity.Properties;
using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.Summaries;
using SafetyAnalysis.Purity.statistics;
using SafetyAnalysis.Util;
using SafetyAnalysis.Purity.callgraph;
using SafetyAnalysis.TypeUtil;

namespace SafetyAnalysis.Purity
{
    public class StatisticsManager
    {       
        private static StatisticsManager _instance = null;
        public static StatisticsManager GetInstance()
        {
            if (_instance == null)
                _instance = new StatisticsManager();
            return _instance;
        }

        private StatisticsManager()
        {                
        }

        public void DumpGraphStats(PurityAnalysisData data, StreamWriter writer)
        {
            int nodes = 0;
            int edges = 0;
            int load_nodes = 0;
            int param_nodes = 0;
            int int_nodes = 0;
            int int_edges = 0;
            int ext_edges = 0;
            int write_set_size = 0;
            int static_node_reachables = 0;
            int exception_reachables = 0;
            int single_field_edges_partitions = 0;
            int single_field_intedges_partitions = 0;
            int single_field_outedges_partitions = 0;
            int skcallsCount = 0;

            if (data.OutHeapGraph.IsVerticesEmpty)
                return;

            var glv = GlobalLoadVertex.GetInstance();
            //compute the nodes reachable from the static fields alone            
            static_node_reachables
                = data.OutHeapGraph.GetReachableVertices(new List<HeapVertexBase> { glv }).Count();

            //compute the nodes reachable from the exceptions
            var thrownVertices = (from edge in data.OutHeapGraph.OutEdges(
                                      ExceptionVertex.GetInstance())
                                  select edge.Target).ToList();
            exception_reachables
                = data.OutHeapGraph.GetReachableVertices(thrownVertices).Count;

            write_set_size = data.MayWriteSet.Count();

            skcallsCount = data.SkippedCalls.Count();

            foreach (var vertex in data.OutHeapGraph.Vertices)
            {
                nodes++;
                if (vertex is InternalHeapVertex)
                    int_nodes++;
                else if (vertex is LoadHeapVertex)
                    load_nodes++;
                else if (vertex is ParameterHeapVertex)
                    param_nodes++;
            }
            foreach (var edge in data.OutHeapGraph.Edges)
            {
                edges++;
                if (edge is InternalHeapEdge)
                    int_edges++;
                else if (edge is ExternalHeapEdge)
                    ext_edges++;
            }
            foreach (var vertex in data.OutHeapGraph.Vertices)
            {
                var edgeGroups = from outEdge in data.OutHeapGraph.OutEdges(vertex)
                                 group outEdge by outEdge.Field;
                var outEdgeGroups = from outEdge in data.OutHeapGraph.OutEdges(vertex).OfType<ExternalHeapEdge>()
                                    group outEdge by outEdge.Field;
                var intEdgeGroups = from outEdge in data.OutHeapGraph.OutEdges(vertex).OfType<InternalHeapEdge>()
                                    group outEdge by outEdge.Field;

                single_field_edges_partitions += edgeGroups.Count();
                single_field_intedges_partitions += intEdgeGroups.Count();
                single_field_outedges_partitions += outEdgeGroups.Count();
            }

            //dump the computed statistics  
            writer.WriteLine("# of vertices: " + nodes);
            writer.WriteLine("# of edges: " + edges);
            writer.WriteLine("# of load nodes: " + load_nodes);
            writer.WriteLine("# of param nodes: " + param_nodes);
            writer.WriteLine("# of internal nodes: " + int_nodes);
            writer.WriteLine("# of internal edges: " + int_edges);
            writer.WriteLine("# of external edges: " + ext_edges);            
            writer.WriteLine("# of static reachables: " + static_node_reachables);
            writer.WriteLine("# of thrown reachables: " + exception_reachables);
            writer.WriteLine("# of sk calls: " + skcallsCount);
            writer.WriteLine("MayWrite set size: " + write_set_size);
            writer.WriteLine("Single field edges partition: " + single_field_edges_partitions);
            writer.WriteLine("Single field internal edges partition: " + single_field_intedges_partitions);
            writer.WriteLine("Single field external edges partition: " + single_field_outedges_partitions);

            //if (single_field_edges_partitions > 0)
            //{
            //    if ((edges / single_field_edges_partitions) > 4)
            //    {
            //        data.Dump();
            //        Console.ReadLine();
            //    }
            //}
        }

        public void DumpOntheFlyStats(StreamWriter writer)
        {            
            writer.WriteLine("Time taken: " + MethodLevelAnalysis.timetaken);
            writer.WriteLine("DBaccess+deserialization time: " + MethodLevelAnalysis.dbaccessTime);
            writer.WriteLine("DBCache lookups: " + MethodLevelAnalysis.dbcacheLookups);
            writer.WriteLine("DBCache hits: " + MethodLevelAnalysis.dbcacheHits);
            writer.WriteLine("# of callee summaries: " + MethodLevelAnalysis.callee_summaries_count);
            writer.WriteLine("# of method calls: " + MethodLevelAnalysis.total_method_calls);
            writer.WriteLine("Max context length: " + MethodLevelAnalysis.maxcontext);
            writer.WriteLine("Captured local objects: " + MethodLevelAnalysis.capturedLocalObjects);
            writer.WriteLine("Captured external objects: " + MethodLevelAnalysis.capturedExternalObjects);
            writer.WriteLine("Captured Skipped calls: " + MethodLevelAnalysis.capturedSkCalls);            
        }

        public void DumpReasonsForExplosion(StreamWriter writer)
        {
            writer.WriteLine("Reasons for Explosion: ");
            foreach (var reason in MethodLevelAnalysis.explosionReasons)
                writer.WriteLine(reason.ToString());
        }

        public void DumpSummaryStats(Phx.Graphs.CallNode node,
            PurityAnalysisData data,
            StreamWriter writer)
        {
            writer.WriteLine("BeginSummaryStats");
            writer.WriteLine("Method: " + PhxUtil.GetQualifiedFunctionName(node.FunctionSymbol));
            //writer.WriteLine("Signature: " + PhxUtil.GetFunctionTypeSignature(node.FunctionSymbol.FunctionType));

            DumpGraphStats(data, writer);
            DumpOntheFlyStats(writer);

            //add the iteration count
            writer.WriteLine("Iteration Count: " + PurityAnalysisPhase.iterationCounts[node]);
            writer.WriteLine("EndSummaryStats");

            //    var witnesses = PurityAnalysisPhase.IsPure(newData, PurityAnalysisPhase.IsConstructor(functionUnit));
            //    if (witnesses.Any())
            //        this.statsFileWriter.WriteLine("IsPure: false");
            //    else
            //        this.statsFileWriter.WriteLine("IsPure: true");            
        }        

        public void DumpUnanalyzableCalls(StreamWriter writer)
        {
            writer.WriteLine("\nCalls without Summaries:");
            writer.WriteLine("-------------------------------------\n");
            foreach (var methodname in MethodLevelAnalysis.unknownTargetCalls)
                writer.WriteLine(methodname);
            writer.Flush();
        }
    }
}
