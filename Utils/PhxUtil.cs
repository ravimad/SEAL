using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phx.Types;
using Phx.Symbols;
using QuickGraph;

namespace SafetyAnalysis.Util
{
    public class PhxUtil
    {           
        /// <summary>
        /// Get the signature of the function type in string format
        /// </summary>
        /// <param name="functionUnit"></param>
        /// <returns></returns>
        public static string GetFunctionTypeSignature(FunctionType functionType)
        {            
            string sig = "(";

            bool first = true;
            foreach (Phx.Types.Parameter parameter in functionType.UserDefinedParameters)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sig += ",";
                }
                AggregateType aggType;
                if (TryGetAggregateType(parameter.Type, out aggType))
                    sig += GetTypeName(NormalizedAggregateType(aggType));
                else
                    sig += GetTypeName(parameter.Type);
            }
            sig += ")";
            //add the return value here.
            //if (functionSymbol.NameString.Equals("op_Implicit")
            //|| (functionSymbol.NameString.Equals("Invoke") &&
            //PhxUtil.IsDelegateType(functionSymbol.EnclosingAggregateType)))
            {
                AggregateType aggType;
                if (TryGetAggregateType(functionType.ReturnType, out aggType))
                    sig += GetTypeName(NormalizedAggregateType(aggType));
                else
                    sig += GetTypeName(functionType.ReturnType);
            }
            return sig;
        }

        public static string GetFunctionName(FunctionSymbol funcSym)
        {
            string name = funcSym.NameString;
            //handle explicit interface implementation here.
            int index = name.LastIndexOf('.');
            if (index != -1)
                name = name.Substring(index + 1);
            return name;
        }

        public static string GetTypeName(Phx.Types.Type type)
        {
            if (type == null)
                return null;
            if (type.IsAggregateType && type.TypeSymbol != null)
            {
                //get the qualified name and remove the assembly name prefix from that
                string qualname = type.TypeSymbol.QualifiedName;                
                //check if the symbol is qualified                        
                if (qualname[0] != '[')
                {
                    var asmsym = PhxUtil.GetAssemblySymbol(type.TypeSymbol);
                    if (asmsym != null)
                    {
                        var asmname = asmsym.NameString;
                        qualname = "[" + asmname + "]" + qualname;
                    }
                }
                return qualname;
            }            
            else
                return type.ToString();
        }

        public static string GetQualifiedFunctionName(FunctionSymbol funcsym)
        {
            var typename = PhxUtil.GetTypeName(funcsym.EnclosingAggregateType);
            var funcname = PhxUtil.GetFunctionName(funcsym);
            var sig = PhxUtil.GetFunctionTypeSignature(funcsym.FunctionType);
            return typename + "::" + funcname + "/" + sig;
        }
        
        public static string GetFieldName(Phx.Types.Field field)
        {
            //get the qualified name and remove the assembly name prefix from that
            string fieldname = field.FieldSymbol.NameString;
            return fieldname;            
        }

        public static string RemoveAssemblyName(string symname)
        {
            //get the qualified name and remove the assembly name prefix from that            
            if (symname.Length > 0)
            {
                int firstchar = symname[0];
                int eindex = symname.IndexOf(']');
                if (firstchar == '[' && eindex != -1)
                    symname = symname.Substring(eindex + 1);
            }
            return symname;
        }               
        
        /// <summary>
        /// Assumes that the called method is an instance method
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public static AggregateType GetNormalizedReceiver(Phx.IR.CallInstruction instruction)
        {
            //TODO check for non-static here but checking for is virtual            
            var referrentType = instruction.SourceOperand2.Type.AsPointerType.ReferentType;
            if (referrentType.IsAggregateType)
            {
                var receiverType = PhxUtil.NormalizedAggregateType(referrentType.AsAggregateType);
                return receiverType;
            }
            else
            {
                //if (referrentType.TypeSymbol != null)
                //    Console.WriteLine("No receiver of aggregate type for virtual call: " + referrentType.TypeSymbol.QualifiedName);
                //else
                //    Console.WriteLine("No receiver of aggregate type for virtual call: " + referrentType.ToString());
                return null;
            }
        }

        public static FunctionSymbol NormalizedFunctionSymbol(FunctionSymbol functionSymbol)
        {
            if (functionSymbol.UninstantiatedFunctionSymbol != null)
                return functionSymbol.UninstantiatedFunctionSymbol;
            else
                return functionSymbol;
        }

        public static Phx.Symbols.AssemblySymbol GetAssemblySymbol(Phx.Symbols.Symbol originalSym)
        {
            Phx.Symbols.Symbol sym = originalSym.LexicalParentSymbol;
            while (sym != null && !sym.IsAssemblySymbol)
            {
                sym = sym.LexicalParentSymbol;
            }
            if (sym == null)
                return null;
            return sym.AsAssemblySymbol;
        }

        public static string GetContainingNamespace(AggregateType aggtype)
        {
            var outermostType = GetOutermostType(aggtype);
            if (outermostType == null)
                return null;

            string typename = outermostType.TypeSymbol.NameString;
            int lastDotIndex = typename.LastIndexOf(".");
            if (lastDotIndex < 0)
                return null;
            string containingNS = typename.Substring(0, lastDotIndex);
            return containingNS;
        }

        public static bool TryGetAggregateType(Phx.Types.Type type, out Phx.Types.AggregateType aggtype)
        {
            if (type.IsAggregateType)
            {
                aggtype = type.AsAggregateType;
                return true;
            }
            else if (type.IsPointerType)
            {
                //pick the innermost non pointer type
                var reftype = type.AsPointerType.ReferentType;
                return TryGetAggregateType(reftype, out aggtype);                
            }
            else
            {
                aggtype = null;
                return false;
            }
        }

        public static AggregateType GetOutermostType(AggregateType aggtype)
        {
            var enclosingType = aggtype;
            if (enclosingType == null)
                return null;
            while (enclosingType.EnclosingAggregateType != null)
                enclosingType = enclosingType.EnclosingAggregateType;
            return enclosingType;
        }

        public static AggregateType NormalizedAggregateType(AggregateType aggregateType)
        {
            if (aggregateType.UninstantiatedAggregateType != null)
                return aggregateType.UninstantiatedAggregateType;
            else
                return aggregateType;
        }

        public static Phx.Types.Type NormalizedType(Phx.Types.Type type)
        {
            AggregateType aggtype;
            if (PhxUtil.TryGetAggregateType(type, out aggtype))
            {
                return NormalizedAggregateType(aggtype);
            }
            else
                return type;
        }
        
        public static bool DoesBelongToCurrentAssembly(Phx.Symbols.Symbol symbol, Phx.PEModuleUnit moduleUnit)
        {
            var asmSym = PhxUtil.GetAssemblySymbol(symbol);
            if (asmSym != null && asmSym.NameString.Equals(moduleUnit.Manifest.Name.NameString))
                return true;
            return false;
        }        

        public static bool ExplicitInterfaceImplementation(FunctionSymbol funcSym)
        {
            //handle explicit interface implementation here.
            int index = funcSym.NameString.LastIndexOf('.');
            if (index != -1)
                return true;
            return false;
        }

        public static bool IsConstructor(string methodname)
        {
            if (methodname.Contains("ctor") ||
                methodname.Contains("cctor"))
                return true;
            return false;
        }

        /// <summary>
        /// This looks for exact signature match which may be a bit too strict
        /// </summary>
        /// <param name="moduleUnit"></param>
        /// <param name="typesig"></param>
        /// <returns></returns>
        public static IEnumerable<FunctionSymbol>
            GetCompatibleFuncionSymbols(Phx.PEModuleUnit moduleUnit, string typesig)
        {
            foreach (var unit in moduleUnit.ChildUnits)
            {
                if (unit.IsFunctionUnit)
                {
                    var funcSym = unit.AsFunctionUnit.FunctionSymbol;
                    if (funcSym.UninstantiatedFunctionSymbol != null)
                        continue;

                    var funcSig = PhxUtil.GetFunctionTypeSignature(funcSym.FunctionType);
                    if (PhxUtil.AreSignaturesCompatible(funcSig, typesig))
                    {
                        yield return funcSym;
                    }
                }
            }            
        }

        public static bool AreSignaturesCompatible(string sig1, string sig2)
        {
            if (sig1.Equals(sig2))
                return true;
            char[] sep = { '(', ')', ',' };
            var tokens1 = sig1.Split(sep);
            var tokens2 = sig2.Split(sep);

            //check if the method names match
            if (tokens1[0].Equals(tokens2[0]))
            {
                //check if parameters and return are compatible
                if(tokens1.Length == tokens2.Length)
                {
                    bool notequal = false;
                    for (int i = 1; i < tokens1.Length; i++)
                    {
                        if (tokens1[i].StartsWith("!")
                        || tokens2[i].StartsWith("!"))
                            continue;

                        if (!tokens1[i].Equals(tokens2[i]))
                        {
                            notequal = true;
                            break;
                        }
                    }
                    if (notequal)
                        return false;
                    else
                        return true;
                }
            }
            return false;
        }        

        public static bool IsLinqMethod(Phx.Symbols.FunctionSymbol funcSymbol)
        {
            FunctionSymbol normSym = PhxUtil.NormalizedFunctionSymbol(funcSymbol);
            if (PhxUtil.GetTypeName(normSym.EnclosingAggregateType).Contains("System.Linq"))
                return true;
            return false;
        }       
    }
}
