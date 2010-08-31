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
        /// public void PublicMethod()
        /// static public void SPublicMethod(char ch)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassPublicMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClass).GetMethod("PublicMethod", BindingFlags.Public | BindingFlags.Instance));
            Expression<Action> expr = Expression.Lambda<Action>(callExp, null);
            // Compile
            Action func = expr.Compile();
            func();

            ParameterExpression[] pExpAry = new ParameterExpression[] { Expression.Parameter(typeof(char), "ch") };
            //callExp = Expression.Call(classCE, typeof(PublicClass).GetMethod("SPublicMethod", BindingFlags.Public | BindingFlags.Static), pExpAry);
	    callExp = Expression.Call(null, typeof(PublicClass).GetMethod("SPublicMethod", BindingFlags.Public | BindingFlags.Static), pExpAry);
            Expression<Action<char>> expr1 = Expression.Lambda<Action<char>>(callExp, pExpAry);
            // Compile
            Action<char> func1 = expr1.Compile();
            func1('Y');

            return 0;
        }

        /// <summary>
        /// string InternalMethod()
        /// static ulong SInternalMethod(ulong ulValue)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassInternalMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClass).GetMethod("InternalMethod", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<string>> expr = Expression.Lambda<Func<string>>(callExp, null);
            // Compile
            try
            {
                Func<string> func = expr.Compile();
                try
                {
                    string returnValue = func();
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
            ParameterExpression[] pExpAry = new ParameterExpression[] { Expression.Parameter(typeof(ulong), "ul") };
//            callExp = Expression.Call(classCE, typeof(PublicClass).GetMethod("SInternalMethod", BindingFlags.NonPublic | BindingFlags.Static), pExpAry);
            callExp = Expression.Call(null, typeof(PublicClass).GetMethod("SInternalMethod", BindingFlags.NonPublic | BindingFlags.Static), pExpAry);
            Expression<Func<ulong, ulong>> expr1 = Expression.Lambda<Func<ulong, ulong>>(callExp, pExpAry);
            // Compile
            try
            {
                Func<ulong, ulong> func1 = expr1.Compile();
                try
                {
                    ulong returnValue1 = func1(987654321);
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
        /// protected internal ArrayList ProtectedInternalMethod(int count)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassProtectedInternalMethod()
        {
            ParameterExpression[] pExpAry = new ParameterExpression[] { Expression.Parameter(typeof(int), "ct") };
            // 
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClass).GetMethod("ProtectedInternalMethod", BindingFlags.NonPublic | BindingFlags.Instance), pExpAry);
            Expression<Func<int, ArrayList>> expr = Expression.Lambda<Func<int, ArrayList>>(callExp, pExpAry);
            // Compile
            try
            {
                Func<int, ArrayList> func = expr.Compile();
                try
                {
                    ArrayList returnValue = func(0);

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
        /// protected decimal ProtectedMethod()
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassProtectedMethod()
        {
            // private string
            MethodCallExpression callExp = Expression.Call(classPara, typeof(PublicClass).GetMethod("ProtectedMethod", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClass, decimal>> expr = Expression.Lambda<Func<PublicClass, decimal>>(callExp, classPara);
            // Compile
            try
            {
                Func<PublicClass, decimal> func = expr.Compile();
                try
                {
                    decimal returnValue = func(classObj);
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
        /// private void PrivateMethod()
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassPrivateMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClass).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Action> expr = Expression.Lambda<Action>(callExp, null);
            try
            {
                Action func = expr.Compile();
                try
                {
                    func();
                }
                catch (Exception ex)
                {
                    return ExceptionHandler(ex, ExceptionSource.Action);
                }
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.Compile);
            }
            return 0;
        }

        #endregion

        #region "Access Struct Methods"
        /// <summary>
        /// public ushort PublicMethod()
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructPublicMethod()
        {
            MethodCallExpression callExp = Expression.Call(structCE, typeof(PublicStruct).GetMethod("PublicMethod", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<ushort>> expr = Expression.Lambda<Func<ushort>>(callExp, null);
            Func<ushort> func = expr.Compile();
            ushort returnValue = func();

            return 0;
        }

        /// <summary>
        /// Method Signature: internal void InternalMethod(int number)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructInternalMethod()
        {
            ParameterExpression p = Expression.Parameter(typeof(int), "p");
            MethodCallExpression callExp = Expression.Call(structCE, typeof(PublicStruct).GetMethod("InternalMethod", BindingFlags.NonPublic | BindingFlags.Instance), p);
            Expression<Action<int>> expr = Expression.Lambda<Action<int>>(callExp, p);
            try
            {
                Action<int> func = expr.Compile();
                try
                {
                    func(99);
                }
                catch (Exception ex)
                {
                    return ExceptionHandler(ex, ExceptionSource.Action);
                }
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.Compile);
            }
            return 0;
        }

        /// <summary>
        /// private void PrivateMethod()
        /// static private sbyte SPrivateMethod(byte btValue)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructPrivateMethod()
        {
            MethodCallExpression callExp = Expression.Call(structCE, typeof(PublicStruct).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Action> expr = Expression.Lambda<Action>(callExp, new ParameterExpression[] { });
            // 
            try
            {
                Action func = expr.Compile();
                try
                {
                    func();
                }
                catch (Exception ex)
                {
                    return ExceptionHandler(ex, ExceptionSource.Action);
                }
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.Compile);
            }

            ParameterExpression[] pExpAry = new ParameterExpression[] { Expression.Parameter(typeof(byte), "bt") };
            // 
//            callExp = Expression.Call(structCE, typeof(PublicStruct).GetMethod("SPrivateMethod", BindingFlags.NonPublic | BindingFlags.Static), pExpAry);
            callExp = Expression.Call(null, typeof(PublicStruct).GetMethod("SPrivateMethod", BindingFlags.NonPublic | BindingFlags.Static), pExpAry);
            Expression<Func<byte, sbyte>> expr1 = Expression.Lambda<Func<byte, sbyte>>(callExp, pExpAry);
            // Compile
            Func<byte, sbyte> func1 = expr1.Compile();
            sbyte returnValue1 = func1(255);

            return 0;
        }
        #endregion

    }
}
