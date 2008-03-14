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
using System.Diagnostics;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {
    class ModuleGlobalSlot : Slot {
        private Slot _wrapper;

        // The value is stored as a System.Object. 
        // This is consistent with the Variable type associated with the slot.
        // If _typeReal is a value-type, then the get/set accessors will box/unbox as needed.
        private readonly Type _typeReal;

        /// <summary>
        /// contsuctor
        /// </summary>
        /// <param name="typeReal">the actual type of the slot.  </param>
        /// <param name="builtinWrapper">the value to be stored as a global</param>
        public ModuleGlobalSlot(Type typeReal,Slot builtinWrapper) {
            Debug.Assert(builtinWrapper.Type == typeof(ModuleGlobalWrapper));
            if (builtinWrapper.Type != typeof(ModuleGlobalWrapper)) throw new ArgumentException("builtinWrapper " + builtinWrapper.GetType().FullName);

            _wrapper = builtinWrapper;
            _typeReal = typeReal;
        }

        // Unbox to the real type if needed.
        void Unbox(LambdaCompiler cg) {
            if (_typeReal.IsValueType) {
                cg.IL.EmitUnbox(_typeReal);
            }
        }

        // Box if needed.
        void Box(LambdaCompiler cg) {
            if (_typeReal.IsValueType) {
                cg.EmitBoxing(_typeReal);
            }
        }

        // Emit code to push the value on the stack. Push on the stack as the real type (not as a boxed object). 
        public override void EmitGet(LambdaCompiler cg) {
            _wrapper.EmitGet(cg);
            cg.EmitPropertyGet(typeof(ModuleGlobalWrapper), "CurrentValue");
            
            Unbox(cg);
        }

        // Emit code to set this slot.
        // Requires that the target value has already been pushed on the slot, and has type=_realType.        
        public override void EmitSet(LambdaCompiler cg) {
            // Caller is responsible for type-matching. The underlying storage here is always System.Object,
            // so box if needed.
            Box(cg);
            Slot val = cg.GetLocalTmp(typeof(object));
            val.EmitSet(cg);

            _wrapper.EmitGet(cg);
            val.EmitGet(cg);

            cg.EmitPropertySet(typeof(ModuleGlobalWrapper), "CurrentValue");

            cg.FreeLocalTmp(val);
        }

        // Emit code to set this slot to val.
        // Requires that val is assignable to type _realType.
        public override void EmitSet(LambdaCompiler cg, Slot val) {
            _wrapper.EmitGet(cg);
            val.EmitGet(cg);
            
            Box(cg);
            cg.EmitPropertySet(typeof(ModuleGlobalWrapper), "CurrentValue");
        }

        // Override this to ensure proper boxing.
        public override void EmitSetUninitialized(LambdaCompiler cg)
        {
            if (_typeReal.IsValueType) {
                Debug.Assert(false);
                throw new InvalidOperationException("Can't set a value-type to Uninitialized");
            }
            base.EmitSetUninitialized(cg);
        }

        // Push the value onto the stack as a System.Object. Box if needed.
        // This is used by optimized module generator for TryGet dict.
        public void EmitGetRawFromObject(LambdaCompiler cg) {
            _wrapper.EmitGet(cg);
            cg.EmitPropertyGet(typeof(ModuleGlobalWrapper), "RawValue");

            // Raw value is already boxed if it's a value-type, so don't need to box here.
        }

        // Emit assuming the boxed object is pushed on the stack.
        public void EmitSetRawFromObject(LambdaCompiler cg, Slot val) {
            _wrapper.EmitGet(cg);
            val.EmitGet(cg);
            cg.EmitPropertySet(typeof(ModuleGlobalWrapper), "CurrentValue");
        }

        public override void EmitCheck(LambdaCompiler cg, SymbolId name) {
            // checks are handled2 in the get_CurrentValue
        }

        // For passing as arg to:
        //    public static void InitializeModuleField(CodeContext context, SymbolId name, ref ModuleGlobalWrapper wrapper) {
        public void EmitWrapperAddr(LambdaCompiler cg) {
            _wrapper.EmitGetAddr(cg);
        }

        public void EmitWrapper(LambdaCompiler cg) {
            _wrapper.EmitGet(cg);
        }

        public override void EmitGetAddr(LambdaCompiler cg) {
            throw new NotSupportedException("Can't get address of module global.");
        }

        // Return the actual type. 
        // The underlying storage for a ModuleGlobalSlot is always a System.Object, but publicly, we 
        // want to appear as the type we were declared as, to keep us consistent with the ASTs and Variables.
        // This will box /unbox under the covers.
        public override Type Type {
            get { return _typeReal; }
        }
    }
}
