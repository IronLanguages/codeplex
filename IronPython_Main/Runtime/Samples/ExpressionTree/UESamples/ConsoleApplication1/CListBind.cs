using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CListBind {
        //Expression.ListBind(MemberInfo, ElementInit[])
        //<Snippet1>
        public class LB1 {
            public List<int> MyField = new List<int>();
        }
        //</Snippet1>
        static public void ListBind1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //First we create a variable of the containing type
            ParameterExpression MyClass = Expression.Variable(typeof(LB1));
                        
            //This expression represent adding elements to a list as part of the containing class's initialization.
            //It assumes the list is instantiated separately, either through the class itself, or through 
            //a MemberAssignment expression (constructed from Expression.Bind).
            MemberListBinding MyListBind1 = Expression.ListBind(
                                        typeof(LB1).GetField("MyField"),
                                        Expression.ElementInit(
                                            typeof(List<int>).GetMethod("Add",new Type[]{typeof(int)}),
                                            Expression.Constant(1)
                                        ),
                                        Expression.ElementInit(
                                            typeof(List<int>).GetMethod("Add",new Type[]{typeof(int)}),
                                            Expression.Constant(2)
                                        )
                                    );
            


            //Here we create the class with the member binding previously defined.
            MemberInitExpression MyMembersInit = Expression.MemberInit(
                Expression.New(
                    typeof(LB1).GetConstructor(new Type[] { })
                ),
                MyListBind1
            );

            
            //Check how many items were added to the list field.
            var ListCount = Expression.Property(
                Expression.Field(
                    MyMembersInit,
                    "MyField"
                ),
                "Count"
            );


            //Should print 2.
            Console.WriteLine(Expression.Lambda<Func<int>>(ListCount).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(ListCount).Compile().Invoke() != 2) throw new Exception("");
        }

        //Expression.ListBind(MemberInfo, Ienumerable<ElementInit>)
        //<Snippet2>
        public class LB2 {
            public List<int> MyField = new List<int>();
        }
        //</Snippet2>
        static public void ListBind2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //First we create a variable of the containing type
            ParameterExpression MyClass = Expression.Variable(typeof(LB2));

            //This expression represent adding elements to a list as part of the containing class's initialization.
            //It assumes the list is instantiated separately, either through the class itself, or through 
            //a MemberAssignment expression (constructed from Expression.Bind).
            MemberListBinding MyListBind1 = Expression.ListBind(
                                        typeof(LB2).GetField("MyField"),
                                        new List<ElementInit>(){
                                            Expression.ElementInit(
                                                typeof(List<int>).GetMethod("Add", new Type[] { typeof(int) }),
                                                Expression.Constant(1)
                                            ),
                                            Expression.ElementInit(
                                                typeof(List<int>).GetMethod("Add", new Type[] { typeof(int) }),
                                                Expression.Constant(2)
                                            )
                                        }
                                    );



            //Here we create the class with the member binding previously defined.
            MemberInitExpression MyMembersInit = Expression.MemberInit(
                Expression.New(
                    typeof(LB2).GetConstructor(new Type[] { })
                ),
                MyListBind1
            );


            //Check how many items were added to the list field.
            var ListCount = Expression.Property(
                Expression.Field(
                    MyMembersInit,
                    "MyField"
                ),
                "Count"
            );


            //Should print 2.
            Console.WriteLine(Expression.Lambda<Func<int>>(ListCount).Compile().Invoke());

            //</Snippet2>

            //Validate sample
            if (Expression.Lambda<Func<int>>(ListCount).Compile().Invoke() != 2) throw new Exception("");
        }



        //Expression.ListBind(MethodInfo, ElementInit[])
        //<Snippet3>
        public class LB3 {
            private List<int> m_prop = new List<int>() { };
            public List<int>  MyProp {
                get {
                    return m_prop;
                }

                set {
                    m_prop = value;
                }
            }
        }
        //</Snippet3>

        static public void ListBind3() {
            //<Snippet3>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //First we create a variable of the containing type
            ParameterExpression MyClass = Expression.Variable(typeof(LB3));

            //This expression represent adding elements to a list as part of the containing class's initialization.
            //It assumes the list is instantiated separately, either through the class itself, or through 
            //a MemberAssignment expression (constructed from Expression.Bind).
            MemberListBinding MyListBind1 = Expression.ListBind(
                                        typeof(LB3).GetMethod("get_MyProp"),
                                        Expression.ElementInit(
                                            typeof(List<int>).GetMethod("Add", new Type[] { typeof(int) }),
                                            Expression.Constant(1)
                                        ),
                                        Expression.ElementInit(
                                            typeof(List<int>).GetMethod("Add", new Type[] { typeof(int) }),
                                            Expression.Constant(2)
                                        )
                                    );



            //Here we create the class with the member binding previously defined.
            MemberInitExpression MyMembersInit = Expression.MemberInit(
                Expression.New(
                    typeof(LB3).GetConstructor(new Type[] { })
                ),
                MyListBind1
            );


            //Check how many items were added to the list field.
            var ListCount = Expression.Property(
                Expression.Property(
                    MyMembersInit,
                    "MyProp"
                ),
                "Count"
            );


            //Should print 2.
            Console.WriteLine(Expression.Lambda<Func<int>>(ListCount).Compile().Invoke());

            //</Snippet3>

            //Validate sample
            if (Expression.Lambda<Func<int>>(ListCount).Compile().Invoke() != 2) throw new Exception("");
        }



        //Expression.ListBind(MethodInfo, IEnumerable<ElementInit>)
        //<Snippet4>
        public class LB4 {
            private List<int> m_prop = new List<int>() { };
            public List<int> MyProp {
                get {
                    return m_prop;
                }

                set {
                    m_prop = value;
                }
            }
        }
        //</Snippet4>

        static public void ListBind4() {
            //<Snippet4>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //First we create a variable of the containing type
            ParameterExpression MyClass = Expression.Variable(typeof(LB4));

            //This expression represent adding elements to a list as part of the containing class's initialization.
            //It assumes the list is instantiated separately, either through the class itself, or through 
            //a MemberAssignment expression (constructed from Expression.Bind).
            MemberListBinding MyListBind1 = Expression.ListBind(
                                        typeof(LB4).GetMethod("get_MyProp"),
                                        new List<ElementInit>(){
                                            Expression.ElementInit(
                                                typeof(List<int>).GetMethod("Add", new Type[] { typeof(int) }),
                                                Expression.Constant(1)
                                            ),
                                            Expression.ElementInit(
                                                typeof(List<int>).GetMethod("Add", new Type[] { typeof(int) }),
                                                Expression.Constant(2)
                                            )
                                        }
                                    );



            //Here we create the class with the member binding previously defined.
            MemberInitExpression MyMembersInit = Expression.MemberInit(
                Expression.New(
                    typeof(LB4).GetConstructor(new Type[] { })
                ),
                MyListBind1
            );


            //Check how many items were added to the list field.
            var ListCount = Expression.Property(
                Expression.Property(
                    MyMembersInit,
                    "MyProp"
                ),
                "Count"
            );


            //Should print 2.
            Console.WriteLine(Expression.Lambda<Func<int>>(ListCount).Compile().Invoke());

            //</Snippet4>

            //Validate sample
            if (Expression.Lambda<Func<int>>(ListCount).Compile().Invoke() != 2) throw new Exception("");
        }
    }
}
