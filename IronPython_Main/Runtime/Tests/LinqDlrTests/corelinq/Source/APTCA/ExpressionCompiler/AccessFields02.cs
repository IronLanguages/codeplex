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
        #region "Access Class Fields"
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int SetPrivateFieldMemberInit()
        {
            Expression fieldValue = Expression.Constant("string set by member binding", typeof(string));
            MemberAssignment mex = Expression.Bind(typeof(PublicClass).GetField("privateString", BindingFlags.NonPublic | BindingFlags.Instance), fieldValue);

            List<MemberBinding> bindings = new List<MemberBinding> { mex };
            MemberInitExpression mix = Expression.MemberInit(Expression.New(typeof(PublicClass)), bindings);
            Expression<Func<PublicClass, PublicClass>> expr = Expression.Lambda<Func<PublicClass, PublicClass>>(mix, classPara);
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
        /// try to get a value from private field and assign to a public field
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int GetPrivateFieldByBind()
        {
            MemberExpression source = Expression.Field(classPara, typeof(PublicClass).GetField("privateInt", BindingFlags.NonPublic | BindingFlags.Instance));
            // binding
            MemberAssignment mass = Expression.Bind(typeof(DerivedClass).GetField("intValue"), source);

            List<MemberBinding> bindings = new List<MemberBinding>{ mass };
            MemberInitExpression mix = Expression.MemberInit(Expression.New(typeof(DerivedClass)), bindings);
            Expression<Func<PublicClass, DerivedClass>> expr = Expression.Lambda<Func<PublicClass, DerivedClass>>(mix, classPara);
            try
            {
                Func<PublicClass, DerivedClass> func = expr.Compile();
                try 
                {
                    DerivedClass returnValue = func(classObj);
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
        /// Get public field value by Lambda
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int GetPublicFieldLambda()
        {
            LambdaExpression lambda = Expression.Lambda(Expression.PropertyOrField(classPara, "roPublicChar"), classPara);
            object returnValue = lambda.Compile().DynamicInvoke(classObj);
            return 0;
        }

        /// <summary>
        /// Get Protected field value by Lambda
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int GetProtectedFieldLambda()
        {
            LambdaExpression lambda = Expression.Lambda(Expression.Field(null, typeof(PublicClass).GetField("sProtectedFloat", BindingFlags.NonPublic | BindingFlags.Static)), new ParameterExpression[] { });
            try
            {
                object returnValue = lambda.Compile().DynamicInvoke();
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.LambdaInvoke);
            }
            return 0;
        }

        /// <summary>
        /// public List<long> numList;
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int SetPublicFieldListInit()
        {
            MethodInfo mi = typeof(List<long>).GetMethod("Add", new Type[] { typeof(long) });
            ElementInit[] paras = new ElementInit[] { 
                    Expression.ElementInit(mi, Expression.Constant(11111L, typeof(long))),
                    Expression.ElementInit(mi, Expression.Constant(22222L, typeof(long)))
                    };

            MemberListBinding mlb = Expression.ListBind(typeof(PublicClass).GetField("numList", BindingFlags.Public | BindingFlags.Instance), paras);
            // ConstructorInfo ci = typeof(PublicStruct).GetConstructor(new Type[] { typeof(long) });
            MemberInitExpression lix = Expression.MemberInit(Expression.New(typeof(PublicClass)), new MemberBinding[] { mlb });
            Expression<Func<PublicClass, PublicClass>> expr = Expression.Lambda<Func<PublicClass, PublicClass>>(lix, classPara);
            Func<PublicClass, PublicClass> func = expr.Compile();
            PublicClass returnValue = func(classObj);
            // 11111
            Console.WriteLine(returnValue.numList[0]);
            return 0;
        }

        #endregion

        #region "Access Struct Fields"
        /// <summary>
        /// public List<short> numList;
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int SetPublicFieldListBind()
        {
            MethodInfo mi = typeof(List<short>).GetMethod("Add", new Type[] { typeof(short) });
            ElementInit[] paras = new ElementInit[] { 
                    Expression.ElementInit(mi, Expression.Constant((short)6, typeof(short))),
                    Expression.ElementInit(mi, Expression.Constant((short)9, typeof(short)))
                    };

            MemberListBinding mlb = Expression.ListBind(typeof(PublicStruct).GetField("numList", BindingFlags.Public | BindingFlags.Instance), paras);
            ConstructorInfo ci = typeof(PublicStruct).GetConstructor(new Type[] { typeof(short) });
            MemberInitExpression lix = Expression.MemberInit(Expression.New(ci, new Expression[] { Expression.Constant(3) }), new MemberBinding[] { mlb });
            Expression<Func<PublicStruct, PublicStruct>> expr = Expression.Lambda<Func<PublicStruct, PublicStruct>>(lix, structPara);
            Func<PublicStruct, PublicStruct> func = expr.Compile();
            PublicStruct returnValue = func(structObj);
            // 6
            Console.WriteLine(returnValue.numList[0]);
            return 0;
        }

        #endregion
    }
}
