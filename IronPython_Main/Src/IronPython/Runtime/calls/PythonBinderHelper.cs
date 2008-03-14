/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Calls {
    static class PythonBinderHelper {
        /// <summary>
        /// dictionary of templated type error rules.
        /// </summary>
        private static readonly Dictionary<TypeErrorKey, object> _typeErrorTemplates = new Dictionary<TypeErrorKey, object>();

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
                exprargs[2] = rule.Parameters[rule.Parameters.Count - 1];
            }

            return exprargs;
        }

        public static void MakeTest(StandardRule rule, bool templatable, params PythonType[] types) {
            rule.Test = MakeTestForTypes(rule, types, templatable, 0);
        }

        public static void MakeTest(StandardRule rule, params PythonType[] types) {
            rule.Test = MakeTestForTypes(rule, types, 0);
        }

        public static Expression MakeTestForTypes(StandardRule rule, PythonType[] types, int index) {
            return MakeTestForTypes(rule, types, false, index);
        }

        public static Expression MakeTestForTypes(StandardRule rule, PythonType[] types, bool templatable, int index) {
            Debug.Assert(rule != null);
            Expression test = MakeTypeTest(rule, types[index], rule.Parameters[index], templatable);
            if (index + 1 < types.Length) {
                Expression nextTests = MakeTestForTypes(rule, types, templatable, index + 1);
                if (ConstantCheck.Check(test, true)) {
                    return nextTests;
                } else if (ConstantCheck.Check(nextTests, true)) {
                    return test;
                } else {
                    return Ast.AndAlso(test, nextTests);
                }
            } else {
                return test;
            }
        }

        public static Expression MakeTypeTest(StandardRule rule, PythonType type, Expression tested, bool templatable) {
            if (!templatable && (type == null || type.IsNull)) {
                return Ast.Equal(tested, Ast.Null());
            }

            Type clrType = type.UnderlyingSystemType;
            bool isStaticType = !typeof(IPythonObject).IsAssignableFrom(clrType);

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
                if (version != PythonType.DynamicVersion) {
                    test = Ast.AndAlso(
                        test,
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CheckTypeVersion"),
                            Ast.ConvertHelper(tested, typeof(object)),
                            GetVersionExpression(rule, templatable, version)
                        )
                    );
                    rule.AddValidator(new PythonTypeValidator(new WeakReference(type), version).Validate);
                } else {
                    version = type.AlternateVersion;
                    test = Ast.AndAlso(
                        test,
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CheckAlternateTypeVersion"),
                            Ast.ConvertHelper(tested, typeof(object)),
                            GetVersionExpression(rule, templatable, version)
                        )
                    );
                    rule.AddValidator(new PythonTypeValidator(new WeakReference(type), version).AlternateValidate);
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
        public static StandardRule<T> TypeError<T>(string message, params PythonType[] types) {
            StandardRule<T> ret = new StandardRule<T>();
            MakeTest(ret, true, types);
            Expression[] formatArgs = new Expression[types.Length + 1];
            for (int i = 1; i < formatArgs.Length; i++) {
                formatArgs[i] = ret.AddTemplatedConstant(typeof(string), PythonTypeOps.GetName(types[i - 1]));
            }
            formatArgs[0] = ret.AddTemplatedConstant(typeof(string), message);
            Type[] typeArgs = CompilerHelpers.MakeRepeatedArray<Type>(typeof(object), types.Length + 1);
            typeArgs[0] = typeof(string);

            ret.Target = 
                Ast.Throw(
                    Ast.Call(
                        typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                        // ??? What does this exactly guarantee? is typeArgs.Length < 3, or can this deal with params array???
                        Ast.ComplexCallHelper(
                            typeof(String).GetMethod("Format", typeArgs),
                            formatArgs
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

            public TypeErrorKey(Type type, PythonType[] types) {
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

        private class PythonTypeValidator {
            /// <summary>
            /// Weak reference to the dynamic type. Since they can be collected,
            /// we need to be able to let that happen and then disable the rule.
            /// </summary>
            private WeakReference _pythonType;

            /// <summary>
            /// Expected version of the instance's dynamic type
            /// </summary>
            private int _version;

            public PythonTypeValidator(WeakReference pythonType, int version) {
                this._pythonType = pythonType;
                this._version = version;
            }

            public bool Validate() {
                PythonType dt = _pythonType.Target as PythonType;
                return dt != null && dt.Version == _version;
            }
            public bool AlternateValidate() {
                PythonType dt = _pythonType.Target as PythonType;
                return dt != null && dt.AlternateVersion == _version;
            }
        }

        //
        // Various helpers related to calling Python __*__ conversion methods 
        //
        internal static Expression GetConversionFailedReturnValue<T>(CodeContext context, ConvertToAction convertToAction, StandardRule<T> rule) {
            ErrorInfo error = context.LanguageContext.Binder.MakeConversionError(convertToAction.ToType, rule.Parameters[0]);
            Expression failed;
            switch (convertToAction.ResultKind) {
                case ConversionResultKind.ImplicitTry:
                case ConversionResultKind.ExplicitTry:
                    failed = CompilerHelpers.GetTryConvertReturnValue(context, rule);
                    break;
                case ConversionResultKind.ExplicitCast:
                case ConversionResultKind.ImplicitCast:
                    failed = error.MakeErrorForRule(rule, context.LanguageContext.Binder);
                    break;
                default: throw new InvalidOperationException(convertToAction.ResultKind.ToString());
            }
            return failed;
        }

        /// <summary>
        /// Used for conversions to bool
        /// </summary>
        internal static Expression GetConvertByLengthBody(VariableExpression tmp) {
            return Ast.NotEqual(
                Ast.Action.ConvertTo(
                    typeof(int),
                    Ast.Action.Call(
                        typeof(object),
                        Ast.Read(tmp)
                    )
                ),
                Ast.Constant(0)
            );
        }
        
        // Make a rule for objects that have immutable isCallable property (like functions).
        internal static StandardRule<T> MakeIsCallableRule<T>(CodeContext context, object self, bool isCallable) {
            return BinderHelper.MakeIsCallableRule<T>(context, self, isCallable);
        }

        internal static MethodCallExpression MakeTryGetTypeMember<T>(StandardRule<T> rule, PythonTypeSlot dts, VariableExpression tmp) {
            return MakeTryGetTypeMember(rule, dts, tmp,
                rule.Parameters[0],
                Ast.ReadProperty(
                    Ast.Convert(
                        rule.Parameters[0],
                        typeof(IPythonObject)),
                    typeof(IPythonObject).GetProperty("PythonType")
                )
            );
        }

        internal static MethodCallExpression MakeTryGetTypeMember<T>(StandardRule<T> rule, PythonTypeSlot dts, VariableExpression tmp, Expression instance, Expression pythonType) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("SlotTryGetBoundValue"),
                Ast.CodeContext(),
                Ast.ConvertHelper(Ast.WeakConstant(dts), typeof(PythonTypeSlot)),
                Ast.ConvertHelper(instance, typeof(object)),
                Ast.ConvertHelper(
                    pythonType,
                    typeof(PythonType)
                ),
                Ast.ReadDefined(tmp)
            );
        }


        // Rule to dynamically check for __call__ attribute.
        // Needed for instances whos call status can change over the lifetime of the instance.
        // For example, if we del(c.__call__) on a oldinstance, it's no longer callable.
        internal static StandardRule<T> MakeIsCallableRule<T>(CodeContext context, object self) {
            // Certain non-python types (encountered during interop) are callable, but don't have 
            // a __call__ attribute. The default base binder also checks these, but since we're overriding
            // the base binder, we check them here.
            Type tSelf = CompilerHelpers.GetType(self);
            if (typeof(Delegate).IsAssignableFrom(tSelf) ||
                typeof(MethodGroup).IsAssignableFrom(tSelf)) {
                return MakeIsCallableRule<T>(context, self, true);
            }

            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(tSelf);

            // Rule is:
            //    getmember(self, __call__) != OperationFailed.Value
            rule.Target =
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.NotEqual(
                        Ast.Action.GetMember(
                            Symbols.Call,
                            GetMemberBindingFlags.Bound | GetMemberBindingFlags.NoThrow,
                            typeof(System.Object),
                            rule.Parameters[0]),
                        Ast.Constant(OperationFailed.Value))
                );

            return rule;
        }

        /// <summary>
        /// Adds a try/finally which enforces recursion limits around the target method.
        /// </summary>
        public static Expression AddRecursionCheck(Expression expr) {
            if (PythonFunction.EnforceRecursion) {
                expr = Ast.Block(
                    Ast.Call(typeof(PythonOps).GetMethod("FunctionPushFrame")),
                    Ast.TryFinally(
                        expr,
                        Ast.Block(
                            Ast.Call(typeof(PythonOps).GetMethod("FunctionPopFrame"))
                        )
                    )
                );
            }
            return expr;
        }
    }
}
