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

import sys
from lib.assert_util import *
skiptest("silverlight")
from lib.process_util import *

load_iron_python_test()
import IronPythonTest.LoadTest as lt
import clr

def test_loadtest():
    AreEqual(lt.Name1.Value, lt.Values.GlobalName1)
    AreEqual(lt.Name2.Value, lt.Values.GlobalName2)
    AreEqual(lt.Nested.Name1.Value, lt.Values.NestedName1)
    AreEqual(lt.Nested.Name2.Value, lt.Values.NestedName2)

def test_negative_assembly_names():
    AssertError(IOError, clr.AddReferenceToFileAndPath, path_combine(testpath.public_testdir, 'this_file_does_not_exist.dll'))
    AssertError(IOError, clr.AddReferenceToFileAndPath, path_combine(testpath.public_testdir, 'this_file_does_not_exist.dll'))
    AssertError(IOError, clr.AddReferenceToFileAndPath, path_combine(testpath.public_testdir, 'this_file_does_not_exist.dll'))
    AssertError(IOError, clr.AddReferenceByName, 'bad assembly name', 'WellFormed.But.Nonexistent, Version=9.9.9.9, Culture=neutral, PublicKeyToken=deadbeefdeadbeef, processorArchitecture=6502')
    AssertError(IOError, clr.AddReference, 'this_assembly_does_not_exist_neither_by_file_name_nor_by_strong_name')
    
    AssertError(TypeError, clr.AddReference, 35)

    for method in [
        clr.AddReference,
        clr.AddReferenceToFile,
        clr.AddReferenceToFileAndPath,
        clr.AddReferenceByName,
        clr.AddReferenceByPartialName,
        clr.LoadAssemblyFromFileWithPath,
        clr.LoadAssemblyFromFile,
        clr.LoadAssemblyByName,
        clr.LoadAssemblyByPartialName,
        ]:
        AssertError(TypeError, method, None)

    for method in [
        clr.AddReference,
        clr.AddReferenceToFile,
        clr.AddReferenceToFileAndPath,
        clr.AddReferenceByName,
        clr.AddReferenceByPartialName,
        ]:
        AssertError(TypeError, method, None, None)
    import System
    AssertError(ValueError, clr.LoadAssemblyFromFile, System.IO.Path.DirectorySeparatorChar)
    AssertError(ValueError, clr.LoadAssemblyFromFile, '')
def test_get_type():
    AreEqual(clr.GetClrType(None), None)
    AssertError(TypeError, clr.GetPythonType, None)

# load iron python test under an alias...
def test_ironpythontest_from_alias():
    IPTestAlias = load_iron_python_test(True)
    AreEqual(dir(IPTestAlias).count('IronPythonTest'), 1)

def test_references():
    refs = clr.References
    atuple = refs + (clr.GetClrType(int).Assembly, ) # should be able to append to references_tuple
    #AssertError(TypeError, refs.__add__, "I am not a tuple")

    s = str(refs)
    temp = ',' + newline
    AreEqual(s, '(' + temp.join(map((lambda x:'<'+x.ToString()+'>'), refs)) + ')' + newline)

def test_gac():
    import System
    def get_gac():
            process = System.Diagnostics.Process()
            process.StartInfo.FileName = System.IO.Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "gacutil.exe")
            process.StartInfo.Arguments = "/nologo /l"
            process.StartInfo.CreateNoWindow = True
            process.StartInfo.UseShellExecute = False
            process.StartInfo.RedirectStandardInput = True
            process.StartInfo.RedirectStandardOutput = True
            process.StartInfo.RedirectStandardError = True
            try:
                process.Start()
            except WindowsError:
                return []
            result = process.StandardOutput.ReadToEnd()
            process.StandardError.ReadToEnd()
            process.WaitForExit()
            if process.ExitCode == 0:
                try:
                    divByNewline = result.split(newline + '  ')[1:]
                    divByNewline[-1] = divByNewline[-1].split(newline + newline)[0]
                    return divByNewline
                except Exception:
                    return []
            return []

    gaclist = get_gac()
    if (len(gaclist) > 0):
	    clr.AddReferenceByName(gaclist[-1])
	

def test_nonamespaceloadtest():	
    import NoNamespaceLoadTest
    a = NoNamespaceLoadTest()
    AreEqual(a.HelloWorld(), 'Hello World')





#####################
# VERIFY clr.AddReferenceToFile behavior...
def test_addreferencetofile_verification():
    tmp = testpath.temporary_dir
    sys.path.append(tmp)

    code1 = """
using System;

public class test1{
    public static string Test1(){
        test2 t2 = new test2();
        return t2.DoSomething();
    }
    
    public static string Test2(){
        return "test1.test2";
    }
}
"""

    code2 = """
using System;

public class test2{
    public string DoSomething(){
        return "hello world";
    }
}
"""

    test1_dll_along_with_ipy = path_combine(sys.prefix, 'test1.dll') # this dll is need for peverify

    # delete the old test1.dll if exists
    delete_files(test1_dll_along_with_ipy)

    test1_cs, test1_dll = path_combine(tmp, 'test1.cs'), path_combine(tmp, 'test1.dll')
    test2_cs, test2_dll = path_combine(tmp, 'test2.cs'), path_combine(tmp, 'test2.dll')
        
    write_to_file(test1_cs, code1)
    write_to_file(test2_cs, code2)
    
    AreEqual(run_csc("/nologo /target:library /out:"+ test2_dll + ' ' + test2_cs), 0)
    AreEqual(run_csc("/nologo /target:library /r:" + test2_dll + " /out:" + test1_dll + ' ' + test1_cs), 0)
    
    clr.AddReferenceToFile('test1')
    
    AreEqual(len([x for x in clr.References if x.FullName.startswith("test1")]), 1)

    # test 2 shouldn't be loaded yet...
    AreEqual(len([x for x in clr.References if x.FullName.startswith("test2")]), 0)
    
    import test1
    # should create test1 (even though we're a top-level namespace)
    a = test1()    
    AreEqual(a.Test2(), 'test1.test2')
    # should load test2 from path
    AreEqual(a.Test1(), 'hello world')    
    AreEqual(len([x for x in clr.References if x.FullName.startswith("test2")]), 0)
    
    # this is to make peverify happy, apparently snippetx.dll referenced to test1
    filecopy(test1_dll, test1_dll_along_with_ipy)
    
#####################
def test_addreference_sanity():
    # add reference directly to assembly
    clr.AddReference(''.GetType().Assembly)
    # add reference via partial name
    clr.AddReference('System.Xml')

    # add a reference via a fully qualified name
    clr.AddReference(''.GetType().Assembly.FullName)

def get_local_filename(base):
    if __file__.count('\\'):
        return __file__.rsplit("\\", 1)[0] + '\\'+ base
    else:
        return base

def compileAndLoad(name, filename, *args):
    import clr
    sys.path.append(sys.exec_prefix)
    AreEqual(run_csc("/nologo /t:library " + ' '.join(args) + " /out:\"" + sys.exec_prefix + "\"\\" + name +".dll \"" + filename + "\""), 0)
    return clr.LoadAssemblyFromFile(name)
    
def test_classname_same_as_ns():
    sys.path.append(sys.exec_prefix)
    AreEqual(run_csc("/nologo /t:library /out:\"" + sys.exec_prefix + "\"\\c4.dll \"" + get_local_filename('c4.cs') + "\""), 0)
    clr.AddReference("c4")
    import c4
    Assert(not c4 is c4.c4)
    Assert(c4!=c4.c4)

def test_local_dll():    
    x = compileAndLoad('c3', get_local_filename('c3.cs') )

    AreEqual(repr(x), "<Assembly c3, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null>")
    AreEqual(repr(x.Foo), "<type 'Foo'>")
    AreEqual(repr(x.BarNamespace), "<module 'BarNamespace' (CLS module from c3, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null)>")
    AreEqual(repr(x.BarNamespace.NestedNamespace), "<module 'NestedNamespace' (CLS module from c3, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null)>")
    AreEqual(repr(x.BarNamespace.Bar.NestedBar), "<type 'NestedBar'>")
    AreEqual(x.__dict__["BarNamespace"], x.BarNamespace)
    AreEqual(x.BarNamespace.__dict__["Bar"], x.BarNamespace.Bar)
    AreEqual(x.BarNamespace.__dict__["NestedNamespace"], x.BarNamespace.NestedNamespace)
    AreEqual(x.BarNamespace.NestedNamespace.__name__, "NestedNamespace")
    AreEqual(x.BarNamespace.NestedNamespace.__file__, "c3, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
    AssertError(AttributeError, lambda: x.BarNamespace.NestedNamespace.not_exist)
    AssertError(AttributeError, lambda: x.Foo2)  # assembly c3 has no type Foo2
    Assert(set(['NestedNamespace', 'Bar']) <= set(dir(x.BarNamespace)))

    def f(): x.BarNamespace.Bar = x.Foo
    AssertError(AttributeError, f)
    
    def f(): del x.BarNamespace.NotExist
    AssertError(AttributeError, f)
    
    def f(): del x.BarNamespace
    AssertError(AttributeError, f)

def test_namespaceimport():
    tmp = testpath.temporary_dir
    if tmp not in sys.path:
        sys.path.append(tmp)

    code1 = "namespace TestNamespace { public class Test1 {} }"
    code2 = "namespace TestNamespace { public class Test2 {} }"

    test1_cs, test1_dll = path_combine(tmp, 'testns1.cs'), path_combine(tmp, 'testns1.dll')
    test2_cs, test2_dll = path_combine(tmp, 'testns2.cs'), path_combine(tmp, 'testns2.dll')
        
    write_to_file(test1_cs, code1)
    write_to_file(test2_cs, code2)
    
    AreEqual(run_csc("/nologo /target:library /out:"+ test1_dll + ' ' + test1_cs), 0)
    AreEqual(run_csc("/nologo /target:library /out:"+ test2_dll + ' ' + test2_cs), 0)
    
    clr.AddReference('testns1')
    import TestNamespace
    AreEqual(dir(TestNamespace), ['Test1'])
    clr.AddReference('testns2')
    # verify that you don't need to import TestNamespace again to see Test2
    AreEqual(dir(TestNamespace), ['Test1', 'Test2'])    

def test_no_names_provided():
    AssertError(TypeError, clr.AddReference, None)
    AssertError(TypeError, clr.AddReferenceToFile, None)
    AssertError(TypeError, clr.AddReferenceByName, None)
    AssertError(TypeError, clr.AddReferenceByPartialName, None)
    AssertError(ValueError, clr.AddReference)
    AssertError(ValueError, clr.AddReferenceToFile)
    AssertError(ValueError, clr.AddReferenceByName)
    AssertError(ValueError, clr.AddReferenceByPartialName)

run_test(__name__)