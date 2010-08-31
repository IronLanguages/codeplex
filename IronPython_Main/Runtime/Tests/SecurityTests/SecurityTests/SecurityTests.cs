using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting;

using SecurityTests;

namespace SecurityTests {
    public partial class SecurityTests {
        public static List<string> Log = new List<string>();

        public static void Main() {
            try {
                var s = new System.Diagnostics.Stopwatch();
                s.Start();

                // trust level irrelevant for this test
                AppDomainDynamicTest(Utils.GetAD(false, true), false); // 4.0 policy, FT
                AppDomainDynamicTest(Utils.GetAD(true, true), true);  // 3.5 policy, FT

                // homogenous irrelevant for this test, no dynamic calls
                AppDomainTrustTest(Utils.GetAD(true, true), false); // 3.5 policy, FT

                AppDomainTrustTest(Utils.GetAD(false, false), true); // 4.0 policy, PT
                AppDomainTrustTest(Utils.GetAD(true, false), true); // 3.5 policy, PT
                AppDomainTrustTest(Utils.GetAD(false, false, new ReflectionPermission(ReflectionPermissionFlag.MemberAccess)), false); // PT with necessary permissions

                // trust level irrelevant for this test
                CrossAppDomainDynamicTest(Utils.GetAD(false, true), Utils.GetAD(false, true), false); // Both homogenous
                CrossAppDomainDynamicTest(Utils.GetAD(true, true), Utils.GetAD(false, true), true); // Caller heterogeneous, both FT
                CrossAppDomainDynamicTest(Utils.GetAD(false, true), Utils.GetAD(true, false), false); // Caller homogenous, callee heterogeneous

                // homogenous irrelevant for this test, no dynamic calls
                CrossAppDomainTrustTest(Utils.GetAD(false, true), Utils.GetAD(false, true), false); // Both FT
                CrossAppDomainTrustTest(Utils.GetAD(true, true), Utils.GetAD(true, false), true); // Caller FT but Callee PT
                CrossAppDomainTrustTest(Utils.GetAD(false, false), Utils.GetAD(true, true), true); // Caller PT but Callee FT

                PartialTrustLINQTest(Utils.GetAD(false, true), false); // FT, homogenous
                PartialTrustLINQTest(Utils.GetAD(false, false), true); // PT
                PartialTrustLINQTest(Utils.GetAD(false, false, new System.Data.SqlClient.SqlClientPermission(PermissionState.Unrestricted)), false); // PT with necessary permissions

                BadNodeLINQTest(Utils.GetAD(false, true), true); // Both FT, homogenous

                IDMOPTest(Utils.GetAD(false, true), false); // homogenous
                IDMOPTest(Utils.GetAD(true, true), true); // heterogeneous

                CrossAppDomainIDMOPTest(Utils.GetAD(false, true), Utils.GetAD(false, true), false); // Both homogenous
                CrossAppDomainIDMOPTest(Utils.GetAD(true, true), Utils.GetAD(false, true), true); // Caller heterogeneous, both FT
                CrossAppDomainIDMOPTest(Utils.GetAD(false, true), Utils.GetAD(true, false, new ReflectionPermission(PermissionState.Unrestricted)), false); // Caller homogenous, callee heterogeneous

                // hetergeneous causes failures here but that's redundant with previous dynamic tests
                ExecuteAssemblyTest(Utils.GetAD(false, true), false); // FT
                ExecuteAssemblyTest(Utils.GetAD(false, false), true); // PT

                COMTrustTest(Utils.GetAD(false, true), false); // FT
                COMTrustTest(Utils.GetAD(false, false), true); // PT
                COMTrustTest(Utils.GetAD(false, false, new SecurityPermission(SecurityPermissionFlag.UnmanagedCode)), false); // PT with necessary permissions

                // Partial Trust AppDomain with evidence from signed assembly
                PermissionSet permissions = new PermissionSet(PermissionState.None);
                permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                AppDomain a = AppDomain.CreateDomain("Medium Trust Sandbox", null, new AppDomainSetup { ApplicationBase = Environment.CurrentDirectory }, permissions, typeof(InternalTypeTest.InternalTest).Assembly.Evidence.GetHostEvidence<StrongName>());

                TreeWithInternalTypeTest(a, true); // PT
                TreeWithInternalTypeTest(Utils.GetAD(false, true), false); // FT

                int TestCount = 0;
                foreach (var r in Log) {
                    Console.WriteLine(r);
                    TestCount++;
                }

                foreach (AppDomain d in Utils.AppDomainCache) {
                    AppDomain.Unload(d);
                }

                s.Stop();
                Console.WriteLine("{0} tests ran in {1} seconds.", TestCount, s.Elapsed.TotalSeconds);
            } catch (DivideByZeroException e) {
                Console.WriteLine(e.GetType());
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
