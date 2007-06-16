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

using Microsoft.Scripting;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using System.Reflection;
using System.Collections;

[assembly: PythonExtensionType(typeof(ClrModule), typeof(ClrModuleOps))]
[assembly: PythonExtensionType(typeof(ClrModule.ReferencesList), typeof(ClrReferencesListOps))]
namespace IronPython.Runtime.Operations {
    public static class ClrModuleOps {
        #region Runtime Type Checking support
#if !SILVERLIGHT // files, paths

        public static void AddReferenceToFileAndPath(ClrModule module, params string[] files) {
            if (files == null) throw new ArgumentTypeException("Expected string, got NoneType");

            foreach (string file in files) {
                AddReferenceToFileAndPath(module, file);
            }
        }

        private static void AddReferenceToFileAndPath(ClrModule module, string file) {
            if (file == null) throw PythonOps.TypeError("Expected string, got NoneType");

            // update our path w/ the path of this file...
            string path = System.IO.Path.GetDirectoryName(file);
            List list = SystemState.Instance.path;
            if (list == null) throw PythonOps.TypeError("cannot update path, it is not a list");

            list.Add(path);

            // then fall through to the normal loading process
            module.AddReferenceToFile(DefaultContext.Default, System.IO.Path.GetFileName(file));
        }

#endif

        [PythonName("accepts")]
        public static object Accepts(ClrModule self, params object[] types) {
            return new ArgChecker(types);
        }

        [PythonName("returns")]
        public static object Returns(ClrModule self, object type) {
            return new ReturnChecker(type);
        }

        [PythonName("Self")]
        public static object Self(ClrModule self) {
            return null;
        }
        #endregion

        public class ArgChecker : ICallableWithCodeContext {
            private object[] expected;

            public ArgChecker(object[] prms) {
                expected = prms;
            }

            #region ICallableWithCodeContext Members

            public object Call(CodeContext context, params object[] args) {
                // expect only to receive the function we'll call here.
                if (args.Length != 1) throw new ArgumentTypeException("bad arg count");

                return new RuntimeArgChecker(args[0], expected);
            }

            #endregion
        }

        public class RuntimeArgChecker : DynamicTypeSlot, ICallableWithCodeContext {
            private object[] expected;
            private object func;
            private object inst;

            public RuntimeArgChecker(object function, object[] expectedArgs) {
                expected = expectedArgs;
                func = function;
            }

            public RuntimeArgChecker(object instance, object function, object[] expectedArgs)
                : this(function, expectedArgs) {
                inst = instance;
            }

            private void ValidateArgs(object[] args) {
                int start = 0;

                if (inst != null) {
                    start = 1;
                }


                // no need to validate self... the method should handle it.
                for (int i = start; i < args.Length + start; i++) {
                    DynamicType dt = DynamicHelpers.GetDynamicType(args[i - start]);

                    DynamicType expct = expected[i] as DynamicType;
                    if (expct == null) expct = ((OldClass)expected[i]).TypeObject;
                    if (dt != expected[i] && !dt.IsSubclassOf(expct)) {
                        throw PythonOps.AssertionError("argument {0} has bad value (got {1}, expected {2})", i, dt, expected[i]);
                    }
                }
            }

            #region ICallableWithCodeContext Members

            public object Call(CodeContext context, params object[] args) {
                ValidateArgs(args);

                if (inst != null) {
                    object[] realArgs = new object[args.Length + 1];
                    realArgs[0] = inst;
                    Array.Copy(args, 0, realArgs, 1, args.Length);
                    return PythonOps.CallWithContext(context, func, realArgs);
                } else {
                    return PythonOps.CallWithContext(context, func, args);
                }
            }

            #endregion

            public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
                value = new RuntimeArgChecker(instance, func, expected);
                return true;
            }
        }

        public class ReturnChecker : ICallableWithCodeContext {
            public object retType;

            public ReturnChecker(object returnType) {
                retType = returnType;
            }

            #region ICallableWithCodeContext Members

            public object Call(CodeContext context, params object[] args) {
                // expect only to receive the function we'll call here.
                if (args.Length != 1) throw PythonOps.TypeError("bad arg count");

                return new RuntimeReturnChecker(args[0], retType);
            }

            #endregion
        }

        public class RuntimeReturnChecker : DynamicTypeSlot, ICallableWithCodeContext {
            private object retType;
            private object func;
            private object inst;

            public RuntimeReturnChecker(object function, object expectedReturn) {
                retType = expectedReturn;
                func = function;
            }

            public RuntimeReturnChecker(object instance, object function, object expectedReturn)
                : this(function, expectedReturn) {
                inst = instance;
            }

            private void ValidateReturn(object ret) {
                // we return void...
                if (ret == null && retType == null) return;

                DynamicType dt = DynamicHelpers.GetDynamicType(ret);
                if (dt != retType) {
                    DynamicType expct = retType as DynamicType;
                    if (expct == null) expct = ((OldClass)retType).TypeObject;

                    if (!dt.IsSubclassOf(expct))
                        throw PythonOps.AssertionError("bad return value returned (expected {0}, got {1})", retType, dt);
                }
            }

            #region ICallableWithCodeContext Members

            public object Call(CodeContext context, params object[] args) {
                object ret;
                if (inst != null) {
                    object[] realArgs = new object[args.Length + 1];
                    realArgs[0] = inst;
                    Array.Copy(args, 0, realArgs, 1, args.Length);
                    ret = PythonOps.CallWithContext(context, func, realArgs);
                } else {
                    ret = PythonOps.CallWithContext(context, func, args);
                }
                ValidateReturn(ret);
                return ret;
            }

            #endregion

            #region IDescriptor Members

            public object GetAttribute(object instance, object owner) {
                return new RuntimeReturnChecker(instance, func, retType);
            }

            #endregion

            public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
                value = GetAttribute(instance, owner);
                return true;
            }
        }

        // backwards compatibility w/ IronPython v1.x
        public static DynamicType GetPythonType(ClrModule self, Type t) {
            return DynamicHelpers.GetDynamicTypeFromType(t);
        }

    }

    public static class ClrReferencesListOps {
        [OperatorMethod]
        public static ClrModule.ReferencesList Add(ClrModule.ReferencesList self, object other) {
            IEnumerator ie = PythonOps.GetEnumerator(other);
            while (ie.MoveNext()) {
                Assembly cur = ie.Current as Assembly;
                if (cur == null) throw PythonOps.TypeError("non-assembly added to references list");

                self.Add(cur);
            }
            return self;
        }

        [OperatorMethod]
        public static string ToString(ClrModule.ReferencesList self) {
            return ToCodeRepresentation(self);
        }

        [OperatorMethod]
        public static string ToCodeRepresentation(ClrModule.ReferencesList self) {
            StringBuilder res = new StringBuilder("(");
            string comma = "";
            foreach (Assembly asm in self) {
                res.Append(comma);
                res.Append('<');
                res.Append(asm.FullName);
                res.Append('>');
                comma = "," + Environment.NewLine;
            }

            res.AppendLine(")");
            return res.ToString();
        }
    }
}
