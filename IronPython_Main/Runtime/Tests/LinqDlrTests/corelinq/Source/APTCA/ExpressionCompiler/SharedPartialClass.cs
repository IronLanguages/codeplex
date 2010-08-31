extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace APTCATest
{
    public enum ExceptionSource
    {
        None,
        Compile,
        ExecFunc,
        Action,
        LambdaInvoke,
        SeqQuery
    }

    public delegate void DelegateWithOut(int input, out string str);
    public delegate bool DelegateWithRefOut(string str, uint count, ref int refValue, out int outValue);
    public delegate bool Delegate5Paras(string str, uint count, int refValue, int outValue);
    public delegate void DelegateWithParams(params char[] charAry);

    public partial class AccessMembersTests
    {
        static PublicClass classObj = new PublicClass();
        static DerivedClass derivedObj = new DerivedClass();
        // init some instance values
        static PublicStruct structObj = new PublicStruct(9);

        static ConstantExpression classCE = Expression.Constant(new PublicClass(), typeof(PublicClass));
        static ConstantExpression structCE = Expression.Constant(new PublicStruct(), typeof(PublicStruct));

        static ParameterExpression classPara = Expression.Parameter(typeof(PublicClass), "para");
        static ParameterExpression structPara = Expression.Parameter(typeof(PublicStruct), "spara");

        static int ExceptionHandler(Exception ex, ExceptionSource source)
        {
            string errMsg = "Exception from '" + source + "' (" + AssemblyA.permFlag + ", " + AssemblyA.accessFlag + ")\r\n" + ex.ToString();
            string innerMsg = String.Empty;
            Exception e = ex;
            while (null != e.InnerException)
            {
                innerMsg = "\r\nINNER EX: " + ex.InnerException.ToString();
                e = e.InnerException;
            }

            if (ex is FieldAccessException || ex is MethodAccessException || ex is TypeLoadException)
            {
                Console.WriteLine("xxx Access Exception - " + errMsg);
                // Expected Exception when no RMA
                if (PermissionFlag.NoMemberAccess == AssemblyA.permFlag)
                {
                    // used for switch return value (0 or 1)
                    if (ExceptionSource.Compile == source)
                        return 0;

                    return 0;
                }
                // Access assembly with higher permission set
                if (AccessFlag.AtoD == AssemblyA.accessFlag)
                    return 0;
                else
                {
                    Console.WriteLine(innerMsg);
                    return 1;
                }
            }
            // Unexpected exception
            Console.WriteLine("xxx Unexpected Exception - " + errMsg);
            Console.WriteLine(innerMsg + "\r\n");
            return 1;
        }
    }
}
