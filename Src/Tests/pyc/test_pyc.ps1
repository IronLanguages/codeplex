#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

#USAGE: powershell %CD%\test_pyc.ps1 ..\..\Public\Tools\Scripts


#------------------------------------------------------------------------------
#--Sanity
if ($env:ROWAN_BIN -eq $null) {
    echo "Cannot run this test without ROWAN_BIN being set!"
    exit 1
}
if (! (test-path $env:ROWAN_BIN\ipy.exe)) {
    echo "Cannot run this test without ipy.exe being built!"
    exit 1
}

#------------------------------------------------------------------------------
#--Prereqs and globals

set-alias IPY_CMD $env:ROWAN_BIN\ipy.exe 
$CURRPATH = split-path -parent $MyInvocation.MyCommand.Definition
$TOOLSPATH = $args[0]
if (! (test-path "$TOOLSPATH\pyc.py")) {
	echo "Cannot run this test without access to $TOOLSPATH\pyc.py!"
    exit 1
}
$FINALEXIT = 0  #We'll use this value for the final exit code of this script
$SLEEP_TIME = 3
pushd $TOOLSPATH #We'll invoke Pyc from Tools\Scripts


function prepare-testcase {
    #Kill off any stagnant test processes
    foreach ($x in @("winforms_hw")) {
        stop-process -name $x 2> $null
        sleep $SLEEP_TIME
        $proc_list = @(get-process | where-object { $_.ProcessName -match "^$x" })
        if ($proc_list.Length -ne 0)  {
            echo "FAILED: $x is currently running - $proc_list"
            $FINALEXIT = 1
        }    
    }
    
    #Remove existing crud
    rm -force *.dll
    rm -force *.exe

    #Copy over prereqs for running pyc.py
    cp -force $env:ROWAN_BIN\IronPython*dll .
    cp -force $env:ROWAN_BIN\Microsoft.*dll .
}


function testcase-helper ($cmd, $expected_files, $exe, $expected_exe_exitcode, $expected_exe_stdout, $arguments) {
    prepare-testcase
    echo "------------------------------------------------------------------------------"
    echo "TEST CASE: $cmd"

    #--Run pyc.py
    $cmd_array = $cmd.split(" ")
    IPY_CMD pyc.py $cmd_array
    if (! $?) {
        echo "FAILED: IPY_CMD pyc.py $cmd"
        $FINALEXIT = 1
        return
    }
    
    #--Ensure pyc.py generated all expected files
    foreach($x in $expected_files) {
        if (! (test-path $PWD\$x)) {
            echo "FAILED: $PWD\$x was not generated!"
            $FINALEXIT = 1
            return
        }
    }

    #--Ensure the exe we will run has been generated
    if (! (test-path $PWD\$exe)) {
        echo "FAILED: $PWD\$exe was not generated!"
        $FINALEXIT = 1
        return
    }
 
    testcase-runner $cmd $exe $expected_exe_exitcode $expected_exe_stdout $arguments
}

function testcase-runner ($cmd, $exe, $expected_exe_exitcode, $expected_exe_stdout, $arguments) {    
    #--Run the generated exe
    $command = Get-Command "$PWD\$exe"
    echo $arguments
    $exe_out = &$command $arguments
    if ($LASTEXITCODE -ne $expected_exe_exitcode) {
        echo "FAILED: actual exit code, $LASTEXITCODE, of $exe was not the expected value ($expected_exe_exitcode)"
        $FINALEXIT = 1
        return
    }
    
    #--Consoleless applications are a special case
    if ($cmd.Contains("/target:winexe")) {
        if ($exe_out -ne $null) {
            echo "FAILED: there should be no console output instead of '$exe_out' for $exe."
            $FINALEXIT = 1
            return
        }
        #The real expected exe stdout is hidden in the MainWindowTitle
        $temp_name = $exe.Split(".exe")[0]
        sleep $SLEEP_TIME
        $exe_out = (get-process | where-object { $_.ProcessName -match "^$temp_name" }).MainWindowTitle
        stop-process -name $temp_name 2> $null
    }
    
    if ($exe_out -ne $expected_exe_stdout) {
        echo "FAILED: '$exe_out' was not the expected output ($expected_exe_stdout) for $exe."
        $FINALEXIT = 1
        return
    }

    echo ""
    echo "PASSED"
    echo ""
}


#------------------------------------------------------------------------------
#--Test cases

#http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=21976
foreach ($x in (dir $CURRPATH\*.py)) {
	$temp_name = $x.Name
	cp -force $x $TOOLSPATH\$temp_name
}

#--Console HelloWorld w/ args
testcase-helper "/main:$CURRPATH\pycpkgtest.py $CURRPATH\pkg\a.py $CURRPATH\pkg\b.py $CURRPATH\pkg\__init__.py " @("pycpkgtest.exe", "pycpkgtest.dll") "pycpkgtest.exe" 0 "<module 'pkg.b' from 'pkg\b'>"

#--Console HelloWorld w/ args
testcase-helper "/main:$CURRPATH\console_hw_args.py" @("console_hw_args.exe", "console_hw_args.dll") "console_hw_args.exe" 0 "(Compiled) Hello World ['foo']" "foo"

#--Console HelloWorld
testcase-helper "/main:$CURRPATH\console_hw.py" @("console_hw.exe", "console_hw.dll") "console_hw.exe" 0 "(Compiled) Hello World"

#--Console HelloWorld with a single dependency
testcase-helper "$CURRPATH\other_hw.py /main:$CURRPATH\console_hw.py" @("console_hw.exe", "console_hw.dll") "console_hw.exe" 0 "(Compiled) Hello World"

#--WinForms HelloWorld
testcase-helper "/main:$CURRPATH\winforms_hw.py /target:winexe" @("winforms_hw.exe", "winforms_hw.dll") "winforms_hw.exe" 0 "(Compiled WinForms) Hello World"


#http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=21976
testcase-helper "/main:$CURRPATH\pycpkgtest.py $CURRPATH\pkg\a.py $CURRPATH\pkg\b.py $CURRPATH\pkg\__init__.py " @("pycpkgtest.exe", "pycpkgtest.dll") "pycpkgtest.exe" 0 "<module 'pkg.b' from 'pkg\b'>"

foreach ($x in (dir $CURRPATH\*.py)) {
    echo $x.Name
	$temp_name = $x.Name
	rm -force $TOOLSPATH\$temp_name
}

testcase-runner "/main:$CURRPATH\pycpkgtest.py $CURRPATH\pkg\a.py $CURRPATH\pkg\b.py $CURRPATH\pkg\__init__.py " "pycpkgtest.exe" 0 "<module 'pkg.b' from 'pkg\b'>"

#------------------------------------------------------------------------------
#Cleanup and exit
rm -force *.dll
rm -force *.exe
popd

exit $FINALEXIT
