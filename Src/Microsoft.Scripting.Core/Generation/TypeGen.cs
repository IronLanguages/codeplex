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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.Scripting.Utils;
using Microsoft.Contracts;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {
    internal class TypeGen {
        private readonly AssemblyGen/*!*/ _myAssembly;
        private readonly TypeBuilder/*!*/ _myType;

        private FieldBuilder _contextField;
        private ILGen _initGen;                        // The IL generator for the .cctor()
        private readonly Dictionary<object, Slot>/*!*/ _constants = new Dictionary<object, Slot>();
        private readonly Dictionary<SymbolId, Slot>/*!*/ _indirectSymbolIds = new Dictionary<SymbolId, Slot>();

        /// <summary>
        /// Gets the Compiler associated with the Type Initializer (cctor) creating it if necessary.
        /// </summary>
        internal ILGen/*!*/ TypeInitializer {
            get {
                if (_initGen == null) {
                    _initGen = new ILGen(_myType.DefineTypeInitializer().GetILGenerator(), this);
                }
                return _initGen;
            }
        }

        internal AssemblyGen/*!*/ AssemblyGen {
            get { return _myAssembly; }
        }

        internal TypeBuilder/*!*/ TypeBuilder {
            get { return _myType; }
        }

        internal FieldBuilder ContextField {
            get { return _contextField; }
        }

        internal TypeGen(AssemblyGen/*!*/ myAssembly, TypeBuilder/*!*/ myType) {
            Assert.NotNull(myAssembly, myType);

            _myAssembly = myAssembly;
            _myType = myType;
        }

        [Confined]
        public override string/*!*/ ToString() {
            return _myType.ToString();
        }

        internal Type/*!*/ FinishType() {
            if (_initGen != null) _initGen.Emit(OpCodes.Ret);

            Type ret = _myType.CreateType();

            //Console.WriteLine("finished: " + ret.FullName);
            return ret;
        }


        internal void AddCodeContextField() {
            _contextField = _myType.DefineField(CodeContext.ContextFieldName,
                typeof(CodeContext),
                FieldAttributes.Public | FieldAttributes.Static
            );
        }

        internal Slot AddStaticField(Type fieldType, string name) {
            FieldBuilder fb = _myType.DefineField(name, fieldType, FieldAttributes.Public | FieldAttributes.Static);
            return new StaticFieldSlot(fb);
        }

        internal Slot AddStaticField(Type fieldType, FieldAttributes attributes, string name) {
            FieldBuilder fb = _myType.DefineField(name, fieldType, attributes | FieldAttributes.Static);
            return new StaticFieldSlot(fb);
        }

        internal ILGen DefineExplicitInterfaceImplementation(MethodInfo/*!*/ baseMethod) {
            ContractUtils.RequiresNotNull(baseMethod, "baseMethod");

            MethodAttributes attrs = baseMethod.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.Public);
            attrs |= MethodAttributes.NewSlot | MethodAttributes.Final;

            Type[] baseSignature = ReflectionUtils.GetParameterTypes(baseMethod.GetParameters());
            MethodBuilder mb = _myType.DefineMethod(
                baseMethod.DeclaringType.Name + "." + baseMethod.Name,
                attrs,
                baseMethod.ReturnType,
                baseSignature);

            TypeBuilder.DefineMethodOverride(mb, baseMethod);
            return new ILGen(mb.GetILGenerator(), this);
        }

        private const MethodAttributes MethodAttributesToEraseInOveride =
            MethodAttributes.Abstract | MethodAttributes.ReservedMask;

        internal ILGen/*!*/ DefineMethodOverride(MethodInfo baseMethod) {
            MethodAttributes finalAttrs = baseMethod.Attributes & ~MethodAttributesToEraseInOveride;
            Type[] baseSignature = ReflectionUtils.GetParameterTypes(baseMethod.GetParameters());
            MethodBuilder mb = _myType.DefineMethod(baseMethod.Name, finalAttrs, baseMethod.ReturnType, baseSignature);

            TypeBuilder.DefineMethodOverride(mb, baseMethod);
            return new ILGen(mb.GetILGenerator(), this);
        }

        /// <summary>
        /// Constants
        /// </summary>
        internal Slot GetOrMakeConstant(object value) {
            Debug.Assert(!(value is CompilerConstant));

            Slot ret;
            if (_constants.TryGetValue(value, out ret)) {
                return ret;
            }

            Type type = value.GetType();

            // Create a name like "c$3.141592$712"
            string name = value.ToString();
            if (name.Length > 20) {
                name = name.Substring(0, 20);
            }
            name = "c$" + name + "$" + _constants.Count;

            FieldBuilder fb = _myType.DefineField(name, type, FieldAttributes.Static | FieldAttributes.InitOnly);
            ret = new StaticFieldSlot(fb);

            TypeInitializer.EmitConstant(value);
            _initGen.EmitFieldSet(fb);

            _constants[value] = ret;
            return ret;
        }

        internal Slot GetOrMakeCompilerConstant(CompilerConstant value) {
            Slot ret;
            if (_constants.TryGetValue(value, out ret)) {
                return ret;
            }

            string name = "c$" + value.Name + "$" + _constants.Count;

            FieldBuilder fb = _myType.DefineField(name, value.Type, FieldAttributes.Static | FieldAttributes.InitOnly);
            ret = new StaticFieldSlot(fb);

            value.EmitCreation(TypeInitializer);
            _initGen.EmitFieldSet(fb);

            _constants[value] = ret;
            return ret;
        }

        internal void EmitIndirectedSymbol(ILGen cg, SymbolId id) {
            Slot value;
            if (!_indirectSymbolIds.TryGetValue(id, out value)) {
                // create field, emit fix-up...

                value = AddStaticField(typeof(SymbolId), FieldAttributes.Private, "symbol_" + SymbolTable.IdToString(id));
                ILGen init = TypeInitializer;
                init.EmitString((string)SymbolTable.IdToString(id));
                init.EmitCall(typeof(SymbolTable), "StringToId");
                value.EmitSet(init);
                _indirectSymbolIds[id] = value;
            }
            value.EmitGet(cg);       
        }
    }
}
