using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CLabel {
        //Expression.Label()
        static public void Label1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This element defines a LabelTarget. The LabelTarget is used to associate Goto, Break and Continue
            //expressions with the desired Labels or loops.
            LabelTarget MyLabelTarget = Expression.Label();

            //This goto jumps over a throw expression:
            var MyBlock = Expression.Block(
                Expression.Goto(MyLabelTarget),
                Expression.Throw(Expression.Constant(new Exception())),
                Expression.Label(MyLabelTarget)
            );

            //No exception should be thrown.
            Expression.Lambda<Action>(MyBlock).Compile().Invoke();
            //</Snippet1>

        }

        //Expression.Label(LabelTarget)
        static public void Label2() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This element defines a LabelTarget. The LabelTarget is used to associate Goto, Break and Continue
            //expressions with the desired Labels or loops.
            LabelTarget MyLabelTarget = Expression.Label();

            //This element defines a Label. A label indentifies an execution point in the program, enabling 
            //execution to jump to it.
            LabelExpression MyLabel = Expression.Label(MyLabelTarget);

            //This goto jumps over a throw expression:
            var MyBlock = Expression.Block(
                Expression.Goto(MyLabelTarget),
                Expression.Throw(Expression.Constant(new Exception())),
                MyLabel
            );

            //No exception should be thrown.
            Expression.Lambda<Action>(MyBlock).Compile().Invoke();
            //</Snippet1>

        }

        //Expression.Label(LabelTarget, Expression)
        static public void Label3() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to create a label target to use with the label.
            LabelTarget MyLabelTarget = Expression.Label(typeof(int));

            //This element defines a Label. A label indentifies an execution point in the program, enabling 
            //execution to jump to it. It also has a default value.
            LabelExpression MyLabel = Expression.Label(
                MyLabelTarget,
                Expression.Constant(5)
            );

            //No exception should be thrown, 5 should be printed.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyLabel).Compile().Invoke());
            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyLabel).Compile().Invoke() != 5) throw new Exception();

        }

        //Expression.Label(string)
        static public void Label4() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This element defines a LabelTarget. The LabelTarget is used to associate Goto, Break and Continue
            //expressions with the desired Labels or loops. It can use a name for debugging purposes.
            LabelTarget MyLabelTarget = Expression.Label("My Label Target");

            //This goto jumps over a throw expression:
            var MyBlock = Expression.Block(
                Expression.Goto(MyLabelTarget),
                Expression.Throw(Expression.Constant(new Exception())),
                Expression.Label(MyLabelTarget)
            );

            //No exception should be thrown.
            Expression.Lambda<Action>(MyBlock).Compile().Invoke();
            //</Snippet1>
        }



        //Expression.Label(Type)
        static public void Label5() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This element defines a LabelTarget. The LabelTarget is used to associate Goto, Break and Continue
            //expressions with the desired Labels or loops. It can define a type, which labels associated with this
            //LabelTarget will have.
            LabelTarget MyLabelTarget = Expression.Label(typeof(int));

            //This element defines a Label. A label indentifies an execution point in the program, enabling 
            //execution to jump to it. It also has a default value, which matches the labeltarget's type.
            LabelExpression MyLabel = Expression.Label(
                MyLabelTarget,
                Expression.Constant(5)
            );

            //No exception should be thrown, 5 should be printed.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyLabel).Compile().Invoke());
            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyLabel).Compile().Invoke() != 5) throw new Exception();

        }

        //Expression.Label(Type, string)
        static public void Label6() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This element defines a LabelTarget. The LabelTarget is used to associate Goto, Break and Continue
            //expressions with the desired Labels or loops. It can define a type, which labels associated with this
            //LabelTarget will have.
            //It defines a name for debugging purposes.
            LabelTarget MyLabelTarget = Expression.Label(typeof(int), "MyLabelTarget");

            //This element defines a Label. A label indentifies an execution point in the program, enabling 
            //execution to jump to it. It also has a default value, which matches the labeltarget's type.
            LabelExpression MyLabel = Expression.Label(
                MyLabelTarget,
                Expression.Constant(5)
            );

            //No exception should be thrown, 5 should be printed.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyLabel).Compile().Invoke());
            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyLabel).Compile().Invoke() != 5) throw new Exception();

        }
    }
}
