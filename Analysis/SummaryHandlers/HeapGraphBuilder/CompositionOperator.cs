using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.Purity;
using SafetyAnalysis.Util;
using SafetyAnalysis.TypeUtil;

namespace SafetyAnalysis.Purity.Summaries
{   
    public class CompositionOperator : IDisposable
    {
        #region private data
        private Phx.PEModuleUnit moduleUnit;
        
        protected PurityAnalysisData _callerData;
        protected PurityAnalysisData _calleeData;

        protected Queue<HeapEdgeBase> worklist;
        protected HashSet<HeapEdgeBase> processedEdges;
        protected VertexMap mu;
        protected VertexMap finalmap;
        protected ExtendedMap<HeapVertexBase, ExternalHeapEdge> eeMap;
        //set of nodes whose edge set has been changed during the summary application 
        private HeapVertexSet modifiedNodes = new HeapVertexSet();

        #endregion

        public CompositionOperator(Phx.PEModuleUnit moduleunit)
        {
            moduleUnit = moduleunit;

            //worklist of edges
            worklist = new Queue<HeapEdgeBase>();

            //set of processed external edges
            processedEdges = new HashSet<HeapEdgeBase>();                    
                        
            //Mapping from the callee vertices to the mapped caller vertices
            mu = new VertexMap();
            finalmap = new VertexMap();

            eeMap = new ExtendedMap<HeapVertexBase, ExternalHeapEdge>();                        
        }     
                
        /// <summary>
        /// The initial mapping is a set of mapping from the caller vertex 
        /// to the corresponding callee vertex.
        /// </summary>
        public void ComposeSummary(
            PurityAnalysisData callerData,
            PurityAnalysisData calleeData,
            VertexMap initialMapping)
        {
            _callerData = callerData;
            _calleeData = calleeData;

            Initialize(initialMapping);
            
            ComputeLeastSolution();
            
            MergeVertexMetadata();            
        }        

        protected virtual void ComputeLeastSolution()
        {
            while (worklist.Any())
            {
                //_callerData.Dump();
                //Console.ReadLine();
                var currentEdge = worklist.Dequeue();                

                if (currentEdge is InternalHeapEdge)
                {
                    //fire an internal edge add event
                    RecordInternalEdge(currentEdge as InternalHeapEdge);
                    //process the internal edge
                    HandleInternalEdge(currentEdge as InternalHeapEdge);
                }
                else
                {
                    //fire an external edge add event
                    RecordExternalEdge(currentEdge as ExternalHeapEdge);
                    //handle external edge       
                    HandleExternalEdge(currentEdge as ExternalHeapEdge);
                }
            }
            finalmap.UnionWith(this.mu);
        }

        protected virtual IEnumerable<ExternalHeapEdge> 
            GetRelevantExternalEdges(HeapVertexBase src)
        {
            if (eeMap.ContainsKey(src))
                return eeMap[src];
            return new List<ExternalHeapEdge>();
        }

        private void AddVertexToCallerGraph(HeapVertexBase v)
        {
            if (!_callerData.OutHeapGraph.ContainsVertex(v))
                _callerData.OutHeapGraph.AddVertex(v);
        }

        protected void AddEdgeToCallerGraph(HeapEdgeBase e)
        {         
            if (!_callerData.OutHeapGraph.ContainsHeapEdge(e))
                _callerData.OutHeapGraph.AddEdge(e);

            //do some book keeping here.
            modifiedNodes.Add(e.Source);
        }

        protected virtual void RecordExternalEdge(ExternalHeapEdge ee)
        {
            //add the currentEdge to the external edges map
            eeMap.Add(ee.Source, ee);
        }

        protected virtual void RecordInternalEdge(InternalHeapEdge ie)
        {            
        }

        private void HandleInternalEdge(InternalHeapEdge currentEdge)
        {
            var src = currentEdge.Source;
            var tgt = currentEdge.Target;
            var field = currentEdge.Field;
            
            //find new mappings that might be introduced because of the internal edge 
            var equivVertices = from ee in this.GetRelevantExternalEdges(src)
                                where ee.Field.EqualsModWildCard(field)
                                select ee.Target;
            var list = new List<HeapVertexBase> { tgt };            
            foreach (var ev in equivVertices)
            {
                if (!mu.Contains(ev, tgt))
                {
                    mu.Add(ev, tgt);
                    TranslateEdges(ev, list);
                }
            }
        }

        private void HandleExternalEdge(ExternalHeapEdge currentEdge)
        {
            var src = currentEdge.Source;
            var tgt = currentEdge.Target;
            var field = currentEdge.Field;

            var equivVertices = from ie in _callerData.OutHeapGraph.OutEdges(src).OfType<InternalHeapEdge>()
                                where ie.Field.EqualsModWildCard(field)
                                select ie.Target;

            var newEquivs = new List<HeapVertexBase>(
                        equivVertices.Except(mu.GetMappedSet(tgt)));            
            foreach (var eqv in newEquivs)
            {
                mu.Add(tgt, eqv);
            }

            TranslateEdges(tgt, newEquivs);            
        }        
      
        /// <summary>
        /// Initializes mapping, graphs and worklist
        /// </summary>
        /// <param name="initMap"></param>
        protected virtual void Initialize(VertexMap initMap)
        {            
            //set of caller vertices passed as parameter
            var callerRootVertices = initMap.Range();
           
            //add the init map to final map
            finalmap.UnionWith(initMap);
            
            // add all load vertices, internal vertices,
            // and variable vertices (that are not return vertices 
            // or exception vertices) in the callee summary to the caller graph
            foreach (var vertex in _calleeData.OutHeapGraph.Vertices)
            {
                //check if vertex not an interface vertex
                if (!(vertex is ParameterHeapVertex
                        || vertex is ReturnVertex
                        || vertex is ExceptionVertex))                                        
                {
                    finalmap.Add(vertex, vertex);
                    AddVertexToCallerGraph(vertex);                    

                    //add corresponding types to the caller graph
                    if (_calleeData.types.ContainsKey(vertex))
                    {
                        foreach (var type in _calleeData.types[vertex])
                        {
                            _callerData.types.Add(vertex, type);
                        }
                    }
                }
            }

            foreach (var edge in _calleeData.OutHeapGraph.Edges)
            {
                var src = edge.Source;
                var tgt = edge.Target;

                var sourceVertices = finalmap.GetMappedSet(src);
                var targetVertices = finalmap.GetMappedSet(tgt);
                
                foreach (HeapVertexBase sourceVertex in sourceVertices)
                {
                    foreach (HeapVertexBase targetVertex in targetVertices)
                    {
                        HeapEdgeBase newedge;
                        if (edge is InternalHeapEdge)
                        {
                            newedge = new InternalHeapEdge(sourceVertex, targetVertex, edge.Field);
                            
                            //record the internal edge
                            RecordInternalEdge(newedge as InternalHeapEdge);                            
                        }
                        else
                        {
                            newedge = new ExternalHeapEdge(sourceVertex, targetVertex, edge.Field);
                            //update worklist
                            if (callerRootVertices.Contains(sourceVertex))
                            {
                                worklist.Enqueue(newedge);
                                processedEdges.Add(newedge);
                            }
                            else
                            {
                                //record the external edge
                                RecordExternalEdge(newedge as ExternalHeapEdge);
                            }
                        }   
                        //add the newedge to the caller graph
                        this.AddEdgeToCallerGraph(newedge);
                    }
                }
            }
        }
       
        protected void TranslateEdges(HeapVertexBase vertex, 
            IEnumerable<HeapVertexBase> vertexList)
        {            
            //merge outedges
            foreach (var edge in _callerData.OutHeapGraph.OutEdges(vertex))
            {
                foreach (HeapVertexBase u in vertexList)
                {                                     
                    HeapEdgeBase newedge;
                    if (edge is InternalHeapEdge)
                        newedge = new InternalHeapEdge(u, edge.Target, edge.Field);
                    else
                        newedge = new ExternalHeapEdge(u, edge.Target, edge.Field);                   

                    //is this a new edge ?
                    if (processedEdges.Add(newedge))
                    {
                        worklist.Enqueue(newedge);        
                
                        //add the newedge to the caller graph
                        this.AddEdgeToCallerGraph(newedge);
                    }
                }
            }
            //merge inedges
            foreach (var edge in _callerData.OutHeapGraph.InEdges(vertex))
            {
                if (edge is InternalHeapEdge)
                {
                    foreach (HeapVertexBase u in vertexList)
                    {                       
                        var newedge = new InternalHeapEdge(edge.Source, u, edge.Field);
                        //is this a new edge ?
                        if (processedEdges.Add(newedge))
                        {
                            worklist.Enqueue(newedge);

                            //add the newedge to the caller graph
                            this.AddEdgeToCallerGraph(newedge);
                        }
                    }
                }
            }        
        }        
       
        protected void MergeVertexMetadata()
        {
            MergeThrownVertices();

            MergeSkippedCalls();

            this.UpdateEffectSet(_calleeData.MayWriteSet, _callerData.MayWriteSet);                                    
            //this.UpdateEffectSet(_calleeData.ReadSet, _callerData.ReadSet);
            //this.UpdateEffectSet(_calleeData.MustWriteSet, _callerData.MustWriteSet);            

            //update the linq methods 
            var methNodes = from calleeVertex in _calleeData.linqMethods
                            from callerVertex in  finalmap.GetMappedSet(calleeVertex)
                            select callerVertex;
            _callerData.linqMethods.UnionWith(methNodes);

            //merge unanalyzable calls
            if (PurityAnalysisPhase.trackUnanalyzableCalls)
                _callerData.unanalyzableCalls.UnionWith(_calleeData.unanalyzableCalls);
        }

        protected void UpdateEffectSet<K, V>(ExtendedMap<K, V> calleeEffectSet,
            ExtendedMap<K, V> callerEffectSet) where K : HeapVertexBase
        {
            foreach (var calleeKey in calleeEffectSet.Keys)
            {
                var callerKeys = finalmap.GetMappedSet(calleeKey).OfType<K>();
                foreach (var callerKey in callerKeys)
                {
                    callerEffectSet.Add(callerKey, calleeEffectSet[calleeKey]);
                }
            }
        }

        private void MergeSkippedCalls()
        {            
            foreach (var call in _calleeData.SkippedCalls)
            {
                foreach (var refv in call.GetReferredVertices())
                {
                    foreach (var v in finalmap.GetMappedSet(refv))
                    {
                        //remove the vertex from the strong update set
                        _callerData.RemoveStrongUpdates(v);

                        //add the vertex to the caller graph
                        AddVertexToCallerGraph(v);
                    }
                }                
                _callerData.AddSkippedCall(call);
            }
        }

        private void MergeThrownVertices()
        {
            var exceptionVertex = ExceptionVertex.GetInstance();
            var calleeThrownVertices = (from edge in _calleeData.OutHeapGraph.OutEdges(exceptionVertex)
                                  select edge.Target).ToList();

            var matchingThrownVertices = from calleeThrownVertex in calleeThrownVertices
                                         from matchingCallerVertex in finalmap.GetMappedSet(calleeThrownVertex)
                                         select matchingCallerVertex;

            //add  them to callerThrownVertices as well
            foreach (var matchingVertex in matchingThrownVertices)
            {                
                InternalHeapEdge edge = new InternalHeapEdge(exceptionVertex, matchingVertex, null);
                //if (!_callerData.OutHeapGraph.ContainsHeapEdge(edge))
                //{                    
                //    _callerData.OutHeapGraph.AddEdge(edge);
                //}
                AddEdgeToCallerGraph(edge);
            }                
        }        

        public void UpdateDestination(Call call)
        {
            if (call.HasReturnValue())
            {
                var retvar = call.GetReturnValue();

                //read from the return vertex and write it to the destination operand                        
                var calleeReturnVertices = (from edge in _calleeData.OutHeapGraph.OutEdges(
                                                ReturnVertex.GetInstance())
                                            select edge.Target).ToList();

                var matchingReturnVertices = from calleeReturnVertex in calleeReturnVertices
                                             from matchingCallerVertex in finalmap.GetMappedSet(calleeReturnVertex)
                                             select matchingCallerVertex;

                if (_callerData.OutHeapGraph.ContainsVertex(retvar))
                {
                    //perform strong update if possible
                    if (PurityAnalysisPhase.FlowSensitivity &&
                            _callerData.CanStrongUpdate(retvar))
                        _callerData.OutHeapGraph.RemoveAllOutEdges(retvar);
                }
                else
                {
                    //this will be entered only during the indirect call resolution
                    _callerData.OutHeapGraph.AddVertex(retvar);
                }

                foreach (var matchingVertex in matchingReturnVertices)
                {
                    var edge = new InternalHeapEdge(retvar, matchingVertex, null);                    
                    AddEdgeToCallerGraph(edge);
                }
            }
        }
        
        public IEnumerable<HeapVertexBase> GetMatchedCallerVertices(HeapVertexBase calleeVertex)
        {
            return finalmap.GetMappedSet(calleeVertex);
        }

        public void Dispose()
        {
            worklist = null;
            finalmap = null;
            processedEdges = null;
            eeMap = null;
            mu = null;
        }

        public IEnumerable<HeapVertexBase> GetModifiedNodes()
        {
            return modifiedNodes;
        }       
    }    

    public class DoubleCompositionOperator : CompositionOperator
    {
        private List<InternalHeapEdge> addedInternalEdges;
        private List<ExternalHeapEdge> oldCallerEEs;

        public DoubleCompositionOperator(Phx.PEModuleUnit modunit)
            : base(modunit)
        {
            addedInternalEdges = new List<InternalHeapEdge>();
        }

        public void ComposeSummary(PurityAnalysisData callerData, 
            PurityAnalysisData calleeData, 
            Call call,
            System.Func<Call, PurityAnalysisData, PurityAnalysisData, VertexMap> paramMapping,
            System.Func<Call, PurityAnalysisData, VertexMap> returnMapping)
        {            
            _callerData = callerData;
            _calleeData = calleeData;

            oldCallerEEs = callerData.OutHeapGraph.Edges.OfType<ExternalHeapEdge>().ToList();
            var oldCallerWriteSet = _callerData.MayWriteSet.Copy();            

            //compose caller and callee data            
            base.Initialize(paramMapping(call,_callerData,_calleeData));
            ComputeLeastSolution();
            MergeVertexMetadata();            
            UpdateDestination(call);

            //clear all temporary data structures (except matched vertices)
            eeMap.Clear();
            processedEdges.Clear();
            worklist.Clear();
            finalmap.Clear();
            //important: for efficiency do not clear "mu"

            //perform the self composition            
            this.Initialize(returnMapping(call,_callerData));
            ComputeLeastSolution();            
            UpdateEffectSet(oldCallerWriteSet, _callerData.MayWriteSet);            
        }

        protected override void RecordInternalEdge(InternalHeapEdge ie)
        {
            base.RecordInternalEdge(ie);

            addedInternalEdges.Add(ie);
        }

        protected override void Initialize(VertexMap initMap)
        {
            //add caller external edges to the eeMap
            foreach (var ee in oldCallerEEs)
                eeMap.Add(ee.Source, ee);

            //add all the edges created during the analysis to the worklist
            foreach (var ie in addedInternalEdges)
            {
                worklist.Enqueue(ie);                
                processedEdges.Add(ie);
            }

            //translate edges on the caller placeholder to the callee's return value
            finalmap.UnionWith(initMap);
            foreach (var key in initMap.Keys)
            {
                TranslateEdges(key, initMap[key]);
            }  
        }
    }
}
