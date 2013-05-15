using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafetyAnalysis.Testutils;
namespace PurityTest {
	 [TestClass()] public class CheckerTest {
		 public TestContext TestContext {get; set;}
[TestMethod()]public void TestLinqWithGenerics(){ 
	string arguments = "/in " + TestUtil.bindir + "TestLinqWithGenerics.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestLinqWithGenerics-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestLinqWithGenerics", arguments, correctOutputFilename);
	 }
[TestMethod()]public void LinqTest(){ 
	string arguments = "/in " + TestUtil.bindir + "LinqTest.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\LinqTest-Purity-Report.dat";
	TestUtil.ValidatePurityReports("LinqTest", arguments, correctOutputFilename);
	 }
[TestMethod()]public void LinqTest2(){ 
	string arguments = "/in " + TestUtil.bindir + "LinqTest2.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\LinqTest2-Purity-Report.dat";
	TestUtil.ValidatePurityReports("LinqTest2", arguments, correctOutputFilename);
	 }
[TestMethod()]public void LinqTest3(){ 
	string arguments = "/in " + TestUtil.bindir + "LinqTest3.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\LinqTest3-Purity-Report.dat";
	TestUtil.ValidatePurityReports("LinqTest3", arguments, correctOutputFilename);
	 }
[TestMethod()]public void LinqTest4(){ 
	string arguments = "/in " + TestUtil.bindir + "LinqTest4.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\LinqTest4-Purity-Report.dat";
	TestUtil.ValidatePurityReports("LinqTest4", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestDictionaryEnumerator(){ 
	string arguments = "/in " + TestUtil.bindir + "TestDictionaryEnumerator.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestDictionaryEnumerator-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestDictionaryEnumerator", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestHashSetEnumerator(){ 
	string arguments = "/in " + TestUtil.bindir + "TestHashSetEnumerator.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestHashSetEnumerator-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestHashSetEnumerator", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestListEnumerator(){ 
	string arguments = "/in " + TestUtil.bindir + "TestListEnumerator.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestListEnumerator-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestListEnumerator", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestMscorlibCalls(){ 
	string arguments = "/in " + TestUtil.bindir + "TestMscorlibCalls.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestMscorlibCalls-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestMscorlibCalls", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestNestedLinq(){ 
	string arguments = "/in " + TestUtil.bindir + "TestNestedLinq.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestNestedLinq-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestNestedLinq", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestSystemCoreCalls(){ 
	string arguments = "/in " + TestUtil.bindir + "TestSystemCoreCalls.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestSystemCoreCalls-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestSystemCoreCalls", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestSystemLibCalls(){ 
	string arguments = "/in " + TestUtil.bindir + "TestSystemLibCalls.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestSystemLibCalls-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestSystemLibCalls", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestKnownMethodCalls(){ 
	string arguments = "/in " + TestUtil.bindir + "TestKnownMethodCalls.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestKnownMethodCalls-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestKnownMethodCalls", arguments, correctOutputFilename);
	 }
[TestMethod()]public void OrderByTest(){ 
	string arguments = "/in " + TestUtil.bindir + "OrderByTest.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\OrderByTest-Purity-Report.dat";
	TestUtil.ValidatePurityReports("OrderByTest", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestGroupBy(){ 
	string arguments = "/in " + TestUtil.bindir + "TestGroupBy.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestGroupBy-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestGroupBy", arguments, correctOutputFilename);
	 }
[TestMethod()]public void TestJoin(){ 
	string arguments = "/in " + TestUtil.bindir + "TestJoin.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\TestJoin-Purity-Report.dat";
	TestUtil.ValidatePurityReports("TestJoin", arguments, correctOutputFilename);
	 }
[TestMethod()]public void LinqTest6(){ 
	string arguments = "/in " + TestUtil.bindir + "LinqTest6.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\LinqTest6-Purity-Report.dat";
	TestUtil.ValidatePurityReports("LinqTest6", arguments, correctOutputFilename);
	 }
[TestMethod()]public void LinqTest7(){ 
	string arguments = "/in " + TestUtil.bindir + "LinqTest7.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\LinqTest7-Purity-Report.dat";
	TestUtil.ValidatePurityReports("LinqTest7", arguments, correctOutputFilename);
	 }
[TestMethod()]public void StubTests(){ 
	string arguments = "/in " + TestUtil.bindir + "StubTests.dll" + " /config-file " + TestUtil.sealHome + @"\Configs\tests-.NET4.config";
	string correctOutputFilename = TestUtil.sealHome + @"\Tests\CorrectOutputs\StubTests-Purity-Report.dat";
	TestUtil.ValidatePurityReports("StubTests", arguments, correctOutputFilename);
	 }

 }}
