using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{
    /// <summary>
    /// This class will handle & (address of) operator
    /// </summary>
    public class AddressOfOperandHandler: IHeapGraphOperandHandler
    {
        IPredicatedOperandHandlerProvider<IHeapGraphOperandHandler> _handlerProvider;

        internal AddressOfOperandHandler()
        {
            _handlerProvider = new PredicatedOperandHandlerProvider<IHeapGraphOperandHandler>();
            _handlerProvider.RegisterHandler(new VariableOperandHandler());
            _handlerProvider.RegisterHandler(new PointerOperandHandler());
            _handlerProvider.RegisterHandler(new SymbolicOperandHandler());
            _handlerProvider.RegisterHandler(new ComplexOperandHandler());
        }

        internal override Predicate<object> GetPredicate()
        {
            return
                obj =>
                {
                    var operand = obj as Phx.IR.Operand;
                    return (operand != null && operand.IsAddress);                        
                };
        }

        protected override bool TryRead(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            out IEnumerable<HeapVertexBase> vertices)
        {            
            //check the type of the operand (it can be value type or a reference type).
            if(operand.Type == null || !operand.Type.IsPointerType)
                throw new NotSupportedException("The type of the operand is null");

            //get the basetype and create a base operand
            var baseType = operand.Type.AsPointerType.ReferentType;
            Phx.IR.Operand baseOperand;
            if (operand.IsVariableOperand)
                baseOperand = Phx.IR.VariableOperand.New(operand.FunctionUnit, baseType, operand.Symbol);
            else if (operand.IsMemoryOperand)
            {
                if (operand.Field != null)
                    baseOperand = Phx.IR.MemoryOperand.New(operand.FunctionUnit, operand.Field,
                        operand.Symbol, operand.BaseOperand, operand.ByteOffset, operand.Alignment, operand.AliasTag, operand.SafetyTag);
                else
                    baseOperand = Phx.IR.MemoryOperand.New(operand.FunctionUnit, baseType,
                    operand.Symbol, operand.BaseOperand, operand.ByteOffset, operand.Alignment, operand.AliasTag, operand.SafetyTag);
            }
            else
                throw new NotSupportedException("Unsupported operand type");

            //is the base type value type ?
            if (baseType.IsNonSelfDescribingAggregate)
            {                
                IHeapGraphOperandHandler handler;
                if (_handlerProvider.TryGetHandler(baseOperand, out handler))
                {
                    vertices = handler.Read(baseOperand, data);
                }
                else                
                    throw new NotSupportedException("Cannot find handler for the base operand: "+ baseOperand);                
            }
            else
            {
                if (baseOperand.IsVariableOperand)
                {
                    var id = NodeEquivalenceRelation.GetVariableId(baseOperand);
                    var funcname = baseOperand.FunctionUnit.FunctionSymbol.QualifiedName;
                    var varnode = NodeEquivalenceRelation.CreateVariableHeapVertex(funcname, id, Context.EmptyContext);

                    if (!data.OutHeapGraph.ContainsVertex(varnode))
                        data.OutHeapGraph.AddVertex(varnode);
                    else
                    {
                        //remove the vertex from the strong update set
                        data.RemoveStrongUpdates(varnode);
                    }

                    vertices = new List<HeapVertexBase> { varnode };
                }
                else
                {
                    //Here the address of a reference field is taken. However, our abstraction cannot represent the pointers inside objects
                    //(other than nested objects). This case arises only when the type of the field is a generic type and a call is invoked on the field.
                    //The following code directly returns the object pointed-to by the field, this is unsound in general but will be sound in this context.
                    IHeapGraphOperandHandler handler;
                    if (_handlerProvider.TryGetHandler(baseOperand, out handler))
                    {
                        vertices = handler.Read(baseOperand, data);
                    }
                    else
                        throw new NotSupportedException("Cannot find handler for the base operand: " + baseOperand);
                }                 
            }
            return true;
        }

        internal override IEnumerable<HeapVertexBase> Read(
            Phx.IR.Operand operand,
            PurityAnalysisData data)
        {
            IEnumerable<HeapVertexBase> vertices;
            TryRead(operand, data, out vertices);
            return vertices;
        }

        internal override void Write(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            IEnumerable<HeapVertexBase> pointstoVertices)
        {
            throw new NotSupportedException("cannot have & on a written operand");
        }

        internal override Field GetField(Phx.IR.Operand operand)
        {
            throw new NotImplementedException("Cannot get field here");
        }
    }
}
