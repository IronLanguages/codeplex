using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

namespace ConsoleApplication1
{
    public class Program
    {
        static void Main(string[] args)
        {
            NoPiaBinaryTest1();
            NoPiaUnaryTest1();
            NoPiaBinaryAssignTest1();
            NoPiaBinaryAssignTest2();
            NoPiaAssign1();
            NoPiaBreak1();
            NoPiaBreak2();
            NoPiaCall1();
            NoPiaContinue1();
            NoPiaConvert1();
            NoPiaDefault1();
            //NoPiaField1(); Bug 680842
            NoPiaGoto1();
            NoPiaLambdaInvoke1();
            NoPiaNewArrayInit1();
            NoPiaReturn1();
            NoPiaReturn2();
            NoPiaSwitch1();
            NoPiaSwitch2();
            NoPiaSwitch3();
            NoPiaTry1();
            NoPiaTry2();
            NoPiaTry3();
            NoPiaTry4();
            NoPiaTypeEqual1();
            NoPiaTypeIs1();
            NoPiaUnbox1();

        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaBinaryTest1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaBinaryTest1() {
            MethodInfo mi = typeof(NoPiaHelperClass.NoPiaHelper).GetMethod("Add");
            var exp = Expression.Add(Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), mi);
            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaUnaryTest1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaUnaryTest1()
        {
            MethodInfo mi = typeof(NoPiaHelperClass.NoPiaHelper).GetMethod("Not");
            var exp = Expression.Not(Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), mi);
            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaBinaryAssignTest1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaBinaryAssignTest1()
        {
            MethodInfo mi = typeof(NoPiaHelperClass.NoPiaHelper).GetMethod("Add");
            ParameterExpression var = Expression.Variable(NoPiaHelper2.NoPiaHelper2.FooStructType);
            var exp = Expression.AddAssign(var, Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), mi);
            Expression.Lambda<Action>(Expression.Block(new ParameterExpression[] { var }, exp)).Compile().Invoke();
        }




        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaBinaryAssignTest2",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaBinaryAssignTest2()
        {
            MethodInfo mi = typeof(NoPiaHelperClass.NoPiaHelper).GetMethod("Add");
            ParameterExpression var = Expression.Variable(NoPiaHelper2.NoPiaHelper2.FooStructType);
            var exp = Expression.AddAssign(var, Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), mi,
                Expression.Lambda(
                    Expression.GetFuncType(new Type[]{NoPiaHelper2.NoPiaHelper2.FooStructType,NoPiaHelper2.NoPiaHelper2.FooStructType}),
                    Expression.Constant(NoPiaHelperClass.NoPiaHelper.X),
                    Expression.Parameter(NoPiaHelper2.NoPiaHelper2.FooStructType)
                )
            );
            Expression.Lambda<Action>(Expression.Block(new ParameterExpression[] { var }, exp)).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaAssign1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaAssign1()
        {
            ParameterExpression var = Expression.Variable(NoPiaHelper2.NoPiaHelper2.FooStructType);
            var exp = Expression.Assign(var, Expression.Constant(NoPiaHelperClass.NoPiaHelper.X));
            Expression.Lambda<Action>(Expression.Block(new ParameterExpression[] { var }, exp)).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaBreak1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaBreak1()
        {
            var lt = Expression.Label();
            Expression.Break(lt, Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), NoPiaHelper2.NoPiaHelper2.FooStructType);
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaBreak2",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaBreak2()
        {
            var lt = Expression.Label(NoPiaHelper2.NoPiaHelper2.FooStructType);
            Expression.Break(lt, Expression.Constant(NoPiaHelperClass.NoPiaHelper.X));
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaCall1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaCall1()
        {
            MethodInfo mi = typeof(NoPiaHelperClass.NoPiaHelper).GetMethod("Add");
            var exp = Expression.Call(null, mi, Expression.Constant(NoPiaHelper2.NoPiaHelper2.X), Expression.Constant(NoPiaHelper2.NoPiaHelper2.X));
            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaContinue1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaContinue1()
        {
            var lt = Expression.Label(typeof(void));
            Expression.Continue(lt, NoPiaHelperClass.NoPiaHelper.FooStructType);
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaConvert1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaConvert1()
        {
            var exp = Expression.Convert(Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), NoPiaHelper2.NoPiaHelper2.FooStructType);
            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaDefault1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaDefault1()
        {
            var exp = Expression.Default(NoPiaHelperClass.NoPiaHelper.FooStructType);
            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Disabled,"NoPiaField1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaField1()
        {
            NoPiaHelperClass.NoPiaHelper.X.Structure = 55;
            var fld = Expression.Field(Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), NoPiaHelper2.NoPiaHelper2.FooStructType.GetField("Structure"));
            if (Expression.Lambda<Func<int>>(
                fld
            ).Compile().Invoke()
                != 55) throw new Exception();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaGoto1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaGoto1()
        {
            var lb = Expression.Label();
            Expression.Goto(lb, Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), NoPiaHelper2.NoPiaHelper2.FooStructType);
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaLambdaInvoke1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaLambdaInvoke1()
        {
            var param1 = Expression.Parameter(NoPiaHelper2.NoPiaHelper2.FooStructType);
            var lmb = Expression.Lambda(Expression.GetActionType(NoPiaHelperClass.NoPiaHelper.FooStructType), Expression.Empty(), param1);

            var lmb2 = Expression.Lambda(Expression.GetFuncType(NoPiaHelper2.NoPiaHelper2.FooStructType, NoPiaHelperClass.NoPiaHelper.FooStructType), Expression.Constant(NoPiaHelper2.NoPiaHelper2.X), param1);
            lmb2.Compile().DynamicInvoke(NoPiaHelperClass.NoPiaHelper.X);
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaNewArrayInit1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaNewArrayInit1()
        {
            var exp = Expression.NewArrayInit(NoPiaHelperClass.NoPiaHelper.FooStructType, Expression.Constant(NoPiaHelper2.NoPiaHelper2.X));
            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaReturn1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaReturn1()
        {
            var lt = Expression.Label();
            Expression.Return(lt, Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), NoPiaHelper2.NoPiaHelper2.FooStructType);
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaReturn2",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaReturn2()
        {
            var lt = Expression.Label(NoPiaHelper2.NoPiaHelper2.FooStructType);
            Expression.Return(lt, Expression.Constant(NoPiaHelperClass.NoPiaHelper.X));
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaSwitch1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaSwitch1()
        {
            var exp = Expression.Switch(Expression.Constant(true),
                Expression.Constant(NoPiaHelper2.NoPiaHelper2.X),
                Expression.SwitchCase(Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), Expression.Constant(true)),
                Expression.SwitchCase(Expression.Constant(NoPiaHelper2.NoPiaHelper2.X), Expression.Constant(true))
                );
            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaSwitch2",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaSwitch2()
        {
            var exp = Expression.Switch(
                NoPiaHelperClass.NoPiaHelper.FooStructType,
                Expression.Constant(true),
                Expression.Constant(NoPiaHelper2.NoPiaHelper2.X),
                null,
                Expression.SwitchCase(Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), Expression.Constant(true)),
                Expression.SwitchCase(Expression.Constant(NoPiaHelper2.NoPiaHelper2.X), Expression.Constant(true))
                );
            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaSwitch3",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaSwitch3()
        {
            var exp = Expression.Switch(
                NoPiaHelperClass.NoPiaHelper.FooStructType,
                Expression.Constant(NoPiaHelper2.NoPiaHelper2.X),
                Expression.Constant(NoPiaHelper2.NoPiaHelper2.X),
                typeof(NoPiaHelperClass.NoPiaHelper).GetMethod("Equal"),
                Expression.SwitchCase(Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), Expression.Constant(NoPiaHelperClass.NoPiaHelper.X)),
                Expression.SwitchCase(Expression.Constant(NoPiaHelper2.NoPiaHelper2.X), Expression.Constant(NoPiaHelperClass.NoPiaHelper.X))
                );
            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaTry1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaTry1()
        {
            var exp = Expression.TryCatch(
                Expression.Constant(NoPiaHelperClass.NoPiaHelper.X),
                Expression.Catch(typeof(Exception),Expression.Constant(NoPiaHelper2.NoPiaHelper2.X))
                );
            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaTry2",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaTry2()
        {
            var exp = Expression.TryCatchFinally(
                Expression.Constant(NoPiaHelperClass.NoPiaHelper.X),
                Expression.Constant(NoPiaHelperClass.NoPiaHelper.X),
                Expression.Catch(typeof(Exception), Expression.Constant(NoPiaHelper2.NoPiaHelper2.X))
                );
            Expression.Lambda<Action>(exp).Compile().Invoke();
        }


        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaTry3",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaTry3()
        {
            Expression.TryFault(
                Expression.Constant(NoPiaHelperClass.NoPiaHelper.X),
                Expression.Constant(NoPiaHelper2.NoPiaHelper2.X)
            );
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaTry4",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaTry4()
        {
            var exp = Expression.TryFinally (
                Expression.Constant(NoPiaHelperClass.NoPiaHelper.X),
                Expression.Constant(NoPiaHelper2.NoPiaHelper2.X)
            );

            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaTypeEqual1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaTypeEqual1()
        {
            var exp = Expression.TypeEqual(
                Expression.Constant(NoPiaHelperClass.NoPiaHelper.X),
                NoPiaHelper2.NoPiaHelper2.FooStructType 
            );

            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaTypeIs1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaTypeIs1()
        {
            var exp = Expression.TypeIs(
                Expression.Constant(NoPiaHelperClass.NoPiaHelper.X),
                NoPiaHelper2.NoPiaHelper2.FooStructType
            );

            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

        [ETUtils.TestAttribute( ETUtils.TestState.Enabled,"NoPiaUnbox1",new string[]{"positive", "NoPia", "FullTrustOnly"})]
        public static void NoPiaUnbox1()
        {
            var exp = Expression.Unbox(
                Expression.Convert(Expression.Constant(NoPiaHelperClass.NoPiaHelper.X), typeof(object)),
                NoPiaHelper2.NoPiaHelper2.FooStructType
            );

            Expression.Lambda<Action>(exp).Compile().Invoke();
        }

    }
}
