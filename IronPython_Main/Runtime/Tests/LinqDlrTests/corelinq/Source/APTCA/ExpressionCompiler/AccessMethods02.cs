extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;

namespace APTCATest
{
    public partial class AccessMembersTests
    {
        #region "Access Class Methods"
        /// <summary>
        /// Method Signature: protected PublicClass(string str)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ExecProtectedConstructor()
        {
            ConstructorInfo ctor = typeof(PublicClass).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string) }, null);
            Expression[] args = new Expression[] { Expression.Constant("Protected Ctor String", typeof(string)) };

            NewExpression nex = Expression.New(ctor, args);
            Expression<Func<PublicClass, PublicClass>> expr = Expression.Lambda<Func<PublicClass, PublicClass>>(nex, classPara);
            try
            {
                Func<PublicClass, PublicClass> func = expr.Compile();
                try
                {
                    PublicClass returnValue = func(classObj);
                }
                catch (Exception ex)
                {
                    return ExceptionHandler(ex, ExceptionSource.ExecFunc);
                }
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.Compile);
            }

            return 0;
        }

        /// <summary>
        /// public bool PublicMethodRefOutParas(string name, uint count, ref int refValue, out int outValue)
        /// COMPARE
        /// public bool PublicMethodNORefOut(string name, uint count, int refValue, int outValue)
        /// 
        /// Bug#81446 resolution is 
        ///     Not allow byref parameter in Lambda and to throw runtime exception
        ///         - System.ArgumentException: A lambda expression may not contain byref parameters.
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int DDB81446_ExecPublicMethodWithRefOutPara()
        {
            ParameterExpression[] paras = new ParameterExpression[] { 
                Expression.Parameter(typeof(string), "p0"), 
                Expression.Parameter(typeof(uint), "p1"), 
                Expression.Parameter(typeof(int), "p2"),
                Expression.Parameter(typeof(int), "p3") };

#if false // access non ref/out params method as comparsion
            MethodCallExpression callExp1 = Expression.Call(classCE, typeof(PublicClass).GetMethod("PublicMethodNORefOut", BindingFlags.Public | BindingFlags.Instance), paras);
            Expression<Delegate5Paras> expr1 = Expression.Lambda<Delegate5Paras>(callExp1, paras);
            try
            {
                Delegate5Paras func1 = expr1.Compile();
                try
                {
                    bool returnValue1 = func1("abc", 9999, 0, 1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("COMP for - ublicMethodNORefOut - failed Flag=" + AssemblyA.permFlag.ToString());
                    Console.WriteLine(ex);
                    // return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FUNC for - ublicMethodNORefOut - failed Flag=" + AssemblyA.permFlag.ToString());
                Console.WriteLine(ex);
                // return 1;
            }
#endif
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClass).GetMethod("PublicMethodRefOutParas", BindingFlags.Public | BindingFlags.Instance), paras);

            try
            {
                Expression<DelegateWithRefOut> expr = Expression.Lambda<DelegateWithRefOut>(callExp, paras);
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine("Expected Exception throw - " + ae.ToString());
                return 0;
            }
            // Should throw exception (DDB#81447)
            return 1;
        }

        /// <summary>
        /// Expect FieldAccessException (Failed with A2B,C and D even having RMC and MC)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ExecLambdaInvokation()
        {
            Expression<Func<DerivedClass, int>> expr = x => x.GetSomeValueThroughReflection();
            try
            {
                Func<DerivedClass, int> func = expr.Compile();
                try
                {
                    int returnValue = func(derivedObj);
                }
                catch (FieldAccessException fx)
                {
                    Console.WriteLine("Expected Exception - " + fx.ToString());
                    return 0;
                }
                catch (Exception ex)
                {
                    return ExceptionHandler(ex, ExceptionSource.ExecFunc);
                }
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.Compile);
            }
            return 0;
        }

        /// <summary>
        /// virtual public int VirtualMethod001(int range)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ExecVirtualMethod()
        {
            Expression<Func<DerivedClass, int, int>> expr = (x, y) => x.VirtualMethod001(y);
            Func<DerivedClass, int, int> func = expr.Compile();
            int returnValue = func(derivedObj, 66);
            return 0;
        }

        /// <summary>
        /// override public void VirtualMethodDoNothing()
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int OverrideVirtualMethod()
        {
            Expression<Action<DerivedClass>> expr = x => x.VirtualMethodDoNothing();
            Action<DerivedClass> func = expr.Compile();
            func(derivedObj);
            return 0;
        }

        /// <summary>
        /// new public [protected internal] long VirtualMethod002(string str)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int HideVirtualMethod()
        {
            Expression<Func<DerivedClass, string, long>> expr = (x, y) => x.VirtualMethod002(y);
            Func<DerivedClass, string, long> func = expr.Compile();

            string str = "HI";
            long returnValue = func(derivedObj, str);
            return 0;
        }

        #endregion

        #region "Access Struct Methods"
        /// <summary>
        /// public void PublicMethodWithOut(int number, out string outString)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ExecPublicMethodWithOutPara()
        {
            ParameterExpression[] paras = new ParameterExpression[] { 
                Expression.Parameter(typeof(int), "p1"),
                Expression.Parameter(typeof(string), "p2")};
            MethodCallExpression callExp = Expression.Call(structCE, typeof(PublicStruct).GetMethod("PublicMethodWithOut", BindingFlags.Public | BindingFlags.Instance), paras);
            try
            {
                Expression<DelegateWithOut> expr = Expression.Lambda<DelegateWithOut>(callExp, paras);
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine("Expected Exception throw - " + ae.ToString());
                return 0;
            }

            // Should throw exception (DDB#81447)
            return 1;
        }

        /// <summary>
        /// public void PublicMethodWithParams(params char[] charAry)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ExecPublicMethodWithParams()
        {
            ParameterExpression[] paras = new ParameterExpression[] { Expression.Parameter(typeof(char[]), "p0") };
            MethodCallExpression callExp = Expression.Call(structCE, typeof(PublicStruct).GetMethod("PublicMethodWithParams", BindingFlags.Public | BindingFlags.Instance), paras);
            Expression<DelegateWithParams> expr = Expression.Lambda<DelegateWithParams>(callExp, paras);
            // Compile
            DelegateWithParams func = expr.Compile();
            // Run
            char[] chAry = new char[] { 'A', 'B', 'C' };
            func(chAry);

            return 0;
        }

        /// <summary>
        /// internal List<char> GetAllFirstChar(List<string> strList)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int DynamicInvokeMethodWithGeneric()
        {
            ParameterExpression[] paras = new ParameterExpression[] { Expression.Parameter(typeof(List<string>), "p0") };
            MethodCallExpression callExp = Expression.Call(structCE, typeof(PublicStruct).GetMethod("GetAllFirstChar", BindingFlags.NonPublic | BindingFlags.Instance), paras);
            LambdaExpression expr = Expression.Lambda(callExp, paras);
            // Compile
            List<string> strList = new List<string> { "111A", "3fdf", "5fdsfndslfjasldks", "788888888888888888888888" };
            try
            {
                List<char> chList = expr.Compile().DynamicInvoke(strList) as List<char>;
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.LambdaInvoke);
            }
            return 0;
        }
        #endregion
    }
}
