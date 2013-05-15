#set the SEALHOME environmental variable
$relpath = Split-Path -Parent $script:MyInvocation.MyCommand.Path
$sealhome = [IO.Path]::GetFullPath($relpath)
[Environment]::SetEnvironmentVariable("SEALHOME", $sealhome, "User")  