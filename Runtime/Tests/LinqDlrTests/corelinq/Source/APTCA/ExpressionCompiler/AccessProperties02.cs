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
        #region "Access Class Properties"

        [APTCATest]
        static public int GetPrivatePropertyMakeMemberAccess()
        {
            MemberExpression mex = Expression.MakeMemberAccess(classPara, typeof(PublicClass).GetProperty("PrivateProperty", BindingFlags.NonPublic | BindingFlags.Instance));
            Expression<Func<PublicClass, string>> expr = Expression.Lambda<Func<PublicClass, string>>(mex, classPara);
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
            return 0;
        }

        /// <summary>
        /// protected long ProtectedProperty
        /// </summary>
        /// <returns></returns>
        [APTCATest]
        static public int SetProtectedPropertyMemberInit()
        {
            Expression fieldValue = Expression.Constant(666666L, typeof(long));
            MemberAssignment mex = Expression.Bind(typeof(PublicClass).GetProperty("ProtectedProperty", BindingFlags.NonPublic | BindingFlags.Instance), fieldValue);

            List<MemberBinding> bindings = new List<MemberBinding> { mex };
            MemberInitExpression mix = Expression.MemberInit(Expression.New(typeof(PublicClass)), bindings);
            Expression<Func<PublicClass, PublicClass>> expr = Expression.Lambda<Func<PublicClass, PublicClass>>(mix, classPara);
            //
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
        /// Get public property value by Lambda
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int GetPublicPropertyLambda()
        {
            LambdaExpression lambda = Expression.Lambda(Expression.PropertyOrField(classPara, "PublicProperty"), classPara);
            object returnValue = lambda.Compile().DynamicInvoke(classObj);
            return 0;
        }

        /// <summary>
        /// Get Private property value by Lambda
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int GetPrivatePropertyLambda()
        {
            LambdaExpression lambda = Expression.Lambda(Expression.PropertyOrField(classPara, "PrivateProperty"), classPara);
            try
            {
                object returnValue = lambda.Compile().DynamicInvoke(classObj);
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.LambdaInvoke);
            }
            return 0;
        }
        #endregion

        #region "Access Struct Properties"
        /// <summary>
        /// Get Internal Property value by Lambda
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int GetInternalPropertyLambda()
        {
            LambdaExpression lambda = Expression.Lambda(Expression.PropertyOrField(structPara, "InternalProperty"), structPara);
            try
            {
                byte? returnValue = lambda.Compile().DynamicInvoke(structObj) as byte?;
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
