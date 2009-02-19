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

from generate import generate

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
        cw.enter_block("public static double %s(%s)" %
                       (self.name, ", ".join(params)))
        cw.write("return Check(Math.%s(%s));" %
                 (self.cname, ", ".join(args)))
        cw.exit_block()
        
#Func('fmod', 2), Func('modf'),
#Func('frexp'),Func('hypot', 2), Func('ldexp', 2),

funcs = [
    Func('acos'), Func('asin'), Func('atan'), Func('atan2', 2),
    Func('ceil', 1, 'Ceiling'), Func('cos'), Func('cosh'), Func('exp'),
    Func('fabs', 1, 'Abs'), Func('floor'),
    Func('pow', 2), Func('sin'), Func('sinh'),
    Func('sqrt'), Func('tan'), Func('tanh'),
]

def gen_funcs(cw):
    for func in funcs:
        func.write(cw)

def main():
    return generate(
        ("math functions", gen_funcs)
    )

if __name__ == "__main__":
    main()