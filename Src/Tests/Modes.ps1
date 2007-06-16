###################################################################################
#Tests the various combinations/permutations of executable modes
#TODO:
# * need a much better mechanism for testing -u
# * basically everything

$global:ERRORS = 0
$global:HELLOPY = "$env:TMP\hello_test.py"
$global:RECURSION = "$env:TMP\recursion_test.py"
$global:EXCEPTION = "$env:TMP\except_test.py"
$global:IS_DEBUG = "$env:ROWAN_BIN".ToLower().EndsWith("bin\debug")

###################################################################################
function test-setup
{
	echo "Setting up test run."
	
	echo "print 'Hello World 1st Line'" > $global:HELLOPY
	echo "x = 2 * 3" >> $global:HELLOPY
	echo "print x" >> $global:HELLOPY

	echo "def fact(n):" > $global:RECURSION
    echo "    if n==1: return 1" >> $global:RECURSION
    echo "    else: return n * fact(n-1)" >> $global:RECURSION
    echo "" >> $global:RECURSION
    echo "import sys" >> $global:RECURSION
    echo "print fact(int(sys.argv[1]))" >> $global:RECURSION

	#TODO: $global:EXCEPTION

	echo ""
}
###################################################################################
function test-pymodes($exe) 
{
	set-alias pyexe $exe
	
	echo "Testing Python modes using $exe"
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
	echo ""

	#------------------------------------------------------------------------------
	## -h
	echo "Testing -h ..."
	
	#Just a simple sanity check will suffice here as it's safe to assume the output
	#of the ipy.exe -h will change over time.
	
	$stuff = pyexe -h 
	$stuff = [string]$stuff
	
	$expected_stuff = "Usage:", "Options:", "Environment variables:"

	foreach ($expected in $expected_stuff)
	{
		if (!$stuff.Contains($expected))
		{
			write-error "Failed."; $global:ERRORS++
		}
	}
	
	#------------------------------------------------------------------------------
	## -O
	echo "Testing WITHOUT use of -O ..."
	
	$stuff = pyexe $global:HELLOPY
	if ($stuff[0] -ne "Hello World 1st Line") {write-error "Failed."; $global:ERRORS++}
	elseif($stuff[1] -ne 6) {write-error "Failed."; $global:ERRORS++}
	
	#------------------------------------------------------------------------------
	## -v
	echo "Testing -v ..."

	echo "CodePlex Work Item 10819"
	$stuff = pyexe -v -c "print 'HelloWorld'"
	#Just ensure there's more output than "HelloWorld"
	if ($false) #$stuff.Length -le "HelloWorld".Length) 
	{
		$stuff = $stuff.Length
		write-error "Failed: $stuff"
		$global:ERRORS++
	}
	
	#------------------------------------------------------------------------------
	## -u
	echo "Testing -u ..."

	#Need a much better test here.  For now just ensure it does not throw
	$stuff = pyexe -u $global:HELLOPY
	if ($stuff[0] -ne "Hello World 1st Line") {write-error "Failed."; $global:ERRORS++}
	elseif($stuff[1] -ne 6) {write-error "Failed."; $global:ERRORS++}
	
	$stuff = pyexe -u -c "print 'HelloWorld'"
	if ($stuff -ne "HelloWorld") {write-error "Failed."; $global:ERRORS++}
	
	#------------------------------------------------------------------------------
	## -d
	#Comment this out until it gets added to IP2.0...
	#echo "-d needs coverage"
	
	#------------------------------------------------------------------------------
	## -OO
	echo "-OO could use more thorough coverage"

	#------------------------------------------------------------------------------
	## -Q arg
	echo "-Q arg needs verification"

	#------------------------------------------------------------------------------
	## -W arg
	echo "-W arg needs verification"
	}
	

###############################################################################
function test-ipymodes($exe) 
{
	set-alias pyexe $exe
	
	echo "Testing IronPython modes using $exe"
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
	echo "    -X:NoOptimize ()"
	echo "    -X:SlowOps"
	echo ""

	#------------------------------------------------------------------------------
	## -X:AssembliesDir
	echo "-X:AssembliesDir needs verification"
	
	#------------------------------------------------------------------------------
	## -X:ExceptionDetail
	echo "-X:ExceptionDetail needs more coverage"
	
	#------------------------------------------------------------------------------
	## -X:FastEval
	echo "-X:FastEval needs coverage"
	
	#------------------------------------------------------------------------------
	## -X:MaxRecursion
	echo "Testing -X:MaxRecursion ..."
	
	
	#--Trivial cases w/o flag
	$stuff = pyexe $global:RECURSION 1
	if ($stuff -ne 1) {write-error "Failed."; $global:ERRORS++}
	
	$stuff = pyexe $global:RECURSION 2
	if ($stuff -ne 2) {write-error "Failed."; $global:ERRORS++}
	
	$stuff = pyexe $global:RECURSION 3
	if ($stuff -ne 6) {write-error "Failed."; $global:ERRORS++}
	
	$stuff = pyexe $global:RECURSION 20
	if ($stuff -ne 2432902008176640000L) {write-error "Failed."; $global:ERRORS++}
	
	
	#--Trivial cases w/ flag
	
	#non-recursive script
	foreach ($i in 0..2)
	{
		$stuff = pyexe "-X:MaxRecursion" $i $global:HELLOPY
		if ($stuff[0] -ne "Hello World 1st Line") {write-error "Failed."; $global:ERRORS++}
		elseif($stuff[1] -ne 6) {write-error "Failed."; $global:ERRORS++}
	}
	
	#simple recursive script
	foreach($i in 3..5)
	{
		$stuff = pyexe "-X:MaxRecursion" $i $global:RECURSION 3
		if ($stuff -ne 6) {write-error "Failed."; $global:ERRORS++}
	}
	
	#simple recursive script where we miss the MaxRecursion depth by one
	#which should trigger a RuntimeError exception
	foreach ($i in 0..2)
	{
		$x_param = $i + 2
		$script_param = $i + 3
		$stuff = pyexe "-X:MaxRecursion" $x_param $global:RECURSION $script_param 2>&1
		$stuff = "$stuff.ErrorDetails"
		if (!$stuff.Contains("maximum recursion depth exceeded")) {write-error "Failed."; $global:ERRORS++}
	}
	
	echo "CodePlex Work Item 10816"
	foreach ($i in @()) #1..-2)
	{
		$stuff = pyexe "-X:MaxRecursion" $i $global:RECURSION 3 2>&1
		$stuff = "$stuff.ErrorDetails"
		if (!$stuff.Contains("maximum recursion depth exceeded")) {write-error "Failed."; $global:ERRORS++}
	}
	
	#------------------------------------------------------------------------------
	## -X:MTA
	echo "-X:MTA needs coverage" #non-winforms apps.  test with System.Thread*
	
	#------------------------------------------------------------------------------
	## -X:NoTraceback
	echo "-X:NoTraceback needs coverage"
	
	#------------------------------------------------------------------------------
	## -X:ShowClrExceptions
	echo "-X:ShowClrExceptions needs coverage"
	
	#------------------------------------------------------------------------------
	#-- -X:ShowASTs
	echo "-X:ShowASTs needs more coverage"
	
	if ($global:IS_DEBUG)
	{
		$stuff = pyexe "-X:ShowASTs" $global:HELLOPY
		#just check to make sure it does not throw for now
		if (! $?) {write-error "Failed."; $global:ERRORS++}
	}
	
	#------------------------------------------------------------------------------
	#-- -X:DumpASTs
	echo "-X:DumpASTs needs more coverage"

	if ($global:IS_DEBUG)
	{
		$stuff = pyexe "-X:DumpASTs" $global:HELLOPY
		#just check to make sure it does not throw for now
		if (! $?) {write-error "Failed."; $global:ERRORS++}
	}

	#------------------------------------------------------------------------------
	#-- -X:ShowRules
	echo "-X:ShowRules needs more coverage"
	
	if ($global:IS_DEBUG)
	{
		$stuff = pyexe "-X:ShowRules" $global:HELLOPY
		#just check to make sure it does not throw for now
		if (! $?) {write-error "Failed."; $global:ERRORS++}
	}

}

###############################################################################
function test-relatedmodes($exe) 
{
	set-alias pyexe $exe
	
	echo "Testing related (Iron)Python modes using $exe"
	echo ""

	#------------------------------------------------------------------------------
	## 
	echo "TBD"
	
}
	
###############################################################################
function test-nonsensemodes($exe) 
{
	set-alias pyexe $exe
	
	echo "Testing nonsense (Iron)Python modes using $exe"
	echo ""

	#------------------------------------------------------------------------------
	## 
	echo "TBD"

}
	
###############################################################################
function test-negmodes($exe) 
{
	set-alias pyexe $exe
	
	echo "Testing negative (Iron)Python modes using $exe"
	echo ""

	#---------------------------------------------------------------------------
	## 
	echo "TBD"
	
}
	
###############################################################################

test-setup

$tests = @("test-pymodes", "test-ipymodes", "test-relatedmodes", "test-nonsensemodes", "test-negmodes")
foreach ($exe_test in $tests)
{
	echo "---------------------------------------------------------------------"
	&$exe_test $env:ROWAN_BIN\ipy.exe
	echo ""
}


if ($global:ERRORS -gt 0)
{
	write-error "$global:ERRORS test(s) failed!"
	exit $global:ERRORS
}

echo "All tests passed."
