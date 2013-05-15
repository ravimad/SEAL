using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using SafetyAnalysis.Util;
using SafetyAnalysis.Framework.Graphs;

namespace SafetyAnalysis.Purity
{
    public class InitializationPhase : Phx.Phases.Phase
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
            InitializationPhase.StaticAnalysisPhaseCtrl = 
                Phx.Controls.ComponentControl.New("init",
                "Perform Intialization", "InitializationPhase.cs");            
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
        public static InitializationPhase New(Phx.Phases.PhaseConfiguration config)
        {
            InitializationPhase initPhase =
                new InitializationPhase();

            initPhase.Initialize(config, "init");
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
            if (!unit.IsPEModuleUnit)
            {
                return;
            }

            Phx.PEModuleUnit moduleUnit = unit.AsPEModuleUnit;

            Phx.Graphs.NodeFlowOrder nodeOrder = Phx.Graphs.NodeFlowOrder.New(unit.Lifetime);
            nodeOrder.Build(moduleUnit.CallGraph, Phx.Graphs.Order.PostOrder);
            for (uint i = 1; i <= nodeOrder.NodeCount; i++)
            {
                Phx.Graphs.CallNode node = nodeOrder.Node(i).AsCallNode;
                if (!AnalyzableMethods.IsAnalyzable(node.FunctionSymbol))
                {
                    continue;
                }

                Phx.FunctionUnit functionUnit = node.FunctionSymbol.FunctionUnit;
                if (!functionUnit.IRState.Equals(Phx.FunctionUnit.HighLevelIRFunctionUnitState))
                    moduleUnit.Raise(node.FunctionSymbol, Phx.FunctionUnit.HighLevelIRFunctionUnitState);                

                AddInstructionIds(functionUnit);                

                if (PurityAnalysisPhase.DumpIR)
                {
                    DumpIR(functionUnit);
                }
                
                InitializePuritySummary(node);
                
                functionUnit.Context.PopUnit();
            }            
        }

        private void DumpIR(Phx.FunctionUnit funit)
        {
            var filename = GeneralUtil.ConvertToFilename(funit);
            var writer = new StreamWriter(new FileStream(PurityAnalysisPhase.outputdir + filename + ".ir",
                FileMode.Create, FileAccess.Write, FileShare.Read));
            foreach (var inst in funit.Instructions)
            {
                writer.WriteLine(inst.InstructionId + "\t" + inst);
            }
            writer.Flush();
            writer.Close();
        }

        private void AddInstructionIds(Phx.FunctionUnit functionUnit)
        {
            uint id = 1;
            foreach (var inst in functionUnit.Instructions)            
                inst.InstructionId = id++;            
        }

        private void InitializePuritySummary(Phx.Graphs.CallNode node)            
        {
            Phx.FunctionUnit functionUnit = node.FunctionSymbol.FunctionUnit;
            Phx.IR.Instruction firstInst = functionUnit.FirstEnterInstruction;
            if (firstInst == null)
            {
                Trace.TraceWarning("Cannot initialize method {0} as first Instruction is null", 
                    node.FunctionSymbol.QualifiedName);                
                return;
            }
            //Contract.Assert(firstInst.Opcode == Phx.Common.Opcode.EnterFunction);

            //initialize the function summary by creating only the parameter nodes
            // and global nodes
            var transformer = new PurityAnalysisTransformers();
            PurityAnalysisData data = new PurityAnalysisData(new HeapGraph());
            transformer.Apply(firstInst, data);

            PurityAnalysisSummary puritySummary =
               PurityAnalysisSummary.New(node.CallGraph.SummaryManager, data as PurityAnalysisData);

            bool summaryAdded = node.CallGraph.SummaryManager.AddSummary(node, puritySummary);

            //Contract.Assert(summaryAdded);
        }
    }
}
