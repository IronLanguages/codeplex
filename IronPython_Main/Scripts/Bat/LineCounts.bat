rem del /s /q /a-r *.*
tf get /version:D%1
%DLR_ROOT%\Languages\IronPython\ipy.exe %DLR_ROOT%\Scripts\Python\linetrack.py
popd
