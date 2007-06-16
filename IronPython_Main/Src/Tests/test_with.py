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

from __future__ import with_statement
from lib.assert_util import *
import exceptions

#test case with RAISE(exit consumes), YIELD, RETURN, BREAK and CONTINUE in WITH
m = 0
class A:
    def __enter__(self):
        globals()["m"] += 99 
        return 300
    def __exit__(self,type,value,traceback): 
        if(type == None and value == None and traceback == None):
            globals()["m"] += 55 
        else:
            globals()["m"] *= 2 
        return 1

a = A()

def foo():
    p = 100
    for y in [1,2,3,4,5,6,7,8,9]:
        for x in [10,20,30,40,50,60,70,80,90]:
            with a as b:
                p = p + 1
                if ( x == 20 ): continue 
                if ( x == 50 and y == 5 ):    break
                if( x != 40 and y != 4) : yield p
                p = p + 5
                p = p +  x * 100
                p = p + 1
                if(x  % 3 == 0):
                    raise RuntimeError("we force exception")
                if(y == 8):
                    globals()["m"] += p
                    return
                if(x  % 3  == 0 and y %3 == 0):
                    raise RuntimeError("we force exception")
                if ( x == 90 ): continue 
                if ( x == 60 and y == 6 ): break
                yield b + p 
                p = p + 1
try: 
    k = foo()
    while(k.next()):pass
except StopIteration: AreEqual(m,427056988)
else :Fail("Expected StopIteration but found None")

# testing __enter__
def just_a_fun(arg): return 300
class B:
    def __enter__(self): return "Iron", "Python", just_a_fun
    def __exit__(self, a,b,c): pass

mydict = {1: [0,1,2], 2:None }
with B() as (mydict[1][0], mydict[2], B.myfun):
    AreEqual((mydict[1],mydict[2],B().myfun()),(["Iron",1,2],"Python",just_a_fun(None)) )

#ensure it is same outside with also
AreEqual((mydict[1],mydict[2],B().myfun()),(["Iron",1,2],"Python",just_a_fun(None)) )

# more args
class C:
    def __enter__(self,morearg): pass
    def __exit__(self, a,b,c): pass
try:
    with C() as something: pass
except TypeError: pass
else :Fail("Expected TypeError but found None")

#enter raises
class D:
    def __enter__(self): 
        raise RuntimeError("we force an error")
    def __exit__(self, a,b,c): pass

try:
    with D() as something: pass
except RuntimeError: pass
else :Fail("Expected RuntimeError but found None")

#missing enter
class MissingEnter: 
    def __exit__(self,a,b,c): pass
try:
    with MissingEnter(): pass
except AttributeError:pass
else: Fail("Expected AttributeError but found None")

# Testing __exit__
# more args
class E:
    def __enter__(self): pass
    def __exit__(self, a,b,c,d,e,f): pass
try:
    with E() as something: pass
except TypeError: pass
else :Fail("Expected TypeError but found None")

# less args
class F:
    def __enter__(self): pass
    def __exit__(self): pass
try:
    with F() as something: pass
except TypeError: pass
else :Fail("Expected TypeError but found None")

#exit raises
class H:
    def __enter__(self): H.var1 = 100 
    def __exit__(self, a,b,c): 
        H.var2 = 200
        raise RuntimeError("we force an error")

try:
    with H(): 
        H.var3 = 300
except RuntimeError: AreEqual((H.var1,H.var2,H.var3),(100,200,300))
else :Fail("Expected RuntimeError but found None")

#exit raises on successful / throwing WITH
class Myerr1(Exception):pass
class Myerr2(Exception):pass
class Myerr3(Exception):pass
class ExitRaise:
    def __enter__(self): H.var1 = 100 
    def __exit__(self, a,b,c):
        if(a == None and b == None and c == None): 
            raise Myerr1
        raise Myerr2

try:
    with ExitRaise(): 
        1+2+3
except Myerr1: pass
else :Fail("Expected Myerr1 but found None")

try:
    with ExitRaise(): 
        raise Myerr3
except Myerr2: pass
else :Fail("Expected Myerr2 but found None")


#exit propagates exception on name deletion ( covers FLOW CHECK scenario)
class PropagateException:
    def __enter__(self): pass
    def __exit__(self, a,b,c): return False
try:
    with PropagateException() as PE:
        del PE
        print PE
except NameError:pass
else: Fail("Expected NameError but found None")

try:
    with PropagateException() as PE:
        PE.var1 = 100
        del PE
        print PE
except AttributeError:pass
else: Fail("Expected AttributeError but found None")

#exit consumes exception 
class ConsumeException:
    def __enter__(self): pass
    def __exit__(self, a,b,c): return [1,2,3],{"dsad":"dsd"},"hello"
with ConsumeException():1/0

#missing exit
class MissingExit: 
    def __enter__(self): pass
try:
    with MissingEnter(): pass
except AttributeError:pass
else: Fail("Expected AttributeError but found None")

#With Stmt under other compound statements (NO YIELD)

gblvar = 0


#inheritance
class cxtmgr:
    def __exit__(self, a, b, c):  
        globals()["gblvar"] += 10
        return False


class inherited_cxtmgr(cxtmgr):
    def __enter__(self): 
        globals()["gblvar"] += 10
        return False


# Building up most complex TRY-CATCH-FINALLY-RAISE-WITH-CLASS combination with inheritance.
#try->(try->(except->(with ->fun ->(try->(with->raise)->Finally(With)))))
try: #Try
    try: #try->try
        globals()["gblvar"] += 1 
        1/0
    except ZeroDivisionError: #try->(try->except)
        globals()["gblvar"] += 2 
        with inherited_cxtmgr() as ic: #try->(try->(except->with(inherited)))
            globals()["gblvar"] += 3
            def fun_in_with(): return "Python is smart"
            AreEqual(fun_in_with(),"Python is smart") #try->(try->(except->(with ->fun)))
            try:                                      #try->(try->(except->(with ->fun ->try)))
                globals()["gblvar"] += 4 
                with inherited_cxtmgr() as inherited_cxtmgr.var: #try->(try->(except->(with ->fun ->(try->with))))
                    globals()["gblvar"] += 5
                    raise Myerr1  #try->(try->(except->(with ->fun ->(try->with->raise))))
            finally:    #try->(try->(except->(with ->fun ->(try->(with->raise)->Finally))))
                # TODO: This is actually incorrect, CPython has the ZeroDivisionError here.
                if not sys.platform=="win32":
                    AreEqual(sys.exc_info()[0],Myerr1)
                else:
                    AreEqual(sys.exc_info()[0], exceptions.ZeroDivisionError)
                globals()["gblvar"] += 6 
                class ClassInFinally:
                    def __enter__(self): 
                        globals()["gblvar"] +=  7
                        return 200
                    def __exit__(self,a,b,c):
                        globals()["gblvar"] += 8
                        return False # it raises
                with ClassInFinally(): #try->(try->(except->(with ->fun ->(try->(with->raise)->Finally(With)))))
                    globals()["gblvar"] += 9 
except Myerr1: AreEqual(globals()["gblvar"],85)

# With in __enter__ and __exit__
gblvar = 0            
class A: 
    def __enter__(self):  globals()["gblvar"] += 1 ; return 100            
    def __exit__(self,a,b,c):  globals()["gblvar"] += 2; return 200    

class WithInEnterExit:
    def __enter__(self): 
        with A() as b:
            globals()["gblvar"] += 3;return A()
    def __exit__(self,a,b,c): 
        with A() as c:
            globals()["gblvar"] += 4; return A()

AreEqual ( 1,1)
with WithInEnterExit() as wie:
    with wie as wie_wie:
        globals()["gblvar"] += 100
        
AreEqual(globals()["gblvar"],116)
