del /s /q /a-r *.*
tf get /version:D%1
pushd %DLR_ROOT%\Solutions
msbuild -t:Rebuild -p:Configuration=Release IronPython.sln
dir /s ..\bin\Release\*.dll
popd
