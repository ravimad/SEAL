#Purity Analysis
#C:\SealRepo\Bin\Checker.exe /in C:\SealRepo\Examples\Basic\bin\Debug\Basic.dll

#SourceSink Analysis
C:\SealRepo\Bin\Checker.exe /in ("C:\SealRepo\Examples\Basic\bin\Debug\Basic.dll") /analysistype("sourcesinkanalysis") /sourcefile("C:\SealRepo\Examples\Basic\Basic.cs") /sourceline 16 /sinkfile("C:\SealRepo\Examples\Basic\Basic.cs") /sinkline 20 /function("Basic.foo1")

#Cast Analysis
#C:\SealRepo\Bin\Checker.exe /in ("C:\SealRepo\Examples\Basic\bin\Debug\Basic.dll") /analysistype("castanalysis") /castfile("C:\SealRepo\Examples\Basic\Basic.cs") /castline 32 /function("Basic.foo3")
