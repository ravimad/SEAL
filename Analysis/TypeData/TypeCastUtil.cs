using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.TypeUtil
{
    public class TypeCastUtil
    {
        public static TypeInfo convertToType;
        public static string castFileName = "";
        public static int castLineNumber = 0;
        public static string analyzingFunction = "";
        public static string castEdgeLabel = "";
        public static bool instructionFound = false;

        public static string answer = "";

        public static void GetTypeInfoFromCastIntruction(Phx.IR.Instruction instruction)
        {

            if (instructionFound)
            {
                return;
            }

            Phx.IR.ImmediateOperand operand = instruction.SourceOperand1.AsImmediateOperand;

            Phx.PEModuleUnit moduleUnit = instruction.FunctionUnit.ParentPEModuleUnit;

            string typename = SafetyAnalysis.Util.PhxUtil.GetTypeName(operand.Symbol.AsTypeSymbol.Type);

            convertToType = CombinedTypeHierarchy.GetInstance(moduleUnit).LookupTypeInfo(typename);

            instructionFound = true;

        }

        public static bool isInstructionCheck(Phx.IR.Instruction instruction)
        {

            if (instruction.GetLineNumber() == castLineNumber)
            {
                if (instruction.GetFileName().ToString().ToLower() == castFileName)
                {
                    return true;
                }
            }

            return false;
        }

        public static void setCastEdgeLabel()
        {
            castEdgeLabel = "__Cast_" + castFileName + "_" + castLineNumber;
        }

        public static void check(Purity.PurityAnalysisData data, Phx.PEModuleUnit moduleUnit)
        {
            if (!instructionFound)
            {
                answer = "Instruction Not Reached Yet";
                return;
            }

            if (convertToType == null)
            {
                answer = "Type to be converted to is not found ***";
                return;
            }


            CombinedTypeHierarchy cth = CombinedTypeHierarchy.GetInstance(moduleUnit);

            //The SuperTypes of the convertToType -> Types which are allowed for the object that is casted
            var superTypes = cth.GetSubTypesFromTypeHierarchy(convertToType);

            if (Purity.PurityAnalysisPhase.EnableConsoleLogging)
            {
                Console.Write("Set of SuperTypes:");

                foreach (TypeInfo x in superTypes)
                {
                    Console.Write(" " + x.GetTypeName());
                }

                Console.Write("\n");
            }

            bool castable = true;

            foreach (Framework.Graphs.HeapVertexBase vertex in data.OutHeapGraph.Vertices)
            {
                if (vertex is Framework.Graphs.GlobalLoadVertex)
                {
                    foreach (Framework.Graphs.HeapEdgeBase edge in data.OutHeapGraph.OutEdges(vertex))
                    {
                        if (edge.Field.ToString().Equals("::" + castEdgeLabel))
                        {
                            IEnumerable<string> types = data.GetTypes(edge.Target);

                            foreach (string x in types)
                            {
                                if (Purity.PurityAnalysisPhase.EnableConsoleLogging) Console.WriteLine("Type to Check: " + x);
                                TypeInfo typex = cth.LookupTypeInfo(x);
                                if (!superTypes.Contains(typex))
                                {
                                    if (Purity.PurityAnalysisPhase.EnableConsoleLogging) Console.WriteLine("Failed");
                                    castable = false;
                                }
                            }
                        }
                    }
                }
            }

            if (!castable)
            {
                answer = "May not be Castable";
            }
            else
            {
                answer = "Always Castable";
            }
        }


    }
}
