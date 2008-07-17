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
using System.Reflection;
using System.Reflection.Emit;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using System.Text;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Used as the key for the RuntimeHelpers.GetDelegate method caching system
    /// </summary>
    internal sealed class DelegateSignatureInfo {
        private readonly CodeContext _context;
        private readonly Type _returnType;
        private readonly ParameterInfo[] _parameters;

        internal static readonly object TargetPlaceHolder = new object();

        internal DelegateSignatureInfo(CodeContext context, Type returnType, ParameterInfo[] parameters) {
            _context = context;
            _parameters = parameters;
            _returnType = returnType;
        }

        [Confined]
        public override bool Equals(object obj) {
            DelegateSignatureInfo dsi = obj as DelegateSignatureInfo;
            if (dsi == null) {
                return false;
            }

            if (dsi._parameters.Length != _parameters.Length) {
                return false;
            }

            if (_returnType != dsi._returnType) {
                return false;
            }

            for (int i = 0; i < _parameters.Length; i++) {
                if (dsi._parameters[i] != _parameters[i]) {
                    return false;
                }
            }

            return true;
        }

        [Confined]
        public override int GetHashCode() {
            int hashCode = 5331;

            for (int i = 0; i < _parameters.Length; i++) {
                hashCode ^= _parameters[i].GetHashCode();
            }
            hashCode ^= _returnType.GetHashCode();
            return hashCode;
        }

        [Confined]
        public override string ToString() {
            StringBuilder text = new StringBuilder();
            text.Append(_returnType.ToString());
            text.Append("(");
            for (int i = 0; i < _parameters.Length; i++) {
                if (i != 0) text.Append(", ");
                text.Append(_parameters[i].ParameterType.Name);
            }
            text.Append(")");
            return text.ToString();
        }

        internal DelegateInfo GenerateDelegateStub() {
            PerfTrack.NoteEvent(PerfTrack.Categories.DelegateCreate, ToString());

            Type[] delegateParams = new Type[_parameters.Length];
            for (int i = 0; i < _parameters.Length; i++) {
                delegateParams[i] = _parameters[i].ParameterType;
            }

            // Create the method
            DynamicILGen cg = Snippets.Shared.CreateDynamicMethod(ToString(), _returnType, ArrayUtils.Insert(typeof(object[]), delegateParams), false);

            // Emit the stub
            object[] constants = EmitClrCallStub(cg);

            // Save the constants in the delegate info class
            return new DelegateInfo(cg.Finish(), constants);
        }

        /// <summary>
        /// Generates stub to receive the CLR call and then call the dynamic language code.
        /// </summary>
        private object[] EmitClrCallStub(ILGen cg) {

            List<ReturnFixer> fixers = new List<ReturnFixer>(0);
            DelegateCallBinder action = new DelegateCallBinder(_parameters.Length);

            // Create strongly typed return type from the site.
            // This will, among other things, generate tighter code.
            Type[] siteTypes = MakeSiteSignature(_parameters.Length + 2);
            if (_returnType != typeof(void)) {
                siteTypes[siteTypes.Length - 1] = _returnType;
            }

            Type siteType = DynamicSiteHelpers.MakeDynamicSiteType(siteTypes);
            CallSite callSite = DynamicSiteHelpers.MakeSite(action, siteType);

            // build up constants array
            object[] constants = new object[] { TargetPlaceHolder, callSite, _context };
            int TargetIndex = 0, CallSiteIndex = 1, ContextIndex = 2;

            LocalBuilder site = cg.DeclareLocal(siteType);
            EmitConstantGet(cg, CallSiteIndex, siteType);
            cg.Emit(OpCodes.Dup);
            cg.Emit(OpCodes.Stloc, site);

            FieldInfo target = siteType.GetField("Target");
            cg.EmitFieldGet(target);
            cg.Emit(OpCodes.Ldloc, site);

            EmitConstantGet(cg, ContextIndex, typeof(CodeContext));
            EmitConstantGet(cg, TargetIndex, typeof(object));

            for (int i = 0; i < _parameters.Length; i++) {
                ReturnFixer rf = ReturnFixer.EmitArgument(cg, i + 1, _parameters[i].ParameterType);
                if (rf != null) fixers.Add(rf);
            }

            cg.EmitCall(target.FieldType, "Invoke");

            foreach (ReturnFixer rf in fixers) {
                rf.FixReturn(cg);
            }

            if (_returnType == typeof(void)) {
                cg.Emit(OpCodes.Pop);
            }
            cg.Emit(OpCodes.Ret);

            return constants;
        }

        private static void EmitConstantGet(ILGen il, int index, Type type) {
            il.Emit(OpCodes.Ldarg_0);
            il.EmitInt(index);
            il.Emit(OpCodes.Ldelem_Ref);
            if (type != typeof(object)) {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        private static Type[] MakeSiteSignature(int nargs) {
            Type[] sig = new Type[nargs + 1];
            sig[0] = typeof(CodeContext);
            for (int i = 1; i < sig.Length; i++) {
                sig[i] = typeof(object);
            }
            return sig;
        }
    }
}
