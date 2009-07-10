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

from generate import generate
import System
import clr

import exceptions

def collect_excs():
    ret = []
    for e in exceptions.__dict__.values():  
        if not hasattr(e, '__bases__'): continue
        if e.__name__ == "exceptions": continue
        if e.__name__ == "__builtin__": continue
        
        assert len(e.__bases__) <= 1, e
        if len(e.__bases__) == 0:
            continue
            #supername = None
        else:
            supername = e.__bases__[0].__name__
        ret.append( (e, supername) )
    return ret
excs = collect_excs()

pythonExcs = ['ImportError', 'RuntimeError', 'UnicodeTranslateError', 'PendingDeprecationWarning', 'EnvironmentError',
              'LookupError', 'OSError', 'DeprecationWarning', 'UnicodeError', 'FloatingPointError', 'ReferenceError',
              'FutureWarning', 'AssertionError', 'RuntimeWarning', 'ImportWarning', 'UserWarning', 'SyntaxWarning', 
	          'UnicodeWarning', 'StopIteration', 'BytesWarning', 'BufferError']


class ExceptionInfo(object):
    def __init__(self, name, clrException, args, fields, subclasses, silverlightSupported = True):
        self.name = name
        self.clrException = clrException
        self.args = args
        self.fields = fields
        self.subclasses = subclasses
        self.silverlightSupported = silverlightSupported
        self.parent = None
        for child in subclasses:
            child.parent = self
    
    @property
    def ConcreteParent(self):        
        while not self.parent.fields:
            self = self.parent
            if self.parent == None: return exceptionHierarchy
        
        return self.parent
        
    @property
    def PythonType(self):
        if not self.parent:
            return 'DynamicHelpers.GetPythonTypeFromType(typeof(%s))' % self.name
        else:
            return self.name

    @property 
    def ClrType(self):
        if not self.parent:
            return 'BaseException'
        elif self.fields:
            return '_' + self.name
        else:
            return self.name
    
    @property
    def DotNetExceptionName(self):
        return self.clrException[self.clrException.rfind('.')+1:]
    
    @property
    def InternalPythonType(self):
        if not self.parent:
            return 'PythonExceptions._' + self.name
        else:
            return 'PythonExceptions.' + self.name

    def BeginSilverlight(self, cw):
        if not self.silverlightSupported:
            cw.writeline('')
            cw.writeline('#if !SILVERLIGHT');

    def EndSilverlight(self, cw):
        if not self.silverlightSupported:
            cw.writeline('#endif // !SILVERLIGHT')
            cw.writeline('');
    
    

    # format is name, args, (fields, ...), (subclasses, ...)
exceptionHierarchy = ExceptionInfo('BaseException', 'System.Exception', None, None, (
            ExceptionInfo('GeneratorExit', 'IronPython.Runtime.Exceptions.GeneratorExitException', None, (), ()),
            ExceptionInfo('SystemExit', 'IronPython.Runtime.Exceptions.SystemExitException', None, ('code',), ()),
            ExceptionInfo('KeyboardInterrupt', 'Microsoft.Scripting.KeyboardInterruptException', None, (), ()),
            ExceptionInfo('Exception', 'System.Exception', None, (), (
                    ExceptionInfo('StopIteration', 'IronPython.Runtime.Exceptions.StopIterationException', None, (), ()),
                    ExceptionInfo('StandardError', 'System.ApplicationException', None, (), (
                            ExceptionInfo('BufferError', 'IronPython.Runtime.Exceptions.BufferException', None, (), ()),
                            ExceptionInfo('ArithmeticError', 'System.ArithmeticException', None, (), (
                                    ExceptionInfo('FloatingPointError', 'IronPython.Runtime.Exceptions.FloatingPointException', None, (), ()),
                                    ExceptionInfo('OverflowError', 'System.OverflowException', None, (), ()),
                                    ExceptionInfo('ZeroDivisionError', 'System.DivideByZeroException', None, (), ()),                                    
                                ),
                            ),
                            ExceptionInfo('AssertionError', 'IronPython.Runtime.Exceptions.AssertionException', None, (), ()),
                            ExceptionInfo('AttributeError', 'System.MissingMemberException', None, (), ()),
                            ExceptionInfo('EnvironmentError', 'System.Runtime.InteropServices.ExternalException', None, ('errno', 'strerror', 'filename'), (
                                    ExceptionInfo('IOError', 'System.IO.IOException', None, (), ()),
                                    ExceptionInfo('OSError', 'IronPython.Runtime.Exceptions.OSException', None, (), (
                                            ExceptionInfo('WindowsError', 'System.ComponentModel.Win32Exception', None, ('winerror',), ()),
                                        ),
                                    ),
                                ),
                            ),
                            ExceptionInfo('EOFError', 'System.IO.EndOfStreamException', None, (), ()),
                            ExceptionInfo('ImportError', 'IronPython.Runtime.Exceptions.ImportException', None, (), ()),
                            ExceptionInfo('LookupError', 'IronPython.Runtime.Exceptions.LookupException', None, (), (
                                    ExceptionInfo('IndexError', 'System.IndexOutOfRangeException', None, (), ()),
                                    ExceptionInfo('KeyError', 'System.Collections.Generic.KeyNotFoundException', None, (), ()),
                                ),
                            ),
                            ExceptionInfo('MemoryError', 'System.OutOfMemoryException', None, (), ()),
                            ExceptionInfo('NameError', 'Microsoft.Scripting.Runtime.UnboundNameException', None, (), (
                                    ExceptionInfo('UnboundLocalError', 'Microsoft.Scripting.Runtime.UnboundLocalException', None, (), ()),
                                ),
                            ),
                            ExceptionInfo('ReferenceError', 'IronPython.Runtime.Exceptions.ReferenceException', None, (), ()),
                            ExceptionInfo('RuntimeError', 'IronPython.Runtime.Exceptions.RuntimeException', None, (), (
                                    ExceptionInfo('NotImplementedError', 'System.NotImplementedException', None, (), ()),
                                ),
                            ),
                            ExceptionInfo('SyntaxError', 'Microsoft.Scripting.SyntaxErrorException', None, ('text', 'print_file_and_line', 'filename', 'lineno', 'offset', 'msg'), (
                                    ExceptionInfo('IndentationError', 'IronPython.Runtime.Exceptions.IndentationException', None, (), (
                                            ExceptionInfo('TabError', 'IronPython.Runtime.Exceptions.TabException', None, (), ()),
                                        ),
                                    ),                                    
                                ),                                
                            ),
                            ExceptionInfo('SystemError', 'System.SystemException', None, (), ()),
                            ExceptionInfo('TypeError', 'Microsoft.Scripting.ArgumentTypeException', None, (), ()),
                            ExceptionInfo('ValueError', 'System.ArgumentException', None, (), (
                                    ExceptionInfo('UnicodeError', 'IronPython.Runtime.Exceptions.UnicodeException', None, (), 
                                        (
                                            ExceptionInfo('UnicodeDecodeError', 'System.Text.DecoderFallbackException', ('encoding', 'object', 'start', 'end', 'reason'), ('start', 'reason', 'object', 'end', 'encoding'), (), False),
                                            ExceptionInfo('UnicodeEncodeError', 'System.Text.EncoderFallbackException', ('encoding', 'object', 'start', 'end', 'reason'), ('start', 'reason', 'object', 'end', 'encoding'), (), False),
                                            ExceptionInfo('UnicodeTranslateError', 'IronPython.Runtime.Exceptions.UnicodeTranslateException', None, ('start', 'reason', 'object', 'end', 'encoding'), ()),
                                        ),
                                    ),
                                ),
                            ),
                        ),
                    ),
                    ExceptionInfo('Warning', 'System.ComponentModel.WarningException', None, (), (
                            ExceptionInfo('DeprecationWarning', 'IronPython.Runtime.Exceptions.DeprecationWarningException', None, (), ()),
                            ExceptionInfo('PendingDeprecationWarning', 'IronPython.Runtime.Exceptions.PendingDeprecationWarningException', None, (), ()),
                            ExceptionInfo('RuntimeWarning', 'IronPython.Runtime.Exceptions.RuntimeWarningException', None, (), ()),
                            ExceptionInfo('SyntaxWarning', 'IronPython.Runtime.Exceptions.SyntaxWarningException', None, (), ()),
                            ExceptionInfo('UserWarning', 'IronPython.Runtime.Exceptions.UserWarningException', None, (), ()),
                            ExceptionInfo('FutureWarning', 'IronPython.Runtime.Exceptions.FutureWarningException', None, (), ()),
                            ExceptionInfo('ImportWarning', 'IronPython.Runtime.Exceptions.ImportWarningException', None, (), ()),
                            ExceptionInfo('UnicodeWarning', 'IronPython.Runtime.Exceptions.UnicodeWarningException', None, (), ()),
                            ExceptionInfo('BytesWarning', 'IronPython.Runtime.Exceptions.BytesWarningException', None, (), ()),
                        ),
                    ),
                ),
            ),      
        ),
    )


def get_all_exceps(l, curHierarchy):
    # if we have duplicate CLR exceptions (e.g. VMSError and Exception)
    # only generate the one highest in the Python hierarchy
    for exception in curHierarchy.subclasses:
        found = False
        for e in l:
            if e.clrException == exception.clrException:
                found = True
                break
        
        if not found:
            l.append(exception)
    for exception in curHierarchy.subclasses:
        get_all_exceps(l, exception)
    return l

ip = clr.LoadAssemblyByPartialName('ironpython')
ms = clr.LoadAssemblyByPartialName('Microsoft.Scripting')
sysdll = clr.LoadAssemblyByPartialName('System')

def get_type(name):
    if name.startswith('IronPython'):            return ip.GetType(name)
    if name.startswith('Microsoft.Scripting'):   return ms.GetType(name)
    if name.startswith('System.ComponentModel'): return sysdll.GetType(name)
    
    return System.Type.GetType(name)


def exception_distance(a):
    distance = 0
    while a.FullName != "System.Exception":
        a = a.BaseType
        distance += 1
    return distance
     
def compare_exceptions(a, b):
    a, b = a.clrException, b.clrException
    
    ta = get_type(a)
    tb = get_type(b)
    
    if ta.IsSubclassOf(tb): return -1
    if tb.IsSubclassOf(ta): return 1
    
    da = exception_distance(ta)
    db = exception_distance(tb)
    
    # put exceptions further from System.Exception 1st, those further later...
    if da != db: return db - da
    
    return cmp(ta.Name, tb.Name)
    
def gen_topython_helper(cw):
    cw.enter_block("private static BaseException/*!*/ ToPythonHelper(System.Exception clrException)")
    
    allExceps = get_all_exceps([], exceptionHierarchy)
    allExceps.sort(cmp=compare_exceptions)
    
    for x in allExceps[:-1]:    # skip System.Exception which is last...
        if not x.silverlightSupported: cw.writeline('#if !SILVERLIGHT')
        if x.fields or x.name == 'BaseException':        
            cw.writeline('if (clrException is %s) return new _%s();' % (x.DotNetExceptionName, x.name))
        else:
            cw.writeline('if (clrException is %s) return new %s(%s);' % (x.DotNetExceptionName, x.ConcreteParent.ClrType, x.name))
        if not x.silverlightSupported: cw.writeline('#endif')
        
    cw.writeline('return new BaseException(Exception);')    
    cw.exit_block()
    
    cw.enter_block("private static System.Exception/*!*/ ToClrHelper(PythonType/*!*/ type, string message)")
    for x in allExceps:
        if not x.silverlightSupported: cw.writeline('#if !SILVERLIGHT')
        #if not x.fields:
        cw.writeline('if (type == %s) return new %s(message);' % (x.name, x.DotNetExceptionName))
        #else:
        #    cw.writeline('if (type == DynamicHelpers.GetPythonTypeFromType(typeof(%s))) return new %s(message);' % (x.name, x.clrException))  
        if not x.silverlightSupported: cw.writeline('#endif')  
    cw.writeline('return new Exception(message);')
    cw.exit_block()
    
        
def get_clr_name(e):
    return e.replace('Error', '') + 'Exception'

FACTORY = """
public static Exception %(name)s(string format, params object[] args) {
    return new %(clrname)s(string.Format(format, args));
}"""

def factory_gen(cw):
    for e in pythonExcs:
        cw.write(FACTORY, name=e, clrname=get_clr_name(e))

CLASS1 = """
[Serializable]
public class %(name)s : %(supername)s {
    public %(name)s() : base() { }
    public %(name)s(string msg) : base(msg) { }
    public %(name)s(string message, Exception innerException)
        : base(message, innerException) {
    }
#if !SILVERLIGHT // SerializationInfo
    protected %(name)s(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
}
"""

def gen_one_exception(cw, e):    
    supername = getattr(exceptions, e).__bases__[0].__name__
    if not supername in pythonExcs and supername != 'Warning':
        supername = ''
    cw.write(CLASS1, name=get_clr_name(e), supername=get_clr_name(supername))

def gen_one_exception_maker(e):
    def gen_one_exception_specialized(x):
        return gen_one_exception(x, e)

    return gen_one_exception_specialized

def fix_object(name):
    if name == "object": return "@object"
    return name

def gen_one_new_exception(cw, exception, parent):
    if exception.fields:
        exception.BeginSilverlight(cw)
        
        cw.writeline('[MultiRuntimeAware]')
        cw.writeline('private static PythonType %sStorage;' % (exception.name, ))
        cw.enter_block('public static PythonType %s' % (exception.name, ))
        cw.enter_block('get')
        cw.enter_block('if (%sStorage == null)' % (exception.name, ))
        cw.enter_block('lock (typeof(PythonExceptions))')
        cw.writeline('%sStorage = CreateSubType(%s, typeof(_%s));' % (exception.name, exception.parent.PythonType, exception.name))
        cw.exit_block() # lock
        cw.exit_block() # if
        cw.writeline('return %sStorage;' % (exception.name, ))
        cw.exit_block()
        cw.exit_block()
        cw.writeline()

        cw.writeline('[PythonType("%s"), PythonHidden, DynamicBaseTypeAttribute, Serializable]' % exception.name)
        if exception.ConcreteParent.fields:
            cw.enter_block('public partial class _%s : _%s' % (exception.name, exception.ConcreteParent.name))
        else:
            cw.enter_block('public partial class _%s : %s' % (exception.name, exception.ConcreteParent.name))
                
        for field in exception.fields:
            cw.writeline('private object _%s;' % field)
        
        if exception.fields:
            cw.writeline('')

        cw.writeline('public _%s() : base(%s) { }' % (exception.name, exception.name))
        cw.writeline('public _%s(PythonType type) : base(type) { }' % (exception.name, ))
        cw.writeline('')
        
        cw.enter_block('public new static object __new__(PythonType cls, params object[] args)')
        cw.writeline('return Activator.CreateInstance(cls.UnderlyingSystemType, cls);')
        cw.exit_block()
        cw.writeline('')

        if exception.args:        
            argstr = ', '.join(['object ' + fix_object(x) for x in exception.args])             
            cw.enter_block('public void __init__(%s)' % (argstr))
            for arg in exception.args:
                cw.writeline('_%s = %s;' % (arg, fix_object(arg)))
            cw.writeline('args = PythonTuple.MakeTuple(' + ', '.join([fix_object(x) for x in exception.args]) + ');')
            cw.exit_block()
            cw.writeline('')
            cw.enter_block('public override void __init__(params object[] args)')
            cw.enter_block('if (args == null || args.Length != %d)' % (len(exception.args), ))
            cw.writeline('throw PythonOps.TypeError("__init__ takes exactly %d arguments ({0} given)", args.Length);' % len(exception.args))
            cw.exit_block()
            cw.writeline('__init__(' + ', '.join([fix_object(x) for x in exception.args]) + ');')
            cw.exit_block()
            cw.writeline('')
        
        for field in exception.fields:
            cw.enter_block('public object %s' % fix_object(field))
            cw.writeline('get { return _%s; }' % field)
            cw.writeline('set { _%s = value; }' % field)
            cw.exit_block()
            cw.writeline('')
        
        cw.exit_block()
        cw.writeline('')

        exception.EndSilverlight(cw)
        
    else:
        cw.writeline('[MultiRuntimeAware]')
        cw.writeline('private static PythonType %sStorage;' % (exception.name, ))
        cw.enter_block('public static PythonType %s' % (exception.name, ))
        cw.enter_block('get')
        cw.enter_block('if (%sStorage == null)' % (exception.name, ))
        cw.enter_block('lock (typeof(PythonExceptions))')
        cw.writeline('%sStorage = CreateSubType(%s, "%s");' % (exception.name, exception.parent.PythonType, exception.name))
        cw.exit_block() # lock
        cw.exit_block() # if
        cw.writeline('return %sStorage;' % (exception.name, ))
        cw.exit_block()
        cw.exit_block()
        cw.writeline()
        
    for child in exception.subclasses:
        gen_one_new_exception(cw, child, exception)
            
def newstyle_gen(cw):
    for child in exceptionHierarchy.subclasses:
        gen_one_new_exception(cw, child, exceptionHierarchy)
        
def gen_one_exception_module_entry(cw, exception, parent):
    exception.BeginSilverlight(cw)

    cw.write("public static PythonType %s = %s;" % (exception.name, exception.InternalPythonType))

    exception.EndSilverlight(cw)

    for child in exception.subclasses:
        gen_one_exception_module_entry(cw, child, exception)
        
def module_gen(cw):
    cw.write("public static object BaseException = DynamicHelpers.GetPythonTypeFromType(typeof(PythonExceptions.BaseException));")
    
    for child in exceptionHierarchy.subclasses:
        gen_one_exception_module_entry(cw, child, exceptionHierarchy)

def gen_one_exception_builtin_entry(cw, exception, parent):
    exception.BeginSilverlight(cw)

    cw.enter_block("public static PythonType %s" % (exception.name, ))
    if exception.fields:
        cw.write('get { return %s; }' % (exception.InternalPythonType, ))
    else:
        cw.write('get { return %s; }' % (exception.InternalPythonType, ))
    cw.exit_block()

    exception.EndSilverlight(cw)

    for child in exception.subclasses:
        gen_one_exception_builtin_entry(cw, child, exception)

def builtin_gen(cw):
    for child in exceptionHierarchy.subclasses:
        gen_one_exception_builtin_entry(cw, child, exceptionHierarchy)

def main():
    gens = [
        ("ToPython Exception Helper", gen_topython_helper),
        ("Exception Factories", factory_gen),
        ("Python New-Style Exceptions", newstyle_gen),
        ("builtin exceptions", builtin_gen),
    ]

    for e in pythonExcs:
        gens.append((get_clr_name(e), gen_one_exception_maker(e)))

    return generate(*gens)

if __name__ == "__main__":
    main()
