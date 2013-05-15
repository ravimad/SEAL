using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

using SafetyAnalysis.Util;
using SafetyAnalysis.TypeUtil;
using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity;

namespace SafetyAnalysis.Purity.Summaries
{    
    using Qualname = Triple<string, string, string>;
    using SigIndex = Dictionary<string, MethodSpecifier>;
    using MethodIndex = Dictionary<string, Dictionary<string, MethodSpecifier>>;
    using TypeIndex = Dictionary<string, Dictionary<string, Dictionary<string, MethodSpecifier>>>;    
    
    public interface StubSpecifier {
        Qualname StubQualifiedName(string actualType, string actualMethod, string actualSig);        
    };

    public class MethodSpecifier : Triple<string,string,string>, StubSpecifier 
    {
        public MethodSpecifier(string typename, string methodname, string signature)
            : base(typename,methodname,signature)
        {            
        }

        public Qualname StubQualifiedName(string actualType, string actualMethod, string actualSig)
        {
            string stubmethod = t2;
            if (t2.Equals(DBStubManager.WildCard))
                stubmethod = actualMethod;

            string stubsig = t3;
            if (t3.Equals(DBStubManager.WildCard))
                stubsig = actualSig;

            return new Qualname(t1, stubmethod, stubsig);
        }
    };

    public class TypeSpecifier : StubSpecifier
    {
        private String stubTypename;

        public TypeSpecifier(string tname)
        {
            stubTypename = tname;
        }

        public Qualname StubQualifiedName(string actualType, string actualMethod, string actualSig)
        {           
            //replace every occurrence of actualType in actualSig by stub typename
            //TODO: need to handle use of other stubbed types in signature.
            var stubsig = actualSig.Replace(actualType, stubTypename);
            return new Qualname(stubTypename, actualMethod, stubsig);
        }
    };    

    public class DBStubManager 
    {
        internal static string WildCard = "*";        
        private TypeIndex stubbedMethods = new TypeIndex();
        private Dictionary<string, TypeSpecifier> stubbedTypes = new Dictionary<string, TypeSpecifier>();
        
        public DBStubManager(string filenameKey)
        {           
            string value;
            if (!PurityAnalysisPhase.properties.TryGetValue(filenameKey, out value))
                return;
           
            StreamReader reader = new StreamReader(new FileStream(PurityAnalysisPhase.sealHome + value, 
                FileMode.Open, FileAccess.Read, FileShare.Read));
            while (!reader.EndOfStream)
            {
                string entry = reader.ReadLine();
                //check if this a stub for an entire type
                if (entry.StartsWith("@Type"))
                {
                    var pair = entry.Substring("@Type".Length).Split('=');
                    var srctype = pair[0].Trim();
                    var desttype = pair[1].Trim();
                    if (stubbedTypes.ContainsKey(srctype))
                        throw new NotSupportedException("Stub for type already exists: "+srctype);
                    stubbedTypes.Add(srctype, new TypeSpecifier(desttype));                    
                }
                else
                {
                    string[] pair;
                    if (entry.StartsWith("@Method"))
                        pair = entry.Substring("@Method".Length).Split('=');
                    else
                    {
                        //By default everything is a method stub.
                        pair = entry.Split('=');
                    }
                    var srcmethod = this.GetMethodSpecifier(pair[0].Trim());
                    var destmethod = this.GetMethodSpecifier(pair[1].Trim());
                    AddToStubIndices(srcmethod, destmethod);
                }
            }
        }

        private void AddToStubIndices(MethodSpecifier src, MethodSpecifier dest)
        {
            var methodMapping = GetOrAddKey<string, MethodIndex>(stubbedMethods, src.t1, new MethodIndex());
            var sigMapping = GetOrAddKey<string, SigIndex>(methodMapping, src.t2, new SigIndex());
            if (sigMapping.ContainsKey(src.t3))
                throw new ArgumentException("Mapping exists for Qualifier: " + src);

            var methodQual = GetOrAddKey<string, MethodSpecifier>(sigMapping, src.t3, dest);            
        }

        private V GetOrAddKey<K, V>(Dictionary<K, V> dic, K key, V val)
        {
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }
            else
            {
                dic.Add(key, val);
                return val;
            }
        }

        private MethodSpecifier GetMethodSpecifier(string qualname)
        {
            var separators = new string[] { "::", "/" };
            var parts = qualname.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if(parts.Length != 3)
                throw new NotSupportedException("the input name does not contain 3 parts");
            var typename = parts[0];
            var methodname = parts[1];
            var signature = parts[2];            
            return new MethodSpecifier(typename, methodname, signature);
        }
        
        /// <summary>
        /// First priority given to exact matches of the full method name.
        /// Second priority to wildcard mathches of method name
        /// Last priority to matching stubs for the declaring type of the method.
        /// </summary>
        /// <param name="typename"></param>
        /// <param name="methodname"></param>
        /// <param name="sig"></param>
        /// <param name="specifier"></param>
        /// <returns></returns>
        public bool TryGetValue(string typename, string methodname, string sig, out StubSpecifier specifier)
        {            
            //Look for stub for the method
            MethodIndex methodMapping;
            if(stubbedMethods.TryGetValue(typename, out methodMapping))
            {
                SigIndex sigMapping;
                if(methodMapping.TryGetValue(methodname, out sigMapping))
                {
                    MethodSpecifier mqual;
                    if (sigMapping.TryGetValue(sig, out mqual))
                    {
                        specifier = mqual;
                        return true;
                    }
                    else if (sigMapping.TryGetValue(DBStubManager.WildCard, out mqual))
                    {
                        specifier = mqual;
                        return true;
                    }
                }
                else if (methodMapping.TryGetValue(DBStubManager.WildCard, out sigMapping))
                {
                    //the sig mapping should have a wild card
                    specifier =  sigMapping[DBStubManager.WildCard];
                    return true;
                }
            }

            //Look for a stub for the contatining type
            TypeSpecifier tspec;
            if (stubbedTypes.TryGetValue(typename, out tspec))
            {
                specifier = tspec;
                return true;
            }

            specifier = null;
            return false;                       
        }

        public bool ExistsStub(string tname, string mname, string sig)
        {
            StubSpecifier specifier;
            return this.TryGetValue(tname, mname, sig, out specifier);
        }

        public PurityAnalysisData RetrieveSummary(string tname,string mname, string sig, CombinedTypeHierarchy th)
        {            
            StubSpecifier stubspec;
            if (this.TryGetValue(tname,mname,sig,out stubspec))
            {
                var qualname = stubspec.StubQualifiedName(tname, mname, sig);
                var stubMethodname = qualname.t1 + "::" + qualname.t2 + "/" + qualname.t3;

                var typeinfo = th.LookupTypeInfo(qualname.t1);                                                
                var stubMethodinfos = typeinfo.GetMethodInfos(qualname.t2, qualname.t3);

                if (!stubMethodinfos.Any())
                {
                    //try including the "this" type in the signature
                    //and I_ to the methodname
                    //var newSig = qualname.t3.Insert(1, qualname.t1 + ",");
                    //var newMname = "I_" + qualname.t2;
                    //stubMethodinfos = typeinfo.GetMethodInfos(newMname, newSig);

                    if(!stubMethodinfos.Any())
                        throw new NotSupportedException("No methodinfos for stub method: " + stubMethodname);
                }
                
                if (stubMethodinfos.Count() > 1)
                    throw new NotSupportedException("More than one summary for stub method: " + stubMethodname);

                var stubMethodinfo = stubMethodinfos.First();

                var summary = stubMethodinfo.ReadSummaryFromStore();
                if (summary == null)
                    throw new NotSupportedException("Null summary for stub method: " + stubMethodname);

                PurityDataUtil.RenameParameter(summary, "thisRef", "this");
                return summary;
            }
            else
                throw new NotSupportedException("No stub Mapping for the method " + tname+"::"+mname+"/"+sig);            
        }         
    }
}
