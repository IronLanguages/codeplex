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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Actions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {
    
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
            PythonType type = MetaPythonObject.GetPythonType(mo);
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

        internal static MetaObject/*!*/ FilterShowCls(Expression/*!*/ codeContext, MetaAction/*!*/ action, MetaObject/*!*/ res, Expression/*!*/ failure) {
            if (action is IPythonSite) {
                Type resType = BindingHelpers.GetCompatibleType(res.Expression.Type, failure.Type);

                return new MetaObject(
                    Ast.Condition(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("IsClsVisible"),
                            codeContext
                        ),
                        Ast.ConvertHelper(res.Expression, resType),
                        Ast.ConvertHelper(failure, resType)

                    ),
                    res.Restrictions
                );
            }

            return res;
        }

        /// <summary>
        /// Gets the best CallSignature from a MetaAction.
        /// 
        /// The MetaAction should be either a Python InvokeBinder, or a DLR InvokeAction or 
        /// CreateAction.  For Python we can use a full-fidelity 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static CallSignature GetCallSignature(MetaAction/*!*/ action) {
            // Python'so own InvokeBinder which has a real sig
            InvokeBinder pib = action as InvokeBinder;
            if (pib != null) {
                return pib.Signature;
            }

            // DLR Invoke which has a argument array
            InvokeAction iac = action as InvokeAction;
            if (iac != null) {
                return ArgumentArrayToSignature(iac.Arguments);
            }

            // DLR Create action which we hand off to our call code, also
            // has an argument array.
            CreateAction ca = action as CreateAction;
            Debug.Assert(ca != null);

            return ArgumentArrayToSignature(ca.Arguments);
        }

        /// <summary>
        /// Transforms a call into a Python GetMember/Invoke.  This isn't quite the correct semantic as
        /// we shouldn't be returning Python members (e.g. object.__repr__) to non-Python callers.  This
        /// can go away as soon as all of the classes implement the full fidelity of the protocol
        /// </summary>
        internal static MetaObject/*!*/ GenericCall(CallAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            if (args[0].NeedsDeferral) {
                return action.Defer(args);
            }

            return new MetaObject(
                Binders.Invoke(
                    BinderState.GetBinderState(action),
                    typeof(object),
                    GetCallSignature(action),
                    ArrayUtils.Insert(
                        Binders.Get(
                            BinderState.GetBinderState(action),
                            typeof(object),
                            action.Name,
                            args[0].Expression
                        ),
                        MetaObject.GetExpressions(ArrayUtils.RemoveFirst(args))
                    )
                ),
                Restrictions.Combine(args).Merge(args[0].Restrict(args[0].LimitType).Restrictions)
            );
        }

        internal static CallSignature ArgumentArrayToSignature(ReadOnlyCollection<Argument/*!*/>/*!*/ args) {
            ArgumentInfo[] ai = new ArgumentInfo[args.Count];

            for (int i = 0; i < ai.Length; i++) {
                switch (args[i].ArgumentType) {
                    case ArgumentType.Named:
                        ai[i] = new ArgumentInfo(
                            ArgumentKind.Named,
                            SymbolTable.StringToId(((NamedArgument)args[i]).Name)
                        );
                        break;
                    case ArgumentType.Positional:
                        ai[i] = new ArgumentInfo(ArgumentKind.Simple);
                        break;
                }
            }

            return new CallSignature(ai);
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
            PythonType x = MetaPythonObject.GetPythonType(xType);
            PythonType y = MetaPythonObject.GetPythonType(yType);
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
                        AstUtils.Try(
                            Ast.Call(typeof(PythonOps).GetMethod("FunctionPushFrame")),
                            Ast.Assign(tmp, expr)
                        ).Finally(
                            Ast.Call(typeof(PythonOps).GetMethod("FunctionPopFrame"))
                        ),
                        tmp
                    ),
                    tmp
                );
            }
            return expr;
        }

        internal static Expression CreateBinderStateExpression() {
            return AstUtils.CodeContext();
        }

        /// <summary>
        /// Helper to do fallback for Invoke's so we can handle both StandardAction and Python's 
        /// InvokeBinder.
        /// </summary>
        internal static MetaObject/*!*/ InvokeFallback(MetaAction/*!*/ action, Expression codeContext, MetaObject/*!*/[]/*!*/ args) {
            StandardAction act = action as StandardAction;
            if (act != null) {
                return act.Fallback(args);
            }

            InvokeBinder invoke = action as InvokeBinder;
            if (invoke != null) {
                return invoke.Fallback(codeContext, args);
            }

            // unreachable, we always have one of these binders
            throw new InvalidOperationException();
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
