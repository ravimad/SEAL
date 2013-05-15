using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

using QuickGraph.Collections;
namespace SafetyAnalysis.Framework.Graphs
{
    public class NodeEquivalenceRelation
    {        
        public static ForestDisjointSet<HeapVertexBase> vertexPool =
            new ForestDisjointSet<HeapVertexBase>();
        
        public static LoadHeapVertex CreateLoadHeapVertex(string functionName, uint siteId, 
            Context context)
        {
            //TODO: decide whether or not function name is useful in computing the site id.
            var loadVertex = LoadHeapVertex.New(siteId, context);
            return GetRepresentative(loadVertex) as LoadHeapVertex;            
        }

        public static ReturnedValueVertex CreateReturnedValueVertex(uint siteId, Context context)
        {
            var rvVertex = ReturnedValueVertex.New(siteId, context);
            return GetRepresentative(rvVertex) as ReturnedValueVertex;
        }

        public static InternalHeapVertex CreateInternalHeapVertex(string functionName, uint siteId,
            Context context)
        {
            //TODO: decide whether or not function name is useful in computing the site id.
            var internalVertex = InternalHeapVertex.New(siteId, context);
            return GetRepresentative(internalVertex) as InternalHeapVertex;
        }

        public static VariableHeapVertex CreateVariableHeapVertex(string funcname, uint id, Context context)
        {                        
            //var funcname = operand.FunctionUnit.FunctionSymbol.QualifiedName;
            VariableHeapVertex varVertex = VariableHeapVertex.New(funcname, id, context);
            return GetRepresentative(varVertex) as VariableHeapVertex;
        }

        public static HeapVertexBase GetRepresentative(HeapVertexBase v)
        {
            if (vertexPool.Contains(v))
            {
                var rep = vertexPool.FindSet(v);
                if (!rep.GetType().Equals(v.GetType()))
                    throw new NotSupportedException("Rep type doesn't match the vertex type");
                return rep;
            }
            return v;
        }

        public static void UnionNodes(HeapVertexBase a, HeapVertexBase b)
        {
            if (!a.GetType().Equals(b.GetType()))
                throw new NotSupportedException("Types of a and b do not match");

            if (!vertexPool.Contains(a))
                vertexPool.MakeSet(a);

            if (!vertexPool.Contains(b))
                vertexPool.MakeSet(b);

            vertexPool.Union(a, b);
        }

        public static HeapVertexBase ChooseRepresentative(HeapVertexSet vertices)
        {
            var first = vertices.First();
            foreach (var v in vertices)
            {
                NodeEquivalenceRelation.UnionNodes(first, v);
            }
            var rep = NodeEquivalenceRelation.GetRepresentative(first);            
            return rep;
        }

        public static uint GetVariableId(Phx.IR.Operand operand)
        {
            return operand.IsTemporary ? operand.TemporaryId : operand.SymbolId;
        }

        public static uint GetLoadNodeId(Phx.IR.Instruction inst)
        {
            return (inst.InstructionId ^ (inst.FunctionUnit.Number << 13));
        }

        public static uint GetInternalNodeId(Phx.IR.Instruction inst)
        {
            return (inst.InstructionId ^ (inst.FunctionUnit.Number << 13));
        }

        //clears all the internal state.
        public static void Reset()
        {
            vertexPool = new ForestDisjointSet<HeapVertexBase>();            
        }
    }
}
