Requirements
------------

* Microsoft.NET 4 Framework
* Visual Studio 2010

Installation Instructions
--------------------------

* Run the script SETENV.ps1 from powershell (reopen the shell for the settings to take effect)
* open the solution Seal.sln in VS 2010
* build the solution
* Run the script CREATEDB.ps1 (this could take several minutes)
* Check the installation by running the testsuites "BasicTests" and "LibTests" using VS 2010.

Notes
-----

* CREATEDB.ps1 analyses the "mscorlib", "system", "system.core" dlls of the .NET4 framwork. 
By default it uses the directory "C:\Windows\Microsoft.NET\Framework\v4.0.30319\". 
The user can change the paths of the above dlls if necessary.

* Some of the tests in the test suite "LibTests" may fail due to the differences in 
the .NET version used during the development and the one installed in the user end.

See the tutorial in the project homepage for more information about the tool.