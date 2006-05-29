@set ROOT=%~dp0
@pushd "%ROOT%
@"%ROOT:~0,-9%IronPythonConsole.exe" %*
@popd
