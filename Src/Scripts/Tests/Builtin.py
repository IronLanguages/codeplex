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
import sys

Assert(max([1,2,3]) == 3)
Assert(max((1,2,3)) == 3)
Assert(max(1,2,3) == 3)
Assert(min([1,2,3]) == 1)
Assert(min((1,2,3)) == 1)
Assert(min(1,2,3) == 1)

class C:
    x=1

Assert(callable(min))
Assert(not callable("a"))
Assert(callable(callable))
Assert(callable(lambda x, y: x + y))
Assert(callable(C))
Assert(not callable(C.x))


success=0
try:
    import this_module_does_not_exist
except ImportError:
    success=1
Assert(success==1)

def verify_file(ff):
    cnt = 0
    for i in ff:
        Assert(i[0:5] == "Hello")
        cnt += 1
    ff.close()
    Assert(cnt == 10)

f = file("testfile.tmp", "w")
for i in range(10):
    f.write("Hello " + str(i) + "\n")
f.close()

f = file("testfile.tmp")
verify_file(f)
f = file("testfile.tmp", "r")
verify_file(f)
f = file("testfile.tmp", "r", -1)
verify_file(f)
f = open("testfile.tmp")
verify_file(f)
f = open("testfile.tmp", "r")
verify_file(f)
f = open("testfile.tmp", "r", -1)
verify_file(f)
f = open("testfile.tmp", "r", 0)
verify_file(f)

if is_cli:
    import System
    fs = System.IO.FileStream("testfile.tmp", System.IO.FileMode.Open, System.IO.FileAccess.Read)
    f = open(fs)
    verify_file(f)
    
    ms = System.IO.MemoryStream(30)
    f = open(ms)
    f.write("hello")
    AreEqual(ms.Length, 5)
    AreEqual(ms.GetBuffer()[0], ord('h'))
    AreEqual(ms.GetBuffer()[4], ord('o'))
    ms.Close()


# more tests for 'open'
AssertError(TypeError, open, None) # arg must be string
AssertError(TypeError, open, [])
AssertError(TypeError, open, 1)

Assert(complex(2) == complex(2, 0))

x = [1,2,3,4,5,6,7,8,9,0]
Assert(x.pop() == 0)
Assert(x.pop(3) == 4)
Assert(x.pop(-5) == 5)
Assert(x.pop(0) == 1)
Assert(x.pop() == 9)
Assert(x.pop(2) == 6)
Assert(x.pop(3) == 8)
Assert(x.pop(-1) == 7)
Assert(x.pop(-2) == 2)
Assert(x.pop() == 3)

x="Hello Worllds"
s = x.split("ll")
Assert(s[0] == "He")
Assert(s[1] == "o Wor")
Assert(s[2] == "ds")

Assert("1,2,3,4,5,6,7,8,9,0".split(",") == ['1','2','3','4','5','6','7','8','9','0'])
Assert("1,2,3,4,5,6,7,8,9,0".split(",", -1) == ['1','2','3','4','5','6','7','8','9','0'])
Assert("1,2,3,4,5,6,7,8,9,0".split(",", 2) == ['1','2','3,4,5,6,7,8,9,0'])
Assert("1--2--3--4--5--6--7--8--9--0".split("--") == ['1','2','3','4','5','6','7','8','9','0'])
Assert("1--2--3--4--5--6--7--8--9--0".split("--", -1) == ['1','2','3','4','5','6','7','8','9','0'])
Assert("1--2--3--4--5--6--7--8--9--0".split("--", 2) == ['1', '2', '3--4--5--6--7--8--9--0'])
AreEqual(''.split(), [])
AreEqual(''.split(' '), [''])


hw = "hello world"
Assert(hw.startswith("hello"))
Assert(not hw.startswith("heloo"))
Assert(hw.startswith("llo", 2))
Assert(not hw.startswith("lno", 2))
Assert(hw.startswith("wor", 6, 9))
Assert(not hw.startswith("wor", 6, 7))
Assert(not hw.startswith("wox", 6, 10))
Assert(not hw.startswith("wor", 6, 2))

Assert("adadad".count("d") == 3)
Assert("adbaddads".count("ad") == 3)

Assert("\ttext\t".expandtabs(0) == "text")
Assert("\ttext\t".expandtabs(-10) == "text")

x = [1,2,3]
x += [4,5,6]
Assert(x == [1,2,3,4,5,6])
x = [1,2,3]
x *= 2
Assert(x == [1,2,3,1,2,3])
x = (1,2,3)
x += (4,5,6)
Assert(x == (1,2,3,4,5,6))
x = (1,2,3)
x *= 2
Assert(x == (1,2,3,1,2,3))

x = ["begin",1,2,3,4,5,6,7,8,9,0,"end"]
del x[6:]
x.reverse()
Assert(x == [5, 4, 3, 2, 1, "begin"])

x = list("iron python")
x.reverse()
Assert(x == ['n','o','h','t','y','p',' ','n','o','r','i'])

def max(a,b):
    if a>b: return a
    else: return b

code = compile("max(10, 15)", "<string>", "eval")
Assert(eval(code) == 15)

code = compile("x = [1,2,3,4,5]\nx.reverse()\nAssert(x == [5,4,3,2,1])", "<string>", "exec")
exec(code)
Assert(x == [5,4,3,2,1])

AssertError(ValueError, compile, "2+2", "<string>", "invalid")
AssertError(SyntaxError, compile, "if 1 < 2: pass", "<string>", "eval")
AssertError(SyntaxError, compile, "a=2", "<string>", "eval")

AssertError(SyntaxError, eval, "a=2")

# stdin, stdout redirect and input, raw_input tests

old_stdin = sys.stdin
old_stdout = sys.stdout
sys.stdout = file("testfile.tmp", "w")
print "Into the file"
print "2+2"
sys.stdout.close()
sys.stdout = old_stdout

sys.stdin = file("testfile.tmp", "r")
s = raw_input()
Assert(s == "Into the file")
s = input()
Assert(s == 4)
sys.stdin.close()
sys.stdin = old_stdin

f = file("testfile.tmp", "r")
g = file("testfile.tmp", "r")
s = f.readline()
t = g.readline()
Assert(s == t)
Assert(s == "Into the file\n")

f.close()
g.close()

f = file("testfile.tmp", "w")
f.writelines(["1\n", "2\n", "2\n", "3\n", "4\n", "5\n", "6\n", "7\n", "8\n", "9\n", "0\n"])
f.close()
f = file("testfile.tmp", "r")
l = f.readlines()
Assert(l == ["1\n", "2\n", "2\n", "3\n", "4\n", "5\n", "6\n", "7\n", "8\n", "9\n", "0\n"])
f.close()

# reversed() test

class ToReverse:
    a = [1,2,3,4,5,6,7,8,9,0]
    def __len__(self): return len(self.a)
    def __getitem__(self, i): return self.a[i]

x = []
for i in reversed(ToReverse()):
    x.append(i)

Assert(x == [0,9,8,7,6,5,4,3,2,1])

# more tests for 'reversed'
AssertError(TypeError, reversed, 1) # arg to reversed must be a sequence
AssertError(TypeError, reversed, None)
AssertError(TypeError, reversed, ToReverse)

def cat(x,y):
    ret = ""
    if x != None: ret += x
    if y != None: ret += y
    return ret

Assert(map(cat, ["a","b"],["c","d", "e"]) == ["ac", "bd", "e"])
Assert(map(lambda x,y: x+y, [1,1],[2,2]) == [3,3])
Assert(map(None, [1,2,3]) == [1,2,3])
Assert(map(None, [1,2,3], [4,5,6]) == [(1,4),(2,5),(3,6)])


# sorted

a = [6,9,4,5,3,1,2,7,8]
Assert(sorted(a) == [1,2,3,4,5,6,7,8,9])
Assert (a == [6,9,4,5,3,1,2,7,8])
Assert(sorted(a, None, None, True) == [9,8,7,6,5,4,3,2,1])

class P:
    def __init__(self, n, s):
        self.n = n
        self.s = s

def equal_p(a,b):
    return a.n == b.n and a.s == b.s

def key_p(a):
    return a.n.lower()

def cmp_s(a,b):
    return cmp(a.s, b.s)

def cmp_n(a,b):
    return cmp(a.n, b.n)

a = [P("John",6),P("Jack",9),P("Gary",4),P("Carl",5),P("David",3),P("Joe",1),P("Tom",2),P("Tim",7),P("Todd",8)]
x = sorted(a, cmp_s)
y = [P("Joe",1),P("Tom",2),P("David",3),P("Gary",4),P("Carl",5),P("John",6),P("Tim",7),P("Todd",8),P("Jack",9)]

for i,j in zip(x,y):
    Assert(equal_p(i,j))

# case sensitive compariso is the default one

a = [P("John",6),P("jack",9),P("gary",4),P("carl",5),P("David",3),P("Joe",1),P("Tom",2),P("Tim",7),P("todd",8)]
x = sorted(a, cmp_n)
y = [P("David",3),P("Joe",1),P("John",6),P("Tim",7),P("Tom",2),P("carl",5),P("gary",4),P("jack",9),P("todd",8)]

for i,j in zip(x,y):
    Assert(equal_p(i,j))

# now compare using keys - case insensitive

x = sorted(a,None,key_p)
y = [P("carl",5),P("David",3),P("gary",4),P("jack",9),P("Joe",1),P("John",6),P("Tim",7),P("todd",8),P("Tom",2)]
for i,j in zip(x,y):
    Assert(equal_p(i,j))

def invcmp(a,b):
    return -cmp(a,b)

Assert(sorted(range(10), None, None, True) == range(10)[::-1])
Assert(sorted(range(9,-1,-1), None, None, False) == range(10))
Assert(sorted(range(10), invcmp, None, True) == sorted(range(9,-1,-1), None, None, False))
Assert(sorted(range(9,-1,-1),invcmp, None, True) == sorted(range(9,-1,-1), None, None, False))

d = {'John': 6, 'Jack': 9, 'Gary': 4, 'Carl': 5, 'David': 3, 'Joe': 1, 'Tom': 2, 'Tim': 7, 'Todd': 8}
x = sorted([(v,k) for k,v in d.items()])
Assert(x == [(1, 'Joe'), (2, 'Tom'), (3, 'David'), (4, 'Gary'), (5, 'Carl'), (6, 'John'), (7, 'Tim'), (8, 'Todd'), (9, 'Jack')])

# file newline handling test

def test_newline(norm, mode):
    f = file("testfile.tmp", mode)
    Assert(f.read() == norm)
    for x in xrange(len(norm)):
        f.seek(0)
        a = f.read(x)
        b = f.read(1)
        c = f.read()
        Assert(a+b+c == norm)
    f.close()

AssertError(TypeError, file, None) # arg must be string
AssertError(TypeError, file, [])
AssertError(TypeError, file, 1)

norm   = "Hi\nHello\nHey\nBye\nAhoy\n"
unnorm = "Hi\r\nHello\r\nHey\r\nBye\r\nAhoy\r\n"
f = file("testfile.tmp", "wb")
f.write(unnorm)
f.close()

test_newline(norm, "r")
test_newline(unnorm, "rb")

import re

s = ''
for i in range(32, 128):
    if not chr(i).isalnum():
        s = s + chr(i)
x = re.escape(s)
Assert(x == '\\ \\!\\"\\#\\$\\%\\&\\\'\\(\\)\\*\\+\\,\\-\\.\\/\\:\\;\\<\\=\\>\\?\\@\\[\\\\\\]\\^\\_\\`\\{\\|\\}\\~\\\x7f')

reg = re.compile("\[(?P<header>.*?)\]")
m = reg.search("[DEFAULT]")
Assert( m.groups() == ('DEFAULT',))
Assert( m.group('header') == 'DEFAULT' )

reg2 = re.compile("(?P<grp>\S+)?")
m2 = reg2.search("")
Assert ( m2.groups() == (None,))
Assert ( m2.groups('Default') == ('Default',))

Assert(re.sub('([^aeiou])y$', r'\lies', 'vacancy') == 'vacan\\lies')
Assert(re.sub('([^aeiou])y$', r'\1ies', 'vacancy') == 'vacancies')

def TryReCall():
    re.compile(None)
AssertError(TypeError, TryReCall)

ex = re.compile(r'\s+')

m = ex.match('(object Petal', 7)
Assert (m.end(0) == 8)

success=False
try:
    nstr("Hi")
except NameError:
    success=True
AreEqual(success, True)

success=False
try:
    zip2([1,2,3],[4,5,6])
except NameError:
    success=True
AreEqual(success, True)

AreEqual(str(), "")
AreEqual(unicode(), u"")

AreEqual(oct(long(0)), "0L")
AreEqual(hex(12297829382473034410), "0xAAAAAAAAAAAAAAAAL")
AreEqual(hex(-1L), "-0x1L")
AreEqual(long("-01L"), -1L)
AreEqual(int(" 1 "), 1)
AreEqual(int(" -   1  "), -1)
AreEqual(long("   -   1 "), -1L)

for f in [ long, int ]:
    AssertError(ValueError, f, 'p')
    AssertError(ValueError, f, 't')
    AssertError(ValueError, f, 'a')
    AssertError(ValueError, f, '3.2')
    AssertError(ValueError, f, '0x0R')
    AssertError(ValueError, f, '09', 8)
    AssertError(ValueError, f, '0A')
    AssertError(ValueError, f, '0x0G')

AssertError(ValueError, int, '1l')

AssertError(TypeError, abs, None)

try:
    import nt
    nt.remove("testfile.tmp")
except:
    pass

AreEqual(int(1e100), 10000000000000000159028911097599180468360808563945281389781327557747838772170381060813469985856815104L)
AreEqual(int(-1e100), -10000000000000000159028911097599180468360808563945281389781327557747838772170381060813469985856815104L)

# tests for 'reduce'
def add(x,y):
    return x+y;
Assert(reduce(add, [2,3,4]) == 9)
Assert(reduce(add, [2,3,4], 1) == 10)
AssertError(TypeError, reduce, None, [1,2,3]) # arg 1 must be callable
AssertError(TypeError, reduce, None, [2,3,4], 1)
AssertError(TypeError, reduce, add, None) # arg 2 must support iteration
AssertError(TypeError, reduce, add, None, 1)
AssertError(TypeError, reduce, add, []) # arg 2 must not be empty sequence with no initial value
AssertError(TypeError, reduce, add, "") # empty string sequence
AssertError(TypeError, reduce, add, ()) # empty tuple sequence
Assert(reduce(add, [], 1), 1) # arg 2 has initial value through arg 3 so no TypeError for this
Assert(reduce(add, [], None) == None)
AssertError(TypeError, reduce, add, [])
AssertError(TypeError, reduce, add, "")
AssertError(TypeError, reduce, add, ())

# tests for 'unichar'
AssertError(ValueError, unichr, -1) # arg must be in the range [0...65535] inclusive
AssertError(ValueError, unichr, 65536)
AssertError(ValueError, unichr, 100000)
Assert(unichr(0) == '\x00')
Assert(unichr(65535) == u'\uffff')

AreEqual(pow(2,3), 8)

# tests for 'coerce'
AreEqual(coerce(None, None), (None, None))
AssertError(TypeError, coerce, None, 1)
AssertError(TypeError, coerce, 1, None)

################
#None type properties

AreEqual(type(None), None.__class__)
AreEqual(str(None), None.__str__())
AreEqual(repr(None), None.__repr__())
None.__init__('abc')


#################
# Type properties
AreEqual(type(type), type.__class__)


#################

#################
# buffer method tests

AssertError(TypeError, buffer, None)
AssertError(TypeError, buffer, None, 0)
AssertError(TypeError, buffer, None, 0, 0)
AssertError(ValueError, buffer, "abc", -1) #offset < 0
AssertError(ValueError, buffer, "abc", -1, 0) #offset < 0
#size < -1; -1 is allowed since that is the way to ask for the default value
AssertError(ValueError, buffer, "abc", 0, -2)
b = buffer("abc", 0, -1)
AreEqual(str(b), "abc")
AreEqual(len(b), 3)

b1 = buffer("abc")
AreEqual(str(b1), "abc")
b2 = buffer("def", 0)
AreEqual(str(b2), "def")
b3 = b1 + b2
AreEqual(str(b3), "abcdef")
b4 = 2 * (b2 * 2)
AreEqual(str(b4), "defdefdefdef")

if is_cli:
    a1 = System.Array[int]([1,2])
    arrbuff1 = buffer(a1, 0, 2)
    AreEqual(1, arrbuff1[0])
    AreEqual(2, arrbuff1[1])

    a2 = System.Array[System.String](["a","b"])
    arrbuff2 = buffer(a2, 0, 2)
    AreEqual("a", arrbuff2[0])
    AreEqual("b", arrbuff2[1])

    AreEqual(len(arrbuff1), len(arrbuff2))

    a2 = System.Array[System.Guid]([])
    AssertError(TypeError, buffer, a2)
    
a = buffer("abc")
b = buffer(a, 0, 2)
AreEqual("ab", str(b))
c = buffer(b, 0, 1)
AreEqual("a", str(c))
d = buffer(b, 0, 100)
AreEqual("ab", str(d))
e = buffer(a, 1, 2)
AreEqual(str(e), "bc")
e = buffer(a, 1, 5)
AreEqual(str(e), "bc")
e = buffer(a, 1, -1)
AreEqual(str(e), "bc")
e = buffer(a, 1, 0)
AreEqual(str(e), "")
e = buffer(a, 1, 1)
AreEqual(str(e), "b")


#################

# exec tests

# verify passing a bad value throws...

try:
    exec(3)
    raise ValueError    # test failed
except TypeError:
    pass

# verify exec(...) takes a code object
codeobj = compile ('1+1', '<compiled code>', 'exec')
exec(codeobj)

# verify exec(...) takes a file...
f = file("testfile.tmp", "w")
f.write("x = [1,2,3,4,5]\nx.reverse()\nAssert(x == [5,4,3,2,1])\n")
f.close()

f = file("testfile.tmp", "r")

exec(f)
Assert(x == [5,4,3,2,1])

f.close()

# and now verify it'll take a .NET Stream as well...
if is_cli:
    f = System.IO.FileStream('testfile.tmp', System.IO.FileMode.Open)
    exec(f)
    f.Dispose()

import nt
nt.remove("testfile.tmp")

# verify that exec'd code has access to existing locals
qqq = 3
exec('qqq+1')

# and not to *non*-existing locals..
del qqq
try:
    exec('qqq+1')
    raise ValueError
except NameError:
    pass

exec('qqq+1', {'qqq':99})

# Test passing alternative local and global scopes to exec.

# Explicit global and local scope.

# Functional form of exec.
myloc = {}
myglob = {}
exec("a = 1; global b; b = 1", myglob, myloc)
Assert("a" in myloc)
Assert("a" not in myglob)
Assert("b" in myglob)
Assert("b" not in myloc)

# Statement form of exec.
myloc = {}
myglob = {}
exec "a = 1; global b; b = 1" in myglob, myloc
Assert("a" in myloc)
Assert("a" not in myglob)
Assert("b" in myglob)
Assert("b" not in myloc)


# Explicit global scope implies the same local scope.

# Functional form of exec.
myloc = {}
myglob = {}
exec("a = 1; global b; b = 1", myglob)
Assert("a" in myglob)
Assert("a" not in myloc)
Assert("b" in myglob)
Assert("b" not in myloc)

# Statement form of exec.
myloc = {}
myglob = {}
exec "a = 1; global b; b = 1" in myglob
Assert("a" in myglob)
Assert("a" not in myloc)
Assert("b" in myglob)
Assert("b" not in myloc)


import sys

(old_copyright, old_byteorder) = (sys.copyright, sys.byteorder)
(sys.copyright, sys.byteorder) = ("foo", "foo")

(old_argv, old_exc_type) = (sys.argv, sys.exc_type)
(sys.argv, sys.exc_type) = ("foo", "foo")

reloaded_sys = reload(sys)

# Most attributes get reset
AreEqual((old_copyright, old_byteorder), (reloaded_sys.copyright, reloaded_sys.byteorder))
# Some attributes are not reset
AreEqual((reloaded_sys.argv, reloaded_sys.exc_type), ("foo", "foo"))
# Put back the original values
(sys.copyright, sys.byteorder) = (old_copyright, old_byteorder)
(sys.argv, sys.exc_type) = (old_argv, old_exc_type)


AreEqual(vars().keys().count('__file__'), 1)

# BUG 433: CPython allows hijacking of __builtins__, but IronPython does not
bug_433 = '''
def foo(arg):
    return "Foo"

# trying to override an attribute of __builtins__ causes a TypeError
try:
    __builtins__.oct = foo
    Assert(False, "Cannot override an attribute of __builtins__")
except TypeError:
    pass

# assigning to __builtins__ passes, but doesn't actually affect function semantics
import custombuiltins
'''
# /BUG


class MyMapping:
    def __getitem__(self, index):
            if index == 'a': return 2
            if index == 'b': return 5
            raise IndexError, 'bad index'

AreEqual(eval('a+b', {}, MyMapping()), 7)

# eval referencing locals / globals
value_a = 13
value_b = 17
global_value = 23
def eval_using_locals():
    value_a = 3
    value_b = 7
    AreEqual(eval("value_a"), 3)
    AreEqual(eval("value_b"), 7)
    AreEqual(eval("global_value"), 23)
    AreEqual(eval("value_a < value_b"), True)
    AreEqual(eval("global_value < value_b"), False)
    return True

Assert(eval_using_locals())

if is_cli: 
    if System.BitConverter.IsLittleEndian == True:
        Assert(sys.byteorder == "little")
    else:
        Assert(sys.byteorder == "big") 

AssertError(TypeError,abs,None)

sortedDir = 3
sortedDir = dir()
sortedDir.sort()
Assert(dir() == sortedDir)

## getattr/hasattr: hasattr should eat exception, and return True/False
class C1:
    def __init__(self):
        self.field = C1
    def method(self): 
        return "method"
    def __getattr__(self, attrname):
        if attrname == "lambda":
            return lambda x: len(x)
        elif attrname == "myassert":
            raise AssertionError
        else:
            raise AttributeError, attrname

class C2(object):             
    def __init__(self):
        self.field = C1
    def method(self): 
        return "method"
    def __getattr__(self, attrname):
        if attrname == "lambda":
            return lambda x: len(x)
        elif attrname == "myassert":
            raise AssertionError
        else:
            raise AttributeError, attrname

def test_getattr(t):
    o = t()
    AreEqual(getattr(o, "field"), C1)
    AreEqual(getattr(o, "method")(), "method")
    AreEqual(getattr(o, "lambda")("a"), 1)

    AssertError(AssertionError, getattr, o, "myassert")
    AssertError(AttributeError, getattr, o, "anything")
    AssertError(AttributeError, getattr, o, "else")

    for attrname in ('field', 'method', '__init__', '__getattr__', 'lambda', '__doc__', '__module__'):
        AreEqual(hasattr(o, attrname), True)

    for attrname in ("myassert", "anything", "else"): 
        AreEqual(hasattr(o,attrname), False)

test_getattr(C1)
test_getattr(C2)

## derived from python native type, and create instance of them without arg

flag = 0
def myinit(self): 
    global flag
    flag = flag + 1

cnt = 0
for bt in (tuple, dict, list, str, set, frozenset, int, float, complex):  
    nt = type("derived", (bt,), dict())
    inst = nt()
    AreEqual(type(inst), nt)
    
    nt2 = type("derived2", (nt,), dict())
    inst2 = nt2()
    AreEqual(type(inst2), nt2)

    nt.__init__ = myinit
    inst = nt()
    cnt += 1
    AreEqual(flag, cnt)
    AreEqual(type(inst), nt)

def foo(): yield 2    

def bar(): 
    yield 2
    yield 3

AreEqual(zip(foo()), [(2,)])
AreEqual(zip(foo(), foo()), [(2,2)])
AreEqual(zip(foo(), foo(), foo()), [(2,2,2)])

AreEqual(zip(bar(), foo()), [(2,2)])
AreEqual(zip(foo(), bar()), [(2,2)])

# override pow, delete it, and it should be gone
import operator
pow = 7
AreEqual(pow, 7)
del pow
AreEqual(operator.isCallable(pow), True)

try:
    del pow
    AreEqual(True,False)
except NameError:
    pass
    
    
import Imp.usebuiltin as test
AreEqual(dir(test).count('x'), 1)       # defined variable, not a builtin
AreEqual(dir(test).count('min'), 1)     # defined name that overwrites a builtin
AreEqual(dir(test).count('max'), 0)     # used builtin, never assigned to
AreEqual(dir(test).count('cmp'), 0)     # used, assigned to, deleted, shouldn't be visibled
AreEqual(dir(test).count('del'), 0)     # assigned to, deleted, never used

# complex from string: negative 
# - space related
l = ['1.2', '.3', '4e3', '.3e-4', "0.031"]

for x in l:   
    for y in l:
        AssertError(ValueError, complex, "%s +%sj" % (x, y))
        AssertError(ValueError, complex, "%s+ %sj" % (x, y))
        AssertError(ValueError, complex, "%s - %sj" % (x, y))
        AssertError(ValueError, complex, "%s-  %sj" % (x, y))
        AssertError(ValueError, complex, "%s-\t%sj" % (x, y))
        AssertError(ValueError, complex, "%sj+%sj" % (x, y))
        AreEqual(complex("   %s+%sj" % (x, y)), complex(" %s+%sj  " % (x, y)))

# derive from complex...

class cmplx(complex): pass

AreEqual(cmplx(), complex())
a = cmplx(1)
b = cmplx(1,0)
c = complex(1)
d = complex(1,0)

for x in [a,b,c,d]:
    for y in [a,b,c,d]:
        AreEqual(x,y)


AreEqual(a ** 2, a)
AreEqual(a-complex(), a)
AreEqual(a+complex(), a)
AreEqual(complex()/a, complex())
AreEqual(complex()*a, complex())
AreEqual(complex()%a, complex())
AreEqual(complex() // a, complex())

## coverage for set / frozenset 
class myset(set): pass
class myfrozenset(set): pass

s1 = [2, 4, 5]
s2 = [4, 7, 9, 10]
s3 = [2, 4, 5, 6]

# equality
for x in (set, frozenset, myset, myfrozenset):
    for y in (set, frozenset, myset, myfrozenset):
        AreEqual(x(s1), y(s1))

for x in (set, frozenset, myset, myfrozenset):
    # creating as default
    y = x()
    AreEqual(len(y), 0)
    # creating with 2 args
    AssertError(TypeError, x, range(3), 3)
    AssertError(TypeError, x.__new__, str)
    AssertError(TypeError, x.__new__, str, 'abc')

    xs1, xs2, xs3 = x(s1), x(s2), x(s3)
    
    # membership
    AreEqual(4 in xs1, True)
    AreEqual(6 in xs1, False)    
    
    # relation with another of the same type
    AreEqual(xs1.issubset(xs2), False)
    AreEqual(xs1.issubset(xs3), True)
    AreEqual(xs3.issuperset(xs1), True)
    AreEqual(xs3.issuperset(xs2), False)

    # equivalent op
    AreEqual(xs1 <= xs2, False)
    AreEqual(xs1 <= xs3, True)
    AreEqual(xs3 >= xs1, True)
    AreEqual(xs3 >= xs2, False)
    
    AreEqual(xs1.union(xs2), x([2, 4, 5, 7, 9, 10]))
    AreEqual(xs1.intersection(xs2), x([4]))
    AreEqual(xs1.difference(xs2), x([2, 5]))
    AreEqual(xs2.difference(xs1), x([7, 9, 10]))
    AreEqual(xs2.symmetric_difference(xs1), x([2, 5, 7, 9, 10]))
    AreEqual(xs3.symmetric_difference(xs1), x([6]))

    # equivalent op
    AreEqual(xs1 | xs2, x([2, 4, 5, 7, 9, 10]))
    AreEqual(xs1 & xs2, x([4]))
    AreEqual(xs1 - xs2, x([2, 5]))
    AreEqual(xs2 - xs1, x([7, 9, 10]))
    AreEqual(xs2 ^ xs1, x([2, 5, 7, 9, 10]))
    AreEqual(xs3 ^ xs1, x([6]))

    # repeat with list
    AreEqual(xs1.issubset(s2), False)
    AreEqual(xs1.issubset(s3), True)
    AreEqual(xs3.issuperset(s1), True)
    AreEqual(xs3.issuperset(s2), False)
    
    AreEqual(xs1.union(s2), x([2, 4, 5, 7, 9, 10]))
    AreEqual(xs1.intersection(s2), x([4]))
    AreEqual(xs1.difference(s2), x([2, 5]))
    AreEqual(xs2.difference(s1), x([7, 9, 10]))
    AreEqual(xs2.symmetric_difference(s1), x([2, 5, 7, 9, 10]))
    AreEqual(xs3.symmetric_difference(s1), x([6]))

s1, s2, s3 = 'abcd', 'be', 'bdefgh'
for t1 in (set, frozenset, myset, myfrozenset):
    for t2 in (set, frozenset, myset, myfrozenset):
        # set/frozenset creation
        AreEqual(t1(t2(s1)), t1(s1))
        
        # ops
        for (op, exp1, exp2) in [('&', 'b', 'bd'), ('|', 'abcde', 'abcdefgh'), ('-', 'acd', 'ac'), ('^', 'acde', 'acefgh')]:
            
            x1 = t1(s1)
            exec "x1   %s= t2(s2)" % op
            AreEqual(x1, t1(exp1))

            x1 = t1(s1)
            exec "x1   %s= t2(s3)" % op
            AreEqual(x1, t1(exp2))
            
            x1 = t1(s1)            
            exec "y = x1 %s t2(s2)" % op
            AreEqual(y, t1(exp1))

            x1 = t1(s1)            
            exec "y = x1 %s t2(s3)" % op
            AreEqual(y, t1(exp2))

# set/frozenset related to None

x, y = set([None, 'd']), set(['a', 'b', 'c', None])
AreEqual(x | y, set([None, 'a', 'c', 'b', 'd']))
AreEqual(y | x, set([None, 'a', 'c', 'b', 'd']))
AreEqual(x & y, set([None]))
AreEqual(y & x, set([None]))
AreEqual(x - y, set('d'))
AreEqual(y - x, set('abc'))

# TypeError: tuple.__new__(str): str is not a subtype of tuple 
AssertError(TypeError, tuple.__new__, str)
AssertError(TypeError, tuple.__new__, str, 'abc')

# abs coverage #
#	long integer passed to abs
AreEqual(22L, abs(22L)) 
AreEqual(22L, abs(-22L))

#	bool passed to abs
AreEqual(1, abs(True)) 
AreEqual(0, abs(False))

#	__abs__ defined on user type
class myclass:
    def __abs__(self):
        return "myabs"
c = myclass()
AreEqual("myabs", abs(c))

# execfile coverage #
AssertError(TypeError, execfile, "somefile", "")

AreEqual(abs.__doc__, "abs(number) -> number\n\nReturn the absolute value of the argument.\r\n")
AreEqual(int.__add__.__doc__, "x.__add__(y) <==> x+y\r\n")


# sub classing built-ins works correctly..

class MyFile(file):
    myfield = 0

f = MyFile('temporary.deleteme','w')
AreEqual(f.myfield, 0)
f.close()
import nt   
nt.unlink('temporary.deleteme')


class C(list):
    def __eq__(self, other):
        return 'Passed'

AreEqual(C() == 1, 'Passed')

# extensible types should hash the same as non-extensibles, and unary operators
# should work too
for x, y in ( (int, 2), (str, 'abc'), (float, 2.0), (long, 2L), (complex, 2+0j) ):
    class foo(x): pass
    
    AreEqual(hash(foo(y)), hash(y))
    
    if x != str: 
        AreEqual(-foo(y), -y)
        AreEqual(+foo(y), +y)
        
        if x != complex and x != float: 
            AreEqual(~foo(y), ~y)
    
    


# can use kw-args w/ file    
try:
    f = file(name='temporary.deleteme', mode='w')
    f.close()
    nt.unlink('temporary.deleteme')
except:
    AreEqual(True, False)


AreEqual(int(x=1), 1)
AreEqual(float(x=2), 2.0)
AreEqual(long(x=3), 3L)
AreEqual(complex(imag=4, real=3), 3 + 4j)
AreEqual(str(object=5), '5')
AreEqual(unicode(string='a', errors='strict'), 'a')
AreEqual(tuple(sequence=range(3)), (0,1,2))
AreEqual(list(sequence=(0,1)), [0,1])
