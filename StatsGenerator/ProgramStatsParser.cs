using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

using QuickGraph;
using QuickGraph.Serialization;
using QuickGraph.Serialization.DirectedGraphML;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.ConnectedComponents;

namespace SafetyAnalysis.Checker
{
    /// <summary>
    /// This reads and parses the stat file that is emitted by checker.exe.
    /// </summary>
    class ProgramStatsParser
    {
        class SummaryEntry
        {
            public string methodname;
            //public string signature;
            public bool ispure;
            public int nodes;
            public int edges;
            public long timetaken;
            public int load_nodes;
            public int param_nodes;
            public int internal_nodes;
            public int int_edges;
            public int ext_edges;
            public int static_reachables;            
            public int thrown_reachables;
            public int writeset_size;
            public int edges_part;
            public int int_edges_part;
            public int ext_edges_part;
            public int method_calls;
            public int callee_summaries;
            public int known_targets;
            public long dbtime;
            public int dbcacheHits;
            public int dbcacheLookups;
            public int itercount;
            public int maxcontext;
            public int capturedlocalobjects;
            public int capturedextobjects;
            public int capturedskcalls;
            public int skcalls;

        }

        class CumulativeStats
        {            
            public long puremethods = 0;
            public long impuremethods = 0;
            public long incompImpuremethods = 0;
            public long condPuremethods = 0;
            public long total_methods = 0;
            public long dbtime = 0;
            public long time = 0;
            public long dbcachelookups = 0;
            public long dbcachehits = 0;
            public BidirectionalGraph<DirectedGraphNode, Edge<DirectedGraphNode>> chagraph;
            public BidirectionalGraph<DirectedGraphNode, Edge<DirectedGraphNode>> finalgraph;
            public int chasccs = 0;
            public int finalsccs = 0;
            public double chaSccSize = 0;
            public double finalSccSize = 0;
            public int chaSccMax = 0;
            public int finalSccMax = 0;
            public double skcallsMean;
            public double skcallsSD;
            public double localMean;
            public double localSD;
            public double localSkMean;
            public double localSkSD;
            public double capMean;
            public double capSD;

            public void Dump(StreamWriter sw)
            {                
                sw.WriteLine("methods: " + total_methods);
                sw.WriteLine("Pure: " + puremethods);
                sw.WriteLine("ConditionallyPure: " + condPuremethods);
                sw.WriteLine("Impure: " + impuremethods);
                sw.WriteLine("ImcompImpure: " + incompImpuremethods);
                sw.WriteLine("Time: " + GetTimeString(time));
                sw.WriteLine("Db+Deserialization: " + GetTimeString(dbtime));                
                sw.WriteLine("DBCacheHit%: " + ((double)dbcachehits / dbcachelookups) * 100);
                //sw.WriteLine("CHAcg: [{0},{1}]", chagraph.VertexCount, chagraph.EdgeCount);
                //sw.WriteLine("Compressedcg: [{0},{1}]", finalgraph.VertexCount, finalgraph.EdgeCount);
                sw.WriteLine("CHAcg: "+ chagraph.EdgeCount);
                sw.WriteLine("CHAsccs: " + chasccs);
                sw.WriteLine("CHASccAvgSize: " + chaSccSize);
                sw.WriteLine("CHASccMaxSize: " + chaSccMax);
                sw.WriteLine("Compressedcg: " + finalgraph.EdgeCount);                
                sw.WriteLine("CompressedSccs: " + finalsccs);                
                sw.WriteLine("CompressedSccAvgSize: " + finalSccSize);                
                sw.WriteLine("CompressedSccMaxSize: " + finalSccMax);
                sw.WriteLine("SkippedCallsMean: " + skcallsMean);
                sw.WriteLine("SkippedCallsSD: " + skcallsSD);
                sw.WriteLine("MeanCapturedNodes(Rel): " + capMean);
                sw.WriteLine("SDCapturedNodes(Rel): " + capSD);
                sw.WriteLine("MeanLocalNodes(Rel): " + localMean);
                sw.WriteLine("SDLocalNodes(Rel): " + localSD);
                sw.WriteLine("MeanCapturedCalls(Rel): " + localSkMean);
                sw.WriteLine("SDCapturedCalls(Rel): " + localSkSD);                
            }
        }

        List<SummaryEntry> sumentries = new List<SummaryEntry>();
        Dictionary<string, SummaryEntry> MethodSummaryMap = new Dictionary<string, SummaryEntry>();
        CumulativeStats cumulativeStats = new CumulativeStats();
        public string outdirname;
        public string indirname;
        public string assemblyname;

        public static string GetTimeString(long time)
        {
            var ts = TimeSpan.FromMilliseconds(time);
            string str = "";
            if (ts.Hours > 0)
                str += ts.Hours + "h";
            if (ts.Minutes > 0)
                str += ts.Minutes + "m";
            if (ts.Seconds > 0)
                str += ts.Seconds+"s";
            return str;
        }

        public static void Main(string[] args)
        {
            if (args.Length < 3)
                throw new SystemException("Stat file/output file not specified");                        
            ProgramStatsParser parser = new ProgramStatsParser();
            parser.outdirname = args[0] + "\\";
            parser.assemblyname = args[1];            
            parser.indirname = args[2];
            Console.WriteLine("OutDirname: " + parser.outdirname);
            Console.WriteLine("Infilename: " + parser.indirname);
            
            //open stats file
            var statsfilename = parser.indirname + "\\" + parser.assemblyname + "-checker-stats.txt";
            FileStream fs = null;
            if (File.Exists(statsfilename))
                fs = new FileStream(statsfilename, FileMode.Open, FileAccess.Read, FileShare.Read);
            else
                throw new ArgumentException(statsfilename + " does not exist");

            StreamReader sr = new StreamReader(fs);            
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Equals("BeginSummaryStats"))
                {
                    //this will read till an end summary stats tag is encountered.
                    parser.ParseSummaries(sr);
                }
                if (line.Equals("BeginCumulativeStats"))
                {
                    //this will read till an end sumulative stats tag is encountered
                    parser.ParseCumulativeStats(sr);
                }
            }
            sr.Close();
            fs.Close();

            //parse call-graphs
            parser.cumulativeStats.chagraph = parser.ParseGraph("-cha-call-graph.dgml");
            parser.cumulativeStats.finalgraph = parser.ParseGraph("-final-call-graph.dgml");        
                        
            //prin stats
            parser.DumpStats();
            
            //print cumulative stats
            fs = new FileStream(parser.outdirname + parser.assemblyname + "-cumulative.txt", FileMode.Create,
                                                    FileAccess.Write, FileShare.Read);
            StreamWriter sw = new StreamWriter(fs);
            parser.cumulativeStats.Dump(sw);
            sw.Flush();
            sw.Close();
        }

        private void ParseCumulativeStats(StreamReader sr)
        {            
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Equals("EndCumulativeStats"))
                    break;

                string[] fields = line.Split(new string[] { ": " }, StringSplitOptions.None);
                if (fields.Length < 2)
                    throw new NotSupportedException("Field length < 2: " + line);

                string key = fields[0].Trim();
                string value = fields[1].Trim();

                if (key.Equals("methods"))
                    cumulativeStats.total_methods = int.Parse(value);
                else if (key.Equals("Pure methods"))
                    cumulativeStats.puremethods = int.Parse(value);
                else if (key.Equals("Conditionally pure methods"))
                    cumulativeStats.condPuremethods = int.Parse(value);
                else if (key.Equals("Impure methods"))
                    cumulativeStats.impuremethods = int.Parse(value);
                else if (key.Equals("Impure incomplete methods"))
                    cumulativeStats.incompImpuremethods = int.Parse(value);
                else
                    throw new NotSupportedException("unknown key: " + key);
            }           
        }

        private BidirectionalGraph<DirectedGraphNode, Edge<DirectedGraphNode>> ParseGraph(string suffix)
        {           
            var cgfilename = this.indirname + "\\" + this.assemblyname + suffix;

            if (File.Exists(cgfilename))
            {
                XmlReader xr = XmlReader.Create(new FileStream(cgfilename, FileMode.Open, FileAccess.Read, FileShare.Read));
                var dg = (DirectedGraph)DirectedGraphMLExtensions.DirectedGraphSerializer.Deserialize(xr);                                
                var g = new BidirectionalGraph<DirectedGraphNode, Edge<DirectedGraphNode>>();               
                var dic = new Dictionary<string, DirectedGraphNode>();
                foreach(var dnode in dg.Nodes)
                {
                    g.AddVertex(dnode);
                    dic.Add(dnode.Id,dnode);
                }
                foreach(var link in dg.Links)
                {
                    g.AddEdge(new Edge<DirectedGraphNode>(dic[link.Source],dic[link.Target]));
                }
                return g;
            }
            else
                throw new ArgumentException(cgfilename + " does not exist");            
        }

        private void ParseSummaries(StreamReader sr)
        {
            SummaryEntry se = new SummaryEntry();
            sumentries.Add(se);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if(line.Equals("EndSummaryStats"))
                    break;

                string[] fields = line.Split(new string[] { ": " }, StringSplitOptions.None);
                if(fields.Length < 2)
                    throw new NotSupportedException("Field length < 2: "+line);

                string key = fields[0].Trim();
                string value = fields[1].Trim();

                if (key.Equals("Method"))
                    se.methodname = value;
                //else if (key.Equals("Signature"))
                //    se.signature = value;
                else if (key.Equals("IsPure"))
                    se.ispure = Boolean.Parse(value);
                else if (key.Equals("Time taken"))
                    se.timetaken = long.Parse(value);
                else if (key.Equals("# of vertices"))
                    se.nodes = int.Parse(value);
                else if (key.Equals("# of edges"))
                    se.edges = int.Parse(value);
                else if (key.Equals("# of load nodes"))
                    se.load_nodes = int.Parse(value);
                else if (key.Equals("# of param nodes"))
                    se.param_nodes = int.Parse(value);
                else if (key.Equals("# of internal nodes"))
                    se.internal_nodes = int.Parse(value);
                else if (key.Equals("# of internal edges"))
                    se.int_edges = int.Parse(value);
                else if (key.Equals("# of external edges"))
                    se.ext_edges = int.Parse(value);
                else if (key.Equals("# of static reachables"))
                    se.static_reachables = int.Parse(value);
                else if (key.Equals("# of thrown reachables"))
                    se.thrown_reachables = int.Parse(value);
                else if (key.Equals("MayWrite set size"))
                    se.writeset_size = int.Parse(value);
                else if (key.Equals("Single field edges partition"))
                    se.edges_part = int.Parse(value);
                else if (key.Equals("Single field internal edges partition"))
                    se.int_edges_part = int.Parse(value);
                else if (key.Equals("Single field external edges partition"))
                    se.ext_edges_part = int.Parse(value);
                else if (key.Equals("# of method calls"))
                    se.method_calls = int.Parse(value);
                else if (key.Equals("Max context length"))
                    se.maxcontext = int.Parse(value);
                else if (key.Equals("# of callee summaries"))
                    se.callee_summaries = int.Parse(value);
                else if (key.Equals("# of known target calls"))
                    se.known_targets = int.Parse(value);
                else if (key.Equals("DBaccess+deserialization time"))
                    se.dbtime = long.Parse(value);
                else if (key.Equals("DBCache lookups"))
                    se.dbcacheLookups = int.Parse(value);
                else if (key.Equals("DBCache hits"))
                    se.dbcacheHits = int.Parse(value);
                else if (key.Equals("Iteration Count"))
                    se.itercount = int.Parse(value);
                else if (key.Equals("Captured local objects"))
                    se.capturedlocalobjects = int.Parse(value);
                else if (key.Equals("Captured Skipped calls"))
                    se.capturedskcalls = int.Parse(value);
                else if (key.Equals("Captured external objects"))
                    se.capturedextobjects = int.Parse(value);
                else if (key.Equals("# of sk calls"))
                    se.skcalls = int.Parse(value);
                else
                    throw new NotSupportedException("unknown key: " + key);
            }
            string mname = se.methodname;
            //if (mname.Contains(".ctor"))
            //    mname = mname.Replace(".ctor", "ctor");
            //if (mname.Contains(".cctor"))
            //    mname = mname.Replace(".cctor", "cctor");
            //var qualname = mname + "/" + se.signature;            
            if (!MethodSummaryMap.ContainsKey(mname))
                MethodSummaryMap.Add(mname, se);
            else
                MethodSummaryMap[mname] = se;
        }

        //private static string ConstructName(string symname)
        //{
        //    //get the qualified name and remove the assembly name prefix from that            
        //    if (symname.Length > 0)
        //    {
        //        int firstchar = symname[0];
        //        int eindex = symname.IndexOf(']');
        //        if (firstchar == '[' && eindex != -1)
        //            symname = symname.Substring(eindex + 1);                
        //    }
        //    return symname;
        //}

        private void DumpStats()
        {         
            //open files and streams
            //var nameWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-methodnames.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var indexWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-nameindex.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var dupWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-dup-fields.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var nodeWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-nodecount.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var edgeWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-edgecount.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var staticReachWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-staticreach.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var calleesumWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-calleesum.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var timeWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-time.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var maxcontextWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-maxcontext.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var iterCount = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-itercount.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var skcallsWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-skcalls.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var capturedSkcallsWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-capturedskcalls.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var capturedLocalobjWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-capturedlocalobj.txt",
            //    FileMode.Create, FileAccess.Write, FileShare.Read));
            //var internalNodesWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + "-internalnodes.txt",
            //     FileMode.Create, FileAccess.Write, FileShare.Read));
            //int index = 1;                 
            foreach (var entry in sumentries)
            {                        
                cumulativeStats.dbtime += entry.dbtime;
                cumulativeStats.time += entry.timetaken;
                cumulativeStats.dbcachehits += entry.dbcacheHits;
                cumulativeStats.dbcachelookups += entry.dbcacheLookups;                

                //nameWr.WriteLine(entry.methodname + "/" + entry.signature);
                //indexWr.WriteLine(index++ + "\t" + entry.methodname + "/" + entry.signature);

                // double duplicate_fields = 0;
                //if (entry.edges_part != 0)                
                //    duplicate_fields = ((double)entry.edges / entry.edges_part);
                //dupWr.WriteLine(duplicate_fields);
                
                //nodeWr.WriteLine(entry.nodes);
                //edgeWr.WriteLine(entry.edges);

                //double reachPercent = ((double)entry.static_reachables * 100.0 / entry.nodes);
                //staticReachWr.WriteLine(reachPercent);

                //calleesumWr.WriteLine(entry.callee_summaries);
                //timeWr.WriteLine(entry.timetaken);
                //maxcontextWr.WriteLine(entry.maxcontext);
                //iterCount.WriteLine(entry.itercount);
                //skcallsWr.WriteLine(entry.skcalls);
                //capturedSkcallsWr.WriteLine(entry.capturedskcalls);
                //capturedLocalobjWr.WriteLine(entry.capturedlocalobjects);
                //internalNodesWr.WriteLine(entry.internal_nodes);
            }
            //compute skipped calls stats
            double[] skcallsCount = new double[MethodSummaryMap.Count];
            int i = 0;
            foreach (var entry in MethodSummaryMap.Values)
            {
                skcallsCount[i++] = entry.skcalls;
            }
            double skmean, sksd;
            MeanStdDev(skcallsCount, out skmean, out sksd);
            cumulativeStats.skcallsMean = skmean;
            cumulativeStats.skcallsSD = sksd;

            //nameWr.Flush();
            //indexWr.Flush();
            //dupWr.Flush();
            //nodeWr.Flush();
            //edgeWr.Flush();
            //staticReachWr.Flush();
            //calleesumWr.Flush();
            //timeWr.Flush();
            //maxcontextWr.Flush();
            //iterCount.Flush();
            //skcallsWr.Flush();
            //capturedSkcallsWr.Flush();
            //capturedLocalobjWr.Flush();
            //internalNodesWr.Flush();

            ComputeCGStats();

            ComputeBUStats();
                                            
       }

        private void ComputeCGStats()
        {
            var chaSccAlgo = new StronglyConnectedComponentsAlgorithm<DirectedGraphNode, Edge<DirectedGraphNode>>(cumulativeStats.chagraph);
            chaSccAlgo.Compute();

            var finalSccAlgo = new StronglyConnectedComponentsAlgorithm<DirectedGraphNode, Edge<DirectedGraphNode>>(cumulativeStats.finalgraph);
            finalSccAlgo.Compute();

            //compute graph stats            
            int count;
            double avgsize;
            int maxsize;
            ComputeSccStats(chaSccAlgo, "-chasccs.txt", out count,out avgsize, out maxsize);
            cumulativeStats.chasccs = count;
            cumulativeStats.chaSccSize = avgsize;
            cumulativeStats.chaSccMax = maxsize;
            
            ComputeSccStats(finalSccAlgo, "-compressedsccs.txt",out count, out avgsize, out maxsize);
            cumulativeStats.finalsccs = count;
            cumulativeStats.finalSccSize = avgsize;
            cumulativeStats.finalSccMax = maxsize;
        }

        private void ComputeSccStats(StronglyConnectedComponentsAlgorithm<DirectedGraphNode, Edge<DirectedGraphNode>> algo, 
            string suffix, 
            out int count, out double avgsize, out int maxsize) 
        {
            count = 0;            
            maxsize = 0;
            avgsize = 0;
            int totalsize = 0;

            //var sccWr = new StreamWriter(new FileStream(this.outdirname + this.assemblyname + suffix,
            //     FileMode.Create, FileAccess.Write, FileShare.Read));
            Dictionary<int, int> componentSizeMap = new Dictionary<int, int>(algo.ComponentCount);            
            foreach (var component in algo.Components)
            {
                if (componentSizeMap.ContainsKey(component.Value))               
                    componentSizeMap[component.Value]++;                
                else
                    componentSizeMap.Add(component.Value, 1);                
            }
            foreach (var pair in componentSizeMap)
            {
                if (pair.Value > 1)
                {
                    //sccWr.WriteLine(pair.Value);
                    count++;
                    totalsize += pair.Value;
                    if (pair.Value > maxsize)
                        maxsize = pair.Value;
                }
            }
            avgsize = ((double)totalsize / count);
            //sccWr.Flush();
            //sccWr.Close();                
        }

        private void ComputeBUStats()
        {
            int[] capturedIntNodes = new int[MethodSummaryMap.Count];
            int[] capturedExtNodes = new int[MethodSummaryMap.Count];
            int[] capturedCalls = new int[MethodSummaryMap.Count];
            int[] intNodes = new int[MethodSummaryMap.Count];
            int[] SkCalls = new int[MethodSummaryMap.Count];
            double[] relIntNodes = new double[MethodSummaryMap.Count];
            double[] relNodes = new double[MethodSummaryMap.Count];
            double[] relCalls = new double[MethodSummaryMap.Count];
            int i=0;
            var graph = cumulativeStats.finalgraph;
            
            var algo = new QuickGraph.Algorithms.Search.DepthFirstSearchAlgorithm<DirectedGraphNode, Edge<DirectedGraphNode>>(
                graph);
            algo.FinishVertex += (DirectedGraphNode node) =>
                {                    
                    var desc = node.Description;
                    Console.WriteLine("Processing: " + desc);
                    if (!MethodSummaryMap.ContainsKey(desc))
                    {
                        Console.WriteLine("No sumentry for: " + desc);
                        return;
                    }
                    var sumentry = MethodSummaryMap[desc];
                    capturedIntNodes[i] = sumentry.capturedlocalobjects;
                    capturedCalls[i] = sumentry.capturedskcalls;                    

                    foreach (var edge in graph.OutEdges(node))
                    {
                        if (edge.Target == node)
                            continue;

                        var tname = edge.Target.Description;
                        if (!MethodSummaryMap.ContainsKey(tname))
                        {
                            continue;
                        }
                        var targetEntry = MethodSummaryMap[tname];
                        capturedIntNodes[i] += targetEntry.capturedlocalobjects;
                        capturedExtNodes[i] += targetEntry.capturedextobjects;
                        capturedCalls[i] += targetEntry.capturedskcalls;
                    }
                    var totalIntnodes = sumentry.internal_nodes + capturedIntNodes[i];
                    var totalSkcalls = sumentry.skcalls + capturedCalls[i];
                    var totalNodes = sumentry.nodes + capturedExtNodes[i] + capturedIntNodes[i];

                    if (totalNodes > 0)
                        relNodes[i] = ((double)(capturedExtNodes[i] + capturedIntNodes[i])) / totalNodes;

                    if (totalIntnodes > 0)
                        relIntNodes[i] = ((double)(capturedIntNodes[i])) / totalIntnodes;

                    if (totalSkcalls > 0)
                        relCalls[i] = ((double)(capturedCalls[i])) / totalSkcalls;                    
                    i++;                    
                };            
            algo.Compute();

            MeanStdDev(relNodes, out cumulativeStats.capMean, out cumulativeStats.capSD);
            
            MeanStdDev(relIntNodes, out cumulativeStats.localMean, out cumulativeStats.localSD);             

            MeanStdDev(relCalls, out cumulativeStats.localSkMean, out cumulativeStats.localSkSD);            
        }

        /// <summary>
        /// Instead of stddev using absolute deviation
        /// </summary>
        /// <param name="array"></param>
        /// <param name="mean"></param>
        /// <param name="stddev"></param>
        private void MeanStdDev(double[] array, out double mean, out double stddev)
        {
            mean = array.Average();
            var vararray = new double[array.Length];            
            for(int i=0; i<array.Length; i++)
            {
                var diff = (array[i] - mean);
                if (diff < 0)
                    diff = -diff;
                //vararray[i] = diff * diff;
                vararray[i] = diff;
            }
           //stddev = Math.Sqrt(vararray.Average());
            stddev = vararray.Average();
        }
    }
}
