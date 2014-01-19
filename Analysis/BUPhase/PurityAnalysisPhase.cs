#region Using namespace declarations

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

using Phx.Graphs;
using SafetyAnalysis.Purity.Properties;
using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.Summaries;
using SafetyAnalysis.Purity.statistics;
using SafetyAnalysis.Util;
using SafetyAnalysis.Purity.callgraph;
using SafetyAnalysis.TypeUtil;
#endregion


namespace SafetyAnalysis.Purity
{       
    public class PurityAnalysisPhase : Phx.Phases.Phase
    {             
        
#if (PHX_DEBUG_SUPPORT)
        private static Phx.Controls.ComponentControl StaticAnalysisPhaseCtrl;
#endif

        public static Dictionary<string, string> properties = new Dictionary<string, string>();
        public static string sealHome;                   
        public const string lambdaDelegateName = "CachedAnonymousMethodDelegate";    

        #region output flags

        public static bool EnableStats = false;
        public static bool EnableConsoleLogging = false;
        public static bool EnableLogging = false;        

        #endregion output flags

        #region debug flags

        public static bool trackUnanalyzableCalls = false;
        public static bool TraceSkippedCallResolution = false;
        public static bool TraceSummaryApplication = false;
        public static bool DumpIR = false;
        public static bool DumpSummariesAsGraphs = false;

        #endregion debug flags

        #region analysis flags
        
        public static bool FlowSensitivity = false;        
        public static bool DisableExternalCallResolution = false;
        public static bool BoundContextStr = false;
        public static int  ContextStrBound = 0;                
        public static bool DisableSummaryReduce = false;
        public static bool clearDBContents = false;
        public static bool TrackPrimitiveTypes = false;
        public static bool TrackStringConstants = false;

        #endregion analysis flags        

        #region outputfiles
        
        public static string outputdir = null;        
        public StreamWriter statsFileWriter;
        public StreamWriter unaCallsWriter;
        public XmlWriter chaCallGraphWriter;
        public XmlWriter finalCallGraphWriter;

        #endregion outputfiles        
       
        #region stats

        public static Dictionary<Phx.Graphs.CallNode, int> iterationCounts = new Dictionary<Phx.Graphs.CallNode, int>();           

        #endregion stats        

        public static PurityDBDataContext DataContext
        {
            get
            {
                string value;
                properties.TryGetValue("db", out value);

                //if (value.Equals("cloud"))
                //    return new PurityDBDataContext(DBSettings.Default.CloudDBConnectionString);
                //else if (value.Equals("localserver"))
                //    return new PurityDBDataContext(DBSettings.Default.LocalDBConnectionString);
                //else 
                if (value.Equals("file"))                
                    return new PurityDBDataContext(DBSettings.Default.FileDBConnectionString);                
                else
                    throw new NotSupportedException("Unknown usedb value");
            }
        }
                
        public static void Initialize()
        {
#if PHX_DEBUG_SUPPORT
            PurityAnalysisPhase.StaticAnalysisPhaseCtrl = Phx.Controls.ComponentControl.New("PurityAnalysis","Perform Static Analysis", "PurityAnlaysisPhase.cs");            
#endif
        }
        
        public static PurityAnalysisPhase New(Phx.Phases.PhaseConfiguration config)
        {
            PurityAnalysisPhase staticAnalysisPhase =
                new PurityAnalysisPhase();

            staticAnalysisPhase.Initialize(config, "Purity Analysis");

#if PHX_DEBUG_SUPPORT
            staticAnalysisPhase.PhaseControl = 
                PurityAnalysisPhase.StaticAnalysisPhaseCtrl;
#endif

            return staticAnalysisPhase;
        }

        public static void InitializeProperties()
        {
            //var assemblyname = moduleunit.Manifest.Name.NameString;

            //Read properties and perform initialization
            string value;
            if (properties.TryGetValue("analyzelist", out value))
                AnalyzableMethods.Initialize(value);

            if (properties.TryGetValue("statistics", out value))            
                EnableStats = Boolean.Parse(value);
                        

            if (properties.TryGetValue("logging", out value))
                EnableLogging = Boolean.Parse(value);            

            if (properties.TryGetValue("dumpprogresstoconsole", out value))
                EnableConsoleLogging = Boolean.Parse(value);            

            //intialize some properties
            string cleardb;
            if (PurityAnalysisPhase.properties.TryGetValue("cleardbcontents", out cleardb))
                clearDBContents = Boolean.Parse(cleardb);            

            //initialize debug flags
            if (properties.TryGetValue("trackunanalyzablecalls", out value))
                trackUnanalyzableCalls = Boolean.Parse(value);            

            if (properties.TryGetValue("traceskippedcallresolution", out value))
                TraceSkippedCallResolution = Boolean.Parse(value);            

            if (properties.TryGetValue("tracesummaryapplication", out value))
                TraceSummaryApplication = Boolean.Parse(value);

            if (properties.TryGetValue("dumpir", out value))
                DumpIR = Boolean.Parse(value);

            if (properties.TryGetValue("dumpsummariesasgraphs", out value))
                DumpSummariesAsGraphs = Boolean.Parse(value);

            //initialize analysis flags
            if (properties.TryGetValue("flowsensitivity", out value))
                FlowSensitivity = Boolean.Parse(value);            

            if (properties.TryGetValue("boundcontextstring", out value))
                BoundContextStr = Boolean.Parse(value);

            if (BoundContextStr)
            {
                properties.TryGetValue("contextbound", out value);
                ContextStrBound = Int32.Parse(value);
            }

            if (properties.TryGetValue("disableexternalcallresolution", out value))
                DisableExternalCallResolution = Boolean.Parse(value);

            if (properties.TryGetValue("disablesummaryreduce", out value))
                DisableSummaryReduce = Boolean.Parse(value);

            if (properties.TryGetValue("trackstringconstants", out value))
                TrackStringConstants = Boolean.Parse(value);

            if (properties.TryGetValue("trackprimitivetypes", out value))
                TrackPrimitiveTypes = Boolean.Parse(value);
        }

        private void InitializeInstanceProperties(Phx.PEModuleUnit moduleunit)
        {
            var assemblyname = moduleunit.Manifest.Name.NameString;
            var dirname = PurityAnalysisPhase.outputdir;
            
            //Read properties and perform initialization
            string value;
            if (EnableStats)
            {
                if (properties.TryGetValue("statsfilenamesuffix", out value))
                    statsFileWriter = new StreamWriter(new FileStream(dirname + assemblyname + value,
                        FileMode.Create, FileAccess.Write, FileShare.Read));
                else throw new ArgumentException("Cannot find the statsfilenamesuffix property");
            }

            if (properties.TryGetValue("chacgfilenamesuffix", out value))
            {
                chaCallGraphWriter = XmlWriter.Create(new FileStream(dirname + assemblyname + value, 
                    FileMode.Create, FileAccess.Write, FileShare.Read));
            }

            if (properties.TryGetValue("finalcgfilenamesuffix", out value))
            {
                finalCallGraphWriter = XmlWriter.Create(new FileStream(dirname + assemblyname + value, 
                    FileMode.Create, FileAccess.Write, FileShare.Read));
            }
            
            if (EnableLogging)
            {
                if (properties.TryGetValue("logfilenamesuffix", out value))
                    Trace.Listeners.Add(new TextWriterTraceListener(dirname + assemblyname + value));                
                else throw new ArgumentException("Cannot find the logfilenamesuffix property");
            }            
        }
       
        protected override void Execute(Phx.Unit unit)
        {
            if (!unit.IsPEModuleUnit)
                return;

            Phx.PEModuleUnit moduleUnit = unit.AsPEModuleUnit;
            var assemblyname = moduleUnit.Manifest.Name.NameString;

            InitializeInstanceProperties(moduleUnit);

            //start a stopwatch
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();

            //construct call graph
            Phx.Phases.PhaseConfiguration callgraphConfig =
                Phx.Phases.PhaseConfiguration.New(moduleUnit.Lifetime, "Callgraph construction phase");

            callgraphConfig.PhaseList.AppendPhase(CHACallGraphConstructionPhase.New(callgraphConfig));
            callgraphConfig.PhaseList.DoPhaseList(moduleUnit);

            if (PurityAnalysisPhase.EnableConsoleLogging)
                Console.WriteLine("Number of call-graph nodes: " + moduleUnit.CallGraph.NodeCount);

            //dump the initial call graph            
            if (chaCallGraphWriter != null)
                GraphUtil.SerializeCallGraphAsDGML(moduleUnit.CallGraph, chaCallGraphWriter, null);

            //compute summaries (this will also construct call graph on the fly)
            AnalyzeBottomUp(moduleUnit);

            //dump the final call graph
            if (finalCallGraphWriter != null)
            {
                GraphUtil.SerializeCallGraphAsDGML(moduleUnit.CallGraph, finalCallGraphWriter,
                    (Phx.Graphs.CallNode n) =>
                    {
                        string summarysize = "";
                        var summary = n.CallGraph.SummaryManager.RetrieveSummary(
                            n, PurityAnalysisSummary.Type) as PurityAnalysisSummary;
                        if (summary != null && summary.PurityData != null)
                        {
                            summarysize += "[" + summary.PurityData.OutHeapGraph.EdgeCount
                               + "," + summary.PurityData.OutHeapGraph.VertexCount
                               + "," + summary.PurityData.SkippedCalls.Count() + "]";
                        }
                        return summarysize;
                    });
            }
            sw.Stop();

            //interaction            
            //Interact(moduleUnit);
            string value;
            //if (properties.TryGetValue("outputfilenamesuffix", out value))
            //    outputFileWriter = new StreamWriter(new FileStream(
            //        PurityAnalysisPhase.outputdir + assemblyname + value, FileMode.Create, FileAccess.Write, FileShare.Read));
            //else
            //    throw new ArgumentException("No output filename specified");

            //Phx.Phases.PhaseConfiguration reportConfig =
            //    Phx.Phases.PhaseConfiguration.New(moduleUnit.Lifetime, "Output generation phase");            
            //var reportphase = PurityReportGenerationPhase.New(reportConfig, this.outputFileWriter);
            //reportConfig.PhaseList.AppendPhase(reportphase);
            //reportConfig.PhaseList.DoPhaseList(moduleUnit);

            //dump analysis time and peak memory used
            Console.WriteLine("Analysis time: " + sw.Elapsed);
            Console.WriteLine("# Memory {0} KB",
                (System.Diagnostics.Process.GetCurrentProcess().PeakVirtualMemorySize64 / 1000));

            if (properties.TryGetValue("unacallsfilenamesuffix", out value))
            {
                this.unaCallsWriter = new StreamWriter(new FileStream(PurityAnalysisPhase.outputdir + assemblyname + value,
                    FileMode.Create, FileAccess.Write, FileShare.Read));
                StatisticsManager.GetInstance().DumpUnanalyzableCalls(unaCallsWriter);
                this.unaCallsWriter.Close();
            }                        
            
            if (PurityAnalysisPhase.properties.TryGetValue("summaryserialization", out value)
                && Boolean.Parse(value))
            {
                Console.WriteLine("serializing ...");
                PurityDBDataContext dbContext = PurityAnalysisPhase.DataContext;
                CombinedTypeHierarchy.GetInstance(moduleUnit).SerializeInternalTypes(moduleUnit, dbContext);
                try
                {
                    dbContext.SubmitChanges();
                }
                catch (System.Data.SqlClient.SqlException sqlE)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Exception occurred on commiting changes: " + sqlE.Message);
                    Console.ResetColor();
                }
                finally
                {
                    dbContext.Dispose();
                }
            }

            if (EnableStats)
            {                
                this.statsFileWriter.Close();
            }
        }

        private void AnalyzeBottomUp(Phx.PEModuleUnit moduleUnit)
        {           
            Phx.Phases.PhaseConfiguration initConfig =
                Phx.Phases.PhaseConfiguration.New(moduleUnit.Lifetime, "Intialization Phases");
            
            initConfig.PhaseList.AppendPhase(InitializationPhase.New(initConfig));            
            initConfig.PhaseList.DoPhaseList(moduleUnit);
            moduleUnit.Context.PopUnit();            

            int methodsToAnalyse = 0, methodsSkipped = 0;            
            IWorklist<Phx.Graphs.CallNode> worklist = new MRWorklist<Phx.Graphs.CallNode>();           

            //analyse the methods in the post-order (add all the methods to the worklist)
            Phx.Graphs.NodeFlowOrder nodeOrder = Phx.Graphs.NodeFlowOrder.New(moduleUnit.Lifetime);                
            nodeOrder.Build(moduleUnit.CallGraph, Phx.Graphs.Order.ReversePostOrder);

            var toRemoveEdges = new HashSet<Phx.Graphs.CallEdge>();
            for (uint i = 1; i <= nodeOrder.NodeCount; i++)
            {
                Phx.Graphs.CallNode node = nodeOrder.Node(i).AsCallNode;
                if (!AnalyzableMethods.IsAnalyzable(node.FunctionSymbol))
                {
                    methodsSkipped++;
                    if (PurityAnalysisPhase.EnableConsoleLogging)
                    {
                        if (node.FunctionSymbol.UninstantiatedFunctionSymbol != null)
                            Console.WriteLine("skipping an instantiation");
                        else if (!PhxUtil.DoesBelongToCurrentAssembly(node.FunctionSymbol, moduleUnit))
                            Console.WriteLine("skipping an external method");
                        else if (node.FunctionSymbol.FunctionUnit == null)
                            Console.WriteLine("skipping an undefined method: " + node.FunctionSymbol.QualifiedName);
                        else
                            Console.WriteLine("skipping not to analyze method: " + node.FunctionSymbol.QualifiedName);
                    }
                }
                else
                {
                    worklist.Enqueue(node);
                    methodsToAnalyse++;
                    foreach (var edge in node.SuccessorEdges)
                        toRemoveEdges.Add(edge);
                }
            }

            //remove all edges from the call-graph
            foreach (var edge in toRemoveEdges)
                moduleUnit.CallGraph.RemoveEdge(edge);
            
            while (worklist.Any())
            {
                var node = worklist.Dequeue();

                if (PurityAnalysisPhase.EnableConsoleLogging)
                    Console.WriteLine("Processing Node: " + node.Id + " Qualified Name: " + node.FunctionSymbol.QualifiedName);

                if (iterationCounts.ContainsKey(node))
                    iterationCounts[node]++;
                else
                    iterationCounts.Add(node, 1);                

                Phx.FunctionUnit functionUnit = node.FunctionSymbol.FunctionUnit;
                if (!functionUnit.IRState.Equals(Phx.FunctionUnit.HighLevelIRFunctionUnitState))
                {
                    moduleUnit.Raise(node.FunctionSymbol, Phx.FunctionUnit.HighLevelIRFunctionUnitState);
                    functionUnit.Context.PopUnit();
                }

                //check the  instruction ids
                foreach (var inst in functionUnit.Instructions)
                {
                    if (inst.InstructionId == 0)
                        throw new SystemException("Instruction Ids not initialized");
                }

                var oldData = (moduleUnit.CallGraph.SummaryManager.RetrieveSummary(
                    node,PurityAnalysisSummary.Type) as PurityAnalysisSummary).PurityData.Copy();

                var newData = (new MethodLevelAnalysis(functionUnit)).Execute();

                //attach the summary                          
                AttachSummary(functionUnit, newData);

                if (EnableStats)
                {
                    (StatisticsManager.GetInstance()).DumpSummaryStats(node, newData, statsFileWriter);
                }

                //check if the summary has changed
                if ((oldData != null && newData != null && !oldData.Equivalent(newData))
                            || (oldData == null && oldData != newData))
                {
                    //add all the predecessors of this node belonging the scc to the worklist.
                    foreach (var predEdges in node.PredecessorEdges)
                    {
                        var predNode = predEdges.CallerCallNode;
                        if (!worklist.Contains(predNode))
                        {
                            worklist.Enqueue(predNode);
                        }
                    }
                }
                //update progress (progress need not be monotonically increasing) 
                var progress = Math.Round(((methodsToAnalyse - worklist.Count()) * 100.0 / methodsToAnalyse),1);

                if (PurityAnalysisPhase.EnableConsoleLogging)
                    Console.WriteLine("Progress: {0}%", progress);
            }

            //wrap up 
            NodeEquivalenceRelation.Reset();            
            //moduleUnit.Context.PopUnit();            
        }

        /// <summary>
        /// Interacts with the user.
        /// </summary>
        /// <param name="moduleUnit"></param>
        private void Interact(Phx.PEModuleUnit moduleUnit)
        {
            while (true)
            {
                Console.WriteLine("Enter Function Symbol name (type \"exit\" to quit): ");
                string input = Console.ReadLine();
                if (input.Equals("exit"))
                    break;
                Phx.Graphs.NodeFlowOrder nodeOrder = Phx.Graphs.NodeFlowOrder.New(moduleUnit.Lifetime);
                nodeOrder.Build(moduleUnit.CallGraph, Phx.Graphs.Order.PostOrder);
                for (uint i = 1; i <= nodeOrder.NodeCount; i++)
                {
                    Phx.Graphs.CallNode node = nodeOrder.Node(i).AsCallNode;
                    if (node.FunctionSymbol.QualifiedName.Equals(input))
                    {
                        PurityAnalysisSummary summary = (PurityAnalysisSummary)
                            moduleUnit.CallGraph.SummaryManager.RetrieveSummary(node, PurityAnalysisSummary.Type);
                        if (summary != null)
                        {                            
                            var isConstructor = PhxUtil.IsConstructor(node.FunctionSymbol.NameString);
                            var report = PurityReportUtil.GetPurityReport(
                                node.FunctionSymbol.QualifiedName, 
                                PhxUtil.GetFunctionTypeSignature(node.FunctionSymbol.FunctionType),
                                summary.PurityData, isConstructor);

                            report.Dump();
                            summary.PurityData.OutHeapGraph.Dump();

                            //report.Print(this.outputFileWriter);
                            //display the heap graph as well
                            //this.outputFileWriter.WriteLine("Summary: ");
                            //this.outputFileWriter.WriteLine(summary.PurityData.OutHeapGraph.ToString());
                            //this.outputFileWriter.Flush();
                            //summary.PurityData.OutHeapGraph.DumpAsDGML("hg.dgml");
                        }
                    }
                }                
            }
        }

        public static void AttachSummary(Phx.FunctionUnit functionUnit, PurityAnalysisData data)
        {
            //Contract.Requires(functionUnit.FunctionSymbol.CallNode != null);
            //Contract.Requires(functionUnit.ParentPEModuleUnit.CallGraph != null);
            //Contract.Requires(data is PurityAnalysisData);

            Phx.Graphs.CallGraph callGraph = functionUnit.ParentPEModuleUnit.CallGraph;

            PurityAnalysisSummary puritySummary =
                PurityAnalysisSummary.New(callGraph.SummaryManager, data);

            //remove all summaries attached to this node and add the new summary
            callGraph.SummaryManager.RemoveAllSummary(functionUnit.FunctionSymbol.CallNode);
            callGraph.SummaryManager.PurgeSummary(functionUnit.FunctionSymbol.CallNode);
            bool summaryAdded = 
                callGraph.SummaryManager.AddSummary(functionUnit.FunctionSymbol.CallNode, puritySummary);
            
            //Contract.Assert(summaryAdded);
        }
        
    }
}
