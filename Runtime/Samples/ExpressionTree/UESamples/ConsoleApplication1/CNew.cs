using System;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;
using System.Text;

namespace Samples {
    class CNew {
        // New(Type)
        // <Snippet1>
        public class MyClass {
            public int data;

            public MyClass() {
                Console.WriteLine("Parameterless Constructor");
            }
            public MyClass(int x) {
                Console.WriteLine("Integer Constructor: {0}", x);
                data = x;
            }
            public string DoWork() {
                return "Work Complete";
            }
        }
        //<Snippet1>

        public static void NewSample1() {
            //<Snippet1>
            // This expression represents instantiating an object of the given type with its parameterless constructor.
            Expression MyNew = Expression.New(typeof(MyClass));

            Expression DoWork = Expression.Call(MyNew, typeof(MyClass).GetMethod("DoWork"));

            // This will print "Parameterless Constructor" followed by "Work Complete"
            Console.WriteLine(Expression.Lambda<Func<string>>(DoWork).Compile().Invoke());
            //</Snippet1>

            // validate sample
            if (Expression.Lambda<Func<string>>(DoWork).Compile().Invoke() != "Work Complete") throw new Exception("");
        }

        //New(ConstructorInfo)
        public static void NewSample2() {
            //<Snippet2>
            // This expression represents instantiating an object of the given type based on the provided ConstructorInfo.
            Expression MyNew = Expression.New(typeof(MyClass).GetConstructor(new Type[] {}));

            Expression DoWork = Expression.Call(MyNew, typeof(MyClass).GetMethod("DoWork"));

            // This will print "Parameterless Constructor" followed by "Work Complete"
            Console.WriteLine(Expression.Lambda<Func<string>>(DoWork).Compile().Invoke());
            //</Snippet2>

            // validate sample
            if (Expression.Lambda<Func<string>>(DoWork).Compile().Invoke() != "Work Complete") throw new Exception("");
        }

        //New(ConstructorInfo, Expression[])
        public static void NewSample3() {
            //<Snippet3>
            // This expression represents instantiating an object of the given type.
            Expression MyNew = Expression.New(
                typeof(MyClass).GetConstructor(new Type[] { typeof(int) }), 
                new Expression[] { Expression.Constant(5) }
            );

            Expression DoWork = Expression.Call(MyNew, typeof(MyClass).GetMethod("DoWork"));

            // This will print "Integer Constructor 5" followed by "Work Complete"
            Console.WriteLine(Expression.Lambda<Func<string>>(DoWork).Compile().Invoke());
            //</Snippet3>

            // validate sample
            if (Expression.Lambda<Func<string>>(DoWork).Compile().Invoke() != "Work Complete") throw new Exception("");
        }

        //New(ConstructorInfo, IEnumerable'1)
        public static void NewSample4() {
            //<Snippet3>
            // This expression represents instantiating an object of the given type.
            Expression MyNew = Expression.New(
                typeof(MyClass).GetConstructor(new Type[] { typeof(int) }),
                new List<Expression>() { Expression.Constant(5) }
            );

            Expression DoWork = Expression.Call(MyNew, typeof(MyClass).GetMethod("DoWork"));

            // This will print "Integer Constructor 5" followed by "Work Complete"
            Console.WriteLine(Expression.Lambda<Func<string>>(DoWork).Compile().Invoke());
            //</Snippet3>

            // validate sample
            if (Expression.Lambda<Func<string>>(DoWork).Compile().Invoke() != "Work Complete") throw new Exception("");
        }

        //New(ConstructorInfo, IEnumerable'1, MemberInfo[])
        public static void NewSample5() {
            //<Snippet4>
            // This expression represents instantiating an object of the given type.
            Expression MyNew = Expression.New(
                typeof(MyClass).GetConstructor(new Type[] { typeof(int) }),
                new List<Expression>() { Expression.Constant(5) },
                // The provided members are purely for debugging and printing purposes.
                new System.Reflection.MemberInfo[] {
                    typeof(MyClass).GetMember("data")[0]
                }
            );

            Expression DoWork = Expression.Call(MyNew, typeof(MyClass).GetMethod("DoWork"));

            // This will print "Integer Constructor 5" followed by "Work Complete"
            Console.WriteLine(Expression.Lambda<Func<string>>(DoWork).Compile().Invoke());
            //</Snippet4>

            // validate sample
            if (Expression.Lambda<Func<string>>(DoWork).Compile().Invoke() != "Work Complete") throw new Exception("");
        }

        //New(ConstructorInfo, IEnumerable'1, IEnumerable'1)
        public static void NewSample6() {
            //<Snippet5>
            // This expression represents instantiating an object of the given type.
            Expression MyNew = Expression.New(
                typeof(MyClass).GetConstructor(new Type[] { typeof(int) }),
                new List<Expression>() { Expression.Constant(5) },
                // The provided members are purely for debugging and printing purposes.
                new List<System.Reflection.MemberInfo>() {
                    typeof(MyClass).GetMember("data")[0]
                }
            );

            Expression DoWork = Expression.Call(MyNew, typeof(MyClass).GetMethod("DoWork"));

            // This will print "Integer Constructor 5" followed by "Work Complete"
            Console.WriteLine(Expression.Lambda<Func<string>>(DoWork).Compile().Invoke());
            //</Snippet5>

            // validate sample
            if (Expression.Lambda<Func<string>>(DoWork).Compile().Invoke() != "Work Complete") throw new Exception("");
        }
    }
}
