using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.TypeUtil
{
    public class TypeMethodInfoUtil
    {
        public static bool IsVirtualCall(Phx.IR.CallInstruction callInst)
        {            
            if (callInst.Opcode.NameString.Trim().ToLower().Equals("callvirt"))
            {                
                var funcSym = PhxUtil.NormalizedFunctionSymbol(callInst.FunctionSymbol);
                var mname = PhxUtil.GetFunctionName(funcSym);
                var sig = PhxUtil.GetFunctionTypeSignature(funcSym.FunctionType);
                var dtype = PhxUtil.GetTypeName(funcSym.EnclosingAggregateType);
                var moduleunit = callInst.FunctionUnit.ParentPEModuleUnit;

                var typeinfo = CombinedTypeHierarchy.GetInstance(moduleunit).LookupTypeInfo(dtype);                
                var methodinfos = typeinfo.GetMethodInfos(mname, sig);
                if (!methodinfos.Any())
                    return false;

                foreach (var methodinfo in methodinfos)
                {
                    if (methodinfo.IsVirtual())
                        return true;
                }
                return false;
            }            
            return false;
        }

        /// <summary>
        /// Caution use this judiciously, looks up the whole typehierarhcy
        /// </summary>
        /// <param name="th"></param>
        /// <param name="typeinfo"></param>
        /// <param name="baseTypename"></param>
        /// <returns></returns>
        public static bool HasBaseTypeContainingTypename(CombinedTypeHierarchy th, TypeInfo typeinfo, string baseTypename)
        {
            var suptypes = th.GetSuperTypesFromTypeHierarhcy(typeinfo);
            foreach (var suptype in suptypes)
            {
                if (suptype.GetTypeName().Contains(baseTypename))
                    return true;
            }
            return false;
        }
        
        public static bool IsDelegateType(CombinedTypeHierarchy th, TypeInfo typeinfo)
        {
            return th.IsDelegateType(typeinfo);
        }

        public static bool IsDelegateCall(Phx.IR.CallInstruction callInstruction)
        {
            var moduelunit = callInstruction.FunctionUnit.ParentPEModuleUnit;
            var th = CombinedTypeHierarchy.GetInstance(moduelunit);
            var funcSymbol = PhxUtil.NormalizedFunctionSymbol(callInstruction.FunctionSymbol);
            var enclTypename = PhxUtil.GetTypeName(funcSymbol.EnclosingAggregateType);
            var typeinfo = th.LookupTypeInfo(enclTypename);

            if (!th.IsHierarhcyKnown(typeinfo))
            {                
                return false;
            }

            if (funcSymbol.NameString.Contains("Invoke") && TypeMethodInfoUtil.IsDelegateType(th,typeinfo))
                return true;
            return false;
        }

        public static bool IsYieldReturnType(CombinedTypeHierarchy th, TypeInfo typeinfo, out string yieldreturnMethod)
        {
            if (TypeMethodInfoUtil.HasBaseTypeContainingTypename(th, typeinfo, "System.Collections.IEnumerable") ||
                TypeMethodInfoUtil.HasBaseTypeContainingTypename(th, typeinfo, "System.Collections.IEnumerator"))
            {
                string typename = typeinfo.GetTypeName();
                int startIndex = typename.IndexOf('<');
                int endIndex = typename.IndexOf('>');
                if (startIndex != -1 && endIndex != -1)
                {
                    //find the string between startIndex and endIndex
                    yieldreturnMethod = typename.Substring(startIndex + 1, (endIndex - startIndex) - 1);
                    if (yieldreturnMethod.Length > 0)
                        return true;
                }
            }
            yieldreturnMethod = null;
            return false;
        }
        
    }
}
