$statsgenhome = "C:\sd\codebox\speculation\SafetyAnalysis\trunk\SafetyAnalysisWithCB"
$outputhome = "C:\Users\t-rakand\Desktop\results\new-results"

### generate statistics
mkdir "$outputhome\$args\stats"
& "$statsgenhome\Debug\StatsGenerator.exe" "$outputhome\$args\stats" "$args" "$outputhome\$args"

$files = Get-ChildItem "$outputhome\$args\stats\*.txt"
#foreach($file in $files)
#{
#    if($file -like "*name*"){continue}
#	if($file -like "*cumulative*"){continue}
	
#	echo $file     
    ### Create a xl file to generate graphs
    #$xl = new-object -comobject excel.application
    ##$xl.Visible = $True
    #$xl.workbooks.OpenText($file,$null,1,1,1,$True,$True,$False,$False,$True,$False)
    #$wkb = $xl.workbooks.Item(1)
    #$wks = $wkb.worksheets.item(1)
 
    ### select the lines to dump to graph
    #$wks.select()
    #$rng = $wks.UsedRange
    #$rng.select()

    ### create a chart 
    #$chart = $wkb.Charts.Add()
    ### xlcharttype line = 4
    #$chart.ChartType = 4
    #$chart.setSourceData($rng)

    ###Save the output
    #$wkb.SaveAs("$file.chart.xlsx")
    #$wkb.Close($false)
    #$xl.Quit();    
#}