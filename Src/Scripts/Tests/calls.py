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

def f(x=0, y=10, z=20, *args, **kws):
    return (x, y, z), args, kws

Assert(f(10, l=20) == ((10, 10, 20), (), {'l': 20}))

Assert(f(1, *(2,), **{'z':20}) == ((1, 2, 20), (), {}))

Assert(f(*[1,2,3]) == ((1, 2, 3), (), {}))

def a(*args, **kws): return args, kws

def b(*args, **kws):
    return a(*args, **kws)

Assert(b(1,2,3, x=10, y=20) == ((1, 2, 3), {'y': 20, 'x': 10}))

def b(*args, **kws):
    return a(**kws)

Assert(b(1,2,3, x=10, y=20) == ((), {'y': 20, 'x': 10}))

try:
    b(**[])
    Assert(False)
except TypeError:
    pass


#verify we can call sorted w/ keyword args

import operator
inventory = [('apple', 3), ('banana', 2), ('pear', 5), ('orange', 1)]
getcount = operator.itemgetter(1)

sorted_inventory = sorted(inventory, key=getcount)


# verify proper handling of keyword args for python functions
def kwfunc(a,b,c):  pass

try:
    kwfunc(10, 20, b=30)
    Assert(False)
except TypeError:
    pass

try:
    kwfunc(10, None, b=30)
    Assert(False)
except TypeError:
    pass


try:
    kwfunc(10, None, 40, b=30)
    Assert(False)
except TypeError:
    pass

if (sys.platform == "cli"):
    import System
    ht = System.Collections.Hashtable()
    def foo(**kwargs):
        return kwargs['key']
        
    ht['key'] = 'xyz'
    
    AreEqual(foo(**ht), 'xyz')


def foo(a,b):
    return a-b
    
AreEqual(foo(b=1, *(2,)), 1)

# kw-args passed to init through method instance

class foo:
    def __init__(self, group=None, target=None):
            AreEqual(group, None)
            AreEqual(target,'baz')

a = foo(target='baz')

foo.__init__(a, target='baz')


# call a params method w/ no params

if is_cli:
    import clr
    AreEqual('abc\ndef'.Split()[0], 'abc') 
    AreEqual('abc\ndef'.Split()[1], 'def')
    x = 'a bc   def'.Split()
    AreEqual(x[0], 'a')
    AreEqual(x[1], 'bc')
    AreEqual(x[2], '')
    AreEqual(x[3], '')
    AreEqual(x[4], 'def')
    
    # calling Double.ToString(...) should work - Double is
    # an OpsExtensibleType and doesn't define __str__ on this
    # overload
    
    AreEqual('1.00', System.Double.ToString(1.0, 'f'))

######################################################################################
# Incorrect number of arguments

def f(a): pass

AssertErrorWithMessage(TypeError, "f() takes exactly 1 argument (0 given)", f)
AssertErrorWithMessage(TypeError, "f() takes exactly 1 argument (3 given)", f, 1, 2, 3)
AssertErrorWithMessage(TypeError, "f() got an unexpected keyword argument 'dummy'", apply, f, [], dict({"dummy":2}))
AssertErrorWithMessage(TypeError, "f() got an unexpected keyword argument 'dummy'", apply, f, [1], dict({"dummy":2}))

def f(a,b,c,d,e,f,g,h,i,j): pass

AssertErrorWithMessage(TypeError, "f() takes exactly 10 arguments (0 given)", f)
AssertErrorWithMessage(TypeError, "f() takes exactly 10 arguments (3 given)", f, 1, 2, 3)
AssertErrorWithMessage(TypeError, "f() got an unexpected keyword argument 'dummy'", apply, f, [], dict({"dummy":2}))
AssertErrorWithMessage(TypeError, "f() got an unexpected keyword argument 'dummy'", apply, f, [1], dict({"dummy":2}))

def f(a, b=2): pass

AssertErrorWithMessage(TypeError, "f() takes at least 1 argument (0 given)", f)
AssertErrorWithMessage(TypeError, "f() takes at most 2 arguments (3 given)", f, 1, 2, 3)
AssertErrorWithMessage(TypeError, "f() takes at least 1 non-keyword argument (0 given)", apply, f, [], dict({"b":2}))
AssertErrorWithMessage(TypeError, "f() got an unexpected keyword argument 'dummy'", apply, f, [], dict({"dummy":3}))
AssertErrorWithMessage(TypeError, "f() got an unexpected keyword argument 'dummy'", apply, f, [], dict({"b":2, "dummy":3}))
AssertErrorWithMessage(TypeError, "f() got an unexpected keyword argument 'dummy'", apply, f, [1], dict({"dummy":3}))

def f(a, *argList): pass

AssertErrorWithMessage(TypeError, "f() takes at least 1 argument (0 given)", f)
AssertErrorWithMessage(TypeError, "f() got an unexpected keyword argument 'dummy'", apply, f, [], dict({"dummy":2}))
AssertErrorWithMessage(TypeError, "f() got an unexpected keyword argument 'dummy'", apply, f, [], dict({"dummy":2}))
AssertErrorWithMessage(TypeError, "f() got an unexpected keyword argument 'dummy'", apply, f, [1], dict({"dummy":2}))

def f(a, **keywordDict): pass

AssertErrorWithMessage(TypeError, "f() takes exactly 1 argument (0 given)", f)
AssertErrorWithMessage(TypeError, "f() takes exactly 1 argument (3 given)", f, 1, 2, 3)
AssertErrorWithMessage(TypeError, "f() takes exactly 1 non-keyword argument (0 given)", apply, f, [], dict({"dummy":2}))
AssertErrorWithMessage(TypeError, "f() takes exactly 1 non-keyword argument (0 given)", apply, f, [], dict({"dummy":3}))
AssertErrorWithMessage(TypeError, "f() takes exactly 1 non-keyword argument (0 given)", apply, f, [], dict({"dummy":2, "dummy2":3}))

AssertErrorWithMessages(TypeError, "abs() takes exactly 1 argument (0 given)",
                                   "abs() takes exactly one argument (0 given)",   abs)
AssertErrorWithMessages(TypeError, "abs() takes exactly 1 argument (3 given)",
                                   "abs() takes exactly one argument (3 given)",   abs, 1, 2, 3)
AssertErrorWithMessages(TypeError, "abs() got an unexpected keyword argument 'dummy'",
                                   "abs() takes no keyword arguments",             apply, abs, [], dict({"dummy":2}))
AssertErrorWithMessages(TypeError, "abs() got an unexpected keyword argument 'dummy'",
                                   "abs() takes no keyword arguments",             apply, abs, [1], dict({"dummy":2}))

# list([m]) has one default argument (built-in type)
#AssertErrorWithMessage(TypeError, "list() takes at most 1 argument (2 given)", list, 1, 2)
#AssertErrorWithMessage(TypeError, "'dummy' is an invalid keyword argument for this function", apply, list, [], dict({"dummy":2}))

#======== BUG 697 ===========
#AssertErrorWithMessage(TypeError, "'dummy' is an invalid keyword argument for this function", apply, list, [1], dict({"dummy":2}))

# complex([x,y]) has two default argument (OpsReflectedType type)
#AssertErrorWithMessage(TypeError, "complex() takes at most 2 arguments (3 given)", complex, 1, 2, 3)
#AssertErrorWithMessage(TypeError, "'dummy' is an invalid keyword argument for this function", apply, complex, [], dict({"dummy":2}))
#AssertErrorWithMessage(TypeError, "'dummy' is an invalid keyword argument for this function", apply, complex, [1], dict({"dummy":2}))

# bool([x]) has one default argument (OpsReflectedType and valuetype type)
#AssertErrorWithMessage(TypeError, "bool() takes at most 1 argument (2 given)", bool, 1, 2)
#AssertErrorWithMessage(TypeError, "'dummy' is an invalid keyword argument for this function", apply, bool, [], dict({"dummy":2}))
#AssertErrorWithMessage(TypeError, "'dummy' is an invalid keyword argument for this function", apply, bool, [1], dict({"dummy":2}))

#class UserClass(object): pass
#AssertErrorWithMessage(TypeError, "default __new__ takes no parameters", UserClass, 1)
#AssertErrorWithMessage(TypeError, "default __new__ takes no parameters", apply, UserClass, [], dict({"dummy":2}))
    
class OldStyleClass: pass
AssertErrorWithMessage(TypeError, "this constructor takes no arguments", OldStyleClass, 1)
AssertErrorWithMessage(TypeError, "this constructor takes no arguments", apply, OldStyleClass, [], dict({"dummy":2}))

###############################################################################################
# accepts / returns runtype type checking tests

if is_cli:
    from clr import *

    @accepts(object)
    def foo(x): 
        return x

    AreEqual(foo('abc'), 'abc')
    AreEqual(foo(2), 2)
    AreEqual(foo(2L), 2L)
    AreEqual(foo(2.0), 2.0)
    AreEqual(foo(True), True)


    @accepts(str)
    def foo(x):
        return x
        
    AreEqual(foo('abc'), 'abc')
    AssertError(AssertionError, foo, 2)
    AssertError(AssertionError, foo, 2L)
    AssertError(AssertionError, foo, 2.0)
    AssertError(AssertionError, foo, True)

    @accepts(str, bool)
    def foo(x, y):
        return x, y

    AreEqual(foo('abc', True), ('abc', True))
    AssertError(AssertionError, foo, ('abc',2))
    AssertError(AssertionError, foo, ('abc',2L))
    AssertError(AssertionError, foo, ('abc',2.0))


    class bar:  
        @accepts(Self(), str)
        def foo(self, x):
            return x


    a = bar()
    AreEqual(a.foo('xyz'), 'xyz')
    AssertError(AssertionError, a.foo, 2)
    AssertError(AssertionError, a.foo, 2L)
    AssertError(AssertionError, a.foo, 2.0)
    AssertError(AssertionError, a.foo, True)

    @returns(str)
    def foo(x):
        return x


    AreEqual(foo('abc'), 'abc')
    AssertError(AssertionError, foo, 2)
    AssertError(AssertionError, foo, 2L)
    AssertError(AssertionError, foo, 2.0)
    AssertError(AssertionError, foo, True)

    @accepts(bool)
    @returns(str)
    def foo(x):
        if x: return str(x)
        else: return 0
        
    AreEqual(foo(True), 'True')

    AssertError(AssertionError, foo, 2)
    AssertError(AssertionError, foo, 2)
    AssertError(AssertionError, foo, False)

    @returns(None)
    def foo(): pass

    AreEqual(foo(), None)

try:
    buffer()
except TypeError, e:
    # make sure we get the right type name when calling w/ wrong # of args
    AreEqual(str(e)[:8], 'buffer()')
    
#try:
#    list(1,2,3)
#except TypeError, e:
    # make sure we get the right type name when calling w/ wrong # of args
#    AreEqual(str(e)[:6], 'list()')    

# oldinstance
class foo:
    def bar(self): pass
    def bar1(self, xyz): pass
    
class foo2: pass
class foo3(object): pass

AssertError(TypeError, foo.bar)
AssertError(TypeError, foo.bar1, None, None)
AssertError(TypeError, foo.bar1, None, 'abc')
AssertError(TypeError, foo.bar1, 'xyz', 'abc')
AssertError(TypeError, foo.bar, foo2())
AssertError(TypeError, foo.bar, foo3())

# usertype
class foo(object):
    def bar(self): pass
    def bar1(self, xyz): pass

AssertError(TypeError, foo.bar)
AssertError(TypeError, foo.bar1, None, None)
AssertError(TypeError, foo.bar1, None, 'abc')
AssertError(TypeError, foo.bar1, 'xyz', 'abc')
AssertError(TypeError, foo.bar, foo2())
AssertError(TypeError, foo.bar, foo3())

# access a method w/ caller context w/ an args parameter.
def foo(*args):
    return hasattr(*args)
    
AreEqual(foo('', 'index'), True)

# dispatch to a ReflectOptimized method

if is_cli:
    from Util.Console import IronPythonInstance
    from System import Environment
    from sys import executable
    
    wkdir = '\\'.join(sys.executable.split('\\')[:-1])+'\\Src\\Scripts\\Tests'
    if "-X:GenerateAsSnippets" in Environment.GetCommandLineArgs():
	    ipi = IronPythonInstance(executable, wkdir, "-X:GenerateAsSnippets")
    else:
	    ipi = IronPythonInstance(executable, wkdir, "")

    if (ipi.Start()):
        result = ipi.ExecuteLine("from Util.Debug import *")
        result = ipi.ExecuteLine("load_iron_python_test()")
        result = ipi.ExecuteLine("from IronPythonTest import DefaultParams")
        response = ipi.ExecuteLine("DefaultParams.FuncWithDefaults(1100, z=82)")
        AreEqual(response, '1184')
        ipi.End()

p = ((1, 2),)

AreEqual(zip(*(p * 10)), [(1, 1, 1, 1, 1, 1, 1, 1, 1, 1), (2, 2, 2, 2, 2, 2, 2, 2, 2, 2)])
AreEqual(zip(*(p * 10)), [(1, 1, 1, 1, 1, 1, 1, 1, 1, 1), (2, 2, 2, 2, 2, 2, 2, 2, 2, 2)])




class A(object): pass

class B(A): pass

#unbound super
for x in [super(B), super(B,None)]:
    AreEqual(x.__thisclass__, B)
    AreEqual(x.__self__, None)
    AreEqual(x.__self_class__, None)

# super w/ both types
x = super(B,B)

AreEqual(x.__thisclass__,B)
AreEqual(x.__self_class__, B)
AreEqual(x.__self__, B)

# super w/ type and instance
b = B()
x = super(B, b)

AreEqual(x.__thisclass__,B)
AreEqual(x.__self_class__, B)
AreEqual(x.__self__, b)

# super w/ mixed types
x = super(A,B)
AreEqual(x.__thisclass__,A)
AreEqual(x.__self_class__, B)
AreEqual(x.__self__, B)

# invalid super cases
try:
    x = super(B, 'abc')
    AreEqual(True, False)
except TypeError:
    pass
    
try:
    super(B,A)
    AreEqual(True, False)
except TypeError:
    pass
    

class A(object):
    def __init__(self, name):
        self.__name__ = name
    def meth(self):
        return self.__name__
    classmeth = classmethod(meth)
    
class B(A): pass

b = B('derived')
AreEqual(super(B,b).__thisclass__.__name__, 'B')
AreEqual(super(B,b).__self__.__name__, 'derived')
AreEqual(super(B,b).__self_class__.__name__, 'B')

AreEqual(super(B,b).classmeth(), 'B')


# descriptor supper
class A(object):
    def meth(self): return 'A'
    
class B(A):
    def meth(self):    
        return 'B' + self.__super.meth()
        
B._B__super = super(B)
b = B()
AreEqual(b.meth(), 'BA')


#################################
# class method calls - class method should get
# correct meta class.

class D(object):
	@classmethod
	def classmeth(cls): pass
	
AreEqual(D.classmeth.im_class, type)

class MetaType(type): pass

class D(object):
	__metaclass__ = MetaType
	@classmethod
	def classmeth(cls): pass
	
AreEqual(D.classmeth.im_class, MetaType)