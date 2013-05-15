using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;

namespace SafetyAnalysis.Purity
{
    //not to be used in heapgraph handler providers.
    //should be invoked explicitly
    internal class DelegateOperandHandler : IHeapGraphOperandHandler
    {
        IPredicatedOperandHandlerProvider<IHeapGraphOperandHandler> _handlerProvider;

        internal DelegateOperandHandler()
        {
            _handlerProvider = new PredicatedOperandHandlerProvider<IHeapGraphOperandHandler>();
            _handlerProvider.RegisterHandler(new VariableOperandHandler());
        }

        internal bool TryRead(
            Phx.IR.Operand operand,
            DelegateField field,
            PurityAnalysisData data,  
            out IEnumerable<HeapVertexBase> vertices)
        {
            //Contract.Requires(operand != null && field != null);                                  

            IHeapGraphOperandHandler baseOperandHandler;
            if (_handlerProvider.TryGetHandler(operand, out baseOperandHandler))
            {
                IEnumerable<HeapVertexBase> baseVertices =
                    baseOperandHandler.Read(operand, data);

                vertices = from baseVertex in baseVertices
                           from successorEdge in data.OutHeapGraph.OutEdges(baseVertex)
                           where field.Equals(successorEdge.Field)
                           select successorEdge.Target;

                return vertices.Any();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal IEnumerable<HeapVertexBase> Read(           
            Phx.IR.Operand operand,
            DelegateField field,
            PurityAnalysisData data)
        {
            //Contract.Requires(operand != null && field != null);            

            IHeapGraphOperandHandler baseOperandHandler;
            if (_handlerProvider.TryGetHandler(operand, out baseOperandHandler))
            {
                IEnumerable<HeapVertexBase> baseVertices = 
                    baseOperandHandler.Read(operand, data);                

                LoadField(operand, data, baseVertices, field);                

                // If this is reading a field of an internal object, the field
                // must have been assigned to 
                // If this is reading a field of an external object, the object
                // must be escaping and hence we must have created a load vertex
                // So this TryRead must always succeed
                IEnumerable<HeapVertexBase> vertices;
                TryRead(operand, field, data, out vertices);
                //Contract.Assert(vertices != null);
                return vertices;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal void Write(
            Phx.IR.Operand operand, 
            DelegateField field,
            PurityAnalysisData data, 
            IEnumerable<HeapVertexBase> pointstoVertices)
        {
            //Contract.Requires(operand != null && field != null);
            
            IHeapGraphOperandHandler baseOperandHandler;
            if (_handlerProvider.TryGetHandler(operand, out baseOperandHandler))
            {
                IEnumerable<HeapVertexBase> baseVertices = baseOperandHandler.Read(operand, data);

                foreach (var baseVertex in baseVertices)
                {
                    foreach (var pointstoVertex in pointstoVertices)
                    {
                        var edge = new InternalHeapEdge(baseVertex, pointstoVertex, field);
                        if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                            data.OutHeapGraph.AddEdge(edge);                        
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        
        internal override Field GetField(Phx.IR.Operand operand)
        {
            throw new NotImplementedException();
        }

        internal override Predicate<object> GetPredicate()
        {
            return
                (obj) => 
                {
                    return false;
                };
        }


        protected override bool TryRead(Phx.IR.Operand operand, PurityAnalysisData data, out IEnumerable<HeapVertexBase> vertices)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<HeapVertexBase> Read(Phx.IR.Operand operand, PurityAnalysisData data)
        {
            throw new NotImplementedException();
        }

        internal override void Write(Phx.IR.Operand operand, PurityAnalysisData data, IEnumerable<HeapVertexBase> pointstoVertices)
        {
            throw new NotImplementedException();
        }
    }
}
