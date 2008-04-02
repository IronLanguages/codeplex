#####################################################################################
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public
# License. A  copy of the license can be found in the License.html file at the
# root of this distribution. If  you cannot locate the  Microsoft Public
# License, please send an email to  dlr@microsoft.com. By using this source
# code in any fashion, you are agreeing to be bound by the terms of the 
# Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#####################################################################################

##
## Testing IronPython Engine
##

from lib.assert_util import *
import sys
import IronPython
import System
#------------------------------------------------------------------------------
#--GLOBALS
pe = IronPython.Hosting.PythonEngine()


#Ensure we can manipulate globals() from within a PythonEngine
globals_test = '''
a = 2

lib.assert_util.AreEqual(globals()["a"], 2)
lib.assert_util.AreEqual(type(globals()), dict)

#--ToString
#Just verify type for now.  This string will generally contain unique
#file paths
lib.assert_util.AreEqual(type(globals().ToString()), str)

#--IsFixedSize
lib.assert_util.AreEqual(globals().IsFixedSize, False)

#--Add
globals()["b"] = None
lib.assert_util.AreEqual(globals()["b"], None)
    
globals().Add("c", "blah")
lib.assert_util.AreEqual(globals()["c"], "blah")
    
temp = System.Collections.Generic.KeyValuePair[object,object]("d", int(3))
globals().Add(temp)
lib.assert_util.AreEqual(globals()["d"], int(3))
    
#--Keys
temp = [temp for temp in globals().Keys]
temp.sort()
lib.assert_util.AreEqual(temp,
                         ['System', '__builtins__', '__name__', 'a', 'assert_util', 'b', 'c', 'd', 'gc', 'lib', 'sys', 'temp'])
    

#--Values
temp = [temp for temp in globals().Values ]
temp.sort()
lib.assert_util.AreEqual(temp.count(2), 1)
lib.assert_util.AreEqual(temp.count(None), 1)
lib.assert_util.AreEqual(temp.count('blah'), 1)
lib.assert_util.AreEqual(temp.count(3), 1)
lib.assert_util.AreEqual(temp.count('defaultModule'), 1)
lib.assert_util.AreEqual(len(temp), 12)


#--GetEnumerator
temp = [temp for temp in globals().GetEnumerator()]
temp.sort()
lib.assert_util.AreEqual(temp,
         ['System', '__builtins__', '__name__', 'a', 'assert_util', 'b', 'c', 'd', 'gc', 'lib', 'sys', 'temp'])
      
#--Contains
temp = System.Collections.Generic.KeyValuePair[object,object]("b", None)
lib.assert_util.Assert(globals().Contains(temp))

temp = System.Collections.Generic.KeyValuePair[object,object]("c", "blah")
lib.assert_util.Assert(globals().Contains(temp))

#CodePlex Work Item 6703
#temp = System.Collections.Generic.KeyValuePair[object,object]("d", int(3))
#lib.assert_util.Assert(globals().Contains(temp))

temp = System.Collections.Generic.KeyValuePair[object,object]("e", "blah")
lib.assert_util.Assert(globals().Contains(temp)==False)

#--ContainsKey
lib.assert_util.Assert(globals().ContainsKey("a"))
lib.assert_util.Assert(globals().ContainsKey("b"))
lib.assert_util.Assert(globals().ContainsKey("c"))
lib.assert_util.Assert(globals().ContainsKey("d"))
lib.assert_util.Assert(globals().ContainsKey("e")==False) 

#--TryGetValue
lib.assert_util.AreEqual(globals().TryGetValue("a"), (True, 2))
lib.assert_util.AreEqual(globals().TryGetValue("b"), (True, None))
lib.assert_util.AreEqual(globals().TryGetValue("c"), (True, "blah"))
lib.assert_util.AreEqual(globals().TryGetValue("d"), (True, int(3)))
lib.assert_util.AreEqual(globals().TryGetValue("e"), (False, None)) 

#--TryGetObjectValue
lib.assert_util.AreEqual(globals().TryGetObjectValue("a"), (True, 2))
lib.assert_util.AreEqual(globals().TryGetObjectValue("b"), (True, None))
lib.assert_util.AreEqual(globals().TryGetObjectValue("c"), (True, "blah"))
lib.assert_util.AreEqual(globals().TryGetObjectValue("d"), (True, int(3)))
lib.assert_util.AreEqual(globals().TryGetObjectValue("e"), (False, None)) 
lib.assert_util.AreEqual(globals().TryGetObjectValue(None), (False, None)) 
lib.assert_util.AreEqual(globals().TryGetObjectValue(1), (False, None)) 

#--ContainsObjectKey
lib.assert_util.AreEqual(globals().ContainsObjectKey("a"), True)
lib.assert_util.AreEqual(globals().ContainsObjectKey("b"), True)
lib.assert_util.AreEqual(globals().ContainsObjectKey("c"), True)
lib.assert_util.AreEqual(globals().ContainsObjectKey("d"), True)
lib.assert_util.AreEqual(globals().ContainsObjectKey("e"), False) 
lib.assert_util.AreEqual(globals().ContainsObjectKey(None), False) 
lib.assert_util.AreEqual(globals().ContainsObjectKey(1), False) 

#CodePlex Work Item 6704
#This occurs from normal interactive sessions as well
#--fromkeys
#lib.assert_util.AreEqual(globals().fromkeys([1, 2], 3), {1: 3, 2: 3})
#lib.assert_util.AreEqual(globals().fromkeys([1, 2]), {1: None, 2: None})
    
#--Count
lib.assert_util.AreEqual(globals().Count, 12)


#--IsReadOnly
lib.assert_util.Assert(not globals().IsReadOnly)
            
#--CopyTo
lib.assert_util.AssertError(NotImplementedError,
                            globals().CopyTo,
                            None, 3)
            
#--SymbolAttributes
globals().SymbolAttributes

#--UnderlyingDictionary
globals().UnderlyingDictionary

#--Remove
lib.assert_util.Assert(globals().ContainsKey("d"))
globals().Remove("d")
lib.assert_util.Assert(globals().ContainsKey("d")==False)

lib.assert_util.Assert(globals().ContainsKey("c"))
globals().Remove(System.Collections.Generic.KeyValuePair[object,object]("c", "blah"))
lib.assert_util.Assert(globals().ContainsKey("c")==False)
    
#--Clear!
globals().Clear()
if [x for x in globals().Keys]!=[] or globals().Count!=0:
    raise Exception("globals() not empty")
    
#--clear!
#just call it (this funtion is the same as Clear). 
#at this point lib.* is gone!
globals()["something"] = 1
globals().clear()
if [x for x in globals().Keys]!=[] or globals().Count!=0:
    raise Exception("globals() not empty")
'''

#------------------------------------------------------------------------------
#--TEST FUNCTIONS
def test_trivial():
    Assert(IronPython.Hosting.PythonEngine.Version != "")


def test_version():
    """ test that the assembly versions are the same as 1.1 release for compatibility."""
    import clr
    import System

    for asm in [
            clr.GetClrType(IronPython.Hosting.PythonEngine).Assembly,
            (1L).GetType().Assembly
        ]:
        av = asm.GetName().Version
        Assert(av != None)
        AreEqual(av.Major, 1)
        AreEqual(av.Minor, 1)
        AreEqual(av.Build, 0)
        AreEqual(av.Revision, 0)

def test_coverage():
    # 1. fasteval 
    save = IronPython.Compiler.Options.FastEvaluation
    IronPython.Compiler.Options.FastEvaluation = True
    AreEqual(eval("None"), None)
    AreEqual(eval("str(2)"), "2")
    IronPython.Compiler.Options.FastEvaluation = save
    
    # 2. ...

# verify no interference between PythonEngine used in IronPython console and user created one
def test_no_interference(): 
    # 1. path
    oldpath = sys.path
    pe = IronPython.Hosting.PythonEngine()
    AreEqual(sys.path, oldpath)
    
    # 2. how about other states...

# now verify CLR loaded modules don't interfere 
def test_no_module_interference():
    pe.Execute("""import clr
clr.AddReferenceByPartialName('System.Security')
import System.Security.Cryptography
if not hasattr(System.Security.Cryptography, 'CryptographicAttributeObject'):
    raise AssertionError("CryptographicAttributeObject not found")
""")

    # verify our engine doesn't see it
    import System.Security
    Assert(not hasattr(System.Security.Cryptography, 'CryptographicAttributeObject'))

# tests wrote in C# EngineTest.cs
def test_engine_from_csharp():
    load_iron_python_test()
    import IronPythonTest
    et = IronPythonTest.EngineTest()
    for s in dir(et):
        if s.startswith("Scenario"):
            if s=="ScenarioGC" and "-X:StaticMethods" in System.Environment.GetCommandLineArgs():
                continue
            getattr(et, s)()

def test_CreateMethod():
    """Test cases specific to PythonEngine.CreateMethod<DelegateType>"""

    pe = IronPython.Hosting.PythonEngine(CreateOptions())
    from IronPython.Hosting import EngineModule
    from clr import Reference
    
    load_iron_python_test()
    
    from IronPythonTest import IntArgDelegate, StringArgDelegate, RefReturnDelegate, OutReturnDelegate
    
    AreEqual(pe.CreateMethod[IntArgDelegate]('arg1 = "abc"\narg2="def"\nreturn arg1')(2, 3), 'abc')
    AreEqual(pe.CreateMethod[IntArgDelegate]('return (arg1+1,arg2+1)')(5,10), (6,11))
    AreEqual(pe.CreateMethod[StringArgDelegate]('arg1 = 2\narg2=3\nreturn arg1')('abc', 'def'), 2)
    y = Reference[object]()
    AreEqual(pe.CreateMethod[RefReturnDelegate]('sender = 2\nres.Value = 3\nreturn sender')('abc', y), 2)
    AreEqual(y.Value, 3)
    y = Reference[object]('abc')
    AreEqual(pe.CreateMethod[OutReturnDelegate]('res.Value = sender\nsender = "def"\nreturn sender')('abc', y), 'def')
    AreEqual(y.Value, 'abc')

def test_CreateLambdaAndMethod():
    """Common Test cases for PythonEngine.CreateLambda<DelegateType>(...) and PythonEngine.CreateMethod<DelegateType>"""
    
    pe = IronPython.Hosting.PythonEngine(CreateOptions())
    from IronPython.Hosting import EngineModule
    
    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegate, SimpleReturnDelegateArg1, SimpleReturnDelegateArg2
    
    for funcInfo in [(pe.CreateLambda,''), (pe.CreateMethod,'return ')]:         
        func = funcInfo[0]
        prepend = funcInfo[1]
        
        # simple parameterless eval works
        AreEqual(func[SimpleReturnDelegate](prepend+'123')(), 123)
        
        # eval w/ different module scopes works
        engineModule1 = pe.CreateModule('test', False)
        engineModule1.Globals['abc'] = 'xyz'
        
        engineModule2 = pe.CreateModule('test', False)
        engineModule2.Globals['abc'] = 'def'
    
        AreEqual(func[SimpleReturnDelegate](prepend+'abc', engineModule1)(), 'xyz')
        AreEqual(func[SimpleReturnDelegate](prepend+'abc', engineModule2)(), 'def')
            
        # scoped w/ 1 arg, none remapped    
        AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + arg1', engineModule1)('qrt'), 'xyzqrt')
        AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + arg1', engineModule2)('qrt'), 'defqrt')
        
        # scoped w/ 2 args, none remapped    
        AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + arg1 + arg2', engineModule1)('qrt','asd'), 'xyzqrtasd')
        AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + arg1 + arg2', engineModule2)('qrt','asd'), 'defqrtasd')
    
        # scoped w/ 1 arg, remapped
        AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + xyz', ('xyz',), engineModule1)('qrt'), 'xyzqrt')
        AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + xyz', ('xyz',),engineModule2)('qrt'), 'defqrt')
        
        # unscoped w/ 1 arg remapped
        AreEqual(func[SimpleReturnDelegateArg1](prepend+'xyz', ('xyz',))('qrt'), 'qrt')
        
        # scoped w/ 2 args, remapped
        AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + xyz + qrt', ('xyz','qrt'), engineModule1)('qrt','asd'), 'xyzqrtasd')
        AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + xyz + qrt', ('xyz','qrt'), engineModule2)('qrt','asd'), 'defqrtasd')

    for funcInfo in [(pe.CreateLambdaUnscoped,''), (pe.CreateMethodUnscoped,'return ')]:
        func = funcInfo[0]
        prepend = funcInfo[1]
        
        # unscoped lambda
        x = func[SimpleReturnDelegate](prepend+'abc')
        
        AreEqual(x(engineModule1)(), 'xyz')
        AreEqual(x(engineModule2)(), 'def')
        
    
    
def test_CreateMethod_Negative():
    """Negative test cases for PythonEngine.CreateMethod<DelegateType>"""
    
    pe = IronPython.Hosting.PythonEngine(CreateOptions())
    from System import ArgumentException, Delegate, MulticastDelegate
    from IronPython.Hosting import EngineModule    

    load_iron_python_test()
    from IronPythonTest import SimpleDelegate, SimpleReturnDelegateArg1, SimpleReturnDelegateArg2
    engineModule = pe.CreateModule()

    # specifying too many arguments should raise a ArgumentException
    try:
        pe.CreateMethod[SimpleDelegate]('abc', ('abc',), engineModule)
        AreEqual(True, False)
    except ArgumentException:
        pass
        
    # specifying too many arguments should raise a ArgumentException (w/o module scope)
    try:
        pe.CreateMethod[SimpleDelegate]('abc', ('abc',))
        AreEqual(True, False)
    except ArgumentException:
        pass

    # scoped w/ 2 args, only 1st arg is remapped, so we throw due to bad args
    try:
        pe.CreateMethod[SimpleReturnDelegateArg2]('abc + xyz + arg2', ('xyz',), engineModule)('qrt','asd')
        AssertUnreachable()
    except ValueError:
        pass
        
    # failing to have a return statements doesn't yield an expression value, it returns None
    AreEqual(pe.CreateMethod[SimpleReturnDelegateArg1]('arg1')('abc'), None)

    # bad generic types used to create lambda
    try:        
        pe.CreateMethod[Delegate]('abc')
        Assert(False)
    except ArgumentException: 
        pass
    
    try:        
        pe.CreateMethod[MulticastDelegate]('abc')
        Assert(False)
    except ArgumentException: 
        pass


    try:        
        pe.CreateMethod[object]('abc')
        Assert(False)
    except ArgumentException: 
        pass

def CreateOptions():
    import sys
    
    from IronPython.Hosting import EngineOptions
    o = EngineOptions()
    if sys.argv.count('-X:ExceptionDetail') > 0: o.ExceptionDetail = True
    return o

def test_CreateLambda_Negative():
    """Negative test cases for PythonEngine.CreateLambda<DelegateType>"""
    pe = IronPython.Hosting.PythonEngine(CreateOptions())

    from System import ArgumentException, Delegate, MulticastDelegate
    from IronPython.Hosting import EngineModule

    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegate, SimpleReturnDelegateArg2
    engineModule = pe.CreateModule()
    engineModule.Globals['abc'] = 'abc'

    # specifying too many arguments should raise a ArgumentException
    try:
        pe.CreateLambda[SimpleReturnDelegate]('abc', ('abc',), engineModule)
        Assert(False)
    except ArgumentException:
        pass
        
    # specifying too many arguments should raise a ArgumentException (w/o module scope)
    try:
        pe.CreateLambda[SimpleReturnDelegate]('abc', ('abc',))
        Assert(False)
    except ArgumentException:
        pass        

    # verify statements don't compile for lambda's
    try:
        pe.CreateLambda[SimpleReturnDelegate]('print abc')()
        Assert(False)
    except SyntaxError:
        pass
        
    # scoped w/ 2 args, only 1st arg is remapped, so we throw due to not providing enough args
    try:
        pe.CreateLambda[SimpleReturnDelegateArg2]('abc + xyz + arg2', ('xyz',), engineModule)('qrt','asd')
        AssertUnreachable()
    except ValueError:
        pass

    # bad generic types used to create lambda
    try:        
        pe.CreateLambda[Delegate]('abc')
        Assert(False)
    except ArgumentException: 
        pass
    
    try:        
        pe.CreateLambda[MulticastDelegate]('abc')
        Assert(False)
    except ArgumentException: 
        pass


    try:        
        pe.CreateLambda[object]('abc')
        Assert(False)
    except ArgumentException: 
        pass


def test_CreateMethod_ImportSys():
    script = "import sys\nreturn sys.version"
    
    pe = IronPython.Hosting.PythonEngine(CreateOptions())
    
    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegate	
    
    method = pe.CreateMethod[SimpleReturnDelegate](script)
    import sys
    AreEqual(method(), sys.version)


def test_CreateLambda_Division():
    pe = IronPython.Hosting.PythonEngine(CreateOptions())
    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegateArg1	
    
    ex = pe.CreateLambda[SimpleReturnDelegateArg1]("1.0 / arg1")
    result = ex(100000)
    Assert(result < 1)

#------------------------------------------------------------------------------
def generate_nb_test():
    '''
    Helper function which generates a test in the form of a single string
    to ensure that locals() and globals() work as expected when executed
    directly from PythonEngines.  To be more precise, this function returns
    a slightly modified version of test_namebinding.py in string format.
    '''
    DEBUG = False
     
    #get the test we want to run.
    f = open(testpath.public_testdir + "\\test_namebinding.py", "r")
    lines = f.readlines()
    f.close()
    
    #strip out imports. PythonEngine does not really work
    #with these at the moment
    lines.remove("from lib.assert_util import *\n")
    lines.remove("from collections import *\n")
    lines.remove("    import System\n")
    lines.remove("    from System import GC\n")
    lines.remove("    import gc\n")

    if DEBUG:
        #Add them back in a format that works better with
        #hosted environments
        lines.insert(0, "import lib\n")
        lines.insert(0, "import lib.assert_util\n")
        lines.insert(1, "import System\n")
        lines.insert(2, "import gc\n")
    
    #fix some namespace issues
    lines = [x.replace("AreEqual(", "lib.assert_util.AreEqual(") for x in lines]
    lines = [x.replace("Assert(", "lib.assert_util.Assert(") for x in lines]
    lines = [x.replace("AssertError(", "lib.assert_util.AssertError(") for x in lines]
    lines = [x.replace("GC.", "System.GC.") for x in lines]
    
    #For debugging purposes the file is written out to disk.
    #This is just to verify it can be run normally.
    if DEBUG:
        f = open(testpath.public_testdir + "\\temp_nb.py", "w")
        f.writelines(lines)
        f.close()
    
    #convert the list of strings into a single string
    ret_val = ""
    for line in lines:
        ret_val = ret_val + line
        
    return ret_val
    
#------------------------------------------------------------------------------
def test_engine_from_python():
    '''
    Tests a PythonEngine from IP.  At the moment this is just intended
    to hit anything that test_engine_from_csharp is missing.
    '''
    temp = IronPython.Hosting.PythonEngine()
    #CodePlex Work Item 6707
    #Assert(temp.Sys.prefix!=None)
    
    #CodePlex Work Item 6708
    #Assert(temp.Import("from sys import winver")!=None)
    
    #--Hit IronPython.Hosting.StringDictionary.AdapterDict
    
    #Partially because of CodePlex Work Item 6707, we need a helper
    #function to import external Python modules
    def get_pe():
        '''
        Helper function returns a new PythonEngine with the appropriate
        Imports
        '''
        import sys
        ret_val = IronPython.Hosting.PythonEngine()
        ret_val.Import("sys")
        ret_val.Execute("sys.path = " + str(sys.path))
        ret_val.Execute("sys.prefix = '" + sys.prefix + "'")
        ret_val.Import("lib")
        ret_val.Import("lib.assert_util")
        ret_val.Import("System")
        ret_val.Import("gc")
        
        return ret_val
    
    #test globals() manipulation
    temp = get_pe()
    temp.Execute(globals_test)
    
    #Merlin Work Item 170204
    #test_namebinding.py
    #temp = get_pe()
    #name_binding_test = generate_nb_test()
    #temp.Execute(name_binding_test)
    
    
    #--FormatException---------
    def a():
        raise System.Exception()

    def b():
        try:
            a()
        except System.Exception, e:
            raise System.Exception("second", e)

    def c():
        try:
            b()
        except System.Exception, e:
            x = System.Exception("first", e)
        return x
    
    #No options
    options = IronPython.Hosting.EngineOptions()
    temp = IronPython.Hosting.PythonEngine(options)
    
    exc_string = temp.FormatException(System.Exception("first", 
                                                       System.Exception("second", 
                                                                        System.Exception())))
    AreEqual(exc_string, 'Traceback (most recent call last):\r\nException: first\r\n')
    exc_string = temp.FormatException(c())
    AreEqual(exc_string.count(" File "), 4)
    AreEqual(exc_string.count(" line "), 4)
    
    #CLR Exceptions option
    options = IronPython.Hosting.EngineOptions()
    options.ShowClrExceptions = True
    temp = IronPython.Hosting.PythonEngine(options)
    
    exc_string = temp.FormatException(System.Exception("first", 
                                                       System.Exception("second", 
                                                                        System.Exception())))
    AreEqual(exc_string, "Traceback (most recent call last):\r\nException: first\r\nCLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n")
    exc_string = temp.FormatException(c())
    AreEqual(exc_string.count(" File "), 4)
    AreEqual(exc_string.count(" line "), 4)
    Assert(exc_string.endswith("CLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n"))
    
    #Detailed Exceptions
    options = IronPython.Hosting.EngineOptions()
    options.ExceptionDetail = True
    temp = IronPython.Hosting.PythonEngine(options)
    
    #CodePlex Work Item 6710
    #exc_string = temp.FormatException(System.Exception("first", System.Exception("second", System.Exception())))
    #AreEqual(exc_string, "Traceback (most recent call last):\r\nException: first\r\nCLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n")
    #exc_string = temp.FormatException(c())
    #AreEqual(exc_string.count(" File "), 4)
    #AreEqual(exc_string.count(" line "), 4)
    #Assert(exc_string.endswith("CLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n"))

#------------------------------------------------------------------------------
def test_optimized_engine_module():
    '''
    Tests Optmized EngineModules
    '''
    temp = IronPython.Hosting.PythonEngine()
    
    opt_module_name = "optimo"
    opt_module_filename = testpath.temporary_dir + "temp_opt_model.py"
    
    #list of optimized engine modules which we'll run some generic 
    #tests on
    oem_list = []
    
    #create a temporary Python module
    opt_module = '''
a = 1
def A():
    global a
    #print a
    a = a * 2
A()
    '''
    
    f = open(opt_module_filename, "w")
    print >> f, opt_module
    f.close()
    
    oem = temp.CreateOptimizedModule(opt_module_filename, opt_module_name, True)
    Assert(oem.ToString().find(oem.Name)!=-1)
    oem.Execute()
    oem_list.append(oem)
        
    oem = temp.CreateOptimizedModule(opt_module_filename, opt_module_name, False)
    oem.Execute()
    oem_list.append(oem)
    
    #--TEST GLOBALS
    for oem in oem_list:
        AssertError(SystemError, oem.Execute)
        play_with_oem_globals(oem.Globals)
    
#---------------------------------------------------------------------------------
def play_with_oem_globals(oem_globals):
    '''
    Helper function used to test an optimized engine module's Globals property.
    '''
    #the caller should have set "a" to 2.
    AreEqual(oem_globals["a"], 2)
    
    #--ToString
    AreEqual(type(oem_globals.ToString()), str)
    
    #--Add
    oem_globals["b"] = None
    AreEqual(oem_globals["b"], None)
    
    oem_globals.Add("c", "blah")
    AreEqual(oem_globals["c"], "blah")
    
    temp = System.Collections.Generic.KeyValuePair[str,object]("d", int(3))
    oem_globals.Add(temp)
    AreEqual(oem_globals["d"], int(3))
    
    #--Keys
    temp = [x for x in oem_globals.Keys]
    #special case must be removed
    if temp.count("__doc__")==1:
        temp.remove('__doc__')
    temp.sort()
    AreEqual(temp, 
             ['A', '__builtins__', '__file__', '__name__', 'a', 'b', 'c', 'd'])
    
    #--Values
    temp = [x for x in oem_globals.Values if callable(x)==False]
    #strip out the path where temp_opt_model.py exists
    for i in range(len(temp)):
        if type(temp[i])==str and temp[i].endswith("temp_opt_model.py"):
            temp[i] = "temp_opt_model.py"
    #special case must be removed
    if temp.count(None)==2:
        temp.remove(None)
    temp.sort()
    AreEqual(temp, 
             [None, 2, 3, 'blah', 'optimo', 'temp_opt_model.py'])
    
    #--GetEnumerator
    ator = oem_globals.GetEnumerator()
    temp = [ele.Key for ele in ator]
    #special case must be removed
    if temp.count("__doc__")==1:
        temp.remove('__doc__')
    temp.sort()
    AreEqual(temp,
             ['A', '__builtins__', '__file__', '__name__', 'a', 'b', 'c', 'd'])
    
    #CodePlex Work Item 6711
    #We should just be able to call 'ator.DoReset()' instead
    #of generating another enumerator.
    #ator.DoReset()
    ator = oem_globals.GetEnumerator()
    temp = [ele.Value for ele in ator if callable(ele.Value)==False]
    #strip out the path where temp_opt_model.py exists
    for i in range(len(temp)):
        if type(temp[i])==str and temp[i].endswith("temp_opt_model.py"):
            temp[i] = "temp_opt_model.py"
    #special case must be removed
    if temp.count(None)==2:
        temp.remove(None)
    temp.sort()
    AreEqual(temp,
             [None, 2, 3, 'blah', 'optimo', 'temp_opt_model.py'])
    
    #--Contains
    temp = System.Collections.Generic.KeyValuePair[str,object]("b", None)
    Assert(oem_globals.Contains(temp))
    
    temp = System.Collections.Generic.KeyValuePair[str,object]("c", "blah")
    Assert(oem_globals.Contains(temp))
    
    #CodePlex Work Item 6703
    #temp = System.Collections.Generic.KeyValuePair[str,object]("d", int(3))
    #Assert(oem_globals.Contains(temp))
    
    temp = System.Collections.Generic.KeyValuePair[str,object]("e", "blah")
    Assert(oem_globals.Contains(temp)==False)
    
    #--ContainsKey
    temp_list = [("a", 1), ("b", 1), ("c", 1), ("d", 1), ("e", 0)]
    for key, expect in temp_list:
        AreEqual(oem_globals.ContainsKey(key), expect)
    
    #--TryGetValue
    temp_dict = {"a" : (True, 2),
                 "b": (True, None), 
                 "c": (True, "blah"), 
                 "d": (True, int(3)), 
                 "e": (False, None)}
    for key in temp_dict.keys():
        AreEqual(oem_globals.TryGetValue(key), temp_dict[key])
    
    #--Count
    if oem_globals.ContainsKey("__doc__"):
        AreEqual(oem_globals.Count, 9)
    else:
        AreEqual(oem_globals.Count, 8)
    
    #--IsReadOnly
    Assert(not oem_globals.IsReadOnly)
            
    #CopyTo
    AssertError(NotImplementedError,
                oem_globals.CopyTo,
                None, 3)
    
    #--Remove
    Assert(oem_globals.ContainsKey("d"))
    oem_globals.Remove("d")
    Assert(oem_globals.ContainsKey("d")==False)
    
    Assert(oem_globals.ContainsKey("c"))
    oem_globals.Remove(System.Collections.Generic.KeyValuePair[str,object]("c", "blah"))
    Assert(oem_globals.ContainsKey("c")==False)
    
    #--Clear!
    oem_globals.Clear()
    AreEqual(oem_globals.Count, 0)
    AreEqual([x for x in oem_globals.Keys], [])
    AreEqual([x for x in oem_globals.Values], [])
    
#---------------------------------------------------------------
    
def test_interactive_input():
    x = pe.ParseInteractiveInput("""x = "abc\\
""", True)
    AreEqual(x, False)

if __name__=="__main__":
    run_test(__name__)