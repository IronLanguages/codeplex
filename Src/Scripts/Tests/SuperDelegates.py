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

import sys
import System
from Util.Debug import *
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