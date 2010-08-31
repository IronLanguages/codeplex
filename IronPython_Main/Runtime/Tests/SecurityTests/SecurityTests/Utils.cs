using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting;

namespace SecurityTests {
    public static class Utils {
        // this will cache any created AppDomains
        internal static List<AppDomain> AppDomainCache = new List<AppDomain>();

        internal static void CheckAppDomainStats(AppDomain ad) {
            Console.WriteLine("Current test AppDomain is: {0}", ad.FriendlyName);
            Console.WriteLine("IsFullyTrusted: {0}\nIsHomogeneous: {1}", ad.IsFullyTrusted, ad.IsHomogenous);
        }

        // Encapsulates error checking logic/result verification
        internal static bool Throws<T>(bool ExpectsError, Action call) where T : Exception {
            bool result = false;

            try {
                call();
                if (!ExpectsError) {
                    result = true;
                }
            } catch (T) {
                if (ExpectsError) {
                    result = true;
                }
            } catch (Exception e) {
                // for tests calling DoSecurity() they may throw a MethodAccessException with a SecurityException
                // as the InnerException rather than just a SecurityException
                if (e.InnerException != null && (e.InnerException.GetType() == typeof(T)) && ExpectsError) {
                    result = true;
                } else {
                    throw;
                }
            }

            return result;
        }

        internal static void LogResult(string testName, AppDomain a, bool testPassed) {
            LogResult(testName, testPassed, a, null);
        }

        internal static void LogResult(string testName, bool testPassed, AppDomain a, AppDomain b = null) {
            string testResult = String.Format("[{0,-50} {1}]", testName, (testPassed ? "passed" : "failed"));

            if (testPassed)
                SecurityTests.Log.Add(testResult);
            else {
                string result = String.Format("{0}\n\twhen A: IsHomogneous:{1}, IsFullTrust:{2}", testResult, a.IsHomogenous, a.IsFullyTrusted);
                if (b != null) {
                    result += String.Format("\n\t     B: IsHomogneous:{0}, IsFullTrust:{1}", b.IsHomogenous, b.IsFullyTrusted);
                }
                SecurityTests.Log.Add(result);
            }
        }

        // creates an AppDomain using 3.5 CAS policy with the given trust level
        // CAS policy AppDomains are hetergeneous by default
        internal static AppDomain CreateDomainWithCAS(bool FullTrust, IPermission pms = null) {
            // Default to all code getting nothing
            PolicyStatement emptyPolicy = new PolicyStatement(new PermissionSet(PermissionState.None));
            UnionCodeGroup policyRoot = new UnionCodeGroup(new AllMembershipCondition(), emptyPolicy);

            // Grant all code the named permission set passed if non-null
            var pset = new PermissionSet(FullTrust ? PermissionState.Unrestricted : PermissionState.None);
            if (pms != null)
                pset.AddPermission(pms);
            if (!FullTrust)
                pset.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            PolicyStatement permissions = new PolicyStatement(pset);
            policyRoot.AddChild(new UnionCodeGroup(new AllMembershipCondition(), permissions));

            // create an AppDomain policy level for the policy tree
            PolicyLevel appDomainLevel = PolicyLevel.CreateAppDomainLevel();
            appDomainLevel.RootCodeGroup = policyRoot;

            // create an AppDomain where this policy will be in effect
            AppDomain ad = AppDomain.CreateDomain("3.5Domain");
            ad.SetAppDomainPolicy(appDomainLevel);

            return ad;
        }

        // this creates a 4.0 security policy AppDomain (i.e., no CAS)
        // 4.0 AppDomains are homogenous by default
        // An explicit permissions set is optional, by default the AppDomain will simply be full or partial trust with execution permission
        internal static AppDomain CreateDomainNewPolicy(bool FullTrust, IPermission pms = null) {
            var pset = new PermissionSet(FullTrust ? PermissionState.Unrestricted : PermissionState.None);
            if (pms != null)
                pset.AddPermission(pms);
            if (!FullTrust)
                pset.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            var setup = new AppDomainSetup();
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

            var ad = AppDomain.CreateDomain("4.0Domain", null, setup, pset, null);

            return ad;
        }

        // Gets an AppDomain with the given policy, trust level and optionally explicit permissions to add to partial trust AppDomains
        // A new AppDomain will be created unless a previous test has created an AppDomain with the same policy and trust level
        // AppDomains with explicit permissions added are not cached.
        internal static AppDomain GetAD(bool LegacyPolicy, bool FullTrust, IPermission pms = null) {
            // check for cached version with the given attributes if no custom permission set is given
            if (pms == null) {
                foreach (var ad in AppDomainCache) {
                    if ((ad.IsFullyTrusted == FullTrust) && (ad.IsHomogenous != LegacyPolicy)) {
                        return ad;
                    }
                }
            }

            // need to create a new AppDomain
            AppDomain result = (LegacyPolicy) ? CreateDomainWithCAS(FullTrust, pms) : CreateDomainNewPolicy(FullTrust, pms);
            if (pms == null) { AppDomainCache.Add(result); }
            return result;
        }
    }
}
