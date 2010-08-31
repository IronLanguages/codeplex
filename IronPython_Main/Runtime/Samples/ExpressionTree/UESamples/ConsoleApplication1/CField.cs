using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CField {
        //Field(Expression, FieldInfo)
        public static void Field1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression represents a field access.
            //It can be used to assign or read the value of a field.
            //for static fields, the expression must be null.
            Expression MyField = Expression.Field(
                null,
                typeof(int).GetField("MaxValue")
            );

            //The end result should be the value of integer.MaxValue
            Console.WriteLine(Expression.Lambda<Func<int>>(MyField).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyField).Compile().Invoke() != int.MaxValue) throw new Exception();
        }

        //Field(Expression, String)
        //<Snippet2>
        public class CField2 {
            public int AField = 42;
        }
        //</Snippet2>
        public static void Field2() {
            //<Snippet2>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression represents a field access.
            //It can be used to assign to or read the value of a field.
            Expression MyField = Expression.Field(
                Expression.New(typeof(CField2)),
                "AField"
            );

            //The end result should be 42.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyField).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyField).Compile().Invoke() != 42) throw new Exception();
        }

        //Field(Expression, Type, String)
        //<Snippet2>
        public class CField3 {
            public int AField = 42;
        }

        public class CField3Derived : CField3 {
            new public int AField = 43;
        }

        //</Snippet2>
        public static void Field3() {
            //<Snippet2>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression represents a field access.
            //It can be used to assign to or read the value of a field.
            //The type argument can be used to access a field on a base 
            //class that is hidden in the current instance.
            Expression MyField = Expression.Field(
                Expression.New(typeof(CField3)),
                typeof(CField3),
                "AField"
            );

            //The end result should be 42.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyField).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyField).Compile().Invoke() != 42) throw new Exception();
        }

    }
}
