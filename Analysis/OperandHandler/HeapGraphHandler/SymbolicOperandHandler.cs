using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{
    // a symbolic operand is a symbol plus an optional offset
    internal class SymbolicOperandHandler :
        IHeapGraphOperandHandler
    {
        internal SymbolicOperandHandler()
        {
        }

        protected override bool TryRead(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            out IEnumerable<HeapVertexBase> vertices)
        {
            //Contract.Requires(operand.BaseOperand == null);
            //Contract.Requires(operand.Symbol != null);
            //Contract.Requires(operand.Symbol.IsStaticFieldSymbol);

            GlobalLoadVertex glv = GlobalLoadVertex.GetInstance();

            var field = GetField(operand);

            if (data.OutHeapGraph.ContainsVertex(glv))
            {
                vertices = HeapVertexSet.Create(
                           from successorEdge in data.OutHeapGraph.OutEdges(glv)
                           where successorEdge.Field.Equals(field)
                           select successorEdge.Target);

                return vertices.Any();
            }
            else
            {
                vertices = new List<HeapVertexBase>();
                return false;
            }
        }

        //update the read set of the global load vertex 
        internal override IEnumerable<HeapVertexBase> Read(
            Phx.IR.Operand operand,
            PurityAnalysisData data)
        {
            GlobalLoadVertex glv = GlobalLoadVertex.GetInstance();

            var field = GetField(operand);
            //if (operand.Type.IsPrimitiveType)
            //{
            //    //data.AddRead(glv, field);
            //    return new List<HeapVertexBase>();
            //}

            LoadField(operand, data, new List<HeapVertexBase> { glv }, field);           

            IEnumerable<HeapVertexBase> vertices;
            TryRead(operand, data, out vertices);
            //Contract.Assert(vertices != null);
            return vertices;
        }

        internal override void Write(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            IEnumerable<HeapVertexBase> pointstoVertices)
        {
            GlobalLoadVertex glv = GlobalLoadVertex.GetInstance();

            var field = GetField(operand);
            if (operand.Type.IsPrimitiveType || !pointstoVertices.Any())
            {
                data.AddMayWrite(glv, field);
                if (!pointstoVertices.Any())
                    return;
            }            

            foreach (var pointstoVertex in pointstoVertices)
            {                                                
                var edge = new InternalHeapEdge(glv, pointstoVertex, field);
                if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                    data.OutHeapGraph.AddEdge(edge);
            }
        }

        internal override Field GetField(Phx.IR.Operand operand)
        {            
            var fieldName = operand.Symbol.NameString;
            string encltypename = null;
            var parentsym = operand.Symbol.LexicalParentSymbol;
            if (parentsym != null && parentsym.Type != null)
            {
                var encltype = PhxUtil.NormalizedType(parentsym.Type);
                if (encltype.IsAggregateType)
                    encltypename = PhxUtil.GetTypeName(encltype);
            }            
            return NamedField.New(fieldName, encltypename);            
        }

        internal override Predicate<object> GetPredicate()
        {
            return
                (obj) =>
                {
                    if (obj is Phx.IR.MemoryOperand)
                    {
                        var memOperand = obj as Phx.IR.MemoryOperand;
                        return (memOperand.AddressMode == Phx.IR.AddressMode.Symbolic)
                            && (!memOperand.IsAddress);
                    }
                    return false;
                };
        }
    }
}