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
from iptest.type_util import *
from iptest.warning_util import warning_trapper

def test_from_string():
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


def test_misc():
    AreEqual(mycomplex(), complex())
    a = mycomplex(1)
    b = mycomplex(1,0)
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

    Assert(complex(2) == complex(2, 0))
    
def test_inherit():
    class mycomplex(complex): pass
    
    a = mycomplex(2+1j)
    AreEqual(a.real, 2)
    AreEqual(a.imag, 1)


def test_repr():
    AreEqual(repr(1-6j), '(1-6j)')
    

def test_infinite():
    AreEqual(repr(1.0e340j),  'inf*j')
    AreEqual(repr(-1.0e340j),'-inf*j')

# Test must sort alphabetically ahead of other uses of the deprecated functions
def test_deprecations():
    w = warning_trapper()
    a = 3j
    b = 5j
    c = 2j
    x = a // 4
    x = b % c
    x = divmod(b, a)
    m = w.finish()
    
    #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=21921
    #AreEqual(len(m), 3)
    #for msg in m:
    #    AreEqual(msg.category, DeprecationWarning)
    #    AreEqual(msg.message, 'complex divmod(), // and % are deprecated')

run_test(__name__)
