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

from generate import CodeGenerator
import operator

class TypeInfo:
    def __init__(self, name, ops, min, max, gen, fp, next):
        self.name = name
        self.ops = ops
        self.min = min
        self.max = max
        self.gen = gen
        self.fp  = fp
        self.next = next            # use this type as next order operation type

        self.signed = min < 0
        self.size = max - min

    def __le__(self, other):
        return self.min >= other.min and self.max <= other.max

# types


#                      name         ops,        min                             max                             gen     float  next

byte_type   = TypeInfo('Byte' ,      "Byte",     0,                              255,                            True,   False, True)
sbyte_type  = TypeInfo('SByte',      "SByte",   -128,                            127,                            True,   False, True)
int16_type  = TypeInfo('Int16',      "Int16",   -32768,                          32767,                          True,   False, True)
uint16_type = TypeInfo('UInt16',     "UInt16",   0,                              65535,                          True,   False, True)
int32_type  = TypeInfo('Int32',      "Int",     -2147483648,                     2147483647,                     False,  False, True)
uint32_type = TypeInfo('UInt32',     "UInt32",   0 ,                             4294967295,                     True,   False, True)
int64_type  = TypeInfo('Int64',      "Int64",   -9223372036854775808,            9223372036854775807,            False,  False, True)
uint64_type = TypeInfo('UInt64',     "UInt64",   0,                              18446744073709551615,           True,   False, True)

single_type = TypeInfo('Single',    "Single",    -3.40282e+038,                   3.40282e+038,                   True,   True, False),
double_type = TypeInfo('Double',    "Float",     -1.79769313486e+308,             1.79769313486e+308,             False,  True, False),
deciml_type = TypeInfo('Decimal',    "Decimal", -79228162514264337593543950335,  79228162514264337593543950335,  False,  True, False),



int_types = [ byte_type, sbyte_type, int16_type, uint16_type, int32_type, uint32_type, int64_type, uint64_type]

fp_types = [
]

complex_types = [
    TypeInfo("Complex64",   "Complex64", -1.79769313486e+308-1.79769313486e+308j, 1.79769313486e+308+1.79769313486e+308j, False, True, False),
]

types = int_types + fp_types

def radd(a,b): return b + a
def rand(a,b): return b & a
def rdiv(a,b): return b / a
def rdivmod(a,b): return divmod(b,a)
def rfloordiv(a,b): return b // a
def rlshift(a,b): return b << a
def rmod(a,b): return b % a
def rmul(a,b): return b * a
def ror(a,b): return b | a
def rpow(a,b): return b ** a
def rrshift(a,b): return b >> a
def rsub(a,b): return b - a
def rtruediv(a,b): return operator.__truediv__(b,a)
def rxor(a,b): return b ^ a


# Binary operator which can use symbol directly for its implementation and overflows to the next order
binary_s_o = """%(oper_type)s result = (%(oper_type)s)(((%(oper_type)s)left%(left_type)s) %(symbol)s ((%(oper_type)s)%(right_value)s));
if (%(left_type)s.MinValue <= result && result <= %(left_type)s.MaxValue) {
    return (%(left_type)s)result;
} else return result;"""

# Binary code with float point argument (same as above, except doesn't try to back-fit the result and leaves it as float point)
binary_f = "return (%(oper_type)s)(((%(oper_type)s)left%(left_type)s) %(symbol)s ((%(oper_type)s)%(right_value)s));"

# Binary operator which cannot use symbol ('//') and must call method to implement the operation
binary_m = "return %(oper_ops)sOps.%(method_impl)s((%(oper_type)s)left%(left_type)s, (%(oper_type)s)%(right_value)s);"

# Call to custom code for binary operator
binary_c = """return %(oper_ops)sOps.%(method_impl)s(left%(left_type)s, (%(right_type)s)right);"""

# Binary operator which can use symbol directly for its implementation and overflows to the next order
r_binary_s_o = """%(oper_type)s result = (%(oper_type)s)(((%(oper_type)s)%(right_value)s) %(symbol)s ((%(oper_type)s)left%(left_type)s));
if (%(left_type)s.MinValue <= result && result <= %(left_type)s.MaxValue) {
    return (%(left_type)s)result;
} else return result;"""

# Binary code with float point argument (same as above, except doesn't try to back-fit the result and leaves it as float point)
r_binary_f = "return (%(oper_type)s)(((%(oper_type)s)%(right_value)s) %(symbol)s ((%(oper_type)s)left%(left_type)s));"

class BinaryOp:
    def __init__(self, symbol, name, method, operation, overflow, intcode, altcode, fpcode):
        self.symbol = symbol
        self.name = name
        self.method = method
        self.operation = operation
        self.overflow = overflow
        self.intcode = intcode
        self.altcode = altcode
        self.fpcode  = fpcode


binaries = [
    #        sym  name               Method               operation               overflow,   code           largest type   float
                                                        
    BinaryOp('+', '__add__',        'Add',                operator.__add__,       True,       binary_s_o,     binary_c,     binary_f),
    BinaryOp('/', '__div__',        'Divide',             operator.__div__,       False,      binary_m,       binary_c,     binary_m),
    BinaryOp('//', '__floordiv__',  'FloorDivide',        operator.__floordiv__,  False,      binary_m,       binary_c,     binary_m),
    BinaryOp('%', '__mod__',        'Mod',                operator.__mod__,       False,      binary_m,       binary_c,     binary_m),
    BinaryOp('*', '__mul__',        'Multiply',           operator.__mul__,       True,       binary_s_o,     binary_c,     binary_f),
    BinaryOp('-', '__sub__',        'Subtract',           operator.__sub__,       True,       binary_s_o,     binary_c,     binary_f),

    BinaryOp('+', '__radd__',       'ReverseAdd',         radd,                   True,       r_binary_s_o,   binary_c,     r_binary_f),
    BinaryOp('/', '__rdiv__',       'ReverseDivide',      rdiv,                   False,      binary_m,       binary_c,     binary_m),
    BinaryOp('//', '__rfloordiv__', 'ReverseFloorDivide', rfloordiv,              False,      binary_m,       binary_c,     binary_m),
    BinaryOp('%', '__rmod__',       'ReverseMod',         rmod,                   False,      binary_m,       binary_c,     binary_m),
    BinaryOp('*', '__rmul__',       'ReverseMultiply',    rmul,                   True,       r_binary_s_o,   binary_c,     r_binary_f),
    BinaryOp('-', '__rsub__',       'ReverseSubtract',    rsub,                   True,       r_binary_s_o,   binary_c,     r_binary_f),
]

unary = [
    ('__abs__', operator.__abs__, ), 
    ('__neg__', operator.__neg__, ),
    ('__pos__', operator.__pos__, ),
    ('__invert__', operator.__invert__, ), 
]

class BitwiseOp:
    def __init__(self, symbol, name, method):
        self.symbol = symbol
        self.name = name
        self.method = method


bitwise = [
    BitwiseOp('&', '__and__',  'BitwiseAnd'), 
    BitwiseOp('&', '__rand__', 'ReverseBitwiseAnd'),
    BitwiseOp('|', '__or__',   'BitwiseOr'),
    BitwiseOp('|', '__ror__',  'ReverseBitwiseOr'),
    BitwiseOp('^', '__rxor__', 'BitwiseXor'),
    BitwiseOp('^', '__xor__',  'ReverseBitwiseXor'),
]

ops = [
    ('__divmod__', divmod, ), 
    ('__lshift__', operator.__lshift__, ),
    ('__pow__', operator.__pow__, ),
    ('__rdivmod__', rdivmod, ),
    ('__rlshift__', rlshift, ),
    ('__rpow__', rpow, ),
    ('__rrshift__', rrshift, ),
    ('__rshift__', operator.__rshift__, ),
    ('__rtruediv__', rtruediv, ),
    ('__truediv__', operator.__truediv__, ),
]


def get_common_type(l, r, op):
    if l <= r: return r, op.intcode
    if r <= l: return l, op.intcode
    
    for c in types:
        if r <= c and l <= c:
            if c.next or l.fp or r.fp:
                return c, op.intcode

    # 3rd pass - return double
    if l.fp or r.fp: return fp_types[-1], op.altcode

    # still no match - right type larger than left, use right
    # otherwise, use left type
    if r.size > l.size: b = r
    else: b = l

    return b, op.altcode

def get_overflow_type(l, r, op):
    if l.name == "Complex64" or r.name == "Complex64":
        return l, 


    values = [op.operation(ll, rr) for ll in [l.min,l.max] for rr in [r.min, r.max]]
    minv = min(values)
    maxv = max(values)
    for t in types:
        if minv >= t.min and maxv <= t.max:
            # the values fit in the type. Use type if :
            #   - either the type allows it (t.next)
            #   - or one of the types if float point
            if l.fp or r.fp:
                return t, op.fpcode
            if t.next:
                return t, op.intcode

    t, c = get_common_type(l, r, op)
    return t, op.altcode

def get_binop_type(l, r, op):
    if op.overflow:
        return get_overflow_type(l,r,op)
    else:
        return get_common_type(l,r,op)

def get_bitwise_type(l, r):
    if l <= r: return r
    if r <= l: return l

    for c in types:
        if not c.fp and r <= c and l <= c:
            return c

    if (l.signed or r.signed):
        for c in types:
            if not c.fp and c.signed:
                if c.size >= r.size and c.size >= l.size: return c
    else:
        for c in types:
            if not c.fp and not c.signed:
                if c.size >= r.size and c.size >= l.size: return c

    if l.size < r.size: return r
    if r.signed: return r
    return l

def get_unsigned_type(t):
    if not t.signed: return t
    
    size = t.size
    for u in types:
        if not u.signed and u.size == size: return u

def find_type_include(*l):
    for t in types:
        for v in l:
            if t.min > v or t.max < v:
                break
        else:
            return t

def gen_binary_prologue(cw, bin, left):
    cw.write("[PythonName(\"%(python_name)s\")]", python_name=bin.name)
    cw.enter_block("public static object %(method_name)s(object left, object right)", method_name=bin.method)
    cw.write(
        "Debug.Assert(left is %(left_type)s);\n" +
        "%(left_type)s left%(left_type)s = (%(left_type)s)left;",
        left_type = left.name)
    cw.write("IConvertible rightConvertible;")
    cw.enter_block("if ((rightConvertible = right as IConvertible) != null)")
    cw.enter_block("switch (rightConvertible.GetTypeCode())")

def gen_binary_epilogue(cw):
    cw.exit_block()
    cw.exit_block()
    cw.write("return Ops.NotImplemented;")
    cw.exit_block()

def gen_binaries(cw, left):
    for bin in binaries:
        gen_binary_prologue(cw, bin, left)
        for right in types:
            cw.enter_block("case TypeCode.%(right_type)s:" % {'right_type' : right.name })
            cw.indent += 1

            ot, code = get_binop_type(left, right, bin)

            kw = {
                'left_type'     : left.name,
                'right_type'    : right.name,
                'symbol'        : bin.symbol,
                'method'        : bin.method,
                'left_ops'      : left.ops,
                'right_ops'     : right.ops,
                'oper_type'     : ot.name,
                'oper_ops'      : ot.ops,
            }
            
            kw["right_value"] = "((%(right_type)s)right)" % kw
            
            # For existing OpsXXX, call the method with the same name,
            # For new ops, call with Impl suffix to prevent stack overflows
            if ot.gen: kw['method_impl'] = bin.method + "Impl"
            else: kw['method_impl'] = bin.method

            cw.write(code % kw)
            cw.exit_block()
            cw.indent -= 1

        cw.exit_block()
#        cw.else_block("if (right is Complex64)")
#        cw.else_block("if (right is BigInteger)")
#        cw.else_block("if (right is ExtensibleInt)")
#        cw.else_block("if (right is ExtensibleFloat)")
#        cw.else_block("if (right is ExtensibleComplex)")
        cw.exit_block()
        cw.write("return Ops.NotImplemented;")
        cw.exit_block()

def get_cast(f, t):
    if f.name == t.name: return ""
    
    cast = "(%s)" % t.name
    
    return cast
    
        
def gen_bitwise(cw, left):
    # no bitwise for floats
    if left.fp: return

    for bin in bitwise:
        gen_binary_prologue(cw, bin, left)
        for right in types:
            if right.fp: continue
            cw.enter_block("case TypeCode.%(right_type)s:" % {'right_type' : right.name })
            cw.indent += 1

            ot = get_bitwise_type(left, right)

            kw = {
                'left_type'         : left.name,
                'right_type'        : right.name,
                'oper_type'         : ot.name,
                'symbol'            : bin.symbol,
                'method'            : bin.method
            }

            ltype = left
            rtype = right
            
            lval = "left%(left_type)s" % kw
            rval = "(%(right_type)s)right" % kw
            
            kw["left_name"]  = lval
            kw["right_name"] = rval
            
            if ltype.size < ot.size:
                cw.write("%(oper_type)s left%(oper_type)s = (%(oper_type)s)%(left_name)s;" % kw)
                kw["left_name"] = lval = "left%(oper_type)s" % kw
                ltype = ot
            if rtype.size < ot.size:
                cw.write("%(oper_type)s right%(oper_type)s = (%(oper_type)s)%(right_name)s;" % kw)
                kw["right_name"] = rval = "right%(oper_type)s" % kw
                rtype = ot

            lc = get_cast(ltype, ot)
            rc = get_cast(rtype, ot)
            
            kw["left_cast"]  = lc
            kw["right_cast"] = rc

            code = "return %(left_cast)s%(left_name)s %(symbol)s %(right_cast)s%(right_name)s;"
            
            cw.write(code % kw)
            
            cw.exit_block()
            cw.indent -= 1            

        gen_binary_epilogue(cw)


div_impl_code_unsigned = """internal static object DivideImpl(%(type_name)s x, %(type_name)s y) {
    %(type_name)s q = (%(type_name)s)(x / y);
    if (x >= 0) {
        if (y > 0) return q;
        else if (x %% y == 0) return q;
        else return q - 1;
    } else {
        if (y > 0) {
            if (x %% y == 0) return q;
            else return q - 1;
        } else return q;
    }
}
"""

div_impl_code_signed = """internal static object DivideImpl(%(type_name)s x, %(type_name)s y) {
    // special case (MinValue / -1) doesn't fit
    if (x == %(type_name)s.MinValue && y == -1) {
        return (%(bigger_type)s)((%(bigger_type)s)%(type_name)s.MaxValue + 1);
    }
    %(type_name)s q = (%(type_name)s)(x / y);
    if (x >= 0) {
        if (y > 0) return q;
        else if (x %% y == 0) return q;
        else return q - 1;
    } else {
        if (y > 0) {
            if (x %% y == 0) return q;
            else return q - 1;
        } else return q;
    }
}
"""

reverse_div_impl_code = """internal static object ReverseDivideImpl(%(type_name)s x, %(type_name)s y) {
    return DivideImpl(y, x);
}
"""

mod_impl_code="""internal static object ModImpl(%(type_name)s x, %(type_name)s y) {
    %(type_name)s r = (%(type_name)s)(x %% y);
    if (x >= 0) {
        if (y > 0) return r;
        else if (r == 0) return 0;
        else return r + y;
    } else {
        if (y > 0) {
            if (r == 0) return r;
            else return r + y;
        } else return r;
    }
}
"""

reverse_mod_impl_code = """internal static object ReverseModImpl(%(type_name)s x, %(type_name)s y) {
    return ModImpl(y, x);
}
"""


floordiv_impl_code = """internal static object FloorDivideImpl(%(type_name)s x, %(type_name)s y) {
    return DivideImpl(x, y);
}
"""

reverse_floordiv_impl_code = """internal static object ReverseFloorDivideImpl(%(type_name)s x, %(type_name)s y) {
    return DivideImpl(y, x);
}
"""

def gen_division_impl(cw, t):
    if t.fp: return     # not for float point types
    if t.signed:
        big_type = find_type_include(t.min, -t.min)
        cw.write(div_impl_code_signed, type_name = t.name, bigger_type = big_type.name)
    else:
        cw.write(div_impl_code_unsigned, type_name = t.name)
    cw.write(reverse_div_impl_code, type_name = t.name)
    cw.write(mod_impl_code, type_name = t.name)   
    cw.write(reverse_mod_impl_code, type_name = t.name)   
    cw.write(floordiv_impl_code, type_name = t.name)
    cw.write(reverse_floordiv_impl_code, type_name = t.name)

def gen_make_dynamic_type(cw, t):
    cw.enter_block("public static DynamicType MakeDynamicType()")
    cw.write("return new OpsReflectedType(\"%(type_name)s\", typeof(%(type_name)s), typeof(%(type_name)sOps), null);", type_name = t.name)
    cw.exit_block()
    cw.write("")

class TypeGenerator:
    def __init__(self, t):
        self.t = t

    def __call__(self, cw):
        gen_make_dynamic_type(cw, t)
        gen_binaries(cw, t)
        gen_bitwise(cw, t)
        gen_division_impl(cw, t)


for t in types:
    if t.gen:
        CodeGenerator(t.name + "Ops", TypeGenerator(t)).doit()
