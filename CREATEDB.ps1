$sealhome = $env:SEALHOME
## set dll paths
#$mscorlib = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll"
#$system = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\system.dll"
#$syscore = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\system.core.dll"

$mscorlib = "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1\mscorlib.dll"
$system = "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1\System.dll"
$syscore = "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1\System.Core.dll"

##populate the database
& Write-Host $sealhome
#& Write-Host "Processing stubs"
#& "$sealhome\Bin\Checker.exe" "/in" "$sealhome\Stubs\Bin\Stubs.dll" "/config-file" "$sealhome\Configs\stubs-.NET4.config"
#& Write-Host "Processing mscorlib"
#& "$sealhome\Bin\Checker.exe" "/in" $mscorlib "/config-file" "$sealhome\Configs\mscorlib-.NET4.config"

#& Write-Host "Processing system"
#& "$sealhome\Bin\Checker.exe" "/in" $system "/config-file" "$sealhome\Configs\system-.NET4.config"

& Write-Host "Processing syscore"
& "$sealhome\Bin\Checker.exe" "/in" $syscore "/config-file" "$sealhome\Configs\system.core-.NET4.config"