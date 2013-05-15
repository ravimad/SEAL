### set up some local variables
$checkerhome = "C:\sd\codebox\speculation\SafetyAnalysis\trunk\SafetyAnalysisWithCB"
$outputhome = "C:\Users\t-rakand\Desktop\new-results"
$benchmarkhome = "$checkerhome\benchmarks-dlls"
#$benchmarks = "DocX","Facebook","dynamicdatadisplay","Quickgraph","AvalonDock","SharpMap","Utilities","PdfSharp"
$benchmarks = "TestApiCore"

foreach($benchmark in $benchmarks)
{
	### invoke the checker program
	& "$checkerhome\Debug\Checker.exe" "/in" "$benchmarkhome\$benchmark.dll" "/config-file" "$checkerhome\Configs\benchmarks.config"
	mkdir "$outputhome\$benchmark"
	mv "$benchmark-*" "$outputhome\$benchmark\"	
}