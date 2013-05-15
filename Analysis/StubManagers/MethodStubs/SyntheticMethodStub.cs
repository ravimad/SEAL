using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;
using SafetyAnalysis.TypeUtil;
using SafetyAnalysis.Purity;

namespace SafetyAnalysis.Purity.Summaries
{
    public class SyntheticMethodStub : MethodStubManager
    {
        private static SyntheticMethodStub _instance = null;

        public static SyntheticMethodStub GetInstance()
        {
            if(_instance == null)
                _instance = new SyntheticMethodStub();
            return _instance;
        }

        private SyntheticMethodStub() { }

        public bool HasSummary(TypeUtil.MethodInfo methodinfo)
        {            
            string qualtypename = methodinfo.DTypename;
            string methodname = methodinfo.Methodname;
            string signature = methodinfo.Sig;
            var typename = PhxUtil.RemoveAssemblyName(qualtypename);

            if (typename.Equals("System.String"))
                return true;
            if (typename.Equals("System.Convert"))
                return true;
            if (typename.Equals("System.Array")
                && methodname.Equals("CreateInstance"))            
                return true;
            //if (typename.Equals("System.Math"))
            //    return true;
            if ( typename.Equals("System.Guid"))
                return true;
            if (typename.Equals("System.Object") &&  methodname.Equals("ctor"))                
                return true;
            if (typename.Equals("System.Object") &&  methodname.Equals("GetType"))
                return true;
            if (typename.Equals("System.Object") &&  methodname.Equals("MemberwiseClone"))
                return true;
            if (typename.Equals("System.Object") &&  methodname.Equals("Equals"))
                return true;
            if (typename.Equals("System.Object") &&  methodname.Equals("GetHashCode"))
                return true;
            if (typename.Equals("System.Object") &&  methodname.Equals("ToString"))
                return true;
            if (methodname.Equals("ctor") && 
                TypeMethodInfoUtil.IsDelegateType(methodinfo.Typeinfo.Their, methodinfo.Typeinfo))
                return true;            
            if (typename.Equals("System.ValueType"))
                return true;
            if (typename.Equals("System.Enum"))
                return true;
            if (typename.Equals("System.Type"))
                return true;            
            if (typename.Equals("System.Text.StringBuilder"))
                return true;
            if (typename.Equals("System.TimeZone"))
                return true;            
            if (typename.Contains("System.SR") &&  methodname.Equals("GetString"))
                return true;
            if (typename.Contains("System.Globalization"))
                return true;
            if ( typename.Contains("System.IO"))
                return true;
            if ( typename.Contains("System.Reflection.Emit"))
                return true;
            if (typename.Contains("System.Runtime.Serialization"))
                return true;            
            if ( typename.Contains("System.Threading")
                && !typename.Contains("System.Threading.Interlocked"))
                return true;
            if ( typename.Equals("System.Reflection.FieldInfo")
                ||  typename.Equals("System.Reflection.PropertyInfo")
                ||  typename.Equals("System.Reflection.MemberInfo")
                ||  typename.Equals("System.Reflection.MethodInfo")
                ||  typename.Equals("System.Reflection.MethodBase")
                ||  typename.Equals("System.Reflection.ParameterInfo")
                ||  typename.Equals("System.Reflection.ConstructorInfo")
                ||  typename.Equals("System.Reflection.FieldInfo")                
                )
                return true;     
            return false;
        }

        public override PurityAnalysisData GetSummary(TypeUtil.MethodInfo methodinfo)
        {
            string qualtypename = methodinfo.DTypename;
            string methodname = methodinfo.Methodname;
            string sig = methodinfo.Sig;
            var typename = PhxUtil.RemoveAssemblyName(qualtypename);
            var qualifiedname = typename + "::" + methodname;

            var pureData = SummaryTemplates.CreatePureData();

            //if (typename.Equals("System.Math"))
            //{
            //    //skip. all methods are pure.
            //    return pureData;
            //}
            //else 
                if (typename.Equals("System.Array") && methodname.Equals("CreateInstance"))
            {
                var outData = SummaryTemplates.CreatePureData();
                SummaryTemplates.MakeAllocator(outData, "[mscorlib]System.Array", methodname);
                return outData;
            }
            else if (typename.Equals("System.String"))
            {
                //this is a string method which is a pure functional implementation
                //so create a new object if the destination operand is a pointer operand                
                var outData = SummaryTemplates.CreatePureData();
                SummaryTemplates.MakeAllocator(outData, "[mscorlib]System.String", methodname);
                return outData;
            }
            else if (typename.Equals("System.Convert"))
            {
                var outData = SummaryTemplates.CreatePureData();
                SummaryTemplates.MakeAllocator(outData, "[mscorlib]System.String", methodname);
                return outData;
            }
            else if (typename.Equals("System.Enum"))
            {
                //system.Enum is pure.                
                return pureData;
            }
            else if (typename.Equals("System.Guid"))
            {
                //system.Guid is pure.                
                return pureData;
            }
            else if (typename.Equals("System.Type"))
            {
                if (methodname.Equals("InvokeMember"))
                {
                    //pollute the global object
                    var outData = SummaryTemplates.CreatePureData();
                    SummaryTemplates.WriteGlobalObject(outData, NamedField.New("InvokeMember", "[mscorlib]System.Type"));
                    return outData;
                }
                else
                {
                    //all other calls are pure (observationally).
                    return pureData;
                }
            }
            else if (typename.Equals("System.Text.StringBuilder"))
            {
                return pureData;
            }
            else if (typename.Equals("System.TimeZone"))
            {
                return pureData;
            }
            else if (typename.Equals("System.Object") && methodname.Equals("ctor"))
            {
                return pureData;
            }
            else if (typename.Equals("System.Object") && methodname.Equals("GetType"))
            {
                return pureData;
            }
            else if (typename.Equals("System.Object") && methodname.Equals("MemberwiseClone"))
            {
                //make return value point-to this vertex (over-approximation)
                var outData = SummaryTemplates.CreatePureData();
                var thisVertex = ParameterHeapVertex.New(1, "this");
                var retVertex = ReturnVertex.GetInstance();

                outData.OutHeapGraph.AddVertex(thisVertex);
                outData.OutHeapGraph.AddVertex(retVertex);

                var edge = new InternalHeapEdge(retVertex, thisVertex, null);
                outData.OutHeapGraph.AddEdge(edge);
                return outData;
            }
            else if (typename.Equals("System.Object") && methodname.Equals("Equals"))
            {
                //pure
                return pureData;
            }
            else if (typename.Equals("System.Object") && methodname.Equals("GetHashCode"))
            {
                //pure
                return pureData;
            }
            else if (typename.Equals("System.Object") && methodname.Equals("ToString"))
            {
                //supposed to return a newly created string object and also be pure.
                var outData = SummaryTemplates.CreatePureData();
                SummaryTemplates.MakeAllocator(outData, "[mscorlib]System.String", methodname);
                return outData;
            }
            else if (methodname.Equals("ctor") 
                && TypeMethodInfoUtil.IsDelegateType(methodinfo.Typeinfo.Their, methodinfo.Typeinfo))
            {
                //supposed to return a newly created delegate and also be pure.
                var outData = SummaryTemplates.CreatePureData();
                SummaryTemplates.MakeAllocator(outData, qualtypename, methodname);
                return outData;
            }
            else if (typename.Equals("System.SR") && methodname.Equals("GetString"))
            {
                //consider this as pure
                return pureData;
            }
            else if (typename.Equals("System.ValueType"))
            {
                //all the methods here are supposed to be pure.
                return pureData;
            }
            else if (typename.Contains("System.Globalization"))
            {
                //consider all the methods as pure.
                return pureData;
            }
            else if (typename.Contains("System.IO"))
            {
                var outData = SummaryTemplates.CreatePureData();
                SummaryTemplates.MakeAllocator(outData, "[mscorlib]System.String", methodname);
                SummaryTemplates.WriteGlobalObject(outData, NamedField.New(qualifiedname, null));
                return outData;
            }
            else if (typename.Contains("System.Reflection.Emit"))
            {
                var outData = SummaryTemplates.CreatePureData();
                SummaryTemplates.WriteGlobalObject(outData, NamedField.New(qualifiedname, null));
                return outData;
            }
            else if (typename.Contains("System.Runtime.Serialization"))
            {
                var outData = SummaryTemplates.CreatePureData();
                SummaryTemplates.WriteGlobalObject(outData, NamedField.New(qualifiedname, null));
                return outData;
            }
            else if (typename.Contains("System.Threading")
                && !typename.Contains("System.Threading.Interlocked"))
            {
                if (methodname.Contains("get_"))
                {
                    //consider this as pure.  
                    return pureData;
                }
                else
                {
                    //considering all the methods here as impure.
                    var outData = SummaryTemplates.CreatePureData();
                    SummaryTemplates.WriteGlobalObject(outData, NamedField.New(qualifiedname, null));
                    return outData;
                }
            }
            else if ((typename.Equals("System.Reflection.FieldInfo")
                || typename.Equals("System.Reflection.PropertyInfo")
                || typename.Equals("System.Reflection.MemberInfo")
                || typename.Equals("System.Reflection.MethodInfo")
                || typename.Equals("System.Reflection.MethodBase")
                || typename.Equals("System.Reflection.ParameterInfo")
                || typename.Equals("System.Reflection.ConstructorInfo")
                || typename.Equals("System.Reflection.FieldInfo")
                ))
            {
                if (methodname.Contains("Get") || methodname.Contains("get_"))
                {
                    //considered as pure
                    return pureData;
                }
                else if (methodname.Contains("Set")
                    || methodname.Contains("set_")
                    || methodname.Contains("Invoke"))
                {
                    //pollute the global object
                    var outData = SummaryTemplates.CreatePureData();
                    SummaryTemplates.WriteGlobalObject(outData, NamedField.New(qualifiedname, null));
                    return outData;
                }
                else
                {
                    //assume all other calls are pure (observationally). These are calls like MakeGeneric..                    
                    return pureData;
                }
            }
            else
            {
                if (this.HasSummary(methodinfo))
                    throw new SystemException("Cannot construct summary for predefined method: " + qualifiedname);
                else
                    throw new SystemException(qualifiedname + " is  not a predefined method");
            }
        }
    }
}
