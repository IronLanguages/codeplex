extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Permissions;

public class PublicClassA : MarshalByRefObject
{
    public bool publicBool = true;
    static public bool sPublicBool = false;

    internal short internalShort = 200;
    protected int protectedInt = 500;
    protected internal long protectedInternalLong = 900;
    private string privateString = @"Class A: private string";

    static private string sPrivateString = @"Class A: static private string";
    static protected float sProtectedFloat = 123.4F;
    static protected internal double sProtectedInternalDouble = 1.23456F;
    static internal decimal sInternalDecimal = 555.5m;

    public short PublicGetSetProperty
    {
        get {   return internalShort;   }
        set {   internalShort = value;  }
    }

    internal int InternalGetProperty
    {
        get {   return protectedInt;   }
    }

    static int StaticInternalAIGetSet
    {
        get;
        set;
    }

    protected long ProtectedGetField
    {
        get
        {
            return protectedInternalLong;
        }
    }

    private string PrivateGetSetField
    {
        get
        {
            return privateString;
        }
        set
        {
            privateString = value;
        }
    }

    public void PublicMethod()
    {
        Console.WriteLine("Class A: Public Method Called");
    }

    public bool PublicMethodWithParas(string name, ulong count, ref int nValue)
    {
        nValue = 0;
        if (String.IsNullOrEmpty(name))
        {
            nValue = -1;
            return false;
        }
        else
        {
            nValue = 888;// (int)(count / (int.MaxValue - 1));
            return true;
        }
    }

    string InternalMethod()
    {
        return PublicClassA.sPrivateString;
    }

    protected decimal ProtectedMethod()
    {
        return sInternalDecimal;
    }

    private void PrivateMethod()
    {
        Console.WriteLine("Class A: Private Method Called");
    }
}

struct InternalStructA
{
    public char publicChar;
    static public char sPublicChar = 'S';

    internal sbyte internalSbyte;
    private byte privateByte;

    static private ushort sPrivateUshort = 11;
    static internal uint sInternalUint;

    public InternalStructA(int number)
    {
        publicChar = 'A';
        internalSbyte = -127;
        privateByte = 255;
        sPrivateUshort = 12;
        sInternalUint = 66666;

        PrivateAIGetSetField = 'I';
    }

    public uint PublicGetSetField
    {
        get { return sInternalUint; }
        set { sInternalUint = value; }
    }

    internal byte InternalGetSetField
    {
        get { return privateByte; }
        set { privateByte = value; }
    }

    static private char PrivateAIGetSetField
    {
        get;
        set;
    }

    public ushort PublicMethod()
    {
        return sPrivateUshort;
    }

    internal void InternalMethodWithOut(int number, out string outString)
    {
        outString = "The number pass in = " + number.ToString();
    }

    private void PrivateMethod()
    {
        int n = publicChar + sPublicChar + internalSbyte;
        Console.WriteLine("Struct A: Private Method Called");
    }
}
