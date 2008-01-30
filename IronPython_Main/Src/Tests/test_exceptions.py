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
import sys

AreEqual(sys.exc_info(), (None, None, None))

if is_cli or is_silverlight:
    def test_system_exception():
        import System
        
        def RaiseSystemException():
            raise System.SystemException()

        AssertError(SystemError, RaiseSystemException)

    AreEqual(sys.exc_info(), (None, None, None))

if is_cli or is_silverlight:
    def test_raise():
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

def test_bigint_division():
    def divide(a, b):
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

def test_handlers():
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

def test_sys_exit1():
    try:
        sys.exit()
        Assert(False)
    except SystemExit, e:
        AreEqual(len(e.args), 0)

def test_sys_exit2():
    try:
        sys.exit(None)
        Assert(False)
    except SystemExit, e:
        AreEqual(e.args, ())

    AreEqual(SystemExit(None).args, (None,))

def test_sys_exit3():
    try:
        sys.exit(-10)
    except SystemExit, e:
        AreEqual(e.code, -10)
        AreEqual(e.args, (-10,))
    else:
        Assert(False)

################################
# exception interop tests
if is_cli or is_silverlight:
    def test_interop():
        load_iron_python_test()
        
        from IronPythonTest import ExceptionsTest
        import System
        import sys
        
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
        
        
        Assert(isinstance(x, System.ArgumentException))
        
        # call through the slow paths...
        
        try:
            a.CallVirtualOverloaded('abc')
        except ex,e:
            Assert(e.__class__ == ex)
            Assert(e.args[0] == "hello world")
        # Note that sys.exc_info() is still set
       
        try:
            a.CallVirtualOverloaded(5)
        except ex,e:
            Assert(e.__class__ == ex)
            Assert(e.args[0] == "hello world")
        
        
        
        try:
            a.CallVirtualOverloaded(a)
        except ex,e:
            Assert(e.__class__ == ex)
            Assert(e.args[0] == "hello world")
        
        
        # catch and re-throw (both throw again and rethrow)
        
        try:
            a.CatchAndRethrow()
        except ex,e:
            Assert(e.__class__ == ex)
            Assert(e.args[0] == "hello world")
        
        try:
            a.CatchAndRethrow2()
        except ex,e:
            Assert(e.__class__ == ex)
            Assert(e.args[0] == "hello world")
        
        
        
        class MyTest(ExceptionsTest):
            def VirtualFunc(self):
                self.ThrowException()
        
        a = MyTest()
        
        # start in python, call CLS which calls Python which calls CLS which raises the exception
        try:
            a.CallVirtual()  # throws index out of range
        except IndexError, e:
            Assert(e.__class__ == IndexError)
        
        
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
        
        # BUG 319 IOError not raised.
        if is_silverlight==False:
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

# TODO: doesn't work in IronPython
#def test_str1():
#    AssertErrorWithMessage(TypeError, "descriptor '__str__' of 'exceptions.BaseException' object needs an argument", Exception.__str__)
#    AssertErrorWithMessage(TypeError, "descriptor '__str__' requires a 'exceptions.BaseException' object but received a 'list'", Exception.__str__, list())
#    AssertErrorWithMessage(TypeError, "descriptor '__str__' requires a 'exceptions.BaseException' object but received a 'list'", Exception.__str__, list(), 1)
#    AssertErrorWithMessage(TypeError, "expected 0 arguments, got 1", Exception.__str__, Exception(), 1)

def test_str2():
    # verify we can assign to sys.exc_*
    sys.exc_traceback = None
    sys.exc_value = None
    sys.exc_type = None

    AreEqual(str(Exception()), '')

#####################################################################

if is_cli or is_silverlight:
    def test_array():
        import System
        try:
            a = System.Array()
        except Exception, e:
            AreEqual(e.__class__, TypeError)
        else: 
            Assert(False, "FAILED!")

def test_assert_error():
    AssertError(ValueError, chr, -1)
    AssertError(TypeError, None)

def test_dir():
    testingdir = 10
    Assert('testingdir' in dir())
    del testingdir
    Assert(not 'testingdir' in dir())

def test_assert():
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


def test_syntax_error_exception():
    try:
        compile("if 2==2: x=2\nelse:y=", "Error", "exec") 
    except SyntaxError, se:
        l1 = dir(se)
        Assert('lineno' in l1)
        Assert('offset' in l1)
        Assert('filename' in l1)
        Assert('text' in l1)
        if is_cli or is_silverlight:
            l2 = dir(se.clsException)            
            Assert('Line' in l2)
            Assert('Column' in l2)
            Assert('GetSymbolDocumentName' in l2)
            Assert('GetCodeLine' in l2)
        AreEqual(se.lineno, 2)
        # Bug 1132
        #AreEqual(se.offset, 7)
        AreEqual(se.filename, "Error") 
        AreEqual(se.text, "else:y=")
        if is_cli or is_silverlight:
            AreEqual(se.clsException.Line, 2)
            # Bug 1132
            #AreEqual(se.clsException.Column, 7)
            AreEqual(se.clsException.GetSymbolDocumentName(), "Error") 
            AreEqual(se.clsException.GetCodeLine(), "else:y=")
    
def test_syntax_error_exception_exec():
    try:
        compile("if 2==2: x=", "Error", "exec") 
    except SyntaxError, se:
        AreEqual(se.lineno, 1)
        # Bug 1132
        #AreEqual(se.offset, 11)
        AreEqual(se.filename, "Error")
        AreEqual(se.text, "if 2==2: x=")
        
def test_syntax_error_exception_eval():
    try:
        compile("if 2==2: x=", "Error", "eval")
    except SyntaxError, se:
        AreEqual(se.lineno, 1)
        # Bug 1132
        #AreEqual(se.offset, 2)
        AreEqual(se.filename, "Error")
        AreEqual(se.text, "if 2==2: x=")

def test_user_syntax_error_exception():
    x = SyntaxError()
    AreEqual(x.lineno, None)
    AreEqual(x.filename, None)
    AreEqual(x.msg, None)
    AreEqual(x.message, '')
    AreEqual(x.offset, None)
    AreEqual(x.print_file_and_line, None)
    AreEqual(x.text, None)    

    x = SyntaxError('hello')
    AreEqual(x.lineno, None)
    AreEqual(x.filename, None)
    AreEqual(x.msg, 'hello')
    AreEqual(x.message, 'hello')
    AreEqual(x.offset, None)
    AreEqual(x.print_file_and_line, None)
    AreEqual(x.text, None)    
    
    x = SyntaxError('hello', (1,2,3,4))
    AreEqual(x.lineno, 2)
    AreEqual(x.filename, 1)
    AreEqual(x.msg, 'hello')
    AreEqual(x.message, '')
    AreEqual(x.offset, 3)
    AreEqual(x.print_file_and_line, None)
    AreEqual(x.text, 4) 
    
    AssertError(IndexError, SyntaxError, 'abc', ())
    AssertError(IndexError, SyntaxError, 'abc', (1,))
    AssertError(IndexError, SyntaxError, 'abc', (1,2))
    AssertError(IndexError, SyntaxError, 'abc', (1,2,3))
    
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

def test_serializable_clionly():
    import clr
    import System
    from IronPythonTest import ExceptionsTest
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
        except_list = [exception_type(), exception_type("a single param")]
        
        for t_except in except_list:
            try:
                raise t_except
            except exception_type, e:
                pass
            
            str_except = str(t_except)
            
            #there is no __getstate__ method of exceptions...
            Assert(not hasattr(t_except, '__getstate__'))
    
    if not is_silverlight:
        #special cases
        exceptions.UnicodeEncodeError("1", u"2", 3, 4, "e")
        #CodePlex Work Item 356
        #AssertError(TypeError, exceptions.UnicodeDecodeError, "1", u"2", 3, 4, "e")
        exceptions.UnicodeDecodeError("1", "2", 3, 4, "e")

def test_nested_exceptions():
    try:
        raise Exception()
    except Exception, e:
        # PushException
        try:
            raise TypeError
        except TypeError, te:
            # PushException
            ei = sys.exc_info()
            # PopException
        ei2 = sys.exc_info()
        AreEqual(ei, ei2)
    ei3 = sys.exc_info()
    AreEqual(ei, ei3)

def test_swallow_from_else():
    def f():
        try:
            pass
        except:
            pass
        else:
            raise AttributeError
        finally:
            return 4
            
    AreEqual(f(), 4)

def test_newstyle_raise():
    # raise a new style exception via raise type, value that returns an arbitrary object
    class MyException(Exception):
        def __new__(cls, *args): return 42
        
    try:
        raise MyException, 'abc'
        AssertUnreachable()
    except Exception, e:
        AreEqual(e, 42)

def test_enverror_init():
    x = EnvironmentError()
    AreEqual(x.message, '')
    AreEqual(x.args, ())
    
    x.__init__('abc')
    AreEqual(x.message, 'abc')
    AreEqual(x.args, ('abc', ))
    
    x.__init__('123', '456')
    AreEqual(x.message, 'abc')
    AreEqual(x.errno, '123')
    AreEqual(x.strerror, '456')    
    AreEqual(x.args, ('123', '456'))
    
    x.__init__('def', 'qrt', 'foo')
    AreEqual(x.message, 'abc')
    AreEqual(x.errno, 'def')
    AreEqual(x.strerror, 'qrt')
    AreEqual(x.filename, 'foo')
    AreEqual(x.args, ('def', 'qrt')) # filename not included in args
    
    x.__init__()
    AreEqual(x.message, 'abc')
    AreEqual(x.errno, 'def')
    AreEqual(x.strerror, 'qrt')
    AreEqual(x.filename, 'foo')
    AreEqual(x.args, ())

    AssertError(TypeError, x.__init__, '1', '2', '3', '4')

    # OSError doesn't override __init__, message should be EnvError
    AssertErrorWithPartialMessage(TypeError, "EnvironmentError", OSError, '1', '2', '3', '4')
    
run_test(__name__)
