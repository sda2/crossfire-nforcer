@echo off
setlocal
cd /d "%~dp0"
set "CF_CSC=%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\csc.exe"
if not exist "%CF_CSC%" (
  echo .NET Framework 2.0 C# compiler not found.
  exit /b 1
)
"%CF_CSC%" /nologo /target:exe /platform:x86 /optimize+ /checked- /warn:4 /warnaserror+ /win32icon:assets\nforcer.ico /out:Crossfire-nForcer.exe src\Engine.cs src\Rules.cs src\Program.cs
exit /b %errorlevel%
