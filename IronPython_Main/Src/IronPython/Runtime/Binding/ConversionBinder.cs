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
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Binders;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using Ast = Microsoft.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {
    
    class ConversionBinder : ConvertBinder, IPythonSite, IExpressionSerializable  {
        private readonly BinderState/*!*/ _state;
        private readonly ConversionResultKind/*!*/ _kind;

        public ConversionBinder(BinderState/*!*/ state, Type/*!*/ type, ConversionResultKind resultKind)
            : base(type, resultKind == ConversionResultKind.ExplicitCast || resultKind == ConversionResultKind.ExplicitTry) {
            Assert.NotNull(state, type);

            _state = state;
            _kind = resultKind;
        }

        public ConversionResultKind ResultKind {
            get {
                return _kind;
            }
        }

        public override MetaObject FallbackConvert(MetaObject self, MetaObject onBindingError) {
            if (self.NeedsDeferral()) {
                return Defer(self);
            }

            Type type = Type;

            MetaObject res = null;
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    res = MakeToBoolConversion(self);
                    break;
                case TypeCode.Char:
                    res = TryToCharConversion(self);
                    break;
                case TypeCode.Object:
                    // !!! Deferral?
                    if (type.IsArray && self.Value is PythonTuple && type.GetArrayRank() == 1) {
                        res = MakeToArrayConversion(self, type);
                    } else if (type.IsGenericType && !type.IsAssignableFrom(CompilerHelpers.GetType(self.Value))) {
                        Type genTo = type.GetGenericTypeDefinition();

                        // Interface conversion helpers...
                        if (genTo == typeof(IList<>)) {
                            res = TryToGenericInterfaceConversion(self, type, typeof(IList<object>), typeof(ListGenericWrapper<>));
                        } else if (genTo == typeof(IDictionary<,>)) {
                            res = TryToGenericInterfaceConversion(self, type, typeof(IDictionary<object, object>), typeof(DictionaryGenericWrapper<,>));
                        } else if (genTo == typeof(IEnumerable<>)) {
                            res = TryToGenericInterfaceConversion(self, type, typeof(IEnumerable), typeof(IEnumerableOfTWrapper<>));
                        }
                    } else if (type == typeof(IEnumerable)) {
                        if (self.LimitType == typeof(string)) {
                            // replace strings normal enumeration with our own which returns strings instead of chars.
                            res = new MetaObject(
                                Ast.Call(
                                    typeof(StringOps).GetMethod("ConvertToIEnumerable"),
                                    AstUtils.Convert(self.Expression, typeof(string))
                                ),
                                Restrictions.GetTypeRestriction(self.Expression, typeof(string))
                            );
                        } else if (!typeof(IEnumerable).IsAssignableFrom(self.LimitType) && IsIndexless(self)) {
                            res = PythonProtocol.ConvertToIEnumerable(this, self.Restrict(self.LimitType));
                        }
                    } else if (type == typeof(IEnumerator) ) {
                        if (!typeof(IEnumerator).IsAssignableFrom(self.LimitType) && 
                            !typeof(IEnumerable).IsAssignableFrom(self.LimitType) &&
                            IsIndexless(self)) {
                            res = PythonProtocol.ConvertToIEnumerator(this, self.Restrict(self.LimitType));
                        }
                    }
                    break;
            }

            if (type.IsEnum && Enum.GetUnderlyingType(type) == self.LimitType) {
                // numeric type to enum, this is ok if the value is zero
                object value = Activator.CreateInstance(type);

                return new MetaObject(
                    Ast.Condition(
                        Ast.Equal(
                            AstUtils.Convert(self.Expression, Enum.GetUnderlyingType(type)),
                            Ast.Constant(Activator.CreateInstance(self.LimitType))
                        ),
                        Ast.Constant(value),
                        Ast.Call(
                            typeof(PythonOps).GetMethod("TypeErrorForBadEnumConversion").MakeGenericMethod(type),
                            AstUtils.Convert(self.Expression, typeof(object))
                        )
                    ),
                    self.Restrictions.Merge(Restrictions.GetTypeRestriction(self.Expression, self.LimitType)),
                    value
                );
            }

            return res ?? Binder.Binder.ConvertTo(Type, ResultKind, self);
        }

        private static bool IsIndexless(MetaObject/*!*/ arg) {
            return arg.LimitType != typeof(OldInstance) &&
                arg.LimitType != typeof(BuiltinFunction) &&
                arg.LimitType != typeof(BuiltinMethodDescriptor);
        }

        public override object CacheIdentity {
            get { return this; }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode() ^ _kind.GetHashCode();
        }

        public override bool Equals(object obj) {
            ConversionBinder ob = obj as ConversionBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && _kind == ob._kind && base.Equals(obj);
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
                        AstUtils.Convert(
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
                            AstUtils.Convert(
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
                    AstUtils.Convert(
                        strExpr,
                        typeof(string)
                    ),
                    typeof(string).GetProperty("Length")
                );

                if (strVal.Length == 1) {
                    res = new MetaObject(
                        Ast.Call(
                            AstUtils.Convert(strExpr, typeof(string)),
                            typeof(string).GetMethod("get_Chars"),
                            Ast.Constant(0)
                        ),
                        self.Restrictions.Merge(Restrictions.GetExpressionRestriction(Ast.Equal(getLen, Ast.Constant(1))))
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
                        self.Restrictions.Merge(Restrictions.GetExpressionRestriction(Ast.NotEqual(getLen, Ast.Constant(1))))
                    );
                }
            } else {
                // let the base class produce the rule
                res = null;
            }

            return res;
        }

        private MetaObject/*!*/ MakeToBoolConversion(MetaObject/*!*/ self) {
            MetaObject res = null;
            if (self.NeedsDeferral()) {
                res = Defer(self);
            } else {
                if (self.HasValue) {
                    self = self.Restrict(self.RuntimeType);
                } 

                if (self.LimitType == typeof(Null)) {
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
                            Ast.Constant(true),
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
                        typeof(ScriptingRuntimeHelpers).GetMethod("SimpleTypeError"),
                        Ast.Constant("Can't convert a Reference<> instance to a bool")
                    )
                ),
                self.Restrictions
            );
        }

        #endregion

        public override string ToString() {
            return String.Format("Python Convert {0} {1}", Type, ResultKind);
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeConversionAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Ast.Constant(Type),
                Ast.Constant(ResultKind)
            );
        }

        #endregion
    }
}