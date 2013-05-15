using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;

using SafetyAnalysis.Util;
using SafetyAnalysis.Purity.Summaries;
using SafetyAnalysis.TypeUtil;

namespace SafetyAnalysis.Purity
{
    public class AddCallGraphEdgesPhase : Phx.Phases.Phase
    {

#if (PHX_DEBUG_SUPPORT)
        private static Phx.Controls.ComponentControl StaticAnalysisPhaseCtrl;
#endif

        //---------------------------------------------------------------------
        //
        // Description:
        //
        //    Initialize the static analysis phase.
        //
        // Remarks:
        //
        //    Sets up a component control under debug builds, so that we
        //    can dump IR before/after the phase and so on. In other
        //    words, because we create this control, you can pass in
        //    options like -predumpmtrace that will dump the IR for each
        //    method before this phase runs.
        //
        //---------------------------------------------------------------------
        public static void Initialize()
        {
#if PHX_DEBUG_SUPPORT
            AddCallGraphEdgesPhase.StaticAnalysisPhaseCtrl =
                Phx.Controls.ComponentControl.New("AddCallGraphEdgesPhase",
                "Add Edges to call graph", "Purity.cs");
#endif
        }

        //---------------------------------------------------------------------
        //
        // Description:
        //
        //    Creates a new StaticAnalysisPhase object
        //
        // Arguments:
        //
        //    config - The encapsulating object that simplifies handling of
        //    the phase list and pre and post phase events.
        //
        //---------------------------------------------------------------------
        public static AddCallGraphEdgesPhase New(Phx.Phases.PhaseConfiguration config)
        {
            AddCallGraphEdgesPhase initPhase = new AddCallGraphEdgesPhase();

            initPhase.Initialize(config, "AddCallGraphEdges");
            return initPhase;
        }

        //---------------------------------------------------------------------
        //
        // Description:
        //
        //    Executes the StaticAnalysisPhase phase.
        //
        // Arguments:
        //
        //    unit - The unit that this phase should operate on.
        //
        // Remarks:
        //
        //    You should add your static analysis code here.  Each unit of the
        //    program will pass through this function.
        //
        //---------------------------------------------------------------------
        protected override void Execute(Phx.Unit unit)
        {
            if (!unit.IsFunctionUnit)
                return;

            Phx.FunctionUnit functionUnit = unit.AsFunctionUnit;
            Phx.PEModuleUnit moduleUnit = unit.ParentPEModuleUnit;
            Phx.Graphs.CallGraph callGraph = moduleUnit.CallGraph;
            Phx.Graphs.CallNode callerNode = functionUnit.FunctionSymbol.CallNode;                        

            if (!AnalyzableMethods.IsAnalyzable(functionUnit.FunctionSymbol))
                return;

            if (!functionUnit.IRState.Equals(Phx.FunctionUnit.HighLevelIRFunctionUnitState))                            
                moduleUnit.Raise(functionUnit.FunctionSymbol, Phx.FunctionUnit.HighLevelIRFunctionUnitState);            

            foreach (Phx.IR.Instruction inst in functionUnit.Instructions)
            {
                if (inst.IsCallInstruction)
                {                    
                    if (inst.AsCallInstruction.IsIndirectCall)
                    {
                        callGraph.CreateUniqueCallEdge(callerNode, null);
                        continue;
                    }

                    //get normalized function symbol
                    var functionSymbol = PhxUtil.NormalizedFunctionSymbol(inst.AsCallInstruction.FunctionSymbol);
                    var enclType = PhxUtil.NormalizedAggregateType(functionSymbol.EnclosingAggregateType);                    
                        
                    if (TypeMethodInfoUtil.IsDelegateCall(inst.AsCallInstruction))
                    {
                        var typesig = PhxUtil.GetFunctionTypeSignature(inst.AsCallInstruction.FunctionType);
                        var calleeSyms = PhxUtil.GetCompatibleFuncionSymbols(moduleUnit, typesig);
                        if (calleeSyms.Any())
                        {
                            foreach (var calleeSym in calleeSyms)
                            {
                                var normalizedSymbol = PhxUtil.NormalizedFunctionSymbol(calleeSym);
                                //do not add edges if this is not analyzable
                                if (!AnalyzableMethods.IsAnalyzable(normalizedSymbol))
                                    continue;
                                callGraph.CreateUniqueCallEdge(callerNode, calleeSym);
                            }
                        }
                        continue;
                    }

                    if (TypeMethodInfoUtil.IsVirtualCall(inst.AsCallInstruction))
                    {
                        //handle virtual calls here
                        var receiverType = PhxUtil.GetNormalizedReceiver(inst.AsCallInstruction);
                        if (receiverType == null)
                            receiverType = enclType;

                        var th = CombinedTypeHierarchy.GetInstance(moduleUnit);
                        var recvrTypename = PhxUtil.GetTypeName(receiverType);
                        var recvrTypeinfo = th.LookupTypeInfo(recvrTypename);


                        string funcname = PhxUtil.GetFunctionName(functionSymbol);
                        string funcsig = PhxUtil.GetFunctionTypeSignature(functionSymbol.FunctionType);

                        //find all the inherited methods with the same signature                            
                        var inheritedMethods =
                            th.GetInheritedMethods(recvrTypeinfo, funcname, funcsig).OfType<InternalMethodInfo>();

                        //find all the sub-types of the receiver type.                                                     
                        var subTypeMethods = new List<InternalMethodInfo>();
                        foreach (var subtypeinfo in th.GetSubTypesFromTypeHierarchy(recvrTypeinfo).OfType<InternalTypeInfo>())
                        {
                            var methodinfos = subtypeinfo.GetMethodInfos(funcname, funcsig).OfType<InternalMethodInfo>();
                            subTypeMethods.AddRange(methodinfos);
                        }

                        //now add edges to all the subtype methods and inherited method                        
                        foreach (var methodinfo in inheritedMethods.Union(subTypeMethods))
                        {
                            var normalizedSymbol = PhxUtil.NormalizedFunctionSymbol(methodinfo.GetFunctionSymbol());

                            //do not add edges if this is not analyzable
                            if (!AnalyzableMethods.IsAnalyzable(normalizedSymbol))
                                continue;

                            if (PhxUtil.DoesBelongToCurrentAssembly(normalizedSymbol, moduleUnit))
                                callGraph.CreateUniqueCallEdge(callerNode, normalizedSymbol);
                        }
                        continue;
                    }
                    
                    //handle direct calls here.    
                    //do not add edges if this is not a analyzable call.
                    if (!AnalyzableMethods.IsAnalyzable(functionSymbol))
                        continue;
                    callGraph.CreateUniqueCallEdge(callerNode, functionSymbol);                        
                }
            }            
        }        
    }
}
