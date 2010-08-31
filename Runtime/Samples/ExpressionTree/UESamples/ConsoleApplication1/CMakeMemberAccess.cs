using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CMakeMemberAccess {
        //MakeMemberAccess(Expression, MemberInfo)
        //<Snippet1>
        public class Index1 {
            public int X;
        }
        //</Snippet1>

        public static void MakeMemberAccess1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            var MyInstance = new Index1();
            MyInstance.X = 5;

            //This expression represents accessing a non indexed member, for either
            //assigning or reading its value.
            MemberExpression MyMakeMemberAccess = Expression.MakeMemberAccess(
                Expression.Constant(MyInstance),
                typeof(Index1).GetMember("X")[0]
            );
            

            //The end result should 5:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMakeMemberAccess).Compile().Invoke());
            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyMakeMemberAccess).Compile().Invoke() != 5) throw new Exception();
        }

    }
}
