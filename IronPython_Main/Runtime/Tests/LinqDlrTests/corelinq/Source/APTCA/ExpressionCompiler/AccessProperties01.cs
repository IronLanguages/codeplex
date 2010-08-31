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
        #region "Access Class Properties"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassPublicProperty()
        {
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClass).GetProperty("PublicProperty", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<PublicClass, short>> expr = Expression.Lambda<Func<PublicClass, short>>(mExp, classPara);
            // Compile
            Func<PublicClass, short> func = expr.Compile();
            short returnValue = func(classObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassInternalProperty()
        {
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClass).GetProperty("InternalProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClass, int>> expr = Expression.Lambda<Func<PublicClass, int>>(mExp, classPara);
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

            mExp = Expression.Property(null, typeof(PublicClass).GetProperty("StaticInternalAIGetSet", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<int>> expr1 = Expression.Lambda<Func<int>>(mExp, null);
            // Compile
            try
            {
                Func<int> func1 = expr1.Compile();
                try
                {
                    func1();
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
        /// protected internal double ProtectedInternalProperty
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassProtectedInternalProperty()
        {
            // private string
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClass).GetProperty("ProtectedInternalProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClass, double>> expr = Expression.Lambda<Func<PublicClass, double>>(mExp, classPara);
            // Compile
            try
            {
                Func<PublicClass, double> func = expr.Compile();
                try
                {
                    double returnValue = func(classObj);
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
        static public int AccessClassProtectedProperty()
        {
            // private string
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClass).GetProperty("ProtectedProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClass, long>> expr = Expression.Lambda<Func<PublicClass, long>>(mExp, classPara);
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
            return 0;
        }

        /// <summary>
        /// Prop: private string PrivateProperty
        /// Prop: static private float SPrivateProperty
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassPrivateProperty()
        {
            // private string
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClass).GetProperty("PrivateProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClass, string>> expr = Expression.Lambda<Func<PublicClass, string>>(mExp, classPara);
            // Compile
            try
            {
                Func<PublicClass, string> func = expr.Compile();
                try
                {
                    string returnValue = func(classObj);
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

            mExp = Expression.Property(null, typeof(PublicClass).GetProperty("SPrivateProperty", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<PublicClass, float>> expr1 = Expression.Lambda<Func<PublicClass, float>>(mExp, classPara);
            // Compile
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

        #endregion

        #region "Access Struct Properties"
        /// <summary>
        /// public uint PublicProperty
        /// static public ushort SPublicProperty
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructPublicProperty()
        {
            MemberExpression mExp = Expression.Property(structPara, typeof(PublicStruct).GetProperty("PublicProperty", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<PublicStruct, uint>> expr = Expression.Lambda<Func<PublicStruct, uint>>(mExp, structPara);
            // Compile
            Func<PublicStruct, uint> func = expr.Compile();
            uint returnValue = func(structObj);

            mExp = Expression.Property(null, typeof(PublicStruct).GetProperty("SPublicProperty", BindingFlags.Public | BindingFlags.Static));
            Expression<Func<ushort>> expr1 = Expression.Lambda<Func<ushort>>(mExp, null);
            // Compile
            Func<ushort> func1 = expr1.Compile();
            ushort returnValue1 = func1();

            return 0;
        }

        /// <summary>
        /// internal byte InternalProperty
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructInternalProperty()
        {
            MemberExpression mExp = Expression.Property(structPara, typeof(PublicStruct).GetProperty("InternalProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStruct, byte>> expr = Expression.Lambda<Func<PublicStruct, byte>>(mExp, structPara);
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

            return 0;
        }

        /// <summary>
        /// private sbyte PrivateProperty
        /// static private char PrivateAIGetSetField
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructPrivateProperty()
        {
            MemberExpression mExp = Expression.Property(structPara, typeof(PublicStruct).GetProperty("PrivateProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStruct, sbyte>> expr = Expression.Lambda<Func<PublicStruct, sbyte>>(mExp, structPara);
            // Compile
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
            mExp = Expression.Property(null, typeof(PublicStruct).GetProperty("PrivateAIGetSetField", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<PublicStruct, char>> expr1 = Expression.Lambda<Func<PublicStruct, char>>(mExp, structPara);
            // Compile 
            try
            {
                Func<PublicStruct, char> func1 = expr1.Compile();
                try
                {
                    char returnValue1 = func1(structObj);
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
