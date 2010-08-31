@REM
@REM Finds the location of a file by searching the PATH environment variable. Finds only the 1st file on the path if more than one exists.
@REM This was originally from \\TKFilToolBox\Tools\21803 and I just extended it to include specifying an environment variable.
@REM
@if "%1"=="" goto HELP
@if not "%2"=="" goto SetEnvVar
@for /f "usebackq" %%f in ('%1') do @if "%%~$PATH:f"=="" (@echo File %1 not found in path.) else (@echo %%~$PATH:f)
@goto EndScript
:SetEnvVar
@for /f "usebackq" %%f in ('%1') do @if "%%~$PATH:f"=="" (@echo File %1 not found in path.) else (set %2=%%~$PATH:f)

@goto EndScript

:HELP
@echo Whereis.cmd FileName [EnvVariable]

:EndScript