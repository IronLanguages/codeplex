if "%1" == "" goto Exit
if not exist %1 goto Exit
if not exist %1\nul goto Exit

pushd %1

if not "%ERRORLEVEL%"=="0" goto Exit

attrib -r -h -s
for /F "delims=" %%d in ('dir /AD /B') do rd /s /q "%%d"
del /q *.*

popd

:Exit
