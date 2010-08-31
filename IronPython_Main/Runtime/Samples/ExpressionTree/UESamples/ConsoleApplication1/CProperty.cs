using System;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;
using System.Text;

namespace Samples {
    class CProperty {
        //Property(Expression, PropertyInfo)
        public static void PropertySample1() {
            //<Snippet1>
            //This expression represents a property access.
            //It can be used to assign or read the value of a property.
            Expression MyProperty = Expression.Property(
                Expression.Constant("Hello"),
                typeof(string).GetProperty("Length")
            );

            //The end result should be the value of "Hello".Length
            Console.WriteLine(Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke() != 5) throw new Exception();
        }

        //Property(Expression, String)
        //<Snippet2>
        public class CProperty2 {
            public int AProperty { get; set; }
            public CProperty2() {
                AProperty = 42;
            }
        }
        //</Snippet2>
        public static void PropertySample2() {
            //<Snippet2>
            //This expression represents a property access.
            //It can be used to assign to or read the value of a property.
            Expression MyProperty = Expression.Property(
                Expression.New(typeof(CProperty2)),
                "AProperty"
            );

            //The end result should be 42.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke() != 42) throw new Exception();
        }

        //Property(Expression, Type, String)
        //<Snippet3>
        public class CProperty3 {
            public int AProperty { get; set; }
            public CProperty3() {
                AProperty = 42;
            }
        }

        public class CProperty3Derived : CProperty3 {
            new public int AProperty { get; set; }
            public CProperty3Derived() {
                AProperty = 43;
            }
        }

        //</Snippet3>
        public static void PropertySample3() {
            //<Snippet3>
            //This expression represents a property access.
            //It can be used to assign to or read the value of a property.
            //The type argument can be used to access a property on a base 
            //class that is hidden in the current instance.
            Expression MyProperty = Expression.Property(
                Expression.New(typeof(CProperty3)),
                typeof(CProperty3),
                "AProperty"
            );

            //The end result should be 42.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke());
            //</Snippet3>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke() != 42) throw new Exception();
        }

        //<Snippet4>
        //<Snippet5>
        //<Snippet6>
        public class CProperty4 {
            public int[] data = { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            public int this[int index] {
                get 
                {
                    return data[index];
                }
                set 
                {
                    data[index] = value;
                }
            }
        }
        //</Snippet4>
        //</Snippet5>
        //</Snippet6>

        // Property(Expression, PropertyInfo, Expression[])
        public static void PropertySample4() {
            //<Snippet4>
            // This expression represents accessing an indexer property.
            Expression MyProperty = Expression.Property(
                Expression.New(typeof(CProperty4)),
                typeof(CProperty4).GetProperty("Item"),
                new Expression[] { Expression.Constant(3) }
            );

            // The result is 7.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke());
            //</Snippet4>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke() != 7) throw new Exception();
        }

        // Property(Expression, PropertyInfo, IEnumerable'1)
        public static void PropertySample5() {
            //<Snippet5>
            // This expression represents accessing an indexer property.
            Expression MyProperty = Expression.Property(
                Expression.New(typeof(CProperty4)),
                typeof(CProperty4).GetProperty("Item"),
                new List<Expression>() { Expression.Constant(3) }
            );

            // The result is 7.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke());
            //</Snippet5>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke() != 7) throw new Exception();
        }

        // Property(Expression, PropertyInfo, Expression[])
        public static void PropertySample6() {
            //<Snippet6>
            // This expression represents accessing an indexer property.
            Expression MyProperty = Expression.Property(
                Expression.New(typeof(CProperty4)),
                "Item",
                new Expression[] { Expression.Constant(3) }
            );

            // The result is 7.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke() != 7) throw new Exception();
        }

        //Property(Expression, MethodInfo)
        public static void PropertySample7() {
            //<Snippet7>
            //This expression represents a property access.
            //It can be used to assign or read the value of a property.
            //The MethodInfo argument points to the accessor method that the property would call.
            Expression MyProperty = Expression.Property(
                Expression.Constant("Hello"),
                typeof(string).GetMethod("get_Length")
            );

            //The end result should be the value of "Hello".Length
            Console.WriteLine(Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke() != 5) throw new Exception();
        }

        //<Snippet8>
        public class MyClass {
            public int AField = 42;
            public int AProperty { get; set; }
        }
        //</Snippet8>

        //Property(Expression, MethodInfo)
        public static void PropertyOrFieldSample1() {
            //<Snippet8>
            //This expression represents a property or field access.
            //It can be used to assign or read the value of a property or field.
            Expression instance = Expression.Constant(new MyClass());

            Expression MyProperty = Expression.Block(
                Expression.Assign(
                    Expression.PropertyOrField(
                        instance,
                        "AProperty"
                    ),
                    Expression.PropertyOrField(
                        instance,
                        "AField"
                    )
                )
             );

            // Now AProperty has been set to the value of AField so 42 is printed.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke());
            //</Snippet8>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyProperty).Compile().Invoke() != 42) throw new Exception();
        }
    }
}
