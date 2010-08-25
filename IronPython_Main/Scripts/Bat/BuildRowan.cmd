if EXIST "%DLR_ROOT%\Internal\Dlr.sln" (
    msbuild.exe %DLR_ROOT%\Internal\Dlr.sln /nologo %*
) else (
    msbuild.exe %DLR_ROOT%\Solutions\IronPython.sln /nologo %*    
)
