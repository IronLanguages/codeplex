if "%1" == "" (
    set BUILD_FLAVOR=Release
) else (
    set BUILD_FLAVOR=%1
)

:GenerateMSI
msbuild %DLR_ROOT%\Msi\Installer.proj /p:Configuration=%BUILD_FLAVOR%

:GenerateZip


set ZIPTEMPDIR=%TEMP%\ZipRelease
:retry
if EXIST "%ZIPTEMPDIR%" (
	set ZIPTEMPDIR=%TEMP%\ZipRelease%RANDOM%
	goto retry
)

set ZIPTEMPDIR=%ZIPTEMPDIR%\IronPython-2.7

mkdir  %ZIPTEMPDIR%
pushd %ZIPTEMPDIR%

mkdir Doc
xcopy /s %DLR_ROOT%\Languages\IronPython\Public\Doc\* Doc

mkdir Tools
xcopy /s %DLR_ROOT%\Languages\IronPython\Public\Tools\* Tools

mkdir Tutorial
xcopy /s %DLR_ROOT%\Languages\IronPython\Public\Tutorial\* Tutorial

mkdir Lib
msbuild %DLR_ROOT%\Languages\IronPython\StdLib\StdLib.pyproj /p:OutputPath=%ZIPTEMPDIR%\Lib /t:CopyFilesForZip

copy %DLR_ROOT%\Bin\%BUILD_FLAVOR%\Microsoft.Dynamic.dll .
copy %DLR_ROOT%\Bin\%BUILD_FLAVOR%\Microsoft.Scripting.dll .
copy %DLR_ROOT%\Bin\%BUILD_FLAVOR%\Microsoft.Scripting.Metadata.dll .

copy %DLR_ROOT%\Bin\%BUILD_FLAVOR%\ipy.exe .
copy %DLR_ROOT%\Bin\%BUILD_FLAVOR%\ipy64.exe .
copy %DLR_ROOT%\Bin\%BUILD_FLAVOR%\ipyw.exe .
copy %DLR_ROOT%\Bin\%BUILD_FLAVOR%\ipyw64.exe .
copy %DLR_ROOT%\Bin\%BUILD_FLAVOR%\IronPython.dll .
copy %DLR_ROOT%\Bin\%BUILD_FLAVOR%\IronPython.Modules.dll .

copy %DLR_ROOT%\Bin\%BUILD_FLAVOR%\IronPython.Modules.xml .
copy %DLR_ROOT%\Bin\%BUILD_FLAVOR%\IronPython.xml .

copy %DLR_ROOT%\Languages\IronPython\Public\License.html .
copy %DLR_ROOT%\Languages\IronPython\Public\License.rtf .
copy %DLR_ROOT%\Languages\IronPython\Public\Readme.html .

mkdir Silverlight\bin
copy %DLR_ROOT%\Bin\Silverlight4%BUILD_FLAVOR%\Microsoft.Dynamic.dll Silverlight\bin\
copy %DLR_ROOT%\Bin\Silverlight4%BUILD_FLAVOR%\Microsoft.Scripting.dll Silverlight\bin\
copy %DLR_ROOT%\Bin\Silverlight4%BUILD_FLAVOR%\Microsoft.Scripting.Silverlight.dll Silverlight\bin\
copy %DLR_ROOT%\Bin\Silverlight4%BUILD_FLAVOR%\Chiron.exe Silverlight\bin\
copy %DLR_ROOT%\Bin\Silverlight4%BUILD_FLAVOR%\Chiron.exe.config Silverlight\bin\

copy %DLR_ROOT%\Bin\Silverlight4%BUILD_FLAVOR%\IronPython.dll Silverlight\bin\
copy %DLR_ROOT%\Bin\Silverlight4%BUILD_FLAVOR%\IronPython.Modules.dll Silverlight\bin\

mkdir Silverlight\script
xcopy %DLR_ROOT%\Hosts\Silverlight\Public\script\* Silverlight\script

mkdir Silverlight\script\templates\python
xcopy /s %DLR_ROOT%\Hosts\Silverlight\Public\script\templates\python Silverlight\script\templates\python

del /Q %DLR_ROOT%\Bin\%BUILD_FLAVOR%\IronPython-Bin.zip
cd ..
%DLR_ROOT%\Util\Misc\zip -9 -r %DLR_ROOT%\Bin\%BUILD_FLAVOR%\IronPython-Bin.zip IronPython-2.7
cd /D %DLR_ROOT%\Bin\%BUILD_FLAVOR%\

dir %DLR_ROOT%\Bin\%BUILD_FLAVOR%\IronPython-Bin.zip

popd
:EXIT