using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

using SafetyAnalysis.Util;

namespace SafetyAnalysis.Framework.Graphs
{
    using method = Triple<string,string,string>;
    
    public class MethodHeapVertex: HeapVertexBase
    {
        public string typename;
        public string methodname;
        public string signature;
        //dependent properties
        public bool IsVirtual;
        public bool IsInstance;        
        //int hashCode = -1;

        private static Dictionary<method, MethodHeapVertex> MethodTable
            = new Dictionary<method, MethodHeapVertex>();

        public static MethodHeapVertex New(
            string enclosingTypename, 
            string mname, 
            string sig,
            bool IsInst, 
            bool IsVirt)
        {
            var method = new method(enclosingTypename, mname, sig);
            if (MethodTable.ContainsKey(method))
                return MethodTable[method];
            else
            {
                var vertex = new MethodHeapVertex(enclosingTypename, mname, sig, IsInst, IsVirt);
                MethodTable.Add(method, vertex);
                return vertex;
            }
        }

        private MethodHeapVertex(string enclosingTypename, string mname, string sig,
            bool IsInst, bool IsVirt)            
        {
            typename = enclosingTypename;
            methodname = mname;
            signature = sig;
            IsInstance = IsInst;
            IsVirtual = IsVirt;            
        }

        public static MethodHeapVertex Create(List<Pair<string,Object>> info)
        {          
            var pair = info[0];
            if (!pair.Key.Equals("typename"))
                throw new NotSupportedException("missing property typename");            
            string typename = (string)pair.Value;
            
            pair = info[1];
            if (!pair.Key.Equals("methodname"))
                throw new NotSupportedException("missing property methodname");            
            var methodname = (string)pair.Value;

            pair = info[2];
            if (!pair.Key.Equals("signature"))
                throw new NotSupportedException("missing property signature");            
            var signature = (string)pair.Value;

            pair = info[3];
            if (!pair.Key.Equals("isinstance"))
                throw new NotSupportedException("missing property isinstance");            
            bool IsInstance = (bool)pair.Value;

            pair = info[4];
            if (!pair.Key.Equals("isvirtual"))
                throw new NotSupportedException("missing property isvirtual");            
            bool IsVirtual = (bool)pair.Value;

            return MethodHeapVertex.New(typename,methodname,signature,IsInstance,IsVirtual);
        }

        //public override bool Equals(object obj)
        //{
        //    var vertex = obj as MethodHeapVertex;

        //    if (vertex == null)
        //        return false;

        //    if (!this.signature.Equals(vertex.signature)
        //        || !this.methodname.Equals(vertex.methodname)
        //        || !this.typename.Equals(vertex.typename))
        //    {
        //        Contract.Assert(this.IsInstance == vertex.IsInstance);
        //        Contract.Assert(this.IsVirtual == vertex.IsVirtual);
        //        return false;
        //    }
        //    return true;
        //}

        //public override int GetHashCode()
        //{
        //    if(hashCode == -1)
        //        hashCode =  (this.GetType().GetHashCode() << 15) ^ 
        //                    (typename.GetHashCode() ^ methodname.GetHashCode() ^ signature.GetHashCode());
        //    return hashCode;
        //}

        public override HeapVertexBase Copy()
        {
            return this;
        }

        public override string ToString()
        {
            return (typename + "::" + methodname + " Type: " + signature);
        }

        public override void GetObjectData(List<Pair<string,Object>> info)
        {
            info.Add(new Pair<string,Object>("typename", this.typename));
            info.Add(new Pair<string,Object>("methodname", this.methodname));
            info.Add(new Pair<string,Object>("signature", this.signature));
            info.Add(new Pair<string,Object>("isinstance", this.IsInstance));
            info.Add(new Pair<string,Object>("isvirtual", this.IsVirtual));
        }        
    }
}
