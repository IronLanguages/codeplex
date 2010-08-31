extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;


namespace APTCATest
{
    public class AssemblyA2A
    {
        static PublicClassA classAObj = new PublicClassA();
        static InternalStructA structAObj = new InternalStructA();

        static ConstantExpression classAce = Expression.Constant(new PublicClassA(), typeof(PublicClassA));
        static ConstantExpression structAce = Expression.Constant(new InternalStructA(), typeof(InternalStructA));

        static ParameterExpression classAPara = Expression.Parameter(typeof(PublicClassA), "para");
        static ParameterExpression structAPara = Expression.Parameter(typeof(InternalStructA), "spara");

        #region "Access Class Fields"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassAPublicField()
        {
            MemberExpression mex = Expression.Field(null, typeof(PublicClassA).GetField("sPublicBool", BindingFlags.Public | BindingFlags.Static));
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
        static public int AccessClassAInternalField()
        {
            MemberExpression mex = Expression.Field(classAPara, typeof(PublicClassA).GetField("internalShort", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassA, short>> expr = Expression.Lambda<Func<PublicClassA, short>>(mex, classAPara);
            // Compile should always PASS
            Func<PublicClassA, short> func = expr.Compile();
            int nRet = 0;
            try
            {
                short returnValue = func(classAObj);
            }
            catch (SecurityException)
            {
                // expected fail
                if (PermissionFlag.NoMemberAccess != AssemblyA.permFlag)
                    nRet = 1;
            }
            mex = Expression.Field(null, typeof(PublicClassA).GetField("sInternalDecimal", BindingFlags.NonPublic | BindingFlags.Static));
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
        static public int AccessClassAPortectedInternalField()
        {
            MemberExpression mex = Expression.Field(classAPara, typeof(PublicClassA).GetField("protectedInternalLong", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassA, long>> expr = Expression.Lambda<Func<PublicClassA, long>>(mex, classAPara);
            // Compile should always PASS
            Func<PublicClassA, long> func = expr.Compile();
            long returnValue = func(classAObj);

            mex = Expression.Field(null, typeof(PublicClassA).GetField("sProtectedInternalDouble", BindingFlags.NonPublic | BindingFlags.Static));
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
        static public int AccessClassAProtectedField()
        {
            MemberExpression mex = Expression.Field(classAPara, typeof(PublicClassA).GetField("protectedInt", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassA, int>> expr = Expression.Lambda<Func<PublicClassA, int>>(mex, classAPara);
            // 
            Func<PublicClassA, int> func = expr.Compile();
            int returnValue = func(classAObj);

            // static private string
            mex = Expression.Field(classAPara, typeof(PublicClassA).GetField("sProtectedFloat", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<PublicClassA, float>> expr1 = Expression.Lambda<Func<PublicClassA, float>>(mex, classAPara);
            Func<PublicClassA, float> func1 = expr1.Compile();
            float returnValue1 = func1(classAObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassAPrivateField()
        {
            // private string
            MemberExpression mex = Expression.Field(classAPara, typeof(PublicClassA).GetField("privateString", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassA, string>> expr = Expression.Lambda<Func<PublicClassA, string>>(mex, classAPara);
            Func<PublicClassA, string> func = expr.Compile();
            string returnValue = func(classAObj);

            // static private string
            mex = Expression.Field(classAPara, typeof(PublicClassA).GetField("sPrivateString", BindingFlags.NonPublic | BindingFlags.Static));
            expr = Expression.Lambda<Func<PublicClassA, string>>(mex, classAPara);
            func = expr.Compile();
            returnValue = func(classAObj);
            return 0;
        }
        #endregion

        #region "Access Class Properties"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassAPublicProperty()
        {
            MemberExpression mExp = Expression.Property(classAPara, typeof(PublicClassA).GetProperty("PublicGetSetProperty", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<PublicClassA, short>> expr = Expression.Lambda<Func<PublicClassA, short>>(mExp, classAPara);
            // Compile should always PASS
            Func<PublicClassA, short> func = expr.Compile();
            short returnValue = func(classAObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassAInternalProperty()
        {
            MemberExpression mExp = Expression.Property(classAPara, typeof(PublicClassA).GetProperty("InternalGetProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassA, int>> expr = Expression.Lambda<Func<PublicClassA, int>>(mExp, classAPara);
            // Compile should always PASS
            Func<PublicClassA, int> func = expr.Compile();
            // should PASS
            int returnValue = func(classAObj);

            mExp = Expression.Property(null, typeof(PublicClassA).GetProperty("StaticInternalAIGetSet", BindingFlags.NonPublic | BindingFlags.Static));
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
        static public int AccessClassAProtectedPorperty()
        {
            // private string
            MemberExpression mExp = Expression.Property(classAPara, typeof(PublicClassA).GetProperty("ProtectedGetField", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassA, long>> expr = Expression.Lambda<Func<PublicClassA, long>>(mExp, classAPara);
            // Compile should always PASS
            Func<PublicClassA, long> func = expr.Compile();
            long returnValue = func(classAObj);
            return 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassAPrivatePorperty()
        {
            // private string
            MemberExpression mExp = Expression.Property(classAPara, typeof(PublicClassA).GetProperty("PrivateGetSetField", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassA, string>> expr = Expression.Lambda<Func<PublicClassA, string>>(mExp, classAPara);
            // Compile should always PASS
            Func<PublicClassA, string> func = expr.Compile();
            string returnValue = func(classAObj);
            return 0;
        }

        #endregion

        #region "Access Class Methods"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassAPublicMethod()
        {
            MethodCallExpression callExp = Expression.Call(classAce, typeof(PublicClassA).GetMethod("PublicMethod", BindingFlags.Public | BindingFlags.Instance));
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
        static public int AccessClassAInternalMethod()
        {
            MethodCallExpression callExp = Expression.Call(classAce, typeof(PublicClassA).GetMethod("InternalMethod", BindingFlags.NonPublic | BindingFlags.Instance));
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
        static public int AccessClassAProtectedMethod()
        {
            // private string
            MethodCallExpression callExp = Expression.Call(classAPara, typeof(PublicClassA).GetMethod("ProtectedMethod", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClassA, decimal>> expr = Expression.Lambda<Func<PublicClassA, decimal>>(callExp, classAPara);
            // Compile should always PASS
            Func<PublicClassA, decimal> func = expr.Compile();
            decimal returnValue = func(classAObj);
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessClassAPrivateMethod()
        {
            MethodCallExpression callExp = Expression.Call(classAce, typeof(PublicClassA).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance));
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
        static public int AccessStructAPublicField()
        {
            MemberExpression mex = Expression.Field(null, typeof(InternalStructA).GetField("sPublicChar", BindingFlags.Public | BindingFlags.Static));
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
        static public int AccessStructAInternalField()
        {
            MemberExpression mex = Expression.Field(structAPara, typeof(InternalStructA).GetField("internalSbyte", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<InternalStructA, sbyte>> expr = Expression.Lambda<Func<InternalStructA, sbyte>>(mex, structAPara);
            // Compile should always PASS
            Func<InternalStructA, sbyte> func = expr.Compile();
            sbyte returnValue = func(structAObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructAPrivateField()
        {
            MemberExpression mex = Expression.Field(structAPara, typeof(InternalStructA).GetField("sPrivateUshort", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<InternalStructA, ushort>> expr = Expression.Lambda<Func<InternalStructA, ushort>>(mex, structAPara);
            // Compile should always PASS
            Func<InternalStructA, ushort> func = expr.Compile();
            ushort returnValue = func(structAObj);
            return 0;
        }
        #endregion

        #region "Access Struct Properties"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructAPublicProperty()
        {
            MemberExpression mex = Expression.Property(structAPara, typeof(InternalStructA).GetProperty("PublicGetSetField", BindingFlags.Public | BindingFlags.Instance));
            Expression<Func<InternalStructA, uint>> expr = Expression.Lambda<Func<InternalStructA, uint>>(mex, structAPara);
            // Compile should always PASS
            Func<InternalStructA, uint> func = expr.Compile();
            uint returnValue = func(structAObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructAInternalProperty()
        {
            MemberExpression mex = Expression.Property(structAPara, typeof(InternalStructA).GetProperty("InternalGetSetField", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<InternalStructA, byte>> expr = Expression.Lambda<Func<InternalStructA, byte>>(mex, structAPara);
            // Compile should always PASS
            Func<InternalStructA, byte> func = expr.Compile();
            byte returnValue = func(structAObj);

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructAPrivateProperty()
        {
            MemberExpression mex = Expression.Property(structAPara, typeof(InternalStructA).GetProperty("PrivateAIGetSetField", BindingFlags.NonPublic | BindingFlags.Static));
            Expression<Func<InternalStructA, char>> expr = Expression.Lambda<Func<InternalStructA, char>>(mex, structAPara);
            // Compile should always PASS
            Func<InternalStructA, char> func = expr.Compile();
            char returnValue = func(structAObj);
            return 0;
        }
        #endregion

        #region "Access Struct Methods"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int AccessStructAPublicMethod()
        {
            MethodCallExpression callExp = Expression.Call(structAce, typeof(InternalStructA).GetMethod("PublicMethod", BindingFlags.Public | BindingFlags.Instance));
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
        static public int AccessStructAPrivateMethod()
        {
            MethodCallExpression callExp = Expression.Call(structAce, typeof(InternalStructA).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Action> expr = Expression.Lambda<Action>(callExp, new ParameterExpression[] { });
            // Compile should always PASS
            Action func = expr.Compile();
            func();
            return 0;
        }
        #endregion
    }
}
