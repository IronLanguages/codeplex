@echo off
set TARGET_DIR=%DLR_ROOT%\Util\Internal\Silverlight\x86ret
set CURRENT_DIR=%CD%
set BUILD_LOG=%TARGET_DIR%\Build.log

echo Sync Main branch...
tf get $/DevDiv/Dev11/PU/MQPro/dlr /recursive

echo Clean up IronPython assemblies from working dir
del /q %TARGET_DIR%\app\.

echo =================
echo Build Silverlight...
echo =================
%DLR_ROOT%\Scripts\Bat\BuildRowan.cmd /p:Configuration="Silverlight3Release" /t:clean >%BUILD_LOG%
%DLR_ROOT%\Scripts\Bat\BuildRowan.cmd /p:Configuration="Silverlight3Release" /t:Rebuild >>%BUILD_LOG%

if %ERRORLEVEL% == 0 (
	echo Built.

	pushd %TARGET_DIR%

	echo ============================
	echo Install CoreCLR ...
	echo ============================
	call %DLR_ROOT%\test\scripts\elevate.bat %TARGET_DIR%\fxpsetup.cmd >>%BUILD_LOG%

	echo =============
	echo Run IronPython tests...
	echo =============
	call run_host.cmd /lang:py runtelestotests.py
	
	echo =============
	echo Run JS tests...
	echo =============
	for %%f in (%DLR_ROOT%\Languages\JS\Tests\*.js) do call run_host.cmd /lang:js "%%f"

	echo Done.
	goto END
)

:ERROR
echo ================================
echo Build error! Tests did not run! 
echo ================================
echo Check %BUILD_LOG% for details.

:END
pushd %CURRENT_DIR%
