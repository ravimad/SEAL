using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.Contracts;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity.Summaries
{
    using VertexPair = Pair<HeapVertexBase, HeapVertexBase>;

    internal class NewObjSummaryHandler : CallSummaryHandler
    {        
        internal NewObjSummaryHandler()
        {
        }

        protected override HeapGraphBuilder GetHeapGraphBuilder(Phx.IR.CallInstruction inst)
        {
            //return new WSRNewObjBuilder();
            return new HigherOrderNewObjBuilder(inst.FunctionUnit);
        }

        internal override Predicate<object> GetPredicate()
        {
            return (obj) =>
            {
                if (!(obj is Phx.IR.CallInstruction))
                    return false;

                var instruction = obj as Phx.IR.CallInstruction;
                if (instruction.Opcode == Phx.Common.Opcode.NewObj)
                    return true;

                return false;
            };
        }
    }    
}
