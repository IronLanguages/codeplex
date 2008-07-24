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
using System.Collections;
using System.Collections.Generic;
using System.Scripting;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Runtime;
using System.Scripting.Utils;

using Microsoft.Scripting.Math;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// Provides a MetaObject for instances of Python's old-style classes.
    /// 
    /// TODO: Lots of CodeConetxt references, need to move CodeContext onto OldClass and pull it from there.
    /// </summary>
    class MetaOldInstance : MetaPythonObject, IPythonInvokable {        
        public MetaOldInstance(Expression/*!*/ expression, Restrictions/*!*/ restrictions, OldInstance/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(InvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, codeContext, args);
        }

        #endregion

        #region MetaObject Overrides

        public override MetaObject/*!*/ Call(CallAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, args);
        }

        public override MetaObject/*!*/ GetMember(GetMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            return MakeMemberAccess(member, member.Name, MemberAccess.Get, args);
        }

        public override MetaObject/*!*/ SetMember(SetMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            return MakeMemberAccess(member, member.Name, MemberAccess.Set, args);
        }

        public override MetaObject/*!*/ DeleteMember(DeleteMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            return MakeMemberAccess(member, member.Name, MemberAccess.Delete, args);
        }

        public override MetaObject/*!*/ Operation(OperationAction/*!*/ operation, params MetaObject/*!*/[]/*!*/ args) {
            if (operation.Operation == StandardOperators.IsCallable) {
                return MakeIsCallable(operation);
            }

            return base.Operation(operation, args);
        }

        public override MetaObject/*!*/ Convert(ConvertAction/*!*/ conversion, MetaObject/*!*/[]/*!*/ args) {
            Type type = conversion.ToType;

            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Boolean:
                        return MakeConvertToBool(conversion, args);
                    case TypeCode.Int32:
                        return MakeConvertToCommon(conversion, Symbols.ConvertToInt, args);
                    case TypeCode.Double:
                        return MakeConvertToCommon(conversion, Symbols.ConvertToFloat, args);
                    case TypeCode.String:
                        return MakeConvertToCommon(conversion, Symbols.String, args);
                    case TypeCode.Object:
                        if (type == typeof(BigInteger)) {
                            return MakeConvertToCommon(conversion, Symbols.ConvertToLong, args);
                        } else if (type == typeof(Complex64)) {
                            return MakeConvertToCommon(conversion, Symbols.ConvertToComplex, args);
                        } else if (type == typeof(IEnumerable)) {
                            return MakeConvertToIEnumerable(conversion, args);
                        } else if (type == typeof(IEnumerator)) {
                            return MakeConvertToIEnumerator(conversion, args);
                        } else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                            return MakeConvertToIEnumerable(conversion, type.GetGenericArguments()[0], args);
                        } else if (conversion.ToType.IsSubclassOf(typeof(Delegate))) {
                            return MakeDelegateTarget(conversion, conversion.ToType, Restrict(typeof(OldInstance)));
                        }

                        break;
                }
            }

            return base.Convert(conversion, args);
        }

        public override MetaObject/*!*/ Invoke(InvokeAction/*!*/ invoke, params MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(invoke, BinderState.GetCodeContext(invoke), args);
        }

        #endregion

        #region Invoke Implementation

        private MetaObject/*!*/ InvokeWorker(MetaAction/*!*/ invoke, Expression/*!*/ codeContext, MetaObject/*!*/[] args) {
            MetaObject self = Restrict(typeof(OldInstance));
            args = ArrayUtils.RemoveFirst(args);

            Expression[] exprArgs = new Expression[args.Length + 1];
            for (int i = 0; i < args.Length; i++) {
                exprArgs[i + 1] = args[i].Expression;
            }

            VariableExpression tmp = Ast.Variable(typeof(object), "callFunc");

            exprArgs[0] = tmp;
            return new MetaObject(
                // we could get better throughput w/ a more specific rule against our current custom old class but
                // this favors less code generation.

                Ast.Scope(
                    Ast.Condition(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("OldInstanceTryGetBoundCustomMember"),
                            codeContext,
                            self.Expression,
                            Ast.Constant(Symbols.Call),
                            tmp
                        ),
                        Ast.Comma(
                            Ast.Try(
                                Ast.Call(typeof(PythonOps).GetMethod("FunctionPushFrame")),
                                Ast.Assign(
                                    tmp,
                                    Ast.ActionExpression(
                                        new InvokeBinder(
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
                        BindingHelpers.InvokeFallback(invoke, codeContext, ArrayUtils.Insert((MetaObject)this, args)).Expression
                    ),
                    tmp
                ),
                self.Restrictions.Merge(Restrictions.Combine(args))
            );
        }        

        #endregion

        #region Conversions
       
        private MetaObject/*!*/ MakeConvertToIEnumerable(ConvertAction/*!*/ conversion, MetaObject/*!*/[]/*!*/ args) {
            VariableExpression tmp = Ast.Variable(typeof(IEnumerable), "res");
            MetaObject self = Restrict(typeof(OldInstance));

            return new MetaObject(
                Ast.Scope(
                    Ast.Condition(
                        Ast.NotEqual(
                            Ast.Assign(
                                tmp,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldInstanceConvertToIEnumerableNonThrowing"),
                                    Ast.Constant(BinderState.GetBinderState(conversion).Context),
                                    Expression
                                )
                            ),
                            Ast.Null()
                        ),
                        tmp,
                        Ast.ConvertHelper(
                            Ast.ConvertHelper(  // first to object (incase it's a throw), then to IEnumerable
                                conversion.Fallback(args).Expression,
                                typeof(object)
                            ),
                            typeof(IEnumerable)
                        )
                    ),
                    tmp
                ),
                self.Restrictions
            );
        }

        private MetaObject/*!*/ MakeConvertToIEnumerator(ConvertAction/*!*/ conversion, MetaObject/*!*/[]/*!*/ args) {
            VariableExpression tmp = Ast.Variable(typeof(IEnumerator), "res");
            MetaObject self = Restrict(typeof(OldInstance));

            return new MetaObject(
                Ast.Scope(
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
                            Ast.Null()
                        ),
                        tmp,
                        Ast.ConvertHelper(
                            Ast.ConvertHelper(
                                conversion.Fallback(args).Expression,
                                typeof(object)
                            ),
                            typeof(IEnumerator)
                        )
                    ),
                    tmp
                ),
                self.Restrictions
            );            
        }

        private MetaObject/*!*/ MakeConvertToIEnumerable(ConvertAction/*!*/ conversion, Type genericType, MetaObject/*!*/[]/*!*/ args) {
            VariableExpression tmp = Ast.Variable(typeof(IEnumerable), "res");
            MetaObject self = Restrict(typeof(OldInstance));

            return new MetaObject(
                Ast.Scope(
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
                            Ast.Null()
                        ),
                        tmp,
                        Ast.ConvertHelper(
                            Ast.ConvertHelper(
                                conversion.Fallback(args).Expression,
                                typeof(object)
                            ),
                            typeof(IEnumerable)
                        )
                    ),
                    tmp
                ),
                self.Restrictions
            );                       
        }

        private MetaObject/*!*/ MakeConvertToCommon(ConvertAction/*!*/ conversion, SymbolId symbolId, MetaObject/*!*/[]/*!*/ args) {
            VariableExpression tmp = Ast.Variable(typeof(object), "convertResult");
            MetaObject self = Restrict(typeof(OldInstance));
            return new MetaObject(
                Ast.Scope(
                    Ast.Condition(
                        MakeOneConvert(conversion, self, symbolId, tmp),
                        tmp,
                        Ast.ConvertHelper(
                            conversion.Fallback(args).Expression,
                            typeof(object)
                        )
                    ),
                    tmp
                ),
                self.Restrictions
            );
        }

        private static BinaryExpression/*!*/ MakeOneConvert(ConvertAction/*!*/ conversion, MetaObject/*!*/ self, SymbolId symbolId, VariableExpression/*!*/ tmp) {
            return Ast.NotEqual(
                Ast.Assign(
                    tmp,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceConvertNonThrowing"),
                        Ast.Constant(BinderState.GetBinderState(conversion).Context),
                        self.Expression,
                        Ast.Constant(symbolId)
                    )
                ),
                Ast.Null()
            );
        }
        
        private MetaObject/*!*/ MakeConvertToBool(ConvertAction/*!*/ conversion, MetaObject/*!*/[]/*!*/ args) {
            MetaObject self = Restrict(typeof(OldInstance));

            VariableExpression tmp = Ast.Variable(typeof(bool?), "tmp");
            MetaObject fallback = conversion.Fallback(args);
            Type resType = BindingHelpers.GetCompatibleType(typeof(bool), fallback.Expression.Type);

            return new MetaObject(
                Ast.Scope(
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
                            Ast.Null()
                        ),
                        Ast.ConvertHelper(tmp, resType),
                        Ast.ConvertHelper(fallback.Expression, resType)
                    ),
                    tmp
                ),
                self.Restrictions
            );
        }

        #endregion

        #region Member Access

        private MetaObject/*!*/ MakeMemberAccess(StandardAction/*!*/ member, string name, MemberAccess access, params MetaObject/*!*/[]/*!*/ args) {
            MetaObject self = Restrict(typeof(OldInstance));

            CustomOldClassDictionaryStorage dict;
            int key = GetCustomStorageSlot(name, out dict);
            if (key == -1) {
                return MakeDynamicMemberAccess(member, name, access, args);
            }

            VariableExpression tmp = Ast.Variable(typeof(object), "dict");
            Expression target;

            ValidationInfo test = new ValidationInfo(
                Ast.NotEqual(
                    Ast.Assign(
                        tmp, 
                        Ast.Call(
                            self.Expression,
                            typeof(OldInstance).GetMethod("GetOptimizedDictionary"),
                            Ast.Constant(dict.KeyVersion)
                        )
                    ), 
                    Ast.Null()
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
                        Ast.ConvertHelper(Expression, typeof(object))
                    );
                    break;
                case MemberAccess.Set:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceDictionarySetExtraValue"),
                        tmp,
                        Ast.Constant(key),
                        Ast.ConvertHelper(args[1].Expression, typeof(object))
                    );
                    break;
                case MemberAccess.Delete:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceDeleteCustomMember"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        Ast.ConvertHelper(Expression, typeof(OldInstance)),
                        Ast.Constant(SymbolTable.StringToId(name))
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
            if (dict == null || Value.__class__.HasSetAttr) {
                return -1;
            }

            return dict.FindKey(name);
        }

        private enum MemberAccess {
            Get,
            Set,
            Delete
        }
        
        private MetaObject/*!*/ MakeDynamicMemberAccess(StandardAction/*!*/ member, string/*!*/ name, MemberAccess access, MetaObject/*!*/[]/*!*/ args) {
            MetaObject self = Restrict(typeof(OldInstance));
            Expression target;
            SymbolId symName = SymbolTable.StringToId(name);

            switch (access) {
                case MemberAccess.Get:                    
                    VariableExpression tmp = Ast.Variable(typeof(object), "result");

                    target = Ast.Scope(
                        Ast.Condition(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("OldInstanceTryGetBoundCustomMember"),
                                Ast.Constant(BinderState.GetBinderState(member).Context),
                                self.Expression,
                                Ast.Constant(symName),
                                tmp
                            ),
                            tmp,
                            Ast.ConvertHelper(
                                member.Fallback(args).Expression,
                                typeof(object)
                            )
                        ),
                        tmp
                    );                    
                    break;
                case MemberAccess.Set:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceSetCustomMember"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        self.Expression,
                        Ast.Constant(symName),
                        Ast.ConvertHelper(args[1].Expression, typeof(object))
                    );
                    break;
                case MemberAccess.Delete:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceDeleteCustomMember"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        self.Expression,
                        Ast.Constant(symName)
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

        #endregion

        #region Operations
        
        private MetaObject/*!*/ MakeIsCallable(OperationAction/*!*/ operation) {
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
