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
from System import *
import clr

load_iron_python_test()

from IronPythonTest import *

test = TypeDescTests()

# new style tests

class bar(int): pass
b = bar(2)

class foo(object): pass
c = foo()


#test.TestProperties(...)

res = test.GetClassName(test)
Assert(res == 'IronPythonTest.TypeDescTests')

#res = test.GetClassName(a)
#Assert(res == 'list')


res = test.GetClassName(c)
Assert(res == 'foo')

res = test.GetClassName(b)
Assert(res == 'bar')

res = test.GetConverter(b)
x = res.ConvertTo(None, None, b, int)
Assert(x == 2)
Assert(type(x) == int)

x = test.GetDefaultEvent(b)
Assert(x == None)

x = test.GetDefaultProperty(b)
Assert(x == None)

x = test.GetEditor(b, object)
Assert(x == None)

x = test.GetEvents(b)
Assert(x.Count == 0)

x = test.GetEvents(b, None)
Assert(x.Count == 0)

x = test.GetProperties(b)
Assert(x.Count > 0)

Assert(test.TestProperties(b, ['__doc__'], []))
b.foobar = 'hello'
Assert(test.TestProperties(b, ['__doc__','foobar'], []))
b.baz = 'goodbye'
Assert(test.TestProperties(b, ['__doc__','foobar', 'baz'], []))
delattr(b, 'baz')
Assert(test.TestProperties(b, ['__doc__','foobar'], ['baz']))
# Check that adding a non-string entry in the dictionary does not cause any grief.
b.__dict__[1] = 1;
Assert(test.TestProperties(b, ['__doc__','foobar'], ['baz']))

#Assert(test.TestProperties(test, ['GetConverter', 'GetEditor', 'GetEvents', 'GetHashCode'] , []))


# old style tests

class foo: pass

a = foo()

Assert(test.TestProperties(a, ['__doc__', '__module__'], []))


res = test.GetClassName(a)
Assert(res == 'foo')


x = test.CallCanConvertToForInt(a)
Assert(x == False)

x = test.GetDefaultEvent(a)
Assert(x == None)

x = test.GetDefaultProperty(a)
Assert(x == None)

x = test.GetEditor(a, object)
Assert(x == None)

x = test.GetEvents(a)
Assert(x.Count == 0)

x = test.GetEvents(a, None)
Assert(x.Count == 0)

x = test.GetProperties(a)
Assert(x.Count > 0)

a.bar = 'hello'

Assert(test.TestProperties(a, ['__doc__', '__module__', 'bar'], []))
delattr(a, 'bar')
Assert(test.TestProperties(a, ['__doc__', '__module__'], ['bar']))

a = a.__class__

Assert(test.TestProperties(a, ['__doc__', '__module__'], []))

a.bar = 'hello'

Assert(test.TestProperties(a, ['__doc__', '__module__','bar'], []))
delattr(a, 'bar')
Assert(test.TestProperties(a, ['__doc__', '__module__'], ['bar']))

x = test.GetClassName(a)
Assert(x == 'IronPython.Runtime.OldClass')

x = test.CallCanConvertToForInt(a)
Assert(x == False)

x = test.GetDefaultEvent(a)
Assert(x == None)

x = test.GetDefaultProperty(a)
Assert(x == None)

x = test.GetEditor(a, object)
Assert(x == None)

x = test.GetEvents(a)
Assert(x.Count == 0)

x = test.GetEvents(a, None)
Assert(x.Count == 0)

x = test.GetProperties(a)
Assert(x.Count > 0)
