if {%1}=={} goto :ERROR
SET X_RUNALL_ARGS=%X_RUNALL_ARGS% -ltags:SQL2005,SQL2000_JPN
SET I_BUGSKIP=NotSupportYet
call %MARATHON_FILES%\MasterRun.bat -component dlinq -mode marathon -custsetup Setenv.bat  %*

if "" == "%CodeCoverage%" GOTO :NormalRun

%TESTROOT%\Dlinq\test\Harness\QACodeCoverage\VSPerfCmd.exe /shutdown
mkdir %DLINQ_LOG_SERVER%\QACodeCoverage\%ComponentVersion%
copy /y %TESTROOT%\Dlinq\test\QA.coverage %DLINQ_LOG_SERVER%\QACodeCoverage\%ComponentVersion%\QA.Coverage
copy /y %TESTROOT%\Dlinq\test\Harness\bin\* %DLINQ_LOG_SERVER%\QACodeCoverage\%ComponentVersion%\*

:NormalRun

goto :EOF

:ERROR
echo For a marathon run you must pass a guid
pause