set MARATHON_PLATFORM=x86
set MARATHON_LOG_SERVER=d:\
Set simulator_pipe=d:\school\testenv\bin\x86\stopit.exe -s800 -g
set iSCFLAGS=/debug+ /o+
set iGLFLAGS=-hc -x -q -err
set PEVERIFY=peverify.exe
set MARATHON_RUN_COUNT=6
set MARATHON_RUN_0_GUID=alink
set MARATHON_RUN_0_RUNTYPE=1
set MARATHON_RUN_0_COMPONENT=2
set MARATHON_RUN_0_SCRIPT=runalinktestsU.bat
set MARATHON_RUN_0_ISCFLAGS=/debug+ /o+
set MARATHON_RUN_1_GUID=cs
set MARATHON_RUN_1_RUNTYPE=1
set MARATHON_RUN_1_COMPONENT=1
set MARATHON_RUN_1_SCRIPT=runcsctestsU.bat
set MARATHON_RUN_1_ISCFLAGS=/debug+ /o+
set MARATHON_RUN_2_GUID=csee
set MARATHON_RUN_2_RUNTYPE=1
set MARATHON_RUN_2_COMPONENT=3
set MARATHON_RUN_2_SCRIPT=runcseetestsU.bat
set MARATHON_RUN_2_ISCFLAGS=/debug+
set MARATHON_RUN_3_GUID=cs3
set MARATHON_RUN_3_RUNTYPE=1
set MARATHON_RUN_3_COMPONENT=12
set MARATHON_RUN_3_SCRIPT=runcsc3testsU.bat
set MARATHON_RUN_3_ISCFLAGS=/debug+ /o+
set MARATHON_RUN_4_GUID=cs4
set MARATHON_RUN_4_RUNTYPE=1
set MARATHON_RUN_4_COMPONENT=18
set MARATHON_RUN_4_SCRIPT=runcsc4testsU.bat
set MARATHON_RUN_4_ISCFLAGS=/debug+ /o+

set MARATHON_RUN_5_GUID=linq
set MARATHON_RUN_5_RUNTYPE=1
set MARATHON_RUN_5_COMPONENT=13
set MARATHON_RUN_5_SCRIPT=runlinqtestsU.bat
set MARATHON_RUN_5_ISCFLAGS=/debug+ /o+

set MARATHON_RUN_6_GUID=laf
set MARATHON_RUN_6_RUNTYPE=1
set MARATHON_RUN_6_COMPONENT=14
set MARATHON_RUN_6_SCRIPT=runlaftestsU.bat
set MARATHON_RUN_6_ISCFLAGS=/debug+ /o+
set MARATHON_VBLTYPE=ret
set PRODUCTTESTLOCATION=\\vcslabsrv\drops\Orcas\vcs_comp\tst\current\qa\md

set SDTREE=%PRODUCTTESTLOCATION%\src\vcs\Compiler
set ROBOCOPYDIR=%SDTREE%\testenv\bin\x86
set MARATHON_FILES=%SDTREE%\testenv\bin

call %MARATHON_FILES%\MasterGet.bat "alink csee csharp csharp3 corelinq laf"

set /a MARATHON_RUN_COUNT=%MARATHON_RUN_COUNT%-1
for /L %%i in (0,1,%MARATHON_RUN_COUNT%) DO call :RUNCOMPONENT %%i
for %%i in (1,3) DO call :RUNCOMPONENT %%i AST
for %%i in (1,3) DO call :RUNCOMPONENT %%i REFACTOR
for %%i in (3) DO call :RUNCOMPONENT %%i QUERYAST

goto :lTheEnd

:RUNCOMPONENT
if defined iSCFLAGS (set OldiSCFLAGS=%iSCFLAGS%)
call set MARATHON_COMP_ID=%%MARATHON_RUN_%1_COMPONENT%%
call set iSCFLAGS=%%MARATHON_RUN_%1_ISCFLAGS%%

REM == Overwrite settings if this is AST/REF
if "%2"=="AST" (
	set COMPILE_ONLY=1
	set iSCFLAGS=/debug+ /o+ /test:AST=ast.xml
)
if "%2"=="REFACTOR" (
	set COMPILE_ONLY=1
	set iSCFLAGS=/debug+ /o+ /test:refactor
)
if "%2"=="QUERYAST" (
	set COMPILE_ONLY=1
	set iSCFLAGS=/debug+ /o+ /test:QUERYAST=ast.xml
)

REM Set TESTROOT to be the "test working dir"
REM It's a good idea to use the VCDriver token, instead of assuming
REM something like D:\SCHOOL.
set TESTROOT=D:\school\

REM Delete unwanted old results... just in case
IF EXIST %TESTROOT%\results.log del %TESTROOT%\results.log
IF EXIST %TESTROOT%\runpl.log del %TESTROOT%\runpl.log

set X_RUNALL_ARGS=-prehook %TESTROOT%testenv\bin\cc_prehook.pl -posthook %TESTROOT%testenv\bin\cc_posthook.pl

call cmd /c %MARATHON_FILES%\%%MARATHON_RUN_%1_SCRIPT%% -guid %%MARATHON_RUN_%1_GUID%%

if defined OldiSCFLAGS (
	set iSCFLAGS=%OldiSCFLAGS%
	set OldiSCFLAGS=
)
:lTheEnd

REM == Collect left-overs...
call %TESTROOT%testenv\bin\CC_CollectData.bat