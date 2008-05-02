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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Used as the key for the RuntimeHelpers.GetDelegate method caching system
    /// </summary>
    class DelegateSignatureInfo {
        private readonly ActionBinder _binder;
        private readonly Type _returnType;
        private readonly ParameterInfo[] _parameters;

#if DEBUG
        internal static readonly object TargetPlaceHolder = new object();
#endif

        public DelegateSignatureInfo(ActionBinder binder, Type returnType, ParameterInfo[] parameters) {
            _binder = binder;
            _parameters = parameters;
            _returnType = returnType;
        }

        [Confined]
        public override bool Equals(object obj) {
            DelegateSignatureInfo dsi = obj as DelegateSignatureInfo;
            if (dsi == null) {
                return false;
            }

            if (dsi._binder != _binder) {
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

            hashCode ^= _binder.GetHashCode();
            for (int i = 0; i < _parameters.Length; i++) {
                hashCode ^= _parameters[i].GetHashCode();
            }
            hashCode ^= _returnType.GetHashCode();
            return hashCode;
        }

        [Confined]
        public override string/*!*/ ToString() {
            StringBuilder text = new StringBuilder();
            text.Append(_returnType.ToString());
            text.Append("(");
            for (int i = 0; i < _parameters.Length; i++) {
                if (i != 0) text.Append(", ");
                text.Append(_parameters[i].ParameterType.Name);
            }
            text.Append("), using ");
            text.Append(_binder.GetType().Name);
            return text.ToString();
        }

        public DelegateInfo GenerateDelegateStub() {
            PerfTrack.NoteEvent(PerfTrack.Categories.DelegateCreate, ToString());

            Type[] delegateParams = new Type[_parameters.Length];
            for (int i = 0; i < _parameters.Length; i++) {
                delegateParams[i] = _parameters[i].ParameterType;
            }

            // Create the method
            LambdaCompiler cg = LambdaCompiler.CreateDynamicLambdaCompiler(null /*LambdaExpression*/, ToString(), _returnType, delegateParams, null);

            // Add the space for the delegate target and save the index at which it was placed,
            // most likely zero.
            int targetIndex = cg.ConstantPool.Count;
#if DEBUG
            Slot target = cg.ConstantPool.AddData(TargetPlaceHolder);
#else
            Slot target = cg.ConstantPool.AddData(null);
#endif

            // Add the CodeContext into the constant pool
            Slot context = cg.ConstantPool.AddData(_binder.Context);
            Debug.Assert(typeof(CodeContext).IsAssignableFrom(context.Type));
            cg.ContextSlot = context;

            // Emit the stub
            EmitClrCallStub(cg, target);

            // Finish the method
            MethodInfo method = cg.CreateDelegateMethodInfo();

            // Save the constants in the delegate info class
            return new DelegateInfo(method, cg.ConstantPool.Data, targetIndex);
        }

        /// <summary>
        /// Generates stub to receive the CLR call and then call the dynamic language code.
        /// </summary>
        private void EmitClrCallStub(LambdaCompiler cg, Slot callTarget) {
            List<ReturnFixer> fixers = new List<ReturnFixer>(0);
            int argsCount = cg.GetLambdaArgumentSlotCount();

            CallAction action = CallAction.Make(_binder, argsCount);

            // Create strongly typed return type from the site.
            // This will, among other things, generate tighter code.
            Type[] siteArguments = CompilerHelpers.MakeRepeatedArray(typeof(object), argsCount + 2);
            Type result = CompilerHelpers.GetReturnType(cg.Method);
            if (result != typeof(void)) {
                siteArguments[argsCount + 1] = result;
            }

            Slot site = cg.CreateDynamicSite(action, siteArguments);
            Type siteType = site.Type;
            PropertyInfo target = siteType.GetProperty("Target");

            site.EmitGet(cg.IL);
            cg.IL.EmitPropertyGet(target);
            site.EmitGet(cg.IL);

            // Emit code context 
            cg.EmitCodeContext();

            if (DynamicSiteHelpers.IsBigTarget(target.PropertyType)) {
                cg.EmitTuple(
                    DynamicSiteHelpers.GetTupleTypeFromTarget(target.PropertyType),
                    argsCount + 1,
                    delegate(int index) {
                        if (index == 0) {
                            callTarget.EmitGet(cg.IL);
                        } else {
                            ReturnFixer rf = ReturnFixer.EmitArgument(cg.IL, cg.GetLambdaArgumentSlot(index - 1));
                            if (rf != null) fixers.Add(rf);
                        }
                    }
                );
            } else {
                callTarget.EmitGet(cg.IL);

                for (int i = 0; i < argsCount; i++) {
                    ReturnFixer rf = ReturnFixer.EmitArgument(cg.IL, cg.GetLambdaArgumentSlot(i));
                    if (rf != null) fixers.Add(rf);
                }
            }

            cg.IL.EmitCall(target.PropertyType, "Invoke");

            foreach (ReturnFixer rf in fixers) {
                rf.FixReturn(cg.IL);
            }

            if (result == typeof(void)) {
                cg.IL.Emit(OpCodes.Pop);
            }
            cg.IL.Emit(OpCodes.Ret);
        }
    }

    /// <summary>
    /// Used as the value for the RuntimeHelpers.GetDelegate method caching system
    /// </summary>
    class DelegateInfo {
        private readonly MethodInfo _method;
        private readonly object[] _constants;
        private readonly int _target;

        internal DelegateInfo(MethodInfo method, object[] constants, int target) {
            Assert.NotNull(method, constants);

            _method = method;
            _constants = constants;
            _target = target;
        }

        public Delegate CreateDelegate(Type delegateType, object target) {
            Assert.NotNull(delegateType, target);

            object[] clone = (object[])_constants.Clone();
            Closure closure = new Closure(null, clone);
#if DEBUG
            Debug.Assert(clone[_target] == DelegateSignatureInfo.TargetPlaceHolder);
#endif
            clone[_target] = target;
            return ReflectionUtils.CreateDelegate(_method, delegateType, closure);
        }
    }
}
