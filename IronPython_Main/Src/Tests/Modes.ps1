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

###################################################################################
#Tests the various combinations/permutations of executable modes
#TODO:
# * need a much better mechanism for testing -u
# * nonsense modes
# * negative modes

#Total number of errors encountered
$global:ERRORS = 0

#Generated tests
$global:TEST_DIR = "$env:TMP\ModeTests"
$global:HELLO = "$global:TEST_DIR\hello_test.py"
$global:RECURSION = "$global:TEST_DIR\recursion_test.py"
$global:EXCEPTION = "$global:TEST_DIR\except_test.py"
$global:DOCSTRING = "$global:TEST_DIR\docstring_test.py"
$global:TRACEBACK = "$global:TEST_DIR\traceback_test.py"

#There are tests which are specifically disabled or enabled for debug binaries
$global:IS_DEBUG = "$env:ROWAN_BIN".ToLower().EndsWith("bin\debug")

#Some tests fail under x64
$global:IS_64 = (test-path $env:SystemRoot\SYSWOW64) -or (test-path $env:systemroot\SYSWOW64)

###################################################################################
function test-setup
{
	echo "Setting up test run."
	
	#Add the temporary test directory to the path if necessary
	if ($env:IRONPYTHONPATH -eq $null)
	{
		$env:IRONPYTHONPATH="$global:TEST_DIR"
	}
	elseif (! $env:IRONPYTHONPATH.Contains($global:TEST_DIR))
	{
		$env:IRONPYTHONPATH="$env:IRONPYTHONPATH;$global:TEST_DIR"
	}
	
	#create the temporary test directory
	if (test-path $global:TEST_DIR) { rm -recurse -force $global:TEST_DIR }
	mkdir $global:TEST_DIR > $null
	
	#--Hello World
	echo "print 'Hello World 1st Line'" | out-file -encoding ascii $global:HELLO
	echo "x = 2 * 3" | out-file -encoding ascii -append $global:HELLO
	echo "print x" | out-file -encoding ascii -append $global:HELLO
	
	#--Recursion
	echo "def fact(n):" | out-file -encoding ascii $global:RECURSION
    echo "    if n==1: return 1" | out-file -encoding ascii -append $global:RECURSION
    echo "    else: return n * fact(n-1)" | out-file -encoding ascii -append $global:RECURSION
    echo "" | out-file -encoding ascii -append $global:RECURSION
    echo "import sys" | out-file -encoding ascii -append $global:RECURSION
    echo "print fact(int(sys.argv[1]))" | out-file -encoding ascii -append $global:RECURSION
	
	#--Exceptions
	echo "def hwExcept():" | out-file -encoding ascii $global:EXCEPTION
	echo "    raise 'Hello World Exception'" | out-file -encoding ascii -append $global:EXCEPTION
	echo "" | out-file -encoding ascii -append $global:EXCEPTION
	echo "def complexExcept():" | out-file -encoding ascii -append $global:EXCEPTION
	echo "    import System" | out-file -encoding ascii -append $global:EXCEPTION
	echo "    import exceptions" | out-file -encoding ascii -append $global:EXCEPTION
	echo "    raise exceptions.OverflowError(exceptions.RuntimeError(System.FormatException('format message', System.Exception('clr message'))))" | out-file -encoding ascii -append $global:EXCEPTION
	echo "" | out-file -encoding ascii -append $global:EXCEPTION
	
	#--Doc strings
	echo "def a():" | out-file -encoding ascii $global:DOCSTRING
	echo "    '''stuff and more stuff taking up space'''" | out-file -encoding ascii -append $global:DOCSTRING
	echo "    pass" | out-file -encoding ascii -append $global:DOCSTRING
	echo "" | out-file -encoding ascii -append $global:DOCSTRING
	echo "print a.__doc__" | out-file -encoding ascii -append $global:DOCSTRING

	#--TRACEBACK
	echo "def abc(): return 1/0" | out-file -encoding ascii $global:TRACEBACK
	echo "try:" | out-file -encoding ascii -append $global:TRACEBACK
	echo "    abc()" | out-file -encoding ascii -append $global:TRACEBACK
	echo "except:" | out-file -encoding ascii -append $global:TRACEBACK
	echo "    import sys" | out-file -encoding ascii -append $global:TRACEBACK
	echo "    if sys.exc_info()[2]==None:" | out-file -encoding ascii -append $global:TRACEBACK
	echo "        print 'No traceback'" | out-file -encoding ascii -append $global:TRACEBACK
	echo "        sys.exit(0)" | out-file -encoding ascii -append $global:TRACEBACK
	echo "    print sys.exc_info()[2].tb_lineno" | out-file -encoding ascii -append $global:TRACEBACK
	echo ""
}

###################################################################################
function hello-helper
{
	set-alias exe $args[0]

	$stuff = exe $args[1..$args.Length] $global:HELLO
	if (! $?) 
	{
		write-error "Failed hello-helper ($args[0] $args[1..$args.Length] $global:HELLO)"
		write-error "$args[0] terminated with a non-zero exit code."
		$global:ERRORS++
	}
	elseif ($stuff[0] -ne "Hello World 1st Line") 
	{
		write-error "Failed hello-helper ($args[0] $args[1..$args.Length] $global:HELLO)"
		write-error "Missing output: Hello World 1st Line"
		$global:ERRORS++
	}
	elseif($stuff[1] -ne 6) 
	{
		write-error "Failed hello-helper ($args[0] $args[1..$args.Length] $global:HELLO)"
		write-error "Missing output: 6"
		$global:ERRORS++
	}
}

###################################################################################
function test-pymodes($pyexe) 
{	
	set-alias pyexe $pyexe

	echo "Testing Python modes using $pyexe"
	echo ""

	#------------------------------------------------------------------------------
	echo "The following modes are well tested by other tests and will not be covered"
	echo "by this script:"
	echo "    -c (test_stdconsole.py)"
	echo "    -x (test_stdconsole.py)"
	echo "    -V (test_stdconsole.py)"
	echo "    -i (test_interactive.py)"
	echo "    -S (test_stdconsole.py)"
	echo "    -t (test_stdconsole.py)"
	echo "    -tt (test_stdconsole.py)"
	echo "    -E (test_stdconsole.py)"
	echo "    -W (test_stdconsole.py)"
	echo ""

	#------------------------------------------------------------------------------
	## -c
	echo "Testing -c ..."
	
	echo "CodePlex Work Item 11103"
	$stuff = pyexe -c "True"
	#if ($stuff -ne "") { write-error "Failed: $stuff"; $global:ERRORS++ }
	
	#------------------------------------------------------------------------------
	## -h
	echo "Testing -h ..."
	
	#Just a simple sanity check will suffice as there are differences in output
	#between CPython and IronPython
	
	$stuff = pyexe -h 
	$stuff = [string]$stuff
	
	$expected_stuff = "-c cmd", "-V ", "-h "

	foreach ($expected in $expected_stuff)
	{
		if (!$stuff.Contains($expected))
		{
			write-error "Failed: $expected"; $global:ERRORS++
		}
	}
	
	#------------------------------------------------------------------------------
	## -O
	echo "Testing -O ..."
	
	#This flag should only affect *.pyo files which IronPython does not 
	#support.  Just make sure it doesn't break anything.
	
	#Run Hello World test w/o -O (test modes M1 and M2 both use -O flag)
	hello-helper $pyexe
	
	hello-helper $pyexe -O
	
	#------------------------------------------------------------------------------
	## -v
	echo "Testing -v ..."

	if(! $pyexe.Endswith("python.exe"))
	{
		hello-helper $pyexe -v 2> $null  #Send stderr to $null for CPython
	}
	else
	{
		echo "Skipping hello-helper test for CPython -v flag (python.exe emits a non-zero exit code)"
	}

	echo "CodePlex Work Item 10819"
	$stuff = pyexe -v -c "print 'HelloWorld'" 2>&1
	#Just ensure there's more output than "HelloWorld"
	if ($false) #$stuff.Length -le "HelloWorld".Length) 
	{
		$stuff = $stuff.Length
		write-error "Failed: $stuff"
		$global:ERRORS++
	}
	
	
	#------------------------------------------------------------------------------
	## -u
	echo "Need verification for -u."

	#For now just ensure it does not throw
	hello-helper $pyexe -u
	
	#------------------------------------------------------------------------------
	## -OO
	echo "Testing -OO ..."
	
	#This flag should only affect *.pyo files which IronPython does not 
	#support.  Just make sure it doesn't break anything.
	
	hello-helper $pyexe -OO
	
	echo "CodePlex Work Item 11102"
	$stuff = pyexe -OO $global:DOCSTRING
	#if ($stuff -ne "stuff and more stuff taking up space") {write-error "Failed: $stuff"; $global:ERRORS++}

	#------------------------------------------------------------------------------
	## -Q arg
	echo "testing -Q ..."
	
	#check stdout first...
	$test_hash = @{ '-Qnew -c "print 3/2"' = "1.5";
					'-Qold -c "print 3/2"' = "1";
					'-Qwarn -c "print 3/2"' = "1";
					'-Qwarnall -c "print 3/2"' = "1";
					'-Q new -c "print 3/2"' = "1.5";
					'-Q old -c "print 3/2"' = "1";
					'-Q warn -c "print 3/2"' = "1";
					'-Q warnall -c "print 3/2"' = "1";
					}
	
	foreach($key in $test_hash.Keys)
	{
		$stuff = pyexe $key.Split(" ") 2> $null 
		if ($stuff -ne $test_hash[$key]) {write-error "Failed: $stuff"; $global:ERRORS++}
	}
	
	#next stderr...
	echo "CodePlex Work Item 11111"
	$test_hash = @{ '-Qnew -c "print 3/2"' = "1.5";
					'-Qold -c "print 3/2"' = "1";
					#'-Qwarn -c "print 3/2"' = "1 -c:1: DeprecationWarning: classic int division";
					#'-Qwarnall -c "print 3/2"' = "1 -c:1: DeprecationWarning: classic int division";
					'-Q new -c "print 3/2"' = "1.5";
					'-Q old -c "print 3/2"' = "1";
					#'-Q warn -c "print 3/2"' = "1 -c:1: DeprecationWarning: classic int division";
					#'-Q warnall -c "print 3/2"' = "1 -c:1: DeprecationWarning: classic int division";
					}
	
	foreach($key in $test_hash.Keys)
	{
		$stuff = pyexe $key.Split(" ") 2>&1
		if ("$stuff" -ne $test_hash[$key]) {write-error "Failed: $stuff"; $global:ERRORS++}
	}
    
}
	
###############################################################################
function assembliesdir-helper
{
	$dlrexe = $args[0]
	
	if (test-path $env:TMP\assemblies_dir) { rm -recurse -force $env:TMP\assemblies_dir }
	mkdir $env:TMP\assemblies_dir > $null
	hello-helper $dlrexe "-X:AssembliesDir" $env:TMP\assemblies_dir $args[1..$args.Length] 
	$stuff = dir $env:TMP\assemblies_dir
	#Just check to make sure it's empty...save assemblies option was not used.
	if ($stuff -ne $null) {write-error "Failed: $stuff"; $global:ERRORS++}
}

function saveassemblies-helper
{
	set-alias dlrexe $args[0]
	
	#Test w/o the use of X:AssembliesDir
	hello-helper $dlrexe "-X:SaveAssemblies" $args[1..$args.Length]
	$stuff = dir $global:TEST_DIR -Name
	
	foreach($expected in @("hello_test.exe", "hello_test.pdb"))
	{
		if (($stuff | select-string $expected) -eq $null) 
		{
			write-error "Failed saveassemblies-helper!"
			write-error "Expected '$expected', but found '$stuff'" 
			$global:ERRORS++
		}
	}
	
	foreach($expected in @("site.exe", "site.pdb"))
	{
		if (($stuff | select-string $expected) -ne $null) 
		{
			write-error "Failed saveassemblies-helper!"
			write-error "Found '$expected' in '$stuff'" 
			$global:ERRORS++
		}
	}
	
	if (test-path $env:TMP\assemblies_dir) { rm -recurse -force $env:TMP\assemblies_dir }
	mkdir $env:TMP\assemblies_dir > $null
	hello-helper $dlrexe "-X:AssembliesDir" $env:TMP\assemblies_dir "-X:SaveAssemblies" $args[1..$args.Length]
	
	$stuff = dir $env:TMP\assemblies_dir -Name
	foreach($expected in @("hello_test.exe", "hello_test.pdb", "site.exe", "site.pdb"))
	{
		if (($stuff | select-string $expected) -eq $null) 
		{
			write-error "Failed saveassemblies-helper (with usage of -X:SaveAssemblies)!"
			write-error "Expected '$expected', but found '$stuff'" 
			$global:ERRORS++
		}
	}
	#Should anything be done with the *.exe's?
}

function exceptiondetail-helper
{
	set-alias dlrexe $args[0]
	$dlrexe = $args[0]

	hello-helper $dlrexe "-X:ExceptionDetail"
	
	$stuff = dlrexe "-X:ExceptionDetail" $args[1..$args.Length] -c "from except_test import *;hwExcept()" 2>&1
	if (! "$stuff".Contains("Hello World Exception")) {write-error "Failed: $stuff"; $global:ERRORS++}
	if (! "$stuff".Contains("at Microsoft.Scripting.")) {write-error "Failed: $stuff"; $global:ERRORS++}
	if (! "$stuff".Contains(":line ")) {write-error "Failed: $stuff"; $global:ERRORS++}
	
	$stuff = dlrexe "-X:ExceptionDetail" $args[1..$args.Length] -c "from except_test import *;complexExcept()" 2>&1
	if (! "$stuff".Contains("OverflowError: System.FormatException: format message ---> System.Exception: clr message")) 
	{
		write-error "Failed: $stuff"; $global:ERRORS++
	}
}

function maxrecursion-helper
{
	set-alias dlrexe $args[0]
	$dlrexe = $args[0]

	#--Trivial cases w/o flag
	$stuff = dlrexe $args[1..$args.Length] $global:RECURSION 1
	if ($stuff -ne 1) {write-error "Failed."; $global:ERRORS++}
	
	$stuff = dlrexe $args[1..$args.Length] $global:RECURSION 2
	if ($stuff -ne 2) {write-error "Failed."; $global:ERRORS++}
	
	$stuff = dlrexe $args[1..$args.Length] $global:RECURSION 3
	if ($stuff -ne 6) {write-error "Failed."; $global:ERRORS++}
	
	$stuff = dlrexe $args[1..$args.Length] $global:RECURSION 20
	if ($stuff -ne 2432902008176640000L) {write-error "Failed."; $global:ERRORS++}
		
	#--Trivial cases w/ flag
	
	#non-recursive script
	foreach ($i in 0..2)
	{
		hello-helper $dlrexe "-X:MaxRecursion" $args[1..$args.Length] $i
	}
	
	#simple recursive script
	foreach($i in 3..5)
	{
		$stuff = dlrexe "-X:MaxRecursion" $i $args[1..$args.Length] $global:RECURSION 3
		if ($stuff -ne 6) {write-error "Failed."; $global:ERRORS++}
	}
	
	#simple recursive script where we miss the MaxRecursion depth by one
	#which should trigger a RuntimeError exception
	foreach ($i in 0..2)
	{
		$x_param = $i + 2
		$script_param = $i + 3
		$stuff = dlrexe "-X:MaxRecursion" $x_param $args[1..$args.Length] $global:RECURSION $script_param 2>&1
		$stuff = "$stuff.ErrorDetails"
		if (!$stuff.Contains("maximum recursion depth exceeded")) {write-error "Failed."; $global:ERRORS++}
	}
	
	echo "CodePlex Work Item 10816"
	foreach ($i in @()) #1..-2)
	{
		$stuff = dlrexe "-X:MaxRecursion" $i $args[1..$args.Length] $global:RECURSION 3 2>&1
		$stuff = "$stuff.ErrorDetails"
		if (!$stuff.Contains("maximum recursion depth exceeded")) {write-error "Failed."; $global:ERRORS++}
	}
}

function notraceback-helper
{
	set-alias dlrexe $args[0]
	$dlrexe = $args[0]

	hello-helper $dlrexe "-X:NoTraceback"
	
	$stuff = dlrexe "-X:NoTraceback" $args[1..$args.Length] $global:TRACEBACK
	if ($stuff -ne "No traceback") {write-error "Failed: $stuff"; $global:ERRORS++}
	
	$stuff = dlrexe $args[1..$args.Length] $global:TRACEBACK
	if (!$global:IS_64)
	{
		if ($stuff -eq "No traceback") {write-error "Failed: $stuff"; $global:ERRORS++}
	}
	else
	{
		echo "CodePlex Work Item 11362"
	}
}

function showclrexceptions-helper
{
	set-alias dlrexe $args[0]
	$dlrexe = $args[0]

	hello-helper $dlrexe "-X:ShowClrExceptions"
	
	$stuff = dlrexe "-X:ShowClrExceptions" $args[1..$args.Length] -c "from except_test import *; hwExcept()" 2>&1
	if(! "$stuff".Contains("Hello World Exception")) {write-error "Failed: $stuff"; $global:ERRORS++}
	if(! "$stuff".Contains("CLR Exception:")) {write-error "Failed: $stuff"; $global:ERRORS++}
	if(! "$stuff".Contains("StringException")) {write-error "Failed: $stuff"; $global:ERRORS++}
	
	$stuff = dlrexe "-X:ShowClrExceptions" $args[1..$args.Length] -c "from except_test import *;complexExcept()" 2>&1
	if (! "$stuff".Contains("OverflowError: System.FormatException: format message ---> System.Exception: clr message")) 
	{
		write-error "Failed: $stuff"; $global:ERRORS++
	}
	if (! "$stuff".Contains("CLR Exception:")) { write-error "Failed: $stuff"; $global:ERRORS++ }
	if (! "$stuff".Contains("OverflowException")) { write-error "Failed: $stuff"; $global:ERRORS++ }
}

function mta-helper
{
	#non-winforms apps.  test with System.Thread*
	$dlrexe = $args[0]
	
	echo "CodePlex Work Item 11014"
	#hello-helper $dlrexe "-X:MTA" $args[1..$args.Length] 
}

###################################################################################
function test-dlrmodes($dlrexe) 
{	
	set-alias dlrexe $dlrexe

	echo "Testing IronPython modes using $dlrexe"
	echo ""

	#------------------------------------------------------------------------------
	echo "The following modes already have sufficient coverage in other tests:"
	echo "    -X:AutoIndent (test_superconsole.py)"
	echo "    -X:GenerateAsSnippets ('M1' RunTests.py option)"
	echo "    -X:PrivateBinding (test_privateBinding.py)"
	echo "    -X:SaveAssmeblies ('M2' RunTests.py option...need verification)"
	echo "    -X:TabCompletion (test_superconsole.py)"
	echo ""

	#------------------------------------------------------------------------------
	echo "The following modes are (or will soon be) undocumented and will not be tested:"
	echo "    -X:IlDebug"
	echo "    -X:PassExceptions"
	echo "    -X:StaticMethods"
	echo ""
	
	#------------------------------------------------------------------------------
	echo "The following modes are or will soon be removed and will not be tested:"
	echo "    -X:ColorfulConsole (likely to be merged)"
	echo "    -X:Frames (probably gone)"
	echo "    -X:NoOptimize"
	echo ""

	#------------------------------------------------------------------------------
	## -D
	echo "-D needs coverage"
	
	hello-helper $dlrexe -D

	#------------------------------------------------------------------------------
	## -X:AssembliesDir
	echo "Testing -X:AssembliesDir ..."	
	assembliesdir-helper $dlrexe 
	
	#------------------------------------------------------------------------------
	## -X:SaveAssemblies
	echo "Testing -X:SaveAssemblies ..."	
	saveassemblies-helper $dlrexe 
	
	#------------------------------------------------------------------------------
	## -X:ExceptionDetail
	echo "Testing -X:ExceptionDetail ..."
	exceptiondetail-helper $dlrexe
	
	#------------------------------------------------------------------------------
	## -X:Interpret
	echo "-X:Interpret needs more coverage"
	hello-helper $dlrexe "-X:Interpret"
	
	#------------------------------------------------------------------------------
	## -X:MaxRecursion
	echo "Testing -X:MaxRecursion ..."
	maxrecursion-helper $dlrexe	
	
	#------------------------------------------------------------------------------
	## -X:MTA
	echo "-X:MTA needs more coverage"
	mta-helper $dlrexe
	
	#------------------------------------------------------------------------------
	## -X:NoTraceback
	echo "Testing -X:NoTraceback ..."
	notraceback-helper $dlrexe
	
	#------------------------------------------------------------------------------
	## -X:ShowClrExceptions
	echo "Testing -X:ShowClrExceptions ..."
	showclrexceptions-helper $dlrexe
	
	#------------------------------------------------------------------------------
	#-- -X:ShowASTs
	echo "-X:ShowASTs needs more coverage"
	
	if ($global:IS_DEBUG)
	{
		$stuff = dlrexe "-X:ShowASTs" $global:HELLO
		#just check to make sure it does not throw for now
		if (! $?) {write-error "Failed."; $global:ERRORS++}
	}
	
	#------------------------------------------------------------------------------
	#-- -X:DumpASTs
	echo "-X:DumpASTs needs more coverage"

	if ($global:IS_DEBUG)
	{
		$stuff = dlrexe "-X:DumpASTs" $global:HELLO
		#just check to make sure it does not throw for now
		if (! $?) {write-error "Failed."; $global:ERRORS++}
	}

	#------------------------------------------------------------------------------
	#-- -X:ShowRules
	echo "-X:ShowRules needs more coverage"
	
	if ($global:IS_DEBUG)
	{
		$stuff = dlrexe "-X:ShowRules" $global:HELLO
		#just check to make sure it does not throw for now
		if (! $?) {write-error "Failed."; $global:ERRORS++}
	}
}

###############################################################################
function test-relatedpy($pyexe) 
{	
	set-alias pyexe $pyexe

	echo "Testing related IronPython modes using $pyexe"
	echo ""
	
	#-X:AutoIndent, -X:ColorfulConsole, -X:TabCompletion, -t, -tt
	echo "Testing -X:AutoIndent, -X:ColorfulConsole, -X:TabCompletion, -t, -tt ..."
	#Just run a few sanity checks to make sure nothing breaks
	hello-helper $pyexe "-X:AutoIndent" "-X:ColorfulConsole" "-X:TabCompletion" -t -tt
	hello-helper $pyexe "-X:AutoIndent" "-X:ColorfulConsole"
	hello-helper $pyexe "-X:AutoIndent" "-X:TabCompletion"
	hello-helper $pyexe "-X:ColorfulConsole" "-X:TabCompletion"
	hello-helper $pyexe "-X:TabCompletion" -t -tt
	hello-helper $pyexe -t -tt
	
	#-X:AssembliesDir, -X:SaveAssemblies
	echo "-X:AssembliesDir and -X:SaveAssemblies are already well tested together."
	
	#-X:ExceptionDetail, -X:NoTraceback, -X:ShowClrExceptions
	echo "Testing -X:ExceptionDetail, -X:NoTraceback, -X:ShowClrExceptions ..."
	hello-helper $pyexe "-X:ExceptionDetail" "-X:NoTraceback" "-X:ShowClrExceptions"
	exceptiondetail-helper $pyexe "-X:ShowClrExceptions" "-X:NoTraceback"
	notraceback-helper $pyexe "-X:ExceptionDetail" "-X:ShowClrExceptions"
	showclrexceptions-helper $pyexe "-X:ExceptionDetail" "-X:NoTraceback"
	
	#-X:Interpret, -X:NoOptimize, -O, -OO
	echo "Testing -X:Interpret, -X:NoOptimize, -O, -OO ..."
	hello-helper $pyexe "-X:Interpret" "-X:NoOptimize" -O -OO
	
	echo "Testing compatible IronPython modes together ..."
	hello-helper $pyexe -O -v -u -E -OO -Qwarn -S -t -tt "-X:AutoIndent" "-X:AssembliesDir" $env:TMP "-X:ColorfulConsole" "-X:ExceptionDetail" "-X:Interpret" "-X:Frames" "-X:GenerateAsSnippets" "-X:ILDebug" "-X:MaxRecursion" 5 "-X:NoOptimize" "-X:NoTraceback" "-X:PassExceptions" "-X:SaveAssemblies" "-X:ShowClrExceptions" "-X:StaticMethods" "-X:TabCompletion"
}
	
###############################################################################
function test-nonsensemodes($pyexe) 
{	
	echo "Testing nonsense (Iron)Python modes using $exe"
	echo ""

	#------------------------------------------------------------------------------
	## 
	echo "TBD"

}
	
###############################################################################
function test-negmodes($pyexe) 
{	
	echo "Testing negative (Iron)Python modes using $exe"
	echo ""

	#---------------------------------------------------------------------------
	## 
	echo "TBD"
	
}
	
###############################################################################


#sanity checks
if (!(test-path $env:ROWAN_BIN\ipy.exe)) 
{
	write-error "ROWAN_BIN environment variable is not set or ipy.exe not built!"
	exit 1
}

echo "---------------------------------------------------------------------"
test-setup


$tests = @("test-pymodes", "test-dlrmodes", "test-relatedpy", "test-nonsensemodes", "test-negmodes")
foreach ($exe_test in $tests)
{
	echo "---------------------------------------------------------------------"
	&$exe_test $env:ROWAN_BIN\ipy.exe
	echo ""
}


#CPython 2.5 is a special case
echo "---------------------------------------------------------------------"
test-pymodes $env:MERLIN_ROOT\..\External\Languages\CPython\25\python.exe


if ($global:ERRORS -gt 0)
{
	write-error "$global:ERRORS test(s) failed!"
	exit 1
}

echo "All tests passed."
