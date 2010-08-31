extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;

namespace APTCATest
{
    public partial class AccessMembersTests
    {
        #region "Access Class Fields"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassPublicField()
        {
            MemberExpression mex = Expression.Field(null, typeof(PublicClass).GetField("sPublicBool", BindingFlags.Public | BindingFlags.Static));
            Expression<Func<bool>> expr = Expression.Lambda<Func<bool>>(mex, null);
            // 
            Func<bool> func = expr.Compile();
            bool returnValue = func();

            return 0;
        }

        /// <summary>
        /// Return upon any exception to simplifier code
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassInternalField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClass).GetField("internalShort", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClass, short>> expr = Expression.Lambda<Func<PublicClass, short>>(mex, classPara);

            Func<PublicClass, short> func = null;
            // Compile
            try
            {
                func = expr.Compile();

                try
                {
                    short returnValue = func(classObj);
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

            mex = Expression.Field(null, typeof(PublicClass).GetField("sInternalDecimal", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<decimal>> expr1 = Expression.Lambda<Func<decimal>>(mex, null);
            // Compile
            try
            {
                Func<decimal> func1 = expr1.Compile();

                try
                {
                    decimal fieldValue1 = func1();
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
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassPortectedInternalField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClass).GetField("protectedInternalLong", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClass, long>> expr = Expression.Lambda<Func<PublicClass, long>>(mex, classPara);
            // Compile
            try
            {
                Func<PublicClass, long> func = expr.Compile();
                try
                {
                    long returnValue = func(classObj);
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

            mex = Expression.Field(null, typeof(PublicClass).GetField("sProtectedInternalDouble", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<double>> expr1 = Expression.Lambda<Func<double>>(mex, null);
            // Compile
            try
            {
                Func<double> func1 = expr1.Compile();
                try
                {
                    double returnValue1 = func1();
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
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassProtectedField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClass).GetField("protectedInt", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClass, int>> expr = Expression.Lambda<Func<PublicClass, int>>(mex, classPara);
            // Compile
            try
            {
                Func<PublicClass, int> func = expr.Compile();

                try
                {
                    int returnValue = func(classObj);
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

            // static private string
            mex = Expression.Field(null, typeof(PublicClass).GetField("sProtectedFloat", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<PublicClass, float>> expr1 = Expression.Lambda<Func<PublicClass, float>>(mex, classPara);
            try
            {
                Func<PublicClass, float> func1 = expr1.Compile();
                try
                {
                    float returnValue1 = func1(classObj);
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
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassPrivateField()
        {
            // private string
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClass).GetField("privateString", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClass, string>> expr = Expression.Lambda<Func<PublicClass, string>>(mex, classPara);
            // Compile
            Func<PublicClass, string> func;
            string returnValue;
            try
            {
                func = expr.Compile();
                try
                {
                    returnValue = func(classObj);
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
            // static private string
            mex = Expression.Field(null, typeof(PublicClass).GetField("sPrivateString", BindingFlags.NonPublic | BindingFlags.Static));
            expr = Expression.Lambda<Func<PublicClass, string>>(mex, classPara);
            try
            {
                func = expr.Compile();
                try
                {
                    returnValue = func(classObj);
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
        #endregion

        #region "Access Struct Fields"
        /// <summary>
        /// public, static public fiels
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructPublicField()
        {
            MemberExpression mex = Expression.Field(null, typeof(PublicStruct).GetField("sPublicChar", BindingFlags.Public | BindingFlags.Static));
            Expression<Func<char>> expr = Expression.Lambda<Func<char>>(mex, null);
            //
            Func<char> func = expr.Compile();
            char returnValue = func();

            mex = Expression.Field(structPara, typeof(PublicStruct).GetField("publicChar", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<PublicStruct, char>> expr1 = Expression.Lambda<Func<PublicStruct, char>>(mex, structPara);
            //
            Func<PublicStruct, char> func1 = expr1.Compile();
            returnValue = func1(structObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructInternalField()
        {
            MemberExpression mex = Expression.Field(structPara, typeof(PublicStruct).GetField("internalSbyte", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStruct, sbyte>> expr = Expression.Lambda<Func<PublicStruct, sbyte>>(mex, structPara);
            try
            {
                Func<PublicStruct, sbyte> func = expr.Compile();
                try
                {
                    sbyte returnValue = func(structObj);
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
            mex = Expression.Field(null, typeof(PublicStruct).GetField("sInternalUint", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<uint>> expr1 = Expression.Lambda<Func<uint>>(mex, null);
            try
            {
                Func<uint> func1 = expr1.Compile();
                try
                {
                    uint returnValue1 = func1();
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
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructPrivateField()
        {
            MemberExpression mex = Expression.Field(structPara, typeof(PublicStruct).GetField("privateByte", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStruct, byte>> expr = Expression.Lambda<Func<PublicStruct, byte>>(mex, structPara);
            // Compile 
            try
            {
                Func<PublicStruct, byte> func = expr.Compile();
                try
                {
                    byte returnValue = func(structObj);
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
            mex = Expression.Field(null, typeof(PublicStruct).GetField("sPrivateUshort", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<PublicStruct, ushort>> expr1 = Expression.Lambda<Func<PublicStruct, ushort>>(mex, structPara);
            // Compile
            try
            {
                Func<PublicStruct, ushort> func1 = expr1.Compile();
                try
                {
                    ushort returnValue1 = func1(structObj);
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
        #endregion
    }
}
