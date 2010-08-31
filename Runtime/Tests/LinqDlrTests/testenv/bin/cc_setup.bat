REM == Run this if you need Magellan!
REM set CC_COMP_DIR=D:\CodeCov\Tools
REM cscript.exe \\tkfiltoolbox\PPRCTools\Magellan\4.60\RTM\magautoinstall.vbs /coverage full /DestDir "%CC_COMP_DIR%"


REM ==
REM == cc_setup.bat Orcas/feature/VCS_COMP ret 20310.00 \\cpvsbuild\drops\orcas\vcs_comp\raw\20310.00\binaries.x86cov\bin\i386\coverage\Store\ret\binaries.x86cov MATTEOT25C-X64 \\vcslabsrv\codecoverage
REM ==

REM == Find out what lab we are using. We strip away all the interemediate components...
REM == so something like Orcas/pu/VCS will become VCS.
set BRANCH=%1
set BTYPESHORT=%2
set BUILD=%3
set ARCH=i386
set BINDROP=%4
REM set BINDROP=\\cpvsbuild\drops\orcas\vcs_comp\raw\%BUILD%\binaries.x86cov\bin\%ARCH%\coverage\Store\%BTYPESHORT%\binaries.x86cov
set CC_SERVER=%5
set CC_SHARE=%6

set BRANCH_SHORT=%BRANCH:/= %
for %%c IN (%BRANCH_SHORT%) DO set BRANCH_SHORT=%%c
set CC_DATABASE_LABEL=%BRANCH_SHORT%.%BUILD%.%ARCH%

set CC_INIFILE=%CC_SHARE%\%CC_DATABASE_LABEL%\CCDB.ini
set CC_INSTBINLOC=%CC_SHARE%\%CC_DATABASE_LABEL%
set CC_COMP_DIR=D:\CodeCov\Tools
set CC_CUSTOMVER=%CC_DATABASE_LABEL%

IF EXIST %CC_INSTBINLOC% goto :eof

mkdir %CC_INSTBINLOC%

cscript //nologo %~d0%~p0cc_createdb.js %CC_SERVER% %CC_DATABASE_LABEL% %CC_INIFILE%

copy %BINDROP%\bin\%ARCH%\al.exe %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\al.pdb %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\csc.exe %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\csc.pdb %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\cscompee.dll %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\cscompee.pdb %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\Microsoft.VisualStudio.DebuggerVisualizers.dll %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\Microsoft.VisualStudio.DebuggerVisualizers.pdb %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\Visualizers\Original\autoexpce.dll %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\Visualizers\Original\autoexpce.pdb %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\vcsexpresspkgs\vcsexpressmnu.dll %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\vcsexpresspkgs\vcsexpressmnu.pdb %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\vcsexpress.exe %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\vcsexpress.pdb %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\vc7\vcpackages\cslangsvc.dll %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\vc7\vcpackages\cslangsvc.pdb %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\csformatui.dll %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\csformatui.pdb %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\microsoft.visualstudio.csharp.services.language.dll %CC_INSTBINLOC%\
copy %BINDROP%\bin\%ARCH%\microsoft.visualstudio.csharp.services.language.pdb %CC_INSTBINLOC%\
copy %BINDROP%\sqlmetal.exe %CC_INSTBINLOC%\
copy %BINDROP%\sqlmetal.pdb %CC_INSTBINLOC%\
copy %BINDROP%\System.Core.dll %CC_INSTBINLOC%\
copy %BINDROP%\System.Core.pdb %CC_INSTBINLOC%\
copy %BINDROP%\System.Data.Linq.Design.dll %CC_INSTBINLOC%\
copy %BINDROP%\System.Data.Linq.Design.pdb %CC_INSTBINLOC%\
copy %BINDROP%\System.Data.Linq.dll %CC_INSTBINLOC%\
copy %BINDROP%\System.Data.Linq.pdb %CC_INSTBINLOC%\

%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\al.exe       /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\csc.exe      /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\cscompee.dll /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\Microsoft.VisualStudio.DebuggerVisualizers.dll /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\autoexpce.dll /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\System.Core.dll /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\System.Data.Linq.Design.dll /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\System.Data.Linq.dll /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\sqlmetal.exe /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\vcsexpressmnu.dll /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\vcsexpress.exe /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\cslangsvc.dll /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\csformatui.dll /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
%CC_COMP_DIR%\bbcover /i %CC_INSTBINLOC%\microsoft.visualstudio.csharp.services.language.dll /ImportOnly /customver "%CC_CUSTOMVER%" /db @"%CC_INIFILE%"
