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

import clr
from lib.assert_util import *
load_iron_python_test()
import IronPythonTest

from System.Threading import Timer, AutoResetEvent
from System import EventArgs

are = AutoResetEvent(False)

def MyTick(state):
    global superTimer
    AreEqual(state, superTimer)
    are.Set()

def SimpleHandler(sender, args):
    global superTimer
    superTimer = Timer(MyTick)
    superTimer.Change(1000, 0)
        

dlgTst = IronPythonTest.DelegateTest()
dlgTst.Event += SimpleHandler

dlgTst.FireInstance(None, EventArgs.Empty)
are.WaitOne()
superTimer.Dispose()

############################################################
# test various combinations of delegates...

dlgTst = IronPythonTest.DelegateTest()

def Handler(self, args):
    global glblSelf, glblArgs, handlerCalled
    
    AreEqual(self, glblSelf)
    AreEqual(args, glblArgs)
    handlerCalled = True

# check the methods w/ object sender, EventArgs sigs first...
glblSelf = 0
for x in [(IronPythonTest.DelegateTest.StaticEvent, IronPythonTest.DelegateTest.FireStatic), 
           (dlgTst.Event, dlgTst.FireInstance), 
           (IronPythonTest.DelegateTest.StaticGenericEvent, IronPythonTest.DelegateTest.FireGenericStatic), 
           (dlgTst.GenericEvent, dlgTst.FireGeneric)]:
    event, fire = x[0], x[1]
    
    event += Handler
    
    glblSelf = glblSelf + 1
    glblArgs = EventArgs()
    handlerCalled = False
    
    fire(glblSelf, glblArgs)
        
    AreEqual(handlerCalled, True)

def ParamsHandler(self, *args):
    global glblSelf, glblArgs, handlerCalled
    
    AreEqual(self, glblSelf)
    AreEqual(args, tuple(range(glblArgs)))
    handlerCalled = True

for x in [(IronPythonTest.DelegateTest.StaticParamsEvent, IronPythonTest.DelegateTest.FireParamsStatic), 
           (dlgTst.ParamsEvent, dlgTst.FireParams)]:
    event, fire = x[0], x[1]
    
    event += ParamsHandler
    
    glblSelf = glblSelf + 1
    
    for x in range(6):
        handlerCalled = False
        
        glblArgs = x
        fire(glblSelf, *tuple(range(x)))
        
        AreEqual(handlerCalled, True)

def BigParamsHandler(self, a, b, c, d, *args):
    global glblSelf, glblArgs, handlerCalled
    
    AreEqual(self, glblSelf)
    AreEqual(args, tuple(range(glblArgs)))
    AreEqual(a, 1)
    AreEqual(b, 2)
    AreEqual(c, 3)
    AreEqual(d, 4)
    handlerCalled = True

for x in [(IronPythonTest.DelegateTest.StaticBigParamsEvent, IronPythonTest.DelegateTest.FireBigParamsStatic), 
           (dlgTst.BigParamsEvent, dlgTst.FireBigParams)]:
    event, fire = x[0], x[1]
    
    event += BigParamsHandler
    
    glblSelf = glblSelf + 1
    
    for x in range(6):
        handlerCalled = False
        
        glblArgs = x
        fire(glblSelf, 1, 2, 3, 4, *tuple(range(x)))
        
        AreEqual(handlerCalled, True)

# out param
def OutHandler(sender):
    global glblSelf, handlerCalled
    
    AreEqual(sender, glblSelf)
    handlerCalled = True
    
    return 23
    
for x in [(IronPythonTest.DelegateTest.StaticOutEvent, IronPythonTest.DelegateTest.FireOutStatic), 
           (dlgTst.OutEvent, dlgTst.FireOut)]:
    event, fire = x[0], x[1]
    
    event += OutHandler
    
    glblSelf = glblSelf + 1
    
    handlerCalled = False
    
    AreEqual(fire(glblSelf), 23)
    
    AreEqual(handlerCalled, True)

# ref param
def RefHandler(sender, refArg):
    global glblSelf, handlerCalled
    
    AreEqual(sender, glblSelf)
    AreEqual(refArg, 42)
    handlerCalled = True
    
    return 23
    
for x in [(IronPythonTest.DelegateTest.StaticRefEvent, IronPythonTest.DelegateTest.FireRefStatic), 
           (dlgTst.RefEvent, dlgTst.FireRef)]:
    event, fire = x[0], x[1]
    
    event += RefHandler
    
    glblSelf = glblSelf + 1
    
    handlerCalled = False
    
    AreEqual(fire(glblSelf, 42), 23)
    
    AreEqual(handlerCalled, True)

# out w/ return type
def OutHandler(sender):
    global glblSelf, handlerCalled
    
    AreEqual(sender, glblSelf)
    handlerCalled = True
    
    return ("23", 42)
    
for x in [(IronPythonTest.DelegateTest.StaticOutReturnEvent, IronPythonTest.DelegateTest.FireOutReturnStatic), 
           (dlgTst.OutReturnEvent, dlgTst.FireOutReturn)]:
    event, fire = x[0], x[1]
    
    event += OutHandler
    
    glblSelf = glblSelf + 1
    
    handlerCalled = False
    
    AreEqual(fire(glblSelf), ("23", 42))
    
    AreEqual(handlerCalled, True)
    
# ref w/ a return type
def RefHandler(sender, refArg):
    global glblSelf, handlerCalled
    
    AreEqual(sender, glblSelf)
    AreEqual(refArg, 42)
    handlerCalled = True
    
    return (23, 42)
    
for x in [(IronPythonTest.DelegateTest.StaticRefReturnEvent, IronPythonTest.DelegateTest.FireRefReturnStatic), 
           (dlgTst.RefReturnEvent, dlgTst.FireRefReturn)]:
    event, fire = x[0], x[1]
    
    event += RefHandler
    
    glblSelf = glblSelf + 1
    
    handlerCalled = False
    
    AreEqual(fire(glblSelf, 42), (23, 42))
    
    AreEqual(handlerCalled, True)


#######

def identity(x): return x

r = IronPythonTest.ReturnTypes()
r.floatEvent += identity

AreEqual(r.RunFloat(1.4), 1.4)

################################################################################################
# verify bound / unbound methods go to the write delegates...
# ParameterizedThreadStart vs ThreadStart is a good example of this, we have a delegate
# that takes a parameter, and one that doesn't, and we need to correctly disambiguiate

class foo(object):
    def bar(self):
        global called, globalSelf
        called = True
        globalSelf = self
    def baz(self, arg):
        global called, globalSelf, globalArg
        called = True
        globalSelf = self
        globalArg = arg

from System.Threading import Thread, ThreadStart, ParameterizedThreadStart

# try parameterized thread

a = foo()
t = Thread(ParameterizedThreadStart(foo.bar))
t.Start(a)
t.Join()

AreEqual(called, True)
AreEqual(globalSelf, a)

# try non-parameterized
a = foo()
called = False

t = Thread(ThreadStart(a.bar))
t.Start()
t.Join()

AreEqual(called, True)
AreEqual(globalSelf, a)


# parameterized w/ self
a = foo()
called = False

t = Thread(ParameterizedThreadStart(a.baz))
t.Start('hello')
t.Join()

AreEqual(called, True)
AreEqual(globalSelf, a)
AreEqual(globalArg, 'hello')


# parameterized w/ self & extra arg, should throw

try:
    t = Thread(ParameterizedThreadStart(foo.baz))
    AreEqual(True, False)
except TypeError: pass


# SuperDelegate Tests

import sys
import System
from lib.assert_util import *
load_iron_python_test()

import IronPythonTest

class TestCounter:
    myhandler = 0
    myhandler2 = 0
    trivial_d = 0
    trivial_d2 = 0
    string_d = 0
    enum_d = 0
    long_d = 0
    complex_d = 0
    structhandler = 0

counter = TestCounter()

class MyHandler:
    def __init__(self, x):
        self.y = x
    def att(self, s1):
        Assert(self.y == "Hi from Python!")
        Assert(s1 == "string value")
        counter.myhandler += 1
        return ""
    def att2(self, s1):
        Assert(self.y == "Hi from Python!")
        Assert(s1 == "second string value")
        counter.myhandler2 += 1
        return ""

def handle_struct(s):
    Assert(s.intVal == 12345)
    counter.structhandler += 1
    return s

def att(s1):
    Assert(s1 == "string value")
    counter.trivial_d += 1
    return ""

def att2(s1):
    Assert(s1 == "second string value")
    counter.trivial_d2 += 1
    return ""

def ats(s1, s2, s3, s4, s5, s6):
    Assert(s1 == "string value")
    Assert(s2 == "second string value")
    Assert(s3 == "string value")
    Assert(s4 == "second string value")
    Assert(s5 == "string value")
    Assert(s6 == "second string value")
    counter.string_d += 1
    return ""

def ate(e):
    Assert(e == IronPythonTest.DeTestEnum.Value_2)
    counter.enum_d += 1
    return 1;

def atel(e):
    Assert(e == IronPythonTest.DeTestEnumLong.Value_1)
    counter.long_d += 1
    return 1;

def atc(s, i, f, d, e, s2, e2, e3):
    Assert(s == "string value")
    Assert(i == 12345)
    Assert(f == 3.5)
    Assert(d == 3.141592653)
    Assert(e == IronPythonTest.DeTestEnum.Value_2)
    Assert(s2 == "second string value")
    Assert(e2 == IronPythonTest.DeTestEnum.Value_2)
    Assert(e3 == IronPythonTest.DeTestEnumLong.Value_1)
    counter.complex_d += 1
    return s

d = IronPythonTest.DeTest()

d.stringVal = "string value"
d.stringVal2 = "second string value"
d.intVal    = 12345
d.floatVal  = 3.5
d.doubleVal = 3.141592653
d.enumVal   = IronPythonTest.DeTestEnum.Value_2
d.longEnumVal = IronPythonTest.DeTestEnumLong.Value_1

d.e_tt += att
d.e_tt2 += att2
d.e_tsd += ats
d.e_ted += ate
d.e_tedl += atel
d.e_tcd += atc
d.RunTest()

c = MyHandler("Hi from Python!")
d.e_tt += c.att
d.e_tt2 += c.att2
d.e_struct += handle_struct
d.RunTest()

Assert(counter.myhandler == 1)
Assert(counter.myhandler2 == 1)
Assert(counter.trivial_d == 2 )
Assert(counter.trivial_d2 == 2 )
Assert(counter.string_d == 2)
Assert(counter.enum_d == 2)
Assert(counter.long_d == 2)
Assert(counter.complex_d == 2)
Assert(counter.structhandler == 1)

d.e_tt -= att
d.e_tt2 -= att2
d.e_tsd -= ats
d.e_ted -= ate
d.e_tedl -= atel
d.e_tcd -= atc
d.e_tt -= c.att
d.e_tt2 -= c.att2
d.e_struct -= handle_struct

d.RunTest()

# even though they're different methods we should have succeeded at removing them
Assert(counter.myhandler == 1)
Assert(counter.myhandler2 == 1)

#All the rest of the event handlers are removed correctly
Assert(counter.trivial_d == 2 )
Assert(counter.trivial_d2 == 2 )
Assert(counter.string_d == 2)
Assert(counter.enum_d == 2)
Assert(counter.long_d == 2)
Assert(counter.complex_d == 2)
Assert(counter.structhandler == 1)

###############################################################
##    Event Handler Add / Removal
###############################################################

class C: pass
c = C()
c.flag = 0

do_remove = -1
do_nothing = 0
do_add = 1

def f1(): c.flag += 10
def f2(): c.flag += 20

class D:
    def f1(self): c.flag += 30
    def f2(self): c.flag += 40
d = D()

def Step(obj, action, handler, expected):
    if action == do_remove:
        obj.SimpleEvent -= handler
    elif action == do_add:
        obj.SimpleEvent += handler

    c.flag = 0
    obj.RaiseEvent()
    Assert(c.flag == expected)

def RunSequence(listOfSteps):
    newobj = IronPythonTest.SimpleType()
    expected = 0
    for step in listOfSteps:
        (action, handler, delta) = step
        expected += delta
        Step(newobj, action, handler, expected)

ls = []
ls.append((do_nothing, None, 0))
ls.append((do_add, f1, 10))
ls.append((do_remove, f2, 0)) ## remove not-added handler
ls.append((do_remove, f1, -10))
ls.append((do_remove, f1, 0)) ## remove again

RunSequence(ls)

## Two events add/remove

ls = []
ls.append((do_add, f1, 10))
ls.append((do_add, f2, 20))
ls.append((do_remove, f1, -10))
ls.append((do_remove, f2, -20))
ls.append((do_remove, f1, 0))

RunSequence(ls)

## Two events add/remove (different order)

ls = []
ls.append((do_add, f1, 10))
ls.append((do_add, f2, 20))
ls.append((do_remove, f2, -20))
ls.append((do_remove, f1, -10))

RunSequence(ls)

## Event handler is function in class instance

ls = []
ls.append((do_add, d.f1, 30))
ls.append((do_add, d.f2, 40))
ls.append((do_remove, d.f2, -40))
ls.append((do_remove, d.f1, -30))

RunSequence(ls)

ls = []
ls.append((do_add, d.f2, 40))
ls.append((do_add, d.f1, 30))
ls.append((do_remove, d.f2, -40))
ls.append((do_remove, d.f1, -30))

RunSequence(ls)


ls = []
ls.append((do_add, f1, 10))
ls.append((do_remove, f2, 0))
ls.append((do_add, d.f2, 40))
ls.append((do_add, f2, 20))
ls.append((do_remove, d.f2, -40))
ls.append((do_add, d.f1, 30))
ls.append((do_nothing, None, 0))
ls.append((do_remove, f1, -10))
ls.append((do_remove, d.f1, -30))

RunSequence(ls)

# verify delegates are calling 

from System.Threading import ThreadStart, ParameterizedThreadStart

def myfunc():
    global myfuncCalled
    myfuncCalled = True
    
def myotherfunc(arg):
    global myfuncCalled, passedarg
    myfuncCalled = True
    passedarg = arg

class myclass(object):
    def myfunc(self):
        global myfuncCalled
        myfuncCalled = True
    def myotherfunc(self, arg):
        global myfuncCalled,passedarg 
        myfuncCalled = True
        passedarg = arg

class myoldclass:
    def myfunc(self):
        global myfuncCalled 
        myfuncCalled = True
    def myotherfunc(self, arg):
        global myfuncCalled, passedarg
        myfuncCalled = True
        passedarg = arg

for target in [myfunc, myclass().myfunc, myoldclass().myfunc]:
    myfuncCalled = False
    ThreadStart(target)()
    AreEqual(myfuncCalled, True)

for target in [myotherfunc, myclass().myotherfunc, myoldclass().myotherfunc]:
    myfuncCalled = False
    ParameterizedThreadStart(target)(1)
    AreEqual(myfuncCalled, True)
    AreEqual(passedarg, 1)
    passedarg = None


# verify we can call a delegate that's to a static method
IronPythonTest.DelegateTest.Simple()