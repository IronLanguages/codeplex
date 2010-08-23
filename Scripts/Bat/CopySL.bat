@echo off
rem Wrapper to invoke our copy_silverlight script

%DLR_ROOT%\Util\IronPython\ipy.exe "%DLR_ROOT%\Scripts\Python\copy_silverlight.py" %*
if NOT "%ERRORLEVEL%" == "0" (
    echo ipy.exe copy_silverlight.py failed
    exit /b 1
)