REM == Script to populate the database

set BRANCH=%1
set BUILD=%2
set MDRUNID=%3

REM == Install Magellan Tools
REM cscript.exe \\tkfiltoolbox\PPRCTools\Magellan\4.60\RTM\magautoinstall.vbs /coverage full /DestDir "%SystemDrive%\CodeCov\Tools"

REM == Add Magellan Tools to the path
REM setx PATH "%PATH%;%SystemDrive%\CodeCov\Tools" /M
REM setx PATH "%PATH%;%SystemDrive%\CodeCov\Tools"

call cc_setup.bat %BRANCH% ret %BUILD% \\cpvsbuild\drops\orcas\%BRANCH%\raw\%BUILD%\binaries.x86cov\bin\i386\coverage\Store\ret\binaries.x86cov VCSSQL\CodeCoverage \\vcslabsrv\codecoverage

REM == Import data to Magellan DB

for /R \\vcslabsrv\CodeCoverage\%MDRUNID% %%f IN (*.covdata) DO @echo covdata /i "%%f" /importandmerge /db @"%CC_INIFILE%" /customver "%CC_CUSTOMVER%"

