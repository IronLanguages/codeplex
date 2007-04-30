#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

"""
This test validates integrity of the operators implemented on the .NET data types
(Byte, SByte, ... UInt64, Single, Double). It does so by evaluating given operator
using standard Python types (int, long, float) and then comparing the results with
results produced by the operators implemented on the .NET types.
"""

from lib.assert_util import *
from operator import add, sub, mul, div, mod, and_, or_, xor, floordiv, truediv, lshift, rshift, neg, pos, abs, invert

if is_cli or is_silverlight:
    from System import Boolean, Byte, UInt16, UInt32, UInt64, SByte, Int16, Int32, Int64, Single, Double
    import clr

class myint(int): pass
class mylong(long): pass
class myfloat(float): pass
class mycomplex(complex): pass

biops = [
    ("add", add),
    ("sub", sub),
    ("mul", mul),
    ("div", div),
    ("floordiv", floordiv),
    ("truediv", truediv),
    ("mod", mod),
    ("pow", pow),
        
    ("and", and_),
    ("or", or_),
    ("xor", xor),
    ("lshift", lshift),
    ("rshift", rshift),
    #("divmod", divmod), !!! not supporting divmod on non-standard types
    ]
    
unops = [
    ("neg", neg),
    ("pos", pos),
    ("abs", abs),
    ("invert", invert),
    ]

def get_clr_values(string, types):
    clr_values = []
    for t in types:
        try:
            r = t.Parse(string)
            clr_values.append(r)
        except:     # do not include values that cannot be parsed as given type
            pass
    return clr_values


# Some values are not generated (myint, mylong, myfloat) because of the semantic difference
# between calling (2L).__div__(single) and (mylong(2L)).__div__(single)

def get_values(values, itypes, ftypes):
    """
    This will return structure of values converted to variety of types as a list of tuples:
    [ ...
    ( python_value, [ all_values ] ),
    ... ]

    all_values: Byte, UInt16, UInt32, UInt64, SByte, Int16, Int32, Int64, Single, Double, myint, mylong, myfloat
    """
    all = []
    for v in values:
        sv  = str(v)
        py  = int(v)
        clr = get_clr_values(sv, itypes)
        clr.append(long(py))
        clr.append(myint(py))
        clr.append(mylong(py))
        all.append( (py, clr) )

        py  = long(v)
        clr = get_clr_values(sv, itypes)
        clr.append(py)
        clr.append(myint(py))
        clr.append(mylong(py))
        all.append( (py, clr) )

        py  = float(v)        
        clr = get_clr_values(sv, ftypes)
        clr.append(myfloat(py))
        all.append( (py, clr) )

        for imag in [0j, 1j, -1j]:
            py = complex(v + imag)
            all.append( (py, [ py, mycomplex(py) ] ) )

    all.append( (True, [ True ] ))
    all.append( (False, [ False ] ))

    return all


def mystr(x):
    if isinstance(x, tuple):
        return "(" + ", ".join(mystr(e) for e in x) + ")"
    elif isinstance(x, Single):
        return str(round(float(str(x)), 3))
    elif isinstance(x, Double):
        return str(round(x, 3))
    else:
        s = str(x)
        if s.endswith("L"): return s[:-1]
        else: return s

def get_message(a, b, op, x_s, x_v, g_s, g_v):
    return """
    Math test failed, operation: %(op)s
    %(op)s( (%(ta)s) (%(a)s), (%(tb)s) (%(b)s) )
    Expected: (%(x_s)s, %(x_v)s)
    Got:      (%(g_s)s, %(g_v)s)
    """ % {
        'ta'  : str(a.GetType()),
        'tb'  : str(b.GetType()),
        'a'   : str(a),
        'b'   : str(b),
        'op'  : str(op),
        'x_s' : str(x_s),
        'x_v' : str(x_v),
        'g_s' : str(g_s),
        'g_v' : str(g_v)
    }

def get_messageun(a, op, x_s, x_v, g_s, g_v):
    return """
    Math test failed, operation: %(op)s
    %(op)s( (%(ta)s) (%(a)s) )
    Expected: (%(x_s)s, %(x_v)s)
    Got:      (%(g_s)s, %(g_v)s)
    """ % {
        'ta'  : str(a.GetType()),
        'a'   : str(a),
        'op'  : str(op),
        'x_s' : str(x_s),
        'x_v' : str(x_v),
        'g_s' : str(g_s),
        'g_v' : str(g_v)
    }

def verify_b(a, b, op, x_s, x_v, g_s, g_v):
    Assert(x_s == g_s, get_message(a, b, op, x_s, x_v, g_s, g_v))

    if x_s:
        # same value
        Assert(mystr(x_v) == mystr(g_v), get_message(a, b, op, x_s, x_v, g_s, g_v))
    else:
        # same exception
        Assert(type(x_v) == type(g_v), get_message(a, b, op, x_s, x_v, g_s, g_v))

def verify_u(a, op, x_s, x_v, g_s, g_v):
    Assert(x_s == g_s, get_messageun(a, op, x_s, x_v, g_s, g_v))
    if x_s:
        # same value
        Assert(mystr(x_v) == mystr(g_v), get_messageun(a, op, x_s, x_v, g_s, g_v))
    else:
        # unary operator should never fail
        Assert(type(x_v) == type(g_v), get_messageun(a, op, x_s, x_v, g_s, g_v))

def calc(op, *args):
    try:
        return True, op(*args)
    except Exception, e:
        return False, e

def verify_implemented_b(implemented, op, a, b):
    if not implemented:
        Fail("Operation not defined: %(op)s( (%(ta)s) (%(a)s), (%(tb)s) (%(b)s) )" % {
            'op' : op, 'ta' : str(a.GetType()), 'tb' : str(b.GetType()), 'a'  : str(a), 'b'  : str(b)
            })

def verify_implemented_u(implemented, op, a):
    if not implemented:
        Fail("Operation not defined: %(op)s( (%(ta)s) (%(a)s))" % { 'op' : op, 'ta' : str(a.GetType()), 'a'  : str(a), })

def extensible(l, r):
    ii = isinstance
    return ii(l, myint) or ii(l, mylong) or ii(l, myfloat) or ii(l, mycomplex) or ii(r, myint) or ii(r, mylong) or ii(r, myfloat) or ii(r, mycomplex)

def validate_binary_ops(all, biops):
    total = 0
    last  = 0
    for l_rec in all:
        for r_rec in all:
            py_l, clr_l = l_rec
            py_r, clr_r = r_rec

            for name, bin in biops:
                x_s, x_v = calc(bin, py_l, py_r)

                for l in clr_l:
                    for r in clr_r:
                        implemented = False

                        # direct binary operator
                        g_s, g_v = calc(bin, l, r)
                        if g_v != NotImplemented:
                            implemented = True
                            verify_b(l, r, name, x_s, x_v, g_s, g_v)
                            total += 1

                        # call __xxx__ and __rxxx__ for all types
                        # l.__xxx__(r)
                        m_name = "__" + name + "__"
                        if hasattr(l, m_name):
                            m = getattr(l, m_name)
                            g_s, g_v = calc(m, r)
                            if g_v != NotImplemented:
                                implemented = True
                                verify_b(l, r, m_name, x_s, x_v, g_s, g_v)
                                total += 1
                                
                        # r.__rxxx__(l)
                        m_name = "__r" + name + "__"
                        if hasattr(r, m_name):
                            m = getattr(r, m_name)
                            g_s, g_v = calc(m, l)
                            if g_v != NotImplemented:
                                implemented = True
                                verify_b(l, r, m_name, x_s, x_v, g_s, g_v)
                                total += 1

                        verify_implemented_b(implemented, name, l, r)

                        if total - last > 10000:
                            print "." ,
                            last = total

    return total

def validate_unary_ops(all):
    total = 0
    for l_rec in all:
        py_l, clr_l = l_rec

        for name, un in unops:
            x_s, x_v = calc(un, py_l)

            for l in clr_l:
                implemented = False

                # direct unary operator
                g_s, g_v = calc(un, l)
                if g_v != NotImplemented:
                    implemented = True
                    verify_u(l, name, x_s, x_v, g_s, g_v)
                    total += 1

                # l.__xxx__()
                m_name = "__" + name + "__"
                if hasattr(l, m_name):
                    m = getattr(l, m_name)
                    g_s, g_v = calc(m)
                    if g_v != NotImplemented:
                        implemented = True
                        verify_u(l, m_name, x_s, x_v, g_s, g_v)
                        total += 1
                            
                verify_implemented_u(implemented, name, l)
    return total

def validate_constructors(values):
    types = [Byte, UInt16, UInt32, UInt64, SByte, Int16, Int32, Int64]
    total = 0
    for value in values:
        for first in types:
            v1 = first(value)
            for second in types:
                v2 = first(second((value)))
            total += 1
    return total

if is_cli or is_silverlight:
    values = [-2, -3, -5, 2, 3, 5, 0]
    itypes = [Byte, UInt16, UInt32, UInt64, SByte, Int16, Int32, Int64]
    ftypes = [Single, Double]
    all = get_values(values, itypes, ftypes)

@skip('win32')
def test_validate_binary_ops1():
    total = validate_binary_ops(all, biops[:(len(biops)+1)/2])
    print total, "tests ran."

@skip('win32')
def test_validate_binary_ops2():
    total = validate_binary_ops(all, biops[(len(biops)+1)/2:])
    print total, "tests ran."

@skip('win32')
def test_validate_unary_ops():
    total = validate_unary_ops(all)
    print total, "tests ran."
    
@skip('win32')
def test_validate_constructors():
    total = validate_constructors(values)
    print total, "tests ran."

if len(sys.argv) == 1 or __name__ != '__main__': 
    run_test(__name__)
elif sys.argv[1] == '1':
    test_validate_binary_ops1()
    test_validate_unary_ops()
    test_validate_constructors()
elif sys.argv[1] == '2': 
    test_validate_binary_ops2()
else:
    print sys.argv
    Fail("unknown args")
