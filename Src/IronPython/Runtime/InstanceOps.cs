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
using System.Collections;

using IronPython.Runtime;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Operations {
    public class InstanceOps {
        public static object NextMethod(object self) {
            IEnumerator i = (IEnumerator)self;
            if (i.MoveNext()) return i.Current;
            throw Ops.StopIteration();
        }

        public static object ReprMethod(object self) {
            return self.ToString();
        }

        public static object GetMethod(object self, object instance, object context) {
            return ((IDescriptor)self).GetAttribute(instance, context);
        }

        public static object CallMethod(object self, params object[] args) {
            return ((ICallable)self).Call(args);
        }
    }
}