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

import generate
reload(generate)
from generate import CodeGenerator, CodeWriter

BigIntFunc_One = """
[PythonName("%(name)s")]
public static double %(tname)s(BigInteger v0) {
    double v0d;
    if (v0.TryToFloat64(out v0d)) {
        return %(tname)s(v0d);
    }

    throw Ops.OverflowError("long too large to convert to float");
}
"""
BigIntFunc_Two =  """
[PythonName("%(name)s")]
public static double %(tname)s(BigInteger v0, BigInteger v1) {
    double v0d, v1d;
    if (v0.TryToFloat64(out v0d) && v1.TryToFloat64(out v1d)) {
        return %(tname)s(v0d, v1d);
    }

    throw Ops.OverflowError("long too large to convert to float");
}
"""

class Func:
    def __init__(self, name, args=1, cname=None):
        self.name = name
        self.args = args
        if cname is None:
            cname = name.capitalize()
        self.cname = cname

    def write(self, cw):
        params = ["double v%d" % i for i in range(self.args)]
        args = ["v%d" % i for i in range(self.args)]
        cw.write('[PythonName("%s")]' % self.name)
        cw.enter_block("public static double %s(%s)" %
                       (self.name.title(), ", ".join(params)))
        cw.write("return Check(Math.%s(%s));" %
                 (self.cname, ", ".join(args)))
        cw.exit_block()
        
        if self.args == 2:   cw.write(BigIntFunc_Two % {'name' : self.name, 'tname': self.name.title()})
        elif self.args == 1: cw.write(BigIntFunc_One % {'name' : self.name, 'tname': self.name.title()})
        else:                raise Exception, "no template for %d args" % self.args

#Func('fmod', 2), Func('modf'),
#Func('frexp'),Func('hypot', 2), Func('ldexp', 2),

funcs = [
    Func('acos'), Func('asin'), Func('atan'), Func('atan2', 2),
    Func('ceil', 1, 'Ceiling'), Func('cos'), Func('cosh'), Func('exp'),
    Func('fabs', 1, 'Abs'), Func('floor'),
    Func('log'), Func('log', 2), Func('log10'),
    Func('pow', 2), Func('sin'), Func('sinh'),
    Func('sqrt'), Func('tan'), Func('tanh'),
]

def gen_funcs(cw):
    for func in funcs:
        func.write(cw)

CodeGenerator("math functions", gen_funcs).doit()

