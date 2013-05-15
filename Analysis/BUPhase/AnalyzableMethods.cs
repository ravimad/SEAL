using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SafetyAnalysis.Util;
using SafetyAnalysis.TypeUtil;

namespace SafetyAnalysis.Purity
{
    public class AnalyzableMethods
    {
        public enum Categories { NAMESPACE, CLASS, SUBCLASS_IN_NS};
        public static Dictionary<Categories, HashSet<string>> analyzableList = null;

        public static void Initialize(string filename)
        {
            analyzableList = new Dictionary<Categories, HashSet<string>>();
            analyzableList.Add(Categories.NAMESPACE, new HashSet<string>());
            analyzableList.Add(Categories.CLASS, new HashSet<string>());
            analyzableList.Add(Categories.SUBCLASS_IN_NS, new HashSet<string>()); 

            StreamReader reader = new StreamReader(new FileStream(PurityAnalysisPhase.sealHome + filename, 
                FileMode.Open, FileAccess.Read, FileShare.Read));
            while (!reader.EndOfStream)
            {
                string entry = reader.ReadLine();
                string[] fields = entry.Split(':');
                if (fields.Length != 2)
                    throw new FormatException("Entry: " + entry + " in file " + filename + " has invalid format");

                HashSet<string> elements;
                analyzableList.TryGetValue(StringToCategory(fields[0]), out elements);
                elements.Add(fields[1].Trim());
            }
        }

        private static Categories StringToCategory(string cat)
        {
            if (cat.ToLower().Equals("namespace"))
                return Categories.NAMESPACE;
            else if (cat.ToLower().Equals("class"))
                return Categories.CLASS;
            else if (cat.ToLower().Equals("subclass_in_ns"))
                return Categories.SUBCLASS_IN_NS;            
            else
                throw new NotSupportedException("invalid category: " + cat);
        }

        public static bool IsAnalyzable(Phx.Types.AggregateType aggtype, Phx.PEModuleUnit moduleunit)
        {
            if (analyzableList == null)
                return true;

            //check if the containing namespace can be analyzed
            {
                string ns = PhxUtil.GetContainingNamespace(aggtype);
                //assuming that no namespace implies that the method can be analyzed.
                if (ns == null)
                    return true;

                HashSet<string> analyzableNamespaces;
                analyzableList.TryGetValue(Categories.NAMESPACE, out analyzableNamespaces);
                if ((ns != null) && analyzableNamespaces.Contains(ns))
                    return true;
            }

            //check if the containing class can be analyzed.
            {
                Phx.Types.AggregateType outermostType = PhxUtil.GetOutermostType(aggtype);
                if (outermostType == null)
                    return true;

                HashSet<string> analyzableClasses;
                analyzableList.TryGetValue(Categories.CLASS, out analyzableClasses);
                if (analyzableClasses.Contains(outermostType.TypeSymbol.NameString))
                    return true;
            }

            //check for base types in namespace
            {                
                string ns = PhxUtil.GetContainingNamespace(aggtype);
                Phx.Types.AggregateType outermostType = PhxUtil.GetOutermostType(aggtype);

                HashSet<string> analyzableBasetypes;
                analyzableList.TryGetValue(Categories.SUBCLASS_IN_NS, out analyzableBasetypes);

                if (analyzableBasetypes.Any())
                {
                    var typename = PhxUtil.GetTypeName(outermostType);
                    if (analyzableBasetypes.Contains(typename))
                        return true;

                    var th = CombinedTypeHierarchy.GetInstance(moduleunit);
                    var typeinfo = th.LookupTypeInfo(typename);
                    if (typeinfo is InternalTypeInfo)
                    {
                        foreach (var baseTypeinfo in th.GetSuperTypesFromTypeHierarhcy(typeinfo).OfType<InternalTypeInfo>())
                        {
                            if (analyzableBasetypes.Contains(baseTypeinfo.GetTypeName()))
                            {
                                string basens = PhxUtil.GetContainingNamespace(baseTypeinfo.Aggtype);
                                if (basens != null && basens.Equals(ns))
                                    return true;
                            }
                        }
                    }                    
                }
            }
            return false;
        }

        public static bool IsAnalyzable(Phx.Symbols.FunctionSymbol funcSym)
        {            
            if (funcSym.FunctionUnit == null
                || funcSym.UninstantiatedFunctionSymbol != null)         
                return false;

            var moduleunit = funcSym.FunctionUnit.ParentPEModuleUnit;
            if (IsAnalyzable(funcSym.EnclosingAggregateType, moduleunit))
                return true;
            return false;
        }
    }
}
