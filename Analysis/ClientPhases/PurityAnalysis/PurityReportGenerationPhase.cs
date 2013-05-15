using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{
    public class PurityReportGenerationPhase : Phx.Phases.Phase
    {
        private static string OutputSuffix = "-sideeffects-output.txt";
        private StreamWriter outputFileWriter = null;                

        #region stats

        public List<PurityReport> allReports = new List<PurityReport>();
        public List<PurityReport> pureReports = new List<PurityReport>();
        public List<PurityReport> impureReports = new List<PurityReport>();
        public List<PurityReport> condPureReports = new List<PurityReport>();
        public List<PurityReport> incompleteImpureReports = new List<PurityReport>();
        public long numMethods = 0;

        #endregion stats

#if (PHX_DEBUG_SUPPORT)
        private static Phx.Controls.ComponentControl StaticAnalysisPhaseCtrl;
#endif

        private PurityReportGenerationPhase()
        {                              
        }

        public static void Initialize()
        {
#if PHX_DEBUG_SUPPORT
            PurityReportGenerationPhase.StaticAnalysisPhaseCtrl = 
                Phx.Controls.ComponentControl.New("Report",
                "Generate Purity report", "PurityReportGenerationPhase.cs");            
#endif
        }

        public static PurityReportGenerationPhase New(
            Phx.Phases.PhaseConfiguration config)
        {
            PurityReportGenerationPhase staticAnalysisPhase = new PurityReportGenerationPhase();
            staticAnalysisPhase.Initialize(config, "Purity Report Generation");

#if PHX_DEBUG_SUPPORT
            staticAnalysisPhase.PhaseControl = PurityReportGenerationPhase.StaticAnalysisPhaseCtrl;
#endif
            return staticAnalysisPhase;
        }

        protected override void Execute(Phx.Unit unit)
        {
            if (!unit.IsPEModuleUnit)
                return;

            Phx.PEModuleUnit moduleUnit = unit.AsPEModuleUnit;
            //initialize output
            var assemblyname = moduleUnit.Manifest.Name.NameString;
            var dirname = PurityAnalysisPhase.outputdir;
            outputFileWriter = new StreamWriter(new FileStream(dirname + assemblyname + OutputSuffix,
                FileMode.Create, FileAccess.Write, FileShare.Read));

            Phx.Graphs.CallGraph callGraph = moduleUnit.CallGraph;
            Phx.Graphs.SummaryManager summaryManager = callGraph.SummaryManager;                       

            string value;
            bool displayAsVSErrorMsg = false;
            if (PurityAnalysisPhase.properties.TryGetValue("displayasvserrormsg", out value))
                displayAsVSErrorMsg = Boolean.Parse(value);

            var nodeOrder = Phx.Graphs.NodeFlowOrder.New(unit.Lifetime);
            nodeOrder.Build(callGraph, Phx.Graphs.Order.PostOrder);
            for (uint i = 1; i <= nodeOrder.NodeCount; i++)
            {
                Phx.Graphs.CallNode node = nodeOrder.Node(i).AsCallNode;
                var funcsym = node.FunctionSymbol;

                if (AnalyzableMethods.IsAnalyzable(node.FunctionSymbol))
                {
                    //Console.WriteLine("Generating report for: "+node.FunctionSymbol.QualifiedName);
                    uint linenumber = 0, colnumber = 0;
                    string filename = String.Empty;
                                        
                    if (PurityAnalysisPhase.properties.TryGetValue("checkonlyusermethods", out value)
                        && Boolean.Parse(value))
                    {                                       
                        linenumber = funcsym.DebugInfo.GetLineNumber(funcsym.DebugTag);                                                
                        if (linenumber == 0)
                            continue;
                        colnumber = funcsym.DebugInfo.GetColumnNumber(funcsym.DebugTag);
                        filename = funcsym.DebugInfo.GetFileName(funcsym.DebugTag).NameString.Split('\\').Last();
                    }
                    
                    PurityAnalysisSummary summary = (PurityAnalysisSummary)summaryManager.RetrieveSummary(node, PurityAnalysisSummary.Type);
                    if (summary != null)
                    {
                        bool isConstructor = PhxUtil.IsConstructor(node.FunctionSymbol.NameString);
                        var report = PurityReportUtil.GetPurityReport(
                            node.FunctionSymbol.QualifiedName, 
                            PhxUtil.GetFunctionTypeSignature(node.FunctionSymbol.FunctionType),
                            summary.PurityData, 
                            isConstructor);
                        
                        if (displayAsVSErrorMsg)
                        {
                            //set some report metadata
                            report.Linenumber = linenumber;
                            report.Colnumber = colnumber;
                            report.Filename = filename;
                        }
                        allReports.Add(report);
                        //report.Print(this.outputFileWriter);

                        if (report.isPure)
                        {
                            if (!report.skwitness.Any())
                                pureReports.Add(report);
                            else
                                condPureReports.Add(report);
                        }                        
                        else
                        {
                            if (!report.skwitness.Any())
                                impureReports.Add(report);
                            else
                                incompleteImpureReports.Add(report);
                        }
                        numMethods++;
                    }
                }
            }
            this.outputFileWriter.WriteLine("Pure methods: ");            
            foreach (var report in pureReports)
            {
                if (displayAsVSErrorMsg)
                    report.DumpAsVSErrorMsg();
                report.Print(this.outputFileWriter);
            }

            this.outputFileWriter.WriteLine("Conditionally Pure methods: ");            
            foreach (var report in condPureReports)
            {
                if (displayAsVSErrorMsg)
                    report.DumpAsVSErrorMsg();
                report.Print(this.outputFileWriter);
            }

            this.outputFileWriter.WriteLine("ImPure methods: ");
            foreach (var report in impureReports)
            {
                if (displayAsVSErrorMsg)
                    report.DumpAsVSErrorMsg();
                report.Print(this.outputFileWriter);
            }

            this.outputFileWriter.WriteLine("ImPure Incomplete methods: ");            
            foreach (var report in incompleteImpureReports)
            {
                if (displayAsVSErrorMsg)
                    report.DumpAsVSErrorMsg();
                report.Print(this.outputFileWriter);
            }

            this.outputFileWriter.WriteLine("# of methods: " + numMethods);
            this.outputFileWriter.WriteLine("# of pure methods: " + pureReports.Count);
            this.outputFileWriter.WriteLine("# of conditionally pure methods: " + condPureReports.Count);
            this.outputFileWriter.WriteLine("# of impure methods: " + impureReports.Count);
            this.outputFileWriter.WriteLine("# of impure incomplete methods: " + incompleteImpureReports.Count);
            
            //serialize the purity reports                                    
            if (PurityAnalysisPhase.properties.TryGetValue("witnessserialization", out value)
                && Boolean.Parse(value))
            {
                string reportfilename = null;
                PurityAnalysisPhase.properties.TryGetValue("reportfilename", out reportfilename);                

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                SerializeReports(allReports, PurityAnalysisPhase.outputdir + reportfilename);
                sw.Stop();
                this.outputFileWriter.WriteLine("Serialization time: " + sw.Elapsed);
            }
        }

        public void DumpCumulativeStats(PurityReportGenerationPhase reportphase)
        {
            this.outputFileWriter.WriteLine("Cumulative Statistics: ");
            this.outputFileWriter.WriteLine("\t methods: " + reportphase.numMethods);
            this.outputFileWriter.WriteLine("\t Pure methods: " + reportphase.pureReports.Count);
            this.outputFileWriter.WriteLine("\t Conditionally pure methods: " + reportphase.condPureReports.Count);
            this.outputFileWriter.WriteLine("\t Impure methods: " + reportphase.impureReports.Count);
            this.outputFileWriter.WriteLine("\t Impure incomplete methods: " + reportphase.incompleteImpureReports.Count);            
        }

        private void SerializeReports(List<PurityReport> reports, string outputFilename)
        {
            BinaryFormatter serializer = new BinaryFormatter();            
            FileStream stream = new FileStream(outputFilename, FileMode.Create, FileAccess.Write, FileShare.None);            
            serializer.Serialize(stream, reports);
            stream.Close();
        }
    }
}
