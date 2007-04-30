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
from lib.process_util import *

load_iron_python_test()
import IronPythonTest.LoadTest as lt
import clr

AreEqual(lt.Name1.Value, lt.Values.GlobalName1)
AreEqual(lt.Name2.Value, lt.Values.GlobalName2)
AreEqual(lt.Nested.Name1.Value, lt.Values.NestedName1)
AreEqual(lt.Nested.Name2.Value, lt.Values.NestedName2)

AssertError(IOError, clr.AddReferenceToFileAndPath, path_combine(testpath.public_testdir, 'this_file_does_not_exist.dll'))
AssertError(IOError, clr.AddReferenceToFileAndPath, path_combine(testpath.public_testdir, 'this_file_does_not_exist.dll'))
AssertError(IOError, clr.AddReferenceToFileAndPath, path_combine(testpath.public_testdir, 'this_file_does_not_exist.dll'))
AssertError(IOError, clr.AddReferenceByName, 'bad assembly name', 'WellFormed.But.Nonexistent, Version=9.9.9.9, Culture=neutral, PublicKeyToken=deadbeefdeadbeef, processorArchitecture=6502')
AssertError(IOError, clr.AddReference, 'this_assembly_does_not_exist_neither_by_file_name_nor_by_strong_name')

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

AreEqual(clr.GetClrType(None), None)
AssertError(TypeError, clr.GetPythonType, None)

# load iron python test under an alias...
IPTestAlias = load_iron_python_test(True)

AreEqual(dir(IPTestAlias).count('IronPythonTest'), 1)

refs = clr.References
atuple = refs + (3,4) # should be able to append to references_tuple
#AssertError(TypeError, refs.__add__, "I am not a tuple")

s = str(refs)
AreEqual(s, '(' + ',\r\n'.join(map((lambda x:'<'+x.ToString()+'>'), refs)) + ')\r\n')

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
                divByNewline = result.split('\r\n  ')[1:]
                divByNewline[-1] = divByNewline[-1].split('\r\n\r\n')[0]
                return divByNewline
            except Exception:
                return []
        return []

gaclist = get_gac()
if (len(gaclist) > 0):
	clr.AddReferenceByName(gaclist[-1])
	
	
import NoNamespaceLoadTest

a = NoNamespaceLoadTest()

AreEqual(a.HelloWorld(), 'Hello World')



AssertError(TypeError, clr.AddReference, 35)

#####################
# VERIFY clr.AddReferenceToFile behavior...

runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
csc = runtimeDir + 'csc.exe'

tmp = System.Environment.GetEnvironmentVariable('TEMP') + "\\ip_load\\"
import nt

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

try:
    try:
        nt.mkdir(tmp)
    except: pass    # directory already exists
    
    a = file(tmp+'test1.cs', 'w')
    a.write(code1)
    a.close()

    a = file(tmp+'test2.cs', 'w')
    a.write(code2)
    a.close()


    result = nt.spawnv(0, csc, (csc, "/target:library", "/nologo", "/out:"+tmp+'test2.dll', tmp+'test2.cs'))
    result = nt.spawnv(0, csc, (csc, "/target:library", "/nologo", "/r:" + tmp + 'test2.dll', "/out:"+tmp+'test1.dll', tmp+'test1.cs'))
    
    clr.AddReferenceToFile('test1')
    
    foundTest1 = False
    for x in clr.References:
        if x.FullName.startswith('test1'):
            foundTest1 = True
            break
    AreEqual(foundTest1, True)
    
    # test 2 shouldn't be loaded yet...
    foundTest2 = False
    for x in clr.References:
        if x.FullName.startswith('test2'):
            foundTest2 = True
            break
    AreEqual(foundTest2, False)
    
    import test1
    # should create test1 (even though we're a top-level namespace)
    a = test1()    
    AreEqual(a.Test2(), 'test1.test2')
    
    # should load test2 from path
    AreEqual(a.Test1(), 'hello world')    
    
    foundTest2 = False
    for x in clr.References:
        if x.FullName.startswith('test2'):
            foundTest2 = True
            break
    AreEqual(foundTest2, False)
    
finally:
    sys.path.Remove(tmp)
    try: nt.unlink(tmp+'test1.cs')
    except: pass
    
    try: nt.unlink(tmp+'test2.cs')
    except: pass
    
    try: nt.unlink(tmp+'test1.dll')
    except: pass
    
    try: nt.unlink(tmp+'test2.dll')
    except: pass
    
    try: nt.rmdir(tmp)
    except: pass
    
    
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

x.BarNamespace.Bar = x.Foo
AreEqual(repr(x.BarNamespace.Bar), "<type 'Foo'>")

def f(): del x.BarNamespace.NotExist
AssertError(AttributeError, f)

del x.BarNamespace.Bar
AssertError(AttributeError, lambda: x.BarNamespace.Bar)

