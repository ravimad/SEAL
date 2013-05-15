using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Xml;

using Phx.Graphs;
using QuickGraph;
using QuickGraph.Serialization;
using QuickGraph.Serialization.DirectedGraphML;

namespace SafetyAnalysis.Util
{
    public class SCCEnumerator
    {
        private Phx.Graphs.CallGraph cg;
        private int index;
        private Stack<Phx.Graphs.CallNode> sccStack;        
        private HashSet<Phx.Graphs.CallNode> visited;
        private Dictionary<Phx.Graphs.CallNode, int[]> indexMap;
        private Dictionary<HashSet<Phx.Graphs.CallNode>, Phx.Graphs.CallNode> sccs;
        private StreamWriter sccWriter;

        public SCCEnumerator(CallGraph callGraph, StreamWriter writer)
        {
            cg = callGraph;
            sccWriter = writer; 
            index = 0;
            sccStack = new Stack<CallNode>();            
            visited = new HashSet<CallNode>();
            indexMap = new Dictionary<CallNode, int[]>();
            sccs = new Dictionary<HashSet<CallNode>, CallNode>(HashSet<CallNode>.CreateSetComparer());
        }

        //Given a callgraph computes the SCCs in the call graph
        //This will return a set of SCCs along with their roots. A root of an SCC is the first node that is visited
        //in the DFS order that resulted in the discovery of the SCC.
        public Dictionary<HashSet<Phx.Graphs.CallNode>, Phx.Graphs.CallNode> FindSCCs()
        {            
            Phx.Graphs.NodeFlowOrder nodeOrder = Phx.Graphs.NodeFlowOrder.New(cg.Lifetime);
            nodeOrder.Build(cg, Phx.Graphs.Order.PostOrder);
            for (uint i = 1; i <= nodeOrder.NodeCount; i++)
            {
                Phx.Graphs.CallNode startNode = nodeOrder.Node(i).AsCallNode;
                if (!visited.Contains(startNode))
                    computeSCC(startNode);
            }

            //compare against the scc of the quick graph
            //CompareWithQuickGraphSCC();

            return sccs;
        }

        private void CompareWithQuickGraphSCC()
        {
            BidirectionalGraph<CallNode, Edge<CallNode>> g = new BidirectionalGraph<CallNode, Edge<CallNode>>();
            Phx.Graphs.NodeFlowOrder nodeOrder = Phx.Graphs.NodeFlowOrder.New(cg.Lifetime);
            nodeOrder.Build(cg, Phx.Graphs.Order.PostOrder);
            for (uint i = 1; i <= nodeOrder.NodeCount; i++)
            {
                Phx.Graphs.CallNode node = nodeOrder.Node(i).AsCallNode;
                g.AddVertex(node);
            }

            foreach (var node in g.Vertices)
            {
                foreach (Phx.Graphs.CallEdge e in node.SuccessorEdges)
                {
                    var edge = new Edge<CallNode>(e.CallerCallNode, e.CalleeCallNode);
                    if (!g.ContainsEdge(edge))
                        g.AddEdge(edge);
                }
            }

            Console.WriteLine("["+g.VertexCount+","+g.EdgeCount+"]");
            QuickGraph.Algorithms.ConnectedComponents.StronglyConnectedComponentsAlgorithm<CallNode, Edge<CallNode>> algo =
                new QuickGraph.Algorithms.ConnectedComponents.StronglyConnectedComponentsAlgorithm<CallNode, Edge<CallNode>>(g);
            algo.Compute();

            //map component ids to a set of nodes.
            Dictionary<int, HashSet<CallNode>> componentMap = new Dictionary<int, HashSet<CallNode>>();
            foreach(var node in g.Vertices)
            {
                int componentId;
                if (algo.Components.TryGetValue(node, out componentId))
                {
                    HashSet<CallNode> sameComponentNodes;
                    if (componentMap.TryGetValue(componentId, out sameComponentNodes))
                        sameComponentNodes.Add(node);
                    else
                    {
                        sameComponentNodes = new HashSet<CallNode>();
                        sameComponentNodes.Add(node);
                        componentMap.Add(componentId, sameComponentNodes);
                    }
                }
            }
            
            foreach (var scc in sccs)
            {
                var root = scc.Value;
                int componentId;
                if (algo.Components.TryGetValue(root, out componentId))
                {
                    HashSet<CallNode> sameComponentNodes;
                    if (componentMap.TryGetValue(componentId, out sameComponentNodes))
                    {
                        if (scc.Key.SetEquals(sameComponentNodes))
                        {
                            Console.WriteLine("My component: ");
                            foreach (var n in scc.Key)
                                Console.WriteLine("\t" + n.FunctionSymbol.QualifiedName);
                            Console.WriteLine("Quick graph component: ");
                            foreach (var n in sameComponentNodes)
                                Console.WriteLine("\t" + n.FunctionSymbol.QualifiedName);
                        }
                        else if (scc.Key.Count > 1)
                            Console.WriteLine("mathched component of size: " + scc.Key.Count);
                    }
                }
            }
        }

        //This implements the Tarjan's algorithm for computing the SCCs
        private void computeSCC(Phx.Graphs.CallNode cgNode)
        {
            int[] callerValue;
            visited.Add(cgNode);
            //int[0] stands for v.index and int[1] stands for v.lowlink
            indexMap.Add(cgNode, new int[] { this.index, this.index });
            this.index++;
            sccStack.Push(cgNode);

            //add successors in the call graph
            HashSet<Phx.Graphs.CallNode> calleeCallNodes = new HashSet<Phx.Graphs.CallNode>();
            foreach (Phx.Graphs.CallEdge e in cgNode.SuccessorEdges)
            {
                //Contract.Assert(e.CalleeCallNode.FunctionSymbol.UninstantiatedFunctionSymbol == null);
                calleeCallNodes.Add(e.CalleeCallNode);
            }

            foreach (Phx.Graphs.CallNode calleeCallNode in calleeCallNodes)
            {
                if (!visited.Contains(calleeCallNode))
                {
                    computeSCC(calleeCallNode);
                    //set the lowlink of the current node to the minimum of the callee's lowlink and current lowlink
                    int[] calleeValue;
                    if (indexMap.TryGetValue(calleeCallNode, out calleeValue)
                        && indexMap.TryGetValue(cgNode, out callerValue))
                    {
                        if (calleeValue[1] < callerValue[1])
                            callerValue[1] = calleeValue[1];
                    }
                    else
                        throw new SystemException("indexMap does not exist for both callee and caller");
                }
                else if (sccStack.Contains(calleeCallNode))
                {
                    //set the lowlink of the current node to the minimum of the callee's index and current lowlink
                    int[] calleeValue;
                    if (indexMap.TryGetValue(calleeCallNode, out calleeValue)
                        && indexMap.TryGetValue(cgNode, out callerValue))
                    {
                        if (calleeValue[0] < callerValue[1])
                            callerValue[1] = calleeValue[0];
                    }
                    else
                        throw new SystemException("indexMap does not exist for both callee and caller");
                }
            }
            if (indexMap.TryGetValue(cgNode, out callerValue))
            {
                if (callerValue[0] == callerValue[1])
                {
                    HashSet<Phx.Graphs.CallNode> scc = new HashSet<Phx.Graphs.CallNode>();
                    Phx.Graphs.CallNode node;
                    do
                    {
                        node = sccStack.Pop();
                        scc.Add(node);
                    } while (!node.Equals(cgNode));
                    sccs.Add(scc, cgNode);

                    if (scc.Count > 100)                    
                        dumpScc(scc);                                  
                }
            }
            else
                throw new SystemException("Index map does not exist for the caller");
        }

        public void dumpScc(HashSet<CallNode> scc)
        {
            //create a quick graph and dump it.
            BidirectionalGraph<CallNode,Edge<CallNode>> sccGraph = new BidirectionalGraph<CallNode, Edge<CallNode>>();            
                        
            foreach (var sccnode in scc)
                sccGraph.AddVertex(sccnode);            
            foreach (var sccnode in scc)
            {
                foreach (Phx.Graphs.CallEdge e in sccnode.SuccessorEdges)
                {
                    if (scc.Contains(e.CalleeCallNode))
                    {
                        var edge = new Edge<CallNode>(e.CallerCallNode, e.CalleeCallNode);
                        if (!sccGraph.ContainsEdge(edge))
                            sccGraph.AddEdge(edge);
                    }
                }
            }            
            GraphUtil.DumpAsText<CallNode, Edge<CallNode>>(sccWriter, sccGraph, 
                (CallNode n) => (n.FunctionSymbol.QualifiedName + "(" + n.Id + ")"));

            GraphUtil.DumpAsDGML<CallNode, Edge<CallNode>>("SCCGraph.dgml", sccGraph,
                (CallNode n) => (n.Id),
                (CallNode n) => (n.FunctionSymbol.QualifiedName + "(" + n.Id + ")"),
                null,null);

            //debugScc(sccGraph);
        }

        private void debugScc(BidirectionalGraph<CallNode,Edge<CallNode>> sccGraph)
        {
            while (true)
            {
                Console.WriteLine("Enter source and dest node ids: ");
                uint src = uint.Parse(Console.ReadLine());
                uint dest = uint.Parse(Console.ReadLine());

                if (src == 0 && dest == 0)
                    break;

                var paths = GraphUtil.FindAcyclicPaths<CallNode,Edge<CallNode>>(sccGraph, 
                    cg.Node(src) as CallNode, cg.Node(dest) as CallNode);
                foreach (var stack in paths)
                {
                    Console.WriteLine("Path: ");
                    foreach (var n in stack)
                        Console.WriteLine("\t" + (n as CallNode).FunctionSymbol.QualifiedName + " (" +
                            (n as Phx.Graphs.CallNode).Id + ")");
                    Console.WriteLine();
                }
            }   
        }                                
    }

    public class GraphUtil
    {        
        public static BidirectionalGraph<CallNode, Edge<CallNode>> CGtoQG(CallGraph cg)
        {            
            BidirectionalGraph<CallNode, Edge<CallNode>> g = new BidirectionalGraph<CallNode, Edge<CallNode>>();
            Phx.Graphs.NodeFlowOrder nodeOrder = Phx.Graphs.NodeFlowOrder.New(cg.Lifetime);
            nodeOrder.Build(cg, Phx.Graphs.Order.PostOrder);                   

            for (uint i = 1; i <= nodeOrder.NodeCount; i++)
            {
                Phx.Graphs.CallNode node = nodeOrder.Node(i).AsCallNode;
                g.AddVertex(node);
            }

            foreach (var node in g.Vertices)
            {
                foreach (Phx.Graphs.CallEdge e in node.SuccessorEdges)
                {
                    var edge = new Edge<CallNode>(e.CallerCallNode, e.CalleeCallNode);                    
                    if (!g.ContainsEdge(edge))
                        g.AddEdge(edge);
                }
            }            
            return g;           
        }

        public static void SerializeCallGraphAsDGML(CallGraph cg, XmlWriter writer, System.Func<CallNode,string> summaryinfo)
        {
            QuickGraph.Action<CallNode, DirectedGraphNode> nodeformater =
                (CallNode cn, DirectedGraphNode dn) =>
                {
                    string qualtypename;
                    string funcname;
                    string typename;
                    var encltype = cn.FunctionSymbol.EnclosingAggregateType;
                    if (encltype == null)
                    {
                        qualtypename = String.Empty;
                        typename = String.Empty;
                        funcname = cn.FunctionSymbol.QualifiedName;
                    }
                    else
                    {
                        qualtypename = PhxUtil.GetTypeName(encltype);
                        if (encltype.TypeSymbol != null)
                        {
                            typename = encltype.TypeSymbol.NameString;
                            //remove package name
                            typename = typename.Substring(typename.LastIndexOf('.') + 1);
                        }
                        else
                            typename = qualtypename;
                        funcname = PhxUtil.GetFunctionName(cn.FunctionSymbol);
                    }
                                        
                    var signature = PhxUtil.GetFunctionTypeSignature(cn.FunctionSymbol.FunctionType);
                    dn.Description = qualtypename +"::" + funcname + "/" + signature;                    
                    dn.Label = typename+"."+funcname;                    
                };

            QuickGraph.Action<Edge<CallNode>, DirectedGraphLink> edgeformater =
                (Edge<CallNode> edge, DirectedGraphLink link) =>
                {
                    link.FontWeight = FontWeightEnum.Bold;
                    if (summaryinfo != null)
                    {
                        link.Label = summaryinfo(edge.Target);
                    }
                    else
                        link.Label = String.Empty;
                };
            
            //create a directed graph
            var qg = GraphUtil.CGtoQG(cg);
            //remove all isolated nodes from the graph
            var isolatednodes = (from node in qg.Vertices
                                where qg.IsInEdgesEmpty(node) && qg.IsOutEdgesEmpty(node)
                                select node).ToList();
            foreach (var node in isolatednodes)
                qg.RemoveVertex(node);

            var dg = DirectedGraphMLExtensions.ToDirectedGraphML<CallNode, Edge<CallNode>>(
                qg, 
                (CallNode cn) => (cn.Id.ToString()), 
                (Edge<CallNode> edge) => (edge.Source.Id.ToString() + edge.Target.Id.ToString()),
                nodeformater, edgeformater);
            DirectedGraphMLExtensions.WriteXml(dg, writer);
            writer.Flush();
            //DirectedGraphMLExtensions.DirectedGraphSerializer.Serialize(writer, dg);
            //writer.Flush();
        }

        public static void DumpAsText<TVertex,TEdge> (
            StreamWriter writer,
            IVertexAndEdgeListGraph<TVertex,TEdge> graph,
            System.Func<TVertex,string> GetLabel)
            where TEdge : IEdge<TVertex>
        {
            writer.WriteLine("Size: " + graph.VertexCount);
            writer.WriteLine("Nodes: ");
            foreach (var v in graph.Vertices)
                writer.WriteLine("\t" + GetLabel(v));
            writer.WriteLine("Edges: ");
            foreach (var v in graph.Vertices)
            {
                writer.WriteLine(GetLabel(v) + " Calls: ");                
                writer.WriteLine("# of Succs: " + graph.OutEdges(v).Count());
                foreach (var e in graph.OutEdges(v))
                    writer.WriteLine("\t" + GetLabel(e.Target));                
            }            
        }

        public static void DumpAsDGML<TVertex, TEdge>(
            String fileName,
            IVertexAndEdgeListGraph<TVertex, TEdge> graph,
            System.Func<TVertex,uint> GetId,
            System.Func<TVertex, string> GetLabel,
            System.Func<TVertex,IEnumerable<Pair<string,string>>> GetVertexAttrValuePair,
            System.Func<TEdge,IEnumerable<Pair<string,string>>> GetEdgeAttrValuePair)
            where TEdge : IEdge<TVertex>
        {
            XmlWriter xmlWriter = XmlWriter.Create(fileName, new XmlWriterSettings() { Encoding = Encoding.UTF8 });
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("DirectedGraph", "http://schemas.microsoft.com/vs/2009/dgml");
            xmlWriter.WriteStartElement("Nodes");

            foreach (var v in graph.Vertices)
            {
                xmlWriter.WriteStartElement("Node");
                xmlWriter.WriteAttributeString("Id", GetId(v).ToString()); // id is an unique identifier of the node 
                xmlWriter.WriteAttributeString("Label", GetLabel(v)); // label is the text on the node you see in the graph
                if (GetVertexAttrValuePair != null)
                {
                    var pairs = GetVertexAttrValuePair(v);
                    foreach (var pair in pairs)
                        xmlWriter.WriteAttributeString(pair.Key, pair.Value);
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("Links");
            foreach (var v in graph.Vertices)
            {
                foreach (var edge in graph.OutEdges(v))
                {
                    xmlWriter.WriteStartElement("Link");
                    xmlWriter.WriteAttributeString("Source", GetId(edge.Source).ToString()); // ID! of the source node
                    xmlWriter.WriteAttributeString("Target", GetId(edge.Target).ToString()); // ID of the target node 
                    if (GetEdgeAttrValuePair != null)
                    {
                        var pairs = GetEdgeAttrValuePair(edge);
                        foreach (var pair in pairs)
                            xmlWriter.WriteAttributeString(pair.Key, pair.Value);
                    }
                    xmlWriter.WriteEndElement();                    
                }
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();           
        }

        public static IEnumerable<Stack> FindAcyclicPaths<TVertex,TEdge>(
            IVertexAndEdgeListGraph<TVertex,TEdge> g, 
            TVertex src, 
            TVertex dest)
            where TEdge : IEdge<TVertex>
        {
            HashSet<TVertex> visited = new HashSet<TVertex>();
            Stack stack = new Stack();
            stack.Push(src);
            return FindAcyclicPathsRecursive<TVertex, TEdge>(g, src, dest, visited, stack);
        }

        public static IEnumerable<Stack> FindAcyclicPathsRecursive<TVertex, TEdge>(
            IVertexAndEdgeListGraph<TVertex, TEdge> g, 
            TVertex n, 
            TVertex dest,    
            HashSet<TVertex> visited, 
            Stack stack)
            where TEdge : IEdge<TVertex>
        {
            if (visited.Contains(n))
                yield break;

            visited.Add(n);
            
            foreach (var edge in g.OutEdges(n))
            {
                if (edge.Target.Equals(dest))
                {
                    stack.Push(edge.Target);
                    yield return (stack.Clone() as Stack);
                    stack.Pop();
                }
                else
                {
                    stack.Push(edge.Target);
                    foreach (var witness in FindAcyclicPathsRecursive<TVertex,TEdge>(g, edge.Target, dest, visited, stack))
                        yield return witness;
                    stack.Pop();
                }
            }
        }
    }
}
