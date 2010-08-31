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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Generation;
using System.Collections;
using EU = ETUtils.ExpressionUtils;

namespace AstTest
{
    public static partial class Scenarios
    {
        // Smoke test for non-constant switch
        public static Expression Positive_SwitchNonConstant(EU.IValidator V)
        {
            var result = Expression.Parameter(typeof(string), "result");
            Func<string, Expression> append = s => Expression.Assign(
                result,
                Expression.Invoke(
                    Expression.Constant(new Func<string, string>(r => r + s)),
                    result
                )
            );

            Func<int, SwitchCase> makeCase = i => Expression.SwitchCase(
                append("_result" + i), Expression.TryFinally(Expression.Constant(i), append("_test" + i))
            );

            var switchValue = Expression.Parameter(typeof(int), "switchValue");
            var e = Expression.Lambda<Func<int, string>>(
                Expression.Block(
                    new[] { result },
                    Expression.Switch(
                        switchValue,
                        append("_default"),
                        makeCase(6),
                        makeCase(1),
                        makeCase(3),
                        makeCase(2),
                        makeCase(1),
                        makeCase(5)
                    )
                ),
                switchValue
            );

            V.Validate(e, f =>
            {
                string def = "_test6_test1_test3_test2_test1_test5_default";
                EU.Equal(f(0), def);
                EU.Equal(f(1), "_test6_test1_result1");
                EU.Equal(f(2), "_test6_test1_test3_test2_result2");
                EU.Equal(f(3), "_test6_test1_test3_result3");
                EU.Equal(f(4), def);
                EU.Equal(f(5), "_test6_test1_test3_test2_test1_test5_result5");
                EU.Equal(f(6), "_test6_result6");
                EU.Equal(f(-1), def);
                EU.Equal(f(7), def);
            });
            return e;
        }

        // Smoke test for non-constant switch with method
        public static Expression Positive_SwitchMethod(EU.IValidator V)
        {
            var result = Expression.Parameter(typeof(string), "result");
            Func<string, Expression> append = s => Expression.Assign(
                result,
                Expression.Invoke(
                    Expression.Constant(new Func<string, string>(r => r + s)),
                    result
                )
            );

            Func<int, SwitchCase> makeCase = i => Expression.SwitchCase(
                append("_result" + i), Expression.TryFinally(Expression.Constant(i), append("_test" + i))
            );

            var switchValue = Expression.Parameter(typeof(int), "switchValue");
            var e = Expression.Lambda<Func<int, string>>(
                Expression.Block(
                    new[] { result },
                    Expression.Switch(
                        switchValue,
                        append("_default"),
                        typeof(Scenarios).GetMethod("StrangeEquality"),
                        makeCase(6),
                        makeCase(1),
                        makeCase(3),
                        makeCase(2),
                        makeCase(1),
                        makeCase(5)
                    )
                ),
                switchValue
            );

            V.Validate(e, f =>
            {
                string def = "_test6_test1_test3_test2_test1_test5_default";
                EU.Equal(f(0), def);
                EU.Equal(f(1), def);
                EU.Equal(f(2), "_test6_test1_result1");
                EU.Equal(f(3), "_test6_test1_test3_test2_result2");
                EU.Equal(f(4), "_test6_test1_test3_result3");
                EU.Equal(f(5), def);
                EU.Equal(f(6), "_test6_test1_test3_test2_test1_test5_result5");
                EU.Equal(f(7), "_test6_result6");
                EU.Equal(f(-1), def);
                EU.Equal(f(8), def);
            });
            return e;
        }

        public static bool StrangeEquality(int x, int y)
        {
            return x == y + 1;
        }

        // Smoke test for compiler & interpreter
        public static Expression Positive_SwitchSimpleWithValue_Compile(EU.IValidator V)
        {
            return TestSwitchSimpleWithValue(true, V);
        }
        // Disabled because LightCompile() causes 6 failed assertions in this test when AstTest.exe run under fulltrust
        public static Expression Disabled_SwitchSimpleWithValue_Interpret(EU.IValidator V)
        {
            return TestSwitchSimpleWithValue(false, V);
        }

        private static Expression TestSwitchSimpleWithValue(bool compile, EU.IValidator V)
        {
            var switchValue = Expression.Parameter(typeof(int), "switchValue");
            var e = Expression.Lambda<Func<int, string>>(
                Expression.Switch(
                    switchValue,
                    Expression.Constant("other"),
                    Expression.SwitchCase(Expression.Constant("six"), Expression.Constant(6)),
                    Expression.SwitchCase(Expression.Constant("one"), Expression.Constant(1)),
                    Expression.SwitchCase(Expression.Constant("three"), Expression.Constant(3)),
                    Expression.SwitchCase(Expression.Constant("two"), Expression.Constant(2)),
                    Expression.SwitchCase(Expression.Constant("FAILED"), Expression.Constant(1)),
                    Expression.SwitchCase(Expression.Constant("five"), Expression.Constant(5))
                ),
                switchValue
            );

            var f = compile ? e.Compile() : e.LightCompile();

            EU.Equal(f(0), "other");
            EU.Equal(f(1), "one");
            EU.Equal(f(2), "two");
            EU.Equal(f(3), "three");
            EU.Equal(f(4), "other");
            EU.Equal(f(5), "five");
            EU.Equal(f(6), "six");
            EU.Equal(f(-1), "other");
            EU.Equal(f(7), "other");
            return e;
        }

        public static Expression Positive_SwitchSimpleVoid_Compile(EU.IValidator V)
        {
            return TestSwitchSimpleVoid(true, V);
        }
        // Disabled because this is failing on IA64 and x64 in partial trust
        public static Expression Disabled_SwitchSimpleVoid_Interpret(EU.IValidator V)
        {
            return TestSwitchSimpleVoid(false, V);
        }
        private static Expression TestSwitchSimpleVoid(bool compile, EU.IValidator V)
        {
            var switchValue = Expression.Parameter(typeof(int), "switchValue");
            var result = Expression.Label(typeof(string));
            var e = Expression.Lambda<Func<int, string>>(
                Expression.Label(
                    result,
                    Expression.Block(
                        Expression.Switch(
                            switchValue,
                            Expression.SwitchCase(Expression.Return(result, Expression.Constant("six")), Expression.Constant(6)),
                            Expression.SwitchCase(Expression.Return(result, Expression.Constant("one")), Expression.Constant(1)),
                            Expression.SwitchCase(Expression.Return(result, Expression.Constant("three")), Expression.Constant(3)),
                            Expression.SwitchCase(Expression.Return(result, Expression.Constant("FAILED")), Expression.Constant(1)),
                            Expression.SwitchCase(Expression.Return(result, Expression.Constant("two")), Expression.Constant(2)),
                            Expression.SwitchCase(Expression.Return(result, Expression.Constant("five")), Expression.Constant(5))
                        ),
                        Expression.Constant("other")
                    )
                ),
                switchValue
            );

            var f = compile ? e.Compile() : e.LightCompile();
            EU.Equal(f(0), "other");
            EU.Equal(f(1), "one");
            EU.Equal(f(2), "two");
            EU.Equal(f(3), "three");
            EU.Equal(f(4), "other");
            EU.Equal(f(5), "five");
            EU.Equal(f(6), "six");
            EU.Equal(f(-1), "other");
            EU.Equal(f(7), "other");
            return e;
        }

        // Smoke test for chars
        public static Expression Positive_SwitchSimpleChars(EU.IValidator V)
        {
            var switchValue = Expression.Parameter(typeof(char), "switchValue");
            var e = Expression.Lambda<Func<char, string>>(
                Expression.Switch(
                    switchValue,
                    Expression.Constant("other"),
                    Expression.SwitchCase(Expression.Constant("six"), Expression.Constant('6')),
                    Expression.SwitchCase(Expression.Constant("one"), Expression.Constant('1')),
                    Expression.SwitchCase(Expression.Constant("three"), Expression.Constant('3')),
                    Expression.SwitchCase(Expression.Constant("two"), Expression.Constant('2')),
                    Expression.SwitchCase(Expression.Constant("FAILED"), Expression.Constant('1')),
                    Expression.SwitchCase(Expression.Constant("five"), Expression.Constant('5'))
                ),
                switchValue
            );

            V.Validate(e, f =>
            {
                EU.Equal(f('0'), "other");
                EU.Equal(f('1'), "one");
                EU.Equal(f('2'), "two");
                EU.Equal(f('3'), "three");
                EU.Equal(f('4'), "other");
                EU.Equal(f('5'), "five");
                EU.Equal(f('6'), "six");
                EU.Equal(f('\uFFFF'), "other");
                EU.Equal(f('7'), "other");
            });
            return e;
        }

        public enum TestEnum : int
        {
            Zero,
            One,
            Two,
            Three,
            Four,
            Five,
            Six
        }

        // Smoke test for enum values
        public static Expression Positive_SwitchSimpleEnum(EU.IValidator V)
        {
            var switchValue = Expression.Parameter(typeof(TestEnum), "switchValue");
            var e = Expression.Lambda<Func<TestEnum, string>>(
                Expression.Switch(
                    switchValue,
                    Expression.Constant("other"),
                    Expression.SwitchCase(Expression.Constant("six"), Expression.Constant(TestEnum.Six)),
                    Expression.SwitchCase(Expression.Constant("one"), Expression.Constant(TestEnum.One)),
                    Expression.SwitchCase(Expression.Constant("three"), Expression.Constant(TestEnum.Three)),
                    Expression.SwitchCase(Expression.Constant("two"), Expression.Constant(TestEnum.Two)),
                    Expression.SwitchCase(Expression.Constant("FAILED"), Expression.Constant(TestEnum.One)),
                    Expression.SwitchCase(Expression.Constant("five"), Expression.Constant(TestEnum.Five))
                ),
                switchValue
            );

            V.Validate(e, f =>
            {
                EU.Equal(f(TestEnum.Zero), "other");
                EU.Equal(f(TestEnum.One), "one");
                EU.Equal(f(TestEnum.Two), "two");
                EU.Equal(f(TestEnum.Three), "three");
                EU.Equal(f(TestEnum.Four), "other");
                EU.Equal(f(TestEnum.Five), "five");
                EU.Equal(f(TestEnum.Six), "six");
                EU.Equal(f((TestEnum)(-1)), "other");
                EU.Equal(f((TestEnum)7), "other");
            });
            return e;
        }

        // Test switch on strings        
        public static Expression Positive_SwitchStrings(EU.IValidator V)
        {
            // Generate 4k unique strings
            int count = 0x1000;
            // 65k iterations, enough to notice a major regression
            int interations = count * 0x10;

            var strings = new string[count];
            var cases = new SwitchCase[count];
            cases[0] = Expression.SwitchCase(Expression.Constant(0), Expression.Constant(null, typeof(string)));
            for (int i = 1, n = count; i < n; i++)
            {
                strings[i] = i.ToString("X");
                cases[i] = Expression.SwitchCase(Expression.Constant(i), Expression.Constant(strings[i]));
            }

            var switchValue = Expression.Parameter(typeof(string), "switchValue");
            var e = Expression.Lambda<Func<string, int>>(
                Expression.Switch(switchValue, Expression.Constant(-1), cases),
                switchValue
            );

            V.Validate(e, f =>
            {
                for (int i = 0; i < interations; i++) {
                    int result = f(strings[i % count]);
                    EU.Equal(result, i % count);
                }
                EU.Equal(f("hello"), -1);
            });

            return e;
        }

        public static void Positive_SwitchNumeric_Byte(EU.IValidator V)
        {
            TestSwitchNumeric<byte>(V);
        }
        public static void Positive_SwitchNumeric_SByte(EU.IValidator V)
        {
            TestSwitchNumeric<sbyte>(V);
        }
        public static void Positive_SwitchNumeric_Int16(EU.IValidator V)
        {
            TestSwitchNumeric<short>(V);
        }
        public static void Positive_SwitchNumeric_UInt16(EU.IValidator V)
        {
            TestSwitchNumeric<ushort>(V);
        }
        public static void Positive_SwitchNumeric_Int32(EU.IValidator V)
        {
            TestSwitchNumeric<int>(V);
        }
        public static void Positive_SwitchNumeric_UInt32(EU.IValidator V)
        {
            TestSwitchNumeric<uint>(V);
        }
        public static void Positive_SwitchNumeric_Int64(EU.IValidator V)
        {
            TestSwitchNumeric<ulong>(V);
        }
        public static void Positive_SwitchNumeric_UInt64(EU.IValidator V)
        {
            TestSwitchNumeric<long>(V);
        }

        private static void TestSwitchNumeric<T>(EU.IValidator V) where T : struct
        {
            decimal max = Convert.ToDecimal(typeof(T).GetField("MaxValue").GetValue(null));
            decimal min = Convert.ToDecimal(typeof(T).GetField("MinValue").GetValue(null));

            // Create seperate clumps of numbers
            // Interesting numbers: min, max, small negatives/zero (if applicable), small positives
            var values = new[] {
                min,
                min + 3,
                min + 6,
                min + 7,
                min + 8,
                0,
                max - 50,
                max - 20,
                max - 18,
                max - 17,
                max - 1,
                max
            };
            var tests = new[] {
                min,
                min + 1,
                min + 2,
                min + 3,
                min + 4,
                min + 5,
                min + 6,
                min + 7,
                min + 8,
                min + 9,
                min + 10,
                0,
                1,
                2,
                max - 50,
                max - 49,
                max - 48,
                max - 22,
                max - 21,
                max - 20,
                max - 19,
                max - 18,
                max - 17,
                max - 16,
                max - 5,
                max - 4,
                max - 3,
                max - 2,
                max - 1,
                max
            };

            TestSwitchNumeric<T>(values, tests, new[] { 1 }, V);
            TestSwitchNumeric<T>(values, tests, new[] { 2 }, V);
            TestSwitchNumeric<T>(values, tests, new[] { 4 }, V);
            TestSwitchNumeric<T>(values, tests, new[] { 10, 5 }, V);
            TestSwitchNumeric<T>(values, tests, new[] { 2, 3, 1 }, V);
        }

        private static void TestSwitchNumeric<T>(decimal[] values, decimal[] tests, int[] groupings, EU.IValidator V) where T : struct
        {
            var cases = new List<SwitchCase>();

            int index = 0, group = 0;
            while (index < values.Length)
            {
                var testValues = new List<Expression>();
                var result = new StringBuilder();
                while (testValues.Count < groupings[group] && index < values.Length)
                {
                    decimal v = values[index++];
#if SILVERLIGHT
                    testValues.Add(Expression.Constant(Convert.ChangeType(v, typeof(T), null)));
#else
                    testValues.Add(Expression.Constant(Convert.ChangeType(v, typeof(T))));
#endif
                    result.Append("{" + v + "}");
                }
                cases.Add(Expression.SwitchCase(Expression.Constant(result.ToString()), testValues));
                group = (group + 1) % groupings.Length;
            }

            var value = Expression.Parameter(typeof(T), "value");
            var e = Expression.Lambda<Func<T, string>>(
                Expression.Switch(value, Expression.Constant("unknown"), null, cases),
                value
            );

            V.Validate(e, f =>
            {
                // Try all of the expected values, as well as nearby values, and
                // make sure we get the correct result.
                foreach (decimal t in tests) {
#if SILVERLIGHT
                    T test = (T)Convert.ChangeType(t, typeof(T), null);
#else
                    T test = (T)Convert.ChangeType(t, typeof(T));
#endif
                    string result = f(test);

                    if (Array.IndexOf(values, t) >= 0) {
                        EU.Equal(result.Contains("{" + t + "}"), true);
                    } else {
                        EU.Equal(result, "unknown");
                    }
                }
            });
        }

        public static void Positive_HeterogeneousSwitch(EU.IValidator V)
        {
#if SILVERLIGHT
            var ht = new Dictionary<string,int>();
#else
            var ht = new Hashtable();
#endif
            ht["Hi"] = 10;
            ht["Hello"] = 20;

            ParameterExpression n = Expression.Parameter(typeof(int), "n");
            var lambda = Expression.Lambda<Func<int, IEnumerable>>(
                Expression.Switch(
                    typeof(IEnumerable),
                    n,
#if SILVERLIGHT
                    Expression.Constant(new Queue<Object>(new object[] { 10, "Hi", 3.5 })),
#else
                    Expression.Constant(new Queue(new object[] { 10, "Hi", 3.5 })),
#endif
                    null,
                    Expression.SwitchCase(
                        Expression.Constant("Hello World"),
                        Expression.Constant(0)
                    ),
                    Expression.SwitchCase(
                        Expression.Constant(new[] { 10, 20, 30, 40 }),
                        Expression.Constant(1)
                    ),
                    Expression.SwitchCase(
#if SILVERLIGHT
                        Expression.Constant(new List<string>(new[] { "Hello", "World" })),
#else
                        Expression.Constant(new ArrayList(new[] { "Hello", "World" })),
#endif
                        Expression.Constant(2)
                    ),
                    Expression.SwitchCase(
                        Expression.Constant(ht),
                        Expression.Constant(3)
                    ),
                    Expression.SwitchCase(
                        Expression.Constant(new BitArray(new[] { true, false, true, true, false, true, false, false, false, true })),
                        Expression.Constant(4)
                    )
                ),
                n
            );

            V.Validate(lambda, d =>
            {
                for (int i = 0; i < 6; i++) {
                    foreach (var o in d(i)) {
                        EU.Equal(o != null, true);
                    }
                }
            });
        }

        public static void Positive_SwitchTestValuesHaveDifferentTypes(EU.IValidator V)
        {
            var switchValue = Expression.Parameter(typeof(object), "switchValue");
            var comparison = typeof(object).GetMethod("Equals", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var e = Expression.Lambda<Func<object, string>>(
                Expression.Switch(
                    switchValue,
                    Expression.Constant("other"),
                    comparison,
                    Expression.SwitchCase(Expression.Constant("zero"), Expression.Constant((object)0, typeof(object))),
                    Expression.SwitchCase(Expression.Constant("one"), Expression.Constant("one")),
                    Expression.SwitchCase(Expression.Constant("null"), Expression.Constant(null))
                ),
                switchValue
            );

            V.Validate(e, f =>
            {
                EU.Equal(f((object)0), "zero");
                EU.Equal(f("one"), "one");
                EU.Equal(f(null), "null");
                EU.Equal(f(new List<int>()), "other");
                EU.Equal(f((object)-1), "other");
                EU.Equal(f(""), "other");
            });
        }

        public static class ComparisonTest
        {
            public static bool Compare(Base x, Base y)
            {
                if (x == null && y == null)
                {
                    return true;
                }
                else
                {
                    if (x == null || y == null)
                    {
                        return false;
                    }
                }
                return x.Value == y.Value;
            }

            public static bool CompareRef(ref Base x, ref Base y)
            {
                if (x == null && y == null)
                {
                    return true;
                }
                else
                {
                    if (x == null || y == null)
                    {
                        return false;
                    }
                }
                return x.Value == y.Value;
            }

            public class Base
            {
                public int Value;
                public Base(int value)
                {
                    Value = value;
                }
            }

            public class Derived : Base
            {
                public Derived(int value)
                    : base(value)
                {
                }
            }

            // method that doesn't return bool.
            public static int Compare1(int x, int y)
            {
                return 0;
            }

            // method that doesn't take two parameters
            public static bool Compare2(int x)
            {
                return x == 0;
            }
        }

        public static void Positive_SwitchTestValuesHaveDifferentTypes2(EU.IValidator V)
        {
            var switchValue = Expression.Parameter(typeof(ComparisonTest.Base), "switchValue");

            var comparison = typeof(ComparisonTest).GetMethod("Compare");
            var e = Expression.Lambda<Func<ComparisonTest.Base, string>>(
                Expression.Switch(
                    switchValue,
                    Expression.Constant("other"),
                    comparison,
                    Expression.SwitchCase(
                        Expression.Constant("zero"),
                        Expression.Constant(new ComparisonTest.Base(0)),
                        Expression.Constant(new ComparisonTest.Derived(0))
                    ),
                    Expression.SwitchCase(Expression.Constant("one"), Expression.Constant(new ComparisonTest.Derived(1)))
                ),
                switchValue
            );

            V.Validate(e, f =>
            {
                EU.Equal(f(new ComparisonTest.Base(0)), "zero");
                EU.Equal(f(new ComparisonTest.Derived(0)), "zero");
                EU.Equal(f(new ComparisonTest.Base(1)), "one");
                EU.Equal(f(new ComparisonTest.Derived(1)), "one");
                EU.Equal(f(new ComparisonTest.Base(2)), "other");
                EU.Equal(f(new ComparisonTest.Derived(2)), "other");
            });
        }

        public static void Positive_SwitchTestValuesHaveDifferentTypesRefParameters(EU.IValidator V)
        {
            var switchValue = Expression.Parameter(typeof(ComparisonTest.Base), "switchValue");

            // The comparison method takes ByRef parameters. There will be no problem
            // for test value and switch value to have non-ByRef types. The behavior is
            // consistent with Expression.Equals.
            var comparison = typeof(ComparisonTest).GetMethod("CompareRef");
            var e = Expression.Lambda<Func<ComparisonTest.Base, string>>(
                Expression.Switch(
                    switchValue,
                    Expression.Constant("other"),
                    comparison,
                    Expression.SwitchCase(Expression.Constant("zero"), Expression.Constant(new ComparisonTest.Base(0))),
                    Expression.SwitchCase(Expression.Constant("one"), Expression.Constant(new ComparisonTest.Derived(1)))
                ),
                switchValue
            );

            V.Validate(e, f =>
            {
                EU.Equal(f(new ComparisonTest.Base(0)), "zero");
                EU.Equal(f(new ComparisonTest.Derived(0)), "zero");
                EU.Equal(f(new ComparisonTest.Base(1)), "one");
                EU.Equal(f(new ComparisonTest.Derived(1)), "one");
                EU.Equal(f(new ComparisonTest.Base(2)), "other");
                EU.Equal(f(new ComparisonTest.Derived(2)), "other");
            });
        }

        public static void Negative_SwitchTestValuesHaveDifferentTypesNoMethod(EU.IValidator V)
        {
            // When there is not comparison method present in the SwitchExpression,
            // all the test values need to have the same type.
            var switchValue = Expression.Parameter(typeof(ComparisonTest.Base), "switchValue");
            EU.Throws<ArgumentException>(
                () =>
                    Expression.Switch(
                        switchValue,
                        Expression.Constant("other"),
                        Expression.SwitchCase(Expression.Constant("zero"), Expression.Constant(new ComparisonTest.Base(0))),
                        Expression.SwitchCase(Expression.Constant("one"), Expression.Constant(new ComparisonTest.Derived(1)))
                    )
            );
        }

        public static void Negative_SwitchTestValueNotMatchMethod(EU.IValidator V)
        {
            var switchValue = Expression.Parameter(typeof(ComparisonTest.Base), "switchValue");
            var comparison = typeof(ComparisonTest).GetMethod("Compare");
            EU.Throws<ArgumentException>(
                () =>
                    Expression.Switch(
                        switchValue,
                        Expression.Constant("other"),
                        comparison,
                        Expression.SwitchCase(Expression.Constant("zero"), Expression.Constant(0))
                    )
            );
        }

        public static void Negative_SwitchSwitchValueNotMatchMethod(EU.IValidator V)
        {
            var switchValue = Expression.Parameter(typeof(int), "switchValue");
            var comparison = typeof(ComparisonTest).GetMethod("Compare");
            EU.Throws<ArgumentException>(
                () =>
                    Expression.Switch(
                        switchValue,
                        Expression.Constant("other"),
                        comparison,
                        Expression.SwitchCase(Expression.Constant("zero"), Expression.Constant(new ComparisonTest.Base(0))),
                        Expression.SwitchCase(Expression.Constant("one"), Expression.Constant(new ComparisonTest.Derived(1)))
                    )
            );
        }

        public static void Negative_SwitchInvalidComparisonMethod(EU.IValidator V)
        {
            var switchValue = Expression.Parameter(typeof(int), "switchValue");
            // The comparison method doesn't return bool.
            var comparison = typeof(ComparisonTest).GetMethod("Compare1");
            EU.Throws<ArgumentException>(
                () =>
                    Expression.Switch(
                        switchValue,
                        Expression.Constant("other"),
                        comparison,
                        Expression.SwitchCase(Expression.Constant("zero"), Expression.Constant(0)),
                        Expression.SwitchCase(Expression.Constant("one"), Expression.Constant(1))
                    )
            );

            // The comparison method doesn't take two parameters.
            comparison = typeof(ComparisonTest).GetMethod("Compare2");
            EU.Throws<ArgumentException>(
                () =>
                    Expression.Switch(
                        switchValue,
                        Expression.Constant("other"),
                        comparison,
                        Expression.SwitchCase(Expression.Constant("zero"), Expression.Constant(0)),
                        Expression.SwitchCase(Expression.Constant("one"), Expression.Constant(1))
                    )
            );
        }

        public static void Positive_SwitchMergeTestTypes(EU.IValidator V)
        {
            var e = Expression.Lambda<Func<int>>(
                Expression.Switch(
                    Expression.Constant(new Exception("TestException")),
                    Expression.Constant(-1),
                    typeof(Scenarios).GetMethod("ExceptionComparer"),
                    new[] {
                        Expression.SwitchCase(
                            Expression.Constant(1),
                            Expression.Constant(new DivideByZeroException("NotTestException"))
                        ),
                        Expression.SwitchCase(
                            Expression.Constant(2),
                            Expression.Constant(new ArgumentException("TestException"))
                        ),
                    }
                )
            );

            V.Validate(e, f =>
            {
                EU.Equal(f(), 2);
            });
        }

        public static bool ExceptionComparer(Exception switchValue, Exception testValue)
        {
            return switchValue.Message == testValue.Message;
        }

        public static void Positive_SwitchMergeTestTypes2(EU.IValidator V)
        {
            var x = Expression.Parameter(typeof(int?), "x");
            var e = Expression.Lambda<Func<int?, int>>(
                Expression.Switch(
                    x,
                    Expression.Constant(-1),
                    typeof(Scenarios).GetMethod("ExceptionComparer2"),
                    new[] {
                        Expression.SwitchCase(
                            Expression.Constant(1),
                            Expression.Constant(new DivideByZeroException("NotTestException"))
                        ),
                        Expression.SwitchCase(
                            Expression.Constant(2),
                            Expression.Constant(new ArgumentException("TestException"))
                        ),
                    }
                ),
                x
            );
            
            V.Validate(e, f =>
            {
                EU.Equal(f(0), 1);
                EU.Equal(f(1), 2);
                EU.Equal(f(2), -1);
                EU.Equal(f(null), -1);
            });
        }

        public static bool ExceptionComparer2(int? switchValue, Exception testValue)
        {
            switch (switchValue)
            {
                case 0:
                    return testValue.Message == "NotTestException";
                case 1:
                    return testValue.Message == "TestException";
                default:
                    return false;
            }
        }


        public static void Positive_SwitchLiftedEquality(EU.IValidator V)
        {
            var x = Expression.Parameter(typeof(int?), "x");
            var e = Expression.Lambda<Func<int?, int>>(
                Expression.Switch(
                    x,
                    Expression.Constant(-1),
                    typeof(Scenarios).GetMethod("LiftedComparer"),
                    new[] {
                        Expression.SwitchCase(
                            Expression.Constant(3),
                            Expression.Constant(null, typeof(double?))
                        ),
                        Expression.SwitchCase(
                            Expression.Constant(1),
                            Expression.Constant(1.0, typeof(double?))
                        ),
                        Expression.SwitchCase(
                            Expression.Constant(2),
                            Expression.Constant(2.0, typeof(double?))
                        ),
                    }
                ),
                x
            );

            V.Validate(e, f =>
            {
                EU.Equal(f(0), -1);
                EU.Equal(f(1), 1);
                EU.Equal(f(2), 2);
                EU.Equal(f(null), 3);
            });
        }

        public static bool LiftedComparer(int switchValue, double testValue)
        {
            return switchValue == testValue;
        }

        // Verify that in a void returning switch expresssion, cases and default
        // can have any types.
        public static void Positive_VoidReturningSwitchWithComparison(EU.IValidator V)
        {
            var mi = typeof(Scenarios).GetMethod("ExcepComparator");

            var testValue = Expression.Parameter(typeof(Exception), "testValue");
            var result = Expression.Parameter(typeof(object).MakeByRefType(), "result");

            var e = Expression.Switch(
                typeof(void), // the switch expression has a void type
                testValue,
                Expression.Block(
                    Expression.Assign(result, Expression.Constant("Default")),
                    Expression.Constant("Default")  // default body has string type
                ),
                mi,
                new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant(1, typeof(object))),
                                Expression.Constant(1) // int type case
                            ),
                            Expression.Constant(new Exception("1"))
                        ),
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant(new Exception("Exception"), typeof(object))),
                                Expression.Constant(new Exception("Exception")) // Exception type case
                            ),
                            Expression.Constant(new Exception("2"))
                        ),
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant(null)),
                                Expression.Empty() // void type case
                            ),
                            Expression.Constant(new Exception("3"))
                        )
                 }
            );

            var le = Expression.Lambda(e, testValue, result);
            var f = le.Compile();
            
            var obj = new object();
            var objs = new object[] { new Exception("1"), obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], (object)1);
            objs = new object[] { new Exception("2"), obj };
            f.DynamicInvoke(objs);
            EU.Equal(((Exception)(objs[1])).Message, "Exception");
            objs = new object[] { new Exception("3"), obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], null);
            objs = new object[] { new Exception("4"), obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], "Default");
        }

        #region "ByRef Args"

        //int
        public static void Positive_VoidByRefInt(EU.IValidator V)
        {
            var testValue = Expression.Parameter(typeof(int), "testValue");
            var result = Expression.Parameter(typeof(int).MakeByRefType(), "result");

            var e = Expression.Switch(
                testValue,
                new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant(int.MaxValue, typeof(int))),
                                Expression.Empty() 
                            ),
                            Expression.Constant(44)
                        )
                 }
            );

            var f = Expression.Lambda(e, testValue, result).Compile();
            int obj = 3;
            var objs = new object[] { 44, obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], (int)int.MaxValue);
        }

        //short
        public static void Positive_VoidReturningSwitchWithComparisonshort(EU.IValidator V)
        {
            var testValue = Expression.Parameter(typeof(short), "testValue");
            var result = Expression.Parameter(typeof(short).MakeByRefType(), "result");

            var e = Expression.Switch(
                testValue,
                new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant((short)short.MaxValue, typeof(short))),
                                Expression.Empty()
                            ),
                            Expression.Constant((short)44,typeof(short))
                        )
                 }
            );

            var f = Expression.Lambda(e, testValue, result).Compile();
            short obj = 3;
            var objs = new object[] { (short)44, obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], (short)short.MaxValue);
        }

        //long
        public static void Positive_VoidReturningSwitchWithComparisonlong(EU.IValidator V)
        {
            var testValue = Expression.Parameter(typeof(long), "testValue");
            var result = Expression.Parameter(typeof(long).MakeByRefType(), "result");

            var e = Expression.Switch(
                testValue,
                new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant((long)long.MaxValue, typeof(long))),
                                Expression.Empty()
                            ),
                            Expression.Constant((long)44,typeof(long))
                        )
                 }
            );

            var f = Expression.Lambda(e, testValue, result).Compile();
            long obj = (long)3;
            var objs = new object[] { (long)44, obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], (long)long.MaxValue);
        }

        //bool
        public static void Positive_VoidReturningSwitchWithComparisonbool(EU.IValidator V)
        {
            var testValue = Expression.Parameter(typeof(bool), "testValue");
            var result = Expression.Parameter(typeof(bool).MakeByRefType(), "result");

            var e = Expression.Switch(
                testValue,
                new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant((bool)true, typeof(bool))),
                                Expression.Empty()
                            ),
                            Expression.Constant((bool)true,typeof(bool))
                        )
                 }
            );

            var f = Expression.Lambda(e, testValue, result).Compile();
            bool obj = (bool)false;
            var objs = new object[] { (bool)true, obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], (bool)true);
        }

        //uint
        public static void Positive_VoidReturningSwitchWithComparisonuint(EU.IValidator V)
        {
            var testValue = Expression.Parameter(typeof(uint), "testValue");
            var result = Expression.Parameter(typeof(uint).MakeByRefType(), "result");

            var e = Expression.Switch(
                testValue,
                new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant((uint)uint.MaxValue, typeof(uint))),
                                Expression.Empty()
                            ),
                            Expression.Constant((uint)44,typeof(uint))
                        )
                 }
            );

            var f = Expression.Lambda(e, testValue, result).Compile();
            uint obj = (uint)3;
            var objs = new object[] { 44u, obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], (uint)uint.MaxValue);
        }

        //ulong
        public static void Positive_VoidReturningSwitchWithComparisonulong(EU.IValidator V)
        {
            var testValue = Expression.Parameter(typeof(ulong), "testValue");
            var result = Expression.Parameter(typeof(ulong).MakeByRefType(), "result");

            var e = Expression.Switch(
                testValue,
                new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant((ulong)ulong.MaxValue, typeof(ulong))),
                                Expression.Empty()
                            ),
                            Expression.Constant((ulong)44,typeof(ulong))
                        )
                 }
            );

            var f = Expression.Lambda(e, testValue, result).Compile();

            ulong obj = (ulong)3;
            var objs = new object[] { (ulong)44, obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], (ulong)ulong.MaxValue);
        }

        //float
        public static void Positive_VoidReturningSwitchWithComparisonfloat(EU.IValidator V)
        {
            var testValue = Expression.Parameter(typeof(float), "testValue");
            var result = Expression.Parameter(typeof(float).MakeByRefType(), "result");

            var e = Expression.Switch(
                testValue,
                new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant((float)float.MaxValue, typeof(float))),
                                Expression.Empty() 
                            ),
                            Expression.Constant((float)44,typeof(float))
                        )
                 }
            );

            var f = Expression.Lambda(e, testValue, result).Compile();
            float obj = (float)3;
            var objs = new object[] { (float)44, obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], (float)float.MaxValue);
        }


        //double
        public static void Positive_VoidReturningSwitchWithComparisondouble(EU.IValidator V)
        {
            var testValue = Expression.Parameter(typeof(double), "testValue");
            var result = Expression.Parameter(typeof(double).MakeByRefType(), "result");

            var e = Expression.Switch(
                testValue,
                Expression.Block(
                    Expression.Assign(result, Expression.Constant((double)5, typeof(double))),
                    Expression.Constant((double)5, typeof(double))
                ),
                new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant((double)double.MaxValue, typeof(double))),
                                Expression.Constant((double)1,typeof(double)) ,
                                Expression.Constant((double)44,typeof(double))
                            ),
                            Expression.Constant((double)44,typeof(double))

                        )
                 }
            );

            var f = Expression.Lambda(e, testValue, result).Compile();
            double obj = (double)3;
            var objs = new object[] { (double)44, obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], (double)double.MaxValue);
        }

        #endregion

        public static bool ExcepComparator(Exception switchValue, Exception testValue)
        {
            return (switchValue.Message == testValue.Message) ? true : false;
        }

        // Verify that in a void returning switch expresssion, cases and default
        // can have any types.
        public static void Positive_VoidReturningSwitchWithoutComparison(EU.IValidator V)
        {
            var testValue = Expression.Parameter(typeof(int), "testValue");
            var result = Expression.Parameter(typeof(object).MakeByRefType(), "result");

            var e = Expression.Switch(
                typeof(void), // the switch expression has a void type
                testValue,
                Expression.Block(
                    Expression.Assign(result, Expression.Constant("Default")),
                    Expression.Constant("Default")  // default body has string type
                ),
                null, //no comparison method
                new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant(1, typeof(object))),
                                Expression.Constant(1) // int type case
                            ),
                            Expression.Constant(1)
                        ),
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant(new Exception("Exception"), typeof(object))),
                                Expression.Constant(new Exception("Exception")) // Exception type case
                            ),
                            Expression.Constant(2)
                        ),
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Assign(result, Expression.Constant(null)),
                                Expression.Empty() // void type case
                            ),
                            Expression.Constant(3)
                        )
                 }
            );

            var le = Expression.Lambda(e, testValue, result);
            var f = le.Compile();
            var obj = new object();
            var objs = new object[] { 1, obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], (object)1);
            objs = new object[] { 2, obj };
            f.DynamicInvoke(objs);
            EU.Equal(((Exception)(objs[1])).Message, "Exception");
            objs = new object[] { 3, obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], null);
            objs = new object[] { 4, obj };
            f.DynamicInvoke(objs);
            EU.Equal(objs[1], "Default");
        }
    }
}
#endif
