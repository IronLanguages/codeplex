using System;
using System.Linq.Expressions;
using System.Security;
using System.Security.Permissions;

[assembly: AllowPartiallyTrustedCallers]

namespace InternalTypeTest
{
    public class InternalTest
    {
        public static void RunTest()
        {
            Expression funcParamEx = Expression.Lambda<Func<PassMe>>(Expression.Constant(null, typeof(PassMe)));
            Expression constructorEx = Expression.New(typeof(ConstructMe).GetConstructors()[0], funcParamEx);

            Func<ConstructMe> compiled = Compile(Expression.Lambda<Func<ConstructMe>>(constructorEx));
            // Utils.Throws in SecurityTests.Scenarios.TreeWithInternalTypeTest will swallow a SecurityException or
            // MethodAccessException from anywhere in this test. Because this is a more complicated situation we
            // want to ensure the test is failing exactly at this call (e.x. were we checking for SecurityException it 
            // could be thrown on tree compilation if a regression were introduced and the test would keep passing).
            // As a result we'll just throw an ArgumentException (a custom exception type
            // would need to be defined in SecurityTests.Utils and add a reference to this class which may affect security
            // related to signing, I'm not sure).
            try
            {
                var result = compiled(); // Used to cause SecurityException in partial trust on x64 only (bug was that it didn't throw on x86)
            }
            catch (MethodAccessException e)
            {
                throw new ArgumentException("Expected", e);
            }
        }

        // We need these attributes and the assembly to be signed in order for this to be allowed in the partial trust AppDomain
        // (otherwise we hit a SecurityException when we enter this method), so that we can progress to invoking the compiled tree.
        [SecuritySafeCritical]
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        private static T Compile<T>(Expression<T> expr)
        {
            return expr.Compile();
        }
    }

    internal class PassMe
    { }

    internal class ConstructMe
    {
        public ConstructMe(Func<PassMe> func)
        { }
    }
}
