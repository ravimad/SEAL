using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafetyAnalysis.Framework.Graphs;

using SafetyAnalysis.TypeUtil;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{
    public abstract class IHeapGraphOperandHandler : IPredicatedOperandHandler
    {
        #region API methods
        
        protected abstract bool TryRead(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            out IEnumerable<HeapVertexBase> vertices);

        internal abstract IEnumerable<HeapVertexBase> Read(
            Phx.IR.Operand operand,
            PurityAnalysisData data);

        internal abstract void Write(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            IEnumerable<HeapVertexBase> pointstoVertices);        

        internal abstract Field GetField(Phx.IR.Operand operand);

        internal void LoadField(Phx.IR.Operand operand,
            PurityAnalysisData data,
            IEnumerable<HeapVertexBase> baseVertices,
            Field field)
        {
            var inst = operand.Instruction;
            var escapingGraphVertices = AnalysisUtil.GetEscapingVertices(data);

            var escapingBaseVertices = from baseVertex in baseVertices
                                       where escapingGraphVertices.Contains(baseVertex)
                                       select baseVertex;

            if (escapingBaseVertices.Any())
            {
                var loadVertex = NodeEquivalenceRelation.CreateLoadHeapVertex(
                            inst.FunctionUnit.FunctionSymbol.QualifiedName,
                            NodeEquivalenceRelation.GetLoadNodeId(inst),
                            Context.EmptyContext);

                if (!data.OutHeapGraph.ContainsVertex(loadVertex))
                    data.OutHeapGraph.AddVertex(loadVertex);

                //add type to the created external node

                AnalysisUtil.AddApproximateType(data, loadVertex, operand.Field.Type);

                var unloadedVertices = escapingBaseVertices;                
                foreach (var unloadedBaseVertex in unloadedVertices)
                {
                    if (!(unloadedBaseVertex is GlobalLoadVertex))
                    {
                        //filter the unloaded vertices that do not have a compatible field  
                        if (operand.Field != null)
                        {
                            bool compatible = this.DoesContainField(data, unloadedBaseVertex, operand, field);
                            if (!compatible)
                                continue;
                        }
                    }

                    var edge = new ExternalHeapEdge(unloadedBaseVertex, loadVertex, field);
                    if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                        data.OutHeapGraph.AddEdge(edge);
                }
            }            

            //demand driven creation of value types                                            
            if (operand.Type.IsNonSelfDescribingAggregate)
            {
                var internalVertex = NodeEquivalenceRelation.CreateInternalHeapVertex(
                            inst.FunctionUnit.FunctionSymbol.QualifiedName,
                            NodeEquivalenceRelation.GetInternalNodeId(inst),
                            Context.EmptyContext);

                if (!data.OutHeapGraph.ContainsVertex(internalVertex))
                    data.OutHeapGraph.AddVertex(internalVertex);

                //add type to the created internal node                
                AnalysisUtil.AddConcreteType(data, internalVertex, operand.Type);                

                var intBaseVertices = baseVertices.OfType<InternalHeapVertex>();
                var assignedVertices = from baseVertex in intBaseVertices
                                       from successorEdge in data.OutHeapGraph.OutEdges(baseVertex).OfType<InternalHeapEdge>()
                                       where successorEdge.Field.Equals(field) && successorEdge.Target is InternalHeapVertex
                                       select baseVertex;
                var unassignedVertices = intBaseVertices.Except(assignedVertices);

                foreach (var unassignedVertex in unassignedVertices)
                {
                    //filter the unassigned vertices that do not have a compatible field                    
                    bool compatible = this.DoesContainField(data, unassignedVertex, operand, field);
                    if (!compatible)
                        continue;

                    var edge = new InternalHeapEdge(unassignedVertex, internalVertex, field);
                    if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                        data.OutHeapGraph.AddEdge(edge);
                }
            }
        }

        internal bool DoesContainField(PurityAnalysisData data, HeapVertexBase v, 
            Phx.IR.Operand operand, Field f)
        {
            if (f is NullField)
                return true;

            var th = CombinedTypeHierarchy.GetInstance(operand.FunctionUnit.ParentPEModuleUnit);
            var enclTypename = f.EnclosingTypename;
            if (String.IsNullOrEmpty(enclTypename))
                return true;

            var enclTypeinfo = th.LookupTypeInfo(enclTypename);
            if (th.IsHierarhcyKnown(enclTypeinfo))
            {
                bool compatible = AnalysisUtil.CanVertexContainField(data, v, th, enclTypeinfo);
                return compatible;
            }
            return true;
        }

        internal void Copy(PurityAnalysisData data, IEnumerable<HeapVertexBase> dests, IEnumerable<HeapVertexBase> srcs)  
        {            
            //go to the predecessor objects of dest and make it also point to src (actually a strong update could be done here)
            ///Important bug fix: Incoming external edges of "dests" will become incoming internal edges of "srcs" as this 
            ///operation is a write to a field
            var inedges = from v in dests
                          from edge in data.OutHeapGraph.InEdges(v)
                          select edge;
            foreach (var edge in inedges.ToList())
            {
                foreach (var src in srcs)
                {
                    HeapEdgeBase newedge = new InternalHeapEdge(edge.Source, src, edge.Field);
                    if (!data.OutHeapGraph.ContainsHeapEdge(newedge))
                        data.OutHeapGraph.AddEdge(newedge);
                }
            }            
        }
        #endregion
    }
}
