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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;

using Microsoft.Scripting.Actions;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;
    using RuntimeHelpers = System.Scripting.Runtime.RuntimeHelpers;
    using System.Diagnostics;

    class ConversionBinder : ConvertAction, IPythonSite, IExpressionSerializable  {
        private readonly BinderState/*!*/ _state;
        private readonly ConversionResultKind/*!*/ _kind;

        public ConversionBinder(BinderState/*!*/ state, Type/*!*/ type, ConversionResultKind resultKind)
            : base(type, resultKind == ConversionResultKind.ExplicitCast || resultKind == ConversionResultKind.ExplicitTry) {
            Debug.Assert(false);
            _state = state;
            _kind = resultKind;
        }

        public ConversionResultKind ResultKind {
            get {
                return _kind;
            }
        }

        public override MetaObject/*!*/ Fallback(MetaObject/*!*/[]/*!*/ args) {
            MetaObject arg = args[0];
            Type type = ToType;

            MetaObject res = null;
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    res = MakeToBoolConversion(args);
                    break;
                case TypeCode.Char:
                    res = TryToCharConversion(arg);
                    break;
                case TypeCode.Object:
                    // !!! Deferral?
                    if (type.IsArray && arg.Value is PythonTuple && type.GetArrayRank() == 1) {
                        res = MakeToArrayConversion(arg, type);
                    } else if (type.IsGenericType && !type.IsAssignableFrom(CompilerHelpers.GetType(arg.Value))) {
                        Type genTo = type.GetGenericTypeDefinition();

                        // Interface conversion helpers...
                        if (genTo == typeof(IList<>)) {
                            res = TryToGenericInterfaceConversion(arg, type, typeof(IList<object>), typeof(ListGenericWrapper<>));
                        } else if (genTo == typeof(IDictionary<,>)) {
                            res = TryToGenericInterfaceConversion(arg, type, typeof(IDictionary<object, object>), typeof(DictionaryGenericWrapper<,>));
                        } else if (genTo == typeof(IEnumerable<>)) {
                            res = TryToGenericInterfaceConversion(arg, type, typeof(IEnumerable), typeof(IEnumerableOfTWrapper<>));
                        }
                    } else if (type == typeof(IEnumerable)) {
                        if (!typeof(IEnumerable).IsAssignableFrom(arg.LimitType) && IsIndexless(arg)) {
                            res = PythonProtocol.ConvertToIEnumerable(this, args[0].Restrict(args[0].LimitType));
                        }
                    } else if (type == typeof(IEnumerator) ) {
                        if (!typeof(IEnumerator).IsAssignableFrom(arg.LimitType) &&
                            !typeof(IEnumerable).IsAssignableFrom(arg.LimitType) && 
                            IsIndexless(arg)) {
                            res = PythonProtocol.ConvertToIEnumerator(this, args[0].Restrict(args[0].LimitType));
                        }
                    }
                    break;
            }

            return res ?? Binder.Binder.ConvertTo(ToType, ResultKind, arg);
        }

        private static bool IsIndexless(MetaObject/*!*/ arg) {
            return arg.LimitType != typeof(OldInstance) &&
                arg.LimitType != typeof(BuiltinFunction) &&
                arg.LimitType != typeof(BoundBuiltinFunction) &&
                arg.LimitType != typeof(BuiltinMethodDescriptor);
        }

        public override object HashCookie {
            get { return this; }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            ConversionBinder ob = obj as ConversionBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj);
        }

        public BinderState/*!*/ Binder {
            get {
                return _state;
            }
        }

        #region Conversion Logic

        private MetaObject TryToGenericInterfaceConversion(MetaObject/*!*/ self, Type/*!*/ toType, Type/*!*/ fromType, Type/*!*/ wrapperType) {
            if (fromType.IsAssignableFrom(CompilerHelpers.GetType(self.Value))) {
                Type making = wrapperType.MakeGenericType(toType.GetGenericArguments());

                self = self.Restrict(CompilerHelpers.GetType(self.Value));

                return new MetaObject(
                    Ast.New(
                        making.GetConstructor(new Type[] { fromType }),
                        Ast.ConvertHelper(
                            self.Expression,
                            fromType
                        )
                    ),
                    self.Restrictions
                );
            }
            return null;
        }

        private MetaObject/*!*/ MakeToArrayConversion(MetaObject/*!*/ self, Type/*!*/ toType) {
            self = self.Restrict(typeof(PythonTuple));

            return new MetaObject(
                Ast.Call(
                    typeof(PythonOps).GetMethod("ConvertTupleToArray").MakeGenericMethod(toType.GetElementType()),
                    self.Expression
                ),
                self.Restrictions
            );
        }

        private MetaObject TryToCharConversion(MetaObject/*!*/ self) {
            MetaObject res;
            // we have an implicit conversion to char if the
            // string length == 1, but we can only represent
            // this is implicit via a rule.
            string strVal = self.Value as string;
            Expression strExpr = self.Expression;
            if (strVal == null) {
                Extensible<string> extstr = self.Value as Extensible<string>;
                if (extstr != null) {
                    strVal = extstr.Value;
                    strExpr =
                        Ast.Property(
                            Ast.ConvertHelper(
                                strExpr,
                                typeof(Extensible<string>)
                            ),
                            typeof(Extensible<string>).GetProperty("Value")
                        );
                }
            }

            // we can only produce a conversion if we have a string value...
            if (strVal != null) {
                self = self.Restrict(self.RuntimeType);

                Expression getLen = Ast.Property(
                    Ast.ConvertHelper(
                        strExpr,
                        typeof(string)
                    ),
                    typeof(string).GetProperty("Length")
                );

                if (strVal.Length == 1) {
                    res = new MetaObject(
                        Ast.Call(
                            Ast.ConvertHelper(strExpr, typeof(string)),
                            typeof(string).GetMethod("get_Chars"),
                            Ast.Constant(0)
                        ),
                        self.Restrictions.Merge(Restrictions.ExpressionRestriction(Ast.Equal(getLen, Ast.Constant(1))))
                    );
                } else {
                    res = new MetaObject(
                        Ast.Throw(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("TypeError"),
                                Ast.Constant("expected string of length 1 when converting to char, got '{0}'"),
                                Ast.NewArrayInit(typeof(object), self.Expression)
                            )
                        ),
                        self.Restrictions.Merge(Restrictions.ExpressionRestriction(Ast.NotEqual(getLen, Ast.Constant(1))))
                    );
                }
            } else {
                // let the base class produce the rule
                res = null;
            }

            return res;
        }

        private MetaObject/*!*/ MakeToBoolConversion(MetaObject/*!*/[]/*!*/ args) {
            MetaObject self = args[0];

            MetaObject res = null;
            if (self.NeedsDeferral) {
                res = Defer(args);
            } else {
                if (self.HasValue) {
                    self = self.Restrict(self.RuntimeType);
                } 

                if (self.LimitType == typeof(None)) {
                    // None has no __nonzero__ and no __len__ but it's always false
                    res = MakeNoneToBoolConversion(self);
                } else if (self.LimitType == typeof(bool)) {
                    // nothing special to convert from bool to bool
                    res = self;
                } else if (typeof(IStrongBox).IsAssignableFrom(self.LimitType)) {
                    // Explictly block conversion of References to bool
                    res = MakeStrongBoxToBoolConversionError(self);
                } else if (self.LimitType.IsPrimitive || self.LimitType.IsEnum) {
                    // optimization - rather than doing a method call for primitives and enums generate
                    // the comparison to zero directly.
                    res = MakePrimitiveToBoolComparison(self);
                } else {
                    // anything non-null that doesn't fall under one of the above rules is true.  So we
                    // fallback to the base Python conversion which will check for __nonzero__ and
                    // __len__.  The fallback is handled by our ConvertTo site binder.
                    return
                        PythonProtocol.ConvertToBool(this, self) ??
                        new MetaObject(
                            Ast.True(),
                            self.Restrictions
                        );
                }
            }

            return res;
        }

        private static MetaObject/*!*/ MakeNoneToBoolConversion(MetaObject/*!*/ self) {
            // null is never true
            return new MetaObject(
                Ast.Constant(false),
                self.Restrictions
            );
        }

        private static MetaObject/*!*/ MakePrimitiveToBoolComparison(MetaObject/*!*/ self) {
            object zeroVal = Activator.CreateInstance(self.LimitType);

            return new MetaObject(
                Ast.NotEqual(
                    Ast.Constant(zeroVal),
                    self.Expression
                ),
                self.Restrictions
            );
        }

        private static MetaObject/*!*/ MakeStrongBoxToBoolConversionError(MetaObject/*!*/ self) {
            return new MetaObject(
                Ast.Throw(
                    Ast.Call(
                        typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                        Ast.Constant("Can't convert a Reference<> instance to a bool")
                    )
                ),
                self.Restrictions
            );
        }

        #endregion

        public override string ToString() {
            return String.Format("Python Convert {0} {1}", ToType, ResultKind);
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeConversionAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Ast.Constant(ToType),
                Ast.Constant(ResultKind)
            );
        }

        #endregion
    }
}
