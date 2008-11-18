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

from iptest.assert_util import *
import sys
if not is_silverlight:
    from iptest.process_util import *

year = 2005
month = 3
day = 16
hour = 14
minute = 53
second = 24

if 1900 < year < 2100 and 1 <= month <= 12 \
   and 1 <= day <= 31 and 0 <= hour < 24 \
   and 0 <= minute < 60 and 0 <= second < 60:   # Looks like a valid date
        pass

# Testing the (expr) support

x = 10
AreEqual(x, 10)
del x
try: y = x
except NameError: pass
else: Fail("x not deleted")

(x) = 20
AreEqual((x), 20)
del (x)
try: y = x
except NameError: pass
else: Fail("x not deleted")

# this is comment \
a=10
AreEqual(a, 10)

x = "Th\
\
e \
qu\
ick\
 br\
ow\
\
n \
fo\
\
x\
 ju\
mp\
s \
ove\
\
r \
th\
e l\
az\
\
y d\
og\
.\
\
 \
\
12\
34\
567\
89\
0"

y="\
The\
 q\
ui\
\
c\
k b\
\
r\
o\
w\
n\
 \
fo\
x\
 \
jum\
ps\
 ov\
er \
t\
he\
 la\
\
\
zy\
\
\
 d\
og\
. 1\
2\
\
3\
\
\
\
\
4\
567\
\
8\
\
90\
"

AreEqual(x, y)

AreEqual("\101", "A")
x='\a\b\c\d\e\f\g\h\i\j\k\l\m\n\o\p\q\r\s\t\u\v\w\y\z'
y=u'\u0007\u0008\\\u0063\\\u0064\\\u0065\u000C\\\u0067\\\u0068\\\u0069\\\u006a\\\u006b\\\u006c\\\u006d\u000A\\\u006f\\\u0070\\\u0071\u000D\\\u0073\u0009\\\u0075\u000B\\\u0077\\\u0079\\\u007a'

Assert(x == y)
AreEqual(x, y)

for a,b in zip(x,y):
    AreEqual(a,b)

Assert((10==20)==(20==10))
AreEqual(10==20, 20==10)
AreEqual(4e4-4, 4e4 - 4)

c = compile("071 + 1", "Error", "eval")

AssertError(SyntaxError, compile, "088 + 1", "Error", "eval")
AssertError(SyntaxError, compile, "099 + 1", "Error", "eval")
AssertError(SyntaxError, compile, """
try:
    pass
""", "Error", "single")

AssertError(SyntaxError, compile, "x=10\ny=x.", "Error", "exec")

def run_compile_test(code, msg, lineno):
    filename = "the file name"
    try:
        compile(code, filename, "exec")
    except SyntaxError, e:
        AreEqual(e.msg, msg)
        AreEqual(e.lineno, lineno)
        AreEqual(e.filename, filename)
    else:
        Assert(False, "Expected exception, got none")

compile_tests = [
    ("for x notin []:\n    pass", "unexpected token 'notin'", 1),
    ("global 1", "unexpected token '1'", 1),
    ("x=10\nyield x\n", "'yield' outside function", 2),
    ("return\n", "'return' outside function", 1),
    ("print >> 1 ,\n", "unexpected token '<eof>'", 1),
    ("def f(x=10, y):\n    pass", "default value must be specified here", 1),
    ("def f(for):\n    pass", "unexpected token 'for'", 1),
    ("f(3 = )", "expected name", 1),
    ("dict(a=1,a=2)", "duplicate keyword argument", 1),
    ("def f(a,a): pass", "duplicate argument 'a' in function definition", 1),
    ("def f((a,b),(c,b)): pass", "duplicate argument 'b' in function definition", 1),
    ("x = 10\nx = x[]", "unexpected token ']'", 2),
    ("None = 2", "assignment to None", 1),
    ("break", "'break' outside loop", 1),
    ("if 1:\n\tbreak", "'break' outside loop", 2),
    ("if 1:\n\tx+y=22", "can't assign to BinaryExpression", 2),
    ("if 1:\n\tdel f()", "can't delete function call", 2),
    ("def a(x):\n    def b():\n        print x\n    del x", "can not delete variable 'x' referenced in nested scope", 2),
    ("if 1:\nfoo()\n", "expected an indented block", 2),
    ("'abc'.1", "invalid syntax", 1),
    ("'abc'.1L", "invalid syntax", 1),
    ("'abc'.1j", "invalid syntax", 1),
    ("'abc'.0xFFFF", "invalid syntax", 1),
    ("'abc' 1L", "invalid syntax", 1),
    ("'abc' 1.0", "invalid syntax", 1),
    ("'abc' 0j", "invalid syntax", 1),
    ("x = 'abc'\nx.1", "invalid syntax", 2),
    ("x = 'abc'\nx 1L", "invalid syntax", 2),
    ("x = 'abc'\nx 1.0", "invalid syntax", 2),
    ("x = 'abc'\nx 0j", "invalid syntax", 2),
    ('def f():\n    del (yield 5)\n', "can't delete yield expression", 2),
    ('a,b,c += 1,2,3', "illegal expression for augmented assignment", 1),
    ('def f():\n    a = yield 3 = yield 4', "can't assign to yield expression", 2),
    ('((yield a), 2,3) = (2,3,4)', "can't assign to yield expression", 1),
    ('(2,3) = (3,4)', "can't assign to literal", 1),
    ('from import abc', "invalid syntax", 1),
    ("""for x in range(100):\n"""
     """    try:\n"""
     """        [1,2][3]\n"""
     """    except IndexError:\n"""
     """        pass\n"""
     """    finally:\n"""
     """        continue\n""", "'continue' not supported inside 'finally' clause", 7)

    #CodePlex 15428
    #("'abc'.", "invalid syntax", 1),
]

if is_cli:
    # different error messages, ok
    for test in compile_tests:
        run_compile_test(*test)
        
AreEqual(float(repr(2.5)), 2.5)

AreEqual(eval("1, 2, 3,"), (1, 2, 3))

# eval validates end of input
AssertError(SyntaxError, compile, "1+2 1", "Error", "eval")

# empty test list in for expression
AssertError(SyntaxError, compile, "for x in : print x", "Error", "exec")
AssertError(SyntaxError, compile, "for x in : print x", "Error", "eval")
AssertError(SyntaxError, compile, "for x in : print x", "Error", "single")

# empty backquote
AssertError(SyntaxError, compile, "``", "Error", "exec")
AssertError(SyntaxError, compile, "``", "Error", "eval")
AssertError(SyntaxError, compile, "``", "Error", "single")

# empty assignment expressions
AssertError(SyntaxError, compile, "x = ", "Error", "exec")
AssertError(SyntaxError, compile, "x = ", "Error", "eval")
AssertError(SyntaxError, compile, "x = ", "Error", "single")
AssertError(SyntaxError, compile, "x = y = ", "Error", "exec")
AssertError(SyntaxError, compile, "x = y = ", "Error", "eval")
AssertError(SyntaxError, compile, "x = y = ", "Error", "single")
AssertError(SyntaxError, compile, " = ", "Error", "exec")
AssertError(SyntaxError, compile, " = ", "Error", "eval")
AssertError(SyntaxError, compile, " = ", "Error", "single")
AssertError(SyntaxError, compile, " = 4", "Error", "exec")
AssertError(SyntaxError, compile, " = 4", "Error", "eval")
AssertError(SyntaxError, compile, " = 4", "Error", "single")
AssertError(SyntaxError, compile, "x <= ", "Error", "exec")
AssertError(SyntaxError, compile, "x <= ", "Error", "eval")
AssertError(SyntaxError, compile, "x <= ", "Error", "single")

#indentation errors - BUG 864
AssertError(IndentationError, compile, "class C:\nx=2\n", "Error", "exec")
AssertError(IndentationError, compile, "class C:\n\n", "Error", "single")

#allow \f
compile('\f\f\f\f\fclass C:\f\f\f pass', 'ok', 'exec')
compile('\f\f\f\f\fclass C:\n\f\f\f    print "hello"\n\f\f\f\f\f\f\f\f\f\f    print "goodbye"', 'ok', 'exec')
compile('class C:\n\f\f\f    print "hello"\n\f\f\f\f\f\f\f\f\f\f    print "goodbye"', 'ok', 'exec')
compile('class \f\f\f\fC:\n\f    print "hello"\n\f\f\f\f\f\f\f\f\f\f    print "goodbye"', 'ok', 'exec')

# multiline expression passed to exec (positive test)
s = """
title = "The Cat"
Assert(title.istitle())
x = 2 + 5
AreEqual(x, 7)
"""
exec s

if is_cli:
    # CPython is complaining about the last \t; we think this is ok
    x = compile("def f(a):\n\treturn a\n\t", "", "single")

# Assignment to None and constant

def NoneAssign():
    exec 'None = 2'
def LiteralAssign():
    exec "'2' = '3'"

AssertError(SyntaxError, NoneAssign)
AssertError(SyntaxError, LiteralAssign)

# beginning of the file handling

c = compile("     # some comment here   \nprint 10", "", "exec")
c = compile("    \n# some comment\n     \nprint 10", "", "exec")

AssertError(SyntaxError, compile, "    x = 10\n\n", "", "exec")
AssertError(SyntaxError, compile, "    \n   #comment\n   x = 10\n\n", "", "exec")

if sys.platform == 'cli':
    c = compile(u"\u0391 = 10\nif \u0391 != 10: 1/0", "", "exec")
    exec c

# from __future__ tests
AssertError(SyntaxError, compile, "def f():\n    from __future__ import division", "", "exec")
AssertError(SyntaxError, compile, "'doc'\n'doc2'\nfrom __future__ import division", "", "exec")

# del x
AssertError(SyntaxError, compile, "def f():\n    del x\n    def g():\n        return x\n", "", "exec")
AssertError(SyntaxError, compile, "def f():\n    def g():\n        return x\n    del x\n", "", "exec")
AssertError(SyntaxError, compile, "def f():\n    class g:\n        def h(self):\n            print x\n        pass\n    del x\n", "", "exec")
# add global to the picture
c = compile("def f():\n    x=10\n    del x\n    def g():\n        global x\n        return x\n    return g\nf()()\n", "", "exec")
AssertError(NameError, eval, c)
c = compile("def f():\n    global x\n    x=10\n    del x\n    def g():\n        return x\n    return g\nf()()\n", "", "exec")
AssertError(NameError, eval, c)

# global following definition test

# affected by bug# 1145
if is_cli:
    AssertError(SyntaxWarning, compile, "def f():\n    a = 1\n    global a\n", "", "exec")
    AssertError(SyntaxWarning, compile, "def f():\n    def a(): pass\n    global a\n", "", "exec")
    AssertError(SyntaxWarning, compile, "def f():\n    for a in []: pass\n    global a\n", "", "exec")
    AssertError(SyntaxWarning, compile, "def f():\n    global a\n    a = 1\n    global a\n", "", "exec")
    AssertError(SyntaxWarning, compile, "def f():\n    print a\n    global a\n", "", "exec")
    AssertError(SyntaxWarning, compile, "def f():\n    a = 1\n    global a\n    global a\n    a = 1", "", "exec")
    AssertError(SyntaxWarning, compile, "x = 10\nglobal x\n", "", "exec")

c = compile("def f():\n    global a\n    global a\n    a = 1\n", "", "exec")

# unqualified exec in nested function
AssertError(SyntaxError, compile, "def f():\n    x = 1\n    def g():\n        exec 'pass'\n        print x", "", "exec")
# correct case - qualified exec in nested function
c = compile("def f():\n    x = 10\n    def g():\n        exec 'pass' in {}\n        print x\n", "", "exec")

# private names test

class C:
    __x = 10
    class ___:
        __y = 20
    class D:
        __z = 30

AreEqual(C._C__x, 10)
AreEqual(C.___.__y, 20)
AreEqual(C.D._D__z, 30)

class B(object):
    def method(self, __a):
        return __a

AreEqual(B().method("__a passed in"), "__a passed in")

class B(object):
    def method(self, (__a, )):
        return __a

AreEqual(B().method(("__a passed in", )), "__a passed in")

class B(object):
    def __f(self):
        pass


Assert('_B__f' in dir(B))

class B(object):
    class __C(object): pass

Assert('_B__C' in dir(B))

class B(object):
	x = lambda self, __a : __a

AreEqual(B.x(B(), _B__a='value'), 'value')


#Hit negative case of 'sublist' in http://www.python.org/doc/2.5.1/ref/grammar.txt.
AssertError(SyntaxError, compile, "def f((1)): pass", "", "exec")

#
# Make sure that augmented assignment also binds in the given scope
#

augassign_code = """
x = 10
def f():
    x %s 10
f()
"""

def test_augassign_binding():
    for op in ["+=", "-=", "**=", "*=", "//=", "/=", "%=", "<<=", ">>=", "&=", "|=", "^="]:
        code = augassign_code % op
        try:
            exec code in {}, {}
        except:
            pass
        else:
            Assert(False, "augassign binding test didn't raise exception")
    return True

Assert(test_augassign_binding())

# tests for multiline compound statements
class MyException(Exception): pass
def test_multiline_compound_stmts():
    tests = [
                "if False: print 'In IF'\nelse: x = 2; raise MyException('expected')",
                "if False: print 'In IF'\nelif True: x = 2;raise MyException('expected')\nelse: print 'In ELSE'",
                "for i in (1,2): x = i\nelse: x = 5; raise MyException('expected')",
                "while 5 in (1,2): print i\nelse:x = 2;raise MyException('expected')",
                "try: x = 2\nexcept: print 'In EXCEPT'\nelse: x=20;raise MyException('expected')",
            ]
    for test in tests:
        try:
            c = compile(test,"","exec")
            exec c
        except MyException:
            pass
        else:
            Assert(False, "multiline_compound stmt test did not raise exception. test = " + test)

test_multiline_compound_stmts()

# Generators cannot have return statements with values in them. SyntaxError is thrown in those cases.
def test_generator_with_nonempty_return():
    tests = [
        "def f():\n     return 42\n     yield 3",
        "def f():\n     yield 42\n     return 3",
        "def f():\n     yield 42\n     return None",
        "def f():\n     if True:\n          return 42\n     yield 42",
        "def f():\n     try:\n          return 42\n     finally:\n          yield 23"
        ]

    for test in tests:
        #Merlin 148614 - Change it to AssertErrorWithMessage once bug is fixed.
        AssertErrorWithPartialMessage(SyntaxError, "'return' with argument inside generator", compile, test, "", "exec")
        
    #Verify that when there is no return value error is not thrown.
    def f():
        yield 42
        return
    
test_generator_with_nonempty_return()

# compile function which returns from finally, but does not yield from finally.
c = compile("def f():\n    try:\n        pass\n    finally:\n        return 1", "", "exec")

def ret_from_finally():
    try:
        pass
    finally:
        return 1
    return 2
    
AreEqual(ret_from_finally(), 1)

def ret_from_finally2(x):
    if x:
        try:
            pass
        finally:
            return 1
    else:
        return 2

AreEqual(ret_from_finally2(True), 1)
AreEqual(ret_from_finally2(False), 2)

def ret_from_finally_x(x):
    try:
        1/0
    finally:
        return x

AreEqual(ret_from_finally_x("Hi"), "Hi")

def ret_from_finally_x2():
    try:
        1/0
    finally:
        raise AssertionError("This one")

try:
    ret_from_finally_x2()
except AssertionError, e:
    AreEqual(e.args[0], "This one")
else:
    Fail("Expected AssertionError, got none")

try:
    pass
finally:
    pass

# The try block can only have one default except clause, and it must be last

try_syntax_error_tests = [
"""
try:
    pass
except:
    pass
except Exception, e:
    pass
""",
"""
try:
    pass
except Exception, e:
    pass
except:
    pass
except:
    pass
""",
"""
try:
    pass
except:
    pass
except:
    pass
"""
]

for code in try_syntax_error_tests:
    AssertError(SyntaxError, compile, code, "code", "exec")

def test_break_in_else_clause():
    def f():
       exec ('''
       while i >= 0:
           pass
       else:
           break''')
            
    AssertError(SyntaxError, f)

#Just make sure these don't throw
print "^L"
temp = 7
print temp

print "No ^L's..."

# keep this at the end of the file, do not insert anything below this line

def endoffile():
    return "Hi" # and some comment here

def test_syntaxerror_text():       
    method_missing_colon = ("    def MethodTwo(self)\n", """
class HasASyntaxException:
    def MethodOne(self):
        print 'hello'
        print 'world'
        print 'again'
    def MethodTwo(self)
        print 'world'""")
        
    function_missing_colon1 = ("def f()", "def f()")
    function_missing_colon2 = ("def f()\n", "def f()\n")
    function_missing_colon3 = ("def f()\r\n", "def f()\r\n")
    function_missing_colon4 = ("def f()\r", "def f()\r")
        
    function_missing_colon2a = ("def f()\n", "print 1\ndef f()\nprint 3")
    function_missing_colon3a = ("def f()\r\n", "print 1\ndef f()\r\nprint 3")
    function_missing_colon4a = ("def f()\rprint 3", "print 1\ndef f()\rprint 3")
    
    tests = (
        method_missing_colon,
        #function_missing_body,
        function_missing_colon1,
        function_missing_colon2,
        function_missing_colon3,
        function_missing_colon4,

        function_missing_colon2a,
        function_missing_colon3a,
        function_missing_colon4a,
    )
    
    for expectedText, testCase in tests:            
        try:
            exec testCase
        except SyntaxError, e:
            AreEqual(e.text, expectedText)


try:
    exec("""
    def f():
        x = 3
            y = 5""")
    AssertUnreachable()
except IndentationError, e:
    AreEqual(e.lineno, 2)

run_test(__name__)