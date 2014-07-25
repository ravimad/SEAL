using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Util
{
    public class SourceSinkUtil
    {
        public static string sourceFileName = "";
        public static int sourceLineNumber = 0;

        public static string sinkFileName = "";
        public static int sinkLineNumber = 0;

        public static string checkingFunction = "";

        public static string configFile = "defaultConfigFile";

        public static string answer = "";
        public static string error1 = "";
        public static string error2 = "";

        /*
        public static string hashFile;

        public static string hashStored;
        public static string hashSourceFile;
        public static int hashSourceLine;
        public static string hashSinkFile;
        public static int hashSinkLine;

        public static string hashNew;
        */

        public static bool sourcefound = false;
        public static bool sinkfound = false;

        public static bool sourceMatter = true;
        public static bool sinkMatter = true;

        public static string sourceEdgeLabel;
        public static string sinkEdgeLabel;

        public static bool checkInstructionSource(Phx.IR.Instruction instruction)
        {

            if (instruction.GetLineNumber() == sourceLineNumber && instruction.Opcode.ToString() != "NOP" && instruction.Opcode.ToString() != "GOTO")
            {
                if (instruction.GetFileName().ToString().ToLower() == sourceFileName)
                {
                    if (instruction.Next == null || instruction.Next.GetLineNumber() != sourceLineNumber || instruction.Next.Opcode.ToString() == "NOP" || instruction.Next.Opcode.ToString() == "GOTO")
                    {
                        if (instruction.DestinationOperand != null && instruction.DestinationOperand.Type != null)
                        {
                            if (!instruction.DestinationOperand.Type.IsPrimitiveType)
                            {
                                sourcefound = true;
                                return true;
                            }
                            else
                            {
                                setError1("Invalid Source : Primitive Type");
                            }
                        }
                        else if (!sourcefound && (instruction.Opcode.ToString() == "CALL"))
                        {
                            setError1("Invalid Source : Void Function Call");
                        }
                        else
                        {
                            setError1("Invalid Source : No Destination (Assignment) ");
                        }
                    }
                }
            }

            return false;
        }

        public static bool checkInstructionSink(Phx.IR.Instruction instruction)
        {

            if (instruction.GetLineNumber() == sinkLineNumber && instruction.Opcode.ToString() != "NOP" && instruction.Opcode.ToString() != "GOTO")
            {
                if (instruction.GetFileName().ToString().ToLower() == sinkFileName)
                {
                    if (instruction.Next == null || instruction.Next.GetLineNumber() != sinkLineNumber || instruction.Next.Opcode.ToString() == "NOP" || instruction.Next.Opcode.ToString() == "GOTO")
                    {
                        if (instruction.DestinationOperand != null && instruction.DestinationOperand.Type != null)
                        {
                            if (!instruction.DestinationOperand.Type.IsPrimitiveType)
                            {
                                sinkfound = true;
                                return true;
                            }
                            else
                            {
                                setError2("Invalid Sink : Primitive Type");
                            }
                        }
                        else if (!sinkfound && (instruction.Opcode.ToString() == "CALL"))
                        {
                            setError2("Invalid Sink : Void Function Call");
                        }
                        else
                        {
                            setError2("Invalid Sink : No Destination (Assignment) ");
                        }
                    }
                }
            }

            return false;
        }

        /*
        private static void recreateHashFile()
        {
            if (File.Exists(hashFile))
            {
                File.Delete(hashFile);
            }
            File.Create(hashFile);
        }

        public static void deleteHashFile()
        {
            if (File.Exists(hashFile))
            {
                File.Delete(hashFile);
            }
        }

        public static void createHashFile()
        {

            StreamWriter w = File.CreateText(hashFile);
            w.WriteLine(hashNew);
            w.WriteLine(sourceFileName);
            w.WriteLine(sourceLineNumber);
            w.WriteLine(sinkFileName);
            w.WriteLine(sinkLineNumber);
            w.Flush();
            w.Close();
        }

        public static bool hashFileExists()
        {
            return File.Exists(hashFile);
        }

        public static void readHashFile()
        {
            FileStream fs = File.OpenRead(hashFile);
            var reader = new StreamReader(fs);
            hashStored = reader.ReadLine();
            hashSourceFile = reader.ReadLine();
            hashSourceLine = Convert.ToInt32(reader.ReadLine());
            hashSinkFile = reader.ReadLine();
            hashSinkLine = Convert.ToInt32(reader.ReadLine());
            reader.Close();
            fs.Close();

            if (hashSourceFile == sourceFileName && hashSourceLine == sourceLineNumber)
            {
                sourceMatter = false;
            }

            if (hashSinkFile == sinkFileName && hashSinkLine == sinkLineNumber)
            {
                sinkMatter = false;
            }
        }
        */

        public static void setError1(string line)
        {
            if (error1 == "")
            {
                error1 = line;
            }
        }

        public static void setError2(string line)
        {
            if (error2 == "")
            {
                error2 = line;
            }
        }


        //Returns true if source does not hinder in extracting the summary from database
        public static bool sourceCheck(Phx.FunctionUnit functionUnit)
        {
            if (!sourceMatter)
            {
                return true;
            }

            if (sourcefound)
            {
                return false;
            }

            if (functionUnit.FirstEnterInstruction.GetFileName().ToString().ToLower() != sourceFileName)
            {
                return true;
            }

            return (functionUnit.FirstInstruction.GetLineNumber() > sourceLineNumber || functionUnit.LastInstruction.GetLineNumber() < sourceLineNumber);

        }

        //Returns true if sink does not hinder in extracting the summary from database
        public static bool sinkCheck(Phx.FunctionUnit functionUnit)
        {
            if (!sinkMatter)
            {
                return true;
            }

            if (sinkfound)
            {
                return false;
            }

            if (functionUnit.FirstEnterInstruction.GetFileName().ToString().ToLower() != sinkFileName)
            {
                return true;
            }

            return (functionUnit.FirstInstruction.GetLineNumber() > sinkLineNumber || functionUnit.LastInstruction.GetLineNumber() < sinkLineNumber);

        }

        public static void generateEdgeLabels()
        {
            sourceEdgeLabel = "__Source_" + sourceFileName + "_" + sourceLineNumber;
            sinkEdgeLabel = "__Sink_" + sinkFileName + "_" + sinkLineNumber;
        }

    }
}