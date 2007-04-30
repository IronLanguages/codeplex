/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using IronPython.Runtime.Exceptions;
using System.Threading;

public static class ThreadStatics {
#if SILVERLIGHT
    private static LocalDataStoreSlot _CompareUtil_CmpStack = Thread.AllocateDataSlot();
    private static LocalDataStoreSlot _Ops_InfiniteRepr = Thread.AllocateDataSlot();
    private static LocalDataStoreSlot _StringFormatter_NumberFormatInfoForThread = Thread.AllocateDataSlot();
    private static LocalDataStoreSlot _SystemState_RawException = Thread.AllocateDataSlot();
    private static LocalDataStoreSlot _SystemState_RawTraceBack = Thread.AllocateDataSlot();
    private static LocalDataStoreSlot _PythonFunction_Depth = Thread.AllocateDataSlot();

    public static Stack<object> CompareUtil_CmpStack {
        get {
            return (Stack<object>)Thread.GetData(_CompareUtil_CmpStack);
        }
        set {
            Thread.SetData(_CompareUtil_CmpStack, value);
        }
    }
    
    public static List<object> Ops_InfiniteRepr {
        get {
            return (List<object>)Thread.GetData(_Ops_InfiniteRepr);
        }
        set {
            Thread.SetData(_Ops_InfiniteRepr, value);
        }
    }
    
    public static NumberFormatInfo StringFormatter_NumberFormatInfoForThread {
        get {
            return (NumberFormatInfo)Thread.GetData(_StringFormatter_NumberFormatInfoForThread);
        }
        set {
            Thread.SetData(_StringFormatter_NumberFormatInfoForThread, value);
        }
    }

    public static TraceBack SystemState_RawTraceBack {
        get {
            return (TraceBack)Thread.GetData(_SystemState_RawTraceBack);
        }
        set {
            Thread.SetData(_SystemState_RawTraceBack, value);
        }
    }

     public static int PythonFunction_Depth {
        get {
            return (int)Thread.GetData(_PythonFunction_Depth);
        }
        set {
            Thread.SetData(_PythonFunction_Depth, value);
        }
    }

#else

    [ThreadStatic]
    public static Stack<object> CompareUtil_CmpStack;

    [ThreadStatic]
    public static List<object> Ops_InfiniteRepr;

    [ThreadStatic]
    public static NumberFormatInfo StringFormatter_NumberFormatInfoForThread;
    
    [ThreadStatic]
    public static TraceBack SystemState_RawTraceBack;

    [ThreadStatic]
    public static int PythonFunction_Depth;

#endif
}
