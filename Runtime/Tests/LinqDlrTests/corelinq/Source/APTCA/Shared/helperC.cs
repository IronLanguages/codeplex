extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

/// <summary>
/// Public class with public, [static | protected] internal, protected, private field and members 
/// </summary>
public class PublicClassC
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
        get {   return privateString;   }
        set {   privateString = value;   }
    }

    static int StaticInternalAIGetSet
    {
        get;
        set;
    }

    public void PublicMethod()
    {
        sProtectedInternalDouble -= 0.0001;
        Console.WriteLine("I-Class C: Public Method Called");
    }

    string InternalMethod()
    {
        int n1 = (publicBool) ? -1 : +1;
        int n2 = (!sPublicBool) ? +100 : -99;
        return sPrivateString;
    }

    private void PrivateMethod()
    {
        sInternalDecimal += 11.123m;
        float f = sProtectedFloat * 3;
        Console.WriteLine("I-Class C: Private Method Called");
    }
}

public struct PublicStructC
{
    public char publicChar;
    static public char sPublicChar = 'S';

    internal sbyte internalSbyte;
    private byte privateByte;

    static private ushort sPrivateUshort;
    static internal uint sInternalUint;

    public PublicStructC(int number)
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

    private char PrivateProperty
    {
        get { return sPublicChar; }
        set { sPublicChar = value; }
    }

    public ushort PublicMethod()
    {
        return sPrivateUshort;
    }

    internal void InternalMethodWithOut(int number, out string outString)
    {
        outString = "C: The number pass in = " + number.ToString();
    }

    private void PrivateMethod()
    {
        int n = publicChar + sPublicChar + internalSbyte;
        Console.WriteLine("Struct C: Private Method Called");
    }
}
