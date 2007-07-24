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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Threading;

using System.Resources;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;

using System.Reflection;
using System.Reflection.Emit;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;

namespace IronPython.Compiler.Generation {
    /// <summary>
    /// Python class hierarchy is represented using the __class__ field in the object. It does not 
    /// use the CLI type system for pure Python types. However, Python types which inherit from a 
    /// CLI type, or from a builtin Python type which is implemented in the engine by a CLI type,
    /// do have to use the CLI type system to interoperate with the CLI world. This means that 
    /// objects of different Python types, but with the same CLI base type, can use the same CLI type - 
    /// they will just have different values for the __class__ field.
    /// 
    /// The easiest way to inspect the functionality implemented by NewTypeMaker is to persist the
    /// generated IL using "ipy.exe -X:SaveAssemblies", and then inspect the
    /// persisted IL using ildasm.
    /// </summary>
    class NewTypeMaker {
        public const string VtableNamesField = "#VTableNames#";
        public const string TypePrefix = "IronPython.NewTypes.";
        private static Publisher<NewTypeInfo, Type> _newTypes = new Publisher<NewTypeInfo, Type>();
        private static int _typeCount;

        protected Type _baseType;
        protected IList<string> _slots;
        protected TypeGen _tg;
        protected Slot _typeField, _dictField, _weakrefField, _slotsField;
        protected Type _tupleType;
        protected IEnumerable<Type> _interfaceTypes;
        protected Tuple _baseClasses;

        private bool _hasBaseTypeField;

        private Dictionary<string, VTableEntry> _vtable = new Dictionary<string, VTableEntry>();

        public static Type GetNewType(string typeName, Tuple bases, IAttributesCollection dict) {
            if (bases == null) bases = Tuple.MakeTuple();
            // we're really only interested in the "correct" base type pulled out of bases
            // and any slot information contained in dict
            // other info might be used for future optimizations

            NewTypeInfo typeInfo = GetTypeInfo(typeName, bases, GetSlots(dict));

            if (typeInfo.BaseType.IsSealed || typeInfo.BaseType.IsValueType)
                throw PythonOps.TypeError("cannot derive from sealed or value types");


            Type ret = _newTypes.GetOrCreateValue(typeInfo,
                delegate() {
                    // creation code                    
                    return GetTypeMaker(bases, typeInfo).CreateNewType();
                });

            if (typeInfo.Slots != null) {
                Type tupleType = NewTuple.MakeTupleType(CompilerHelpers.MakeRepeatedArray(typeof(object), typeInfo.Slots.Count));
                ret = ret.MakeGenericType(tupleType);

                for (int i = 0; i < typeInfo.Slots.Count; i++) {                    
                    string name = typeInfo.Slots[i];
                    if (name.StartsWith("__") && !name.EndsWith("__")) {
                        name = "_" + typeName + name;
                    }

                    if (name == "__dict__") {
                        dict[SymbolTable.StringToId(name)] = new DynamicTypeDictSlot();
                        continue;
                    } else if (name == "__weakref__") {
                        continue;
                    }
                    
                    dict[SymbolTable.StringToId(name)] = new ReflectedSlotProperty(name, ret, i);
                }
            }

            return ret;
        }

        private static NewTypeMaker GetTypeMaker(Tuple bases, NewTypeInfo ti) {
            if (IsInstanceType(ti.BaseType)) {
                return new NewSubtypeMaker(bases, ti);
            }

            return new NewTypeMaker(bases, ti);
        }

        private static List<string> GetSlots(IAttributesCollection dict) {
            List<string> res = null;
            object slots;
            if (dict != null && dict.TryGetValue(Symbols.Slots, out slots)) {
                res = SlotsToList(slots);
            }

            return res;
        }

        internal static List<string> SlotsToList(object slots) {
            List<string> res = new List<string>();
            ISequence seq = slots as ISequence;
            if (seq != null && !(seq is ExtensibleString)) {
                res = new List<string>(seq.GetLength());
                for (int i = 0; i < seq.GetLength(); i++) {
                    res.Add(GetSlotName(seq[i]));
                }

                res.Sort();
            } else {
                res = new List<string>(1);
                res.Add(GetSlotName(slots));
            }
            return res;
        }


        private static string GetSlotName(object o) {
            string value;
            if (!Converter.TryConvertToString(o, out value) || String.IsNullOrEmpty(value))
                throw PythonOps.TypeError("slots must be one string or a list of strings");

            for (int i = 0; i < value.Length; i++) {
                if ((value[i] >= 'a' && value[i] <= 'z') ||
                    (value[i] >= 'A' && value[i] <= 'Z') ||
                    (i != 0 && value[i] >= '0' && value[i] <= '9') ||
                    value[i] == '_') {
                    continue;
                }
                throw PythonOps.TypeError("__slots__ must be valid identifiers");
            }

            return value;
        }

        /// <summary>
        /// Is this a type used for instances Python types (and not for the types themselves)?
        /// </summary>
        internal static bool IsInstanceType(Type type) {
            return type.FullName.IndexOf(NewTypeMaker.TypePrefix) == 0;
        }

        /// <summary>
        /// "bases" contains a set of PythonTypes. These can include types defined in Python (say cpy1, cpy2),
        /// CLI types (say cCLI1, cCLI2), and CLI interfaces (say iCLI1, iCLI2). Here are some
        /// examples of how this works:
        /// 
        /// (bases)                      => baseType,        {interfaceTypes}
        /// 
        /// (cpy1)                       => System.Object,   {}
        /// (cpy1, cpy2)                 => System.Object,   {}
        /// (cpy1, cCLI1, iCLI1, iCLI2)  => cCLI1,           {iCLI1, iCLI2}
        /// [some type that satisfies the line above] => 
        ///                                 cCLI1,           {iCLI1, iCLI2}
        /// (cCLI1, cCLI2)               => error
        /// </summary>
        private static NewTypeInfo GetTypeInfo(string typeName, Tuple bases, List<string> slots) {
            List<Type> interfaceTypes = new List<Type>();
            Type baseCLIType = typeof(object); // Pure Python object instances inherit from System.Object
            DynamicType basePythonType = null;

            foreach (object curBaseType in bases) {
                DynamicType curBasePythonType = curBaseType as DynamicType;

                if (curBasePythonType == null) {
                    if (curBaseType is OldClass)
                        continue;
                    throw PythonOps.TypeError(typeName + ": unsupported base type for new-style class: " + baseCLIType);
                }

                IList<Type> baseInterfaces = new Type[0];
                Type curTypeToExtend = curBasePythonType.ExtensionType;
                if (curBasePythonType.ExtensionType.IsInterface) {
                    baseInterfaces = new Type[] { curTypeToExtend };
                    curTypeToExtend = typeof(object);
                } else {
                    if (IsInstanceType(curTypeToExtend)) {
                        DynamicTypeSlot dummy;
                        baseInterfaces = new List<Type>();
                        if (!curBasePythonType.TryLookupSlot(DefaultContext.Default, Symbols.Slots, out dummy) &&
                            (slots == null || slots.Count == 0)) {
                            curTypeToExtend = GetBaseTypeFromUserType(curBasePythonType, baseInterfaces, curTypeToExtend.BaseType);
                        }
                    }
                }

                if (curTypeToExtend == null || typeof(BuiltinFunction).IsAssignableFrom(curTypeToExtend) || typeof(PythonFunction).IsAssignableFrom(curTypeToExtend))
                    throw PythonOps.TypeError(typeName + ": {0} is not an acceptable base type", curBasePythonType.Name);
                if (curTypeToExtend.ContainsGenericParameters)
                    throw PythonOps.TypeError(typeName + ": cannot inhert from open generic instantiation {0}. Only closed instantiations are supported.", curBasePythonType);

                foreach (Type interfaceType in baseInterfaces) {
                    if (interfaceType.ContainsGenericParameters)
                        throw PythonOps.TypeError(typeName + ": cannot inhert from open generic instantiation {0}. Only closed instantiations are supported.", interfaceType);

                    interfaceTypes.Add(interfaceType);
                }

                if (curTypeToExtend != typeof(object)) {
                    if (baseCLIType != typeof(object) && baseCLIType != curTypeToExtend) {
                        bool isOkConflit = false;
                        if (IsInstanceType(baseCLIType) && IsInstanceType(curTypeToExtend)) {
                            List<string> slots1 = SlotsToList(curBasePythonType.GetBoundMember(DefaultContext.Default, null, Symbols.Slots));
                            List<string> slots2 = SlotsToList(basePythonType.GetBoundMember(DefaultContext.Default, null, Symbols.Slots));
                            if (curBasePythonType.UnderlyingSystemType.BaseType == basePythonType.UnderlyingSystemType.BaseType &&
                                slots1.Count == 1 && slots2.Count == 1 &&
                                ((slots1[0] == "__dict__" && slots2[0] == "__weakref__") ||
                                (slots2[0] == "__dict__" && slots1[0] == "__weakref__"))) {
                                isOkConflit = true;
                                curTypeToExtend = curBasePythonType.UnderlyingSystemType.BaseType;
                                if (slots != null) {
                                    if (slots.Contains("__weakref__"))
                                        throw PythonOps.TypeError("__weakref__ disallowed, base class already defines this");

                                    slots.Add("__weakref__");
                                    if (!slots.Contains("__dict__"))
                                        slots.Add("__dict__");
                                }
                            }
                        }
                        if (!isOkConflit) throw PythonOps.TypeError(typeName + ": can only extend one CLI or builtin type, not both {0} (for {1}) and {2} (for {3})",
                                             baseCLIType.FullName, basePythonType, curTypeToExtend.FullName, curBasePythonType);
                    }

                    baseCLIType = curTypeToExtend;
                    basePythonType = curBasePythonType;
                }

            }

            return new NewTypeInfo(baseCLIType, interfaceTypes, slots);
        }

        private static Type GetBaseTypeFromUserType(DynamicType curBasePythonType, IList<Type> baseInterfaces, Type curTypeToExtend) {
            Queue<DynamicType> processing = new Queue<DynamicType>();
            processing.Enqueue(curBasePythonType);

            do {
                DynamicType walking = processing.Dequeue();
                foreach (DynamicType dt in walking.BaseTypes) {
                    if (dt.ExtensionType == curTypeToExtend) continue;

                    if (dt.ExtensionType.IsInterface) {
                        baseInterfaces.Add(dt.ExtensionType);
                    } else if (IsInstanceType(dt.ExtensionType)) {
                        processing.Enqueue(dt);
                    } else if(!Mro.IsOldStyle(dt)) {
                        curTypeToExtend = null;
                        break;
                    }
                }
            } while (processing.Count > 0);
            return curTypeToExtend;
        }

        internal NewTypeMaker(Tuple baseClasses, NewTypeInfo typeInfo) {
            this._baseType = typeInfo.BaseType;
            this._baseClasses = baseClasses;
            this._interfaceTypes = typeInfo.InterfaceTypes;
            this._slots = typeInfo.Slots;
        }

        private static string GetBaseName(MethodInfo mi, Dictionary<string, bool> specialNames) {
            Debug.Assert(mi.Name.StartsWith("#base#"));

            string newName = mi.Name.Substring(6);

            Debug.Assert(specialNames.ContainsKey(newName));

            if (specialNames[newName] == true) {
                if (newName == "get_Item") return "__getitem__";
                else if (newName == "set_Item") return "__setitem__";
            }
            return newName;
        }

        // Coverts a well-known CLI name to its Python equivalent
        private static string NormalizeName(string name) {
            if (name == "ToString") return "__str__";
            return name;
        }

        // Build a name which is unique to this TypeInfo.
        protected virtual string GetName() {
            StringBuilder name = new StringBuilder(_baseType.Namespace);
            name.Append('.');
            name.Append(_baseType.Name);
            foreach (Type interfaceType in _interfaceTypes) {
                name.Append("#");
                name.Append(interfaceType.Name);
            }

            name.Append("_");
            name.Append(System.Threading.Interlocked.Increment(ref _typeCount));
            return name.ToString();

        }

        protected virtual void ImplementInterfaces() {
            foreach (Type interfaceType in _interfaceTypes) {
                ImplementInterface(interfaceType);
            }
        }

        protected void ImplementInterface(Type interfaceType) {
            _tg.TypeBuilder.AddInterfaceImplementation(interfaceType);
        }

        private Type CreateNewType() {
            AssemblyGen ag = ScriptDomainManager.CurrentManager.Snippets.Assembly;

            string name = GetName();
            _tg = ag.DefinePublicType(TypePrefix + name, _baseType);
            _tg.Binder = PythonEngine.CurrentEngine.DefaultBinder;

            ImplementInterfaces();

            GetOrDefineClass();

            GetOrDefineDict();

            ImplementSlots();

            ImplementPythonObject();

            ImplementConstructors();

            Dictionary<string, bool> specialNames = new Dictionary<string, bool>();

            OverrideVirtualMethods(_baseType, specialNames);

            Dictionary<Type, bool> doneTypes = new Dictionary<Type, bool>();
            foreach (Type interfaceType in _interfaceTypes) {
                DoInterfaceType(interfaceType, doneTypes, specialNames);
            }

            InitializeVTableStrings();

            // Hashtable slots = collectSlots(dict, tg);
            // if (slots != null) tg.createAttrMethods(slots);

            Type ret = _tg.FinishType();

            AddBaseMethods(ret, specialNames);

            return ret;
        }

        protected virtual void ImplementPythonObject() {
            ImplementSuperDynamicObject();

            ImplementDynamicObject();

#if !SILVERLIGHT // ICustomTypeDescriptor
            ImplementCustomTypeDescriptor();
#endif
            ImplementPythonEquals();

            ImplementWeakReference();
        }

        private void GetOrDefineDict() {
            FieldInfo baseDictField = _baseType.GetField("_dict");
            if (baseDictField == null) {
                _dictField = _tg.AddField(typeof(IAttributesCollection), "__dict__");
            } else {
                _dictField = new FieldSlot(new ThisSlot(_tg.TypeBuilder), baseDictField);
            }
        }

        private void GetOrDefineClass() {
            FieldInfo baseTypeField = _baseType.GetField("__class__");
            if (baseTypeField == null) {
                _typeField = _tg.AddField(typeof(DynamicType), "__class__");
            } else {
                Debug.Assert(baseTypeField.FieldType == typeof(DynamicType));
                _typeField = new FieldSlot(new ThisSlot(_tg.TypeBuilder), baseTypeField);
                _hasBaseTypeField = true;
            }
        }

        protected virtual ParameterInfo[] GetOverrideCtorSignature(ParameterInfo[] original) {
            ParameterInfo[] argTypes = new ParameterInfo[original.Length + 1];
            if (original.Length == 0 || original[0].ParameterType != typeof(CodeContext)) {
                argTypes[0] = new ParameterInfoWrapper(_typeField.Type, "cls");
                Array.Copy(original, 0, argTypes, 1, argTypes.Length - 1);
            } else {
                argTypes[0] = original[0];
                argTypes[1] = new ParameterInfoWrapper(_typeField.Type, "cls");
                Array.Copy(original, 1, argTypes, 2, argTypes.Length - 2);
            }            
            
            return argTypes;
        }

        private void ImplementConstructors() {
            ConstructorInfo[] constructors;
            constructors = _baseType.GetConstructors(BindingFlags.Public |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.Instance
                                                    );

            foreach (ConstructorInfo ci in constructors) {
                if (!(ci.IsPublic || ci.IsFamily)) continue;
                OverrideConstructor(ci);
            }
        }

        protected virtual bool ShouldOverrideVirtual(MethodInfo mi) {
            return true;
        }

        private void AddBaseMethods(Type finishedType, Dictionary<string, bool> specialNames) {
            // "Adds" base methods to super type (should really add to the derived type)
            // this makes super(...).xyz to work - otherwise we'd return a function that
            // did a virtual call resulting in a stack overflow.
            DynamicType rt = DynamicHelpers.GetDynamicTypeFromType(_baseType);

            foreach (MethodInfo mi in finishedType.GetMethods()) {
                if (!ShouldOverrideVirtual(mi)) continue;

                string methodName = mi.Name;
                if (methodName.StartsWith("#base#")) {
                    string newName = GetBaseName(mi, specialNames);
                    DynamicMixinBuilder dtb = DynamicMixinBuilder.GetBuilder(rt);
                    DynamicTypeSlot dts;
                    if(rt.TryLookupSlot(DefaultContext.Default, SymbolTable.StringToId(newName), out dts)) {
                        BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;
                        if (bmd != null) {
                            bmd.Template.AddMethod(mi);
                        }
                    }
                }
            }
        }

        private void DoInterfaceType(Type interfaceType, Dictionary<Type, bool> doneTypes, Dictionary<string, bool> specialNames) {
            if (doneTypes.ContainsKey(interfaceType)) return;
            doneTypes.Add(interfaceType, true);
            OverrideVirtualMethods(interfaceType, specialNames);

            foreach (Type t in interfaceType.GetInterfaces()) {
                DoInterfaceType(t, doneTypes, specialNames);
            }
        }

        private ConstructorBuilder OverrideConstructor(ConstructorInfo parentConstructor) {
            ParameterInfo[] pis = parentConstructor.GetParameters();
            ParameterInfo[] overrideParams = GetOverrideCtorSignature(pis);

            Type[] argTypes = new Type[overrideParams.Length];
            string[] paramNames = new string[overrideParams.Length];
            for (int i = 0; i < overrideParams.Length; i++) {
                argTypes[i] = overrideParams[i].ParameterType;
                paramNames[i] = overrideParams[i].Name;
            }

            ConstructorBuilder cb = _tg.TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, argTypes);

            for (int i = 0; i < overrideParams.Length; i++) {
                ParameterBuilder pb = cb.DefineParameter(i + 1,
                    overrideParams[i].Attributes,
                    overrideParams[i].Name);

                int origIndex = GetOriginalIndex(pis, overrideParams, i);
                if (origIndex >= 0) {
                    if (pis[origIndex].IsDefined(typeof(ParamArrayAttribute), false)) {
                        pb.SetCustomAttribute(new CustomAttributeBuilder(
                            typeof(ParamArrayAttribute).GetConstructor(Utils.Reflection.EmptyTypes), RuntimeHelpers.EmptyObjectArray));
                    } else if (pis[origIndex].IsDefined(typeof(ParamDictionaryAttribute), false)) {
                        pb.SetCustomAttribute(new CustomAttributeBuilder(
                            typeof(ParamDictionaryAttribute).GetConstructor(Utils.Reflection.EmptyTypes), RuntimeHelpers.EmptyObjectArray));
                    }
                }
            }

            CodeGen cg = _tg.CreateCodeGen(cb, cb.GetILGenerator(), argTypes);

            // <typeField> = <arg0>
            if (pis.Length == 0 || pis[0].ParameterType != typeof(CodeContext)) {
                cg.EmitArgGet(0);
            } else {                
                cg.EmitArgGet(1);
            }
            _typeField.EmitSet(cg);

            // initialize all slots to Uninitialized.instance
            if (_slots != null) {
                MethodInfo init = typeof(PythonOps).GetMethod("InitializeUserTypeSlots");

                cg.EmitType(_tupleType);
                cg.EmitCall(init);
                cg.Emit(OpCodes.Unbox_Any, _tupleType);
                _slotsField.EmitSet(cg);
            }

            CallBaseConstructor(parentConstructor, pis, overrideParams, cg);
            return cb;
        }

        /// <summary>
        /// Gets the position for the parameter which we are overriding.
        /// </summary>
        /// <param name="pis"></param>
        /// <param name="overrideParams"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static int GetOriginalIndex(ParameterInfo[] pis, ParameterInfo[] overrideParams, int i) {
            if (pis.Length == 0 || pis[0].ParameterType != typeof(CodeContext)) {
                return i - (overrideParams.Length - pis.Length);
            }

            // context & cls are swapped, context comes first.
            if (i == 1) return -1;
            if (i == 0) return 0;

            return i - (overrideParams.Length - pis.Length);
        }

        private static void CallBaseConstructor(ConstructorInfo parentConstructor, ParameterInfo[] pis, ParameterInfo[] overrideParams, CodeGen cg) {
            cg.EmitThis();
#if DEBUG
            int lastIndex = -1;
#endif
            for (int i = 0; i < overrideParams.Length; i++) {
                int index = GetOriginalIndex(pis, overrideParams, i);

#if DEBUG
                // we insert a new parameter (the class) but the parametrers should
                // still remain in the same order after the extra parameter is removed.
                if (index >= 0) {
                    Debug.Assert(index > lastIndex);
                    lastIndex = index;
                }
#endif
                if(index >= 0) cg.EmitArgGet(i);
            }
            cg.Emit(OpCodes.Call, parentConstructor);
            cg.Emit(OpCodes.Ret);
        }

        private void InitializeVTableStrings() {
            string[] names = new string[_vtable.Count];
            foreach (VTableEntry slot in _vtable.Values) {
                names[slot.index] = slot.name;
            }

            Slot namesField = _tg.AddStaticField(typeof(string[]), VtableNamesField);

            CodeGen cg = _tg.TypeInitializer;
            cg.EmitArray(names);
            namesField.EmitSet(cg);
        }

#if !SILVERLIGHT // ICustomTypeDescriptor
        private void ImplementCustomTypeDescriptor() {
            _tg.TypeBuilder.AddInterfaceImplementation(typeof(ICustomTypeDescriptor));

            foreach (MethodInfo m in typeof(ICustomTypeDescriptor).GetMethods()) {
                ImplementCTDOverride(m);
            }
        }

        private void ImplementCTDOverride(MethodInfo m) {
            CodeGen cg = _tg.DefineExplicitInterfaceImplementation(m);
            cg.EmitThis();

            ParameterInfo[] pis = m.GetParameters();
            Type[] paramTypes = new Type[pis.Length + 1];
            paramTypes[0] = typeof(object);
            for (int i = 0; i < pis.Length; i++) {
                cg.EmitArgGet(i);
                paramTypes[i + 1] = pis[i].ParameterType;
            }

            cg.EmitCall(typeof(CustomTypeDescHelpers), m.Name, paramTypes);
            cg.EmitConvertToObject(m.ReturnType);
            cg.EmitReturn();
            cg.Finish();
        }
#endif

        protected bool NeedsDictionary {
            get {
                if (_slots == null) return true;
                if (_slots.Contains("__dict__")) return true;
                
                foreach (DynamicType pt in _baseClasses) {
                    if (IsInstanceType(pt.UnderlyingSystemType)) return true;
                }

                return false;
            }
        }

        private void ImplementDynamicObject() {
            _tg.TypeBuilder.AddInterfaceImplementation(typeof(IDynamicObject));

            CodeGen getRuleMethod = _tg.DefineMethodOverride(typeof(IDynamicObject).GetMethod("GetRule"));
            MethodInfo mi = typeof(UserTypeOps).GetMethod("GetRuleHelper");
            GenericTypeParameterBuilder[] types = ((MethodBuilder)getRuleMethod.MethodInfo).DefineGenericParameters("T");

            for (int i = 0; i < 3; i++) getRuleMethod.EmitArgGet(i);

            getRuleMethod.EmitCall(mi.MakeGenericMethod(types));
            getRuleMethod.EmitReturn();
            getRuleMethod.Finish();

            CodeGen getContextMethod = _tg.DefineMethodOverride(typeof(IDynamicObject).GetMethod("get_LanguageContext"));
            getContextMethod.EmitCall(typeof(PythonOps), "GetLanguageContext");
            getContextMethod.EmitReturn();
            getContextMethod.Finish();
        }

        private void ImplementSuperDynamicObject() {
            CodeGen cg;

            _tg.TypeBuilder.AddInterfaceImplementation(typeof(ISuperDynamicObject));

            MethodAttributes attrs = (MethodAttributes)0;
            if (_slots != null) attrs = MethodAttributes.Virtual;

            cg = _tg.DefineMethodOverride(attrs, typeof(ISuperDynamicObject).GetMethod("get_Dict"));
            if (NeedsDictionary) {
                _dictField.EmitGet(cg);
                cg.EmitReturn();
            } else {
                cg.Emit(OpCodes.Ldnull);
                cg.EmitReturn();
            }
            cg.Finish();

            cg = _tg.DefineMethodOverride(attrs, typeof(ISuperDynamicObject).GetMethod("ReplaceDict"));
            if (NeedsDictionary) {
                cg.EmitArgGet(0);
                _dictField.EmitSet(cg);
                cg.EmitBoolean(true);
                cg.EmitReturn();
            } else {
                cg.EmitBoolean(false);
                cg.EmitReturn();
            }
            cg.Finish();

            cg = _tg.DefineMethodOverride(attrs, typeof(ISuperDynamicObject).GetMethod("get_HasDictionary"));
            if (NeedsDictionary) {
                cg.EmitBoolean(true);
                cg.EmitReturn();
            } else {
                cg.EmitBoolean(false);
                cg.EmitReturn();
            }
            cg.Finish();

            cg = _tg.DefineMethodOverride(attrs, typeof(ISuperDynamicObject).GetMethod("SetDict"));
            if (NeedsDictionary) {
                _dictField.EmitGetAddr(cg);
                cg.EmitArgGet(0);
                cg.EmitCall(typeof(UserTypeOps), "SetDictHelper");
                                
                cg.EmitReturn();
            } else {
                cg.EmitNull();
                cg.EmitReturn();
            }
            cg.Finish();

            if (_hasBaseTypeField) return;

            cg = _tg.DefineMethodOverride(attrs, typeof(ISuperDynamicObject).GetMethod("get_DynamicType"));
            _typeField.EmitGet(cg);
            cg.EmitReturn();
            cg.Finish();

            cg = _tg.DefineMethodOverride(attrs, typeof(ISuperDynamicObject).GetMethod("SetDynamicType"));
            cg.EmitArgGet(0);
            _typeField.EmitSet(cg);
            cg.EmitReturn();
            cg.Finish();
        }

        /// <summary>
        /// Defines an interface on the type that forwards all calls
        /// to a helper method in UserType.  The method names all will
        /// have Helper appended to them to get the name for UserType.  The 
        /// UserType version should take 1 extra parameter (self).
        /// </summary>
        /// <param name="intf"></param>
        /// <param name="fExplicit"></param>
        private void DefineHelperInterface(Type intf, bool fExplicit) {
            _tg.TypeBuilder.AddInterfaceImplementation(intf);
            MethodInfo[] mis = intf.GetMethods();

            foreach (MethodInfo mi in mis) {

                CodeGen cg = fExplicit ? _tg.DefineExplicitInterfaceImplementation(mi) : _tg.DefineMethodOverride(mi);
                ParameterInfo[] pis = mi.GetParameters();

                MethodInfo helperMethod = typeof(UserTypeOps).GetMethod(mi.Name + "Helper");
                int offset = 0;
                if (pis.Length > 0 && pis[0].ParameterType == typeof(CodeContext)) {
                    // if the interface takes CodeContext then the helper method better take
                    // it as well.
                    Debug.Assert(helperMethod.GetParameters()[0].ParameterType == typeof(CodeContext));
                    offset = 1;
                    cg.EmitArgGet(0);
                } 

                cg.EmitThis();
                for (int i = offset; i < pis.Length; i++) {
                    cg.EmitArgGet(i);
                }

                cg.EmitCall(helperMethod);
                cg.EmitReturn();
                cg.Finish();
            }
        }

        private void ImplementPythonEquals() {
            if (this._baseType.GetInterface("IValueEquality", false) == null) {
                DefineHelperInterface(typeof(IValueEquality), false);
            }
        }

        private void CreateWeakRefField() {
            if (_weakrefField != null) return;

            FieldInfo fi = _baseType.GetField("__weakref__");
            if (fi != null) {
                // base defines it
                _weakrefField = new FieldSlot(new ThisSlot(_baseType), fi);
            }

            if (_weakrefField == null) {
                _weakrefField = _tg.AddField(typeof(WeakRefTracker), "__weakref__");
            }
        }

        internal bool BaseHasWeakRef(DynamicType curType) {
            DynamicType dt = curType;
            DynamicTypeSlot dts;
            if (dt != null && 
                dt.TryLookupSlot(DefaultContext.Default, Symbols.Slots, out dts) &&
                dt.TryLookupSlot(DefaultContext.Default, Symbols.WeakRef, out dts)) {
                return true;
            }

            foreach (DynamicType baseType in curType.BaseTypes) {
                if (BaseHasWeakRef(baseType)) return true;
            }
            return false;
        }
        protected virtual void ImplementWeakReference() {
            CreateWeakRefField();

            bool isWeakRefAble = true;
            if (_slots != null && !_slots.Contains("__weakref__")) {
                // always define the field, only implement the interface
                // if we are slotless or the user defined __weakref__ in slots
                bool baseHasWeakRef = false;
                foreach (object pt in _baseClasses) {
                    DynamicType dt = pt as DynamicType;
                    if (dt != null && BaseHasWeakRef(dt)) {
                        baseHasWeakRef = true;
                        break;
                    }
                }
                if (baseHasWeakRef) return;

                isWeakRefAble = false;
            }

            _tg.TypeBuilder.AddInterfaceImplementation(typeof(IWeakReferenceable));

            CodeGen cg = _tg.DefineMethodOverride(typeof(IWeakReferenceable).GetMethod("SetWeakRef"));
            if (!isWeakRefAble) {
                cg.EmitBoolean(false);
                cg.EmitReturn();
            } else {
                cg.EmitArgGet(0);
                _weakrefField.EmitSet(cg);
                cg.EmitBoolean(true);
                cg.EmitReturn();
            }
            cg.Finish();

            cg = _tg.DefineMethodOverride(typeof(IWeakReferenceable).GetMethod("SetFinalizer"));
            cg.EmitArgGet(0);
            _weakrefField.EmitSet(cg);
            cg.EmitReturn();
            cg.Finish();

            cg = _tg.DefineMethodOverride(typeof(IWeakReferenceable).GetMethod("GetWeakRef"));
            _weakrefField.EmitGet(cg);
            cg.EmitReturn();
            cg.Finish();
        }

        private void ImplementSlots() {
            if (_slots != null) {
                GenericTypeParameterBuilder[] tbp = _tg.TypeBuilder.DefineGenericParameters("Slots");
                _slotsField = _tg.AddField(tbp[0], ".SlotValues");
                _tupleType = tbp[0];

                PropertyBuilder pb = _tg.DefineProperty("$SlotValues", PropertyAttributes.None, tbp[0]);

                CodeGen getter = _tg.DefineMethod(MethodAttributes.Public,
                        "get_$SlotValues",
                        tbp[0],
                        new Type[0],
                        new string[0]);

                _slotsField.EmitGet(getter);
                getter.EmitReturn();
                getter.Finish();

                pb.SetGetMethod(getter.MethodInfo as MethodBuilder);
            }
        }

        private void OverrideVirtualMethods(Type type, Dictionary<string, bool> specialNames) {
            foreach (MethodInfo mi in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                if (!ShouldOverrideVirtual(mi)) continue;

                if (mi.IsPublic || mi.IsFamily || mi.IsFamilyOrAssembly) {
                    if (mi.IsGenericMethodDefinition) continue;

                    if (mi.IsSpecialName) {
                        OverrideSpecialName(mi, specialNames);
                    } else {
                        OverrideBaseMethod(mi, specialNames);
                    }
                }
            }
        }

        private void OverrideSpecialName(MethodInfo mi, Dictionary<string, bool> specialNames) {
            if (mi == null || !mi.IsVirtual || mi.IsFinal) return;

            string name;
            PropertyInfo[] pis = mi.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            specialNames[mi.Name] = true;
            foreach (PropertyInfo pi in pis) {                
                if (pi.GetIndexParameters().Length > 0) {
                    if (mi == pi.GetGetMethod(true)) {
                        CreateVTableMethodOverride(mi, GetOrMakeVTableEntry("__getitem__"));
                        if (!mi.IsAbstract) CreateVirtualMethodHelper(_tg, mi);
                        return;
                    } else if (mi == pi.GetSetMethod(true)) {
                        CreateVTableMethodOverride(mi, GetOrMakeVTableEntry("__setitem__"));
                        if (!mi.IsAbstract) CreateVirtualMethodHelper(_tg, mi);
                        return;
                    }
                } else if (mi == pi.GetGetMethod(true)) {
                    if (mi.Name != "get_DynamicType") {
                        if (NameConverter.TryGetName(DynamicHelpers.GetDynamicTypeFromType(mi.DeclaringType), pi, mi, out name) == NameType.None) return;
                        CreateVTableGetterOverride(mi, GetOrMakeVTableEntry(name));
                    }
                    return;
                } else if (mi == pi.GetSetMethod(true)) {
                    if (NameConverter.TryGetName(DynamicHelpers.GetDynamicTypeFromType(mi.DeclaringType), pi, mi, out name) == NameType.None) return;
                    CreateVTableSetterOverride(mi, GetOrMakeVTableEntry(name));
                    return;
                }
            }

            EventInfo[] eis = mi.DeclaringType.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (EventInfo ei in eis) {
                if (ei.GetAddMethod() == mi) {
                    if (NameConverter.TryGetName(DynamicHelpers.GetDynamicTypeFromType(mi.DeclaringType), ei, mi, out name) == NameType.None) return;
                    CreateVTableEventOverride(mi, GetOrMakeVTableEntry(mi.Name));
                    return;
                } else if (ei.GetRemoveMethod() == mi) {
                    if (NameConverter.TryGetName(DynamicHelpers.GetDynamicTypeFromType(mi.DeclaringType), ei, mi, out name) == NameType.None) return;
                    CreateVTableEventOverride(mi, GetOrMakeVTableEntry(mi.Name));
                    return;
                }
            }
        }

        /// <summary>
        /// Loads all the incoming arguments of cg and forwards them to mi which
        /// has the same signature and then returns the result
        /// </summary>
        private static void EmitBaseMethodDispatch(MethodInfo mi, CodeGen cg) {
            if (!mi.IsAbstract) {
                cg.EmitThis();
                foreach (Slot argSlot in cg.ArgumentSlots) argSlot.EmitGet(cg);
                cg.EmitCall(OpCodes.Call, mi, null); // base call must be non-virtual
                cg.EmitReturn();
            } else {
                cg.EmitThis();
                cg.EmitString(mi.Name);
                cg.EmitCall(typeof(PythonOps), "MissingInvokeMethodException");
                cg.Emit(OpCodes.Throw);
            }
        }

        private void OverrideBaseMethod(MethodInfo mi, Dictionary<string, bool> specialNames) {
            if ((!mi.IsVirtual || mi.IsFinal) && !mi.IsFamily) {
                return;
            }

            DynamicType baseDynamicType;
            if (_baseType == mi.DeclaringType || _baseType.IsSubclassOf(mi.DeclaringType)) {
                baseDynamicType = DynamicHelpers.GetDynamicTypeFromType(_baseType);
            } else {
                // We must be inherting from an interface
                Debug.Assert(mi.DeclaringType.IsInterface);
                baseDynamicType = DynamicHelpers.GetDynamicTypeFromType(mi.DeclaringType);
            }

            string name = null;
            if (NameConverter.TryGetName(baseDynamicType, mi, out name) == NameType.None)
                return;

            if (mi.DeclaringType == typeof(object) && mi.Name == "Finalize") return;

            specialNames[mi.Name] = false;
            
            CreateVTableMethodOverride(mi, GetOrMakeVTableEntry(name));
            if (!mi.IsAbstract) CreateVirtualMethodHelper(_tg, mi);
        }

        private VTableEntry GetOrMakeVTableEntry(string name) {
            VTableEntry ret;
            if (_vtable.TryGetValue(name, out ret)) return ret;

            ret = new VTableEntry(name, _vtable.Count);
            _vtable[name] = ret;
            return ret;
        }

        private static void EmitBadCallThrow(CodeGen cg, MethodInfo mi, string reason) {
            cg.EmitString("Cannot override method from IronPython {0} because " + reason);
            cg.EmitInt(1);
            cg.Emit(OpCodes.Newarr, typeof(object));
            cg.Emit(OpCodes.Dup);
            cg.EmitInt(0);
            cg.EmitString(mi.Name);
            cg.Emit(OpCodes.Stelem_Ref);
            cg.EmitCall(typeof(PythonOps), "TypeError");
            cg.Emit(OpCodes.Throw);
        }

        /// <summary>
        /// Emits code to check if the class has overriden this specific
        /// function.  For example:
        /// 
        /// MyDerivedType.SomeVirtualFunction = ...
        ///     or
        /// 
        /// class MyDerivedType(MyBaseType):
        ///     def SomeVirtualFunction(self, ...):
        /// 
        /// </summary>
        internal Slot EmitBaseClassCallCheckForProperties(CodeGen cg, MethodInfo baseMethod, VTableEntry methField) {
            Label instanceCall = cg.DefineLabel();
            Slot callTarget = cg.GetLocalTmp(typeof(object));

            _typeField.EmitGet(cg);
            cg.EmitThis();
            EmitSymbolId(cg, methField.name);
            callTarget.EmitGetAddr(cg);
            cg.EmitCall(typeof(UserTypeOps), "TryGetNonInheritedValueHelper");

            cg.Emit(OpCodes.Brtrue, instanceCall);

            EmitBaseMethodDispatch(baseMethod, cg);

            cg.MarkLabel(instanceCall);

            return callTarget;
        }

        private void CreateVTableGetterOverride(MethodInfo mi, VTableEntry methField) {
            CodeGen cg = _tg.DefineMethodOverride(mi);
            Slot callTarget = EmitBaseClassCallCheckForProperties(cg, mi, methField);

            callTarget.EmitGet(cg);
            cg.EmitThis();
            EmitSymbolId(cg, methField.name);
            cg.EmitCall(typeof(UserTypeOps), "GetPropertyHelper");

            cg.EmitReturnFromObject();
            cg.Finish();
        }

        private void CreateVTableSetterOverride(MethodInfo mi, VTableEntry methField) {
            CodeGen cg = _tg.DefineMethodOverride(mi);
            Slot callTarget = EmitBaseClassCallCheckForProperties(cg, mi, methField);

            callTarget.EmitGet(cg);  // property
            cg.EmitThis();           // instance
            cg.EmitArgGet(0);
            cg.EmitConvertToObject(mi.GetParameters()[0].ParameterType);    // newValue
            EmitSymbolId(cg, methField.name);    // name
            cg.EmitCall(typeof(UserTypeOps), "SetPropertyHelper");

            cg.EmitReturn();
            cg.Finish();
        }

        private void CreateVTableEventOverride(MethodInfo mi, VTableEntry methField) {
            // override the add/remove method            
            CodeGen cg = _tg.DefineMethodOverride(mi);

            Slot callTarget = EmitBaseClassCallCheckForProperties(cg, mi, methField);

            callTarget.EmitGet(cg);
            cg.EmitThis();
            _typeField.EmitGet(cg);
            cg.EmitArgGet(0);
            cg.EmitConvertToObject(mi.GetParameters()[0].ParameterType);
            EmitSymbolId(cg, methField.name);
            cg.EmitCall(typeof(UserTypeOps), "AddRemoveEventHelper");

            cg.EmitReturn();
            cg.Finish();
        }

        private void CreateVTableMethodOverride(MethodInfo mi, VTableEntry methField) {
            ParameterInfo[] parameters = mi.GetParameters();
            CodeGen cg = (mi.IsVirtual && !mi.IsFinal) ? _tg.DefineMethodOverride(mi) : _tg.DefineMethod(
                mi.IsVirtual ? (mi.Attributes | MethodAttributes.NewSlot) : mi.Attributes,
                    mi.Name,
                    mi.ReturnType,
                    Utils.Reflection.GetParameterTypes(parameters),
                    CompilerHelpers.GetArgumentNames(parameters));

            Label instanceCall = cg.DefineLabel();
            Slot callTarget = cg.GetLocalTmp(typeof(object));

            // emit call to helper to do lookup
            _typeField.EmitGet(cg);
            cg.EmitThis();
            EmitSymbolId(cg, methField.name);
            callTarget.EmitGetAddr(cg);
            cg.EmitCall(typeof(UserTypeOps), "TryGetNonInheritedMethodHelper");
            
            cg.Emit(OpCodes.Brtrue, instanceCall);

            EmitBaseMethodDispatch(mi, cg);

            cg.MarkLabel(instanceCall);

            int argStart = 0;
            StubGenerator.CallType attrs = StubGenerator.CallType.None;

            ParameterInfo[] pis = mi.GetParameters();
            Slot context = null;
            if (pis.Length > 0) {
                if (pis[0].ParameterType == typeof(CodeContext)) {
                    argStart = 1;
                    context = cg.ArgumentSlots[0];
                }
                if (pis[pis.Length - 1].IsDefined(typeof(ParamArrayAttribute), false)) {
                    attrs |= StubGenerator.CallType.ArgumentList;
                }
            }
            if (context == null) {
                context = new PropertySlot(null, typeof(DefaultContext).GetProperty("Default"));
            }
            cg.ContextSlot = context;

            StubGenerator.EmitClrCallStub(cg, callTarget, argStart, attrs, null);

            cg.Finish();
        }

        public static CodeGen CreateVirtualMethodHelper(TypeGen tg, MethodInfo mi) {
            ParameterInfo[] parms = mi.GetParameters();
            Type[] types = Utils.Reflection.GetParameterTypes(parms);
            string[] paramNames = new string[parms.Length];
            Type miType = mi.DeclaringType;
            for (int i = 0; i < types.Length; i++) {
                paramNames[i] = parms[i].Name;
                if (types[i] == miType) {
                    types[i] = tg.TypeBuilder;
                }
            }
            CodeGen cg = tg.DefineMethod(MethodAttributes.Public | MethodAttributes.HideBySig,
                                         "#base#" + mi.Name, mi.ReturnType, types, paramNames);

            EmitBaseMethodDispatch(mi, cg);
            cg.Finish();
            return cg;
        }

        private static void EmitSymbolId(CodeGen cg, string name) {
            Debug.Assert(name != null);
            cg.EmitSymbolId(SymbolTable.StringToId(name));
        }
    }

    class VTableEntry {
        public readonly string name;
        public readonly int index;

        public VTableEntry(string name, int index) {
            this.name = name;
            this.index = index;
        }
    }
}
