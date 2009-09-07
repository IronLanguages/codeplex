/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/


using System;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    public static partial class DynamicInstructions {
        private static Type GetDynamicInstructionType(Type delegateType) {
            Type[] argTypes = delegateType.GetGenericArguments();
            if (argTypes.Length == 0) return null;
            Type genericType;
            Type[] newArgTypes = ArrayUtils.RemoveFirst(argTypes);
            switch (newArgTypes.Length) {
                #region Generated Dynamic Instruction Types

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_types from: generate_dynamic_instructions.py

                case 1: genericType = typeof(DynamicInstruction<>); break;
                case 2: genericType = typeof(DynamicInstruction<,>); break;
                case 3: genericType = typeof(DynamicInstruction<,,>); break;
                case 4: genericType = typeof(DynamicInstruction<,,,>); break;
                case 5: genericType = typeof(DynamicInstruction<,,,,>); break;
                case 6: genericType = typeof(DynamicInstruction<,,,,,>); break;
                case 7: genericType = typeof(DynamicInstruction<,,,,,,>); break;
                case 8: genericType = typeof(DynamicInstruction<,,,,,,,>); break;
                case 9: genericType = typeof(DynamicInstruction<,,,,,,,,>); break;
                case 10: genericType = typeof(DynamicInstruction<,,,,,,,,,>); break;
                case 11: genericType = typeof(DynamicInstruction<,,,,,,,,,,>); break;
                case 12: genericType = typeof(DynamicInstruction<,,,,,,,,,,,>); break;
                case 13: genericType = typeof(DynamicInstruction<,,,,,,,,,,,,>); break;
                case 14: genericType = typeof(DynamicInstruction<,,,,,,,,,,,,,>); break;
                case 15: genericType = typeof(DynamicInstruction<,,,,,,,,,,,,,,>); break;
                case 16: genericType = typeof(DynamicInstruction<,,,,,,,,,,,,,,,>); break;

                // *** END GENERATED CODE ***

                #endregion
                default: throw new NotImplementedException();
            }
            
            return genericType.MakeGenericType(newArgTypes);
        }
    }

    #region Generated Dynamic Instructions

    // *** BEGIN GENERATED CODE ***
    // generated by function: gen_instructions from: generate_dynamic_instructions.py

    internal class DynamicInstruction<TRet> : Instruction {
        private CallSite<Func<CallSite,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<TRet>(CallSite<Func<CallSite,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 0; } }
        public override int Run(InterpretedFrame frame) {
            frame.Push(_site.Target(
                _site));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,TRet>(CallSite<Func<CallSite,T0,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,TRet>(CallSite<Func<CallSite,T0,T1,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 2; } }
        public override int Run(InterpretedFrame frame) {
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,TRet>(CallSite<Func<CallSite,T0,T1,T2,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 3; } }
        public override int Run(InterpretedFrame frame) {
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 4; } }
        public override int Run(InterpretedFrame frame) {
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,T4,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,T4,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,T4,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,T4,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,T4,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 5; } }
        public override int Run(InterpretedFrame frame) {
            object arg4 = frame.Pop();
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3,
                (T4)arg4));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,T4,T5,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,T4,T5,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 6; } }
        public override int Run(InterpretedFrame frame) {
            object arg5 = frame.Pop();
            object arg4 = frame.Pop();
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3,
                (T4)arg4,
                (T5)arg5));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 7; } }
        public override int Run(InterpretedFrame frame) {
            object arg6 = frame.Pop();
            object arg5 = frame.Pop();
            object arg4 = frame.Pop();
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3,
                (T4)arg4,
                (T5)arg5,
                (T6)arg6));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 8; } }
        public override int Run(InterpretedFrame frame) {
            object arg7 = frame.Pop();
            object arg6 = frame.Pop();
            object arg5 = frame.Pop();
            object arg4 = frame.Pop();
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3,
                (T4)arg4,
                (T5)arg5,
                (T6)arg6,
                (T7)arg7));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 9; } }
        public override int Run(InterpretedFrame frame) {
            object arg8 = frame.Pop();
            object arg7 = frame.Pop();
            object arg6 = frame.Pop();
            object arg5 = frame.Pop();
            object arg4 = frame.Pop();
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3,
                (T4)arg4,
                (T5)arg5,
                (T6)arg6,
                (T7)arg7,
                (T8)arg8));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 10; } }
        public override int Run(InterpretedFrame frame) {
            object arg9 = frame.Pop();
            object arg8 = frame.Pop();
            object arg7 = frame.Pop();
            object arg6 = frame.Pop();
            object arg5 = frame.Pop();
            object arg4 = frame.Pop();
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3,
                (T4)arg4,
                (T5)arg5,
                (T6)arg6,
                (T7)arg7,
                (T8)arg8,
                (T9)arg9));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 11; } }
        public override int Run(InterpretedFrame frame) {
            object arg10 = frame.Pop();
            object arg9 = frame.Pop();
            object arg8 = frame.Pop();
            object arg7 = frame.Pop();
            object arg6 = frame.Pop();
            object arg5 = frame.Pop();
            object arg4 = frame.Pop();
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3,
                (T4)arg4,
                (T5)arg5,
                (T6)arg6,
                (T7)arg7,
                (T8)arg8,
                (T9)arg9,
                (T10)arg10));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 12; } }
        public override int Run(InterpretedFrame frame) {
            object arg11 = frame.Pop();
            object arg10 = frame.Pop();
            object arg9 = frame.Pop();
            object arg8 = frame.Pop();
            object arg7 = frame.Pop();
            object arg6 = frame.Pop();
            object arg5 = frame.Pop();
            object arg4 = frame.Pop();
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3,
                (T4)arg4,
                (T5)arg5,
                (T6)arg6,
                (T7)arg7,
                (T8)arg8,
                (T9)arg9,
                (T10)arg10,
                (T11)arg11));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 13; } }
        public override int Run(InterpretedFrame frame) {
            object arg12 = frame.Pop();
            object arg11 = frame.Pop();
            object arg10 = frame.Pop();
            object arg9 = frame.Pop();
            object arg8 = frame.Pop();
            object arg7 = frame.Pop();
            object arg6 = frame.Pop();
            object arg5 = frame.Pop();
            object arg4 = frame.Pop();
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3,
                (T4)arg4,
                (T5)arg5,
                (T6)arg6,
                (T7)arg7,
                (T8)arg8,
                (T9)arg9,
                (T10)arg10,
                (T11)arg11,
                (T12)arg12));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 14; } }
        public override int Run(InterpretedFrame frame) {
            object arg13 = frame.Pop();
            object arg12 = frame.Pop();
            object arg11 = frame.Pop();
            object arg10 = frame.Pop();
            object arg9 = frame.Pop();
            object arg8 = frame.Pop();
            object arg7 = frame.Pop();
            object arg6 = frame.Pop();
            object arg5 = frame.Pop();
            object arg4 = frame.Pop();
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3,
                (T4)arg4,
                (T5)arg5,
                (T6)arg6,
                (T7)arg7,
                (T8)arg8,
                (T9)arg9,
                (T10)arg10,
                (T11)arg11,
                (T12)arg12,
                (T13)arg13));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }
    internal class DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,TRet> : Instruction {
        private CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,TRet>> _site;
        public static Instruction Factory(CallSiteBinder binder) {
            return new DynamicInstruction<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,TRet>(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,TRet>>.Create(binder));
        }
        private DynamicInstruction(CallSite<Func<CallSite,T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,TRet>> site) {
            this._site = site;
        }
        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 15; } }
        public override int Run(InterpretedFrame frame) {
            object arg14 = frame.Pop();
            object arg13 = frame.Pop();
            object arg12 = frame.Pop();
            object arg11 = frame.Pop();
            object arg10 = frame.Pop();
            object arg9 = frame.Pop();
            object arg8 = frame.Pop();
            object arg7 = frame.Pop();
            object arg6 = frame.Pop();
            object arg5 = frame.Pop();
            object arg4 = frame.Pop();
            object arg3 = frame.Pop();
            object arg2 = frame.Pop();
            object arg1 = frame.Pop();
            object arg0 = frame.Pop();
            frame.Push(_site.Target(
                _site,
                (T0)arg0,
                (T1)arg1,
                (T2)arg2,
                (T3)arg3,
                (T4)arg4,
                (T5)arg5,
                (T6)arg6,
                (T7)arg7,
                (T8)arg8,
                (T9)arg9,
                (T10)arg10,
                (T11)arg11,
                (T12)arg12,
                (T13)arg13,
                (T14)arg14));
            return +1;
        }
        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }

    // *** END GENERATED CODE ***

    #endregion


}
