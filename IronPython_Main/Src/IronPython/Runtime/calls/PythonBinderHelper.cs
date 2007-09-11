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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting;

using IronPython.Runtime.Operations;
using Microsoft.Scripting.Types;

namespace IronPython.Runtime.Calls {
    static class PythonBinderHelper {
        public static Expression[] GetCollapsedIndexArguments<T>(DoOperationAction action, object[] args, StandardRule<T> rule) {
            int simpleArgCount = action.Operation == Operators.GetItem ? 2 : 3;

            Expression[] exprargs = new Expression[simpleArgCount];
            exprargs[0] = Ast.CodeContext();

            if (args.Length > simpleArgCount) {
                Expression[] tupleArgs = new Expression[args.Length - simpleArgCount + 1];
                for (int i = 0; i < tupleArgs.Length; i++) {
                    tupleArgs[i] = rule.Parameters[i + 1];
                }
                // multiple index arguments, pack into tuple.
                exprargs[1] = Ast.Call(null,
                    typeof(PythonOps).GetMethod("MakeTuple"),
                    tupleArgs);
            } else {
                // single index argument
                exprargs[1] = rule.Parameters[1];
            }

            if (action.Operation == Operators.SetItem) {
                exprargs[2] = rule.Parameters[rule.Parameters.Length - 1];
            }

            return exprargs;
        }

        public static void MakeTest(StandardRule rule, params DynamicType[] types) {
            rule.SetTest(MakeTestForTypes(rule, types, 0));
        }

        public static Expression MakeTypeTest(StandardRule rule, DynamicType type, int index) {
            return MakeTypeTest(rule, type, rule.Parameters[index]);
        }

        public static Expression MakeTestForTypes(StandardRule rule, DynamicType[] types, int index) {
            Expression test = MakeTypeTest(rule, types[index], index);
            if (index + 1 < types.Length) {
                Expression nextTests = MakeTestForTypes(rule, types, index + 1);
                if (test.IsConstant(true)) {
                    return nextTests;
                } else if (nextTests.IsConstant(true)) {
                    return test;
                } else {
                    return Ast.AndAlso(test, nextTests);
                }
            } else {
                return test;
            }
        }

        public static Expression MakeTypeTest(StandardRule rule, DynamicType type, Expression tested) {
            if (type == null || type.IsNull) 
                return Ast.Equal(tested, Ast.Null());

            Type clrType = type.UnderlyingSystemType;
            bool isStaticType = !typeof(ISuperDynamicObject).IsAssignableFrom(clrType);

            Expression test = StandardRule.MakeTypeTestExpression(type.UnderlyingSystemType, tested);

            if (!isStaticType) {
                int version = type.Version;
                if (version != DynamicType.DynamicVersion) {
                    test = Ast.AndAlso(test,
                        Ast.Call(null, typeof(RuntimeHelpers).GetMethod("CheckTypeVersion"), tested, Ast.Constant(version)));
                    rule.AddValidator(new DynamicTypeValidator(new WeakReference(type), version).Validate);
                } else {
                    version = type.AlternateVersion;
                    test = Ast.AndAlso(test,
                        Ast.Call(null, typeof(RuntimeHelpers).GetMethod("CheckAlternateTypeVersion"), tested, Ast.Constant(version)));
                    rule.AddValidator(new DynamicTypeValidator(new WeakReference(type), version).AlternateValidate);
                }
            }
            return test;            
        }

        public static StandardRule<T> TypeError<T>(string message, params DynamicType[] types) {
            StandardRule<T> ret = new StandardRule<T>();
            MakeTest(ret, types);
            ret.SetTarget(
                Ast.Statement(Ast.Throw(
                    Ast.Call(null, typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                        Ast.Constant(message)))));
            return ret;
        }

        private class DynamicTypeValidator {
            /// <summary>
            /// Weak reference to the dynamic type. Since they can be collected,
            /// we need to be able to let that happen and then disable the rule.
            /// </summary>
            private WeakReference _dynamicType;

            /// <summary>
            /// Expected version of the instance's dynamic type
            /// </summary>
            private int _version;

            public DynamicTypeValidator(WeakReference dynamicType, int version) {
                this._dynamicType = dynamicType;
                this._version = version;
            }

            public bool Validate() {
                DynamicType dt = _dynamicType.Target as DynamicType;
                return dt != null && dt.Version == _version;
            }
            public bool AlternateValidate() {
                DynamicType dt = _dynamicType.Target as DynamicType;
                return dt != null && dt.AlternateVersion == _version;
            }
        }

    }
}
