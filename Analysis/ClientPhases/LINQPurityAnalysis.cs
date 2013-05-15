using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.IO;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;
using SafetyAnalysis.Purity.Summaries;

namespace SafetyAnalysis.Purity
{
    public class LINQPurityPhase : Phx.Phases.Phase
    {                    

#if (PHX_DEBUG_SUPPORT)
        private static Phx.Controls.ComponentControl StaticAnalysisPhaseCtrl;
#endif
        IEnumerable<MethodHeapVertex> linqmethods;
        private static string linqOutputSuffix = "-linq-output.txt";
        private StreamWriter outputFileWriter;

        private LINQPurityPhase(IEnumerable<MethodHeapVertex> linqmeths)
        {
            linqmethods = linqmeths;            
        }
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
            LINQPurityPhase.StaticAnalysisPhaseCtrl = 
                Phx.Controls.ComponentControl.New("LINQPurity",
                "Check LINQ methods for purity", "LINQPurityPhase.cs");            
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
        public static LINQPurityPhase New(Phx.Phases.PhaseConfiguration config,
            IEnumerable<MethodHeapVertex> linqmeths)
        {
            LINQPurityPhase initPhase =
                new LINQPurityPhase(linqmeths);

            initPhase.Initialize(config, "LINQPurity");
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

            //initialize output
            var assemblyname = moduleUnit.Manifest.Name.NameString;
            var dirname = PurityAnalysisPhase.outputdir;
            outputFileWriter = new StreamWriter(new FileStream(dirname + assemblyname + linqOutputSuffix, 
                FileMode.Create, FileAccess.Write, FileShare.Read));

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Generating Report for methods passed to linq queries");
            Console.ResetColor();
            this.outputFileWriter.WriteLine("Generating Report for linq methods");

            int linq_methods = 0;
            int pure_methods = 0;

            foreach (var methodVertex in linqmethods)
            {
                //synthesize a dummy call object
                var call = new StaticCall(1,
                    methodVertex.methodname,
                    methodVertex.typename,
                    methodVertex.signature, new List<string>());

                var datalist = (new CalleeSummaryReader(null, moduleUnit)).GetTargetSummaries(call);
                var data = AnalysisUtil.CollapsePurityData(datalist);
                
                linq_methods++;
                PurityReport report =
                    PurityReportUtil.GetPurityReport(
                    methodVertex.typename + "::" + methodVertex.methodname,
                    methodVertex.signature, data, false);

                report.Dump();
                report.Print(outputFileWriter);

                if (report.isPure)
                    pure_methods++;

            }
            outputFileWriter.WriteLine("# of methods passed to linq queries: " + linq_methods);
            outputFileWriter.WriteLine("# of methods pure methods passed to linq queries: " + pure_methods);
            outputFileWriter.Flush();
            outputFileWriter.Close();            
        }        
    }
}
