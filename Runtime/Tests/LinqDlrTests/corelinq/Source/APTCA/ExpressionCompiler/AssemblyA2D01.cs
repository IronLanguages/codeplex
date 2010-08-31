extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;


namespace APTCATest
{
    public class AssemblyA2D
    {
        static PublicClassD classObj = new PublicClassD();
        static PublicStructD structObj = new PublicStructD();

        static ConstantExpression classCE = Expression.Constant(new PublicClassD(), typeof(PublicClassD));
        static ConstantExpression structCE = Expression.Constant(new PublicStructD(), typeof(PublicStructD));

        static ParameterExpression classPara = Expression.Parameter(typeof(PublicClassD), "para");
        static ParameterExpression structPara = Expression.Parameter(typeof(PublicStructD), "spara");

        #region "Access Class Fields"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDPublicField()
        {
            MemberExpression mex = Expression.Field(null, typeof(PublicClassD).GetField("sPublicBool", BindingFlags.Public | BindingFlags.Static));
            Expression<Func<bool>> expr = Expression.Lambda<Func<bool>>(mex, null);
            // 
            Func<bool> func = expr.Compile();
            try
            {
                bool returnValue = func();
            }
            catch (SecurityException)
            {
                // expected fail
                if (PermissionFlag.NoMemberAccess == AssemblyA.permFlag)
                    return 0;
                else
                    return 1;
            }

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDInternalField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassD).GetField("internalShort", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassD, short>> expr = Expression.Lambda<Func<PublicClassD, short>>(mex, classPara);
            // Compile should always PASS
            Func<PublicClassD, short> func = expr.Compile();
            int nRet = 0;
            try
            {
                short returnValue = func(classObj);
            }
            catch (SecurityException)
            {
                // expected fail
                if (PermissionFlag.NoMemberAccess != AssemblyA.permFlag)
                    nRet = 1;
            }
            mex = Expression.Field(null, typeof(PublicClassD).GetField("sInternalDecimal", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<decimal>> expr1 = Expression.Lambda<Func<decimal>>(mex, null);
            // Compile should always PASS
            Func<decimal> func1 = expr1.Compile();
            try
            {
                decimal fieldValue1 = func1();
            }
            catch (SecurityException)
            {
                // expected fail
                if (PermissionFlag.NoMemberAccess == AssemblyA.permFlag)
                    nRet = 1;
            }
            return nRet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDPortectedInternalField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassD).GetField("protectedInternalLong", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassD, long>> expr = Expression.Lambda<Func<PublicClassD, long>>(mex, classPara);
            // Compile should always PASS
            Func<PublicClassD, long> func = expr.Compile();
            long returnValue = func(classObj);

            mex = Expression.Field(null, typeof(PublicClassD).GetField("sProtectedInternalDouble", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<double>> expr1 = Expression.Lambda<Func<double>>(mex, null);
            // Compile should always PASS
            Func<double> func1 = expr1.Compile();
            double returnValue1 = func1();

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDProtectedField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassD).GetField("protectedInt", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassD, int>> expr = Expression.Lambda<Func<PublicClassD, int>>(mex, classPara);
            // 
            Func<PublicClassD, int> func = expr.Compile();
            int returnValue = func(classObj);

            // static private string
            mex = Expression.Field(classPara, typeof(PublicClassD).GetField("sProtectedFloat", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<PublicClassD, float>> expr1 = Expression.Lambda<Func<PublicClassD, float>>(mex, classPara);
            Func<PublicClassD, float> func1 = expr1.Compile();
            float returnValue1 = func1(classObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDPrivateField()
        {
            // private string
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassD).GetField("privateString", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassD, string>> expr = Expression.Lambda<Func<PublicClassD, string>>(mex, classPara);
            Func<PublicClassD, string> func = expr.Compile();
            string returnValue = func(classObj);

            // static private string
            mex = Expression.Field(classPara, typeof(PublicClassD).GetField("sPrivateString", BindingFlags.NonPublic | BindingFlags.Static));
            expr = Expression.Lambda<Func<PublicClassD, string>>(mex, classPara);
            func = expr.Compile();
            returnValue = func(classObj);
            return 0;
        }
        #endregion

        #region "Access Class Properties"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDPublicProperty()
        {
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassD).GetProperty("PublicProperty", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<PublicClassD, short>> expr = Expression.Lambda<Func<PublicClassD, short>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassD, short> func = expr.Compile();
            short returnValue = func(classObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDInternalProperty()
        {
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassD).GetProperty("InternalProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassD, int>> expr = Expression.Lambda<Func<PublicClassD, int>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassD, int> func = expr.Compile();
            int returnValue = func(classObj);
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDProtectedPorperty()
        {
            // private string
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassD).GetProperty("ProtectedProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassD, long>> expr = Expression.Lambda<Func<PublicClassD, long>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassD, long> func = expr.Compile();
            long returnValue = func(classObj);
            return 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDPrivatePorperty()
        {
            // private string
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassD).GetProperty("PrivateProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassD, string>> expr = Expression.Lambda<Func<PublicClassD, string>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassD, string> func = expr.Compile();
            string returnValue = func(classObj);
            return 0;
        }

        #endregion

        #region "Access Class Methods"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDPublicMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClassD).GetMethod("PublicMethod", BindingFlags.Public | BindingFlags.Instance));
            Expression<Action> expr = Expression.Lambda<Action>(callExp, null);
            // Compile should always PASS
            Action func = expr.Compile();
            func();
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDInternalMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClassD).GetMethod("InternalMethod", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<string>> expr = Expression.Lambda<Func<string>>(callExp, null);
            // Compile should always PASS
            Func<string> func = expr.Compile();
            string returnValue = func();
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDProtectedMethod()
        {
            // private string
            MethodCallExpression callExp = Expression.Call(classPara, typeof(PublicClassD).GetMethod("ProtectedMethod", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassD, decimal>> expr = Expression.Lambda<Func<PublicClassD, decimal>>(callExp, classPara);
            // Compile should always PASS
            Func<PublicClassD, decimal> func = expr.Compile();
            decimal returnValue = func(classObj);
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassDPrivateMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClassD).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Action> expr = Expression.Lambda<Action>(callExp, null);

            Action func = expr.Compile();
            func();
            return 0;
        }

        #endregion

        #region "Access Struct Fields"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructDPublicField()
        {
            MemberExpression mex = Expression.Field(null, typeof(PublicStructD).GetField("sPublicChar", BindingFlags.Public | BindingFlags.Static));
            Expression<Func<char>> expr = Expression.Lambda<Func<char>>(mex, null);
            // Compile should always PASS
            Func<char> func = expr.Compile();
            char returnValue = func();

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructDInternalField()
        {
            MemberExpression mex = Expression.Field(structPara, typeof(PublicStructD).GetField("internalSbyte", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStructD, sbyte>> expr = Expression.Lambda<Func<PublicStructD, sbyte>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructD, sbyte> func = expr.Compile();
            sbyte returnValue = func(structObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructDPrivateField()
        {
            MemberExpression mex = Expression.Field(structPara, typeof(PublicStructD).GetField("sPrivateUshort", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<PublicStructD, ushort>> expr = Expression.Lambda<Func<PublicStructD, ushort>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructD, ushort> func = expr.Compile();
            ushort returnValue = func(structObj);
            return 0;
        }
        #endregion

        #region "Access Struct Properties"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructDPublicProperty()
        {
            MemberExpression mex = Expression.Property(structPara, typeof(PublicStructD).GetProperty("PublicGetSetField", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<PublicStructD, uint>> expr = Expression.Lambda<Func<PublicStructD, uint>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructD, uint> func = expr.Compile();
            uint returnValue = func(structObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructDInternalProperty()
        {
            MemberExpression mex = Expression.Property(structPara, typeof(PublicStructD).GetProperty("InternalGetSetField", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStructD, byte>> expr = Expression.Lambda<Func<PublicStructD, byte>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructD, byte> func = expr.Compile();
            byte returnValue = func(structObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructDPrivateProperty()
        {
            MemberExpression mex = Expression.Property(structPara, typeof(PublicStructD).GetProperty("PrivateProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStructD, sbyte>> expr = Expression.Lambda<Func<PublicStructD, sbyte>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructD, sbyte> func = expr.Compile();
            sbyte returnValue = func(structObj);
            return 0;
        }
        #endregion

        #region "Access Struct Methods"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructDPublicMethod()
        {
            MethodCallExpression callExp = Expression.Call(structCE, typeof(PublicStructD).GetMethod("PublicMethod", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<ushort>> expr = Expression.Lambda<Func<ushort>>(callExp, null);
            Func<ushort> func = expr.Compile();
            ushort returnValue = func();

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructDPrivateMethod()
        {
            MethodCallExpression callExp = Expression.Call(structCE, typeof(PublicStructD).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Action> expr = Expression.Lambda<Action>(callExp, new ParameterExpression[] { });
            // Compile should always PASS
            Action func = expr.Compile();
            func();
            return 0;
        }
        #endregion
    }
}
