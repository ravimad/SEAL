using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TestGenerator
{
    public class TestGenerator
    {
        private string testfile;
        private StreamWriter writer;

        public static string[] basicTests = { 
            "TestSkippedCallsMerging",
            "TestGenerics2",
            "TestStructs",
            "TestComposition",
            "IteratorPurityTest",
            "LinkedList",                                             
            "TestAliasingRule",
            "TestArrayHandling",                                                                                    
            "TestDelegates",                          
            "TestEnumCBResolution", 
            "TestEscapingNodes",                                      
            "TestExceptions",                                         
            "TestFixPoint",                                           
            "TestGenericMethods",                                               
            "TestHeapCloning",                                        
            "TestInstanceDelegates",                                  
            "TestInterfaces",                                                                          
            "TestLoadNodeMerge",                                      
            "TestNodeMerging",                                        
            "TestOutParams",                                          
            "TestParameterUpdate",                                    
            "TestPointerReturn",                                      
            "TestRecursiveCall",                                      
            "TestStaticRecursiveCall",                                
            "TestStaticUpdate",                                       
            "TestVariableParameters",                                 
            "TestVirtualDelegates",                                   
            "TestVirtualMethod" };

        public static string[] libTests = { 
            "TestLinqWithGenerics",
            "LinqTest",                                               
            "LinqTest2",                                             
            "LinqTest3",                                              
            "LinqTest4",                                                                                     
            "TestDictionaryEnumerator",                               
            "TestHashSetEnumerator",                                  
            "TestListEnumerator",                                     
            "TestMscorlibCalls",                                      
            "TestNestedLinq",                                         
            "TestSystemCoreCalls",                                    
            "TestSystemLibCalls",                                     
            "TestKnownMethodCalls",
            "OrderByTest",
            "TestGroupBy",
            "TestJoin",
            "LinqTest6",
            "LinqTest7",
            "StubTests" };

        public TestGenerator(string basicTestFile)
        {
            testfile = basicTestFile;
            writer = new StreamWriter(testfile);
        }

        public static void Main(string[] args)
        {                        
            string sealHome = Environment.GetEnvironmentVariable("SEALHOME");
            string basicTestFile = sealHome + "\\Tests\\BasicTests\\BasicTests.cs";
            string libTestFile = sealHome + "\\Tests\\LibTests\\LibTests.cs";

            //create and populate  basic tests file
            var btgen = new TestGenerator(basicTestFile);
            btgen.AddHeader();
            foreach(var testname in basicTests)
            {
                btgen.AddTestMethod(testname);
            }
            btgen.AddTrailer();

            //create and populate lib tests file
            var libgen = new TestGenerator(libTestFile);
            libgen.AddHeader();
            foreach(var testname in libTests)
            {
                libgen.AddTestMethod(testname);
            }
            libgen.AddTrailer();
        }

        public void AddHeader()
        {
            //using declarations
            var usingstr = "using Microsoft.VisualStudio.TestTools.UnitTesting;\n" + 
                            "using SafetyAnalysis.Testutils;";
            writer.WriteLine(usingstr);
            //add namespace declaration            
            writer.WriteLine("namespace PurityTest {");
            //add class declaration
            writer.WriteLine("\t [TestClass()] public class CheckerTest {");
            //add preamble            
            writer.WriteLine("\t\t public TestContext TestContext {get; set;}");            
        }

        public void AddTestMethod(string testname)
        {
            var str = "[TestMethod()]public void " + testname + "(){ \n\t" +
                       "string arguments = \"/in \" + TestUtil.bindir + \""+testname+".dll\" + " + 
                                           "\" /config-file \" + TestUtil.sealHome + @\"\\Configs\\tests-.NET4.config\";"+ "\n\t" +
                       "string correctOutputFilename = TestUtil.sealHome + @\"\\Tests\\CorrectOutputs\\"+testname+"-Purity-Report.dat\";" + "\n\t" +
                        "TestUtil.ValidatePurityReports(\""+testname+"\", arguments, correctOutputFilename);" +
                       "\n\t }";
            writer.WriteLine(str);        
        }

        public void AddTrailer()
        {
            //close the class and namespace definitions
            writer.WriteLine("\n }}");
            writer.Flush();
            writer.Close();
        }
    }
}
