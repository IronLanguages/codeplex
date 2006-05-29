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

from Util.Debug import *
import IronPython.CodeDom
import System.IO as SIO
import System.CodeDom.Compiler as SCC
import System
import sys

prov = IronPython.CodeDom.PythonProvider()
parser = prov.CreateParser()

#classes for generating python code

class fileGen(object):
    def __init__(self, members):
        self.members = members
    def generate(self, indent = 0):
        txt = ""
        for a in self.members:
            txt = txt + a.generate(indent)
        return txt

class typeGen(object):
    def __init__(self, name, baseClass, members, fields=None):
        self.name = name
        self.baseClass = baseClass
        self.members = members
        self.fields = fields
    def generate(self, indent):
        t = "\r\n%sclass %s(%s):\r\n" %("    "*indent, self.name, self.baseClass)
        if self.fields != None:
            slots = []
            doccomment = []
            for f in self.fields:
                doccomment.append( 'type(%s) == %s' % (f.name, f.type) )
                slots.append("'%s'" % f.name)
            t = t + '%s"""%s"""\r\n' % ('    '*(indent+1), ', '.join(doccomment))
            t = t + '%s__slots__ = [%s]\r\n' % ('    '*(indent+1), ', '.join(slots) )
        for m in self.members: t = t + m.generate(indent+1)
        return t+"\r\n"
        
    
class methGen(object):
   def __init__(self, name, args, body, retType = None, argTypes = None):
        self.name = name
        self.args = args
        self.body = body
        self.retType = retType
        self.argTypes = argTypes
   def generate(self, indent):
        m = ''
        if self.argTypes != None:
            m = m + '%s@accepts(%s)\r\n' % ('    '*indent, ', '.join(self.argTypes))
        if self.retType != None:
            m = m + "%s@returns(%s)\r\n" % ('    '*indent, self.retType)

        m = m + "%sdef %s(%s):\r\n" % ("    "*indent, self.name, ', '.join(self.args))
        for b in self.body: m = m + b.generate(indent+1)
        m = m + '    '*indent + "\r\n"
        
        return m
        
class ifStmt(object):
    def __init__(self, lhs, op, rhs, suite=None):
        self.lhs = lhs
        self.op = op
        self.rhs = rhs
        self.suite = suite
    def generate(self, indent): 
        if self.suite == None:
            return "%sif %s %s %s:\r\n" % ('    '*indent, self.lhs, self.op, self.rhs)
        else:
            res = "%sif %s %s %s:\r\n" % ('    '*indent, self.lhs, self.op, self.rhs)
            return res + self.suite.generate(indent+1)
      

class assignStmt(object):
    def __init__(self, lhs, rhs):
        self.lhs = lhs
        self.rhs = rhs
    def generate(self, indent): return '%s%s = %s\r\n' % ('    '*indent, self.lhs, self.rhs.generate())
        

class returnStmt(object):
    def __init__(self, val):
        self.value = val
    def generate(self, indent): return '%sreturn %s\r\n' % ('    '*indent, self.value)

class eventAttachStmt(object):
    def __init__(self, lhs, rhs):
        self.lhs = lhs
        self.rhs = rhs
    def generate(self, indent):
        return "%s%s += %s\r\n" % ('    '*indent, self.lhs, self.rhs)

class exprStmt(object):
    def __init__(self, expr):
        self.expr = expr
    def generate(self, indent):
        return '    '*indent + self.expr.generate() + '\r\n'
        
        
class importStmt(object):
    def __init__(self, name, imports=None):
        self.name = name
        self.imports = imports
    def generate(self, indent):
        if self.imports == None:
            return '%simport %s\r\n' % ('    '*indent, self.name)
        return '%sfrom %s import %s\r\n' % ('    '*indent, self.name, self.imports)
       
class fieldMember(object):
    def __init__(self, name, type):
        self.name = name
        self.type = type
    
class commentStmt(object):
    def __init__(self, text):
        self.text = text
    def generate(self, indent=0):
        return '# ' + self.text
   
class methodCall(object):
    def __init__(self, name, args, target=None):
        self.target = target
        self.args = args
        self.name = name
    def generate(self): 
        if self.target != None:
            return '%s.%s(%s)' % (self.target, self.name, ', '.join(self.args))
        else:
            return '%s(%s)' % (self.name, ', '.join(self.args))
        
class idRef(object):
    def __init__(self, id):
        self.id = id
    def generate(self): return self.id
        
        
def RunGeneratedExe(file):
    targetdir = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(file)).ToLower()
    ippath = sys.prefix.ToLower()

    if targetdir != ippath:
        System.IO.File.Copy(System.IO.Path.Combine(sys.prefix, 'IronPython.dll'), targetdir + '\\IronPython.dll', True)
        System.IO.File.Copy(System.IO.Path.Combine(sys.prefix, 'IronMath.dll'), targetdir + '\\IronMath.dll', True)

    oldDir = System.Environment.CurrentDirectory
    try:
        System.Environment.CurrentDirectory = targetdir
        retval = nt.spawnl(0, file)
    except Exception, e:
        print "exception while running: %s" %e
        retval = 1
    System.Environment.CurrentDirectory = oldDir

    Assert(not retval)
        
# test cases made out of builders
        
testCases = [
#    typeGen('foo', 'object', 
#        [methGen('test1', ('x','y','z'), [ assignStmt('a', idRef('5')), commentStmt('hello world') ])],
#        [fieldMember('foo', 'bool')],
#        ),        

    fileGen( [importStmt('clr', '*'), 
            typeGen('foo', 'object', [    
                methGen('test1', ('self',), [
                    ifStmt("disposing","and", "(components != None)", exprStmt(methodCall("Disposing", [], "components")))])])]    ),
    
    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', [
        methGen('test1', ('x','y','z'), [ assignStmt('a', idRef('x')), returnStmt('a') ], 'int', ('Self()', 'int', 'int')),
        methGen('test2', ('x','y','z'), [ assignStmt('a', idRef('x')) ]) ])]),
        
    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', [
        methGen('test1', ('x','y','z'), [ assignStmt('a', idRef('x')) ]),
        methGen('test2', ('x','y','z'), [ assignStmt('a', idRef('x')) ]) ])]),
        
    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', 
        [methGen('test1', ('x','y','z'), [ assignStmt('a', idRef('x')) ])],
        [fieldMember('foo', 'int')],
        )]),

    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', 
        [methGen('test1', ('x','y','z'), [ assignStmt('a', methodCall('dir', ['x'])) ])],
        [fieldMember('foo', 'int')],
        )]),

    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', 
        [methGen('test1', ('x','y','z'), [ assignStmt('a', methodCall('test2', ['x', 'y', 'z'], 'self')) ]),
        methGen('test2', ('self', 'x','y','z'), [ returnStmt('x') ]) ],
        [fieldMember('foo', 'int')],
        )]),

    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', 
        [methGen('test1', ('x','y','z'), [ assignStmt('a', idRef('x.y.z')) ])],
        [fieldMember('foo', 'int')],
        )]),
        
    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', 
        [methGen('test1', ('x','y','z'), [ assignStmt('a', idRef('None')) ])],
        [fieldMember('foo', 'int')],
        )]),        

    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', 
        [methGen('test1', ('x','y','z'), [ assignStmt('a', idRef('True')) ])],
        [fieldMember('foo', 'int')],
        )]),        

    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', 
        [methGen('test1', ('x','y','z'), [ assignStmt('a', idRef('5000000000000000000000000000L')) ])],
        [fieldMember('foo', 'int')],
        )]),        

    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', 
        [methGen('test1', ('x','y','z'), [ assignStmt('a', idRef("'abcdef'")) ])],
        [fieldMember('foo', 'int')],
        )]),        

    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', 
        [methGen('test1', ('x','y','z'), [ assignStmt('a', idRef('1L')) ])],
        [fieldMember('foo', 'float')],
        )]),        

    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', 
        [methGen('test1', ('x','y','z'), [ assignStmt('a', idRef('5.0')) ])],
        [fieldMember('foo', 'System.Object')],
        )]),        
        
    fileGen( [importStmt('clr', '*'), typeGen('foo', 'object', 
        [methGen('test1', ('x','y','z'), [ assignStmt('a', idRef('5')) ])],
        [fieldMember('foo', 'bool')],
        )]),        

    fileGen( [importStmt('clr', '*'), importStmt('System'), importStmt('System.Windows.Forms', '*'), typeGen('foo', 'System.Windows.Forms.Form',
        [methGen('handler', ('sender', 'eventArgs'), [ exprStmt(methodCall('dir', ['sender'])) ], 'None', ('Self()', 'System.EventArgs')),
        methGen('test2', ('self',), [eventAttachStmt('self.FormClosed', 'self.handler'), assignStmt('self.ctrl.Dock', idRef('System.Windows.Forms.DockStyle.Fill')) ]) ],
        [fieldMember('foo', 'System.Drawing.Point'), fieldMember('ctrl', 'System.Windows.Forms.Control')])])
                
    ]

def runTextTest(txt, log = False):
    #Verify we can parse from both a simple string and a TextReader stream
    if log: print "parsing as a string..."
    runTextTestWorker(txt, False, log)
    if log: print "parsing as a TextReader..."
    runTextTestWorker(txt, True, log)


def runTextTestWorker(txt, asStream = False, log = False):
    if log: print "input:"
    if log: print txt
    if asStream:
        parsed = parser.Parse(SIO.StringReader(txt))
    else:
        parsed = parser.Parse(txt)
    
    res = SIO.StringWriter()
    opts = SCC.CodeGeneratorOptions()
    opts.BlankLinesBetweenMembers = False

    prov.GenerateCodeFromCompileUnit(parsed, res, opts)
    if log: print "round-tripped:"
    if log: print str(res)
    other = str(res)
    
    if txt != other:
        for i in range(len(txt)):
            if txt[i] != str(res)[i]: 
                print 'differ at ', i, repr(txt[i-15:i+15]), repr(str(res)[i-15:i+15])
                break
    
    AreEqual(txt, str(res))


def runTest(test, log=False):
    txt = test.generate(0)
    if log: print "generated:"
    if log: print txt
    parsed = parser.Parse(txt)
    
    res = SIO.StringWriter()
    opts = SCC.CodeGeneratorOptions()
    opts.BlankLinesBetweenMembers = False

    prov.GenerateCodeFromCompileUnit(parsed, res, opts)

    if log: print "round-tripped:"
    if log: print str(res)

    other = str(res)

    if txt != other:
        for i in range(len(txt)):
            if txt[i] != str(res)[i]: 
                print 'differ at ', i
                break
    
    AreEqual(txt, str(res))
    

def compileTest(test, log=False):
    
    txt = test.generate()
    if log: print 'generated:'
    if log: print txt
    
    parsed = parser.Parse(txt)
    pathToExe = iron_python_root+iron_python_tests.replace('/','\\')+'\\test.exe'    
    co = SCC.CompilerParameters(['System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'], pathToExe, False)
    co.GenerateInMemory = False
    res = prov.CompileAssemblyFromDom(co, parsed)
    if log: print res.Errors.Count
    
    if log:
        for err in res.Errors:
            print err
            
    AreEqual(res.Errors.Count, 0)
    
    RunGeneratedExe(pathToExe)
    try:
        SIO.File.Delete(pathToExe)
    except Exception:
        pass

test = """import System
from clr import *

class Foo(object):
    @returns(System.Int32)
    def get_System(self):
        return 3
    
    System = property(fget=get_System)
    @accepts(Self())
    @returns(System.Int32)
    def Test(self):
        retVal = System
        return retVal
    

""".replace('\n', '\r\n')

runTextTest(test, False)


test = """import System
from clr import *

class Foo(object):
    @returns(System.Int32)
    def get_System(self):
        return 3
    
    System = property(fget=get_System)
    @accepts(Self())
    @returns(System.Int32)
    def Test(self):
        retVal = self.System
        return retVal
    

""".replace('\n', '\r\n')

runTextTest(test, False)

test = """class WindowsApplicationCS: # namespace
    from System import *
    from System.Windows.Forms import *
    from Form1 import *
    from clr import *
    
    @staticmethod
    def RealEntryPoint():
        if __name__ == '__main__':
            Application.EnableVisualStyles()
            Application.Run(Form1())


WindowsApplicationCS.RealEntryPoint()
""".replace('\n', '\r\n')

runTextTest(test, False)

test = r"""class WindowsApplicationPY: # namespace
    from System import *
    from System.Windows.Forms import *
    from System.ComponentModel import *
    from System.Drawing import *
    from clr import *
    
    class Form1(Form):
        def __init__(self):
            self.InitializeComponent()
        
        @accepts(Self(), bool)
        @returns(None)
        def Dispose(self, disposing):
            super(type(self), self).Dispose(disposing)
        
        @returns(None)
        def InitializeComponent(self):
            self.SuspendLayout()
            self.AutoScaleDimensions = SizeF(6.0, 13.0)
            self.AutoScaleMode = AutoScaleMode.Font
            self.ClientSize = Size(292, 266)
            self.Name = 'Form1'
            self.Text = 'Form1'
            self.ResumeLayout(False)
            self.PerformLayout()
        
    

""".replace('\n', '\r\n')

runTextTest(test, False)

# verify we can handle imports defined outside the namespace, and round-trip that correctly
test = r"""from System import *
from System.Windows.Forms import *
from System.ComponentModel import *
from System.Drawing import *
from clr import *
class WindowsApplicationPY: # namespace
    
    class Form1(Form):
        def __init__(self):
            self.InitializeComponent()
        
        @accepts(Self(), bool)
        @returns(None)
        def Dispose(self, disposing):
            super(type(self), self).Dispose(disposing)
        
        @returns(None)
        def InitializeComponent(self):
            self.SuspendLayout()
            self.AutoScaleDimensions = SizeF(6.0, 13.0)
            self.AutoScaleMode = AutoScaleMode.Font
            self.ClientSize = Size(292, 266)
            self.Name = 'Form1'
            self.Text = 'Form1'
            self.ResumeLayout(False)
            self.PerformLayout()
        
    

""".replace('\n', '\r\n')

runTextTest(test, False)

test = r"""from clr import *

class Class1(object):
    def _PrivateMethod(self):
        self.field1 = 312
    
    def PublicMethod(self):
        self._PrivateMethod()
    

""".replace('\n', '\r\n')

runTextTest(test, False)

test = r"""from clr import *
import System

class Class1(object):
    """'"""type(field1) == System.Int32, type(_PrivateField) == System.String"""'"""
    __slots__ = ['field1', '_PrivateField']
    def __init__(self):
        self.Method1()
        self._ConstructorFieldInitFunction()
    
    @returns(System.Int32)
    def Method1(self):
        self.field1 = 93
    
    def _ConstructorFieldInitFunction(self):
        self.field1 = 312

""".replace('\n', '\r\n')

runTextTest(test, False)

test = r"""from clr import *
class Namespace: # namespace
    
    class Class1(object):
        def Method1(self):
            self.field1 = 1 + 1
            self.field1 = 3 & 9
            self.field1 = 2 | 7
            self.field1 = 9 / 3
            self.field1 = 3 == 4
            self.field1 = 7 > 2
            self.field1 = 3 >= 4
            self.field1 = 3 is int
            self.field1 = 6 < 8
            self.field1 = 6 <= 8
            self.field1 = 6 % 3
            self.field1 = 4 * 5
            self.field1 = 3 - 2
            self.field1 = 5 != 4
            while 3 > 2:
                self.field1 = 900
            while 7 < 32:
                pass
            if 1 == 1: pass
            if 2 == 3:
                self.field = 2
            else:
                self.field = 3
        
    
    class Class2(object): pass
    
    class Class3(object):
        def Method1(self):
            pass
        
    

""".replace('\n', '\r\n')

runTextTest(test, False)

for test in testCases:
    runTest(test, False)
    compileTest(test)
    


keywords =  [ 'and', 'del', 'for', 'is',  'raise', 'assert',    'elif',  'from',  'lambda',
'return', 'break',     'else',      'global',    'not',       'try', 'class',     'except',
'if'       , 'or',        'while', 'continue' ,  'exec',      'import',    'pass'     ,'yield',
'def',        'finally',   'in',       'print' ]

def CheckIdentifier(id,isvalid):
    global prov
    newid = prov.CreateValidIdentifier(id)
    if isvalid:
        Assert(id==newid)
    else:
        Assert(id!=newid)

for kw in keywords:
    CheckIdentifier(kw,False)

CheckIdentifier('something_256_valid',True)
CheckIdentifier('invalid!?',False)
CheckIdentifier('0invalid',False)

#BUG
#AreEqual(prov.FileExtension, "py")
#/BUG

# verify that we raise consistently for currently unsupported syntax
def ParseUnsupported(text):
    global parser
    try:
        parser.Parse(text)
        Fail('Unexpected success parsing \''+text+'\'')
    except SystemError:
        pass

ParseUnsupported('del(var)')
ParseUnsupported("""class C: pass
raise C()""")
ParseUnsupported("""try: pass
finally: pass""")
ParseUnsupported("""try: pass
except: pass""")
ParseUnsupported('break')
ParseUnsupported('continue')
ParseUnsupported('exec(\'pass\')')
ParseUnsupported('global var')
ParseUnsupported('for i in range(3): pass')
ParseUnsupported('assert(True)')
#ParseUnsupported('def f(): yield 3')
ParseUnsupported('[1,2,3]')
ParseUnsupported('var[2:]')
ParseUnsupported('(1,2)')

# Query what PythonGenerator supports
Assert(not prov.Supports(SCC.GeneratorSupport.GenericTypeDeclaration))
Assert(prov.Supports(SCC.GeneratorSupport.GenericTypeReference))
Assert(not prov.Supports(SCC.GeneratorSupport.MultipleInterfaceMembers))
Assert(not prov.Supports(SCC.GeneratorSupport.PartialTypes))
Assert(not prov.Supports(SCC.GeneratorSupport.Resources))
Assert(not prov.Supports(SCC.GeneratorSupport.Win32Resources))


AreEqual(prov.FileExtension, 'py')