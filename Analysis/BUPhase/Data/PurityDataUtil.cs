using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.IO;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{
    public class PurityDataUtil
    {
        public static void CheckInvariant(PurityAnalysisData data)
        {
            //check for uncontained edges
            var uncontainedEdges = from edge in data.OutHeapGraph.Edges
                                   where !(data.OutHeapGraph.ContainsVertex(edge.Source)
                                             && data.OutHeapGraph.ContainsVertex(edge.Target))
                                   select edge;
            //Contract.Assert(!uncontainedEdges.Any());

            //check for uncontained skipped call vertices
            var argset = new HeapVertexSet();
            foreach (var call in data.SkippedCalls)
            {
                argset.UnionWith(call.GetReferredVertices());
            }
            var uncontainedArgs = from arg in argset
                                  where !data.OutHeapGraph.ContainsVertex(arg)
                                  select arg;
            //Contract.Assert(!uncontainedArgs.Any());
        }

        public static HeapVertexBase CollapseVertices(PurityAnalysisData data, HeapVertexSet toCollapseSet)
        {
            var rep = NodeEquivalenceRelation.ChooseRepresentative(toCollapseSet);
            toCollapseSet.Remove(rep);
            CollapseVertices(data, toCollapseSet, rep);
            return rep;
        }

        /// <summary>       
        /// This method will merge the input vertices in the outHeapGraph and also 
        /// adjust read/write sets.        
        /// Preconditions: 
        /// (a) 'rep' does not belong to 'collapse set'
        /// (b) requires all vertices in 'collapse set' to be of the same type
        /// (c) 'rep' need not be of the same type as 'collapse set'
        /// </summary>
        /// <param name="vertices"></param>
        public static void CollapseVertices(PurityAnalysisData data, 
            ICollection<HeapVertexBase> toCollapse, 
            HeapVertexBase rep)
        {
            data.OutHeapGraph.CollapseVertices(toCollapse, rep);

            //collapse types
            UnionSetOfPairs<HeapVertexBase, string>(data.types, toCollapse, rep);

            //collapse effect sets
            UnionSetOfPairs<HeapVertexBase, Field>(data.MayWriteSet, toCollapse, rep);

            //need not modify skipped calls as variables will not be collapsed

            //collapse linq methods set            
            if (data.linqMethods.Any((HeapVertexBase b) => (toCollapse.Contains(b))))
            {
                data.linqMethods.RemoveWhere((HeapVertexBase b) => (toCollapse.Contains(b)));
                data.linqMethods.Add(rep);
            }
        }

        public static void UnionSetOfPairs<K, V>(ExtendedMap<K, V> map,
            ICollection<K> vertices, K rep) where K : HeapVertexBase
        {
            List<V> valueList = new List<V>();
            foreach (var v in vertices)
            {
                if (map.ContainsKey(v))
                {
                    valueList.AddRange(map[v]);
                    map.Remove(v);
                }
            }
            foreach (var value in valueList)
                map.Add(rep, value);
        }

        public static int filecount = 0;
        public static void DumpAsDGML(PurityAnalysisData data, string filename, Call appliedCall)
        {
            if (filename == null)
            {
                filename = "temp" + (++filecount) + ".dgml";
                var writer = File.CreateText("temp" + filecount + ".txt");
                writer.Write(data.ToString());
                writer.Flush();
                writer.Close();
            }
            //label of the vertices
            var labelmap = new Dictionary<HeapVertexBase, string>();
            int varcount = 0;
            int loadcount = 0;
            int internalcount = 0;
            int rvcount = 0;

            //colour of the vertices
            var colormap = new Dictionary<HeapVertexBase, string>();

            foreach (var v in data.OutHeapGraph.Vertices)
            {
                if (v is ParameterHeapVertex)
                {
                    labelmap.Add(v, (v as ParameterHeapVertex).name);
                    colormap.Add(v, "LightPink");
                }
                else if (v is InternalHeapVertex)
                {
                    labelmap.Add(v, "I" + internalcount++);
                    colormap.Add(v, "LightGreen");
                }
                else if (v is GlobalLoadVertex)
                {
                    labelmap.Add(v, "Global");
                    colormap.Add(v, "Maroon");
                }
                else if (v is LoadHeapVertex)
                {
                    labelmap.Add(v, "L" + loadcount++);
                    colormap.Add(v, "Aqua");
                }
                else if (v is ReturnedValueVertex)
                {
                    labelmap.Add(v, "rv" + rvcount++);
                    colormap.Add(v, "SandyBrown");
                }
                else if (v is VariableHeapVertex)
                {
                    labelmap.Add(v, "var" + varcount++);
                    colormap.Add(v, "SlateGray");
                }
                else
                {
                    labelmap.Add(v, v.ToString());
                    colormap.Add(v, "White");
                }
            }
            System.Func<Field, string> labelGen = (Field f) =>
            {
                if (f is NamedField)
                    return (f as NamedField).GetFieldName();
                else if (f is RangeField)
                    return "[]";
                else if (f is DelegateMethodField)
                    return "del-method";
                else if (f is DelegateRecvrField)
                    return "del-recvr";
                else
                    return f.ToString();
            };

            //if (File.Exists(filename))
            //    File.Copy(filename, "old_"+filename, true);
            
            XmlWriter xmlWriter = XmlWriter.Create(filename, new XmlWriterSettings() { Encoding = Encoding.UTF8 });
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("DirectedGraph", "http://schemas.microsoft.com/vs/2009/dgml");
            xmlWriter.WriteStartElement("Nodes");
            foreach (var v in data.OutHeapGraph.Vertices)
            {
                xmlWriter.WriteStartElement("Node");
                xmlWriter.WriteAttributeString("Id", v.GetHashCode().ToString()); // id is an unique identifier of the node 
                xmlWriter.WriteAttributeString("Label", labelmap[v]); // label is the text on the node you see in the graph
                xmlWriter.WriteAttributeString("Background", colormap[v]);
                xmlWriter.WriteAttributeString("Description", v.ToString());

                if (v is InternalHeapVertex)
                {
                    string typestr = "";
                    foreach (var typename in data.GetTypes(v))
                    {
                        typestr += typename + ";";
                    }
                    xmlWriter.WriteAttributeString("Typename", typestr);
                }
                xmlWriter.WriteEndElement();
            }

            //add all the skipped calls as nodes
            var skcallIdmap = new Dictionary<Call, string>();
            int skid = 0;
            foreach (var skcall in data.SkippedCalls)
            {                
                var id = "skcall" + skid;
                
                //write element
                xmlWriter.WriteStartElement("Node");
                xmlWriter.WriteAttributeString("Id", id); 
                
                if(skcall is VirtualCall)
                    xmlWriter.WriteAttributeString("Label", (skcall as VirtualCall).methodname);
                else if(skcall is DelegateCall)
                    xmlWriter.WriteAttributeString("Label", "del"+skid);

                xmlWriter.WriteAttributeString("Background", "Tan");
                xmlWriter.WriteAttributeString("Description", skcall.ToString());
                xmlWriter.WriteAttributeString("ContextID", skcall.contextid.ToString());

                if (appliedCall != null && appliedCall.Equals(skcall))
                {
                    xmlWriter.WriteAttributeString("Stroke", "Black");
                    xmlWriter.WriteAttributeString("StrokeThickness", "5");
                }
                xmlWriter.WriteEndElement();

                skcallIdmap.Add(skcall, id);
                skid++;
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Links");
            foreach (var edge in data.OutHeapGraph.Edges)
            {
                xmlWriter.WriteStartElement("Link");
                xmlWriter.WriteAttributeString("Source", edge.Source.GetHashCode().ToString()); // ID! of the source node
                xmlWriter.WriteAttributeString("Target", edge.Target.GetHashCode().ToString()); // ID of the target node 

                //format edge
                if (edge is InternalHeapEdge)
                {
                    xmlWriter.WriteAttributeString("Stroke", "Black");
                    xmlWriter.WriteAttributeString("StrokeThickness", "2");
                }
                else
                {
                    xmlWriter.WriteAttributeString("Stroke", "Blue");
                    xmlWriter.WriteAttributeString("StrokeDashArray", "5");
                    xmlWriter.WriteAttributeString("StrokeThickness", "2");
                }
                if (edge.Field != null)
                    xmlWriter.WriteAttributeString("Label", labelGen(edge.Field));
                xmlWriter.WriteEndElement();
            }

            //add all the skipped call as edges
            foreach (var skcall in data.SkippedCalls)
            {
                var id = skcallIdmap[skcall];
                int argnum = 1;
                foreach(var p in skcall.GetAllParams())
                {
                    xmlWriter.WriteStartElement("Link");
                    xmlWriter.WriteAttributeString("Source", id); 
                    xmlWriter.WriteAttributeString("Target", p.GetHashCode().ToString());
                    xmlWriter.WriteAttributeString("Stroke", "Black");
                    xmlWriter.WriteAttributeString("StrokeThickness", "2");
                    xmlWriter.WriteAttributeString("Label", "Arg"+ argnum++);
                    xmlWriter.WriteEndElement();
                }
                if (skcall.HasReturnValue())
                {
                    xmlWriter.WriteStartElement("Link");
                    xmlWriter.WriteAttributeString("Source", id);
                    xmlWriter.WriteAttributeString("Target", skcall.GetReturnValue().GetHashCode().ToString());
                    xmlWriter.WriteAttributeString("Stroke", "Black");
                    xmlWriter.WriteAttributeString("StrokeThickness", "2");
                    xmlWriter.WriteAttributeString("Label", "retvar");
                    xmlWriter.WriteEndElement();
                }
                if (skcall is DelegateCall)
                {
                    xmlWriter.WriteStartElement("Link");
                    xmlWriter.WriteAttributeString("Source", id);
                    xmlWriter.WriteAttributeString("Target", (skcall as DelegateCall).target.GetHashCode().ToString());
                    xmlWriter.WriteAttributeString("Stroke", "Black");
                    xmlWriter.WriteAttributeString("StrokeThickness", "2");
                    xmlWriter.WriteAttributeString("Label", "targetptr");
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        public static void TraceData(PurityAnalysisData data)
        {
            Trace.Indent();

            var indentsize = Trace.IndentSize;
            var indentstr = "";
            //for (int i = 0; i < indentsize; i++)
            //    indentstr += " ";

            var hg = data.OutHeapGraph;
            Trace.WriteLine(indentstr +
                String.Format("HeapGraph - Vertices[{0}], Edges[{1}]",
                hg.VertexCount, hg.EdgeCount));

            foreach (HeapVertexBase vertex in hg.Vertices)
            {
                // print vertex
                Trace.WriteLine(indentstr +
                    String.Format(
                    "#{0}: Type[{1}],Contents[{2}]",
                    vertex.Id,
                    vertex.GetType().Name,
                    vertex.ToString()));

                // print edges
                if (!hg.IsOutEdgesEmpty(vertex))
                {
                    foreach (HeapEdgeBase edge in hg.OutEdges(vertex))
                    {
                        Trace.WriteLine(indentstr +
                            String.Format(
                            "\t{0}: Field[{1}],Target[{2}]",
                            edge.GetType().Name,
                            edge.Field == null ? "" : edge.Field.ToString(),
                            edge.Target.Id));
                    }
                }
            }
            
            Trace.WriteLine(indentstr + "SkippedCalls: ");
            foreach (var call in data.SkippedCalls)            
                Trace.WriteLine(indentstr + call);            

            Trace.WriteLine(indentstr + "MayWriteSet: ");
            foreach (var key in data.MayWriteSet.Keys)
            {
                Trace.Write(indentstr + key + " :- ");
                foreach (var value in data.MayWriteSet[key])
                    Trace.Write(indentstr + value + ",");
                Trace.WriteLine("");
            }
            Trace.Unindent();
        }

        /// <summary>
        /// Renames a parameter vertex. Assumes that there is only one 
        /// instance of a parameter vertex in the input data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="oldname"></param>
        /// <param name="newname"></param>
        public static void RenameParameter(PurityAnalysisData data,
            string oldname, string newname)
        {
            var oldParams = data.OutHeapGraph.Vertices.OfType<ParameterHeapVertex>().Where(
                                (ParameterHeapVertex p) => (p.name.Equals(oldname)));
            if (oldParams.Any())
            {
                foreach (var oldParam in oldParams)
                    oldParam.name = newname;
            }
        }
    }
}
