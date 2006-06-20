CSC=csc

all:../IronMath.dll ../IronPython.dll ../ipy.exe ../ipyw.exe IronPythonTest/bin/Debug/IronPythonTest.dll

../IronMath.dll:
	$(CSC) -t:library -out:../IronMath.dll -recurse:IronMath/*.cs

../IronPython.dll: ../IronMath.dll
	$(CSC) -t:library -r:../IronMath.dll -r:System.Drawing.dll -r:System.Design.dll -out:../IronPython.dll -recurse:IronPython/*.cs

../ipy.exe: ../IronPython.dll
	$(CSC) -t:exe -r:../IronPython.dll -out:../ipy.exe -recurse:IronPythonConsole/*.cs

../ipyw.exe: ../IronPython.dll
	$(CSC) -t:exe -r:../IronPython.dll -t:winexe -define:IRONPYTHON_WINDOW -out:../ipyw.exe -recurse:IronPythonConsole/*.cs

IronPythonTest/bin/Debug/IronPythonTest.dll: ../IronMath.dll ../IronPython.dll
	mkdir IronPythonTest/bin/Debug
	$(CSC) -t:library -r:../IronMath.dll -r:../IronPython.dll -out:IronPythonTest/bin/Debug/IronPythonTest.dll -recurse:IronPythonTest/*.cs
