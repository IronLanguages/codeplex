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

def test_scenarios(templates, cmps):
    values = [3.5, 4.5, 4, 0, -200L, 12345678901234567890]
    for l in values:
        for r in values:
            for t in templates:
                for c in cmps:
                    easy = "%s %s %s" % (l, c, r)
                    inst = t % (l, c, r)
                    #print inst, eval(easy), eval(inst)
                    Assert(eval(easy) == eval(inst))
                   

templates1 = [ "C(%s) %s C(%s)", "C2(%s) %s C2(%s)",
               "C(%s) %s D(%s)", "D(%s) %s C(%s)", 
               "C2(%s) %s D(%s)", "D(%s) %s C2(%s)", 
               "C(%s) %s D2(%s)", "D2(%s) %s C(%s)", 
               "C2(%s) %s D2(%s)", "D2(%s) %s C2(%s)"]
templates2 = [x for x in templates1 if x.startswith('C')]

# OldClass: both C and D define __lt__
class C:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class D:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">"])

# OldClass: C defines __lt__, D does not
class C:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class D:
    def __init__(self, value):
        self.value = value
class C2(C): pass
class D2(D): pass
test_scenarios(templates2, ["<"])

# UserType: both C and D define __lt__
class C(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class D(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">"])

# UserType: C defines __lt__, D does not
class C(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class D(object):
    def __init__(self, value):
        self.value = value
class C2(C): pass
class D2(D): pass
test_scenarios(templates2, ["<"])

# Mixed: both C and D define __lt__
class C:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value

class D(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">"])

# Mixed, with all cmpop
class C(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
    def __gt__(self, other):
        return self.value > other.value
    def __le__(self, other):
        return self.value <= other.value
    def __ge__(self, other):
        return self.value >= other.value

class D:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
    def __gt__(self, other):
        return self.value > other.value
    def __le__(self, other):
        return self.value <= other.value
    def __ge__(self, other):
        return self.value >= other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">", "<=", ">="])

# verify two instances of class compare differently

Assert( (cmp(C(3), C(3)) == 0) == False)        
Assert( (cmp(D(3), D(3)) == 0) == False)        
Assert( (cmp(C2(3), C2(3)) == 0) == False)        
Assert( (cmp(D2(3), D2(3)) == 0) == False)        
      
Assert( (cmp(D(5), C(5)) == 0) == False)        
Assert( (cmp(C(3), C(5)) == -1) == True)        
Assert( (cmp(D2(5), C(3)) == 1) == True)        
Assert( (cmp(D(5), C2(8)) == -1) == True)  

# define __cmp__; do not move this before those above cmp testing
class C:
    def __init__(self, value):
        self.value = value    
    def __cmp__(self, other):
        return self.value - other.value

class D:
    def __init__(self, value):
        self.value = value    
    def __cmp__(self, other):
        return self.value - other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">", ">=", "<="])

Assert( (cmp(C(3), C(3)) == 0) == True)        
Assert( (cmp(C2(3), D(3)) == 0) == True)        
Assert( (cmp(C(3.0), D2(4.6)) > 0) == False)        
Assert( (cmp(D(3), C(4.9)) < 0) == True)        
Assert( (cmp(D2(3), D2(1234567890)) > 0) == False)        

class C(object):
    def __init__(self, value):
        self.value = value    
    def __cmp__(self, other):
        return self.value - other.value

class D(object):
    def __init__(self, value):
        self.value = value    
    def __cmp__(self, other):
        return self.value - other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">", ">=", "<="])

Assert( (cmp(C(3), C(3)) == 0) == True)        
Assert( (cmp(C2(3.4), D(3.4)) == 0) == True)        
Assert( (cmp(C(3.3), D2(4.9232)) > 0) == False)        
Assert( (cmp(D(3L), C(4000000000)) < 0) == True)        
Assert( (cmp(D2(3), D2(4.9)) < 0) == True)        


from Util.Debug import *
load_iron_python_test()
from IronPythonTest import ComparisonTest

def test_comparisons(typeObj):
    class Callback:
        called = False
        def __call__(self, value):
            #print value, expected
            AreEqual(value, expected)
            self.called = True
        def check(self):
            Assert(self.called)
            self.called = False

    cb = Callback()
    ComparisonTest.report = cb
    
    values = [3.5, 4.5, 4, 0]

    for l in values:
        for r in values:
            ctl = typeObj(l)
            ctr = typeObj(r)

            AreEqual(str(ctl), "ct<%s>" % str(l))
            AreEqual(str(ctr), "ct<%s>" % str(r))

            expected = "< on [ct<%s>, ct<%s>]" % (l, r)
            AreEqual(ctl < ctr, l < r)
            cb.check()
            expected = "> on [ct<%s>, ct<%s>]" % (l, r)
            AreEqual(ctl > ctr, l > r)
            cb.check()
            expected = "<= on [ct<%s>, ct<%s>]" % (l, r)
            AreEqual(ctl <= ctr, l <= r)
            cb.check()
            expected = ">= on [ct<%s>, ct<%s>]" % (l, r)
            AreEqual(ctl >= ctr, l >= r)
            cb.check()
            
class ComparisonTest2(ComparisonTest): pass
    
test_comparisons(ComparisonTest)
test_comparisons(ComparisonTest2)

class C:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
    def __gt__(self, other):
        return self.value > other.value     
class C2(C): pass        
D = ComparisonTest
D2 = ComparisonTest2  
test_scenarios(templates1, ["<", ">"])

class C(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
    def __gt__(self, other):
        return self.value > other.value     
class C2(C): pass     

# test_scenarios(templates1, ["<", ">"])

ComparisonTest.report = None
Assert( (cmp(ComparisonTest(5), ComparisonTest(5)) == 0) == False)        
Assert( (cmp(ComparisonTest(5), ComparisonTest(8)) == -1) == True)        
Assert( (cmp(ComparisonTest2(50), ComparisonTest(8)) == 1) == True)        


Assert( (None < None) == False)
Assert( (None > None) == False)
Assert( (None <= None) == True)
Assert( (None >= None) == True)
Assert( (None == "") == False)
Assert( (None != "") == True)
Assert( (None < "") == True)
Assert( (None > "") == False)
Assert( (None <= "") == True)
Assert( (None >= "") == False)

def check(c):
    Assert( (c < None) == False)
    Assert( (c > None) == True)
    Assert( (c <= None) == False)
    Assert( (c >= None) == True)
    Assert( (None < c) == True)
    Assert( (None > c) == False)
    Assert( (None <= c) == True)
    Assert( (None >= c) == False)

class C1: pass
class C2(object): pass
class C3(C2): pass

for x in [C1, C2, C3]:
    check(x())


ignore = '''    
############ Let us get some strange ones ############ 
# both C and D claims bigger
class C:
    def __lt__(self, other):
        return False
class D:
    def __lt__(self, other):
        return False

Assert( (C() < D()) == False )
Assert( (C() > D()) == False )
Assert( (D() < C()) == False )
Assert( (D() > C()) == False )

# C is always larger
class C(object):
    def __lt__(self, other):
        return False
        
        
class D: pass

Assert( (C() < D()) == False )
Assert( (C() > D()) == True )
Assert( (D() < C()) == True )
Assert( (D() > C()) == False )
'''
