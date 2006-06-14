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

from lib.console_util import IronPythonInstance
from lib.assert_util import *
from sys import executable
from System import Environment
from sys import exec_prefix

extraArgs = ""
if "-X:GenerateAsSnippets" in Environment.GetCommandLineArgs():
    extraArgs += " -X:GenerateAsSnippets"

ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
AreEqual(ipi.Start(), True)

# String exception
response = ipi.ExecuteLine("raise 'foo'")
Assert(response.find("Traceback (most recent call last):") > -1)
Assert(response.find("foo") > -1)

# Multi-line string literal
ipi.ExecutePartialLine("\"\"\"Hello")
ipi.ExecutePartialLine("")
ipi.ExecutePartialLine("")
AreEqual("'Hello\\n\\n\\nWorld'", ipi.ExecuteLine("World\"\"\""))

ipi.ExecutePartialLine("if False: print 3")
ipi.ExecutePartialLine("else: print 'hello'")
AreEqual(r'hello', ipi.ExecuteLine(""))

# Empty line
AreEqual("", ipi.ExecuteLine(""))

ipi.End()


###############################################################################
# Test "ironpythonconsole.exe -i script.py"

inputScript = testpath.test_inputs_dir + "\\simpleCommand.py"
ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -i \"" + inputScript + "\"")
AreEqual(ipi.Start(), True)
ipi.EnsureInteractive()
AreEqual("1", ipi.ExecuteLine("x"))
ipi.End()

inputScript = testpath.test_inputs_dir + "\\raise.py"
ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -i \"" + inputScript + "\"")
AreEqual(ipi.Start(), True)
ipi.EnsureInteractive()
AreEqual("1", ipi.ExecuteLine("x"))
ipi.End()

inputScript = testpath.test_inputs_dir + "\\syntaxError.py"
ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -i \"" + inputScript + "\"")
AreEqual(ipi.Start(), True)
ipi.EnsureInteractive()
Assert(ipi.ExecuteLine("x").find("NameError") != -1)
ipi.End()

inputScript = testpath.test_inputs_dir + "\\exit.py"
ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -i \"" + inputScript + "\"")
(result, output, exitCode) = ipi.StartAndRunToCompletion()
AreEqual(exitCode, 0)
ipi.End()

###############################################################################
# Test sys.exitfunc

inputScript = testpath.test_inputs_dir + "\\exitFuncRuns.py"
ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " \"" + inputScript + "\"")
(result, output, exitCode) = ipi.StartAndRunToCompletion()
AreEqual(exitCode, 0)
AreEqual(output.find('hello world') > -1, True)
ipi.End()

inputScript = testpath.test_inputs_dir + "\\exitFuncRaises.py"
ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " \"" + inputScript + "\"")
(result, output, exitCode) = ipi.StartAndRunToCompletion()
AreEqual(exitCode, 0)
AreEqual(output.find('Error in sys.exitfunc:') > -1, True)
AreEqual("-X:GenerateAsSnippets" in Environment.GetCommandLineArgs() or 
         output.find('exitFuncRaises.py, line 19, in foo') > -1, True)
ipi.End()

#############################################################################
# verify we need to dedent to a previous valid indentation level

ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
AreEqual(ipi.Start(), True)
ipi.ExecutePartialLine("if False:")
ipi.ExecutePartialLine("    print 'hello'")
response = ipi.ExecuteLine("  print 'goodbye'")
AreEqual(response.find('IndentationError') > 1, True)
ipi.End()

#############################################################################
# verify we dump exception details 

ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -X:ExceptionDetail")
AreEqual(ipi.Start(), True)
response = ipi.ExecuteLine("raise 'goodbye'")
AreEqual(response.count('IronPython.Runtime') >= 1 and response.count("IronPython.Hosting") >= 1, True)
ipi.End()

#############################################################################
# make sure we can enter try/except blocks

ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
AreEqual(ipi.Start(), True)
ipi.ExecutePartialLine("try:")
ipi.ExecutePartialLine("    raise 'foo'")
ipi.ExecutePartialLine("except 'foo':")
ipi.ExecutePartialLine("    print 'okay'")
response = ipi.ExecuteLine("")
Assert(response.find('okay') > -1)
ipi.End()

###########################################################
# Throw on "complete" incomplete syntax bug #864

ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
AreEqual(ipi.Start(), True)
ipi.ExecutePartialLine("class K:")
response = ipi.ExecuteLine("")
Assert("IndentationError:" in response)
ipi.End()

##########################################################
# Support multiple-levels of indentation
ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
AreEqual(ipi.Start(), True)
ipi.ExecutePartialLine("class K:")
ipi.ExecutePartialLine("  def M(self):")
ipi.ExecutePartialLine("    if 1:")
ipi.ExecutePartialLine("      pass")
response = ipi.ExecuteLine("")
ipi.End()

##########################################################
# Support partial lists
ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
AreEqual(ipi.Start(), True)
ipi.ExecutePartialLine("[1")
ipi.ExecutePartialLine("  ,")
ipi.ExecutePartialLine("    2")
response = ipi.ExecuteLine("]")
Assert("[1, 2]" in response)
ipi.End()

###########################################################
# Some whitespace wackiness
ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
AreEqual(ipi.Start(), True)
ipi.ExecutePartialLine("  ")
response = ipi.ExecuteLine("")
ipi.End()

ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
AreEqual(ipi.Start(), True)
ipi.ExecutePartialLine("  ")
response = ipi.ExecuteLine("2")
Assert("2" in response)
ipi.End()

ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
AreEqual(ipi.Start(), True)
ipi.ExecutePartialLine("  ")
response = ipi.ExecuteLine("  2")
Assert("SyntaxError:" in response)
ipi.End()


###########################################################
# test the indentation error in the interactive mode
ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
AreEqual(ipi.Start(), True)
ipi.ExecutePartialLine("class C:pass")
response = ipi.ExecuteLine("")
AreEqual(response, "")
ipi.ExecutePartialLine("class D(C):")
response = ipi.ExecuteLine("")
Assert("IndentationError:" in response)
ipi.End()
