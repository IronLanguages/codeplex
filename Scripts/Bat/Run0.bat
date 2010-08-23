:: This runs checkin suites by default
@echo off

if /i '%1' == '-?' goto USAGE
if /i '%1' == '/?' goto USAGE
if /i '%1' == '-h' goto USAGE
if /i '%1' == '/h' goto USAGE

set _BAT=%~dp0
set _ROOT=%_BAT:~0,-13%
set _LOG=%_ROOT%\Test\Scripts\build.log

REM by default we build "Debug" and test "Debug"
set _FLAVOR=Debug
set _CONFIG=Debug
set _BIN=bin
set _NeedToBuild=False
set _Build_Rowan=0
set _Build_Silverlight=0

REM default case, just run checkin for Debug build
if '%1' == '' goto RUNTEST

if /i "%1" == "/v" (
    set _VERBOSE=-V
    shift /1
)

REM Do we need to build? And which one?
set _ARG1=%1
if /i '%_ARG1:~0,3%' == '/b:' (
    rem expected /b:000, no build for 0 and build for non-0
    rem 3 bits ordered as below
    set _NeedToBuild=True
    if '%_ARG1:~3,1%' == '' (goto PARSED) else (set _Build_Rowan=%_ARG1:~3,1%)
    if '%_ARG1:~4,1%' == '' (goto PARSED) else (set _Build_Silverlight=%_ARG1:~4,1%)
    :PARSED
    shift /1
)

REM which flavor we should use?
if /i '%1' == 'Debug' (
    set _FLAVOR=Debug
    set _CONFIG=Debug
    shift /1
    if '%_NeedToBuild%' == 'False' goto RUNTEST
)

if /i '%1' == 'Release' (
    set _FLAVOR=Release
    set _CONFIG=Release
    shift /1
    if '%_NeedToBuild%' == 'False' goto RUNTEST
)

REM  ============
REM  build stuff
REM  ============
rem clean the log file
time /t >%_LOG%

:BuildRowan
if %_Build_Rowan%==0 goto BuildSilverlight

echo -------------------------------
echo Building Rowan "%_FLAVOR%"
echo -------------------------------
call %_ROOT%\Scripts\Bat\BuildRowan.cmd /p:Configuration="%_CONFIG%" /t:Clean >>%_LOG%
if "%ERRORLEVEL%" == "0" (
    echo Cleaned.
) else (
    echo Failed to clean. See %_LOG%
    goto END
)

call %_ROOT%\Scripts\Bat\BuildRowan.cmd /p:Configuration="%_CONFIG%" /t:Rebuild >>%_LOG%
if "%ERRORLEVEL%" == "0" (
    echo Built.
) else (
    echo Failed to build. See %_LOG%
    goto END
)

REM FxCop
call %_ROOT%\Scripts\Bat\Check.cmd >>%_LOG%

if "%ERRORLEVEL%" == "0" (
    echo FxCop passed.
) else (
    echo FxCop check failed. See %_LOG%
    goto END
)

:BuildSilverlight
if %_Build_Silverlight%==0 goto COPYBINARIES

echo -------------------
echo Building Silverlight...
echo -------------------
call %_ROOT%\Scripts\Bat\BuildRowan.cmd /p:Configuration="Silverlight3Release" /t:Rebuild >>%_LOG%
if "%ERRORLEVEL%" == "0" (
    echo Cleaned.
) else (
    echo Failed to clean. See %_LOG%
    goto END
)

call %_ROOT%\Scripts\Bat\BuildRowan.cmd /p:Configuration="Silverlight3Release" /t:Rebuild >>%_LOG%
if "%ERRORLEVEL%" == "0" (
    echo Built.
) else (
    echo Failed to build. See %_LOG%
    goto END
)

if %_Build_Silverlight%==1 goto COPYBINARIES
rem only install Silverlight conditionally:
echo ----------------------------------
echo Installing Silverlight build...
echo ----------------------------------
call %_ROOT%\Scripts\Bat\CopySL.bat install >>%_LOG%
if "%ERRORLEVEL%" == "0" (
    echo Done.
) else (
    echo Failed to install Silverlight under Program Files. See %_LOG%
    goto END
)

echo --------------------------------
echo Building SilverlightTestApp...
echo --------------------------------
call %_ROOT%\Hosts\Silverlight\Testsuites\setup\buildSilverlightTests.bat >>%_LOG%
if "%ERRORLEVEL%" == "0" (
    echo Built.
) else (
    echo Failed to build SilverlightTestApp. See %_LOG%
    goto END
)

:COPYBINARIES
set _BIN=TestBin
echo ------------------------------------------------------------------------
echo  Copy the Rowan binaries from bin\%_FLAVOR% to %_BIN%\%_FLAVOR%
echo ------------------------------------------------------------------------
rem   We only copy the flavor that was just built, Debug or Release.
rem   For Silverlight, since they don't run from bin anyway. Leave it there.
xcopy %_ROOT%\Bin\%_FLAVOR% %_ROOT%\%_BIN%\%_FLAVOR% /Y /S /I >> %_LOG%
if "%ERRORLEVEL%" == "0" (
    echo Copied bits to %_ROOT%\%_BIN%\%_FLAVOR%
) else (
    echo Failed to copy bits into %_BIN%. See %_LOG%
    goto END
)


:RUNTEST
echo ------------------------
echo Ready to run tests ...
echo ------------------------
pushd %_ROOT%\Test\Scripts
set DLR_BIN=%_ROOT%\%_BIN%\%_FLAVOR%

if [%1]==[] (
    set _Options=-M:"" -logPrefix:run0 Suites/Sanity
) else (
    set _Options=%~1
)

echo Binary location is %DLR_BIN%
echo Running "ipy -X:ExceptionDetail RunTests.py %_VERBOSE% %_Options%" in
cd
call %DLR_BIN%\ipy.exe -X:ExceptionDetail RunTests.py %_VERBOSE% %_Options%

popd
goto END

:USAGE
echo Internal helper batch file for Run.bat. 
echo.
echo Usage:
echo.
echo    Run0 [/b:[###]] [Debug^|Release] ["Test List"]
echo.
echo    [/b:[###]]     Build options. 
echo.
echo                   1st digit for Rowan
echo                   2nd digit for Silverlight
echo                   3rd digit for Nessie
echo                       0 = no Build
echo                       1 = Clean and Build
echo.
echo                   Tests run from TestBin when /b is used.
echo                   Default, no build and tests run from bin.
echo.
echo    [Debug^|Release]     Build flavor. Default is Debug.
echo.
echo    ["Test List"]        Options for RunTests.py. 
echo                         Need to be quoted when multiple options provided.
echo                         Default is "checkin".
echo.
echo.

:END
