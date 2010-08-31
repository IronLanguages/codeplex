extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

public class MethodBodyReader
{
    public static OpCode[] singleByteOpCodes;

    public static void LoadOpCodes()
    {
        singleByteOpCodes = new OpCode[0x100];
        FieldInfo[] infoArray1 = typeof(OpCodes).GetFields();
        for (int num1 = 0; num1 < infoArray1.Length; num1++)
        {
            FieldInfo info1 = infoArray1[num1];
            if (info1.FieldType == typeof(OpCode))
            {
                OpCode code1 = (OpCode)info1.GetValue(null);
                ushort num2 = (ushort)code1.Value;
                if (num2 < 0x100)
                {
                    singleByteOpCodes[(int)num2] = code1;
                }
            }
        }
    }

    private static int ReadInt32(byte[] il, ref int position)
    {
        return (((il[position++] | (il[position++] << 8)) | (il[position++] << 0x10)) | (il[position++] << 0x18));
    }

    public static string GetCode(object operand)
    {
        string result = "";
        if (operand is MethodBase)
        {
            MethodBase method = operand as MethodBase;
            if (method.ReflectedType.ToString().IndexOf("System.Security") >= 0)
            {
                result = method.IsStatic ? "static " : "instance ";
                string returnType = "void";
                if (method is MethodInfo)
                {
                    returnType = (method as MethodInfo).ReturnType.ToString();
                }
                result += returnType +
                    " " + method.ReflectedType.ToString() +
                    "::" + method.Name + "()\n";
            }
        }
        return result;
    }

    public static string GetSecurityCalls(MethodInfo method)
    {
        Module module = method.Module;
        string result = "";
        if (method.GetMethodBody() != null && method.GetMethodBody().GetILAsByteArray() != null)
        {
            byte[] il = method.GetMethodBody().GetILAsByteArray();
            int position = 0;
            while (position < il.Length)
            {
                // get the operation code of the current instruction
                OpCode code = OpCodes.Nop;
                ushort value = il[position++];
                if (value != 0xfe)
                {
                    code = singleByteOpCodes[(int)value];
                }
                int metadataToken = 0;
                // get the operand of the current operation
                if (code.OperandType == OperandType.InlineMethod)
                {
                    metadataToken = ReadInt32(il, ref position);
                    try
                    {
                        result += GetCode(module.ResolveMethod(metadataToken));
                    }
                    catch { }
                    {
                    }
                }
            }
        }
        return result;
    }
}
