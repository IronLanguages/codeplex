call :SETTIMESTAMP
set LOGFILE=%temp%\masterrundebug_%TIMESTAMP%.log

set > %LOGFILE% 
@echo OFF
@if not "%ECHO%" == "" echo %ECHO%

@title %*
rem /////////////////////////////////////////////////////////////////////////////
rem // Parameter to pass are: 
rem // Required: X_COMPONENT (CSEEREMOTE, CSEE, ALINK, CSHARP, CSHARP3, CSHARP4, LAF, CORELINQ, ASMMETAF, CSHARPSTRESS, FSHARP)
rem // 		 X_MODE (MARATHON, RELEASE, MADDOG_CS)
rem // Optional: X_NORUN (set to 1 if just want setup but do not want to run)
rem // 	         X_DEBUG (set to 1 to print all cammands)
rem //  	 X_WOWRUN (set to indicate WOW run)
rem //  	 X_IGNORESKIPRULES (if defined, then ignore SkipRules (RELEASE only runs)
rem //           X_BRANCH (VBL/Branch, e.g. Main)
rem //           X_VERSION (20222.01)
rem // 
rem // For NETCF runs make sure the following env variable are defined (in either marathon or maddog)
rem //     MARATHON_NETCF_WIN32_DIR      (e.g. \\netcfdist\drop\latest\dev\Win32x86\bin\release)
rem //     MARATHON_NETCF_MANAGED_DIR    (e.g. \\netcfdist\drop\latest\dev\Profile4\BCL\bin\release)
rem //
rem // For Compiler PRIVATE runs runs make sure the following env variable are defined (in either marathon or maddog)
rem //     MARATHON_CSC_PATH
rem ///////////////////////////////////////////////////////////////////////////

rem Set Globals
if not defined TESTDRIVE set TESTDRIVE=%SystemDrive%
if not defined TESTROOT set TESTROOT=%TESTDRIVE%\school

ECHO>>%LOGFILE% === masterrundebug.log (beginning) ===

rem /////////////////////////////////////////////////////////////////////////////
rem // Check requirements

@REM COMMAND LINE PROCESSING
@REM ========================
@REM Here we iterate through all arguments and process it into env. vars
@REM After that we validate their values and display an error plus
@REM usage if it's not OK. 
:NEXT_ARG
if "%1"=="" goto done_arg
    echo %1 %2
	if /i "%1"=="-component" (
        shift /1
        set X_COMPONENT=%2
    ) else if /i "%1"=="-mode" (
        shift /1
        set X_MODE=%2
    ) else if /i "%1"=="-branch" (
        shift /1
        set X_BRANCH=%2
    ) else if /i "%1"=="-version" (
        shift /1
        set X_VERSION=%2
    ) else if /i "%1"=="-custsetup" (
        shift /1
        set X_CUSTOMSETUP=%2
    ) else if /i "%1"=="-custclean" (
        shift /1
        set X_CUSTOMCLEANUP=%2
    ) else if /i "%1"=="-debug" (
        set X_DEBUG=1
    ) else if /i "%1"=="-wow" (
        set X_WOWRUN=0
    ) else if /i "%1"=="-guid" (
        shift /1
        set X_GUID=%2
    ) else if /i "%1"=="-runroot" (
        shift /1
        set X_RUNROOT=%2
    ) else if /i "%1"=="-norun" (
        set X_NORUN=TRUE
    ) else if /i "%1"=="-runallargs" (
        shift /1
        set X_RUNALL_ARGS=%2
    ) else if /i "%1"=="-nosdk" (
        set X_NOSDK=TRUE
    ) else if /i "%1"=="-nocpp" (
        set X_NOCPP=TRUE
    ) else if /i "%1"=="-cross" (
        shift /1
	set X_CROSS_RUN=%2
    ) else if /i "%1"=="-?" (
        call :USAGE
    ) else if /i "%1"=="/?" (
        call :USAGE
    ) else if /i "%1"=="-help" (
        call :USAGE
    ) else if /i "%1"=="-h" (
        call :USAGE
    )
shift /1
goto :NEXT_ARG
:done_arg

rem // Do this so we can pass multiple comma delimited parameters
if defined X_RUNALL_ARGS (
  set X_RUNALL_ARGS=%X_RUNALL_ARGS:"=%
)
set X_

rem //////////////
set X_DEBUG=1
rem /////////////


rem Admin by default
if not defined UserType set UserType=0  

rem nu?
if /i "%ISADMIN%"=="false" set UserType=1

rem CrossRun is 0 by default
if not defined CrossRun set CrossRun=0

if "%X_WOWRUN%"=="0" set CrossRun=0
if "%X_WOWRUN%"=="1" set CrossRun=1
if "%X_WOWRUN%"=="2" set CrossRun=2

rem CCoverage is 0 by default
if not defined CCoverage set CCoverage=0  

if "%CodeCoverage%"=="1" set CCoverage=1


if not defined X_COMPONENT (
	echo Must set X_COMPONENT to one of: ^[ CSEEREMOTE CSEE ALINK CSHARP CSHARP3 CSHARP4 DLINQ CORELINQ ASMMETAF LAF FSHARP^]
	pause
	goto :EOF
)
if not defined X_MODE (
	echo Must set X_MODE or use -mode ^[ MARATHON RELEASE MADDOG_CS ^]
	pause
	goto :EOF
)

if /i "%X_MODE%" == "MARATHON" (
	if not defined X_GUID (
		call :ERROR Must set X_GUID in MARATHON mode.
	)
	if not defined MARATHON_LOG_SERVER (
		call :ERROR Must set MARATHON_LOG_SERVER in MARATHON mode.
	)
)

if /i "%X_MODE%" == "RELEASE" (
	IF NOT defined simulator_pipe set simulator_pipe=stopit.exe -g -s300 -pError##OK
)
rem /////////////////////////////////////////////////////////////////////////////

rem Set local scope and call MAIN
if not defined X_NORUN (
	setlocal & setlocal ENABLEDELAYEDEXPANSION & pushd & set RET=
) else (
 	color 4e
)
    rem setting admin pipe to a space, make sure there is a space or things break
    if not defined ADMIN_PIPE (
        if %UserType%==0 set ADMIN_PIPE= 
	if %UserType%==1 set ADMIN_PIPE=elevator.exe
    )

	set COMPLUS_LOGENABLE=
	set COMPLUS_DBGJITDEBUGLAUNCHSETTING=1
	set COMPLUS_LOGTOCONSOLE=0x1
	set COMPLUS_NOGUIONASSERT=1

	if "%X_DEBUG%"=="1" (set TRACE=echo) else (set TRACE=rem)
	if /i {%1}=={/?} (call :HELP %2) & (goto :HELPEXIT)
	call :MAIN %*
	:HELPEXIT

if defined X_NORUN (
	goto :EOF
)

popd & endlocal & set RET=%RET%

goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
rem HELP procedure
:HELP

if defined TRACE %TRACE% [proc %0 %*]
	echo Usage: ToDo:
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
rem // MAIN MAIN MAIN MAIN MAIN MAIN MAIN MAIN MAIN MAIN MAIN MAIN MAIN MAIN MAIN 
rem /////////////////////////////////////////////////////////////////////////////
:MAIN

if defined TRACE %TRACE% [proc %0 %*]
	cd /d %TESTROOT%

	call :SETCOMMONENVVARIABLES
	call :DELETELOGS
	call :GETPLATFORM
	call :COPYPVTCSC
	call :SETUPNETCF
	call :SETPATHS
	call :REGPRIVCOMP
	call :PLATFORM_SETUP
	call :SETGREENBITSPATH
	call :REMOTESETUP
	call :VERIFYBITS
	call :GETI_RUNROOT
	call :SET_COMPONENT_ID	
	call :CUSTSETUP
	if defined X_NORUN goto :EOF
	call :BUILDTOOLS
	call :RUNALL
        call :PLATFORM_CLEANUP
	call :CUSTCLEANUP
	call :COPYLOGS

goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
rem Delete result.log from D:\School before running the tests
:DELETELOGS

if defined TRACE %TRACE% [proc %0 %*]
	if exist "results.log" del results.log
	if exist "runpl.log" del runpl.log
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:SETPATHS
if defined TRACE %TRACE% [proc %0 %*]
	call :PREPENDTOPATH %TESTROOT%\testenv\bin;%TESTROOT%\testenv\bin\%RUNPLATFORM%;
	call :ADDTOPATH ;%TESTROOT%\bin
	if defined X_NOCPP (
		call :GETCLRSDKPATH
		goto :EOF
	)

	REM == Set WinSDK paths - in case WinSDK 6.0 is installed...
        REM == Normally, this is not installed and we rely on VS for
        REM == the platform SDK (or the VCTOOLS setup on IA64)
        REM == If VS is not installed, then it's ok and we won't set the
        REM == path to the VC compiler.
        REM == If WinSDK6.0 is installed, then we also set the path to
        REM == al.exe (32/64bit, as required by the run we are doing)
        REM == This setup is slightly different from the setup you'd get
        REM == by opening a WinSDK command prompt: the only component of 
        REM == the WinSDK we care about is ALINK - so we want to test
        REM == both 32 and 64 bit.

        REM == The official location of the WinSDK is in the registry
        REM == All we need to to is query the native registry...
        REM == The logic is a little bit more complex ("spawn64.exe")
        REM == because we might be executed by a 32bit shell...
        set WINSDK60PATHPREFIX=
        set NATIVEREGEXE=reg.exe
        IF NOT "%PROCESSOR_ARCHITEW6432%"=="" set NATIVEREGEXE=%TESTROOT%\testenv\bin\%OSARCH%\spawn64.exe reg.exe
        FOR /F "tokens=2* delims=	 " %%A IN ('%NATIVEREGEXE% QUERY "HKLM\Software\Microsoft\Microsoft SDKs\Windows" /v CurrentInstallFolder')  DO SET WINSDK60PATHPREFIX=%%B

        IF NOT EXIST "%WINSDK60PATHPREFIX%" goto :lWinSDKDone
        set VCPATHSUFFIX=

	IF /I "%OSARCH%"=="AMD64" IF /I "%RUNPLATFORM%"=="AMD64" set VCPATHSUFFIX=\x64
	IF /I "%OSARCH%"=="IA64"  IF /I "%RUNPLATFORM%"=="IA64"  set VCPATHSUFFIX=\ia64

        set XLIB=
        IF EXIST "%WINSDK60PATHPREFIX%\VC\lib%VCPATHSUFFIX%"  set XLIB=%XLIB%;%WINSDK60PATHPREFIX%\VC\lib%VCPATHSUFFIX%
        IF EXIST "%WINSDK60PATHPREFIX%\LIB%VCPATHSUFFIX%"     set XLIB=%XLIB%;%WINSDK60PATHPREFIX%\LIB%VCPATHSUFFIX%

        REM == Stay clean and avoid to add an unnecessary trailing ;
        IF NOT "%LIB%"=="" set LIB=%XLIB%;%LIB%
        IF     "%LIB%"=="" set LIB=%XLIB%
        IF NOT "%LIB%"=="" set LIB=%LIB:;;=;%

        set INCLUDE=%WINSDK60PATHPREFIX%\VC\Include;%WINSDK60PATHPREFIX%\VC\Include\Sys;%WINSDK60PATHPREFIX%\Include;%WINSDK60PATHPREFIX%\Include\gl;%INCLUDE%


        REM == Set path to SDK tools.
	REM == On 64bit, for WoW64 runs we want to add the 
        REM == path to the 32bit tools and then the 64bit tools 
        REM == (mainly because ildasm.exe is only available native).
        REM == If the architecture is *not* x86, then we want to append the path to the 32bit tools (mainly because on C# only runs
        REM == that's all we get.)
        IF NOT defined X_WOWRUN IF      "%OSARCH%"=="x86" set WINSDK60PATHS=%WINSDK60PATHPREFIX%\BIN%VCPATHSUFFIX%
        IF NOT defined X_WOWRUN IF  NOT "%OSARCH%"=="x86" set WINSDK60PATHS=%WINSDK60PATHPREFIX%\BIN%VCPATHSUFFIX%;%WINSDK60PATHS%;%WINSDK60PATHPREFIX%\BIN
        IF     defined X_WOWRUN IF "%CrossRun%"=="0" set WINSDK60PATHS=%WINSDK60PATHPREFIX%\BIN;%WINSDK60PATHPREFIX%\BIN\%OSARCH:AMD=x%;%WINSDK60PATHPREFIX%\BIN
        IF     defined X_WOWRUN IF "%CrossRun%"=="1" set WINSDK60PATHS=%WINSDK60PATHPREFIX%\BIN;%WINSDK60PATHPREFIX%\BIN\%OSARCH:AMD=x%;%WINSDK60PATHPREFIX%\BIN
        IF     defined X_WOWRUN IF "%CrossRun%"=="2" set WINSDK60PATHS=%WINSDK60PATHPREFIX%\BIN\%OSARCH:AMD=x%;%WINSDK60PATHPREFIX%\BIN

        set PATH=%WINSDK60PATHPREFIX%\VC\BIN%VCPATHSUFFIX%;%WINSDK60PATHS%;%PATH%

:lWinSDKDone

	REM == Set VC paths (note: how do we cross compile on IA64?)
        set VCPATHSUFFIX=
        set VCPATHPLATFSDKSUFFIX=

        IF /I "%OSARCH%"=="AMD64" set VCPATHPREFIX=%X86_PROGRAMFILES%\Microsoft Visual Studio 10.0\VC
	IF /I "%OSARCH%"=="AMD64" IF /I "%RUNPLATFORM%"=="AMD64" set VCPATHSUFFIX=\AMD64
	IF /I "%OSARCH%"=="AMD64" IF /I "%RUNPLATFORM%"=="AMD64" set VCPATHPLATFSDKSUFFIX=\win64\amd64

        IF /I "%OSARCH%"=="x86"   set VCPATHPREFIX=%PROGRAMFILES%\Microsoft Visual Studio 10.0\VC

        IF /I "%OSARCH%"=="IA64"  set VCPATHPREFIX=%PROGRAMFILES%\Microsoft Visual Studio 10.0\VC
        IF /I "%OSARCH%"=="IA64"  set VCPATHSUFFIX=
	IF /I "%OSARCH%"=="IA64"  IF /I "%RUNPLATFORM%"=="IA64"  set  VCPATHPLATFSDKSUFFIX=\win64\ia64

        set XLIB=
	IF EXIST "%VCPATHPREFIX%\ATLMFC\LIB%VCPATHSUFFIX%"      set XLIB=%XLIB%;%VCPATHPREFIX%\ATLMFC\LIB%VCPATHSUFFIX%
        IF EXIST "%VCPATHPREFIX%\PlatformSDK\lib%VCPATHSUFFIX%" set XLIB=%XLIB%;%VCPATHPREFIX%\PlatformSDK\lib%VCPATHSUFFIX%
        IF EXIST "%VCPATHPREFIX%\LIB%VCPATHSUFFIX%"             set XLIB=%XLIB%;%VCPATHPREFIX%\LIB%VCPATHSUFFIX%

        REM == Stay clean and avoid to add an unnecessary trailing ;
        IF NOT "%LIB%"=="" set LIB=%XLIB%;%LIB%
        IF     "%LIB%"=="" set LIB=%XLIB%
        IF NOT "%LIB%"=="" set LIB=%LIB:;;=;%

        IF EXIST "%VCPATHPREFIX%\ATLMFC\LIB%VCPATHSUFFIX%" set LIBPATH=%VCPATHPREFIX%\ATLMFC\LIB%VCPATHSUFFIX%
        set INCLUDE=%VCPATHPREFIX%\ATLMFC\INCLUDE;%VCPATHPREFIX%\INCLUDE;%VCPATHPREFIX%\PlatformSDK\include;%INCLUDE%
        set PATH=%VCPATHPREFIX%\BIN%VCPATHSUFFIX%;%VCPATHPREFIX%\PlatformSDK\bin%VCPATHPLATFSDKSUFFIX%;%VCPATHPREFIX%\PlatformSDK\bin;%VCPATHPREFIX%\VCPackages;%VCPATHPREFIX%\..\Common7\IDE;%PATH%

	echo ------------------------
	echo %PATH%
	echo ------------------------
	echo "2nd ***********"

	call :GETCLRSDKPATH

        if /I "%RUNPLATFORM%"=="IA64" call sdkvars.bat

        if defined MARATHON_CSC_PATH set PATH=%TESTROOT%\privbin;%PATH%
        if defined MARATHON_FSC_PATH set PATH=%TESTROOT%\privbin;%PATH%

	if defined TRACE %TRACE% [%PATH%]
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:PLATFORM_SETUP
if defined TRACE %TRACE% [proc %0 %*]
	if defined X_WOWRUN (
                if %CrossRun%==2     FOR /F "tokens=*" %%i IN ('%TESTROOT%\testenv\bin\%OSARCH%\getclrver -e') DO %%i
                if NOT %CrossRun%==2 FOR /F "tokens=*" %%i IN ('%TESTROOT%\testenv\bin\x86\getclrver -e') DO %%i

		if %CrossRun%==1 (
			call :ADDTOPATH ;%EXT_ROOT%;%windir%\Microsoft.Net\Framework64\%EXT_ROOT:~-10,10%
		)
		if %CrossRun%==0 (
		
		call :ADDTOPATH ;%EXT_ROOT%;%windir%\Microsoft.Net\Framework64\%EXT_ROOT:~-10,10%
		
		)
		if %CrossRun%==2 (
			call :ADDTOPATH ;%windir%\Microsoft.Net\Framework64\%EXT_ROOT:~-10,10%
		)


		set I_BUGSKIP=BUG_WOW_%RUNPLATFORM%
		
	) else (
		set I_BUGSKIP=BUG_%RUNPLATFORM%
	)
	if defined X_WOWRUN (	
			echo wow is defined
		if defined EXT_ROOT ( 
			echo extroot is defined
			if %CrossRun%==1 (
				echo This ldr64.exe set64 
				%ADMIN_PIPE% ldr64.exe set64
				if defined SDK_ROOT call :ADDTOPATH ;%SDK_ROOT%Bin
			)
			if %CrossRun%==0 (
				echo This ldr64.exe setwow 
				%ADMIN_PIPE% ldr64.exe setwow
				if defined SDK_ROOT call :ADDTOPATH ;%SDK_ROOT%Bin
			)
			if %CrossRun%==2 (
				echo This ldr64.exe setwow 
				%ADMIN_PIPE% ldr64.exe setwow
				if defined SDK_ROOT call :ADDTOPATH ;%SDK_ROOT%Bin
			)

		) else (
			call :ERROR "GETTING CLR VERSION FAILED in LDR64"
		)
	)
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:PLATFORM_CLEANUP
if defined TRACE %TRACE% [proc %0 %*]
        REM If wow or cross run, just make sure we
        REM restore the ldr64 (it wont hurt if it is
        REM already set).
	if defined X_WOWRUN %ADMIN_PIPE% ldr64.exe set64

        echo>>%LOGFILE% ==== :PLATFORM_CLEANUP Exiting section

goto :EOF

rem ///////////////
:SETCOMMONENVVARIABLES

        REM == Find out OS architecture, no matter what cmd prompt
        SET OSARCH=%PROCESSOR_ARCHITECTURE%
        IF "%PROCESSOR_ARCHITEW6432%"=="" GOTO lOsArchDone
        SET OSARCH=%PROCESSOR_ARCHITEW6432%
	:lOsArchDone
	IF "%OSARCH%"=="" set OSARCH=x86

        REM == Find out path to native 'Program Files', no matter what
        REM == architecture we are running on and no matter what command
        REM == prompt we came from.
        IF /I "%OSARCH%"=="x86"                                             set NATIVE_PROGRAMFILES=%ProgramFiles%
        IF /I "%OSARCH%"=="IA64"  IF /I "%PROCESSOR_ARCHITEW6432%"==""      set NATIVE_PROGRAMFILES=%ProgramFiles%
        IF /I "%OSARCH%"=="IA64"  IF /I "%PROCESSOR_ARCHITEW6432%"=="IA64"  set NATIVE_PROGRAMFILES=%ProgramW6432%
        IF /I "%OSARCH%"=="AMD64" IF /I "%PROCESSOR_ARCHITEW6432%"==""      set NATIVE_PROGRAMFILES=%ProgramFiles%
        IF /I "%OSARCH%"=="AMD64" IF /I "%PROCESSOR_ARCHITEW6432%"=="AMD64" set NATIVE_PROGRAMFILES=%ProgramW6432%

        REM == Find out path to native 'Program Files 32bit', no matter what
        REM == architecture we are running on and no matter what command
        REM == prompt we came from.
        IF /I "%OSARCH%"=="x86"   set X86_PROGRAMFILES=%ProgramFiles%
        IF /I "%OSARCH%"=="IA64"  set X86_PROGRAMFILES=%ProgramFiles(x86)%
        IF /I "%OSARCH%"=="AMD64" set X86_PROGRAMFILES=%ProgramFiles(x86)%

        REM == Find out path to Net Framework v4.0
        REM == This is tricky if we are on 64bit architectures because this script
        REM == could be executed in wow64 or native.

        set GREENBITS32=
        set GREENBITS64=

        FOR /F "tokens=3*" %%p in ('reg query "HKLM\Software\Microsoft\Net Framework Setup\NDP\v4.0" /v InstallPath') DO set GREENBITS64=%%p
        IF NOT "%GREENBITS64%"=="" set GREENBITS64=%GREENBITS64:~0,-1%
        FOR /F "tokens=3*" %%p in ('reg query "HKLM\Software\Wow6432Node\Microsoft\Net Framework Setup\NDP\v4.0" /v InstallPath') DO set GREENBITS32=%%p
        IF NOT "%GREENBITS32%"=="" set GREENBITS32=%GREENBITS32:~0,-1%

     REM == ==================
     REM == HACK = HACK = HACK
     REM == ==================
        REM == Last chance! If we still have no NetFx folder (apparently CLR4.0 setup does not write the reg key)
        REM == we enumerate the different folders and we pick the "highest" one.
        IF "%GREENBITS32%"=="" for /d %%a IN (%WINDIR%\Microsoft.Net\Framework\v4.?.?????) DO SET GREENBITS32=%%a
        IF "%GREENBITS64%"=="" for /d %%a IN (%WINDIR%\Microsoft.Net\Framework64\v4.?.?????) DO SET GREENBITS32=%%a
        IF "%GREENBITS64%"=="" set GREENBITS64=%GREENBITS32%
     REM == ==================
     REM == HACK = HACK = HACK
     REM == ==================

        REM == On 32bit architectures, the reg queries return the same values.
        IF "%OSARCH%"=="x86" SET GREENBITS32=%GREENBITS64%&& SET GREENBITS64=

        REM == If we are in WoW64, we can't get (easily) to the 64bit registry so we take a shortcut...
        IF NOT "%OSARCH%"=="%PROCESSOR_ARCHITECTURE%" set GREENBITS64=%GREENBITS32:Framework=Framework64%
        echo %GREENBITS32%
        echo %GREENBITS64%

        REM == System.Core.dll is sitting next to csc.exe... so ASSEMBLYFOLDER* is just legacy and will be
        REM == removed soon.
        IF NOT "%GREENBITS32%"=="" set ASSEMBLYFOLDER32=%GREENBITS32%
        IF NOT "%GREENBITS64%"=="" set ASSEMBLYFOLDER64=%GREENBITS64%

        REM == Find out path to MSVSMON32 and MSVSMON64. Typically, this will be used in CSEEREMOTE runs
        REM == I do not know a 'safe' way to find it, so I'm using some heuristic here
        REM == First, if MSVSMON is defined we don't do anything
        REM == Then we assume it was installed in the default location
        REM == TODO: verify this is correct on IA64
        IF NOT defined MSVSMON32 set MSVSMON32=%NATIVE_PROGRAMFILES%\Microsoft Visual Studio 10.0\Common7\IDE\Remote Debugger\x86\msvsmon.exe
        IF NOT defined MSVSMON64 set MSVSMON64=%NATIVE_PROGRAMFILES%\Microsoft Visual Studio 10.0\Common7\IDE\Remote Debugger\%OSARCH:AMD=x%\msvsmon.exe
        echo %MSVSMON32%
        echo %MSVSMON64%

goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:GETPLATFORM
if defined TRACE %TRACE% [proc %0 %*]

	if "%X_WOWRUN%"=="0" set RUNPLATFORM=x86
	if "%X_WOWRUN%"=="1" (
            if defined PROCESSOR_ARCHITEW6432 (
                set RUNPLATFORM=%PROCESSOR_ARCHITEW6432%
            ) else (
                set RUNPLATFORM=%PROCESSOR_ARCHITECTURE%
            )
	)

	if "%X_WOWRUN%"=="2" set RUNPLATFORM=x86

	if NOT defined X_WOWRUN (
  	    if /i "%PROCESSOR_ARCHITECTURE%" == "x86" (
                if NOT defined PROCESSOR_ARCHITEW6432 (
                    REM == Calling process is native 32bit  => platform is really x86
		    set RUNPLATFORM=x86
                ) else (
                    REM == Calling process is 32bit running in WoW64 => platform is a 64bit one
		    set RUNPLATFORM=%PROCESSOR_ARCHITEW6432%
                )
            ) else (
                REM == Calling process is 64bit running in native 64bit => platform is a 64bit one
		set RUNPLATFORM=%PROCESSOR_ARCHITECTURE%
	    )
        )

goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:GETCLRSDKPATH	
	if defined TRACE %TRACE% [proc %0 %*]

	if "%X_WOWRUN%"=="0" FOR /F "tokens=*" %%i IN ('%TESTROOT%\testenv\bin\x86\getclrver -e') DO %%i
	if "%X_WOWRUN%"=="1" FOR /F "tokens=*" %%i IN ('%TESTROOT%\testenv\bin\x86\getclrver -e') DO %%i
	if "%X_WOWRUN%"=="2" FOR /F "tokens=*" %%i IN ('%TESTROOT%\testenv\bin\%OSARCH%\getclrver -e') DO %%i
	if "%X_WOWRUN%"==""  FOR /F "tokens=*" %%i IN ('%TESTROOT%\testenv\bin\%OSARCH%\getclrver -e') DO %%i

	if defined EXT_ROOT ( 
		call :PREPENDTOPATH %EXT_ROOT%;
	) else (
		call :ERROR "GETTING CLR VERSION FAILED"
	)

        REM == Ignore 'Orcas' SDK if WinSDK is installed...
        REM == TODO: Change this once WinSDK/Orcas SDK story is clearer... [MatteoT, 10/21/2006]
        call :WHEREIS sn.exe
        IF NOT ERRORLEVEL 1 goto :GetClrSDKPathDone

	if defined SDK_ROOT call :PREPENDTOPATH %SDK_ROOT%Bin;
	REM == On PPE runs, the SDK_ROOT\Lib folder is not installed.
	if defined SDK_ROOT IF EXIST "%SDK_ROOT%lib" call :PREPENDTOLIB %SDK_ROOT%lib;

:GetClrSDKPathDone

goto :EOF
		
rem /////////////////////////////////////////////////////////////////////////////
:GETI_RUNROOT
if defined TRACE %TRACE% [proc %0 %*]
	if defined X_RUNROOT (
		set I_RUNROOT=%X_RUNROOT%
		goto :EOF
    )
	set I_RUNROOT=%TESTROOT%\%X_COMPONENT%\Source

        REM == CSEE and CSEEREMOTE share the same I_RUNROOT
        if /i "%X_COMPONENT%"=="CSEEREMOTE" set I_RUNROOT=%TESTROOT%\CSEE\Source

        REM == 
        if /i "%X_COMPONENT%"=="ASMMETAF" set I_RUNROOT=%TESTROOT%\asmmeta\features

goto :EOF


rem /////////////////////////////////////////////////////////////////////////////
rem /// CSCQA_COMPONENTID are used in CC runs to identify a trace!
:SET_COMPONENT_ID
if defined TRACE %TRACE% [proc %0 %*]
	

	if /i "%X_COMPONENT%" == "CSHARP" (
		set CSCQA_COMPONENTID=1
		set CSCQA_RUNTAGS=1,3
	) 

	if /i "%X_COMPONENT%" == "ALINK" (
		set CSCQA_COMPONENTID=2
		set CSCQA_RUNTAGS=1,3
	) 

	if /i "%X_COMPONENT%" == "CSEE" (
		set CSCQA_COMPONENTID=3
		set CSCQA_RUNTAGS=1,3
	) 

	if /i "%X_COMPONENT%" == "ASMMETAF" (
		set CSCQA_COMPONENTID=7
		set CSCQA_RUNTAGS=1,3
	) 

	if /i "%X_COMPONENT%" == "CSEEREMOTE" (
		set CSCQA_COMPONENTID=9
		set CSCQA_RUNTAGS=1,3
	) 

	if /i "%X_COMPONENT%" == "CSHARP3" (
		set CSCQA_COMPONENTID=12
		set CSCQA_RUNTAGS=1,3
	) 

	if /i "%X_COMPONENT%" == "CSHARP4" (
		set CSCQA_COMPONENTID=18
		set CSCQA_RUNTAGS=1,3
	) 

	if /i "%X_COMPONENT%" == "CORELINQ" (
		set CSCQA_COMPONENTID=13
		set CSCQA_RUNTAGS=1,3
	) 

	if /i "%X_COMPONENT%" == "LAF" (
		set CSCQA_COMPONENTID=14
		set CSCQA_RUNTAGS=1,3
	) 


	if /i "%X_COMPONENT%" == "CSHARPSTRESS" (
		set CSCQA_COMPONENTID=15
		set CSCQA_RUNTAGS=1,3
	) 

	if /i "%X_COMPONENT%" == "FSHARP" (
		set CSCQA_COMPONENTID=16
		set CSCQA_RUNTAGS=1,3
	) 
goto :EOF



rem /////////////////////////////////////////////////////////////////////////////
:REMOTESETUP

if defined TRACE %TRACE% [proc %0 %*]

        REM == MADDOG_CS <-> binary compat run (in Maddog, with 2 machines)	
	if /i "%X_CROSS_RUN%" == "MADDOG_CS" (
    	  set commondir=%CD%
    	  set simulator_pipe=issuerun.exe stopit.exe -g -s120
	)

	if /i "%X_CROSS_RUN%" == "REMOTE_DEBUG" (	
    	  net use t: \\%TierComputerName%\d$
    	  cd /d t:\school\csee\source
    	  set iGLFLAGS=-m %TierComputerName%
    	  set REMOTEDRIVE=D
          set TESTROOT=t:\school
	)
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////

:CUSTSETUP

if defined TRACE %TRACE% [proc %0 %*]
	
	echo runroot %I_RUNROOT%
	cd /d %I_RUNROOT%

	if defined X_CUSTOMSETUP (
		if not defined I_RUNROOT call :ERROR I_RUNROOT is not defined %X_CUSTOMSETUP% will fail!!
		pushd %I_RUNROOT%
		echo [ proc %0 ] %I_RUNROOT%\%X_CUSTOMSETUP%
		call %I_RUNROOT%\%X_CUSTOMSETUP%
                REM == CUSTSETUP script might set X_CUSTRUNALLTAGS
                REM == which might be later on consumed (blindly) by
                REM == :RUNALL
		popd
	)

goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:RUNALL

if defined TRACE %TRACE% [proc %0 %*]
	if not defined X_COMPONENT call :ERROR X_COMPONENT is not defined runall will fail!!!
	if not defined I_RUNROOT call :ERROR I_RUNROOT is not defined runall will fail!!!

        REM SDK installed? Assumes that a the current machine config/setup is ok
        set SDKTAG=
        call :WHEREIS sn.exe
        IF ERRORLEVEL 1 set SDKTAG=-nottags:SDK

        REM peverify available? This could be a Redist only run...
        REM There always a chance that PEVERIFY is set externally: in that case
        REM we do not do anything.
        if NOT "%PEVERIFY%"=="" goto :lPeverifyDone
        call :WHEREIS sn.exe
        IF ERRORLEVEL 1 set PEVERIFY=echo
:lPeverifyDone

        REM CPP installed? Assumes that a the current machine config/setup is ok
        set CPPTAG=
        call :WHEREIS sn.exe
        IF ERRORLEVEL 1 set CPPTAG=-nottags:CPP

        REM CPPEE installed? Assumes that a the current machine config/setup is ok
        REM This is a quick check to verify whether MSCOREE.lib is available...
        set CPPEETAG=
        echo>%TEMP%\dummycee.cpp int main() {}
        cl.exe /clr %TEMP%\dummycee.cpp
        IF ERRORLEVEL 1 set CPPEETAG=-nottags:CPPEE

	REM == dump environment to log file
	echo>>%LOGFILE%  ==== ENVIRONMENT BEFORE RUNALL ====
	echo>>%LOGFILE%  set PATH=%PATH%
	echo>>%LOGFILE%  set LIB=%LIB%
	echo>>%LOGFILE%  set INCLUDE=%INCLUDE%
	echo>>%LOGFILE%  set LIBPATH=%LIBPATH%
	echo>>%LOGFILE%  set OSARCH=%OSARCH%
	echo>>%LOGFILE%  set RUNPLATFORM=%RUNPLATFORM%
	echo>>%LOGFILE%  set VCPATHPREFIX=%VCPATHPREFIX%
	echo>>%LOGFILE%  set VCPATHSUFFIX=%VCPATHSUFFIX%
	echo>>%LOGFILE%  set X_WOWRUN=%X_WOWRUN%
	echo>>%LOGFILE%  set CrossRun=%CrossRun%
	echo>>%LOGFILE%  set X_BRANCH=%X_BRANCH%
	echo>>%LOGFILE%  set X_VERSION=%X_VERSION%
	echo>>%LOGFILE%  set X_COMPONENT=%X_COMPONENT%
	echo>>%LOGFILE%  set X_MODE=%X_MODE%
	echo>>%LOGFILE%  set X_IGNORESKIPRULES=%X_IGNORESKIPRULES%
	echo>>%LOGFILE%  set I_RUNROOT=%I_RUNROOT%
	echo>>%LOGFILE%  set X_CUSTRUNALLTAGS=%X_CUSTRUNALLTAGS%
	echo>>%LOGFILE%  set ASSEMBLYFOLDER32=%ASSEMBLYFOLDER32%
	echo>>%LOGFILE%  set ASSEMBLYFOLDER64=%ASSEMBLYFOLDER64%
	echo>>%LOGFILE%  set MSVSMON32=%MSVSMON32%
	echo>>%LOGFILE%  set MSVSMON64=%MSVSMON64%
	echo>>%LOGFILE%  set GREENBITS32=%GREENBITS32%
	echo>>%LOGFILE%  set GREENBITS64=%GREENBITS64%
	echo>>%LOGFILE%  set COMPLUS_LOGENABLE=%COMPLUS_LOGENABLE%
	echo>>%LOGFILE%  set COMPLUS_DBGJITDEBUGLAUNCHSETTING=%COMPLUS_DBGJITDEBUGLAUNCHSETTING%
	echo>>%LOGFILE%  set COMPLUS_LOGTOCONSOLE=%COMPLUS_LOGTOCONSOLE%
	echo>>%LOGFILE%  set COMPLUS_NOGUIONASSERT=%COMPLUS_NOGUIONASSERT%
	echo>>%LOGFILE%  set PEVERIFY=%PEVERIFY%
	echo>>%LOGFILE%  set SIMULATOR_PIPE=%SIMULATOR_PIPE%
	echo>>%LOGFILE%  set ADMIN_PIPE=%ADMIN_PIPE%
	echo>>%LOGFILE%  set CSC_PIPE=%CSC_PIPE%

        REM == Try to figure out what flavor of the compiler we are using - shp is the same as ret
        call :WHEREIS csc.exe
        for /f %%t IN ("%WHEREIS%") DO for /F "tokens=6" %%i IN ('filever %%t') DO set CSCFLAVOR=%%i
        IF "%CSCFLAVOR%"=="shp" set CSCFLAVOR=ret
        IF "%CSCFLAVOR%"=="dbg" set CSCFLAVOR=chk
        echo>>%LOGFILE%  set CSCFLAVOR=%CSCFLAVOR%

        REM == Find out what lab we are using. We strip away all the intermediate components...
        REM == so something like Orcas/pu/VCS will become VCS. This is because in Marathon we
        REM == use a 'short' name...
        set X_BRANCH_SHORT=%X_BRANCH:/= %
        for %%c IN (%X_BRANCH_SHORT%) DO set X_BRANCH_SHORT=%%c

        set KNOWNFAIL=
	if /i "%X_MODE%" == "MARATHON" goto :lDoneSkipRules
        if defined X_IGNORESKIPRULES echo>>%LOGFILE%  ==== Ignoring SkipRules (X_IGNORESKIPRULES is defined) && goto :lDoneSkipRules

        REM == 
        REM == The current implementation passes context via iSCSFLAGS
        REM ==

        REM == Export rules on the fly from Marathon DB...
        REM == If for some reason the necessary parameter are missing
        REM == we play safe and we do not export anything.
        REM == SkipRuleExtractor should be in testenv\bin
        
        set SKIPRULEEXTRACTORCMD=
        IF defined X_BRANCH_SHORT IF defined X_VERSION IF defined X_COMPONENT set SKIPRULEEXTRACTORCMD=SkipRuleExtractor.exe -branch:%X_BRANCH_SHORT% -version:%X_VERSION% -component:%X_COMPONENT%
        echo>>%LOGFILE%  ==== SKIPRULEEXTRACTORCMD=%SKIPRULEEXTRACTORCMD%
        %SKIPRULEEXTRACTORCMD%

        REM == If a SkipRules.txt file was generated, then we process it
        REM == to generate a knownfail.lst file (which is understood by runall)
        REM == otherwise we bail out.
	IF NOT EXIST SkipRules.txt goto :lDoneSkipRules

        set SKIPRULEPARSERCMD=perl -S SkipRuleParser.pl SkipRules.txt "" "%iSCFLAGS%" %RUNPLATFORM% %OSARCH% %CSCFLAVOR%
        echo>>%LOGFILE%  ==== SKIPRULEPARSERCMD=%SKIPRULEPARSERCMD%

        %SKIPRULEPARSERCMD% > knownfail.lst
        echo>>%LOGFILE%  ==== knownfail [iSCFLAGS=%iSCFLAGS%, RUNPLATFORM=%RUNPLATFORM%, OSARCH=%OSARCH%, CSCFLAVOR=%CSCFLAVOR%] ====

:lDoneSkipRules

        REM == Is this a TRANSFORMER run? If so, take a look and see if there is a knownfail.lst
        REM == corresponding to this transformer. If there is, append it to the existing knownfail.lst
        REM == Ideally this should be dealt with using normal SkipRules, but we don't have anything like that now,
        REM == so for now we use static files...

REM === [MatteoT] For now we do not want to use this logic
REM === This code should be updated (i.e. deleted) once
REM === Ying's tool is working properly.
goto :lDoneKnownfailTransformer

        IF "%TRANSFORM%"=="" goto :lDoneKnownfailTransformer

        echo>>%LOGFILE%  ==== This is a transformer (%TRANSFORM%) run!
        IF NOT EXIST KnownFail\Transformers\%TRANSFORM%\KnownFail.lst goto :lDoneKnownfailTransformer

        REM == Append to existing knowfail.lst
        type>>knownfail.lst KnownFail\Transformers\%TRANSFORM%\KnownFail.lst
        
:lDoneKnownfailTransformer

        REM == Is there a knownfail.lst (as a result of a previous step)? If so, add the runall option
        REM == If such file exist, emit it in the log
        IF EXIST Knownfail.lst set KNOWNFAIL=-knownfail knownfail.lst
        IF EXIST Knownfail.lst type>>%LOGFILE% knownfail.lst

	REM == Code coverage run?
        set RUNALLHOOKS=
	IF "%CodeCoverage%"=="1" set RUNALLHOOKS=-prehook %TESTROOT%testenv\bin\cc_prehook.pl -posthook %TESTROOT%testenv\bin\cc_posthook.pl
	echo>>%LOGFILE%  set RUNALLHOOKS=%RUNALLHOOKS%

	echo>>%LOGFILE%  ==== Binaries' locations ====
	call :WHEREIS>>%LOGFILE%  cl.exe
	call :WHEREIS>>%LOGFILE%  sn.exe
	call :WHEREIS>>%LOGFILE%  gacutil.exe
	call :WHEREIS>>%LOGFILE%  csc.exe
	call :WHEREIS>>%LOGFILE%  jsc.exe
	call :WHEREIS>>%LOGFILE%  al.exe

        REM == Try to figure out what flavor of the compiler we are using - shp is the same as ret
        call :WHEREIS csc.exe
        for /f %%t IN ("%WHEREIS%") DO for /F "tokens=6" %%i IN ('filever %%t') DO set CSCFLAVOR=%%i
        IF "%CSCFLAVOR%"=="shp" set CSCFLAVOR=ret
        echo>>%LOGFILE%  set CSCFLAVOR=%CSCFLAVOR%

        set KNOWNFAIL=
	if /i "%X_MODE%" == "RELEASE" (
            REM == 
            REM == The current implementation passes context via iSCSFLAGS
            REM == 
            if exist SkipRules.txt perl -S SkipRuleParser.pl SkipRules.txt "" "%iSCFLAGS%" %RUNPLATFORM% %OSARCH% %CSCFLAVOR% > knownfail.lst
            echo>>%LOGFILE%  ==== knownfail [iSCFLAGS=%iSCFLAGS%, RUNPLATFORM=%RUNPLATFORM%, OSARCH=%OSARCH%, CSCFLAVOR=%CSCFLAVOR%] ====
            if exist SkipRules.txt type>>%LOGFILE% knownfail.lst
            if "%X_IGNORESKIPRULES%"=="" if exist SkipRules.txt set KNOWNFAIL=-knownfail knownfail.lst
        )

        REM == Set the runall flag to exclude tests based on the flavor of the compiler
        REM == For now we support 'ret' and 'not-ret'
        REM == A test that requires a RET compiler should be tagged with RETreq
        REM == A test that requires a CHK compiler should be tagged with CHKreq
        IF     "%CSCFLAVOR%"=="ret" set CSCFLAVORTAG=-nottags:CHKreq
        IF NOT "%CSCFLAVOR%"=="ret" set CSCFLAVORTAG=-nottags:RETreq

	echo>>%LOGFILE%  ==== 64bit status ====
	ldr64>>%LOGFILE%  query
        call :WHEREIS ldr64.exe
        IF ERRORLEVEL 1 echo>>%LOGFILE% This is a 32bit OS or ldr64 not in the path

	for /f "tokens=*" %%i in ('perl -e "print scalar localtime"') do set MTN_START_TIME=%%i
        echo            perl -S runall.pl %X_RUNALL_ARGS% %RUNALLHOOKS% %KNOWNFAIL% -target:%RUNPLATFORM% %CSCFLAVORTAG% %SDKTAG% %CPPEETAG% %CPPTAG% %X_CUSTRUNALLTAGS%
        echo>>%LOGFILE% perl -S runall.pl %X_RUNALL_ARGS% %RUNALLHOOKS% %KNOWNFAIL% -target:%RUNPLATFORM% %CSCFLAVORTAG% %SDKTAG% %CPPEETAG% %CPPTAG% %X_CUSTRUNALLTAGS%
                        perl -S runall.pl %X_RUNALL_ARGS% %RUNALLHOOKS% %KNOWNFAIL% -target:%RUNPLATFORM% %CSCFLAVORTAG% %SDKTAG% %CPPEETAG% %CPPTAG% %X_CUSTRUNALLTAGS%

        echo>>%LOGFILE% ==== :RUNALL - Runall completed.
	
	REM == Coverage run _and_ non-Maddog run? If so, collect left-overs...
	IF "%CodeCoverage%"=="1" IF /i NOT "%X_MODE%"=="RELEASE" call %TESTROOT%testenv\bin\CC_CollectData.bat
	IF "%CodeCoverage%"=="1" IF /i NOT "%X_MODE%"=="RELEASE" call %TESTROOT%testenv\bin\CC_DeleteData.bat

        echo>>%LOGFILE% ==== :RUNALL - After code coverage section 

REM === Run Ying's tool to post-process the 
REM === results. Note: the ultimate results we
REM === want to achieve is to post-process the
REM === runall output files so they get uploaded
REM === normally to Marathon. For now, the tool
REM === simply generates a csv file that can be
REM === used to create baselines.
REM === Of course, this only makes sense if we are
REM === doing a TRANSFORMER run.
	IF NOT "%TRANSFORM%"=="" echo>>%LOGFILE% Invoking TestLogParser.exe
        IF NOT "%TRANSFORM%"=="" TestLogParser.exe

        IF NOT "%TRANSFORM%"=="" copy /y %TRANSFORM%*.log \\vcslabsrv\Marathon\TPs\RTM_TP\Transformers\%X_COMPONENT%\
	IF NOT "%TRANSFORM%"=="" copy /y %TRANSFORM%*.xls \\vcslabsrv\Marathon\TPs\RTM_TP\Transformers\%X_COMPONENT%\

        echo>>%LOGFILE% ==== :RUNALL - After transform section 


	for /f "tokens=*" %%i in ('perl -e "print scalar localtime"') do set MTN_END_TIME=%%i
 
        echo>>%LOGFILE% ==== :RUNALL - Exiting section
goto :EOF


rem /////////////////////////////////////////////////////////////////////////////
:BUILDTOOLS

if defined TRACE %TRACE% [proc %0 %*]

     IF NOT EXIST %TESTROOT%\toolsrc goto :EOF

     pushd %TESTROOT%\toolsrc
     for /D %%d IN (*) DO (
        pushd %%d
        IF EXIST build_deploy.cmd call build_deploy.cmd %TESTROOT%\testenv\bin
        popd
     )
     popd

goto :EOF

rem /////////////////////////////////////////////////////////////////////////////

:CUSTCLEANUP
if defined TRACE %TRACE% [proc %0 %*]
	echo  Current Dir %CD%
	if defined X_CUSTOMCLEANUP (
		echo cleaningup
		call :CHECKANDCALL %X_CUSTOMCLEANUP%
	)

      echo>>%LOGFILE% ==== :CUSTCLEANUP - Exiting section
goto :EOF


rem /////////////////////////////////////////////////////////////////////////////

:REMOTECLEANUP

if defined TRACE %TRACE% [proc %0 %*]
	@rem ### disconnect drive when we are through in cross run ###
	if defined X_CROSS_RUN ( 
 		set common_dir=
 		set simulator_pipe=
		issuerun.exe TERMINATE_READRUN
	)	
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////

:COPYLOGS

if defined TRACE %TRACE% [proc %0 %*]

        echo>>%LOGFILE% :COPYLOGS Begin of section

        REM == Set COPYCMP (on Vista, this is a system command so we don't need our own robocopy)
	call :SETTIMESTAMP
        set COPYCMD=ROBOCOPY.exe

        echo>>%LOGFILE% :COPYLOGS TIMESTAMP=%TIMESTAMP%

        set COPYCMDOPTS=/R:10 /W:60 /NC /NS /NFL /LOG+:%TEMP%\RoboCopy_CopyLogs_%TIMESTAMP%.log
        call :WHEREIS ROBOCOPY.EXE
        IF ERRORLEVEL 1 set COPYCMD=%ROBOCOPYDIR%\%COPYCMD%

        echo>>%LOGFILE% COPYLOGS: COPYCMD=%COPYCMD%

	if /i "%X_MODE%" == "MARATHON" (
                echo>>%LOGFILE% COPYLOGS: Mode = Marathon

		if defined MARATHON_LOG_SERVER (
                        echo>>%LOGFILE% COPYLOGS: MARATHON_LOG_SERVER=%MARATHON_LOG_SERVER%

			md %MARATHON_LOG_SERVER%\%X_GUID%
                        IF ERRORLEVEL 1 echo>>%LOGFILE% COPYLOGS: md %MARATHON_LOG_SERVER%\%X_GUID% failed!
			%COPYCMD% . %MARATHON_LOG_SERVER%\%X_GUID% results.log %COPYCMDOPTS%
			%COPYCMD% . %MARATHON_LOG_SERVER%\%X_GUID% runpl.log   %COPYCMDOPTS%
                        echo>>%LOGFILE% COPYLOGS: Done copying
		)
	) 

	if not exist "results.log" call :ERROR No results.log
	copy results.log %TestRoot%\results.log 
	if not exist "runpl.log" call :ERROR No runpl.log
	copy runpl.log %TestRoot%\runpl.log
	
	if defined MARATHON_LOG_SERVER (
		call :CREATECONFIG %X_GUID%
		%COPYCMD% . %MARATHON_LOG_SERVER%\%X_GUID% runconfig.xml %COPYCMDOPTS%
                echo>>%LOGFILE% COPYLOGS: Done %COPYCMD% . %MARATHON_LOG_SERVER%\%X_GUID% runconfig.xml %COPYCMDOPTS%
	)

        REM == Desperately try to copy log file to \VCDriver (if available)
        IF EXIST \VCDriver copy /y %LOGFILE% \VCDriver\

	call :REMOTECLEANUP

        echo>>%LOGFILE% COPYLOGS: Exiting...
		 
goto :EOF
rem /////////////////////////////////////////////////////////////////////////////
:CREATECONFIG
if defined TRACE %TRACE% [proc %0 %*]

	if not defined COMPUTERNAME call :ERROR COMPUTERNAME not defined in createconfig
	if not defined NUMBER_OF_PROCESSORS call :ERROR NUMBER_OF_PROCESSORS not defined in createconfig
	if not defined MTN_END_TIME for /f "tokens=*" %%i in ('perl -e "print scalar localtime"') do set MTN_END_TIME=%%i
	if not defined MTN_END_TIME call :ERROR MTN_END_TIME not defined in createconfig
	if not defined MTN_START_TIME call :ERROR MTN_START_TIME not defined in createconfig
	if not defined MARATHON_LOG_SERVER call :ERROR MARATHON_LOG_SERVER not defined in createconfig

	echo ^<^?xml version=^"1.0^" encoding=^"utf-8^"^?^> > runconfig.xml
	echo ^<RunConfigData xmlns:xsd=^"http:^/^/www.w3.org^/2001^/XMLSchema^" xmlns:xsi=^"http:^/^/www.w3.org^/2001^/XMLSchema-instance^"^> >> runconfig.xml
	echo    ^<GUID^>%X_GUID%^</GUID^> >> runconfig.xml
	echo    ^<MachineName^>%COMPUTERNAME%^</MachineName^> >> runconfig.xml
	echo    ^<NumProc^>%NUMBER_OF_PROCESSORS%^</NumProc^> >> runconfig.xml
	echo    ^<RunEndTime^>%MTN_END_TIME%^</RunEndTime^> >> runconfig.xml
	echo    ^<RunStartTime^>%MTN_START_TIME%^</RunStartTime^> >> runconfig.xml
	echo    ^<LogLocation^>%MARATHON_LOG_SERVER%^\%1^<^/LogLocation^> >> runconfig.xml
	echo ^<^/RunConfigData^> >> runconfig.xml

goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
rem // SETUPNETCF is used to get Compact Framework files (if needed)
rem // 
:SETUPNETCF

if NOT defined MARATHON_NETCF_WIN32_DIR goto :EOF
if NOT defined MARATHON_NETCF_MANAGED_DIR  goto :EOF

if NOT EXIST %TESTROOT%\NetCF\Win32x86 MD %TESTROOT%\NetCF

set WIN32_FILES=mscoree.dll mscoree3_5.dll l_except_3_5.nlp l_intl_3_5.nlp sortkey_3_5.nlp sorttbls_3_5.nlp big5.nlp bopomofo.nlp ksc.nlp  prc.nlp prcp.nlp xjis.nlp cgacutil.exe install.cmd gacfiles.txt uninstall.cmd netcfagl3_5.dll
set MANAGED_FILES=Microsoft.VisualBasic.dll Microsoft.WindowsCE.Forms.dll mscorlib.dll system.data.dll system.data.sqlclient.dll system.data.sqlserverce.dll system.dll system.drawing.dll system.messaging.dll system.net.irda.dll system.web.services.dll system.windows.forms.dll system.xml.dll Microsoft.WindowsMobile.DirectX.dll CustomMarshalers.dll System.ServiceModel.dll System.Runtime.Serialization.dll Microsoft.ServiceModel.Channels.Mail.dll Microsoft.ServiceModel.Channels.Mail.WindowsMobile.dll System.Data.DataSetExtensions.dll System.Core.dll System.Xml.Linq.dll system.sr.dll

for %%f in (%WIN32_FILES%)   do xcopy /y/d %MARATHON_NETCF_WIN32_DIR%\%%f   %TESTROOT%\NetCF\
for %%f in (%MANAGED_FILES%) do xcopy /y/d %MARATHON_NETCF_MANAGED_DIR%\%%f %TESTROOT%\NetCF\

xcopy /y/d %MARATHON_NETCF_WIN32_DIR%\rhost.exe %TESTROOT%\NetCF\

set CSC_PIPE=csc.exe /nostdlib /noconfig /r:%TESTROOT%\NetCF\MsCorlib.dll /r:%TESTROOT%\NetCF\System.Data.dll /r:%TESTROOT%\NetCF\System.dll /r:%TESTROOT%\NetCF\System.Core.dll /r:%TESTROOT%\NetCF\System.Drawing.dll /r:%TESTROOT%\NetCF\System.Messaging.dll /r:%TESTROOT%\NetCF\System.Net.IrDA.dll /r:%TESTROOT%\NetCF\System.Web.Services.dll /r:%TESTROOT%\NetCF\System.Windows.Forms.dll /r:%TESTROOT%\NetCF\Microsoft.WindowsCE.Forms.dll /r:%TESTROOT%\NetCF\System.Xml.dll /r:%TESTROOT%\NetCF\System.Xml.Linq.dll /r:%TESTROOT%\NetCF\System.Data.DataSetExtensions.dll
set SIMULATOR_PIPE=%TESTROOT%\NetCF\rhost.exe

%ADMIN_PIPE% REG ADD HKLM\SOFTWARE\Microsoft\.NETCompactFramework /v DbgJITDebugLaunchSetting /t REG_DWORD /d 1 /f

call :ADDTOPATH ;%TESTROOT%\NetCF

goto :EOF


rem /////////////////////////////////////////////////////////////////////////////
rem // COPYPVTCSC is used to get private drops of the compiler.
rem // the Path will be set in the SETPATH section!
:COPYPVTCSC

if NOT defined MARATHON_CSC_PATH (
    if NOT defined MARATHON_FSC_PATH  (
        echo MARATHON_CSC_PATH  and MARATHON_FSC_PATH are not defined.
        echo Cannot call to COPYVTCSC without a private path.
        goto :EOF
    )
)

if exist %TESTROOT%\privbin goto :EOF

md %TESTROOT%\privbin

REM == Private run on 32bit or 64bit?
if NOT defined X_WOWRUN if     "%RUNPLATFORM%"=="x86" xcopy>>%LOGFILE% /f /s %GREENBITS32%\*.* %TESTROOT%\privbin
if NOT defined X_WOWRUN if NOT "%RUNPLATFORM%"=="x86" xcopy>>%LOGFILE% /f /s %GREENBITS64%\*.* %TESTROOT%\privbin

if not defined MARATHON_CSC_PATH (
    echo This looks like a FSharp only private run.
    goto :PVTFSC_ONLY
)

REM == ASMMETA?
xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\ASMMETA.exe" %TESTROOT%\privbin\
xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\ASMMETA.pdb" %TESTROOT%\privbin\

REM == CSC?
xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\csc.exe" %TESTROOT%\privbin\
xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\csc.pdb" %TESTROOT%\privbin\
xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\csc.rsp" %TESTROOT%\privbin\
IF EXIST "%MARATHON_CSC_PATH%\cscompui.dll"      xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\cscompui.dll"      %TESTROOT%\privbin\1033\
IF EXIST "%MARATHON_CSC_PATH%\1033\cscompui.dll" xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\1033\cscompui.dll" %TESTROOT%\privbin\1033\

REM == ALINK?
xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\al.exe"     %TESTROOT%\privbin\
xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\al.pdb"     %TESTROOT%\privbin\
xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\alink.dll"  %TESTROOT%\privbin\
xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\alink.pdb"  %TESTROOT%\privbin\
IF EXIST "%MARATHON_CSC_PATH%\alinkui.dll"      xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\alinkui.dll"      %TESTROOT%\privbin\1033\
IF EXIST "%MARATHON_CSC_PATH%\1033\alinkui.dll" xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\1033\alinkui.dll" %TESTROOT%\privbin\1033\

REM == EE?
%ADMIN_PIPE%  copy>>%LOGFILE% "%VS100COMNTOOLS%..\Packages\Debugger\cscompee.dll" "%VS100COMNTOOLS%..\Packages\Debugger\cscompee.dll.orig"
%ADMIN_PIPE%  copy>>%LOGFILE% "%VS100COMNTOOLS%..\Packages\Debugger\cscompee.pdb" "%VS100COMNTOOLS%..\Packages\Debugger\cscompee.pdb.orig"
%ADMIN_PIPE%  copy>>%LOGFILE% "%VS100COMNTOOLS%..\Packages\Debugger\1033\cscompeeui.dll" "%VS100COMNTOOLS%..\Packages\Debugger\1033\cscompeeui.dll.orig"
%ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\cscompee.dll"   "%VS100COMNTOOLS%..\Packages\Debugger\"
%ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\cscompee.pdb"   "%VS100COMNTOOLS%..\Packages\Debugger\"
IF EXIST "%MARATHON_CSC_PATH%\cscompeeui.dll"      %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\cscompeeui.dll"      "%VS100COMNTOOLS%..\Packages\Debugger\1033\"
IF EXIST "%MARATHON_CSC_PATH%\1033\cscompeeui.dll" %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\1033\cscompeeui.dll" "%VS100COMNTOOLS%..\Packages\Debugger\1033\"

%ADMIN_PIPE%  copy>>%LOGFILE%          "%VS100COMNTOOLS%..\IDE\PublicAssemblies\Microsoft.VisualStudio.DebuggerVisualizers.dll" "%VS100COMNTOOLS%..\IDE\PublicAssemblies\Microsoft.VisualStudio.DebuggerVisualizers.dll.orig"
%ADMIN_PIPE%  copy>>%LOGFILE%          "%VS100COMNTOOLS%..\IDE\PublicAssemblies\Microsoft.VisualStudio.DebuggerVisualizers.pdb" "%VS100COMNTOOLS%..\IDE\PublicAssemblies\Microsoft.VisualStudio.DebuggerVisualizers.pdb.orig"
%ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\Microsoft.VisualStudio.DebuggerVisualizers.dll"                    "%VS100COMNTOOLS%..\IDE\PublicAssemblies\"
%ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\Microsoft.VisualStudio.DebuggerVisualizers.pdb"                    "%VS100COMNTOOLS%..\IDE\PublicAssemblies\"

if NOT defined X_WOWRUN if NOT "%RUNPLATFORM%"=="x86" %ADMIN_PIPE% copy>>%LOGFILE% "%VS100COMNTOOLS%..\IDE\Remote Debugger\%RUNPLATFORM:AMD=x%\msvsmon.exe" "%VS100COMNTOOLS%..\IDE\Remote Debugger\%RUNPLATFORM:AMD=x%\msvsmon.exe.orig"
if NOT defined X_WOWRUN if NOT "%RUNPLATFORM%"=="x86" %ADMIN_PIPE% copy>>%LOGFILE% "%VS100COMNTOOLS%..\IDE\Remote Debugger\%RUNPLATFORM:AMD=x%\msvsmon.pdb" "%VS100COMNTOOLS%..\IDE\Remote Debugger\%RUNPLATFORM:AMD=x%\msvsmon.pdb.orig"

if NOT defined X_WOWRUN if NOT "%RUNPLATFORM%"=="x86" %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\msvsmon.exe" "%VS100COMNTOOLS%..\IDE\Remote Debugger\%RUNPLATFORM:AMD=x%\"
if NOT defined X_WOWRUN if NOT "%RUNPLATFORM%"=="x86" %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\msvsmon.pdb" "%VS100COMNTOOLS%..\IDE\Remote Debugger\%RUNPLATFORM:AMD=x%\"

REM == vsassert?
IF EXIST "%MARATHON_CSC_PATH%\vsassert.dll" xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\vsassert.dll" %TESTROOT%\privbin\
if NOT defined X_WOWRUN if NOT "%RUNPLATFORM%"=="x86" %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%windir%\system32\vsassert.dll" "%VS100COMNTOOLS%..\IDE\Remote Debugger\%RUNPLATFORM:AMD=x%\"

REM == cslangsvc.dll
%ADMIN_PIPE% copy>>%LOGFILE% "%VS100COMNTOOLS%..\..\VC#\VCSPackages\cslangsvc.dll"        "%VS100COMNTOOLS%..\..\VC#\VCSPackages\cslangsvc.dll.orig"
%ADMIN_PIPE% copy>>%LOGFILE% "%VS100COMNTOOLS%..\..\VC#\VCSPackages\1033\cslangsvcui.dll" "%VS100COMNTOOLS%..\..\VC#\VCSPackages\1033\cslangsvcui.dll.orig"

IF EXIST "%MARATHON_CSC_PATH%\cslangsvc.dll"   %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\cslangsvc.dll"   "%VS100COMNTOOLS%..\..\VC#\VCSPackages\"
IF EXIST "%MARATHON_CSC_PATH%\cslangsvcui.dll" %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\cslangsvcui.dll" "%VS100COMNTOOLS%..\..\VC#\VCSPackages\1033\"
IF EXIST "%MARATHON_CSC_PATH%\VC7\VCPackages\cslangsvc.dll"        %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\VC7\VCPackages\cslangsvc.dll"        "%VS100COMNTOOLS%..\..\VC#\VCSPackages\"
IF EXIST "%MARATHON_CSC_PATH%\VC7\VCPackages\1033\cslangsvcui.dll" %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\VC7\VCPackages\1033\cslangsvcui.dll" "%VS100COMNTOOLS%..\..\VC#\VCSPackages\1033\"

REM == System.Core.dll - csc.exe will always look for this assembly in the ASSEMBLYFOLDER32 or ASSEMBLYFOLDER64 folder
REM == There's a hack here! On a 64bit machine, we are copying the same binary to both the 32 and 64 location
REM == This is fine for now, as System.Core.dll is platform agnostic...

IF EXIST "%MARATHON_CSC_PATH%\..\..\System.Core.dll" if defined ASSEMBLYFOLDER32 %ADMIN_PIPE%  copy>>%LOGFILE%          "%ASSEMBLYFOLDER32%\System.Core.dll" "%ASSEMBLYFOLDER32%\System.Core.dll.orig"
IF EXIST "%MARATHON_CSC_PATH%\..\..\System.Core.dll" if defined ASSEMBLYFOLDER64 %ADMIN_PIPE%  copy>>%LOGFILE%          "%ASSEMBLYFOLDER64%\System.Core.dll" "%ASSEMBLYFOLDER64%\System.Core.dll.orig"
IF EXIST "%MARATHON_CSC_PATH%\System.Core.dll"       if defined ASSEMBLYFOLDER32 %ADMIN_PIPE%  copy>>%LOGFILE%          "%ASSEMBLYFOLDER32%\System.Core.dll" "%ASSEMBLYFOLDER32%\System.Core.dll.orig"
IF EXIST "%MARATHON_CSC_PATH%\System.Core.dll"       if defined ASSEMBLYFOLDER64 %ADMIN_PIPE%  copy>>%LOGFILE%          "%ASSEMBLYFOLDER64%\System.Core.dll" "%ASSEMBLYFOLDER64%\System.Core.dll.orig"

IF EXIST "%MARATHON_CSC_PATH%\..\..\System.Core.dll" if defined ASSEMBLYFOLDER32 %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\..\..\System.Core.dll" "%ASSEMBLYFOLDER32%\"
IF EXIST "%MARATHON_CSC_PATH%\..\..\System.Core.dll" if defined ASSEMBLYFOLDER32 %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\..\..\System.Core.dll" "%ASSEMBLYFOLDER32%\"
IF EXIST "%MARATHON_CSC_PATH%\System.Core.dll"       if defined ASSEMBLYFOLDER64 %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\System.Core.dll" "%ASSEMBLYFOLDER64%\"
IF EXIST "%MARATHON_CSC_PATH%\System.Core.dll"       if defined ASSEMBLYFOLDER64 %ADMIN_PIPE% xcopy>>%LOGFILE% /f /y /v "%MARATHON_CSC_PATH%\System.Core.dll" "%ASSEMBLYFOLDER64%\"

:PVTFSC_ONLY
REM == FSHARP?
REM:[12/7/2007 a-sazach] ASSUMPTION: At this point in time MasterRun will be used ONLY for private FSharp runs.  
REM:    i.e. MARATHON_CSC_PATH is defined
REM     Fsharp expects the bits to be in \bin, but MasterRun puts them in \privbin.  
REM:    I am going to leave the MasterRun.bat logic (\privbin) intact.  And copy to \bin in the custom setup step
if /i "%X_COMPONENT%" == "FSHARP" (
    if defined MARATHON_FSC_PATH (
        xcopy>>%LOGFILE% /f /y /v %MARATHON_FSC_PATH%\* %TESTROOT%\privbin\* 
    ) else (
        call :ERROR MARATHON_FSC_PATH must be defined for FSharp runs.
    )
)


goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
rem // SETGREENBITSPATH is used to set the path to the 3.0 C# Compiler
rem // It is only set if we are *NOT* doing a private run, i.e. MARATHON_CSC_PATH
rem // is not set.
:SETGREENBITSPATH

if defined MARATHON_CSC_PATH goto :EOF

REM == Once again, ugly code because of the parenthesis (64bit)
if defined X_WOWRUN if "%CrossRun%"=="1" set PATH=%GREENBITS32%;%GREENBITS64%;%PATH%
if defined X_WOWRUN if "%CrossRun%"=="0" set PATH=%GREENBITS32%;%GREENBITS64%;%PATH%
if defined X_WOWRUN if "%CrossRun%"=="2" set PATH=%GREENBITS64%;%GREENBITS32%;%PATH%
if NOT defined X_WOWRUN if "%RUNPLATFORM%"=="x86" set PATH=%GREENBITS32%;%PATH%
if NOT defined X_WOWRUN if NOT "%RUNPLATFORM%"=="x86" set PATH=%GREENBITS64%;%PATH%

echo PATH after SETGREENBITSPATH:
echo %PATH%

goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
rem // REGPRIVCOMP is used to register the private components (for both C# and F#) 
rem // Note the primitive for of "switch" translated into IF... GOTO 
rem // to stay away from parenthesis inside "then/else" bodies of an IF statement 
:REGPRIVCOMP

:REGPRIVCOMP_CSC
if NOT defined MARATHON_CSC_PATH goto :REGPRIVCOMP_FSC

	%ADMIN_PIPE% sn -Vr "%VS100COMNTOOLS%..\IDE\PublicAssemblies\Microsoft.VisualStudio.DebuggerVisualizers.dll"
	%ADMIN_PIPE% gacutil -if "%VS100COMNTOOLS%..\IDE\PublicAssemblies\Microsoft.VisualStudio.DebuggerVisualizers.dll"

	if NOT defined X_WOWRUN if     "%RUNPLATFORM%"=="x86" %ADMIN_PIPE% sn -Vr "%ASSEMBLYFOLDER32%\System.Core.dll"
	if NOT defined X_WOWRUN if NOT "%RUNPLATFORM%"=="x86" %ADMIN_PIPE% sn -Vr "%ASSEMBLYFOLDER64%\System.Core.dll"
	if NOT defined X_WOWRUN if     "%RUNPLATFORM%"=="x86" %ADMIN_PIPE% gacutil -if "%ASSEMBLYFOLDER32%\System.Core.dll"
	if NOT defined X_WOWRUN if NOT "%RUNPLATFORM%"=="x86" %ADMIN_PIPE% gacutil -if "%ASSEMBLYFOLDER64%\System.Core.dll"

	REM == This is needed on 64bit (in general, due to setup issues)
	if NOT defined X_WOWRUN if NOT "%RUNPLATFORM%"=="x86" pushd "%VS100COMNTOOLS%..\IDE\Remote Debugger\%RUNPLATFORM%" && msvsmon.exe /regautolaunch && popd

:REGPRIVCOMP_FSC
if NOT defined MARATHON_FSC_PATH goto :REGPRIVCOMP_DONE

	%ADMIN_PIPE% gacutil -if "%TESTROOT%\privbin\FSharp.Core.dll"
	%ADMIN_PIPE% gacutil -if "%TESTROOT%\privbin\FSharp.Compatibility.dll"

:REGPRIVCOMP_DONE

goto :EOF


rem /////////////////////////////////////////////////////////////////////////////
:VERIFYBITS
if defined TRACE %TRACE% [proc %0 %*]

	call :VERIFYBIT perl.exe
	call :VERIFYBIT runall.pl
	call :VERIFYBIT csc.exe
	if not defined X_NOCPP (
		call :VERIFYBIT cl.exe
	)
	
	if not defined X_NOSDK (
		call :VERIFYBIT sn.exe
		call :VERIFYBIT al.exe
		call :VERIFYBIT peverify.exe
		call :VERIFYBIT ildasm.exe
	)

goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:SETTIMESTAMP
for /f %%i IN ('perl -e "($ss,$mm,$hh,$mday,$mon,$year,$x,$x,$z) = localtime; $_ = sprintf(\"%%02d%%02d%%02d_%%02d%%02d%%02d\",$year%%100, $mon+1, $mday, $hh, $mm, $ss); print"') do set TIMESTAMP=%%i
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:ADDTOPATH
if defined TRACE %TRACE% [proc %0 %*]
	echo %*
	set path=%path%%*
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:ADDTOLIB
if defined TRACE %TRACE% [proc %0 %*]
	echo %*
	set LIB=%LIB%%*
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:PREPENDTOPATH
if defined TRACE %TRACE% [proc %0 %*]
	echo %*
	set path=%*%path%
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:PREPENDTOLIB
if defined TRACE %TRACE% [proc %0 %*]
	echo %*
	set LIB=%*%LIB%
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
rem /// Set errorlevel to 0/1 if found/not_found
rem /// also set global env variable %WHEREIS% to the result
:WHEREIS
@IF EXIST %1 (@set WHEREIS=%CD%\%~nx1&&@EXIT /B 0)
@for /f "usebackq" %%f in ('%1') do @if "%%~$PATH:f"=="" (@set WHEREIS=&&@EXIT /B 1) else (@set WHEREIS=%%~$PATH:f&&@echo %%~$PATH:f&&@EXIT /B 0)
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////
:VERIFYBIT
if defined TRACE %TRACE% [proc %0 %*]
	call :WHEREIS %1
	if ERRORLEVEL 1 (call :ERROR %1 Could not be verified !! ) else ( echo %1 OK )
goto :EOF


rem /////////////////////////////////////////////////////////////////////////////	
:CHECKANDCALL
	if defined TRACE %TRACE% [proc %0 %*]
		call :WHEREIS %1
Rem Need to clean this flag for next tests as more than 1 test running on the same machine
		set X_CUSTOMCLEANUP=
		if ERRORLEVEL 1 (call :ERROR %1 was not found ) else (call %1) 
goto :EOF

rem /////////////////////////////////////////////////////////////////////////////


:USAGE

echo 
:ERROR
	echo %*
	if /i "%X_MODE%" == "MARATHON" (
		pause
	)
goto :EOF

:EOF
