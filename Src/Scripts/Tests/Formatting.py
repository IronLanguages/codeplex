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
import datetime


x = datetime.date(2005,3,22)
AreEqual(x.year, 2005)
AreEqual(x.month, 3)
AreEqual(x.day, 22)

AreEqual(x.strftime("%y-%a-%b"), "05-Tue-Mar")
AreEqual(x.strftime("%Y-%A-%B"), "2005-Tuesday-March")

######################################################################################
# Formatting of floats
#

# 12 significant digits near the decimal point

AreEqual(str(12345678901.2), "12345678901.2")
AreEqual(str(1.23456789012), "1.23456789012")

# 12 significant digits near the decimal point, preceeded by upto 3 0s

AreEqual(str(123456789012.00), "123456789012.0")
AreEqual(str(123456789012.0), "123456789012.0")
AreEqual(str(00.123456789012), "0.123456789012")
AreEqual(str(0.000123456789012), "0.000123456789012")

# 12 significant digits near the decimal point, followed by 0s, or preceeded more than 3 0s

AreEqual(str(1234567890120.00), "1.23456789012e+012")
AreEqual(str(0.0000123456789012), "1.23456789012e-005")

# More than 12 significant digits near the decimal point, with rounding down

AreEqual(str(12345678901.23), "12345678901.2")
AreEqual(str(123456789012.3), "123456789012.0")
AreEqual(str(1.234567890123), "1.23456789012")

# More than 12 significant digits near the decimal point, with rounding up

AreEqual(str(12345678901.25), "12345678901.3")
AreEqual(str(123456789012.5), "123456789013.0")
if (sys.platform == "cli"):
    AreEqual(str(1.234567890125), "1.23456789013")
else:
    AreEqual(str(1.234567890125), "1.23456789012")
AreEqual(str(1.234567890126), "1.23456789013")

# Signficiant digits away from the decimal point

AreEqual(str(100000000000.0), "100000000000.0")
AreEqual(str(1000000000000.0), "1e+012")
AreEqual(str(0.0001), "0.0001")
AreEqual(str(0.00001), "1e-005")

# Near the ends of the number line

# System.Double.MaxValue
AreEqual(str(1.79769313486232e+308), "1.#INF")
AreEqual(str(1.79769313486231e+308), "1.79769313486e+308")
# System.Double.MinValue
AreEqual(str(-1.79769313486232e+308), "-1.#INF")
AreEqual(str(-1.79769313486231e+308), "-1.79769313486e+308")
# System.Double.Epsilon
if (sys.platform == "cli"):
    AreEqual(str(4.94065645841247e-324), "4.94065645841e-324")
else:
    AreEqual(str(4.94065645841247e-324), "0.0")
# NaN
AreEqual(str((1.79769313486232e+308 * 2.0) * 0.0), "-1.#IND")

#

AreEqual(str(2.0), "2.0")
AreEqual(str(.0), "0.0")
AreEqual(str(-.0), "0.0")
# verify small strings display all precision by default
x = 123.456E-19 * 2.0
AreEqual(str(x), "2.46912e-017")

######################################################################################

values = [ 
          # 6 significant digits near the decimal point

          (123456, "123456"),
          (123456.0, "123456"),
          (12345.6, "12345.6"),
          (1.23456, "1.23456"),
          (0.123456, "0.123456"),

          # 6 significant digits near the decimal point, preceeded by upto 3 0s

          (0.000123456, "0.000123456"),

          # More than 6 significant digits near the decimal point, with rounding down

          (123456.4, "123456"),
          (0.0001234564, "0.000123456"),

          # More than 6 significant digits near the decimal point, with rounding up

          (123456.5, "123457"),
          (0.0001234565, "0.000123457"),

          # Signficiant digits away from the decimal point

          (100000.0, "100000"),
          (1000000.0, "1e+006"),
          (0.0001, "0.0001"),
          (0.00001, "1e-005")]

for v in values:
    AreEqual("%g" % v[0], v[1])
    AreEqual("%.6g" % v[0], v[1])
    # AreEqual("% .6g" % v[0], v[1])

######################################################################################
# Formatting of System.Single

if is_cli:
    load_iron_python_test()
    import IronPythonTest
    f = IronPythonTest.DoubleToFloat.ToFloat(1.0)
    AreEqual(str(f), "1.0")
    AreEqual("%g" % f, "1")
    f = IronPythonTest.DoubleToFloat.ToFloat(1.1)
    AreEqual(str(f), "1.1")
    AreEqual("%g" % f, "1.1")
    f = IronPythonTest.DoubleToFloat.ToFloat(1.2345678)
    AreEqual(str(f), "1.23457")
    AreEqual("%g" % f, "1.23457")
    f = IronPythonTest.DoubleToFloat.ToFloat(1234567.8)
    AreEqual(str(f), "1.23457e+006")
    AreEqual("%g" % f, "1.23457e+006")

######################################################################################

def formatError():
    "%d" % (1,2)

AssertError(TypeError, formatError, None)

AreEqual(len('%10d' %(1)), 10)

#formatting should accept a float for int formatting...
AreEqual('%d' % 3.7, '3')

AreEqual("%0.3f" % 10.8, "10.800")

# test that %s / %r work correctly

# oldstyle class
class A:
	def __str__(self):
		return "str"
	def __repr__(self):
		return "repr"

class B(object):
	def __str__(self):
		return "str"
	def __repr__(self):
		return "repr"
		
a = A()
AreEqual("%s" % a, "str")
AreEqual("%r" % a, "repr")
b = B()
# BUG 153
#AreEqual("%s" % b, "str")
# /BUG
AreEqual("%r" % b, "repr")

# if str() returns Unicode, so should 
# test character
AreEqual("%c" % 23, chr(23))
AssertError(StandardError, (lambda: "%c" % -1) ) # IP currently raises TypeError, CP raises Overflow
AreEqual("%c" % unicode('x'), unicode('x'))
try:
    AreEqual("%c" % 65535, u'\uffff')
except OverflowError:
	pass
	
# test %i, %u, not covered in test_format
AreEqual('%i' % 23, '23')
AreEqual('%i' % 23.9,  '23')
AreEqual('%+u' % 5, '+5')
AreEqual('%05u' % 3, '00003')
AreEqual('% u' % 19, ' 19')
AreEqual('%*u' % (5,10), '   10')
AreEqual('%e' % 1000000, '1.000000e+006')
AreEqual('%E' % 1000000, '1.000000E+006')

AreEqual('%.2e' % 1000000, '1.00e+006')
AreEqual('%.2E' % 1000000, '1.00E+006')
AreEqual('%.2g' % 1000000, '1e+006')

AreEqual('%G' % 100, '100')


# test named inputs
fmtstr = '%(x)+d -- %(y)s -- %(z).2f'
AreEqual(fmtstr % {'x':9, 'y':'quux', 'z':3.1415}, '+9 -- quux -- 3.14')
AssertError(KeyError, (lambda:fmtstr % {}))
AssertError(KeyError, (lambda:fmtstr % {'x': 3}))
AssertError(TypeError, (lambda:fmtstr % {'x': 'notanint', 'y':'str', 'z':2.1878}))

AreEqual('%(key)s %(yek)d' % {'key':'ff', 'yek':200}, "ff 200")

AreEqual(repr(u"\u00F4"), "u'\\xf4'")
AreEqual(repr(u"\u10F4"), "u'\\u10f4'")

AssertError(TypeError, lambda: "%5c" % None)
AreEqual("%5c" % 'c', '    c')
AreEqual("%+5c" % 'c', '    c')
AreEqual("%-5c" % 'c', 'c    ')

AreEqual("%5s" % None, ' None')
AreEqual("%5s" % 'abc', '  abc')
AreEqual("%+5s" % 'abc', '  abc')
AreEqual("%-5s" % 'abc', 'abc  ')