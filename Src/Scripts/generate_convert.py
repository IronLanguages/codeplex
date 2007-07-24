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

from generate import CodeGenerator, CodeWriter

# The TryConvertToXXX methods are to be ultimately removed. Do not add new ones.
helper_method_types = [
    "Byte",
    "SByte",
    "Int16",
    "Int32",
    "Int64",
    "UInt16",
    "UInt32",
    "UInt64",
    "Double",
    "BigInteger",
    "Complex64",
    "String",
    "Char",
]

helper_methods="""
/// <summary>
/// Conversion routine TryConvertTo%(to_type)s - converts object to %(to_type)s
/// Try to avoid using this method, the goal is to ultimately remove it!
/// </summary>
internal static bool TryConvertTo%(to_type)s(object value, out %(to_type)s result) {
    try {
        result = ConvertTo%(to_type)s(value);
        return true;
    } catch {
        result = default(%(to_type)s);
        return false;
    }
}"""

def converter_generator(cw):
    for to_type in helper_method_types:
        cw.write(helper_methods, to_type = to_type)

import clr
from System import Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal

class EnumType:
    def __init__(self, base):
        self.name = clr.GetClrType(base).Name
        self.min = int(base.MinValue)
        self.max = int(base.MaxValue)
        self.signed = self.min < 0

    def __repr__(self):
        return "EnumType %s" % self.name

enum_types = [
    EnumType(Int32),
    EnumType(Byte),
    EnumType(SByte),
    EnumType(Int16),
    EnumType(Int64),
    EnumType(UInt16),
    EnumType(UInt32),
    EnumType(UInt64),
]

def genenum(cw, et, v):
    cw.write("""/// <summary>
/// Explicit conversion of Enum to %(to_type)s
/// </summary>""", to_type = et.name
    )
    cw.enter_block("internal static %(to_type)s CastEnumTo%(to_type)s(object value)", to_type = et.name)
    cw.write("Debug.Assert(value is Enum);")
    cw.enter_block("switch (((Enum)value).GetTypeCode())")
    for i in v:
        cw.case_label("case TypeCode.%s:" % i.name)
        if i != et:
            code = "return (%(to_type)s)(%(from_type)s)value;"
        else:
            code = "return (%(from_type)s)value;"
        cw.write(code, from_type=i.name, to_type=et.name)
        cw.dedent()

    cw.exit_block()
    cw.write("""// Should never get here
Debug.Assert(false, "Invalid enum detected");
return default(%(to_type)s);""", to_type=et.name)
    cw.exit_block()

def enum_converter_generator(cw):
    for et in enum_types:
        genenum(cw, et, enum_types)

    cw.enter_block("internal static Boolean CastEnumToBoolean(object value)")
    cw.write("Debug.Assert(value is Enum);")
    cw.enter_block("switch (((Enum)value).GetTypeCode())")

    for i in enum_types:
        cw.writeline("case TypeCode.%s:" % i.name)
        cw.writeline("    return (%s)value != 0;" % i.name)

    cw.exit_block()
    cw.write("""// Should never get here
Debug.Assert(false, "Invalid enum detected");
return default(Boolean);""", to_type=et.name)
    cw.exit_block()

float_types = [ Single, Double ]

class ClrType:
    def __init__(self, t):
        self.name = clr.GetClrType(t).Name
        self.type = self.name
        self.min = int(t.MinValue)
        self.max = int(t.MaxValue)
        self.signed = self.min < 0
        self.int = not t in float_types
        self.value = "value"
        
class ExType:
    def __init__(self, name, ct):
        self.name = name
        self.type = clr.GetClrType(ct).Name
        self.min = int(ct.MinValue)
        self.max = int(ct.MaxValue)
        self.signed = self.min < 0
        self.int = not ct in float_types
        self.value = "((%s)value).Value" % name

class CType:
    def __init__(self, name, signed, int, value="value"):
        self.name = name
        self.type = name
        self.signed = signed
        self.int    = int
        self.value  = value

x_int_type   = ExType("Extensible<int>", Int32)
x_float_type = ExType("Extensible<double>", Double)

bool_type    = CType("Boolean", False, True)
bigint_type  = CType("BigInteger", True, True)
x_long_type  = CType("Extensible<BigInteger>", True, True, "((Extensible<BigInteger>)value).Value")


byte_type   = ClrType(Byte)
sbyte_type  = ClrType(SByte)
int16_type  = ClrType(Int16)
int32_type  = ClrType(Int32)
int64_type  = ClrType(Int64)
uint16_type = ClrType(UInt16)
uint32_type = ClrType(UInt32)
uint64_type = ClrType(UInt64)
single_type = ClrType(Single)
double_type = ClrType(Double)
decimal_type = ClrType(Decimal)

# These are directly spelled to keep the right order
from_types = [ int32_type, bool_type, bigint_type, double_type, x_int_type, x_long_type, x_float_type, int64_type,
             byte_type,  sbyte_type, int16_type, uint16_type, uint32_type, uint64_type, single_type, decimal_type ]
to_types =   [ byte_type, sbyte_type, int16_type, uint16_type, int32_type, uint32_type, int64_type, uint64_type,
               single_type, double_type, decimal_type, bigint_type ]

# types in to_types which are reference types - will generate null conversion for those
reference_types = [ bigint_type ]

# types which don't have conversions using the ConvertToXXX => ConvertToXXXImpl patter, but go directly through ConvertToXXX
direct_conversions = [ byte_type, sbyte_type, int16_type, int64_type, uint16_type, uint32_type, uint64_type, single_type, decimal_type ]

# Conversion code snippets. Each tuple contains 2 version of the code.
# First one returns the converted value via the output parameter "result",
# the other returns the converted value directly.
# The code generator runs on these tuples, only right before the emit will
# it call get_code to choose the right element from the tuple for given type.
identity_conversion = ("result = (%(to_type)s)%(value)s; return true;",
                       "return (%(to_type)s)%(value)s;")
unchecked_cast      = ("result = (%(to_type)s)(%(from_type)s)%(value)s; return true;",
                       "return (%(to_type)s)(%(from_type)s)%(value)s;")
checked_cast        = ("result = checked((%(to_type)s)(%(from_type)s)%(value)s); return true;",
                       "return checked((%(to_type)s)(%(from_type)s)%(value)s);")
checked_float_cast  = (
"""result = checked((%(to_type)s)(%(from_type)s)%(value)s);
if (%(to_type)s.IsInfinity(result)) {
    throw PythonOps.OverflowError("{0} won't fit into %(to_type)s", %(value)s);
}
return true;""",
"""%(to_type)s %(to_type)sValue = checked((%(to_type)s)(%(from_type)s)%(value)s);
if (%(to_type)s.IsInfinity(%(to_type)sValue)) {
    throw PythonOps.OverflowError("{0} won't fit into %(to_type)s", %(value)s);
}
return %(to_type)sValue;""")

bool_to_anything = ( "result = (%(from_type)s)%(value)s ? (%(to_type)s)1 : (%(to_type)s)0; return true;",
                     "return (%(from_type)s)%(value)s ? (%(to_type)s)1 : (%(to_type)s)0;" )
bool_to_bigint   = ( "result = (%(from_type)s)%(value)s ? BigInteger.One : BigInteger.Zero; return true;",
                     "return (%(from_type)s)%(value)s ? BigInteger.One : BigInteger.Zero;")


bigint_to_int_via_uint32  = (
"""UInt32 UInt32Value = ((BigInteger)%(value)s).ToUInt32();
result = checked((%(to_type)s)UInt32Value); return true;""",
"""UInt32 UInt32Value = ((BigInteger)%(value)s).ToUInt32();
return checked((%(to_type)s)UInt32Value);""")

bigint_to_int_via_int32  = (
"""Int32 Int32Value = ((BigInteger)%(value)s).ToInt32();
result = checked((%(to_type)s)Int32Value); return true;""",
"""Int32 Int32Value = ((BigInteger)%(value)s).ToInt32();
return checked((%(to_type)s)Int32Value);""")

bigint_to_uint32 = ("result = ((BigInteger)%(value)s).ToUInt32(); return true;",
                    "return ((BigInteger)%(value)s).ToUInt32();")
bigint_to_int32  = ("result = ((BigInteger)%(value)s).ToInt32(); return true;",
                    "return ((BigInteger)%(value)s).ToInt32();")
bigint_to_uint64 = ("result = ((BigInteger)%(value)s).ToUInt64(); return true;",
                    "return ((BigInteger)%(value)s).ToUInt64();")
bigint_to_int64  = ("result = ((BigInteger)%(value)s).ToInt64(); return true;",
                    "return ((BigInteger)%(value)s).ToInt64();")
bigint_to_single    = (
"""result = checked(((BigInteger)%(value)s).ToFloat64());
if (%(to_type)s.IsInfinity(result)) {
    throw PythonOps.OverflowError("{0} won't fit into %(to_type)s", %(value)s);
}
return true;""",
"""%(to_type)s %(to_type)sValue = checked((%(to_type)s)((BigInteger)%(value)s).ToFloat64());
if (%(to_type)s.IsInfinity(%(to_type)sValue)) {
    throw PythonOps.OverflowError("{0} won't fit into %(to_type)s", %(value)s);
}
return %(to_type)sValue;""")

bigint_to_double    = ("result = ((BigInteger)%(value)s).ToFloat64(); return true;",
                       "return ((BigInteger)%(value)s).ToFloat64();")

bigint_to_decimal   = ("result = ((BigInteger)%(value)s).ToDecimal(); return true;",
                       "return ((BigInteger)%(value)s).ToDecimal();")

float_to_int        = (
"""// DEPRECATED IMPLICIT CONVERSION FROM FLOAT TO INT
result = checked((%(to_type)s)(%(from_type)s)%(value)s); return true;""",
"""// DEPRECATED IMPLICIT CONVERSION FROM FLOAT TO INT
return checked((%(to_type)s)(%(from_type)s)%(value)s);""")

conversion_routine_header = (
"""/// <summary>
/// ConvertTo%(to_type)sImpl Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
/// </summary>
private static bool ConvertTo%(to_type)sImpl(object value, out %(to_type)s result) {""",

"""/// <summary>
/// ConvertTo%(to_type)s Conversion Routine. If no conversion exists, throws TypeError false. Can throw OverflowException.
/// </summary>
public static %(to_type)s ConvertTo%(to_type)s(object value) {"""
)

conversion_routine_footer = (
"""result = default(%(to_type)s);
return false;""",
"""
Object result;
if(TryConvertObject(value, typeof(%(to_type)s), out result) && result is %(to_type)s) {
    return (%(to_type)s)result;
}

throw CannotConvertTo("%(to_type)s", value);""")

null_conversion = ("result = null; return true;", "return null;")

def get_code(code_tuple, to_type):
    return code_tuple[to_type in direct_conversions]

def fits_into(ft, tt):
    return tt.min <= ft.min and ft.max <= tt.max

def get_bigint_code(tt):
    if tt in [ bigint_type, x_long_type ]:
        return identity_conversion
    if tt == decimal_type:
        return bigint_to_decimal
    elif tt == single_type:
        return bigint_to_single
    elif tt == double_type:
        return bigint_to_double
    elif tt.int:
        if tt == uint64_type:   return bigint_to_uint64
        elif tt == uint32_type: return bigint_to_uint32
        elif tt == int64_type:  return bigint_to_int64
        elif tt == int32_type:  return bigint_to_int32
        elif tt.signed:
            # convert via signed types
            return bigint_to_int_via_int32
        else:
            return bigint_to_int_via_uint32

def get_int_to_bigint_code(tt):
    if tt == bool_type:
        return bool_to_bigint
    else:
        return unchecked_cast

def get_conv_code(ft, tt):
    if ft == tt:
        return identity_conversion

    # special cases for conversion double => int
    #if ft in [ double_type, x_float_type ]:
    #    if tt == int32_type:
    #        return float_to_int

    if ft in [ bigint_type, x_long_type ]:
        return get_bigint_code(tt)

    if ft == bool_type:
        if tt == bigint_type: return bool_to_bigint
        else: return bool_to_anything

    if ft.int:
        if tt == bigint_type:
            return get_int_to_bigint_code(ft)
        elif fits_into(ft, tt):
            return unchecked_cast
        else:
            return checked_cast
    else:
        if tt == decimal_type:
            return checked_cast
        elif tt.int:
            return None # No conversion from float to int other than above
        elif fits_into(ft, tt):
            return unchecked_cast
        else:
            return checked_float_cast

def get_from_types(to_type):
    """
    This will return list of types from which to geenrate conversion.
    If the list includes t_type, it will be placed at the head of the list.
    """
    ft = list(from_types)
    if to_type in ft:
        i = ft.index(to_type)
        if i > 0:
            del ft[i]
            ft.insert(0, to_type)
    return ft

def conv_impl(cw):
    for tt in to_types:
        code = get_code(conversion_routine_header, tt)
        cw.write(code, to_type = tt.type)
        cw.indent()

        first = True
        for ft in get_from_types(tt):
            if first: block = cw.enter_block
            else: block = cw.else_block

            code = get_conv_code(ft, tt)
            
            if not code:
                continue
                
            code = get_code(code, tt)

            block("if (value is %(from_type)s)", from_type=ft.name)
            cw.write(code, from_type = ft.type, to_type = tt.type, value = ft.value)
            first = False
            
        if tt in reference_types:
            cw.else_block("if (value == null)")
            code = get_code(null_conversion, tt)
            cw.write(code, to_type = tt.type)

        cw.exit_block()
        code = get_code(conversion_routine_footer, tt)
        cw.write(code, to_type = tt.name)
        cw.exit_block()

CodeGenerator("conversion implementations", conv_impl).doit()
CodeGenerator("conversion helpers", converter_generator).doit()
CodeGenerator("explicit enum conversion", enum_converter_generator).doit()
