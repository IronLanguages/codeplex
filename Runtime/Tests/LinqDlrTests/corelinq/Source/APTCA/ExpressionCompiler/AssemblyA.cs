extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
// using System.IO;
using System.Linq;
using System.Linq.Expressions;
// using System.Text;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

// [assembly: FileIOPermissionAttribute(SecurityAction.RequestRefuse, Unrestricted = true)]
// [assembly: SecurityPermissionAttribute(SecurityAction.RequestRefuse, UnmanagedCode = true)]
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
    public class AssemblyA : MarshalByRefObject
    {
        // default values
        public static PermissionFlag permFlag = PermissionFlag.RestrictedMemberAccess;
        public static AccessFlag accessFlag = AccessFlag.AtoA;

        public void RunTest()
        {
            // Not able to pass from domain creator
            SetRunParas(Environment.GetEnvironmentVariable("CMDLINE"));
            Type tests = null;
            bool bFail = false;
            switch (accessFlag)
            {
                case AccessFlag.AtoA:
                    tests = Type.GetType("APTCATest.AssemblyA2A");
                    break;
                case AccessFlag.AtoB:
                    tests = Type.GetType("APTCATest.AssemblyA2B");
                    break;
                case AccessFlag.AtoC:
                    tests = Type.GetType("APTCATest.AssemblyA2C");
                    break;
                case AccessFlag.AtoD:
                    tests = Type.GetType("APTCATest.AssemblyA2D");
                    break;
            }
            if (null == tests)
                return;

            foreach (MethodInfo testCase in tests.GetMethods())
            {
                foreach (var attr in testCase.GetCustomAttributes(true))
                {
                    if (attr is APTCATestAttribute)
                    {
                        Console.WriteLine("\r\n========================================================================");
                        try
                        {
                            if ((int)testCase.Invoke(null, null) != 0)
                            {
                                Console.WriteLine("xxx FAILED - '{0}' xxx", testCase.Name);
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
                            Console.WriteLine("TESTCASE '{0}' Failed with EXCEPTION\r\n {1}", testCase.Name, ex.ToString());
                            bFail = true;
                        }
                    }
                }
            }
            if (bFail)
                throw new Exception("Testing Failed");
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
