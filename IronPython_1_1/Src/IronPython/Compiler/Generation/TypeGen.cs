/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;

using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using System.Reflection.Emit;

using System.Security.Permissions;

using IronPython.Runtime;
using IronMath;

namespace IronPython.Compiler.Generation {
    class TypeGen {
        public readonly AssemblyGen myAssembly;
        public readonly TypeBuilder myType;

        public Slot moduleSlot;

        private ConstructorBuilder initializer; // The .cctor() of the type
        private CodeGen initGen; // The IL generator for the .cctor()
        private Dictionary<object, Slot> constants = new Dictionary<object, Slot>();
        private Dictionary<SymbolId, Slot> indirectSymbolIds = new Dictionary<SymbolId, Slot>();
        private List<TypeGen> nestedTypeGens = new List<TypeGen>();

        internal ConstructorBuilder DefaultConstructor;

        public TypeGen(AssemblyGen myAssembly, TypeBuilder myType) {
            this.myAssembly = myAssembly;
            this.myType = myType;
        }

        public override string ToString() {
            return myType.ToString();
        }
        public CodeGen GetOrMakeInitializer() {
            if (initializer == null) {
                initializer = myType.DefineTypeInitializer();
                initGen = new CodeGen(this, initializer, initializer.GetILGenerator(), Type.EmptyTypes);
            }
            return initGen;
        }

        public Type FinishType() {
            if (initGen != null) initGen.Emit(OpCodes.Ret);

            Type ret = myType.CreateType();
            foreach (TypeGen ntb in nestedTypeGens) {
                ntb.FinishType();
            }
            //Console.WriteLine("finished: " + ret.FullName);
            return ret;
        }

        public TypeGen DefineNestedType(string name, Type parent) {
            TypeBuilder tb = myType.DefineNestedType(name, TypeAttributes.NestedPublic);
            tb.SetParent(parent);
            TypeGen ret = new TypeGen(myAssembly, tb);
            nestedTypeGens.Add(ret);

            ret.AddModuleField(typeof(PythonModule));

            return ret;
        }

        public void AddModuleField(Type moduleType) {
            FieldBuilder moduleField = this.myType.DefineField(CompiledModule.ModuleFieldName,
                moduleType, FieldAttributes.Public | FieldAttributes.Static);
            moduleField.SetCustomAttribute(new CustomAttributeBuilder(typeof(PythonHiddenFieldAttribute).GetConstructor(new Type[0]), Runtime.Operations.Ops.EMPTY));
            this.moduleSlot = new StaticFieldSlot(moduleField);
        }

        public Slot AddField(Type fieldType, string name) {
            FieldBuilder fb = myType.DefineField(name, fieldType, FieldAttributes.Public);
            return new FieldSlot(new ThisSlot(myType), fb);
        }
        public Slot AddStaticField(Type fieldType, string name) {
            FieldBuilder fb = myType.DefineField(name, fieldType, FieldAttributes.Public | FieldAttributes.Static);
            return new StaticFieldSlot(fb);
        }

        public Slot AddStaticField(Type fieldType, FieldAttributes attributes, string name) {
            FieldBuilder fb = myType.DefineField(name, fieldType, attributes | FieldAttributes.Static);
            return new StaticFieldSlot(fb);
        }

        public CodeGen DefineExplicitInterfaceImplementation(MethodInfo baseMethod) {
            MethodAttributes attrs = baseMethod.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.Public);
            attrs |= MethodAttributes.NewSlot | MethodAttributes.Final;

            MethodBuilder mb = myType.DefineMethod(
                baseMethod.DeclaringType.Name + "." + baseMethod.Name,
                attrs,
                baseMethod.ReturnType,
                CompilerHelpers.GetTypes(baseMethod.GetParameters()));
            CodeGen ret = new CodeGen(this, mb, mb.GetILGenerator(), baseMethod.GetParameters());
            ret.methodToOverride = baseMethod;
            return ret;
        }

        public PropertyBuilder DefineProperty(string name, PropertyAttributes attrs, Type returnType) {
            return myType.DefineProperty(name, attrs, returnType, new Type[0]);
        }

        private const MethodAttributes MethodAttributesToEraseInOveride =
            MethodAttributes.Abstract | MethodAttributes.ReservedMask;

        public CodeGen DefineMethodOverride(MethodAttributes extraAttrs, MethodInfo baseMethod) {
            MethodAttributes finalAttrs = (baseMethod.Attributes & ~MethodAttributesToEraseInOveride) | extraAttrs;
            MethodBuilder mb = myType.DefineMethod(baseMethod.Name, finalAttrs, baseMethod.ReturnType,
                CompilerHelpers.GetTypes(baseMethod.GetParameters()));
            CodeGen ret = new CodeGen(this, mb, mb.GetILGenerator(), baseMethod.GetParameters());
            ret.methodToOverride = baseMethod;
            return ret;
        }

        public CodeGen DefineMethodOverride(MethodInfo baseMethod) {
            return DefineMethodOverride((MethodAttributes)0, baseMethod);
        }

        public CodeGen DefineMethod(string name, Type retType, Type[] paramTypes, string[] paramNames) {
            return DefineMethod(CompilerHelpers.PublicStatic, name, retType, paramTypes, paramNames);
        }

        public CodeGen DefineMethod(string name, Type retType, Type[] paramTypes, string[] paramNames, object[] defaultVals) {
            return DefineMethod(CompilerHelpers.PublicStatic, name, retType, paramTypes, paramNames, defaultVals);
        }

        public CodeGen DefineMethod(MethodAttributes attrs, string name, Type retType, Type[] paramTypes, string[] paramNames, object[] defaultVals) {
            return DefineMethod(attrs, name, retType, paramTypes, paramNames, defaultVals, null);
        }

        public CodeGen DefineMethod(string name, Type retType, Type[] paramTypes, string[] paramNames, object[] defaultVals, CustomAttributeBuilder[] cabs) {
            return DefineMethod(CompilerHelpers.PublicStatic, name, retType, paramTypes, paramNames, defaultVals, cabs);
        }

        public CodeGen DefineMethod(MethodAttributes attrs, string name, Type retType, Type[] paramTypes, string[] paramNames, object[] defaultVals, CustomAttributeBuilder[] cabs) {
            MethodBuilder mb = myType.DefineMethod(name, attrs, retType, paramTypes);
            CodeGen res = new CodeGen(this, mb, mb.GetILGenerator(), paramTypes);

            for (int i = 0; i < paramTypes.Length; i++) {
                // parameters are index from 1.
                ParameterBuilder pb = res.DefineParameter(i + 1, ParameterAttributes.None, paramNames[i]);
                if (defaultVals != null && i < defaultVals.Length && defaultVals[i] != DBNull.Value) {
                    pb.SetConstant(defaultVals[i]);
                }

                if (cabs != null && i < cabs.Length && cabs[i] != null) {
                    pb.SetCustomAttribute(cabs[i]);
                }
            }
            return res;
        }

        public CodeGen DefineMethod(MethodAttributes attrs, string name, Type retType, Type[] paramTypes, string[] paramNames) {
            return DefineMethod(attrs, name, retType, paramTypes, paramNames, null);
        }

        public CodeGen DefineUserHiddenMethod(MethodAttributes attrs, string name, Type retType, Type[] paramTypes) {
            MethodBuilder mb = myType.DefineMethod(name, attrs, retType, paramTypes);
            CodeGen res = new CodeGen(this, mb, mb.GetILGenerator(), paramTypes);
            return res;
        }

        public CodeGen DefineConstructor(Type[] paramTypes) {
            ConstructorBuilder cb = myType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, paramTypes);
            return new CodeGen(this, cb, cb.GetILGenerator(), paramTypes);
        }

        public CodeGen DefineStaticConstructor() {
            ConstructorBuilder cb = myType.DefineTypeInitializer();
            return new CodeGen(this, cb, cb.GetILGenerator(), new ParameterInfo[0]);
        }

        public void SetCustomAttribute(Type t, object[] values) {

            Type[] types = new Type[values.Length];
            for (int i = 0; i < types.Length; i++) {
                if (values[i] != null) {
                    types[i] = values[i].GetType();
                } else {
                    types[i] = typeof(object);
                }
            }
            CustomAttributeBuilder cab = new CustomAttributeBuilder(t.GetConstructor(types), values);

            myType.SetCustomAttribute(cab);
        }

        /// <summary>
        /// Constants
        /// </summary>

        public Slot GetOrMakeConstant(object value) {
            return GetOrMakeConstant(value, typeof(object));
        }

        public Slot GetOrMakeConstant(object value, Type type) {
            Slot ret;
            if (constants.TryGetValue(value, out ret)) return ret;

            // Create a name like "c$3.141592$712"
            string symbolicName = value.ToString();
            if (symbolicName.Length > 20)
                symbolicName = symbolicName.Substring(0, 20);
            string name = "c$" + symbolicName + "$" + constants.Count;

            FieldBuilder fb = myType.DefineField(name, type, FieldAttributes.Static | FieldAttributes.InitOnly);
            ret = new StaticFieldSlot(fb);

            GetOrMakeInitializer().EmitConstantBoxed(value);
            initGen.EmitFieldSet(fb);

            constants[value] = ret;
            return ret;
        }

        public void EmitIndirectedSymbol(CodeGen cg, SymbolId id) {
            Slot value;
            if (!indirectSymbolIds.TryGetValue(id, out value)) {
                // create field, emit fix-up...

                value = AddStaticField(typeof(int), FieldAttributes.Private, "symbol_" + SymbolTable.IdToString(id));
                CodeGen init = GetOrMakeInitializer();
                Slot localTmp = init.GetLocalTmp(typeof(SymbolId));
                init.EmitString((string)SymbolTable.IdToString(id));
                init.EmitCall(typeof(SymbolTable), "StringToId");
                localTmp.EmitSet(init);
                localTmp.EmitGetAddr(init);
                init.EmitFieldGet(typeof(SymbolId), "Id");
                value.EmitSet(init);

                cg.FreeLocalTmp(localTmp);
                indirectSymbolIds[id] = value;
            }

            value.EmitGet(cg);
        }
    }
}
