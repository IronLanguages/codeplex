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

from lib.assert_util import *
import sys
AreEqual(sys.exc_info(), (None, None, None))

if is_cli:
    import System
    
    def RaiseSystemException():
        raise System.SystemException()

    AssertError(SystemError, RaiseSystemException)

AreEqual(sys.exc_info(), (None, None, None))

try:
    def Raise1():
        raise "some string"

    try:
        Raise1()
        Assert(False, "FAILED! Should have thrown string!")
    except Exception, e:
        Assert(False, "FAILED! Should not have caught string!")
except "some string":
    pass

try:
    def Raise2():
        raise "some string"
    try:
        Raise2()
        Assert(False, "FAILED! Should have caught string!")
    except "some string":
        pass
except Exception, e:
    Assert(False, "FAILED!")

try:
     Fail("Message")
except AssertionError, e:
     AreEqual(e.__str__(), e.args[0])
else:
    Fail("Expected exception")

try:
    def Raise3():
        raise "some string"
    try:
        Raise3()
        Assert(False, "FAILED! Should have thrown string!")
    except "NOT some string":
        Assert(False, "FAILED! Should not have caught this string!")
except "some string":
    pass

def divide(a, b) :
    s = 0
    try:
        c = a / b
    except ZeroDivisionError:
        s = 1
    Assert(s == 1)
    s = 0
    try:
        c = a % b
    except ZeroDivisionError:
        s = 1
    Assert(s == 1)
    s = 0
    try:
        c = a // b
    except ZeroDivisionError:
        s = 1
    Assert(s == 1)


big0 = 9999999999999999999999999999999999999999999999999999999999999999999999
big0 = big0-big0

pats = [0L, 0, 0.0, big0, (0+0j)]
nums = [42, 987654321, 7698736985726395723649587263984756239847562983745692837465928374569283746592837465923, 2352345324523532523, 5223523.3453, (10+25j)]

for divisor in pats:
    for number in nums:
        divide(number, divisor)


# sys.exit() test


handlers = []

def a():
    try:
        b()
    finally:
        handlers.append("finally a")

def b():
    try:
        c()
    finally:
        handlers.append("finally b")

def c():
    try:
        d()
    finally:
        handlers.append("finally c")

def d():
    sys.exit("abnormal termination")

try:
    a()
except SystemExit, e:
    handlers.append(e.args[0])

Assert(handlers == ["finally c", "finally b", "finally a", "abnormal termination"])

try:
    sys.exit()
    Assert(False)
except Exception, e:
    AreEqual(len(e.args), 0)

try:
    sys.exit(None)
    Assert(False)
except Exception, e:
    AreEqual(e[0], None)

try:
    sys.exit(-10)
except Exception, e:
    AreEqual(e.code, -10)
    AreEqual(e.args, (-10,))
else:
    Assert(False)

################################
# exception interop tests

load_iron_python_test()

from IronPythonTest import *
import System

a = ExceptionsTest()

try:
    a.ThrowException()  # throws index out of range
except IndexError, e:
    Assert(e.__class__ == IndexError)

class MyTest(ExceptionsTest):
    def VirtualFunc(self):
        raise ex, "hello world"


ex = ValueError


a = MyTest()

# raise in python, translate into .NET, catch in Python
try:
    a.CallVirtual()
except ex, e:
    Assert(e.__class__ == ValueError)
    Assert(e.args[0] == "hello world")

# raise in python, catch in .NET, verify .NET got an ArgumentException

try:
    x = a.CallVirtCatch()
except ex, e:
    Assert(False)

AreEqual(sys.exc_info(), (None, None, None))

Assert(isinstance(x, System.ArgumentException))

# call through the slow paths...

try:
    a.CallVirtualOverloaded('abc')
except ex,e:
    Assert(e.__class__ == ex)
    Assert(e.args[0] == "hello world")

AreEqual(sys.exc_info(), (None, None, None))

try:
    a.CallVirtualOverloaded(5)
except ex,e:
    Assert(e.__class__ == ex)
    Assert(e.args[0] == "hello world")


AreEqual(sys.exc_info(), (None, None, None))

try:
    a.CallVirtualOverloaded(a)
except ex,e:
    Assert(e.__class__ == ex)
    Assert(e.args[0] == "hello world")


AreEqual(sys.exc_info(), (None, None, None))
# catch and re-throw (both throw again and rethrow)

try:
    a.CatchAndRethrow()
except ex,e:
    Assert(e.__class__ == ex)
    Assert(e.args[0] == "hello world")
AreEqual(sys.exc_info(), (None, None, None))

try:
    a.CatchAndRethrow2()
except ex,e:
    Assert(e.__class__ == ex)
    Assert(e.args[0] == "hello world")

AreEqual(sys.exc_info(), (None, None, None))


class MyTest(ExceptionsTest):
    def VirtualFunc(self):
        self.ThrowException()

AreEqual(sys.exc_info(), (None, None, None))
a = MyTest()

# start in python, call CLS which calls Python which calls CLS which raises the exception
try:
    a.CallVirtual()  # throws index out of range
except IndexError, e:
    Assert(e.__class__ == IndexError)


AreEqual(sys.exc_info(), (None, None, None))
# verify we can throw arbitrary classes
class MyClass: pass

try:
    raise MyClass
    Assert(False)
except MyClass, mc:
    Assert(mc.__class__ == MyClass)

# BUG 430 intern(None) should throw TypeError
try:
    intern(None)
    Assert(False)
except TypeError:
    pass
# /BUG

AreEqual(sys.exc_info(), (None, None, None))

# BUG 424 except "string", <data>
try:
    raise "foo", 32
except "foo", X:
    Assert(X == 32)
# /BUG

# BUG 393 exceptions throw when bad value passed to except
try:
    try:
        raise SyntaxError("foo")
    except 12:
        Assert(false)
        pass
except SyntaxError:
    pass
# /BUG

AreEqual(sys.exc_info(), (None, None, None))
# BUG 319 IOError not raised.
try:
    fp = file('thisfiledoesnotexistatall.txt')
except IOError:
    pass
# /BUG

# verify we can raise & catch CLR exceptions
try:
    raise System.Exception('Hello World')
except System.Exception, e:
    Assert(type(e) == System.Exception)
AreEqual(sys.exc_info(), (None, None, None))



# BUG 481 Trying to pass raise in Traceback should cause an error until it is implemented
try:
    raise "BadTraceback", "somedata", "a string is not a traceback"
    Assert (false, "fell through raise for some reason")
except "BadTraceback":
    Assert(false)
except TypeError:
    pass

try:
    raise TypeError
except:
    import sys
    if (sys.exc_traceback != None):
        x = dir(sys.exc_traceback)
        x.sort()
        AreEqual(x,  ['tb_frame', 'tb_lasti', 'tb_lineno', 'tb_next'])
        try:
            raise "foo", "Msg", sys.exc_traceback
        except "foo", X:
            pass

                  

try:
    raise Exception(3,4,5)
except Exception, X:
    AreEqual(X[0], 3)
    AreEqual(X[1], 4)
    AreEqual(X[2], 5)


try:
    raise Exception
except:
    import exceptions
    AreEqual(sys.exc_info()[0], exceptions.Exception)    
    AreEqual(sys.exc_info()[1].__class__, exceptions.Exception)
    
try:
    Fail("message")
except AssertionError, e:
    import exceptions
    
    AreEqual(e.__class__, exceptions.AssertionError)
    AreEqual(len(e.args), 1)
    AreEqual(e.args[0], "message")
else:
    Fail("Expected exception")

#####################################################################################
# __str__ behaves differently for exceptions because of implementation (ExceptionConverter.ExceptionToString)

import re
AssertErrorWithMatch(TypeError, re.escape("unbound method __str__() must be called with <type 'Exception'> instance as first argument (got ") + "*", Exception.__str__)
AssertErrorWithMatch(TypeError, re.escape("unbound method __str__() must be called with <type 'Exception'> instance as first argument (got ") + "*", Exception.__str__, list())
AssertErrorWithMessage(TypeError, "__str__() takes exactly 1 argument (2 given)", Exception.__str__, Exception(), 1)
AssertErrorWithMatch(TypeError, re.escape("unbound method __str__() must be called with <type 'Exception'> instance as first argument (got ") + "*", Exception.__str__, list(), 1)


# verify we can assign to sys.exc_*
sys.exc_traceback = None
sys.exc_value = None
sys.exc_type = None


AreEqual(str(Exception()), '')

#####################################################################

if is_cli:
    import System
    try:
        a = System.Array()
    except Exception, e:
        AreEqual(e.__class__, TypeError)
    else: 
        Assert(False, "FAILED!")

AssertError(ValueError, chr, -1)
AssertError(TypeError, None)

testingdir = 10
Assert('testingdir' in dir())
del testingdir
Assert(not 'testingdir' in dir())

try:
    Assert(False, "Failed message")
except AssertionError, e:
    Assert(e.args[0] == "Failed message")
else: 
    Fail("should have thrown")

try:
    Assert(False, "Failed message 2")
except AssertionError, e:
    Assert(e.args[0] == "Failed message 2")
else: 
    Fail("should have thrown")
