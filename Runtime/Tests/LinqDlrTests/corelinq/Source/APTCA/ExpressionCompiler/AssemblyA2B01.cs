extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;


namespace APTCATest
{
    public class AssemblyA2B
    {
        static PublicClassB classObj = new PublicClassB();
        static PublicStructB structObj = new PublicStructB();

        static ConstantExpression classCE = Expression.Constant(new PublicClassB(), typeof(PublicClassB));
        static ConstantExpression structCE = Expression.Constant(new PublicStructB(), typeof(PublicStructB));

        static ParameterExpression classPara = Expression.Parameter(typeof(PublicClassB), "para");
        static ParameterExpression structPara = Expression.Parameter(typeof(PublicStructB), "spara");

        #region "Access Class Fields"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassBPublicField()
        {
            MemberExpression mex = Expression.Field(null, typeof(PublicClassB).GetField("sPublicBool", BindingFlags.Public | BindingFlags.Static));
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
        static public int AccessClassBInternalField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassB).GetField("internalShort", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassB, short>> expr = Expression.Lambda<Func<PublicClassB, short>>(mex, classPara);
            // Compile should always PASS
            Func<PublicClassB, short> func = expr.Compile();
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
            mex = Expression.Field(null, typeof(PublicClassB).GetField("sInternalDecimal", BindingFlags.NonPublic | BindingFlags.Static));
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
        static public int AccessClassBPortectedInternalField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassB).GetField("protectedInternalLong", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassB, long>> expr = Expression.Lambda<Func<PublicClassB, long>>(mex, classPara);
            // Compile should always PASS
            Func<PublicClassB, long> func = expr.Compile();
            long returnValue = func(classObj);

            mex = Expression.Field(null, typeof(PublicClassB).GetField("sProtectedInternalDouble", BindingFlags.NonPublic | BindingFlags.Static));
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
        static public int AccessClassBProtectedField()
        {
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassB).GetField("protectedInt", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassB, int>> expr = Expression.Lambda<Func<PublicClassB, int>>(mex, classPara);
            // 
            Func<PublicClassB, int> func = expr.Compile();
            int returnValue = func(classObj);

            // static private string
            mex = Expression.Field(classPara, typeof(PublicClassB).GetField("sProtectedFloat", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<PublicClassB, float>> expr1 = Expression.Lambda<Func<PublicClassB, float>>(mex, classPara);
            Func<PublicClassB, float> func1 = expr1.Compile();
            float returnValue1 = func1(classObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassBPrivateField()
        {
            // private string
            MemberExpression mex = Expression.Field(classPara, typeof(PublicClassB).GetField("privateString", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassB, string>> expr = Expression.Lambda<Func<PublicClassB, string>>(mex, classPara);
            Func<PublicClassB, string> func = expr.Compile();
            string returnValue = func(classObj);

            // static private string
            mex = Expression.Field(classPara, typeof(PublicClassB).GetField("sPrivateString", BindingFlags.NonPublic | BindingFlags.Static));
            expr = Expression.Lambda<Func<PublicClassB, string>>(mex, classPara);
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
        static public int AccessClassBPublicProperty()
        {
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassB).GetProperty("PublicProperty", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<PublicClassB, short>> expr = Expression.Lambda<Func<PublicClassB, short>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassB, short> func = expr.Compile();
            short returnValue = func(classObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassBInternalProperty()
        {
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassB).GetProperty("InternalProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassB, int>> expr = Expression.Lambda<Func<PublicClassB, int>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassB, int> func = expr.Compile();
            // should PASS
            int returnValue = func(classObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassBProtectedPorperty()
        {
            // private string
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassB).GetProperty("ProtectedProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassB, long>> expr = Expression.Lambda<Func<PublicClassB, long>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassB, long> func = expr.Compile();
            long returnValue = func(classObj);
            return 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassBPrivatePorperty()
        {
            // private string
            MemberExpression mExp = Expression.Property(classPara, typeof(PublicClassB).GetProperty("PrivateProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassB, string>> expr = Expression.Lambda<Func<PublicClassB, string>>(mExp, classPara);
            // Compile should always PASS
            Func<PublicClassB, string> func = expr.Compile();
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
        static public int AccessClassBPublicMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClassB).GetMethod("PublicMethod", BindingFlags.Public | BindingFlags.Instance));
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
        static public int AccessClassBInternalMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClassB).GetMethod("InternalMethod", BindingFlags.NonPublic | BindingFlags.Instance));
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
        static public int AccessClassBPrivateMethod()
        {
            MethodCallExpression callExp = Expression.Call(classCE, typeof(PublicClassB).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance));
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
        static public int AccessStructBPublicField()
        {
            MemberExpression mex = Expression.Field(null, typeof(PublicStructB).GetField("sPublicChar", BindingFlags.Public | BindingFlags.Static));
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
        static public int AccessStructBInternalField()
        {
            MemberExpression mex = Expression.Field(structPara, typeof(PublicStructB).GetField("internalSbyte", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStructB, sbyte>> expr = Expression.Lambda<Func<PublicStructB, sbyte>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructB, sbyte> func = expr.Compile();
            sbyte returnValue = func(structObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructBPrivateField()
        {
            MemberExpression mex = Expression.Field(structPara, typeof(PublicStructB).GetField("sPrivateUshort", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<PublicStructB, ushort>> expr = Expression.Lambda<Func<PublicStructB, ushort>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructB, ushort> func = expr.Compile();
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
        static public int AccessStructBPublicProperty()
        {
            MemberExpression mex = Expression.Property(structPara, typeof(PublicStructB).GetProperty("PublicGetSetField", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<PublicStructB, uint>> expr = Expression.Lambda<Func<PublicStructB, uint>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructB, uint> func = expr.Compile();
            uint returnValue = func(structObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructBInternalProperty()
        {
            MemberExpression mex = Expression.Property(structPara, typeof(PublicStructB).GetProperty("InternalGetSetField", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStructB, byte>> expr = Expression.Lambda<Func<PublicStructB, byte>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructB, byte> func = expr.Compile();
            byte returnValue = func(structObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructBPrivateProperty()
        {
            MemberExpression mex = Expression.Property(structPara, typeof(PublicStructB).GetProperty("PrivateProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicStructB, sbyte>> expr = Expression.Lambda<Func<PublicStructB, sbyte>>(mex, structPara);
            // Compile should always PASS
            Func<PublicStructB, sbyte> func = expr.Compile();
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
        static public int AccessStructBPublicMethod()
        {
            MethodCallExpression callExp = Expression.Call(structCE, typeof(PublicStructB).GetMethod("PublicMethod", BindingFlags.Public | BindingFlags.Instance));
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
        static public int AccessStructBPrivateMethod()
        {
            MethodCallExpression callExp = Expression.Call(null, typeof(PublicStructB).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Action> expr = Expression.Lambda<Action>(callExp, new ParameterExpression[] { });
            // 
            Action func = expr.Compile();
            func();
            return 0;
        }
        #endregion
    }
}
