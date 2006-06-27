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
using System.Collections.Generic;
using System.Text;

namespace IronPython.Runtime.Operations {
    class CharOps {

        public static object Equals(char self, object other) {
            string strOther;
            if (other is char) {
                return Ops.Bool2Object(self == (char)other);
            } else if ((strOther= other as string)!=null && strOther.Length == 1) {
                return Ops.Bool2Object(strOther[0] == self);
            }

            return Ops.NotImplemented;
        }

        public static bool EqualsRetBool(char self, object other) {
            string strOther;
            if (other is char) {
                return self == (char)other;
            } else if ((strOther = other as string) != null && strOther.Length == 1) {
                return strOther[0] == self;
            }

            return false;
        }

        public static object Compare(char self, object other) {
            string strOther;
            
            if (other is char) {
                int diff = self - (char)other;
                return diff > 0 ? 1 : diff < 0 ? -1 : 0;
            } else if ((strOther = other as string) != null && strOther.Length == 1) {
                int diff = self - strOther[0];
                return diff > 0 ? 1 : diff < 0 ? -1 : 0;
            }

            return Ops.NotImplemented;
        }
    }
}
