using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafetyAnalysis.Testutils;
namespace PurityTest {
	 [TestClass()] public class CheckerTest {
		 public TestContext TestContext {get; set;}
[TestMethod()]public void TestSkippedCallsMerging(){ 
	string arguments = "/in " + TestUtil.bindir + "TestSkippedCallsMerging.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestSkippedCallsMerging-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestSkippedCallsMerging", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestGenerics2(){ 
	string arguments = "/in " + TestUtil.bindir + "TestGenerics2.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestGenerics2-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestGenerics2", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestStructs(){ 
	string arguments = "/in " + TestUtil.bindir + "TestStructs.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestStructs-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestStructs", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestComposition(){ 
	string arguments = "/in " + TestUtil.bindir + "TestComposition.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestComposition-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestComposition", arguments, correctOutputFilename);
	 }
[TestMethod()]public void IteratorPurityTest(){ 
	string arguments = "/in " + TestUtil.bindir + "IteratorPurityTest.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\IteratorPurityTest-Purity-Report.dat";
	TestUtil.ValidatePurityReports("IteratorPurityTest", arguments, correctOutputFilename);
	 }
[TestMethod()]public void LinkedList(){ 
	string arguments = "/in " + TestUtil.bindir + "LinkedList.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\LinkedList-Purity-Report.dat";
	TestUtil.ValidatePurityReports("LinkedList", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestAliasingRule(){ 
	string arguments = "/in " + TestUtil.bindir + "TestAliasingRule.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestAliasingRule-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestAliasingRule", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestArrayHandling(){ 
	string arguments = "/in " + TestUtil.bindir + "TestArrayHandling.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestArrayHandling-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestArrayHandling", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestDelegates(){ 
	string arguments = "/in " + TestUtil.bindir + "TestDelegates.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestDelegates-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestDelegates", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestEnumCBResolution(){ 
	string arguments = "/in " + TestUtil.bindir + "TestEnumCBResolution.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestEnumCBResolution-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestEnumCBResolution", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestEscapingNodes(){ 
	string arguments = "/in " + TestUtil.bindir + "TestEscapingNodes.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestEscapingNodes-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestEscapingNodes", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestExceptions(){ 
	string arguments = "/in " + TestUtil.bindir + "TestExceptions.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestExceptions-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestExceptions", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestFixPoint(){ 
	string arguments = "/in " + TestUtil.bindir + "TestFixPoint.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestFixPoint-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestFixPoint", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestGenericMethods(){ 
	string arguments = "/in " + TestUtil.bindir + "TestGenericMethods.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestGenericMethods-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestGenericMethods", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestHeapCloning(){ 
	string arguments = "/in " + TestUtil.bindir + "TestHeapCloning.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestHeapCloning-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestHeapCloning", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestInstanceDelegates(){ 
	string arguments = "/in " + TestUtil.bindir + "TestInstanceDelegates.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestInstanceDelegates-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestInstanceDelegates", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestInterfaces(){ 
	string arguments = "/in " + TestUtil.bindir + "TestInterfaces.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestInterfaces-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestInterfaces", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestLoadNodeMerge(){ 
	string arguments = "/in " + TestUtil.bindir + "TestLoadNodeMerge.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestLoadNodeMerge-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestLoadNodeMerge", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestNodeMerging(){ 
	string arguments = "/in " + TestUtil.bindir + "TestNodeMerging.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestNodeMerging-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestNodeMerging", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestOutParams(){ 
	string arguments = "/in " + TestUtil.bindir + "TestOutParams.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestOutParams-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestOutParams", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestParameterUpdate(){ 
	string arguments = "/in " + TestUtil.bindir + "TestParameterUpdate.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestParameterUpdate-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestParameterUpdate", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestPointerReturn(){ 
	string arguments = "/in " + TestUtil.bindir + "TestPointerReturn.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestPointerReturn-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestPointerReturn", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestRecursiveCall(){ 
	string arguments = "/in " + TestUtil.bindir + "TestRecursiveCall.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestRecursiveCall-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestRecursiveCall", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestStaticRecursiveCall(){ 
	string arguments = "/in " + TestUtil.bindir + "TestStaticRecursiveCall.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestStaticRecursiveCall-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestStaticRecursiveCall", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestStaticUpdate(){ 
	string arguments = "/in " + TestUtil.bindir + "TestStaticUpdate.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestStaticUpdate-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestStaticUpdate", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestVariableParameters(){ 
	string arguments = "/in " + TestUtil.bindir + "TestVariableParameters.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestVariableParameters-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestVariableParameters", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestVirtualDelegates(){ 
	string arguments = "/in " + TestUtil.bindir + "TestVirtualDelegates.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestVirtualDelegates-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestVirtualDelegates", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestVirtualMethod(){ 
	string arguments = "/in " + TestUtil.bindir + "TestVirtualMethod.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestVirtualMethod-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestVirtualMethod", arguments, correctOutputFilename);
	 }

 }}
