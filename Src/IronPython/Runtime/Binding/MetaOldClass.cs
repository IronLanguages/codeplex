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

using System; using Microsoft;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;

    class MetaOldClass : MetaPythonObject, IPythonInvokable, IPythonGetable {
        public MetaOldClass(Expression/*!*/ expression, Restrictions/*!*/ restrictions, OldClass/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(PythonInvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args) {
            return MakeCallRule(pythonInvoke, codeContext, args);
        }

        #endregion

        #region IPythonGetable Members

        public MetaObject GetMember(PythonGetMemberBinder member, Expression codeContext) {
            // no codeContext filtering but avoid an extra site by handling this action directly
            return MakeGetMember(member, codeContext);
        }

        #endregion

        #region MetaObject Overrides

        public override MetaObject/*!*/ BindInvokeMember(InvokeMemberBinder/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, this, args);
        }

        public override MetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ call, params MetaObject/*!*/[]/*!*/ args) {
            return MakeCallRule(call, Ast.Constant(BinderState.GetBinderState(call).Context), args);
        }

        public override MetaObject/*!*/ BindCreateInstance(CreateInstanceBinder/*!*/ create, params MetaObject/*!*/[]/*!*/ args) {
            return MakeCallRule(create, Ast.Constant(BinderState.GetBinderState(create).Context), args);
        }

        public override MetaObject/*!*/ BindGetMember(GetMemberBinder/*!*/ member) {
            return MakeGetMember(member, BinderState.GetCodeContext(member));
        }

        public override MetaObject/*!*/ BindSetMember(SetMemberBinder/*!*/ member, MetaObject/*!*/ value) {
            return MakeSetMember(member.Name, value);
        }

        public override MetaObject/*!*/ BindDeleteMember(DeleteMemberBinder/*!*/ member) {
            return MakeDeleteMember(member);
        }

        [Obsolete]
        public override MetaObject/*!*/ BindOperation(OperationBinder operation, params MetaObject/*!*/[]/*!*/ args) {
            if (operation.Operation == StandardOperators.IsCallable) {
                return new MetaObject(
                    Ast.Constant(true),
                    Restrictions.Merge(Restrictions.GetTypeRestriction(Expression, typeof(OldClass)))
                );
            }

            return base.BindOperation(operation, args);
        }

        public override MetaObject BindConvert(ConvertBinder/*!*/ conversion) {
            if (conversion.Type.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateTarget(conversion, conversion.Type, Restrict(typeof(OldClass)));
            }
            return conversion.FallbackConvert(this);
        }

        #endregion

        #region Calls

        private MetaObject/*!*/ MakeCallRule(MetaObjectBinder/*!*/ call, Expression/*!*/ codeContext, MetaObject[] args) {
            CallSignature signature = BindingHelpers.GetCallSignature(call);
            // TODO: If we know __init__ wasn't present we could construct the OldInstance directly.

            Expression[] exprArgs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++) {
                exprArgs[i] = args[i].Expression;
            }

            ParameterExpression init = Ast.Variable(typeof(object), "init");
            ParameterExpression instTmp = Ast.Variable(typeof(object), "inst");
            MetaObject self = Restrict(typeof(OldClass));

            return new MetaObject(
                Ast.Block(
                    new ParameterExpression[] { init, instTmp },
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
                            new PythonInvokeBinder(
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
                self.Restrictions.Merge(Restrictions.Combine(args))
            );
        }

        private Expression NoInitCheckNoArgs(CallSignature signature, MetaObject self, MetaObject[] args) {
            int unusedCount = args.Length;

            Expression dictExpr = GetArgumentExpression(signature, ArgumentType.Dictionary, ref unusedCount, args);
            Expression listExpr = GetArgumentExpression(signature, ArgumentType.List, ref unusedCount, args);

            if (signature.IsSimple || unusedCount > 0) {
                if (args.Length > 0) {
                    return Ast.Call(
                        typeof(PythonOps).GetMethod("OldClassMakeCallError"),
                        self.Expression
                    );
                }

                return Ast.Constant(null);
            }

            return Ast.Call(
                typeof(PythonOps).GetMethod("OldClassCheckCallError"),
                self.Expression,
                dictExpr,
                listExpr
            );
        }

        private Expression GetArgumentExpression(CallSignature signature, ArgumentType kind, ref int unusedCount, MetaObject/*!*/[]/*!*/ args) {
            int index = signature.IndexOf(kind);
            if (index != -1) {
                unusedCount--;
                return args[index].Expression;
            }

            return Ast.Constant(null);
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

            Expression call, valueExpr = AstUtils.Convert(value.Expression, typeof(object));
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

        private MetaObject/*!*/ MakeDeleteMember(DeleteMemberBinder/*!*/ member) {
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

        private MetaObject/*!*/ MakeGetMember(MetaObjectBinder/*!*/ member, Expression codeContext) {
            MetaObject self = Restrict(typeof(OldClass));

            Expression target;
            string memberName = GetGetMemberName(member);
            switch (memberName) {
                case "__dict__":
                    target = Ast.Block(
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
                    ParameterExpression tmp = Ast.Variable(typeof(object), "lookupVal");
                    return new MetaObject(
                        Ast.Block(
                            new ParameterExpression[] { tmp },
                            Ast.Condition(
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldClassTryLookupValue"),
                                    Ast.Constant(BinderState.GetBinderState(member).Context),
                                    self.Expression,
                                    AstUtils.Constant(SymbolTable.StringToId(memberName)),
                                    tmp
                                ),
                                tmp,
                                AstUtils.Convert(
                                    GetMemberFallback(member, codeContext).Expression,
                                    typeof(object)
                                )
                            )
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
