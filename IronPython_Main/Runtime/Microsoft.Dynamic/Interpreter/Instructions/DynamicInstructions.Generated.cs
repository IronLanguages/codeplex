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


using System;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    internal partial class DynamicInstructionN {
        internal static Type GetDynamicInstructionType(Type delegateType) {
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
                default:
                    throw Assert.Unreachable;
            }
            
            return genericType.MakeGenericType(newArgTypes);
        }

        internal static Instruction CreateUntypedInstruction(CallSiteBinder binder, int argCount) {
            switch (argCount) {
                #region Generated Untyped Dynamic Instructions

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_untyped from: generate_dynamic_instructions.py

                case 0: return DynamicInstruction<object>.Factory(binder);
                case 1: return DynamicInstruction<object, object>.Factory(binder);
                case 2: return DynamicInstruction<object, object, object>.Factory(binder);
                case 3: return DynamicInstruction<object, object, object, object>.Factory(binder);
                case 4: return DynamicInstruction<object, object, object, object, object>.Factory(binder);
                case 5: return DynamicInstruction<object, object, object, object, object, object>.Factory(binder);
                case 6: return DynamicInstruction<object, object, object, object, object, object, object>.Factory(binder);
                case 7: return DynamicInstruction<object, object, object, object, object, object, object, object>.Factory(binder);
                case 8: return DynamicInstruction<object, object, object, object, object, object, object, object, object>.Factory(binder);
                case 9: return DynamicInstruction<object, object, object, object, object, object, object, object, object, object>.Factory(binder);
                case 10: return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);
                case 11: return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);
                case 12: return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);
                case 13: return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);
                case 14: return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);
                case 15: return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);

                // *** END GENERATED CODE ***

                #endregion
                
                default: return null;
            }
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 0; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 0] = _site.Target(_site);
            frame.StackIndex -= -1;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 1] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 1]);
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 2; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 2] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 2], (T1)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 1;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 3; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 3] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 3], (T1)frame.Data[frame.StackIndex - 2], (T2)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 2;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 4; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 4] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 4], (T1)frame.Data[frame.StackIndex - 3], (T2)frame.Data[frame.StackIndex - 2], (T3)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 3;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 5; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 5] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 5], (T1)frame.Data[frame.StackIndex - 4], (T2)frame.Data[frame.StackIndex - 3], (T3)frame.Data[frame.StackIndex - 2], (T4)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 4;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 6; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 6] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 6], (T1)frame.Data[frame.StackIndex - 5], (T2)frame.Data[frame.StackIndex - 4], (T3)frame.Data[frame.StackIndex - 3], (T4)frame.Data[frame.StackIndex - 2], (T5)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 5;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 7; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 7] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 7], (T1)frame.Data[frame.StackIndex - 6], (T2)frame.Data[frame.StackIndex - 5], (T3)frame.Data[frame.StackIndex - 4], (T4)frame.Data[frame.StackIndex - 3], (T5)frame.Data[frame.StackIndex - 2], (T6)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 6;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 8; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 8] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 8], (T1)frame.Data[frame.StackIndex - 7], (T2)frame.Data[frame.StackIndex - 6], (T3)frame.Data[frame.StackIndex - 5], (T4)frame.Data[frame.StackIndex - 4], (T5)frame.Data[frame.StackIndex - 3], (T6)frame.Data[frame.StackIndex - 2], (T7)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 7;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 9; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 9] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 9], (T1)frame.Data[frame.StackIndex - 8], (T2)frame.Data[frame.StackIndex - 7], (T3)frame.Data[frame.StackIndex - 6], (T4)frame.Data[frame.StackIndex - 5], (T5)frame.Data[frame.StackIndex - 4], (T6)frame.Data[frame.StackIndex - 3], (T7)frame.Data[frame.StackIndex - 2], (T8)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 8;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 10; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 10] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 10], (T1)frame.Data[frame.StackIndex - 9], (T2)frame.Data[frame.StackIndex - 8], (T3)frame.Data[frame.StackIndex - 7], (T4)frame.Data[frame.StackIndex - 6], (T5)frame.Data[frame.StackIndex - 5], (T6)frame.Data[frame.StackIndex - 4], (T7)frame.Data[frame.StackIndex - 3], (T8)frame.Data[frame.StackIndex - 2], (T9)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 9;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 11; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 11] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 11], (T1)frame.Data[frame.StackIndex - 10], (T2)frame.Data[frame.StackIndex - 9], (T3)frame.Data[frame.StackIndex - 8], (T4)frame.Data[frame.StackIndex - 7], (T5)frame.Data[frame.StackIndex - 6], (T6)frame.Data[frame.StackIndex - 5], (T7)frame.Data[frame.StackIndex - 4], (T8)frame.Data[frame.StackIndex - 3], (T9)frame.Data[frame.StackIndex - 2], (T10)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 10;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 12; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 12] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 12], (T1)frame.Data[frame.StackIndex - 11], (T2)frame.Data[frame.StackIndex - 10], (T3)frame.Data[frame.StackIndex - 9], (T4)frame.Data[frame.StackIndex - 8], (T5)frame.Data[frame.StackIndex - 7], (T6)frame.Data[frame.StackIndex - 6], (T7)frame.Data[frame.StackIndex - 5], (T8)frame.Data[frame.StackIndex - 4], (T9)frame.Data[frame.StackIndex - 3], (T10)frame.Data[frame.StackIndex - 2], (T11)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 11;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 13; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 13] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 13], (T1)frame.Data[frame.StackIndex - 12], (T2)frame.Data[frame.StackIndex - 11], (T3)frame.Data[frame.StackIndex - 10], (T4)frame.Data[frame.StackIndex - 9], (T5)frame.Data[frame.StackIndex - 8], (T6)frame.Data[frame.StackIndex - 7], (T7)frame.Data[frame.StackIndex - 6], (T8)frame.Data[frame.StackIndex - 5], (T9)frame.Data[frame.StackIndex - 4], (T10)frame.Data[frame.StackIndex - 3], (T11)frame.Data[frame.StackIndex - 2], (T12)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 12;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 14; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 14] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 14], (T1)frame.Data[frame.StackIndex - 13], (T2)frame.Data[frame.StackIndex - 12], (T3)frame.Data[frame.StackIndex - 11], (T4)frame.Data[frame.StackIndex - 10], (T5)frame.Data[frame.StackIndex - 9], (T6)frame.Data[frame.StackIndex - 8], (T7)frame.Data[frame.StackIndex - 7], (T8)frame.Data[frame.StackIndex - 6], (T9)frame.Data[frame.StackIndex - 5], (T10)frame.Data[frame.StackIndex - 4], (T11)frame.Data[frame.StackIndex - 3], (T12)frame.Data[frame.StackIndex - 2], (T13)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 13;
            return 1;
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
            _site = site;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 15; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex - 15] = _site.Target(_site, (T0)frame.Data[frame.StackIndex - 15], (T1)frame.Data[frame.StackIndex - 14], (T2)frame.Data[frame.StackIndex - 13], (T3)frame.Data[frame.StackIndex - 12], (T4)frame.Data[frame.StackIndex - 11], (T5)frame.Data[frame.StackIndex - 10], (T6)frame.Data[frame.StackIndex - 9], (T7)frame.Data[frame.StackIndex - 8], (T8)frame.Data[frame.StackIndex - 7], (T9)frame.Data[frame.StackIndex - 6], (T10)frame.Data[frame.StackIndex - 5], (T11)frame.Data[frame.StackIndex - 4], (T12)frame.Data[frame.StackIndex - 3], (T13)frame.Data[frame.StackIndex - 2], (T14)frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 14;
            return 1;
        }

        public override string ToString() {
            return "Dynamic(" + _site.Binder.ToString() + ")";
        }
    }


    // *** END GENERATED CODE ***

    #endregion


}
