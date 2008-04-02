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

from lib.assert_util import *
import sys
AreEqual(sys.exc_info(), (None, None, None))

if is_cli:
    import System
    
    def RaiseSystemException():
        raise System.SystemException()

    AssertError(SystemError, RaiseSystemException)

AreEqual(sys.exc_info(), (None, None, None))

if is_cli:
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
    try:
        c = a / b
        Fail("Expected ZeroDivisionError for %r / %r == %r" % (a, b, c))
    except ZeroDivisionError:
        pass

    try:
        c = a % b
        Fail("Expected ZeroDivisionError for %r %% %r == %r" % (a, b, c))
    except ZeroDivisionError:
        pass

    try:
        c = a // b
        Fail("Expected ZeroDivisionError for %r // %r == %r" % (a, b, c))
    except ZeroDivisionError:
        pass


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
    AreEqual(e.args, ())

AreEqual(SystemExit(None).args, (None,))

try:
    sys.exit(-10)
except Exception, e:
    AreEqual(e.code, -10)
    AreEqual(e.args, (-10,))
else:
    Assert(False)

################################
# exception interop tests

if is_cli:
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
            Assert(set(x) > set(['tb_frame', 'tb_lasti', 'tb_lineno', 'tb_next']))
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
AssertErrorWithMatch(TypeError, re.escape("unbound method __str__() must be called with Exception instance as first argument (got ") + "*", Exception.__str__)
AssertErrorWithMatch(TypeError, re.escape("unbound method __str__() must be called with Exception instance as first argument (got ") + "*", Exception.__str__, list())
AssertErrorWithMessage(TypeError, "__str__() takes exactly 1 argument (2 given)", Exception.__str__, Exception(), 1)
AssertErrorWithMatch(TypeError, re.escape("unbound method __str__() must be called with Exception instance as first argument (got ") + "*", Exception.__str__, list(), 1)

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

def test_methsonexcepobject():
    try:
        compile("if 2==2: x=2\nelse:y=", "Error", "exec") 
    except SyntaxError, se:
        l1 = dir(se)
        Assert('lineno' in l1)
        Assert('offset' in l1)
        Assert('filename' in l1)
        Assert('text' in l1)
        if( sys.platform == 'cli' ):
            l2 = dir(se.clsException)
            Assert('lineno' in l2)
            Assert('offset' in l2)
            Assert('filename' in l2)
            Assert('text' in l2)
        AreEqual(se.lineno, 2)
        # Bug 1132
        #AreEqual(se.offset, 7)
        AreEqual(se.filename, "Error") 
        AreEqual(se.text, "else:y=")
        if is_cli:
            AreEqual(se.clsException.lineno, 2)
            # Bug 1132
            #AreEqual(se.clsException.offset, 7)
            AreEqual(se.clsException.filename, "Error") 
            AreEqual(se.clsException.text, "else:y=")
    
    try:
        compile("if 2==2: x=", "Error", "exec") 
    except SyntaxError, se:
        AreEqual(se.lineno, 1)
        # Bug 1132
        #AreEqual(se.offset, 11)
        AreEqual(se.filename, "Error")
        AreEqual(se.text, "if 2==2: x=")
        if is_cli:
            AreEqual(se.clsException.lineno, 1)
            # Bug 1132
            #AreEqual(se.clsException.offset, 11)
            AreEqual(se.clsException.filename, "Error")
            AreEqual(se.clsException.text, "if 2==2: x=")
        
    try:
        compile("if 2==2: x=", "Error", "eval")
    except SyntaxError, se:
        AreEqual(se.lineno, 1)
        # Bug 1132
        #AreEqual(se.offset, 2)
        AreEqual(se.filename, "Error")
        AreEqual(se.text, "if 2==2: x=")
        if is_cli:
            AreEqual(se.clsException.lineno, 1)
            # Bug 1132
            #AreEqual(se.clsException.offset, 2)
            AreEqual(se.clsException.filename, "Error")
            AreEqual(se.clsException.text, "if 2==2: x=")

def test_return():    
    def test_func():
        try: pass
        finally:
            try: raise 'foo'
            except:
                return 42
                
    AreEqual(test_func(), 42)
            
    def test_func():
        try: pass
        finally:
            try: raise 'foo'
            except:
                try: raise 'foo'
                except:
                    return 42            

    AreEqual(test_func(), 42)
    
    def test_func():
        try: pass
        finally:
            try: pass
            finally:
                try: raise 'foo'
                except:
                    try: raise 'foo'
                    except:
                        return 42            

    AreEqual(test_func(), 42)

    def test_func():
        try: raise 'foo'
        except:
            try: pass
            finally:
                try: raise 'foo'
                except:
                    try: raise 'foo'
                    except:
                        return 42            

    AreEqual(test_func(), 42)

def test_break_and_continue():
    class stateobj(object):
        __slots__ = ['loops', 'finallyCalled']
        def __init__(self):
            self.loops = 0
            self.finallyCalled = False
            
    def test_break(state):
        try:
            try:
                raise Exception()
            except:
                for n in range(10): 
                    state.loops += 1
                    break
            return 42
        except: pass
    
    
    def test_continue(state):
        try:
            try:
                raise Exception()
            except:
                for n in range(10): 
                    state.loops += 1
                    continue
            return 42
        except: pass
        
    
    
    def test_multi_break(state):
        try:
            try:
                raise Exception()
            except:
                for n in range(10):
                    state.loops += 1
                    if False: break
                    
                    break
    
            return 42
        except: pass
        
        
    def test_multi_continue(state):
        try:
            try:
                raise Exception()
            except:
                for n in range(10): 
                    state.loops += 1
                    if False: continue
                    
                    continue
    
            return 42
        except: pass
            
    state = stateobj()
    AreEqual(test_break(state), 42)
    AreEqual(state.loops, 1)
    
    state = stateobj()
    AreEqual(test_continue(state), 42)
    AreEqual(state.loops, 10)
    
    state = stateobj()
    AreEqual(test_multi_break(state), 42)
    AreEqual(state.loops, 1)
    
    state = stateobj()
    AreEqual(test_multi_continue(state), 42)
    AreEqual(state.loops, 10)
    
    def test_break_in_finally_raise(state):
        for x in range(10):
            try:
                raise 'foo'
            finally:
                state.finallyCalled = True
                break
        return 42

    def test_break_in_finally(state):        
        for x in range(10):
            try: pass
            finally:
                state.finallyCalled = True
                break
        return 42

    state = stateobj()
    AreEqual(test_break_in_finally_raise(state), 42)
    AreEqual(state.finallyCalled, True)
    
    state = stateobj()
    AreEqual(test_break_in_finally(state), 42)
    AreEqual(state.finallyCalled, True)

    def test_outer_for_with_finally(state, shouldRaise):
        for x in range(10):
            try:
                try: 
                    if shouldRaise:
                        raise 'hello world'
                finally:
                    state.finallyCalled = True
                    break
            except:
                pass
            raise 'bad!!!'
        return 42
        
    state = stateobj()
    AreEqual(test_outer_for_with_finally(state, False), 42)
    AreEqual(state.finallyCalled, True)
        
    state = stateobj()
    AreEqual(test_outer_for_with_finally(state, True), 42)
    AreEqual(state.finallyCalled, True)
    
    def test_outer_for_with_finally(state, shouldRaise):
        for x in range(10):
            try:
                try: 
                    if shouldRaise:
                        raise 'hello world'
                finally:
                    state.finallyCalled = True
                    break
            except:
                pass
            raise 'bad!!!'
        return 42
    
    state = stateobj()
    AreEqual(test_outer_for_with_finally(state, False), 42)
    AreEqual(state.finallyCalled, True)
        
    state = stateobj()
    AreEqual(test_outer_for_with_finally(state, True), 42)
    AreEqual(state.finallyCalled, True)

@skip("win32")
def test_throw_from_compiled():
    def bar(): return 1 + 'abc'
    unique_string = "<this is unique string>"
    c = compile('bar()', unique_string, 'single')
    
    try:    eval(c)
    except: x= sys.exc_info()
    Assert(unique_string in str(x[1].clsException))

@skip("win32")
def test_serializable():
    import clr        
    import System
    path = clr.GetClrType(ExceptionsTest).Assembly.Location
    mbro = System.AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(path, "IronPythonTest.EngineTest")
    AssertError(AssertionError, mbro.Run, 'raise AssertionError')
    
def test_sanity():
    '''
    Sanity checks to ensure all exceptions implemented can be created/thrown/etc
    in the standard ways.
    '''
    #build up a list of all valid exceptions
    import exceptions
    #special cases - do not test these like everything else
    special_types = [ "UnicodeTranslateError", "UnicodeEncodeError", "UnicodeDecodeError"]
    exception_types = [ x for x in exceptions.__dict__.keys() if x.startswith("__")==False and special_types.count(x)==0]
    exception_types = [ eval("exceptions." + x) for x in exception_types]
    
    #run a few sanity checks
    for exception_type in exception_types:
        except_list = [exception_type(), exception_type("a single param"), exception_type("a single param", "another param")]
        
        for t_except in except_list:
            try:
                raise t_except
            except exception_type, e:
                pass
            
            str_except = str(t_except)
            
            #there is no __getstate__ method of CPython exceptions...
            if is_cli:
                t_except.__getstate__()
    
    #special cases
    exceptions.UnicodeEncodeError("1", u"2", 3, 4, "e")
    #CodePlex Work Item 356
    #AssertError(TypeError, exceptions.UnicodeDecodeError, "1", u"2", 3, 4, "e")
    exceptions.UnicodeDecodeError("1", "2", 3, 4, "e")
        
run_test(__name__)