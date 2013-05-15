using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.Purity;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity.Summaries
{
    using VertexPair = Pair<HeapVertexBase, HeapVertexBase>;
   
    public class HigherOrderHeapGraphBuilder : HeapGraphBuilder
    {
        public HigherOrderHeapGraphBuilder(Phx.FunctionUnit funit)
        {
            functionUnit = funit;
            moduleUnit = funit.ParentPEModuleUnit;
            summaryReader = new CalleeSummaryReader(functionUnit, moduleUnit);
        }

        public ExtendedMap<Call, string> GetMergedTargets()
        {
            return summaryReader.GetFoundTargets();
        }
        
        public override void ComposeCalleeSummary(
            Call call,
            PurityAnalysisData callerData, 
            PurityAnalysisData calleeData)
        {
            Trace.TraceInformation("Applying callee summary for " + call);            
            if (PurityAnalysisPhase.TraceSummaryApplication)
            {
                Trace.TraceInformation("caller data: ");
                PurityDataUtil.TraceData(callerData);                
                Trace.TraceInformation("callee data: ");
                PurityDataUtil.TraceData(calleeData);
            }        

            //match parameters
            var initialMapping = MatchParameters(call, callerData, calleeData);
            
            //compose
            var builder = new CompositionOperator(moduleUnit);
            builder.ComposeSummary(callerData,calleeData,initialMapping);            
            builder.UpdateDestination(call);            

            //reduce the size of the graphs
            //Caution: Do not use the modified nodes of the builder for reducing as the caller statements does introduce 
            //new load nodes for every read which also have to be reduced.
            if (!PurityAnalysisPhase.DisableSummaryReduce)
            {
                LosslessNodeMerger.CreateNodeMerger().MergeNodes(callerData);
            }
            (new Simplifier(callerData)).RemoveCapturedLoadNodes();

            if (PurityAnalysisPhase.TraceSummaryApplication)
            {
                Trace.TraceInformation("Final data: ");
                PurityDataUtil.TraceData(callerData);
            }            
        }

        public bool ComposeResolvableSkippedCalls(PurityAnalysisData data)
        {
            //List<Call> capturedCalls = new List<Call>();
            var cspairs = new List<Pair<Call, PurityAnalysisData>>();            
                        
            //apply the summaries of resolvable calls until a fix-point is reached.
            //The skipped calls are treated flow-insensitively.                        
            int oldVertexCount;
            int oldEdgeCount;
            int oldSkippedCallCount;
            int i = 0;
            do
            {
                if (PurityAnalysisPhase.EnableConsoleLogging)
                    Console.WriteLine("Begining of Iteration {0}: [{1},{2},{3}]", i,
                        data.OutHeapGraph.VertexCount, data.OutHeapGraph.EdgeCount,
                        data.SkippedCalls.Count());
                
                //initialize old state
                oldVertexCount = data.OutHeapGraph.VertexCount;
                oldEdgeCount = data.OutHeapGraph.EdgeCount;
                oldSkippedCallCount = data.SkippedCalls.Count();

                //read summaries of the resolvalble calls
                cspairs.Clear();
                foreach (var call in data.SkippedCalls)
                {
                    PurityAnalysisData callSummary = null;
                    var calltype = CallUtil.GetCallType(call, data);

                    //ignoring unresolvable calls
                    if ((calltype.isResolvable == true) && (calltype.hasTargets == true))
                    {                        
                        callSummary = summaryReader.GetCalleeData(call, data);                        
                    }
                    if (callSummary != null)
                    {
                        uint contextid = call.contextid;
                        var clonedCalleeData = AnalysisUtil.TranslateSummaryToCallerNamespace(
                                callSummary, new List<object> { contextid });

                        cspairs.Add(new Pair<Call, PurityAnalysisData>(call, clonedCalleeData));
                        //if (call is VirtualCall
                        //    && (call as VirtualCall).methodname.Equals("MoveNext"))
                        //{
                        //    Console.WriteLine("Call: " + call);
                        //    Console.WriteLine("[{0},{1},{2}]", clonedCalleeData.OutHeapGraph.VertexCount,
                        //        clonedCalleeData.OutHeapGraph.EdgeCount,
                        //        clonedCalleeData.skippedCalls.Count);
                        //}
                    }
                }
                if (!cspairs.Any())
                {                    
                    break;
                }

                if (PurityAnalysisPhase.EnableConsoleLogging)
                {
                    Console.WriteLine("\t -[{0}]", cspairs.Count);
                }
                
                foreach (var pair in cspairs)
                {
                    if (PurityAnalysisPhase.TraceSkippedCallResolution)
                    {
                        Trace.TraceInformation("Resolving skipped call: " + pair.Key);
                        PurityDataUtil.TraceData(pair.Value);
                    }
                    var builder = new DoubleCompositionOperator(moduleUnit);                    
                    builder.ComposeSummary(data, pair.Value, pair.Key, MatchParameters, MatchReturnValues);                    

                    //Creating a custom node merger
                    var modnodes = builder.GetModifiedNodes();
                    var reducer = new NodeMerger(
                           (PurityAnalysisData pdata, List<HeapVertexSet> mergeableVertices) =>
                           {
                               foreach (var v in modnodes)
                               {
                                   LossyNodeMerger.PopulateMergableReadNodes(pdata, v, mergeableVertices);
                                   LossyNodeMerger.PopulateMergableWriteNodes(pdata, v, mergeableVertices);
                               }
                               return true;
                           },
                           (PurityAnalysisData pdata, HeapVertexBase v, List<HeapVertexSet> mergeableVertices) =>
                           {
                               LossyNodeMerger.PopulateMergableReadNodes(pdata, v, mergeableVertices);
                               LossyNodeMerger.PopulateMergableWriteNodes(pdata, v, mergeableVertices);
                               return true;
                           });

                    if (!PurityAnalysisPhase.DisableSummaryReduce)
                        reducer.MergeNodes(data);

                    if (PurityAnalysisPhase.TraceSkippedCallResolution)
                    {
                        Trace.TraceInformation("After Resolving: " + pair.Key);
                        PurityDataUtil.TraceData(data);
                        PurityDataUtil.DumpAsDGML(data, null, pair.Key);                        
                    }
                }

                if (PurityAnalysisPhase.EnableConsoleLogging)
                    Console.WriteLine("End of Iteration {0} : [{1},{2},{3}]", i++,
                        data.OutHeapGraph.VertexCount, data.OutHeapGraph.EdgeCount,
                        data.SkippedCalls.Count());

            } while ((oldVertexCount != data.OutHeapGraph.VertexCount)
                || (oldEdgeCount != data.OutHeapGraph.EdgeCount)
                || (oldSkippedCallCount != data.SkippedCalls.Count()));           

            return true;
        }

        #region  functions for creating initial mappings

        private VertexMap MatchParameters(
            Call call, 
            PurityAnalysisData callerData,
            PurityAnalysisData calleeData)
        {
            var mappedSet = new VertexMap();

            //map global load vertex to itself (this mapping can be removed without affecting correctness)
            var glv = GlobalLoadVertex.GetInstance();
            mappedSet.Add(glv, glv);

            if (call is DelegateCall)
            {
                var dcall = call as DelegateCall;

                var delEdges = from edge in callerData.OutHeapGraph.OutEdges(dcall.GetTarget())
                               from deledge in callerData.OutHeapGraph.OutEdges(edge.Target)
                               select deledge;

                var targetMethods = from edge in delEdges
                                    where edge.Field.Equals(DelegateMethodField.GetInstance())
                                        && (edge.Target is MethodHeapVertex)
                                    select edge.Target as MethodHeapVertex;

                var isStatic = targetMethods.Any((MethodHeapVertex m) => (!m.IsInstance));                
                var isInstance = targetMethods.Any((MethodHeapVertex m) => (m.IsInstance));
                
                if (isInstance)
                {
                    //match this with recvr vertex
                    var thisvertex = HeapVertexUtil.GetThisVertex(calleeData.OutHeapGraph);
                    if (thisvertex != null)
                    {
                        var recvrVertices = from edge in delEdges
                                            where edge.Field.Equals(DelegateRecvrField.GetInstance())
                                            select edge.Target;
                        foreach (var rv in recvrVertices)
                            mappedSet.Add(thisvertex, rv);

                        MatchParametersInSequence(call, callerData, calleeData, 1, mappedSet);
                    }
                }

                if (isStatic)
                {
                    MatchParametersInSequence(call, callerData, calleeData, 0, mappedSet);
                }
            }
            else
                MatchParametersInSequence(call, callerData, calleeData, 0, mappedSet);       
            return mappedSet;
        }

        private void MatchParametersInSequence(Call call, PurityAnalysisData callerData,
            PurityAnalysisData calleeData, int parameterCount, VertexMap mappedSet)
        {
            foreach (var param in call.GetAllParams())
            {
                parameterCount++;
                
                if (!callerData.OutHeapGraph.ContainsVertex(param))
                {
                    Trace.TraceWarning("Argument {0} not present in caller", param);
                    continue;
                }

                var parameterVertices = from edge in callerData.OutHeapGraph.OutEdges(param)
                                        select edge.Target;
                if (parameterVertices.Any())
                {
                    var calleeVertices =
                        from paramVertex in calleeData.OutHeapGraph.Vertices.OfType<ParameterHeapVertex>()
                        where paramVertex.Index == parameterCount
                        select paramVertex;

                    if (!calleeVertices.Any())
                    {
                        Trace.TraceWarning("No callee vertices for parameter #: " + parameterCount);                                             
                    }

                    foreach (var callerVertex in parameterVertices)
                    {
                        foreach (var calleevertex in calleeVertices)
                        {
                            mappedSet.Add(calleevertex, callerVertex);
                        }
                    }
                }
                else
                {
                    Trace.TraceWarning("No targets for argument {0} in caller", param);                    
                }
            }
        }

        private VertexMap MatchArguments(Call call, 
            PurityAnalysisData headData)
        {
            var map = MatchReturnValues(call, headData);

            //map globa load vertex to itself
            var glv = GlobalLoadVertex.GetInstance();
            map.Add(glv, glv);                            
            
            //match targets of argument variables in headData with itself
            foreach (var arg in call.GetAllParams())
            {                                
                var headArgVertices = from edge in headData.OutHeapGraph.OutEdges(arg)
                                        select edge.Target;
                if (headArgVertices.Any())
                {
                    foreach (var headArgVertex in headArgVertices)
                    {
                        map.Add(headArgVertex, headArgVertex);                       
                    }
                }
                else 
                {
                    Trace.TraceWarning("No targets for argument {0} in Head data",arg);                    
                }
            }            

            //if this is a delegate call match the receiver vertices(if any) of the delegates to themselves
            if (call is DelegateCall)
            {
                var dcall = call as DelegateCall;
                var recvrVertices = from edge in headData.OutHeapGraph.OutEdges(dcall.GetTarget())
                                    from deledge in headData.OutHeapGraph.OutEdges(edge.Target)
                                    where deledge.Field.Equals(DelegateRecvrField.GetInstance())
                                    select deledge.Target;
                foreach (var rv in recvrVertices)
                    map.Add(rv, rv);
            }
            return map;
        }

        private VertexMap MatchReturnValues(Call call,
            PurityAnalysisData headData)
        {
            var map = new VertexMap();
            
            //match return nodes of the skipped method in the headData with 
            //placeholder nodes pointed to by the return value in tailData.
            if (call.HasReturnValue())
            {
                var retvar = call.GetReturnValue();
                var retVertices = from edge in headData.OutHeapGraph.OutEdges(retvar)
                                  select edge.Target;

                var headRetVertices = retVertices.Where((HeapVertexBase v) => !(v is ReturnedValueVertex));
                if (headRetVertices.Any())
                {
                    var tailRetVertices = retVertices.OfType<ReturnedValueVertex>();
                    if (tailRetVertices.Any())
                    {
                        foreach (var tailRetvertex in tailRetVertices)
                        {
                            foreach (var headRetVertex in headRetVertices)
                            {
                                map.Add(tailRetvertex, headRetVertex);
                            }
                        }
                    }
                    else
                        Trace.TraceWarning("Returned value variable {0} in the tail arg does not point to any retuned value vertex", retvar);
                }
                else
                    Trace.TraceWarning("Return variable {0} in the head arg does not point to any non returned value vertex", retvar);
            }
            return map;
        }              
        #endregion 

        #region private data
        
        protected Phx.PEModuleUnit moduleUnit;
        protected Phx.FunctionUnit functionUnit; 
        protected CalleeSummaryReader summaryReader;

        #endregion
    }

    internal class HigherOrderNewObjBuilder : HigherOrderHeapGraphBuilder
    {
        private InternalHeapVertex _newObjectVertex;

        public HigherOrderNewObjBuilder(Phx.FunctionUnit funit)
            : base(funit)
        {            
        }

        public override void ComposeCalleeSummary(
           Call call,
           PurityAnalysisData callerData,
           PurityAnalysisData calleeData)
        {
            //Contract.Assert(callerData != null);
            //Contract.Assert(calleeData != null);

            //create a new object from the call instruction            
            createNewObject(call, callerData);            

            //match constructor parameters
            VertexMap initialMapping;
            MatchCtorParameters(call, callerData, calleeData, out initialMapping);
            
            var builder = new CompositionOperator(moduleUnit);
            builder.ComposeSummary(callerData, calleeData, initialMapping);            
            UpdateCtorDestination(call, callerData);

            //simplify the caller out heap graph
            (new Simplifier(callerData)).RemoveCapturedLoadNodes();                   
        }

        private void createNewObject(
            Call call,
            PurityAnalysisData callerData)
        {
            var scall = call as StaticCall;

            _newObjectVertex =
                NodeEquivalenceRelation.CreateInternalHeapVertex(
                    scall.methodname, scall.contextid, Context.EmptyContext);

            if (!callerData.OutHeapGraph.ContainsVertex(_newObjectVertex))
            {
                callerData.OutHeapGraph.AddVertex(_newObjectVertex);
                //update the concrete type of the object                                   
                callerData.AddConcreteType(_newObjectVertex, scall.declaringType);
            }            
            //TODO: To be investigated further.
            //else
            //{
            //    var vertex = _callerData.OutHeapGraph.Vertices.Single(obj => { return obj.Equals(_newObjectVertex); });
            //    vertex.RepresentsSingleObject = false;
            //}            
        }
        
        private void MatchCtorParameters(
            Call call,
            PurityAnalysisData callerData,
            PurityAnalysisData calleeData,
            out VertexMap map)
        {
            map = new VertexMap();
            
            //match the newobject with the this vertex
            var thisVertices = from calleeVertex in calleeData.OutHeapGraph.Vertices.OfType<ParameterHeapVertex>()
                               where HeapVertexUtil.IsThis(calleeVertex)
                               select calleeVertex;            
            foreach (var thisVertex in thisVertices)
            {
                map.Add(thisVertex,_newObjectVertex);
            }            
            
            //call parameters will not include this and hence is 1 less than the parameters in the summary
            int parameterCount = 1;
            foreach (var param in call.GetAllParams())
            {
                parameterCount++;                
                
                if (!callerData.OutHeapGraph.ContainsVertex(param))
                {                    
                    Trace.TraceWarning("Argument {0} not present in caller", param);
                    continue;
                }

                var parameterVertices = from edge in callerData.OutHeapGraph.OutEdges(param)
                                        select edge.Target;
                if (parameterVertices.Any())
                {
                    var calleeVertices =
                        from paramVertex in calleeData.OutHeapGraph.Vertices.OfType<ParameterHeapVertex>()
                        where paramVertex.Index == parameterCount
                        select paramVertex;

                    if (!calleeVertices.Any())
                    {
                        Trace.TraceWarning("No callee vertices for parameter #: " + parameterCount);                                                                 
                    }

                    foreach (var callerVertex in parameterVertices)
                    {
                        foreach (var calleevertex in calleeVertices)
                        {
                            map.Add(calleevertex, callerVertex);
                        }
                    }
                }
                else
                {
                    //a variable may not be bound to a abstract object under several circumstance
                    // 1. It may correspond to the exception thrown by an unknown method.
                    Trace.TraceWarning("Argument {0} does not refer to any vertices in the caller", param);
                }
            }            
        }


        private void UpdateCtorDestination(
            Call call,
            PurityAnalysisData callerData)
        {
            if (!call.HasReturnValue())
            {
                Console.WriteLine("Constructor with no return value: possibly unsafe code");
                return;
            }
            var retvar = call.GetReturnValue();
            var edge = new InternalHeapEdge(retvar, _newObjectVertex, null);

            if (!callerData.OutHeapGraph.ContainsVertex(retvar))
                callerData.OutHeapGraph.AddVertex(retvar);

            //do a weak update here
            if (!callerData.OutHeapGraph.ContainsHeapEdge(edge))
                callerData.OutHeapGraph.AddEdge(edge);
        }
    }
}
