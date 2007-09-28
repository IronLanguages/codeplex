#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

from lib.assert_util import *
skiptest("silverlight")
from lib.console_util import IronPythonInstance
import sys
import nt
import re
from System import *

# Test that IronPython console behaves as expected (command line argument processing etc.).

# Get a temporary directory in which the tests can scribble.
tmpdir = Environment.GetEnvironmentVariable("TEMP")
tmpdir = IO.Path.Combine(tmpdir, "IronPython")

# Name of a temporary file used to capture console output.
tmpfile = IO.Path.Combine(tmpdir, "tmp_output.txt")

# Name of a batch file used to execute the console to workaround the fact we have no way to redirect stdout
# from nt.spawnl.
batfile = IO.Path.Combine(tmpdir, "__runconsole.bat")

f = file(batfile, "w")
f.write("@" + sys.executable + " >" + tmpfile + " 2>&1 %*\n")
f.close()

############################################################
# Runs the console with the given tuple of arguments and verifies that the output and exit code are as
# specified. The expected_output argument can be specified in various ways:
#   None        : No output comparison is performed
#   a string    : Full output is compared (remember to include newlines where appropriate)
#   a tuple     : A tuple of the form (optionstring, valuestring), valid optionstrings are:
#       "firstline" : valuestring is compared against the first line of the output
#       "lastline"  : valuestring is compared against the last line of the output
#       "regexp"    : valuestring is a regular expression compared against the entire output
def TestCommandLine(args, expected_output, expected_exitcode = 0):
    realargs = [batfile]
    realargs.extend(args)
    exitcode = nt.spawnv(0, batfile, realargs)
    cmdline = "ipy " + ' '.join(args)
    Assert(exitcode == expected_exitcode, "'" + cmdline + "' generated unexpected exit code " + str(exitcode))
    if (expected_output != None):
        f = file(tmpfile)
        if isinstance(expected_output, str):
            output = f.read()
        else:
            output = f.readlines()
        f.close()
        
        # normalize \r\n to \n
        if type(output) == list:
            for i in range(len(output)):
                output[i] = output[i].replace('\r\n', '\n')
        else:
            output = output.replace('\r\n', '\n')
        
        # then check the output
        if isinstance(expected_output, str):
            Assert(output == expected_output, "'" + cmdline + "' generated unexpected output:\n" + output)
        elif isinstance(expected_output, tuple):
            if expected_output[0] == "firstline":
                Assert(output[0] == expected_output[1], "'" + cmdline + "' generated unexpected first line of output:\n" + repr(output[0]))
            elif expected_output[0] == "lastline":
                Assert(output[-1] == expected_output[1], "'" + cmdline + "' generated unexpected last line of output:\n" + repr(output[-1]))
            elif expected_output[0] == "regexp":
                output = ''.join(output)
                Assert(re.match(expected_output[1], output, re.M | re.S), "'" + cmdline + "' generated unexpected output:\n" + repr(output))
            else:
                Assert(False, "Invalid type for expected_output")
        else:
            Assert(False, "Invalid type for expected_output")

############################################################
# Runs the console with the given argument string with the expectation that it should enter interactive mode.
# Meaning, for one, no -c parameter.  This is useful for catching certain argument parsing errors.
def TestInteractive(args, expected_exitcode = 0):
    ipi = IronPythonInstance(sys.executable, sys.exec_prefix, args)
    AreEqual(ipi.Start(), True)
    
    #Verify basic behavior
    AreEqual("4", ipi.ExecuteLine("2+2"))
    
    ipi.End()

############################################################
def TestScript(commandLineArgs, script, expected_output, expected_exitcode = 0):
    scriptFileName = "script_" + str(hash(script)) + ".py"
    tmpscript = IO.Path.Combine(tmpdir, scriptFileName)
    f = file(tmpscript, "w")
    f.write(script)
    f.close()
    args = commandLineArgs + (tmpscript,)
    TestCommandLine(args, expected_output, expected_exitcode)

############################################################
def test_exit():
    # Test exit code with sys.exit(int)
    TestCommandLine(("-c", "import sys; sys.exit(0)"),          "",         0)
    TestCommandLine(("-c", "import sys; sys.exit(200)"),        "",         200)
    TestScript((), "import sys\nclass C(int): pass\nc = C(200)\nsys.exit(c)\n", "", 200)

    # Test exit code with sys.exit(non-int)
    TestCommandLine(("-c", "import sys; sys.exit(None)"),       "",         0)
    TestCommandLine(("-c", "import sys; sys.exit('goodbye')"),  "goodbye\n",1)
    TestCommandLine(("-c", "import sys; sys.exit(200L)"),       "200\n",    1)

############################################################
# Test the -c (command as string) option.

# regexp for the output of PrintUsage
usageRegex = "Usage.*"

TestCommandLine(("-c", "print 'foo'"), "foo\n")
TestCommandLine(("-c", "raise 'foo'"), ("lastline", "foo\n"), 1)
TestCommandLine(("-c", "import sys; sys.exit(123)"), "", 123)
TestCommandLine(("-c", "import sys; print sys.argv", "foo", "bar", "baz"), "['-c', 'foo', 'bar', 'baz']\n")
TestCommandLine(("-c",), "Argument expected for the -c option.\n", -1)

############################################################
# Test the -S (suppress site initialization) option.

# Create a local site.py that sets some global context. Do this in a temporary directory to avoid accidently
# overwriting a real site.py or creating confusion. Use the IRONPYTHONPATH environment variable to point
# IronPython at this version of site.py.
f = file(tmpdir + "\\site.py", "w")
f.write("import sys\nsys.foo = 123\n")
f.close()
Environment.SetEnvironmentVariable("IRONPYTHONPATH", tmpdir)

# Verify that the file gets loaded by default.
TestCommandLine(("-c", "import sys; print sys.foo"), "123\n")

# Verify that Lib remains in sys.path.
TestCommandLine(("-S", "-c", "import sys; print str(sys.exec_prefix + '\\lib').lower() in [x.lower() for x in sys.path]"), "True\n")

# Now check that we can suppress this with -S.
TestCommandLine(("-S", "-c", "import sys; print sys.foo"), ("lastline", "AttributeError: 'module' object has no attribute 'foo'\n"), 1)

# Test the -V (print version and exit) option.
TestCommandLine(("-V",), ("regexp", "IronPython ([0-9.]+)(.*) on .NET ([0-9.]+)\n"))

############################################################
# Test the -OO (suppress doc string optimization) option.
def test_OO():
    foo_doc = "def foo():\n\t'OK'\nprint foo.__doc__\n"
    TestScript((),       foo_doc, "OK\n")
    TestScript(("-OO",), foo_doc, "None\n")

############################################################
# Test the -t and -tt (warnings/errors on inconsistent tab usage) options.

# Write a script containing inconsistent use fo tabs.
tmpscript = tmpdir + "\\tabs.py"
f = file(tmpscript, "w")
f.write("if (1):\n\tpass\n        pass\nprint 'OK'\n")
f.close()

TestCommandLine((tmpscript, ), "OK\n")
msg = "inconsistent use of tabs and spaces in indentation"
TestCommandLine(("-t", tmpscript), ("lastline", "SyntaxWarning: %s (%s, line %d)\n"  % (msg, tmpscript, 3)), 1)
TestCommandLine(("-tt", tmpscript), ("lastline", "TabError: " + msg + "\n"), 1)


# Test the -E (suppress use of environment variables) option.

# Re-use the generated site.py from above and verify that we can stop it being picked up from IRONPYTHONPATH
# using -E.
TestCommandLine(("-E", "-c", "import sys; print sys.foo"), ("lastline", "AttributeError: 'module' object has no attribute 'foo'\n"), 1)

# Create an override startup script that exits right away
tmpscript = tmpdir + "\\startupdie.py"
f = file(tmpscript, "w")
f.write("from System import Environment\nprint 'Boo!'\nEnvironment.Exit(27)\n")
f.close()
Environment.SetEnvironmentVariable("IRONPYTHONSTARTUP", tmpscript)
TestCommandLine((), None, 27)

tmpscript2 = tmpdir + "\\something.py"
f = file(tmpscript2, "w")
f.write("print 2+2\n")
f.close()
TestCommandLine(('-E', tmpscript2), "4\n")

tmpscript3 = tmpdir + "\\startupdie.py"
f = file(tmpscript3, "w")
f.write("import sys\nprint 'Boo!'\nsys.exit(42)\n")
f.close()
Environment.SetEnvironmentVariable("IRONPYTHONSTARTUP", tmpscript3)
TestCommandLine((), None, 42)

Environment.SetEnvironmentVariable("IRONPYTHONSTARTUP", "")
nt.unlink(tmpscript)
nt.unlink(tmpscript2)
nt.unlink(tmpscript3)

# Test -W (set warning filters) option.
def test_W():
    TestCommandLine(("-c", "import sys; print sys.warnoptions"), "[]\n")
    TestCommandLine(("-W", "foo", "-c", "import sys; print sys.warnoptions"), "['foo']\n")
    TestCommandLine(("-W", "foo", "-W", "bar", "-c", "import sys; print sys.warnoptions"), "['foo', 'bar']\n")
    TestCommandLine(("-W",), "Argument expected for the -W option.\n", -1)

# Test -?
# TestCommandLine(("-?",), ("regexp", usageRegex))

# Test -X:NoOptimize
TestCommandLine(("-X:NoOptimize", "-c", "from System import Console; Console.WriteLine('System')"), "System\n")

# Test -X:FastEval
TestCommandLine(("-X:Interpret", "-c", "2+2"), "4\n")
TestCommandLine(("-X:Interpret", "-c", "eval('2+2')"), "4\n")
TestCommandLine(("-X:Interpret", "-c", "x = 3; eval('x+2')"), "5\n")

# Test -X:TrackPerformance
TestCommandLine(("-X:TrackPerformance", "-c", "2+2"), "4\n")

# Test -X:Frames
TestCommandLine(("-X:Frames", "-c", "2+2"), "4\n")

# Test -u (Unbuffered stdout & stderr): only test this can be passed in 
TestCommandLine(('-u', '-c', 'print 2+2'), "4\n")

# Test -X:MaxRecursion
TestCommandLine(("-X:MaxRecursion", "2", "-c", "2+2"), "4\n")
TestCommandLine(("-X:MaxRecursion", "3.14159265", "-c", "2+2"), "The argument for the -X:MaxRecursion option must be an integer.\n", -1)
TestCommandLine(("-X:MaxRecursion",), "Argument expected for the -X:MaxRecursion option.\n", -1)

# Test -X:ILDebug
for fName in nt.listdir(tmpdir):
    if re.match('.*\.il$', fName):
        nt.unlink(tmpdir + '\\' + fName)

TestCommandLine(("-X:ILDebug", "-c", "def f(): pass"), None)
#Assert(len([fName for fName in nt.listdir(tmpdir) if re.match('gen_f.*\.il', fName)]) > 0)

# Test -x (ignore first line)
tmpxoptscript = tmpdir + '\\xopt.py'
f = file(tmpxoptscript, "w")
f.write("first line is garbage\nprint 	2+2\n")
f.close()
TestCommandLine(('-x', tmpxoptscript), "4\n")
nt.unlink(tmpxoptscript)

# Test invocation of a nonexistent file
nt.unlink("nonexistent.py")
TestCommandLine(("nonexistent.py",), "File nonexistent.py does not exist.\n", 1)

# Test -Q
def test_Q():
    TestCommandLine(("-Qnew", "-c", "3/2"), "1.5\n")
    TestCommandLine(("-Qold", "-c", "3/2"), "1\n")
    TestCommandLine(("-Qwarn", "-c", "3/2"), "1\n")
    TestCommandLine(("-Qwarnall", "-c", "3/2"), "1\n")
    TestCommandLine(("-Q", "new", "-c", "3/2"), "1.5\n")
    TestCommandLine(("-Q", "old", "-c", "3/2"), "1\n")
    TestCommandLine(("-Q", "warn", "-c", "3/2"), "1\n")
    TestCommandLine(("-Q", "warnall", "-c", "3/2"), "1\n")

@disabled("CodePlex Work Item 12965")
def test_doc():
    TestCommandLine(("", "-c", "print __doc__"), "None\n", 0)

run_test(__name__)

