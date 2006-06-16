/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

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
    /// generated IL using "IronPythonConsole.exe -X:SaveAssemblies", and then inspect the
    /// persisted IL using ildasm.
    /// </summary>
    class NewTypeMaker {
        public const string VtableNamesField = "#VTableNames#";
        public const string TypePrefix = "IronPython.NewTypes.";
        private static Publisher<NewTypeInfo, Type> newTypes = new Publisher<NewTypeInfo, Type>();
        private static int typeCount = 0;

        protected Type baseType;
        protected IList<string> slots;
        protected TypeGen tg;
        protected Slot typeField, dictField, weakrefField;
        protected IEnumerable<Type> interfaceTypes;
        protected Tuple baseClasses;

        private bool hasBaseTypeField = false;
        private IList<Slot> slotsSlots;

        private Dictionary<string, VTableSlot> vtable = new Dictionary<string, VTableSlot>();

        public static Type GetNewType(string typeName, Tuple bases, IDictionary<object, object> dict) {
            // we're really only interested in the "correct" base type pulled out of bases
            // and any slot information contained in dict
            // other info might be used for future optimizations

            Debug.Assert(bases != null);
            NewTypeInfo typeInfo = GetTypeInfo(typeName, bases, GetSlots(dict));
            
            if (typeInfo.BaseType.IsSealed || typeInfo.BaseType.IsValueType)
                throw Ops.TypeError("cannot derive from sealed or value types");


            Type ret = newTypes.GetOrCreateValue(typeInfo,
                delegate() {
                    // creation code                    
                    return GetTypeMaker(bases, typeName, typeInfo).CreateNewType();
                });

            if (typeInfo.Slots != null) {
                // update dict w/ slots that point at the correct fields.

                for (int i = 0; i < typeInfo.Slots.Count; i++) {
                    PropertyInfo pi = ret.GetProperty(typeInfo.Slots[i]);
                    string name = typeInfo.Slots[i];
                    if(name.StartsWith("__") && !name.EndsWith("__")) {
                        name = "_" + typeName  + name;
                    }
                    dict[name] = new ReflectedSlotProperty(pi, pi.GetGetMethod(), pi.GetSetMethod(), NameType.PythonProperty);
                }
            }

            return ret;
        }

        private static NewTypeMaker GetTypeMaker(Tuple bases, string typeName, NewTypeInfo ti) {
            if (IsInstanceType(ti.BaseType)) return new NewSubtypeMaker(bases, typeName, ti);

            return new NewTypeMaker(bases, typeName, ti);
        }

        private static List<string> GetSlots(IDictionary<object, object> dict) {
            List<string> res = null;
            object slots;            
            IAttributesDictionary attrDict = dict as IAttributesDictionary;
            if (attrDict != null && attrDict.TryGetValue(SymbolTable.Slots, out slots)) {
                res = SlotsToList(slots);            
            }

            return res;
        }

        private static List<string> SlotsToList(object slots) {
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
            Conversion conv;
            string value = Converter.TryConvertToString(o, out conv);
            if (String.IsNullOrEmpty(value) || conv == Conversion.None) throw Ops.TypeError("slots must be one string or a list of strings");

            for (int i = 0; i < value.Length; i++) {
                if ((value[i] >= 'a' && value[i] <= 'z') ||
                    (value[i] >= 'A' && value[i] <= 'Z') ||
                    (i != 0 && value[i] >= '0' && value[i] <= '9') ||
                    value[i] == '_') {
                    continue;
                }
                throw Ops.TypeError("__slots__ must be valid identifiers");
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
            PythonType basePythonType = null;

            foreach (object curBaseType in bases) {
                PythonType curBasePythonType = curBaseType as PythonType;

                if (curBasePythonType == null) {
                    if (curBaseType is OldClass)
                        continue;
                    throw Ops.TypeError(typeName + ": unsupported base type for new-style class: " + baseCLIType);
                }

                IList<Type> baseInterfaces;
                Type curTypeToExtend = curBasePythonType.GetTypesToExtend(out baseInterfaces);

                if (curTypeToExtend == null || curTypeToExtend == typeof(BuiltinFunction))
                    throw Ops.TypeError(typeName + ": {0} is not an acceptable base type", curBasePythonType);
                if (curTypeToExtend.ContainsGenericParameters)
                    throw Ops.TypeError(typeName + ": cannot inhert from open generic instantiation {0}. Only closed instantiations are supported.", curBasePythonType);

                foreach (Type interfaceType in baseInterfaces) {
                    if (interfaceType.ContainsGenericParameters)
                        throw Ops.TypeError(typeName + ": cannot inhert from open generic instantiation {0}. Only closed instantiations are supported.", interfaceType);
                    interfaceTypes.Add(interfaceType);
                }

                if (curTypeToExtend != typeof(object)) {
                    if (baseCLIType != typeof(object) && baseCLIType != curTypeToExtend) {
                        bool isOkConflit = false;
                        if (IsInstanceType(baseCLIType) && IsInstanceType(curTypeToExtend)) {
                            List<string> slots1 = SlotsToList(curBasePythonType.dict[SymbolTable.Slots]);
                            List<string> slots2 = SlotsToList(basePythonType.dict[SymbolTable.Slots]);
                            if (curBasePythonType.type.BaseType == basePythonType.type.BaseType && 
                                slots1.Count == 1 && slots2.Count == 1 &&
                                ((slots1[0] == "__dict__" && slots2[0] == "__weakref__") ||
                                (slots2[0] == "__dict__" && slots1[0] == "__weakref__"))) {
                                isOkConflit = true;
                                curTypeToExtend = curBasePythonType.type.BaseType;
                            }
                        }
                        if(!isOkConflit) throw Ops.TypeError(typeName + ": can only extend one CLI or builtin type, not both {0} (for {1}) and {2} (for {3})",
                                            baseCLIType.FullName, basePythonType, curTypeToExtend.FullName, curBasePythonType);
                    }

                    baseCLIType = curTypeToExtend;
                    basePythonType = curBasePythonType;
                }

            }

            return new NewTypeInfo(baseCLIType, interfaceTypes, slots);
        }

        protected NewTypeMaker(Tuple baseClasses, String name, NewTypeInfo typeInfo) {
            this.baseType = typeInfo.BaseType;
            this.baseClasses = baseClasses;
            this.interfaceTypes = typeInfo.InterfaceTypes;
            this.slots = typeInfo.Slots;
        }

        private static string GetBaseName(MethodInfo mi, Dictionary<string,bool> specialNames){
            Debug.Assert(mi.Name.StartsWith("#base#"));

            string newName = mi.Name.Substring(6);

            Debug.Assert(specialNames.ContainsKey(newName));

            if (specialNames[newName] == true) {
                if(newName == "get_Item") return "__getitem__";
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
            StringBuilder name = new StringBuilder(baseType.FullName);
            foreach (Type interfaceType in interfaceTypes) {
                name.Append("#");
                name.Append(interfaceType.Name); 
            }

            name.Append("_");
            name.Append(System.Threading.Interlocked.Increment(ref typeCount));
            return name.ToString();

        }

        protected virtual void ImplementInterfaces() {
            foreach (Type interfaceType in interfaceTypes) {
                ImplementInterface(interfaceType);
            }
        }

        protected void ImplementInterface(Type interfaceType) {
            tg.myType.AddInterfaceImplementation(interfaceType);
        }

        private Type CreateNewType() {
            AssemblyGen ag = OutputGenerator.Snippets;

            string name = GetName();
            tg = ag.DefinePublicType(TypePrefix + name, baseType);

            ImplementInterfaces(); 

            GetOrDefineClass();

            GetOrDefineDict();

            ImplementSlots();

            ImplementPythonObject();

            ImplementConstructors();

            Dictionary<string, bool> specialNames = new Dictionary<string, bool>();

            OverrideVirtualMethods(baseType, specialNames);

            Dictionary<Type, bool> doneTypes = new Dictionary<Type, bool>();
            foreach (Type interfaceType in interfaceTypes) {                    
                DoInterfaceType(interfaceType, doneTypes, specialNames);
            }

            InitializeVTableStrings();

            // Hashtable slots = collectSlots(dict, tg);
            // if (slots != null) tg.createAttrMethods(slots);

            Type ret = tg.FinishType();

            AddBaseMethods(ret, specialNames);

            return ret;
        }

        protected virtual void ImplementPythonObject() {
            ImplementDynamicObject();

            ImplementCustomTypeDescriptor();

            ImplementPythonEquals();

            ImplementPythonComparable();

            ImplementWeakReference();
        }

        private void GetOrDefineDict() {
            FieldInfo baseDictField = baseType.GetField("dict");
            if (baseDictField == null) {
                dictField = tg.AddField(typeof(IAttributesDictionary), "__dict__");
            } else {
                dictField = new FieldSlot(new ThisSlot(tg.myType), baseDictField);
            }
        }

        private void GetOrDefineClass() {
            FieldInfo baseTypeField = baseType.GetField("__class__");
            if (baseTypeField == null) {
                typeField = tg.AddField(typeof(UserType), "__class__");
            } else {
                Debug.Assert(baseTypeField.FieldType == typeof(UserType));
                typeField = new FieldSlot(new ThisSlot(tg.myType), baseTypeField);
                hasBaseTypeField = true;
            }
        }

        protected virtual ParameterInfo[] GetOverrideCtorSignature(ParameterInfo[] original) {
            ParameterInfo[] argTypes = new ParameterInfo[original.Length + 1];

            argTypes[0] = new TrackingParamInfo(typeField.Type, "cls");
            Array.Copy(original, 0, argTypes, 1, argTypes.Length - 1);
            return argTypes;
        }

        private void ImplementConstructors() {
            ConstructorInfo[] constructors;
            constructors = baseType.GetConstructors(BindingFlags.Public |
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

        private void AddBaseMethods(Type finishedType, Dictionary<string,bool> specialNames) {            
            // "Adds" base methods to super type (should really add to the derived type)
            // this makes super(...).xyz to work - otherwise we'd return a function that
            // did a virtual call resulting in a stack overflow.
            ReflectedType rt = Ops.GetDynamicTypeFromType(baseType) as ReflectedType;
            rt.Initialize();

            foreach (MethodInfo mi in finishedType.GetMethods()) {
                if (!ShouldOverrideVirtual(mi)) continue;

                string methodName = mi.Name;
                if (methodName.StartsWith("#base#")) {
                    string newName = GetBaseName(mi, specialNames);
                    rt.StoreReflectedBaseMethod(newName, mi, NameType.Method);                  
                }
            }
        }

        private void DoInterfaceType(Type interfaceType, Dictionary<Type, bool> doneTypes, Dictionary<string,bool> specialNames) {
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

            ConstructorBuilder cb = tg.myType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, argTypes);

            for (int i = 0; i < overrideParams.Length; i++) {
                ParameterBuilder pb = cb.DefineParameter(i + 1, 
                    overrideParams[i].Attributes, 
                    overrideParams[i].Name);

                int origIndex = i - (overrideParams.Length - pis.Length);
                if (origIndex >= 0) {
                    if (pis[origIndex].IsDefined(typeof(ParamArrayAttribute), false)) {
                        pb.SetCustomAttribute(new CustomAttributeBuilder(
                            typeof(ParamArrayAttribute).GetConstructor(Type.EmptyTypes), new object[0]));
                    } else if (pis[origIndex].IsDefined(typeof(ParamDictAttribute), false)) {
                        pb.SetCustomAttribute(new CustomAttributeBuilder(
                            typeof(ParamDictAttribute).GetConstructor(Type.EmptyTypes), new object[0]));
                    }
                }
            }

            CodeGen cg = new CodeGen(tg, cb, cb.GetILGenerator(), argTypes);

            // <typeField> = <arg0>
            cg.EmitArgGet(0);
            typeField.EmitSet(cg);

            // initialize all slots to Uninitialized
            if (slots != null) {
                for (int i = 0; i < slots.Count; i++) {
                    if (slots[i] != "__weakref__" && slots[i] != "__dict__") {
                        cg.EmitString(slots[i]);
                        cg.EmitNew(typeof(Uninitialized), new Type[] { typeof(string) });
                    } else {
                        cg.Emit(OpCodes.Ldnull);
                    }
                    slotsSlots[i].EmitSet(cg);
                }
            }

            CallBaseConstructor(parentConstructor, overrideParams.Length - pis.Length, overrideParams, cg);            
            return cb;
        }

        private static void CallBaseConstructor(ConstructorInfo parentConstructor, int offset, ParameterInfo[] pis, CodeGen cg) {
            cg.EmitThis();

            for (int i = offset; i < pis.Length; i++) {
                cg.EmitArgGet(i);
            }
            cg.Emit(OpCodes.Call, parentConstructor);
            cg.Emit(OpCodes.Ret);
        }

        private void InitializeVTableStrings() {
            string[] names = new string[vtable.Count];
            foreach (VTableSlot slot in vtable.Values) {
                names[slot.index] = slot.name;
            }

            Slot namesField = tg.AddStaticField(typeof(string[]), VtableNamesField);

            CodeGen cg = tg.GetOrMakeInitializer();
            cg.EmitStringArray(names);
            namesField.EmitSet(cg);
        }

        private void ImplementCustomTypeDescriptor() {
            tg.myType.AddInterfaceImplementation(typeof(ICustomTypeDescriptor));

            foreach (MethodInfo m in typeof(ICustomTypeDescriptor).GetMethods()) {
                ImplementCTDOverride(m);
            }
        }

        private void ImplementCTDOverride(MethodInfo m) {
            CodeGen cg = tg.DefineExplicitInterfaceImplementation(m);
            cg.EmitThis();

            ParameterInfo[] pis = m.GetParameters();
            Type[] paramTypes = new Type[pis.Length + 1];
            paramTypes[0] = typeof(object);
            for (int i = 0; i < pis.Length; i++) {
                cg.EmitArgGet(i);
                paramTypes[i + 1] = pis[i].ParameterType;
            }

            cg.EmitCall(typeof(CustomTypeDescHelpers), m.Name, paramTypes);
            cg.EmitCastToObject(m.ReturnType);
            cg.EmitReturn();
            cg.Finish();
        }

        private bool NeedsDictionary {
            get{
                if (slots == null) return true;
                if (slots.Contains("__dict__")) return true;

                foreach(DynamicType dt in baseClasses){
                    if(dt is UserType) return true;
                }

                return false;
            }
        }

        private void ImplementDynamicObject() {
            CodeGen cg;

            tg.myType.AddInterfaceImplementation(typeof(ISuperDynamicObject));

            MethodAttributes attrs = (MethodAttributes)0;            
            if (slots != null) attrs = MethodAttributes.Virtual;
            
            cg = tg.DefineMethodOverride(attrs, typeof(ISuperDynamicObject).GetMethod("GetDict"));
            if (NeedsDictionary) {
                dictField.EmitGet(cg);
                cg.EmitReturn();
            } else {
                cg.Emit(OpCodes.Ldnull);
                cg.EmitReturn();
            }
            cg.Finish();

            cg = tg.DefineMethodOverride(attrs, typeof(ISuperDynamicObject).GetMethod("SetDict"));
            if (NeedsDictionary) {
                cg.EmitArgGet(0);
                dictField.EmitSet(cg);
                cg.EmitRawConstant(true);
                cg.EmitReturn();
            } else {
                cg.EmitRawConstant(false);
                cg.EmitReturn();
            }
            cg.Finish();

            if (hasBaseTypeField) return;

            cg = tg.DefineMethodOverride(attrs, typeof(IDynamicObject).GetMethod("GetDynamicType"));
            typeField.EmitGet(cg);
            cg.EmitReturn();
            cg.Finish();

            cg = tg.DefineMethodOverride(attrs, typeof(ISuperDynamicObject).GetMethod("SetDynamicType"));
            cg.EmitArgGet(0);
            typeField.EmitSet(cg);
            cg.EmitReturn();
            cg.Finish();
        }

        private static void EmitNoDict(CodeGen cg) {
            // can't set __dict__ on class w/ __slots__
            cg.EmitString("{0} object has no attribute '__dict__'");
            cg.EmitObjectArray(1, delegate(int index) {
                cg.EmitThis();
                cg.EmitCall(typeof(Ops), "GetDynamicType");
                cg.EmitCall(typeof(Ops), "StringRepr");
            });
            cg.EmitCall(typeof(Ops), "AttributeError");
            cg.Emit(OpCodes.Throw);
        }

        /// <summary>
        /// Defines an interface on the type that forwards all calls
        /// to a helper method in UserType.  The method names all will
        /// have Helper appended to them to get the name for UserType.  The 
        /// UserType version should take 1 extra parameter (self).
        /// </summary>
        /// <param name="intf"></param>
        private void DefineHelperInterface(Type intf, bool fExplicit) {
            tg.myType.AddInterfaceImplementation(intf);
            MethodInfo[] mis = intf.GetMethods();

            foreach (MethodInfo mi in mis) {

                CodeGen cg = fExplicit ? tg.DefineExplicitInterfaceImplementation(mi) : tg.DefineMethodOverride(mi);
                cg.EmitThis();
                ParameterInfo[] pis = mi.GetParameters();
                for (int i = 0; i < pis.Length; i++) {
                    cg.EmitArgGet(i);
                }
                cg.EmitCall(typeof(UserType).GetMethod(mi.Name + "Helper"));
                cg.EmitReturn();
                cg.Finish();
            }
        }

        private void ImplementPythonEquals() {
            if (this.baseType.GetInterface("IRichEquality") == null) {
                DefineHelperInterface(typeof(IRichEquality), false);
            }
        }

        private void ImplementPythonComparable() {
            if (this.baseType.GetInterface("IRichComparable") == null) {
                DefineHelperInterface(typeof(IRichComparable), false);
            }
        }

        private void CreateWeakRefField() {
            if (weakrefField != null) return;

            FieldInfo fi = baseType.GetField("__weakref__");
            if (fi != null) {
                // base defines it
                weakrefField = new FieldSlot(new ThisSlot(baseType), fi);
            }

            if (weakrefField == null) {
                weakrefField = tg.AddField(typeof(WeakRefTracker), "__weakref__");
            }
        }

        internal bool BaseHasWeakRef(DynamicType curType) {
            UserType ut = curType as UserType;
            if (ut != null && ut.HasSlots && ut.HasWeakRef) {
                return true;
            }

            foreach (DynamicType baseType in curType.BaseClasses) {
                if (BaseHasWeakRef(baseType)) return true;
            }
            return false;
        }
        protected virtual void ImplementWeakReference() {
            CreateWeakRefField();

            bool isWeakRefAble = true;
            if (slots != null && !slots.Contains("__weakref__")) {
                // always define the field, only implement the interface
                // if we are slotless or the user defined __weakref__ in slots
                bool baseHasWeakRef = false;
                foreach (DynamicType dt in baseClasses) {
                    if (BaseHasWeakRef(dt)) {
                        baseHasWeakRef = true;
                        break;
                    }
                }
                if (baseHasWeakRef) return;

                isWeakRefAble = false;
            } 

            tg.myType.AddInterfaceImplementation(typeof(IWeakReferenceable));

            CodeGen cg = tg.DefineMethodOverride(typeof(IWeakReferenceable).GetMethod("SetWeakRef"));
            if (!isWeakRefAble) {
                cg.EmitRawConstant(false);
                cg.EmitReturn();
            } else {
                cg.EmitArgGet(0);
                weakrefField.EmitSet(cg);
                cg.EmitRawConstant(true);
                cg.EmitReturn();
            }
            cg.Finish();

            cg = tg.DefineMethodOverride(typeof(IWeakReferenceable).GetMethod("SetFinalizer"));
            cg.EmitArgGet(0);
            weakrefField.EmitSet(cg);
            cg.EmitReturn();
            cg.Finish();

            cg = tg.DefineMethodOverride(typeof(IWeakReferenceable).GetMethod("GetWeakRef"));
            weakrefField.EmitGet(cg);
            cg.EmitReturn();
            cg.Finish();
        }

        private void ImplementSlots() {
            if (slots != null) {
                slotsSlots = new List<Slot>();
                for (int i = 0; i < slots.Count; i++) {
                    Slot s;
                    switch(slots[i]){
                        case "__weakref__": CreateWeakRefField(); s = weakrefField; break;
                        case "__dict__": s = dictField; break;
                        default:
                            // mangled w/ a . to never collide w/ any regular names
                            s = tg.AddField(typeof(object), "." + slots[i]);
                            break;
                    }
                    
                    slotsSlots.Add(s);

                    PropertyBuilder pb = tg.DefineProperty(slots[i], PropertyAttributes.None, s.Type);

                    CodeGen getter = tg.DefineMethod(MethodAttributes.Public, 
                        "get_" + slots[i], 
                        s.Type, 
                        new Type[0], 
                        new string[0]);
                    s.EmitGet(getter);
                    getter.Emit(OpCodes.Dup);
                    getter.EmitThis();
                    getter.EmitCall(typeof(Ops), "CheckInitializedAttribute");                        
                    getter.EmitReturn();                    
                    getter.Finish();

                    CodeGen setter = tg.DefineMethod(MethodAttributes.Public, 
                        "set_" + slots[i], 
                        typeof(void), 
                        new Type[] { s.Type }, 
                        new string[] { "value" });
                    
                    setter.EmitArgGet(0);
                    s.EmitSet(setter);
                    setter.EmitReturn();

                    setter.Finish();

                    pb.SetGetMethod(getter.MethodInfo as MethodBuilder);
                    pb.SetSetMethod(setter.MethodInfo as MethodBuilder);
                }
            }
        }

        private void OverrideVirtualMethods(Type type, Dictionary<string, bool> specialNames) {
            foreach (MethodInfo mi in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                if (!ShouldOverrideVirtual(mi)) continue;

                if (mi.IsPublic || mi.IsFamily || mi.IsFamilyOrAssembly) {
                    if (mi.IsSpecialName) {
                        OverrideSpecialName(mi, specialNames);
                    } else {
                        OverrideBaseMethod(mi, specialNames);
                    }
                }
            }
        }

        private static string[] SkipMethodNames = new string[] { "GetDynamicType", };

        private void OverrideSpecialName(MethodInfo mi, Dictionary<string, bool> specialNames) {
            if (mi == null || !mi.IsVirtual || mi.IsFinal) return;
            
            string name;
            PropertyInfo[] pis = mi.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            specialNames[mi.Name] = true;
            foreach (PropertyInfo pi in pis) {
                if (pi.GetIndexParameters().Length > 0) {
                    if (mi == pi.GetGetMethod(true)) {                        
                        Slot methField = GetOrMakeField("__getitem__");
                        CreateVirtualMethodOverride(mi, methField);
                        if (!mi.IsAbstract) CreateVirtualMethodHelper(tg, mi);
                        break;
                    } else if (mi == pi.GetSetMethod(true)) {
                        Slot methField = GetOrMakeField("__setitem__");
                        CreateVirtualMethodOverride(mi, methField);
                        if (!mi.IsAbstract) CreateVirtualMethodHelper(tg, mi);
                        break;
                    }
                }else  if (mi == pi.GetGetMethod(true)) {
                    if (NameConverter.TryGetName(Ops.GetDynamicTypeFromType(mi.DeclaringType), pi, mi, out name) == NameType.None) return;
                    Slot methField = GetOrMakeField(name);
                    CreateVTableGetterOverride(tg, mi, methField as VTableSlot);
                    break;
                } else if (mi == pi.GetSetMethod(true)) {
                    if (NameConverter.TryGetName(Ops.GetDynamicTypeFromType(mi.DeclaringType), pi, mi, out name) == NameType.None) return;
                    Slot methField = GetOrMakeField(name);
                    CreateVTableSetterOverride(tg, mi, methField as VTableSlot);
                    break;
                } 
            }
        }

        // Loads all the incoming arguments of cg and forwards them to mi which
        // has the same signature.
        // The return value (if any) is left on the IL stack.
        private static void EmitBaseMethodDispatch(MethodInfo mi, CodeGen cg) {
            cg.EmitThis();
            for (int i = 0; i < mi.GetParameters().Length; i++)
                cg.EmitArgGet(i);
            cg.EmitCall(OpCodes.Call, mi, null); // base call must be non-virtual
        }

        private void OverrideBaseMethod(MethodInfo mi, Dictionary<string, bool> specialNames) {
            if (mi == null ||
                !mi.IsVirtual ||
                mi.IsFinal ||
                Array.IndexOf(SkipMethodNames, mi.Name) != -1) {
                return;
            }

            DynamicType baseDynamicType;
            if (baseType == mi.DeclaringType || baseType.IsSubclassOf(mi.DeclaringType)) {
                baseDynamicType = Ops.GetDynamicTypeFromType(baseType);
            } else {
                // We must be inherting from an interface
                Debug.Assert(mi.DeclaringType.IsInterface);
                baseDynamicType = Ops.GetDynamicTypeFromType(mi.DeclaringType);
            }

            string name = null;
            if (NameConverter.TryGetName(baseDynamicType, mi, out name) == NameType.None)
                return;

            if (mi.DeclaringType == typeof(object) && mi.Name == "Finalize") return;

            specialNames[mi.Name] = false;
            Slot methField = GetOrMakeField(name);

            CreateVirtualMethodOverride(mi, methField);
            if (!mi.IsAbstract) CreateVirtualMethodHelper(tg, mi);
        }

        private Slot GetVTableSlot(string name) {
            VTableSlot ret;
            if (vtable.TryGetValue(name, out ret)) return ret;

            ret = new VTableSlot(typeField, name, vtable.Count);
            vtable[name] = ret;
            return ret;
        }

        public Slot GetOrMakeField(string name) {
            name = NormalizeName(name);
            FieldInfo fi = typeof(PythonType).GetField(name + "F");
            if (fi != null) return new FieldSlot(typeField, fi);

            //return null;

            return GetVTableSlot(name);
        }
        private void CreateVTableMethodOverrideSimple(CodeGen cg, MethodInfo mi, VTableSlot methField) {
            cg.EmitThis();

            for (int i = 0; i < mi.GetParameters().Length; i++) {
                cg.EmitArgGet(i);
                cg.EmitCastToObject(mi.GetParameters()[i].ParameterType);
            }
            cg.EmitCall(typeof(Ops), "InvokeMethod", CompilerHelpers.MakeRepeatedArray(typeof(object), 3 + mi.GetParameters().Length));
        }
        private void CreateVTableMethodOverrideComplex(CodeGen cg, MethodInfo mi, VTableSlot methField) {
            cg.EmitInt(mi.GetParameters().Length + 1);
            cg.Emit(OpCodes.Newarr, typeof(object));

            cg.Emit(OpCodes.Dup);
            cg.EmitInt(0);
            cg.EmitThis();
            cg.Emit(OpCodes.Stelem_Ref);

            for (int i = 0; i < mi.GetParameters().Length; i++) {
                cg.Emit(OpCodes.Dup);
                cg.EmitInt(i + 1);
                cg.EmitArgGet(i);
                cg.EmitCastToObject(mi.GetParameters()[i].ParameterType);
                cg.Emit(OpCodes.Stelem_Ref);
            }

            cg.EmitCall(typeof(Ops), "InvokeMethod", new Type[] { typeof(object), typeof(object), typeof(object[]) });
        }

        private static void CreateVTableMethodOverrideParams(CodeGen cg, MethodInfo mi, VTableSlot methField, int paramsIndex) {
            cg.EmitArgGet(paramsIndex);
            cg.Emit(OpCodes.Ldlen);
            cg.EmitInt(mi.GetParameters().Length);
            cg.Emit(OpCodes.Add);
            Slot destArray = cg.GetLocalTmp(typeof(object[]));
            cg.Emit(OpCodes.Newarr, typeof(object));
            cg.Emit(OpCodes.Dup);
            destArray.EmitSet(cg);


            cg.EmitInt(0);
            cg.EmitThis();
            cg.Emit(OpCodes.Stelem_Ref);
            Slot tmp = cg.GetLocalTmp(typeof(int));
            cg.EmitInt(1);
            tmp.EmitSet(cg);

            for (int i = 0; i < mi.GetParameters().Length; i++) {
                if (i != paramsIndex) {
                    // non params argument, just stick it in our array...
                    destArray.EmitGet(cg);
                    tmp.EmitGet(cg);
                    cg.EmitArgGet(i);
                    cg.EmitCastToObject(mi.GetParameters()[i].ParameterType);
                    cg.Emit(OpCodes.Stelem_Ref);

                    // increment the write counter
                    tmp.EmitGet(cg);
                    cg.EmitInt(1);
                    cg.Emit(OpCodes.Add);
                    tmp.EmitSet(cg);
                } else {
                    // params index, copy contents of array.
                    cg.EmitArgGet(paramsIndex);
                    cg.EmitInt(0);
                    destArray.EmitGet(cg);
                    tmp.EmitGet(cg);
                    cg.EmitArgGet(paramsIndex);
                    cg.Emit(OpCodes.Ldlen);

                    cg.EmitCall(typeof(Array), "Copy", new Type[] { typeof(Array), typeof(int), typeof(Array), typeof(int), typeof(int) });

                    // update the write counter
                    tmp.EmitGet(cg);
                    cg.EmitArgGet(paramsIndex);
                    cg.Emit(OpCodes.Ldlen);
                    cg.Emit(OpCodes.Add);
                    tmp.EmitSet(cg);
                }
            }

            destArray.EmitGet(cg);
            cg.EmitCall(typeof(Ops), "InvokeMethod", new Type[] { typeof(object), typeof(object), typeof(object[]) });
        }

        private static bool CanOverride(MethodInfo mi, out int paramsIndex) {
            bool fCanOverride = true;
            paramsIndex = -1;

            ParameterInfo[] pis = mi.GetParameters();
            for (int i = 0; i < pis.Length; i++) {
                if (pis[i].ParameterType.IsByRef) {
                    fCanOverride = false;
                } else {
                    object[] paramArr = pis[i].GetCustomAttributes(typeof(ParamArrayAttribute), false);
                    if (paramArr != null && paramArr.Length > 0) {
                        paramsIndex = i;
                    }
                }
            }

            return fCanOverride;
        }

        private static void EmitBadCallThrow(CodeGen cg, MethodInfo mi, string reason) {
            cg.EmitString("Cannot override method from IronPython {0} because " + reason);
            cg.EmitInt(1);
            cg.Emit(OpCodes.Newarr, typeof(object));
            cg.Emit(OpCodes.Dup);
            cg.EmitInt(0);
            cg.EmitString(mi.Name);
            cg.Emit(OpCodes.Stelem_Ref);
            cg.EmitCall(typeof(Ops), "TypeError");
            cg.Emit(OpCodes.Throw);
        }

        /// <summary>
        /// Emits code to check if the instance has overriden this specific
        /// function from the base class.  For example:
        /// 
        /// a = MyDerivedType()
        /// a.SomeVirtualFunction = myFunction
        /// </summary>
        private void EmitInstanceCallCheck(CodeGen cg, MethodInfo mi, VTableSlot methField) {
            Slot tmp = cg.GetLocalTmp(typeof(object));
            Label notThere = cg.DefineLabel(), done = cg.DefineLabel();

            // first, see if we have a dict (usually we shouldn't)            
            dictField.EmitGet(cg);
            cg.Emit(OpCodes.Dup);
            cg.Emit(OpCodes.Ldnull);
            cg.Emit(OpCodes.Beq, notThere);

            // then see if the dict contains our entry (usually it won't)...
            cg.EmitSymbolId(methField.name);
            tmp.EmitGetAddr(cg);
            cg.EmitCall(typeof(IAttributesDictionary), "TryGetValue");
            cg.Emit(OpCodes.Brfalse, done);
            
            // finally dispatch the call to function if it's there.

            // load the function
            tmp.EmitGet(cg);

            // allocate an array for the parameters
            ParameterInfo[] pis = mi.GetParameters();
            cg.EmitInt(pis.Length);
            cg.Emit(OpCodes.Newarr, typeof(object));
                        
            // the instance should be a bound parameter, so we
            // don't pass it in here.
            
            // store args...            
            for (int i = 0; i < pis.Length; i++) {
                cg.Emit(OpCodes.Dup);
                cg.EmitInt(i);
                cg.EmitArgGet(i);
                if (pis[i].IsOut || pis[i].ParameterType.IsByRef) {
                    cg.EmitLoadValueIndirect(pis[i].ParameterType.GetElementType());
                    cg.EmitCastToObject(pis[i].ParameterType.GetElementType());
                } else {
                    cg.EmitCastToObject(pis[i].ParameterType);
                }
                cg.Emit(OpCodes.Stelem_Ref);
            }

            // finally emit the call to Ops
            cg.EmitCall(typeof(Ops), "Call", new Type[] { typeof(object), typeof(object[]) });

            cg.EmitCastFromObject(mi.ReturnType);

            // and return whatever it returns.
            cg.Emit(OpCodes.Ret);

            // we have no dict
            cg.MarkLabel(notThere);
            cg.Emit(OpCodes.Pop);

            // we have a dict, but no overload.
            cg.MarkLabel(done);

            cg.FreeLocalTmp(tmp);
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
        internal static Label EmitBaseClassCallCheck(CodeGen cg, VTableSlot methField) {
            Label baseCall = cg.DefineLabel();
            Slot resultOut = cg.GetLocalTmp(typeof(object));

            methField.EmitTryGetNonInheritedValue(cg, resultOut);
            cg.Emit(OpCodes.Brfalse, baseCall);

            resultOut.EmitGet(cg);

            return baseCall;
        }

        internal static void EmitBaseClassCall(CodeGen cg, MethodInfo mi) {
            if (!mi.IsAbstract) {
                cg.EmitThis();
                for (int i = 0; i < mi.GetParameters().Length; i++) cg.EmitArgGet(i);
                cg.EmitCall(OpCodes.Call, mi, null); // base call must be non-virtual
                cg.EmitReturn();
            } else {
                cg.EmitThis();
                cg.EmitString(mi.Name);
                cg.EmitCall(typeof(Ops), "MissingInvokeMethodException");
                cg.Emit(OpCodes.Throw);
            }
        }

        private static void CreateVTableGetterOverride(TypeGen tg, MethodInfo mi, VTableSlot methField) {
            CodeGen cg = tg.DefineMethodOverride(mi);
            Label baseCall = EmitBaseClassCallCheck(cg, methField);

            int paramsIndex;
            bool fCanOverride = CanOverride(mi, out paramsIndex);

            if (fCanOverride) {
                // check that the value we pulled out is a
                // descriptor and call __get__ on it for our
                // instance, or throw if the user assigned it to something else.
                cg.Emit(OpCodes.Isinst, typeof(IDescriptor));
                cg.Emit(OpCodes.Dup);
                cg.EmitPythonNone();
                cg.Emit(OpCodes.Ceq);
                Label notProp = cg.DefineLabel();

                cg.Emit(OpCodes.Brtrue, notProp);

                cg.EmitThis();
                cg.EmitPythonNone();
                cg.EmitCall(typeof(IDescriptor), "GetAttribute");

                cg.EmitCastFromObject(mi.ReturnType);

                cg.EmitReturn();

                cg.MarkLabel(notProp);

                EmitBadCallThrow(cg, mi, "is not a property");
            } else {
                EmitBadCallThrow(cg, mi, "it includes a by-ref parameter");
            }

            cg.MarkLabel(baseCall);
            EmitBaseClassCall(cg, mi);

            cg.Finish();
        }

        private static void CreateVTableSetterOverride(TypeGen tg, MethodInfo mi, VTableSlot methField) {
            CodeGen cg = tg.DefineMethodOverride(mi);
            Label baseCall = EmitBaseClassCallCheck(cg, methField);

            int paramsIndex;
            bool fCanOverride = CanOverride(mi, out paramsIndex);

            if (fCanOverride) {
                cg.Emit(OpCodes.Isinst, typeof(IDataDescriptor));
                cg.Emit(OpCodes.Dup);
                cg.EmitPythonNone();
                cg.Emit(OpCodes.Ceq);
                Label notProp = cg.DefineLabel();

                cg.Emit(OpCodes.Brtrue, notProp);

                cg.EmitThis();
                cg.EmitArgGet(0);
                cg.EmitCastToObject(mi.GetParameters()[0].ParameterType);
                cg.EmitCall(typeof(IDataDescriptor), "SetAttribute");
                cg.Emit(OpCodes.Pop);

                cg.EmitReturn();

                cg.MarkLabel(notProp);

                EmitBadCallThrow(cg, mi, "is not a property");
            } else {
                EmitBadCallThrow(cg, mi, "it includes a by-ref parameter");
            }

            cg.MarkLabel(baseCall);
            EmitBaseClassCall(cg, mi);

            cg.Finish();

        }

        private void CreateVTableMethodOverride(MethodInfo mi, VTableSlot methField) {
            CodeGen cg = tg.DefineMethodOverride(mi);

            EmitInstanceCallCheck(cg, mi, methField);

            Label baseCall = EmitBaseClassCallCheck(cg, methField);

            typeField.EmitGet(cg);  // for InvokeMethod call, 2nd param (EmitCaseClassCallCheck emits method)

            int paramsIndex;
            bool fCanOverride = CanOverride(mi, out paramsIndex);

            if (fCanOverride) {
                if (paramsIndex != -1) {
                    CreateVTableMethodOverrideParams(cg, mi, methField, paramsIndex);
                } else if (mi.GetParameters().Length > 2) {
                    CreateVTableMethodOverrideComplex(cg, mi, methField);
                } else {
                    CreateVTableMethodOverrideSimple(cg, mi, methField);
                }
                cg.EmitCastFromObject(mi.ReturnType);

                cg.EmitReturn();
            } else {
                EmitBadCallThrow(cg, mi, "it includes a by-ref parameter");
            }

            cg.MarkLabel(baseCall);
            EmitBaseClassCall(cg, mi);

            cg.Finish();
        }

        private void CreateVirtualMethodOverride(MethodInfo mi, Slot methField) {
            if (methField is VTableSlot) {
                CreateVTableMethodOverride(mi, (VTableSlot)methField);
                return;
            }

            CodeGen cg = tg.DefineMethodOverride(mi);
            Label baseCall = cg.DefineLabel();

            methField.EmitGet(cg);
            cg.EmitCall(typeof(MethodWrapper), "IsBuiltinMethod");
            cg.Emit(OpCodes.Brtrue, baseCall);

            methField.EmitGet(cg);
            cg.EmitThis();
            for (int i = 0; i < mi.GetParameters().Length; i++) {
                cg.EmitArgGet(i);
                cg.EmitCastToObject(mi.GetParameters()[i].ParameterType);
            }

            if (mi.GetParameters().Length > 3) {
                throw new NotImplementedException("OverrideBaseMethod: " + mi);
            }
            //Console.WriteLine("Invoke: " + mi + ", " + mi.GetParameters().Length);
            cg.EmitCall(typeof(MethodWrapper), "Invoke", CompilerHelpers.MakeRepeatedArray(typeof(object), 1 + mi.GetParameters().Length));
            MethodInfo object_ToString = typeof(object).GetMethod("ToString");

            if (mi.MethodHandle != object_ToString.MethodHandle) {
                cg.EmitCastFromObject(mi.ReturnType);
                cg.EmitReturn();
            } else {
                EmitReturnNonNullString(cg);
            }

            cg.MarkLabel(baseCall);

            if (mi.MethodHandle == object_ToString.MethodHandle) {
                // object.ToString() displays the CLI type name. However, __class__ is the real type for Python type instances
                cg.EmitThis();
                cg.EmitCall(typeof(UserType).GetMethod("ToStringHelper"));
                cg.EmitReturn();
            } else if (mi.IsAbstract) {
                EmitBadCallThrow(cg, mi, "must override abstract method"); //@todo better exception
            } else {
                // Just forward to the base method implementation
                EmitBaseMethodDispatch(mi, cg);
                cg.EmitReturn();
            }
                        
            cg.Finish();
        }

        private static void EmitReturnNonNullString(CodeGen cg) {
            // need to check for null for ToString
            Label badRet = cg.DefineLabel();
            cg.Emit(OpCodes.Dup);
            cg.Emit(OpCodes.Ldnull);
            cg.Emit(OpCodes.Beq, badRet);

            cg.Emit(OpCodes.Dup);
            Slot s = cg.GetLocalTmp(typeof(Conversion));
            cg.EmitTryCastFromObject(typeof(string), s);

            Slot convertedVal = cg.GetLocalTmp(typeof(string));
            convertedVal.EmitSet(cg);

            s.EmitGet(cg);
            cg.EmitInt((int)Conversion.None);
            cg.Emit(OpCodes.Beq, badRet);
            cg.Emit(OpCodes.Pop);   // remove unconverted value
            convertedVal.EmitGet(cg);
            cg.EmitReturn();    // value's good, return it

            cg.FreeLocalTmp(s);
            cg.FreeLocalTmp(convertedVal);

            cg.MarkLabel(badRet);
            // stack contains only unconverted value
            Slot tmp = cg.GetLocalTmp(typeof(object));
            tmp.EmitSet(cg);

            cg.EmitString("__str__ returned non-string type ({0})");
            cg.EmitObjectArray(1, delegate(int index) {
                tmp.EmitGet(cg);
                cg.EmitCall(typeof(Ops), "GetDynamicType");
            });
            cg.EmitCall(typeof(Ops), "TypeError");
            cg.Emit(OpCodes.Throw);

            cg.FreeLocalTmp(tmp);
        }

        internal static CodeGen CreateVirtualMethodHelper(TypeGen tg, MethodInfo mi) {
            ParameterInfo[] parms = mi.GetParameters();
            Type[] types = CompilerHelpers.GetTypes(parms);
            string[] paramNames = new string[parms.Length];
            Type miType = mi.DeclaringType;
            for (int i = 0; i < types.Length; i++) {
                paramNames[i] = parms[i].Name;
                if (types[i] == miType) {
                    types[i] = tg.myType;
                }
            }
            CodeGen cg = tg.DefineMethod(MethodAttributes.Public | MethodAttributes.HideBySig,
                                         "#base#" + mi.Name, mi.ReturnType, types, paramNames);

            EmitBaseMethodDispatch(mi, cg);
            cg.EmitReturn();
            cg.Finish();
            return cg;
        }
    }
}
