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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    static class PythonBinderHelper {
        public static Expression[] GetCollapsedIndexArguments<T>(OldDoOperationAction action, object[] args, RuleBuilder<T> rule) where T : class {
            int simpleArgCount = (action.Operation == Operators.GetItem || action.Operation == Operators.DeleteItem) ? 2 : 3;

            Expression[] exprargs = new Expression[simpleArgCount];
            exprargs[0] = rule.Context;

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

        public static void MakeTest(RuleBuilder rule, params PythonType[] types) {
            rule.Test = MakeTestForTypes(rule, types, 0);
        }

        public static Expression MakeTestForTypes(RuleBuilder rule, PythonType[] types, int index) {
            Debug.Assert(rule != null);
            Expression test = MakeTypeTest(rule, types[index], rule.Parameters[index]);
            if (index + 1 < types.Length) {
                Expression nextTests = MakeTestForTypes(rule, types, index + 1);
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

        public static Expression MakeTypeTest(RuleBuilder rule, PythonType type, Expression tested) {
            if (type == null || type.IsNull) {
                return Ast.Equal(tested, Ast.Null());
            }

            Type clrType = type.UnderlyingSystemType;
            bool isStaticType = !typeof(IPythonObject).IsAssignableFrom(clrType);

            Expression test = RuleBuilder.MakeTypeTestExpression(type.UnderlyingSystemType, tested);

            if (!isStaticType) {
                int version = type.Version;
                test = Ast.AndAlso(
                    test,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("CheckTypeVersion"),
                        Ast.ConvertHelper(tested, typeof(object)),
                        Ast.Constant(version)
                    )
                );
                rule.AddValidator(new PythonTypeValidator(new WeakReference(type), version).Validate);
            }

            return test;
        }

        /// <summary>
        /// Produces an error message for the provided message and type names.  The error message should contain
        /// string formatting characters ({0}, {1}, etc...) for each of the type names.
        /// </summary>
        public static RuleBuilder<T> TypeError<T>(string message, params PythonType[] types) where T : class {
            RuleBuilder<T> ret = new RuleBuilder<T>();
            MakeTest(ret, types);
            Expression[] formatArgs = new Expression[types.Length + 1];
            for (int i = 1; i < formatArgs.Length; i++) {
                formatArgs[i] = Ast.Constant(types[i - 1].Name);
            }
            formatArgs[0] = Ast.Constant(message);
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

            return ret;
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
        }

        //
        // Various helpers related to calling Python __*__ conversion methods 
        //
        internal static Expression GetConversionFailedReturnValue<T>(CodeContext context, OldConvertToAction convertToAction, RuleBuilder<T> rule) where T : class {
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
        internal static Expression GetConvertByLengthBody(RuleBuilder rule, CodeContext context, VariableExpression tmp) {
            ActionBinder binder = PythonContext.GetContext(context).Binder;
            return Ast.NotEqual(
                AstUtils.ConvertTo(
                    binder,
                    typeof(int),
                    rule.Context,
                    AstUtils.Call(
                        binder,
                        typeof(object),
                        rule.Context,
                        tmp
                    )
                ),
                Ast.Constant(0)
            );
        }
        
        // Make a rule for objects that have immutable isCallable property (like functions).
        internal static RuleBuilder<T> MakeIsCallableRule<T>(CodeContext context, object self, bool isCallable) where T : class {
            return BinderHelper.MakeIsCallableRule<T>(context, self, isCallable);
        }

        internal static MethodCallExpression MakeTryGetTypeMember<T>(RuleBuilder<T> rule, PythonTypeSlot dts, VariableExpression tmp) where T : class {
            return MakeTryGetTypeMember(rule, dts, tmp,
                rule.Parameters[0],
                Ast.Property(
                    Ast.Convert(
                        rule.Parameters[0],
                        typeof(IPythonObject)),
                    TypeInfo._IPythonObject.PythonType
                )
            );
        }

        internal static MethodCallExpression MakeTryGetTypeMember<T>(RuleBuilder<T> rule, PythonTypeSlot dts, VariableExpression tmp, Expression instance, Expression pythonType) where T : class {
            return Ast.Call(
                TypeInfo._PythonOps.SlotTryGetBoundValue,
                rule.Context,
                Ast.ConvertHelper(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                Ast.ConvertHelper(instance, typeof(object)),
                Ast.ConvertHelper(
                    pythonType,
                    typeof(PythonType)
                ),
                tmp
            );
        }


        // Rule to dynamically check for __call__ attribute.
        // Needed for instances whos call status can change over the lifetime of the instance.
        // For example, if we del(c.__call__) on a oldinstance, it's no longer callable.
        internal static RuleBuilder<T> MakeIsCallableRule<T>(CodeContext context, object self) where T : class {
            // Certain non-python types (encountered during interop) are callable, but don't have 
            // a __call__ attribute. The default base binder also checks these, but since we're overriding
            // the base binder, we check them here.
            Type tSelf = CompilerHelpers.GetType(self);
            if (typeof(Delegate).IsAssignableFrom(tSelf) ||
                typeof(MethodGroup).IsAssignableFrom(tSelf)) {
                return MakeIsCallableRule<T>(context, self, true);
            }

            RuleBuilder<T> rule = new RuleBuilder<T>();
            rule.MakeTest(tSelf);

            // Rule is:
            //    getmember(self, __call__) != OperationFailed.Value
            rule.Target =
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.NotEqual(
                        AstUtils.GetMember(
                            PythonContext.GetContext(context).Binder,
                            "__call__",
                            GetMemberBindingFlags.Bound | GetMemberBindingFlags.NoThrow,
                            typeof(System.Object),
                            rule.Context,
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
                expr = AstUtils.Try(
                    Ast.Call(typeof(PythonOps).GetMethod("FunctionPushFrame")),
                    expr
                ).Finally(
                    Ast.Call(typeof(PythonOps).GetMethod("FunctionPopFrame"))
                );
            }
            return expr;
        }
    }
}
