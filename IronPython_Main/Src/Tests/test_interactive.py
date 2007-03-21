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
import IronPython

extraArgs = ""
if "-X:GenerateAsSnippets" in Environment.GetCommandLineArgs():
    extraArgs += " -X:GenerateAsSnippets"

def test_strings():
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
# Test "ipy.exe -i script.py"

def test_interactive_mode():
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

def test_sys_exitfunc():
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

def test_indentation():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecutePartialLine("if False:")
    ipi.ExecutePartialLine("    print 'hello'")
    response = ipi.ExecuteLine("  print 'goodbye'")
    AreEqual(response.find('IndentationError') > 1, True)
    ipi.End()

#############################################################################
# verify we dump exception details 

def test_dump_exception():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -X:ExceptionDetail")
    AreEqual(ipi.Start(), True)
    response = ipi.ExecuteLine("raise 'goodbye'")
    AreEqual(response.count('IronPython.Runtime') >= 1 and response.count("IronPython.Hosting") >= 1, True)
    ipi.End()

#############################################################################
# make sure we can enter try/except blocks

def test_try_except():
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

def test_incomplate_syntax():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecutePartialLine("class K:")
    response = ipi.ExecuteLine("")
    Assert("IndentationError:" in response)
    ipi.End()

###########################################################
# if , while, try, for and then EOF.
def test_missing_test():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    response = ipi.ExecuteLine("if")
    Assert("SyntaxError:" in response)
    ipi.End()

    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    response = ipi.ExecuteLine("while")
    Assert("SyntaxError:" in response)
    ipi.End()

    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    response = ipi.ExecuteLine("for")
    Assert("SyntaxError:" in response)
    ipi.End()

    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    response = ipi.ExecuteLine("try")
    Assert("SyntaxError:" in response)
    ipi.End()

##########################################################
# Support multiple-levels of indentation
def test_indentation_levels():
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
def test_partial_lists():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecutePartialLine("[1")
    ipi.ExecutePartialLine("  ,")
    ipi.ExecutePartialLine("    2")
    response = ipi.ExecuteLine("]")
    Assert("[1, 2]" in response)
    ipi.End()
    
##########################################################
# Support partial dicts
def test_partial_dicts():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecutePartialLine("{2:2")
    ipi.ExecutePartialLine("  ,")
    ipi.ExecutePartialLine("    2:2")
    response = ipi.ExecuteLine("}")
    Assert("{2: 2}" in response)
    
    ipi.ExecutePartialLine("{")
    response = ipi.ExecuteLine("}")
    Assert("{}" in response)
    
    ipi.ExecutePartialLine("a = {")
    ipi.ExecutePartialLine("    None:2")
    ipi.ExecutePartialLine(" , ")
    ipi.ExecuteLine("}")
    response = ipi.ExecuteLine("a")
    Assert("{None: 2}" in response)
    
    ipi.ExecutePartialLine("{")
    ipi.ExecutePartialLine("'joe'")
    ipi.ExecutePartialLine(": ")
    ipi.ExecutePartialLine("       42")
    ipi.ExecutePartialLine(",")
    ipi.ExecutePartialLine("3:45")
    response = ipi.ExecuteLine("}")
    Assert("{'joe': 42, 3: 45}" in response)
    
    ipi.End()

###########################################################
# Some whitespace wackiness
def test_whitespace():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecuteLine("  ")
    response = ipi.ExecuteLine("")
    ipi.End()
    
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecuteLine("  ")
    response = ipi.ExecuteLine("2")
    Assert("2" in response)
    ipi.End()
    
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecuteLine("  ")
    response = ipi.ExecuteLine("  2")
    Assert("SyntaxError:" in response)
    ipi.End()


###########################################################
# test the indentation error in the interactive mode
def test_indentation_interactive():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecutePartialLine("class C:pass")
    response = ipi.ExecuteLine("")
    AreEqual(response, "")
    ipi.ExecutePartialLine("class D(C):")
    response = ipi.ExecuteLine("")
    Assert("IndentationError:" in response)
    ipi.End()

###########################################################
# test -X:MTA w/ no other args

def test_mta():
    ipi = IronPythonInstance(executable, exec_prefix, '-X:MTA')
    AreEqual(ipi.Start(), True)
    ipi.ExecutePartialLine("class C:pass")
    response = ipi.ExecuteLine("")
    AreEqual(response, "")
    ipi.ExecutePartialLine("class D(C):")
    response = ipi.ExecuteLine("")
    Assert("IndentationError:" in response)
    ipi.End()


###########################################################
# test for comments  in interactive input

def test_comments():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    
    response = ipi.ExecuteLine("# this is some comment line")
    AreEqual(response, "")
    response = ipi.ExecuteLine("    # this is some comment line")
    AreEqual(response, "")
    response = ipi.ExecuteLine("# this is some more comment line")
    AreEqual(response, "")
    ipi.ExecutePartialLine("if 100:")
    ipi.ExecutePartialLine("    print 100")
    ipi.ExecutePartialLine("# this is some more comment line inside if")
    ipi.ExecutePartialLine("#     this is some indented comment line inside if")
    ipi.ExecutePartialLine("    print 200")
    response = ipi.ExecuteLine("")
    AreEqual(response, "100\r\n200")
    ipi.End()
    
def test_global_values():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecuteLine("import clr")
    response = ipi.ExecuteLine("[x for x in globals().Values]")
    Assert(response.startswith('['))
    d = eval(ipi.ExecuteLine("globals().fromkeys(['a', 'b'], 'c')"))
    AreEqual(d, {'a':'c', 'b':'c'})

def test_console_input_output():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    input_output = [
    ("x=100",""),
    ("x=200\n",""),
    ("\nx=300",""),
    ("\nx=400\n",""),
    ("500","500"),
    ("600\n\n\n\n\n\n\n\n\n\n\n","600"),
    ("valid=3;more_valid=4;valid","3"),
    ("valid=5;more_valid=6;more_valid\n\n\n\n\n","6"),
    ("valid=7;more_valid=8;#valid",""),
    ("valid=9;valid;# more_valid\n","9"),
    ("valid=11;more_valid=12;more_valid# should be valid input\n\n\n\n","12"),
    ]
    
    
    for x in input_output:
        AreEqual(ipi.Start(), True)
        AreEqual(ipi.ExecuteLine(x[0]),x[1])
        ipi.End()
    
def test_engine_input():    
    pe = IronPython.Hosting.PythonEngine()
    
    input_output = [
    "\n\n\n\n\n\ninvalid=3;more_invalid=4;more_invalid",
    "\n\n\n\n\n\ninvalid=3;more_invalid=4;more_invalid\n\n\n\n",
    "\n\n\n\n\n\n\n\nvalid=3;more_valid=4;more_valid#print should be valid input\n\n\n\n",
    "\n\n\n\n\n\n700",
    "\n\n\n\n\n\n800\n\n\n\n\n\n",
    "x=1\ninvalid",
    "x=1\nmore_invalid",
    "\ncomplete_invalid",
    "valid=3\n;#more_valid=4;more_valid\n\n\n\n\n",
    "valid=3\n;#more_valid=4;more_valid\n\n\n\n\n"
    ]
    
    for x in input_output:
        try:
            pe.ExecuteToConsole(x)
            Assert(False,"Invalid Input "+x+"accepted")
        except SyntaxError, e:
            pass
        except :
            Assert(False,"Exception raised on input"+x)

# expect a clean exception message/stack from thread
def test_thrown_from_thread():
    inputScript = path_combine(testpath.temporary_dir, "throwingfromthread.py")
    write_to_file(inputScript, '''
def f(): raise AssertionError, 'hello'
import thread, time
thread.start_new_thread(f, tuple())
time.sleep(2)
''')   
    
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " " + inputScript)
    (result, output, exitCode) = ipi.StartAndRunToCompletion()
    AreEqual(exitCode, 0)    
    Assert("AssertionError: hello" in output)
    Assert("IronPython." not in output)     # '.' is necessary here
    ipi.End()

run_test(__name__)