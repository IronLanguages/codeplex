extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class PublicClass
{
    public bool publicBool = true;
    static public bool sPublicBool = false;
    readonly public char roPublicChar;

    internal short internalShort = 200;
    protected int protectedInt = 500;
    protected internal long protectedInternalLong = 900;
    private int privateInt = 12345;
    private string privateString = @"Class: private string";

    static private string sPrivateString = @"Class: static private string";
    static protected float sProtectedFloat = 123.4F;
    static protected internal double sProtectedInternalDouble = 1.23456F;
    static internal decimal sInternalDecimal = 555.5m;

    public List<long> numList = new List<long>();
    internal uint[] internalUintArray = new uint[] { 1, 2, 3 };
    protected List<string> protectedStringList = new List<string> { "en-US", "ar-SA", "1", "ZZZzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz", "" };
    private Delegate[] privateDelegateArray = new Delegate[99];

    public Stack publicStack;
    private Queue privateQueue; 

    public PublicClass()
    {
        roPublicChar = 'R';
        publicStack = InitStack(99, typeof(string));
        privateQueue = InitQueue(111, typeof(Random));
    }

    Stack InitStack(int count, Type t)
    {
        if (0 >= count)
            return null;

        // default ctor
        ConstructorInfo ctor = t.GetConstructor(new Type[] { });
        if (null == ctor)
            return null;

        Stack st = new Stack(count);
        for (int i = 0; i < count; i++)
        {
            object obj = ctor.Invoke(new object[] { });
            st.Push(obj);
        }
        return st;
    }

    Queue InitQueue(int count, Type t)
    {
        if (0 >= count)
            return null;

        // default ctor
        ConstructorInfo ctor = t.GetConstructor(new Type[] { });
        if (null == ctor)
            return null;

        Queue q = new Queue(count);
        for (int i = 0; i < count; i++)
        {
            object obj = ctor.Invoke(new object[] { });
            q.Enqueue(obj);
        }
        return q;
    }

    protected PublicClass(string str)
    {
        if (String.IsNullOrEmpty(str))
            privateString = "Empty ctor string";
        else
            privateString = str;
    }

    private PublicClass(int nValue)
    {
        privateInt = nValue;
    }

    #region "Properties"

    public short PublicProperty
    {
        get {   return internalShort;   }
        set {   internalShort = value;  }
    }

    internal int InternalProperty
    {
        get {   return protectedInt;   }
    }

    static int StaticInternalAIGetSet
    {
        get;
        set;
    }

    protected long ProtectedProperty
    {
        get {   return protectedInternalLong;  }
        set {   protectedInternalLong = value;  }
    }

    protected internal double ProtectedInternalProperty
    {
        get { return sProtectedInternalDouble; }
        set { sProtectedInternalDouble = value; }
    }

    private string PrivateProperty
    {
        get {   return privateString;  }
        set {   privateString = value; }
    }

    static private float SPrivateProperty
    {
        get { return sProtectedFloat; }
        set { sProtectedFloat = value; }
    }

    #endregion

    #region "Basic Methods"
    public void PublicMethod()
    {
        var date = DateTime.Now.ToLongDateString();
        var time = new[] { DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second };
        Console.WriteLine("Class : Public Method Called at date:" + date + "on time - " + time[1].ToString());
    }
    
    static public void SPublicMethod(char ch)
    {
        Console.WriteLine("Class : STATIC Public Method Called with char=" + ch);
    }

    string InternalMethod()
    {
        bool b1 = (publicBool) ? true : false;
        bool b2 = (!sPublicBool) ? true : false;
        return sPrivateString;
    }

    static ulong SInternalMethod(ulong ulValue)
    {
        return (ulValue * ulValue) % (ulong.MaxValue - ulong.MinValue);
    }

    protected internal ArrayList ProtectedInternalMethod(int count)
    {
        if (0 >= count)
            return null;
        else
            return new ArrayList(count);
    }

    protected decimal ProtectedMethod()
    {
        return sInternalDecimal;
    }

    private void PrivateMethod()
    {
        sProtectedInternalDouble -= 0.0001;
        Console.WriteLine("Class : Private Method Called");
    }
    #endregion

    #region "More Methods"

    public bool PublicMethodRefOutParas(string name, uint count, ref int refValue, out int outValue)
    {
        outValue = 0;
        if (String.IsNullOrEmpty(name))
        {
            outValue = refValue - 99;
            return false;
        }
        else
        {
            outValue = (int)(count * refValue) % int.MaxValue;
            return true;
        }
    }

    public bool PublicMethodNORefOut(string name, uint count, int refValue, int outValue)
    {
        if (String.IsNullOrEmpty(name))
        {
            if (0 == outValue || -1 == refValue)
                return false;
        }
        else
        {
            int nValue = (int)(count * refValue * outValue) % int.MaxValue;
            return true;
        }
        return true;
    }

    virtual public void VirtualMethodDoNothing()
    {
    }

    virtual public int VirtualMethod001(int range)
    {
        return range * range % (int.MaxValue - 1);
    }

    virtual protected internal long VirtualMethod002(string str)
    {
        Console.WriteLine("PublicClass protected internal Virtual Method002 - " + str);
        return (long) DateTime.Now.Second;
    }

    #endregion
}

public class DerivedClass : PublicClass
{
    public int intValue;

    public int GetSomeValueThroughReflection()
    {
        FieldInfo fi = typeof(PublicClass).GetField("privateInt", BindingFlags.NonPublic | BindingFlags.Instance);

        PublicClass pc = new PublicClass();
        intValue = (int)fi.GetValue(pc);
        return intValue;
    }

    override public void VirtualMethodDoNothing()
    {
        Console.WriteLine("Override in derived class");
    }

    new public long VirtualMethod002(string str)
    {
        Console.WriteLine("Hide (new) in derived class -> " + str);
        return (long)DateTime.Now.Minute;
    }
}

public struct PublicStruct
{
    public char publicChar;
    static public char sPublicChar = 'S';

    internal sbyte internalSbyte;
    static internal uint sInternalUint;

    private byte privateByte;
    static private ushort sPrivateUshort = 11;

    public List<short> numList;

    public PublicStruct(int number)
    {
        publicChar = 'A';
        internalSbyte = -127;
        privateByte = 255;
        sPrivateUshort = 12;
        sInternalUint = 66666;

        PrivateAIGetSetField = 'I';

        numList = new List<short>();
    }

    #region "Properties"
    public uint PublicProperty
    {
        get { return sInternalUint; }
        set { sInternalUint = value; }
    }

    static public ushort SPublicProperty
    {
        get { return sPrivateUshort; }
        set { sPrivateUshort = value; }
    }

    internal byte InternalProperty
    {
        get { return privateByte; }
        set { privateByte = value; }
    }

    private sbyte PrivateProperty
    {
        get { return internalSbyte; }
    }

    static private char PrivateAIGetSetField
    {
        get;
        set;
    }
    #endregion

    #region "Methods"
    public ushort PublicMethod()
    {
        return sPrivateUshort;
    }

    internal void InternalMethod(int number)
    {
        Console.WriteLine("The number pass in = " + number.ToString());
    }

    private void PrivateMethod()
    {
        int n = publicChar + sPublicChar + internalSbyte;
        Console.WriteLine("Struct: Private Method Called");
    }

    static private sbyte SPrivateMethod(byte btValue)
    {
        return (sbyte)(btValue - 128);
    }

    #region "More Methods"

    public void PublicMethodWithOut(int number, out string outString)
    {
        outString = "The number pass in = " + number.ToString();
    }

    public void PublicMethodWithParams(params char[] charAry)
    {
        if (null == charAry || 0 == charAry.Length)
            Console.Write("Empty list");
        else
        {
            foreach (char ch in charAry)
            {
                Console.Write(ch + "-");
            }
        }
    }

    internal List<char> GetAllFirstChar(List<string> strList)
    {
        if (null == strList || 0 == strList.Count)
            return null;

        List<char> charList = new List<char>(strList.Count);
        foreach (string str in strList)
        {
            if (!String.IsNullOrEmpty(str))
                charList.Add(str[0]);
        }

        return charList;
    }

    #endregion

    #endregion
}

#region "Class with Different Ctors"
public class DefaultCtorClass
{
    public void Method()
    {
        Console.WriteLine(this.GetType());
    }
}

public class PublicCtorClass
{
    readonly int n;
    public PublicCtorClass()
    {
        n = 99;
    }

    public void Method()
    {
        Console.WriteLine(n);
    }
}

public class PublicParaCtorClass
{
    readonly int n;
    public PublicParaCtorClass(int num)
    {
        n = num;
    }

    public void Method()
    {
        Console.WriteLine(n);
    }
}

public class Public2CtorsClass
{
    readonly int n;
    public Public2CtorsClass()
    {
        n = 100;
    }

    public Public2CtorsClass(int num)
    {
        n = num;
    }

    virtual public void Method1()
    {
        Console.WriteLine(n);
    }
}

public class PublicCtorsSubClass : Public2CtorsClass
{
    readonly int n;
    public PublicCtorsSubClass()
    {
        n = 101;
    }

    public PublicCtorsSubClass(int num)
        : base(num)
    {
    }

    override public void Method1()
    {
        Console.WriteLine(n);
    }
}

public class ProtectedCtorClass
{
    readonly int n;
    protected ProtectedCtorClass()
    {
        n = 200;
    }

    public void Method()
    {
        Console.WriteLine(n);
    }
}

public class InternalCtorClass
{
    readonly int n;
    internal InternalCtorClass()
    {
        n = 300;
    }

    public void Method()
    {
        Console.WriteLine(n);
    }
}

public class PrivateCtorClass 
{
    readonly int n;

    private PrivateCtorClass()
    {
        n = 400;
    }

    public void Method()
    {
        Console.WriteLine(n);
    }
}

public class Private2CtorsClass
{
    readonly int n;

    private Private2CtorsClass()
    {
        n = 404;
    }

    private Private2CtorsClass(int num)
    {
        n = num;
    }

    public void Method()
    {
        Console.WriteLine(n);
    }
}

public class StaticCtorClass
{
    static readonly int n;

    static StaticCtorClass()
    {
        n = 500;
    }

    public void Method()
    {
        Console.WriteLine(n);
    }
}

public class Static2CtorsClass
{
    readonly int m;
    static readonly int n;

    static Static2CtorsClass()
    {
        n = 505;
    }

    Static2CtorsClass(int num)
    {
        m = num;
    }

    public void Method()
    {
        Console.WriteLine(n);
        Console.WriteLine(m);
    }
}
#endregion

#region "Struct with Different Ctors"
public struct DefaultCtorStruct
{
    decimal d;

    public void Method()
    {
        d = 12.345m;
        Console.WriteLine(this.GetType() + d.ToString());
    }
}

public struct PublicParaCtorStruct
{
    readonly decimal d;
    public PublicParaCtorStruct(decimal num)
    {
        d = num;
    }

    public void Method()
    {
        Console.WriteLine(d);
    }
}

public struct InternalCtorStruct
{
    readonly decimal d;
    internal InternalCtorStruct(decimal num)
    {
        d = num;
    }

    public void Method()
    {
        Console.WriteLine(d);
    }
}

public struct PrivateCtorStruct
{
    readonly decimal d;
    private PrivateCtorStruct(decimal num)
    {
        d = num;
    }

    public void Method()
    {
        Console.WriteLine(d);
    }
}

#endregion
