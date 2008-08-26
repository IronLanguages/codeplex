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
using System.Linq.Expressions;
using System.Scripting.Actions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    class MetaOldClass : MetaPythonObject, IPythonInvokable {
        public MetaOldClass(Expression/*!*/ expression, Restrictions/*!*/ restrictions, OldClass/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(InvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args) {
            return MakeCallRule(pythonInvoke, codeContext, ArrayUtils.RemoveFirst(args));
        }

        #endregion

        #region MetaObject Overrides

        public override MetaObject/*!*/ Call(CallAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, args);
        }

        public override MetaObject/*!*/ Invoke(InvokeAction/*!*/ call, params MetaObject/*!*/[]/*!*/ args) {
            return MakeCallRule(call, Ast.Constant(BinderState.GetBinderState(call).Context), ArrayUtils.RemoveFirst(args));
        }

        public override MetaObject/*!*/ Create(CreateAction/*!*/ create, params MetaObject/*!*/[]/*!*/ args) {
            return MakeCallRule(create, Ast.Constant(BinderState.GetBinderState(create).Context), args);
        }

        public override MetaObject/*!*/ GetMember(GetMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            return MakeGetMember(member, args);
        }

        public override MetaObject/*!*/ SetMember(SetMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            return MakeSetMember(member.Name, args[1]);
        }

        public override MetaObject/*!*/ DeleteMember(DeleteMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            return MakeDeleteMember(member);
        }

        public override MetaObject/*!*/ Operation(OperationAction operation, params MetaObject/*!*/[]/*!*/ args) {
            if (operation.Operation == StandardOperators.IsCallable) {
                return new MetaObject(
                    Ast.Constant(true),
                    Restrictions.Merge(Restrictions.TypeRestriction(Expression, typeof(OldClass)))
                );
            }

            return base.Operation(operation, args);
        }

        public override MetaObject Convert(ConvertAction/*!*/ conversion, MetaObject/*!*/[]/*!*/ args) {
            if (conversion.ToType.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateTarget(conversion, conversion.ToType, Restrict(typeof(OldClass)));
            }
            return conversion.Fallback(args);
        }

        #endregion

        #region Calls

        private MetaObject/*!*/ MakeCallRule(MetaAction/*!*/ call, Expression/*!*/ codeContext, MetaObject[] args) {
            CallSignature signature = BindingHelpers.GetCallSignature(call);
            // TODO: If we know __init__ wasn't present we could construct the OldInstance directly.

            Expression[] exprArgs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++) {
                exprArgs[i] = args[i].Expression;
            }

            VariableExpression init = Ast.Variable(typeof(object), "init");
            VariableExpression instTmp = Ast.Variable(typeof(object), "inst");
            MetaObject self = Restrict(typeof(OldClass));

            return new MetaObject(
                Ast.Scope(
                    Ast.Comma(
                        Ast.Assign(
                            instTmp,
                            Ast.New(
                                typeof(OldInstance).GetConstructor(new Type[] { typeof(CodeContext), typeof(OldClass) }),
                                codeContext,
                                self.Expression
                            )
                        ),
                        Ast.Condition(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("OldClassTryLookupInit"),
                                self.Expression,
                                instTmp,
                                init
                            ),
                            Ast.Dynamic(
                                new InvokeBinder(
                                    BinderState.GetBinderState(call),
                                    signature
                                ),
                                typeof(object),
                                ArrayUtils.Insert<Expression>(codeContext, init, exprArgs)
                            ),
                            NoInitCheckNoArgs(signature, self, args)
                        ),
                        instTmp
                    ),
                    init,
                    instTmp
                ),
                self.Restrictions.Merge(Restrictions.Combine(args))
            );
        }

        private Expression NoInitCheckNoArgs(CallSignature signature, MetaObject self, MetaObject[] args) {
            int unusedCount = args.Length;

            Expression dictExpr = GetArgumentExpression(signature, ArgumentKind.Dictionary, ref unusedCount, args);
            Expression listExpr = GetArgumentExpression(signature, ArgumentKind.List, ref unusedCount, args);

            if (signature.IsSimple || unusedCount > 0) {
                if (args.Length > 0) {
                    return Ast.Call(
                        typeof(PythonOps).GetMethod("OldClassMakeCallError"),
                        self.Expression
                    );
                }

                return Ast.Null();
            }

            return Ast.Call(
                typeof(PythonOps).GetMethod("OldClassCheckCallError"),
                self.Expression,
                dictExpr,
                listExpr
            );
        }

        private Expression GetArgumentExpression(CallSignature signature, ArgumentKind kind, ref int unusedCount, MetaObject/*!*/[]/*!*/ args) {
            int index = signature.IndexOf(kind);
            if (index != -1) {
                unusedCount--;
                return args[index].Expression;
            }

            return Ast.Null();
        }

        public object MakeCallError() {
            // Normally, if we have an __init__ method, the method binder detects signature mismatches.
            // This can happen when a class does not define __init__ and therefore does not take any arguments.
            // Beware that calls like F(*(), **{}) have 2 arguments but they're empty and so it should still
            // match against def F(). 
            throw PythonOps.TypeError("this constructor takes no arguments");
        }

        #endregion

        #region Member Access

        private MetaObject/*!*/ MakeSetMember(string/*!*/ name, MetaObject/*!*/ value) {
            MetaObject self = Restrict(typeof(OldClass));

            Expression call, valueExpr = Ast.ConvertHelper(value.Expression, typeof(object));
            switch (name) {
                case "__bases__":
                    call = Ast.Call(
                        typeof(PythonOps).GetMethod("OldClassSetBases"),
                        self.Expression,
                        valueExpr
                    );
                    break;
                case "__name__":
                    call = Ast.Call(
                        typeof(PythonOps).GetMethod("OldClassSetName"),
                        self.Expression,
                        valueExpr
                    );
                    break;
                case "__dict__":
                    call = Ast.Call(
                        typeof(PythonOps).GetMethod("OldClassSetDictionary"),
                        self.Expression,
                        valueExpr
                    );
                    break;
                default:
                    call = Ast.Call(
                        typeof(PythonOps).GetMethod("OldClassSetNameHelper"),
                        self.Expression,
                        AstUtils.Constant(SymbolTable.StringToId(name)),
                        valueExpr
                    );
                    break;
            }

            return new MetaObject(
                call,
                self.Restrictions.Merge(value.Restrictions)
            );
        }

        private MetaObject/*!*/ MakeDeleteMember(DeleteMemberAction/*!*/ member) {
            MetaObject self = Restrict(typeof(OldClass));

            return new MetaObject(
                Ast.Call(
                    typeof(PythonOps).GetMethod("OldClassDeleteMember"),
                    Ast.Constant(BinderState.GetBinderState(member).Context),
                    self.Expression,
                    AstUtils.Constant(SymbolTable.StringToId(member.Name))
                ),
                self.Restrictions
            );
        }

        private MetaObject/*!*/ MakeGetMember(GetMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            MetaObject self = Restrict(typeof(OldClass));

            Expression target;
            switch (member.Name) {
                case "__dict__":
                    target = Ast.Comma(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("OldClassDictionaryIsPublic"),
                            self.Expression
                        ),
                        Ast.Field(
                            self.Expression,
                            typeof(OldClass).GetField("__dict__")
                        )
                    );
                    break;
                case "__bases__":
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldClassGetBaseClasses"),
                        self.Expression
                    );
                    break;
                case "__name__":
                    target = Ast.Property(
                        self.Expression,
                        typeof(OldClass).GetProperty("__name__")
                    );
                    break;
                default:
                    VariableExpression tmp = Ast.Variable(typeof(object), "lookupVal");
                    return new MetaObject(
                        Ast.Scope(
                            Ast.Condition(
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldClassTryLookupValue"),
                                    Ast.Constant(BinderState.GetBinderState(member).Context),
                                    self.Expression,
                                    AstUtils.Constant(SymbolTable.StringToId(member.Name)),
                                    tmp
                                ),
                                tmp,
                                Ast.ConvertHelper(
                                    member.Fallback(args).Expression,
                                    typeof(object)
                                )
                            ),
                            tmp
                        ),
                        self.Restrictions
                    );
            }

            return new MetaObject(
                target,
                self.Restrictions
            );
        }       

        #endregion

        #region Helpers

        public new OldClass/*!*/ Value {
            get {
                return (OldClass)base.Value;
            }
        }

        #endregion
    }
}
