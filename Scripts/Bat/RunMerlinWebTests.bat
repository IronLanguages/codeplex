@echo off
set CURRENTDIR=%CD%
set DEBUGONLY=No
pushd %DLR_ROOT%\Hosts\MerlinWeb\Test\TestSuites

echo Sync Main branch ...
tf get $/DevDiv/Dev11/PU/MQPro/dlr /recursive

if "%1"=="d" set DEBUGONLY=Y
if "%1"=="/d" set DEBUGONLY=Y
if "%DEBUGONLY%"=="Y" goto TEST_DEBUG

if "%1"=="r" goto BUILD_RUN_RELEASE
if "%1"=="/r" goto BUILD_RUN_RELEASE

:TEST_DEBUG
echo Clean and build Rowan debug... 
%DLR_ROOT%\Scripts\Bat\BuildRowan.cmd /t:clean /p:Configuration="Debug" >>RowanClean.log
%DLR_ROOT%\Scripts\Bat\BuildRowan.cmd /p:Configuration="Debug" >>RowanBuild.log

if %ERRORLEVEL% == 0 (
	echo ========================================
	echo Run MerlinWeb tests for DEBUG build ...
	echo ========================================
	%DLR_ROOT%\Bin\Debug\ipy.exe mwt_run.py
) else (
	goto ERROR
)

if "%DEBUGONLY%"=="Y" goto END
	
:BUILD_RUN_RELEASE
rem set /p RUNTEST="Run tests for Release build(y/n)?"
rem if "%RUNTEST%"=="y" (

echo Clean and build Rowan release... 
%DLR_ROOT%\Scripts\Bat\BuildRowan.cmd /t:clean /p:Configuration="Release" >>RowanClean.log
%DLR_ROOT%\Scripts\Bat\BuildRowan.cmd /nologo /p:Platform="Any CPU" /p:Configuration="Release" >>RowanBuild.log

if %ERRORLEVEL% == 0 (
	echo =========================================
	echo Run MerlinWeb tests for RELEASE build ...
	echo =========================================
	%DLR_ROOT%\Bin\Release\ipy.exe mwt_run.py
	goto END
)

:ERROR
echo ================================
echo Build error! Tests did not run! 
echo ================================
echo See %CD%\RowanBuild.log for details.

:END
pushd %CURRENTDIR%