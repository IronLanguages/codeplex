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

"module doc"

from Util.Debug import *

AreEqual(__doc__, "module doc")
__doc__ = "new module doc"
AreEqual(__doc__, "new module doc")

def f_1():
    "f 1 doc"
    return __doc__

def f_2():
    __doc__ = "f 2 doc"
    return __doc__

def f_3():
    "f 3 doc"
    __doc__ = "new f 3 doc"
    return __doc__

def f_4():
    return __doc__

class c_1:
    "c 1 doc"
    AreEqual(__doc__, "c 1 doc")

class c_2:
    "c 2 doc"
    AreEqual(__doc__, "c 2 doc")

class c_3:
    "c 3 doc"
    AreEqual(__doc__, "c 3 doc")
    __doc__ = "c 3 doc 2"
    AreEqual(__doc__, "c 3 doc 2")

class c_4:
    __doc__ = "c 4 doc"
    AreEqual(__doc__, "c 4 doc")

class n_1(object):
    "n 1 doc"
    AreEqual(__doc__, "n 1 doc")

class n_2(object):
    "n 2 doc"
    AreEqual(__doc__, "n 2 doc")

class n_3(object):
    "n 3 doc"
    AreEqual(__doc__, "n 3 doc")
    __doc__ = "n 3 doc 2"
    AreEqual(__doc__, "n 3 doc 2")

class n_4(object):
    __doc__ = "n 4 doc"
    AreEqual(__doc__, "n 4 doc")

class d:
    "d doc 1"
    AreEqual(__doc__, "d doc 1")

    def m_1(self):
        "m 1 doc"
        return __doc__

    AreEqual(m_1.__doc__, "m 1 doc")
    AreEqual(__doc__, "d doc 1")
    __doc__ = "d doc 2"
    AreEqual(__doc__, "d doc 2")
    AreEqual(m_1.__doc__, "m 1 doc")

    def m_2(self):
        __doc__ = "m 2 doc"
        return __doc__

    AreEqual(m_2.__doc__, None)
    AreEqual(__doc__, "d doc 2")
    __doc__ = "d doc 3"
    AreEqual(__doc__, "d doc 3")
    AreEqual(m_2.__doc__, None)

    def m_3(self):
        "m 3 doc"
        __doc__ = "new m 3 doc"
        return __doc__

    AreEqual(m_3.__doc__, "m 3 doc")
    AreEqual(__doc__, "d doc 3")
    __doc__ = "d doc 4"
    AreEqual(__doc__, "d doc 4")
    AreEqual(m_3.__doc__, "m 3 doc")

    def m_4(self):
        return __doc__

    AreEqual(m_4.__doc__, None)
    AreEqual(__doc__, "d doc 4")
    __doc__ = "d doc 5"
    AreEqual(__doc__, "d doc 5")
    AreEqual(m_4.__doc__, None)


AreEqual(f_1.__doc__, "f 1 doc")
AreEqual(f_2.__doc__, None)
AreEqual(f_3.__doc__, "f 3 doc")
AreEqual(f_4.__doc__, None)

AreEqual(c_1.__doc__, "c 1 doc")
AreEqual(c_2.__doc__, "c 2 doc")
AreEqual(c_3.__doc__, "c 3 doc 2")
AreEqual(c_4.__doc__, "c 4 doc")

AreEqual(n_1.__doc__, "n 1 doc")
AreEqual(n_2.__doc__, "n 2 doc")
AreEqual(n_3.__doc__, "n 3 doc 2")
AreEqual(n_4.__doc__, "n 4 doc")

AreEqual(d.__doc__, "d doc 5")
AreEqual(d.m_1.__doc__, "m 1 doc")
AreEqual(d.m_2.__doc__, None)
AreEqual(d.m_3.__doc__, "m 3 doc")
AreEqual(d.m_4.__doc__, None)


result = f_1()
result = f_2()
result = f_3()
result = f_4()

result = c_1()
result = c_2()
result = c_3()
result = c_4()

result = n_1()
result = n_2()
result = n_3()
result = n_4()

dd = d()
result = dd.m_1()
result = dd.m_2()
result = dd.m_3()
result = dd.m_4()
