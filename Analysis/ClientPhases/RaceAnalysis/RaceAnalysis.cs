﻿using System;
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
    public class RaceAnalysis : Phx.Phases.Phase
    {
        private static string OutputSuffix = "-race-output.txt";
        private StreamWriter outputFileWriter = null;                

        #region stats

        public List<RaceReport> allReports = new List<RaceReport>();
        public long numMethods = 0;

        #endregion stats

#if (PHX_DEBUG_SUPPORT)
        private static Phx.Controls.ComponentControl StaticAnalysisPhaseCtrl;
#endif

        private RaceAnalysis()
        {                              
        }

        public static void Initialize()
        {
#if PHX_DEBUG_SUPPORT
            RaceAnalysis.StaticAnalysisPhaseCtrl = 
                Phx.Controls.ComponentControl.New("Report",
                "Generate Purity report", "RaceAnalysis.cs");            
#endif
        }

        public static RaceAnalysis New(
            Phx.Phases.PhaseConfiguration config)
        {
            RaceAnalysis staticAnalysisPhase = new RaceAnalysis();
            staticAnalysisPhase.Initialize(config, "Race Analysis Phase");

#if PHX_DEBUG_SUPPORT
            staticAnalysisPhase.PhaseControl = RaceAnalysis.StaticAnalysisPhaseCtrl;
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

                    //check if the method is auto generated by the compiler                    
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
                        var report = RaceReportUtil.FindRaces(
                            node.FunctionSymbol.QualifiedName, 
                            PhxUtil.GetFunctionTypeSignature(node.FunctionSymbol.FunctionType),
                            summary.PurityData);
                        
                        //set some report metadata
                            report.Linenumber = linenumber;
                            report.Colnumber = colnumber;
                            report.Filename = filename;     
                  
                        allReports.Add(report);                        
                        numMethods++;
                    }
                }
            }            
            foreach (var report in allReports)
            {
                report.Dump();
                report.Print(this.outputFileWriter);
            }            
        }        
    }
}