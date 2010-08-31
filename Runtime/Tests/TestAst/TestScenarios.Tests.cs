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
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace TestAst {
    using AstUtils = Utils;
    using Expr = Expression;
    using EU = ETUtils.ExpressionUtils;

    public partial class TestScenarios {
        public delegate Expression Test(TestScenarios ts);
#if TODO
        // Helper method for operations
        OldDoOperationAction MakeOperation(Operators op) {
            return OldDoOperationAction.Make(Binder, op);
        }
#endif
        #region Test Scenarios.  Add new tests here.
        //@TODO - when/if you switch to generating methods for each of these it might make sense to have the test delegate
        //take a LambdaExpression so that we can call LambdaExpression.CreateVariable somewhat freely in these methods with less concern
        //about collisions.

        [TestAttribute("Creates a very basic AST which merely prints a line of text.  This is used to verify the surrounding TestAst infrastructure.")]
        private static Expression Scenario_Basic(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenPrint("Very simple code to ensure basic operation of our surrounding infrastructure works."));
            //For this basic scenario the MethodCallExpression generated above is more than enough
            return EU.BlockVoid(expressions);
        }

        internal static LambdaBuilder MakeLambdaBuilder(Type returnType, string name) {
            var builder = AstUtils.Lambda(returnType, name);
#if TODO // code context
            builder.AddCodeContext = true;
#endif
            return builder;
        }

        private class TypedConstants {
            private readonly Type _type;
            private readonly object _zero;
            private readonly object _one;
            private readonly object _two;
            private readonly object _notZero;
            private readonly object _notOne;
            private readonly object _notTwo;

            public Type Type {
                get { return _type; }
            }

            public object Zero {
                get { return _zero; }
            }
            public object One {
                get { return _one; }
            }
            public object Two {
                get { return _two; }
            }
            public object NotZero {
                get { return _notZero; }
            }
            public object NotOne {
                get { return _notOne; }
            }
            public object NotTwo {
                get { return _notTwo; }
            }

            public TypedConstants(Type type, object zero, object one, object two, object notZero, object notOne, object notTwo) {
                _type = type;
                _zero = zero;
                _one = one;
                _two = two;
                _notZero = notZero;
                _notOne = notOne;
                _notTwo = notTwo;

                Debug.Assert(type == zero.GetType());
                Debug.Assert(type == one.GetType());
                Debug.Assert(type == two.GetType());
                Debug.Assert(type == notZero.GetType());
                Debug.Assert(type == notOne.GetType());
                Debug.Assert(type == notTwo.GetType());
            }
        }

        private static Expression BoxIf(bool box, Expression expression) {
            if (box) {
                return AstUtils.Convert(expression, typeof(object));
            } else {
                return expression;
            }
        }

        private class ConvertTest {
            public Type Type;
            public Expression ConvertedValue;
            public Expression ResultingValue;
            public ConversionResultKind Kind;
            public bool ShouldThrow;
            public bool? JustCheck;
            private int _id;
            private static int _curId;

            /// <summary>
            /// Creates a conversion test that throws on failure to convert
            /// </summary>
            public static ConvertTest Throwing(Type convertType, Expression convertVal, Expression compareVal) {
                return new ConvertTest(convertType, convertVal, compareVal, ConversionResultKind.ExplicitCast, false, null);
            }

            /// <summary>
            /// Creates a conversion test that throws on failure to convert that is expected to throw
            /// </summary>
            public static ConvertTest ThrowErr(Type convertType, Expression convertVal) {
                return new ConvertTest(convertType, convertVal, null, ConversionResultKind.ExplicitCast, true, null);
            }
            /// <summary>
            /// Creates a conversion test that returns true/false indicating the success of the conversion
            /// </summary>
            public static ConvertTest Checking(Type convertType, Expression convertVal, bool success) {
                return new ConvertTest(convertType, convertVal, null, ConversionResultKind.ExplicitTry, false, success);

            }

            /// <summary>
            /// Creates a conversion test that returns the value or default(T) if the conversion fails
            /// </summary>
            public static ConvertTest Default(Type convertType, Expression convertVal, Expression compareVal) {
                return new ConvertTest(convertType, convertVal, compareVal, ConversionResultKind.ExplicitTry, false, null);
            }

            private ConvertTest(Type convertType, Expression convertVal, Expression compareVal, ConversionResultKind kind, bool shouldThrow, bool? justCheck) {
                Type = convertType;
                ConvertedValue = convertVal;
                ResultingValue = compareVal;
                Kind = kind;
                ShouldThrow = shouldThrow;
                JustCheck = justCheck;
                _id = ++_curId;
            }

            public override string ToString() {
                string value = ConvertedValue.ToString();
                if (ConvertedValue is ConstantExpression) {
                    if (((ConstantExpression)ConvertedValue).Value == null) {
                        value = "(null)";
                    } else {
                        value = ((ConstantExpression)ConvertedValue).Value.ToString();
                    }
                }
                return String.Format("{2}: {0} to {1} via {3} throwing {4}", value, Type.Name, _id, Kind, ShouldThrow);
            }
        }

#if TODO // obsolete: code context, old action, global variables

        [TestAttribute("Tests conversions and dynamic sites")]
        private static Expression Scenario_Conversions(TestScenarios ts) {
            ConvertTest[] tests = new ConvertTest[] {
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // simple positive test cases:
                // throwing on failure (1-16)
                ConvertTest.Throwing(typeof(int),     Expr.Constant(1),                                                Expr.Constant(1)),                 // int -> int
                ConvertTest.Throwing(typeof(int),     Expr.Convert(Expr.Constant(1), typeof(object)),                   Expr.Constant(1)),                 // boxed int -> int
                ConvertTest.Throwing(typeof(int),     MakeExtensibleIntExpression(1),                                 Expr.Constant(1)),                 // extensible int -> int
                ConvertTest.Throwing(typeof(int),     Expr.Convert(MakeExtensibleIntExpression(1), typeof(object)),    Expr.Constant(1)),                 // boxed extensible int -> int
                ConvertTest.Throwing(typeof(decimal), Expr.Constant(1),                                                MakeDecimalExpression(1)),        // used defined: int -> decimal
                ConvertTest.Throwing(typeof(decimal), Expr.Convert(Expr.Constant(1), typeof(object)),                   MakeDecimalExpression(1)),        // used defined: boxed int -> decimal
                ConvertTest.Throwing(typeof(int),     Expr.Constant((byte)1),                                          Expr.Constant(1)),                 // primitive: byte -> int
                ConvertTest.Throwing(typeof(int),     Expression.Convert(Expression.Constant((byte)1), typeof(object)),             Expression.Constant(1)),                 // primitive: boxed byte -> int
                ConvertTest.Throwing(typeof(int?),    Expr.Constant(null),                                             Expr.Constant(null)),              // null -> Nullable<T>
                ConvertTest.Throwing(typeof(int?),    Expr.Convert(Expr.Constant(null), typeof(object)),                Expr.Constant(null)),              // boxed null -> Nullable<T>
                ConvertTest.Throwing(typeof(int?),    Expression.Constant(1),                                          MakeNullableIntExpression(1)),    // T -> Nullable<T>
                ConvertTest.Throwing(typeof(int?),    Expression.Convert(Expression.Constant(1), typeof(object)),      MakeNullableIntExpression(1)),    // boxed T -> Nullable<T>
                ConvertTest.Throwing(typeof(object),  Expression.Constant(1),                                                Expr.Convert(Expression.Constant(1), typeof(object))),               // int -> object
                ConvertTest.Throwing(typeof(object),  Expression.Convert(Expression.Constant(1), typeof(object)),            Expr.Convert(Expression.Constant(1), typeof(object))),               // boxed int -> object
                ConvertTest.Throwing(typeof(object),  Expr.Constant("foo"),                                            Expr.Constant("foo")),             // str -> object
                ConvertTest.Throwing(typeof(object),  Expr.Convert(Expr.Constant("foo"), typeof(object)),               Expr.Constant("foo")),             // "boxed" str -> object
                ConvertTest.Throwing(typeof(Type),    Expr.Convert(Expr.Constant(typeof(int)), typeof(Type)),           Expr.Constant(typeof(int))),       // Type -> Type
                ConvertTest.Throwing(typeof(Type),    Expr.Convert(Expr.Constant(typeof(int)), typeof(object)),         Expr.Constant(typeof(int))),       // "boxed" RuntimeType -> Type

                // returning default on failure (16-32)
                ConvertTest.Default(typeof(int),      Expr.Constant(1),                                                Expr.Constant(1)),                 // int -> int
                ConvertTest.Default(typeof(int),      Expr.Convert(Expr.Constant(1), typeof(object)),                   Expr.Constant(1)),                 // boxed int -> int
                ConvertTest.Default(typeof(int),      MakeExtensibleIntExpression(1),                                 Expr.Constant(1)),                 // extensible int -> int
                ConvertTest.Default(typeof(int),      Expr.Convert(MakeExtensibleIntExpression(1), typeof(object)),    Expr.Constant(1)),                 // boxed extensible int -> int
                ConvertTest.Default(typeof(decimal),  Expr.Constant(1),                                                MakeDecimalExpression(1)),        // used defined: int -> decimal
                ConvertTest.Default(typeof(decimal),  Expr.Convert(Expr.Constant(1), typeof(object)),                   MakeDecimalExpression(1)),        // used defined: boxed int -> decimal
                ConvertTest.Default(typeof(int),      Expr.Constant((byte)1),                                          Expr.Constant(1)),                 // primitive: byte -> int
                ConvertTest.Default(typeof(int),      Expression.Convert(Expression.Constant((byte)1), typeof(object)),             Expression.Constant(1)),                 // primitive: boxed byte -> int
                ConvertTest.Default(typeof(int?),     Expr.Constant(null),                                             Expr.Constant(null)),              // null -> Nullable<T>
                ConvertTest.Default(typeof(int?),     Expr.Convert(Expr.Constant(null), typeof(object)),                Expr.Constant(null)),              // boxed null -> Nullable<T>
                ConvertTest.Default(typeof(int?),     Expression.Constant(1),                                          MakeNullableIntExpression(1)),    // T -> Nullable<T>
                ConvertTest.Default(typeof(int?),     Expression.Convert(Expression.Constant(1), typeof(object)),      MakeNullableIntExpression(1)),    // boxed T -> Nullable<T>
                ConvertTest.Default(typeof(object),   Expression.Constant(1),                                                Expr.Convert(Expression.Constant(1), typeof(object))),               // int -> object
                ConvertTest.Default(typeof(object),   Expression.Convert(Expression.Constant(1), typeof(object)),          Expr.Convert(Expression.Constant(1), typeof(object))),               // boxed int -> object
                ConvertTest.Default(typeof(object),   Expr.Constant("foo"),                                            Expr.Constant("foo")),             // str -> object
                ConvertTest.Default(typeof(object),   Expr.Convert(Expr.Constant("foo"), typeof(object)),               Expr.Constant("foo")),             // "boxed" str -> object
                ConvertTest.Default(typeof(Type),    Expr.Convert(Expr.Constant(typeof(int)), typeof(Type)),           Expr.Constant(typeof(int))),        // Type -> Type
                ConvertTest.Default(typeof(Type),    Expr.Convert(Expr.Constant(typeof(int)), typeof(object)),         Expr.Constant(typeof(int))),        // "boxed" RuntimeType -> Type

                // returning true/false on failure (33-48)
                ConvertTest.Checking(typeof(int),     Expr.Constant(1),                                                true),              // int -> int
                ConvertTest.Checking(typeof(int),     Expr.Convert(Expr.Constant(1), typeof(object)),                   true),              // boxed int -> int
                ConvertTest.Checking(typeof(int),     MakeExtensibleIntExpression(1),                                 true),              // extensible int -> int
                ConvertTest.Checking(typeof(int),     Expr.Convert(MakeExtensibleIntExpression(1), typeof(object)),    true),              // boxed extensible int -> int
                ConvertTest.Checking(typeof(decimal), Expr.Constant(1),                                                true),              // used defined: int -> decimal
                ConvertTest.Checking(typeof(decimal), Expr.Convert(Expr.Constant(1), typeof(object)),                   true),              // used defined: boxed int -> decimal
                ConvertTest.Checking(typeof(int),     Expr.Constant((byte)1),                                          true),              // primitive: byte -> int
                ConvertTest.Checking(typeof(int),     Expression.Convert(Expression.Constant((byte)1), typeof(object)),             true),              // primitive: boxed byte -> int
                ConvertTest.Checking(typeof(int?),    Expr.Constant(null),                                             false),              // null -> Nullable<T>
                ConvertTest.Checking(typeof(int?),    Expr.Convert(Expr.Constant(null), typeof(object)),                false),              // boxed null -> Nullable<T>
                ConvertTest.Checking(typeof(int?),    Expr.Constant(1),                                                true),              // T -> Nullable<T>
                ConvertTest.Checking(typeof(int?),    Expr.Convert(Expr.Constant(1), typeof(object)),                   true),              // boxed T -> Nullable<T>                 
                ConvertTest.Checking(typeof(object),  Expression.Constant(1),                                                true),            // int -> object
                ConvertTest.Checking(typeof(object),  Expression.Convert(Expression.Constant(1), typeof(object)),                   true),            // boxed int -> object
                ConvertTest.Checking(typeof(object),  Expr.Constant("foo"),                                            true),              // str -> object
                ConvertTest.Checking(typeof(object),  Expr.Convert(Expr.Constant("foo"), typeof(object)),               true),              // "boxed" str -> object
                ConvertTest.Checking(typeof(Type),    Expr.Convert(Expr.Constant(typeof(int)), typeof(Type)),           true),              // Type -> Type
                ConvertTest.Checking(typeof(Type),    Expr.Convert(Expr.Constant(typeof(int)), typeof(object)),         true),              // "boxed" RuntimeType -> Type

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // negative test cases

                // throwing on failure (49-54)
                ConvertTest.ThrowErr(typeof(int),     Expr.Constant("foo")),                                                                             // str -> int
                ConvertTest.ThrowErr(typeof(int),     Expr.Convert(Expr.Constant("foo"), typeof(object))),                                                // boxed str -> int
                ConvertTest.ThrowErr(typeof(int?),    Expr.Constant("foo")),                                                                             // str -> Nullable<int>
                ConvertTest.ThrowErr(typeof(int?),    Expr.Convert(Expr.Constant("foo"), typeof(object))),                                                // boxed str -> Nullable<T>
                ConvertTest.ThrowErr(typeof(string),  MakeExtensibleIntExpression(1)),                                                                  // Extensible<int> -> str
                ConvertTest.ThrowErr(typeof(string),  Expr.Convert(MakeExtensibleIntExpression(1), typeof(object))),                                     // boxed Extensible<int> -> str

                // returning default on failure (int/int? versions disabled due to interpretted mode bug where rule ret type == object instead of int)
                ConvertTest.Default(typeof(int),     Expression.Constant("foo"),                                            Expression.Constant(0)),                  // str -> int
                ConvertTest.Default(typeof(int),     Expression.Convert(Expression.Constant("foo"), typeof(object)),               Expression.Constant(0)),                  // boxed str -> int
                ConvertTest.Default(typeof(int?),    Expression.Constant("foo"),                                            MakeNullableIntExpression(null)),  // str -> Nullable<int>
                ConvertTest.Default(typeof(int?),    Expression.Convert(Expression.Constant("foo"), typeof(object)),               MakeNullableIntExpression(null)),  // boxed str -> Nullable<T>
                ConvertTest.Default(typeof(string),  MakeExtensibleIntExpression(1),                                 Expr.Constant(null)),               // Extensible<int> -> str
                ConvertTest.Default(typeof(string),  Expr.Convert(MakeExtensibleIntExpression(1), typeof(object)),    Expr.Constant(null)),               // boxed Extensible<int> -> str

                // returning true/false on failure
                ConvertTest.Checking(typeof(int),     Expr.Constant("foo"),                                            false),              // str -> int
                ConvertTest.Checking(typeof(int),     Expr.Convert(Expr.Constant("foo"), typeof(object)),               false),              // boxed str -> int
                ConvertTest.Checking(typeof(int?),    Expr.Constant("foo"),                                            false),              // str -> Nullable<int>
                ConvertTest.Checking(typeof(int?),    Expr.Convert(Expr.Constant("foo"), typeof(object)),               false),              // boxed str -> Nullable<T>
                ConvertTest.Checking(typeof(string),  MakeExtensibleIntExpression(1),                                 false),              // Extensible<int> -> str
                ConvertTest.Checking(typeof(string),  Expr.Convert(MakeExtensibleIntExpression(1), typeof(object)),    false),              // boxed Extensible<int> -> str
            };

            List<Expression> expressions = new List<Expression>();
            foreach (ConvertTest test in tests) {
                if (test.JustCheck.HasValue) {
                    if (test.JustCheck.Value) {
                        expressions.Add(
                            GenAreNotEqual(
                                Expression.Dynamic(OldConvertToAction.Make(ts.Binder, test.Type, test.Kind), typeof(object), Utils.CodeContext(), test.ConvertedValue),
                                Expr.Constant(null),
                                test.ToString()
                            )
                        );
                    } else {
                        expressions.Add(
                            GenAreEqual(
                                Expression.Dynamic(OldConvertToAction.Make(ts.Binder, test.Type, test.Kind), typeof(object), Utils.CodeContext(), test.ConvertedValue),
                                Expr.Constant(null),
                                test.ToString()
                            )
                        );
                    }
                } else if (!test.ShouldThrow) {
                    expressions.Add(
                        GenAreEqual(
                            Expression.Dynamic(OldConvertToAction.Make(ts.Binder, test.Type, test.Kind), test.Type, Utils.CodeContext(), test.ConvertedValue),
                            test.ResultingValue,
                            test.ToString()
                        )
                    );
                } else {
                    expressions.Add(
                        GenAssertExceptionThrown(
                            typeof(ArgumentTypeException),
                            Expression.Dynamic(OldConvertToAction.Make(ts.Binder, test.Type, test.Kind), test.Type, Utils.CodeContext(), test.ConvertedValue)
                        )
                    );
                }
            }

            return EU.BlockVoid(expressions);
        }

        [TestAttribute("Tests bitwise complement and dynamic sites")]
        private static Expression Scenario_BitwiseComplement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            TypedConstants[] constants = {
                new TypedConstants(typeof(int), (int)0, (int)1, (int)2, (int)~(int)0, (int)~(int)1, (int)~(int)2),
                new TypedConstants(typeof(uint), (uint)0, (uint)1, (uint)2, (uint)~(uint)0, (uint)~(uint)1, (uint)~(uint)2),
                new TypedConstants(typeof(sbyte), (sbyte)0, (sbyte)1, (sbyte)2, (sbyte)~(sbyte)0, (sbyte)~(sbyte)1, (sbyte)~(sbyte)2),
                new TypedConstants(typeof(byte), (byte)0, (byte)1, (byte)2, unchecked((byte)~(byte)0), unchecked((byte)~(byte)1), unchecked((byte)~(byte)2)),
                new TypedConstants(typeof(short), (short)0, (short)1, (short)2, (short)~(short)0, (short)~(short)1, (short)~(short)2),
                new TypedConstants(typeof(ushort), (ushort)0, (ushort)1, (ushort)2, unchecked((ushort)~(ushort)0), unchecked((ushort)~(ushort)1), unchecked((ushort)~(ushort)2)),
                new TypedConstants(typeof(long), (long)0, (long)1, (long)2, (long)~(long)0, (long)~(long)1, (long)~(long)2),
                new TypedConstants(typeof(ulong), (ulong)0, (ulong)1, (ulong)2, (ulong)~(ulong)0, (ulong)~(ulong)1, (ulong)~(ulong)2),
            };

            // Primitive types
            foreach (bool box in new bool[] { true, false }) {    // if we box the incoming args or not...
                foreach (TypedConstants t in constants) {
                    expressions.Add(
                        GenAreEqual(
                            Expr.Constant(t.NotZero),
                            Expr.Dynamic(ts.MakeOperation(Operators.Not), t.Type, Utils.CodeContext(), BoxIf(box, Expr.Constant(t.Zero)))
                        )
                    );
                    expressions.Add(
                        GenAreEqual(
                            Expr.Constant(t.NotOne),
                            Expr.Dynamic(ts.MakeOperation(Operators.Not), t.Type, Utils.CodeContext(), BoxIf(box, Expr.Constant(t.One)))
                        )
                    );
                    expressions.Add(
                        GenAreEqual(
                            Expr.Constant(t.NotTwo),
                            Expr.Dynamic(ts.MakeOperation(Operators.Not), t.Type, Utils.CodeContext(), BoxIf(box, Expr.Constant(t.Two)))
                        )
                    );
                }
            }

            //@TODO - Much much more
            return EU.BlockVoid(expressions);
        }

        [TestAttribute("Tests action expressions and dynamic sites")]
        private static Expression Scenario_DynamicSites(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            // DoOperationBinderHelper tests 

            // Primitive types
            foreach (bool boxed in new bool[] { true, false }) {    // if we box the incoming args or not...
                foreach (Type t in new Type[] { typeof(Int32), typeof(UInt32), typeof(SByte), typeof(Byte), typeof(Int16), typeof(UInt16), typeof(Int64), typeof(UInt64), typeof(Single), typeof(Double) }) {
                    Expression zero = Expr.Constant(Convert.ChangeType(0, t));
                    Expression one = Expr.Constant(Convert.ChangeType(1, t));
                    Expression two = Expr.Constant(Convert.ChangeType(2, t));
                    object comp = ~2;
                    Expression negtwo = null;
                    if (t.Name.StartsWith("U") || t == typeof(Byte)) {
                        comp = unchecked((byte)~2);
                    } else {
                        negtwo = Expr.Constant(Convert.ChangeType(-2, t));
                    }

                    Expression twocomp = Expr.Constant(Convert.ChangeType(comp, t));

                    Expression four = Expr.Constant(Convert.ChangeType(4, t));
                    Expression trueExpr = Expr.Constant(true);
                    Expression falseExpr = Expr.Constant(false);

                    Expression unboxedFour = four;
                    Expression unboxedZero = zero;
                    Expression unboxedOne = one;
                    Expression unboxedTwo = two;

                    if (boxed) {
                        zero = Expr.Convert(zero, typeof(object));
                        one = Expr.Convert(one, typeof(object));
                        two = Expr.Convert(two, typeof(object));
                        twocomp = Expr.Convert(twocomp, typeof(object));
                        four = Expr.Convert(four, typeof(object));
                    }


                    if (t != typeof(SByte) && t != typeof(Byte)) {
                        // unary negation if signed
                        if (negtwo != null) {
                            expressions.Add(GenAreEqual(negtwo,
                                Expr.Dynamic(ts.MakeOperation(Operators.Negate), t, Utils.CodeContext(), two))
                                );
                        }

                        // binary operators
                        expressions.Add(GenAreEqual(unboxedFour,
                            Expr.Dynamic(ts.MakeOperation(Operators.Add), t, Utils.CodeContext(), two, two))
                            );

                        expressions.Add(GenAreEqual(unboxedZero,
                            Expr.Dynamic(ts.MakeOperation(Operators.Subtract), t, Utils.CodeContext(), two, two))
                            );

                        expressions.Add(GenAreEqual(unboxedOne,
                            Expr.Dynamic(ts.MakeOperation(Operators.Divide), t, Utils.CodeContext(), two, two))
                            );

                        expressions.Add(GenAreEqual(unboxedZero,
                            Expr.Dynamic(ts.MakeOperation(Operators.Mod), t, Utils.CodeContext(), two, two))
                            );

                        expressions.Add(GenAreEqual(unboxedFour,
                            Expr.Dynamic(ts.MakeOperation(Operators.Multiply), t, Utils.CodeContext(), two, two))
                            );

                        // bitwise operations
                        if (t != typeof(Double) && t != typeof(Single)) {
                            expressions.Add(GenAreEqual(unboxedTwo,
                                Expr.Dynamic(ts.MakeOperation(Operators.BitwiseAnd), t, Utils.CodeContext(), two, two))
                                );

                            expressions.Add(GenAreEqual(unboxedTwo,
                                Expr.Dynamic(ts.MakeOperation(Operators.BitwiseOr), t, Utils.CodeContext(), two, two))
                                );

                            expressions.Add(GenAreEqual(unboxedZero,
                                Expr.Dynamic(ts.MakeOperation(Operators.ExclusiveOr), t, Utils.CodeContext(), two, two))
                                );


                            /* unverifable code:
                            expressions.Add(GenAreEqual(unboxedFour,
                                Expression.Action.Operator(Operators.LeftShift, t, two, one))
                            );

                            expressions.Add(GenAreEqual(unboxedZero,
                                Expression.Action.Operator(Operators.RightShift, t, two, two))
                            );*/
                        }
                    }

                    expressions.Add(GenAreEqual(trueExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.Equals), typeof(bool), Utils.CodeContext(), two, two))
                        );

                    expressions.Add(GenAreEqual(falseExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.Equals), typeof(bool), Utils.CodeContext(), two, four))
                        );

                    expressions.Add(GenAreEqual(falseExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.NotEquals), typeof(bool), Utils.CodeContext(), two, two))
                        );

                    expressions.Add(GenAreEqual(trueExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.NotEquals), typeof(bool), Utils.CodeContext(), two, four))
                        );

                    expressions.Add(GenAreEqual(falseExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.GreaterThan), typeof(bool), Utils.CodeContext(), two, two))
                        );

                    expressions.Add(GenAreEqual(falseExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.GreaterThan), typeof(bool), Utils.CodeContext(), two, four))
                        );

                    expressions.Add(GenAreEqual(trueExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.GreaterThan), typeof(bool), Utils.CodeContext(), four, two))
                        );

                    expressions.Add(GenAreEqual(falseExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.LessThan), typeof(bool), Utils.CodeContext(), two, two))
                        );

                    expressions.Add(GenAreEqual(trueExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.LessThan), typeof(bool), Utils.CodeContext(), two, four))
                        );

                    expressions.Add(GenAreEqual(falseExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.LessThan), typeof(bool), Utils.CodeContext(), four, two))
                        );

                    expressions.Add(GenAreEqual(trueExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.GreaterThanOrEqual), typeof(bool), Utils.CodeContext(), two, two))
                        );

                    expressions.Add(GenAreEqual(falseExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.GreaterThanOrEqual), typeof(bool), Utils.CodeContext(), two, four))
                        );

                    expressions.Add(GenAreEqual(trueExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.GreaterThanOrEqual), typeof(bool), Utils.CodeContext(), four, two))
                        );

                    expressions.Add(GenAreEqual(trueExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.LessThanOrEqual), typeof(bool), Utils.CodeContext(), two, two))
                        );

                    expressions.Add(GenAreEqual(trueExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.LessThanOrEqual), typeof(bool), Utils.CodeContext(), two, four))
                        );

                    expressions.Add(GenAreEqual(falseExpr,
                        Expr.Dynamic(ts.MakeOperation(Operators.LessThanOrEqual), typeof(bool), Utils.CodeContext(), four, two))
                        );
                }
            }

            expressions.Add(GenAreEqual(Expr.Constant(false),
                Expr.Dynamic(ts.MakeOperation(Operators.Not), typeof(bool), Utils.CodeContext(), Expr.Constant(true)))
                );

            expressions.Add(GenAreEqual(Expr.Constant(true),
                Expr.Dynamic(ts.MakeOperation(Operators.Not), typeof(bool), Utils.CodeContext(), Expr.Constant(false)))
                );

            expressions.Add(GenAreEqual(Expr.Constant(false),
                Expr.Dynamic(ts.MakeOperation(Operators.Not), typeof(bool), Utils.CodeContext(), Expr.Convert(Expr.Constant(true), typeof(object))))
                );

            expressions.Add(GenAreEqual(Expr.Constant(true),
                Expr.Dynamic(ts.MakeOperation(Operators.Not), typeof(bool), Utils.CodeContext(), Expr.Convert(Expr.Constant(false), typeof(object))))
                );

            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Scenario_DynamicSites2(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            //var expr
            ParameterExpression var = TestScope.Current.HiddenVariable(typeof(Int32), "scenario_dynamicsites_temp");

            //expr = 5
            expressions.Add(Expr.Assign(var, Expr.Constant(5)));

            //verify 7 == expr + 2 where expr+2 is a dynamic site resolved by the DefaultActionBinder to DefaultIntOps.Add(int,int)
            expressions.Add(GenAreEqual(Expr.Constant(7),
                Expr.Dynamic(OldDoOperationAction.Make(ts.Binder, Operators.Add), typeof(Int32), Utils.CodeContext(), var, Expr.Constant(2)))
                );

            //Generate this function:
            //def simplefunction():
            //    return 13
            //
            //And call it via a site verifying the return value
            //Hits CallTarget0
            expressions.Add(GenFunctionDefinition("simplefunction", Expr.Constant(13)));
            expressions.Add(
                GenAreEqual(
                    Expr.Constant(13),
                    Expr.Convert(
                        Expr.Dynamic(
                            OldCallAction.Make(ts.Binder, 0),
                            typeof(object),
                            Utils.CodeContext(),
                            Utils.Read(SymbolTable.StringToId("simplefunction"))
                        ),
                        typeof(int)
                    )
                )
            );

            //@TODO
            //DynamicSites_Helper generates a more complex scenario involving a method
            //with the given number of arguments being called through one site
            //with rotating parameter types.
            //expressions.Add(DynamicSites_Helper(2)); //CallTarget2
            //expressions.Add(DynamicSites_Helper(3)); //CallTarget3
            //expressions.Add(DynamicSites_Helper(4)); //CallTarget4
            //expressions.Add(DynamicSites_Helper(5)); //CallTarget5
            //expressions.Add(DynamicSites_Helper(6)); //CallTargetWithContextN
            //expressions.Add(DynamicSites_Helper(128)); //CallTargetWithContextN
            //expressions.Add(DynamicSites_Helper(256)); //CallTargetWithContextN
            //expressions.Add(DynamicSites_Helper(512)); //CallTargetWithContextN
            //expressions.Add(DynamicSites_Helper(1024)); //CallTargetWithContextN

            return EU.BlockVoid(expressions);
        }
#endif

        [TestAttribute]
        private static Expression Test_ActionExpression(TestScenarios ts) {
            //@TODO - Cover the few blocks not hit by the dynamic sites scenario tests above
            return Expr.Empty();
        }

        public static bool IsTrue(object obj) {
            return obj is bool && (bool)obj == true || obj is int && (int)obj != 0;
        }

        public static bool IsTrueBool(bool b) {
            return b;
        }

        [TestAttribute]
        private static Expression Test_CoalesceTrue(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            Expression cTrue = Expr.Constant(true);
            Expression cFalse = Expr.Constant(false);
            Expression one = Expr.Constant(1);

            Expression cTrueObject = Expr.Convert(Expr.Constant(true), typeof(object));
            Expression cFalseObject = Expr.Convert(Expr.Constant(false), typeof(object));
            Expression oneObject = Expr.Convert(Expr.Constant(1), typeof(object));

            MethodInfo isTrue = new Func<object, bool>(IsTrue).Method;
            MethodInfo isTrueBool = ((Func<bool, bool>)IsTrueBool).Method;

            //Cover basic negative cases
            AssertExceptionThrown<ArgumentNullException>(delegate() { AstUtils.CoalesceTrue(TestScope.Current.Block, null, cTrue, isTrue); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { AstUtils.CoalesceTrue(TestScope.Current.Block, cFalse, null, isTrue); });

            //Basic positive cases
            //false && true
            expressions.Add(GenAreEqual(cFalse, AstUtils.CoalesceTrue(TestScope.Current.Block, cFalse, cTrue, isTrueBool)));

            //true && false
            expressions.Add(GenAreEqual(cFalse, AstUtils.CoalesceTrue(TestScope.Current.Block, cTrue, cFalse, isTrueBool)));

            //false && false
            expressions.Add(GenAreEqual(cFalse, AstUtils.CoalesceTrue(TestScope.Current.Block, cFalse, cFalse, isTrueBool)));

            //true && true
            expressions.Add(GenAreEqual(cTrue, AstUtils.CoalesceTrue(TestScope.Current.Block, cTrue, cTrue, isTrueBool)));

            //Confirm early exit fo the (false and X) case
            Expression ae = AstUtils.CoalesceTrue(
                TestScope.Current.Block,
                cFalse,
                Expr.Call(
                    typeof(TestScenarios).GetMethod("Throw"),
                    Expr.Constant("Did not early exit in a (false and X) expression")
                ),
                isTrueBool
            );

            expressions.Add(GenAreEqual(cFalse, ae)); //Should not throw

            //true && 1
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceTrue(TestScope.Current.Block, cTrue, one, isTrue); });
            //false && 1
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceTrue(TestScope.Current.Block, cFalse, one, isTrue); });
            //1 && true
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceTrue(TestScope.Current.Block, one, cTrue, isTrue); });
            //true && null
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceTrue(TestScope.Current.Block, cTrue, Expr.Constant(null), isTrue); });
            //false && null
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceTrue(TestScope.Current.Block, cFalse, Expr.Constant(null), isTrue); });
            //null && true
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceTrue(TestScope.Current.Block, Expr.Constant(null), cTrue, isTrue); });

            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_ArrayIndexAssignment(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            ParameterExpression var = TestScope.Current.HiddenVariable(typeof(Int32[]), "test_arrayindexassignment_temp");
            ConstantExpression zero = Expr.Constant(0);
            ConstantExpression one = Expr.Constant(1);
            ConstantExpression two = Expr.Constant(2);
            ConstantExpression three = Expr.Constant(3);
            ConstantExpression four = Expr.Constant(4);
            ConstantExpression five = Expr.Constant(5);
            ConstantExpression six = Expr.Constant(6);

            //Test various types of arrays
            expressions.Add(ArrayHelper<sbyte>());
            expressions.Add(ArrayHelper<short>());
            expressions.Add(ArrayHelper<int>());
            expressions.Add(ArrayHelper<long>());
            expressions.Add(ArrayHelper<float>());
            expressions.Add(ArrayHelper<double>());
            expressions.Add(ArrayHelper<byte>());
            expressions.Add(ArrayHelper<ushort>());
            expressions.Add(ArrayHelper<uint>());
            expressions.Add(ArrayHelper<ulong>());

            //Negative cases
            expressions.Add(Expr.Assign(var, Expr.NewArrayInit(typeof(int), one, two, three)));
            expressions.Add(GenAssertExceptionThrown(typeof(IndexOutOfRangeException), Expr.Assign(Expr.ArrayAccess(var, Expr.Constant(-1)), two)));
            expressions.Add(GenAssertExceptionThrown(typeof(IndexOutOfRangeException), Expr.Assign(Expr.ArrayAccess(var, three), two)));

            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Assign(Expr.ArrayAccess(null, one), one); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Assign(Expr.ArrayAccess(var, (Expression)null), one); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Assign(Expr.ArrayAccess(var, one), null); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Assign(Expr.ArrayAccess(one, one), one); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Assign(Expr.ArrayAccess(var, Expr.Constant((short)0)), one); });

            return EU.BlockVoid(expressions);
        }

        private static Expression ArrayHelper<T>() {
            //Cover arrays of varying types...
            List<Expression> expressions = new List<Expression>();
            ConstantExpression zero = Expr.Constant(((IConvertible)0).ToType(typeof(T), null));
            ConstantExpression one = Expr.Constant(((IConvertible)1).ToType(typeof(T), null));
            ConstantExpression two = Expr.Constant(((IConvertible)2).ToType(typeof(T), null));
            ConstantExpression three = Expr.Constant(((IConvertible)3).ToType(typeof(T), null));
            ConstantExpression four = Expr.Constant(((IConvertible)4).ToType(typeof(T), null));
            ConstantExpression five = Expr.Constant(((IConvertible)5).ToType(typeof(T), null));
            ConstantExpression six = Expr.Constant(((IConvertible)6).ToType(typeof(T), null));
            ParameterExpression var = TestScope.Current.HiddenVariable(typeof(T[]), String.Format("test_arrayindexassignment_temp{0}", typeof(T).ToString()));

            ConstantExpression intOne = Expr.Constant(1);
            ConstantExpression intTwo = Expr.Constant(2);

            //var = [1,2,3]
            expressions.Add(Expr.Assign(var, Expr.NewArrayInit(typeof(T), one, two, three)));

            //verify var[1]==2
            expressions.Add(GenAreEqual(two, Expr.ArrayIndex(var, intOne)));

            //Create a new ArrayIndexAssignment and excercise the properties
            var aia = Expr.Assign(Expr.ArrayAccess(var, intOne), five);
            var ai = (IndexExpression)aia.Left;
            AreEqual(ai.Object, var);
            AreEqual(ai.Arguments[0], intOne);
            AreEqual(aia.Right, five);

            //var[1]=5
            expressions.Add(aia);

            //verify var[1]==5
            expressions.Add(GenAreEqual(five, Expr.ArrayIndex(var, intOne)));

            //var[0]=4
            expressions.Add(Expr.Assign(Expr.ArrayAccess(var, Expr.Constant(0)), four));

            //verify var[0]==4
            expressions.Add(GenAreEqual(four, Expr.ArrayIndex(var, Expr.Constant(0))));

            //var[2]=6
            expressions.Add(Expr.Assign(Expr.ArrayAccess(var, intTwo), six));

            //verify var[2]==6
            expressions.Add(GenAreEqual(six, Expr.ArrayIndex(var, intTwo)));

            expressions.Add(Expr.Assign(var, Expr.NewArrayInit(typeof(T), one, two, three)));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_ArrayIndexExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            ParameterExpression var = TestScope.Current.HiddenVariable(typeof(Int32[]), "test_arrayindexexpression_temp");
            ConstantExpression negone = Expr.Constant(-1);
            ConstantExpression zero = Expr.Constant(0);
            ConstantExpression one = Expr.Constant(1);
            ConstantExpression two = Expr.Constant(2);
            ConstantExpression three = Expr.Constant(3);

            //var = [1,2,3]
            expressions.Add(Expr.Assign(var, Expr.NewArrayInit(typeof(Int32), one, two, three)));

            //verify each array element
            expressions.Add(GenAssertExceptionThrown(typeof(IndexOutOfRangeException), Expr.ArrayIndex(var, negone)));
            expressions.Add(GenAreEqual(one, Expr.ArrayIndex(var, zero)));
            expressions.Add(GenAreEqual(two, Expr.ArrayIndex(var, one)));
            expressions.Add(GenAreEqual(three, Expr.ArrayIndex(var, two)));
            expressions.Add(GenAssertExceptionThrown(typeof(IndexOutOfRangeException), Expr.ArrayIndex(var, three)));

            //Exercise properties
            BinaryExpression aie = Expr.ArrayIndex(var, two);
            AreEqual(var, aie.Left);
            AreEqual(two, aie.Right);

            //Negative construction cases
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.ArrayIndex(null, two); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.ArrayIndex(var, (Expression)null); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.ArrayIndex(two, two); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.ArrayIndex(var, Expr.Constant((short)1)); });

            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_MultidimensionalArrayIndexExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            ParameterExpression var = TestScope.Current.HiddenVariable(typeof(Int32[,]), "test_arrayindexexpression_temp1");
            ConstantExpression negone = Expr.Constant(-1);
            ConstantExpression zero = Expr.Constant(0);
            ConstantExpression one = Expr.Constant(1);
            ConstantExpression two = Expr.Constant(2);
            ConstantExpression three = Expr.Constant(3);

            //var = [[1,2,3],[2,3,1]]
            expressions.Add(Expr.Assign(var, Expr.NewArrayBounds(typeof(int), two, three)));
            MethodInfo setter = typeof(Int32[,]).GetMethod("Set");
            expressions.Add(Expr.Call(var, setter, zero, zero, one));
            expressions.Add(Expr.Call(var, setter, zero, one, two));
            expressions.Add(Expr.Call(var, setter, zero, two, three));
            expressions.Add(Expr.Call(var, setter, one, zero, two));
            expressions.Add(Expr.Call(var, setter, one, one, three));
            expressions.Add(Expr.Call(var, setter, one, two, one));

            //verify each array element
            expressions.Add(GenAssertExceptionThrown(typeof(IndexOutOfRangeException), Expr.ArrayIndex(var, one, negone)));
            expressions.Add(GenAreEqual(one, Expr.ArrayIndex(var, zero, zero)));
            expressions.Add(GenAreEqual(three, Expr.ArrayIndex(var, zero, two)));
            expressions.Add(GenAreEqual(three, Expr.ArrayIndex(var, one, one)));
            expressions.Add(GenAssertExceptionThrown(typeof(IndexOutOfRangeException), Expr.ArrayIndex(var, one, three)));

            //Negative construction cases
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.ArrayIndex(null, two, two); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.ArrayIndex(var, null, one); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.ArrayIndex(var, one, null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.ArrayIndex(var, (Expression[])null); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.ArrayIndex(two, two, two); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.ArrayIndex(var, one, Expr.Constant((short)1)); });

            return EU.BlockVoid(expressions);
        }

        public class TestIndexedProperty {
            private int[,] _prop;
            public int this[int x, int y] {
                get { return _prop[x, y]; }
                set { _prop[x, y] = value; }
            }

            public TestIndexedProperty() {
                _prop = new int[2, 3];
            }

            public string TakesByRef(int x, ref string y) {
                return "ref";
            }
        }

        [TestAttribute]
        private static Expression Test_IndexedPropertyExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            ParameterExpression var = TestScope.Current.HiddenVariable(typeof(TestIndexedProperty), "test_indexpropexpression_temp1");

            ConstantExpression negone = Expr.Constant(-1);
            ConstantExpression zero = Expr.Constant(0);
            ConstantExpression one = Expr.Constant(1);
            ConstantExpression two = Expr.Constant(2);
            ConstantExpression three = Expr.Constant(3);

            expressions.Add(Expr.Assign(var, Expr.New(typeof(TestIndexedProperty).GetConstructors()[0])));

            PropertyInfo prop = typeof(TestIndexedProperty).GetProperty("Item");
            expressions.Add(Expr.Assign(Expr.Property(var, prop, zero, zero), one));
            expressions.Add(Expr.Assign(Expr.Property(var, prop, zero, one), two));
            expressions.Add(Expr.Assign(Expr.Property(var, prop, zero, two), three));
            expressions.Add(Expr.Assign(Expr.Property(var, prop, one, zero), two));
            expressions.Add(Expr.Assign(Expr.Property(var, prop, one, one), three));
            expressions.Add(Expr.Assign(Expr.Property(var, prop, one, two), one));

            //verify each array element
            expressions.Add(GenAssertExceptionThrown(typeof(IndexOutOfRangeException), Expr.Property(var, prop, one, negone)));
            expressions.Add(GenAreEqual(one, Expr.Property(var, prop, zero, zero)));
            expressions.Add(GenAreEqual(three, Expr.Property(var, prop, zero, two)));
            expressions.Add(GenAreEqual(three, Expr.Property(var, prop, one, one)));
            expressions.Add(GenAssertExceptionThrown(typeof(IndexOutOfRangeException), Expr.Property(var, prop, one, three)));

            //Negative construction cases
            //Accessing a non-static property using null instance causes ArgumentException
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Property(null, prop, two, two); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Property(var, prop, (Expression)null, one); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Property(var, prop, one, null); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Property(var, prop, (Expression[])null); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Property(two, prop, two, two); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Property(var, prop, one, Expr.Constant((short)1)); });

            return EU.BlockVoid(expressions);
        }

       [TestAttribute]
        private static Expression Test_IndexedPropertyExpressionSpill(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            ParameterExpression var = TestScope.Current.HiddenVariable(typeof(TestIndexedProperty), "test_indexpropexpression_temp1");

            ConstantExpression negone = Expr.Constant(-1);
            ConstantExpression zero = Expr.Constant(0);
            ConstantExpression one = Expr.Constant(1);
            ConstantExpression two = Expr.Constant(2);
            ConstantExpression three = Expr.Constant(3);

            TryStatementBuilder t = Utils.Try(Expression.Constant(42)).Finally();
            Expression twoComma = Expression.Block(t, Expression.Constant(2));

            expressions.Add(Expr.Assign(var, Expr.New(typeof(TestIndexedProperty).GetConstructors()[0])));

            PropertyInfo prop = typeof(TestIndexedProperty).GetProperty("Item");
            expressions.Add(Expr.Assign(Expr.Property(var, prop, zero, zero), one));
            expressions.Add(Expr.Assign(Expr.Property(var, prop, zero, one), two));
            expressions.Add(Expr.Assign(Expr.Property(var, prop, zero, two), three));
            expressions.Add(Expr.Assign(Expr.Property(var, prop, one, zero), two));
            expressions.Add(Expr.Assign(Expr.Property(var, prop, one, one), three));
            expressions.Add(Expr.Assign(Expr.Property(var, prop, one, twoComma), one));   // <-- comma

            //verify each array element
            expressions.Add(GenAssertExceptionThrown(typeof(IndexOutOfRangeException), Expr.Property(var, prop, one, negone)));
            expressions.Add(GenAreEqual(one, Expr.Property(var, prop, zero, zero)));
            expressions.Add(GenAreEqual(three, Expr.Property(var, prop, zero, twoComma)));   // <-- comma
            expressions.Add(GenAreEqual(three, Expr.Property(var, prop, one, one)));
            expressions.Add(GenAssertExceptionThrown(typeof(IndexOutOfRangeException), Expr.Property(var, prop, one, three)));

            //Negative construction cases
            //Accessing a non-static property using null instance causes ArgumentException
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Property(null, prop, two, two); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Property(var, prop, (Expression)null, one); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Property(var, prop, one, null); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Property(var, prop, (Expression[])null); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Property(two, prop, two, two); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Property(var, prop, one, Expr.Constant((short)1)); });

            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_BinaryExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            ConstantExpression cTrue = Expr.Constant(true);
            ConstantExpression cFalse = Expr.Constant(false);
            ConstantExpression cNull = Expr.Constant(null);

            //Basic negative cases
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Equal(null, cFalse); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Equal(cFalse, null); });
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.Multiply(Expr.Constant(3.2), Expr.Constant(5)); });
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.Multiply(Expr.Constant(3), Expr.Constant(5.7)); });

            //Properties
            BinaryExpression be = Expr.AndAlso(cTrue, cFalse);
            AreEqual(cTrue, be.Left);
            AreEqual(cFalse, be.Right);
            AreEqual(typeof(bool), be.Type);

            //Operators.Equal
            //true==true
            be = Expr.Equal(cTrue, cTrue);
            expressions.Add(GenAreEqual(cTrue, be));

            //true==false
            be = Expr.Equal(cTrue, cFalse);
            expressions.Add(GenAreEqual(cFalse, be));

            //1.0==1.2
            be = Expr.Equal(Expr.Constant(1.0), Expr.Constant(1.2));
            expressions.Add(GenAreEqual(cFalse, be));

            //null==null
            be = Expr.Equal(Expr.Constant(null), Expr.Constant(null));
            expressions.Add(GenAreEqual(cTrue, be));

            //false==null
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.Equal(cFalse, Expr.Constant(null)); });

            //null==false
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.Equal(Expr.Constant(null), cFalse); });

            //BinaryOperators.Multiply==BinaryOperators.Multiply
            be = Expr.Equal(Expr.Constant(ExpressionType.Multiply), Expr.Constant(ExpressionType.Multiply));
            expressions.Add(GenAreEqual(cTrue, be));

            //BinaryOperators.Multiply==BinaryOperators.Add
            be = Expr.Equal(Expr.Constant(ExpressionType.Multiply), Expr.Constant(ExpressionType.Add));
            expressions.Add(GenAreEqual(cFalse, be));

            //BinaryOperators.NotEqual
            //true!=true
            be = Expr.NotEqual(cTrue, cTrue);
            expressions.Add(GenAreEqual(cFalse, be));

            //true!=false
            be = Expr.NotEqual(cTrue, cFalse);
            expressions.Add(GenAreEqual(cTrue, be));

            //1!=1.2
            be = Expr.NotEqual(Expr.Constant(1.0), Expr.Constant(1.2));
            expressions.Add(GenAreEqual(cTrue, be));

            //null!=null
            be = Expr.NotEqual(Expr.Constant(null), Expr.Constant(null));
            expressions.Add(GenAreEqual(cFalse, be));

            //false!=null
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.NotEqual(cFalse, Expr.Constant(null)); });

            //null!=false
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.NotEqual(Expr.Constant(null), cFalse); });

            //BinaryOperators.Multiply!=BinaryOperators.Multiply
            be = Expr.NotEqual(Expr.Constant(ExpressionType.Multiply), Expr.Constant(ExpressionType.Multiply));
            expressions.Add(GenAreEqual(cFalse, be));

            //BinaryOperators.Multiply!=BinaryOperators.Add
            be = Expr.NotEqual(Expr.Constant(ExpressionType.Multiply), Expr.Constant(ExpressionType.Add));
            expressions.Add(GenAreEqual(cTrue, be));

            //Operator.AndAlso
            //false AndAlso true
            be = Expr.AndAlso(cFalse, cTrue);
            expressions.Add(GenAreEqual(cFalse, be));

            //true AndAlso false
            be = Expr.AndAlso(cTrue, cFalse);
            expressions.Add(GenAreEqual(cFalse, be));

            //false AndAlso false
            be = Expr.AndAlso(cFalse, cFalse);
            expressions.Add(GenAreEqual(cFalse, be));

            //true AndAlso true
            be = Expr.AndAlso(cTrue, cTrue);
            expressions.Add(GenAreEqual(cTrue, be));

            //false AndAlso throw
            be = Expr.AndAlso(
                cFalse,
                Expr.Call(
                    typeof(TestScenarios).GetMethod("Throw"),
                    Expr.Constant("Did not early exit in a (false and X) expression")
                )
            );
            expressions.Add(GenAreEqual(cFalse, be)); //Should not throw

            //true AndAlso 1
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.AndAlso(cTrue, Expr.Constant(1)); });
            //false AndAlso 1
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.AndAlso(cFalse, Expr.Constant(1)); });
            //1 AndAlso true
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.AndAlso(Expr.Constant(1), cTrue); });
            //true AndAlso null
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.AndAlso(cTrue, Expr.Constant(null)); });
            //false AndAlso null
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.AndAlso(cFalse, Expr.Constant(null)); });
            //null AndAlso true
            AssertExceptionThrown<InvalidOperationException>(delegate() { Expr.AndAlso(Expr.Constant(null), cTrue); });

            //Operator.Multiply
            //30 == 5*6
            be = Expr.Multiply(Expr.Constant(5), Expr.Constant(6));
            expressions.Add(GenAreEqual(Expr.Constant(30), be));

            //63 != 7.1*9.9 (floats don't get converted to ints via truncation)            
            be = Expr.Multiply(Expr.Constant(7.1), Expr.Constant(9.9));
            expressions.Add(GenAreNotEqual(Expr.Constant(63.0), be));

            //5*null
            AssertExceptionThrown<InvalidOperationException>(delegate() {
                Expr.Multiply(Expr.Constant(5), Expr.Constant(null));
            });

            //null*5
            AssertExceptionThrown<InvalidOperationException>(delegate() {
                Expr.Multiply(Expr.Constant(null), Expr.Constant(5));
            });

            //5*str
            AssertExceptionThrown<InvalidOperationException>(delegate() {
                Expr.Multiply(Expr.Constant(5), Expr.Constant("str"));
            });

            //str*5
            AssertExceptionThrown<InvalidOperationException>(delegate() {
                Expr.Multiply(Expr.Constant("str"), Expr.Constant(5));
            });

            //@TODO - Emit case here

            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_BoundAssignment(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            ParameterExpression var = TestScope.Current.HiddenVariable(typeof(Int32), "test_boundassignment_temp");

            //Basic negative cases
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Assign(null, Expr.Constant(3)); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Assign(var, null); });

            //Simple positive case hitting the only blocks not already hit elsewhere
            var ba = Expr.Assign(var, Expr.Constant(42));

            expressions.Add(ba);
            expressions.Add(GenAreEqual(Expr.Constant(42), var));

            //@TODO - More complete coverage
            return EU.BlockVoid(expressions);
        }

        public static int foo(int x, int y) { return x; }
        public struct ss {            
            private int dummy;
            void foo()  // to get rid of the error
            {
                dummy = 5;
                Console.WriteLine(dummy);
            }
        }

        [TestAttribute]
        private static Expression Test_ThrowAssignment(TestScenarios ts) {


            List<Expression> expressions = new List<Expression>();
            ParameterExpression var = TestScope.Current.HiddenVariable(typeof(ss), "test_throw_temp");

            // comma with throw inside
            Expression rhs = Expr.Block(Expression.Constant(42), Expr.Throw(Expression.Constant(new Exception()), typeof(void)), Expression.Constant(42));
            Expression spilled_call = Expr.Call(typeof(TestScenarios), "foo", null, Expression.Constant(42), rhs);

            expressions.Add(spilled_call);
            // throw as an RHS
            expressions.Add(Expression.Assign(var, Expr.Throw(Expression.Constant(new Exception()), typeof(ss))));

            // balanced Condition
            expressions.Add(
                Expression.Condition(
                    Expression.Constant(true),
                    Expr.Throw(Expression.Constant(new Exception()), typeof(ss)),
                    Expression.Constant(default(ss))
                )
            );

            // misbalanced Condition
            AssertExceptionThrown<ArgumentException>(
                delegate() {
                    Expression.Condition(
                        Expression.Constant(true),
                        Expr.Throw(Expression.Constant(new Exception()), typeof(decimal)),
                        Expr.Throw(Expression.Constant(new Exception()))
                    );
                }
            );

            Expression t = Expression.TryCatch(EU.BlockVoid(expressions), Expression.Catch(typeof(Exception), Expression.Empty()));

            return EU.BlockVoid(t);
        }
#if TODO // obsolete: code context, old action, global variables

        [TestAttribute]
        private static Expression Test_BreakStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            //Test break in a DoStatement
            //Set up a counter we'll use in the loop
            ParameterExpression var1 = TestScope.Current.HiddenVariable(typeof(Int32), "test_breakstatement_temp1");
            expressions.Add(Expr.Assign(var1, Expr.Constant(0)));

            //Define the loop test and create the loop
            //var1 = 0
            // for (;;) {
            //    var1 = var1 + 1
            //    if var1==2
            //        break
            //    if  (var1 <= 5) {
            //    } else {
            //       break
            //    } 
            //} 
            LabelTarget label = Expr.Label();

            expressions.Add(
                TestSpan.GetDebugInfoForFrame(
                    Expression.Loop(
                        EU.BlockVoid(
                            ts.GenIncrement(var1),
                            Utils.If(Expr.Equal(Expr.Constant(2), var1),
                                TestSpan.GetDebugInfoForFrame(Expression.Break(label))
                            ),
                            Expression.Condition(
                                LessThanEquals(ts, var1, Expr.Constant(5)),
                                Expression.Empty(),
                                Expression.Break(label)
                            )
                        ),
                        label,
                        null
                    )
                )
            );

            //Verify the loop executed the right amount of times
            expressions.Add(GenAreEqual(var1, Expr.Constant(2)));

            //@TODO - Test in LoopStatement
            //@TODO - Execute test
            //@TODO - Test in various other interesting blocks, especially try/catch/finally blocks
            //@TODO - Test not in a loop
            return EU.BlockVoid(expressions);
        }
#endif
        [TestAttribute]
        private static Expression Test_CatchBlock(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            //The one negative construction case
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Catch(typeof(Exception), null); });

            //Hit the properties
            DefaultExpression body = Expr.Empty();
            CatchBlock cb = Expr.Catch(typeof(InvalidOperationException), body, null);
            AreEqual(cb.Body, body);

            AreEqual(cb.Test, typeof(InvalidOperationException));
            AreEqual(cb.Variable, null);

            //Now some simple positive scenarios...
             
            //try{ throw ApplicationException } catch(ApplicationException){}
            TryExpression @try = Expr.TryCatch(GenThrow<ApplicationException>("Fail"), Expr.Catch(typeof(ApplicationException), Expr.Empty()));
            expressions.Add(@try);

            //try{ throw ApplicationException } catch(Exception){}
            @try = Expr.TryCatch(GenThrow<ApplicationException>("Fail"), Expr.Catch(typeof(Exception), Expr.Empty()));
            expressions.Add(@try);

            //try{ throw ApplicationException } catch(Object){}
            @try = Expr.TryCatch(GenThrow<ApplicationException>("Fail"), Expr.Catch(typeof(Object), Expr.Empty()));
            expressions.Add(@try);

            //try{ throw ApplicationException }
            //catch(InvalidOperationException){ throw Exception }
            //catch(ApplicationException){ }
            //catch(ArgumentException){ throw Exception }
            @try = Expr.TryCatch(GenThrow<ApplicationException>("Fail"), Expr.Catch(typeof(InvalidOperationException), GenThrow("Fail")), Expr.Catch(typeof(ApplicationException), Expr.Empty()), Expr.Catch(typeof(ArgumentException), GenThrow("Fail")));
            expressions.Add(@try);

            //try{ throw ApplicationException }
            //catch(InvalidOperationException){ throw Exception }
            //catch(Exception){ }
            //catch(ApplicationException){ throw Exception }
            @try = Expr.TryCatch(GenThrow<ApplicationException>("Fail"), Expr.Catch(typeof(InvalidOperationException), GenThrow("Fail")), Expr.Catch(typeof(Exception), Expr.Empty()), Expr.Catch(typeof(ApplicationException), GenThrow("Fail")));
            expressions.Add(@try);

            //try{
            //    try { throw ApplicationException }
            //    catch (ApplicationException) { throw ArgumentException }
            //    throw InvalidOperationException
            //}catch (ArgumentException) {}
            @try = Expr.TryCatch(EU.BlockVoid(Expr.TryCatch(GenThrow<ApplicationException>("Fail"), Expr.Catch(typeof(ApplicationException), GenThrow<ArgumentException>("Fail"))),
                GenThrow<InvalidOperationException>("Fail")), Expr.Catch(typeof(ArgumentException), Expr.Empty()));
            expressions.Add(@try);

            //@TODO - With a variable target and a non-empty body
            //@TODO - Throw from more block types (finally, etc)
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_CodeBlock(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            //This is partially covered in some of the DynamicSite tests
            expressions.Add(GenThrow("@TODO"));
            //@TODO - Test strongly-types parameters
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_CodeBlockExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

#if TODO // obsolete: code context, old action, global variables

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_CodeContextExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }
#endif
        [TestAttribute]
        private static Expression Test_CommaExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            ConstantExpression one = Expr.Constant(1);
            ConstantExpression two = Expr.Constant(2);
            ConstantExpression three = Expr.Constant(3);

            //Cover basic negative cases
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Block((Expression)null); });

            //Basic positive cases
            expressions.Add(GenAreEqual(three, Expr.Block(new Expression[] { one, two, three })));

            //Properties
            BlockExpression ce = Expr.Block(new Expression[] { one, two, three });
            AreEqual(3, ce.Expressions.Count);
            AreEqual(one, ce.Expressions[0]);
            AreEqual(two, ce.Expressions[1]);
            AreEqual(three, ce.Expressions[2]);

            //Regression case for bug 271799
            MethodInfo mi = typeof(System.Console).GetMethod("WriteLine", new Type[] { typeof(string), typeof(object) });
            expressions.Add(Expr.Call(null, mi,
                Expr.Constant("A"),
                Expr.Block(
                    Utils.Try().Catch(typeof(Exception)),
                    Expr.Constant("B")
                )
            ));


            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_ConditionalExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            ConstantExpression cTrue = Expr.Constant(true);
            ConstantExpression cFalse = Expr.Constant(false);
            ConstantExpression one = Expr.Constant(1);
            ConstantExpression two = Expr.Constant(2);

            //Basic negative cases
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Condition(null, one, two); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Condition(cTrue, null, two); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Condition(cTrue, one, null); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Condition(cTrue, one, cFalse); });

            //Properties
            ConditionalExpression ce = Expr.Condition(cTrue, two, one);
            AreEqual(cTrue, ce.Test);
            AreEqual(two, ce.IfTrue);
            AreEqual(one, ce.IfFalse);
            AreEqual(typeof(Int32), ce.Type);

            //Basic positive cases
            expressions.Add(GenAreEqual(one, Expr.Condition(cTrue, one, two)));
            expressions.Add(GenAreEqual(two, Expr.Condition(cFalse, one, two)));

            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_ConstantExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            //@TODO - AreNotEqual in general (and maybe IsGreaterThan, IsLessThan, etc)

            //Compiler.cs supports the following types for constants...
            //null
            expressions.Add(GenAreEqual(Expr.Constant(null), Expr.Constant(null)));

            //int
            expressions.Add(GenAreEqual(Expr.Constant(int.MinValue), Expr.Constant(int.MinValue)));
            expressions.Add(GenAreEqual(Expr.Constant((int)-1), Expr.Constant((int)-1)));
            expressions.Add(GenAreEqual(Expr.Constant((int)0), Expr.Constant((int)0)));
            expressions.Add(GenAreEqual(Expr.Constant((int)1), Expr.Constant((int)1)));
            expressions.Add(GenAreEqual(Expr.Constant(int.MaxValue), Expr.Constant(int.MaxValue)));

            //float
            //@TODO - NaN, except NaN!=NaN so...
            expressions.Add(GenAreEqual(Expr.Constant(float.NegativeInfinity), Expr.Constant(float.NegativeInfinity)));
            expressions.Add(GenAreEqual(Expr.Constant(float.MinValue), Expr.Constant(float.MinValue)));
            expressions.Add(GenAreEqual(Expr.Constant((float)-1), Expr.Constant((float)-1)));
            expressions.Add(GenAreEqual(Expr.Constant((float)0), Expr.Constant((float)0)));
            expressions.Add(GenAreEqual(Expr.Constant(float.Epsilon), Expr.Constant(float.Epsilon)));
            expressions.Add(GenAreEqual(Expr.Constant(float.MaxValue), Expr.Constant(float.MaxValue)));
            expressions.Add(GenAreEqual(Expr.Constant(float.PositiveInfinity), Expr.Constant(float.PositiveInfinity)));

            //double
            //@TODO - NaN, except NaN!=NaN so...
            expressions.Add(GenAreEqual(Expr.Constant(double.NegativeInfinity), Expr.Constant(double.NegativeInfinity)));
            expressions.Add(GenAreEqual(Expr.Constant(double.MinValue), Expr.Constant(double.MinValue)));
            expressions.Add(GenAreEqual(Expr.Constant((double)-1), Expr.Constant((double)-1)));
            expressions.Add(GenAreEqual(Expr.Constant((double)0), Expr.Constant((double)0)));
            expressions.Add(GenAreEqual(Expr.Constant(double.Epsilon), Expr.Constant(double.Epsilon)));
            expressions.Add(GenAreEqual(Expr.Constant(double.MaxValue), Expr.Constant(double.MaxValue)));
            expressions.Add(GenAreEqual(Expr.Constant(double.PositiveInfinity), Expr.Constant(double.PositiveInfinity)));

            //long
            expressions.Add(GenAreEqual(Expr.Constant(long.MinValue), Expr.Constant(long.MinValue)));
            expressions.Add(GenAreEqual(Expr.Constant((long)-1), Expr.Constant((long)-1)));
            expressions.Add(GenAreEqual(Expr.Constant((long)0), Expr.Constant((long)0)));
            expressions.Add(GenAreEqual(Expr.Constant((long)1), Expr.Constant((long)1)));
            expressions.Add(GenAreEqual(Expr.Constant(long.MaxValue), Expr.Constant(long.MaxValue)));

            //Complex64
            //expressions.Add(GenAreEqual(Expression.Constant(new Complex64(0, 1)), Expression.Constant(new Complex64(0, 1))));
            //expressions.Add(GenAreEqual(Expression.Constant(new Complex64(1, 0)), Expression.Constant(new Complex64(1, 0))));
            //expressions.Add(GenAreEqual(Expression.Constant(new Complex64(1, 1)), Expression.Constant(new Complex64(1, 1))));

            //BigInteger
            //expressions.Add(GenAreEqual(Expression.Constant(BigInteger.Negate(BigInteger.One)), Expression.Constant(BigInteger.Negate(BigInteger.One))));
            expressions.Add(GenAreEqual(Utils.Constant(BigInteger.Zero), Utils.Constant(BigInteger.Zero)));
            expressions.Add(GenAreEqual(Utils.Constant(BigInteger.One), Utils.Constant(BigInteger.One)));

            //string
            //@TODO - include globalization

            //bool
            expressions.Add(GenAreEqual(Expr.Constant(true), Expr.Constant(true)));
            expressions.Add(GenAreEqual(Expr.Constant(false), Expr.Constant(false)));

            //Missing
            expressions.Add(GenAreEqual(Expr.Constant(Missing.Value), Expr.Constant(Missing.Value)));

            //Enum
            expressions.Add(GenAreEqual(Expr.Constant(Int32Enum.One), Expr.Constant(Int32Enum.One)));
            expressions.Add(GenAreEqual(Expr.Constant(Int64Enum.One), Expr.Constant(Int64Enum.One)));
            expressions.Add(GenAreEqual(Expr.Constant(Int16Enum.One), Expr.Constant(Int16Enum.One)));
            expressions.Add(GenAreEqual(Expr.Constant(UInt32Enum.One), Expr.Constant(UInt32Enum.One)));
            expressions.Add(GenAreEqual(Expr.Constant(UInt64Enum.One), Expr.Constant(UInt64Enum.One)));
            expressions.Add(GenAreEqual(Expr.Constant(SByteEnum.One), Expr.Constant(SByteEnum.One)));
            expressions.Add(GenAreEqual(Expr.Constant(UInt16Enum.One), Expr.Constant(UInt16Enum.One)));
            expressions.Add(GenAreEqual(Expr.Constant(ByteEnum.One), Expr.Constant(ByteEnum.One)));

            //uint
            //char
            //byte
            //sbyte
            //short
            //ushort
            //ulong
            //Type
            //RuntimeTypeHandle
            //All other types cause a NotImplementedException

            //@TODO - All the other types and negative cases

            return EU.BlockVoid(expressions);
        }
#if TODO // obsolete: code context, old action, global variables

        [TestAttribute]
        private static Expression Test_ContinueStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            //Test continue in a DoStatement
            //Set up a counter we'll use in the loop
            ParameterExpression var1 = TestScope.Current.HiddenVariable(typeof(Int32), "test_continuestatement_temp1");
            expressions.Add(Expr.Assign(var1, Expr.Constant(0)));
            ParameterExpression var2 = TestScope.Current.HiddenVariable(typeof(Int32), "test_continuestatement_temp2");
            expressions.Add(Expr.Assign(var2, Expr.Constant(0)));

            //Define the loop test and create the loop
            //var1 = 0
            //var2 = 0
            //for (;;) {
            //    var1 = var1 + 1
            //    if var2==2 {
            //        goto continue;
            //    }
            //    var2 = var2 + 1
            // continue:
            //    if (var1 <= 5) {
            //    } else {
            //        break;
            //    }
            // }
            LabelTarget @continue = Expr.Label();
            LabelTarget @break = Expr.Label();

            expressions.Add(
                TestSpan.GetDebugInfoForFrame(
                    Expression.Loop(
                        EU.BlockVoid(
                            ts.GenIncrement(var1),
                            Utils.If(Expr.Equal(Expr.Constant(2), var2),
                                TestSpan.GetDebugInfoForFrame(Expression.Continue(@continue))
                            ),
                            ts.GenIncrement(var2),
                            Expression.Label(@continue),
                            Expression.Condition(
                                LessThanEquals(ts, var1, Expr.Constant(5)),
                                Expression.Empty(),
                                Expression.Break(@break)
                            )
                        ),
                        @break,
                        null
                    )
                )
            );

            //Verify var1==6
            expressions.Add(GenAreEqual(var1, Expr.Constant(6)));

            //Verify var2==2
            expressions.Add(GenAreEqual(var2, Expr.Constant(2)));

            //@TODO - Test in LoopStatement
            //@TODO - Execute test
            //@TODO - Test in various other interesting blocks, especially try/catch/finally blocks
            //@TODO - Test not in a loop
            return EU.BlockVoid(expressions);
        }
       
        [TestAttribute]
        private static Expression Test_DoStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            //Set up a counter we'll use in the loop
            ParameterExpression var1 = TestScope.Current.HiddenVariable(typeof(Int32), "test_dostatement_temp1");
            expressions.Add(Expr.Assign(var1, Expr.Constant(0)));

            //Define the loop test and create the loop
            // do {
            //    var1 += 1
            // } while (var1 <= 5)
            LabelTarget @break = Expression.Label();
            expressions.Add(
                TestSpan.GetDebugInfoForFrame(
                    Expression.Loop(
                        EU.BlockVoid(
                            ts.GenIncrement(var1),
                            Expression.Condition(
                                LessThanEquals(ts, var1, Expr.Constant(5)),
                                Expression.Empty(),
                                Expression.Break(@break)
                            )
                        ),
                        @break,
                        null
                    )
                )
            );

            //Verify the loop executed the right amount of times
            expressions.Add(GenAreEqual(var1, Expr.Constant(6)));

            //@TODO - Execute() coverage

            return EU.BlockVoid(expressions);
        }
#endif
        [TestAttribute]
        private static Expression Test_EmptyStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(Expr.Empty());
            expressions.Add(TestSpan.GetDebugInfoForFrame(Expr.Empty()));
            //@TODO - Empty statement in different block contexts
            return EU.BlockVoid(expressions);
        }

#if TODO // global variables
        [TestAttribute]
        private static Expression Test_LambdaInference(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            Expression var = Utils.GlobalVariable(typeof(object), "test_lambdainference_temp0");

            // infer Action    (no args, no returns)
            Expression ba0 = EU.BlockVoid(Utils.Assign(var, Expr.Constant("Action", typeof(object))));
            LambdaExpression lambda0 = Expr.Lambda(ba0, "inf_action0", new ParameterExpression[0]);
            MethodCallExpression call0 = Expr.Call(lambda0, typeof(Action).GetMethod("Invoke"), new Expression[0]);
            expressions.Add(call0);
            expressions.Add(GenAreEqual(Expr.Constant("Action", typeof(object)), var));

            // infer Action<object>    (1 arg, no returns)
            ParameterExpression param = Expr.Parameter(typeof(object), "par1");
            Expression assignParam = EU.BlockVoid(Utils.Assign(var, param));
            LambdaExpression lambda1 = Expr.Lambda(assignParam, "inf_action1", new ParameterExpression[] { param });
            Expression arg1 = Expr.Constant("param0", typeof(object));
            MethodCallExpression call1 = Expr.Call(lambda1, typeof(Action<object>).GetMethod("Invoke"), new Expression[] { arg1 });
            expressions.Add(call1);
            expressions.Add(GenAreEqual(Expr.Constant("param0", typeof(object)), var));


            // infer Func<object>    (no args, returns object)
            Expression expr2 = Expr.Constant("Func", typeof(object));
            LambdaExpression lambda2 = Expr.Lambda(typeof(Func<Object>), expr2, "inf_function2", new ParameterExpression[0]);
            MethodCallExpression call2 = Expr.Call(lambda2, typeof(Func<object>).GetMethod("Invoke"), new Expression[0]);
            expressions.Add(Utils.Assign(var, call2));
            expressions.Add(GenAreEqual(Expr.Constant("Func", typeof(object)), var));

            // infer Func<object, object, object>    (2 args, returns object)
            ParameterExpression param3 = Expr.Parameter(typeof(object), "par3");
            ParameterExpression param3a = Expr.Parameter(typeof(object), "par3a");
            Expression returnParam3 = param3a;
            LambdaExpression lambda3 = Expr.Lambda(returnParam3, "inf_function3", new ParameterExpression[] { param3, param3a });
            Expression arg3 = Expr.Constant("par3val", typeof(object));
            Expression arg3a = Expr.Constant("par3aval", typeof(object));
            MethodCallExpression call3 = Expr.Call(lambda3, typeof(Func<object, object, object>).GetMethod("Invoke"), new Expression[] { arg3, arg3a });
            expressions.Add(Utils.Assign(var, call3));
            expressions.Add(GenAreEqual(Expr.Constant("par3aval", typeof(object)), var));
            return EU.BlockVoid(expressions);
        }
#endif

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_EnvironmentExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_ExpressionStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_IfStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(Utils.IfThenElse(
                Expr.Constant(true),
                Expr.Empty(),
                GenThrow("fail")));
            expressions.Add(Utils.IfThenElse(
                Expr.Constant(false),
                GenThrow("fail"),
                Expr.Empty()));
            //@TODO - More complete coverage
            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_IfStatementTest(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            AssertExceptionThrown<ArgumentNullException>(delegate() { IfStatementTest t = AstUtils.IfCondition(null, Expr.Empty()); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { IfStatementTest t = AstUtils.IfCondition(Expr.Constant(true), null); });

            Expression ifs = Utils.If(Expr.Constant(true), Expr.Empty()).Else(GenThrow("fail"));

            expressions.Add(ifs);

            ifs = Utils.If(Expr.Constant(true),
                        Expr.Empty()
                    ).Else(
                        Expr.Empty()
                    );

            expressions.Add(ifs);

            ifs = Utils.If(Expr.Constant(true),
                        Expr.Empty()
                    ).ElseIf(Expr.Constant(false),
                        GenThrow("fail")
                    ).Else(
                        Expr.Empty()
                    );

            expressions.Add(ifs);

            ifs = Utils.If(Expr.Constant(false),
                        GenThrow("fail")
                    ).ElseIf(Expr.Constant(true),
                        Expr.Empty()
                    ).Else(
                        Expr.Empty()
                    );

            expressions.Add(ifs);

            ifs = Utils.If(Expr.Constant(true),
                        Expr.Empty()
                    ).ElseIf(Expr.Constant(false),
                        GenThrow("fail")
                    ).ElseIf(Expr.Constant(true),
                        GenThrow("fail")
                    ).Else(
                        Expr.Empty()
                    );

            expressions.Add(ifs);

            ifs = Utils.If(Expr.Constant(false),
                        GenThrow("fail")
                    ).ElseIf(Expr.Constant(true),
                        Expr.Empty()
                    ).ElseIf(Expr.Constant(true),
                        GenThrow("fail")
                    ).Else(
                        Expr.Empty()
                    );

            expressions.Add(ifs);

            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_IndexAssignment(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            //Basic negative cases

            //Properties and static helpers

            //@TODO - More complete coverage
            expressions.Add(Expr.Empty());
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_LabeledStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }
#if TODO // obsolete: code context, old action, global variables

        [TestAttribute]
        private static Expression Test_LoopStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            //Regression check for bug 230214
            LoopExpression loop = AstUtils.Loop(Expr.Constant(true), Expr.Constant(3), Expr.Empty(), Expr.Empty());

            //var1=0
            //var2=5
            //for (var1; var1<=5; var1=var1+1)
            //    var2 = var2+1
            //verify(var1==6)
            //verify(var2==11)

            //Set up a counter we'll use in the loop
            ParameterExpression var1 = TestScope.Current.HiddenVariable(typeof(Int32), "test_loopstatement_temp1");
            ParameterExpression var2 = TestScope.Current.HiddenVariable(typeof(Int32), "test_loopstatement_temp2");
            expressions.Add(Expr.Assign(var1, Expr.Constant(0)));
            expressions.Add(Expr.Assign(var2, Expr.Constant(5)));

            //Define the loop test and create the loop
            Expression test1 = LessThanEquals(ts, var1, Expr.Constant(5));
            Expression ls = TestSpan.GetDebugInfoForFrame(AstUtils.Loop(test1, ts.GenIncrement(var1), ts.GenIncrement(var2), Expr.Empty(), null, null));
            expressions.Add(ls);

            //Verify the loop executed the right amount of times
            expressions.Add(GenAreEqual(var1, Expr.Constant(6)));
            expressions.Add(GenAreEqual(var2, Expr.Constant(11)));

            //@TODO - Better else coverage
            //@TODO - Execute() coverage
            return EU.BlockVoid(expressions);
        }
#endif
        [TestAttribute]
        private static Expression Test_MemberAssignment(TestScenarios ts) {
            //Test_MemberExpression below hits most of MemberAssignment
            //@TODO - Fill in whatever isn't covered
            return Test_MemberExpression(ts);
        }

        [TestAttribute]
        private static Expression Test_MemberExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            //Init an instance with members we can reference
            ParameterExpression var = TestScope.Current.HiddenVariable(typeof(Reference), "test_memberexpression_temp");
            //reference = new Reference()
            expressions.Add(Expr.Assign(var, Expr.New(typeof(Reference).GetConstructor(new Type[] { }))));

            //Gather MemberInfos we'll use in testing
            MemberInfo field = typeof(Reference).GetField("publicIntField");
            MemberInfo property = typeof(Reference).GetProperty("publicIntProperty");
            MemberInfo method = typeof(Reference).GetMethod("Method");

            //reference.publicIntField=0
            expressions.Add(Expr.Assign(Expr.Field(var, (FieldInfo)field), Expr.Constant(0)));
            //reference.publicIntProperty=0
            expressions.Add(Expr.Assign(Expr.Property(var, (PropertyInfo)property), Expr.Constant(0)));
            //reference.publicIntField=5
            expressions.Add(Expr.Assign(Expr.Field(var, (FieldInfo)field), Expr.Constant(5)));
            //reference.publicIntProperty=50
            expressions.Add(Expr.Assign(Expr.Property(var, (PropertyInfo)property), Expr.Constant(50)));

            ////////////////////////////////////////
            //Do that all again with a value class
            ////////////////////////////////////////

            ParameterExpression value = TestScope.Current.HiddenVariable(typeof(Value), "test_memberexpression_temp2");
            //value = new Value()
            expressions.Add(
                Expr.Assign(
                    value,
                    Expr.Call(
                        typeof(TestScenarios).GetMethod("GetValueClass")
                    )
                )
            );
            var = value;

            //Gather MemberInfos we'll use in testing
            field = typeof(Value).GetField("publicIntField");
            property = typeof(Value).GetProperty("publicIntProperty");
            method = typeof(Value).GetMethod("Method");
            FieldInfo staticField = typeof(Value).GetField("publicStaticIntField");
            PropertyInfo staticProperty = typeof(Value).GetProperty("publicStaticIntProperty");

            //reference.publicIntField=0
            expressions.Add(Expr.Assign(Expr.Field(var, (FieldInfo)field), Expr.Constant(0)));
            //reference.publicIntProperty=0
            expressions.Add(Expr.Assign(Expr.Property(var, (PropertyInfo)property), Expr.Constant(0)));
            //reference.publicIntField=5
            expressions.Add(Expr.Assign(Expr.Field(var, (FieldInfo)field), Expr.Constant(5)));
            //reference.publicIntProperty=50
            expressions.Add(Expr.Assign(Expr.Property(var, (PropertyInfo)property), Expr.Constant(50)));

            //Field()
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Field(var, (FieldInfo)null); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Field(null, (FieldInfo)field); });
            //Reference.publicStaticIntField = 0
            expressions.Add(Expr.Assign(Expr.Field(null, staticField), Expr.Constant(0)));
            //0==Reference.publicStaticIntField
            expressions.Add(GenAreEqual(Expr.Constant(0), Expr.Field(null, staticField)));
            //Reference.publicStaticIntField = 60
            expressions.Add(Expr.Assign(Expr.Field(null, staticField), Expr.Constant(60)));
            //60==Reference.publicStaticIntField
            expressions.Add(GenAreEqual(Expr.Constant(60), Expr.Field(null, staticField)));

            //Property()
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.Property(var, (PropertyInfo)null); });
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.Property(null, (PropertyInfo)property); });
            //Reference.publicStaticIntProperty = 0
            expressions.Add(Expr.Assign(Expr.Property(null, staticProperty), Expr.Constant(0)));
            //0==Reference.publicStaticIntProperty
            expressions.Add(GenAreEqual(Expr.Constant(0), Expr.Property(null, staticProperty)));
            //Reference.publicStaticIntProperty = 60
            expressions.Add(Expr.Assign(Expr.Property(null, staticProperty), Expr.Constant(60)));
            //60==Reference.publicStaticIntProperty
            expressions.Add(GenAreEqual(Expr.Constant(60), Expr.Property(null, staticProperty)));

            //@TODO - Evaluate cases and extended varieties of members and types
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_MethodCallExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            //@TODO - Order of evaluation verification
            //@TODO - Test many types of method targets (generic, static, instance, variable args, etc.)
            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_InvocationExpression(TestScenarios ts) {
            // Test InvocationExpression with an inlined lambda

            // (int expr, int y) => expr + y
            LambdaBuilder lambda = MakeLambdaBuilder(typeof(int), "test_InvocationExpression1_foo");
            ParameterExpression x = lambda.Parameter(typeof(int), "x");
            ParameterExpression y = lambda.Parameter(typeof(int), "y");
            lambda.Body = Expr.Add(x, y);

            // Invoke((int expr, int y) => expr + y, 123, 444)
            Expression invoke = Expr.Invoke(lambda.MakeLambda(), Expr.Constant(123), Expr.Constant(444));

            ParameterExpression z = TestScope.Current.HiddenVariable(typeof(int), "test_InvocationExpression1_z");

            return EU.BlockVoid(GenAreEqual(invoke, Expr.Constant(567)));
        }

        [TestAttribute]
        private static Expression Test_NewArrayExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.NewArrayInit(null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.NewArrayInit(typeof(int), null); });

            ConstantExpression three = Expr.Constant(3);
            NewArrayExpression nae = Expr.NewArrayInit(typeof(int), three);
            AreEqual(1, nae.Expressions.Count);
            AreEqual(three, nae.Expressions[0]);

            //@TODO - More complete testing
            expressions.Add(Expr.Empty());
            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_NewExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            ConstructorInfo ci = typeof(StringBuilder).GetConstructor(new Type[] { typeof(string) });
            ConstantExpression str = Expr.Constant("str");
            NewExpression ne = Expr.New(ci, str);
            AreEqual(ci, ne.Constructor);
            AreEqual(1, ne.Arguments.Count);
            AreEqual(str, ne.Arguments[0]);
            //@TODO - More complete coverage
            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_NewExpression_NoArgs(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            ConstantExpression zero = Expr.Constant(0);
            ConstantExpression message = Expr.Constant("Exception of type 'System.Exception' was thrown.");

            //verify ref type
            ParameterExpression var0 = TestScope.Current.HiddenVariable(typeof(Exception), "new_expr_noargs0");
            expressions.Add(Expr.Assign(var0, Expr.New(typeof(Exception))));
            expressions.Add(GenAreEqual(message, Expr.Property(var0, typeof(Exception), "Message")));

            //verify value type
            ParameterExpression var1 = TestScope.Current.HiddenVariable(typeof(Int32), "new_expr_noargs1");
            expressions.Add(Expr.Assign(var1, Expr.New(typeof(Int32))));
            expressions.Add(GenAreEqual(zero, var1));


            //Negative construction cases
            AssertExceptionThrown<ArgumentNullException>(delegate() { Expr.New((ConstructorInfo)null); });
            //no default constructor
            AssertExceptionThrown<ArgumentException>(delegate() { Expr.New(typeof(Action)); });

            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_CoalesceFalse(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            Expression cTrue = Expr.Constant(true);
            Expression cFalse = Expr.Constant(false);
            Expression one = Expr.Constant(1);

            Expression cTrueObject = Expr.Convert(Expr.Constant(true), typeof(object));
            Expression cFalseObject = Expr.Convert(Expr.Constant(false), typeof(object));
            Expression oneObject = Expr.Convert(Expr.Constant(1), typeof(object));

            MethodInfo isTrue = new Func<object, bool>(IsTrue).Method;
            MethodInfo isTrueBool = ((Func<bool, bool>)IsTrueBool).Method;

            //Cover basic negative cases
            AssertExceptionThrown<ArgumentNullException>(delegate() { AstUtils.CoalesceFalse(TestScope.Current.Block, null, cTrue, isTrue); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { AstUtils.CoalesceFalse(TestScope.Current.Block, cFalse, null, isTrue); });

            //Basic positive cases
            //false || true
            expressions.Add(GenAreEqual(cTrue, AstUtils.CoalesceFalse(TestScope.Current.Block, cFalse, cTrue, isTrueBool)));

            //true || false
            expressions.Add(GenAreEqual(cTrue, AstUtils.CoalesceFalse(TestScope.Current.Block, cTrue, cFalse, isTrueBool)));

            //false || false
            expressions.Add(GenAreEqual(cFalse, AstUtils.CoalesceFalse(TestScope.Current.Block, cFalse, cFalse, isTrueBool)));

            //true || true
            expressions.Add(GenAreEqual(cTrue, AstUtils.CoalesceFalse(TestScope.Current.Block, cTrue, cTrue, isTrueBool)));

            //Confirm early exit fo the (true or X) case
            Expression ae = AstUtils.CoalesceFalse(
                TestScope.Current.Block,
                cTrue,
                Expr.Call(
                    typeof(TestScenarios).GetMethod("Throw"),
                    Expr.Constant("Did not early exit in a (true or X) expression")
                ),
                isTrueBool
            );

            expressions.Add(GenAreEqual(cTrue, ae)); //Should not throw

            //true || 1
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceFalse(TestScope.Current.Block, cTrue, one, isTrue); });
            //false || 1
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceFalse(TestScope.Current.Block, cFalse, one, isTrue); });
            //1 || true
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceFalse(TestScope.Current.Block, one, cTrue, isTrue); });
            //true || null
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceFalse(TestScope.Current.Block, cTrue, Expr.Constant(null), isTrue); });
            //false || null
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceFalse(TestScope.Current.Block, cFalse, Expr.Constant(null), isTrue); });
            //null || true
            AssertExceptionThrown<ArgumentException>(delegate() { AstUtils.CoalesceFalse(TestScope.Current.Block, Expr.Constant(null), cTrue, isTrue); });

            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_ParamsExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_ReturnStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            //Partially covered by some of the DynamicSites tests
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_ScopeExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_Statement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_UnaryExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            ConstantExpression operand;
            UnaryExpression se;

            // Coerce from value type to value type (double to int)
            // TODO: all implicit/explicit numeric conversions
            operand = Expr.Constant(3.4);
            se = Expr.Convert(operand, typeof(Int32));
            AreEqual(operand, se.Operand); //Hit the property real quick while we have one
            expressions.Add(GenAreEqual(Expr.Constant(3), se));

            ////@TODO - Coerce to Nullable<>

            // Cast float to int
            operand = Expr.Constant((float)17.3);
            se = Expr.Convert(operand, typeof(Int32));
            expressions.Add(GenAreEqual(Expr.Constant(17), se));

            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_ThrowStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            ConstantExpression one = Expr.Constant(1);
            AssertExceptionThrown<ArgumentException>(delegate() { UnaryExpression te = Expr.Throw(one); });
            expressions.Add(Test_CatchBlock(ts)); //Some is covered by this
            //@TODO - Although this is used many other places it would be good to have more complete coverage here.
            //@TODO - throw from various contexts (try/catch/finally, method/generator method/global, etc.)
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_TryStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_TypeBinaryExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();

            ConstantExpression cTrue = Expr.Constant(true);
            ConstantExpression cFalse = Expr.Constant(false);

            //5.isinstanceof(int32)
            TypeBinaryExpression tbe = Expr.TypeIs(Expr.Constant(5), typeof(Int32));
            expressions.Add(GenAreEqual(cTrue, tbe));

            //5.isinstanceof(valuetype)
            tbe = Expr.TypeIs(Expr.Constant(5), typeof(ValueType));
            expressions.Add(GenAreEqual(cTrue, tbe));

            //!5.isinstanceof(int64)
            tbe = Expr.TypeIs(Expr.Constant(5), typeof(Int64));
            expressions.Add(GenAreEqual(cFalse, tbe));

            //!5.isinstanceof(string)
            tbe = Expr.TypeIs(Expr.Constant(5), typeof(String));
            expressions.Add(GenAreEqual(cFalse, tbe));

            //"Blue".isinstanceof(string)
            tbe = Expr.TypeIs(Expr.Constant("Blue"), typeof(String));
            expressions.Add(GenAreEqual(cTrue, tbe));

            //"Blue".isinstanceof(object)
            tbe = Expr.TypeIs(Expr.Constant("Blue"), typeof(Object));
            expressions.Add(GenAreEqual(cTrue, tbe));

            //!"Blue".isinstanceof(TestScenarios)
            tbe = Expr.TypeIs(Expr.Constant("Blue"), typeof(TestScenarios));
            expressions.Add(GenAreEqual(cFalse, tbe));

            //!"Blue".isinstanceof(int32)
            tbe = Expr.TypeIs(Expr.Constant("Blue"), typeof(Int32));
            expressions.Add(GenAreEqual(cFalse, tbe));

            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_UnboundAssignment(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_UnboundExpression(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute()]
        private static Expression Test_Variable(TestScenarios ts) {
            /*
             * Declaration of local/Globals variables
             * Assignment
             * Reading values
             */
            LambdaBuilder Test1 = MakeLambdaBuilder(typeof(int), "VariableDeclarations");

            List<Expression> expressions = new List<Expression>();

            System.Collections.Generic.List<KeyValuePair<Type, object>> TypeList = new System.Collections.Generic.List<KeyValuePair<Type, object>>();
            TypeList.Add(new KeyValuePair<Type, object>(typeof(byte), (byte)5));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(sbyte), (sbyte)6));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(short), (short)7));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(Decimal), (Decimal)9));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(String), "Test"));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(String), ""));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(String), null));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(Double), (Double)0.77));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(char), (char)'A'));

            int SI = 1;
            foreach (KeyValuePair<Type, object> varType in TypeList) {
                Expression Local = Test1.Variable(varType.Key, (SI++).ToString());
                expressions.Add(Expr.Assign(Local, Expr.Constant(varType.Value, varType.Key)));
                //Read back and compare
                expressions.Add(GenAreEqual(Expr.Constant(varType.Value, varType.Key), Local));
            }

            //Add two variables with the same name
            //The id isn't really used by the compiler, so this is ok.
            Expression Local1 = Test1.Variable(typeof(int), SI.ToString());
            Expression Local2 = Test1.Variable(typeof(int), SI.ToString());
            expressions.Add(Local1);
            expressions.Add(Local2);
            //Add to one, read from the other
            expressions.Add(Expr.Assign(Local1, Expr.Constant(5)));
            expressions.Add(Expr.Assign(Local2, Expr.Constant(6)));
            expressions.Add(GenAreEqual(Expr.Constant(5), Local1));
            expressions.Add(GenAreEqual(Expr.Constant(6), Local2));

            Test1.Body = EU.BlockVoid(expressions);

            //Temp variable
            //add one in a lambda, assign to it.
            //Read it back.
            LambdaBuilder Test3 = MakeLambdaBuilder(typeof(object), "GlobalVariableDeclarations2");
            expressions.Clear();

            ParameterExpression Temp1 = Test3.HiddenVariable(typeof(int), (SI++).ToString());
            expressions.Add(Expr.Assign(Temp1, Expr.Constant(15)));

            expressions.Add(GenAreEqual(Expr.Constant(15), Temp1));

            Test3.Body = EU.BlockVoid(expressions);

            return EU.BlockVoid(Expr.Invoke(Test1.MakeLambda(), new Expression[] { }), Expr.Invoke(Test3.MakeLambda(), new Expression[] { }));
        }

        [TestAttribute()]
        private static Expression Test_New_Variable(TestScenarios ts) {
            /*
             * Declaration of local/Globals variables
             * Assignment 
             * Reading values
             */
            LambdaBuilder Test1 = MakeLambdaBuilder(typeof(void), "VariableDeclarations");

            List<Expression> expressions = new List<Expression>();

            System.Collections.Generic.List<KeyValuePair<Type, object>> TypeList = new System.Collections.Generic.List<KeyValuePair<Type, object>>();
            TypeList.Add(new KeyValuePair<Type, object>(typeof(byte), (byte)5));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(sbyte), (sbyte)6));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(short), (short)7));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(Decimal), (Decimal)9));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(String), "Test"));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(String), ""));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(String), null));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(Double), (Double)0.77));
            TypeList.Add(new KeyValuePair<Type, object>(typeof(char), (char)'A'));

            List<ParameterExpression> Test1Variables = new List<ParameterExpression>();
            int SI = 1;
            foreach (KeyValuePair<Type, object> varType in TypeList) {
                ParameterExpression Local = Expression.Variable(varType.Key, "Local" + SI.ToString());
                Test1Variables.Add(Local);
                expressions.Add(Expr.Assign(Local, Expr.Constant(varType.Value, varType.Key)));
                //Read back and compare
                expressions.Add(GenAreEqual(Expr.Constant(varType.Value, varType.Key), Local));

                //TODO: fix usage of globals.
                //Globals are undergoing a re-write. 
                //In any case, it's unlikely that it makes sense
                //to add them to a scope.
                /*ParameterExpression Global = Expr.Global(varType.Key, "Global" + SI.ToString());
                Test1Variables.Add(Global);
                expressions.Add(Expr.Assign(Global, Expr.Constant(varType.Value, varType.Key)));
                //Read back and compare
                expressions.Add(GenAreEqual(Expr.Constant(varType.Value, varType.Key), Global));*/

                ParameterExpression Temp = Expression.Variable(varType.Key, "Temporary" + SI.ToString());
                Test1Variables.Add(Temp);
                expressions.Add(Expr.Assign(Temp, Expr.Constant(varType.Value, varType.Key)));
                //Read back and compare
                expressions.Add(GenAreEqual(Expr.Constant(varType.Value, varType.Key), Temp));

                SI += 1;
            }

            Test1.Body = EU.BlockVoid(expressions);

            /*
             * Using a local variable on two nested levels of lambdas.             
             */
            LambdaBuilder Test2 = MakeLambdaBuilder(typeof(void), "Levels of variables, nested");
            LambdaBuilder Test2Child1 = MakeLambdaBuilder(typeof(void), "Levels of variables, Child 1");
            expressions.Clear();
            //Declare variable
            ParameterExpression GenLocal1 = Expression.Variable(typeof(int), "GenLocal1");
            //Assign to it on nested lambda
            expressions.Add(Expr.Assign(GenLocal1, Expr.Constant(5)));
            Test2Child1.Body = EU.BlockVoid(expressions);

            //read variable on the root lambda
            expressions.Clear();
            expressions.Add(Expr.Invoke(Test2Child1.MakeLambda(), new Expression[] { }));
            expressions.Add(GenAreEqual(Expr.Constant(5), GenLocal1));
            Test2.Body = EU.BlockVoid(new ParameterExpression[] { GenLocal1 }, expressions);

            return EU.BlockVoid(
                Expr.Block(Test1Variables, Expr.Invoke(Test1.MakeLambda(), new Expression[] { })),
                Expr.Invoke(Test2.MakeLambda(), new Expression[] { })
            );
        }

        [TestAttribute(TestState.Disabled, "Doesn't make sense without Expression.Local TODO: remove or make this test positive")]
        private static Expression Test_New_Variable2(TestScenarios ts) {
            /* 
             * Using a local variable on two parallel levels of lambdas.
             */
            //Now with a second same level lambda in parallel to the first one, that reads the local variable value.
            //This is illegal. need to figure out how to check for compiler errors.

            List<Expression> expressions = new List<Expression>();

            ParameterExpression GenLocal2 = Expression.Variable(typeof(int), "GenLocal2");
            LambdaBuilder Test3 = MakeLambdaBuilder(typeof(object), "Levels of variables, parallel");
            LambdaBuilder Test3Child1 = MakeLambdaBuilder(typeof(object), "Levels of variables, Child 1");
            LambdaBuilder Test3Child2 = MakeLambdaBuilder(typeof(object), "Levels of variables, Child 2");

            expressions.Clear();
            expressions.Add(Expr.Assign(GenLocal2, Expr.Constant(6)));
            Test3Child1.Body = EU.BlockVoid(expressions);


            expressions.Clear();
            expressions.Add(GenAreEqual(Expr.Constant(6), GenLocal2));
            Test3Child2.Body = EU.BlockVoid(expressions);

            //and set the code on the root lambda to just call each lambda.
            expressions.Clear();
            expressions.Add(Expr.Invoke(Test3Child1.MakeLambda(), new Expression[] { }));
            expressions.Add(Expr.Invoke(Test3Child2.MakeLambda(), new Expression[] { }));

            Test3.Body = EU.BlockVoid(expressions);

            return Test3.MakeLambda();
        }




        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_VariableReference(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_YieldStatement(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute(TestState.Disabled, "@TODO")]
        private static Expression Test_YieldTarget(TestScenarios ts) {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(GenThrow("@TODO"));
            return EU.BlockVoid(expressions);
        }

        [TestAttribute]
        private static Expression Test_ClosureOverTemps1(TestScenarios ts) {

            // Test a simple closure over a temp
            ParameterExpression temp = Expression.Variable(typeof(int), "$liftedTemp");
            return GenAreEqual(
                Expression.Invoke(
                    Expression.Lambda(
                        typeof(Func<int>),
                        Expression.Block(
                            new ParameterExpression[] { temp },
                            Expression.Assign(temp, Expression.Constant(123)),
                            Expression.Invoke(
                                Expression.Lambda(
                                    Expression.Assign(temp, Expression.Add(temp, Expression.Constant(444))),
                                    "Test_ClosureOverTemps_inner_lambda",
                                    new ParameterExpression[0]
                                )
                            ),
                            temp
                        ),
                        "Test_ClosureOverTemps_outer_lambda",
                        new ParameterExpression[0]
                    )
                ),
                Expression.Constant(567)
            );
        }
        #endregion

        #region Runtime value helpers

        /// <summary>
        /// Helper to generate code at runtime that creates a new Extensible of int.
        /// </summary>
        private static Expression MakeExtensibleIntExpression(int value) {
            return Expr.Call(typeof(TestScenarios).GetMethod("GetExtensibleInt"), Expr.Constant(value));
        }

        /// <summary>
        /// Helper to create a new Extensible of int at runtime
        /// </summary>
        public static Extensible<int> GetExtensibleInt(int value) {
            return new Extensible<int>(value);
        }

        public static Nullable<int> GetNullableInt() {
            return new Nullable<int>();
        }

        /// <summary>
        /// Helper to generate code at runtime that creates a new Extensible of int.
        /// </summary>
        private static Expression MakeDecimalExpression(int value) {
            return Expr.Call(typeof(TestScenarios).GetMethod("GetDecimal"), Expr.Constant(value));
        }

        /// <summary>
        /// Helper to create a new Extensible of int at runtime
        /// </summary>
        public static decimal GetDecimal(int value) {
            return new decimal(value);
        }

        private static Expression MakeNullableIntExpression(int? value) {
            if (value.HasValue) {
                return Expr.New(typeof(int?).GetConstructor(new Type[] { typeof(int) }), Expr.Constant(value));
            } else {
                return Expr.Call(typeof(TestScenarios).GetMethod("GetNullableInt"));
            }
        }
#if TODO // obsolete: code context, old action, global variables
        private static Expression LessThanEquals(TestScenarios ts, Expression left, Expression right) {
            return Expression.Dynamic(OldDoOperationAction.Make(ts.Binder, Operators.LessThanOrEqual), typeof(bool), Utils.CodeContext(), left, right);
        }
#endif
        #endregion

        public static void PrintDisabled() {
            MethodInfo[] Methods = typeof(TestScenarios).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
            foreach (MethodInfo mi in Methods) {
                if (TestAttribute.IsTest(mi) && !TestAttribute.IsEnabled(mi)) {
                    TestAttribute.PrintDisabled(mi);
                }
            }
        }

        internal static void RunNegative(TestEngineOptions Options) {

            List<MethodInfo> tests = new List<MethodInfo>();


            if (Options.RunTests.Length > 0) {
                foreach (string testName in Options.RunTests) {
                    tests.Add(GetTest(testName));
                }

            } else {
                tests.AddRange(GetAllTests());
            }
            if (Options.SkipTests.Length > 0) {
                foreach (string testName in Options.SkipTests) {
                    tests.Remove(GetTest(testName));
                }
            }


            String Result = "PASS";
            foreach (MethodInfo mi in tests) {
                //get expected exception
                var attr = ETUtils.TestAttribute.GetAttribute(mi);
                if (attr == null)
                    continue;
                bool isNegative = (attr.KeyWords[0] == "negative") ? true : false;

                // for ignoring tests which need different permissions to run correctly in TestAst (they work in AstTest)
                bool requiresPartialTrust = false;
                foreach (string s in attr.KeyWords) {
                    if (s == "PartialTrustOnly")
                        requiresPartialTrust = true;
                }

                if (ETUtils.TestAttribute.IsTest(mi) && ETUtils.TestAttribute.IsEnabled(mi) && isNegative && !requiresPartialTrust) {    
                    //if no priority is specfied, the default is pri1 (the default for the attribute also.)
                    if (Options.PriorityOfTests.Length > 0) {
                        bool validPri = false;
                        foreach (int p in Options.PriorityOfTests) {
                            if (p == attr.Priority) {
                                validPri = true;
                                break;
                            }
                        }
                        if (!validPri) continue;
                    }



                    Console.WriteLine("\n[Testing '{0}']", mi.Name);

                    Type Expected = attr.Exception;
                    Expression Expr;

                    //get expression (can throw here)
                    //compile expression (can throw here)
                    //run compiled code (can throw here)
                    try {
                        //Expr = (Expression)mi.Invoke(null, BindingFlags.Static | BindingFlags.NonPublic, null, new object[] { null }, null);
                        //Expr = Expr.Call(mi, new Expression[] { });
                        
                        Expr = (Expression)mi.Invoke(null, new object[] { });
                        Result = "PASS";
                    } catch (Exception Ex) {
                        //Can be deeply nested.
                        while (Ex.InnerException != null) { Ex = Ex.InnerException; }
                        Console.WriteLine("[FAIL - '{0}'] <<<<<<<<<<<<<<<<<<<<<<<<<<<", mi.Name);
                        Console.WriteLine("Expected: {0} \nActual Result:", Expected.Name);
                        Console.WriteLine(Ex.ToString());
                        Result = "FAIL";
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("Done - " + Result);
            if (Result == "FAIL") throw new Exception("A negative testcases did not pass.");
        }

        public static void RunExternal(Assembly asm) {
            //Get all enabled positive tests from assembly
            List<MethodInfo> PosTests = ETUtils.TestAttribute.GetTests(asm, ETUtils.TestState.Enabled, null, new string[] { "positive" }, null, null);
            //Compile and run each test.
            String Result = "PASS";
            foreach (MethodInfo mi in PosTests) {
                Console.WriteLine("\n[Testing '{0}']", mi.Name);
                try {
                    Expr Expr = (Expr)mi.Invoke(null, new object[] {});
                    Console.WriteLine("[Pass - '{0}']", mi.Name);
                } catch (Exception ex) {
                    Console.WriteLine("[FAIL - '{0}'] <<<<<<<<<<<<<<<<<<<<<<<<<<<", mi.Name);
                    Console.WriteLine(ex.ToString());
                    Result = "FAIL";
                }
            }
            Console.WriteLine();
            Console.WriteLine("Done - " + Result);

            //Get all enabled negative tests from assembly
            List<MethodInfo> NegTests = ETUtils.TestAttribute.GetTests(asm, ETUtils.TestState.Enabled, null, new string[] { "negative" }, null, null);

            foreach (MethodInfo mi in PosTests) {
                Console.WriteLine("\n[Testing '{0}']", mi.Name);
                try {
                    Expr Expr = (Expr)mi.Invoke(null, new object[] {});
                    Result = "PASS";
                } catch (Exception ex) {
                    Console.WriteLine("[FAIL - '{0}'] <<<<<<<<<<<<<<<<<<<<<<<<<<<", mi.Name);
                    Console.WriteLine(ex.ToString());
                    Result = "FAIL";
                }
            }
            if (Result == "FAIL") throw new Exception("A testcase did not pass.");
            Console.WriteLine();
            Console.WriteLine("Done - " + Result);
        }

        private static Boolean OriginalExceptionCheck(Exception Ex, Type Expected) {
            while (Ex.InnerException != null) { Ex = Ex.InnerException; }

            return Ex.GetType() == Expected;
        }
    }
}
