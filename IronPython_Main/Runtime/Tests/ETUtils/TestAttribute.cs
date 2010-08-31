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
using System.Collections.Generic;


namespace ETUtils {
    public enum TestState {
        Enabled,
        Disabled,
        Any,
        //SkipEval,
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TestAttribute : Attribute {
        private static Dictionary<Assembly, Dictionary<string, MethodInfo>> _Tests;

        private TestState _state;
        private string _description;
        private string[] _keywords = new string[] { };
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

        public int Priority {
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

        public string[] KeyWords {
            get {
                return (string[])_keywords.Clone();
            }
        }

        public TestAttribute(TestState state, string description) {
            _state = state;
            _description = description;
        }

        public TestAttribute()
            : this(TestState.Enabled, (string)null) {
        }

        public TestAttribute(TestState state, string description, string[] KeyWords)
            : this(state, description) {
            _keywords = (string[]) KeyWords.Clone();
        }

        public TestAttribute(string description)
            : this(TestState.Enabled, description) {
        }

        private static TestAttribute GetTestAttribute(MethodInfo mi) {
            foreach (object ca in mi.GetCustomAttributes(false))
                if (ca.GetType() == typeof(TestAttribute))
                    return (TestAttribute)ca;

            return null;
        }

        public static bool IsTest(MethodInfo mi) {
            return GetTestAttribute(mi) != null;
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
                default:
                    throw new ArgumentException(String.Format("Unexpected test state, '{0}', for method '{1}'.", attr.State.ToString(), mi.Name));
            }
        }

        public static MethodInfo GetTest(Assembly assembly, string description) {
            if (_Tests == null) {
                _Tests = new Dictionary<Assembly, Dictionary<string, MethodInfo>>();
            }

            Dictionary<string, MethodInfo> tests;
            if (!_Tests.TryGetValue(assembly, out tests)) {
                // Populate the dictionary with all types in the assembly
                tests = new Dictionary<string, MethodInfo>();
                foreach (Type t in assembly.GetTypes()) {
                    foreach (MethodInfo m in t.GetMethods(BindingFlags.Static | BindingFlags.Public)) {
                        var attr = GetTestAttribute(m);
                        if (attr != null) {
                            if (tests.ContainsKey(attr.Description)) {
                                // TODO: fix this, tests should not have
                                // duplicate keys if they're going to be looked
                                // up by name!
                                                                
                                throw new Exception("Duplicate scenario names: " + attr.Description);
                            }
                            tests.Add(attr.Description, m);
                        }
                    }
                }
                _Tests.Add(assembly, tests);
            }

            MethodInfo result;
            tests.TryGetValue(description, out result);
            return result;
        }

        public static List<MethodInfo> GetTests(Assembly Asm){
            List<MethodInfo> Ret = new List<MethodInfo>();
            //Find types defined in assembly
            //Find methods defined in the types
            //Find methods with the attribute on them.
            Type[] Types = Asm.GetTypes();
            foreach (Type T in Types) {
                MethodInfo[] Methods = T.GetMethods(BindingFlags.Static | BindingFlags.Public);
                foreach (MethodInfo M in Methods) {
                    if(IsTest(M)) {
                        Ret.Add(M);
                    }
                }
            }
            return Ret;
        }

        public static List<MethodInfo> GetTests(Assembly Asm,TestState State ) {
            List<MethodInfo> Ret = new List<MethodInfo>();
            //Find types defined in assembly
            //Find methods defined in the types
            //Find methods with the attribute on them.
            Type[] Types = Asm.GetTypes();
            foreach (Type T in Types) {
                MethodInfo[] Methods = T.GetMethods(BindingFlags.Static | BindingFlags.Public);
                foreach (MethodInfo M in Methods) {
                    if (IsTest(M) && GetTestAttribute(M).State == State ) {
                        Ret.Add(M);
                    }
                }
            }
            return Ret;
        }

        /// <summary>
        /// Returns a list of method infos for Tests on a given assembly, that either contain one or all of the specified keywords.
        /// </summary>
        /// <param name="Asm">The assembly to be searched for tests.</param>
        /// <param name="State">Whether to return tests that are Enabled or Disabled.</param>
        /// <param name="IncludeAllKeywords">The keyword set that a test has to have to be returned.</param>
        /// <param name="IncludeKeywords">The keywords from which a test has to have at least one to be returned.</param>
        /// <param name="ExcludeAllKeywords">The keyword set that a test cannot have to be returned.</param>
        /// <param name="ExcludeKeywords">The keywords that a test cannot have a single one to be returned.</param>
        /// <returns></returns>
        public static List<MethodInfo> GetTests(Assembly Asm, TestState State, String[] IncludeAllKeywords, String[] IncludeKeywords, String[] ExcludeAllKeyworkds, String[] ExcludeKeyworkds) {
            List<MethodInfo> Ret = new List<MethodInfo>();
            List<MethodInfo> Methods = GetTests(Asm, State);
            foreach (MethodInfo M in Methods) {
                List<String> TestKeywords = new List<String>(GetTestAttribute(M).KeyWords);

                //if any of the Keywords is in the Exclude list, skip testcase
                if (Intersect(TestKeywords.ToArray(), ExcludeKeyworkds).Length > 0) {
                    continue;
                }

                //if the exclude list is contained in the testcase keywords, skip testcase
                if (SubSet(TestKeywords.ToArray(), ExcludeAllKeyworkds)) {
                    continue;
                }

                //if an Include List was provided, and the test has a keyword there, add the testcase
                if (IncludeKeywords != null && Intersect(TestKeywords.ToArray(), IncludeKeywords).Length >0) {
                    Ret.Add(M);
                    continue;
                }

                //if an IncludeAll List was provided, and the test keywords include the list, add the testcase
                if (IncludeAllKeywords != null && SubSet(TestKeywords.ToArray(), IncludeAllKeywords)) {
                    Ret.Add(M);
                    continue;
                }

                //if neither an include list or an includeall list were provided, include testcase by default.
                if (IncludeAllKeywords == null && IncludeKeywords == null) {
                    Ret.Add(M);
                    continue;
                }
            }
            return Ret;
        }

        private static Boolean SubSet(String[] LargeSet, String[] SmallSet) {
            List<String> LL = new List<String>();
            LL.AddRange(LargeSet);
            foreach(String s in SmallSet){
                if (!LL.Contains(s)) {
                    return false;
                }
            }
            return true;
        }

        private static String[] Intersect(String[] Set1, String[] Set2) {
            List<String> LL = new List<String>();
            List<String> ret = new List<String>();
            LL.AddRange(Set1);
            foreach (String s in Set2) {
                if (!LL.Contains(s)) {
                    ret.Add(s);
                }
            }
            return ret.ToArray();
        }

        public static TestAttribute GetAttribute(MethodInfo Mi) {
            if (Mi.GetCustomAttributes(typeof(TestAttribute), true).Length > 0) {
                return (TestAttribute)Mi.GetCustomAttributes(typeof(TestAttribute), true)[0];
            } else { 
                return null;
            }
        }
    }
}

