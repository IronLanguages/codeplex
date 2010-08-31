if {%1}=={} goto :ERROR
call %MARATHON_FILES%\MasterRun.bat -component corelinq -mode marathon -custsetup setup.cmd %* %MARATHON_WOW%
goto :EOF

:ERROR
echo For a marathon run you must pass a guid
pause