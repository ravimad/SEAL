using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;
using SafetyAnalysis.TypeUtil;

namespace SafetyAnalysis.Purity
{
    using Query = Pair<TypeUtil.TypeInfo, TypeUtil.TypeInfo>;
    public class Simplifier
    {
        private static HashSet<string> benignSideEffectsList = null;        

        //instance data
        PurityAnalysisData data;

        public Simplifier(PurityAnalysisData purityData)
        {
            data = purityData;
        }

        /// <summary>
        /// This will remove captured load nodes and external edges on 
        /// captured vertices.
        /// </summary>
        public void RemoveCapturedLoadNodes()
        {
            HashSet<HeapEdgeBase> toRemoveEdges = new HashSet<HeapEdgeBase>();
            HashSet<LoadHeapVertex> capturedLoadVertices = new HashSet<LoadHeapVertex>();
            HashSet<LoadHeapVertex> isolatedLoadVertices = new HashSet<LoadHeapVertex>();

            HeapVertexSet escapedVertices = AnalysisUtil.GetEscapingVertices(data);

            //remove all external edges from a captured node. Also remove a captured load node
            foreach (HeapVertexBase vertex in data.OutHeapGraph.Vertices)
            {
                //is vertex a captured node ?
                if (!escapedVertices.Contains(vertex))
                {
                    foreach (HeapEdgeBase edge in data.OutHeapGraph.OutEdges(vertex).OfType<ExternalHeapEdge>())
                    {
                        toRemoveEdges.Add(edge);
                    }
                    //is this a load node
                    if (vertex is LoadHeapVertex)
                        capturedLoadVertices.Add(vertex as LoadHeapVertex);
                }
            }
            //remove all unneeded out edges
            foreach (HeapEdgeBase toRemoveEdge in toRemoveEdges)
                data.OutHeapGraph.RemoveEdge(toRemoveEdge);

            //remove all captured load vertices             
            foreach (LoadHeapVertex loadVertex in capturedLoadVertices)
                data.RemoveVertex(loadVertex);

            //remove load vertices that do not have an incoming external edge
            foreach (LoadHeapVertex lv in data.OutHeapGraph.Vertices.OfType<LoadHeapVertex>())
            {
                if (!(lv is GlobalLoadVertex)
                    && !data.OutHeapGraph.InEdges(lv).OfType<ExternalHeapEdge>().Any())
                    isolatedLoadVertices.Add(lv);
            }            
            foreach (LoadHeapVertex loadVertex in isolatedLoadVertices)
                data.RemoveVertex(loadVertex);

            if (PurityAnalysisPhase.EnableStats)
            {
                MethodLevelAnalysis.capturedExternalObjects += isolatedLoadVertices.Count + capturedLoadVertices.Count;
            }
        }

        public void RemoveResolvedReturnedValueNodes()
        {
            var retvars = from call in data.SkippedCalls
                          where call.HasReturnValue()
                          select call.GetReturnValue();

            var retnodes = from var in retvars
                           from edge in data.OutHeapGraph.OutEdges(var)
                           where edge.Target is ReturnedValueVertex
                           select edge.Target;

            var toRemoveNodes = (from v in data.OutHeapGraph.Vertices.OfType<ReturnedValueVertex>()
                                 where !retnodes.Contains(v)
                                 select v).ToList();

            foreach (var node in toRemoveNodes)
                data.RemoveVertex(node);

            if (PurityAnalysisPhase.EnableStats)
            {
                MethodLevelAnalysis.capturedExternalObjects += toRemoveNodes.Count;
            }
        }

        public void RemoveCapturedOrUnresolvableCalls()
        {
            //for stats
            var oldSkCallsCount = data.SkippedCalls.Count();

            var toRemoveCalls = new HashSet<Call>();
            foreach (var call in data.SkippedCalls)
            {
                var calltype = CallUtil.GetCallType(call, data);
                if (calltype.isResolvable == false)
                    toRemoveCalls.Add(call);

                if (calltype.isCallback == false)
                    toRemoveCalls.Add(call);
            }
            foreach (var toRemoveCall in toRemoveCalls)
            {
                data.RemoveSkippedCall(toRemoveCall);
            }

            //also check if the load nodes that the call refer to escapes only due to other skipped calls            
            HashSet<Call> capturedCalls = new HashSet<Call>();
            capturedCalls.UnionWith(data.SkippedCalls);

            var enodes = from v in data.OutHeapGraph.Vertices
                         where ((v is ParameterHeapVertex)
                              || (v is GlobalLoadVertex)
                              || (v is ReturnVertex)
                              || (v is ExceptionVertex)
                              )
                         select v;
            var primaryEscapes = new HashSet<HeapVertexBase>();
            primaryEscapes.UnionWith(data.OutHeapGraph.GetReachableVertices(enodes));

            int oldCount;
            do
            {
                oldCount = capturedCalls.Count;
                foreach (var call in data.SkippedCalls)
                {
                    List<HeapVertexBase> receivers = new List<HeapVertexBase>();
                    if (call is VirtualCall)
                    {
                        receivers.AddRange(AnalysisUtil.GetReceiverVertices(call as VirtualCall, data));
                    }
                    else if (call is DelegateCall)
                    {
                        var targets = AnalysisUtil.GetTargetVertices(call as DelegateCall, data);
                        receivers.AddRange(targets);

                        var methods = AnalysisUtil.GetMethodVertices(data, targets);
                        if (methods.Any((MethodHeapVertex m) => (m.IsVirtual)))
                        {
                            receivers.AddRange(AnalysisUtil.GetReceiverVertices(data, targets));
                        }
                    }

                    var escRecvrs = from v in receivers
                                    where (v is LoadHeapVertex || v is ParameterHeapVertex || v is ReturnedValueVertex)
                                    select v;

                    var trueEscapes = from v in escRecvrs
                                      where primaryEscapes.Contains(v)
                                      select v;
                    if (trueEscapes.Any())
                    {
                        capturedCalls.Remove(call);
                        var reachVertices = data.OutHeapGraph.GetReachableVertices(call.GetReferredVertices());
                        primaryEscapes.UnionWith(reachVertices);
                    }
                }
            } while (oldCount != capturedCalls.Count);

            if (capturedCalls.Any())
            {
                foreach (var toRemoveCall in capturedCalls)
                {
                    data.RemoveSkippedCall(toRemoveCall);
                }
            }

            if (PurityAnalysisPhase.EnableStats)
            {
                MethodLevelAnalysis.capturedSkCalls += oldSkCallsCount - data.SkippedCalls.Count();
            }
        }                

        /// <summary>
        /// This  method will remove the captured nodes from the out heap graph.
        /// This should be called only at the end.
        /// </summary>
        public void RemoveNonEscapingNodes()
        {
            //remove all the captured nodes 
            HeapVertexSet toRemoveSet = new HeapVertexSet();
         
            //collect all escaping nodes or variables referred to by the skipped calls
            HeapVertexSet escapingVertices = new HeapVertexSet();
            escapingVertices.UnionWith(AnalysisUtil.GetSkcallVariables(data));
            escapingVertices.UnionWith(AnalysisUtil.GetEscapingVertices(data));            

            foreach (HeapVertexBase vertex in data.OutHeapGraph.Vertices)
            {
                if (!escapingVertices.Contains(vertex))
                    toRemoveSet.Add(vertex);
            }
            foreach (HeapVertexBase toRemoveVertex in toRemoveSet)
                data.RemoveVertex(toRemoveVertex);

            if (PurityAnalysisPhase.EnableStats)
            {
                MethodLevelAnalysis.capturedLocalObjects += toRemoveSet.OfType<InternalHeapVertex>().Count();
            }
        }

        /// <summary>
        /// This assumes that the graph has only nodes that escape.
        /// safe nodes are read only load nodes for which no externally visible reference is created
        /// </summary>        
        public void RemoveSafeVertices()
        {
            //first add all the nodes that are written or has a externally reachable reference to a worklist
            Queue<HeapVertexBase> worklist = new Queue<HeapVertexBase>();
            //visited set is also the polluted set.
            HeapVertexSet visited = new HeapVertexSet();

            foreach (var vertex in data.OutHeapGraph.Vertices.OfType<LoadHeapVertex>())
            {
                if (data.GetMayWriteFields(vertex).Any()
                    || data.OutHeapGraph.OutEdges(vertex).OfType<InternalHeapEdge>().Any()
                    || data.OutHeapGraph.InEdges(vertex).OfType<InternalHeapEdge>().Any()
                    )
                {
                    worklist.Enqueue(vertex);
                    visited.Add(vertex);
                }
            }
            while (worklist.Any())
            {
                var load_node = worklist.Dequeue();
                //pollute each of the predecessors (along external edge) of the load node
                foreach (var predEdge in data.OutHeapGraph.InEdges(load_node).OfType<ExternalHeapEdge>())
                {
                    var pred = predEdge.Source;
                    if ((pred is LoadHeapVertex) && !visited.Contains(pred))
                    {
                        visited.Add(pred);
                        worklist.Enqueue(pred);
                    }
                }
            }
            var safeVertices = (from v in data.OutHeapGraph.Vertices.OfType<LoadHeapVertex>()
                                where !(v is GlobalLoadVertex) && !visited.Contains(v)
                                select v).ToList();
            foreach (HeapVertexBase safeVertex in safeVertices)
                data.RemoveVertex(safeVertex);
        }

        /// <summary>
        /// Removes known benign side-effects
        /// </summary>
        public void RemoveKnownBenignSideEffects()
        {
            if (Simplifier.benignSideEffectsList == null)
                populateSideEffectsList();

            //remove all "CachedAnonymousMethodDelegate" effects
            System.Func<Field, bool> selector = (Field field) =>
            {
                if (AnalysisUtil.IsLambdaExprDelegate(field))
                    return true;

                if (field is NamedField)
                {
                    var namedfield = field as NamedField;
                    if (Simplifier.benignSideEffectsList.Contains(namedfield.GetQualifiedName()))
                        return true;
                }
                return false;
            };

            var glv = GlobalLoadVertex.GetInstance();
            var toRemoveEdges = (from edge in data.OutHeapGraph.OutEdges(glv)
                                 where selector(edge.Field)
                                 select edge).ToList();

            foreach (var e in toRemoveEdges)
                data.OutHeapGraph.RemoveEdge(e);
        }

        public void populateSideEffectsList()
        {
            Simplifier.benignSideEffectsList = new HashSet<string>();
            string value;
            PurityAnalysisPhase.properties.TryGetValue("benignsideeffectsfilename", out value);
            StreamReader reader = new StreamReader(new FileStream(PurityAnalysisPhase.sealHome + value, 
                FileMode.Open,FileAccess.Read, FileShare.Read));
            while (!reader.EndOfStream)
            {
                string entry = reader.ReadLine();
                Simplifier.benignSideEffectsList.Add(entry.Trim());
            }
        }
        
        public void RemoveTypeIncompatibleEdges(CombinedTypeHierarchy th)
        {            
            //collect all reachability queries                        
            var suptypeQueries = new Dictionary<Query, bool>();      

            //edge can remain if any of the corresponding queries evaluates to true
            var edgeToQueryMap = new ExtendedMap<HeapEdgeBase,Query>();

            //write effect to Query map
            var writeToQueryMap = new ExtendedMap<Pair<HeapVertexBase, Field>, Query>();

            //incompatible array edges
            HashSet<HeapEdgeBase> incompatibleArrayEdges = new HashSet<HeapEdgeBase>();

            foreach (var v in data.OutHeapGraph.Vertices)
            {
                if (v is GlobalLoadVertex)
                    continue;

                var types = data.GetTypes(v);
                if(!types.Any())
                    continue;

                var typeinfos = new List<TypeUtil.TypeInfo>();
                bool hastypeinfos = true;
                bool isArrayType = false;

                foreach (var type in types)
                {
                    if (type == PurityAnalysisData.AnyType)
                    {
                        hastypeinfos = false;
                        break;
                    }

                    if (type == PurityAnalysisData.ArrayType
                        || type.Equals("[mscorlib]System.Array"))
                        isArrayType = true;

                    var info = th.LookupTypeInfo(type);
                    if(!th.IsHierarhcyKnown(info))
                    {
                        hastypeinfos = false;
                        break;
                    }
                    typeinfos.Add(info);
                }
                if (!hastypeinfos)
                    continue;

                //generate queries from edges                
                foreach (var e in data.OutHeapGraph.OutEdges(v))
                {
                    if (e.Field == null)
                        continue;

                    if (e.Field is RangeField)
                    {
                        if (!isArrayType)                        
                            incompatibleArrayEdges.Add(e);
                        continue;
                    }

                    if (String.IsNullOrEmpty(e.Field.EnclosingTypename))
                        continue;

                    var enclTypeinfo = th.LookupTypeInfo(e.Field.EnclosingTypename);
                    if (!th.IsHierarhcyKnown(enclTypeinfo))
                        continue;

                    foreach (var info in typeinfos)
                    {
                        //add new query                                                                                 
                        var query = new Query(enclTypeinfo, info);                

                        if(!suptypeQueries.ContainsKey(query))
                            suptypeQueries.Add(query, false);
                        
                        edgeToQueryMap.Add(e,query);

                        if (!(v is InternalHeapVertex))
                        {
                            //add a super type query as well here
                            query = new Query(info, enclTypeinfo);

                            if (!suptypeQueries.ContainsKey(query))
                                suptypeQueries.Add(query, false);

                            edgeToQueryMap.Add(e, query);
                        }
                    }
                }

                if (!data.MayWriteSet.ContainsKey(v))
                    continue;

                //generate queries from write set
                foreach (var wfield in data.MayWriteSet[v])
                {
                    if (String.IsNullOrEmpty(wfield.EnclosingTypename))
                        continue;

                    var enclTypeinfo = th.LookupTypeInfo(wfield.EnclosingTypename);
                    if (!th.IsHierarhcyKnown(enclTypeinfo))
                        continue;

                    foreach (var info in typeinfos)
                    {
                        //add new query                                                                                 
                        var query = new Query(enclTypeinfo, info);
                        var weffect = new Pair<HeapVertexBase,Field>(v,wfield);

                        if (!suptypeQueries.ContainsKey(query))
                            suptypeQueries.Add(query, false);

                        writeToQueryMap.Add(weffect, query);

                        if (!(v is InternalHeapVertex))
                        {
                            //add a super type query as well here
                            query = new Query(info, enclTypeinfo);

                            if (!suptypeQueries.ContainsKey(query))
                                suptypeQueries.Add(query, false);

                            writeToQueryMap.Add(weffect, query);
                        }
                    }
                }
            }

            //remove incompatible array edges
            foreach (var incompArrayEdge in incompatibleArrayEdges)
                data.OutHeapGraph.RemoveEdge(incompArrayEdge);

            //verify all queries
            th.ResolveSuptypeQueries(suptypeQueries);

            //remove incompatible edges 
            foreach (var edge in edgeToQueryMap.Keys)
            {
                //if (edge.Source is ParameterHeapVertex)
                //{
                //    Console.WriteLine("Checking edge: " + edge);
                //}
                bool isAnyTrue = false;
                foreach (var query in edgeToQueryMap[edge])
                {
                    if (suptypeQueries[query])
                    {
                        //if (edge.Source is ParameterHeapVertex)
                        //    Console.WriteLine("\tSuptype query: {0} > {1} is true ", query.Key.GetTypeName(),
                        //        query.Value.GetTypeName());
                        isAnyTrue = true;
                        break;
                    }
                }
                if (!isAnyTrue)
                    data.OutHeapGraph.RemoveEdge(edge);
            }

            //remove incompatible writes
            foreach (var weffect in writeToQueryMap.Keys)
            {
                //if (weffect.Key is ParameterHeapVertex)
                //{
                //    Console.WriteLine("Checking effect: ({0},{1}) ",weffect.Key, weffect.Value);
                //}
                bool isAnyTrue = false;
                foreach (var query in writeToQueryMap[weffect])
                {
                    if (suptypeQueries[query])
                    {
                        //if (weffect.Key is ParameterHeapVertex)
                        //    Console.WriteLine("\tSuptype query: {0} > {1} is true ", query.Key.GetTypeName(),
                        //        query.Value.GetTypeName());
                        isAnyTrue = true;
                        break;
                    }
                }
                if (!isAnyTrue)
                    data.MayWriteSet[weffect.Key].Remove(weffect.Value);
            }
        }       
    }
}