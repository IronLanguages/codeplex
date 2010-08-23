set _BAT=%~dp0
set _ROOT=%_BAT:~0,-13%
set _FLAVOR=Testing

REM cmd /c %_ROOT%\Scripts\Bat\BuildRowan.cmd /p:Configuration="Debug" /t:Rebuild /p:OutputPath="%_ROOT%\Bin\%_FLAVOR%"

set DLR_BIN=%_ROOT%\Bin\%_FLAVOR%
set DLR_ROOT=%_ROOT%


REM Do not just close the command window to cancel the run; instead, 
REM press Ctrl-C, and type exit to quit

pushd %_ROOT%\Runtime\Tests\LinqDlrTests\corelinq\Source

REM cmd /c RunAdd.bat
cmd /c RunTrees4.bat

start .

popd

