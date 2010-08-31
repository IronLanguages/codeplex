/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Dynamic;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.ComInterop;

namespace ComTest {
    public partial class Scenarios {

        public static void Positive_Convert() {
            object com = Utils.CreateComObject("DlrComLibrary.Properties");

            var s_convert = CallSite<Func<CallSite, object, IDispatchForReflection>>.Create(TestConvert.Create(typeof(IDispatchForReflection)));
            var result = s_convert.Target(s_convert, com);

            Utils.Equal(com, result);
        }

        public static void Negative_Convert() {
            object com = Utils.CreateComObject("DlrComLibrary.Properties");

            var s_convert = CallSite<Func<CallSite, object, IDisposable>>.Create(TestConvert.Create(typeof(IDisposable)));

            try {
                var result = s_convert.Target(s_convert, com);
                throw new Exception("should not get here");
            } catch (InvalidCastException) {
            }

            var s_convert1 = CallSite<Func<CallSite, object, object>>.Create(TestConvert.Create(typeof(Object)));
            try {
                var result = s_convert1.Target(s_convert1, com);
                throw new Exception("should not get here");
            } catch (NotImplementedException) {
            }

        }

        public delegate void InOutStr(string i, out string o);
        public delegate void InOutStrSite(CallSite site, object target, string i, out string o);

        public static void InOutHandler(string i, out string o) {
            o = "in: " + i;
        }

        // bug624747
        public static void Negative_EventsHandlers() {

            object com = Utils.CreateComObject("DlrComLibrary.DispEvents");

            var s_getEvent = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("eInOutBstr"));
            object bde = s_getEvent.Target(s_getEvent, com);

            var addHandler = CallSite<Func<CallSite, object, object, object>>.Create(new BinaryOpBinder(ExpressionType.AddAssign));
            var removeHandler = CallSite<Func<CallSite, object, object, object>>.Create(new BinaryOpBinder(ExpressionType.SubtractAssign));

            try {
                addHandler.Target(addHandler, bde, null);
                throw new Exception("should not get here");
            } catch (ArgumentNullException) {
            }

            try {
                addHandler.Target(addHandler, bde, 1);
                throw new Exception("should not get here");
            } catch (InvalidOperationException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                    Utils.Equal("Attempting to pass an event handler of an unsupported type.", ex.Message);
            }

            try {
                removeHandler.Target(removeHandler, bde, null);
                throw new Exception("should not get here");
            } catch (ArgumentNullException) {
            }


            try {
                removeHandler.Target(removeHandler, bde, 1);
                throw new Exception("should not get here");
            } catch (InvalidOperationException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                    Utils.Equal("Attempting to pass an event handler of an unsupported type.", ex.Message);
            }
        }


        public static void Positive_EventsRemove() {

            object com = Utils.CreateComObject("DlrComLibrary.DispEvents");

            var s_getEvent = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("eInOutBstr"));
            object bde = s_getEvent.Target(s_getEvent, com);

            InOutStr handler = InOutHandler;

            var removeHandler = CallSite<Func<CallSite, object, object, object>>.Create(new BinaryOpBinder(ExpressionType.SubtractAssign));
            removeHandler.Target(removeHandler, bde, handler);

            var s_getTrigger = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("triggerInOutBstr"));
            object trigger = s_getTrigger.Target(s_getTrigger, com);

            var s_invokeTrigger = CallSite<InOutStrSite>.Create(TestInvoke.Create(typeof(InOutStrSite)));

            string o = "ha";
            s_invokeTrigger.Target(s_invokeTrigger, trigger, "hello", out o);
            Utils.Equal(null, o);

        }


        public static void Positive_EventsComCallableHandlesComEvent() {

            object com = Utils.CreateComObject("DlrComLibrary.DispEvents");
            var s_getEvent = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("eInOutBstr"));
            object bde = s_getEvent.Target(s_getEvent, com);

            object como = Utils.CreateComObject("DlrComLibrary.OutParams");

            // must use mVariant here. It would be nice to use mBstr, but eInOutBstr has the second parameter 
            // is "out string" and when it goes through DispInvoke, "out" params become nulls and loose their types
            // so "out string" becomes "ref object" and we can match it only with a handler that takes "ref Variant"
            var s_comHandler = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("mVariant", true));
            object handler = s_comHandler.Target(s_comHandler, como);

            var addHandler = CallSite<Func<CallSite, object, object, object>>.Create(new BinaryOpBinder(ExpressionType.AddAssign));
            addHandler.Target(addHandler, bde, handler);

            var s_getTrigger = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("triggerInOutBstr"));
            object trigger = s_getTrigger.Target(s_getTrigger, com);

            var s_invokeTrigger = CallSite<InOutStrSite>.Create(TestInvoke.Create(typeof(InOutStrSite)));

            string o;
            s_invokeTrigger.Target(s_invokeTrigger, trigger, "hello", out o);
            Utils.Equal("hello", o);

            o = null;
            s_invokeTrigger.Target(s_invokeTrigger, trigger, "bye", out o);
            Utils.Equal("bye", o);
        }


        public static void Positive_EventsDelegate() {

            object com = Utils.CreateComObject("DlrComLibrary.DispEvents");

            var s_getEvent = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("eInOutBstr"));
            object bde = s_getEvent.Target(s_getEvent, com);

            InOutStr handler = InOutHandler;

            var addHandler = CallSite<Func<CallSite, object, object, object>>.Create(new BinaryOpBinder(ExpressionType.AddAssign));
            addHandler.Target(addHandler, bde, handler);

            var s_getTrigger = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("triggerInOutBstr"));
            object trigger = s_getTrigger.Target(s_getTrigger, com);

            var s_invokeTrigger = CallSite<InOutStrSite>.Create(TestInvoke.Create(typeof(InOutStrSite)));

            string o;
            s_invokeTrigger.Target(s_invokeTrigger, trigger, "hello", out o);
            Utils.Equal("in: hello", o);

        }



        public static void Positive_Events() {

            object com = Utils.CreateComObject("DlrComLibrary.DispEvents");
            string flag = "before";

            var s_getEvent = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("eInUshort"));
            object bde = s_getEvent.Target(s_getEvent, com);

            var addHandler = CallSite<Func<CallSite, object, object, object>>.Create(new BinaryOpBinder(ExpressionType.AddAssign));
            addHandler.Target(
                addHandler,
                bde,
                new ComEventTarget(
                    new Action<ushort>(
                        val => flag = "eInUshort: " + val
                    )
                )
            );

            var s_getTrigger = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("triggerUShort"));
            object trigger = s_getTrigger.Target(s_getTrigger, com);

            var s_invokeTrigger = CallSite<Func<CallSite, object, object, object>>.Create(TestInvoke.Create(typeof(Func<CallSite, object, object, object>)));

            Utils.Equal("before", flag);
            s_invokeTrigger.Target(s_invokeTrigger, trigger, 123);
            Utils.Equal("eInUshort: 123", flag);

        }



        public static void Positive_Invoke() {
            object com = Utils.CreateComObject("DlrComLibrary.Properties");

            var s_set = CallSite<Action<CallSite, object, object, object>>.Create(TestSetIndex.Create(typeof(Action<CallSite, object, object, object>)));
            var s_invoke = CallSite<Func<CallSite, object, object, object>>.Create(TestInvoke.Create(typeof(Func<CallSite, object, object, object>)));

            s_set.Target(s_set, com, 42, true);
            object result = s_invoke.Target(s_invoke, com, 33);

            Utils.Equal(true, result);

            s_set.Target(s_set, com, 42, false);
            result = s_invoke.Target(s_invoke, com, 33);

            Utils.Equal(false, result);
        }


        public static void Negative_Excel() {
            object ExcelApp;
            try {
                ExcelApp = Utils.CreateComObject("Excel.Application");
            } catch {
                // In cases where we don't have XL, we'll use this object which is around everywhere.
                ExcelApp = Utils.CreateComObject("Scripting.FileSystemObject");
            }

            //should not crash here
            var names = ComBinder.GetDynamicMemberNames(ExcelApp);
            var e = names.GetEnumerator();
            //should be nonempty
            e.MoveNext();
        }

        public static void Negative_ManyParams() {
            object como = Utils.CreateComObject("DlrComLibrary.ParamsInRetval");

            var mScode = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>>.
                Create(TestCall.Create("mScode", typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>)));

            var val = new ErrorWrapper(123);
            try {
                var result = mScode.Target(mScode, como, val, val, val, val, val, val, val, val, val);
                throw new Exception("should not get here");
            } catch (System.Reflection.TargetParameterCountException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                    Utils.Equal("Error while invoking mScode.", ex.Message);
            }
        }

        public static void Positive_SCODE() {
            object como = Utils.CreateComObject("DlrComLibrary.ParamsInRetval");

            var mScode = CallSite<Func<CallSite, object, object, object>>.
                Create(TestCall.Create("mScode", typeof(Func<CallSite, object, object, object>)));

            var val = new ErrorWrapper(123);
            var result = mScode.Target(mScode, como, val);

            Utils.Equal(123, result);
        }


        public static void Positive_SettrongBox() {
            object como = Utils.CreateComObject("DlrComLibrary.Properties");

            var pVariantPut = CallSite<Action<CallSite, object, object>>.Create(TestSetMember.Create("pVariant"));

            var sb = new StrongBox<int>(123);
            pVariantPut.Target(pVariantPut, como, sb);

            var pVariantGet = CallSite<Func<CallSite, object, object>>.
                Create(TestCall.Create("pVariant", typeof(Func<CallSite, object, object>)));

            var result = pVariantGet.Target(pVariantGet, como);

            Utils.Equal(sb, result);
        }

        public static void Positive_Default() {
            object com = Utils.CreateComObject("DlrComLibrary.Properties");

            var s_set = CallSite<Action<CallSite, object, object, object>>.
                Create(TestSetIndex.Create(typeof(Action<CallSite, object, object, object>)));

            var s_get = CallSite<Func<CallSite, object, object, object>>.
                Create(TestGetIndex.Create(typeof(Func<CallSite, object, object, object>)));

            s_set.Target(s_set, com, 42, true);
            object result = s_get.Target(s_get, com, 33);

            Utils.Equal(true, result);

            s_set.Target(s_set, com, 42, false);
            result = s_get.Target(s_get, com, 33);

            Utils.Equal(false, result);
        }


        public static void Positive_CallableSet() {
            object com = Utils.CreateComObject("DlrComLibrary.Properties");

            var PropertyWithParamSite = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("PropertyWithParam"));
            object PropertyWithParam = PropertyWithParamSite.Target(PropertyWithParamSite, com);

            var s_set = CallSite<Action<CallSite, object, object, object>>.
                Create(TestSetIndex.Create(typeof(Action<CallSite, object, object, object>)));

            s_set.Target(s_set, PropertyWithParam, 42, 21);

            var s_get = CallSite<Func<CallSite, object, object, object>>.
                Create(TestGetIndex.Create(typeof(Func<CallSite, object, object, object>)));

            object result = s_get.Target(s_get, PropertyWithParam, 7);

            Utils.Equal(56.0, result);

        }
        public static void Positive_Nulls() {
            object como = Utils.CreateComObject("DlrComLibrary.ParamsInRetval");
            var mIDispatch = CallSite<Func<CallSite, object, object, object>>.
                Create(TestCall.Create("mIDispatch", typeof(Func<CallSite, object, object, object>)));

            object dw = new DispatchWrapper(null);

            object result = mIDispatch.Target(mIDispatch, como, dw);
            Utils.Equal(null, result);

            var mIUnknown = CallSite<Func<CallSite, object, object, object>>.
                Create(TestCall.Create("mIUnknown", typeof(Func<CallSite, object, object, object>)));

            object uw = new DispatchWrapper(null);

            result = mIUnknown.Target(mIUnknown, como, uw);
            Utils.Equal(null, result);

            var mBstr = CallSite<Func<CallSite, object, object, object>>.
                Create(TestCall.Create("mBstr", typeof(Func<CallSite, object, object, object>)));

            object bw = new BStrWrapper(null);

            result = mBstr.Target(mBstr, como, bw);
            Utils.Equal(null, result);

        }

        public static void Positive_PutPutRef() {
            object como = Utils.CreateComObject("DlrComLibrary.Properties");

            var pVariantPut = CallSite<Action<CallSite, object, object>>.Create(TestSetMember.Create("pVarIant"));
            pVariantPut.Target(pVariantPut, como, como);

            var pVariantGet = CallSite<Func<CallSite, object, object>>.
                Create(TestCall.Create("pVariant", typeof(Func<CallSite, object, object>)));

            var result = pVariantGet.Target(pVariantGet, como);

            Utils.Equal(como, result);

            pVariantPut.Target(pVariantPut, como, 123);
            result = pVariantGet.Target(pVariantGet, como);

            Utils.Equal(123, result);
        }

        // seems to fail once in a while because if IE being in a bad state.
        public static void Disabled_TestIE() {
            object com;

            com = Utils.CreateComObject("InternetExplorer.Application");

            var Navigate = CallSite<Action<CallSite, object, object>>.
                Create(TestCall.Create("NaviGate", typeof(Action<CallSite, object, object>)));

            Navigate.Target(Navigate, com, "about:blank");

            var s_get = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("LoCationName"));
            object sm = s_get.Target(s_get, com);

            var s_invoke = CallSite<Func<CallSite, object, object>>.Create(TestInvoke.Create(typeof(Func<CallSite, object, object>)));
            object rs = s_invoke.Target(s_invoke, sm);

            Utils.Equal("about:blank", rs);


            var StatusTextSet = CallSite<Action<CallSite, object, string>>.Create(TestSetMember.Create("StatusText"));
            StatusTextSet.Target(StatusTextSet, com, "Hello there");

            var StatusTextGet = CallSite<Func<CallSite, object, string>>.
                Create(TestGetMember.Create("StaTusText", false));

            var status = StatusTextGet.Target(StatusTextGet, com);

            // Odd little bug on XP with IE6.   Has trailing null chars
            if (System.Environment.OSVersion.VersionString.Contains("NT 5.1")) status = status.TrimEnd('\0');

            Utils.Equal("Hello there", status);
        }

        public static void Positive_TestComplex() {
            object x = (BigInteger)123;

            object como = Utils.CreateComObject("DlrComLibrary.OutParams");
            var s_callVoidVariantInOut = CallSite<VoidObjectRefObjectDelegate>.
                Create(TestCall.Create("mVariant", typeof(VoidObjectRefObjectDelegate)));

            object so = null;

            s_callVoidVariantInOut.Target(s_callVoidVariantInOut, como, x, ref so);
            Utils.Equal((BigInteger)123, so);
        }


        //TODO: blocked by integration. Uncomment when DlrComLibrary gets more scenarios.

        //public delegate void ArrDel(CallSite site, object o, object o_in, ref object o_out);

        //public static void Positive_TestSafearrayOut() {
        //    object como = Utils.CreateComObject("DlrComLibrary.OutParams");
        //    var s_callVoidVariantInOut = CallSite<ArrDel>.Create(CsCall.Create("mIntArray"));

        //    object si = new int[3];
        //    ((int[])si)[2] = 42;

        //    object dw = new int[10];

        //    s_callVoidVariantInOut.Target(s_callVoidVariantInOut, como, si, ref dw);

        //    Utils.Equal(42, ((int[])dw)[2]);
        //}



        public delegate void EnumRef(CallSite site, object o, E1 a, ref E1 b);
        public delegate void EnumRef1(CallSite site, object o, object a, ref object b);
        public static void Positive_Enum() {
            object comr = Utils.CreateComObject("DlrComLibrary.OutParams");

            E1 e = E1.a;

            var mByte = CallSite<EnumRef>.Create(TestCall.Create("mByte", typeof(EnumRef)));
            mByte.Target.Invoke(mByte, comr, E1.b, ref e);
            Utils.Equal(E1.b, e);

            object eo = E1.a;

            var mByte1 = CallSite<EnumRef1>.Create(TestCall.Create("mByte", typeof(EnumRef1)));
            mByte1.Target.Invoke(mByte1, comr, E1.b, ref eo);
            Utils.Equal(E1.b, (E1)eo);
        }

        public delegate object ConvNullableRef(CallSite site, object o, double? a, ref ConvertibleToDoubleStruct? b, double? c);
        public static void Positive_ConvNullable() {
            object comr = Utils.CreateComObject("DlrComLibrary.MultipleParams");

            double? var1 = -1;
            ConvertibleToDoubleStruct? var2 = new ConvertibleToDoubleStruct(7);
            double? var3 = 4;

            var mThreeParams = CallSite<Func<CallSite, object, object, object, object, object>>.
                Create(TestCall.Create("mThreeParams", typeof(Func<CallSite, object, object, object, object, object>)));

            var res1 = mThreeParams.Target.Invoke(mThreeParams, comr, var1, var2, var3);
            Utils.Equal(10.0, res1);

            var mThreeParamsN = CallSite<Func<CallSite, object, double?, ConvertibleToDoubleStruct?, double?, object>>.
                Create(TestCall.Create("mThreeParams", typeof(Func<CallSite, object, double?, ConvertibleToDoubleStruct?, double?, object>)));


            var res2 = (double?)mThreeParamsN.Target.Invoke(mThreeParamsN, comr, var1, var2, var3);
            Utils.Equal(10.0, res2);

        }


        public static void Negative_ConvMany() {
            object comr = Utils.CreateComObject("DlrComLibrary.MultipleParams");

            double? var1 = -1;
            ConvertibleToManyStruct? var2 = new ConvertibleToManyStruct(7);
            double? var3 = 4;

            try {
                var mThreeParams = CallSite<Func<CallSite, object, object, object, object, object>>.Create(TestCall.Create("mThreeParams", typeof(ConvNullableRef)));
                var res1 = mThreeParams.Target.Invoke(mThreeParams, comr, var1, var2, var3);
                Utils.Equal(10.0, res1);
                throw new Exception("should not get here");
            } catch (System.Reflection.AmbiguousMatchException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                    Utils.Equal("There are valid conversions from ConvertibleToManyStruct to Int32, Byte, Double and Decimal.", ex.Message);
            }

        }


        public static void Negative_ConvNullable() {
            object comr = Utils.CreateComObject("DlrComLibrary.MultipleParams");

            double? var1 = 1;
            ConvertibleToDoubleStruct? var2 = null;
            double? var3 = 3;

            var mThreeParams = CallSite<Func<CallSite, object, object, object, object, object>>.
                Create(TestCall.Create("mThreeParams", typeof(Func<CallSite, object, object, object, object, object>)));

            var res1 = mThreeParams.Target.Invoke(mThreeParams, comr, var1, var2, var3);
            Utils.Equal(4.0, res1);

            var mThreeParamsN = CallSite<Func<CallSite, object, double?, ConvertibleToDoubleStruct?, double?, object>>.
                Create(TestCall.Create("mThreeParams", typeof(Func<CallSite, object, double?, ConvertibleToDoubleStruct?, double?, object>)));

            try {
                var res2 = (double?)mThreeParamsN.Target.Invoke(mThreeParamsN, comr, var1, var2, var3);
                throw new Exception("should not get here");
            } catch (InvalidOperationException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                    Utils.Equal("Nullable object must have a value.", ex.Message);
            }

            var mThreeParamsNr = CallSite<ConvNullableRef>.
                Create(TestCall.Create("mThreeParams", typeof(ConvNullableRef)));

            try {
                var res3 = mThreeParamsNr.Target.Invoke(mThreeParamsNr, comr, var1, ref var2, var3);
                throw new Exception("should not get here");
            } catch (InvalidOperationException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                    Utils.Equal("Nullable object must have a value.", ex.Message);
            }

            // this will still not work since we do not do ByRef for implicitly convertible
            var2 = new ConvertibleToDoubleStruct(7);

            try {
                var res3 = mThreeParamsNr.Target.Invoke(mThreeParamsNr, comr, var1, ref var2, var3);
                Utils.Equal(10.0, res3);
            } catch (ArgumentException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                    Utils.Equal("Value does not fall within the expected range.", ex.Message);
            }

        }


        public delegate object NullableRef(CallSite site, object o, double? a, ref double? b, double? c);
        public static void Positive_Nullable() {
            object comr = Utils.CreateComObject("DlrComLibrary.MultipleParams");

            double? var1 = -1;
            double? var2 = 0;
            double? var3 = 4;

            var mThreeParams = CallSite<Func<CallSite, object, object, object, object, object>>.
                Create(TestCall.Create("mThreeParams", typeof(Func<CallSite, object, object, object, object, object>)));

            var res1 = mThreeParams.Target.Invoke(mThreeParams, comr, var1, var2, var3);
            Utils.Equal(3.0, res1);

            var mThreeParamsN = CallSite<Func<CallSite, object, double?, double?, double?, object>>.
                Create(TestCall.Create("mThreeParams", typeof(Func<CallSite, object, double?, double?, double?, object>)));

            var res2 = (double?)mThreeParamsN.Target.Invoke(mThreeParamsN, comr, var1, var2, var3);
            Utils.Equal(3.0, res2);

            var mThreeParamsNr = CallSite<NullableRef>.
                Create(TestCall.Create("mThreeParams", typeof(NullableRef)));

            var res3 = mThreeParamsNr.Target.Invoke(mThreeParamsNr, comr, var1, ref var2, var3);
            Utils.Equal(3.0, res3);
        }

        public static void Negative_Nullable() {
            object comr = Utils.CreateComObject("DlrComLibrary.MultipleParams");

            double? var1 = null;
            double? var2 = 0;
            double? var3 = 3;

            var mThreeParams = CallSite<Func<CallSite, object, object, object, object, object>>.
                Create(TestCall.Create("mThreeParams", typeof(Func<CallSite, object, object, object, object, object>)));

            var res1 = mThreeParams.Target.Invoke(mThreeParams, comr, var1, var2, var3);
            Utils.Equal(3.0, res1);

            var mThreeParamsN = CallSite<Func<CallSite, object, double?, double?, double?, object>>.
                Create(TestCall.Create("mThreeParams", typeof(Func<CallSite, object, double?, double?, double?, object>)));

            try {
                var res2 = mThreeParamsN.Target.Invoke(mThreeParamsN, comr, var1, var2, var3);
                throw new Exception("should not get here");
            } catch (InvalidOperationException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                    Utils.Equal("Nullable object must have a value.", ex.Message);
            }

            var mThreeParamsNr = CallSite<NullableRef>.
                Create(TestCall.Create("mThreeParams", typeof(NullableRef)));

            try {
                var res3 = mThreeParamsNr.Target.Invoke(mThreeParamsNr, comr, var1, ref var2, var3);
                throw new Exception("should not get here");
            } catch (InvalidOperationException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                    Utils.Equal("Nullable object must have a value.", ex.Message);
            }

        }

        public delegate object Delegate1(CallSite site, object comObj, object str1, ref string str2);
        public static void Positive_TestComScripting() {
            object com;

            com = Utils.CreateComObject("Scripting.FileSystemObject");

            var DrivesSite = CallSite<Func<CallSite, object, object>>.
                Create(TestCall.Create("Drives", typeof(Func<CallSite, object, object>)));

            var Drives = DrivesSite.Target(DrivesSite, com);

            var PathSite = CallSite<Func<CallSite, object, object>>.
                Create(TestCall.Create("Path", typeof(Func<CallSite, object, object>)));

            // get drives via Clr
            var drivesTbl = new Dictionary<string, System.IO.DriveInfo>();
            foreach (var drive in System.IO.DriveInfo.GetDrives()) {
                drivesTbl.Add(drive.Name, drive);
            }

            // remove drives found via COM
            foreach (var drive in (IEnumerable)Drives) {
                var path = PathSite.Target(PathSite, drive);
                Console.WriteLine(path.ToString());
                drivesTbl.Remove(path.ToString() + "\\");
            }

            // should be 0 drives in the table
            Utils.Equal(0, drivesTbl.Count);

            // second parameter strongly typed to string& , 
            // it is not required, but it should work.
            var BuildPathSite = CallSite<Delegate1>.Create(TestCall.Create("BuildPath", typeof(Delegate1)));
            string local_path = "bar";
            var BuiltPath = BuildPathSite.Target.Invoke(BuildPathSite, com, "C:\\foo\\", ref local_path);

            Utils.Equal("C:\\foo\\bar", BuiltPath);
            Utils.Equal("bar", local_path);  // should not change as it was passed to a ByVal target
        }

        public static void Positive_TestCom() {
            object com;

            com = Utils.CreateComObject("DlrComLibrary.DlrComServer");

            var s_get = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("SimpleMethod"));
            object sm = s_get.Target(s_get, com);


            var s_invoke = CallSite<Func<CallSite, object, object>>.Create(TestInvoke.Create(typeof(Func<CallSite, object, object>)));
            object rs = s_invoke.Target(s_invoke, sm);

            var s_callSimpleMethod = CallSite<Func<CallSite, object, object>>.
                Create(TestCall.Create("SimpleMethod", typeof(Func<CallSite, object, object>)));

            rs = s_callSimpleMethod.Target(s_callSimpleMethod, com);


            var s_getStringMethod = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("StringArguments"));
            object stringMethod = s_getStringMethod.Target(s_getStringMethod, com);


            var s_invokeString = CallSite<Func<CallSite, object, object, object, object>>.Create(TestInvoke.Create(typeof(Func<CallSite, object, object, object, object>)));
            object result = s_invokeString.Target(s_invokeString, stringMethod, "Hello", "World");
        }


        public static void Positive_Properties() {
            object com = Utils.CreateComObject("DlrComLibrary.Properties");

            var s_setpBstr = CallSite<Func<CallSite, object, object, object>>.Create(TestSetMember.Create("pBstr"));
            s_setpBstr.Target(s_setpBstr, com, "Hello");

            var s_getpBstr = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("pBstR", false));
            object bstr = s_getpBstr.Target(s_getpBstr, com);

            Utils.Equal("Hello", bstr);

            var s_setpPutAndPutRef = CallSite<Func<CallSite, object, object, object>>.Create(TestSetMember.Create("PutAndPutRefProperty"));
            s_setpPutAndPutRef.Target(s_setpPutAndPutRef, com, 4.0);

            var s_getpPutAndPutRef = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("PutAndPutRefProperty"));
            object bstr1 = s_getpPutAndPutRef.Target(s_getpPutAndPutRef, com);

            Utils.Equal(4.0, bstr1);

            var s_getpPutAndPutRef2 = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("PutAndPutRefProperty", false));
            object bstr2 = s_getpPutAndPutRef.Target(s_getpPutAndPutRef, com);

            Utils.Equal(4.0, bstr2);

        }

        public delegate void VoidSBoxDoubleDelegate(CallSite site, object o, StrongBox<double> a);
        public delegate void VoidRefDoubleDelegate(CallSite site, object o, ref double a);

        public delegate void VoidSBoxStringDelegate(CallSite site, object o, StrongBox<string> a);
        public delegate void VoidRefStringDelegate(CallSite site, object o, ref string a);
        public delegate void VoidRefObjectDelegate(CallSite site, object o, ref object a);

        public delegate void VoidObjectRefObjectDelegate(CallSite site, object o, object o_in, ref object o_out);

        public delegate void VoidCurrencySBoxObjectDelegate(CallSite site, object o, CurrencyWrapper a, StrongBox<object> b);
        public delegate void VoidDateRefDateDelegate(CallSite site, object o, out DateTime a, ref DateTime b);



        public static void Positive_DoubleRef() {
            object comr = Utils.CreateComObject("DlrComLibrary.InOutParams");

            StrongBox<double> box = new StrongBox<double>(123);

            var s_callVoidSboxDbl = CallSite<VoidSBoxDoubleDelegate>.
                Create(TestCall.Create("mSingleRefParam", typeof(VoidSBoxDoubleDelegate)));

            s_callVoidSboxDbl.Target(s_callVoidSboxDbl, comr, box);

            Utils.Equal(125.0, box.Value);

            // what happens if ref is a field of a heap object
            box.Value = 2.2;
            var s_callVoidRefDbl = CallSite<VoidRefDoubleDelegate>.
                Create(TestCall.Create("mSingleRefParam", typeof(VoidRefDoubleDelegate)));

            s_callVoidRefDbl.Target(s_callVoidRefDbl, comr, ref box.Value);

            Utils.Equal(4.2, box.Value);

            // what happens if ref is an element of an array (also on heap)
            double[] a = new double[] { 1.2, 0, 1 };
            s_callVoidRefDbl.Target(s_callVoidRefDbl, comr, ref a[0]);
            Utils.Equal(3.2, a[0]);
        }

        public static void Positive_TestByRefStringStronglyTyped() {
            object comr = Utils.CreateComObject("DlrComLibrary.InOutParams");

            string s = "hello";
            var s_callVoidRefString = CallSite<VoidRefStringDelegate>.
                Create(TestCall.Create("mBstr", typeof(VoidRefStringDelegate)));

            s_callVoidRefString.Target(s_callVoidRefString, comr, ref s);
            Utils.Equal("helloa", s);

            var s_callVoidSboxString = CallSite<VoidSBoxStringDelegate>.
                Create(TestCall.Create("mBstr", typeof(VoidSBoxStringDelegate)));

            StrongBox<string> sbs = new StrongBox<string>("hello");
            s_callVoidSboxString.Target(s_callVoidSboxString, comr, sbs);
            Utils.Equal("helloa", sbs.Value);

            s = null;
            s_callVoidRefString.Target(s_callVoidRefString, comr, ref s);
            Utils.Equal("a", s);


            s = "";
            s_callVoidRefString.Target(s_callVoidRefString, comr, ref s);
            Utils.Equal("a", s);
        }


        public static void Positive_TestByRefDateTime() {
            object comr = Utils.CreateComObject("DlrComLibrary.InOutParams");

            DateTime d1 = new DateTime(1961, 4, 12);
            DateTime d2 = default(DateTime);

            var s_callVoidRefDate = CallSite<VoidDateRefDateDelegate>.
                Create(TestCall.Create("mOutAndInOutParams", typeof(VoidDateRefDateDelegate)));

            s_callVoidRefDate.Target(s_callVoidRefDate, comr, out d1, ref d2);
            Utils.Equal(new DateTime(1961, 4, 12), d1);
            Utils.Equal(new DateTime(1961, 4, 12), d2);
        }

        public static void Positive_TestByRefCurrency() {
            object comr = Utils.CreateComObject("DlrComLibrary.InOutParams");
            object cy1 = new CurrencyWrapper(123);
            object cy2 = new CurrencyWrapper(42);

            var s_callVoidRefCurrency = CallSite<VoidObjectRefObjectDelegate>.
                Create(TestCall.Create("mInAndInOutParams", typeof(VoidObjectRefObjectDelegate)));

            s_callVoidRefCurrency.Target(s_callVoidRefCurrency, comr, cy1, ref cy2);
            Utils.Equal((decimal)123, ((CurrencyWrapper)cy1).WrappedObject);
            Utils.Equal((decimal)123, ((CurrencyWrapper)cy2).WrappedObject);

            CurrencyWrapper cy21 = new CurrencyWrapper(123);
            StrongBox<object> cy22 = new StrongBox<object>(new CurrencyWrapper(42));

            var s_callVoidSboxCurrency = CallSite<VoidCurrencySBoxObjectDelegate>.
                Create(TestCall.Create("mInAndInOutParams", typeof(VoidCurrencySBoxObjectDelegate)));

            s_callVoidSboxCurrency.Target(s_callVoidSboxCurrency, comr, cy21, cy22);
            Utils.Equal((decimal)123, cy21.WrappedObject);
            Utils.Equal((decimal)123, ((CurrencyWrapper)cy22.Value).WrappedObject);
        }

        public static void Positive_TestByRefImplicitlyConvertible() {
            object comr = Utils.CreateComObject("DlrComLibrary.InOutParams");
            object cy1 = new ConvertibleToCW(new CurrencyWrapper(123));
            object cy2 = new CurrencyWrapper(42);

            var s_callVoidRefCurrency = CallSite<VoidObjectRefObjectDelegate>.
                Create(TestCall.Create("mInAndInOutParams", typeof(VoidObjectRefObjectDelegate)));

            s_callVoidRefCurrency.Target(s_callVoidRefCurrency, comr, cy1, ref cy2);
            Utils.Equal((decimal)123, ((CurrencyWrapper)cy2).WrappedObject);
        }

        /// <summary>
        /// Test method for IConvertible
        /// </summary>
        /// <param name="tc">TypeCode enum that specifies which type is being used</param>
        /// <param name="expectedValue">Expected value from the COM method. The type of this arg corresponds to the 'tc' arg</param>
        /// <param name="dummyVal">Value that will be preset in the 'out' arg so that the effect of COM method can be tested</param>
        /// <param name="targetMethod">COM method name ( ex: mBstr, mDouble etc)</param>
        private static void TestIConvertible<T>(TypeCode tc, T expectedValue, T dummyVal, string targetMethod) {
            object como = Utils.CreateComObject("DlrComLibrary.OutParams");
            MyIconvertible cis = new MyIconvertible(expectedValue, tc);
            object sio = cis;
            object outObj = dummyVal;

            var s_ComMethod = CallSite<VoidObjectRefObjectDelegate>.
                Create(TestCall.Create(targetMethod, typeof(VoidObjectRefObjectDelegate)));

            s_ComMethod.Target(s_ComMethod, como, sio, ref outObj);
            Utils.Equal(expectedValue, outObj);
        }

        public static void Positive_TestByRefIConvertible_Double() {
            try {
                TestIConvertible<Double>(TypeCode.Double, 100.01, 99.01, "mDouble");
            } catch (Exception ex) {
                var v = ex;
            }
        }

        public static void Positive_TestByRefIConvertible_Int() {
            TestIConvertible<int>(TypeCode.Int32, 100, 99, "mint");
        }

        public static void Positive_TestByRefIConvertible_Boolean() {
            TestIConvertible<Boolean>(TypeCode.Boolean, true, false, "mVARIANT_BOOL");
        }

        public static void Positive_TestByRefIConvertible_Byte() {
            TestIConvertible<Byte>(TypeCode.Byte, 254, 1, "mByte");
        }

        public static void Positive_TestByRefIConvertible_Char() {
            object como = Utils.CreateComObject("DlrComLibrary.OutParams");
            MyIconvertible cis = new MyIconvertible('a', TypeCode.Char);
            object sio = cis;
            object outObj = 'x';

            var s_callVoidStringInOut = CallSite<VoidObjectRefObjectDelegate>.
                Create(TestCall.Create("mUShort", typeof(VoidObjectRefObjectDelegate)));

            s_callVoidStringInOut.Target(s_callVoidStringInOut, como, sio, ref outObj);
            Utils.Equal('a', Convert.ToChar(outObj));
        }

        public static void Positive_TestByRefIConvertible_DateTime() {//fails at (99,9,9)
            TestIConvertible(TypeCode.DateTime, new DateTime(2008, 12, 08), new DateTime(2007, 10, 1), "mDate");
        }

        public static void Positive_TestByRefIConvertible_Decimal() {
            TestIConvertible(TypeCode.Decimal, (Decimal)(-1.2), (Decimal)100.23, "mDECIMAL");
        }

        public static void Positive_TestByRefIConvertible_Int16() {
            TestIConvertible<Int16>(TypeCode.Int16, 30000, 21000, "mInt16");
        }

        public static void Positive_TestByRefIConvertible_Int64() {
            TestIConvertible<Int64>(TypeCode.Int64, Int64.MaxValue, Int64.MinValue, "mINT64");
        }

        // Wasn't able to use TestIConvertible function b/c the call makes a copy of the object which doesn't compare equally
        public static void Positive_TestByRefIConvertible_Object() {

            object como2 = Utils.CreateComObject("DlrComLibrary.OutParams");
            UnknownWrapper dw = new UnknownWrapper(como2);
            UnknownWrapper dw2 = new UnknownWrapper(como2);

            object como = Utils.CreateComObject("DlrComLibrary.OutParams");
            MyIconvertible cis = new MyIconvertible(dw, TypeCode.Object);

            object sio = cis;
            object outObj = dw2;

            var s_ComMethod = CallSite<VoidObjectRefObjectDelegate>.Create(TestCall.Create("mIUnknown", typeof(VoidObjectRefObjectDelegate)));
            s_ComMethod.Target(s_ComMethod, como, sio, ref outObj);


        }

        public static void Positive_TestByRefIConvertible_Empty() {
            VariantWrapper vw = new VariantWrapper(System.DBNull.Value);
            VariantWrapper vw2 = new VariantWrapper(1);

            object como = Utils.CreateComObject("DlrComLibrary.OutParams");
            MyIconvertible cis = new MyIconvertible(vw, TypeCode.Empty);

            object sio = cis;
            object outObj = vw2;

            var s_ComMethod = CallSite<VoidObjectRefObjectDelegate>.Create(TestCall.Create("mVariant", typeof(VoidObjectRefObjectDelegate)));
            s_ComMethod.Target(s_ComMethod, como, sio, ref outObj);

            vw2 = (VariantWrapper)outObj;
            if (!(vw2.WrappedObject == null)) {
                throw new Exception("should not get here");
            }
        }


        // Wasn't able to use TestIConvertible function b/c the call makes a copy of the object which doesn't compare equally
        public static void Positive_TestByRefIConvertible_DBNull() {
            VariantWrapper vw = new VariantWrapper(System.DBNull.Value);
            VariantWrapper vw2 = new VariantWrapper(1);

            object como = Utils.CreateComObject("DlrComLibrary.OutParams");
            MyIconvertible cis = new MyIconvertible(vw, TypeCode.DBNull);

            object sio = cis;
            object outObj = vw2;

            var s_ComMethod = CallSite<VoidObjectRefObjectDelegate>.Create(TestCall.Create("mVariant", typeof(VoidObjectRefObjectDelegate)));
            s_ComMethod.Target(s_ComMethod, como, sio, ref outObj);

            vw2 = (VariantWrapper)outObj;
            //Ensure we get a DBNull back
            if (!(vw2.WrappedObject is System.DBNull)) {
                throw new Exception("should not get here");
            }

        }

        //public static void Positive_TestByRefIConvertible_Object() {
        //    TestIConvertible<object>(TypeCode.Object, "new object()", "new Object()", "mBstr");
        //}

        public static void Positive_TestByRefIConvertible_Single() {
            try {
                TestIConvertible<VariantWrapper>(TypeCode.Single, new VariantWrapper(26), new VariantWrapper(26), "mVariant");
            } catch (NotImplementedException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                    Utils.Equal(ex.Message, "ToSingle() is not implemented");
            }
        }

        public static void Positive_TestByRefIConvertible_SByte() {
            TestIConvertible<SByte>(TypeCode.SByte, 26, 49, "mChar");
        }

        public static void Positive_TestByRefIConvertible_UInt16() {
            TestIConvertible<UInt16>(TypeCode.UInt16, 300, 100, "mUShort");
        }

        public static void Positive_TestByRefIConvertible_UInt32() {
            TestIConvertible<UInt32>(TypeCode.UInt32, 200, 100, "mUINT");
        }

        public static void Positive_TestByRefIConvertible_UINT64() {
            TestIConvertible<UInt64>(TypeCode.UInt64, 400, 101, "mUINT64");
        }

        public static void Positive_TestByRefIConvertible_String() {
            TestIConvertible<string>(TypeCode.String, "hello world", "dlrow olleh", "mBstr");
        }

        public static void Positive_TestByRefStringWeaklyTyped() {
            object como = Utils.CreateComObject("DlrComLibrary.OutParams");
            var s_callVoidStringInOut = CallSite<VoidObjectRefObjectDelegate>.
                Create(TestCall.Create("mBstr", typeof(VoidObjectRefObjectDelegate)));

            object sio = "lala";
            object so = "h";
            s_callVoidStringInOut.Target(s_callVoidStringInOut, como, sio, ref so);
            Utils.Equal("lala", so);

            sio = new BStrWrapper("bsw");
            so = "h";
            s_callVoidStringInOut.Target(s_callVoidStringInOut, como, sio, ref so);
            Utils.Equal("bsw", so);

            sio = new BStrWrapper("bsw1");
            so = new BStrWrapper(null);
            s_callVoidStringInOut.Target(s_callVoidStringInOut, como, sio, ref so);
            Utils.Equal("bsw1", ((BStrWrapper)so).WrappedObject);
        }

        public static void Positive_TestByVariantInOut() {
            object como = Utils.CreateComObject("DlrComLibrary.OutParams");
            var s_callVoidVariantInOut = CallSite<VoidObjectRefObjectDelegate>.
                Create(TestCall.Create("mVariant", typeof(VoidObjectRefObjectDelegate)));

            object sio = new int[100];
            ((int[])sio)[3] = 42;
            object so = null;

            s_callVoidVariantInOut.Target(s_callVoidVariantInOut, como, sio, ref so);
            Utils.Equal(42, ((int[])so)[3]);

            sio = System.Reflection.Missing.Value;
            so = null;
            s_callVoidVariantInOut.Target(s_callVoidVariantInOut, como, sio, ref so);
            Utils.Equal(System.Reflection.Missing.Value, so);

            sio = new Guid();
            so = null;
            s_callVoidVariantInOut.Target(s_callVoidVariantInOut, como, sio, ref so);
            Utils.Equal(sio, so);

            sio = new VariantWrapper(new Guid());
            so = null;
            s_callVoidVariantInOut.Target(s_callVoidVariantInOut, como, sio, ref so);
            Utils.Equal(((VariantWrapper)sio).WrappedObject, so);
        }

        public static void Positive_TestByDispatchInOut() {
            object como = Utils.CreateComObject("DlrComLibrary.OutParams");
            var s_callVoidVariantInOut = CallSite<VoidObjectRefObjectDelegate>.
                Create(TestCall.Create("mIDispatch", typeof(VoidObjectRefObjectDelegate)));

            object so = new DispatchWrapper(null);
            s_callVoidVariantInOut.Target(s_callVoidVariantInOut, como, como, ref so);
            Utils.Equal(como, ((DispatchWrapper)(so)).WrappedObject);
        }

        private static void Test_One_Primitive_Type(string method, object arg) {
            var com = Utils.CreateComObject("DlrComLibrary.OutParams");
            var site = CallSite<VoidObjectRefObjectDelegate>.Create(TestCall.Create(method, typeof(VoidObjectRefObjectDelegate)));
            var result = arg;
            site.Target(site, com, arg, ref result);
        }

        public static void Positive_PrimitiveTypes() {
            Test_One_Primitive_Type("mUShort", 'a');
            Test_One_Primitive_Type("mBstr", "Hello");
            Test_One_Primitive_Type("mByte", (byte)10);
            Test_One_Primitive_Type("mChar", (sbyte)98);
            Test_One_Primitive_Type("mCy", new CurrencyWrapper(10));
            Test_One_Primitive_Type("mDouble", 3.14);
            Test_One_Primitive_Type("mFloat", (float)3.14);
            Test_One_Primitive_Type("mLong", 123);
            Test_One_Primitive_Type("mLongLong", 123L);
            Test_One_Primitive_Type("mScode", new ErrorWrapper(0x01234));
            Test_One_Primitive_Type("mShort", (short)10);
            Test_One_Primitive_Type("mUlong", 1234U);
            Test_One_Primitive_Type("mULongLong", 1234UL);
            Test_One_Primitive_Type("mUShort", (ushort)123);
            Test_One_Primitive_Type("mUCHAR", (byte)10);
            Test_One_Primitive_Type("mINT16", (short)123);
            Test_One_Primitive_Type("mINT64", 1234567890L);
            Test_One_Primitive_Type("mint", 123456);
            Test_One_Primitive_Type("m__int32", 12345);
            Test_One_Primitive_Type("mUINT", 1234U);
            Test_One_Primitive_Type("mUINT64", 1234567980UL);
            Test_One_Primitive_Type("mINT8", (sbyte)10);
            Test_One_Primitive_Type("mwchar_t", 'a');
            Test_One_Primitive_Type("mDECIMAL", (decimal)100);
            Test_One_Primitive_Type("mCURRENCY", new CurrencyWrapper(100));
            Test_One_Primitive_Type("mVARIANT_BOOL", true);
            Test_One_Primitive_Type("mVARIANT_BOOL", false);
        }

        public delegate void NullableCharDelegate(CallSite site, object com, char? arg, ref object result);

        private static void Test_One_Nullable_Char(string method) {
            var com = Utils.CreateComObject("DlrComLibrary.OutParams");
            var site = CallSite<NullableCharDelegate>.Create(TestCall.Create(method, typeof(NullableCharDelegate)));

            char? ch = 'a';
            object result = ch;
            site.Target(site, com, ch, ref result);
        }

        public static void Positive_NullableChar() {
            Test_One_Nullable_Char("mUShort");
            Test_One_Nullable_Char("mwchar_t");
        }

        public static void Negative_TestByRefCurrency() {
            object comr = Utils.CreateComObject("DlrComLibrary.InOutParams");

            object cy1 = new ConvertibleToCW(new CurrencyWrapper(123));
            object cy2 = new ConvertibleToCW(new CurrencyWrapper(42));

            var s_callVoidRefCurrency = CallSite<VoidObjectRefObjectDelegate>.
                Create(TestCall.Create("mInAndInOutParams", typeof(VoidObjectRefObjectDelegate)));

            try {
                s_callVoidRefCurrency.Target(s_callVoidRefCurrency, comr, cy1, ref cy2);
                throw new Exception("should not get here");
            } catch (ArgumentException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US") {
                    if (ex.Message != "Could not convert argument 0 for call to mInAndInOutParams.") {
                        throw;
                    }
                }
                Utils.Equal((decimal)42, ((CurrencyWrapper)((ConvertibleToCW)cy2)).WrappedObject);
            }
        }

        public static void Negative_TestByRefString() {
            object comr = Utils.CreateComObject("DlrComLibrary.InOutParams");

            var s_callVoidRefObjectStr = CallSite<VoidRefObjectDelegate>.
                Create(TestCall.Create("mBstr", typeof(VoidRefObjectDelegate)));

            object so = "###";
            s_callVoidRefObjectStr.Target(s_callVoidRefObjectStr, comr, ref so);
            Utils.Equal(so, "###a");

            try {
                so = null;
                s_callVoidRefObjectStr.Target(s_callVoidRefObjectStr, comr, ref so);
                throw new Exception("should not get here");
            } catch (ArgumentException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US") {
                    if (ex.Message != "Could not convert argument 0 for call to mBstr.") {
                        throw;
                    }
                }
                Utils.Equal(so, null);
            }

            try {
                so = 42;
                s_callVoidRefObjectStr.Target(s_callVoidRefObjectStr, comr, ref so);

                throw new Exception("should not get here");
            } catch (ArgumentException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US") {
                    if (ex.Message != "Could not convert argument 0 for call to mBstr.") {
                        throw;
                    }
                }
                Utils.Equal(so, 42);
            }
        }

        public static void Negative_ConvertibleToString() {
            object comr = Utils.CreateComObject("DlrComLibrary.InOutParams");
            var s_callVoidRefObjectStr = CallSite<VoidRefObjectDelegate>.
                Create(TestCall.Create("mBstr", typeof(VoidRefObjectDelegate)));

            object so = null;
            ConvertibleToString cs = new ConvertibleToString("convertibleToString");
            try {
                so = cs;
                s_callVoidRefObjectStr.Target(s_callVoidRefObjectStr, comr, ref so);

                throw new Exception("should not get here");
            } catch (ArgumentException ex) {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US") {
                    if (ex.Message != "Could not convert argument 0 for call to mBstr.") {
                        throw;
                    }
                }
                Utils.Equal(so, cs);
            }

        }

        // Bug 695326
        public static void Positive_SetMemberValue() {
            object library = Utils.CreateComObject("DlrComLibrary.IndexedProp");

            var s0 = CallSite<Action<CallSite, object, int>>.Create(TestSetMember.Create("IntOne"));
            var s1 = CallSite<Func<CallSite, object, int, object>>.Create(TestSetMember.Create("IntOne"));

            s0.Target(s0, library, 1);

            object result = s1.Target(s1, library, 2);

            Utils.Equal(result, 2);
        }

        // Bug 695326
        public static void Positive_SetIndexValue() {
            object library = Utils.CreateComObject("DlrComLibrary.IndexedProp");

            var s0 = CallSite<Func<CallSite, object, object>>.Create(TestGetMember.Create("IntTwo"));
            var s1 = CallSite<Func<CallSite, object, int, int, object>>.Create(TestSetIndex.Create(typeof(Func<CallSite, object, int, int, object>)));

            object indexer = s0.Target(s0, library);
            object result = s1.Target(s1, indexer, 1, 2);

            Utils.Equal(result, 2);
        }

        public static void Positive_GetMembers()
        {
            //Use reflection to get the internal methods we're gong to call
            MethodInfo getdynamicdatamembernames = typeof(ComBinder).GetMethod("GetDynamicDataMemberNames", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo getdynamicdatamembers = typeof(ComBinder).GetMethod("GetDynamicDataMembers", BindingFlags.Static | BindingFlags.NonPublic);
            if (getdynamicdatamembernames == null || getdynamicdatamembers == null)
                throw new InvalidOperationException("Couldn't retrieve GetDynamicDataMember* methods from ComBinder");

            //Instantiate the com objects we're going to inspect (all from DlrComLibrary)
            object dlrcomserver = Utils.CreateComObject("DlrComLibrary.DlrComServer");
            object properties = Utils.CreateComObject("DlrComLibrary.Properties");
            object hiddenmembers = Utils.CreateComObject("DlrComLibrary.HiddenMembers");

            //These are the members we expect for each com object
            //string[] dlrcomserver_members = new string[] { };
            string[] properties_members = new string[] { "pDate", "RefProperty", "PutAndPutRefProperty", "pVariant", "ReadOnlyProperty", "pBstr", "pLong" };
            string[] hiddenmembers_members = new string[] { "SimpleProperty", "HiddenProperty" };

            //For each of our three com objects...

            //Get the member names
            IList<string> membernames = (IList<string>)getdynamicdatamembernames.Invoke(null, new object[] { dlrcomserver });
            //Ensure that list of names is correct
            if (membernames.Count != 0)
                throw new InvalidOperationException("Unexpected number of data members in dlrcomserver");
            //Get the data for those members
            IList<KeyValuePair<string, object>> memberdata = (IList<KeyValuePair<string, object>>)getdynamicdatamembers.Invoke(null, new object[] { dlrcomserver, membernames });
            //Ensure that data is correct
            if (memberdata.Count != 0)
                throw new InvalidOperationException("Unexpected amount of data in dlrcomserver");


            membernames = (IList<string>)getdynamicdatamembernames.Invoke(null, new object[] { properties });
            if (membernames.Count != properties_members.Length)
                throw new InvalidOperationException("Unexpected number of data members in properties");
            foreach (string member in properties_members)
                if (!membernames.Contains(member))
                    throw new InvalidOperationException("Unexpected data members in properties");
            memberdata = (IList<KeyValuePair<string, object>>)getdynamicdatamembers.Invoke(null, new object[] { properties, membernames });
            if (memberdata.Count != 7) //@TODO - Verify the values returned for each data point
                throw new InvalidOperationException("Unexpected amount of data in properties");


            membernames = (IList<string>)getdynamicdatamembernames.Invoke(null, new object[] { hiddenmembers });
            if (membernames.Count != hiddenmembers_members.Length)
                throw new InvalidOperationException("Unexpected number of data members in hiddenmembers");
            foreach (string member in hiddenmembers_members)
                if (!membernames.Contains(member))
                    throw new InvalidOperationException("Unexpected data members in hiddenmembers");
            memberdata = (IList<KeyValuePair<string, object>>)getdynamicdatamembers.Invoke(null, new object[] { hiddenmembers, membernames });
            if (memberdata.Count != 2) //@TODO - Verify the values returned for each data point
                throw new InvalidOperationException("Unexpected amount of data in hiddenmembers");


            //Try a restricted member list, in other words, ask for fewer names than we could get
            memberdata = (IList<KeyValuePair<string, object>>)getdynamicdatamembers.Invoke(null, new object[] { hiddenmembers, new string[] {"SimpleProperty"} });
            if (memberdata.Count != 1) //@TODO - Verify the values returned for each data point
                throw new InvalidOperationException("Unexpected amount of data in hiddenmembers");
            if (memberdata[0].Key != "SimpleProperty" || (int)(memberdata[0].Value) != 0)
                throw new InvalidOperationException("Unexpected data");


            //Ask for a name that doesn't exist on the object
            memberdata = (IList<KeyValuePair<string, object>>)getdynamicdatamembers.Invoke(null, new object[] { hiddenmembers, new string[] { "SomethingNonExistent" } });
            if (memberdata.Count != 0) //@TODO - Verify the values returned for each data point
                throw new InvalidOperationException("Unexpected amount of data in hiddenmembers");


            //Ask for a null name
            memberdata = (IList<KeyValuePair<string, object>>)getdynamicdatamembers.Invoke(null, new object[] { hiddenmembers, new string[] { null } });
            if (memberdata.Count != 0) //@TODO - Verify the values returned for each data point
                throw new InvalidOperationException("Unexpected amount of data in hiddenmembers");


            //Pass null for the names and expect all possible names to be returned
            memberdata = (IList<KeyValuePair<string, object>>)getdynamicdatamembers.Invoke(null, new object[] { hiddenmembers, null });
            if (memberdata.Count != 2) //@TODO - Verify the values returned for each data point
                throw new InvalidOperationException("Unexpected amount of data in hiddenmembers");
        }

        /*
                //Bug 562825
                class Reference { }
                struct Value { }
                public static void Positive_PassingUnknownObjects() {
                    object comobj = Utils.CreateComObject("DlrComLibrary.Properties");
                    foreach (object val in new object[] { new Reference(), new StrongBox<Value>(new Value()) }) {
                        var pVariantPut = CallSite<Action<CallSite, object, object>>.Create(TestGetMember.Create("pVariant"));

                        pVariantPut.Target(pVariantPut, comobj, val);

                        var pVariantGet = CallSite<Func<CallSite, object, object>>.
                            Create(TestCall.Create("pVariant", typeof(Func<CallSite, object, object>)));

                        var result = pVariantGet.Target(pVariantGet, comobj);

                        Utils.Equal(val, result);
                    }
                }*/
    }
}
