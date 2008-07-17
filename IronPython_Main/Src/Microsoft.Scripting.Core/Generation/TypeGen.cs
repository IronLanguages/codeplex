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

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using Microsoft.Contracts;

namespace System.Scripting.Generation {
    internal class TypeGen {
        private readonly AssemblyGen _myAssembly;
        private readonly TypeBuilder _myType;

        private ILGen _initGen;                        // The IL generator for the .cctor()
        private readonly Dictionary<SymbolId, FieldBuilder> _indirectSymbolIds = new Dictionary<SymbolId, FieldBuilder>();

        /// <summary>
        /// Gets the Compiler associated with the Type Initializer (cctor) creating it if necessary.
        /// </summary>
        internal ILGen TypeInitializer {
            get {
                if (_initGen == null) {
                    _initGen = new ILGen(_myType.DefineTypeInitializer().GetILGenerator(), this);
                }
                return _initGen;
            }
        }

        internal AssemblyGen AssemblyGen {
            get { return _myAssembly; }
        }

        internal TypeBuilder TypeBuilder {
            get { return _myType; }
        }

        internal TypeGen(AssemblyGen myAssembly, TypeBuilder myType) {
            Assert.NotNull(myAssembly, myType);

            _myAssembly = myAssembly;
            _myType = myType;
        }

        [Confined]
        public override string ToString() {
            return _myType.ToString();
        }

        internal Type FinishType() {
            if (_initGen != null) _initGen.Emit(OpCodes.Ret);

            Type ret = _myType.CreateType();

            //Console.WriteLine("finished: " + ret.FullName);
            return ret;
        }

        internal FieldBuilder AddStaticField(Type fieldType, string name) {
            return _myType.DefineField(name, fieldType, FieldAttributes.Public | FieldAttributes.Static);
        }

        internal FieldBuilder AddStaticField(Type fieldType, FieldAttributes attributes, string name) {
            return _myType.DefineField(name, fieldType, attributes | FieldAttributes.Static);
        }

        internal ILGen DefineExplicitInterfaceImplementation(MethodInfo baseMethod) {
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

        internal ILGen DefineMethodOverride(MethodInfo baseMethod) {
            MethodAttributes finalAttrs = baseMethod.Attributes & ~MethodAttributesToEraseInOveride;
            Type[] baseSignature = ReflectionUtils.GetParameterTypes(baseMethod.GetParameters());
            MethodBuilder mb = _myType.DefineMethod(baseMethod.Name, finalAttrs, baseMethod.ReturnType, baseSignature);

            TypeBuilder.DefineMethodOverride(mb, baseMethod);
            return new ILGen(mb.GetILGenerator(), this);
        }

        internal void EmitIndirectedSymbol(ILGen cg, SymbolId id) {
            if (id == SymbolId.Empty) {
                cg.EmitFieldGet(typeof(SymbolId).GetField("Empty"));
            } else {
                FieldBuilder value;
                if (!_indirectSymbolIds.TryGetValue(id, out value)) {
                    // create field, emit fix-up...

                    value = AddStaticField(typeof(SymbolId), FieldAttributes.Public, "symbol_" + SymbolTable.IdToString(id));
                    ILGen init = TypeInitializer;
                    if (_indirectSymbolIds.Count == 0) {
                        init.EmitType(_myType);
                        init.EmitCall(typeof(RuntimeHelpers), "InitializeSymbols");
                    }
                    _indirectSymbolIds[id] = value;
                }
                cg.Emit(OpCodes.Ldsfld, value);
            }
        }
    }
}
