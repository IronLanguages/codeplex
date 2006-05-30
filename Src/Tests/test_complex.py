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

from lib.assert_util import *

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
