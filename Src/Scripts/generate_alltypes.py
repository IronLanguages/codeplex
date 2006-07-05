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
import clr

from System import Boolean, SByte, Byte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal

class ClrType:
    def __init__(self, type, ops, cast, **kw):
        self.name = clr.GetClrType(type).Name
        self.ops  = ops
        self.min  = cast(type.MinValue)
        self.max  = cast(type.MaxValue)
        self.size = self.max - self.min
        self.signed = self.min < 0
        self.fp = cast == float
        self.__dict__.update(kw)
        self.rvalue = "(%s)right" % self.name

class XType:
    def __init__(self, name, ct, **kw):
        self.name = name
        self.ops = ct.ops
        self.min = ct.min
        self.max = ct.max
        self.size = ct.size
        self.fp = ct.fp
        self.__dict__.update(kw)

class CustomType:
    def __init__(self, **kw):
        self.__dict__.update(kw)

def smaller(t1, t2):
    "True iff t1 <= t2"
    if t1 in complex_types:
        return t2 in complex_types
    elif t2 in complex_types:
        return True
    return t1.min >= t2.min and t1.max <= t2.max

# types
bool_type   = CustomType(name="Boolean", ops="Bool", min=0, max=1, size=1, signed=False, fp=False, next=True, rvalue="((Boolean)right ? (%(left_type)s)1 : (%(left_type)s)0)")

byte_type   = ClrType(Byte, "Byte", int, fp=False, next=True)
sbyte_type  = ClrType(SByte, "SByte", int, fp=False, next=True)
int16_type  = ClrType(Int16, "Int16", int, fp=False, next=True)
uint16_type = ClrType(UInt16, "UInt16", int, fp=False, next=True)
int32_type  = ClrType(Int32, "Int", int, fp=False, next=True)
uint32_type = ClrType(UInt32, "UInt32", int, fp=False, next=True)
int64_type  = ClrType(Int64, "Int64",  int, fp=False, next=True)
uint64_type = ClrType(UInt64, "UInt64", int, fp=False, next=True)

single_type = ClrType(Single, "Single", float, fp=True, next=False)
double_type = ClrType(Double, "Float", float, fp=True, next=False)
deciml_type = ClrType(Decimal, "Decimal", float, fp=True, next=False)

bigint_type = CustomType(name='BigInteger', ops="Long", min=-(2**200), max=2**200, size=2**201, fp=False, next=False, rvalue="(BigInteger)right")

# Extensible types
x_int_type  = XType('ExtensibleInt', int32_type, fp=False, next=True, rvalue="((ExtensibleInt)right).value")
x_bigint_type = XType('ExtensibleLong', bigint_type, fp=False, next=False, rvalue="((ExtensibleLong)right).Value")
x_float_type = XType('ExtensibleFloat', double_type, fp=True, next=False, rvalue="((ExtensibleFloat)right).value")

complex_type = CustomType(name="Complex64", ops="Complex", fp=True, next=False,rvalue="(Complex64)right")
x_complex_type = CustomType(name="ExtensibleComplex", ops="Complex", fp=True, next=False,rvalue="((ExtensibleComplex)right).value")

int_types = [ bool_type, byte_type, sbyte_type, int16_type, uint16_type, int32_type, uint32_type, int64_type, uint64_type]
fp_types = [ single_type, double_type ]
complex_types = [ complex_type, x_complex_type ]

generate_types = [ byte_type, sbyte_type, int16_type, uint16_type, uint32_type, uint64_type, single_type ]

types = int_types + fp_types

def radd(a,b): return b + a
def rdiv(a,b): return b / a
def rfloordiv(a,b): return b // a
def rmod(a,b): return b % a
def rmul(a,b): return b * a
def rsub(a,b): return b - a

# Binary operator which can use symbol directly for its implementation and overflows to the next order
binary_s_o = """%(oper_type)s result = (%(oper_type)s)(((%(oper_type)s)%(left_value)s) %(symbol)s ((%(oper_type)s)%(right_value)s));
if (%(result_type)s.MinValue <= result && result <= %(result_type)s.MaxValue) {
    return (%(result_type)s)result;
} else return result;"""

# Binary operator which cannot use symbol ('//') and must call method to implement the operation
binary_m = "return %(oper_ops)sOps.%(method_impl)s((%(oper_type)s)%(left_value)s, (%(oper_type)s)%(right_value)s);"

# Call to custom code for binary operator
binary_c = """return %(oper_ops)sOps.%(method_impl)s(%(left_value)s, %(right_value)s);"""

# Binary operator which can use symbol directly for its implementation and overflows to the next order
r_binary_s_o = """%(oper_type)s result = (%(oper_type)s)(((%(oper_type)s)%(right_value)s) %(symbol)s ((%(oper_type)s)%(left_value)s));
if (%(result_type)s.MinValue <= result && result <= %(result_type)s.MaxValue) {
    return (%(result_type)s)result;
} else return result;"""

class BinaryOp:
    def __init__(self, symbol, name, method, rmethod, operation, overflow, intcode, altcode, fpcode):
        self.symbol = symbol
        self.name = name
        self.method = method
        self.rmethod = rmethod
        self.operation = operation
        self.overflow = overflow
        self.intcode = intcode
        self.altcode = altcode
        self.fpcode  = fpcode

binaries = [
    #        sym  name               Method                RevMethod            operation               overflow,   integer_code    largest type  float
                                                                              
    BinaryOp('+', '__add__',        'Add',                'ReverseAdd',         operator.__add__,       True,       binary_s_o,     binary_c,     binary_m),
    BinaryOp('/', '__div__',        'Divide',             'ReverseDivide',      operator.__div__,       False,      binary_m,       binary_c,     binary_m),
    BinaryOp('//', '__floordiv__',  'FloorDivide',        'ReverseFloorDivide', operator.__floordiv__,  False,      binary_m,       binary_c,     binary_m),
    BinaryOp('%', '__mod__',        'Mod',                'ReverseMod',         operator.__mod__,       False,      binary_m,       binary_c,     binary_m),
    BinaryOp('*', '__mul__',        'Multiply',           'ReverseMultiply',    operator.__mul__,       True,       binary_s_o,     binary_c,     binary_m),
    BinaryOp('-', '__sub__',        'Subtract',           'ReverseSubtract',    operator.__sub__,       True,       binary_s_o,     binary_c,     binary_m),
                                                          
    BinaryOp('+', '__radd__',       'ReverseAdd',         'Add',                radd,                   True,       r_binary_s_o,   binary_c,     binary_m),
    BinaryOp('/', '__rdiv__',       'ReverseDivide',      'Divide',             rdiv,                   False,      binary_m,       binary_c,     binary_m),
    BinaryOp('//', '__rfloordiv__', 'ReverseFloorDivide', 'FloorDivide',        rfloordiv,              False,      binary_m,       binary_c,     binary_m),
    BinaryOp('%', '__rmod__',       'ReverseMod',         'Mod',                rmod,                   False,      binary_m,       binary_c,     binary_m),
    BinaryOp('*', '__rmul__',       'ReverseMultiply',    'Multiply',           rmul,                   True,       r_binary_s_o,   binary_c,     binary_m),
    BinaryOp('-', '__rsub__',       'ReverseSubtract',    'Subtract',           rsub,                   True,       r_binary_s_o,   binary_c,     binary_m),
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

# manually implemented binary ops 
# the purpose of codegen is to find common type to perform operation in
class BinaryOpM:
    def __init__(self, name, method, gen_fp, only_double):
        self.name   = name
        self.method = method
        self.gen_fp = gen_fp
        self.only_double = only_double

manual_ones = [                                         # generate op for float point       # only double
    BinaryOpM('__divmod__',     'DivMod',               True,                               False),
    BinaryOpM('__rdivmod__',    'ReverseDivMod',        True,                               False),
    BinaryOpM('__lshift__',     'LeftShift',            False,                              False),
    BinaryOpM('__rlshift__',    'ReverseLeftShift',     False,                              False),
    BinaryOpM('__pow__',        'Power',                True,                               False),
    BinaryOpM('__rpow__',       'ReversePower',         True,                               False),
    BinaryOpM('__rshift__',     'RightShift',           False,                              False),
    BinaryOpM('__rrshift__',    'ReverseRightShift',    False,                              False),

    # this may need to be manual as well.
    BinaryOpM('__truediv__',    'TrueDivide',           True,                               True),
    BinaryOpM('__rtruediv__',   'ReverseTrueDivide',    True,                               True)
]

# Unary operators are implemented manually, no codegen needed
unary = [
    ('__abs__', operator.__abs__, ), 
    ('__neg__', operator.__neg__, ),
    ('__pos__', operator.__pos__, ),
    ('__invert__', operator.__invert__, ), 
]

def get_common_type(l, r, op):
    if l.name == "Complex64": return l, op.altcode
    if r.name == "Complex64": return r, op.altcode

    if smaller(l, r): return r, op.intcode
    if smaller(r, l): return l, op.intcode
    
    for c in types:
        if smaller(r, c) and smaller(l, c):
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
    # complex is handled in ComplexOps
    if l.name == "Complex64": return l, op.altcode
    if r.name == "Complex64": return r, op.altcode

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

def get_preferred_result_type(l, r):
    if smaller(l, r): return r
    else: return l

def get_binop_type(l, r, op):
    if op.overflow:
        return get_overflow_type(l,r,op)
    else:
        return get_common_type(l,r,op)

def get_binop_bigint_type(r, op):
    if r.fp:
        # float point - go double
        return double_type, op.fpcode
    else:
        # otherwise, stay bigint
        return bigint_type, op.altcode

def get_bitwise_type(l, r):
    if smaller(l, r): return r
    if smaller(r, l): return l

    for c in types:
        if not c.fp and smaller(r, c) and smaller(l, c):
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

def get_rvalue(l, r):
    return r.rvalue % { 'left_type' : l.name }

def get_method_name(ot, method):
    # For existing OpsXXX, call the method with the same name,
    # For new ops, call with Impl suffix to prevent stack overflows
    if ot in generate_types:
        return method + "Impl"
    return method

binary_operator_prologue = """[PythonName(\"%(python_name)s\")]
public static object %(method_name)s(object left, object right) {
    if (!(left is %(left_type)s)) {
        throw Ops.TypeError("'%(python_name)s' requires %(left_type)s, but received {0}", Ops.GetDynamicType(left).__name__);
    }
    %(left_type)s left%(left_type)s = (%(left_type)s)left;
    IConvertible rightConvertible;
    if ((rightConvertible = right as IConvertible) != null) {
        switch (rightConvertible.GetTypeCode()) {"""

def gen_binary_prologue(cw, bin, left):
    cw.write(binary_operator_prologue, python_name=bin.name, method_name=bin.method, left_type=left.name)
    cw.indent(); cw.indent(); cw.indent()

def gen_binary_body(cw, left, right, alt_right, bin):
    ot, code = get_binop_type(left, alt_right, bin)
    # get the type of the two that is the optimal result (unless overflow happens)
    # e.g. Byte + Int  ==> preferably Int, but ban overflow to Long
    rt = get_preferred_result_type(left, alt_right)

    kw = {
        'left_type'     : left.name,
        'right_type'    : right.name,
        'symbol'        : bin.symbol,
        'method'        : bin.method,
        'left_ops'      : left.ops,
        'right_ops'     : right.ops,
        'oper_type'     : ot.name,
        'oper_ops'      : ot.ops,
        'result_type'   : rt.name,
        'left_value'    : "left%s" % left.name,
        'right_value'   : get_rvalue(left, right),
        'method_impl'   : get_method_name(ot, bin.method)
    }

    cw.write(code % kw)

def generate_binop_bigint(cw, left, right, bin):
    ot, code = get_binop_bigint_type(left, bin)
    kw = {
        'left_type'     : left.name,
        'right_type'    : right.name,
        'symbol'        : bin.symbol,
        'method'        : bin.method,
        'left_ops'      : left.ops,
        'right_ops'     : bigint_type.ops,
        'oper_type'     : ot.name,
        'oper_ops'      : ot.ops,
        'left_value'    : "left%s" % left.name,
        'right_value'   : get_rvalue(left, right),
        'method_impl'   : get_method_name(ot, bin.method)
    }

    cw.write(code % kw)

def gen_binaries(cw, left):
    for bin in binaries:
        gen_binary_prologue(cw, bin, left)
        for right in types:
            cw.case_block("case TypeCode.%(right_type)s:", right_type = right.name)
            gen_binary_body(cw, left, right, right, bin)
            cw.exit_case_block()

        cw.exit_block()
        cw.exit_block()
        cw.enter_block("if (right is BigInteger)")
        generate_binop_bigint(cw, left, bigint_type, bin)
        cw.else_block("if (right is Complex64)")
        gen_binary_body(cw, left, complex_type, complex_type, bin)
        cw.else_block("if (right is ExtensibleInt)")
        gen_binary_body(cw, left, x_int_type, int32_type, bin)
        cw.else_block("if (right is ExtensibleLong)")
        generate_binop_bigint(cw, left, x_bigint_type, bin)
        cw.else_block("if (right is ExtensibleFloat)")
        gen_binary_body(cw, left, x_float_type, double_type, bin)
        cw.else_block("if (right is ExtensibleComplex)")
        gen_binary_body(cw, left, x_complex_type, complex_type, bin)
        cw.exit_block()
        cw.write("return Ops.NotImplemented;")
        cw.exit_block()

def get_cast(f, t):
    if f.name == t.name: return ""
    
    cast = "(%s)" % t.name
    
    return cast

def gen_bitwise_body(cw, left, right, alt_right, bin):
    ot = get_bitwise_type(left, alt_right)

    kw = {
        'left_type'         : left.name,
        'right_type'        : right.name,
        'oper_type'         : ot.name,
        'symbol'            : bin.symbol,
        'method'            : bin.method,
        'left_value'        : "left%s" % left.name,
        'right_value'       : get_rvalue(left, right)
    }

    ltype = left
    rtype = right

    if ltype.size < ot.size:
        cw.write("%(oper_type)s left%(oper_type)s = (%(oper_type)s)%(left_value)s;" % kw)
        kw["left_value"] = lval = "left%(oper_type)s" % kw
        ltype = ot
    if rtype.size < ot.size:
        cw.write("%(oper_type)s right%(oper_type)s = (%(oper_type)s)%(right_value)s;" % kw)
        kw["right_value"] = rval = "right%(oper_type)s" % kw
        rtype = ot

    lc = get_cast(ltype, ot)
    rc = get_cast(rtype, ot)

    kw["left_cast"]  = lc
    kw["right_cast"] = rc

    code = "return %(left_cast)s%(left_value)s %(symbol)s %(right_cast)s%(right_value)s;"
    cw.write(code % kw)

def gen_bitwise(cw, left):
    # no bitwise for floats
    if left.fp: return

    for bin in bitwise:
        gen_binary_prologue(cw, bin, left)
        for right in types:
            if right.fp: continue
            cw.case_block("case TypeCode.%(right_type)s:", right_type = right.name)
            gen_bitwise_body(cw, left, right, right, bin)
            cw.exit_case_block()

        cw.exit_block()
        cw.exit_block()
        cw.enter_block("if (right is BigInteger)")
        gen_bitwise_body(cw, left, bigint_type, bigint_type, bin)
        cw.else_block("if (right is ExtensibleInt)")
        gen_bitwise_body(cw, left, x_int_type, int32_type, bin)
        cw.else_block("if (right is ExtensibleLong)")
        gen_bitwise_body(cw, left, x_bigint_type, bigint_type, bin)
        cw.exit_block()
        cw.write("return Ops.NotImplemented;")
        cw.exit_block()

def get_manual_common_type(l, r, op):
    if op.only_double:
        return double_type

    if l.fp == r.fp and smaller(l, r): return r
    if l.fp == r.fp and smaller(r, l): return l

    if not l.fp and not r.fp:
        # common larger integer type
        for c in types:
            if not c.fp and smaller(r, c) and smaller(l, c):
                return c
    else:
        # any common larger type
        for c in types:
            if smaller(r, c) and smaller(l, c):
                return c

    if l.name == "Complex64": return l
    if r.name == "Complex64": return r

    if l.size < r.size: return r
    return l

def gen_manual_ones_body(cw, left, right, alt_right, bin):
    # use the alt_type to determine the type of the operation
    ot = get_manual_common_type(left, alt_right, bin)
    kw = {
        "oper_ops"      : ot.ops,
        "left_type"     : left.name,
        "right_type"    : right.name,
        "left_value"    : "left%s" % left.name,
        "right_value"   : get_rvalue(left, right),
        "method_impl"   : get_method_name(ot, bin.method),
    }

    # use binary custom code
    cw.write(binary_c % kw)

def gen_manual_ones(cw, left):
    for bin in manual_ones:
        # skip if not defined for float point types
        if left.fp and not bin.gen_fp: continue
        gen_binary_prologue(cw, bin, left)
        for right in types:
            # skip if not defined for float point types
            if right.fp and not bin.gen_fp: continue
            cw.case_label("case TypeCode.%(right_type)s:", right_type = right.name)
            gen_manual_ones_body(cw, left, right, right, bin)
            cw.dedent()

        cw.exit_block()
        cw.exit_block()
        cw.enter_block("if (right is BigInteger)")
        gen_manual_ones_body(cw, left, bigint_type, bigint_type, bin)
        cw.else_block("if (right is ExtensibleInt)")
        gen_manual_ones_body(cw, left, x_int_type, int32_type, bin)
        cw.else_block("if (right is ExtensibleLong)")
        gen_manual_ones_body(cw, left, x_bigint_type, bigint_type, bin)
        if bin.gen_fp:
            cw.else_block("if (right is Complex64)")
            gen_manual_ones_body(cw, left, complex_type, complex_type, bin)
            cw.else_block("if (right is ExtensibleFloat)")
            gen_manual_ones_body(cw, left, x_float_type, double_type, bin)
            cw.else_block("if (right is ExtensibleComplex)")
            gen_manual_ones_body(cw, left, x_complex_type, complex_type, bin)
        cw.exit_block()
        cw.write("return Ops.NotImplemented;")
        cw.exit_block()

div_impl_code_unsigned = """internal static object DivideImpl(%(type_name)s x, %(type_name)s y) {
    return (%(type_name)s)(x / y);
}
internal static object ModImpl(%(type_name)s x, %(type_name)s y) {
    return (%(type_name)s)(x %% y);
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
internal static object ModImpl(%(type_name)s x, %(type_name)s y) {
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

implementation_code = """
internal static object DivModImpl(%(type_name)s x, %(type_name)s y) {
    object div = DivideImpl(x, y);
    if (div == Ops.NotImplemented) return div;
    object mod = ModImpl(x, y);
    if (mod == Ops.NotImplemented) return mod;
    return Tuple.MakeTuple(div, mod);
}
internal static object ReverseDivideImpl(%(type_name)s x, %(type_name)s y) {
    return DivideImpl(y, x);
}
internal static object ReverseModImpl(%(type_name)s x, %(type_name)s y) {
    return ModImpl(y, x);
}
internal static object ReverseDivModImpl(%(type_name)s x, %(type_name)s y) {
    return DivModImpl(y, x);
}
internal static object FloorDivideImpl(%(type_name)s x, %(type_name)s y) {
    return DivideImpl(x, y);
}
internal static object ReverseFloorDivideImpl(%(type_name)s x, %(type_name)s y) {
    return DivideImpl(y, x);
}
internal static object ReverseLeftShiftImpl(%(type_name)s x, %(type_name)s y) {
    return LeftShiftImpl(y, x);
}
internal static object ReversePowerImpl(%(type_name)s x, %(type_name)s y) {
    return PowerImpl(y, x);
}
internal static object ReverseRightShiftImpl(%(type_name)s x, %(type_name)s y) {
    return RightShiftImpl(y, x);
}
"""

def gen_implementations(cw, t):
    if t.fp: return     # not for float point types
    if t.signed:
        big_type = find_type_include(t.min, -t.min)
        cw.write(div_impl_code_signed, type_name = t.name, bigger_type = big_type.name)
    else:
        cw.write(div_impl_code_unsigned, type_name = t.name)
    cw.write(implementation_code, type_name = t.name)

make_dynamic_type = """private static ReflectedType %(type_name)sType;
public static DynamicType MakeDynamicType() {
    if (%(type_name)sType == null) {
        OpsReflectedType ort = new OpsReflectedType(\"%(type_name)s\", typeof(%(type_name)s), typeof(%(type_name)sOps), null);
        if (Interlocked.CompareExchange<ReflectedType>(ref %(type_name)sType, ort, null) == null) {
            return ort;
        }
    }
    return %(type_name)sType;
}
"""

def gen_make_dynamic_type(cw, t):
    cw.write(make_dynamic_type, type_name = t.name)

constructor_prologue = """[PythonName("__new__")]
public static object Make(DynamicType cls, object value) {
    if (cls != %(type_name)sType) {
        throw Ops.TypeError(\"%(type_name)s.__new__: first argument must be %(type_name)s type.\");
    }
    IConvertible valueConvertible;
    if ((valueConvertible = value as IConvertible) != null) {
        switch (valueConvertible.GetTypeCode()) {"""

constructor_epilogue = """        }
    }
    if (value is String) {
        return %(type_name)s.Parse((String)value);
    } else if (value is BigInteger) {
        return (%(type_name)s)(BigInteger)value;
    } else if (value is ExtensibleInt) {
        return (%(type_name)s)((ExtensibleInt)value).value;
    } else if (value is ExtensibleLong) {
        return (%(type_name)s)((ExtensibleLong)value).Value;
    } else if (value is ExtensibleFloat) {
        return (%(type_name)s)((ExtensibleFloat)value).value;"""

constructor_integer_addition = """    } else if (value is Enum) {
        return Converter.CastEnumTo%(type_name)s(value);"""
constructor_end = """    }
    throw Ops.ValueError("invalid value for %(type_name)s.__new__");
}
"""

def gen_constructor(cw, t):
    cw.write(constructor_prologue, type_name = t.name)
    cw.indent(); cw.indent(); cw.indent()
    for right in types:
        if right.name == "Boolean": continue    # not from Boolean
        cw.case_label("case TypeCode.%(right_type)s: return (%(type_name)s)(%(right_type)s)value;", right_type = right.name, type_name = t.name)
        cw.dedent()
    cw.dedent(); cw.dedent(); cw.dedent()
    cw.write(constructor_epilogue, type_name = t.name)
    if not t.fp:
        cw.write(constructor_integer_addition, type_name = t.name)
    cw.write(constructor_end, type_name = t.name)

class TypeGenerator:
    def __init__(self, t):
        self.t = t

    def __call__(self, cw):
        gen_make_dynamic_type(cw, t)
        gen_constructor(cw, t)
        gen_binaries(cw, t)
        gen_bitwise(cw, t)
        gen_manual_ones(cw, t)
        gen_implementations(cw, t)

for t in generate_types:
    CodeGenerator(t.name + "Ops", TypeGenerator(t)).doit()
