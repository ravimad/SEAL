using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafetyAnalysis.Framework.Graphs;

namespace SafetyAnalysis.Purity
{
    class ImmediateOperandHandler : IHeapGraphOperandHandler
    {
        internal override Predicate<object> GetPredicate()
        {
            return
                obj =>
                {
                    return (obj is Phx.IR.ImmediateOperand);
                };            
        }

        protected override bool TryRead( 
            Phx.IR.Operand operand, 
            PurityAnalysisData data,
            out IEnumerable<HeapVertexBase> vertices)
        {
            vertices = null;
            return false;
        }

        internal override IEnumerable<HeapVertexBase> Read(
            Phx.IR.Operand operand,
            PurityAnalysisData data)
        {
            yield break;
        }

        internal override void Write(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            IEnumerable<HeapVertexBase> pointstoVertices)
        {
            return;
        }


#if POINTSTO
        internal override bool TryGetPointstoVertices(
            HeapGraph graph, 
            Phx.IR.Operand operand, 
            out IEnumerable<HeapVertexBase> vertices)
        {
            vertices = null;
            return false;
        }

        internal override IEnumerable<HeapVertexBase> GetOrCreatePointstoVertices(
            HeapGraph graph, 
            Phx.IR.Operand operand)
        {
            yield break;
        }
#endif

        internal override Field GetField(Phx.IR.Operand operand)
        {
            return null;
        }
    }
}
