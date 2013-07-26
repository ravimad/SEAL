using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
using SafetyAnalysis.TypeUtil;
using SafetyAnalysis.Framework.Graphs;

namespace SafetyAnalysis.Purity
{
    public class AnalysisUtil
    {
        /// <summary>
        /// A function written for debugging purposes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="varid"></param>
        /// <returns></returns>
        public static bool HasVar(PurityAnalysisData data, int varid)
        {
            var res = from vert in data.OutHeapGraph.Vertices
                      where (vert is VariableHeapVertex) && 
                        ((vert as VariableHeapVertex).Id == varid)
                      select vert;
            if (res.Any())
                return true;
            return false;
        }

        public static IEnumerable<HeapVertexBase> GetSkcallVariables(PurityAnalysisData data)
        {
            var eargs = from call in data.SkippedCalls
                        from refv in call.GetReferredVertices()
                        select refv;
            return eargs;
        }
  
        /// <summary>
        /// Everything reachbale from parameters, globals 
        /// and skipped calls escape.
        /// </summary>
        /// <param name="data"></param>
        public static HeapVertexSet GetInitialEscapeSet(PurityAnalysisData data)
        {                                                    
            var argnodes = from var in GetSkcallVariables(data)                           
                           from edge in data.OutHeapGraph.OutEdges(var)
                           select edge.Target;
                               
            var enodes =   from v in data.OutHeapGraph.Vertices
                           where ((v is ParameterHeapVertex)
                                || (v is GlobalLoadVertex)
                                || (v is ReturnVertex)
                                || (v is ExceptionVertex)                                        
                                )
                           select v;

            HeapVertexSet eset = new HeapVertexSet();
            eset.UnionWith(enodes);
            eset.UnionWith(argnodes);            
            return eset;
        }

        public static HeapVertexSet GetEscapingVertices(PurityAnalysisData data)
        {            
            HeapVertexSet reachSet = new HeapVertexSet();
            var initSet = GetInitialEscapeSet(data);
            reachSet.UnionWith(initSet);
            data.OutHeapGraph.VisitBfs(initSet, null,
                (HeapEdgeBase edge) =>
                {                    
                    reachSet.Add(edge.Target);
                    return true;
                });
            return reachSet;
        }        

        public static IEnumerable<HeapVertexBase> GetReceiverVertices(VirtualCall vcall,
            PurityAnalysisData data)
        {
            var recvr =  vcall.GetReceiver();
            //Contract.Assert(recvr != null);

            if (!data.OutHeapGraph.ContainsVertex(recvr))
            {
                data.OutHeapGraph.AddVertex(recvr);
                return new List<HeapVertexBase>();
            } 
            var recvrVertices = from edge in data.OutHeapGraph.OutEdges(vcall.GetReceiver())
                                select edge.Target;
            return recvrVertices;
        }

        public static IEnumerable<HeapVertexBase> GetTargetVertices(DelegateCall dcall,
           PurityAnalysisData data)
        {
            var targetVertices = from edge in data.OutHeapGraph.OutEdges(dcall.GetTarget())
                                 select edge.Target;
            return targetVertices;
        }

        public static IEnumerable<MethodHeapVertex> 
            GetMethodVertices(PurityAnalysisData data, IEnumerable<HeapVertexBase> delegateVertices)
        {
            var methods = from del in delegateVertices
                          from methodedge in data.OutHeapGraph.OutEdges(del)
                          where methodedge.Field.Equals(DelegateMethodField.GetInstance())
                          && (methodedge.Target is MethodHeapVertex)
                          select methodedge.Target as MethodHeapVertex;
            return methods;
        }

        public static IEnumerable<HeapVertexBase> 
            GetReceiverVertices(PurityAnalysisData data, IEnumerable<HeapVertexBase> delegateVertices)
        {
            var recvrs = from del in delegateVertices
                          from recvredge in data.OutHeapGraph.OutEdges(del)
                          where recvredge.Field.Equals(DelegateRecvrField.GetInstance())                          
                          select recvredge.Target;
            return recvrs;
        }        
       
        //public static string GetQualifiedName(Call call, PurityAnalysisData data)
        //{            
        //    if (call is VirtualCall)
        //    {
        //        VirtualCall vcall = call as VirtualCall;
        //        var name = vcall.ToString() + " Rtypes: ";
        //        var typenames = new HashSet<string>();
        //        foreach (var recvr in AnalysisUtil.GetReceiverVertices(vcall, data).OfType<InternalHeapVertex>())
        //        {                    
        //            foreach (var typename in data.GetTypes(recvr))
        //            {
        //                if (!typenames.Contains(typename))
        //                {
        //                    typenames.Add(typename);
        //                    name += typename + "; ";
        //                }
        //            }
        //        }
        //        return name;
        //    }
        //    else if (call is DelegateCall)
        //    {
        //        return call.ToString();
        //    }
        //    else
        //        return call.ToString();            
        //}

        public static string GetQualifiedName(Call call)
        {
            if (call is CallWithMethodName)
            {
                var mcall = call as CallWithMethodName;
                return mcall.GetDeclaringType() + "::" + mcall.GetMethodName() + "/" + mcall.GetSignature();
            }
            else
                return call.ToString();
        }

        /// <summary>
        /// check whether field is "CachedAnonymousMethodDelegate"
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool IsLambdaExprDelegate(Field field)
        {
            if (field is NamedField)
            {
                var nf = field as NamedField;
                if (nf.GetFieldName().Contains(PurityAnalysisPhase.lambdaDelegateName))
                    return true;
            }
            return false;
        }
       
        public static PurityAnalysisData CollapsePurityData(IEnumerable<PurityAnalysisData> datalist)
        {
            var collapsedData = new PurityAnalysisData(new HeapGraph());
            if (datalist.Any())
            {
                collapsedData.JoinAllData(datalist);
                if (!PurityAnalysisPhase.DisableSummaryReduce)
                    LossyNodeMerger.CreateNodeMerger().MergeNodes(collapsedData);
            }
            return collapsedData;
        }

        public static uint GetCallInstId(Phx.IR.Instruction callInstruction)
        {
            return callInstruction.InstructionId ^ (callInstruction.FunctionUnit.Number << 13);            
        }

        //public static HeapVertexSet GetGloballyReachableVertices(PurityAnalysisData data)
        //{
        //    //find all the globally reachable vertices
        //    var globallyReachableVertices = HeapGraphUtil.GetReachableVertices(
        //        data.OutHeapGraph, new List<HeapVertexBase> { GlobalLoadVertex.GetInstance() });
        //    foreach (var call in data.skippedCalls)
        //    {
        //        var receiverVertices = new HeapVertexSet();
        //        if (call is VirtualCall)
        //        {
        //            var r = (call as VirtualCall).GetReceiver();
        //            var outVertices = from edge in data.OutHeapGraph.OutEdges(r)
        //                              select edge.Target;
        //            receiverVertices.UnionWith(outVertices);                    
        //        }                
        //        if (call is DelegateCall)
        //        {
        //            var t = (call as DelegateCall).GetTarget();
        //            var outVertices = from edge in data.OutHeapGraph.OutEdges(t)
        //                              select edge.Target;
        //            receiverVertices.UnionWith(outVertices);

        //            var tgtVertices = from v in outVertices
        //                              from edge in data.OutHeapGraph.OutEdges(v)
        //                              where edge.Field is DelegateRecvrField
        //                              select edge.Target;
        //            receiverVertices.UnionWith(tgtVertices);
        //        }
        //        if (globallyReachableVertices.Intersect(receiverVertices).Any())
        //        {
        //            globallyReachableVertices.UnionWith(call.GetReferredVertices());
        //        }
        //    }
        //    return globallyReachableVertices;
        //}

        public static bool IsStrConst(HeapVertexBase v)
        {
            return (v is InternalHeapVertex) && ((v as InternalHeapVertex).SiteId == PurityAnalysisTransformers.strconstid);
        }

        public static bool IsString(PurityAnalysisData data, HeapVertexBase v)
        {
            var strtypes = from typename in data.GetTypes(v)
                           where typename.Equals("[mscorlib]System.String")
                           select typename;
            if(strtypes.Any())
                return true;            
            return false;
        }

        public static bool IsClonable(PurityAnalysisData data, HeapVertexBase v)
        {
            return (!IsStrConst(v) && !IsString(data, v));
        }

        public static PurityAnalysisData TranslateSummaryToCallerNamespace(
            PurityAnalysisData calleeData, List<object> extStr)
        {
            var transData = new PurityAnalysisData(new HeapGraph());
            var nsMap = new Dictionary<HeapVertexBase, HeapVertexBase>();
            //var globallyReachableVertices = AnalysisUtil.GetGloballyReachableVertices(calleeData);

            //create a mapping
            foreach (var vertex in calleeData.OutHeapGraph.Vertices)
            {
                if ((vertex is LoadHeapVertex)
                     || (vertex is InternalHeapVertex)
                     || (vertex is MethodHeapVertex)
                     || (vertex is VariableHeapVertex)
                     || (vertex is ReturnedValueVertex))
                {                    
                    //check if the node has context and it can be cloned
                    if (vertex is VertexWithContext 
                        && !(vertex is GlobalLoadVertex)
                        && IsClonable(calleeData, vertex))
                    {                     
                        var clonedVertex = AnalysisUtil.AppendCallerContext(extStr,vertex as VertexWithContext);                        
                        nsMap.Add(vertex, clonedVertex); 
                       
                        //stats
                        if (PurityAnalysisPhase.EnableStats)
                        {
                            if (clonedVertex.context.Count > MethodLevelAnalysis.maxcontext)
                                MethodLevelAnalysis.maxcontext = clonedVertex.context.Count;
                        }
                    }
                    else
                    {
                        var rep = NodeEquivalenceRelation.GetRepresentative(vertex);
                        nsMap.Add(vertex, rep);                        
                    }
                }
                else
                    nsMap.Add(vertex, vertex);

                if (!transData.OutHeapGraph.ContainsVertex(nsMap[vertex]))
                    transData.OutHeapGraph.AddVertex(nsMap[vertex]);
            }

            //translate the edges
            foreach (var edge in calleeData.OutHeapGraph.Edges)
            {
                var src = nsMap[edge.Source];
                var tgt = nsMap[edge.Target];

                HeapEdgeBase newedge;
                if (edge is InternalHeapEdge)
                {
                    newedge = new InternalHeapEdge(src, tgt, edge.Field);
                }
                else
                {
                    newedge = new ExternalHeapEdge(src, tgt, edge.Field);
                }
                if (!transData.OutHeapGraph.ContainsHeapEdge(newedge))
                    transData.OutHeapGraph.AddEdge(newedge);
            }

            //translate the metadata

            //translate skipped calls
            AnalysisUtil.TranslateSkippedCalls(calleeData, transData, nsMap);

            //translate types and  may writeset
            MapSetOfPairs<string>(calleeData.types, transData.types, nsMap);
            MapSetOfPairs<Field>(calleeData.MayWriteSet, transData.MayWriteSet, nsMap);

            //translate linqmethods     
            var transLinqMethods = from method in calleeData.linqMethods                                  
                                   select nsMap[method];
            transData.linqMethods = new HeapVertexSet(transLinqMethods);

            return transData;
        }

        private static void TranslateSkippedCalls(PurityAnalysisData calleeData, 
            PurityAnalysisData transData,
            Dictionary<HeapVertexBase, HeapVertexBase> nsMap)
        {
            foreach (var skcall in calleeData.SkippedCalls)
            {
                var transcall = skcall.ShallowClone();
                var transparam = new List<VariableHeapVertex>();
                foreach (var p in skcall.param)
                {
                    var prep = nsMap[p] as VariableHeapVertex;
                    transparam.Add(prep);
                }
                transcall.param = transparam;
                if (transcall.HasReturnValue())
                    transcall.ret = nsMap[transcall.GetReturnValue()] as VariableHeapVertex;
                if (transcall is DelegateCall)
                {
                    var dcall = transcall as DelegateCall;
                    var deltgt = dcall.target;
                    dcall.target = nsMap[deltgt] as VariableHeapVertex;
                }

                transData.AddSkippedCall(transcall);

                if (calleeData.skippedCallTargets.ContainsKey(skcall))
                {
                    var sktgts = calleeData.skippedCallTargets[skcall];
                    transData.skippedCallTargets.Add(transcall, sktgts);
                }
            }
        }

        private static void MapSetOfPairs<V>(ExtendedMap<HeapVertexBase, V> calleeSet,
            ExtendedMap<HeapVertexBase, V> transSet,
            Dictionary<HeapVertexBase,HeapVertexBase> nsMap)
        {
            foreach (var key in calleeSet.Keys)
            {
                var image = nsMap[key];                
                transSet.Add(image, calleeSet[key]);
            }
        }

        public static VertexWithContext AppendCallerContext(List<object> extStr, VertexWithContext v)
        {
            var ctstr = AnalysisUtil.CreateContextString(v.context.GetContextString(), extStr);            
            var context = Context.New(ctstr);

            if (v is InternalHeapVertex)
            {
                var intv = v as InternalHeapVertex;
                return NodeEquivalenceRelation.CreateInternalHeapVertex(null, intv.SiteId, context);
            }
            else if (v is LoadHeapVertex)
            {
                var loadv = v as LoadHeapVertex;
                return NodeEquivalenceRelation.CreateLoadHeapVertex(null, loadv.SiteId, context);
            }
            else if (v is ReturnedValueVertex)
            {
                var rvalv = v as ReturnedValueVertex;
                return NodeEquivalenceRelation.CreateReturnedValueVertex(rvalv.SiteId, context);
            }
            else if (v is VariableHeapVertex)
            {
                var varv = v as VariableHeapVertex;
                return NodeEquivalenceRelation.CreateVariableHeapVertex(varv.functionName, varv.index, context);
            }
            return null;
        }

        public static List<Object> CreateContextString(List<object> ctstr, List<object> extStr)
        {
            List<object> newCtstr = new List<object>(ctstr);

            if (!PurityAnalysisPhase.BoundContextStr || PurityAnalysisPhase.ContextStrBound > 0)
            {
                foreach (var extElem in extStr)
                {
                    if (!newCtstr.Contains(extElem))
                        newCtstr.Add(extElem);
                }

                if (PurityAnalysisPhase.BoundContextStr)
                {
                    int excess = newCtstr.Count - PurityAnalysisPhase.ContextStrBound;
                    if (excess > 0)
                        newCtstr.RemoveRange(0, excess);
                }
            }
            return newCtstr;
        }

        internal static bool CanVertexContainField(PurityAnalysisData data,             
            HeapVertexBase vertex,
            CombinedTypeHierarchy th,
            TypeUtil.TypeInfo enclTypeinfo)
        {
            var types = data.GetTypes(vertex);
            if (!types.Any())
                return true;

            foreach (var typename in types)
            {
                var nodeTypeinfo = th.LookupTypeInfo(typename);
                if(!th.IsHierarhcyKnown(nodeTypeinfo))
                    return true;

                if (vertex is InternalHeapVertex)
                {
                    //in this case: nodeTypeinfo is a concrete type of the node
                    if (th.IsSuperType(enclTypeinfo, nodeTypeinfo))
                        return true;
                }
                else
                {
                    //in this case: nodeTypeinfo is only an approximate type of the node
                    if (th.AreRelated(enclTypeinfo, nodeTypeinfo))
                        return true;
                }
            }
            return false;
        }

        public static void AddConcreteType(PurityAnalysisData data, InternalHeapVertex v, Phx.Types.Type rawtype)
        {
            var type = PhxUtil.NormalizedType(rawtype);
            if (type.IsUnmanagedArrayType || type.IsManagedArrayType)
            {
                data.AddConcreteType(v, PurityAnalysisData.ArrayType);
            }
            else if (type.IsAggregateType)
            {
                var typename = PhxUtil.GetTypeName(type);
                if (!String.IsNullOrEmpty(typename))
                    data.AddConcreteType(v, typename);
                else
                    data.AddConcreteType(v, PurityAnalysisData.AnyType);
            }
            else if (!type.IsPrimitiveType)
            {
                data.AddConcreteType(v, PurityAnalysisData.AnyType);
            }            
        }

        public static void AddApproximateType(PurityAnalysisData data, HeapVertexBase v, Phx.Types.Type rawtype)
        {
            var type = PhxUtil.NormalizedType(rawtype);
            if (type.IsUnmanagedArrayType || type.IsManagedArrayType)
            {
                data.AddApproximateType(v, PurityAnalysisData.ArrayType);
            }
            else if (type.IsAggregateType)
            {
                var typename = PhxUtil.GetTypeName(type);
                if (!String.IsNullOrEmpty(typename))
                    data.AddApproximateType(v, typename);
                else
                    data.AddApproximateType(v, PurityAnalysisData.AnyType);
            }
            else if (type.IsPrimitiveType)
            {                
                data.AddApproximateType(v, type.AsPrimitiveType.ToString());
            }
            else 
            {
                data.AddApproximateType(v, PurityAnalysisData.AnyType);
            }
        }
    }
}
