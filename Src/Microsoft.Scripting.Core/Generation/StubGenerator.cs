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
using System.Collections.Generic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using System.Reflection.Emit;
using Microsoft.Scripting.Runtime;
using System.Reflection;

namespace Microsoft.Scripting.Generation {
    static class StubGenerator {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public enum CallType {
            None = 0,
            ArgumentList = 1,
            KeywordDictionary = 2,
        }

        /// <summary>
        /// Generates stub to receive the CLR call and then call the dynamic language code.
        /// </summary>
        public static void EmitClrCallStub(LambdaCompiler cg, Slot callTarget, CallType functionAttributes) {
            List<ReturnFixer> fixers = new List<ReturnFixer>(0);
            int argsCount = cg.GetLambdaArgumentSlotCount();

            CallAction action;
            if ((functionAttributes & CallType.ArgumentList) != 0) {
                ArgumentInfo[] infos = CompilerHelpers.MakeRepeatedArray(ArgumentInfo.Simple, argsCount);
                infos[argsCount - 1] = new ArgumentInfo(ArgumentKind.List);

                action = CallAction.Make(new CallSignature(infos));
            } else {
                action = CallAction.Make(argsCount);
            }

            // Create strongly typed return type from the site.
            // This will, among other things, generate tighter code.
            Type[] siteArguments = CompilerHelpers.MakeRepeatedArray(typeof(object), argsCount + 2);
            Type result = CompilerHelpers.GetReturnType(cg.Method);
            if (result != typeof(void)) {
                siteArguments[argsCount + 1] = result;
            }

            bool fast;
            Slot site = cg.CreateDynamicSite(action, siteArguments, out fast);
            Type siteType = site.Type;
            PropertyInfo target = siteType.GetProperty("Target");

            site.EmitGet(cg);
            cg.EmitPropertyGet(target);
            site.EmitGet(cg);

            if (!fast) {
                cg.EmitCodeContext();
            }

            if (DynamicSiteHelpers.IsBigTarget(target.PropertyType)) {
                cg.EmitTuple(
                    DynamicSiteHelpers.GetTupleTypeFromTarget(target.PropertyType),
                    argsCount + 1,
                    delegate(int index) {
                        if (index == 0) {
                            callTarget.EmitGet(cg);
                        } else {
                            ReturnFixer rf = ReturnFixer.EmitArgument(cg, cg.GetLambdaArgumentSlot(index - 1));
                            if (rf != null) fixers.Add(rf);
                        }
                    }
                );
            } else {
                callTarget.EmitGet(cg);

                for (int i = 0; i < argsCount; i++) {
                    ReturnFixer rf = ReturnFixer.EmitArgument(cg, cg.GetLambdaArgumentSlot(i));
                    if (rf != null) fixers.Add(rf);
                }
            }

            cg.EmitCall(target.PropertyType, "Invoke");

            foreach (ReturnFixer rf in fixers) {
                rf.FixReturn(cg);
            }

            if (result == typeof(void)) {
                cg.Emit(OpCodes.Pop);
            }
            cg.Emit(OpCodes.Ret);
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
