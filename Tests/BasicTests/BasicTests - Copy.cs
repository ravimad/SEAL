using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafetyAnalysis.Testutils;

namespace PurityTest
{        
    /// <summary>
    ///This is a test class for CheckerTest and is intended
    ///to contain all CheckerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CheckerTest
    {
        private TestContext testContextInstance;        

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for aliaisng rule
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void AliasingRuleTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestAliasingRule.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestAliasingRule-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestAliasingRule", arguments, correctOutputFilename);                        
        }

        /// <summary>
        ///A test for New object handler
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void NewObjHandlerTest()
        {
            
            string arguments = "/in " + TestUtil.bindir +  @"\TestBin\TestPointerReturn.dll";
                                 
            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestPointerReturn-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestPointerReturn", arguments, correctOutputFilename);
        }

        /// <summary>
        ///A test for Parameter update
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void ParameterUpdateTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestParameterUpdate.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestParameterUpdate-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestParameterUpdate", arguments, correctOutputFilename);
        }

        /// <summary>
        ///A test for static update
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void StaticUpdateTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestStaticUpdate.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestStaticUpdate-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestStaticUpdate", arguments, correctOutputFilename);
        }

        /// <summary>
        ///A test for Load node merge
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void LoadNodeMergeTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestLoadNodeMerge.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestLoadNodeMerge-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestLoadNodeMerge", arguments, correctOutputFilename);
        }
    
        /// <summary>
        ///A test for Virtual method calls
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void VirtualMethodCallTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestVirtualMethod.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestVirtualMethod-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestVirtualMethod", arguments, correctOutputFilename);
        }

        /// <summary>
        ///A test for Recursive method calls
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void RecursiveMethodCallTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestRecursiveCall.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestRecursiveCall-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestRecursiveCall", arguments, correctOutputFilename);
        }

        /// <summary>
        ///A test for Static Recursive method calls
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void StaticRecursiveMethodCallTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestStaticRecursiveCall.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestStaticRecursiveCall-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestStaticRecursiveCall", arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void ArrayHandlingTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestArrayHandling.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestArrayHandling-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestArrayHandling", arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void LinkedListTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\LinkedList.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\LinkedList-Purity-Report.dat";
            TestUtil.ValidatePurityReports("LinkedList", arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void FixPointTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestFixPoint.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestFixPoint-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestFixPoint", arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void InterfacesTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestInterfaces.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestInterfaces-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestInterfaces", arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void GenericMethodsTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestGenericMethods.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestGenericMethods-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestGenericMethods", arguments, correctOutputFilename);
        }

        //[TestMethod()]
        //[DeploymentItem("Checker.exe")]
        //public void GraphLibraryTest()
        //{
        //    string TestUtil.testBinDir = TestUtil.testProjectdir + "Tests\\GraphLibrary";
        //    string arguments = "/in " + TestUtil.testBinDir + @"\TestBin\GraphLibrary.dll";

        //    string correctOutputFilename = TestUtil.testBinDir + @"\CorrectOutputs\GraphLibrary-Purity-Report.dat";
        //    TestUtil.ValidatePurityReports(TestUtil.testBinDir, arguments, correctOutputFilename);
        //}

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void EscapingNodesTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestEscapingNodes.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestEscapingNodes-Purity-Report.dat";
            TestUtil.ValidatePurityReports("TestEscapingNodes", arguments, correctOutputFilename);
        }        

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void VariableParamTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestVariableParameters.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestVariableParameters-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void OutParamTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestOutParams.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestOutParams-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void ExceptionsTest()
        {            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestExceptions.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestExceptions-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void EnumCBResolutionTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestEnumCBResolution.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestEnumCBResolution-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void NodeMergingTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestNodeMerging.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestNodeMerging-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void IteratorPurityTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\IteratorPurityTest.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\IteratorPurityTest-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void DelegatesTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestDelegates.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestDelegates-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void InstanceDelegatesTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestInstanceDelegates.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestInstanceDelegates-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void VirtualDelegatesTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestVirtualDelegates.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestVirtualDelegates-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void HeapCloningTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestHeapCloning.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestHeapCloning-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void SkippedCallsMergingTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestSkippedCallsMerging.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestSkippedCallsMerging-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void StructsTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestStructs.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestStructs-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void GenericsTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestGenerics2.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestGenerics2-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }

        [TestMethod()]
        [DeploymentItem("Checker.exe")]
        public void CompositionTest()
        {
            
            string arguments = "/in " + TestUtil.bindir + @"\TestBin\TestComposition.dll";

            string correctOutputFilename = TestUtil.bindir + @"\CorrectOutputs\TestComposition-Purity-Report.dat";
            TestUtil.ValidatePurityReports(TestUtil.bindir, arguments, correctOutputFilename);
        }
    }
}
