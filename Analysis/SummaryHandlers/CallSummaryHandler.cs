using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity;
using SafetyAnalysis.TypeUtil;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity.Summaries
{
    using CallDataPair = KeyValuePair<Phx.Graphs.CallNode, PurityAnalysisData>;

    public class CallSummaryHandler : IPuritySummaryHandler
    {
        public static bool constrainedprefix = false;

        internal override Predicate<object> GetPredicate()
        {
            return (obj) =>
            {
                if (!(obj is Phx.IR.CallInstruction))
                    return false;

                var instruction = obj as Phx.IR.CallInstruction;
                if (instruction.Opcode == Phx.Common.Opcode.Call ||
                    instruction.Opcode == Phx.Common.Opcode.CallVirt)
                {                                        
                    return true;
                }

                return false;
            };
        }

        protected virtual HeapGraphBuilder GetHeapGraphBuilder(Phx.IR.CallInstruction inst)
        {           
            return new HigherOrderHeapGraphBuilder(inst.FunctionUnit);
        }
        
        private CallResolver GetResolver(Call call, Phx.FunctionUnit funit)
        {
            return new CallBackResolver(funit);                
        }
        
        internal override void ApplySummary(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            //Contract.Requires(instruction is Phx.IR.CallInstruction);
            var callInstruction = instruction.AsCallInstruction;
            var functionSymbol = PhxUtil.NormalizedFunctionSymbol(callInstruction.FunctionSymbol);
            var call = this.GetCallFromInstruction(callInstruction, data);            

            //stats;
            if (PurityAnalysisPhase.EnableStats)
                MethodLevelAnalysis.total_method_calls++;

            //If the callee is a linq method identify the delegates passed to the linq methods.
            //This does not have any relevance to the summary application as such.            
            if (PhxUtil.IsLinqMethod(functionSymbol))
            {
                foreach (var target in
                    GetDelegateParamsTarget(callInstruction, data))
                    data.AddLinqMethod(target);
            }

            CallResolver resolver = GetResolver(call, callInstruction.FunctionUnit);            
            resolver.ApplySummary(call, data, this.GetHeapGraphBuilder(callInstruction));            
        }
        
        public Call GetCallFromInstruction(Phx.IR.CallInstruction callInstruction, PurityAnalysisData data)
        {
            uint cid = AnalysisUtil.GetCallInstId(callInstruction);
            var funcSym = PhxUtil.NormalizedFunctionSymbol(callInstruction.FunctionSymbol);
            var enclType = PhxUtil.NormalizedAggregateType(funcSym.EnclosingAggregateType);
            var containingSym = callInstruction.FunctionUnit.FunctionSymbol;
            var containingMethodName = containingSym.QualifiedName;
            var QualifiedContainingMethodName = PhxUtil.GetQualifiedFunctionName(containingSym);

            if (TypeMethodInfoUtil.IsDelegateCall(callInstruction))
            {
                var opdIter = callInstruction.ExplicitSourceOperands.GetEnumerator();
                while (opdIter.MoveNext())
                {
                    var current = opdIter.Current;

                    if (current.IsCodeTargetOperand)
                        continue;
                    //if (current.Type.IsPrimitiveType)
                    //    continue;
                    break;
                }
                var targetOpd = opdIter.Current;
                var id = NodeEquivalenceRelation.GetVariableId(targetOpd);
                var targetVertex = NodeEquivalenceRelation.CreateVariableHeapVertex(
                    containingMethodName, id, Context.EmptyContext);
                
                if (!data.OutHeapGraph.ContainsVertex(targetVertex))
                    data.OutHeapGraph.AddVertex(targetVertex);

                var dcall = new DelegateCall(cid, targetVertex,
                                    PhxUtil.GetFunctionTypeSignature(funcSym.FunctionType), new List<string> { QualifiedContainingMethodName });

                int paramCount = 0;
                while (opdIter.MoveNext())
                {
                    var current = opdIter.Current;

                    if (current.IsCodeTargetOperand)
                        continue;
                    //if (current.Type.IsPrimitiveType)
                    //    continue;

                    var currid = NodeEquivalenceRelation.GetVariableId(current);
                    var vertex = NodeEquivalenceRelation.CreateVariableHeapVertex(
                        containingMethodName, currid, Context.EmptyContext);

                    if (!data.OutHeapGraph.ContainsVertex(vertex))
                        data.OutHeapGraph.AddVertex(vertex);

                    dcall.AddParam(paramCount, vertex);
                    paramCount++;
                }

                var destOperand = callInstruction.DestinationOperand;
                if (destOperand.IsVariableOperand
                    /*&& !destOperand.Type.IsPrimitiveType*/)
                {
                    var destid = NodeEquivalenceRelation.GetVariableId(destOperand);
                    var destVertex = NodeEquivalenceRelation.CreateVariableHeapVertex(
                        containingMethodName, destid, Context.EmptyContext);

                    if (!data.OutHeapGraph.ContainsVertex(destVertex))
                        data.OutHeapGraph.AddVertex(destVertex);

                    dcall.AddReturnValue(destVertex);
                }
                return dcall;
            }
            else
            {
                Call call;
                if (TypeMethodInfoUtil.IsVirtualCall(callInstruction))
                    call = new VirtualCall(cid, PhxUtil.GetFunctionName(funcSym),
                        PhxUtil.GetFunctionTypeSignature(funcSym.FunctionType),
                        PhxUtil.GetTypeName(enclType), new List<string> { QualifiedContainingMethodName });
                else // normal call 
                    call = new StaticCall(cid,
                        PhxUtil.GetFunctionName(funcSym),
                        PhxUtil.GetTypeName(enclType),
                        PhxUtil.GetFunctionTypeSignature(funcSym.FunctionType), new List<string> { QualifiedContainingMethodName });

                int paramCount = 0;
                foreach (Phx.IR.Operand parameterOperand in callInstruction.ExplicitSourceOperands)
                {
                    if (parameterOperand.IsCodeTargetOperand)
                        continue;

                    //if (parameterOperand.Type.IsPrimitiveType)
                    //    continue;

                    var id = NodeEquivalenceRelation.GetVariableId(parameterOperand);
                    var paramv = NodeEquivalenceRelation.CreateVariableHeapVertex(containingMethodName, id, Context.EmptyContext);

                    if (!data.OutHeapGraph.ContainsVertex(paramv))
                        data.OutHeapGraph.AddVertex(paramv);

                    //check if the constrained prefic flag is set
                    if (constrainedprefix && (call is VirtualCall) && (paramCount == 0))
                    {
                        constrainedprefix = false;

                        //in case the reciever is pointer to a ref var the deref the receiver (semantics of constrainedprefix)
                        var tgtvars = from edge in data.OutHeapGraph.OutEdges(paramv)                                      
                                      select edge.Target;
                        if (tgtvars.Count() == 1 && tgtvars.First() is VariableHeapVertex)
                        {
                            paramv = tgtvars.First() as VariableHeapVertex;
                        }
                    }
                    call.AddParam(paramCount, paramv);
                    paramCount++;
                }

                var destOperand = callInstruction.DestinationOperand;
                if (destOperand.IsVariableOperand
                    /*&& !destOperand.Type.IsPrimitiveType*/)
                {
                    var destid = NodeEquivalenceRelation.GetVariableId(destOperand);
                    var destVertex = NodeEquivalenceRelation.CreateVariableHeapVertex(
                        containingMethodName, destid, Context.EmptyContext);

                    if (!data.OutHeapGraph.ContainsVertex(destVertex))
                        data.OutHeapGraph.AddVertex(destVertex);

                    call.AddReturnValue(destVertex);
                }
                return call;
            }
        }

        private IEnumerable<HeapVertexBase> GetDelegateParamsTarget(
            Phx.IR.CallInstruction callInst,
            PurityAnalysisData callerData)
        {
            var moduleunit = callInst.FunctionUnit.ParentPEModuleUnit;
            var th = CombinedTypeHierarchy.GetInstance(moduleunit);

            foreach (Phx.IR.Operand sourceOperand in callInst.ExplicitSourceOperands)
            {
                Phx.Types.AggregateType aggtype;
                if (PhxUtil.TryGetAggregateType(sourceOperand.Type, out aggtype))
                {
                    var normaggtype = PhxUtil.NormalizedAggregateType(aggtype);
                    var typeinfo = th.LookupTypeInfo(PhxUtil.GetTypeName(normaggtype));
                    if (!th.IsHierarhcyKnown(typeinfo))                    
                        continue;
                    
                    if (TypeMethodInfoUtil.IsDelegateType(th, typeinfo))
                    {
                        return (new DelegateOperandHandler()).Read(
                            sourceOperand, DelegateMethodField.GetInstance(), callerData);
                    }
                }
            }
            return new List<HeapVertexBase>();
        }

    }
}
