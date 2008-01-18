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

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Microsoft.Contracts;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    public class TypeGen {
        private readonly AssemblyGen _myAssembly;
        private readonly TypeBuilder _myType;
        private Slot _contextSlot;
        private ConstructorBuilder _initializer; // The .cctor() of the type
        private Compiler _initGen; // The IL generator for the .cctor()
        private Dictionary<object, Slot> _constants = new Dictionary<object, Slot>();
        private Dictionary<SymbolId, Slot> _indirectSymbolIds = new Dictionary<SymbolId, Slot>();
        private List<TypeGen> _nestedTypeGens = new List<TypeGen>();
        private ConstructorBuilder _defaultCtor;
        private ActionBinder _binder;

        private static readonly Type[] SymbolIdIntCtorSig = new Type[] { typeof(int) };

        public TypeGen(AssemblyGen myAssembly, TypeBuilder myType) {
            this._myAssembly = myAssembly;
            this._myType = myType;
        }

        [Confined]
        public override string/*!*/ ToString() {
            return _myType.ToString();
        }

        /// <summary>
        /// Gets the Compiler associated with the Type Initializer (cctor) creating it if necessary.
        /// </summary>
        public Compiler TypeInitializer {
            get {
                if (_initializer == null) {
                    _initializer = _myType.DefineTypeInitializer();
                    _initGen = CreateCodeGen(_initializer, _initializer.GetILGenerator(), ArrayUtils.EmptyTypes);
                }
                return _initGen;
            }
        }

        public Compiler CreateCodeGen(MethodBase mi, ILGenerator ilg, IList<Type> paramTypes) {
            return CreateCodeGen(mi, ilg, paramTypes, null);
        }

        internal Compiler CreateCodeGen(MethodBase mi, ILGenerator ilg, IList<Type> paramTypes, ConstantPool constantPool) {
            Compiler ret = new Compiler(this, _myAssembly, mi, ilg, paramTypes, constantPool);
            if (_binder != null) ret.Binder = _binder;
            if (_contextSlot != null) ret.ContextSlot = _contextSlot;
            return ret;
        }

        public Type FinishType() {
            if (_initGen != null) _initGen.Emit(OpCodes.Ret);

            Type ret = _myType.CreateType();
            foreach (TypeGen ntb in _nestedTypeGens) {
                ntb.FinishType();
            }
            //Console.WriteLine("finished: " + ret.FullName);
            return ret;
        }

        public ConstructorBuilder DefaultConstructor {
            get {
                return _defaultCtor;
            }
            set {
                _defaultCtor = value;
            }
        }

        public ActionBinder Binder {
            get {
                return _binder;
            }
            set {
                _binder = value;
            }
        }

        public void AddCodeContextField() {
            FieldBuilder contextField = _myType.DefineField(CodeContext.ContextFieldName,
                    typeof(CodeContext),
                    FieldAttributes.Public | FieldAttributes.Static);
            //contextField.SetCustomAttribute(new CustomAttributeBuilder(typeof(IronPython.Runtime.PythonHiddenFieldAttribute).GetConstructor(ArrayUtils.EmptyTypes), Runtime.Operations.ArrayUtils.EmptyObjects));
            _contextSlot = new StaticFieldSlot(contextField);
        }

        public Slot AddField(Type fieldType, string name) {
            FieldBuilder fb = _myType.DefineField(name, fieldType, FieldAttributes.Public);
            return new FieldSlot(new ThisSlot(_myType), fb);
        }
        public Slot AddStaticField(Type fieldType, string name) {
            FieldBuilder fb = _myType.DefineField(name, fieldType, FieldAttributes.Public | FieldAttributes.Static);
            return new StaticFieldSlot(fb);
        }

        public Slot AddStaticField(Type fieldType, FieldAttributes attributes, string name) {
            FieldBuilder fb = _myType.DefineField(name, fieldType, attributes | FieldAttributes.Static);
            return new StaticFieldSlot(fb);
        }

        public Compiler DefineExplicitInterfaceImplementation(MethodInfo baseMethod) {
            Contract.RequiresNotNull(baseMethod, "baseMethod");

            MethodAttributes attrs = baseMethod.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.Public);
            attrs |= MethodAttributes.NewSlot | MethodAttributes.Final;

            Type[] baseSignature = ReflectionUtils.GetParameterTypes(baseMethod.GetParameters());
            MethodBuilder mb = _myType.DefineMethod(
                baseMethod.DeclaringType.Name + "." + baseMethod.Name,
                attrs,
                baseMethod.ReturnType,
                baseSignature);
            Compiler ret = CreateCodeGen(mb, mb.GetILGenerator(), baseSignature);
            ret.MethodToOverride = baseMethod;
            return ret;
        }

        public PropertyBuilder DefineProperty(string name, PropertyAttributes attrs, Type returnType) {
            return _myType.DefineProperty(name, attrs, returnType, ArrayUtils.EmptyTypes);
        }

        private const MethodAttributes MethodAttributesToEraseInOveride =
            MethodAttributes.Abstract | MethodAttributes.ReservedMask;

        // TODO: Remove
        internal Compiler DefineMethodOverride(MethodInfo baseMethod) {
            MethodAttributes finalAttrs = baseMethod.Attributes & ~MethodAttributesToEraseInOveride;
            Type[] baseSignature = ReflectionUtils.GetParameterTypes(baseMethod.GetParameters());
            MethodBuilder mb = _myType.DefineMethod(baseMethod.Name, finalAttrs, baseMethod.ReturnType, baseSignature);
            Compiler ret = CreateCodeGen(mb, mb.GetILGenerator(), baseSignature);
            ret.MethodToOverride = baseMethod;
            return ret;
        }

        public ILGen DefineMethod(string name, Type returnType, Type[] parameterTypes, string[] parameterNames) {
            Contract.RequiresNotNull(name, "name");
            Contract.RequiresNotNull(returnType, "returnType");
            Contract.RequiresNotNullItems(parameterTypes, "parameterTypes");
            if (parameterNames != null) {
                Contract.Requires(parameterTypes.Length == parameterNames.Length, "parameterNames");
                Contract.RequiresNotNullItems(parameterNames, "parameterNames");
            }

            MethodBuilder mb = _myType.DefineMethod(name, CompilerHelpers.PublicStatic, returnType, parameterTypes);

            if (parameterNames != null) {
                for (int i = 0; i < parameterNames.Length; i++) {
                    ParameterBuilder pb = mb.DefineParameter(i + 1, ParameterAttributes.None, parameterNames[i]);
                }
            }
            return new ILGen(mb.GetILGenerator());
        }

        internal Compiler DefineMethod(string name, Type retType, IList<Type> paramTypes, IList<string> paramNames, ConstantPool constantPool) {
            Type[] parameterTypes = CompilerHelpers.MakeParamTypeArray(paramTypes, constantPool);

            MethodBuilder mb = _myType.DefineMethod(name, CompilerHelpers.PublicStatic, retType, parameterTypes);
            Compiler res = CreateCodeGen(mb, mb.GetILGenerator(), parameterTypes, constantPool);

            if (paramNames != null) {
                // parameters are index from 1, with constant pool we need to skip the first arg
                int offset = constantPool != null ? 2 : 1;
                for (int i = 0; i < paramNames.Count; i++) {
                    ParameterBuilder pb = res.DefineParameter(i + offset, ParameterAttributes.None, paramNames[i]);
                }
            }
            return res;
        }

        public Compiler DefineConstructor(Type[] paramTypes) {
            ConstructorBuilder cb = _myType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, paramTypes);
            return CreateCodeGen(cb, cb.GetILGenerator(), paramTypes);
        }

        public Compiler DefineStaticConstructor() {
            ConstructorBuilder cb = _myType.DefineTypeInitializer();
            return CreateCodeGen(cb, cb.GetILGenerator(), ArrayUtils.EmptyTypes);
        }

        public void SetCustomAttribute(Type type, object[] values) {
            Contract.RequiresNotNull(type, "type");

            Type[] types = new Type[values.Length];
            for (int i = 0; i < types.Length; i++) {
                if (values[i] != null) {
                    types[i] = values[i].GetType();
                } else {
                    types[i] = typeof(object);
                }
            }
            CustomAttributeBuilder cab = new CustomAttributeBuilder(type.GetConstructor(types), values);

            _myType.SetCustomAttribute(cab);
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

            TypeInitializer.EmitConstantNoCache(value);
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

        public void EmitIndirectedSymbol(Compiler cg, SymbolId id) {
            Slot value;
            if (!_indirectSymbolIds.TryGetValue(id, out value)) {
                // create field, emit fix-up...

                value = AddStaticField(typeof(int), FieldAttributes.Private, "symbol_" + SymbolTable.IdToString(id));
                Compiler init = TypeInitializer;
                Slot localTmp = init.GetLocalTmp(typeof(SymbolId));
                init.EmitString((string)SymbolTable.IdToString(id));
                init.EmitCall(typeof(SymbolTable), "StringToId");
                localTmp.EmitSet(init);
                localTmp.EmitGetAddr(init);
                init.EmitPropertyGet(typeof(SymbolId), "Id");
                value.EmitSet(init);

                init.FreeLocalTmp(localTmp);
                _indirectSymbolIds[id] = value;
            }

            value.EmitGet(cg);
            cg.EmitNew(typeof(SymbolId), SymbolIdIntCtorSig);
        }


        public AssemblyGen AssemblyGen {
            get { return _myAssembly; }
        }

        public TypeBuilder TypeBuilder {
            get { return _myType; }
        }
    }
}
