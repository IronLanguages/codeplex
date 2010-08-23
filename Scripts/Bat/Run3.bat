set _BAT=%~dp0
set _ROOT=%_BAT:~0,-13%
set _FLAVOR=Testing
%_ROOT%\Scripts\Bat\BuildRowan.cmd /p:Configuration="Debug" /t:Rebuild /p:OutputPath="%_ROOT%\Bin\%_FLAVOR%"
set DLR_BIN=%_ROOT%\Bin\%_FLAVOR%
set DLR_ROOT=%_ROOT%

REM Do not just close the command window to cancel the run; instead, 
REM press Ctrl-C, and type exit to quit

pushd %_ROOT%\Test\Scripts
start "run iron tests" /low call %DLR_BIN%\ipy.exe RunTests.py Languages\IP\builtinfuncs Languages\IP\builtintypes Languages\IP\console Languages\IP\hosting Languages\IP\modules Languages\IP\netinterop Languages\IP\stress

REM Sleep 10 seconds in order to avoid result log file conflicit
sleep 10

start "run compat tests -M:2" /low call %DLR_BIN%\ipy.exe RunTests.py -M:"-O -D -X:SaveAssemblies -X:AssembliesDir %TMP%" Languages/IP/cpy
sleep 10

start "run misc, lib and regress tests" /low call %DLR_BIN%\ipy.exe RunTests.py Languages\IP\cgcheck Languages\IP\Regress Languages\IP\pystone Languages\IP\parrot
sleep 10

start "run iron/standard tests" /low call %DLR_BIN%\ipy.exe RunTests.py Languages\IP\standard
popd