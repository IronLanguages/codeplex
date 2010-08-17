setlocal ENABLEEXTENSIONS

set START_DIR=%1
set _WIX="%DLR_ROOT%\Util\Wix35"
set _UNZIP="%DLR_ROOT%\External\Tools\unzip.exe"
set ORIG_IRONPYTHONPATH=%IRONPYTHONPATH%
set IRONPYTHONPATH=%DLR_ROOT%\External.LCA_RESTRICTED\Languages\CPython\27\Lib

if "%PKG_MSIFILE%"=="" set PKG_MSIFILE=IronPython.msi
if "%DLR_BIN%"=="" set DLR_BIN=%DLR_ROOT%\Bin\Release

if not exist "%DLR_ROOT%\Languages\IronPython\Wix\MsiLicense.rtf" (
	echo Need MsiLicense.rtf to create an MSI.
	exit /b 1
)
copy %DLR_ROOT%\Languages\IronPython\Wix\MsiLicense.rtf %START_DIR%\

set TEMPSTDLIB=%TEMP%\%RANDOM%\StdLib
mkdir %TEMPSTDLIB%
REM HACK - importing certain standard lib modules requires the presence of python27.dll
REM        or an error message box requiring the user to click "OK" gets spawned.
if not exist "%DLR_ROOT%\Bin\Debug\python27.dll" (
  copy %DLR_ROOT%\External.LCA_RESTRICTED\Languages\IronPython\27\python27.dll %DLR_ROOT%\Bin\Debug\python27.dll
)

%DLR_ROOT%\Bin\Debug\ipy.exe %~dp0getModuleList.py %TEMPSTDLIB%
%_WIX%\heat dir %TEMPSTDLIB%\Lib -dr INSTALLDIR -gg -cg StdLibGroup -var var.SourceDir -o %~dp0StdLib.wxs
if not "%ERRORLEVEL%" == "0" (
	echo heat CPython standard lib failed!
	exit /b %ERRORLEVEL%
)

%_WIX%\heat dir %START_DIR%\Tools -dr INSTALLDIR -gg -cg ToolsGroup -var var.SourceDir -o %~dp0Tools.wxs
if not "%ERRORLEVEL%" == "0" (
	echo heat IronPython Tools failed!
	exit /b %ERRORLEVEL%
)

%_WIX%\heat dir %START_DIR%\Doc -dr INSTALLDIR -gg -cg DocGroup -var var.SourceDir -o %~dp0Doc.wxs
if not "%ERRORLEVEL%" == "0" (
	echo heat IronPython Documentation failed!
	exit /b %ERRORLEVEL%
)


set VSIXOUT=%START_DIR%\IpyTools
mkdir %VSIXOUT%
rem unzip the vsix to get it's file contents
%_UNZIP% -o %DLR_ROOT%\bin\Release\IronPythonTools.vsix -d %VSIXOUT%
rem refresh potentially signed binaries
copy /y %DLR_ROOT%\bin\Release\IronPythonTools.dll %VSIXOUT%
copy /y %DLR_ROOT%\bin\Release\IronPythonTools.Core.dll %VSIXOUT%

set ISVSIXOUT=%START_DIR%\IronStudio
mkdir %ISVSIXOUT%
rem unzip the vsix to get it's file contents
%_UNZIP% -o %DLR_ROOT%\bin\Release\IronStudio.vsix -d %ISVSIXOUT%
rem refresh potentially signed binaries
copy /y %DLR_ROOT%\bin\Release\IronStudio.dll %ISVSIXOUT%
copy /y %DLR_ROOT%\bin\Release\IronStudio.Core.dll %ISVSIXOUT%
copy /y %DLR_ROOT%\bin\Release\RemoteScriptFactory.exe %ISVSIXOUT%


%_WIX%\heat dir %VSIXOUT%\Templates -dr IpyToolsInstallDir -gg -cg ToolsTemplateGroup -var var.SourceDir -o %~dp0IpyTools.wxs

:BuildMSMs
%_WIX%\candle %~dp0DLR.wxs -ext WixNetFxExtension 
if not "%ERRORLEVEL%" == "0" (
	echo candle DLR.wxs failed!
	exit /b %ERRORLEVEL%
)

%_WIX%\candle %~dp0IronStudio.wxs
if not "%ERRORLEVEL%" == "0" (
	echo candle IronStudio.wxs failed!
	exit /b %ERRORLEVEL%
)

%_WIX%\candle %~dp0IpyRedist.wxs -ext WixNetFxExtension -ext WixUIExtension 
if not "%ERRORLEVEL%" == "0" (
	echo candle IpyRedist.wxs failed!
	exit /b %ERRORLEVEL%
)

%_WIX%\candle -dSourceDir=%START_DIR%\Lib %~dp0StdLib.wxs
if not "%ERRORLEVEL%" == "0" (
	echo candle IronPython.wxs failed!
	exit /b %ERRORLEVEL%
)

%_WIX%\light "-cultures:en-us" -b %START_DIR% -out DLR.msm DLR.wixobj -ext WixNetFxExtension -ext WixUIExtension 
if not "%ERRORLEVEL%" == "0" (
	echo light DLR.wixobj failed!
	exit /b %ERRORLEVEL%
)
%_WIX%\light "-cultures:en-us" -b %START_DIR% -out IronStudio.msm IronStudio.wixobj -ext WixNetFxExtension -ext WixUIExtension 
if not "%ERRORLEVEL%" == "0" (
	echo light IronStudio.wixobj failed!
	exit /b %ERRORLEVEL%
)
%_WIX%\light "-cultures:en-us" -b %START_DIR% -out IpyRedist.msm IpyRedist.wixobj StdLib.wixobj -ext WixNetFxExtension -ext WixUIExtension 
if not "%ERRORLEVEL%" == "0" (
	echo light IpyRedist.wixobj failed!
	exit /b %ERRORLEVEL%
)

%_WIX%\candle -dSourceDir=%START_DIR%\Tools %~dp0Tools.wxs
if not "%ERRORLEVEL%" == "0" (
	echo candle IronPython.wxs failed!
	exit /b %ERRORLEVEL%
)
%_WIX%\candle -dSourceDir=%START_DIR%\Doc %~dp0Doc.wxs
if not "%ERRORLEVEL%" == "0" (
	echo candle IronPython.wxs failed!
	exit /b %ERRORLEVEL%
)
%_WIX%\candle -dSourceDir=%VSIXOUT%\Templates %~dp0IpyTools.wxs
if not "%ERRORLEVEL%" == "0" (
	echo candle IronPython.wxs failed!
	exit /b %ERRORLEVEL%
)

%_WIX%\candle %~dp0IronPython.wxs %~dp0Core.wxs %~dp0Tutorial.wxs -ext WixNetFxExtension -ext WixVSExtension
if not "%ERRORLEVEL%" == "0" (
	echo candle IronPython.wxs failed!
	exit /b %ERRORLEVEL%
)
%_WIX%\light "-cultures:en-us" -b %START_DIR% -out %PKG_MSIFILE% IronPython.wixobj Core.wixobj Tutorial.wixobj StdLib.wixobj Tools.wixobj Doc.wixobj IpyTools.wixobj -ext WixNetFxExtension -ext WixUIExtension 
if not "%ERRORLEVEL%" == "0" (
	echo light IronPython.wixobj failed!
	exit /b %ERRORLEVEL%
)

set IRONPYTHONPATH=%ORIG_IRONPYTHONPATH%
endlocal
