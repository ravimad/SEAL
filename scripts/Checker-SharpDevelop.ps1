### set up some local variables
$checkerhome = "C:\Users\t-rakand\Desktop\SEAL"
$benchmarkhome = "$checkerhome\benchmarks-dlls"
$outputhome = "C:\Users\t-rakand\Desktop\new-results"

foreach($benchmark in $benchmarks)
{
	### invoke the checker program
	& "$checkerhome\Debug\Checker.exe" "/in" "$benchmarkhome\SharpDevelop\ICSharpCode.NRefactory.dll" "/in" "$benchmarkhome\SharpDevelop\ICSharpCode.Core.dll" "/in" "$benchmarkhome\SharpDevelop\ICSharpCode.SharpDevelop.Dom.dll" "/in" "$benchmarkhome\SharpDevelop\ICSharpCode.SharpDevelop.Widgets.dll" "/in" "$benchmarkhome\SharpDevelop\ICSharpCode.SharpDevelop.BuildWorker.exe" "/in" "$benchmarkhome\SharpDevelop\ICSharpCode.SharpDevelop.dll" "/in" "$benchmarkhome\SharpDevelop\SharpDevelop.exe" "/config-file" "$checkerhome\Configs\benchmarks.config"
	mkdir "$outputhome\SharpDevelop"
	mv "$benchmark-*" "$outputhome\SharpDevelop\"	
}