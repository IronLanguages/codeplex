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
using System.Diagnostics;
using System.Reflection;
using System.Text;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting;

using IronPython.Runtime.Operations;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Generation;

namespace IronPython.Runtime.Calls {
    static class PythonBinderHelper {
        /// <summary>
        /// dictionary of templated type error rules.
        /// </summary>
        private static Dictionary<TypeErrorKey, object> _typeErrorTemplates = new Dictionary<TypeErrorKey, object>();

        public static Expression[] GetCollapsedIndexArguments<T>(DoOperationAction action, object[] args, StandardRule<T> rule) {
            int simpleArgCount = (action.Operation == Operators.GetItem || action.Operation == Operators.DeleteItem) ? 2 : 3;

            Expression[] exprargs = new Expression[simpleArgCount];
            exprargs[0] = Ast.CodeContext();

            if (args.Length > simpleArgCount) {
                Expression[] tupleArgs = new Expression[args.Length - simpleArgCount + 1];
                for (int i = 0; i < tupleArgs.Length; i++) {
                    tupleArgs[i] = rule.Parameters[i + 1];
                }
                // multiple index arguments, pack into tuple.
                exprargs[1] = Ast.ComplexCallHelper(
                    typeof(PythonOps).GetMethod("MakeTuple"),
                    tupleArgs
                );
            } else {
                // single index argument
                exprargs[1] = rule.Parameters[1];
            }

            if (action.Operation == Operators.SetItem) {
                exprargs[2] = rule.Parameters[rule.Parameters.Length - 1];
            }

            return exprargs;
        }

        public static void MakeTest(StandardRule rule, bool templatable, params DynamicType[] types) {
            rule.SetTest(MakeTestForTypes(rule, types, templatable, 0));
        }

        public static void MakeTest(StandardRule rule, params DynamicType[] types) {
            rule.SetTest(MakeTestForTypes(rule, types, 0));
        }

        public static Expression MakeTestForTypes(StandardRule rule, DynamicType[] types, int index) {
            return MakeTestForTypes(rule, types, false, index);
        }

        public static Expression MakeTestForTypes(StandardRule rule, DynamicType[] types, bool templatable, int index) {
            Expression test = MakeTypeTest(rule, types[index], rule.Parameters[index], templatable);
            if (index + 1 < types.Length) {
                Expression nextTests = MakeTestForTypes(rule, types, templatable, index + 1);
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

        public static Expression MakeTypeTest(StandardRule rule, DynamicType type, Expression tested, bool templatable) {
            if (!templatable && (type == null || type.IsNull)) {
                return Ast.Equal(tested, Ast.Null());
            }

            Type clrType = type.UnderlyingSystemType;
            bool isStaticType = !typeof(ISuperDynamicObject).IsAssignableFrom(clrType);

            Expression test;
            if (!templatable) {
                test = StandardRule.MakeTypeTestExpression(type.UnderlyingSystemType, tested);
            } else {
                test = Ast.Equal(
                    Ast.Call(
                        typeof(CompilerHelpers).GetMethod("GetType", new Type[] { typeof(object) }),
                        Ast.ConvertHelper(tested, typeof(object))
                    ),
                    rule.AddTemplatedConstant(typeof(Type), type.UnderlyingSystemType)
                );
            }

            if (!isStaticType) {
                int version = type.Version;
                if (version != DynamicType.DynamicVersion) {
                    test = Ast.AndAlso(
                        test,
                        Ast.Call(
                            typeof(RuntimeHelpers).GetMethod("CheckTypeVersion"),
                            Ast.ConvertHelper(tested, typeof(object)),
                            GetVersionExpression(rule, templatable, version)
                        )
                    );
                    rule.AddValidator(new DynamicTypeValidator(new WeakReference(type), version).Validate);
                } else {
                    version = type.AlternateVersion;
                    test = Ast.AndAlso(
                        test,
                        Ast.Call(
                            typeof(RuntimeHelpers).GetMethod("CheckAlternateTypeVersion"),
                            Ast.ConvertHelper(tested, typeof(object)),
                            GetVersionExpression(rule, templatable, version)
                        )
                    );
                    rule.AddValidator(new DynamicTypeValidator(new WeakReference(type), version).AlternateValidate);
                }
            }

            return test;
        }

        private static Expression GetVersionExpression(StandardRule rule, bool templatable, int version) {
            Expression verExpr;
            if (!templatable) {
                verExpr = Ast.Constant(version);
            } else {
                verExpr = rule.AddTemplatedConstant(typeof(int), version);
            }
            return verExpr;
        }

        /// <summary>
        /// Produces an error message for the provided message and type names.  The error message should contain
        /// string formatting characters ({0}, {1}, etc...) for each of the type names.
        /// </summary>
        public static StandardRule<T> TypeError<T>(string message, params DynamicType[] types) {
            StandardRule<T> ret = new StandardRule<T>();
            MakeTest(ret, true, types);
            Expression[] formatArgs = new Expression[types.Length + 1];
            for (int i = 1; i < formatArgs.Length; i++) {
                formatArgs[i] = ret.AddTemplatedConstant(typeof(string), DynamicTypeOps.GetName(types[i - 1]));
            }
            formatArgs[0] = ret.AddTemplatedConstant(typeof(string), message);
            Type[] typeArgs = CompilerHelpers.MakeRepeatedArray<Type>(typeof(object), types.Length + 1);
            typeArgs[0] = typeof(string);

            ret.SetTarget(
                Ast.Throw(
                    Ast.Call(
                        typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                        // ??? What does this exactly guarantee? is typeArgs.Length < 3, or can this deal with params array???
                        Ast.ComplexCallHelper(
                            typeof(String).GetMethod("Format", typeArgs),
                            formatArgs
                        )
                    )
                )
            );

            TemplatedRuleBuilder<T> builder;
            lock (_typeErrorTemplates) {
                object objBuilder;
                TypeErrorKey tek = new TypeErrorKey(typeof(T), types);
                if (!_typeErrorTemplates.TryGetValue(tek, out objBuilder)) {
                    _typeErrorTemplates[tek] = builder = ret.GetTemplateBuilder();
                } else {
                    builder = (TemplatedRuleBuilder<T>)objBuilder;
                    builder.CopyTemplateToRule(DefaultContext.Default, ret);
                }
            }
            return ret;
        }

        class TypeErrorKey {
            private Type _type;
            private bool[] _static;

            public TypeErrorKey(Type type, DynamicType[] types) {
                _type = type;
                _static = new bool[types.Length];
                for (int i = 0; i < types.Length; i++) {
                    _static[i] = types[i].IsSystemType;
                }
            }

            public override int GetHashCode() {
                return _type.GetHashCode();
            }

            public override bool Equals(object obj) {
                TypeErrorKey other = obj as TypeErrorKey;
                if (other == null) return false;

                if (_type != other._type) return false;
                if (_static.Length != other._static.Length) return false;

                for (int i = 0; i < _static.Length; i++) {
                    if (_static[i] != other._static[i]) return false;
                }
                return true;
            }
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
