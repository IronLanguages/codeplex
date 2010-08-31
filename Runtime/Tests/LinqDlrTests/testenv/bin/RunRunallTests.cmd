@echo off
REM == Limitations:
REM == - Only supports 'native' runs

cls

call :SETCOMMONENVVARIABLES
call :DROPSHARESELECTION
call :COMPONENTSELECTION
call :SELECTBRANCH
call :SELECTTESTBUILD
call :SELECTTESTREQBINPATH
call :SELECTQAMDPATH
call :SETMARATHONVARS
call :RUNCUSTOMIZATION
call :GETANDRUN

goto :eof

:DROPSHARESELECTION
echo ========= DROP SHARE SELECTION (where you installed VS from) ==
echo                1. \\cpvsbuild\drops\dev10
echo                2. custom (you will be prompted to enter a UNC path)
set DROPSHAREID=1
set DROPSHARE=\\cpvsbuild\drops\orcas
set /P DROPSHAREID=Please, select drop share: [%DROPSHAREID%] 
IF %DROPSHAREID%==1 set DROPSHARE=\\cpvsbuild\drops\dev10
IF %DROPSHAREID%==2 set /P DROPSHARE=Please, select enter path: [%DROPLOCATIONROOT%] 
goto :EOF


:COMPONENTSELECTION
echo ======== COMPONENT SELECTION =======
echo                1. C#2.0
echo                2. Alink
echo                3. C#EE
echo                7. ASMMETA Feature
echo                9. C#EE Remote
echo               12. C#3.0
echo               13. CoreLinq
echo               14. LAF
echo               15. CSHARPSTRESS
echo               16. C#4.0
echo ====================================
set COMP=1
set /P COMP=Please, select component from the list: [%COMP%] 
goto :eof

:SELECTBRANCH
echo ========= Branch SELECTION ==
echo                1. vs_langs01
echo                2. main
echo                3. sxs
echo                4. custom (you will be prompted to enter a UNC path; also, there won't be any support for SkipRules)
set BRANCHID=1
set BRANCHNAME=vs_langs01
set /P BRANCHID=Please, select branch: [%BRANCHID%] 
IF %BRANCHID%==1 set BRANCHNAME=vs_langs01
IF %BRANCHID%==2 set BRANCHNAME=main
IF %BRANCHID%==3 set BRANCHNAME=sxs
IF %BRANCHID%==4 set BRANCHNAME=custom
IF %BRANCHID%==4 set /P DROPLOCATIONROOT=Please, select enter path: [%DROPLOCATIONROOT%] 
IF NOT %BRANCHNAME%==custom set DROPLOCATIONROOT=%DROPSHARE%\%BRANCHNAME%

goto :EOF

:SELECTTESTBUILD
for /F %%i IN ('dir /b %DROPLOCATIONROOT%\tst') DO @set TESTBUILD=%%i
echo ========= TEST BUILD SELECTION (@%DROPLOCATIONROOT%\tst) ==
for /F %%i IN ('dir /b %DROPLOCATIONROOT%\tst') DO @echo                %%i
set /P TESTBUILD=Please, select Test build: [%TESTBUILD%] 
goto :EOF

:SELECTTESTREQBINPATH
echo ========= Select TESTREQBINPATH (suitebin) ==
echo                1. %DROPLOCATIONROOT%\tst\%TESTBUILD%\x86chk\suitebin
echo                2. %DROPLOCATIONROOT%\tst\%TESTBUILD%\x86ret\suitebin
IF NOT %TESTBUILD%==current echo                3. %DROPLOCATIONROOT%\tst\current\x86chk\suitebin
IF NOT %TESTBUILD%==current echo                4. %DROPLOCATIONROOT%\tst\current\x86ret\suitebin
echo                5. custom (you will be prompted to enter a UNC path)
set TQBPID=1
set /P TQBPID=Please, select TESTREQBINPATH: [%TQBPID%] 
set TESTREQBINPATH=%DROPLOCATIONROOT%\tst\%TESTBUILD%\x86chk\suitebin
IF %TQBPID%==1 set TESTREQBINPATH=%DROPLOCATIONROOT%\tst\%TESTBUILD%\x86chk\suitebin
IF %TQBPID%==2 set TESTREQBINPATH=%DROPLOCATIONROOT%\tst\%TESTBUILD%\x86ret\suitebin
IF %TQBPID%==3 set TESTREQBINPATH=%DROPLOCATIONROOT%\tst\current\x86chk\suitebin
IF %TQBPID%==4 set TESTREQBINPATH=%DROPLOCATIONROOT%\tst\current\x86ret\suitebin
IF %TQBPID%==5 set /P TESTREQBINPATH=Please, select enter path: [%TESTREQBINPATH%] 
goto :EOF


:SELECTQAMDPATH
set MDQAPATH=\\vcslabsrv\drops\orcas\%BRANCHNAME%\tst\%TESTBUILD%\qa\md
echo ========= Select TEST SOURCE LOCATION (...\qa\md) ==
echo                1. %MDQAPATH%
echo                2. custom (you will be prompted to enter a UNC path)
set MDQAPATHID=1
set /P MDQAPATHID=Please, select Test Src Location: [%MDQAPATHID%] 
IF %MDQAPATHID%==2 set /P MDQAPATH=Please, select enter path: [%MDQAPATH%] 
goto :EOF

:RUNCUSTOMIZATION
echo === Additional run customization ===
echo ====================================
set MARATHON_ISREFACTOR=N
set /P MARATHON_ISREFACTOR=Is this a 'refactor' run (y/N)? [%MARATHON_ISREFACTOR%] 
IF /I %MARATHON_ISREFACTOR%==y (
    set X_RUNALL_ARGS=-nottags:xml
    set MARATHON_CUSTOM_ISCFLAGS=/test:refactor
)

echo ====================================


REM ==
REM == Last chance to customize Runall Flags!!!
REM ==
set /P X_RUNALL_ARGS=Enter custom X_RUNALL_ARGS [%X_RUNALL_ARGS%] 

echo ====================================

REM ==
REM == Last chance to customize Run Flags!!!
REM ==
set ISCFLAGS=%MARATHON_COMP_ISCFLAGS% %MARATHON_CUSTOM_ISCFLAGS%
set /P ISCFLAGS=Enter custom ISCFLAGS [%ISCFLAGS%] 

echo ====================================

goto :EOF

:SETMARATHONVARS

set MARATHON_PLATFORM=%OSARCH%
set MARATHON_LOG_SERVER=%TEMP%
IF NOT DEFINED simulator_pipe Set simulator_pipe=stopit.exe -s800 -g
IF NOT DEFINED CSC_PIPE set CSC_PIPE=stopit.exe -s300 csc.exe
REM == set iSCFLAGS=/debug+ /o+
REM == set iGLFLAGS=-hc -x -q -err

REM = ? = set MARATHON_RUN_0_ISCFLAGS=/debug+ /o+

set MARATHON_COMP_TO_SCRIPT_2=runalinktestsU.bat
set MARATHON_COMP_TO_SCRIPT_1=runcsctestsU.bat
set MARATHON_COMP_TO_SCRIPT_3=runcseetestsU.bat
set MARATHON_COMP_TO_SCRIPT_7=RunAsmmetaFU.bat
set MARATHON_COMP_TO_SCRIPT_9=runcseeremotetestsU.bat
set MARATHON_COMP_TO_SCRIPT_12=runcsc3testsU.bat
set MARATHON_COMP_TO_SCRIPT_13=runlinqtestsU.bat
set MARATHON_COMP_TO_SCRIPT_14=runlaftestsU.bat
set MARATHON_COMP_TO_SCRIPT_15=runcscstresstestsU.bat
set MARATHON_COMP_TO_SCRIPT_18=runcsc4testsU.bat
set MARATHON_COMP_TO_ISCFLAGS_2=/debug+ /o+
set MARATHON_COMP_TO_ISCFLAGS_1=/debug+ /o+
set MARATHON_COMP_TO_ISCFLAGS_3=/debug+
set MARATHON_COMP_TO_ISCFLAGS_7=
set MARATHON_COMP_TO_ISCFLAGS_9=/debug+
set MARATHON_COMP_TO_ISCFLAGS_12=/debug+ /o+
set MARATHON_COMP_TO_ISCFLAGS_13=/debug+ /o+
set MARATHON_COMP_TO_ISCFLAGS_14=/debug+ /o+
set MARATHON_COMP_TO_ISCFLAGS_15=/debug+ /o+
set MARATHON_COMP_TO_ISCFLAGS_18=/debug+ /o+
set MARATHON_COMP_TO_NAME_2=alink
set MARATHON_COMP_TO_NAME_1=csharp
set MARATHON_COMP_TO_NAME_3=csee
set MARATHON_COMP_TO_NAME_7=asmmetaf
set MARATHON_COMP_TO_NAME_9=cseeremote
set MARATHON_COMP_TO_NAME_12=csharp3
set MARATHON_COMP_TO_NAME_13=corelinq
set MARATHON_COMP_TO_NAME_14=laf
set MARATHON_COMP_TO_NAME_15=csharpstress
set MARATHON_COMP_TO_NAME_18=csharp4

set SDTREE=%MDQAPATH%\src\vcs\Compiler
set ROBOCOPYDIR=%SDTREE%\testenv\bin\x86
set MARATHON_FILES=%SDTREE%\testenv\bin

call :MAPCOMP MARATHON_COMP_NAME     %COMP% NAME
call :MAPCOMP MARATHON_COMP_SCRIPT   %COMP% SCRIPT
call :MAPCOMP MARATHON_COMP_ISCFLAGS %COMP% ISCFLAGS

set MARATHON_COMP_ID=%COMP%

goto :EOF

:SETCOMMONENVVARIABLES

        REM == Find out OS architecture, no matter what cmd prompt
        SET OSARCH=%PROCESSOR_ARCHITECTURE%
        IF "%PROCESSOR_ARCHITEW6432%"=="" GOTO lOsArchDone
        SET OSARCH=%PROCESSOR_ARCHITEW6432%
	:lOsArchDone
	IF "%OSARCH%"=="" set OSARCH=x86

        REM == Find out path to native 'Program Files', no matter what
        REM == architecture we are running on and no matter what command
        REM == prompt we came from.
        IF /I "%OSARCH%"=="x86"                                             set NATIVE_PROGRAMFILES=%ProgramFiles%
        IF /I "%OSARCH%"=="IA64"  IF /I "%PROCESSOR_ARCHITEW6432%"==""      set NATIVE_PROGRAMFILES=%ProgramFiles%
        IF /I "%OSARCH%"=="IA64"  IF /I "%PROCESSOR_ARCHITEW6432%"=="IA64"  set NATIVE_PROGRAMFILES=%ProgramW6432%
        IF /I "%OSARCH%"=="AMD64" IF /I "%PROCESSOR_ARCHITEW6432%"==""      set NATIVE_PROGRAMFILES=%ProgramFiles%
        IF /I "%OSARCH%"=="AMD64" IF /I "%PROCESSOR_ARCHITEW6432%"=="AMD64" set NATIVE_PROGRAMFILES=%ProgramW6432%

        REM == Find out path to native 'Program Files 32bit', no matter what
        REM == architecture we are running on and no matter what command
        REM == prompt we came from.
        IF /I "%OSARCH%"=="x86"   set X86_PROGRAMFILES=%ProgramFiles%
        IF /I "%OSARCH%"=="IA64"  set X86_PROGRAMFILES=%ProgramFiles(x86)%
        IF /I "%OSARCH%"=="AMD64" set X86_PROGRAMFILES=%ProgramFiles(x86)%

        REM == Find out path to Net Framework v4.0
        REM == This is tricky if we are on 64bit architectures because this script
        REM == could be executed in wow64 or native.

        set GREENBITS32=
        set GREENBITS64=

        FOR /F "tokens=3*" %%p in ('reg query "HKLM\Software\Microsoft\Net Framework Setup\NDP\v4.0" /v InstallPath') DO set GREENBITS64=%%p
        IF NOT "%GREENBITS64%"=="" set GREENBITS64=%GREENBITS64:~0,-1%
        FOR /F "tokens=3*" %%p in ('reg query "HKLM\Software\Wow6432Node\Microsoft\Net Framework Setup\NDP\v4.0" /v InstallPath') DO set GREENBITS32=%%p
        IF NOT "%GREENBITS32%"=="" set GREENBITS32=%GREENBITS32:~0,-1%

     REM == ==================
     REM == HACK = HACK = HACK
     REM == ==================
        REM == Last chance! If we still have no NetFx folder (apparently CLR4.0 setup does not write the reg key)
        REM == we enumerate the different folders and we pick the "highest" one.
        IF "%GREENBITS32%"=="" for /d %%a IN (%WINDIR%\Microsoft.Net\Framework\v4.?.?????) DO SET GREENBITS32=%%a
        IF "%GREENBITS64%"=="" for /d %%a IN (%WINDIR%\Microsoft.Net\Framework64\v4.?.?????) DO SET GREENBITS32=%%a
        IF "%GREENBITS64%"=="" set GREENBITS64=%GREENBITS32%
     REM == ==================
     REM == HACK = HACK = HACK
     REM == ==================

        REM == On 32bit architectures, the reg queries return the same values.
        IF "%OSARCH%"=="x86" SET GREENBITS32=%GREENBITS64%&& SET GREENBITS64=

        REM == If we are in WoW64, we can't get (easily) to the 64bit registry so we take a shortcut...
        IF NOT "%OSARCH%"=="%PROCESSOR_ARCHITECTURE%" set GREENBITS64=%GREENBITS32:Framework=Framework64%

        echo GREENBITS32=%GREENBITS32%
        echo GREENBITS64=%GREENBITS64%

        call :NORMALIZEFILENAME TESTROOT "%TEMP%\school"
        REM set TESTROOT=%FILENAME83%\school

goto :EOF

:GETANDRUN

REM == Set COPYCMP (on Vista, this is a system command so we don't need our own robocopy)
set COPYCMD=ROBOCOPY.exe /R:10 /W:60 /MIR /NC /NS /NFL
where ROBOCOPY.EXE >NUL
IF ERRORLEVEL 1 set COPYCMD=%ROBOCOPYDIR%\%COPYCMD%
IF NOT "%ISADMIN%"=="FALSE"     set COPYCMD=%COPYCMD% /ZB /LOG+:%TEMP%\RoboCopy_%RANDOM%.log
IF     "%ISADMIN%"=="FALSE"     set COPYCMD=%COPYCMD%     /LOG+:%TEMP%\RoboCopy_%RANDOM%.log

%COPYCMD% /LEV:0 \\vcslabsrv\Marathon\Perl          "%TEMP%\Perl"
%COPYCMD% \\vcslabsrv\Marathon\Perl\%OSARCH% "%TEMP%\Perl\%OSARCH%"

pushd "%TEMP%\Perl"
call setenv-ms.bat
popd

IF EXIST %TESTROOT%\results.log del %TESTROOT%\results.log
IF EXIST %TESTROOT%\runpl.log del %TESTROOT%\runpl.log

if NOT EXIST %TESTROOT% mkdir %TESTROOT%
pushd %TESTROOT%

call %MARATHON_FILES%\MasterGet.bat %MARATHON_COMP_NAME%

call %MARATHON_FILES%\%MARATHON_COMP_SCRIPT% -mode RELEASE -guid %MARATHON_COMP_NAME% -version %TESTBUILD% -branch %BRANCHNAME%

goto :EOF

:NORMALIZEFILENAME
call set %1=%%~s2%
goto :eof

:MAPCOMP
call set %1=%%MARATHON_COMP_TO_%3_%2%%
goto :eof
