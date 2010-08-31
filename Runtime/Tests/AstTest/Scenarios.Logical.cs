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

#if !SILVERLIGHT3
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {

    // Tests of the logical operator optimizations (== != && || ! typeis)
    // (We want to generate good IL for the common cases but not regress the
    // general behavior)
    public static partial class Scenarios {

        #region Slow_OptimizedLogical

        // Marked as slow so we get collectible delegates
        public static void Slow_OptimizedLogical() {
            var rng = new Random(0xCAFE);
            int n = 0x1000;
            for (int i = 0; i < n; i++) {
                var testExpr = MakeLogicalExpression(rng, 8);
                bool baseline = EvalLogicalExpression(testExpr);
                var func = Expression.Lambda<Func<bool>>(testExpr).Compile();
                bool result = func();
                EU.Equal(baseline, result);
            }
        }

        public static bool OptimizedLogical_GetTrue() { return true; }
        public static bool OptimizedLogical_GetFalse() { return false; }

        // Build an arbitrary logical expression:
        // Leaf nodes are:
        //   Constant(boolValue)
        //   Method returning boolValue
        //   Equal(boolValue, null comparison)
        //   NotEqual(boolValue, null comparison)
        // Non-leaf nodes are:
        //   AndAlso
        //   OrElse
        //   Conditional
        private static readonly Expression _OptimizedLogical_TrueConstant = Expression.Constant(true);
        private static readonly Expression _OptimizedLogical_FalseConstant = Expression.Constant(false);
        private static readonly Expression _OptimizedLogical_TrueMethod = Expression.Call(typeof(Scenarios).GetMethod("OptimizedLogical_GetTrue"));
        private static readonly Expression _OptimizedLogical_FalseMethod = Expression.Call(typeof(Scenarios).GetMethod("OptimizedLogical_GetFalse"));
        private static readonly Expression _OptimizedLogical_TrueEqual = Expression.Equal(Expression.Constant(null), Expression.Constant(null));
        private static readonly Expression _OptimizedLogical_FalseEqual = Expression.Equal(Expression.Constant(new object()), Expression.Constant(null));
        private static readonly Expression _OptimizedLogical_TrueNotEqual = Expression.NotEqual(Expression.Constant(new object()), Expression.Constant(null));
        private static readonly Expression _OptimizedLogical_FalseNotEqual = Expression.NotEqual(Expression.Constant(null), Expression.Constant(null));

        private static Expression MakeLogicalExpression(Random rng, int maxDepth) {
            if (rng.Next(4) == 0 || maxDepth == 1) {
                switch (rng.Next(8)) {
                    case 0: return _OptimizedLogical_TrueConstant;
                    case 1: return _OptimizedLogical_FalseConstant;
                    case 2: return _OptimizedLogical_TrueMethod;
                    case 3: return _OptimizedLogical_FalseMethod;
                    case 4: return _OptimizedLogical_TrueEqual;
                    case 5: return _OptimizedLogical_FalseEqual;
                    case 6: return _OptimizedLogical_TrueNotEqual;
                    case 7: return _OptimizedLogical_FalseNotEqual;
                }
            } else {
                maxDepth--;
                switch (rng.Next(4)) {
                    case 0:
                        return Expression.AndAlso(
                            MakeLogicalExpression(rng, maxDepth),
                            MakeLogicalExpression(rng, maxDepth)
                        );
                    case 1:
                        return Expression.OrElse(
                            MakeLogicalExpression(rng, maxDepth),
                            MakeLogicalExpression(rng, maxDepth)
                        );
                    case 2:
                        return Expression.Condition(
                            MakeLogicalExpression(rng, maxDepth),
                            MakeLogicalExpression(rng, maxDepth),
                            MakeLogicalExpression(rng, maxDepth)
                        );
                    case 3:
                        return Expression.Not(
                            MakeLogicalExpression(rng, maxDepth)
                        );
                }
            }
            throw new Exception("unreachable");
        }

        private static bool EvalLogicalExpression(Expression node) {
            switch (node.NodeType) {
                case ExpressionType.Constant:
                    return node == _OptimizedLogical_TrueConstant;
                case ExpressionType.Call:
                    return node == _OptimizedLogical_TrueMethod;
                case ExpressionType.Equal:
                    return node == _OptimizedLogical_TrueEqual;
                case ExpressionType.NotEqual:
                    return node == _OptimizedLogical_TrueNotEqual;
                case ExpressionType.AndAlso:
                    var andAlso = (BinaryExpression)node;
                    return EvalLogicalExpression(andAlso.Left) && EvalLogicalExpression(andAlso.Right);
                case ExpressionType.OrElse:
                    var orElse = (BinaryExpression)node;
                    return EvalLogicalExpression(orElse.Left) || EvalLogicalExpression(orElse.Right);
                case ExpressionType.Conditional:
                    var cond = (ConditionalExpression)node;
                    return EvalLogicalExpression(cond.Test) ? EvalLogicalExpression(cond.IfTrue) : EvalLogicalExpression(cond.IfFalse);
                case ExpressionType.Not:
                    var not = (UnaryExpression)node;
                    return !EvalLogicalExpression(not.Operand);
                default: throw new Exception("unreachable");
            }
        }

        #endregion

        #region Positive_OptimizedTypeIs

        public enum EnumInt16 : short {
            A, B, C
        }
        public enum EnumUInt16 : ushort {
            A, B, C
        }
        public enum EnumInt32 : int {
            A, B, C
        }
        public enum EnumUInt32 : uint {
            A, B, C
        }

//ICloneable is inaccessible in Silverlight and the ComputeTypeIs call below will cause us to iterate over it and fail.
#if !SILVERLIGHT
        // Compare TypeIs result for a whole bunch of types, it should be
        // equivalent to isinst
        //
        // Don't run this with SaveAssemblies because it compiles very many ETs
        // If run with SaveAssemblies on, it will run out of memory because
        // TypeBuilder's aren't collectible.
        public static void Positive_OptimizedTypeIs(EU.IValidator V) {
            var types = new List<Type> {
                typeof(Byte), typeof(SByte), typeof(Boolean),
                typeof(Int16), typeof(UInt16), typeof(Char),
                typeof(Int32), typeof(UInt32),
                typeof(Int64), typeof(UInt64),
                typeof(Single), typeof(Double), typeof(Decimal),
                typeof(DateTime),
                typeof(String),
                typeof(Object),
                typeof(DBNull),
                typeof(TimeSpan), // struct
                typeof(EnumInt16), typeof(EnumUInt16), typeof(EnumInt32), typeof(EnumUInt32),
                // special types
                typeof(ValueType), typeof(Enum), typeof(Array),
                typeof(IEnumerable), typeof(IComparable), typeof(IList),
            };

            var objs = new List<object>();
            objs.Add(null);
            foreach (Type t in types.ToArray()) {
                types.Add(t.MakeArrayType());
                if (t.IsValueType) {
                    types.Add(typeof(Nullable<>).MakeGenericType(t));
                }
                types.Add(typeof(IEnumerable<>).MakeGenericType(t));

                if (!t.IsInterface && !t.IsAbstract) {
                    object obj;
                    if (t == typeof(string)) {
                        obj = "Hello";
                    } else if (t == typeof(DBNull)) {
                        obj = DBNull.Value;
                    } else {
                        obj = Activator.CreateInstance(t);
                    }
                    objs.Add(obj);
                    objs.Add(Array.CreateInstance(t, 0));
                }
            }

            foreach (var obj in objs) {
                //Console.Write('.');
                foreach (var type in types) {
                    bool expected = ComputeIsinst(obj, type);

                    Type objType = obj != null ? obj.GetType() : type;

                    foreach (var manifestType in GetManifestTypes(objType)) {
                        if (obj == null && manifestType.IsValueType && Nullable.GetUnderlyingType(manifestType) == null) {
                            // Can't cast null to a value type
                            continue;
                        }

                        bool result = ComputeTypeIs(obj, type, manifestType);
                        if (expected != result) {
                            throw new InvalidOperationException(
                                string.Format(
                                    "Test failed: expected '{0}' (type: {1}) TypeIs {2} to equal {3}, but got {4} when manifest type is {5}",
                                    obj ?? "<null>", objType, type, expected, result, manifestType
                                )
                            );
                        }
                    }
                }
            }
        }
#endif

        private static bool ComputeTypeIs(object obj, Type type, Type manifestType) {
            var arg = Expression.Parameter(typeof(object), "arg");
            var test = Expression.Lambda<Func<object, bool>>(
                Expression.TypeIs(Expression.Convert(arg, manifestType), type),
                arg
            ).Compile();

            return test(obj);
        }

        // Gets the type that this object appears to be to the compiler
        // This is not the same as the run time type.
        private static IEnumerable<Type> GetManifestTypes(Type type) {
            for (Type t = type; t != null; t = t.BaseType) {
                yield return t;
            }
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null) {
                // can be Nullable<T>
                yield return typeof(Nullable<>).MakeGenericType(type);
            }
            foreach (Type i in type.GetInterfaces()) {
                yield return i;
                // only return one interface to make the test faster
                yield break;
            }
        }

        private static bool ComputeIsinst(object obj, Type type) {
            var dm = new DynamicMethod("test", typeof(bool), new[] { typeof(object) });
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Isinst, type);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Cgt_Un);
            il.Emit(OpCodes.Ret);
            var test = (Func<object, bool>)dm.CreateDelegate(typeof(Func<object, bool>));
            return test(obj);
        }

        #endregion

        #region Positive_TypeEqual

        private static bool ComputeTypeEqual(object obj, Type type, Type manifestType) {

            var arg = Expression.Parameter(typeof(object), "arg");
            var temp = Expression.Parameter(typeof(int), "temp");
            var result1 = Expression.Parameter(typeof(bool), "result1");
            var result2 = Expression.Parameter(typeof(bool), "result2");

            var sideEffectExpr = Expression.Block(
                Expression.Assign(temp, Expression.Constant(42)),
                arg
            );

            var expr1 = Expression.Assign(
                result1,
                Expression.TypeEqual(Expression.Convert(arg, manifestType), type)
            );
            var expr2 = Expression.Assign(
                result2,
                Expression.TypeEqual(Expression.Convert(sideEffectExpr, manifestType), type)
            );

            var block = Expression.Block(
                new ParameterExpression[] { temp, result1, result2 },
                expr1,
                expr2,
                Expression.Condition(
                    Expression.Equal(result1, result2),
                    Expression.Empty(),
                    Expression.Throw(Expression.Constant(new Exception("different results for TypeEquals")))
                ),
                Expression.Condition(
                    Expression.Equal(temp, Expression.Constant(42)),
                    Expression.Empty(),
                    Expression.Throw(Expression.Constant(new Exception("no expected sideeffects")))
                ),
                result1
            );

            var test = Expression.Lambda<Func<object, bool>>(
                block,
                arg
            );

            return test.Compile()(obj);
        }

//ICloneable is inaccessible in Silverlight and the ComputeTypeIs call below will cause us to iterate over it and fail.
#if !SILVERLIGHT
        public static void Positive_TypeEqual(EU.IValidator V) {
            var types = new List<Type> {
                typeof(Byte), 
                typeof(SByte),
                typeof(String),
                typeof(Object),
                typeof(DBNull),
                typeof(TimeSpan), // struct
                typeof(EnumInt16), typeof(EnumUInt16),
                // special types
                typeof(ValueType), typeof(Enum), typeof(Array),
                typeof(IEnumerable), typeof(IComparable), typeof(IList)
            };

            var objs = new List<object>();
            objs.Add(null);
            foreach (Type t in types.ToArray()) {
                types.Add(t.MakeArrayType());
                Type nullable = null;
                if (t.IsValueType) {
                    types.Add(nullable = typeof(Nullable<>).MakeGenericType(t));
                }
                types.Add(typeof(IEnumerable<>).MakeGenericType(t));

                if (!t.IsInterface && !t.IsAbstract) {
                    object obj;
                    if (t == typeof(string)) {
                        obj = "Hello";
                    } else if (t == typeof(DBNull)) {
                        obj = DBNull.Value;
                    } else {
                        obj = Activator.CreateInstance(t);
                    }
                    objs.Add(obj);
                    objs.Add(Array.CreateInstance(t, 0));
                }
            }


            foreach (var obj in objs) {
                //Console.Write('.');
                foreach (var type in types) {
                    Type objType = obj != null ? obj.GetType() : type;

                    foreach (var manifestType in GetManifestTypes(objType)) {
                        if (obj == null && manifestType.IsValueType && Nullable.GetUnderlyingType(manifestType) == null) {
                            // Can't cast null to a value type
                            continue;
                        }

                        bool result = ComputeTypeEqual(obj, type, manifestType);
                        bool expected = TypeEqual(obj, type, manifestType);

                        if (expected != result) {
                            throw new InvalidOperationException(
                               string.Format(
                                   "Test failed: expected '{0}' (type: {1}) TypeEquals {2} to equal {3}, but got {4} when manifest type is {5}",
                                   obj ?? "<null>", objType, type, expected, result, manifestType
                               )
                           );
                        }
                    }
                }
            }
        }
#endif

        private static bool TypeEqual(object obj, Type type, Type manifestType) {
            return obj != null && obj.GetType() == GetNonNullable(type);
        }

        private static Type GetNonNullable(Type t) {
            return Nullable.GetUnderlyingType(t) ?? t;
        }

        private static bool IsNullable(Type t) {
            return Nullable.GetUnderlyingType(t) != null;
        }

        public static Expression Positive_TypeIsVoid(EU.IValidator V) {
            _VoidMethodCalled = 0;
            var e = Expression.Lambda<Func<bool>>(
                Expression.TypeIs(Expression.Call(typeof(Scenarios).GetMethod("VoidMethod")), typeof(void))
            );
            
            V.Validate(e, f =>
            {
                bool voidResult = f();
                EU.Equal(voidResult, false);
                EU.Equal(_VoidMethodCalled, 1);
            });
            return e;
        }

        static int _VoidMethodCalled = 0;
        public static void VoidMethod() { _VoidMethodCalled++; }

        public static Expression Positive_TypeIsNull(EU.IValidator V) {
            // Constant null
            var e = Expression.Lambda<Func<bool>>(
                Expression.TypeIs(Expression.Constant(null), typeof(object))
            );
            V.Validate(e, f =>
            {
                EU.Equal(f(), false);
            });

            // Constant not-null
            e = Expression.Lambda<Func<bool>>(
                Expression.TypeIs(Expression.Constant(new object()), typeof(object))
            );
            V.Validate(e, f =>
            {
                EU.Equal(f(), true);
            });

            // Argument null/non-null
            var x = Expression.Parameter(typeof(object), null);
            var e2 = Expression.Lambda<Func<object, bool>>(Expression.TypeIs(x, typeof(object)), x);
            
            V.Validate(e2, f2 =>
            {
                EU.Equal(f2(null), false);
                EU.Equal(f2(123), true);
                EU.Equal(f2(new object()), true);
            });

            return e;
        }

        public static Expression Positive_TypeIsNullable(EU.IValidator V) {
            // Constant null
            var e = Expression.Lambda<Func<bool>>(
                Expression.TypeIs(Expression.Constant(null), typeof(int?))
            );
            V.Validate(e, f =>
            {
                EU.Equal(f(), false);
            });

            // default(T), equivalent to null
            e = Expression.Lambda<Func<bool>>(
                Expression.TypeIs(Expression.Constant(default(int?), typeof(int?)), typeof(int?))
            );
            V.Validate(e, f =>
            {
                EU.Equal(f(), false);
            });

            // Constant not-null
            e = Expression.Lambda<Func<bool>>(
                Expression.TypeIs(Expression.Constant(123, typeof(int?)), typeof(int?))
            );
            V.Validate(e, f =>
            {
                EU.Equal(f(), true);
            });

            e = Expression.Lambda<Func<bool>>(
                Expression.TypeIs(Expression.Constant(123, typeof(int?)), typeof(int?))
            );
            V.Validate(e, f =>
            {
                EU.Equal(f(), true);
            });

            // Argument null/default(T)/non-null
            var x = Expression.Parameter(typeof(int?), null);
            var e2 = Expression.Lambda<Func<int?, bool>>(Expression.TypeIs(x, typeof(int?)), x);
            V.Validate(e2, f2 =>
            {
                EU.Equal(f2(null), false);
                EU.Equal(f2(123), true);
                EU.Equal(f2(default(int?)), false);
            });

            var y = Expression.Parameter(typeof(object), null);
            var e3 = Expression.Lambda<Func<object, bool>>(Expression.TypeIs(y, typeof(int?)), y);
            
            V.Validate(e3, f3 =>
            {
                EU.Equal(f3(null), false);
                EU.Equal(f3(123), true);
                EU.Equal(f3(default(int?)), false);
                EU.Equal(f3(new object()), false);
            });
            return e;
        }

        #endregion

        public static DateTime TestDateField;
        public static int TestIntField = 2;
        public static decimal TestDecimalField = decimal.One;

        public static decimal VBConvertToDecimal(bool value) {
            return value ? decimal.MinusOne : decimal.Zero;
        }

        public static Expression Positive_ComplexLogical(EU.IValidator V) {
            var e = Expression.Lambda<Func<bool>>(
                Expression.AndAlso(
                    Expression.NotEqual(
                        Expression.Convert(
                            Expression.TypeIs(
                                Expression.Convert(Expression.Field(null, typeof(Scenarios), "TestDateField"), typeof(object)),
                                typeof(double)
                            ),
                            typeof(decimal),
                            typeof(Scenarios).GetMethod("VBConvertToDecimal")
                        ),
                        Expression.Add(
                            Expression.Field(null, typeof(Scenarios), "TestDecimalField"),
                            Expression.Convert(
                                Expression.Field(null, typeof(Scenarios), "testIntField"),
                                typeof(decimal),
                                typeof(decimal).GetMethod("op_Implicit", new[] { typeof(int) })
                            ),
                            typeof(decimal).GetMethod("Add")
                        ),
                        true,
                        typeof(decimal).GetMethod("op_Inequality")
                    ),
                    Expression.Constant(true, typeof(bool))
                )
            );

            V.Validate(e, f =>
            {
                EU.Equal(f(), true);
            });
            return e;
        }

        public struct TestShortCircuit {
            public readonly bool Value;

            public TestShortCircuit(bool val) {
                Value = val;
            }

            public static TestShortCircuit operator |(TestShortCircuit a, TestShortCircuit b) {
                return new TestShortCircuit(a.Value | b.Value);
            }

            public static TestShortCircuit operator &(TestShortCircuit a, TestShortCircuit b) {
                return new TestShortCircuit(a.Value & b.Value);
            }

            public static bool operator true(TestShortCircuit a) {
                return a.Value;
            }

            public static bool operator false(TestShortCircuit a) {
                return !a.Value;
            }

            public override string ToString() {
                return Value.ToString();
            }
        }

        // .NET semantics for userdefined short circuiting operators
        public static void Positive_ShortCircuit_Userdefined_AndAlso(EU.IValidator V) {
            foreach (bool left in new[] { true, false }) {
                foreach (bool right in new[] { true, false }) {
                    bool evaluatedLeft = false;
                    bool evaluatedRight = false;

                    var e = Expression.Lambda<Func<TestShortCircuit>>(
                        Expression.AndAlso(
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<TestShortCircuit>(() => { evaluatedLeft = true; return new TestShortCircuit(left); })
                                )
                            ),
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<TestShortCircuit>(() => { evaluatedRight = true; return new TestShortCircuit(right); })
                                )
                            )
                        )
                    );

                    V.Validate(e, f =>
                    {
                        var actual = ((TestShortCircuit)f()).Value;
                        EU.Equal(actual, left & right);
                        EU.Equal(evaluatedLeft, true);
                        EU.Equal(evaluatedRight, left);
                    });
                }
            }
        }

        // see comment for AndAlso (above)
        public static void Positive_ShortCircuit_Userdefined_OrElse(EU.IValidator V) {
            foreach (var left in new[] { true, false }) {
                foreach (var right in new[] { true, false }) {
                    bool evaluatedLeft = false;
                    bool evaluatedRight = false;

                    var e = Expression.Lambda<Func<TestShortCircuit>>(
                        Expression.OrElse(
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<TestShortCircuit>(() => { evaluatedLeft = true; return new TestShortCircuit(left); })
                                )
                            ),
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<TestShortCircuit>(() => { evaluatedRight = true; return new TestShortCircuit(right); })
                                )
                            )
                        )
                    );

                    V.Validate(e, f =>
                    {
                        var actual = ((TestShortCircuit)f()).Value;
                        EU.Equal(actual, left | right);
                        EU.Equal(evaluatedLeft, true);
                        EU.Equal(evaluatedRight, !left);
                    });
                }
            }
        }

        // Expects VB.NET semantics for (bool? AndAlso bool?)
        // Essentially, three valued logic where "null" is "unknown"
        public static void Positive_ShortCircuit_Lifted_AndAlso(EU.IValidator V) {
            foreach (var left in new bool?[] { true, false, null }) {
                foreach (var right in new bool?[] { true, false, null }) {
                    bool evaluatedLeft = false;
                    bool evaluatedRight = false;

                    var actual = Expression.Lambda<Func<bool?>>(
                        Expression.AndAlso(
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<bool?>(() => { evaluatedLeft = true; return left; })
                                )
                            ),
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<bool?>(() => { evaluatedRight = true; return right; })
                                )
                            )
                        )
                    ).Compile()();

                    EU.Equal(actual, left & right);
                    EU.Equal(evaluatedLeft, true);
                    EU.Equal(evaluatedRight, left != false);
                }
            }
        }

        // see comment for AndAlso (above)
        public static void Positive_ShortCircuit_Lifted_OrElse(EU.IValidator V) {
            foreach (var left in new bool?[] { true, false, null }) {
                foreach (var right in new bool?[] { true, false, null }) {
                    bool evaluatedLeft = false;
                    bool evaluatedRight = false;

                    var actual = Expression.Lambda<Func<bool?>>(
                        Expression.OrElse(
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<bool?>(() => { evaluatedLeft = true; return left; })
                                )
                            ),
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<bool?>(() => { evaluatedRight = true; return right; })
                                )
                            )
                        )
                    ).Compile()();

                    EU.Equal(actual, left | right);
                    EU.Equal(evaluatedLeft, true);
                    EU.Equal(evaluatedRight, left != true);

                }
            }
        }

        // Expects VB.NET semantics for (Userdefined? AndAlso Userdefined?)
        // It is the same as the normal userdefined case, but if either operand
        // is null, the result will be null.
        public static void Positive_ShortCircuit_LiftedUserdefined_AndAlso(EU.IValidator V) {
            var values = new TestShortCircuit?[] { new TestShortCircuit(true), new TestShortCircuit(false), null };
            foreach (var left in values) {
                foreach (var right in values) {
                    bool evaluatedLeft = false, evaluatedRight = false;

                    var e = Expression.Lambda<Func<TestShortCircuit?>>(
                        Expression.AndAlso(
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<TestShortCircuit?>(() => { evaluatedLeft = true; return left; })
                                )
                            ),
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<TestShortCircuit?>(() => { evaluatedRight = true; return right; })
                                )
                            )
                        )
                    );

                    V.Validate(e, f =>
                    {
                        var actual = f();
                        bool expectedEvalRight = false;
                        var expected = AndAlso(
                            () => { return left; },
                            () => { expectedEvalRight = true; return right; }
                        );

                        EU.Equal(actual, expected);
                        EU.Equal(evaluatedLeft, true);
                        EU.Equal(evaluatedRight, expectedEvalRight);
                    });
                }
            }
        }

        // see comment for AndAlso (above)
        public static void Positive_ShortCircuit_LiftedUserdefined_OrElse(EU.IValidator V) {
            var values = new TestShortCircuit?[] { new TestShortCircuit(true), new TestShortCircuit(false), null };
            foreach (var left in values) {
                foreach (var right in values) {
                    bool evaluatedLeft = false, evaluatedRight = false;

                    var e = Expression.Lambda<Func<TestShortCircuit?>>(
                        Expression.OrElse(
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<TestShortCircuit?>(() => { evaluatedLeft = true; return left; })
                                )
                            ),
                            Expression.Invoke(
                                Expression.Constant(
                                    new Func<TestShortCircuit?>(() => { evaluatedRight = true; return right; })
                                )
                            )
                        )
                    );
                    V.Validate(e, f =>
                    {
                        var actual = f();
                        bool expectedEvalRight = false;
                        var expected = OrElse(
                            () => { return left; },
                            () => { expectedEvalRight = true; return right; }
                        );

                        EU.Equal(actual, expected);
                        EU.Equal(evaluatedLeft, true);
                        EU.Equal(evaluatedRight, expectedEvalRight);
                    });
                }
            }
        }

        private static TestShortCircuit? AndAlso(Func<TestShortCircuit?> getLeft, Func<TestShortCircuit?> getRight) {
            TestShortCircuit? left = getLeft();
            if (!left.HasValue) {
                return null;
            }

            if (left.GetValueOrDefault().Value == false) {
                return left;
            }

            TestShortCircuit? right = getRight();
            if (!right.HasValue) {
                return null;
            }

            return new TestShortCircuit?(left.GetValueOrDefault() & right.GetValueOrDefault());
        }

        private static TestShortCircuit? OrElse(Func<TestShortCircuit?> getLeft, Func<TestShortCircuit?> getRight) {
            TestShortCircuit? left = getLeft();
            if (!left.HasValue) {
                return null;
            }

            if (left.GetValueOrDefault().Value == true) {
                return left;
            }

            TestShortCircuit? right = getRight();
            if (!right.HasValue) {
                return null;
            }

            return new TestShortCircuit?(left.GetValueOrDefault() | right.GetValueOrDefault());
        }

        #region Verify that OrElse and AndAlso work when there is a MethodInfo and the 2nd operand has try/catch
        public class MyType {
            public bool Val { get; set; }
            public static bool operator true(MyType x) {
                return x.Val;
            }

            public static bool operator false(MyType x) {
                return !(x.Val);
            }

            public static MyType operator |(MyType x, MyType y) {
                return new MyType { Val = x.Val | y.Val };
            }

            public static MyType operator &(MyType x, MyType y) {
                return new MyType { Val = x.Val & y.Val };
            }
        }
        public static void Positive_OrElseWithMethod(EU.IValidator V) {
            var x = Expression.Parameter(typeof(MyType), "x");
            var y = Expression.Parameter(typeof(MyType), "y");

            var le = Expression.Lambda<Func<MyType, MyType, MyType>>(
                Expression.Block(
                    Expression.OrElse(
                        x,
                        Expression.Block(
                            Expression.TryCatch(
                                Expression.Empty(),
                                Expression.Catch(
                                    typeof(Exception),
                                    Expression.Empty()
                                )
                            ),
                            y
                        )
                    )
                ),
                x, y
            );

            V.Validate(le, f =>
            {
                var @true = new MyType { Val = true };
                var @false = new MyType { Val = false };

                EU.Equal(f(@true, @true).Val, true);
                EU.Equal(f(@true, @false).Val, true);
                EU.Equal(f(@false, @true).Val, true);
                EU.Equal(f(@false, @false).Val, false);
            });
        }

        public static void Positive_AndAlsoWithMethod(EU.IValidator V) {
            var x = Expression.Parameter(typeof(MyType), "x");
            var y = Expression.Parameter(typeof(MyType), "y");

            var le = Expression.Lambda<Func<MyType, MyType, MyType>>(
                Expression.Block(
                    Expression.AndAlso(
                        x,
                        Expression.Block(
                            Expression.TryCatch(
                                Expression.Empty(),
                                Expression.Catch(
                                    typeof(Exception),
                                    Expression.Empty()
                                )
                            ),
                            y
                        )
                    )
                ),
                x, y
            );

            V.Validate(le, f =>
            {
                var @true = new MyType { Val = true };
                var @false = new MyType { Val = false };

                EU.Equal(f(@true, @true).Val, true);
                EU.Equal(f(@true, @false).Val, false);
                EU.Equal(f(@false, @true).Val, false);
                EU.Equal(f(@false, @false).Val, false);
            });
        }
        #endregion

        public static void Positive_ConditionalWithType(EU.IValidator V) {
            var c = Expression.Parameter(typeof(bool), "c");
            string[] one = new[] { "Hi", "Hello" };
            ReadOnlyCollection<string> two = new ReadOnlyCollection<string>(one);


            // This should fail.
            EU.Throws<ArgumentException>(
                delegate {
                    var result = Expression.Condition(
                        c,
                        Expression.Constant(one),
                        Expression.Constant(two)
                    );
                }
            );

            var le = Expression.Lambda<Func<bool, IList<string>>>(
                Expression.Condition(
                    c,
                    Expression.Constant(one),
                    Expression.Constant(two),
                    typeof(IList<string>)
                ),
                c
            );

            V.Validate(le, d =>
            {
                EU.Equal((object)d(true), (object)one);
                EU.Equal((object)d(false), (object)two);
            });
        }

        public static void Positive_UnevenConditionalWithType(EU.IValidator V) {
            var c = Expression.Parameter(typeof(bool), "c");
            var s = Expression.Parameter(typeof(StrongBox<int>), "s");

            var le = Expression.Lambda<Action<bool, StrongBox<int>>>(
                Expression.Condition(
                    c,
                    Expression.Block(
                        Expression.Assign(
                            Expression.Field(s, typeof(StrongBox<int>).GetField("Value")),
                            Expression.Constant(10)
                        ),
                        Expression.Empty()
                    ),
                    Expression.Constant("Hello"),
                    typeof(void)
                ),
                c, s
            );

            V.Validate(le, d =>
            {
                var sb = new StrongBox<int>(0);

                // shouldn't change the value of StrongBox
                d(false, sb);
                EU.Equal(sb.Value, 0);

                d(true, sb);
                EU.Equal(sb.Value, 10);
            });
        }

        public static void Positive_Bug634624(EU.IValidator V) {
            Expression MyAnd = Expression.AndAlso(
                Expression.Constant(new AndAlsoTestDerived()),
                Expression.Constant(new AndAlsoTest())
            );

            var l = Expression.Lambda<Func<AndAlsoTest>>(MyAnd);
            V.Validate(l, f =>
            {
                EU.Equal(f(), null);
            });
        }

        public static void Negative_Bug634624(EU.IValidator V) {
            EU.Throws<InvalidOperationException>(
                () =>
                Expression.AndAlso(
                    Expression.Constant(false),
                    Expression.Constant(true),
                    ((Func<bool, bool, bool>)AndAlsoTest.AndAlso).Method
                )
            );
        }

        public static void Positive_OptimizedOrElse(EU.IValidator V) {
           
            var e = Expression.Lambda<Func<object>>(
                Expression.Call(
                    typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) }),
                    Expression.Constant("Hello"),
                    Expression.Condition(
                        Expression.OrElse(Expression.Constant(true), Expression.Constant(false)),
                        Expression.Constant(null),
                        Expression.Constant(null)
                    )
                )
            );

            var f = e.CompileAndVerify();
            Utils.Equals(f(), null);
        }


        public class AndAlsoTest {
            public static bool AndAlso(bool arg1, bool arg2) {
                return arg1 && arg2;
            }

            public static AndAlsoTest operator &(AndAlsoTest arg1, AndAlsoTest arg2) {
                return null;
            }

            public static bool operator true(AndAlsoTest arg1) {
                return true;
            }

            public static bool operator false(AndAlsoTest arg1) {
                return false;
            }
        }
        public class AndAlsoTestDerived : AndAlsoTest {
        }
   }
}
#endif
