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

###############################################################################
#Check to ensure ipyw.exe works as expected
#
#TODO:
# * run the tests against pythonw.exe to ensure compatability

#Emit the line needed to rerun this test
echo "To re-run this test execute the following line from a PowerShell prompt:"
echo     $MyInvocation.Line
echo "---------------------------------------------------------------------"
if (! (test-path  $env:ROWAN_BIN\ipyw.exe))
{
    write-error "Must set the ROWAN_BIN environment variable or build ipy.exe!"
    exit 1
}
set-alias ipyw $env:ROWAN_BIN\ipyw.exe

#------------------------------------------------------------------------------
function setup
{
	echo "Setting up test run..."
	echo ""

    #Total number of errors encountered
    $global:ERRORS = 0    
    
    if($env:IPY_OPTIONS -eq $null)
    {
        echo "Setting the IPY_OPTIONS environment variable."
        $env:IPY_OPTIONS=""
    }

    push-location $env:MERLIN_ROOT\Languages\IronPython\Tests\specialcontext

    #Kill off any ipyw processess
    stop-process -name ipyw 2> $null
    sleep 3
    $proc_list = @(get-process | where-object { $_.ProcessName -match "^ipyw" })
    if ($proc_list.Length -ne 0) 
    {
    	write-error "Failed: ipyw currently running - $proc_list"
    	exit 1
    }    
}

#------------------------------------------------------------------------------
#-- Test a few sets of modes just to ensure they emit no output
function test-nooutput
{
    $ipy_modes = @("", "-h", "-c 'print None'", "-c 'print BadNone'", "does_not_exist.py", "-X:NoSuchMode", "-X:NoSuchMode does_not_exist.py")
    foreach ($mode in $ipy_modes) 
    {
	    $stuff = ipyw $mode.Split(" ") 2>&1
	    if (! $?) 
	        {
	        write-error "Failed: ipyw.exe returned a non-zero exit code!"
	        $global:ERRORS++
	        }
	    if ("$stuff" -ne "") 
	    {
    	    write-error "Failed: the output of ipyw.exe isn't empty - $stuff"
	        $global:ERRORS++
	    }
    }

    #cleanup
    #Kill off ipyw processess
    echo "CodePlex Work Item 12900 - must forcefully kill ipyw.exe processes"
	taskkill /F /IM ipyw.exe /T 2> $null
	sleep 5
    $proc_list = @(get-process | where-object { $_.ProcessName -match "^ipyw" })
    if ($proc_list.Length -ne 0) 
    {
    	write-error "Failed: could not stop ipyw processes started by this script - $proc_list"
    	exit 1
    }    
}
#------------------------------------------------------------------------------
#-- Run a real test case

function test-verify_ipyw
{
    #scripts executed by ipyw must write their output to log files as ipyw throws away
    #stdout/stderr
    $test_log = "verify_ipyw.log"
    if (test-path $test_log)
    {
    	rm $test_log
    }


    #run the ipyw verification script
    $stuff = ipyw $env:IPY_OPTIONS.Split(" ") verify_ipyw.py some_param 2>&1
    if (! $?) 
    {
        write-error "Failed: ipyw.exe returned a non-zero exit code!"
        $global:ERRORS++
    }
    if ("$stuff" -ne "") 
    {
        write-error "Failed: the output of ipyw.exe isn't empty - $stuff"
        $global:ERRORS++
    } 
    #verify_ipyw.py has a 'sleep' statement so it should still be alive...
    $proc_list = @(get-process | where-object { $_.ProcessName -match "^ipyw" })
    if ($proc_list.Length -ne 1) 
    {
        write-error "Failed: verify_ipyw.py exited prematurely - $proc_list"
        write-error "        ipyw.exe should have been an active process for no less than five seconds!"
        $global:ERRORS++
    }
    else
    {
        #Give ipyw.exe a chance to exit
        sleep 30
    }

    #Make sure ipyw.exe has now exited
    $proc_list = @(get-process | where-object { $_.ProcessName -match "^ipyw" })
    if ($proc_list.Length -ne 0) 
    {
        write-error "Failed: verify_ipyw.py never exited - $proc_list"
        $global:ERRORS++
    }


    #check to see if the process created the log
    if (!(test-path $test_log))
    {
    	write-error "Failed: 'ipyw verify_ipyw.py' never created $test_log!"
    	$global:ERRORS++
    	exit 1
    }

    #make sure the log contains the right data
    $stuff = gc $test_log
    $stuff = [string]$stuff
	
    if($stuff.Contains("FAILED")) 
    {
	    write-error "Failed some test in verify_ipyw.py: $stuff"
	    $global:ERRORS++
    }
    if(! $stuff.Contains("PASSED"))
    {
    	write-error "Failed: verify_ipyw.py never emitted 'PASSED' - $stuff"
    	$global:ERRORS++
    }
}

#------------------------------------------------------------------------------
setup

$tests = @("test-nooutput", "test-verify_ipyw")
foreach ($exe_test in $tests)
{
	echo "Running $exe_test..."
	&$exe_test
}

if ($global:ERRORS -ne 0)
{
    exit 1
}