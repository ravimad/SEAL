using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.Util;
using SafetyAnalysis.TypeUtil;

namespace SafetyAnalysis.Purity
{
    internal class PointerOperandHandler : IHeapGraphOperandHandler
    {
        IPredicatedOperandHandlerProvider<IHeapGraphOperandHandler> _handlerProvider;

        internal PointerOperandHandler()
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
            //Contract.Requires(operand.Field != null);

            //field could be null
            var field = GetField(operand);            

            IHeapGraphOperandHandler baseOperandHandler;
            if (_handlerProvider.TryGetHandler(operand.BaseOperand, out baseOperandHandler))
            {
                IEnumerable<HeapVertexBase> baseVertices =
                    baseOperandHandler.Read(operand.BaseOperand, data);

                vertices = HeapVertexSet.Create(
                           from baseVertex in baseVertices
                           from successorEdge in data.OutHeapGraph.OutEdges(baseVertex)
                           where field.EqualsModWildCard(successorEdge.Field)
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
            //Contract.Requires(operand.Field != null);

            //field could be null
            var field = GetField(operand);            

            IHeapGraphOperandHandler baseOperandHandler;
            if (_handlerProvider.TryGetHandler(operand.BaseOperand, out baseOperandHandler))
            {
                IEnumerable<HeapVertexBase> baseVertices = 
                    baseOperandHandler.Read(operand.BaseOperand, data);
                
                //if (operand.Type.IsPrimitiveType)
                //{
                //    //foreach (var baseVertex in baseVertices)
                //    //    data.AddRead(baseVertex, field);
                //    return new List<HeapVertexBase>();
                //}

                if (operand.Type.IsNonSelfDescribingAggregate && field is NullField)
                {
                    //just return the base vertices as it represents a value type
                    return baseVertices;
                }

                LoadField(operand, data, baseVertices, field);                

                // If this is reading a field of an internal object, the field
                // must have been assigned to 
                // If this is reading a field of an external object, the object
                // must be escaping and hence we must have created a load vertex
                // So this TryRead must always succeed
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
            var field = GetField(operand);
                        
            IHeapGraphOperandHandler baseOperandHandler;
            if (_handlerProvider.TryGetHandler(operand.BaseOperand, out baseOperandHandler))
            {
                IEnumerable<HeapVertexBase> baseVertices = 
                    baseOperandHandler.Read(operand.BaseOperand, data).ToList();

                if (field is NullField && operand.Type.IsNonSelfDescribingAggregate)
                {
                    //do a copy here 
                    this.Copy(data, baseVertices, pointstoVertices);                
                    return;
                }                

                foreach (var baseVertex in baseVertices)
                {                                        
                    //else branch (fields of value type are modeled as separate objects)
                    if (operand.Type.IsPrimitiveType || !pointstoVertices.Any())
                    {
                        data.AddMayWrite(baseVertex, field);
                        if (!pointstoVertices.Any())
                            continue;
                    }

                    //check for type compatability
                    if (!this.DoesContainField(data, baseVertex, operand, field))
                        continue;

                    foreach (var pointstoVertex in pointstoVertices)
                    {
                        var edge = new InternalHeapEdge(baseVertex, pointstoVertex, field);
                        if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                            data.OutHeapGraph.AddEdge(edge);                        
                    }                                                     
                }

                //if the field is of value type merge all the nodes on the field, this captures the effect of copying (not strictly necessary)
                //if (operand.Type.IsNonSelfDescribingAggregate)
                //{
                //    var targets = new HeapVertexSet(from baseVertex in baseVertices
                //                                    from edge in data.OutHeapGraph.OutEdges(baseVertex)
                //                                    where (edge.Target is InternalHeapVertex) && (field != null && field.Equals(edge.Field))
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
            if (operand.Field.FieldSymbol != null)
            {
                var fieldName = PhxUtil.GetFieldName(operand.Field);
                var encltype = PhxUtil.NormalizedType(operand.Field.EnclosingType);
                string encltypename = null;
                if (encltype.IsAggregateType)                
                    encltypename = PhxUtil.GetTypeName(encltype);                
                return NamedField.New(fieldName, encltypename);                
            }
            return NullField.Instance;
        }

        internal override Predicate<object> GetPredicate()
        {
            return
                (obj) =>
                {
                    if (obj is Phx.IR.MemoryOperand)
                    {
                        var memOperand = obj as Phx.IR.MemoryOperand;
                        return (memOperand.AddressMode == Phx.IR.AddressMode.Pointer)
                            && (!memOperand.IsAddress);
                    }
                    return false;
                };
        }
    }
}
