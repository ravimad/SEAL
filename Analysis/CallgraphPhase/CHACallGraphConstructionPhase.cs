using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Purity.callgraph
{
    public class CHACallGraphConstructionPhase : Phx.Phases.Phase
    {             
        
#if (PHX_DEBUG_SUPPORT)
        private static Phx.Controls.ComponentControl StaticAnalysisPhaseCtrl;
#endif

        public static void Initialize()
        {
#if PHX_DEBUG_SUPPORT
            CHACallGraphConstructionPhase.StaticAnalysisPhaseCtrl =
                Phx.Controls.ComponentControl.New("CHAcallgraph",
                "CHA based call-graph construction", "CHACallGraphConstructionPhasecs");            
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
        public static CHACallGraphConstructionPhase New(Phx.Phases.PhaseConfiguration config)
        {
            CHACallGraphConstructionPhase staticAnalysisPhase =
                new CHACallGraphConstructionPhase();

            staticAnalysisPhase.Initialize(config, "Callgraph construction");

#if PHX_DEBUG_SUPPORT
            staticAnalysisPhase.PhaseControl =
                 CHACallGraphConstructionPhase.StaticAnalysisPhaseCtrl;
#endif
            return staticAnalysisPhase;
        }

        protected override void Execute(Phx.Unit unit)
        {
            if (!unit.IsPEModuleUnit)
                return;

            Phx.PEModuleUnit moduleUnit = unit.AsPEModuleUnit;            
            Phx.Phases.PhaseConfiguration callGraphPhaseConfiguration =
               Phx.Phases.PhaseConfiguration.New(unit.Lifetime, "CallGraph Phases");

            callGraphPhaseConfiguration.PhaseList.AppendPhase(Phx.PE.BuildCallGraphNodesPhase.New(
                callGraphPhaseConfiguration));

            Phx.PE.UnitListPhaseList unitPhases = Phx.PE.UnitListPhaseList.New(callGraphPhaseConfiguration,
                Phx.PE.UnitListWalkOrder.PrePass);

            unitPhases.AppendPhase(Phx.PE.RaiseIRPhase.New(callGraphPhaseConfiguration,
                Phx.FunctionUnit.SymbolicFunctionUnitState));
            unitPhases.AppendPhase(Phx.PE.SymbolReferenceMapPhase.New(callGraphPhaseConfiguration));
            unitPhases.AppendPhase(SafetyAnalysis.Purity.AddCallGraphEdgesPhase.New(callGraphPhaseConfiguration));
            unitPhases.AppendPhase(Phx.PE.DiscardIRPhase.New(callGraphPhaseConfiguration));

            callGraphPhaseConfiguration.PhaseList.AppendPhase(unitPhases);
            callGraphPhaseConfiguration.PhaseList.DoPhaseList(moduleUnit);                         
        }
    }
}
