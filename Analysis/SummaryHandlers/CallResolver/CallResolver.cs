using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Purity.HandlerProvider;

namespace SafetyAnalysis.Purity.Summaries
{
    public abstract class CallResolver
    {
        protected IPredicatedOperandHandlerProvider<IHeapGraphOperandHandler> _operandHandlerProvider;
        internal CallResolver()
        {            
            _operandHandlerProvider = new PredicatedOperandHandlerProvider<IHeapGraphOperandHandler>();
            _operandHandlerProvider.RegisterHandler(new VariableOperandHandler());
            _operandHandlerProvider.RegisterHandler(new AbsoluteOperandHandler());            
            _operandHandlerProvider.RegisterHandler(new ComplexOperandHandler());
            _operandHandlerProvider.RegisterHandler(new SymbolicOperandHandler());
            _operandHandlerProvider.RegisterHandler(new PointerOperandHandler());
            _operandHandlerProvider.RegisterHandler(new ImmediateOperandHandler());
        }        

        public abstract void ApplySummary(
            Call call,
            PurityAnalysisData data,
            HeapGraphBuilder builder);

        protected void ApplyTargetsSummary(
           Call call,
           PurityAnalysisData callerData,
           PurityAnalysisData calleeSummary,
           HigherOrderHeapGraphBuilder builder)
        {
            //skip this callee if there no way to come back to the caller from the callee. 
            //may happen when the callee terminates the program or throw an Exception unconditionally.             
            if (calleeSummary != null &&
                !calleeSummary.OutHeapGraph.IsVerticesEmpty)
            {
                var clonedCalleeData = AnalysisUtil.TranslateSummaryToCallerNamespace(
                    calleeSummary, new List<object> { call.contextid });

                //add the return vertex to the data and mark it as strong update
                //if it is not added    
                if (call.HasReturnValue())
                {
                    var retvar = call.GetReturnValue();
                    if (PurityAnalysisPhase.FlowSensitivity &&
                        callerData.OutHeapGraph.ContainsVertex(retvar))
                    {
                        callerData.AddVertexWithStrongUpdate(retvar);
                    }
                }
                builder.ComposeCalleeSummary(call, callerData, clonedCalleeData);
            }
        }
    }
}
