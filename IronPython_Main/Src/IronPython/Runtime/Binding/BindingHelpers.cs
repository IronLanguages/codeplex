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
using System.Scripting;
using System.Scripting.Actions;
using System.Linq.Expressions;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;
    using System.Collections.Generic;
    using System.Diagnostics;
    
    /// <summary>
    /// Common helpers used by the various binding logic.
    /// </summary>
    static class BindingHelpers {
        /// <summary>
        /// Trys to get the BuiltinFunction for the given name on the type of the provided MetaObject.  
        /// 
        /// Succeeds if the MetaObject is a BuiltinFunction, BuiltinMethodDescriptor, or BoundBuiltinFunction.
        /// </summary>
        internal static bool TryGetStaticFunction(BinderState/*!*/ state, SymbolId op, MetaObject/*!*/ mo, out BuiltinFunction function) {
            PythonType type = DynamicHelpers.GetPythonType(mo.Value);
            function = null;
            if (op != SymbolId.Empty) {
                PythonTypeSlot xSlot;
                object val;
                if (type.TryResolveSlot(state.Context, op, out xSlot) &&
                    xSlot.TryGetValue(state.Context, null, type, out val)) {
                    function = TryConvertToBuiltinFunction(val);
                    if (function == null) return false;
                }
            }
            return true;
        }

        internal static bool IsNoThrow(GetMemberAction action) {
            GetMemberBinder gmb = action as GetMemberBinder;
            if (gmb != null) {
                return gmb.IsNoThrow;
            }

            return true;
        }

        internal static Expression GetSiteCodeContext() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("GetContextFromPythonSite"),
                MetaAction.GetSelfExpression()
            );
        }

        internal static MetaObject/*!*/ FilterShowCls(MetaAction/*!*/ action, MetaObject/*!*/ res, Expression/*!*/ failure) {
            if (action is IPythonSite) {
                Type resType = BindingHelpers.GetCompatibleType(res.Expression.Type, failure.Type);

                return new MetaObject(
                    Ast.Condition(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("IsClsVisible"),
                            BindingHelpers.GetSiteCodeContext()
                        ),
                        Ast.ConvertHelper(res.Expression, resType),
                        Ast.ConvertHelper(failure, resType)

                    ),
                    res.Restrictions
                );
            }

            return res;
        }

        internal static CallSignature GetCallSignature(InvokeAction/*!*/ action) {
            InvokeBinder ib = action as InvokeBinder;
            if (ib != null) {
                return ib.Signature;
            }

            ArgumentInfo[] ai = new ArgumentInfo[action.Arguments.Count];

            for (int i = 0; i < ai.Length; i++) {
                switch (action.Arguments[i].ArgumentType) {
                    case ArgumentType.Named:
                        ai[i] = new ArgumentInfo(
                            ArgumentKind.Named,
                            SymbolTable.StringToId(((NamedArgument)action.Arguments[i]).Name)
                        );
                        break;
                    case ArgumentType.Positional:
                        ai[i] = new ArgumentInfo(ArgumentKind.Simple);
                        break;
                }
            }

            return new CallSignature(ai);
        }

        internal static IList<Argument/*!*/>/*!*/ GetArguments(CallSignature signature) {
            Argument[] args = new Argument[signature.ArgumentCount];
            ArgumentInfo[] sigArgs = signature.GetArgumentInfos();
            for (int i = 0; i < sigArgs.Length; i++) {
                switch (sigArgs[i].Kind) {
                    case ArgumentKind.Named:
                        args[i] = Ast.NamedArg(SymbolTable.IdToString(sigArgs[i].Name));
                        break;
                    case ArgumentKind.Simple:
                        args[i] = Ast.PositionalArg(i);
                        break;
                    default:
                        // BUGBUG!                        
                        args[i] = Ast.PositionalArg(Int32.MaxValue);
                        break;
                }
            }

            return args;
        }

        internal static Type/*!*/ GetCompatibleType(/*!*/Type t, Type/*!*/ otherType) {
            if (t != otherType) {
                if (t.IsSubclassOf(otherType)) {
                    // subclass
                    t = otherType;
                } else if (otherType.IsSubclassOf(t)) {
                    // keep t
                } else {
                    // incompatible, both go to object
                    t = typeof(object);
                }
            }
            return t;
        }

        /// <summary>
        /// Determines if the type associated with the first MetaObject is a subclass of the
        /// type associated with the second MetaObject.
        /// </summary>
        internal static bool IsSubclassOf(MetaObject/*!*/ xType, MetaObject/*!*/ yType) {
            PythonType x = DynamicHelpers.GetPythonType(xType.Value);
            PythonType y = DynamicHelpers.GetPythonType(yType.Value);
            return x.IsSubclassOf(y);
        }
        
        private static BuiltinFunction TryConvertToBuiltinFunction(object o) {
            BuiltinMethodDescriptor md = o as BuiltinMethodDescriptor;

            if (md != null) {
                return md.Template;
            }

            BoundBuiltinFunction bbf = o as BoundBuiltinFunction;
            if (bbf != null) {
                return bbf.Target;
            }

            return o as BuiltinFunction;
        }

        internal static MetaObject/*!*/ AddDynamicTestAndDefer(MetaAction/*!*/ operation, MetaObject/*!*/ res, MetaObject/*!*/[] args, ValidationInfo typeTest, params VariableExpression[] temps) {
            return AddDynamicTestAndDefer(operation, res, args, typeTest, null, temps);
        }

        internal static MetaObject/*!*/ AddDynamicTestAndDefer(MetaAction/*!*/ operation, MetaObject/*!*/ res, MetaObject/*!*/[] args, ValidationInfo typeTest, Type deferType, params VariableExpression[] temps) {
            if (typeTest != null) {
                if (typeTest.Test != null) {
                    // add the test and a validator if persent
                    Expression defer = operation.Defer(args).Expression;
                    if (deferType != null) {
                        defer = Ast.ConvertHelper(defer, deferType);
                    }

                    Type bestType = BindingHelpers.GetCompatibleType(defer.Type, res.Expression.Type);

                    res = new MetaObject(
                        Ast.Condition(
                            typeTest.Test,
                            Ast.ConvertHelper(res.Expression, bestType),
                            Ast.ConvertHelper(defer, bestType)
                        ),
                        res.Restrictions // ,
                        //typeTest.Validator
                    );
                } else if (typeTest.Validator != null) {
                    // just add the validator
                    res = new MetaObject(res.Expression, res.Restrictions); // , typeTest.Validator
                }
            } 
            
            if (temps.Length > 0) {
                // finally add the scoped variables
                res = new MetaObject(
                    Ast.Scope(res.Expression, temps),
                    res.Restrictions,
                    null
                );
            }

            return res;
        }
        
        internal static Expression MakeTypeTests(params MetaObject/*!*/[] args) {
            Expression typeTest = null;

            for (int i = 0; i < args.Length; i++) {
                if (args[i].HasValue) {
                    IPythonObject val = args[i].Value as IPythonObject;
                    if (val != null) {
                        Expression test = CheckTypeVersion(args[i].Expression, val.PythonType.Version);

                        if (typeTest != null) {
                            typeTest = Ast.AndAlso(typeTest, test);
                        } else {
                            typeTest = test;
                        }
                    }
                }
            }


            return typeTest;
        }

        internal static MethodCallExpression/*!*/ CheckTypeVersion(Expression/*!*/ tested, int version) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("CheckTypeVersion"),
                Ast.ConvertHelper(tested, typeof(object)),
                Ast.Constant(version)
            );
        }

        internal static ValidationInfo/*!*/ GetValidationInfo(Expression/*!*/ tested, PythonType type) {
            int version = type.Version;

            return new ValidationInfo(
                Ast.Call(
                    typeof(PythonOps).GetMethod("CheckTypeVersion"),
                    Ast.ConvertHelper(tested, typeof(object)),
                    Ast.Constant(version)
                ),
                new PythonTypeValidator(type, version).Validate
            );
        }

        public static ValidationInfo GetValidationInfo(MetaObject metaSelf, params MetaObject[] args) {
            Func<bool> validation = null;
            Expression typeTest = null;
            if (metaSelf != null) {
                IPythonObject self = metaSelf.Value as IPythonObject;
                if (self != null) {
                    PythonType pt = self.PythonType;
                    int version = pt.Version;

                    typeTest = BindingHelpers.CheckTypeVersion(metaSelf.Expression, version);
                    validation = ValidatorAnd(validation, new PythonTypeValidator(pt, version).Validate);
                }
            }

            for (int i = 0; i < args.Length; i++) {
                if (args[i].HasValue) {
                    IPythonObject val = args[i].Value as IPythonObject;
                    if (val != null) {
                        Expression test = BindingHelpers.CheckTypeVersion(args[i].Expression, val.PythonType.Version);
                        PythonType pt = val.PythonType;
                        int version = pt.Version;

                        validation = ValidatorAnd(validation, new PythonTypeValidator(pt, version).Validate);
                        
                        if (typeTest != null) {
                            typeTest = Ast.AndAlso(typeTest, test);
                        } else {
                            typeTest = test;
                        }
                    }
                }
            }

            return new ValidationInfo(typeTest, validation);
        }

        private static Func<bool> ValidatorAnd(Func<bool> self, Func<bool> other) {
            if (self == null) {
                return other;
            } else if (other == null) {
                return self;
            }

            return delegate() {
                return self() && other();
            };
        }
        
        internal class PythonTypeValidator {
            /// <summary>
            /// Weak reference to the dynamic type. Since they can be collected,
            /// we need to be able to let that happen and then disable the rule.
            /// </summary>
            private WeakReference _pythonType;

            /// <summary>
            /// Expected version of the instance's dynamic type
            /// </summary>
            private int _version;

            public PythonTypeValidator(PythonType pythonType, int version) {
                this._pythonType = new WeakReference(pythonType);
                this._version = version;
            }

            public bool Validate() {
                PythonType dt = _pythonType.Target as PythonType;
                return dt != null && dt.Version == _version;
            }
        }

        /// <summary>
        /// Adds a try/finally which enforces recursion limits around the target method.
        /// </summary>
        internal static Expression AddRecursionCheck(Expression expr) {
            if (PythonFunction.EnforceRecursion) {
                VariableExpression tmp = Ast.Variable(expr.Type, "callres");

                expr = Ast.Scope(
                    Ast.Comma(
                        Ast.Call(typeof(PythonOps).GetMethod("FunctionPushFrame")),
                        Ast.TryFinally(
                            Ast.Assign(tmp, expr),
                            Ast.Block(
                                Ast.Call(typeof(PythonOps).GetMethod("FunctionPopFrame"))
                            )
                        ),
                        tmp
                    ),
                    tmp
                );
            }
            return expr;
        }

        internal static Expression CreateBinderStateExpression() {
            return Ast.CodeContext();
        }
    }

    internal class ValidationInfo {
        public readonly Expression Test;
        public readonly Func<bool> Validator;
        public static readonly ValidationInfo Empty = new ValidationInfo(null, null);

        public ValidationInfo(Expression test, Func<bool> validator) {
            Test = test;
            Validator = validator;
        }
    }
}
