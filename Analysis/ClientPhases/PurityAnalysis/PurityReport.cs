using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SafetyAnalysis.Purity
{
    [Serializable]
    public class PurityReport
    {
        public string functionName;
        public string functionType;
        public bool isPure;
        public List<AccessPathRegexp> witnesses = new List<AccessPathRegexp>();
        public List<string> skwitness = new List<string>();

        //report metadata (need not be serialized)
        public uint Linenumber { get; set; }
        public uint Colnumber { get; set; }
        public string Filename { get; set; }

        public PurityReport(string fname, string ftype, bool purity)
        {
            functionName = fname;
            functionType = ftype;
            isPure = purity;
        }

        public PurityReport(string fname, string ftype, 
            bool purity, IEnumerable<AccessPathRegexp> w) : 
            this(fname,ftype,purity)
        {            
            witnesses.AddRange(w);
        }

        public void addWitnesses(AccessPathRegexp elem)
        {
            witnesses.Add(elem);
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
            if (obj is PurityReport)
            {
                var output = obj as PurityReport;

                var apset = new HashSet<AccessPathRegexp>(witnesses);
                var skset = new HashSet<string>(skwitness);

                return (output.functionName.Equals(functionName) &&
                    output.functionType.Equals(functionType) &&
                    (output.isPure == isPure) &&
                    apset.SetEquals(output.witnesses) &&
                    skset.SetEquals(output.skwitness));           
            }
            return false;
        }

        //prints the output to the console in a decorated way        
        public void Dump()
        {
            if (isPure)
            {
                if (!skwitness.Any())
                    Console.ForegroundColor = ConsoleColor.Green;
                else Console.ForegroundColor = ConsoleColor.Cyan;                
            }
            else
                Console.ForegroundColor = ConsoleColor.Red;

            Console.Write("Method {0} Type {1} ", functionName, functionType);
            Console.WriteLine(" is {0} pure ", isPure ? "" : "not ");

            if (!isPure)
            {                
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Written Access-paths: ");
                foreach (var witness in witnesses)
                {
                    //dump the witness
                    Console.WriteLine(witness.ToString());
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
            writer.Write("Method {0} Type {1} ", functionName, functionType);
            writer.WriteLine(" is {0} pure ", isPure ? (skwitness.Any() ? "conditionally" : "") : "not ");            

            if (!isPure)
            {                                          
                foreach (var witness in witnesses)
                {
                        writer.WriteLine(witness.ToString());
                        writer.WriteLine();
                }                
            }
            if (skwitness.Any())
            {
                writer.WriteLine("Skipped calls: ");
                foreach (var call in skwitness)
                {
                    writer.WriteLine(call);
                }
            }
        }

        internal void DumpAsVSErrorMsg()
        {            
            Console.WriteLine("{0}({1},{2}): {3} : Method {4} is {5} pure", 
                Filename, Linenumber, Colnumber, 
                isPure ? "Proved" : "Warning",
                functionName, isPure ? ( skwitness.Any() ? "conditionally" : "" ) : "not ");            
            if (!isPure)
            {                
                Console.WriteLine("Written Access-paths: ");
                foreach (var witness in witnesses)
                {                    
                    Console.WriteLine("  " + witness.ToString());             
                }
            }

            if (skwitness.Any())
            {                
                Console.WriteLine("Potential call-backs: ");
                foreach (var call in skwitness)
                {
                    Console.WriteLine("  " + call.ToString());
                }
            }            
        }
    }
}
