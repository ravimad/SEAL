### set up some local variables
$outputhome = "C:\Users\t-rakand\Desktop\results\new-results\DotSpatial"
$benchmarks = "Dotspatial.Positioning","Dotspatial.Symbology","Dotspatial.Projections","Dotspatial.Data","Dotspatial.Topology","Dotspatial.Serialization"

### create a new cumulative table file
$rfile = "$outputhome\results-summary.txt"
$result = New-Object System.Collections.ArrayList
$count = 0
foreach($benchmark in $benchmarks)
{	
	### Generate statistics
	& ".\DotSpatialStats.ps1" "$benchmark"
	
	$file = "$outputhome\stats\$benchmark-cumulative.txt"
	if($count -eq 0)
	{
		$count++
		$firstcolumn = & cut "-f1" '-d":"' $file 			
		$tempstr = ""
		foreach($element in $firstcolumn)
		{
			$tempstr = "$tempstr`t$element"
		}	
		$result.Add("$tempstr")	
	}
	
	### read the second column and concat it with result file
	$column = & cut "-f2" '-d":"' $file 			
	$tempstr = ""
	foreach($element in $column)
	{
		$tempstr = "$tempstr$element`t"
	}
	$result.Add("$benchmark`t$tempstr")
}
echo $result > $rfile

### Create a xl file to generate graphs
$xl = new-object -comobject excel.application	    
#$xl.Visible = $True
$xl.workbooks.OpenText($rfile,437,1,1,1,$True,$True,$False,$False,$True,$False)
$wkb = $xl.workbooks.Item(1)
$wks = $wkb.worksheets.item(1)

### do some temp stuff so that it saves as xl
$range = $wks.usedRange
$range.EntireColumn.AutoFit()

$wkb.SaveAs("$outputhome\results-summary",51)
$wkb.Close($false)
$xl.Quit();	 	   