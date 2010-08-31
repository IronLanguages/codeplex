/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Runtime.CompilerServices;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace TestAst.Runtime {
    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using RuntimeHelpers = Microsoft.Scripting.Runtime.ScriptingRuntimeHelpers;
    
    class ConversionBinder : ConvertBinder  {
        private readonly ConversionResultKind/*!*/ _kind;

        public ConversionBinder(Type type, ConversionResultKind resultKind)
            : base(type, resultKind == ConversionResultKind.ExplicitCast || resultKind == ConversionResultKind.ExplicitTry) {
            _kind = resultKind;
        }

        public ConversionResultKind ResultKind {
            get {
                return _kind;
            }
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject self, DynamicMetaObject onBindingError) {
            if (self.NeedsDeferral()) {
                return Defer(self);
            }

            Type type = Type;

            DynamicMetaObject res = null;
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    res = MakeToBoolConversion(self);
                    break;
                case TypeCode.Char:
                    res = TryToCharConversion(self);
                    break;
            }

            var binder = (DefaultActionBinder)TestContext._TestContext.Binder;
            return res ?? binder.ConvertTo(Type, ResultKind, self, null);
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _kind.GetHashCode();
        }

        public override bool Equals(object obj) {
            ConversionBinder ob = obj as ConversionBinder;
            if (ob == null) {
                return false;
            }

            return _kind == ob._kind && base.Equals(obj);
        }

        #region Conversion Logic

        private DynamicMetaObject TryToGenericInterfaceConversion(DynamicMetaObject/*!*/ self, Type/*!*/ toType, Type/*!*/ fromType, Type/*!*/ wrapperType) {
            if (fromType.IsAssignableFrom(CompilerHelpers.GetType(self.Value))) {
                Type making = wrapperType.MakeGenericType(toType.GetGenericArguments());

                self = self.Restrict(CompilerHelpers.GetType(self.Value));

                return new DynamicMetaObject(
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

        private DynamicMetaObject TryToCharConversion(DynamicMetaObject/*!*/ self) {
            DynamicMetaObject res;
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
                    res = new DynamicMetaObject(
                        Ast.Call(
                            AstUtils.Convert(strExpr, typeof(string)),
                            typeof(string).GetMethod("get_Chars"),
                            Ast.Constant(0)
                        ),
                        self.Restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Ast.Equal(getLen, Ast.Constant(1))))
                    );
                } else {
                    res = new DynamicMetaObject(
                        Ast.Throw(
                            Ast.New(
                                typeof(ArgumentTypeException).GetConstructor(new [] { typeof(string)}),
                                Ast.Call(
                                    typeof(string).GetMethod("Format", new []{ typeof(string), typeof(object[])}),
                                    Ast.Constant("expected string of length 1 when converting to char, got '{0}'"),
                                    Ast.NewArrayInit(typeof(object), self.Expression)
                                )
                            )
                        ),
                        self.Restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Ast.NotEqual(getLen, Ast.Constant(1))))
                    );
                }
            } else {
                // let the base class produce the rule
                res = null;
            }

            return res;
        }

        private DynamicMetaObject/*!*/ MakeToBoolConversion(DynamicMetaObject/*!*/ self) {
            DynamicMetaObject res = null;
            if (self.NeedsDeferral()) {
                res = Defer(self);
            } else {
                if (self.HasValue) {
                    self = self.Restrict(self.RuntimeType);
                } 

                if (self.GetLimitType() == typeof(DynamicNull)) {
                    // None has no __nonzero__ and no __len__ but it's always false
                    res = MakeNoneToBoolConversion(self);
                } else if (self.GetLimitType() == typeof(bool)) {
                    // nothing special to convert from bool to bool
                    res = self;
                } else if (typeof(IStrongBox).IsAssignableFrom(self.GetLimitType())) {
                    // Explictly block conversion of References to bool
                    res = MakeStrongBoxToBoolConversionError(self);
                } else if (self.GetLimitType().IsPrimitive || self.GetLimitType().IsEnum) {
                    // optimization - rather than doing a method call for primitives and enums generate
                    // the comparison to zero directly.
                    res = MakePrimitiveToBoolComparison(self);
                } else {
                    return new DynamicMetaObject(
                        Ast.Constant(true),
                        self.Restrictions
                    );
                }
            }

            return res;
        }

        private static DynamicMetaObject/*!*/ MakeNoneToBoolConversion(DynamicMetaObject/*!*/ self) {
            // null is never true
            return new DynamicMetaObject(
                Ast.Constant(false),
                self.Restrictions
            );
        }

        private static DynamicMetaObject/*!*/ MakePrimitiveToBoolComparison(DynamicMetaObject/*!*/ self) {
            object zeroVal = Activator.CreateInstance(self.GetLimitType());

            return new DynamicMetaObject(
                Ast.NotEqual(
                    Ast.Constant(zeroVal),
                    self.Expression
                ),
                self.Restrictions
            );
        }

        private static DynamicMetaObject/*!*/ MakeStrongBoxToBoolConversionError(DynamicMetaObject/*!*/ self) {
            return new DynamicMetaObject(
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
            return String.Format("Test Convert {0} {1}", Type, ResultKind);
        }
    }
}
