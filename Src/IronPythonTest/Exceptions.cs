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

namespace IronPythonTest {
    public class ExceptionsTest {
        // a call  that takes the fast path
        public void CallVirtual() {
            VirtualFunc();
        }

        public virtual void VirtualFunc() {
            // a virtual function we can override for throwing from Python code
        }

        // overloads forced to take the slow path...
        public void CallVirtualOverloaded(int bar) {
            VirtualFunc();
        }

        public void CallVirtualOverloaded(string s) {
            VirtualFunc();
        }

        public void CallVirtualOverloaded(object foo) {
            VirtualFunc();
        }

        public void ThrowException() {
            throw new IndexOutOfRangeException("Index out of range!");
        }

        public object CallVirtCatch() {
            try {
                CallVirtual();
            } catch (Exception e) {
                return e;
            }
            return null;
        }

        public object CatchAndRethrow() {
            try {
                CallVirtual();
            } catch (Exception e) {
                throw e;
            }
            return null;
        }
        public object CatchAndRethrow2() {
            try {
                CallVirtual();
            } catch (Exception) {
                throw;
            }
            return null;
        }
    }
}