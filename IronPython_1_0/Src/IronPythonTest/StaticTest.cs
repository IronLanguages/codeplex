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

namespace IronPythonTest.StaticTest {
    public class B { }
    public class D : B { }

    public delegate string MyEventHandler();

    public class Base {
        public static string Method_None() { return "Base.Method_None"; }
        public static string Method_OneArg(int arg) { return "Base.Method_OneArg"; }
        public static string Method_Base(Base arg) { return "Base.Method_Base"; }

        public static string Method_Inheritance1(B arg) { return "Base.Method_Inheritance1"; }
        public static string Method_Inheritance2(D arg) { return "Base.Method_Inheritance2"; }

        public static string Field = "Base.Field";

        static string s_for_property = "Base.Property";
        public static string Property {
            get { return s_for_property; }
            set { s_for_property = value; }
        }

        public static event MyEventHandler Event;

        public static string TryEvent() {
            if (Event == null) {
                return "Still None";
            } else {
                return Event();
            }
        }
    }

    public class OverrideNothing : Base {
    }

    public class OverrideAll : Base {
        public new static string Method_None() { return "OverrideAll.Method_None"; }
        public new static string Method_OneArg(int arg) { return "OverrideAll.Method_OneArg"; }
        public static string Method_Base(OverrideAll arg) { return "OverrideAll.Method_Base"; }

        public static string Method_Inheritance1(D arg) { return "OverrideAll.Method_Inheritance1"; }
        public static string Method_Inheritance2(B arg) { return "OverrideAll.Method_Inheritance2"; }

        public new static string Field = "OverrideAll.Field";

        static string s_for_property = "OverrideAll.Property";
        public new static string Property {
            get { return s_for_property; }
            set { s_for_property = value; }
        }

        public new static event MyEventHandler Event;

        public new static string TryEvent() {
            if (Event == null) {
                return "Still None here";
            } else {
                return Event();
            }
        }
    }
}
