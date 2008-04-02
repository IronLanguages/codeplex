#####################################################################################
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public
# License. A  copy of the license can be found in the License.html file at the
# root of this distribution. If  you cannot locate the  Microsoft Public
# License, please send an email to  dlr@microsoft.com. By using this source
# code in any fashion, you are agreeing to be bound by the terms of the 
# Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#####################################################################################

import generate
reload(generate)
from generate import CodeGenerator, CodeWriter

binaries = [('+',   'add',      4,  'Add',          'Add',          '+'),
            ('-',   'sub',      4,  'Subtract',     'Subtract',     '-'),
            ('**',  'pow',      6,  'Power',        'Power',        None),
            ('*',   'mul',      5,  'Multiply',     'Multiply',     '*'),
            ('/',   'div',      5,  'Divide',       'Divide',       '/'),
            ('//',  'floordiv', 5,  'FloorDivide',  'Divide',       '/'),
            ('///', 'truediv',  5,  'TrueDivide',   'Divide',       '/'),
            ('%',   'mod',      5,  'Mod',          'Mod',          '%'),
            ('<<',  'lshift',   3,  'LeftShift',    'LeftShift',    '<<'),
            ('>>',  'rshift',   3,  'RightShift',   'RightShift',   '>>'),
            ('&',   'and',      2,  'BitwiseAnd',   'BitwiseAnd',   '&'),
            ('|',   'or',       0,  'BitwiseOr',    'BitwiseOr',    '|'),
            ('^',   'xor',      1,  'Xor',          'Xor',          '^')]

extensible_ops = """[PythonName("__%(pyOp)s__")]
public virtual object %(cliOp)s(object other) {
    return %(type)sOps.%(cliOp)s(value, other);
}

[PythonName("__r%(pyOp)s__")]
public virtual object Reverse%(cliOp)s(object other) {
    return %(type)sOps.Reverse%(cliOp)s(value, other);
}"""

class ExtensibleOpsGen:
    def __init__(self, theType, excludes):
        self.theType = theType
        self.excludes = excludes
    def __call__(self, cw):
        for x in binaries:
            if self.excludes.count(x[3]): continue
            cw.write(extensible_ops, pyOp = x[1], type=self.theType, cliOp=x[3])

floatExcludes = ['LeftShift', 'RightShift', 'BitwiseAnd', 'BitwiseOr', 'Xor']

for theType in [('Int',[]), ('Long', []), ('Float', floatExcludes)]: #('Complex', floatExcludes)
    CodeGenerator("Extensible %sOps" % theType[0], ExtensibleOpsGen(theType[0], theType[1])).doit()

def genInum(cw):
    for op in binaries:
        if floatExcludes.count(op[3]): continue
        cw.write('object %s(object other);\nobject Reverse%s(object other);\n' % (op[3], op[3]))

CodeGenerator("INumber Methods", genInum).doit()

long_base_code = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    BigInteger bi;
    INumber num;
    ExtensibleComplex xc;

    if (other is int) return x %(sym)s ((int)other);
    if (other is Complex64) return x %(sym)s ((Complex64)other);
    if (other is double) return x %(sym)s ((double)other);
    if ((object)(bi = other as BigInteger) != null) return x %(sym)s bi;
    if ((num = other as INumber) != null) return num.Reverse%(name)s(x);
    if (other is bool) return x %(sym)s ((bool)other ? 1 : 0);
    if (other is long) return x %(sym)s ((long)other);
    if ((object)(xc = other as ExtensibleComplex) != null) return x %(sym)s xc.value;
    if (other is byte) return x %(sym)s (int)((byte)other);
    return Ops.NotImplemented;
}
"""

long_code_altname = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    BigInteger bi;
    INumber num;
    ExtensibleComplex xc;

    if (other is int) return %(altname)s(x, (int)other);
    if (other is Complex64) {
        Complex64 y = (Complex64)other;
        if (y.IsZero) throw Ops.ZeroDivisionError();
        return ComplexOps.%(name)s(Complex64.MakeReal(x), y);
    }
    if (other is double) return FloatOps.%(name)s(x, (double)other);
    if (other is bool) return %(altname)s(x, (bool)other ? 1 : 0);
    if (other is long) return %(altname)s(x, (long)other);
    if ((object)(bi = other as BigInteger) != null) return %(altname)s(x, bi);
    if ((object)(num = other as INumber) != null) return num.Reverse%(name)s(x);
    if ((object)(xc = other as ExtensibleComplex) != null) {
        Complex64 y = xc.value;
        if (y.IsZero) throw Ops.ZeroDivisionError();
        return ComplexOps.%(name)s(Complex64.MakeReal(x), y);
    }
    if (other is byte) return %(altname)s(x, (int)((byte)other));
    return Ops.NotImplemented;
}


[PythonName("__r%(pyName)s__")]
public static object Reverse%(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) return IntOps.%(altname)s((int)other, x);
    if (other is Complex64) {
        Complex64 y = (Complex64)other;
        if (x == 0) throw Ops.ZeroDivisionError();
        return ComplexOps.%(name)s(y, Complex64.MakeReal(x));
    }
    if (other is double) return FloatOps.%(name)s((double)other, x);
    if (other is bool) return %(altname)s((bool)other ? 1 : 0, x);
    if (other is long) return %(altname)s((long)other, x);
    if ((object)(bi = other as BigInteger) != null) return %(altname)s(bi, x);
    if ((object)(num = other as INumber) != null) return num.%(name)s(x);
    if ((object)(xc = other as ExtensibleComplex) != null) {
        Complex64 y = xc.value;
        if (x == 0) throw Ops.ZeroDivisionError();
        return ComplexOps.%(name)s(y, Complex64.MakeReal(x));
    }
    if (other is byte) return IntOps.%(altname)s((int)((byte)other), x);
    return Ops.NotImplemented;
}

"""

long_code_integers = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(BigInteger x, object other) {
    BigInteger bi;
    ExtensibleInt xi;
    ExtensibleLong xl;

    if ((object)(bi = other as BigInteger) != null) return x %(sym)s bi;
    if (other is long) return x %(sym)s (long)other;
    if (other is int) return x %(sym)s (int)other;
    if (other is bool) return x %(sym)s ((bool)other ? 1 : 0);
    if ((object)(xi = other as ExtensibleInt) != null) return xi.Reverse%(name)s(x);
    if ((object)(xl = other as ExtensibleLong) != null) return xl.Reverse%(name)s(x);
    if (other is byte) return x %(sym)s (int)((byte)other);
    return Ops.NotImplemented;
}
"""

long_code_m = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(BigInteger x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) return %(name)s(x, (int)other);
    if ((object)(bi = other as BigInteger) != null) return %(name)s(x, bi);
    if ((num = other as INumber) != null) return num.Reverse%(name)s(x);
    if (other is double) return %(name)s(x, (double)other);
    if (other is Complex64) return ComplexOps.%(name)s(x, (Complex64)other);
    if (other is bool) return %(name)s(x, (bool)other ? 1 : 0);
    if (other is long) return %(name)s(x, (long)other);
    if ((object)(xc = other as ExtensibleComplex) != null) return %(name)s(x, xc.value);
    if (other is byte) return %(name)s(x, (int)((byte)other));
    return Ops.NotImplemented;
}

[PythonName("__r%(pyName)s__")]
public static object Reverse%(name)s(BigInteger x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) return IntOps.%(name)s((int)other, x);
    if ((object)(bi = other as BigInteger) != null) return %(name)s(bi, x);
    if ((num = other as INumber) != null) return num.%(name)s(x);
    if (other is double) return FloatOps.%(name)s((double)other, x);
    if (other is Complex64) return ComplexOps.%(name)s((Complex64)other, x);
    if (other is bool) return IntOps.%(name)s((bool)other ? 1 : 0, x);
    if (other is long) return Int64Ops.%(name)s((long)other, x);
    if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.%(name)s(xc.value, x);
    if (other is byte) return IntOps.%(name)s((int)((byte)other), x);
    return Ops.NotImplemented;
}
"""

float_code = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(double x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is double) return x %(sym)s ((double)other);
    if (other is int) return x %(sym)s ((int)other);
    if (other is Complex64) return ComplexOps.%(name)s(Complex64.MakeReal(x), (Complex64)other);
    if ((object)(bi = other as BigInteger) != null) return x %(sym)s bi;
    if (other is float) return x %(sym)s ((float)other);
    if ((object)(num = other as INumber) != null) return num.Reverse%(name)s(x);
    if (other is string) return Ops.NotImplemented;
    if (other is IConvertible) {
        double y = ((IConvertible)other).ToDouble(null);
        return x %(sym)s y;
    }
    if (other is long) return x %(sym)s ((long)other);
    if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.%(name)s(Complex64.MakeReal(x), xc.value);
    return Ops.NotImplemented;
}


[PythonName("__r%(pyName)s__")]
public static object Reverse%(name)s(double x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is double) return ((double)other) %(sym)s x;
    if (other is int) return ((int)other) %(sym)s x;
    if (other is Complex64) return ComplexOps.%(name)s((Complex64)other, Complex64.MakeReal(x));
    if ((object)(bi = other as BigInteger) != null) return bi %(sym)s x;
    if (other is float) return ((float)other) %(sym)s x;
    if ((object)(num = other as INumber) != null) return num.%(name)s(x);
    if (other is string) return Ops.NotImplemented;
    if (other is long) return ((long)other) %(sym)s x;
    if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.%(name)s(xc.value, Complex64.MakeReal(x));
    if (other is IConvertible) {
        double y = ((IConvertible)other).ToDouble(null);
        return y %(sym)s x;
    }
    return Ops.NotImplemented;
}
"""

float_code_m = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(double x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is double) return %(name)s(x, ((double)other));
    if (other is int) return %(name)s(x, ((int)other));
    if (other is Complex64) return ComplexOps.%(name)s(Complex64.MakeReal(x), (Complex64)other);
    if ((object)(bi = other as BigInteger) != null) return %(name)s(x, bi);
    if (other is bool) return %(name)s(x, (bool)other ? 1.0 : 0.0);
    if (other is float) return %(name)s(x, ((float)other));
    if ((object)(num = other as INumber) != null) return num.Reverse%(name)s(x);
    if (other is long) return %(name)s(x, ((long)other));
    if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.%(name)s(Complex64.MakeReal(x), xc.value);
    if (other is byte) return %(name)s(x, (int)((byte)other));
    return Ops.NotImplemented;
}

[PythonName("__r%(pyName)s__")]
public static object Reverse%(name)s(double x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is double) return Reverse%(name)s(x, ((double)other));
    if (other is int) return Reverse%(name)s(x, ((int)other));
    if (other is Complex64) return ComplexOps.Reverse%(name)s(Complex64.MakeReal(x), (Complex64)other);
    if ((object)(bi = other as BigInteger) != null) return Reverse%(name)s(x, bi);
    if (other is bool) return Reverse%(name)s(x, (bool)other ? 1.0 : 0.0);
    if (other is float) return Reverse%(name)s(x, ((float)other));
    if ((object)(num = other as INumber) != null) return num.Reverse%(name)s(x);
    if (other is long) return Reverse%(name)s(x, ((long)other));
    if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Reverse%(name)s(Complex64.MakeReal(x), xc.value);
    if (other is byte) return Reverse%(name)s(x, (int)((byte)other));
    return Ops.NotImplemented;
}
"""


complex_code = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) {
        return x %(sym)s (int)other;
    } else if (other is Complex64) {
        return x %(sym)s (Complex64)other;
    } else if (other is double) {
        return x %(sym)s (double)other;
    } else if ((object)(bi = other as BigInteger) != null) {
        return x %(sym)s bi;
    } else if (other is long) {
        return x %(sym)s (long)other;
    } else if ((object)(xc = other as ExtensibleComplex) != null) {
        return x %(sym)s xc.value;
    } else if ((object)(num = other as INumber) != null) {
        return num.Reverse%(name)s(x);
    } else if (other is string) {
        return Ops.NotImplemented;
    } else if (other is IConvertible) {
        double y = ((IConvertible)other).ToDouble(null);
        return x %(sym)s y;
    }
    return Ops.NotImplemented;
}


[PythonName("__r%(pyName)s__")]
public static object Reverse%(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) {
        return (int)other %(sym)s x;
    } else if (other is Complex64) {
        return (Complex64)other %(sym)s x;
    } else if (other is double) {
        return (double)other %(sym)s x;
    } else if ((object)(bi = other as BigInteger) != null) {
        return bi %(sym)s x;
    } else if (other is long) {
        return (long)other %(sym)s x;
    } else if ((object)(xc = other as ExtensibleComplex) != null) {
        return xc.value %(sym)s x;
    } else if ((object)(num = other as INumber) != null) {
        return num.%(name)s(x);
    } else if (other is string) {
        return Ops.NotImplemented;
    } else if (other is IConvertible) {
        double y = ((IConvertible)other).ToDouble(null);
        return y %(sym)s x;
    }
    return Ops.NotImplemented;
}
"""

complex_code_m = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    BigInteger bi;
    INumber num;
    ExtensibleComplex xc;

    if (other is int) return %(name)s(x, (Complex64)((int)other));
    if (other is Complex64) return %(name)s(x, (Complex64)other);
    if (other is double) return %(name)s(x, (Complex64)((double)other));
    if ((object)(bi = other as BigInteger) != null) return %(name)s(x, (Complex64)bi);
    if (other is bool) return %(name)s(x, (Complex64)((bool)other ? 1 : 0));
    if (other is long) return %(name)s(x, (Complex64)((long)other));
    if ((object)(xc = other as ExtensibleComplex) != null) return %(name)s(x, xc.value);
    if ((object)(num = other as INumber) != null) return num.Reverse%(name)s(x);
    if (other is byte) return %(name)s(x, (Complex64)(int)((byte)other));
    return Ops.NotImplemented;
}


[PythonName("__r%(pyName)s__")]
public static object Reverse%(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) return Reverse%(name)s(x, (Complex64)((int)other));
    if (other is Complex64) return Reverse%(name)s(x, (Complex64)other);
    if (other is double) return Reverse%(name)s(x, (Complex64)((double)other));
    if ((object)(bi = other as BigInteger) != null) return Reverse%(name)s(x, (Complex64)bi);
    if (other is bool) return Reverse%(name)s(x, (Complex64)((bool)other ? 1 : 0));
    if (other is long) return Reverse%(name)s(x, (Complex64)((long)other));
    if ((object)(xc = other as ExtensibleComplex) != null) return Reverse%(name)s(x, xc.value);
    if ((object)(num = other as INumber) != null) return num.%(name)s(x);
    if (other is byte) return Reverse%(name)s(x, (Complex64)(int)((byte)other));
    return Ops.NotImplemented;
}
"""

int_code = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) {
        int y = (int)other;
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if ((object)(bi = other as BigInteger) != null) {
        return BigInteger.Create(x) %(sym)s bi;
    } else if (other is double) {
        return x %(sym)s (double)other;
    } else if (other is Complex64) {
        return ComplexOps.%(name)s(Complex64.MakeReal(x), other);
    } else if (other is bool) {
        bool b = (bool)other;
        return x %(sym)s (b ? 1 : 0);
    } else if (other is long) {
        long y = (long)other;
        try {
            return checked(x %(sym)s y);
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if (other is float) {
        return x %(sym)s (float)other;
    } else if (other is byte) {
        return x %(sym)s (byte)other;
    } else if ((object)(num = other as INumber) != null) {
        return num.Reverse%(name)s(x);
    } else if ((object)(xc = other as ExtensibleComplex) != null) {
        return ComplexOps.%(name)s(Complex64.MakeReal(x), xc);
    } else if (other is byte) {
        int y = (byte)other;
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    }
    return Ops.NotImplemented;
}
"""

int_code_divide = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) {
        int y = (int)other;
        try {
            return Ops.%(titleType)s2Object(%(altname)s(x, y));
        } catch (OverflowException) {
            return LongOps.%(name)s(BigInteger.Create(x), y);
        }
    } else if ((object)(bi = other as BigInteger) != null) {
        return LongOps.%(name)s(BigInteger.Create(x), bi);
    } else if (other is double) {
        return FloatOps.%(name)s(x, (double)other);
    } else if (other is Complex64) {
        Complex64 y = (Complex64)other;
        if (y.IsZero) throw Ops.ZeroDivisionError();
        return ComplexOps.%(name)s(Complex64.MakeReal(x), y);
    } else if (other is bool) {
        int y = (bool)other ? 1 : 0;
        try {
            return Ops.%(titleType)s2Object(%(altname)s(x, y));
        } catch (OverflowException) {
            return LongOps.%(name)s(BigInteger.Create(x), y);
        }
    } else if (other is long) {
        long y = (long)other;
        try {
            return %(altname)s(x, y);
        } catch (OverflowException) {
            return LongOps.%(name)s(BigInteger.Create(x), y);
        }
    } else if (other is float) {
        return FloatOps.%(name)s(x, (float)other);
    } else if (other is byte) {
        return Ops.%(titleType)s2Object(%(altname)s(x, (int)((byte)other)));
    } else if ((object)(num = other as INumber) != null) {
        return num.Reverse%(name)s(x);
    } else if ((object)(xc = other as ExtensibleComplex) != null) {
        Complex64 y = xc.value;
        if (y.IsZero) throw Ops.ZeroDivisionError();
        return ComplexOps.%(name)s(Complex64.MakeReal(x), y);
    }

    return Ops.NotImplemented;
}


[PythonName("__r%(pyName)s__")]
public static object Reverse%(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) {
        int y = (int)other;
        try {
            return Ops.%(titleType)s2Object(Reverse%(altname)s(x, y));
        } catch (OverflowException) {
            return LongOps.Reverse%(name)s(BigInteger.Create(x), y);
        }
    } else if ((object)(bi = other as BigInteger) != null) {
        return LongOps.Reverse%(name)s(BigInteger.Create(x), bi);
    } else if (other is double) {
        return FloatOps.Reverse%(name)s(x, (double)other);
    } else if (other is Complex64) {
        Complex64 y = (Complex64)other;
        if (x == 0) throw Ops.ZeroDivisionError();
        return ComplexOps.Reverse%(name)s(Complex64.MakeReal(x), y);
    } else if (other is bool) {
        int y = ((bool)other) ? 1 : 0;
        try {
            return Ops.%(titleType)s2Object(Reverse%(altname)s(x, y));
        } catch (OverflowException) {
            return LongOps.Reverse%(name)s(BigInteger.Create(x), y);
        }
    } else if (other is long) {
        long y = (long)other;
        try {
            return Reverse%(altname)s(x, y);
        } catch (OverflowException) {
            return LongOps.Reverse%(name)s(BigInteger.Create(x), y);
        }
    } else if (other is float) {
        return FloatOps.Reverse%(name)s(x, (float)other);
    } else if (other is byte) {
        return Ops.%(titleType)s2Object(Reverse%(altname)s(x, (int)((byte)other)));
    } else if ((object)(num = other as INumber) != null) {
        return num.%(name)s(x);
    } else if ((object)(xc = other as ExtensibleComplex) != null) {
        Complex64 y = xc.value;
        if (x == 0) throw Ops.ZeroDivisionError();
        return ComplexOps.Reverse%(name)s(Complex64.MakeReal(x), y);
    }
    return Ops.NotImplemented;
}
"""

int_code_bitwise = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleInt xi;
    ExtensibleLong el;

    if (other is int) {
        return Ops.%(titleType)s2Object(x %(sym)s (int)other);
    } else if (other is long) {
        long lx = (long)x;
        return lx %(sym)s (long)other;
    } else if ((object)(bi = other as BigInteger) != null) {
        return BigInteger.Create(x) %(sym)s bi;
    } else if (other is bool) {
        return Ops.%(titleType)s2Object(x %(sym)s ((bool)other ? 1 : 0));
    } else if ((object)(xi = other as ExtensibleInt) != null) {
        return xi.Reverse%(name)s(x);
    } else if (other is byte) {
        return Ops.%(titleType)s2Object(x %(sym)s (int)((byte)other));
    } else if ((object)(el = other as ExtensibleLong) != null) {
        return el.Reverse%(name)s(x);
    }
    return Ops.NotImplemented;
}
"""

int64_code = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) {
        int y = (int)other;
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if ((object)(bi = other as BigInteger) != null) {
        return BigInteger.Create(x) %(sym)s bi;
    } else if (other is double) {
        return x %(sym)s (double)other;
    } else if (other is Complex64) {
        return Complex64.MakeReal(x) %(sym)s (Complex64)other;
    } else if (other is bool) {
        int y = (bool)other ? 1 : 0;
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if (other is long) {
        long y = (long)other;
        try {
            return checked(x %(sym)s y);
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if (other is float) {
        return x %(sym)s (float)other;
    } else if ((object)(num = other as INumber) != null) {
        return num.Reverse%(name)s(x);
    } else if ((object)(xc = other as ExtensibleComplex) != null) {
        return Complex64.MakeReal(x) %(sym)s xc.value;
    } else if (other is byte) {
        int y = (int)((byte)other);
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    }
    return Ops.NotImplemented;
}

[PythonName("__r%(pyName)s__")]
public static object Reverse%(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) {
        int y = (int)other;
        try {
            return Ops.%(titleType)s2Object(checked(y %(sym)s x));
        } catch (OverflowException) {
            return y %(sym)s BigInteger.Create(x);
        }
    } else if ((object)(bi = other as BigInteger) != null) {
        return bi %(sym)s BigInteger.Create(x);
    } else if (other is double) {
        return (double)other %(sym)s x;
    } else if (other is Complex64) {
        return (Complex64)other %(sym)s Complex64.MakeReal(x);
    } else if (other is bool) {
        int y = (bool)other ? 1 : 0;
        try {
            return Ops.%(titleType)s2Object(checked(y %(sym)s x));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if (other is long) {
        long y = (long)other;
        try {
            return checked(y %(sym)s x);
        } catch (OverflowException) {
            return y %(sym)s BigInteger.Create(x);
        }
    } else if (other is float) {
        return (float)other %(sym)s x;
    } else if ((object)(num = other as INumber) != null) {
        return num.%(name)s(x);
    } else if ((object)(xc = other as ExtensibleComplex) != null) {
        return xc.value %(sym)s Complex64.MakeReal(x);
    } else if (other is byte) {
        int y = (int)((byte)other);
        try {
            return Ops.%(titleType)s2Object(checked(y %(sym)s x));
        } catch (OverflowException) {
            return y %(sym)s BigInteger.Create(x);
        }
    }
    return Ops.NotImplemented;
}
"""

int64_code_altname = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) {
        int y = (int)other;
        try {
            return Ops.%(titleType)s2Object(%(altname)s(x, y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(altsym)s y;
        }
    } else if ((object)(bi = other as BigInteger) != null) {
        return LongOps.%(name)s(BigInteger.Create(x), bi);
    } else if (other is double) {
        return FloatOps.%(name)s(x, (double)other);
    } else if (other is Complex64) {
        return ComplexOps.%(name)s(Complex64.MakeReal(x), (Complex64)other);
    } else if (other is bool) {
        int y = (bool)other ? 1 : 0;
        try {
            return Ops.%(titleType)s2Object(%(altname)s(x, y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(altsym)s y;
        }
    } else if (other is long) {
        long y = (long)other;
        try {
            return %(altname)s(x, y);
        } catch (OverflowException) {
            return BigInteger.Create(x) %(altsym)s y;
        }
    } else if (other is float) {
        return FloatOps.%(name)s(x, (float)other);
    } else if ((object)(num = other as INumber) != null) {
        return num.Reverse%(name)s(x);
    } else if ((object)(xc = other as ExtensibleComplex) != null) {
        return ComplexOps.%(name)s(Complex64.MakeReal(x), xc.value);
    } else if (other is byte) {
        int y = (int)((byte)other);
        try {
            return Ops.%(titleType)s2Object(%(altname)s(x, y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(altsym)s y;
        }
    }

    return Ops.NotImplemented;
}


[PythonName("__r%(pyName)s__")]
public static object Reverse%(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) {
        int y = (int)other;
        try {
            return Ops.%(titleType)s2Object(Reverse%(altname)s(x, y));
        } catch (OverflowException) {
            return y %(altsym)s BigInteger.Create(x);
        }
    } else if ((object)(bi = other as BigInteger) != null) {
        return LongOps.Reverse%(name)s(BigInteger.Create(x), bi);
    } else if (other is double) {
        return FloatOps.Reverse%(name)s(x, (double)other);
    } else if (other is Complex64) {
        return ComplexOps.Reverse%(name)s(Complex64.MakeReal(x), (Complex64)other);
    } else if (other is bool) {
        int y = (bool)other ? 1 : 0;
        try {
            return Ops.%(titleType)s2Object(Reverse%(altname)s(x, y));
        } catch (OverflowException) {
            return y %(altsym)s BigInteger.Create(x);
        }
    } else if (other is long) {
        long y = (long)other;
        try {
            return Reverse%(altname)s(x, y);
        } catch (OverflowException) {
            return y %(altsym)s BigInteger.Create(x);
        }
    } else if (other is float) {
        return FloatOps.Reverse%(name)s(x, (float)other);
    } else if ((object)(num = other as INumber) != null) {
        return num.%(name)s(x);
    } else if ((object)(xc = other as ExtensibleComplex) != null) {
        return ComplexOps.Reverse%(name)s(Complex64.MakeReal(x), xc.value);
    } else if (other is byte) {
        int y = (int)((byte)other);
        try {
            return Ops.%(titleType)s2Object(Reverse%(altname)s(x, y));
        } catch (OverflowException) {
            return y %(altsym)s BigInteger.Create(x);
        }
    }

    return Ops.NotImplemented;
}
"""

int64_code_bitwise = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    BigInteger bi;
    ExtensibleInt xi;
    ExtensibleLong el;

    if (other is int) {
        long y = (long)(int)other;
        return Ops.%(titleType)s2Object(x %(sym)s y);
    } else if (other is long) {
        return x %(sym)s (long)other;
    } else if ((object)(bi = other as BigInteger) != null) {
        return BigInteger.Create(x) %(sym)s bi;
    } else if (other is bool) {
        return Ops.%(titleType)s2Object(x %(sym)s ((bool)other ? 1L : 0L));
    } else if ((object)(xi = other as ExtensibleInt) != null) {
        return xi.Reverse%(name)s(x);
    } else if ((object)(el = other as ExtensibleLong) != null) {
        return el.Reverse%(name)s(BigInteger.Create(x));
    } else if (other is byte) {
        return Ops.%(titleType)s2Object(x %(sym)s (long)((byte)other));
    }
    return Ops.NotImplemented;
}
"""

int64_code_m = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(long x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) return %(name)s(x, (int)other);
    if ((object)(bi = other as BigInteger) != null) return %(name)s(x, bi);
    if (other is long) return %(name)s(x, (long)other);
    if (other is double) return %(name)s(x, (double)other);
    if (other is Complex64) return ComplexOps.%(name)s(x, (Complex64)other);
    if (other is bool) return %(name)s(x, (bool)other ? 1 : 0);
    if (other is float) return %(name)s(x, (float)other);
    if ((object)(num = other as INumber) != null) return num.Reverse%(name)s(x);
    if ((object)(xc = other as ExtensibleComplex) != null) return %(name)s(x, xc.value);
    if (other is byte) return %(name)s(x, (int)((byte)other));
    return Ops.NotImplemented;
}
"""

int_code_m = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(int x, object other) {
    BigInteger bi;
    ExtensibleComplex xc;
    INumber num;

    if (other is int) {
        return %(name)s(x, (int)other);
    } else if (other is double) {
        return %(name)s(x, (double)other);
    } else if (other is long) {
        return %(name)s(x, (long)other);
    } else if ((object)(bi = other as BigInteger) != null) {
        return %(name)s(x, bi);
    } else if (other is bool) {
        return %(name)s(x, (bool)other ? 1 : 0);
    } else if (other is Complex64) {
        return %(name)s(x, (Complex64)other);
    } else if ((object)(num = other as INumber) != null) {
        return num.Reverse%(name)s(x);
    } else if ((object)(xc = other as ExtensibleComplex) != null) {
        return %(name)s(x, xc.value);
    }
    return Ops.NotImplemented;
}
"""

float_custom_syms = ('**', '/', '%', '//')
float_im_syms = ('///', )

int_custom_syms = ('>>','<<')
int32_custom_syms = ('**',)
long_use_altname = ('/', '%', '//')
int_bitwise = ('&', '|', '^')

int_divide = ('/','%', '//')

im_syms = ('**', '///', )
int32_im_syms = ('///', )

i_syms = ('<<', '>>')
any_integer_syms = ('&', '|', '^')
integer_syms =  any_integer_syms + i_syms

complex_custom_syms = ('//', '%')

class GenFuncs:
    def __init__(self, tname, sym_map, default_template, default_div, swap_syms = {}):
        self.tname = tname
        self.default_div = default_div
        self.swap_syms = swap_syms
        self.sym_map = {}
        for sym, name, prec, cname, altname, altsym in binaries:
            self.sym_map[sym] = default_template

        for sym_list, template in sym_map.items():
            for sym in sym_list:
                self.sym_map[sym] = template
        if tname == 'int': self.nextType = 'long'
        else: self.nextType = 'BigInteger'

    def __call__(self, cw):
        for sym, name, prec, cname, altname, altsym in binaries:
            sym = self.swap_syms.get(sym, sym)
            template = self.sym_map[sym]
            if template is not None:
                cw.write(template,
                         name=cname,
                         sym=sym,
                         type=self.tname,
                         titleType = self.tname.title(),
                         nextType=self.nextType,
                         pyName=name,
                         altname=altname,
                         altsym=altsym
                         )


CodeGenerator("LongOps",
    GenFuncs('BigInteger', {
        long_use_altname:long_code_altname,
        int_custom_syms:None,
        im_syms:long_code_m,
        any_integer_syms:long_code_integers
    }, long_base_code, 'FloorDivide')).doit()


CodeGenerator("IntOps",
    GenFuncs('int', {
        int_divide:int_code_divide,
        int_bitwise:int_code_bitwise,
        int32_custom_syms+int_custom_syms:None,
        int32_im_syms:int_code_m
    }, int_code, 'FloorDivide')).doit()

CodeGenerator("Int64Ops",
    GenFuncs('long', {
        long_use_altname:int64_code_altname,
        int_bitwise:int64_code_bitwise,
        int_custom_syms:None,
        im_syms:int64_code_m
    }, int64_code, 'FloorDivide')).doit()


CodeGenerator("FloatOps",
    GenFuncs('double', {
        integer_syms+float_custom_syms:None,
        float_im_syms:float_code_m
    }, float_code, "TrueDivide")).doit()

CodeGenerator("ComplexOps",
    GenFuncs('Complex64', {
        integer_syms:None,
        im_syms:complex_code_m,
        ('//', '/', '%'): None
    }, complex_code, 'TrueDivide')).doit()
