@echo off
setlocal

set IRONPYTHONPATH=%IRONPYTHONPATH%;%Merlin_root%\..\External\Languages\ironpython21\25\Lib

%MERLIN_ROOT%\Bin\Debug\ipy Failed_Testcases\g_%1.py > Failed_Testcases\g_%1_ipy_out.txt 2>&1
%MERLIN_ROOT%\..\External\Languages\ironpython21\25\Python.exe Failed_Testcases\g_%1.py > Failed_Testcases\g_%1_cpy_out.txt 2>&1

@echo on
fc /l /n Failed_Testcases\g_%1_cpy_out.txt Failed_Testcases\g_%1_ipy_out.txt

@echo off
if "%2"=="-cpy" (
    "%PROGRAM_FILES_32%\Beyond Compare 2\Bc2.exe" Failed_Testcases\g_%1.py CPy_Testcases\%1.py
)
if "%2"=="-ipy" (
    "%PROGRAM_FILES_32%\Beyond Compare 2\Bc2.exe" Failed_Testcases\g_%1.py %1.py
)