if {%1}=={} goto :ERROR

call %MARATHON_FILES%\MasterRun.bat -component cseeremote -mode marathon -custclean CleanupRemoteGlassRun.bat -custsetup SetupRemoteGlassRun.bat %* %MARATHON_WOW%
goto :EOF

:ERROR
echo For a marathon run you must pass a guid
pause