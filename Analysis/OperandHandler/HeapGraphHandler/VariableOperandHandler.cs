using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{
    internal class VariableOperandHandler : IHeapGraphOperandHandler
    {        
        internal VariableOperandHandler()
        {
        }

        internal override Predicate<object> GetPredicate()
        {
            return
                obj =>
                {
                    var varoperand = obj as Phx.IR.VariableOperand;
                    return (varoperand != null && !varoperand.IsAddress);                        
                };
        }

        protected override bool TryRead(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            out IEnumerable<HeapVertexBase> vertices)
        {
            var id = NodeEquivalenceRelation.GetVariableId(operand);
            var funcname = operand.FunctionUnit.FunctionSymbol.QualifiedName;

            var vertex = NodeEquivalenceRelation.CreateVariableHeapVertex(funcname, id, Context.EmptyContext);
            if (data.OutHeapGraph.ContainsVertex(vertex))
            {
                vertices = HeapVertexSet.Create(
                            from edge in data.OutHeapGraph.OutEdges(vertex)
                            select edge.Target);

                return vertices.Any();
            }
            else
            {
                vertices = new List<HeapVertexBase>();
                return false;
            }
        }

        internal override IEnumerable<HeapVertexBase> Read(
            Phx.IR.Operand operand,
            PurityAnalysisData data)
        {
            var id = NodeEquivalenceRelation.GetVariableId(operand);
            var funcname = operand.FunctionUnit.FunctionSymbol.QualifiedName;

            var varvertex = NodeEquivalenceRelation.CreateVariableHeapVertex(funcname, id, Context.EmptyContext);

            if (!data.OutHeapGraph.ContainsVertex(varvertex))
            {
                if (PurityAnalysisPhase.FlowSensitivity)
                {
                    //mark as strong update
                    data.AddVertexWithStrongUpdate(varvertex);
                }
                else
                    data.OutHeapGraph.AddVertex(varvertex);
            }            

            //load a vertex to the variable
            LoadField(operand, data, new List<HeapVertexBase> { varvertex }, NullField.Instance);

            var vertices = HeapVertexSet.Create(
                            from edge in data.OutHeapGraph.OutEdges(varvertex)
                            select edge.Target);

            //if (!vertices.Any() && operand.Type.IsNonSelfDescribingAggregate)
            //{
            //    //create a value type vertex on demand.
            //    var intvertex = NodeEquivalenceRelation.CreateInternalHeapVertex(funcname, id, Context.EmptyContext);

            //    if(!data.OutHeapGraph.ContainsVertex(intvertex))
            //        data.OutHeapGraph.AddVertex(intvertex);

            //    //add type to the internal vertex
            //    var type = PhxUtil.NormalizedType(operand.Type);
            //    if (type.IsAggregateType)
            //    {
            //        var typename = PhxUtil.GetTypeName(type);
            //        if (!String.IsNullOrEmpty(typename))
            //            data.AddConcreteType(intvertex, typename);
            //    }

            //    //add an edge from the variable vertex
            //    var edge = new InternalHeapEdge(varvertex, intvertex, null);
            //    data.OutHeapGraph.AddEdge(edge);
            //    return new List<HeapVertexBase> { intvertex };
            //}
            return vertices;
        }

        internal override void Write(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            IEnumerable<HeapVertexBase> pointstoVertices)
        {
            var id = NodeEquivalenceRelation.GetVariableId(operand);
            var funcname = operand.FunctionUnit.FunctionSymbol.QualifiedName;            

            var vertex = NodeEquivalenceRelation.CreateVariableHeapVertex(funcname, id, Context.EmptyContext);

            if (!data.OutHeapGraph.ContainsVertex(vertex)) 
            {
                if (PurityAnalysisPhase.FlowSensitivity)
                {
                    //mark as strong update
                    data.AddVertexWithStrongUpdate(vertex);
                }
                else
                    data.OutHeapGraph.AddVertex(vertex);
            }
            else
            {
                //do strong update only in the case of flow sensitive analysis
                if (PurityAnalysisPhase.FlowSensitivity)
                {
                    //check if it is possible to perform strong updates on the vertex
                    if (data.CanStrongUpdate(vertex))
                        data.OutHeapGraph.RemoveAllOutEdges(vertex);
                }
            }

            foreach (var pointstoVertex in pointstoVertices)
            {
                InternalHeapEdge edge = new InternalHeapEdge(vertex, pointstoVertex, null);

                if (!data.OutHeapGraph.ContainsVertex(pointstoVertex))
                    data.OutHeapGraph.AddVertex(pointstoVertex);

                if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                    data.OutHeapGraph.AddEdge(edge);
            }
        }

        internal override Field GetField(Phx.IR.Operand operand)
        {
            throw new NotSupportedException("Cannot call get field here");
        }
    }
}   
