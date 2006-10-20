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

def x(a,b,c):
    z = 8
    if a < b:
        return c
    elif c < 5 :
        return a + b
    else:
        return z


Assert(x(1,2,10) == 10)
Assert(x(2,1,4) == 3)
Assert(x(1,1,10) == 8)

def f():
    pass

f.a = 10

Assert(f.a == 10)
AreEqual(f.__module__, __name__)

def g():
    g.a = 20

g()

Assert(g.a == 20)


def foo(): pass

AreEqual(foo.func_code.co_filename.lower().endswith('functions.py'), True)
AreEqual(foo.func_code.co_firstlineno, 48)  # if you added lines to the top of this file you need to update this number.

def a(*args): return args
def b(*args): return a(*args)
AreEqual(b(1,2,3), (1,2,3))

# some coverage for Function3 code
def xwd(a=0,b=1,c=3):
    z = 8
    if a < b:
        return c
    elif c < 5 :
        return a + b
    else:
        return z
        
AreEqual(x,x)
AreEqual(xwd(), 3)
AssertError(TypeError, (lambda:x()))
AreEqual(xwd(2), 3)
AssertError(TypeError, (lambda:x(1)))
AreEqual(xwd(0,5), 3)
AssertError(TypeError, (lambda:x(0,5)))
AreEqual( (x == "not-a-Function3"), False)

def y(a,b,c,d):
	return a+b+c+d

def ywd(a=0, b=1, c=2, d=3):
	return a+b+c+d

AreEqual(y, y)
AreEqual(ywd(), 6)
AssertError(TypeError, y)
AreEqual(ywd(4), 10)
AssertError(TypeError, y, 4)
AreEqual(ywd(4,5), 14)
AssertError(TypeError, y, 4, 5)
AreEqual(ywd(4,5,6), 18)
AssertError(TypeError, y, 4,5,6)
AreEqual( (y == "not-a-Function4"), False)

def foo(): "hello world"
AreEqual(foo.__doc__, 'hello world')

############# coverage ############# 

# function5
def f1(a=1, b=2, c=3, d=4, e=5):    return a * b * c * d * e
def f2(a, b=2, c=3, d=4, e=5):    return a * b * c * d * e
def f3(a, b, c=3, d=4, e=5):    return a * b * c * d * e
def f4(a, b, c, d=4, e=5):    return a * b * c * d * e
def f5(a, b, c, d, e=5):    return a * b * c * d * e
def f6(a, b, c, d, e):    return a * b * c * d * e

for f in (f1, f2, f3, f4, f5, f6):
    AssertError(TypeError, f, 1, 1, 1, 1, 1, 1)             # 6 args
    AreEqual(f(10,11,12,13,14), 10 * 11 * 12 * 13 * 14)     # 5 args

for f in (f1, f2, f3, f4, f5):
    AreEqual(f(10,11,12,13), 10 * 11 * 12 * 13 * 5)         # 4 args
for f in (f6,):    
    AssertError(TypeError, f, 1, 1, 1, 1)

for f in (f1, f2, f3, f4):
    AreEqual(f(10,11,12), 10 * 11 * 12 * 4 * 5)             # 3 args
for f in (f5, f6):    
    AssertError(TypeError, f, 1, 1, 1)

for f in (f1, f2, f3):
    AreEqual(f(10,11), 10 * 11 * 3 * 4 * 5)                 # 2 args
for f in (f4, f5, f6):    
    AssertError(TypeError, f, 1, 1)

for f in (f1, f2):
    AreEqual(f(10), 10 * 2 * 3 * 4 * 5)                     # 1 args
for f in (f3, f4, f5, f6):    
    AssertError(TypeError, f, 1)

for f in (f1,):
    AreEqual(f(), 1 * 2 * 3 * 4 * 5)                        # no args
for f in (f2, f3, f4, f5, f6):    
    AssertError(TypeError, f)

# method
class C1:
    def f0(self): return 0
    def f1(self, a): return 1
    def f2(self, a, b): return 2
    def f3(self, a, b, c): return 3
    def f4(self, a, b, c, d): return 4
    def f5(self, a, b, c, d, e): return 5
    def f6(self, a, b, c, d, e, f): return 6
    def f7(self, a, b, c, d, e, f, g): return 7

class C2: pass

c1, c2 = C1(), C2()

line = ""
for i in range(8):
    args = ",".join(['1'] * i)
    line += "AreEqual(c1.f%d(%s), %d)\n" % (i, args, i)
    line +=  "AreEqual(C1.f%d(c1,%s), %d)\n" % (i, args, i)
    #line +=  "try: C1.f%d(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n" % (i, args)
    #line +=  "try: C1.f%d(c2, %s) \nexcept TypeError: pass \nelse: raise AssertionError\n" % (i, args)

#print line
exec line    

def SetAttrOfInstanceMethod():
    C1.f0.attr = 1
AssertError(AttributeError, SetAttrOfInstanceMethod)

C1.f0.im_func.attr = 1
AreEqual(C1.f0.attr, 1)
AreEqual(dir(C1.f0).__contains__("attr"), True)

AreEqual(C1.f0.__module__, __name__)
