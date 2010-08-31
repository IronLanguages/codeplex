using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CSwitch {
        //Expression.Switch(Expression, Expression, SwitchCase[])
        public static void SwitchSample() {
            //<Snippet15>
            ConstantExpression switchValue = Expression.Constant(4);

            // This defines a SwitchExpression matching first argument's value against the test values for all provided SwitchCases.
            // If no SwitchCase is matched then the second argument, the default case, will be executed.
            // All SwitchCases in a given SwitchExpression must have the same type unless explicitly specifying a void type for the SwitchExpression.
            // Each SwitchCase has an implicit break statement so that there is no fallthrough.
            SwitchExpression MySwitch =
                Expression.Switch(
                    switchValue,
                    Expression.Constant("DefaultCase"),
                    new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Constant("Case1"),
                            Expression.Constant(1)
                        ),
                        Expression.SwitchCase(
                            Expression.Constant("Case2"),
                            Expression.Constant(2)
                        )
                    }
                );

            // the switchValue did not match either of the SwitchCase testValues so the result is the value of the default case
            Console.WriteLine(Expression.Lambda<Func<string>>(MySwitch).Compile().Invoke());
            //</Snippet15>

            // validate sample
            if (Expression.Lambda<Func<string>>(MySwitch).Compile().Invoke() != "DefaultCase")
                throw new Exception("SwitchSample failed");
        }

        //Expression.Switch(Type, Expression, Expression, MethodInfo, SwitchCase[])
        // <Snippet16>
        // A custom compare method for the SwitchExpression which returns true if the switch value is evenly divisble by the test value
        // This will be used in place of the default comparison that would take place between integers in a SwitchExpression.
        public static bool Compare(int switchValue, int testValue) {
            return (switchValue % testValue == 0) ? true : false;
        }
        // </Snippet16>

        public static void SwitchSample2() {
            //<Snippet16_1>
            ConstantExpression switchValue = Expression.Constant(10);

            // This defines a SwitchExpression matching first argument's value against the test values for all provided SwitchCases.
            // If no SwitchCase is matched then the second argument, the default case, will be executed.
            // All SwitchCases must return a type compatible with the type specified by the SwitchExpression,
            // if the SwitchExpression does not specify a type then all SwitchCases must have the same type.
            // Each SwitchCase has an implicit break statement so that there is no fallthrough.
            SwitchExpression MySwitch =
                Expression.Switch(
                    typeof(IEnumerable<int>),
                    switchValue,
                    Expression.Constant(new int[] { 7, 8, 9, 10 }),
                    ((Func<int, int, bool>)Compare).Method,
                    new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Constant(new int[] { 1, 2, 3 }),
                            Expression.Constant(3)
                        ),
                        // this case will be matched because 10 % 2 == 0 is true
                        Expression.SwitchCase(
                            // this SwitchCase can return any type compatible with IEnumerable<int>,
                            // it need not be the exact same type as the other SwitchCases
                            Expression.Constant(new List<int>() { 4, 5, 6 }),
                            Expression.Constant(2)
                        )
                    }
                );

            // a List<int> is returned as the final result
            Console.WriteLine(Expression.Lambda<Func<IEnumerable<int>>>(MySwitch).Compile().Invoke());
            //</Snippet16_1>

            // validate sample
            if (Expression.Lambda<Func<IEnumerable<int>>>(MySwitch).Compile().Invoke().GetType() != typeof(List<int>))
                throw new Exception("SwitchSample2 failed");
        }

        //Expression.Switch(Type, Expression, Expression, MethodInfo, IEnumerable<SwitchCase>)
        // <Snippet17>
        // A custom compare method for the SwitchExpression which returns true if the switch value is evenly divisble by the test value
        // This will be used in place of the default comparison that would take place between integers in a SwitchExpression.
        // </Snippet17>
        public static void SwitchSample3() {
            //<Snippet17_1>
            ConstantExpression switchValue = Expression.Constant(10);

            // This defines a SwitchExpression matching first argument's value against the test values for all provided SwitchCases.
            // If no SwitchCase is matched then the second argument, the default case, will be executed.
            // All SwitchCases must return a type compatible with the type specified by the SwitchExpression,
            // if the SwitchExpression does not specify a type then all SwitchCases must have the same type.
            // Each SwitchCase has an implicit break statement so that there is no fallthrough.
            SwitchExpression MySwitch =
                Expression.Switch(
                    typeof(IEnumerable<int>),
                    switchValue,
                    Expression.Constant(new int[] { 7, 8, 9, 10 }),
                    ((Func<int, int, bool>)Compare).Method,
                    new List<SwitchCase> {
                        Expression.SwitchCase(
                            Expression.Constant(new int[] { 1, 2, 3 }),
                            Expression.Constant(3)
                        ),
                        // this case will be matched because 10 % 2 == 0 is true
                        Expression.SwitchCase(
                            // this SwitchCase can return any type compatible with IEnumerable<int>,
                            // it need not be the exact same type as the other SwitchCases
                            Expression.Constant(new List<int>() { 4, 5, 6 }),
                            Expression.Constant(2)
                        )
                    }
                );

            // a List<int> is returned as the final result
            Console.WriteLine(Expression.Lambda<Func<IEnumerable<int>>>(MySwitch).Compile().Invoke());
            //</Snippet17_1>

            // validate sample
            if (Expression.Lambda<Func<IEnumerable<int>>>(MySwitch).Compile().Invoke().GetType() != typeof(List<int>))
                throw new Exception("SwitchSample3 failed");
        }

        //Expression.Switch(Expression, SwitchCase[])
        public static void SwitchSample4() {
            //<Snippet18>
            ConstantExpression switchValue = Expression.Constant(2);

            System.Reflection.MethodInfo mi = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });

            // This defines a SwitchExpression matching first argument's value against the test values for all provided SwitchCases.
            // When no default case is provided all SwitchCases must return void.
            SwitchExpression MySwitch =
                Expression.Switch(
                    switchValue,
                    new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Case1")),
                                Expression.Empty()
                            ),
                            Expression.Constant(1)
                        ),
                        Expression.SwitchCase(
                            Expression.Block(
                                Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Case2")),
                                Expression.Empty()
                            ),
                            Expression.Constant(2)
                        )
                    }
                );

            // The second case was matched and "Case2" is printed. The SwitchExpression itself returns void.
            Expression.Lambda<Action>(MySwitch).Compile().Invoke();
            //</Snippet18>
        }


        //Expression.Switch(Expression, Expression, MethodInfo, SwitchCase[])
        // <Snippet20>
        // A custom compare method for the SwitchExpression which returns true if the switch value is evenly divisble by the test value,
        // but not equal to it.
        // This will be used in place of the default comparison that would take place between integers in a SwitchExpression.
        public static bool Compare2(int switchValue, int testValue) {
            if (switchValue == testValue)
                return false;
            else
                return (switchValue % testValue == 0) ? true : false;
        }
        // </Snippet20>

        public static void SwitchSample5() {
            //<Snippet20_1>
            ConstantExpression switchValue = Expression.Constant(10);

            // This defines a SwitchExpression matching first argument's value against the test values for all provided SwitchCases.
            // If no SwitchCase is matched then the second argument, the default case, will be executed.
            SwitchExpression MySwitch =
                Expression.Switch(
                    switchValue,
                    Expression.Constant("DefaultCase"),
                    ((Func<int, int, bool>)Compare2).Method,
                    new SwitchCase[] {
                        Expression.SwitchCase(
                            Expression.Constant("Case1"),
                            Expression.Constant(3)
                        ),
                        // This case is not matched because the custom comparator overrides the default comparison behavior for integers.
                        Expression.SwitchCase(
                            Expression.Constant("Case2"),
                            Expression.Constant(10)
                        )
                    }
                );

            // DefaultCase is returned as the final result because no case matched when using the custom comparator method.
            Console.WriteLine(Expression.Lambda<Func<string>>(MySwitch).Compile().Invoke());
            //</Snippet20_1>

            // validate sample
            if (Expression.Lambda<Func<string>>(MySwitch).Compile().Invoke() != "DefaultCase")
                throw new Exception("SwitchSample5 failed");
        }

        //Expression.Switch(Expression, Expression, MethodInfo, IEnumerable'1)
        // <Snippet21>
        // A custom compare method for the SwitchExpression which returns true if the switch value is evenly divisble by the test value,
        // but not equal to it.
        // This will be used in place of the default comparison that would take place between integers in a SwitchExpression.
        //public static bool Compare2(int switchValue, int testValue) {
        //    if (switchValue == testValue)
        //        return false;
        //    else
        //        return (switchValue % testValue == 0) ? true : false;
        //}
        // </Snippet21>

        public static void SwitchSample6() {
            //<Snippet21_1>
            ConstantExpression switchValue = Expression.Constant(10);

            // This defines a SwitchExpression matching first argument's value against the test values for all provided SwitchCases.
            // If no SwitchCase is matched then the second argument, the default case, will be executed.
            SwitchExpression MySwitch =
                Expression.Switch(
                    switchValue,
                    Expression.Constant("DefaultCase"),
                    ((Func<int, int, bool>)Compare2).Method,
                    new List<SwitchCase>() {
                        Expression.SwitchCase(
                            Expression.Constant("Case1"),
                            Expression.Constant(3)
                        ),
                        // This case is not matched because the custom comparator overrides the default comparison behavior for integers.
                        Expression.SwitchCase(
                            Expression.Constant("Case2"),
                            Expression.Constant(10)
                        )
                    }
                );

            // DefaultCase is returned as the final result because no case matched when using the custom comparator method.
            Console.WriteLine(Expression.Lambda<Func<string>>(MySwitch).Compile().Invoke());
            //</Snippet21_1>

            // validate sample
            if (Expression.Lambda<Func<string>>(MySwitch).Compile().Invoke() != "DefaultCase")
                throw new Exception("SwitchSample6 failed");
        }
    }
}
