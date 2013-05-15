using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafetyAnalysis.Framework.Graphs;

namespace SafetyAnalysis.Purity.HeapGraphHandler
{
    internal class FunctionOperandHandler : IHeapGraphOperandHandler
    {
        internal FunctionOperandHandler()
        {
        }

        internal override Predicate<object> GetPredicate()
        {
            return
                obj =>
                {
                    return (obj is Phx.IR.FunctionOperand);
                };
        }

        protected override bool TryRead(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            out IEnumerable<HeapVertexBase> vertices)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<HeapVertexBase> Read(
            Phx.IR.Operand operand,
            PurityAnalysisData data)
        {
            throw new NotImplementedException();
        }

        internal override void Write(
            Phx.IR.Operand operand,
            PurityAnalysisData data,
            IEnumerable<HeapVertexBase> pointstoVertices)
        {
            throw new NotImplementedException();
        }

        internal override Field GetField(Phx.IR.Operand operand)
        {
            return null;
        }
    }
}
