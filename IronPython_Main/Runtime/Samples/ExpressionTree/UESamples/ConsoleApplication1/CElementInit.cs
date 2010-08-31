using System;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;
using System.Text;

namespace Samples {
    class CElementInit {
        //Expression.ElementInit(MethodInfo, Expression[])
        static public void ElementInit1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //List variable
            ParameterExpression MyList = Expression.Parameter(typeof(List<int>));
            var NewList = Expression.New(typeof(List<int>).GetConstructor(new Type[] { }));
            
            //These expressions represent elements that can be used to initialize an object.
            ElementInit MyElementInit1 = Expression.ElementInit(
                                        typeof(List<int>).GetMethod("Add",new Type[]{typeof(int)}),
                                        Expression.Constant(1)
                                    );

            ElementInit MyElementInit2 = Expression.ElementInit(
                                        typeof(List<int>).GetMethod("Add",new Type[]{typeof(int)}),
                                        Expression.Constant(2)
                                    );

            ElementInit MyElementInit3 = Expression.ElementInit(
                                        typeof(List<int>).GetMethod("Add",new Type[]{typeof(int)}),
                                        Expression.Constant(3)
                                    );

            //Create a list initialization, with three values: {1,2,3}
            var ListInit = Expression.ListInit(
                NewList,
                MyElementInit1,
                MyElementInit2,
                MyElementInit3
            );
                
            
            
            var CheckListLength = Expression.Property(
                                    MyList,
                                    "Count"
                                );

            //create list, initialize with 1, 2, 3, then check number of elements on the list.
            var MyBlock = Expression.Block(
                new ParameterExpression[] { MyList },
                Expression.Assign(
                    MyList,
                    ListInit
                ),
                CheckListLength
            );
                    
                

            //Should print 3
            Console.WriteLine(Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke() != 3) throw new Exception("");
        }

        //Expression.ElementInit(MethodInfo, IEnumerable<Expression>)
        static public void ElementInit2() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //List variable
            ParameterExpression MyList = Expression.Parameter(typeof(List<int>));
            var NewList = Expression.New(typeof(List<int>).GetConstructor(new Type[] { }));

            //These expressions represent elements that can be used to initialize an object.
            ElementInit MyElementInit1 = Expression.ElementInit(
                                        typeof(List<int>).GetMethod("Add", new Type[] { typeof(int) }),
                                        Expression.Constant(1)
                                    );

            ElementInit MyElementInit2 = Expression.ElementInit(
                                        typeof(List<int>).GetMethod("Add", new Type[] { typeof(int) }),
                                        Expression.Constant(2)
                                    );

            ElementInit MyElementInit3 = Expression.ElementInit(
                                        typeof(List<int>).GetMethod("Add", new Type[] { typeof(int) }),
                                        Expression.Constant(3)
                                    );

            //A list with containing the initializers
            var Initializers = new List<ElementInit>() {MyElementInit1, MyElementInit2, MyElementInit3} ;
            

            //Create a list initialization, with three values: {1,2,3}
            var ListInit = Expression.ListInit(
                NewList,
                Initializers
            );



            var CheckListLength = Expression.Property(
                                    MyList,
                                    "Count"
                                );

            //create list, initialize with 1, 2, 3, assign the resulting list to a variable,
            //then check number of elements on the list.
            var MyBlock = Expression.Block(
                new ParameterExpression[] { MyList },
                Expression.Assign(
                    MyList,
                    ListInit
                ),
                CheckListLength
            );



            //Should print 3
            Console.WriteLine(Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke() != 3) throw new Exception("");
        }
    }
}
