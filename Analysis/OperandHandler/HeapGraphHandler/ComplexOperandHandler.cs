using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.TypeUtil;

namespace SafetyAnalysis.Purity
{
    internal class ComplexOperandHandler : IHeapGraphOperandHandler
    {
        IPredicatedOperandHandlerProvider<IHeapGraphOperandHandler> _handlerProvider;

        internal ComplexOperandHandler()
        {
            _handlerProvider = new PredicatedOperandHandlerProvider<IHeapGraphOperandHandler>();
            _handlerProvider.RegisterHandler(new VariableOperandHandler());
        }

        protected override bool TryRead(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            out IEnumerable<HeapVertexBase> vertices)
        {
            //Contract.Requires(operand.BaseOperand != null);
            //Contract.Requires(operand.IndexOperand != null);

            var field = GetField(operand);

            IHeapGraphOperandHandler baseOperandHandler;
            if (_handlerProvider.TryGetHandler(operand.BaseOperand, out baseOperandHandler))
            {
                IEnumerable<HeapVertexBase> baseVertices 
                    = baseOperandHandler.Read(operand.BaseOperand, data);

                vertices = HeapVertexSet.Create(
                           from baseVertex in baseVertices
                           from successorEdge in data.OutHeapGraph.OutEdges(baseVertex)
                           where field.Equals(successorEdge.Field)
                           select successorEdge.Target);

                return vertices.Any();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal override IEnumerable<HeapVertexBase> Read(
            Phx.IR.Operand operand,
            PurityAnalysisData data)
        {
            //Contract.Requires(operand.BaseOperand != null);
            //Contract.Requires(operand.IndexOperand != null);

            var field = GetField(operand);

            IHeapGraphOperandHandler baseOperandHandler;
            if (_handlerProvider.TryGetHandler(operand.BaseOperand, out baseOperandHandler))
            {
                IEnumerable<HeapVertexBase> baseVertices
                    = baseOperandHandler.Read(operand.BaseOperand, data);

                //are we not tracking primitive types ?
                if (operand.Type.IsPrimitiveType && !PurityAnalysisPhase.TrackPrimitiveTypes)
                {
                    //foreach (var baseVertex in baseVertices)
                    //    data.AddRead(baseVertex, field);
                    return new List<HeapVertexBase>();
                }                
                
                LoadField(operand, data, baseVertices, field);

                IEnumerable<HeapVertexBase> vertices;
                TryRead(operand, data, out vertices);
                //Contract.Assert(vertices != null);
                return vertices;                
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal override void Write(
            Phx.IR.Operand operand, 
            PurityAnalysisData data, 
            IEnumerable<HeapVertexBase> pointstoVertices)
        {
            //Contract.Requires(operand.BaseOperand != null);
            //Contract.Requires(operand.IndexOperand != null);

            var field = GetField(operand);

            IHeapGraphOperandHandler baseOperandHandler;
            if (_handlerProvider.TryGetHandler(operand.BaseOperand, out baseOperandHandler))
            {
                var baseVertices = baseOperandHandler.Read(operand.BaseOperand, data);

                foreach (var baseVertex in baseVertices)
                {
                    if ((operand.Type.IsPrimitiveType && !PurityAnalysisPhase.TrackPrimitiveTypes) 
                        || !pointstoVertices.Any())
                    {
                        data.AddMayWrite(baseVertex, field);
                        if (!pointstoVertices.Any())
                            continue;
                    }

                    //if (!this.DoesContainField(data, baseVertex, operand, RangeField.AllField))
                    //    continue;

                    foreach (var pointstoVertex in pointstoVertices)
                    {
                        var edge = new InternalHeapEdge(baseVertex, pointstoVertex, field);
                        if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                            data.OutHeapGraph.AddEdge(edge);                        
                    }                    
                }
                //if the field is of value type merge all the nodes on the field, this captures the effect of copying
                //if (operand.Type.IsNonSelfDescribingAggregate)
                //{
                //    var targets = new HeapVertexSet(from baseVertex in baseVertices
                //                                    from edge in data.OutHeapGraph.OutEdges(baseVertex)
                //                                    where (edge.Target is InternalHeapVertex) && field.Equals(edge.Field)
                //                                    select edge.Target);
                //    if (targets.Count > 1)
                //    {
                //        //merge nodes
                //        PurityDataUtil.CollapseVertices(data, targets);
                //    }
                //}
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal override Field GetField(Phx.IR.Operand operand)
        {                        
            return RangeField.GetInstance();
        }

        internal override Predicate<object> GetPredicate()
        {
            return
                (obj) =>
                {
                    if (obj is Phx.IR.MemoryOperand)
                    {
                        var memOperand = obj as Phx.IR.MemoryOperand;
                        return (!memOperand.IsAddress) 
                            && (memOperand.AddressMode == Phx.IR.AddressMode.Complex
                            || memOperand.AddressMode == Phx.IR.AddressMode.Based);
                    }
                    return false;
                };
        }
    }
}
