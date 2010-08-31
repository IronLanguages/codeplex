@echo off
echo Running Runtrees.Bat.
echo dlr_root=%DLR_ROOT%
echo LINQONSYSTEMCORE=%LINQONSYSTEMCORE%
echo DLR_BIN=%DLR_BIN%
echo program files=%PROGRAMFILES%

setlocal

set COMPlus_LegacyCASPolicy=1
set COMPlus_DefaultSecurityRuleSet=1

set PATH=%DLR_ROOT%\Runtime\Tests\LinqDlrTests\testenv\perl\bin;%SYSTEMROOT%\Microsoft.NET\Framework\v2.0.50727;%PATH%


@rem 0 = Ask, 1 = No ask/No Debug, 2 = No ask/Debug
set Complus_DbgJITDebugLaunchSetting=1


echo Setting up SYSTEMCORE
IF NOT DEFINED LINQONSYSTEMCORE (
	GOTO NOLINQONSYSCORE
) ELSE (
	GOTO LINQONSYSCORE
)


:NOLINQONSYSCORE
SET SYSTEMCORE="%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5\System.Core.Dll"
set SYSTEMCORE=%SYSTEMCORE:~1%
set SYSTEMCORE=%SYSTEMCORE:~0,-1%

echo SYSTEMCORE=%SYSTEMCORE%

GOTO CONTINUELINQONSYSCORE





:LINQONSYSCORE

set PEVERIFY="%ProgramFiles%\Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools\peverify.exe"
set SYSTEMCORE=%FRAMEWORK_PATH%\System.Core.Dll

GOTO CONTINUELINQONSYSCORE



:CONTINUELINQONSYSCORE



echo Setting up scflags
set scflags=%DLR_ROOT%\Runtime\Tests\LinqDlrTests\corelinq\Source\System.Core.cs

if NOT DEFINED DLR_BIN (
	set DLR_BIN=%DLR_ROOT%\Bin\Debug
)


REM we need to do this for one particular testcase.
REM /r:system.Scripting.ExtensionAttribute.dll 
set APTCAFUNKYFLAGS=/noconfig /r:system.dll /r:system.xml.dll /r:%DLR_BIN%\Microsoft.Scripting.Core.dll /r:%DLR_BIN%\Microsoft.Scripting.dll "/r:Core=%SYSTEMCORE%" %DLR_ROOT%\Runtime\Tests\LinqDlrTests\corelinq\Source\System.Core.cs

rem /r:system.Scripting.ExtensionAttribute.dll 

echo DEFINING CSC_PIPE
echo SYSTEMCORE=%SYSTEMCORE%
if NOT DEFINED LINQONSYSTEMCORE (
set CSC_PIPE="%WINDIR%\Microsoft.NET\Framework\v3.5\csc" /noconfig /r:system.dll /r:system.xml.dll /r:%DLR_BIN%\Microsoft.Scripting.Core.dll /r:%DLR_BIN%\Microsoft.Scripting.dll "/r:Core=%SYSTEMCORE%"
) else (
set CSC_PIPE="%FRAMEWORK_PATH%\csc" /noconfig /r:system.dll /r:system.xml.dll "/r:%SYSTEMCORE%" "/r:Core=%SYSTEMCORE%"
)

set EXPRCOMPROOT=%CD%\ExpressionCompiler


REM cleanup before running
Echo Cleanup before running
cmd /c cleanup.cmd

@echo on
perl.exe ..\..\testenv\bin\runall.pl %*
@echo off
set ERRORLEVELBAK=%ERRORLEVEL%



:END
exit /b %ERRORLEVELBAK%
