@set ROOT=%~dp0
@pushd "%ROOT%
@"%ROOT:~0,-9%ipy.exe" %*
@popd
