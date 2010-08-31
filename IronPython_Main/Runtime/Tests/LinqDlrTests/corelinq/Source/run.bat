@echo off

setlocal

set _NTTREE=%DLR_ROOT%\..\..\..\binaries\x86chk
set COMPLUS_DEFAULTVERSION=v4.0.x86ret
set path="%Programfiles%\Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools";%path%
set path=%path%;%~dp0\..\..\testenv\bin\x86
set PEVERIFY="%Programfiles%\Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools"

@rem 0 = Ask, 1 = No ask/No Debug, 2 = No ask/Debug
set Complus_DbgJITDebugLaunchSetting=1


set CSC_PIPE=csc /noconfig /r:%_NTTREE%\System.Core.dll
set EXPRCOMPROOT=%CD%\ExpressionCompiler

@echo.
@echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@echo GAC INSTALLING System.Core.dll
@echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@echo.

gacutil /i %_NTTREE%\System.Core.dll /f

if "%ERRORLEVEL%" == "0" (
    echo INSTALLED.
) else (
    echo Failed to install System.Core into GAC
    goto END
)

perl ..\..\testenv\bin\runall.pl %*


@echo.
@echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@echo GAC UNINSTALLING System.Core.dll
@echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@echo.

gacutil /u System.Core,Version=4.0.0.0,PublicKeyToken=b77a5c561934e089


:END
