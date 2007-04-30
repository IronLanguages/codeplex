/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

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

        bool _marker = false;

        public bool Marker { 
            get { return _marker; }
            set { _marker = value; }
        }

        public void SetMarker() {
            _marker = true;
        }

        public void AddSetMarkerDelegateToInstanceTest() {
            InstanceTest += new EventTestDelegate(this.SetMarker);
        }
    }
}
