using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CListInit {
        //Expression.ListInit(NewExpression, ElementInit[])
        static public void ListInit1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //List variable
            ParameterExpression MyList = Expression.Parameter(typeof(List<int>));
            var NewList = Expression.New(typeof(List<int>).GetConstructor(new Type[] { }));
            
            //Create element initialization expressions
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

            //This Expression represents a List creation and initialization with multiple elements.
            ListInitExpression ListInit = Expression.ListInit(
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




        //Expression.ListInit(NewExpression, Expression[])
        static public void ListInit2() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //List variable
            ParameterExpression MyList = Expression.Parameter(typeof(List<int>));
            var NewList = Expression.New(typeof(List<int>).GetConstructor(new Type[] { }));


            //This Expression represents a List creation and initialization with multiple elements.
            ListInitExpression ListInit = Expression.ListInit(
                NewList,
                Expression.Constant(1),
                Expression.Constant(2),
                Expression.Constant(3)
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






        //Expression.ListInit(NewExpression, IEnumerable<ElementInit>)
        static public void ListInit3() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //List variable
            ParameterExpression MyList = Expression.Parameter(typeof(List<int>));
            var NewList = Expression.New(typeof(List<int>).GetConstructor(new Type[] { }));

            //Create element initialization expressions
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

            //This Expression represents a List creation and initialization with multiple elements.
            ListInitExpression ListInit = Expression.ListInit(
                NewList,
                new List<ElementInit>(){
                    MyElementInit1,
                    MyElementInit2,
                    MyElementInit3
                }
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




        //Expression.ListInit(NewExpression, IEnumerable<Expression>)
        static public void ListInit4() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //List variable
            ParameterExpression MyList = Expression.Parameter(typeof(List<int>));
            var NewList = Expression.New(typeof(List<int>).GetConstructor(new Type[] { }));


            //This Expression represents a List creation and initialization with multiple elements.
            ListInitExpression ListInit = Expression.ListInit(
                NewList,
                new List<Expression>{
                    Expression.Constant(1),
                    Expression.Constant(2),
                    Expression.Constant(3)
                }
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



        //Expression.ListInit(NewExpression, MethodInfo, Expression[])
        static public void ListInit5() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //List variable
            ParameterExpression MyList = Expression.Parameter(typeof(List<int>));
            var NewList = Expression.New(typeof(List<int>).GetConstructor(new Type[] { }));


            //This Expression represents a List creation and initialization with multiple elements.
            ListInitExpression ListInit = Expression.ListInit(
                NewList,
                typeof(List<int>).GetMethod("Add", new Type[] { typeof(int) }),
                Expression.Constant(1),
                Expression.Constant(2),
                Expression.Constant(3)
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


        //Expression.ListInit(NewExpression, MethodInfo, IEnumerable<Expression>)
        static public void ListInit6() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //List variable
            ParameterExpression MyList = Expression.Parameter(typeof(List<int>));
            var NewList = Expression.New(typeof(List<int>).GetConstructor(new Type[] { }));


            //This Expression represents a List creation and initialization with multiple elements.
            ListInitExpression ListInit = Expression.ListInit(
                NewList,
                typeof(List<int>).GetMethod("Add", new Type [] {typeof(int)}),
                new List<Expression>{
                    Expression.Constant(1),
                    Expression.Constant(2),
                    Expression.Constant(3)
                }
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
    }
}
