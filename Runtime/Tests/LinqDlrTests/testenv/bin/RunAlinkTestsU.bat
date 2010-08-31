if {%1}=={} goto :ERROR
call %MARATHON_FILES%\MasterRun.bat -component alink -mode marathon %* %MARATHON_WOW%
goto :EOF

:ERROR
echo For a marathon run you must pass a guid
pause