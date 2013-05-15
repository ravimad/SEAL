using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
using SafetyAnalysis.Purity;
using SafetyAnalysis.Purity.Summaries;
using Phx.Types;
using Phx.Symbols;

namespace SafetyAnalysis.TypeUtil
{
    public abstract class TypeInfo
    {
        private static int GUID = 17;
        private int Id;        
        
        public CombinedTypeHierarchy Their { get; private set; }               

        protected TypeInfo(CombinedTypeHierarchy th)
        {
            this.Id = GUID++;
            this.Their = th;
        }
               
        public override bool Equals(object obj)
        {
            return (this == obj);
        }
        
        public override int GetHashCode()
        {
            return this.Id;
        }

        public abstract bool HasInfo();

        public abstract bool IsInterface();

        public abstract string GetTypeName();

        public abstract IEnumerable<MethodInfo> GetMethodInfos(string mname,string msig);        
    }

    public class InternalTypeInfo : TypeInfo
    {        
        public AggregateType Aggtype { get;  private set; }
        public string Typename { get; private set; }
        public List<MethodInfo> methodinfos = null;

        public static TypeInfo New(AggregateType aggty, string name, CombinedTypeHierarchy their)
        {
            return new InternalTypeInfo(aggty, name, their);
        }

        private InternalTypeInfo(AggregateType aggty, string name, CombinedTypeHierarchy their) 
            : base(their)
        {            
            Aggtype = aggty;
            Typename = name;
        }

        public override bool IsInterface()
        {
            return Aggtype.IsOnlyInterface;
        }

        public override IEnumerable<MethodInfo> GetMethodInfos(string mname, string msig)
        {
            if (methodinfos == null)
                PopulateMethods();

            //select the methodinfos with compatible signatures and returns them
            foreach (var minfo in methodinfos)
            {
                if (minfo.Methodname.Equals(mname))
                {
                    if (PhxUtil.AreSignaturesCompatible(minfo.Sig, msig))
                        yield return minfo;
                }
            }            
        }

        private void PopulateMethods()
        {
            methodinfos = new List<MethodInfo>();
            foreach (FunctionSymbol unnormalizedFunc in Aggtype.FunctionSymbols)
            {
                FunctionSymbol func = PhxUtil.NormalizedFunctionSymbol(unnormalizedFunc);
                methodinfos.Add(InternalMethodInfo.New(func, this));   
            }
        }

        public override string GetTypeName()
        {
            return Typename;
        }

        public void Serialize(Phx.PEModuleUnit moduleUnit, PurityDBDataContext dbContext)
        {
            if (this.Typename.Length >= 200)
            {
                throw new NotSupportedException("Cannot serialize type: Type length: " + 
                    this.Typename.Length);
            }

            string dllname = moduleUnit.Manifest.Name.NameString;            

            //serialize the type properties
            var typeinfo = new SafetyAnalysis.Purity.TypeInfo();
            typeinfo.dllname = dllname;
            typeinfo.typename = this.Typename;
            typeinfo.IsInterface = Aggtype.IsOnlyInterface;

            dbContext.TypeInfos.InsertOnSubmit(typeinfo);            
  
            //serialize methods
            //foreach (FunctionSymbol func in Aggtype.FunctionSymbols)
            //{
            //    //ignore instantiations
            //    if (func.UninstantiatedFunctionSymbol == null)
            //    {
            //        var methodinfo = InternalMethodInfo.New(func);
            //        methodinfo.Serialize(moduleUnit, dbContext);
            //    }
            //}
            if (methodinfos == null)
                PopulateMethods();

            foreach (var methodinfo in methodinfos)
            {
                if (methodinfo is InternalMethodInfo)
                {
                    var intmeth = methodinfo as InternalMethodInfo;
                    var func = intmeth.GetFunctionSymbol();
                    //ignore instantiations
                    if (func.UninstantiatedFunctionSymbol == null)
                    {                        
                        intmeth.Serialize(moduleUnit, dbContext);
                    }
                }
            }
        }

        public override bool HasInfo()
        {
            return true;
        }
    }

    public class ExternalTypeInfo : TypeInfo
    {
        public string Typename { get; private set; }
        List<ExternalMethodInfo> methodInfos = null;

        //properties of the type
        bool populatedProperties = false;
        bool isInterface = false;
        bool? hasInfo = null;

        protected static Dictionary<string, ExternalTypeInfo> ExternalTypeTable = new Dictionary<string, ExternalTypeInfo>();
        public static ExternalTypeInfo New(string name, CombinedTypeHierarchy their)
        {
            if (ExternalTypeTable.ContainsKey(name))
            {
                var existingTypeInfo = ExternalTypeTable[name];
                var newTypeInfo = new ExternalTypeInfo(name, their);               
                //copy the rest of the information
                existingTypeInfo.Copy(newTypeInfo);
                return newTypeInfo;
            }
            else
            {
                var typeinfo = new ExternalTypeInfo(name, their);
                ExternalTypeTable.Add(name, typeinfo);
                return typeinfo;
            }
        }

        private void Copy(ExternalTypeInfo typeinfo)
        {
            typeinfo.populatedProperties = this.populatedProperties;
            typeinfo.isInterface = this.isInterface;
            typeinfo.hasInfo = this.hasInfo;

            //copy the method infos
            if (this.methodInfos != null)
            {
                typeinfo.methodInfos = new List<ExternalMethodInfo>();
                foreach (var minfo in this.methodInfos)
                {
                    var newMethodInfo = ExternalMethodInfo.New(minfo.DTypename, minfo.Methodname, minfo.Sig, minfo.IsVirtual(),
                        minfo.IsAbstract(), minfo.IsInstance(), typeinfo);
                    minfo.CopySummary(newMethodInfo);
                    typeinfo.methodInfos.Add(newMethodInfo);
                }
            }        
        }        

        private ExternalTypeInfo(string tname, CombinedTypeHierarchy their)
            : base(their)
        {
            Typename = tname;            
        }

        private void PopulateProperties()
        {            
            if (PurityAnalysisPhase.DisableExternalCallResolution)
            {
                hasInfo = false;
                populatedProperties = true;
                return;
            }
            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();            

            //reads the properties of the type from the database and stores it            
            PurityDBDataContext dbcontext = PurityAnalysisPhase.DataContext;
            var records = from record in dbcontext.TypeInfos
                          where record.typename.Equals(Typename)
                          select record;

            if (!records.Any())
            {
                hasInfo = false;
            }
            else
            {
                hasInfo = true;
                foreach (var record in records)
                {
                    this.isInterface = record.IsInterface;
                }
            }
            
            populatedProperties = true;
            dbcontext.Dispose();

            sw.Stop();
            if (PurityAnalysisPhase.EnableStats)            
                MethodLevelAnalysis.dbaccessTime += sw.ElapsedMilliseconds;                         
        }

        public override bool IsInterface()
        {
            if (!populatedProperties)
                PopulateProperties();

            return isInterface;
        }

        private void PopulateMethods()
        {
            methodInfos = new List<ExternalMethodInfo>();

            if (PurityAnalysisPhase.DisableExternalCallResolution)
                return;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();    

            //reads the set of all method of this type from the database and stores it
            PurityDBDataContext dbcontext = PurityAnalysisPhase.DataContext;
            var records = from record in dbcontext.MethodInfos
                          where record.typename.Equals(Typename)
                          select record;
            foreach (var record in records)
            {
                methodInfos.Add(ExternalMethodInfo.New(
                    record.typename,
                    record.methodname,
                    record.methodSignature,
                    record.IsVirtual,
                    record.IsAbstract,
                    record.IsInstance, 
                    this));
            }
            dbcontext.Dispose();

            sw.Stop();
            if (PurityAnalysisPhase.EnableStats)
                MethodLevelAnalysis.dbaccessTime += sw.ElapsedMilliseconds;
        }

        public override IEnumerable<MethodInfo> GetMethodInfos(string mname, string msig)
        {
            if (HasInfo())
            {
                if (methodInfos == null)
                    PopulateMethods();
                //Console.WriteLine("Getting method info for: " + typename + "::" + mname + "/" + msig);

                //select the methodinfos with compatible signatures and returns them
                foreach (var minfo in methodInfos)
                {
                    if (minfo.Methodname.Equals(mname))
                    {
                        if (PhxUtil.AreSignaturesCompatible(minfo.Sig, msig))
                            yield return minfo;
                    }
                }
            }
            else
            {
                //this could be a stub method                                
                bool found = false;
                if (methodInfos != null)
                {
                    foreach (var minfo in methodInfos)
                    {
                        if (minfo.Methodname.Equals(mname))
                        {
                            if (PhxUtil.AreSignaturesCompatible(minfo.Sig, msig))
                            {
                                found = true;
                                yield return minfo;
                            }
                        }
                    }
                }
                if (!found)
                {
                    var methodinfo = ExternalMethodInfo.New(Typename, mname, msig, false, false, false, this);
                    if (methodInfos == null)
                        methodInfos = new List<ExternalMethodInfo>();
                    methodInfos.Add(methodinfo);
                    yield return methodinfo;
                    yield break;
                }                                                
            }
        }

        public override string GetTypeName()
        {
            return Typename;
        }

        public override bool HasInfo()
        {
            if (!populatedProperties)
            {
                PopulateProperties();
            }
            return hasInfo.Value;
        }
    }
}
