using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Purity.statistics
{
    public abstract class ExplosionReasons
    {
        int old_edges, new_edges;
        public ExplosionReasons(int oe, int ne)
        {
            old_edges = oe;
            new_edges = ne;
        }

        public string Pair2String()
        {
            return "[" + old_edges + " , " + new_edges + "]";
        }

        public abstract override string ToString();
    }

    public class UnknownReason : ExplosionReasons
    {
        public UnknownReason(int oe, int ne)
            : base(oe, ne)
        {
        }

        public override string ToString()
        {
            return "Unknown Reason "+base.Pair2String();
        }
    }

    public class CalleeSummary : ExplosionReasons
    {
        Phx.Graphs.CallNode node;
        public CalleeSummary(int oe, int ne, Phx.Graphs.CallNode callNode)
            : base(oe, ne)        
        {
            node = callNode;
        }

        public override string ToString()
        {
            return ("HugeCalleeSummary " + base.Pair2String() + " : " + node.FunctionSymbol.QualifiedName);
        }
    }

    public class VirtualMethodCall : ExplosionReasons
    {
        int targets;
        Phx.IR.CallInstruction callInst;

        public VirtualMethodCall(int oe, int ne, int noTargets, Phx.IR.CallInstruction inst)
            : base(oe, ne)
        {
            targets = noTargets;
            callInst = inst;
        }

        public override string ToString()
        {
            return ("TooManyTargets " + base.Pair2String() + " : " + targets + " , " + callInst.FunctionSymbol.QualifiedName);
        }
    }

    public class StaticImpureCallee : ExplosionReasons
    {
        Phx.Graphs.CallNode node;
        public StaticImpureCallee(int oe, int ne, Phx.Graphs.CallNode callNode)
            : base(oe, ne)
        {
            node = callNode;
        }

        public override string ToString()
        {
            return ("StaticImpureCallee " + node.FunctionSymbol.QualifiedName);
        }
    }
}
