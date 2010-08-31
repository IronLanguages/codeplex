REM Set the following variable (typically this is done by automation)
REM    set CC_CUSTOMVER=vcs_comp.21030.2848486.i386
REM    set CC_INIFILE=\\vcslabsrv\codecoverage\vcs_comp.21030.2848486.i386\CCDB.ini
REM    set Coverage=D:\CodCov\Data  <= set by Maddog Magellan Package

REM Also make sure that:
REM == the executing account has r/w permissiono on the DB
REM == make sure the magellan tools are in the PATH 

for /R %Coverage% %%f IN (*.covdata) DO covdata /i "%%f" /importandmerge /db @"%CC_INIFILE%" /customver "%CC_CUSTOMVER%"
