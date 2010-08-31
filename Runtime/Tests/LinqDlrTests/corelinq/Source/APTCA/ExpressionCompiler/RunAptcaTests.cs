extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace APTCATest
{
    public enum PermissionFlag
    {
        None,
        RestrictedMemberAccess,
        NoMemberAccess,
        MemberAccess
    }

    public enum AccessFlag
    {
        None,
        AtoA,
        AtoB,
        AtoC,
        AtoD
    }

    public class APTCATestAttribute : Attribute
    {
    }

    [Serializable]
    public class AssemblyA
    {
        // default values
        public static PermissionFlag permFlag = PermissionFlag.RestrictedMemberAccess;
        public static AccessFlag accessFlag = AccessFlag.AtoA;

        public void RunTest()
        {
            VerifyPartialTrust();

            // Not able to pass from domain creator
            SetRunParas(Environment.GetEnvironmentVariable("CMDLINE"));
            Type tests = Type.GetType("APTCATest.AccessMembersTests");
            if (null == tests)
            {
                Console.WriteLine("NO Test");
                return;
            }

#if false
            try
            {
                new PermissionSet(PermissionState.Unrestricted).Demand();
                throw new Exception("Caller has FullTrust");
            }
            catch (System.Security.SecurityException)
            {
                // System.Windows.Forms.MessageBox.Show("PT!");
                Console.WriteLine("PT:)");
            }
#endif

            bool bFail = false;
            foreach (MethodInfo testCase in tests.GetMethods())
            {
                foreach (var attr in testCase.GetCustomAttributes(true))
                {
                    if (attr is APTCATestAttribute)
                    {
                        try
                        {
                            if ((int)testCase.Invoke(null, null) != 0)
                            {
                                Console.WriteLine("FAIL - '{0}' :(", testCase.Name);
                                bFail = true;
                            }
                            else
                            {
                                Console.WriteLine("PASS - '{0}'", testCase.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            // unexpected exception
                            string exMsg = ex.ToString();
                            Exception e = ex;
                            while (null != e.InnerException)
                            {
                                exMsg += @"\r\n" + e.InnerException.ToString();
                                e = e.InnerException;
                            }
                            Console.WriteLine("TESTCASE '{0}' Failed with EXCEPTION\r\n {1}", testCase.Name, exMsg);
                            Console.WriteLine("....................................");
                            bFail = true;
                        }
                        Console.WriteLine("---------------------------------------------------\r\n");
                    }
                }
            }
            if (bFail)
                throw new Exception("Testing Failed");
        }

        public void VerifyPartialTrust()
        {
            try
            {
                new PermissionSet(PermissionState.Unrestricted).Demand();
                throw new Exception("Caller has FullTrust");
            }
            catch (System.Security.SecurityException)
            {
                // Console.WriteLine("PARTIAL TRUST:)");
            }
        }

        private void SetRunParas(string runPara)
        {
            // Default is A2A_RMA
            if (String.IsNullOrEmpty(runPara))
                return;

            runPara = runPara.Trim().ToUpper();
            string[] paras = runPara.Split(' ');
            if (null == paras)
                return;
            switch (paras[0])
            {
                case "RMA":
                    permFlag = PermissionFlag.RestrictedMemberAccess;
                    break;
                case "NMA":
                    permFlag = PermissionFlag.NoMemberAccess;
                    break;
                case "MA":
                    permFlag = PermissionFlag.MemberAccess;
                    break;
            }
            if (1 < paras.Length)
            {
                switch (paras[1])
                {
                    case "A2A":
                        accessFlag = AccessFlag.AtoA;
                        break;
                    case "A2B":
                        accessFlag = AccessFlag.AtoB;
                        break;
                    case "A2C":
                        accessFlag = AccessFlag.AtoC;
                        break;
                    case "A2D":
                        accessFlag = AccessFlag.AtoD;
                        break;
                }
            }
        }
    }
}
