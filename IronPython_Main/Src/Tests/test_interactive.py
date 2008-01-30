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


from lib.assert_util import *
skiptest("silverlight")
from lib.console_util import IronPythonInstance

remove_ironpython_dlls(testpath.public_testdir)

from sys import executable
from System import Environment
from sys import exec_prefix

extraArgs = ""
if "-X:GenerateAsSnippets" in Environment.GetCommandLineArgs():
    extraArgs += " -X:GenerateAsSnippets"

def test_strings():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    
    # String exception
    response = ipi.ExecuteLine("raise 'foo'", True)
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
    Assert(ipi.ExecuteLine("x", True).find("NameError") != -1)
    ipi.End()
    
    inputScript = testpath.test_inputs_dir + "\\exit.py"
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -i \"" + inputScript + "\"")
    (result, output, output2, exitCode) = ipi.StartAndRunToCompletion()
    AreEqual(exitCode, 0)
    ipi.End()
    
    # interactive + -c
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -i -c x=2")
    AreEqual(ipi.Start(), True)
    ipi.EnsureInteractive()
    Assert(ipi.ExecuteLine("x", True).find("2") != -1)
    ipi.End()

    
###############################################################################
# Test sys.exitfunc

def test_sys_exitfunc():
    inputScript = testpath.test_inputs_dir + "\\exitFuncRuns.py"
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " \"" + inputScript + "\"")
    (result, output, output2, exitCode) = ipi.StartAndRunToCompletion()
    AreEqual(exitCode, 0)
    AreEqual(output.find('hello world') > -1, True)
    ipi.End()
    
    inputScript = testpath.test_inputs_dir + "\\exitFuncRaises.py"
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " \"" + inputScript + "\"")
    (result, output, output2, exitCode) = ipi.StartAndRunToCompletion()
    AreEqual(exitCode, 0)
    AreEqual(output2.find('Error in sys.exitfunc:') > -1, True)
    AreEqual("-X:GenerateAsSnippets" in Environment.GetCommandLineArgs() or 
             output2.find('exitFuncRaises.py, line 19, in foo') > -1, True)
    ipi.End()

#############################################################################
# verify we need to dedent to a previous valid indentation level

def test_indentation():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecutePartialLine("if False:")
    ipi.ExecutePartialLine("    print 'hello'")
    response = ipi.ExecuteLine("  print 'goodbye'", True)
    AreEqual(response.find('IndentationError') > 1, True)
    ipi.End()

#############################################################################
# verify we dump exception details 

def test_dump_exception():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -X:ExceptionDetail")
    AreEqual(ipi.Start(), True)
    response = ipi.ExecuteLine("raise 'goodbye'", True)
    AreEqual(response.count("Microsoft.Scripting") >= 1, True)
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
    response = ipi.ExecuteLine("", True)
    Assert("IndentationError:" in response)
    ipi.End()

def test_incomplate_syntax_backslash():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
       
    for i in xrange(4):
        for j in xrange(i): 
            ipi.ExecutePartialLine("\\")
        ipi.ExecutePartialLine("1 + \\")
        for j in xrange(i): 
            ipi.ExecutePartialLine("\\")
        response = ipi.ExecuteLine("2", True)
        Assert("3" in response)
    
    ipi.End()

###########################################################
# if , while, try, for and then EOF.
def test_missing_test():
    for x in ['if', 'while', 'for', 'try']:
        ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
        AreEqual(ipi.Start(), True)
        response = ipi.ExecuteLine(x, True)
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

@disabled("CodePlex Work Item 5904")    
def test_partial_lists_broken():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecutePartialLine("[")
    ipi.ExecutePartialLine("")
    ipi.ExecutePartialLine("")
    response = ipi.ExecuteLine("]")
    Assert("[]" in response)
    ipi.End()    
    
def test_partial_lists_cp3530():

    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    
    try:
        ipi.ExecutePartialLine("[{'a':None},")
        response = ipi.ExecuteLine("]")
        Assert("[{'a': None}]" in response, response)
    
        ipi.ExecutePartialLine("[{'a'")
        response = ipi.ExecutePartialLine(":None},")
        response = ipi.ExecuteLine("]")
        Assert("[{'a': None}]" in response, response)
    
        ipi.ExecutePartialLine("[{'a':None},")
        ipi.ExecutePartialLine("1,")
        response = ipi.ExecuteLine("2]")
        Assert("[{'a': None}, 1, 2]" in response, response)
    
    finally:
        ipi.End()
    
    
##########################################################
# Support partial tuples
def test_partial_tuples():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecutePartialLine("(2")
    ipi.ExecutePartialLine("  ,")
    ipi.ExecutePartialLine("    3")
    response = ipi.ExecuteLine(")")
    Assert("(2, 3)" in response)
    
    ipi.ExecutePartialLine("(")
    response = ipi.ExecuteLine(")")
    Assert("()" in response)
    
    ipi.ExecutePartialLine("'abc %s %s %s %s %s' % (")
    ipi.ExecutePartialLine("    'def'")
    ipi.ExecutePartialLine("    ,'qrt',")
    ipi.ExecutePartialLine("    'jkl'")
    ipi.ExecutePartialLine(",'jkl'")
    ipi.ExecutePartialLine("")
    ipi.ExecutePartialLine(",")
    ipi.ExecutePartialLine("")
    ipi.ExecutePartialLine("")
    ipi.ExecutePartialLine("'123'")
    response = ipi.ExecuteLine(")")
    Assert("'abc def qrt jkl jkl 123'" in response)
    
    ipi.ExecutePartialLine("a = (")
    ipi.ExecutePartialLine("    1")
    ipi.ExecutePartialLine(" , ")
    ipi.ExecuteLine(")")
    response = ipi.ExecuteLine("a")
    Assert("(1,)" in response)
    
    ipi.ExecutePartialLine("(")
    ipi.ExecutePartialLine("'joe'")
    ipi.ExecutePartialLine(" ")
    ipi.ExecutePartialLine("       #")
    ipi.ExecutePartialLine(",")
    ipi.ExecutePartialLine("2")
    response = ipi.ExecuteLine(")")
    Assert("('joe', 2)" in response)
    
    ipi.ExecutePartialLine("(")
    ipi.ExecutePartialLine("")
    ipi.ExecutePartialLine("")
    response = ipi.ExecuteLine(")")
    Assert("()" in response)
    
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
    
    ipi.ExecutePartialLine("{")
    ipi.ExecutePartialLine("")
    ipi.ExecutePartialLine("")
    response = ipi.ExecuteLine("}")
    Assert("{}" in response)

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
    response = ipi.ExecuteLine("  2", True)
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
    response = ipi.ExecuteLine("", True)
    Assert("IndentationError:" in response)
    ipi.End()

###########################################################
# test /mta w/ no other args

def test_mta():
    ipi = IronPythonInstance(executable, exec_prefix, '/mta')
    AreEqual(ipi.Start(), True)
    ipi.ExecutePartialLine("class C:pass")
    response = ipi.ExecuteLine("")
    AreEqual(response, "")
    ipi.ExecutePartialLine("class D(C):")
    response = ipi.ExecuteLine("", True)
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
    AreEqual(response, "100" + newline + "200")
    ipi.End()
    
def test_global_values():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecuteLine("import clr")
    response = ipi.ExecuteLine("[x for x in globals().values()]")
    Assert(response.startswith('['))
    #CodePlex Work Item #6704
    #d = eval(ipi.ExecuteLine("globals().fromkeys(['a', 'b'], 'c')"))
    #AreEqual(d, {'a':'c', 'b':'c'})
    
def test_globals8961():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    
    response = ipi.ExecuteLine("print globals().keys()")
    AreEqual(response, "['__builtins__', '__name__', '__doc__']")
    
    ipi.ExecuteLine("a = None")
    response = ipi.ExecuteLine("print globals().keys()")
    AreEqual(response, "['__builtins__', '__name__', '__doc__', 'a']")
    response = ipi.ExecuteLine("print globals().values()")
    AreEqual(response, "[<module '__builtin__' (built-in), '__main__', None, None]")
    
    ipi.ExecuteLine("b = None")
    response = ipi.ExecuteLine("print globals().values()")
    AreEqual(response, "[<module '__builtin__' (built-in), '__main__', None, None, None]")
    

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
    (result, output, output2, exitCode) = ipi.StartAndRunToCompletion()
    AreEqual(exitCode, 0)    
    Assert("AssertionError: hello" in output)
    Assert("IronPython." not in output)     # '.' is necessary here
    ipi.End()

def test_aform_feeds():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    response = ipi.ExecuteLine("\fprint 'hello'")
    AreEqual(response, "hello")
    response = ipi.ExecuteLine("      \fprint 'hello'")
    AreEqual(response, "hello")
    
    ipi.ExecutePartialLine("def f():")
    ipi.ExecutePartialLine("\f    print 'hello'")
    ipi.ExecuteLine('')
    response = ipi.ExecuteLine('f()')
    AreEqual(response, "hello")
    
    # \f resets indent to 0
    ipi.ExecutePartialLine("def f():")
    ipi.ExecutePartialLine("    \f    x = 'hello'")
    ipi.ExecutePartialLine("\f    print x")
    
    ipi.ExecuteLine('')
    response = ipi.ExecuteLine('f()')
    AreEqual(response, "hello")

    # \f resets indent to 0
    ipi.ExecutePartialLine("def f():")
    ipi.ExecutePartialLine("    \f    x = 'hello'")
    ipi.ExecutePartialLine("    print x")
    
    ipi.ExecuteLine('')
    response = ipi.ExecuteLine('f()')
    AreEqual(response, "hello")

def test_ipy_dash_S():
    """ipy -S should still install Lib into sys.path"""
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -S")
    AreEqual(ipi.Start(), True)
    response = ipi.ExecuteLine("import sys")
    response = ipi.ExecuteLine("print sys.path")
    Assert(response.find('Lib') != -1)

def test_startup_dir():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    response = ipi.ExecuteLine("print dir()")
    AreEqual(sorted(eval(response)), sorted(['__builtins__', '__doc__', '__name__']))

def test_ipy_dash_m():
    import sys
    for path in sys.path:
        if path.find('Lib') != -1:
            filename = System.IO.Path.Combine(path, 'somemodule.py')
            break

    try:
        f = file(filename, 'w')
        f.write('print "hello"\n')
        f.write('import sys\n')
        f.write('print sys.argv')
        f.close()
        
        # simple case works
        ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -m somemodule")
        res, output, err, exit = ipi.StartAndRunToCompletion()
        AreEqual(res, True) # run should have worked
        AreEqual(exit, 0)   # should have returned 0
        output = output.replace('\r\n', '\n')
        lines = output.split('\n')
        AreEqual(lines[0], 'hello') 
        AreEqual(eval(lines[1]), [filename])
        
        # we receive any arguments in sys.argv
        ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -m somemodule foo bar")
        res, output, err, exit = ipi.StartAndRunToCompletion()
        AreEqual(res, True) # run should have worked
        AreEqual(exit, 0)   # should have returned 0
        output = output.replace('\r\n', '\n')
        lines = output.split('\n')
        AreEqual(lines[0], 'hello') 
        AreEqual(eval(lines[1]), [filename, 'foo', 'bar'])

        f = file(filename, 'w')
        f.write('print "hello"\n')
        f.write('import sys\n')
        f.write('sys.exit(1)')
        f.close()
        
        # sys.exit works
        ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -m somemodule")
        res, output, err, exit = ipi.StartAndRunToCompletion()
        AreEqual(res, True) # run should have worked
        AreEqual(exit, 1)   # should have returned 0
        output = output.replace('\r\n', '\n')
        lines = output.split('\n')
        AreEqual(lines[0], 'hello')
        
        # Python packages work
        ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -m lib")
        res, output, err, exit = ipi.StartAndRunToCompletion()
        AreEqual(res, True) # run should have worked
        AreEqual(exit, 0)   # should have returned 0
        AreEqual(output, "")
        
        # Bad module names should not work
        ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -m libxyz")
        res, output, err, exit = ipi.StartAndRunToCompletion()
        AreEqual(res, True) # run should have worked
        AreEqual(exit, 1)   # should have returned 0
        Assert("ImportError: No module named libxyz" in err, 
               "stderr is:" + str(err))
              
    finally:
        nt.unlink(filename)
        
@disabled("CodePlex Work Item 10925")        
def test_ipy_dash_m_negative():
    # builtin modules should not work
    for modname in [ "sys", "datetime" ]:
        ipi = IronPythonInstance(executable, exec_prefix, 
                                 extraArgs + " -m " + modname)
        res, output, err, exit = ipi.StartAndRunToCompletion()
        AreEqual(exit, 1)
        AreEqual(output, "")
        Assert("ImportError" in err)

    # Modules within packages should not work
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs + " -m lib.assert_util")
    res, output, err, exit = ipi.StartAndRunToCompletion()
    AreEqual(res, True) # run should have worked
    AreEqual(exit, 1)   # should have returned 0
    Assert("SyntaxError: invalid syntax" in err, 
           "stderr is:" + str(err))        
    
def test_ipy_dash_c():
    """verify ipy -c cmd doesn't print expression statements"""
    ipi = IronPythonInstance(executable, exec_prefix, "-c True;False")
    res = ipi.StartAndRunToCompletion()
    AreEqual(res[0], True)  # should have started
    AreEqual(res[1], '')    # no std out
    AreEqual(res[2], '')    # no std err
    AreEqual(res[3], 0)     # should return 0

#############################################################################
# CP11924 - verify 'from __future__ import division' works
def test_future_division():
    ipi = IronPythonInstance(executable, exec_prefix, extraArgs)
    AreEqual(ipi.Start(), True)
    ipi.ExecuteLine("from __future__ import division")
    response = ipi.ExecuteLine("11/4")
    AreEqual(response, "2.75")
    ipi.End()



#------------------------------------------------------------------------------
run_test(__name__)
