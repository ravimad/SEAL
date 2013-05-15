using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{
    public class CallType
    {
        public bool? isCallback = null;        
        public bool? hasTargets = null;
        public bool? isResolvable = null;
        public bool? stubbed = null;
        public bool? typehierarchy = null;
    }

    [Serializable]
    public abstract class Call : ISerializable
    {                
        public List<VariableHeapVertex> param;
        public VariableHeapVertex ret = null;
        public uint contextid;
        public List<string> callingMethodnames = null;        

        public Call(uint cid, List<string> callingmethods)
        {
            param = new List<VariableHeapVertex>();
            contextid = cid;
            this.callingMethodnames = callingmethods;
        }        

        public Call(SerializationInfo info, StreamingContext context)
        {
            var paramlist = (List<HeapNodeWrapper>)info.GetValue("paramlist", typeof(List<HeapNodeWrapper>));
            var retwrap = (HeapNodeWrapper)info.GetValue("ret",typeof(HeapNodeWrapper));

            param = new List<VariableHeapVertex>();
            foreach (var p in paramlist)
                param.Add(p.GetNode() as VariableHeapVertex);
            if(retwrap != null)
                ret = retwrap.GetNode() as VariableHeapVertex;            
            contextid = (uint)info.GetValue("cid", typeof(uint));

            try
            {
                this.callingMethodnames = (List<string>)info.GetValue("callingmethods", typeof(List<string>));
            }
            catch (SerializationException se)
            {
                //cannot find calling methodnames take corrective action
                this.callingMethodnames = new List<string>();
            }
        }

        public virtual void AddParam(int index, VariableHeapVertex v)
        {
            param.Insert(index,v);
        }

        public void AddReturnValue(VariableHeapVertex v)
        {
            //Contract.Assert(v != null);
            ret = v;
        }

        public bool HasReturnValue()
        {
            return (ret != null);
        }

        public VariableHeapVertex GetReturnValue()
        {
            //Contract.Assert(ret != null);
            return ret;
        }

        public int GetParamCount()
        {
            return param.Count;
        }

        public VariableHeapVertex GetParam(int i)
        {
            return param.ElementAt(i);
        }

        public IEnumerable<VariableHeapVertex> GetAllParams()
        {
            return param;
        }

        public override int GetHashCode()
        {
            return (this.GetType().GetHashCode() ^ this.param.Count);
        }

        public override bool Equals(Object obj)
        {
            if (obj is Call)
            {
                var call = obj as Call;
                if (this.param.SequenceEqual(call.param))
                    return true;
            }
            return false;
        }

        public virtual IEnumerable<HeapVertexBase> GetReferredVertices()
        {
            foreach (var p in GetAllParams())
                yield return p;
            if (HasReturnValue())
                yield return GetReturnValue();
        }

        public virtual void ShallowCopyParams(Call c)
        {        
            c.param = this.param;
            c.ret = this.ret;
        }

        public abstract Call ShallowClone();

        #region ISerializable Members

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var paramlist = new List<HeapNodeWrapper>();
            foreach (var p in param)
            {
                paramlist.Add(new HeapNodeWrapper(p));
            }
            info.AddValue("paramlist", paramlist);
            if(ret != null)
                info.AddValue("ret", new HeapNodeWrapper(this.ret));
            else
                info.AddValue("ret", null);
            info.AddValue("cid", contextid);

            info.AddValue("callingmethods", callingMethodnames);
        }

        #endregion
    }

    public interface CallWithMethodName
    {
        string GetDeclaringType();
        string GetMethodName();
        string GetSignature();
    }

    [Serializable]
    public class StaticCall : Call,CallWithMethodName
    {
        public string signature;
        public string declaringType;
        public string methodname;        

        public StaticCall(uint cid, string mname, string dtype, string sig, List<string> callingmethods)
            : base(cid, callingmethods)
        {
            methodname = mname;
            declaringType = dtype;
            signature = sig;            
        }

        public StaticCall(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            signature = (string)info.GetValue("signature", typeof(string));
            declaringType = (string)info.GetValue("declaringtype", typeof(string));
            methodname = (string)info.GetValue("methodname", typeof(string));
        }

        public string GetSignature() { return signature; }
        public string GetMethodName() { return methodname; }
        public string GetDeclaringType() { return declaringType; }

        public override int GetHashCode()
        {
            return ((this.methodname.GetHashCode() << 5) ^ base.GetHashCode());
        }

        public override bool Equals(Object obj)
        {
            if (obj is StaticCall)
            {
                var scall = obj as StaticCall;
                if (methodname.Equals(scall.methodname)
                    && signature.Equals(scall.signature)
                    && declaringType.Equals(scall.declaringType)
                    && base.Equals(scall))
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return (declaringType + "::" + methodname + " Type: " + signature);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("signature", signature, typeof(string));
            info.AddValue("declaringtype", declaringType, typeof(string));
            info.AddValue("methodname", methodname, typeof(string));
        }

        public override Call ShallowClone()
        {
            var scall = new StaticCall(this.contextid, this.methodname, this.declaringType, this.signature, this.callingMethodnames);
            base.ShallowCopyParams(scall);
            return scall;
        }      
    }

    [Serializable]   
    public class VirtualCall : Call,CallWithMethodName
    {        
        public string signature;
        public string declaringtype;
        public string methodname;

        public VirtualCall(uint cid, string mname, string sig, string dtype, List<string> callingmethods)
            : base(cid, callingmethods)
        {
            if (cid == 0)
                throw new NotSupportedException("cid is zero for " + dtype + "::" + mname + "/" + sig);
            methodname = mname;
            signature = sig;
            declaringtype = dtype; 
        }

        public VirtualCall(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            signature = (string)info.GetValue("signature", typeof(string));
            declaringtype = (string)info.GetValue("declaringtype", typeof(string));
            methodname = (string)info.GetValue("methodname", typeof(string));
        }

        /// <summary>
        /// The first vertex is implicity assumed to be the receiver
        /// </summary>
        /// <returns></returns>
        public VariableHeapVertex GetReceiver()
        {
            return this.param.First();
        }

        public string GetDeclaringType() { return declaringtype; }
        public string GetSignature() { return signature; }
        public string GetMethodName() { return methodname; }

        public override int GetHashCode()
        {
            return ((this.methodname.GetHashCode() << 5) ^ base.GetHashCode());
        }

        public override bool Equals(Object obj)
        {
            if (obj is VirtualCall)
            {
                var vcall = obj as VirtualCall;
                if (methodname.Equals(vcall.methodname)
                    && signature.Equals(vcall.signature)                    
                    && base.Equals(vcall))
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return (declaringtype + "::" + methodname + " Type: " + signature);
        }

        public override Call ShallowClone()
        {
            var vcall = new VirtualCall(this.contextid, this.methodname, this.signature, this.declaringtype, this.callingMethodnames);
            base.ShallowCopyParams(vcall);
            return vcall;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("signature", signature, typeof(string));
            info.AddValue("declaringtype", declaringtype, typeof(string));
            info.AddValue("methodname", methodname, typeof(string));
        }
    }

    [Serializable]
    public class DelegateCall : Call
    {        
        public string signature;
        public VariableHeapVertex target;

        public DelegateCall(uint cid, VariableHeapVertex tgt, string sig, List<string> callingmethods)
            : base(cid, callingmethods)
        {
            target = tgt;
            signature = sig;
        }

        public DelegateCall(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            signature = (string)info.GetValue("signature", typeof(string));
            var targetWrap = (HeapNodeWrapper)info.GetValue("target", typeof(HeapNodeWrapper));
            target = (VariableHeapVertex)targetWrap.GetNode();            
        }

        public string GetSignature() { return signature; }
        
        public VariableHeapVertex GetTarget() {
            return target;
        }

        public override int GetHashCode()
        {
            return ((this.target.GetHashCode() << 5) ^ base.GetHashCode());
        }

        public override bool Equals(Object obj)
        {
            if (obj is DelegateCall)
            {
                var dcall = obj as DelegateCall;
                if (target.Equals(dcall.target)                    
                    && base.Equals(dcall))
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            //return "Delegate Call: "+GetTarget().ToString() +"/"+ signature;
            return "Delegate Call: " + signature;
        }

        public override IEnumerable<HeapVertexBase> GetReferredVertices()
        {
            foreach(var v in  base.GetReferredVertices())
                yield return v;
            yield return target;
        }

        public override Call ShallowClone()
        {
            var dcall = new DelegateCall(this.contextid, this.target, this.signature, this.callingMethodnames);
            base.ShallowCopyParams(dcall);
            return dcall;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("signature", signature, typeof(string));
            info.AddValue("target", new HeapNodeWrapper(target), typeof(HeapNodeWrapper));            
        }
    }
}
