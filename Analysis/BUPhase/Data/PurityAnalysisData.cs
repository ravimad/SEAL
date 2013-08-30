using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.Summaries;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{    
    [Serializable]
    public class PurityAnalysisData : ISerializable
    {
        //wilcard type that matches anything
        public const string AnyType = "AnyType";
        public const string ArrayType = "ArrayType";

        //transformer graph part       
        public virtual HeapGraphBase OutHeapGraph { get; protected set; }       
 
        //indirect call part
        protected HashSet<Call> skippedCalls;        

        //additional data for optimization
        public ExtendedMap<Call, string> skippedCallTargets;
        public ExtendedMap<HeapVertexBase, string> types;                
        
        //additional data for precision
        protected HeapVertexSet strongUpdateVertices;

        //additional analysis data for measuring unsoundness
        public HashSet<string> unanalyzableCalls;

        //additional data for handling concurrency
        public HeapVertexSet liveThreadObjects;

        //Client state
        public HeapVertexSet linqDelegates;                
        public ExtendedMap<HeapVertexBase, Field> MayWriteSet;
        public ExtendedMap<HeapVertexBase, Field> ReadSet;        
        //public HashSet<Pair<HeapVertexBase, Field>> MustWriteSet;        
       
        public PurityAnalysisData(HeapGraphBase hg)
        {            
            //init core state
            this.OutHeapGraph = hg;
            this.skippedCalls = new HashSet<Call>();
            this.skippedCallTargets = new ExtendedMap<Call, string>();
            this.types = new ExtendedMap<HeapVertexBase, string>();
            this.unanalyzableCalls = new HashSet<string>();
            this.strongUpdateVertices = new HeapVertexSet();
            this.liveThreadObjects = new HeapVertexSet();
            
            InitClientState();
        }        

        public PurityAnalysisData(SerializationInfo info, StreamingContext context)
        {                        
            this.OutHeapGraph = (HeapGraphBase)info.GetValue("outheapgraph", typeof(HeapGraphBase));

            this.skippedCalls = new HashSet<Call>((List<Call>)info.GetValue("skippedcalls", typeof(List<Call>)));            

            this.skippedCallTargets = new ExtendedMap<Call, string>(
                (List<Pair<Call, string>>)info.GetValue("skippedcalltargets", typeof(List<Pair<Call, string>>)));

            //deserialize concrete types
            this.types = new ExtendedMap<HeapVertexBase, string>();
            var sourceTypes = (List<Pair<HeapNodeWrapper, string>>)info.GetValue("concretetypes",
                typeof(List<Pair<HeapNodeWrapper, string>>));
            foreach (var pair in sourceTypes)
            {
                this.types.Add(pair.Key.GetNode(), pair.Value);
            }

            //conservatively initialize strong update vertices
            this.strongUpdateVertices = new HeapVertexSet();

            //make unanalyzable calls set empty
            this.unanalyzableCalls = new HashSet<string>();

            //deserialize liveThreadObjects
            //this.liveThreadObjects = 
            //    new HeapVertexSet((List<HeapVertexBase>)info.GetValue("livethreadobjects", typeof(List<HeapVertexBase>)));
            this.liveThreadObjects = new HeapVertexSet();

            this.DeserializeClientState(info, context);
        }        
        
        public void AddConcreteType(HeapVertexBase v, string type)
        {
            this.types.Add(v, type);
        }

        public void AddApproximateType(HeapVertexBase v, string type)
        {
            this.types.Add(v, type);
        }       

        public IEnumerable<string> GetTypes(HeapVertexBase v)
        {
            if (types.ContainsKey(v))
                return types[v];
            return new List<string>();
        }        

        public IEnumerable<string> GetProcessedTargets(Call c)
        {
            if (skippedCallTargets.ContainsKey(c))
                return skippedCallTargets[c];
            return new List<string>();
        }

        public bool CanStrongUpdate(HeapVertexBase v)
        {
            return this.strongUpdateVertices.Contains(v);
        }        

        public void AddVertexWithStrongUpdate(HeapVertexBase v)
        {
            if (this.OutHeapGraph is PartitionGraph)
            {
                (this.OutHeapGraph as PartitionGraph).AddVertexUnique(v);
            }
            else 
                this.OutHeapGraph.AddVertex(v);

            this.strongUpdateVertices.Add(v);            
        }

        public void RemoveStrongUpdates(HeapVertexBase v)
        {
            this.strongUpdateVertices.Remove(v);

            if (this.OutHeapGraph is PartitionGraph)
            {
                (this.OutHeapGraph as PartitionGraph).MoveOutEdgesToShared(v);
            }
        }

        public void AddThreadObjects(IEnumerable<HeapVertexBase> thObjs)
        {
            this.liveThreadObjects.UnionWith(thObjs);
        }

        public HeapVertexSet GetThreadObjs()
        {
            return this.liveThreadObjects;
        }

        public void RemoveFromThreadObjs(HeapVertexBase thObj)
        {
            this.liveThreadObjects.Remove(thObj);
        }

        public PurityAnalysisData Copy()
        {
            PurityAnalysisData data = new PurityAnalysisData(this.OutHeapGraph.Copy());                        
            data.skippedCalls.UnionWith(this.skippedCalls);
            data.skippedCallTargets.UnionWith(this.skippedCallTargets);
            data.types.UnionWith(this.types);
            data.strongUpdateVertices.UnionWith(this.strongUpdateVertices);
            data.liveThreadObjects.UnionWith(this.liveThreadObjects);
            data.unanalyzableCalls.UnionWith(this.unanalyzableCalls);

            CopyClientState(this, data);       
            return data;
        }
        
        public void CopyInto(PurityAnalysisData data)
        {            
            this.OutHeapGraph = data.OutHeapGraph;
            this.skippedCalls = data.skippedCalls;
            this.skippedCallTargets = data.skippedCallTargets;
            this.types = data.types;                        
            this.strongUpdateVertices = data.strongUpdateVertices;
            this.liveThreadObjects = data.liveThreadObjects;
            this.unanalyzableCalls = data.unanalyzableCalls;

            OverwriteClientState(data,this);
        }
 
        public void Union(PurityAnalysisData data)
        {
            this.OutHeapGraph.Union(data.OutHeapGraph);
            this.skippedCalls.UnionWith(data.skippedCalls);
            this.skippedCallTargets.UnionWith(data.skippedCallTargets);
            this.types.UnionWith(data.types);            
            this.strongUpdateVertices.UnionWith(data.strongUpdateVertices);
            this.liveThreadObjects.UnionWith(data.liveThreadObjects);
            this.unanalyzableCalls.UnionWith(data.unanalyzableCalls);            

            this.UnionClientState(data);
        }     

        public bool Equivalent(PurityAnalysisData purityData)
        {
            //check for equality of the graphs 
            if ((this.OutHeapGraph.VertexCount == purityData.OutHeapGraph.VertexCount)
                && (this.OutHeapGraph.EdgeCount == purityData.OutHeapGraph.EdgeCount))
            {
                if (this.skippedCalls.SetEquals(purityData.skippedCalls)
                    && EquivalentClientStates(purityData))
                {
                    if (this.OutHeapGraph.ContainedIn(purityData.OutHeapGraph))
                        return true;
                }
            }
            return false;
        }        

        public override bool Equals(object obj)
        {
            if (obj is PurityAnalysisData)
                return this.Equivalent(obj as PurityAnalysisData);
            return false;
        }

        public override int GetHashCode()
        {
            return this.OutHeapGraph.GetHashCode();
        }

        //removes a reference to the vertex from the purity data.
        public void RemoveVertex(HeapVertexBase v)
        {            
            this.OutHeapGraph.RemoveVertex(v);                        
            this.types.Remove(v);                                                                                    
            this.strongUpdateVertices.Remove(v);
            this.liveThreadObjects.Remove(v);

            RemoveFromClientState(v);
        }        

        internal void JoinAllData(IEnumerable<PurityAnalysisData> datalist)
        {
            var array = datalist.ToArray();
            this.CopyInto(array[0]);
            for (int i = 1; i < array.Length; i++)
            {
                this.Union(array[i]);
            }
        }

        #region skipped calls mangement

        public virtual void AddSkippedCall(Call c)
        {
            this.skippedCalls.Add(c);
        }

        public virtual void RemoveSkippedCall(Call c)
        {
            skippedCalls.Remove(c);
            skippedCallTargets.Remove(c);                       
        }
        
        public IEnumerable<string> GetTargets(Call call)
        {
            HashSet<string> targets;
            if (this.skippedCallTargets.TryGetValue(call, out targets))
                return targets;
            return new List<string>();
        }

        public IEnumerable<Call> SkippedCalls
        {
            get { return this.skippedCalls; }
        }

        #endregion 

        public void Dump()
        {
            this.OutHeapGraph.Dump();

            Console.ForegroundColor = ConsoleColor.Green;            
            Console.WriteLine("SkippedCalls: ");
            foreach (var call in skippedCalls)
                Console.Write(call + ";");
            Console.WriteLine();

            DumpClientState();

            Console.ResetColor();
        }

        public override string ToString()
        {            
            StringWriter writer = new StringWriter();            
            writer.WriteLine();
            writer.WriteLine(this.OutHeapGraph.ToString());

            writer.WriteLine("SkippedCalls: ");
            foreach (var call in skippedCalls)
                writer.WriteLine(call + ";");            
            
            return (writer.GetStringBuilder().ToString() + GetClientStateAsString()) ;
        }                       

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {            
            info.AddValue("outheapgraph", OutHeapGraph);
            info.AddValue("skippedcalls", skippedCalls.ToList());                                    
            info.AddValue("skippedcalltargets", skippedCallTargets.GetListOfPairs());

            //serialize concrete types
            var typelist = new List<Pair<HeapNodeWrapper, string>>();
            foreach (var pair in types.GetListOfPairs())
            {
                typelist.Add(new Pair<HeapNodeWrapper,string>(new HeapNodeWrapper(pair.Key), pair.Value));
            }
            info.AddValue("concretetypes",typelist);

            var thobjList = from thobj in this.liveThreadObjects
                            select new HeapNodeWrapper(thobj);
            info.AddValue("livethreadobjects", thobjList.ToList());                        
            //info.AddValue("unanalyzablecalls", delegateTargets.ToList());      

            this.SerializeClientData(info, context);
        }        

        #region Client state manipulation

        public void InitClientState()
        {
            this.linqDelegates = new HeapVertexSet();
            this.MayWriteSet = new ExtendedMap<HeapVertexBase, Field>();
            this.ReadSet = new ExtendedMap<HeapVertexBase, Field>();
            //this.MustWriteSet = new HashSet<Pair<HeapVertexBase, Field>>();                                 
        }

        public void DeserializeClientState(SerializationInfo info, StreamingContext context)
        {
            //deserialize may write set
            this.MayWriteSet = new ExtendedMap<HeapVertexBase, Field>();
            var sourceWS = (List<Pair<HeapNodeWrapper, FieldWrapper>>)info.GetValue("maywriteset",
                typeof(List<Pair<HeapNodeWrapper, FieldWrapper>>));
            foreach (var pair in sourceWS)
            {
                this.MayWriteSet.Add(pair.Key.GetNode(), pair.Value.GetField());
            }

            //deserialize read set
            this.ReadSet = new ExtendedMap<HeapVertexBase, Field>();
            var sourceRS = (List<Pair<HeapNodeWrapper, FieldWrapper>>)info.GetValue("readset",
                typeof(List<Pair<HeapNodeWrapper, FieldWrapper>>));
            foreach (var pair in sourceRS)
            {
                this.ReadSet.Add(pair.Key.GetNode(), pair.Value.GetField());
            }
            //this.MustWriteSet = new HashSet<Pair<HeapVertexBase, Field>>(
            //    (List<Pair<HeapVertexBase, Field>>)info.GetValue("mustwriteset", typeof(List<Pair<HeapVertexBase, Field>>)));                                          

            this.linqDelegates = new HeapVertexSet();
        }                            

        public void CopyClientState(PurityAnalysisData src, PurityAnalysisData dest)
        {
            dest.linqDelegates.UnionWith(src.linqDelegates);
            dest.MayWriteSet.UnionWith(src.MayWriteSet);
            dest.ReadSet.UnionWith(src.ReadSet);
            //data.MustWriteSet.UnionWith(this.MustWriteSet);                                                     
        }

        public void OverwriteClientState(PurityAnalysisData src, PurityAnalysisData dest)
        {
            dest.linqDelegates = src.linqDelegates;
            dest.MayWriteSet = src.MayWriteSet;
            dest.ReadSet = src.ReadSet;
            //this.MustWriteSet = data.MustWriteSet;                        
        }

        public void UnionClientState(PurityAnalysisData data)
        {
            this.linqDelegates.UnionWith(data.linqDelegates);
            this.MayWriteSet.UnionWith(data.MayWriteSet);
            this.ReadSet.UnionWith(data.ReadSet);
            //this.MustWriteSet.IntersectWith(purityDependencyData.MustWriteSet);       
        }

        public bool EquivalentClientStates(PurityAnalysisData data)
        {
            //ignoring other client states in the equality check (as they are implied 
            // the following heck)
            return (this.MayWriteSet.SetEquals(data.MayWriteSet)
                && this.ReadSet.SetEquals(data.ReadSet));
        }

        public void RemoveFromClientState(HeapVertexBase v)
        {
            this.linqDelegates.Remove(v);
            this.MayWriteSet.Remove(v);
            this.ReadSet.Remove(v);
            //this.MustWriteSet.RemoveWhere((Pair<HeapVertexBase, Field> pair) => pair.Key.Equals(v));            
        }

        public void DumpClientState()
        {
            Console.WriteLine("MayWriteSet: ");
            this.DumpEffectSet(this.MayWriteSet);
            Console.WriteLine("ReadSet: ");
            this.DumpEffectSet(this.ReadSet);
            //Console.WriteLine("MustWriteSet: ");
            //this.DumpEffectSet(this.MustWriteSet);
        }

        private void DumpEffectSet(ExtendedMap<HeapVertexBase, Field> effectSet)
        {
            foreach (var key in effectSet.Keys)
            {
                Console.Write("{0} :- ", key);
                foreach (var value in effectSet[key])
                    Console.Write("{0},", value);
                Console.WriteLine();
            }
        }

        public string GetClientStateAsString()
        {
            StringWriter writer = new StringWriter();
            writer.WriteLine("MayWriteSet: ");
            foreach (var key in MayWriteSet.Keys)
            {
                writer.Write("{0} :- ", key);
                foreach (var value in MayWriteSet[key])
                    writer.Write("{0},", value);
                writer.WriteLine();
            }

            writer.WriteLine("ReadSet: ");
            foreach (var key in ReadSet.Keys)
            {
                writer.Write("{0} :- ", key);
                foreach (var value in ReadSet[key])
                    writer.Write("{0},", value);
                writer.WriteLine();
            }
            return writer.GetStringBuilder().ToString();
        }        

        public void SerializeClientData(SerializationInfo info, StreamingContext context)
        {
            //serialize may write set            
            var wslist = new List<Pair<HeapNodeWrapper, FieldWrapper>>();
            foreach (var pair in MayWriteSet.GetListOfPairs())
            {
                wslist.Add(new Pair<HeapNodeWrapper, FieldWrapper>(new HeapNodeWrapper(pair.Key),
                    new FieldWrapper(pair.Value)));
            }
            info.AddValue("maywriteset", wslist);

            var readlist = new List<Pair<HeapNodeWrapper, FieldWrapper>>();
            foreach (var pair in MayWriteSet.GetListOfPairs())
            {
                readlist.Add(new Pair<HeapNodeWrapper, FieldWrapper>(new HeapNodeWrapper(pair.Key),
                    new FieldWrapper(pair.Value)));
            }
            info.AddValue("readset", readlist);

            //info.AddValue("mustwriteset",MustWriteSet.ToList());
        }

        public IEnumerable<Field> GetMayWriteFields(HeapVertexBase v)
        {
            if (MayWriteSet.ContainsKey(v))
                return MayWriteSet[v];
            return new List<Field>();
        }

        public IEnumerable<Field> GetReadFields(HeapVertexBase v)
        {
            if (ReadSet.ContainsKey(v))
                return ReadSet[v];
            return new List<Field>();
        }

        public void AddLinqMethod(HeapVertexBase v)
        {
            this.linqDelegates.Add(v);
        }

        public void AddMayWrite(HeapVertexBase v, Field field)
        {
            MayWriteSet.Add(v, field);
        }

        public void AddRead(HeapVertexBase v, Field field)
        {
            ReadSet.Add(v, field);
        }

        #endregion
    }
}
