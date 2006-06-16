#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
#
#  You must not remove this notice, or any other, from this software.
#
######################################################################################

from lib.assert_util import *
import sys
import nt
import re
from System import *

# Test that IronPythonConsole behaves as expected (command line argument processing etc.).

# Get a temporary directory in which the tests can scribble.
tmpdir = Environment.GetEnvironmentVariable("TEMP")

# Name of a temporary file used to capture console output.
tmpfile = tmpdir + "\\tmp_output.txt"

# Name of a batch file used to execute the console to workaround the fact we have no way to redirect stdout
# from nt.spawnl.
batfile = tmpdir + "\\__runconsole.bat"

f = file(batfile, "w")
f.write("@" + sys.executable + " >" + tmpfile + " 2>&1 %*\n")
f.close()

# Runs the console with the given tuple of arguments and verifies that the output and exit code are as
# specified. The expected_output argument can be specified in various ways:
#   None        : No output comparison is performed
#   a string    : Full output is compared (remember to include newlines where appropriate)
#   a tuple     : A tuple of the form (optionstring, valuestring), valid optionstrings are:
#       "firstline" : valuestring is compared against the first line of the output
#       "lastline"  : valuestring is compared against the last line of the output
#       "regexp"    : valuestring is a regular expression compared against the entire output
def TestCommandLine(args, expected_output, expected_exitcode = 0):
    exitcode = nt.spawnl(0, batfile, *args)
    cmdline = "IronPythonConsole " + ' '.join(args)
    Assert(exitcode == expected_exitcode, "'" + cmdline + "' generated unexpected exit code " + str(exitcode))
    if (expected_output != None):
        f = file(tmpfile)
        if isinstance(expected_output, str):
            output = f.read()
        else:
            output = f.readlines()
        f.close()
        if isinstance(expected_output, str):
            Assert(output == expected_output, "'" + cmdline + "' generated unexpected output:\n" + output)
        elif isinstance(expected_output, tuple):
            if expected_output[0] == "firstline":
                Assert(output[0] == expected_output[1], "'" + cmdline + "' generated unexpected first line of output:\n" + output[0])
            elif expected_output[0] == "lastline":
                Assert(output[-1] == expected_output[1], "'" + cmdline + "' generated unexpected last line of output:\n" + output[-1])
            elif expected_output[0] == "regexp":
                output = ''.join(output)
                Assert(re.match(expected_output[1], output, re.M | re.S), "'" + cmdline + "' generated unexpected output:\n" + output)
            else:
                Assert(False, "Invalid type for expected_output")
        else:
            Assert(False, "Invalid type for expected_output")

# regexp for the output of PrintUsage
usageRegex = "IronPython console:(.+)Usage.*"

# Test the -c (command as string) option.
TestCommandLine(("-c", "print 'foo'"), "foo\n")
TestCommandLine(("-c", "raise 'foo'"), ("lastline", "foo\n"), 1)
TestCommandLine(("-c", "import sys; sys.exit(123)"), "", 123)
TestCommandLine(("-c", "import sys; print sys.argv", "foo", "bar", "baz"), "['-c', 'foo', 'bar', 'baz']\n")
TestCommandLine(("-c",), ("regexp", usageRegex))

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

# Now check that we can suppress this with -S.
TestCommandLine(("-S", "-c", "import sys; print sys.foo"), ("lastline", "AttributeError: 'module' object has no attribute 'foo'\n"), 1)

# Test the -V (print version and exit) option.
TestCommandLine(("-V",), ("regexp", "IronPython ([0-9.]+)(.*) on .NET ([0-9.]+)\n"))

# Test the -OO (suppress doc string optimization) option.

# Write a script which defines a function with a doc string and then attempts to read it back (it's
# essentially impossible to do this from a -c command line since there's no way to terminate the function
# suite).
tmpscript = tmpdir + "\\doc.py"
f = file(tmpscript, "w")
f.write("def foo():\n\t'OK'\nprint foo.__doc__\n")
f.close()

TestCommandLine((tmpscript, ), "OK\n")
TestCommandLine(("-OO", tmpscript), "None\n")


# Test the -t and -tt (warnings/errors on inconsistent tab usage) options.

# Write a script containing inconsistent use fo tabs.
tmpscript = tmpdir + "\\tabs.py"
f = file(tmpscript, "w")
f.write("if (1):\n\tpass\n        pass\nprint 'OK'\n")
f.close()

TestCommandLine((tmpscript, ), "OK\n")
msg = "inconsistent use of tabs and spaces in indentation (%s, line %d)\n" % (tmpscript, 3)
TestCommandLine(("-t", tmpscript), msg + "OK\n")
TestCommandLine(("-tt", tmpscript), ("lastline", "TabError: " + msg), 1)


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
TestCommandLine(("-c", "import sys; print sys.warnoptions"), "[]\n")
TestCommandLine(("-W", "foo", "-c", "import sys; print sys.warnoptions"), "['foo']\n")
TestCommandLine(("-W", "foo", "-W", "bar", "-c", "import sys; print sys.warnoptions"), "['foo', 'bar']\n")
TestCommandLine(("-W",), ("regexp", usageRegex))

# Test -?
TestCommandLine(("-?",), ("regexp", usageRegex))

# Test -X:MTA
TestCommandLine(("-X:MTA", "-c", "print 'OK'"), "OK\n")

# Test -X:NoOptimize
TestCommandLine(("-X:NoOptimize", "-c", "from System import Console; Console.WriteLine('System')"), "System\n")

# Test -X:FastEval
TestCommandLine(("-X:FastEval", "-c", "2+2"), "4\n")
TestCommandLine(("-X:FastEval", "-c", "eval('2+2')"), "4\n")
TestCommandLine(("-X:FastEval", "-c", "x = 3; eval('x+2')"), "5\n")

# Test -X:TrackPerformance
TestCommandLine(("-X:TrackPerformance", "-c", "2+2"), "4\n")

# Test -X:Frames
TestCommandLine(("-X:Frames", "-c", "2+2"), "4\n")

# Test -X:MaxRecursion
TestCommandLine(("-X:MaxRecursion", "2", "-c", "2+2"), "4\n")
TestCommandLine(("-X:MaxRecursion", "3.14159265", "-c", "2+2"), ("regexp", usageRegex))
TestCommandLine(("-X:MaxRecursion",), ("regexp", usageRegex))

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
TestCommandLine(("nonexistent.py",), "File nonexistent.py does not exist\n", 1)

# Test -Q
TestCommandLine(("-Qnew", "-c", "3/2"), "1.5\n")
TestCommandLine(("-Qold", "-c", "3/2"), "1\n")
TestCommandLine(("-Qwarn", "-c", "3/2"), "1\n")
TestCommandLine(("-Qwarnall", "-c", "3/2"), "1\n")
TestCommandLine(("-Q", "new", "-c", "3/2"), "1.5\n")
TestCommandLine(("-Q", "old", "-c", "3/2"), "1\n")
TestCommandLine(("-Q", "warn", "-c", "3/2"), "1\n")
TestCommandLine(("-Q", "warnall", "-c", "3/2"), "1\n")

