### set up some local variables
$checkerhome = "C:\sd\codebox\speculation\SafetyAnalysis\trunk\SafetyAnalysisWithCB"
$benchmarkhome = "$checkerhome\benchmarks-dlls"
$outputhome = "C:\Users\t-rakand\Desktop\new-results"

foreach($benchmark in $benchmarks)
{
	### invoke the checker program
	& "$checkerhome\Debug\Checker.exe" "/in" "$benchmarkhome\DotSpatial\DotSpatial.Serialization.dll" "/in" "$benchmarkhome\DotSpatial\DotSpatial.Topology.dll" "/in" "$benchmarkhome\DotSpatial\DotSpatial.Data.dll" "/in" "$benchmarkhome\DotSpatial\DotSpatial.Positioning.dll" "/in" "$benchmarkhome\DotSpatial\DotSpatial.Symbology.dll" "/in" "$benchmarkhome\DotSpatial\DotSpatial.Analysis.dll" "/config-file" "$checkerhome\Configs\benchmarks.config"
	mkdir "$outputhome\DotSpatial\"
	mv "$benchmark-*" "$outputhome\DotSpatial\"	
}
#"/in" "$benchmarkhome\DotSpatial\DotSpatial.Projections.dll"