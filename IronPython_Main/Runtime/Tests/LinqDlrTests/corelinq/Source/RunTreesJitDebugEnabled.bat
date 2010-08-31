@echo off

setlocal

set path=%path%;%WINDIR%\Microsoft.NET\Framework\v3.5
set path=%path%;%CD%\..\..\testenv\bin\x86

@rem 0 = Ask, 1 = No ask/No Debug, 2 = No ask/Debug
@rem set Complus_DbgJITDebugLaunchSetting=1


set CSC_PIPE=csc /r:%DLR_ROOT%\Bin\Debug\Microsoft.Scripting.Core.dll /debug+ /optimize- /define:DEBUG 
set EXPRCOMPROOT=%CD%\ExpressionCompiler

@echo.
@echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@echo GAC INSTALLING Microsoft.Scripting.Core.dll
@echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@echo.

gacutil /i %DLR_ROOT%\Bin\Debug\Microsoft.Scripting.Core.dll /f

if "%ERRORLEVEL%" == "0" (
    echo INSTALLED.
) else (
    echo Failed to install Microsoft.Scripting.Core into GAC. Did you build Debug ?
    goto END
)

perl ..\..\testenv\bin\runall.pl %RUN_TREES_ARGS%


@echo.
@echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@echo GAC UNINSTALLING Microsoft.Scripting.Core.dll
@echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@echo.

gacutil /u  Microsoft.Scripting.Core


:END

