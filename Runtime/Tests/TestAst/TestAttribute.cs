/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Reflection;

namespace TestAst {
    enum TestState {
        Enabled,
        Disabled,
        SkipEval,
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=true)]
    class TestAttribute : Attribute {
        private TestState _state;
        private string _description;
        private Type _exception = null;
        private int _priority = 1;

        public Type Exception {
            get {
                return _exception;
            }
            set {
                _exception = value;
            }
        }

        public int Priority{
            get {
                return _priority; 
            }
            set {
                _priority = value;
            }
        }


        public TestState State {
            get {
                return _state;
            }
        }

        public string Description {
            get {
                if (_description == null)
                    return "Unknown";
                else
                    return _description;
            }
        }

        public TestAttribute(TestState state, string description) {
            _state = state;
            _description = description;
        }

        public TestAttribute()
            : this(TestState.Enabled,(string)null) {
        }

        public TestAttribute(string description)
            : this(TestState.Enabled, description) {
        }

        private static TestAttribute GetTestAttribute(MethodInfo mi){
            foreach (object ca in mi.GetCustomAttributes(false))
                if (ca.GetType() == typeof(TestAttribute))
                    return (TestAttribute)ca;

            return null;
        }

        public static bool IsTest(MethodInfo mi) {
            if (null == GetTestAttribute(mi))
                return false;
            else
                return true;
        }

        public static bool IsEnabled(MethodInfo mi) {
            TestAttribute attr = GetTestAttribute(mi);

            if (attr == null)
                return false;

            switch (attr.State) {
                case TestState.Disabled:
                    //PrintDisabled(attr, mi);
                    return false;
                case TestState.Enabled:
                    return true;
                case TestState.SkipEval:
                    return true;
                default:
                    throw new ArgumentException(String.Format("Unexpected test state, '{0}', for method '{1}'.", attr.State.ToString(), mi.Name));
            }
        }

        public static bool IsNegative(MethodInfo mi) {
            TestAttribute attr = GetTestAttribute(mi);

            if (attr == null)
                throw new ArgumentException("MethodInfo is not a test");

            return attr._exception != null;
        }

        public static int GetPriority(MethodInfo mi) {
            TestAttribute attr = GetTestAttribute(mi);

            return attr.Priority;
        }



        private static void PrintDisabled(TestAttribute attr, MethodInfo mi) {
            Console.WriteLine("Warning '{0}' is '{1}': '{2}'.", mi.Name, attr.State.ToString(), attr.Description);
        }

        public static void PrintDisabled(MethodInfo mi) {
            PrintDisabled(GetTestAttribute(mi), mi);
        }
    }
}
