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
using System.Reflection.Emit;

namespace Microsoft.Scripting.Generation {
    public static class StubGenerator {

        public enum CallType {
            None = 0,
            ArgumentList = 1,
            KeywordDictionary = 2,
        }

        /// <summary>
        /// Generates stub to receive the CLR call and then call the dynamic language code.
        /// </summary>
        public static void  EmitClrCallStub(CodeGen cg, Slot callTarget, int firstArg, CallType functionAttributes, Action<Exception> handler) {
            if (handler != null) {
                if (cg.ConstantPool == null) {
                    throw new InvalidOperationException("calling stubs with exception handlers requires constant pool");
                }
                cg.BeginExceptionBlock();
            }

            cg.EmitCodeContext();
            callTarget.EmitGet(cg);

            // TODO: use dynamic call sites here
            
            List<ReturnFixer> fixers = new List<ReturnFixer>(0);
            IList<Slot> args = cg.ArgumentSlots;
            int nargs = args.Count - firstArg;
            if (nargs <= CallTargets.MaximumCallArgs && (functionAttributes & CallType.ArgumentList) == 0) {
                for (int i = firstArg; i < args.Count; i++) {
                    ReturnFixer rf = ReturnFixer.EmitArgument(cg, args[i]);
                    if (rf != null) fixers.Add(rf);
                }
                cg.EmitCall(typeof(RuntimeHelpers), "CallWithContext", CreateSignatureWithContext(nargs + 1));
            } else if ((functionAttributes & CallType.ArgumentList) == 0) {
                cg.EmitArray(typeof(object), nargs, delegate(int index) {
                    ReturnFixer rf = ReturnFixer.EmitArgument(cg, args[index + firstArg]);
                    if (rf != null) fixers.Add(rf);
                });
                cg.EmitCall(typeof(RuntimeHelpers), "CallWithContext", new Type[] { typeof(CodeContext), typeof(object), typeof(object[]) });
            } else {
                cg.EmitArray(typeof(object), nargs - 1, delegate(int index) {
                    ReturnFixer rf = ReturnFixer.EmitArgument(cg, args[index + firstArg]);
                    if (rf != null) fixers.Add(rf);
                });

                args[args.Count - 1].EmitGet(cg);
                cg.EmitCall(typeof(RuntimeHelpers), "CallWithArgsTuple");
            }

            if (handler != null) {
                Label ret = cg.DefineLabel();
                Slot local = cg.GetLocalTmp(typeof(object));
                local.EmitSet(cg);
                cg.Emit(OpCodes.Leave_S, ret);
                cg.BeginCatchBlock(typeof(Exception));

                Slot exSlot = cg.GetLocalTmp(typeof(Exception));
                exSlot.EmitSet(cg);

                Slot handlerSlot = cg.ConstantPool.AddData(handler);
                handlerSlot.EmitGet(cg);

                exSlot.EmitGet(cg);
                cg.EmitCall(typeof(Action<Exception>).GetMethod("Invoke"));

                cg.EmitNull();
                local.EmitSet(cg);
                cg.Emit(OpCodes.Leave_S, ret);
                cg.EndExceptionBlock();
                cg.MarkLabel(ret);
                local.EmitGet(cg);
            }

            foreach (ReturnFixer rf in fixers) {
                rf.FixReturn(cg);
            }
            cg.EmitReturnFromObject();
        }

        private static Type[] CreateSignatureWithContext(int count) {
            Type[] array = new Type[count + 1];
            while (count > 0) {
                array[count--] = typeof(object);
            }
            array[0] = typeof(CodeContext);
            return array;
        }
    }
}
