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

        //write set part
        public ExtendedMap<HeapVertexBase, Field> MayWriteSet;
        
        //additional data for precision
        protected HeapVertexSet strongUpdateVertices;

        //auxilliary part
        public HeapVertexSet linqMethods;        
        public HashSet<string> unanalyzableCalls;
                
        //public HashSet<Pair<HeapVertexBase,Field>> ReadSet;   
        //public HashSet<Pair<HeapVertexBase, Field>> MustWriteSet;        
       
        public PurityAnalysisData(HeapGraphBase hg)
        {            
            //create state
            this.OutHeapGraph = hg;
            this.skippedCalls = new HashSet<Call>();
            this.skippedCallTargets = new ExtendedMap<Call, string>();
            this.types = new ExtendedMap<HeapVertexBase, string>();
            this.MayWriteSet = new ExtendedMap<HeapVertexBase, Field>();            
            //this.ReadSet = new HashSet<Pair<HeapVertexBase, Field>>();
            //this.MustWriteSet = new HashSet<Pair<HeapVertexBase, Field>>();         

            InitAuxiliaryState();
        }

        public void InitAuxiliaryState()
        {
            this.linqMethods = new HeapVertexSet();
            this.unanalyzableCalls = new HashSet<string>();
            this.strongUpdateVertices = new HeapVertexSet();
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

            //deserialize may write set
            this.MayWriteSet = new ExtendedMap<HeapVertexBase, Field>();
            var sourceWS = (List<Pair<HeapNodeWrapper, FieldWrapper>>)info.GetValue("maywriteset", 
                typeof(List<Pair<HeapNodeWrapper, FieldWrapper>>));
            foreach (var pair in sourceWS)
            {
                this.MayWriteSet.Add(pair.Key.GetNode(), pair.Value.GetField());
            }
                            
            //this.ReadSet = new HashSet<Pair<HeapVertexBase, Field>>(
            //    (List<Pair<HeapVertexBase, Field>>)info.GetValue("readset", typeof(List<Pair<HeapVertexBase, Field>>)));            
            //this.MustWriteSet = new HashSet<Pair<HeapVertexBase, Field>>(
            //    (List<Pair<HeapVertexBase, Field>>)info.GetValue("mustwriteset", typeof(List<Pair<HeapVertexBase, Field>>)));                                          

            InitAuxiliaryState();
        }                            
        
        public void AddConcreteType(HeapVertexBase v, string type)
        {
            this.types.Add(v, type);
        }

        public void AddApproximateType(HeapVertexBase v, string type)
        {
            this.types.Add(v, type);
        }

        public void AddLinqMethod(HeapVertexBase v)
        {
            this.linqMethods.Add(v);
        }
        
        public void AddMayWrite(HeapVertexBase v, Field field)
        {            
            MayWriteSet.Add(v,field);
        }

        public IEnumerable<string> GetTypes(HeapVertexBase v)
        {
            if (types.ContainsKey(v))
                return types[v];
            return new List<string>();
        }

        public IEnumerable<Field> GetMayWriteFields(HeapVertexBase v)
        {
            if (MayWriteSet.ContainsKey(v))
                return MayWriteSet[v];
            return new List<Field>();
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

        public PurityAnalysisData Copy()
        {
            PurityAnalysisData data = new PurityAnalysisData(this.OutHeapGraph.Copy());                        
            data.skippedCalls.UnionWith(this.skippedCalls);
            data.skippedCallTargets.UnionWith(this.skippedCallTargets);
            data.types.UnionWith(this.types);            
            data.MayWriteSet.UnionWith(this.MayWriteSet);            
            //data.ReadSet.UnionWith(this.ReadSet);
            //data.MustWriteSet.UnionWith(this.MustWriteSet);          
        
            //copy auxiliary state
            data.linqMethods.UnionWith(this.linqMethods);
            data.strongUpdateVertices.UnionWith(this.strongUpdateVertices);
            data.unanalyzableCalls.UnionWith(this.unanalyzableCalls);
            
            return data;
        }

        public void CopyInto(PurityAnalysisData data)
        {            
            this.OutHeapGraph = data.OutHeapGraph;
            this.skippedCalls = data.skippedCalls;
            this.skippedCallTargets = data.skippedCallTargets;
            this.types = data.types;            
            this.MayWriteSet = data.MayWriteSet;            
            //this.ReadSet = data.ReadSet;
            //this.MustWriteSet = data.MustWriteSet;                        

            //set auxiliary state
            this.linqMethods = data.linqMethods;
            this.strongUpdateVertices = data.strongUpdateVertices;
            this.unanalyzableCalls = data.unanalyzableCalls;            
        }        

        public void Union(PurityAnalysisData data)
        {
            this.OutHeapGraph.Union(data.OutHeapGraph);
            this.skippedCalls.UnionWith(data.skippedCalls);
            this.skippedCallTargets.UnionWith(data.skippedCallTargets);
            this.types.UnionWith(data.types);
            this.MayWriteSet.UnionWith(data.MayWriteSet);                  
            //this.ReadSet.UnionWith(purityDependencyData.ReadSet);
            //this.MustWriteSet.IntersectWith(purityDependencyData.MustWriteSet);       
            
            //merge auxiliary state            
            this.linqMethods.UnionWith(data.linqMethods);      
            this.strongUpdateVertices.UnionWith(data.strongUpdateVertices);
            this.unanalyzableCalls.UnionWith(data.unanalyzableCalls);
        }        

        public bool Equivalent(PurityAnalysisData purityData)
        {
            //check for equality of the graphs 
            if ((this.OutHeapGraph.VertexCount == purityData.OutHeapGraph.VertexCount)
                && (this.OutHeapGraph.EdgeCount == purityData.OutHeapGraph.EdgeCount))
            {
                if (this.skippedCalls.SetEquals(purityData.skippedCalls)
                    && this.MayWriteSet.SetEquals(purityData.MayWriteSet)
                    && this.skippedCalls.SetEquals(purityData.skippedCalls))
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
            this.MayWriteSet.Remove(v);
            this.types.Remove(v);                                                
            //this.ReadSet.RemoveWhere((Pair<HeapVertexBase, Field> pair) => pair.Key.Equals(v));
            //this.MustWriteSet.RemoveWhere((Pair<HeapVertexBase, Field> pair) => pair.Key.Equals(v));            

            //remove from auxiliary data
            this.linqMethods.Remove(v);
            this.strongUpdateVertices.Remove(v);
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
            Console.WriteLine("MayWriteSet: ");
            this.DumpEffectSet(this.MayWriteSet);
            Console.WriteLine("SkippedCalls: ");
            foreach (var call in skippedCalls)
                Console.Write(call + ";");
            Console.WriteLine();
            //Console.WriteLine("ReadSet: ");
            //this.DumpEffectSet(this.ReadSet);
            //Console.WriteLine("MustWriteSet: ");
            //this.DumpEffectSet(this.MustWriteSet);
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

            writer.WriteLine("MayWriteSet: ");
            foreach (var key in MayWriteSet.Keys)
            {
                writer.Write("{0} :- ", key);
                foreach (var value in MayWriteSet[key])
                    writer.Write("{0},", value);
                writer.WriteLine();
            }
            return writer.GetStringBuilder().ToString();
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
        
        #region ISerializable Members

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

            //serialize may write set            
            var wslist = new List<Pair<HeapNodeWrapper, FieldWrapper>>();
            foreach (var pair in MayWriteSet.GetListOfPairs())
            {
                wslist.Add(new Pair<HeapNodeWrapper, FieldWrapper>(new HeapNodeWrapper(pair.Key),
                    new FieldWrapper(pair.Value)));
            }
            info.AddValue("maywriteset", wslist);            

            //info.AddValue("readset", ReadSet.ToList());
            //info.AddValue("mustwriteset",MustWriteSet.ToList());                        
            //info.AddValue("unanalyzablecalls", delegateTargets.ToList());            
        }

        #endregion        
    }
}
