/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using IronMath;

namespace IronPython.Runtime {
    static partial class UInt64Ops {

        internal static object AddImpl(UInt64 left, UInt64 right) {
            if (left > UInt64.MaxValue - right) {
                return BigInteger.Create(left) + BigInteger.Create(right);
            }
            return left + right;
        }

        internal static object ReverseAddImpl(UInt64 left, UInt64 right) {
            return AddImpl(right, left);
        }

        internal static object AddImpl(UInt64 left, Int64 right) {
            if (right >= 0) {
                UInt64 leftUInt64 = (UInt64)right;
                if (leftUInt64 > UInt64.MaxValue - left) {
                    return BigInteger.Create(leftUInt64) + BigInteger.Create(left);
                }
                return leftUInt64 + left;
            } else {    // right < 0
                UInt64 rightNegUIn64 = (UInt64)(-right);

                if (rightNegUIn64 <= left) {
                    return left - rightNegUIn64;
                }

                // left < rightNegUIn64 ==> left < -(Int64.MinValue)
                return (Int64)left + right;
            }
        }

        internal static object ReverseAddImpl(UInt64 left, Int64 right) {
            return AddImpl(right, left);
        }

        internal static object AddImpl(Int64 left, UInt64 right) {
            return AddImpl(right, left);
        }

        internal static object ReverseAddImpl(Int64 left, UInt64 right) {
            return AddImpl(right, left);
        }

        internal static object MultiplyImpl(UInt64 left, UInt64 right) {
            if (left > UInt64.MaxValue / right) {
                return BigInteger.Create(left) * BigInteger.Create(right);
            }
            return left * right;
        }

        internal static object MultiplyImpl(UInt64 left, Int64 right) {
            if (left > Int64.MaxValue) {
                if (right == -1 && left == (UInt64)(Int64.MaxValue) + 1) {
                    return Int64.MinValue;
                }
                if (right == 0) {
                    return right;
                }
            } else {
                if (right == 0 || left == 0) {
                    return 0L;
                }
                Int64 leftInt64 = (Int64)left;
                if (right > 0) {
                    if (Int64.MaxValue / right >= leftInt64) {
                        return right * leftInt64;
                    }
                } else {
                    if (right >= Int64.MinValue / leftInt64) {
                        return right * leftInt64;
                    }
                }
            }

            return BigInteger.Create(right) * BigInteger.Create(left);
        }

        internal static object MultiplyImpl(Int64 left, UInt64 right) {
            // l * r == r * l
            return MultiplyImpl(right, left);
        }

        internal static object ReverseMultiplyImpl(UInt64 left, UInt64 right) {
            return MultiplyImpl(right, left);
        }

        internal static object ReverseMultiplyImpl(Int64 left, UInt64 right) {
            return MultiplyImpl(right, left);
        }

        internal static object ReverseMultiplyImpl(UInt64 left, Int64 right) {
            return MultiplyImpl(right, left);
        }

        internal static object SubtractImpl(UInt64 left, UInt64 right) {
            if (left < right) {
                return BigInteger.Create(left) - BigInteger.Create(right);
            }
            return left - right;
        }

        internal static object SubtractImpl(Int64 left, UInt64 right) {
            if (right > (UInt64)(left - Int64.MinValue)) {
                return BigInteger.Create(left) - BigInteger.Create(right);
            }
            return left - (Int64)right;
        }

        internal static object SubtractImpl(UInt64 left, Int64 right) {
            if (right > 0) {
                return SubtractImpl(left, (UInt64)right);
            }
            return AddImpl(left, (UInt64)(-right));
        }

        internal static object ReverseSubtractImpl(UInt64 left, UInt64 right) {
            return SubtractImpl(right, left);
        }

        internal static object ReverseSubtractImpl(Int64 left, UInt64 right) {
            return SubtractImpl(right, left);
        }

        internal static object ReverseSubtractImpl(UInt64 left, Int64 right) {
            return SubtractImpl(right, left);
        }

        internal static object DivideImpl(Int64 left, UInt64 right) {
            if (right == 0) {
                throw new DivideByZeroException();
            }
            if (left < 0) {
                if (right > ((UInt64)Int64.MaxValue) + 1) {
                    return (Int64)(-1);
                }
                if (left == Int64.MinValue && right == ((UInt64)Int64.MaxValue) + 1) {
                    return (Int64)(-1);
                }

                // left < 0 && right <= Int64.MaxValue
                Int64 rightInt64 = (Int64)right;
                Int64 result = left / rightInt64;

                if (left % rightInt64 == 0) return result;
                else return result - 1;                 // OK, result is signed
            } else {
                return (Int64)((UInt64)left / right);
            }
        }

        internal static object ReverseDivideImpl(Int64 left, UInt64 right) {
            return DivideImpl(right, left);
        }

        internal static object DivideImpl(UInt64 left, Int64 right) {
            if (right == 0) {
                throw new DivideByZeroException();
            }

            if (right < 0) {
                return LongOps.Divide(BigInteger.Create(left), BigInteger.Create(right));
            }

            return left / (UInt32)right;
        }

        internal static object ReverseDivideImpl(UInt64 left, Int64 right) {
            return DivideImpl(right, left);
        }

        internal static object FloorDivideImpl(Int64 left, UInt64 right) {
            return DivideImpl(left, right);
        }

        internal static object ReverseFloorDivideImpl(Int64 left, UInt64 right) {
            return DivideImpl(right, left);
        }

        internal static object FloorDivideImpl(UInt64 left, Int64 right) {
            return DivideImpl(left, right);
        }

        internal static object ReverseFloorDivideImpl(UInt64 left, Int64 right) {
            return DivideImpl(right, left);
        }

        internal static object ModImpl(Int64 left, UInt64 right) {
            if (right == 0) {
                throw new DivideByZeroException();
            }

            if (left < 0) {
                UInt64 leftUInt64 = (UInt64)(-left);        // This will fit
                UInt64 umod = (leftUInt64 % right);         // mod <= left ... must fit

                if (umod == 0) return (Int64)0;
                else return right - umod;
            } else {
                return (UInt64)left % right;
            }
        }

        internal static object ModImpl(UInt64 left, Int64 right) {
            if (right == 0) {
                throw new DivideByZeroException();
            }

            if (right < 0) {
                UInt64 negRightUInt64 = (UInt64)(-right);
                UInt64 mod = left % negRightUInt64;
                if (mod == 0) return mod;

                return -(Int64)(negRightUInt64 - mod);
            } else {
                return left % (UInt64)right;
            }
        }

        internal static object ReverseModImpl(Int64 left, UInt64 right) {
            return ModImpl(right, left);
        }

        internal static object ReverseModImpl(UInt64 left, Int64 right) {
            return ModImpl(right, left);
        }
    }
}
