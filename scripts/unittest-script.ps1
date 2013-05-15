../Bin/Checker.exe /in ../Tests/TestBin/$args.dll /outdir C:\sd\codebox\speculation\SafetyAnalysis\trunk\Release-Package\Seal-Sources\Scripts\outputs
chmod 777 ../Tests/CorrectOutputs/$args-Purity-Report.dat
rm ../Tests/CorrectOutputs/$args-Purity-Report.dat
mv ./outputs/Purity-Report.dat ../Tests/CorrectOutputs/$args-Purity-Report.dat
