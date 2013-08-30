using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SafetyAnalysis.Purity
{
    [Serializable]
    public class RaceReport
    {
        public string functionName;
        public string functionType;
        public HashSet<AccessPathRegexp> readset;
        public HashSet<AccessPathRegexp> writeset;
        public List<string> skwitness = new List<string>();

        //report metadata (need not be serialized)
        public uint Linenumber { get; set; }
        public uint Colnumber { get; set; }
        public string Filename { get; set; }

        public RaceReport(string fname, string ftype)
        {
            functionName = fname;
            functionType = ftype;            
        }

        public RaceReport(string fname, string ftype, HashSet<AccessPathRegexp> rset,
            HashSet<AccessPathRegexp> wset)
        {
            functionName = fname;
            functionType = ftype;      
            readset = rset;
            writeset = wset;
        }

        public void AddSkippedCalls(Call skcall)
        {
            skwitness.Add(skcall.ToString());            
        }

        public override int GetHashCode()
        {
            return 3;
        }

        public override bool Equals(Object obj)
        {
            if (obj is RaceReport)
            {
                var output = obj as RaceReport;                
                var skset = new HashSet<string>(skwitness);

                return (output.functionName.Equals(functionName) &&
                    output.functionType.Equals(functionType) &&                    
                    readset.SetEquals(output.readset) &&
                    writeset.SetEquals(output.writeset) &&
                    skset.SetEquals(output.skwitness));           
            }
            return false;
        }

        //prints the output to the console in a decorated way        
        public void Dump()
        {
            Console.WriteLine("{0}({1},{2}, Method {3} Type {4} ",
                Filename, Linenumber, Colnumber, functionName, functionType);

            if (writeset.Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Written Access-paths: ");
                foreach (var ap in writeset)
                {
                    //dump the witness
                    Console.WriteLine(ap.ToString());
                    Console.WriteLine();
                }
            }

            if (readset.Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Read Access-paths: ");
                foreach (var ap in readset)
                {
                    //dump the witness
                    Console.WriteLine(ap.ToString());
                    Console.WriteLine();
                }
            }

            if (skwitness.Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Potential call-backs: ");
                foreach (var call in skwitness)
                {
                    Console.WriteLine(call);
                }
            }
            Console.ResetColor();
        }

        public void Print(StreamWriter writer)
        {
            writer.WriteLine("Method {0} Type {1} ", functionName, functionType);
            writer.WriteLine("Written Access-paths: ");
            foreach (var ap in writeset)
            {                
                writer.WriteLine(ap.ToString());
                writer.WriteLine();
            }           
            writer.WriteLine("Read Access-paths: ");
            foreach (var ap in readset)
            {                
                writer.WriteLine(ap.ToString());
                writer.WriteLine();
            }

            if (skwitness.Any())
            {             
                writer.WriteLine("Potential call-backs: ");
                foreach (var call in skwitness)
                {
                    writer.WriteLine(call);
                }
            }            
        }       
    }
}
