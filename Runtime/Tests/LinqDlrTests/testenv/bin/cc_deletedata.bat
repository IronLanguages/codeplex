covercmd.exe /close
for /R %Coverage% %%f IN (*.covdata) DO del "%%f"
