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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using System.Diagnostics;

using System.Reflection;
using System.Reflection.Emit;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Generation {
    using Compiler = Microsoft.Scripting.Ast.Compiler;
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
        public const string BaseMethodPrefix = "#base#";
        public const string FieldGetterPrefix = "#field_get#", FieldSetterPrefix = "#field_set#";

        private static Publisher<NewTypeInfo, Type> _newTypes = new Publisher<NewTypeInfo, Type>();
        private static int _typeCount;

        protected Type _baseType;
        protected IList<string> _slots;
        protected TypeGen _tg;
        protected Slot _typeField, _dictField, _weakrefField, _slotsField;
        protected Type _tupleType;
        protected IEnumerable<Type> _interfaceTypes;
        protected PythonTuple _baseClasses;

        private bool _hasBaseTypeField;

        private Dictionary<string, VTableEntry> _vtable = new Dictionary<string, VTableEntry>();

        public static Type GetNewType(string typeName, PythonTuple bases, IAttributesCollection dict) {
            if (bases == null) bases = PythonTuple.MakeTuple();
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
                Type tupleType = Tuple.MakeTupleType(CompilerHelpers.MakeRepeatedArray(typeof(object), typeInfo.Slots.Count));
                ret = ret.MakeGenericType(tupleType);

                for (int i = 0; i < typeInfo.Slots.Count; i++) {                    
                    string name = typeInfo.Slots[i];
                    if (name.StartsWith("__") && !name.EndsWith("__")) {
                        name = "_" + typeName + name;
                    }

                    if (name == "__dict__") {
                        dict[SymbolTable.StringToId(name)] = new PythonTypeDictSlot();
                        continue;
                    } else if (name == "__weakref__") {
                        continue;
                    }
                    
                    dict[SymbolTable.StringToId(name)] = new ReflectedSlotProperty(name, ret, i);
                }
            }

            DynamicSiteHelpers.InitializeFields(DefaultContext.Default, ret, true);

            return ret;
        }

        private static NewTypeMaker GetTypeMaker(PythonTuple bases, NewTypeInfo ti) {
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
        private static NewTypeInfo GetTypeInfo(string typeName, PythonTuple bases, List<string> slots) {
            List<Type> interfaceTypes = new List<Type>();
            Type baseCLIType = typeof(object); // Pure Python object instances inherit from System.Object
            PythonType basePythonType = null;

            foreach (object curBaseType in bases) {
                PythonType curBasePythonType = curBaseType as PythonType;

                if (curBasePythonType == null) {
                    if (curBaseType is OldClass)
                        continue;
                    throw PythonOps.TypeError(typeName + ": unsupported base type for new-style class: " + baseCLIType);
                }

                IList<Type> baseInterfaces = ArrayUtils.EmptyTypes;
                Type curTypeToExtend = curBasePythonType.ExtensionType;
                if (curBasePythonType.ExtensionType.IsInterface) {
                    baseInterfaces = new Type[] { curTypeToExtend };
                    curTypeToExtend = typeof(object);
                } else {
                    if (IsInstanceType(curTypeToExtend)) {
                        PythonTypeSlot dummy;
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

        private static Type GetBaseTypeFromUserType(PythonType curBasePythonType, IList<Type> baseInterfaces, Type curTypeToExtend) {
            Queue<PythonType> processing = new Queue<PythonType>();
            processing.Enqueue(curBasePythonType);

            do {
                PythonType walking = processing.Dequeue();
                foreach (PythonType dt in walking.BaseTypes) {
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

        internal NewTypeMaker(PythonTuple baseClasses, NewTypeInfo typeInfo) {
            this._baseType = typeInfo.BaseType;
            this._baseClasses = baseClasses;
            this._interfaceTypes = typeInfo.InterfaceTypes;
            this._slots = typeInfo.Slots;
        }

        private static IEnumerable<string> GetBaseName(MethodInfo mi, Dictionary<string, List<string>> specialNames) {
            Debug.Assert(mi.Name.StartsWith(BaseMethodPrefix));

            string newName = mi.Name.Substring(6);

            Debug.Assert(specialNames.ContainsKey(newName));

            return specialNames[newName];
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
            _tg.Binder = PythonBinder.Instance;

            ImplementInterfaces();

            GetOrDefineClass();

            GetOrDefineDict();

            ImplementSlots();

            ImplementPythonObject();

            ImplementConstructors();

            Dictionary<string, List<string>> specialNames = new Dictionary<string, List<string>>();

            OverrideVirtualMethods(_baseType, specialNames);

            ImplementProtectedFieldAccessors();

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
                _typeField = _tg.AddField(typeof(PythonType), "__class__");
            } else {
                Debug.Assert(baseTypeField.FieldType == typeof(PythonType));
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

        private static bool CanOverrideMethod(MethodInfo mi) {
#if !SILVERLIGHT
            return true;
#else
            // can only override the method if it is not SecurityCritical
            return mi.GetCustomAttributes(typeof(System.Security.SecurityCriticalAttribute), false).Length == 0;
#endif
        }

        private void AddBaseMethods(Type finishedType, Dictionary<string, List<string>> specialNames) {
            // "Adds" base methods to super type (should really add to the derived type)
            // this makes super(...).xyz to work - otherwise we'd return a function that
            // did a virtual call resulting in a stack overflow.
            PythonType rt = DynamicHelpers.GetPythonTypeFromType(_baseType);

            foreach (MethodInfo mi in finishedType.GetMethods()) {
                if (!ShouldOverrideVirtual(mi)) continue;

                string methodName = mi.Name;
                if (methodName.StartsWith(BaseMethodPrefix)) {
                    foreach (string newName in GetBaseName(mi, specialNames)) {
                        PythonTypeBuilder dtb = PythonTypeBuilder.GetBuilder(rt);
                        PythonTypeSlot dts;
                        if (rt.TryLookupSlot(DefaultContext.Default, SymbolTable.StringToId(newName), out dts)) {
                            BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;
                            if (bmd != null) {
                                bmd.Template.AddMethod(mi);
                            }
                        }
                    }
                }
            }
        }

        private void DoInterfaceType(Type interfaceType, Dictionary<Type, bool> doneTypes, Dictionary<string, List<string>> specialNames) {
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
                            typeof(ParamArrayAttribute).GetConstructor(ArrayUtils.EmptyTypes), ArrayUtils.EmptyObjects));
                    } else if (pis[origIndex].IsDefined(typeof(ParamDictionaryAttribute), false)) {
                        pb.SetCustomAttribute(new CustomAttributeBuilder(
                            typeof(ParamDictionaryAttribute).GetConstructor(ArrayUtils.EmptyTypes), ArrayUtils.EmptyObjects));
                    }
                }
            }

            Compiler cg = _tg.CreateCodeGen(cb, cb.GetILGenerator(), argTypes);

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
                cg.EmitUnbox(_tupleType);
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

        private static void CallBaseConstructor(ConstructorInfo parentConstructor, ParameterInfo[] pis, ParameterInfo[] overrideParams, Compiler cg) {
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

            Compiler cg = _tg.TypeInitializer;
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
            Compiler cg = _tg.DefineExplicitInterfaceImplementation(m);
            cg.EmitThis();

            ParameterInfo[] pis = m.GetParameters();
            Type[] paramTypes = new Type[pis.Length + 1];
            paramTypes[0] = typeof(object);
            for (int i = 0; i < pis.Length; i++) {
                cg.EmitArgGet(i);
                paramTypes[i + 1] = pis[i].ParameterType;
            }

            cg.EmitCall(typeof(CustomTypeDescHelpers), m.Name, paramTypes);
            cg.EmitBoxing(m.ReturnType);
            cg.EmitReturn();
            cg.Finish();
        }
#endif

        protected bool NeedsDictionary {
            get {
                if (_slots == null) return true;
                if (_slots.Contains("__dict__")) return true;
                
                foreach (PythonType pt in _baseClasses) {
                    if (IsInstanceType(pt.UnderlyingSystemType)) return true;
                }

                return false;
            }
        }

        private void ImplementDynamicObject() {
            _tg.TypeBuilder.AddInterfaceImplementation(typeof(IDynamicObject));

            Compiler getRuleMethod = _tg.DefineMethodOverride(typeof(IDynamicObject).GetMethod("GetRule"));
            MethodInfo mi = typeof(UserTypeOps).GetMethod("GetRuleHelper");
            GenericTypeParameterBuilder[] types = ((MethodBuilder)getRuleMethod.Method).DefineGenericParameters("T");

            for (int i = 0; i < 3; i++) getRuleMethod.EmitArgGet(i);

            getRuleMethod.EmitCall(mi.MakeGenericMethod(types));
            getRuleMethod.EmitReturn();
            getRuleMethod.Finish();

            Compiler getContextMethod = _tg.DefineMethodOverride(typeof(IDynamicObject).GetMethod("get_LanguageContext"));
            getContextMethod.EmitCall(typeof(PythonOps), "GetLanguageContext");
            getContextMethod.EmitReturn();
            getContextMethod.Finish();
        }

        private void ImplementSuperDynamicObject() {
            Compiler cg;

            _tg.TypeBuilder.AddInterfaceImplementation(typeof(IPythonObject));

            MethodAttributes attrs = (MethodAttributes)0;
            if (_slots != null) attrs = MethodAttributes.Virtual;

            cg = _tg.DefineMethodOverride(attrs, typeof(IPythonObject).GetMethod("get_Dict"));
            if (NeedsDictionary) {
                _dictField.EmitGet(cg);
                cg.EmitReturn();
            } else {
                cg.Emit(OpCodes.Ldnull);
                cg.EmitReturn();
            }
            cg.Finish();

            cg = _tg.DefineMethodOverride(attrs, typeof(IPythonObject).GetMethod("ReplaceDict"));
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

            cg = _tg.DefineMethodOverride(attrs, typeof(IPythonObject).GetMethod("get_HasDictionary"));
            if (NeedsDictionary) {
                cg.EmitBoolean(true);
                cg.EmitReturn();
            } else {
                cg.EmitBoolean(false);
                cg.EmitReturn();
            }
            cg.Finish();

            cg = _tg.DefineMethodOverride(attrs, typeof(IPythonObject).GetMethod("SetDict"));
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

            cg = _tg.DefineMethodOverride(attrs, typeof(IPythonObject).GetMethod("get_PythonType"));
            _typeField.EmitGet(cg);
            cg.EmitReturn();
            cg.Finish();

            cg = _tg.DefineMethodOverride(attrs, typeof(IPythonObject).GetMethod("SetPythonType"));
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

                Compiler cg = fExplicit ? _tg.DefineExplicitInterfaceImplementation(mi) : _tg.DefineMethodOverride(mi);
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

        internal bool BaseHasWeakRef(PythonType curType) {
            PythonType dt = curType;
            PythonTypeSlot dts;
            if (dt != null && 
                dt.TryLookupSlot(DefaultContext.Default, Symbols.Slots, out dts) &&
                dt.TryLookupSlot(DefaultContext.Default, Symbols.WeakRef, out dts)) {
                return true;
            }

            foreach (PythonType baseType in curType.BaseTypes) {
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
                    PythonType dt = pt as PythonType;
                    if (dt != null && BaseHasWeakRef(dt)) {
                        baseHasWeakRef = true;
                        break;
                    }
                }
                if (baseHasWeakRef) return;

                isWeakRefAble = false;
            }

            _tg.TypeBuilder.AddInterfaceImplementation(typeof(IWeakReferenceable));

            Compiler cg = _tg.DefineMethodOverride(typeof(IWeakReferenceable).GetMethod("SetWeakRef"));
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

                Compiler getter = _tg.DefineMethod(MethodAttributes.Public,
                        "get_$SlotValues",
                        tbp[0],
                        ArrayUtils.EmptyTypes,
                        ArrayUtils.EmptyStrings);

                _slotsField.EmitGet(getter);
                getter.EmitReturn();
                getter.Finish();

                pb.SetGetMethod(getter.Method as MethodBuilder);
            }
        }

        private void ImplementProtectedFieldAccessors() {
            // For protected fields to be accessible from the derived type in Silverlight,
            // we need to create public helper methods that expose them. These methods are
            // used by the IDynamicObject implementation (in UserTypeOps.GetRuleHelper)

            FieldInfo[] fields = _baseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo fi in fields) {
                if (!fi.IsFamily) continue;

                Compiler cg = _tg.DefineMethod(MethodAttributes.Public | MethodAttributes.HideBySig,
                                             FieldGetterPrefix + fi.Name, fi.FieldType, ArrayUtils.EmptyTypes, ArrayUtils.EmptyStrings);

                cg.EmitThis();
                cg.EmitFieldGet(fi);
                cg.EmitReturn();
                cg.Finish();

                cg = _tg.DefineMethod(MethodAttributes.Public | MethodAttributes.HideBySig,
                                             FieldSetterPrefix + fi.Name, null, new Type[] { fi.FieldType }, new string[] { "value" });

                cg.EmitThis();
                cg.EmitArgGet(0);
                cg.EmitFieldSet(fi);                
                cg.EmitReturn();
                cg.Finish();
            }
        }

        private void OverrideVirtualMethods(Type type, Dictionary<string, List<string>> specialNames) {
            // if we have conflicting virtual's do to new slots only override the methods on the
            // most derived class.
            Dictionary<KeyValuePair<string, MethodSignatureInfo>, MethodInfo> added = new Dictionary<KeyValuePair<string,MethodSignatureInfo>, MethodInfo>();
            
            MethodInfo overridden;
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            
            foreach (MethodInfo mi in methods) {
                KeyValuePair<string, MethodSignatureInfo> key = new KeyValuePair<string, MethodSignatureInfo>(mi.Name, new MethodSignatureInfo(mi.IsStatic, mi.GetParameters()));

                if (!added.TryGetValue(key, out overridden)) {
                    added[key] = mi;
                    continue;
                }

                if (overridden.DeclaringType.IsAssignableFrom(mi.DeclaringType)) {
                    added[key] = mi;
                }
            }

            foreach (MethodInfo mi in added.Values) {
                if (!ShouldOverrideVirtual(mi) || !CanOverrideMethod(mi)) continue;

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

        private void OverrideSpecialName(MethodInfo mi, Dictionary<string, List<string>> specialNames) {
            if (mi == null || !mi.IsVirtual || mi.IsFinal) {
                if (mi.IsFamily) {
                    // need to be able to call into protected getter/setter methods from derived types,
                    // even if these methods aren't virtual and we are in partial trust.
                    List<string> methodNames = new List<string>();
                    methodNames.Add(mi.Name);
                    specialNames[mi.Name] = methodNames;
                    CreateVirtualMethodHelper(_tg, mi);
                }
                return;
            }

            string name;
            PropertyInfo[] pis = mi.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            List<string> names = new List<string>();
            names.Add(mi.Name);
            specialNames[mi.Name] = names;
            foreach (PropertyInfo pi in pis) {                
                if (pi.GetIndexParameters().Length > 0) {
                    if (mi == pi.GetGetMethod(true)) {
                        names.Add("__getitem__");
                        CreateVTableMethodOverride(mi, GetOrMakeVTableEntry("__getitem__"));
                        if (!mi.IsAbstract) CreateVirtualMethodHelper(_tg, mi);
                        return;
                    } else if (mi == pi.GetSetMethod(true)) {
                        names.Add("__setitem__");
                        CreateVTableMethodOverride(mi, GetOrMakeVTableEntry("__setitem__"));
                        if (!mi.IsAbstract) CreateVirtualMethodHelper(_tg, mi);
                        return;
                    }
                } else if (mi == pi.GetGetMethod(true)) {
                    if (mi.Name != "get_PythonType") {
                        names.Add("__getitem__");
                        if (NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType), pi, mi, out name) == NameType.None) return;
                        CreateVTableGetterOverride(mi, GetOrMakeVTableEntry(name));
                        if (!mi.IsAbstract) CreateVirtualMethodHelper(_tg, mi);
                    }
                    return;
                } else if (mi == pi.GetSetMethod(true)) {
                    names.Add("__setitem__");
                    if (NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType), pi, mi, out name) == NameType.None) return;
                    CreateVTableSetterOverride(mi, GetOrMakeVTableEntry(name));
                    if (!mi.IsAbstract) CreateVirtualMethodHelper(_tg, mi);
                    return;
                }
            }

            EventInfo[] eis = mi.DeclaringType.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (EventInfo ei in eis) {
                if (ei.GetAddMethod() == mi) {
                    if (NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType), ei, mi, out name) == NameType.None) return;
                    CreateVTableEventOverride(mi, GetOrMakeVTableEntry(mi.Name));
                    return;
                } else if (ei.GetRemoveMethod() == mi) {
                    if (NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType), ei, mi, out name) == NameType.None) return;
                    CreateVTableEventOverride(mi, GetOrMakeVTableEntry(mi.Name));
                    return;
                }
            }

            OverrideBaseMethod(mi, specialNames);
        }

        /// <summary>
        /// Loads all the incoming arguments of cg and forwards them to mi which
        /// has the same signature and then returns the result
        /// </summary>
        private static void EmitBaseMethodDispatch(MethodInfo mi, Compiler cg) {
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

        private void OverrideBaseMethod(MethodInfo mi, Dictionary<string, List<string>> specialNames) {
            if ((!mi.IsVirtual || mi.IsFinal) && !mi.IsFamily) {
                return;
            }

            PythonType basePythonType;
            if (_baseType == mi.DeclaringType || _baseType.IsSubclassOf(mi.DeclaringType)) {
                basePythonType = DynamicHelpers.GetPythonTypeFromType(_baseType);
            } else {
                // We must be inherting from an interface
                Debug.Assert(mi.DeclaringType.IsInterface);
                basePythonType = DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType);
            }

            string name = null;
            if (NameConverter.TryGetName(basePythonType, mi, out name) == NameType.None)
                return;

            if (mi.DeclaringType == typeof(object) && mi.Name == "Finalize") return;

            List<string> names = new List<string>();
            names.Add(mi.Name);
            if (name != mi.Name) names.Add(name);
            specialNames[mi.Name] = names;
            
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

        private static void EmitBadCallThrow(Compiler cg, MethodInfo mi, string reason) {
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
        internal Slot EmitBaseClassCallCheckForProperties(Compiler cg, MethodInfo baseMethod, VTableEntry methField) {
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
            Compiler cg = _tg.DefineMethodOverride(mi);
            Slot callTarget = EmitBaseClassCallCheckForProperties(cg, mi, methField);

            callTarget.EmitGet(cg);
            cg.EmitThis();
            EmitSymbolId(cg, methField.name);
            cg.EmitCall(typeof(UserTypeOps), "GetPropertyHelper");

            cg.EmitReturnFromObject();
            cg.Finish();
        }

        private void CreateVTableSetterOverride(MethodInfo mi, VTableEntry methField) {
            Compiler cg = _tg.DefineMethodOverride(mi);
            Slot callTarget = EmitBaseClassCallCheckForProperties(cg, mi, methField);

            callTarget.EmitGet(cg);  // property
            cg.EmitThis();           // instance
            cg.EmitArgGet(0);
            cg.EmitBoxing(mi.GetParameters()[0].ParameterType);    // newValue
            EmitSymbolId(cg, methField.name);    // name
            cg.EmitCall(typeof(UserTypeOps), "SetPropertyHelper");

            cg.EmitReturn();
            cg.Finish();
        }

        private void CreateVTableEventOverride(MethodInfo mi, VTableEntry methField) {
            // override the add/remove method            
            Compiler cg = _tg.DefineMethodOverride(mi);

            Slot callTarget = EmitBaseClassCallCheckForProperties(cg, mi, methField);

            callTarget.EmitGet(cg);
            cg.EmitThis();
            _typeField.EmitGet(cg);
            cg.EmitArgGet(0);
            cg.EmitBoxing(mi.GetParameters()[0].ParameterType);
            EmitSymbolId(cg, methField.name);
            cg.EmitCall(typeof(UserTypeOps), "AddRemoveEventHelper");

            cg.EmitReturn();
            cg.Finish();
        }

        private void CreateVTableMethodOverride(MethodInfo mi, VTableEntry methField) {
            ParameterInfo[] parameters = mi.GetParameters();
            Compiler cg = (mi.IsVirtual && !mi.IsFinal) ? _tg.DefineMethodOverride(mi) : _tg.DefineMethod(
                mi.IsVirtual ? (mi.Attributes | MethodAttributes.NewSlot) : mi.Attributes,
                    mi.Name,
                    mi.ReturnType,
                    ReflectionUtils.GetParameterTypes(parameters),
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

            StubGenerator.EmitClrCallStub(cg, callTarget, argStart, attrs);

            cg.Finish();
        }

        public static Compiler CreateVirtualMethodHelper(TypeGen tg, MethodInfo mi) {
            ParameterInfo[] parms = mi.GetParameters();
            Type[] types = ReflectionUtils.GetParameterTypes(parms);
            string[] paramNames = new string[parms.Length];
            Type miType = mi.DeclaringType;
            for (int i = 0; i < types.Length; i++) {
                paramNames[i] = parms[i].Name;
                if (types[i] == miType) {
                    types[i] = tg.TypeBuilder;
                }
            }
            Compiler cg = tg.DefineMethod(MethodAttributes.Public | MethodAttributes.HideBySig,
                                         BaseMethodPrefix + mi.Name, mi.ReturnType, types, paramNames);

            EmitBaseMethodDispatch(mi, cg);
            cg.Finish();
            return cg;
        }

        private static void EmitSymbolId(Compiler cg, string name) {
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
