@echo off
SETLOCAL
REM Note, on Vista, you may choose one of below options to run the tests
REM 1. Click OK on the consent dialogue once.
REM 2. Run it from command window running as administrator, no clicking is needed.
REM 3. Disable UAC prompt by running \\merlinsnap1\snap\snapsetup\lua\install_lua.bat. Not recommended.

if "%1" == "/?" goto USAGE
if "%1" == "-?" goto USAGE
if /i "%1" == "/h" goto USAGE
if /i "%1" == "-h" goto USAGE

rem by default, we build
set _NOBUILD=
if /i "%1" == "/nobuild" (
    set _NOBUILD=True
    shift /1
)

set _CONFIG=Debug
if /i "%1" == "/d" (
    set _CONFIG=Debug
    shift /1
)

if /i "%1" == "/r" (
    set _CONFIG=Release
    shift /1
)

if /i "%1" == "/v" (
    set _VERBOSE=True
    shift /1
)

:: Define a list of options here
set OPTIONS=1234567890x

REM if there are more arguments
if "%1" == "" goto ASK

REM check if run option is already given
set _valid=False
for %%i in (0 1 2 3 4 5 6 7 8 9 x) do (
    if /i "%%i" == "%1" (
        set _valid=True
    )
)
if "%_valid%" == "True" (
    echo %1 | Choice /c %OPTIONS% /N /M "You've selected run" > NUL
    shift /1
    goto RUNTEST
)

:ASK
REM if no option is selected, prompt for it
echo.
echo          ===================================
echo          Please select one of below options:
echo          ===================================
echo.
echo  0  Sanity suite                                        ^< 15 min
echo  1  Hosts tests (MerlinWeb, Silverlight)                ^< 15 min
echo  2  Languages (JS, RB)                                  ~  5 min
echo  3  DLR (call run3.bat)                                 ~ ?? h
echo  4  UNUSED
echo  5  SNAP legacy queue tests (iron1x, mweb, etc)         ~  6 h
echo  6  SNAP main queue tests                               ~ 20 h
echo  7  Full Rowan test suites (including IP 1.x)           ^> 20 h
echo  8  Linq expression trees                               ~ 5 - 20 min
echo  X  Run for your own test list
echo.

Choice /c %OPTIONS% /N /T 10 /D 0 /M "Please select one option (0 is default) :"

:RUNTEST
set _OPTION=%ERRORLEVEL%
set _Elevate="%Dlr_root%\test\scripts\elevate.bat"
set _Space= 
pushd %Dlr_root%\test\scripts

echo.
goto RUN_%_OPTION%

:RUN_1:
echo Run all hosts tests (MerlinWeb, Silverlight)...
echo Warning: Silverlight will be re-installed on your machine!
echo.
set __E=%_Elevate%
set __B=/b:120
set __L="MerlinWeb Silverlight"
goto CALLRUN0

:RUN_2:
echo Run non-IronPython Languages tests (JS, RB) ...
echo.
set __E=%_Space%
set __B=/b:100
set __L="Languages\JS Languages\Ruby"
goto CALLRUN0

:RUN_3:
echo Run DLR/IronPython tests. Calling run3.bat...
echo.
call %~dp0run3.bat
goto END

:RUN_5:
echo Run SNAP tests of Legacy queue ...
echo.
set __E=%_Space%
REM @TODO - Mechanism to build legacy
set __B=/b:000
set __L="Suites\Legacy"
goto END

:RUN_6:
echo Run SNAP tests of Main queue ...
echo.
set __E=%_Space%
set __B=/b:111
set __L="Suites\Sanity Suites\Checkin Volatile"
goto CALLRUN0

:RUN_7:
echo.
set __E=%_Elevate%
set __B=/b:111
set __L="Suites\Sanity Suites\Checkin Suites\Extended Suites\Legacy Volatile"
goto CALLRUN0

:RUN_8:
echo Run Linq expression tree tests. Calling RunLinq.bat...
echo.
call %~dp0RunLinq.bat
goto END

:RUN_10:
echo Run sanity suites ...
echo.
set __E=%_Space%
set __B=/b:111
set __L=%_Space%
rem set flag so that we know this is called by default option of run.bat
set _RUN0=TRUE
goto CALLRUN0

:RUN_11:
rem assume people don't pick tests that need to be elevated
rem if they do, they may click "Continue", which is not too bad.
rem Also we don't build, bur still run tests from TestBin
set __E=%_Space%
set __B=/b:000
if NOT [%1] == [] (
    set __L="%~1"
    goto CALLRUN0
)

rem if no option is given, ask for it:
echo Please enter your test list, which will be passed to RunTests.py:
set /p __L=" "
if "%__L%" == "" (
	echo Nothing to run. Done.
	goto END
)

rem add quotes:
set __L="%__L%"
goto CALLRUN0

:CALLRUN0
if defined _NOBUILD set __B=%_Space%
if defined _VERBOSE set __V=/v
rem Call %__E% "%~dp0run0.bat" %__V% %__B% %_CONFIG% %__L%
Call %__E% "%~dp0run0.bat" %__V% %__B% %_CONFIG% %__L%

goto END

:USAGE
echo A batch file for people to run Merlin tests on office
echo machine. Different options are provided.
echo.
echo Usages:
echo.
echo        Run [/nobuild] [/d^|/r] [/v] [0^|1^|2^|3^|4^|5^|6^|7^|x ["Test List"]]
echo.
echo        /nobuild    Do not build. Run tests from current bin.
echo.
echo        /d^|/r      Build flavor. Default is Debug.
echo             /d = Debug
echo             /r = Release
echo        /v          Verbose
echo.
echo        0  Sanity suite (default)
echo        1  Hosts tests (MerlinWeb, Silverlight)
echo        2  Languages (JS, RB)
echo        3  DLR (IronPython 2.x)
echo        4  UNUSED
echo        5  SNAP legacy queue tests (IronPython 1.x, MerlinWeb, etc)
echo        6  SNAP main queue tests
echo        7  Full Rowan test suites (excluding IP 1.x)
echo        X  Run for your own test list (Test list needs to be quoted.)

:END
endlocal 
popd