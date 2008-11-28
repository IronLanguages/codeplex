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
using System.Collections;
using System.Collections.Generic;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;

    /// <summary>
    /// Provides a MetaObject for instances of Python's old-style classes.
    /// 
    /// TODO: Lots of CodeConetxt references, need to move CodeContext onto OldClass and pull it from there.
    /// </summary>
    class MetaOldInstance : MetaPythonObject, IPythonInvokable, IPythonGetable {        
        public MetaOldInstance(Expression/*!*/ expression, Restrictions/*!*/ restrictions, OldInstance/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(PythonInvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, codeContext, args);
        }

        #endregion

        #region IPythonGetable Members

        public MetaObject GetMember(PythonGetMemberBinder member, Expression codeContext) {
            // no codeContext filtering but avoid an extra site by handling this action directly
            return MakeMemberAccess(member, member.Name, MemberAccess.Get, this);
        }

        #endregion

        #region MetaObject Overrides

        public override MetaObject/*!*/ BindInvokeMember(InvokeMemberBinder/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, this, args);
        }

        public override MetaObject/*!*/ BindGetMember(GetMemberBinder/*!*/ member) {
            return MakeMemberAccess(member, member.Name, MemberAccess.Get, this);
        }

        public override MetaObject/*!*/ BindSetMember(SetMemberBinder/*!*/ member, MetaObject/*!*/ value) {
            return MakeMemberAccess(member, member.Name, MemberAccess.Set, this, value);
        }

        public override MetaObject/*!*/ BindDeleteMember(DeleteMemberBinder/*!*/ member) {
            return MakeMemberAccess(member, member.Name, MemberAccess.Delete, this);
        }

        [Obsolete]
        public override MetaObject/*!*/ BindOperation(OperationBinder/*!*/ operation, params MetaObject/*!*/[]/*!*/ args) {
            if (operation.Operation == StandardOperators.IsCallable) {
                return MakeIsCallable(operation);
            }

            return base.BindOperation(operation, args);
        }

        public override MetaObject/*!*/ BindConvert(ConvertBinder/*!*/ conversion) {
            Type type = conversion.Type;

            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Boolean:
                        return MakeConvertToBool(conversion);
                    case TypeCode.Int32:
                        return MakeConvertToCommon(conversion, Symbols.ConvertToInt);
                    case TypeCode.Double:
                        return MakeConvertToCommon(conversion, Symbols.ConvertToFloat);
                    case TypeCode.String:
                        return MakeConvertToCommon(conversion, Symbols.String);
                    case TypeCode.Object:
                        if (type == typeof(BigInteger)) {
                            return MakeConvertToCommon(conversion, Symbols.ConvertToLong);
                        } else if (type == typeof(Complex64)) {
                            return MakeConvertToCommon(conversion, Symbols.ConvertToComplex);
                        } else if (type == typeof(IEnumerable)) {
                            return MakeConvertToIEnumerable(conversion);
                        } else if (type == typeof(IEnumerator)) {
                            return MakeConvertToIEnumerator(conversion);
                        } else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                            return MakeConvertToIEnumerable(conversion, type.GetGenericArguments()[0]);
                        } else if (conversion.Type.IsSubclassOf(typeof(Delegate))) {
                            return MakeDelegateTarget(conversion, conversion.Type, Restrict(typeof(OldInstance)));
                        }

                        break;
                }
            }

            return base.BindConvert(conversion);
        }

        public override MetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ invoke, params MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(invoke, BinderState.GetCodeContext(invoke), args);
        }

        #endregion

        #region Invoke Implementation

        private MetaObject/*!*/ InvokeWorker(MetaObjectBinder/*!*/ invoke, Expression/*!*/ codeContext, MetaObject/*!*/[] args) {
            MetaObject self = Restrict(typeof(OldInstance));

            Expression[] exprArgs = new Expression[args.Length + 1];
            for (int i = 0; i < args.Length; i++) {
                exprArgs[i + 1] = args[i].Expression;
            }

            ParameterExpression tmp = Ast.Variable(typeof(object), "callFunc");

            exprArgs[0] = tmp;
            return new MetaObject(
                // we could get better throughput w/ a more specific rule against our current custom old class but
                // this favors less code generation.

                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("OldInstanceTryGetBoundCustomMember"),
                            codeContext,
                            self.Expression,
                            AstUtils.Constant(Symbols.Call),
                            tmp
                        ),
                        Ast.Block(
                            Utils.Try(
                                Ast.Call(typeof(PythonOps).GetMethod("FunctionPushFrame")),
                                Ast.Assign(
                                    tmp,
                                    Ast.Dynamic(
                                        new PythonInvokeBinder(
                                            BinderState.GetBinderState(invoke),
                                            BindingHelpers.GetCallSignature(invoke)
                                        ),
                                        typeof(object),
                                        ArrayUtils.Insert(codeContext, exprArgs)
                                    )
                                )
                            ).Finally(
                                Ast.Call(typeof(PythonOps).GetMethod("FunctionPopFrame"))
                            ),
                            tmp
                        ),
                        BindingHelpers.InvokeFallback(invoke, codeContext, this, args).Expression
                    )
                ),
                self.Restrictions.Merge(Restrictions.Combine(args))
            );
        }        

        #endregion

        #region Conversions
       
        private MetaObject/*!*/ MakeConvertToIEnumerable(ConvertBinder/*!*/ conversion) {
            ParameterExpression tmp = Ast.Variable(typeof(IEnumerable), "res");
            MetaObject self = Restrict(typeof(OldInstance));

            return new MetaObject(
                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        Ast.NotEqual(
                            Ast.Assign(
                                tmp,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldInstanceConvertToIEnumerableNonThrowing"),
                                    Ast.Constant(BinderState.GetBinderState(conversion).Context),
                                    self.Expression
                                )
                            ),
                            Ast.Constant(null)
                        ),
                        tmp,
                        AstUtils.Convert(
                            AstUtils.Convert(  // first to object (incase it's a throw), then to IEnumerable
                                conversion.FallbackConvert(this).Expression,
                                typeof(object)
                            ),
                            typeof(IEnumerable)
                        )
                    )
                ),
                self.Restrictions
            );
        }

        private MetaObject/*!*/ MakeConvertToIEnumerator(ConvertBinder/*!*/ conversion) {
            ParameterExpression tmp = Ast.Variable(typeof(IEnumerator), "res");
            MetaObject self = Restrict(typeof(OldInstance));

            return new MetaObject(
                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        Ast.NotEqual(
                            Ast.Assign(
                                tmp,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldInstanceConvertToIEnumeratorNonThrowing"),
                                    Ast.Constant(BinderState.GetBinderState(conversion).Context),
                                    self.Expression
                                )
                            ),
                            Ast.Constant(null)
                        ),
                        tmp,
                        AstUtils.Convert(
                            AstUtils.Convert(
                                conversion.FallbackConvert(this).Expression,
                                typeof(object)
                            ),
                            typeof(IEnumerator)
                        )
                    )
                ),
                self.Restrictions
            );            
        }

        private MetaObject/*!*/ MakeConvertToIEnumerable(ConvertBinder/*!*/ conversion, Type genericType) {
            ParameterExpression tmp = Ast.Variable(typeof(IEnumerable), "res");
            MetaObject self = Restrict(typeof(OldInstance));

            return new MetaObject(
                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        Ast.NotEqual(
                            Ast.Assign(
                                tmp,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldInstanceConvertToIEnumerableOfTNonThrowing").MakeGenericMethod(genericType),
                                    Ast.Constant(BinderState.GetBinderState(conversion).Context),
                                    Expression
                                )
                            ),
                            Ast.Constant(null)
                        ),
                        tmp,
                        AstUtils.Convert(
                            AstUtils.Convert(
                                conversion.FallbackConvert(this).Expression,
                                typeof(object)
                            ),
                            typeof(IEnumerable)
                        )
                    )
                ),
                self.Restrictions
            );                       
        }

        private MetaObject/*!*/ MakeConvertToCommon(ConvertBinder/*!*/ conversion, SymbolId symbolId) {
            ParameterExpression tmp = Ast.Variable(typeof(object), "convertResult");
            MetaObject self = Restrict(typeof(OldInstance));
            return new MetaObject(
                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        MakeOneConvert(conversion, self, symbolId, tmp),
                        tmp,
                        AstUtils.Convert(
                            conversion.FallbackConvert(this).Expression,
                            typeof(object)
                        )
                    )
                ),
                self.Restrictions
            );
        }

        private static BinaryExpression/*!*/ MakeOneConvert(ConvertBinder/*!*/ conversion, MetaObject/*!*/ self, SymbolId symbolId, ParameterExpression/*!*/ tmp) {
            return Ast.NotEqual(
                Ast.Assign(
                    tmp,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceConvertNonThrowing"),
                        Ast.Constant(BinderState.GetBinderState(conversion).Context),
                        self.Expression,
                        AstUtils.Constant(symbolId)
                    )
                ),
                Ast.Constant(null)
            );
        }
        
        private MetaObject/*!*/ MakeConvertToBool(ConvertBinder/*!*/ conversion) {
            MetaObject self = Restrict(typeof(OldInstance));

            ParameterExpression tmp = Ast.Variable(typeof(bool?), "tmp");
            MetaObject fallback = conversion.FallbackConvert(this);
            Type resType = BindingHelpers.GetCompatibleType(typeof(bool), fallback.Expression.Type);

            return new MetaObject(
                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        Ast.NotEqual(
                            Ast.Assign(
                                tmp,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldInstanceConvertToBoolNonThrowing"),
                                    Ast.Constant(BinderState.GetBinderState(conversion).Context),
                                    self.Expression
                                )
                            ),
                            Ast.Constant(null)
                        ),
                        AstUtils.Convert(tmp, resType),
                        AstUtils.Convert(fallback.Expression, resType)
                    )
                ),
                self.Restrictions
            );
        }

        #endregion

        #region Member Access

        private MetaObject/*!*/ MakeMemberAccess(MetaObjectBinder/*!*/ member, string name, MemberAccess access, params MetaObject/*!*/[]/*!*/ args) {
            MetaObject self = Restrict(typeof(OldInstance));

            CustomOldClassDictionaryStorage dict;
            int key = GetCustomStorageSlot(name, out dict);
            if (key == -1) {
                return MakeDynamicMemberAccess(member, name, access, args);
            }

            ParameterExpression tmp = Ast.Variable(typeof(object), "dict");
            Expression target;

            ValidationInfo test = new ValidationInfo(
                Ast.NotEqual(
                    Ast.Assign(
                        tmp, 
                        Ast.Call(
                            typeof(PythonOps).GetMethod("OldInstanceGetOptimizedDictionary"),
                            self.Expression,
                            Ast.Constant(dict.KeyVersion)
                        )
                    ), 
                    Ast.Constant(null)
                ),
                null
            );

            switch (access) {
                case MemberAccess.Get:
                    // BUG: There's a missing Fallback path here that's always been present even
                    // in the version that used rules.
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceDictionaryGetValueHelper"),
                        tmp,
                        Ast.Constant(key),
                        AstUtils.Convert(Expression, typeof(object))
                    );
                    break;
                case MemberAccess.Set:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceDictionarySetExtraValue"),
                        tmp,
                        Ast.Constant(key),
                        AstUtils.Convert(args[1].Expression, typeof(object))
                    );
                    break;
                case MemberAccess.Delete:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceDeleteCustomMember"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        AstUtils.Convert(Expression, typeof(OldInstance)),
                        AstUtils.Constant(SymbolTable.StringToId(name))
                    );
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return BindingHelpers.AddDynamicTestAndDefer(
                member,
                new MetaObject(
                    target,
                    Restrictions.Combine(args).Merge(self.Restrictions)
                ),
                args,
                test,
                tmp
            );                            
        }

        private int GetCustomStorageSlot(string name, out CustomOldClassDictionaryStorage dict) {
            dict = Value.Dictionary._storage as CustomOldClassDictionaryStorage;
            if (dict == null || Value._class.HasSetAttr) {
                return -1;
            }

            return dict.FindKey(name);
        }

        private enum MemberAccess {
            Get,
            Set,
            Delete
        }
        
        private MetaObject/*!*/ MakeDynamicMemberAccess(MetaObjectBinder/*!*/ member, string/*!*/ name, MemberAccess access, MetaObject/*!*/[]/*!*/ args) {
            MetaObject self = Restrict(typeof(OldInstance));
            Expression target;
            SymbolId symName = SymbolTable.StringToId(name);

            switch (access) {
                case MemberAccess.Get:                    
                    ParameterExpression tmp = Ast.Variable(typeof(object), "result");

                    target = Ast.Block(
                        new ParameterExpression[] { tmp },
                        Ast.Condition(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("OldInstanceTryGetBoundCustomMember"),
                                Ast.Constant(BinderState.GetBinderState(member).Context),
                                self.Expression,
                                AstUtils.Constant(symName),
                                tmp
                            ),
                            tmp,
                            AstUtils.Convert(
                                FallbackGet(member, args),
                                typeof(object)
                            )
                        )
                    );                    
                    break;
                case MemberAccess.Set:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceSetCustomMember"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        self.Expression,
                        AstUtils.Constant(symName),
                        AstUtils.Convert(args[1].Expression, typeof(object))
                    );
                    break;
                case MemberAccess.Delete:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceDeleteCustomMember"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        self.Expression,
                        AstUtils.Constant(symName)
                    );
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return new MetaObject(
                target,
                self.Restrictions.Merge(Restrictions.Combine(args))
            );
        }

        private Expression FallbackGet(MetaObjectBinder member, MetaObject[] args) {
            GetMemberBinder sa = member as GetMemberBinder;
            if (sa != null) {
                return sa.FallbackGetMember(args[0]).Expression;
            }

            return ((PythonGetMemberBinder)member).Fallback(args[0], Ast.Constant(BinderState.GetBinderState(member).Context)).Expression;
        }

        #endregion

        #region Operations
        
        private MetaObject/*!*/ MakeIsCallable(OperationBinder/*!*/ operation) {
            MetaObject self = Restrict(typeof(OldInstance));

            return new MetaObject(
                Ast.Call(
                    typeof(PythonOps).GetMethod("OldInstanceIsCallable"),
                    Ast.Constant(BinderState.GetBinderState(operation).Context),
                    self.Expression
                ),
                self.Restrictions
            );
        }


        #endregion

        #region Helpers

        public new OldInstance/*!*/ Value {
            get {
                return (OldInstance)base.Value;
            }
        }

        #endregion
    }
}
