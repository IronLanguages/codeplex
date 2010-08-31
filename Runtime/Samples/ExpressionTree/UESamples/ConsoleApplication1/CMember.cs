using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CMember {
        //MemberInit(NewExpression, IEnumerable'1)
        //<Snippet1>
        public class CMembers {
            public int data;
        }
        //</Snippet1>

        public static void MemberInitSample1() {
            //<Snippet1>
            // This variable will be the result of the MemberInit expression.
            ParameterExpression Variable = Expression.Variable(typeof(CMembers), "Variable");

            // This expression represents initializing the members of a new class instance.
            Expression MyMemberInit =
                Expression.Block(
                    new ParameterExpression[] { Variable },
                    Expression.Assign(
                        Variable,
                        Expression.MemberInit(
                            // This expression instantiates a new instance of CMembers with the parameterless constructor.
                            Expression.New(typeof(CMembers)),
                            new List<MemberBinding>() {
                                // This expression will set the data field to 10 for the instance of CMembers.
                                Expression.Bind(typeof(CMembers).GetMember("data")[0], Expression.Constant(10))
                            }
                        )
                    ),
                    Expression.Field(Variable, "data")
                );

            // This will print the value of the data field after it was initialized above with a value of 10.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMemberInit).Compile().Invoke());
            //</Snippet1>

            //validate sample
            if (Expression.Lambda<Func<int>>(MyMemberInit).Compile().Invoke() != 10) throw new Exception("");
        }

        // MemberInit(NewExpression, MemberBinding[])
        public static void MemberInitSample2() {
            //<Snippet2>
            // This variable will be the result of the MemberInit expression.
            ParameterExpression Variable = Expression.Variable(typeof(CMembers), "Variable");

            // This expression represents initializing the members of a new class instance.
            Expression MyMemberInit =
                Expression.Block(
                    new ParameterExpression[] { Variable },
                    Expression.Assign(
                        Variable,
                        Expression.MemberInit(
                            // This expression instantiates a new instance of CMembers with the parameterless constructor.
                            Expression.New(typeof(CMembers)),
                            new MemberBinding[] {
                                // This expression will set the data field to 10 for the instance of CMembers.
                                Expression.Bind(typeof(CMembers).GetMember("data")[0], Expression.Constant(10))
                            }
                        )
                    ),
                    Expression.Field(Variable, "data")
                );

            // This will print the value of the data field after it was initialized above with a value of 10.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMemberInit).Compile().Invoke());
            //</Snippet2>

            //validate sample
            if (Expression.Lambda<Func<int>>(MyMemberInit).Compile().Invoke() != 10) throw new Exception("");
        }

        //<Snippet3>
        public class Circle {
            public double Radius;
            public Point Center;
        }

        public struct Point {
            public int x;
            public int y;
        }
        //</Snippet3>

        // MemberBind(MemberInfo, MemberBinding[])
        public static void MemberBindSample1() {
            //<Snippet3>
            // This variable will be the result of the MemberInit expression.
            ParameterExpression Variable = Expression.Variable(typeof(Circle), "Variable");

            // This expression represents initializing the members of a new class instance.
            Expression MyMemberBind =
                Expression.Block(
                    new ParameterExpression[] { Variable },
                    Expression.Assign(
                        Variable,
                        // This expression represents initializing a new instance of the Circle class
                        Expression.MemberInit(
                            Expression.New(typeof(Circle)),
                            // This array contains bindings of the Circle's members with values
                            new MemberBinding[] { 
                                // The Radius field is a value type that is initialized via Bind
                                Expression.Bind(typeof(Circle).GetMember("Radius")[0], Expression.Constant(5.2)),
                                // The Center field is of a type that has members of its own that need to be initialized.
                                // This expression represents binding the values of Circle's Center member.
                                Expression.MemberBind(
                                    typeof(Circle).GetField("Center"),
                                    new MemberBinding[] {
                                        Expression.Bind(typeof(Point).GetField("x"), Expression.Constant(1)),
                                        Expression.Bind(typeof(Point).GetField("y"), Expression.Constant(2))
                                    }
                                )
                            }
                        )
                    ),
                    Expression.Field(Expression.Field(Variable, "Center"), "y")
                );

            // This will print the value of the y field of the Circle's Center member which was initialized to 2.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMemberBind).Compile().Invoke());
            //</Snippet3>

            //validate sample
            if (Expression.Lambda<Func<int>>(MyMemberBind).Compile().Invoke() != 2) throw new Exception("");
        }

        // MemberBind(MemberInfo, IEnumerable'1)
        public static void MemberBindSample2() {
            //<Snippet4>
            // This variable will be the result of the MemberInit expression.
            ParameterExpression Variable = Expression.Variable(typeof(Circle), "Variable");

            // This expression represents initializing the members of a new class instance.
            Expression MyMemberBind =
                Expression.Block(
                    new ParameterExpression[] { Variable },
                    Expression.Assign(
                        Variable,
                        // This expression represents initializing a new instance of the Circle class
                        Expression.MemberInit(
                            Expression.New(typeof(Circle)),
                            // This array contains bindings of the Circle's members with values
                            new List<MemberBinding>() { 
                                // The Radius field is a value type that is initialized via Bind
                                Expression.Bind(typeof(Circle).GetMember("Radius")[0], Expression.Constant(5.2)),
                                // The Center field is of a type that has members of its own that need to be initialized.
                                // This expression represents binding the values of Circle's Center member.
                                Expression.MemberBind(
                                    typeof(Circle).GetField("Center"),
                                    new MemberBinding[] {
                                        Expression.Bind(typeof(Point).GetField("x"), Expression.Constant(1)),
                                        Expression.Bind(typeof(Point).GetField("y"), Expression.Constant(2))
                                    }
                                )
                            }
                        )
                    ),
                    Expression.Field(Expression.Field(Variable, "Center"), "y")
                );

            // This will print the value of the y field of the Circle's Center member which was initialized to 2.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMemberBind).Compile().Invoke());
            //</Snippet4>

            //validate sample
            if (Expression.Lambda<Func<int>>(MyMemberBind).Compile().Invoke() != 2) throw new Exception("");
        }

        //<Snippet5>
        public class Circle2 {
            public double Radius;
            public Point2 _center;
            public Point2 Center {
                get {
                    if (_center == null)
                        _center = new Point2();
                    return _center;
                }
                set {
                    _center = value;
                }
            }
        }

        public class Point2 {
            public int _x;
            public int _y;
        }
        //</Snippet5>

        // MemberBind(MethodInfo, MemberBinding[])
        public static void MemberBindSample3() {
            //<Snippet5>
            // This variable will be the result of the MemberInit expression.
            ParameterExpression Variable = Expression.Variable(typeof(Circle2), "Variable");

            // This expression represents initializing the members of a new class instance.
            Expression MyMemberBind =
                Expression.Block(
                    new ParameterExpression[] { Variable },
                    Expression.Assign(
                        Variable,
                        // This expression represents initializing a new instance of the Circle class
                        Expression.MemberInit(
                            Expression.New(typeof(Circle2)),
                            // This array contains bindings of the Circle's members with values
                            new MemberBinding[] { 
                                // The Radius field is a value type that is initialized via Bind
                                Expression.Bind(typeof(Circle2).GetMember("Radius")[0], Expression.Constant(5.2)),
                                // The Center field is of a type that has members of its own that need to be initialized.
                                // This expression represents binding the values of Circle's Center member.
                                // This overload of MemberBind requires a MethodInfo to the member's property accessor method.
                                // The member must be a reference type and have been initialized when this Expression executes.
                                Expression.MemberBind(
                                    typeof(Circle2).GetMethod("get_Center"),
                                    new MemberBinding[] {
                                        Expression.Bind(typeof(Point2).GetField("_x"), Expression.Constant(1)),
                                        Expression.Bind(typeof(Point2).GetField("_y"), Expression.Constant(2))
                                    }
                                )
                            }
                        )
                    ),
                    Expression.Field(Expression.Property(Variable, "Center"), "_y")
                );

            // This will print the value of the y field of the Circle's Center member which was initialized to 2.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMemberBind).Compile().Invoke());
            //</Snippet5>

            //validate sample
            if (Expression.Lambda<Func<int>>(MyMemberBind).Compile().Invoke() != 2) throw new Exception("");
        }

        // MemberBind(MethodInfo, IEnumerable'1)
        public static void MemberBindSample4() {
            //<Snippet6>
            // This variable will be the result of the MemberInit expression.
            ParameterExpression Variable = Expression.Variable(typeof(Circle2), "Variable");

            // This expression represents initializing the members of a new class instance.
            Expression MyMemberBind =
                Expression.Block(
                    new ParameterExpression[] { Variable },
                    Expression.Assign(
                        Variable,
                        // This expression represents initializing a new instance of the Circle class
                        Expression.MemberInit(
                            Expression.New(typeof(Circle2)),
                            // This array contains bindings of the Circle's members with values
                            new MemberBinding[] { 
                                // The Radius field is a value type that is initialized via Bind
                                Expression.Bind(typeof(Circle2).GetMember("Radius")[0], Expression.Constant(5.2)),
                                // The Center field is of a type that has members of its own that need to be initialized.
                                // This expression represents binding the values of Circle's Center member.
                                // This overload of MemberBind requires a MethodInfo to the member's property accessor method.
                                // The member must be a reference type and have been initialized when this Expression executes.
                                Expression.MemberBind(
                                    typeof(Circle2).GetMethod("get_Center"),
                                    new List<MemberBinding>() {
                                        Expression.Bind(typeof(Point2).GetField("_x"), Expression.Constant(1)),
                                        Expression.Bind(typeof(Point2).GetField("_y"), Expression.Constant(2))
                                    }
                                )
                            }
                        )
                    ),
                    Expression.Field(Expression.Property(Variable, "Center"), "_y")
                );

            // This will print the value of the y field of the Circle's Center member which was initialized to 2.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMemberBind).Compile().Invoke());
            //</Snippet6>

            //validate sample
            if (Expression.Lambda<Func<int>>(MyMemberBind).Compile().Invoke() != 2) throw new Exception("");
        }
    }
}
