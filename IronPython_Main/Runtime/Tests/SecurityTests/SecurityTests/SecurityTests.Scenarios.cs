using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;

namespace SecurityTests {
    /// <summary>
    /// Various scenarios to verify DLR security behavior with various AppDomain settings.
    /// Primarily focused on testing behavior in homogenous vs. heterogenous AppDomains (the latter does not allow rule compilation)
    /// as well as full trust vs. partial trust.
    /// 
    /// Limiting Factors in creating tests:
    ///     - Callsites and Binders aren't serializable. So anything requiring the serialization of a dynamic type across AppDomains results in a SerializationException
    ///       before the fun begins because the binders and callsites can't go with the object (ex AppDomain.DoCallBack( () => myIDMOP.SomeMethod() or manually creating
    ///       and invoking a Callsite Target).
    ///     - Defining a dynamic type in another AppDomain via AppDomain.CreateInstanceXXX requires specifiying the exact type to instantiate.
    ///       This means the object ends up being statically typed so no dynamic calls occur. If the proxy object is typed as dynamic it may result in a 
    ///       dynamic call in the other AppDomain (not sure) but first the initial call on the proxy object (which sends the message to the other AppDomain where the real work 
    ///       happens) will fail in the test driver's (heterogeneous) AppDomain.
    ///     - ILGenerator and its ilk also aren't serializable so we can't callback to another AppDomain directly injecting IL to get around the problems above.
    ///     - Can only use AppDomain.DefineDynamicAssembly in current AppDomain so no using it to define a partial trust assembly in passed in AppDomains.
    /// </summary>
    public partial class SecurityTests {
        public static string CallingAssembly = String.Format("{0}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Assembly.GetCallingAssembly().GetName().Name);
        public static string TestClass = "SecurityTests.TestClass";
        public static string LinqTestClass = "SecurityTests.LinqTestClass";

        // Testing: Rule compilation only allowed in homogenous AppDomains
        // Expected Result: Test should fail if AppDomain is heterogeneous
        public static void AppDomainDynamicTest(AppDomain a, bool ExpectsError) {
            TestClass t = (TestClass)a.CreateInstanceAndUnwrap(CallingAssembly, TestClass);
            bool result = Utils.Throws<InvalidOperationException>(ExpectsError, () => t.DoDynamic());
            Utils.LogResult("AppDomainDynamicTest", a, result);
        }

        // Testing: Instantiate a type in an AppDomain and invoke a member on that instance which results in a call that requires full trust
        // Expected Result: Test should only work in full trust
        public static void AppDomainTrustTest(AppDomain a, bool ExpectsError) {
            TestClass t = (TestClass)a.CreateInstanceAndUnwrap(CallingAssembly, TestClass);
            bool result = Utils.Throws<SecurityException>(ExpectsError, () => t.DoTrustedOperation());
            Utils.LogResult("AppDomainTrustTest", a, result);
        }

        // Testing: Instantiate a type in one AppDomain and in another AppDomain do a callback to that instance which results in a dynamic call
        // Expected Result: Test will fail if calling AppDomain is heterogeneous
        public static void CrossAppDomainDynamicTest(AppDomain a, AppDomain b, bool ExpectsError) {
            TestClass t = (TestClass)a.CreateInstanceAndUnwrap(CallingAssembly, TestClass);
            bool result = Utils.Throws<InvalidOperationException>(ExpectsError, () => b.DoCallBack(t.DoDynamic));
            Utils.LogResult("CrossAppDomainDynamicTest", result, a, b);
        }

        // Testing: Instantiate a type in one AppDomain and in another AppDomain do a callback to that instance which requires full trust
        // Expected Result: Test will fail if either AppDomain is only partial trust
        public static void CrossAppDomainTrustTest(AppDomain a, AppDomain b, bool ExpectsError) {
            TestClass t = (TestClass)a.CreateInstanceAndUnwrap(CallingAssembly, TestClass);
            bool result = Utils.Throws<SecurityException>(ExpectsError, () => b.DoCallBack(t.DoTrustedOperation));
            Utils.LogResult("CrossAppDomainTrustTest", result, a, b);
        }

        // Testing: A partially trusted LINQ provider attempts to pass a LINQ query to a full trust AppDomain to execute code it otherwise couldn't
        // Expected Result: Rule compilation only allowed in homogenous AppDomains, if either AppDomain is heterogenous or lacks SqlClientPermission this should fail.
        public static void PartialTrustLINQTest(AppDomain a, bool ExpectsError) {
            LinqTestClass t = (LinqTestClass)a.CreateInstanceAndUnwrap(CallingAssembly, LinqTestClass);
            bool result = Utils.Throws<SecurityException>(ExpectsError, () => t.DoQuery());
            Utils.LogResult("PartialTrustLINQTest", a, result);
            t.Dispose();
        }

        // Testing: Pass a LINQ provider a node type it doesn't understand
        // Expected Result: Failure depends on how the LINQ provider handles unknown nodes, LINQ to SQL should fail here
        public static void BadNodeLINQTest(AppDomain a, bool ExpectsError) {
            LinqTestClass t = (LinqTestClass)a.CreateInstanceAndUnwrap(CallingAssembly, LinqTestClass);

            bool result = Utils.Throws<ArgumentException>(ExpectsError, () => t.DoInvalidQuery());
            Utils.LogResult("BadNodeLINQTest", a, result);
            t.Dispose();
        }

        // Testing: Any operation on an IDMOP should only be allowed in homogeneous AppDomains
        // Expected Result: Failure if executing in a heterogeneous AppDomain
        public static void IDMOPTest(AppDomain a, bool ExpectsError) {
            TestClass t = (TestClass)a.CreateInstanceAndUnwrap(CallingAssembly, TestClass);
            bool result = Utils.Throws<InvalidOperationException>(ExpectsError, () => t.AccessIDO());
            Utils.LogResult("IDOTrustTest", a, result);
        }

        // Testing: Instantiate a type in one AppDomain and in another AppDomain do a callback to that instance which results in an operation on an IDMOP
        // Expected Result: Test will fail if calling AppDomain is heterogeneous or if calling AppDomain doesn't have ReflectionPermissions
        public static void CrossAppDomainIDMOPTest(AppDomain a, AppDomain b, bool ExpectsError) {
            TestClass t = (TestClass)a.CreateInstanceAndUnwrap(CallingAssembly, TestClass);
            bool result = Utils.Throws<InvalidOperationException>(ExpectsError, () => b.DoCallBack(t.AccessIDO));
            Utils.LogResult("CrossAppDomainIDMOPTest", result, a, b);
        }

        // Testing: In the given AppDomain execute an assembly which uses an IDMOP which performs file system manipulation in its binder
        // Expected Result: Failure if the given AppDomain doesn't have file system permissions (also if it's heterogeneous but that's not what this test is for)
        public static void ExecuteAssemblyTest(AppDomain a, bool ExpectsError) {
            bool result = Utils.Throws<SecurityException>(ExpectsError, () =>
            {
                a.ExecuteAssembly(System.Environment.GetEnvironmentVariable("DLR_BIN") + "\\DynamicTest.exe");
            }
            );

            Utils.LogResult("ExecuteAssemblyTest", a, result);
        }

        // Testing: Executing a method on an IDMOP that creates a COM object only works in Ap~pDomains with unmanaged code permissions
        // Expected Result: Test should fail when run in an AppDomain that doesn't have UnmanagedCode permissions
        public static void COMTrustTest(AppDomain a, bool ExpectsError)
        {
            MBRODynamicObject obj = (MBRODynamicObject)a.CreateInstanceAndUnwrap(CallingAssembly, "SecurityTests.MBRODynamicObject");
            bool result = Utils.Throws<SecurityException>(ExpectsError, () =>
            {
                var x = obj.GetComObj() as DlrComLibraryLib.Properties;
                if (x == null)
                    throw new Exception("COMTrustTest failed after cast");
            }
            );
            Utils.LogResult("COMTrustTest", a, result);
        }

        // Testing: Compile/execute Expression Tree containing internal type from another assembly marked as partial trust only
        // Expected Result: Test should fail when run in AppDomain that doesn't have full trust.
        public class M : MarshalByRefObject
        {
            public void PartialTrustMain()
            {
                InternalTypeTest.InternalTest.RunTest();
            }
        }

        // Testing: A tree containing types internal to another assembly is compiled within a [SecuritySafeCritical] method but invoked outside of that method.
        // Expected Result: Throws MethodAccessException when run in partial trust on x86 or x64 (works on full trust), bug had caused it to not throw on x86.
        public static void TreeWithInternalTypeTest(AppDomain a, bool ExpectsError)
        {
            var m = (M)a.CreateInstanceAndUnwrap(typeof(M).Assembly.FullName, typeof(M).FullName);
            // ArgumentException is a little hack to make sure we threw a MethodAccessException at just the right place
            var result = Utils.Throws<ArgumentException>(ExpectsError, () => m.PartialTrustMain());
            Utils.LogResult("TreeWithInternalTypeTest", a, result);
        }
    }
}
