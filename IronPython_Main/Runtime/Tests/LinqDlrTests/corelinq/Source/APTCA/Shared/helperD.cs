extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Permissions;

// [assembly: FileDialogPermission(SecurityAction.RequestMinimum, Unrestricted=true)]
/// <summary>
/// Public class with public, [static | protected] internal, protected, private field and members 
/// </summary>
public class PublicClassD
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

    public short PublicProperty
    {
        get {   return internalShort;   }
        set {   internalShort = value;   }
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
        get {   return privateString;   }
        set {   privateString = value;   }
    }

    public void PublicMethod()
    {
        Console.WriteLine("P-Class D: Public Method Called");
    }

    string InternalMethod()
    {
        return sPrivateString;
    }

    protected decimal ProtectedMethod()
    {
        return sInternalDecimal;
    }

    private void PrivateMethod()
    {
        sInternalDecimal += 11.123m;
        Console.WriteLine("P-Class D: Private Method Called");
    }
}

public struct PublicStructD
{
    public char publicChar;
    static public char sPublicChar = 'S';

    internal sbyte internalSbyte;
    private byte privateByte;

    static private ushort sPrivateUshort;
    static internal uint sInternalUint;

    public PublicStructD(int number)
    {
        publicChar = 'D';
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
        outString = "The number pass in = " + number.ToString();
    }

    private void PrivateMethod()
    {
        int n = publicChar + sPublicChar + internalSbyte;
        Console.WriteLine("P-Struct D: Private Method Called");
    }
}
