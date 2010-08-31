if {%1}=={} goto :ERROR

REM GET FILES
call %MARATHON_FILES%\masterget.bat FSHARP

REM RUN TESTS
call %MARATHON_FILES%\Masterrun.bat -component fsharp -mode marathon %* -runroot %TESTROOT%\FSHARP\tests  -custsetup build-and-run-runall-custom-setup.bat -custclean build-and-run-runall-custom-cleanup.bat

goto :EOF

:ERROR
echo For a marathon run you must pass a guid
pause