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
    public delegate void EventTestDelegate();
    public delegate void OtherEvent(object sender, EventArgs args);

    public class Events {
        public Events() {
        }


        public static object GetTrue() {
                return true;
        }
        public static object GetFalse() {
                return false;
        }
        public static event EventTestDelegate StaticTest;
        public event EventTestDelegate InstanceTest;

        public static event OtherEvent OtherStaticTest;

        public void CallInstance() {
            InstanceTest();
        }

        public static void CallStatic() {
            StaticTest();
        }

        public static void CallOtherStatic(object sender, EventArgs args) {
            OtherStaticTest(sender, args);
        }
    }
}
