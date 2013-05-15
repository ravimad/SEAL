using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SafetyAnalysis.Framework.Graphs;

namespace SafetyAnalysis.Purity
{
    /// <summary>
    /// Use any hash algorithm to do this efficiently and precisely
    /// </summary>
    public abstract class SkippedCallHash
    {
        /// <summary>
        /// Computes an hashcode for the given set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <returns></returns>
        protected int GetHashCodeForSet<T>(IEnumerable<T> set)
        {
            //add the hashcodes of the elements of the set
            int hashcodesum = 0;
            foreach (var elem in set)
                hashcodesum += elem.GetHashCode();
            return hashcodesum;
        }

        public override abstract int GetHashCode();
        public override abstract bool Equals(object obj);
    }

    public class CompleteEqualityHash : SkippedCallHash 
    {
        List<HashSet<HeapVertexBase>> state = new List<HashSet<HeapVertexBase>>();
        string methodname = String.Empty;
        string typename = String.Empty;
        string signature = null;
        Call skcall;

        //hashcode computation data structures
        List<int> hashcodeList = new List<int>();
        int hashcode = -1;

        public static CompleteEqualityHash GetInstance(Call call, PurityAnalysisData data)
        {
            return new CompleteEqualityHash(call,data);
        }

        private CompleteEqualityHash(Call call, PurityAnalysisData data)
        {
            skcall = call;
            //used in computing the hashcode
            var hashcodeList = new List<int>();

            if (call is VirtualCall)
            {
                var vcall = call as VirtualCall;
                methodname = vcall.methodname;
                typename = vcall.declaringtype;
                signature = vcall.GetSignature();
            }
            else
            {
                var dcall = call as DelegateCall;
                signature = dcall.GetSignature();
            }

            var parms = call.GetAllParams();
            if (call is DelegateCall)
            {
                var tgtvar = new List<VariableHeapVertex> { (call as DelegateCall).GetTarget() };
                parms = parms.Concat(tgtvar);
            }

            foreach (var p in parms)
            {
                var ptVertices = from e in data.OutHeapGraph.OutEdges(p)
                                 select e.Target;
                var set = new HashSet<HeapVertexBase>(ptVertices);
                state.Add(set);

                //compute hashcode for the set
                var setHashcode = this.GetHashCodeForSet(set);
                hashcodeList.Add(setHashcode);
            }

            //compute a hashcode from hashcodeList
            BinaryFormatter serializer = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, hashcodeList);
            StreamReader reader = new StreamReader(ms);
            string serializedstring = reader.ReadToEnd() + methodname + typename;
            hashcode = serializedstring.GetHashCode();
        }
        
        public override int GetHashCode()
        {
            return hashcode;
        }

        public override bool Equals(object obj)
        {
            if (obj is CompleteEqualityHash)
            {
                var skhash = obj as CompleteEqualityHash;
                if (this.skcall.GetType().Equals(skhash.skcall.GetType()))
                {
                    if (this.methodname.Equals(skhash.methodname)
                        && this.typename.Equals(skhash.typename)
                        && this.signature.Equals(skhash.signature))
                    {
                        //compare the states here
                        if (this.state.Count == skhash.state.Count)
                        {
                            return this.state.SequenceEqual(skhash.state,
                                HashSet<HeapVertexBase>.CreateSetComparer());
                        }
                    }
                }
            }
            return false;
        }
    }
}
