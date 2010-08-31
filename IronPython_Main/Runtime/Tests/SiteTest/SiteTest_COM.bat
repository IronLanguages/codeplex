@setlocal enableextensions
%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe %DLR_ROOT%\Test\Scripts\install_dlrcom.ps1
if not "%errorlevel%"=="0" exit /b 1

if not defined DLR_BIN set DLR_BIN=%DLR_ROOT%\Bin\Debug
%DLR_BIN%\SiteTest.exe COM