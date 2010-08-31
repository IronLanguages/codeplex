extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

public class CreatePartialTrustedDomain
{
    static int Main()
    {
        // Create a restricted AppDomain
        AppDomainSetup ads = new AppDomainSetup();
        ads.ApplicationBase = Environment.CurrentDirectory;
        Evidence baseEvidence = AppDomain.CurrentDomain.Evidence;
        Evidence evidence = new Evidence(baseEvidence);
        AppDomain partialTrustedDomain = AppDomain.CreateDomain("PartialTrustedDomain", evidence, ads);

        PolicyLevel policyLevel = ConfigSecurityPermission(Environment.GetEnvironmentVariable("CMDLINE"));
        partialTrustedDomain.SetAppDomainPolicy(policyLevel);

        int nRet = 0;
        try
        {
            APTCATest.AssemblyA testCases = (APTCATest.AssemblyA)partialTrustedDomain.CreateInstanceAndUnwrap(typeof(APTCATest.AssemblyA).Assembly.FullName, typeof(APTCATest.AssemblyA).FullName);
            partialTrustedDomain.DoCallBack(new CrossAppDomainDelegate(testCases.RunTest));
        }
        catch (Exception ex)
        {
            nRet = 1;
            Console.WriteLine(ex.Message);
            Console.WriteLine("========================================================================\r\n");
        }

        AppDomain.Unload(partialTrustedDomain);
        return nRet;
    }

    private static PolicyLevel ConfigSecurityPermission(string runPara)
    {
        // Permission Set
        PermissionSet lowerPermissionSet = new PermissionSet(PermissionState.None);
        lowerPermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
        lowerPermissionSet.AddPermission(new IsolatedStorageFilePermission(PermissionState.Unrestricted));
        lowerPermissionSet.AddPermission(new EnvironmentPermission(EnvironmentPermissionAccess.Read, "CMDLINE"));

        PermissionSet mediumPermissionSet = new PermissionSet(PermissionState.None);
        mediumPermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
        mediumPermissionSet.AddPermission(new IsolatedStorageFilePermission(PermissionState.Unrestricted));
        mediumPermissionSet.AddPermission(new FileDialogPermission(FileDialogPermissionAccess.Open));
        mediumPermissionSet.AddPermission(new EnvironmentPermission(EnvironmentPermissionAccess.Read, "CMDLINE"));

        PermissionSet higherPermissionSet = new PermissionSet(PermissionState.None);
        higherPermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
        higherPermissionSet.AddPermission(new IsolatedStorageFilePermission(PermissionState.Unrestricted));
        higherPermissionSet.AddPermission(new FileDialogPermission(PermissionState.Unrestricted));
        higherPermissionSet.AddPermission(new EnvironmentPermission(EnvironmentPermissionAccess.Read, "CMDLINE;PATH;OS"));

        // Same as Medium one + possible RMA/MA
        PermissionSet partialTrustedPermissionSet = new PermissionSet(PermissionState.None);
        partialTrustedPermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
        partialTrustedPermissionSet.AddPermission(new IsolatedStorageFilePermission(PermissionState.Unrestricted));
        partialTrustedPermissionSet.AddPermission(new FileDialogPermission(FileDialogPermissionAccess.Open));
        partialTrustedPermissionSet.AddPermission(new EnvironmentPermission(EnvironmentPermissionAccess.Read, "CMDLINE"));

        // This is for the midiem permission set only as it is used for assembly who access other assembly's data
        if (String.IsNullOrEmpty(runPara))
        {
            partialTrustedPermissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));
        }
        else
        {
                if (runPara.StartsWith("RMA", StringComparison.OrdinalIgnoreCase))
                {
                    partialTrustedPermissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));
                }
                else if (runPara.StartsWith("MA", StringComparison.OrdinalIgnoreCase))
                {
                    partialTrustedPermissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
                }
                else if (runPara.StartsWith("NMA", StringComparison.OrdinalIgnoreCase))
                {
                    // partialTrustedPermissionSet.RemovePermission(typeof(ReflectionPermission));
                }
                else
                {
                    partialTrustedPermissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));
                }
        }

        // create a policy root that is unrestricted
        PolicyStatement basePolicy = new PolicyStatement(new PermissionSet(PermissionState.Unrestricted));
        UnionCodeGroup policyRoot = new UnionCodeGroup(new AllMembershipCondition(), basePolicy);

        // add an exclusive group for the assembly we are attempting to restrict
        PolicyStatement mainPolicy = new PolicyStatement(partialTrustedPermissionSet, PolicyStatementAttribute.Exclusive);
        string url = System.IO.Path.Combine(Environment.CurrentDirectory, "AssemblyA.dll");
        policyRoot.AddChild(new UnionCodeGroup(new UrlMembershipCondition(url), mainPolicy));

        // grant AssemblyB lower permission
        url = System.IO.Path.Combine(Environment.CurrentDirectory, "AssemblyB.dll");
        PolicyStatement lowerPolicy = new PolicyStatement(lowerPermissionSet, PolicyStatementAttribute.Exclusive);
        policyRoot.AddChild(new UnionCodeGroup(new UrlMembershipCondition(url), lowerPolicy));

        // grant AssemblyC miedium permission
        url = System.IO.Path.Combine(Environment.CurrentDirectory, "AssemblyC.dll");
        PolicyStatement mediumPolicy = new PolicyStatement(mediumPermissionSet, PolicyStatementAttribute.Exclusive);
        policyRoot.AddChild(new UnionCodeGroup(new UrlMembershipCondition(url), mediumPolicy));

        // grant AssemblyD higher permission
        url = System.IO.Path.Combine(Environment.CurrentDirectory, "AssemblyD.dll");
        PolicyStatement higherPolicy = new PolicyStatement(higherPermissionSet, PolicyStatementAttribute.Exclusive);
        policyRoot.AddChild(new UnionCodeGroup(new UrlMembershipCondition(url), higherPolicy));

        // create an AppDomain policy level for the policy tree
        PolicyLevel policyLevel = PolicyLevel.CreateAppDomainLevel();
        policyLevel.RootCodeGroup = policyRoot;

        return policyLevel;
    }
}
