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

import generate
reload(generate)
from generate import CodeGenerator, CodeWriter

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
              'OverflowWarning', 'FutureWarning',
              'AssertionError', 'RuntimeWarning',
              'UserWarning', 'SyntaxWarning', 'Warning']

silverlight_unsupported = ["WindowsError", "UnicodeEncodeError", "UnicodeDecodeError"]
        
        
FACTORY = """
public static Exception %(name)s(string format, params object[] args) {
    return new Python%(name)sException(string.Format(format, args));
}"""

def factory_gen(cw):
    for e in pythonExcs:
        cw.write(FACTORY, name=e)


CodeGenerator("Exception Factories", factory_gen).doit()

CLASS1 = """
[PythonType("%(name)s")]
[Serializable]
public class Python%(name)sException : %(supername)sException {
    public Python%(name)sException() : base() { }
    public Python%(name)sException(string msg) : base(msg) { }
    public Python%(name)sException(string message, Exception innerException)
        : base(message, innerException) {
    }
#if !SILVERLIGHT // SerializationInfo
    protected Python%(name)sException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
}
"""


def factory_gen(cw):
    for e in pythonExcs:
        supername = getattr(exceptions, e).__bases__[0].__name__
        if not supername in pythonExcs:
            supername = ''
        else:
            supername = 'Python' + supername 

        cw.write(CLASS1, name=e, supername=supername)


CodeGenerator("PythonException Classes", factory_gen).doit()

def builtin_gen(cw):
    for e, supername in excs:        
        
        if e.__name__ in silverlight_unsupported:
            cw.write("#if !SILVERLIGHT");
        
        cw.write("public static object %s = ExceptionConverter.GetPythonException(\"%s\");" %
                 (e.__name__, e.__name__))
                 
        if e.__name__ in silverlight_unsupported:
            cw.write("#endif");

CodeGenerator("builtin exceptions", builtin_gen).doit()



def excep_module_gen(cw):
    cw.write("public static object Exception = ExceptionConverter.GetPythonException(\"Exception\");")
    for e, super in excs:
        if e.__name__ in silverlight_unsupported:
            cw.write("#if !SILVERLIGHT");

        cw.write("public static object %s = ExceptionConverter.GetPythonException(\"%s\");" % (e.__name__, e.__name__))
        
        if e.__name__ in silverlight_unsupported:
            cw.write("#endif");
        
CodeGenerator("Exceptions Module", excep_module_gen).doit()
