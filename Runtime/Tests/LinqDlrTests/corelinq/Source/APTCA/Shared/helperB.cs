extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Permissions;

// make AssemblyB with less permission then A
// [assembly: FileDialogPermission(SecurityAction.RequestRefuse, Open=false)]
/// <summary>
/// Public class with public, [static | protected] internal, protected, private field and members 
/// </summary>
public class PublicClassB
{
    public bool publicBool = true;
    static public bool sPublicBool = false;

    internal short internalShort = 200;
    protected int protectedInt = 500;
    protected internal long protectedInternalLong = 900;
    private string privateString = @"I-Class B: private string";

    static private string sPrivateString = @"I-Class B: static private string";
    static protected float sProtectedFloat = 123.4F;
    static protected internal double sProtectedInternalDouble = 1.23456F;
    static internal decimal sInternalDecimal = 555.5m;

    public short PublicProperty
    {
        get
        {
            return internalShort;
        }
    }

    int InternalProperty
    {
        get
        {
            return protectedInt;
        }
    }

    protected long ProtectedProperty
    {
        get
        {
            return protectedInternalLong;
        }
    }

    private string PrivateProperty
    {
        get
        {
            return privateString;
        }
    }

    public void PublicMethod()
    {
        float f = sProtectedFloat * 2;
        Console.WriteLine("I-Class B: Public Method Called");
    }

    string InternalMethod()
    {
        bool b1 = (publicBool) ? true : false;
        bool b2 = (!sPublicBool) ? true : false;
        return sPrivateString;
    }

    protected string ProtectedMethod()
    {
        return Environment.CommandLine;
    }

    private void PrivateMethod()
    {
        sInternalDecimal += 11.123m;
        sProtectedInternalDouble -= 0.0001;
        Console.WriteLine("I-Class B: Private Method Called");
    }
}

public struct PublicStructB
{
    public char publicChar;
    static public char sPublicChar = 'S';

    internal sbyte internalSbyte;
    private byte privateByte;

    static private ushort sPrivateUshort;
    static internal uint sInternalUint;

    public PublicStructB(int number)
    {
        publicChar = 'A';
        internalSbyte = -127;
        privateByte = 255;
        sPrivateUshort = 12;
        sInternalUint = 66666;
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

    private sbyte PrivateProperty
    {
        get { return internalSbyte; }
    }

    public ushort PublicMethod()
    {
        return sPrivateUshort;
    }

    internal void InternalMethodWithOut(int number, out string outString)
    {
        outString = "B: The number pass in = " + number.ToString() + publicChar + sPublicChar;
    }

    static private void PrivateMethod()
    {
        Console.WriteLine("P-Struct B: Private Method Called");
    }
}
