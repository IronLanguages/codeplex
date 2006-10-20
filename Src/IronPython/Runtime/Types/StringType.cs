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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using System.Reflection;

using System.Globalization;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {

    /// <summary>
    /// The type the user gets when doing type('abc') or str.  This allows us
    /// to extend String with the methods in StringOps.  StringDynamicType also
    /// defines overloads for adding and multiplying strings so:
    /// 
    /// Ops.GetDynamicType('abc').Multiply('abc', 3) and such works properly.
    /// </summary>
    public class StringDynamicType : OpsReflectedType {
        public StringDynamicType()
            : base("str", typeof(string), typeof(StringOps), typeof(ExtensibleString), new CallTarget2(FastNew)) {
        }

        internal static object FastNew(object context, object x) {
            if (x == null) {
                return "None";
            }
            if (x is string) {
                // check ascii
                string s = (string)x;
                for (int i = 0; i < s.Length; i++) {
                    if (s[i] > '\x80')
                        return StringOps.Make(
                            (ICallerContext)context,
                            (DynamicType)Ops.GetDynamicTypeFromType(typeof(String)),
                            s,
                            null,
                            "strict"
                            );
                }
                return s;
            }
            return Ops.ToString(x);
        }

        public override object Add(object self, object other) {
            string s = other as string;
            if (s == null) return Ops.NotImplemented;
            return ((string)self) + s;
        }

        public override object Multiply(object self, object other) {
            ExtensibleLong el;
            BigInteger bi;
            ExtensibleInt ei;

            if (other is int) {
                return StringOps.Multiply((string)self, (int)other);
            } else if (other is bool) {
                return StringOps.Multiply((string)self, ((bool)other) ? 1 : 0);
            } else if (!Object.ReferenceEquals((bi = other as BigInteger), null)) {
                return MultiplyBigInt(self, bi);
            } else if ((el = other as ExtensibleLong) != null) {
                return MultiplyBigInt(self, el.Value);
            } else if ((ei = other as ExtensibleInt) != null) {
                return StringOps.Multiply((string)self, ei.value);
            }
            return Ops.NotImplemented;
        }

        private object MultiplyBigInt(object self, BigInteger value) {
            int size;
            if (value.AsInt32(out size))
                return StringOps.Multiply((string)self, size);

            throw Ops.OverflowError("long int too large to convert to int");
        }

        public override object ReverseMultiply(object self, object other) {
            ExtensibleLong el;
            if (other is int) {
                return StringOps.Multiply((string)self, (int)other);
            } else if (other is bool) {
                return StringOps.Multiply((string)self, ((bool)other) ? 1 : 0);
            } else if (other is BigInteger) {
                BigInteger bi = other as BigInteger;
                int size;
                if (bi.AsInt32(out size))
                    return StringOps.Multiply((string)self, size);
                else
                    throw Ops.OverflowError("long int too large to convert to int");
            } else if ((el = other as ExtensibleLong) != null) {
                return ReverseMultiply(self, el.Value);
            } else if (other is ExtensibleInt) {
                return StringOps.Multiply((string)self, ((ExtensibleInt)other).value);
            }
            return Ops.NotImplemented;
        }

        public override object Mod(object self, object other) {
            return new StringFormatter((string)self, other).Format();
        }
    }
}