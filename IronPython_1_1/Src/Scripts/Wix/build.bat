setlocal ENABLEEXTENSIONS

set _BAT=%~dp0
set _ROOT=%_BAT:~0,-24%
set _WIX=%_ROOT%\External\Wix
set _PUBLIC=%_ROOT%\Public

if "%PKG_MSIFILE%"=="" set PKG_MSIFILE=IronPython.msi

%_WIX%\candle IronPython.wxs Core.wxs Doc.wxs Src.wxs
%_WIX%\light -b %_PUBLIC% -out %PKG_MSIFILE% -loc %_WIX%\WixUI_en-us.wxl IronPython.wixobj Core.wixobj Doc.wixobj Src.wixobj %_WIX%\wixui.wixlib

endlocal
