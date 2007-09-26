/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;

[assembly: PythonExtensionType(typeof(ClrModule), typeof(ClrModuleOps))]
[assembly: PythonExtensionType(typeof(ClrModule.ReferencesList), typeof(ClrReferencesListOps))]
namespace IronPython.Runtime.Operations {
    public static class ClrModuleOps {
        private static DynamicType _strongBoxType;

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

            Assembly asm = DefaultContext.Default.LanguageContext.LoadAssemblyFromFile(file);
            if (asm == null) throw PythonOps.IOError("file does not exist: {0}", file);
            module.AddReference(asm);
        }

#endif

        public static Type GetClrType(ClrModule self, Type type) {
            return type;
        }
        
        public static DynamicType GetDynamicType(ClrModule self, Type t) {
            return DynamicHelpers.GetDynamicTypeFromType(t);
        }

        [PropertyMethod]
        public static DynamicType GetReference(ClrModule mod) {
           return GetStrongBox(mod);
        }

        [PropertyMethod]
        public static DynamicType GetStrongBox(ClrModule mod) {
            if (_strongBoxType == null) {
                _strongBoxType = DynamicHelpers.GetDynamicTypeFromType(typeof(StrongBox<>));
            }
            return _strongBoxType;
        }

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

        public class ArgChecker : ICallableWithCodeContext, IFancyCallable {
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

            #region IFancyCallable Members

            public object Call(CodeContext context, object[] args, string[] names) {
                if (names.Length == 0) {
                    return Call(context, args);
                }

                throw new ArgumentTypeException("bad arg count");
            }

            #endregion
        }

        public class RuntimeArgChecker : DynamicTypeSlot, ICallableWithCodeContext, IFancyCallable {
            private object[] _expected;
            private object _func;
            private object _inst;

            public RuntimeArgChecker(object function, object[] expectedArgs) {
                _expected = expectedArgs;
                _func = function;
            }

            public RuntimeArgChecker(object instance, object function, object[] expectedArgs)
                : this(function, expectedArgs) {
                _inst = instance;
            }

            private void ValidateArgs(object[] args) {
                int start = 0;

                if (_inst != null) {
                    start = 1;
                }


                // no need to validate self... the method should handle it.
                for (int i = start; i < args.Length + start; i++) {
                    DynamicType dt = DynamicHelpers.GetDynamicType(args[i - start]);

                    DynamicType expct = _expected[i] as DynamicType;
                    if (expct == null) expct = ((OldClass)_expected[i]).TypeObject;
                    if (dt != _expected[i] && !dt.IsSubclassOf(expct)) {
                        throw PythonOps.AssertionError("argument {0} has bad value (got {1}, expected {2})", i, dt, _expected[i]);
                    }
                }
            }

            #region ICallableWithCodeContext Members

            public object Call(CodeContext context, params object[] args) {
                ValidateArgs(args);

                if (_inst != null) {
                    return PythonOps.CallWithContext(context, _func, ArrayUtils.Insert(_inst, args));
                } else {
                    return PythonOps.CallWithContext(context, _func, args);
                }
            }

            #endregion

            public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
                value = new RuntimeArgChecker(instance, _func, _expected);
                return true;
            }

            #region IFancyCallable Members

            public object Call(CodeContext context, object[] args, string[] names) {
                ValidateArgs(args);

                if (_inst != null) {
                    return PythonOps.CallWithKeywordArgs(context, _func, ArrayUtils.Insert(_inst, args), names);
                } else {
                    return PythonOps.CallWithKeywordArgs(context, _func, args, names);
                }
            }

            #endregion
        }

        public class ReturnChecker : ICallableWithCodeContext, IFancyCallable {
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

            #region IFancyCallable Members

            public object Call(CodeContext context, object[] args, string[] names) {
                if (names.Length == 0) {
                    return Call(context, args);
                }

                throw PythonOps.TypeError("bad arg count");
            }

            #endregion
        }

        public class RuntimeReturnChecker : DynamicTypeSlot, ICallableWithCodeContext, IFancyCallable {
            private object _retType;
            private object _func;
            private object _inst;

            public RuntimeReturnChecker(object function, object expectedReturn) {
                _retType = expectedReturn;
                _func = function;
            }

            public RuntimeReturnChecker(object instance, object function, object expectedReturn)
                : this(function, expectedReturn) {
                _inst = instance;
            }

            private void ValidateReturn(object ret) {
                // we return void...
                if (ret == null && _retType == null) return;

                DynamicType dt = DynamicHelpers.GetDynamicType(ret);
                if (dt != _retType) {
                    DynamicType expct = _retType as DynamicType;
                    if (expct == null) expct = ((OldClass)_retType).TypeObject;

                    if (!dt.IsSubclassOf(expct))
                        throw PythonOps.AssertionError("bad return value returned (expected {0}, got {1})", _retType, dt);
                }
            }

            #region ICallableWithCodeContext Members

            public object Call(CodeContext context, params object[] args) {
                object ret;
                if (_inst != null) {
                    ret = PythonOps.CallWithContext(context, _func, ArrayUtils.Insert(_inst, args));
                } else {
                    ret = PythonOps.CallWithContext(context, _func, args);
                }
                ValidateReturn(ret);
                return ret;
            }

            #endregion

            #region IDescriptor Members

            public object GetAttribute(object instance, object owner) {
                return new RuntimeReturnChecker(instance, _func, _retType);
            }

            #endregion

            public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
                value = GetAttribute(instance, owner);
                return true;
            }

            #region IFancyCallable Members

            public object Call(CodeContext context, object[] args, string[] names) {
                object ret;
                if (_inst != null) {
                    ret = PythonOps.CallWithKeywordArgs(context, _func, ArrayUtils.Insert(_inst, args), names);
                } else {
                    ret = PythonOps.CallWithKeywordArgs(context, _func, args, names);
                }
                ValidateReturn(ret);
                return ret;
            }

            #endregion
        }

        // backwards compatibility w/ IronPython v1.x
        public static DynamicType GetPythonType(ClrModule self, Type t) {
            return DynamicHelpers.GetDynamicTypeFromType(t);
        }

    }

    public static class ClrReferencesListOps {
        [SpecialName]
        public static ClrModule.ReferencesList Add(ClrModule.ReferencesList self, object other) {
            IEnumerator ie = PythonOps.GetEnumerator(other);
            while (ie.MoveNext()) {
                Assembly cur = ie.Current as Assembly;
                if (cur == null) throw PythonOps.TypeError("non-assembly added to references list");

                self.Add(cur);
            }
            return self;
        }

        [SpecialName]
        public static string ToString(ClrModule.ReferencesList self) {
            return ToCodeRepresentation(self);
        }

        [SpecialName]
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
