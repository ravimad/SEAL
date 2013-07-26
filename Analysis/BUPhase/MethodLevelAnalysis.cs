#region Using namespace declarations

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.Summaries;
using SafetyAnalysis.Purity.statistics;
using SafetyAnalysis.TypeUtil;
using SafetyAnalysis.Util;

#endregion

namespace SafetyAnalysis.Purity
{
    public class MethodLevelAnalysis
    {        

        #region static state

                    
        public static HashSet<string> unknownTargetCalls = new HashSet<string>();
        public static HashSet<MethodHeapVertex> MethodsPassedToLinqQueries = new HashSet<MethodHeapVertex>();        
        
        #endregion static state

        #region stats 

        public static List<ExplosionReasons> explosionReasons;
        public static int callee_summaries_count = 0;
        public static int total_method_calls = 0;
        public static int known_target_methods = 0;
        public static long dbaccessTime = 0;        
        public static int dbcacheHits = 0;
        public static int dbcacheLookups = 0;        
        public static long timetaken = 0;
        public static long maxcontext = 0;
        public static int capturedExternalObjects = 0;
        public static int capturedLocalObjects = 0;
        public static int capturedSkCalls = 0;
        
        #endregion stats

        #region private state
        
        private Phx.FunctionUnit functionUnit;

        #endregion private state              

        #region DEBUG STATE

        public static bool debug_flag = false;

        #endregion DEBUG STATE

        public MethodLevelAnalysis(Phx.FunctionUnit funit)
        {
            functionUnit = funit;
        }

        private void ResetStats()
        {
            //on-the-fly stats
            callee_summaries_count = 0;
            total_method_calls = 0;
            known_target_methods = 0;
            dbaccessTime = 0;
            dbcacheHits = 0;
            dbcacheLookups = 0;
            timetaken = 0;
            maxcontext = 0;
            capturedLocalObjects = 0;
            capturedExternalObjects = 0;
            capturedSkCalls = 0;
        }

        public PurityAnalysisData Execute()
        {
            Trace.TraceInformation("Method {0}", functionUnit.FunctionSymbol.QualifiedName);
            Trace.Indent();

            //if (functionUnit.FunctionSymbol.QualifiedName.Equals("SafetyAnalysis.Purity.PurityDataUtil::CollapseVertices"))
            //{
            //    debug_flag = true;
            //}
            bool flowGraphExists = (functionUnit.FlowGraph != null);
            if (!flowGraphExists)
            {
                functionUnit.BuildFlowGraph();                                
            }

            PurityAnalysisData endPurityData;            

            //reset per summary stats
            ResetStats();

            //record the time taken to compute the summary 
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (PurityAnalysisPhase.FlowSensitivity)
            {
                endPurityData = new FlowSensitiveAnalysis().Compute(functionUnit);                
            }
            else
                endPurityData = (new FlowInsensitivePurityAnalysis()).Compute(functionUnit);            

            if (!endPurityData.OutHeapGraph.IsVerticesEmpty)
            {
                //cleanup summary
                CleanupSummary(endPurityData);

                // reduce
                if (!PurityAnalysisPhase.DisableSummaryReduce)
                {
                    new NodeSkippedCallMerger(
                        LossyNodeMerger.CreateNodeMerger(),
                        CompleteEqualityHash.GetInstance).MergeWithSkippedCalls(endPurityData);
                }

                if (PurityAnalysisPhase.TraceSkippedCallResolution)
                {
                    Trace.TraceInformation("Before skipped call resolution: ");
                    PurityDataUtil.TraceData(endPurityData);
                    PurityDataUtil.DumpAsDGML(endPurityData, null, null);
                }

                //resolve all the skipped calls
                var builder = new HigherOrderHeapGraphBuilder(functionUnit);
                builder.ComposeResolvableSkippedCalls(endPurityData);
                endPurityData.skippedCallTargets.UnionWith(builder.GetMergedTargets());

                //Simplify the summary
                Simplify(endPurityData);

                //cleanup summary again (after resolution)
                CleanupSummary(endPurityData);

                //finally reduce
                if (!PurityAnalysisPhase.DisableSummaryReduce)
                {
                    new NodeSkippedCallMerger(
                        LossyNodeMerger.CreateNodeMerger(),
                        CompleteEqualityHash.GetInstance).MergeWithSkippedCalls(endPurityData);
                    new Simplifier(endPurityData).RemoveNonEscapingNodes();
                }

                //collect all methods passed to linq 
                CollectLinqMethods(endPurityData);
            }

            PurityDataUtil.CheckInvariant(endPurityData);
            sw.Stop();
            MethodLevelAnalysis.timetaken = sw.ElapsedMilliseconds;
            
            if (!flowGraphExists)
            {
                functionUnit.DeleteFlowGraph();
            }

            if (PurityAnalysisPhase.DumpSummariesAsGraphs)
            {
                var filename = GeneralUtil.ConvertToFilename(functionUnit);
                PurityDataUtil.DumpAsDGML(endPurityData, filename + ".dgml", null);
                //File.WriteAllText(filename + ".txt", endPurityData.ToString());                
                //var insts = new List<string>();
                //foreach (var inst in functionUnit.Instructions)
                //{
                //    insts.Add(inst.ToString());
                //}
                //File.WriteAllLines(filename + ".txt", insts.ToArray<string>());
            }

            Trace.Unindent();
            return endPurityData;
        }

        public void Simplify(PurityAnalysisData data)
        {
            //iteratively remove captured/unresolvable skipped calls
            //and captured load nodes, resolved return vertices
            var simplifier = new Simplifier(data);

            //to identify if the data has changed we use the vertex and edge count of the heapgraph
            int oldVertexCount;
            int oldEdgeCount;
            do
            {
                oldVertexCount = data.OutHeapGraph.VertexCount;
                oldEdgeCount = data.OutHeapGraph.EdgeCount;

                simplifier.RemoveCapturedOrUnresolvableCalls();
                simplifier.RemoveResolvedReturnedValueNodes();
                simplifier.RemoveCapturedLoadNodes();

            } while ((oldVertexCount != data.OutHeapGraph.VertexCount)
                || (oldEdgeCount != data.OutHeapGraph.EdgeCount));
        }

        private void CleanupSummary(PurityAnalysisData data)
        {
            var simplifier = new Simplifier(data);
            //removes known benign side-effects
            simplifier.RemoveKnownBenignSideEffects();

            //remove captured vertices.
            simplifier.RemoveNonEscapingNodes();

            //remove useless load vertices
            simplifier.RemoveSafeVertices();

            //remove type incompatible edges
            var th = CombinedTypeHierarchy.GetInstance(functionUnit.ParentPEModuleUnit);
            simplifier.RemoveTypeIncompatibleEdges(th);
        }

        /// <summary>
        /// This directly manipulates the purity analysis data
        /// </summary>
        /// <param name="data"></param>
        private void CollectLinqMethods(PurityAnalysisData data)
        {
            var knownLinqMethods = (data.linqMethods.OfType<MethodHeapVertex>()).ToList();
            MethodLevelAnalysis.MethodsPassedToLinqQueries.UnionWith(knownLinqMethods);
            data.linqMethods.RemoveWhere((HeapVertexBase v) => (v is MethodHeapVertex));
        }        
    }      
}
