extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;


namespace APTCATest
{
    public class AssemblyA2C
    {
        static PublicClassC classObj = new PublicClassC();
        static PublicStructC structObj = new PublicStructC();

        static ConstantExpression classCE = Expression.Constant(new PublicClassC(), typeof(PublicClassC));
        static ConstantExpression structCE = Expression.Constant(new PublicStructC(), typeof(PublicStructC));

        static ParameterExpression classPara = Expression.Parameter(typeof(PublicClassC), "para");
        static ParameterExpression structPara = Expression.Parameter(typeof(PublicStructC), "spara");

        #region "Access Class Fields"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassCPublicField()
        {
            MemberExpression mex = Expression.Field(null, typeof(PublicClassC).GetField("sPublicBool", BindingFlags.Public | BindingFlags.Static));
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
        static public int AccessClassCInternalField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassC).GetField("internalShort", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassC, short>> expr = Expression.Lambda<Func<PublicClassC, short>>(mex, classPara);
            // Compile should always PASS
            Func<PublicClassC, short> func = expr.Compile();
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
            mex = Expression.Field(null, typeof(PublicClassC).GetField("sInternalDecimal", BindingFlags.NonPublic | BindingFlags.Static));
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
        static public int AccessClassCPortectedInternalField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassC).GetField("protectedInternalLong", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassC, long>> expr = Expression.Lambda<Func<PublicClassC, long>>(mex, classPara);
            // Compile should always PASS
            Func<PublicClassC, long> func = expr.Compile();
            long returnValue = func(classObj);

            mex = Expression.Field(null, typeof(PublicClassC).GetField("sProtectedInternalDouble", BindingFlags.NonPublic | BindingFlags.Static));
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
        static public int AccessClassCProtectedField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassC).GetField("protectedInt", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassC, int>> expr = Expression.Lambda<Func<PublicClassC, int>>(mex, classPara);
            // 
            Func<PublicClassC, int> func = expr.Compile();
            int returnValue = func(classObj);

            // static private string
            mex = Expression.Field(classPara, typeof(PublicClassC).GetField("sProtectedFloat", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<PublicClassC, float>> expr1 = Expression.Lambda<Func<PublicClassC, float>>(mex, classPara);
            Func<PublicClassC, float> func1 = expr1.Compile();
            float returnValue1 = func1(classObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassCPrivateField()
        {
            // private string
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassC).GetField("privateString", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassC, string>> expr = Expression.Lambda<Func<PublicClassC, string>>(mex, classPara);
            Func<PublicClassC, string> func = expr.Compile();
            string returnValue = func(classObj);

            // static private string
            mex = Expression.Field(classPara, typeof(PublicClassC).GetField("sPrivateString", BindingFlags.NonPublic | BindingFlags.Static));
            expr = Expression.Lambda<Func<PublicClassC, string>>(mex, classPara);
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
        static public int AccessClassCPublicProperty()
        {
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassC).GetProperty("PublicProperty", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<PublicClassC, short>> expr = Expression.Lambda<Func<PublicClassC, short>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassC, short> func = expr.Compile();
            short returnValue = func(classObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassCInternalProperty()
        {
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassC).GetProperty("InternalProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassC, int>> expr = Expression.Lambda<Func<PublicClassC, int>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassC, int> func = expr.Compile();
            // should PASS
            int returnValue = func(classObj);

            mExp = Expression.Property(null, typeof(PublicClassC).GetProperty("StaticInternalAIGetSet", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<int>> expr1 = Expression.Lambda<Func<int>>(mExp, null);
            // Compile should always PASS
            Func<int> func1 = expr1.Compile();
            returnValue = func1();

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassCProtectedPorperty()
        {
            // private string
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassC).GetProperty("ProtectedProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassC, long>> expr = Expression.Lambda<Func<PublicClassC, long>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassC, long> func = expr.Compile();
            long returnValue = func(classObj);
            return 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassCPrivatePorperty()
        {
            // private string
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassC).GetProperty("PrivateProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassC, string>> expr = Expression.Lambda<Func<PublicClassC, string>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassC, string> func = expr.Compile();
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
        static public int AccessClassCPublicMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClassC).GetMethod("PublicMethod", BindingFlags.Public | BindingFlags.Instance));
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
        static public int AccessClassCInternalMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClassC).GetMethod("InternalMethod", BindingFlags.NonPublic | BindingFlags.Instance));
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
        static public int AccessClassCPrivateMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClassC).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance));
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
        static public int AccessStructCPublicField()
        {
            MemberExpression mex = Expression.Field(null, typeof(PublicStructC).GetField("sPublicChar", BindingFlags.Public | BindingFlags.Static));
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
        static public int AccessStructCInternalField()
        {
            MemberExpression mex = Expression.Field(structPara, typeof(PublicStructC).GetField("internalSbyte", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStructC, sbyte>> expr = Expression.Lambda<Func<PublicStructC, sbyte>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructC, sbyte> func = expr.Compile();
            sbyte returnValue = func(structObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructCPrivateField()
        {
            MemberExpression mex = Expression.Field(structPara, typeof(PublicStructC).GetField("sPrivateUshort", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<PublicStructC, ushort>> expr = Expression.Lambda<Func<PublicStructC, ushort>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructC, ushort> func = expr.Compile();
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
        static public int AccessStructCPublicProperty()
        {
            MemberExpression mex = Expression.Property(structPara, typeof(PublicStructC).GetProperty("PublicGetSetField", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<PublicStructC, uint>> expr = Expression.Lambda<Func<PublicStructC, uint>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructC, uint> func = expr.Compile();
            uint returnValue = func(structObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructCInternalProperty()
        {
            MemberExpression mex = Expression.Property(structPara, typeof(PublicStructC).GetProperty("InternalGetSetField", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStructC, byte>> expr = Expression.Lambda<Func<PublicStructC, byte>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructC, byte> func = expr.Compile();
            byte returnValue = func(structObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructCPrivateProperty()
        {
            MemberExpression mex = Expression.Property(structPara, typeof(PublicStructC).GetProperty("PrivateProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStructC, char>> expr = Expression.Lambda<Func<PublicStructC, char>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructC, char> func = expr.Compile();
            char returnValue = func(structObj);
            return 0;
        }
        #endregion

        #region "Access Struct Methods"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructCPublicMethod()
        {
            MethodCallExpression callExp = Expression.Call(structCE, typeof(PublicStructC).GetMethod("PublicMethod", BindingFlags.Public | BindingFlags.Instance));
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
        static public int AccessStructCPrivateMethod()
        {
            MethodCallExpression callExp = Expression.Call(structCE, typeof(PublicStructC).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Action> expr = Expression.Lambda<Action>(callExp, new ParameterExpression[] { });
            // Compile should always PASS
            Action func = expr.Compile();
            func();
            return 0;
        }
        #endregion
    }
}
