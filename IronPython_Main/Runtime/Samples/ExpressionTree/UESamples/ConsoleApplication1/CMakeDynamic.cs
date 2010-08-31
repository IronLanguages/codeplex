using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CMakeDynamic {
        //Expression.MakeDynamic(Type, CallSiteBinder, Expression)
        //<Snippet1>
        public class MyCallSiteBinder : System.Runtime.CompilerServices.CallSiteBinder {
            public override Expression Bind(object[] args, System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                //For this sample, we just always return a constant.
                return Expression.Return(
                    returnLabel,
                    Expression.Constant((int)args[0] + 1)
                );
            }
        }
        //</Snippet1>

        static public void MakeDynamic1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //Instantiate the CallSiteBinder that describes the operation.
            var MyCallSiteBinder = new MyCallSiteBinder();
            

            //This Expression represents a dynamic operation.
            Expression MyMakeDynamic = Expression.MakeDynamic(
                typeof(Func<CallSite,int, int>),
                MyCallSiteBinder,
                Expression.Constant(5)
            );
            

            //Should print 6
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke() != 6) throw new Exception("");
        }


        //Expression.MakeDynamic(Type, CallSiteBinder, Expression, Expression)
        //<Snippet2>
        public class MyCallSiteBinder2 : System.Runtime.CompilerServices.CallSiteBinder {
            public override Expression Bind(object[] args, System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                //For this sample, we just always return a constant.
                return Expression.Return(
                    returnLabel,
                    Expression.Constant((int)args[0] + (int)args[1])
                );
            }
        }
        //</Snippet2>

        static public void MakeDynamic2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //Instantiate the CallSiteBinder that describes the operation.
            var MyCallSiteBinder = new MyCallSiteBinder2();


            //This Expression represents a dynamic operation.
            Expression MyMakeDynamic = Expression.MakeDynamic(
                typeof(Func<CallSite,int, int, int>),
                MyCallSiteBinder,
                Expression.Constant(5),
                Expression.Constant(1)
            );

            //Should print 6
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke());

            //</Snippet2>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke() != 6) throw new Exception("");
        }

        //Expression.MakeDynamic(Type, CallSiteBinder, Expression, Expression, Expression)
        //<Snippet3>
        public class MyCallSiteBinder3 : System.Runtime.CompilerServices.CallSiteBinder {
            public override Expression Bind(object[] args, System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                //For this sample, we just always return a constant.
                return Expression.Return(
                    returnLabel,
                    Expression.Constant((int)args[0] + (int)args[1] + (int)args[2])
                );
            }
        }
        //</Snippet3>

        static public void MakeDynamic3() {
            //<Snippet3>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //Instantiate the CallSiteBinder that describes the operation.
            var MyCallSiteBinder = new MyCallSiteBinder3();


            //This Expression represents a dynamic operation. In this case, the Binder adds all arguments.
            Expression MyMakeDynamic = Expression.MakeDynamic(
                typeof(Func<CallSite, int, int, int, int>),
                MyCallSiteBinder,
                Expression.Constant(5),
                Expression.Constant(1),
                Expression.Constant(8)
            );

            //Should print 14
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke());

            //</Snippet3>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke() != 14) throw new Exception("");
        }


        //Expression.MakeDynamic(Type, CallSiteBinder, Expression, Expression, Expression, Expression)
        //<Snippet4>
        public class MyCallSiteBinder4 : System.Runtime.CompilerServices.CallSiteBinder {
            public override Expression Bind(object[] args, System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                //For this sample, we just always return a constant.
                return Expression.Return(
                    returnLabel,
                    Expression.Constant((int)args[0] + (int)args[1] + (int)args[2] + (int)args[3])
                );
            }
        }
        //</Snippet4>

        static public void MakeDynamic4() {
            //<Snippet4>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //Instantiate the CallSiteBinder that describes the operation.
            var MyCallSiteBinder = new MyCallSiteBinder4();


            //This Expression represents a dynamic operation. In this case, the Binder adds all arguments.
            Expression MyMakeDynamic = Expression.MakeDynamic(
                typeof(Func<CallSite, int, int, int, int, int>),
                MyCallSiteBinder,
                Expression.Constant(5),
                Expression.Constant(1),
                Expression.Constant(8),
                Expression.Constant(12)
            );

            //Should print 26
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke());

            //</Snippet4>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke() != 26) throw new Exception("");
        }



        //Expression.MakeDynamic(Type, CallSiteBinder, Expression[])
        //<Snippet5>
        public class MyCallSiteBinder5 : System.Runtime.CompilerServices.CallSiteBinder {
            public override Expression Bind(object[] args, System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                //For this sample, we just always return a constant with value of all the arguments added.
                int res = 0;
                foreach (object value in args) {
                    res += (int)value;
                }

                return Expression.Return(
                    returnLabel,
                    Expression.Constant(res)
                );
            }
        }
        //</Snippet5>

        static public void MakeDynamic5() {
            //<Snippet5>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //Instantiate the CallSiteBinder that describes the operation.
            var MyCallSiteBinder = new MyCallSiteBinder5();


            //This Expression represents a dynamic operation. In this case, the Binder adds all arguments.
            Expression MyMakeDynamic = Expression.MakeDynamic(
                typeof(Func<CallSite, int, int, int, int, int, int, int>),
                MyCallSiteBinder,
                Expression.Constant(5),
                Expression.Constant(1),
                Expression.Constant(8),
                Expression.Constant(12),
                Expression.Constant(24),
                Expression.Constant(36)
            );

            //Should print 86
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke());

            //</Snippet5>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke() != 86) throw new Exception("");
        }


        //Expression.MakeDynamic(CallSiteBinder, Type, IEnumerable<Expression>)
        //<Snippet6>
        public class MyCallSiteBinder6 : System.Runtime.CompilerServices.CallSiteBinder {
            public override Expression Bind(object[] args, System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                //For this example, we just return a constant with value of all the arguments added.
                int res = 0;
                foreach (object value in args) {
                    res += (int)value;
                }

                return Expression.Return(
                    returnLabel,
                    Expression.Constant(res)
                );
            }
        }
        //</Snippet6>

        static public void MakeDynamic6() {
            //<Snippet6>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //Instantiate the CallSiteBinder that describes the operation.
            var MyCallSiteBinder = new MyCallSiteBinder6();

            //Create a list with the arguments for the operation
            var Arguments = new List<Expression >();
            Arguments.Add(Expression.Constant(5));
            Arguments.Add(Expression.Constant(1));
            Arguments.Add(Expression.Constant(8));
            Arguments.Add(Expression.Constant(12));
            Arguments.Add(Expression.Constant(24));
            Arguments.Add(Expression.Constant(36));

            //This Expression represents a dynamic operation. In this case, the Binder adds all arguments.
            Expression MyMakeDynamic = Expression.MakeDynamic(
                typeof(Func<CallSite, int, int, int, int, int, int, int>),
                MyCallSiteBinder,
                Arguments
            );

            //Should print 86
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke());

            //</Snippet6>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyMakeDynamic).Compile().Invoke() != 86) throw new Exception("");
        }
    }
}
