if {%1}=={} goto :ERROR

call %MARATHON_FILES%\MasterRun.bat -component csee -mode marathon -custsetup SetupGlassRun.bat %* %MARATHON_WOW%
goto :EOF

:ERROR
echo For a marathon run you must pass a guid
pause