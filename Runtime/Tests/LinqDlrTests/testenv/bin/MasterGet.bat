setlocal

set LOGFILE=%temp%\mastergetdebug.log

time /t >>%LOGFILE%

REM == Common files

REM == Find out OS architecture, no matter what cmd prompt
REM == we are invoked from. It will work on Win9X as well.
SET OSARCH=%PROCESSOR_ARCHITECTURE%
IF "%PROCESSOR_ARCHITEW6432%"=="" GOTO lOsArchDone
SET OSARCH=%PROCESSOR_ARCHITEW6432%

:lOsArchDone
IF "%OSARCH%"=="" set OSARCH=x86
ECHO OSARCH=%OSARCH%

REM == csharp | csee | cseeremote | dlinq | corelinq | alink | coreaccept | laf | asmmeta | csharpstress | Fsharp
REM == For example: (csharp csee)
set COMPONENTS=%1

REM == For now, we do not have contexts, so X_WOWRUN is going to be set based on
REM == the env variables set by marathon. Logic is as follows:
REM == If MARATHON_WOW is defined (most likely set to -wow), then this could be
REM == either a CROSSRUN (if %CROSSRUNS% is defined) or a pure WOW (otherwise).
IF NOT "%MARATHON_WOW%"=="" (
    set X_WOWRUN=0
    IF NOT "%CROSSRUNS%"=="" set X_WOWRUN=%CROSSRUNS%
)

REM ==
REM == X_WOWRUN
REM == For C#:
REM ==  - undefined --> native 
REM ==  - 0         --> pure wow64  (compiler=32bit, runtime=32bit, tools=32bit)
REM ==  - 1         --> mixed wow64 (compiler=32bit, runtime=64bit, tools=64bit) 
REM ==  - 2         --> mixed wow64 (compiler=64bit, runtime=32bit, tools=32bit)
IF "%X_WOWRUN%"==""               set RUNARCH=%OSARCH%
IF "%X_WOWRUN%"=="0"              set RUNARCH=x86
IF "%X_WOWRUN%"=="2"              set RUNARCH=x86
IF "%X_WOWRUN%"=="1"              set RUNARCH=%OSARCH%

ECHO>>%LOGFILE% === MasterGet.bat debug ===
ECHO>>%LOGFILE% RUNARCH=%RUNARCH%
ECHO>>%LOGFILE% X_WOWRUN=%X_WOWRUN%
ECHO>>%LOGFILE% MARATHON_WOW=%MARATHON_WOW%
ECHO>>%LOGFILE% CROSSRUNS=%CROSSRUNS%

REM == Should be updated and/or automated by SE when we start having SPs for NDP...
REM set NDP20SP=0

REM == Set COPYCMP (on Vista, this is a system command so we don't need our own robocopy)
set COPYCMD=ROBOCOPY.exe /R:10 /W:60 /MIR /NC /NS /NFL
where ROBOCOPY.EXE
IF ERRORLEVEL 1 set COPYCMD=%ROBOCOPYDIR%\%COPYCMD%

IF NOT "%ISADMIN%"=="FALSE"     set COPYCMD=%COPYCMD% /ZB /LOG+:%TEMP%\RoboCopy_%RANDOM%.log
IF     "%ISADMIN%"=="FALSE"     set COPYCMD=%COPYCMD%     /LOG+:%TEMP%\RoboCopy_%RANDOM%.log

REM ==
REM == Take care of 'testenv' folder
REM ==
  %COPYCMD:/MIR=% /LEV:0 %SDTREE%\testenv\bin          testenv\bin
  %COPYCMD%              %SDTREE%\testenv\bin\x86      testenv\bin\x86
  IF NOT "%OSARCH%"=="x86" %COPYCMD% %SDTREE%\testenv\bin\%OSARCH% testenv\bin\%OSARCH%

REM ==
REM == Copy necessary tools from SuiteBin folder
REM == (%TESTREQBINPATH%) to testenv\bin
REM ==
  IF EXIST %TESTREQBINPATH% %COPYCMD:/MIR=%  "%TESTREQBINPATH%\i386" testenv\bin  COLOADERLib.dll glass2.exe glass2.pdb Microsoft.VisualStudio.Debugger.Interop.dll Microsoft.VisualStudio.Debugger.Interop.Internal.dll

REM ==
REM == Take care of copying misc tool folders
REM ==
  %COPYCMD%       /E %SDTREE%\toolsrc            toolsrc

REM == Remove double quotes
FOR %%i IN (%COMPONENTS%) DO set XCOMPONENTS=%%~i

REM == Everything with /MIR below is duplicated
REM == I do not know what, but it looks like robocopy misses some files
REM == the first time... very weird...
FOR %%c IN (%XCOMPONENTS%) DO (

    IF /I %%c==asmmeta %COPYCMD:/MIR=% /LEV:0  %SDTREE%\CSHARP\source                        CSHARP\source

    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\CSHARP\source\Conformance\friend     CSHARP\source\Conformance\friend
    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\CSHARP\source\Conformance\friend     CSHARP\source\Conformance\friend
    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\CSHARP\source\Conformance\attributes CSHARP\source\Conformance\attributes
    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\CSHARP\source\Conformance\attributes CSHARP\source\Conformance\attributes
    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\CSHARP\source\Conformance\generics   CSHARP\source\Conformance\generics
    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\CSHARP\source\Conformance\generics   CSHARP\source\Conformance\generics
    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\CSHARP\source\clrfx                  CSHARP\source\clrfx
    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\CSHARP\source\clrfx                  CSHARP\source\clrfx
    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\testenv\sniff                        testenv\sniff
    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\testenv\sniff                        testenv\sniff
    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\%%c                                  %%c
    IF /I %%c==asmmeta %COPYCMD% /E            %SDTREE%\%%c                                  %%c

    IF /I %%c==ALINK  %COPYCMD% /E %SDTREE%\%%c\source                %%c\source

    IF /I %%c==DLINQ  %COPYCMD% /E %SDTREE%\..\%%c                    %%c
    IF /I %%c==DLINQ  %COPYCMD% /E %SDTREE%\..\%%c                    %%c

    IF /I %%c==CORELINQ   %COPYCMD% /E %SDTREE%\%%c\source            %%c\source

    IF /I %%c==CSHARP %COPYCMD% /E %SDTREE%\%%c\source                %%c\source
    IF /I %%c==CSHARP %COPYCMD% /E %SDTREE%\%%c\source                %%c\source
    IF /I %%c==CSHARP %COPYCMD% /E %SDTREE%\%%c\acceptHarness         %%c\acceptHarness
    IF /I %%c==CSHARP %COPYCMD% /E %SDTREE%\%%c\acceptHarness         %%c\acceptHarness

    IF /I %%c==CSHARPSTRESS %COPYCMD% /E %SDTREE%\%%c\source          %%c\source

    IF /I %%c==CSHARP3 %COPYCMD% /E %SDTREE%\%%c\source                %%c\source
    IF /I %%c==CSHARP3 %COPYCMD% /E %SDTREE%\%%c\source                %%c\source
    IF /I %%c==CSHARP3 %COPYCMD% /E %SDTREE%\%%c\acceptHarness         %%c\acceptHarness
    IF /I %%c==CSHARP3 %COPYCMD% /E %SDTREE%\%%c\acceptHarness         %%c\acceptHarness

    IF /I %%c==CSHARP4 %COPYCMD% /E %SDTREE%\%%c\source                %%c\source
    IF /I %%c==CSHARP4 %COPYCMD% /E %SDTREE%\%%c\source                %%c\source
 
    IF /I %%c==coreaccept %COPYCMD% /E              %SDTREE%\CSHARP\source                       CSHARP\source
    IF /I %%c==coreaccept %COPYCMD% /E              %SDTREE%\CSHARP\source                       CSHARP\source
    IF /I %%c==coreaccept %COPYCMD:/MIR=% /E /LEV:0 %SDTREE%\CSHARP\acceptHarness                CSHARP\acceptHarness

    IF /I %%c==CSEE   %COPYCMD% /E              %SDTREE%\%%c\source                  %%c\source
    IF /I %%c==CSEE   %COPYCMD% /E              %SDTREE%\%%c\source                  %%c\source

    IF /I %%c==CSEEREMOTE   %COPYCMD% /E              %SDTREE%\CSEE\source                  CSEE\source
    IF /I %%c==CSEEREMOTE   %COPYCMD% /E              %SDTREE%\CSEE\source                  CSEE\source

    IF /I %%c==LAF    %COPYCMD% /E %SDTREE%\%%c\source                %%c\source

    IF /I %%c==FSHARP    %COPYCMD% /E %FSHARPTREE%\tests                %%c\tests
    IF /I %%c==FSHARP    %COPYCMD% /E %FSHARPTOOLS%\perf                tools\perf
    IF /I %%c==FSHARP    %COPYCMD% /E %FSHARPTOOLS%\win86               tools\win86
)

time /t >>%LOGFILE%