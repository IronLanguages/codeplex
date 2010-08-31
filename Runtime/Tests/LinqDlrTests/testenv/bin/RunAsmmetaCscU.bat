if {%1}=={} goto :ERROR
call %MARATHON_FILES%\MasterRun.bat -runroot d:\school\csharp\source -component asmmeta -mode marathon -runallargs "-ttags:generics,attributes,friend" -custsetup asmmetasetup.bat %* %MARATHON_WOW%
goto :EOF

:ERROR
echo For a marathon run you must pass a guid
pause