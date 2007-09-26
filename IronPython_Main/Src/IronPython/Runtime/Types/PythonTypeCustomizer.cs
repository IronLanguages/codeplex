/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;

using IronPython.Compiler;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    class PythonTypeCustomizer : CoreReflectedTypeBuilder {
        private static DocumentationDescriptor _docDescr = new DocumentationDescriptor();
        private static MethodInfo[] _equalsHelper, _notEqualsHelper;
        private static Dictionary<Type, string> _sysTypes = MakeSystemTypes();
        private static Dictionary<DynamicType, Type> _inited = new Dictionary<DynamicType, Type>();

        private PythonTypeCustomizer(DynamicTypeBuilder builder) {
            Builder = builder;
        }

        public static void OnTypeInit(object sender, TypeCreatedEventArgs e) {
            DynamicType dt = e.Type;
            if (!dt.IsSystemType) return;
            lock (_inited) {
                if (_inited.ContainsKey(dt) || ExtensionTypeAttribute.IsExtensionType(dt.UnderlyingSystemType)) {
                    return;
                }

                _inited[dt] = dt.UnderlyingSystemType;

#if DEBUG
                Dictionary<Type, Type> assertDict = new Dictionary<Type,Type>();
                foreach (KeyValuePair<DynamicType, Type> types in _inited) {
                    Debug.Assert(!assertDict.ContainsKey(types.Value));

                    assertDict[types.Value] = types.Value;
                }
#endif
            }

            PythonTypeCustomizer customizer = new PythonTypeCustomizer(DynamicTypeBuilder.GetBuilder(dt));

            customizer.AddInitCode();
            customizer.AddDocumentation();
            customizer.AddOperators();
            customizer.AddPythonProtocolMethods();
            customizer.AddRichEqualityProtocols();
            customizer.AddToStringProtocols();
            if (ScriptDomainManager.Options.PrivateBinding) {
                customizer.AddPrivateMembers();
            }

            if (_sysTypes.ContainsKey(dt.UnderlyingSystemType)) {
                customizer.HideMembers();
            }
        }

        private void HideMembers() {
            Type sysType = Builder.UnfinishedType.UnderlyingSystemType;
            foreach (FieldInfo fi in sysType.GetFields()) {
                SetValue(SymbolTable.StringToId(fi.Name), PythonContext.Id, new ReflectedField(fi, NameType.Field));
            }

            foreach (PropertyInfo pi in sysType.GetProperties()) {
                SetValue(SymbolTable.StringToId(pi.Name), PythonContext.Id, new ReflectedProperty(pi, pi.GetGetMethod(), pi.GetSetMethod(), NameType.Property));
            }
        }

        private void AddInitCode() {
            if (Builder.UnfinishedType.UnderlyingSystemType.IsSubclassOf(typeof(Delegate)) || 
                Builder.UnfinishedType.UnderlyingSystemType.IsSubclassOf(typeof(Array))) {
                return;
            }

            // __new__
            object newFunc;
            if (TryGetValue(Symbols.NewInst, PythonContext.Id, out newFunc)) {
                // user provided a __new__ method, first argument should be DynamicType
                // We will set our allocator to be a bound-method that passes our type
                // through, and we'll leave __new__ unchanged, other than making sure
                // it's a function, not a method.

                BuiltinFunction bf = newFunc as BuiltinFunction;
                Debug.Assert(bf != null);
                Builder.SetConstructor(bf);

                bf.FunctionType = (bf.FunctionType & ~FunctionType.FunctionMethodMask) | FunctionType.Function;
            } else {
                CreateNewMethod();
            }

            // __init__
            if (!TryGetValue(Symbols.Init, PythonContext.Id, out newFunc)) {
                SetValue(Symbols.Init, PythonContext.Id, InstanceOps.Init);
            }
        }

        private void AddDocumentation() {
            object val;
            if (!TryGetValue(Symbols.Doc, PythonContext.Id, out val)) {
                SetValue(Symbols.Doc, PythonContext.Id, _docDescr);
            }
        }

        private void AddOperators() {
            Type sysType = Builder.UnfinishedType.UnderlyingSystemType;
            if (!sysType.IsPublic && !sysType.IsNestedPublic && !ScriptDomainManager.Options.PrivateBinding) return;
            bool isSysType = _sysTypes.ContainsKey(sysType);
            BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (MethodInfo mi in sysType.GetMethods(bf)) {
                if (!ShouldInclude(mi)) continue;

                if (isSysType) {
                    // we want to store a context sensitive version for system types (object, int, etc...)
                    // so that we hide the non-context sensitive version.
                    if (mi.IsStatic) {
                        StoreMethod(mi.Name, PythonContext.Id, mi, FunctionType.Function);
                    } else {
                        StoreMethod(mi.Name, PythonContext.Id, mi, FunctionType.Method);
                    }
                }

                if (mi.IsPrivate || !mi.DeclaringType.IsPublic || (mi.DeclaringType.DeclaringType != null && !mi.DeclaringType.IsNestedPublic)) {
                    string name;
                    NameType nt = NameConverter.TryGetName(Builder.UnfinishedType, mi, out name);
                    object dummy;
                    if (nt != NameType.None && !TryGetValue(SymbolTable.StringToId(name), PythonContext.Id, out dummy)) {
                        StoreMethodNoConflicts(name, PythonContext.Id, mi, FunctionType.Method | FunctionType.AlwaysVisible);
                    }
                }

                if (!mi.IsSpecialName) {
                    continue;
                }

                if (mi.Name == "get_Item") {
                    AddTupleExpansionGetOrDeleteItem(Symbols.GetItem, mi);
                    continue;
                } else if (mi.Name == "set_Item") {
                    AddTupleExpansionSetItem(mi);
                    continue;
                }

                bool forward, reverse;
                OperatorMapping opmap = PythonExtensionTypeAttribute.GetRegularReverse(Builder.UnfinishedType.UnderlyingSystemType, mi, out forward, out reverse);
                FunctionType ft = FunctionType.Method | FunctionType.AlwaysVisible;

                if (opmap != null) {
                    switch (opmap.Operator) {
                        case Operators.DeleteItem: AddTupleExpansionGetOrDeleteItem(Symbols.DelItem, mi); break;
                        default:
                            if (opmap.IsBinary) ft |= FunctionType.BinaryOperator;

                            if (forward) {
                                SymbolId name;
                                DynamicTypeSlot slot;
                                if (PythonExtensionTypeAttribute.ReverseOperatorTable.TryGetValue(opmap, out name)) {
                                    slot = StoreMethodNoConflicts(SymbolTable.IdToString(name),
                                        PythonContext.Id,
                                        mi,
                                        ft);
                                } else {
                                    slot = StoreMethodNoConflicts(mi.Name,
                                        ContextId.Empty,
                                        mi,
                                        ft & ~FunctionType.AlwaysVisible);
                                }

                                if(slot != null) AddOperator(slot, opmap);
                            }

                            SymbolId revName;
                            if (reverse && PythonExtensionTypeAttribute.ReverseOperatorTable.TryGetValue(opmap.GetReversed(), out revName)) {
                                if (opmap.IsBinary) ft |= FunctionType.ReversedOperator;

                                DynamicTypeSlot slot = StoreMethodNoConflicts(SymbolTable.IdToString(revName),
                                    PythonContext.Id,
                                    mi,
                                    ft);

                                if(slot != null) AddOperator(slot, opmap);
                            }
                            break;
                    }
                } 
            }
        }

        private void AddPrivateMembers() {
            BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

            Type sysType = Builder.UnfinishedType.UnderlyingSystemType;
            foreach (FieldInfo fi in sysType.GetFields(bf)) {
                string name;
                NameType nt = NameConverter.TryGetName(Builder.UnfinishedType, fi, out name); 
                if(nt != NameType.None) {
                    SetValue(SymbolTable.StringToId(name), 
                        PythonContext.Id, 
                        new ReflectedField(fi, nt));
                }
            }

            foreach (PropertyInfo pi in sysType.GetProperties(bf)) {
                MethodInfo get = pi.GetGetMethod(true);
                MethodInfo set = pi.GetSetMethod(true);

                string getName = null;
                NameType gnt = NameType.None, snt = NameType.None;
                string setName = null;
                if (get != null) {
                    gnt = NameConverter.TryGetName(Builder.UnfinishedType, pi, get, out getName);
                }
                if (set != null) {
                    snt = NameConverter.TryGetName(Builder.UnfinishedType, pi, set, out setName);
                }

                if (gnt != NameType.None || snt != NameType.None) {
                    SetValue(SymbolTable.StringToId(getName ?? setName), 
                        PythonContext.Id, 
                        new ReflectedProperty(pi, get, set, NameType.PythonProperty));
                }
            }

            foreach (EventInfo ei in sysType.GetEvents(bf)) {
                string aname = null, rname = null;
                NameType ant = NameType.None, rnt = NameType.None;

                MethodInfo add = ei.GetAddMethod(true);
                MethodInfo rem = ei.GetRemoveMethod(true);

                if (add != null) {
                    ant = NameConverter.TryGetName(Builder.UnfinishedType, ei, add, out aname);
                }

                if (rem != null) {
                    rnt = NameConverter.TryGetName(Builder.UnfinishedType, ei, rem, out rname);
                }

                if (ant != NameType.None || rnt != NameType.None) {
                    SetValue(SymbolTable.StringToId(aname ?? rname), 
                        PythonContext.Id, 
                        new ReflectedEvent(ei, false));
                }
                
            }
        }

        private static bool ShouldInclude(MethodInfo getter) {
            return getter != null &&
                (getter.IsFamily || getter.IsFamilyOrAssembly || getter.IsPublic || ScriptDomainManager.Options.PrivateBinding);
        }

        /// <summary>
        /// Stores a method only if there's no conflicts.  Useful because it allows us to define
        /// methods in our Ops classes which override methods defined in a type.  An example is
        /// BigInteger division where Python has special semantics - therefore we define our own
        /// division and want that to replace BigInteger's division.
        /// </summary>
        private void AddPythonProtocolMethods() {
            Type type = Builder.UnfinishedType.UnderlyingSystemType;

            if (typeof(System.Collections.IEnumerator).IsAssignableFrom(type)) {
                AddProtocolMethod(Symbols.GeneratorNext, "NextMethod");
            }

            if (type != typeof(string)) {   // no __iter__ on string, just __getitem__
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type)) {
                    AddProtocolMethod(Symbols.Iterator, "IterMethod");
                }
            }

            if (typeof(DynamicTypeSlot).IsAssignableFrom(type)) {
                AddProtocolMethod(Symbols.GetDescriptor, "GetMethod");
                // TODO: Set & delete
            }

            if (typeof(ICallableWithCodeContext).IsAssignableFrom(type)) {
                AddProtocolMethod(Symbols.Call, FunctionType.SkipThisCheck, GetMethodSet("CallMethod", 2));
            }
        }

        /// <summary>
        /// Adds the protocol methods for rich equality (__eq__, __ne__, __lt__, __gt__, __le__, __ge__, and __hash__)
        /// 
        /// If the type implements IValueEquality we forward these to helper methods which are allowed
        /// to return object.  The helper methods will check that the type matches the declaring type
        /// and return NotImplemented if it's different.  Types can be customized by providing __hash__, 
        /// __eq__, or __ne__ which will take precedence over the automatically added versions.
        /// 
        /// If the type doesn't implement IValueEquality we'll forward these to methods which 
        /// call the .NET Equals/!Equals/GetHashCode methods.
        /// </summary>
        private void AddRichEqualityProtocols() {
            Type sysType = Builder.UnfinishedType.UnderlyingSystemType;

            if(typeof(IValueEquality).IsAssignableFrom(sysType) && !sysType.IsInterface) {
                // for the generic ValueEquality helpers we want to instantiate w/
                // the declaring type so we do the right thing for subtypes.
                InterfaceMapping imap = sysType.GetInterfaceMap(typeof(IValueEquality));
                foreach(MethodInfo mi in imap.TargetMethods) {
                    if (mi.Name.EndsWith("ValueEquals")) {
                        if (_equalsHelper == null) _equalsHelper = GetMethodSet("ValueEqualsMethod", 3);
                        AddProtocolMethod(Symbols.OperatorEquals, mi.DeclaringType, _equalsHelper);
                    } else if (mi.Name.EndsWith("ValueNotEquals")) {
                        if (_notEqualsHelper == null) _notEqualsHelper = GetMethodSet("ValueNotEqualsMethod", 3);
                        AddProtocolMethod(Symbols.OperatorNotEquals, mi.DeclaringType, _notEqualsHelper);
                    } else if (mi.Name.EndsWith("GetValueHashCode")) {
                        AddProtocolMethod(Symbols.Hash, "ValueHashMethod");
                    }
                }
            } else {
                MethodInfo equalsMethod = sysType.GetMethod("Equals", new Type[] { typeof(object) });
                // we wrap this in an indirect helper call because the type might not be public
                if (equalsMethod != null && 
                    equalsMethod.DeclaringType == sysType && 
                    (equalsMethod.Attributes & MethodAttributes.NewSlot)==0) {  
                  
                    AddProtocolMethod(Symbols.OperatorEquals, "EqualsMethod");
                    AddProtocolMethod(Symbols.OperatorNotEquals, "NotEqualsMethod");
                }

                MethodInfo getHashCode = sysType.GetMethod("GetHashCode", new Type[] {});
                if (getHashCode != null &&
                    getHashCode.DeclaringType == sysType &&
                    (getHashCode.Attributes & MethodAttributes.NewSlot) == 0) {
                    AddProtocolMethod(Symbols.Hash, "GetHashCodeMethod");
                }
            }
        }

        private void AddToStringProtocols() {
            Type sysType = Builder.UnfinishedType.UnderlyingSystemType;
            if (sysType == typeof(object)) return;

            MethodInfo toStringMethod = sysType.GetMethod("ToString", ArrayUtils.EmptyTypes);

            if (toStringMethod != null && toStringMethod.DeclaringType == sysType) {
                AddProtocolMethod(Symbols.String, "ToStringMethod");
            }

            if (typeof(ICodeFormattable).IsAssignableFrom(sysType) && !sysType.IsInterface) {
                MethodInfo repr = sysType.GetInterfaceMap(typeof(ICodeFormattable)).TargetMethods[0];

                AddProtocolMethod(Symbols.Repr, "ReprHelper");
            } else if (toStringMethod != null && toStringMethod.DeclaringType == sysType) {
                if (sysType.IsDefined(typeof(PythonTypeAttribute), false)) {
                    AddProtocolMethod(Symbols.Repr, "ToStringMethod");
                } else {
                    AddProtocolMethod(Symbols.Repr, "FancyRepr");
                }
            }
        }

        private static MethodInfo[] GetMethodSet(string name, int expected) {
            MethodInfo[] methods = typeof(InstanceOps).GetMethods();
            MethodInfo[] filtered = new MethodInfo[expected];
            int j = 0;
            for (int i = 0; i < methods.Length; i++) {
                if (methods[i].Name == name) {
                    filtered[j++] = methods[i];
#if !DEBUG
                    if (j == expected) break;
#endif
                }
            }
            Debug.Assert(j == expected);
            return filtered;
        }
#if FALSE
        private void PythonAddOperator() {
            // some operators need specific transformations from the DLR semantics into the 
            // Python semantics - those transformations are done here.
            /*if (op.Operator == Operators.Call) {
                builder.AddOperator(Operators.Call, MakeNonLookupCall(dt, callable));
            } else if (op.Operator == Operators.GetBoundAttr) {
                builder.AddOperator(op.Operator,
                    delegate(CodeContext context, object self, object other, out object ret) {
                        object value;
                        callable.TryGetValue(context, self, dt, out value);
                        ret = Ops.CallWithContext(context, value, other);
                        return true;
                    }
                );
            } else if (op.Operator == Operators.SetItem) {
                AddOperatorSetItem(builder);    //!! lookup!
            } else if (op.Operator == Operators.GetItem) {
                AddOperatorGetItem(builder);    //!! lookup!
            } else if (op.Operator == Operators.DeleteItem) {
                AddOperatorDeleteItem(builder); //!! lookup!
            } else { */
        }
#endif

        private void CreateNewMethod() {
            BuiltinFunction reflectedCtors = GetConstructors();

            if (reflectedCtors != null) {
                object newVal;
                if (reflectedCtors.Targets.Length == 1 && reflectedCtors.Targets[0].GetParameters().Length == 0) {
                    if (IsPythonType()) {
                        newVal = InstanceOps.New;
                    } else {
                        newVal = InstanceOps.NewCls;
                    }
                } else {
                    newVal = new ConstructorFunction(InstanceOps.NonDefaultNewInst, reflectedCtors.Targets);
                }

                SetValue(Symbols.NewInst, PythonContext.Id, newVal);
            }
        }

        private bool IsPythonType() {
            return IsPythonType(Builder.UnfinishedType.UnderlyingSystemType);
        }

        public static bool IsPythonType(Type t) {
            return t.IsDefined(typeof(PythonTypeAttribute), false) || _sysTypes.ContainsKey(t);
        }

        private static string GetDocumentation(DynamicType type) {
            // Python documentation
            object[] docAttr = type.UnderlyingSystemType.GetCustomAttributes(typeof(DocumentationAttribute), false);
            if (docAttr != null && docAttr.Length > 0) {
                return ((DocumentationAttribute)docAttr[0]).Documentation;
            }

            if (type == TypeCache.None) return null;

            // Auto Doc (XML or otherwise)
            string autoDoc = DocBuilder.CreateAutoDoc(type.UnderlyingSystemType);
            if (autoDoc == null) {
                autoDoc = String.Empty;
            } else {
                autoDoc += Environment.NewLine + Environment.NewLine;
            }

            // Simple generated helpbased on ctor, if available.
            ConstructorInfo[] cis = type.UnderlyingSystemType.GetConstructors();
            foreach (ConstructorInfo ci in cis) {
                autoDoc += FixCtorDoc(type, DocBuilder.CreateAutoDoc(ci)) + Environment.NewLine;
            }

            return autoDoc;
        }

        private static string FixCtorDoc(DynamicType type, string autoDoc) {
            return autoDoc.Replace("__new__(cls)", type.Name + "()").
                            Replace("__new__(cls, ", type.Name + "(");
        }

        class DocumentationDescriptor : DynamicTypeSlot {
            public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
                value = GetDocumentation((DynamicType)owner);
                return true;
            }
        }

        protected override void AddImplicitConversion(MethodInfo mi) {
            throw new NotImplementedException();
        }


        private void AddProtocolMethod(SymbolId symbol, string helper) {
            AddProtocolMethod(symbol, FunctionType.SkipThisCheck|FunctionType.OpsFunction, typeof(InstanceOps).GetMethod(helper));
        }

        private void AddProtocolMethod(SymbolId symbol, Type genericType, params MethodInfo[] methodInfos) {
            MethodInfo[] mis = new MethodInfo[methodInfos.Length];
            for (int i = 0; i < methodInfos.Length; i++) {
                mis[i] = methodInfos[i].MakeGenericMethod(genericType);
            }

            AddProtocolMethod(symbol, FunctionType.SkipThisCheck|FunctionType.OpsFunction, mis);
        }

        private void AddProtocolMethod(SymbolId symbol, params MethodInfo[] methodInfos) {
            AddProtocolMethod(symbol, FunctionType.OpsFunction, methodInfos);
        }

        private void AddProtocolMethod(SymbolId symbol, FunctionType functionType, params MethodInfo[] methodInfos) {
            object tmp;
            if (TryGetValue(symbol, PythonContext.Id, out tmp)) {
                // type has specified PythonName to provide this protocol method explicitly
                // w/ the desired behavior
                return;
            }

            Debug.Assert(methodInfos.Length != 0);

            DynamicTypeSlot method = null;

            foreach(MethodInfo mi in methodInfos) {
                method = StoreMethod(SymbolTable.IdToString(symbol),
                    PythonContext.Id,
                    mi,
                    functionType | FunctionType.Method | FunctionType.AlwaysVisible);
            }

            OperatorMapping opMap;
            if (PythonExtensionTypeAttribute._pythonOperatorTable.TryGetValue(symbol, out opMap)) {
                AddOperator(PythonContext.Id, method, opMap);
            }
        }

        private void AddTupleExpansionGetOrDeleteItem(SymbolId op, MethodInfo mi) {
            DynamicTypeSlot callable = StoreMethodNoConflicts(SymbolTable.IdToString(op), 
                PythonContext.Id, 
                mi, 
                FunctionType.Method | FunctionType.AlwaysVisible);

            if (callable == null) return;

            Builder.AddOperator(PythonContext.Id,
                PythonExtensionTypeAttribute._pythonOperatorTable[op].Operator,
                delegate(CodeContext context, object self, object other, out object ret) {
                    object func;
                    if (!callable.TryGetValue(context, self, Builder.UnfinishedType, out func)) {
                        ret = null;
                        return false;
                    }

                    IParameterSequence t = other as IParameterSequence;
                    if (t != null && t.IsExpandable) {
                        ret = PythonOps.CallWithContext(context,
                            func,
                            t.Expand(null));
                        return true;
                    }
                    ret = PythonOps.CallWithContext(context,
                        func,
                        other);
                    return true;
                });
        }

        private void AddTupleExpansionSetItem(MethodInfo mi) {
            DynamicTypeSlot callable = StoreMethodNoConflicts("__setitem__",
                PythonContext.Id,
                mi,
                FunctionType.Method | FunctionType.AlwaysVisible);

            if (callable == null) return;

            Builder.AddOperator(PythonContext.Id, 
                Operators.SetItem,
                delegate(CodeContext context, object self, object value1, object value2, out object ret) {
                    object func;
                    if (!callable.TryGetValue(context, self, Builder.UnfinishedType, out func)) {
                        ret = null;
                        return false;
                    }

                    IParameterSequence t = value1 as IParameterSequence;
                    if (t != null && t.IsExpandable) {
                        value1 = t.Expand(null);
                        ret = PythonOps.CallWithContext(context,
                            func,
                            t.Expand(value2));
                        return true;
                    }
                    ret = PythonOps.CallWithContext(context,
                        func,
                        value1,
                        value2);
                    return true;
                });
        }

        /// <summary>
        /// Creates a table of standard .NET types which are also standard Python types
        /// </summary>
        private static Dictionary<Type, string> MakeSystemTypes() {
            Dictionary<Type, string> res = new Dictionary<Type, string>();

            res[typeof(object)] = "object";
            res[typeof(string)] = "str";
            res[typeof(int)] = "int";
            res[typeof(bool)] = "bool";
            res[typeof(double)] = "float";
            res[typeof(decimal)] = "decimal";
            res[typeof(BigInteger)] = "long";
            res[typeof(Complex64)] = "complex";
            res[typeof(DynamicType)] = "type";
            res[typeof(DynamicMixin)] = "mixin";
            res[typeof(ScriptModule)] = "module";
            res[typeof(SymbolDictionary)] = "dict";
            res[typeof(CustomSymbolDictionary)] = "dict";
            res[typeof(BaseSymbolDictionary)] = "dict";
            res[typeof(BuiltinFunction)] = "builtin_function_or_method";
            res[typeof(BuiltinMethodDescriptor)] = "method_descriptor";
            res[typeof(ClassMethodDescriptor)] = "method_descriptor";
            res[typeof(BoundBuiltinFunction)] = "builtin_function_or_method";
            res[typeof(ReflectedField)] = "field#";
            res[typeof(ReflectedProperty)] = "property#";
            res[typeof(ReflectedIndexer)] = "indexer#";
            res[typeof(ReflectedEvent)] = "event#";
            res[typeof(ValueType)] = "ValueType";   // just hiding it's methods in the inheritance hierarchy
            res[typeof(TypeGroup)] = "type-collision";
            res[typeof(None)] = "NoneType";
            res[typeof(IAttributesCollection)] = "dict";

            return res;
        }

        public static Dictionary<Type, string> SystemTypes {
            get {
                return _sysTypes;
            }
        }
    }
}
