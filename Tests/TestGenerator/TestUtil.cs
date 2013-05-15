using SafetyAnalysis.Checker;
using SafetyAnalysis.Purity;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SafetyAnalysis.Testutils
{
    public class TestUtil
    {        
        public static string sealHome = Environment.GetEnvironmentVariable("SEALHOME");        
        public static string bindir = sealHome + @"\Tests\TestBin\";        
        public static string serializedFilename = @"Purity-Report.dat";        

        public static void ValidatePurityReports(string testname, string arguments, string correctOutputFilename)
        {
            //create an output dir
            //var outdir = root + "\\CheckerOutput";
            //if (!Directory.Exists(outdir))
            //    Directory.CreateDirectory(outdir);
            //arguments += " /outdir " + outdir;
            
            int actual;
            Process checkerProcess = new Process();
            checkerProcess.StartInfo.WorkingDirectory = bindir;            
            checkerProcess.StartInfo.FileName = "Checker.exe";                
            checkerProcess.StartInfo.Arguments = arguments;
            //checkerProcess.StartInfo.RedirectStandardOutput = true;
            //checkerProcess.StartInfo.RedirectStandardError = true;
            //checkerProcess.StartInfo.UseShellExecute = false; 
            checkerProcess.Start();
            checkerProcess.WaitForExit();            
            actual = checkerProcess.ExitCode;            
            if (actual != 0)
            {
                //Console.WriteLine("Exited with: " + checkerProcess.StandardError.ReadToEnd());
                Assert.Fail("Checker exited with an error");
            }            

            //Get the correct purity report
            Console.WriteLine("Deserializing");
            BinaryFormatter serailizer = new BinaryFormatter();
            FileStream stream = new FileStream(correctOutputFilename, FileMode.Open, FileAccess.Read, FileShare.None);
            //StreamReader reader = new StreamReader(stream);
            List<PurityReport> correctOutput = (List<PurityReport>)serailizer.Deserialize(stream);

            //Get the purity report for the current testcase            
            stream = new FileStream(bindir + serializedFilename, FileMode.Open, FileAccess.Read, FileShare.None);
            //reader = new StreamReader(stream);
            List<PurityReport> currentReports = (List<PurityReport>)serailizer.Deserialize(stream);
            stream.Close();
           
            //Compare the purity reports
            if (currentReports.Count != correctOutput.Count)
            {
                PrintCorrectOutput(correctOutput, testname);
                Assert.Fail("mismatch in number of reports");
            }
            StreamWriter writer = null;
            foreach (PurityReport report in currentReports)
            {
                if (!correctOutput.Contains(report))
                {
                    if(writer == null)
                        writer = PrintCorrectOutput(correctOutput, testname);                    
                    writer.WriteLine("This report not present in correctoutput");
                    report.Print(writer);
                    writer.Flush();                    
                    Assert.Fail("Purity Reports are not equal");
                }
            }
            if (writer != null)
            {                
                writer.Close();
            }
        }

        private static StreamWriter PrintCorrectOutput(List<PurityReport> correctOutputs, string testname)
        {
            StreamWriter writer = new StreamWriter(new FileStream(bindir + testname +"-Error.log",
                FileMode.Create, FileAccess.Write, FileShare.None));
            //for debuging
            writer.WriteLine("Printing Correct output: ");
            foreach (PurityReport rep in correctOutputs)
            {
                rep.Print(writer);
            }
            writer.Flush();
            return writer;
        }
    }
}
