REM == ==
REM == This script is invoked by the Tier machine setup...
REM == ==

set TESTSOURCESLOCATION=%1
set WORKDIR=%2
set RUNAUTOMATION=%WORKDIR%\runautomation.bat
set SDTREE=%TESTSOURCESLOCATION%\src\vcs\Compiler
set ROBOCOPYDIR=%SDTREE%\testenv\bin\x86

REM == Abort if TESTSOURCESLOCATION/workdir are not defined!
REM == FILESHARE is something like \\vcslabsrv\drops\orcas\vcs_comp\tst\20128.01\qa\md
REM == WORKDIR is something like D:\SCHOOL (maddog token <%WOR%>)
IF "%TESTSOURCESLOCATION%"=="" pause
IF "%WORKDIR%"=="" pause

REM == Create RUNAUTOMATION file
REM == This will be invoked the next time the machine
REM == reboots and the user log on
REM == This is needed because, for some reason,
REM == Maddog or the Univ SxS installer reboot the machine
REM == when done installing packages

echo>%RUNAUTOMATION% REM == Execute Csharp automation
echo>>%RUNAUTOMATION% IF "%%1"=="" start cmd /K %%0 ok ^&^& goto :eof
echo>>%RUNAUTOMATION% ping -n 1 -w 30000 1.1.1.1
echo>>%RUNAUTOMATION% REM == Enable fire sharing (VIsta and above)
echo>>%RUNAUTOMATION% netsh firewall set service FILEANDPRINT ENABLE
echo>>%RUNAUTOMATION% set SDTREE=%SDTREE%
echo>>%RUNAUTOMATION% set ROBOCOPYDIR=%ROBOCOPYDIR%
echo>>%RUNAUTOMATION% pushd %WORKDIR%
echo>>%RUNAUTOMATION% IF "%iSCFLAGS%"=="" set iSCFLAGS=/debug+ /o+
echo>>%RUNAUTOMATION% call %SDTREE%\testenv\bin\MasterGet.bat CSHARP
echo>>%RUNAUTOMATION% call %SDTREE%\testenv\bin\MasterRun.bat -cross MADDOG_CS -component csharp -mode release -runallargs "-nottags:watson,CPP,SDK,CPPEE,diags,stress,NETFX35"
echo>>%RUNAUTOMATION% popd

REG ADD HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce /v RunAutomation /t REG_SZ /d %RUNAUTOMATION% /f
