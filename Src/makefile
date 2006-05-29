CSC=csc

all:../IronMath.dll ../IronPython.dll ../IronPythonConsole.exe IronPythonTest/bin/Debug/IronPythonTest.dll

../IronMath.dll:
	$(CSC) -t:library -out:../IronMath.dll -recurse:IronMath/*.cs

../IronPython.dll: ../IronMath.dll
	$(CSC) -t:library -r:../IronMath.dll -r:System.Drawing.dll -r:System.Design.dll -out:../IronPython.dll -recurse:IronPython/*.cs

../IronPythonConsole.exe: ../IronPython.dll
	$(CSC) -t:exe -r:../IronPython.dll -out:../IronPythonConsole.exe -recurse:IronPythonConsole/*.cs


IronPythonTest/bin/Debug/IronPythonTest.dll: ../IronMath.dll ../IronPython.dll
	mkdir IronPythonTest/bin/Debug
	$(CSC) -t:library -r:../IronMath.dll -r:../IronPython.dll -out:IronPythonTest/bin/Debug/IronPythonTest.dll -recurse:IronPythonTest/*.cs
