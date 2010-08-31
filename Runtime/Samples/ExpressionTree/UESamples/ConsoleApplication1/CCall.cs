using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples
{
    class CCall
    {
        //Call(Expression, MethodInfo)
        static public void Call1()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  
            


            //This Expression represents a call to a method on an object - in this case int.ToString()
            Expression MyCall = Expression.Call(Expression.Constant(5), typeof(int).GetMethod("ToString",new Type[]{}));

            //Should print 5
            Console.WriteLine(Expression.Lambda<Func<string>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (String.Compare(Expression.Lambda<Func<string>>(MyCall).Compile().Invoke(), "5") != 0) throw new Exception();
        }

        //Call(Expression, MethodInfo, Expression, Expression)
        //<Snippet1>
        public class CCall2
        {
            public int CallMe(int arg1, int arg2)
            {
                return arg1 + arg2;
            }
        }
        //</Snippet1>
        static public void Call2()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  



            //This Expression represents a call to a method on an object - in this case CCall2.CallMe(int, int)
            Expression MyCall = Expression.Call(
                                    Expression.Constant(new CCall2()), 
                                    typeof(CCall2).GetMethod("CallMe", new Type[] {typeof(int),typeof(int)}),
                                    Expression.Constant(1),
                                    Expression.Constant(2)
                                );

            //Should print 3
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 3) throw new Exception();
        }

        //Call(Expression, MethodInfo, Expression, Expression, Expression)
        //<Snippet1>
        public class CCall3
        {
            public int CallMe(int arg1, int arg2, int arg3)
            {
                return (arg1 + arg2) *arg3;
            }
        }
        //</Snippet1>
        static public void Call3()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  



            //This Expression represents a call to a method on an object - in this case CCall3.CallMe(int, int, int)
            Expression MyCall = Expression.Call(
                                    Expression.Constant(new CCall3()),
                                    typeof(CCall3).GetMethod("CallMe", new Type[] { typeof(int), typeof(int), typeof(int) }),
                                    Expression.Constant(1),
                                    Expression.Constant(2),
                                    Expression.Constant(3)
                                );

            //Should print 9
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 9) throw new Exception();
        }


        //Call(Expression, MethodInfo, Expression[])
        //<Snippet1>
        public class CCall4
        {
            public int CallMe(int arg1, int arg2, int arg3, int arg4)
            {
                return (arg1 + arg2) * arg3 / arg4;
            }
        }
        //</Snippet1>
        static public void Call4()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  



            //This Expression represents a call to a method on an object - in this case CCall4.CallMe(int, int, int, int)
            Expression MyCall = Expression.Call(
                                    Expression.Constant(new CCall4()),
                                    typeof(CCall4).GetMethod("CallMe", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }),
                                    Expression.Constant(2),
                                    Expression.Constant(2),
                                    Expression.Constant(3),
                                    Expression.Constant(4)
                                );

            //Should print 3
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 3) throw new Exception();
        }


        //Call(Expression, MethodInfo, IEnumerable`1)
        //<Snippet1>
        public class CCall5
        {
            public int CallMe(int arg1, int arg2, int arg3, int arg4)
            {
                return (arg1 + arg2) * arg3 / arg4;
            }
        }
        //</Snippet1>
        static public void Call5()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  
            
            //This list will represent the arguments we want to pass to the method call.
            List<Expression> Arguments = new List<Expression>();
            Arguments.Add(Expression.Constant(2));
            Arguments.Add(Expression.Constant(2));
            Arguments.Add(Expression.Constant(3));
            Arguments.Add(Expression.Constant(4));

            //This Expression represents a call to a method on an object - in this case CCall5.CallMe(int, int, int, int)
            Expression MyCall = Expression.Call(
                                    Expression.Constant(new CCall5()),
                                    typeof(CCall5).GetMethod("CallMe", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }),
                                    Arguments
                                );

            //Should print 3
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 3) throw new Exception();
        }

        //Call(Expression, String, Type[], Expression[])
        //<Snippet1>
        public class CCall6
        {
            public int CallMe<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                return ((int)(object)arg1 + (int)(object)arg2) * (int)(object)arg3 / (int)(object)arg4;
            }
        }
        //</Snippet1>
        static public void Call6()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a call to a generic method on an object - in this case CCall6.CallMe<int, int, int, int>(arg1, arg2, arg3, arg4)
            Expression MyCall = Expression.Call(
                                    Expression.Constant(new CCall6()),
                                    "CallMe",
                                    new Type[] { typeof(int), typeof(int), typeof(int), typeof(int)},
                                    Expression.Constant(2),
                                    Expression.Constant(2),
                                    Expression.Constant(3), 
                                    Expression.Constant(4)
                                );

            //Should print 3
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 3) throw new Exception();
        }

        //Call(MethodInfo, Expression)
        //<Snippet1>
        public class CCall7
        {
            static public int CallMe(int arg1)
            {
                return arg1 + 1;
            }
        }
        //</Snippet1>
        static public void Call7()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  
            
            //This Expression represents a call to a static method on an object with a single argument 
            Expression MyCall = Expression.Call(
                                    typeof(CCall7).GetMethod("CallMe"),
                                    Expression.Constant(2)
                                );

            //Should print 3
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 3) throw new Exception();
        }

        //Call(MethodInfo, Expression, Expression)
        //<Snippet1>
        public class CCall8
        {
            static public int CallMe(int arg1, int arg2)
            {
                return arg1 + arg2;
            }
        }
        //</Snippet1>
        static public void Call8()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a call to a static method on an object with two arguments
            Expression MyCall = Expression.Call(
                                    typeof(CCall8).GetMethod("CallMe"),
                                    Expression.Constant(2),
                                    Expression.Constant(3)
                                );

            //Should print 5
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 5) throw new Exception();
        }

        //Call(MethodInfo, Expression, Expression, Expression)
        //<Snippet1>
        public class CCall9
        {
            static public int CallMe(int arg1, int arg2, int arg3)
            {
                return arg1 + arg2 + arg3;
            }
        }
        //</Snippet1>
        static public void Call9()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a call to a static method on an object with three arguments
            Expression MyCall = Expression.Call(
                                    typeof(CCall9).GetMethod("CallMe"),
                                    Expression.Constant(2),
                                    Expression.Constant(3), 
                                    Expression.Constant(4)
                                );

            //Should print 9
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 9) throw new Exception();
        }

        //Call(MethodInfo, Expression, Expression, Expression, Expression)
        //<Snippet1>
        public class CCall10
        {
            static public int CallMe(int arg1, int arg2, int arg3, int arg4)
            {
                return arg1 + arg2 + arg3 + arg4;
            }
        }
        //</Snippet1>
        static public void Call10()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a call to a static method on an object with four arguments
            Expression MyCall = Expression.Call(
                                    typeof(CCall10).GetMethod("CallMe"),
                                    Expression.Constant(2),
                                    Expression.Constant(3),
                                    Expression.Constant(4),
                                    Expression.Constant(5)
                                );

            //Should print 14
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 14) throw new Exception();
        }

        //Call(MethodInfo, Expression, Expression, Expression, Expression, Expression)
        //<Snippet1>
        public class CCall11
        {
            static public int CallMe(int arg1, int arg2, int arg3, int arg4, int arg5)
            {
                return arg1 + arg2 + arg3 + arg4 + arg5;
            }
        }
        //</Snippet1>
        static public void Call11()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a call to a static method on an object with five arguments
            Expression MyCall = Expression.Call(
                                    typeof(CCall11).GetMethod("CallMe"),
                                    Expression.Constant(2),
                                    Expression.Constant(3),
                                    Expression.Constant(4),
                                    Expression.Constant(5),
                                    Expression.Constant(6)
                                );

            //Should print 20
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 20) throw new Exception();
        }

        //Call(MethodInfo, Expression[])
        //<Snippet1>
        public class CCall12
        {
            static public int CallMe(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6)
            {
                return arg1 + arg2 + arg3 + arg4 + arg5 + arg6;
            }
        }
        //</Snippet1>
        static public void Call12()
        {
            //<Snippet1>
            // add the following directive to your file:
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a call to a static method on an object with more than five arguments.
            Expression MyCall = Expression.Call(
                                    typeof(CCall12).GetMethod("CallMe"),
                                    Expression.Constant(2),
                                    Expression.Constant(3),
                                    Expression.Constant(4),
                                    Expression.Constant(5),
                                    Expression.Constant(6),
                                    Expression.Constant(7)
                                );

            //Should print 27
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 27) throw new Exception();
        }

        //Call(MethodInfo, IEnumerable`1)
        //<Snippet1>
        public class CCall13
        {
            static public int CallMe(int arg1, int arg2)
            {
                return arg1 + arg2;
            }
        }
        //</Snippet1>
        static public void Call13()
        {
            //<Snippet1>
            // add the following directive to your file:
            // using Microsoft.Scripting.Ast;  

            //We need to create a list of the arguments we want to call.
            var Arguments = new List<Expression>();
            Arguments.Add(Expression.Constant(2));
            Arguments.Add(Expression.Constant(3));

            //This Expression represents a call to a static method with any number of arguments.
            Expression MyCall = Expression.Call(
                                    typeof(CCall13).GetMethod("CallMe"),
                                    Arguments
                                );

            //Should print 5
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 5) throw new Exception();
        }

        //Call(Type, String, Type[], Expression[])
        //<Snippet1>
        public class CCall14
        {
            static public int CallMe<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                return ((int)(object)arg1 + (int)(object)arg2) * (int)(object)arg3 / (int)(object)arg4;
            }
        }
        //</Snippet1>
        static public void Call14()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a call to a static generic method on an object - in this case CCall14.CallMe<int, int, int, int>(arg1, arg2, arg3, arg4)
            Expression MyCall = Expression.Call(
                                    typeof(CCall14),
                                    "CallMe",
                                    new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) },
                                    Expression.Constant(2),
                                    Expression.Constant(2),
                                    Expression.Constant(3),
                                    Expression.Constant(4)
                                );

            //Should print 3
            Console.WriteLine(Expression.Lambda<Func<int>>(MyCall).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyCall).Compile().Invoke() != 3) throw new Exception();
        }        
    }
}
