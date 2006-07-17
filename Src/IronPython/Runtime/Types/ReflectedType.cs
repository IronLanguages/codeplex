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
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using IronPython.Compiler;
using IronPython.Modules;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;
using IronMath;

namespace IronPython.Runtime.Types {
    // ReflectedType represents types that the user did not define. These include:
    // 1. Built-in types supported intrinsically by the engine (and implemented as CLI types in the engine)
    // 2. Non-Python types imported from CLI assemblies
    //
    // ReflectedType's are read-only once they are created as the user is not allowed to assign 
    // properties to them.

    [PythonType(typeof(DynamicType))]
    public class ReflectedType : DynamicType, IContextAwareMember {
        #region Member variables

        /* A reflected type can be setup to behave in several different ways.
         *    First, a type can either be a "Python Type" (a type declared in the IP engine w/ PythonTypeAttribute)
         *    or it can be a normal CLR type.  For Python Types we only want to expose the python-visible
         *    features until the user imports a clr namespace (or our clr module).  We lazily do this check
         *    and cache this information.
         * 
         * Types that do have PythonTypeAttribute can also masquerade as other types.  This is done w/ 
         * effectivePythonType.  eg:
         * 
         * Ops.GetDynamicTypeFromType(IronPython.Runtime.Function0).effectivePythonType ==
         * Ops.GetDynamicTypeFromType(IronPython.Runtime.Function)
         * 
         * All other state is inherited from DynamicType and DynamicType.
         */

        private bool isPythonType, clsOnly, isPythonTypeChecked;
        private bool initialized;
        private List<MethodInfo> conversions;           // implicit conversions supported by the type.
        public ReflectedType effectivePythonType;

        private static Hashtable operatorTable;

        private IAttributesInjector prependedAttrs;
        private IAttributesInjector appendedAttrs;

        #endregion

        #region ReflectedType factories

        public static ReflectedType FromType(Type type) {
            if (type.IsArray) {
                return new ReflectedArrayType("array_" + Ops.GetDynamicTypeFromType(type.GetElementType()).__name__,
                    type);
            }

            if (type.IsSubclassOf(typeof(Delegate))) {
                return new ReflectedDelegateType(type);
            }

            if (ComObject.Is__ComObject(type)) {
                return ComType.MakeDynamicType();
            }

            return new ReflectedType(type);
        }

        public static ReflectedType FromClsOnlyType(Type type) {
            ReflectedType res = FromType(type);
            res.clsOnly = true;
            return res;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor only makes the mapping between the Type and ReflectedType objects 
        /// All other initialization is done lazily only when needed
        /// </summary>
        protected ReflectedType(Type type)
            : base(type) {
            __name__ = GetName(type);
        }
        #endregion


        #region Protected API Surface

        protected virtual void AddOps() {
        }

        #endregion

        #region Internal API Surface

        internal bool TryConvertTo(object value, ReflectedType to, out object result) {
            if (conversions != null && conversions.Count > 0) {
                for (int i = 0; i < conversions.Count; i++) {
                    if (conversions[i].ReturnType == to.type) {
                        try {
                            result = conversions[i].Invoke(null, new object[] { value });
                            return true;
                        } catch (TargetInvocationException tie) {
                            throw ExceptionConverter.UpdateForRethrow(tie);
                        }
                    }
                }
            }
            result = null;
            return false;
        }

        internal bool TryConvertFrom(object value, out object result) {
            if (conversions != null && conversions.Count > 0) {
                for (int i = 0; i < conversions.Count; i++) {
                    if (conversions[i].ReturnType == this.type &&
                        conversions[i].GetParameters()[0].ParameterType.IsAssignableFrom(value.GetType())) {
                        try {
                            result = conversions[i].Invoke(null, new object[] { value });
                            return true;
                        } catch (TargetInvocationException tie) {
                            throw ExceptionConverter.UpdateForRethrow(tie);
                        }
                    }
                }
            }
            result = null;
            return false;
        }

        /// <summary> Generic helper for doing the different types of method stores. </summary>
        internal void StoreMethod(string name, MethodInfo mi, FunctionType ft) {
            object existingMember;
            BuiltinFunction rm = null;
            SymbolId methodId = SymbolTable.StringToId(name);
            if (dict.TryGetValue(methodId, out existingMember)) {
                BuiltinMethodDescriptor bimd = existingMember as BuiltinMethodDescriptor;
                if (bimd != null) rm = bimd.template as BuiltinFunction;
                else rm = existingMember as BuiltinFunction;

                if (rm != null) {
                    rm.FunctionType |= ft;
                    rm.AddMethod(mi);

                    object newDescriptor;
                    if (bimd == null && (newDescriptor = rm.GetDescriptor()) != rm) {
                        // previously we didn't need a descriptor, but now we do.  This
                        // happens if we added a static function & then an instance function
                        // w/ the same name.  We'll replace the function w/ a descriptor.
                        dict[methodId] = newDescriptor;
                    }
                }
            }

            if (rm == null) {
                // This is destructive, Assert
                Debug.Assert(existingMember == null, String.Format("Replacing {0} with new BuiltinFunction", existingMember));
                if (name == "__init__") name = (string)this.__name__;
                rm = BuiltinFunction.MakeMethod(name, mi, ft);
                dict[methodId] = rm.GetDescriptor();
            }
        }

        internal void StoreReflectedMethod(string name, MethodInfo mi, NameType nt) {
            FunctionType ft = CompilerHelpers.IsStatic(mi) ? FunctionType.Function : FunctionType.Method;
            if (nt == NameType.PythonMethod || (!IsPythonType && !clsOnly)) ft |= FunctionType.PythonVisible;

            StoreMethod(name, mi, ft);
        }

        internal void StoreClassMethod(string name, MethodInfo mi) {
            SymbolId methodId = SymbolTable.StringToId(name);
            object existingMethod;
            if (dict.TryGetValue(methodId, out existingMethod)) {
                ClassMethodDescriptor cm = existingMethod as ClassMethodDescriptor;

                Debug.Assert(cm != null,
                    String.Format("Replacing existing method {0} on {1}", methodId, this));

                cm.func.AddMethod(mi);
            } else {
                dict[methodId] = new ClassMethodDescriptor(BuiltinFunction.MakeMethod(name, mi, FunctionType.Function | FunctionType.PythonVisible));
            }
        }

        internal void StoreReflectedBaseMethod(string name, MethodInfo mi, NameType nt) {
            object val;
            SymbolId methodId = SymbolTable.StringToId(name);
            if (dict.TryGetValue(methodId, out val)) {
                // generate a new optimized method that can handle the base class.
                BuiltinMethodDescriptor bmd = val as BuiltinMethodDescriptor;
                BuiltinFunction bf = (bmd == null) ? val as BuiltinFunction : bmd.template;

                if (bf != null) {
                    bf.AddMethod(mi);
                }
            }
        }

        internal void RegisterAttributesInjector(IAttributesInjector attrInjector, bool prepend) {
            if (prepend) {
                if (prependedAttrs != null) {
                    throw new InvalidOperationException("Attributes injector already registered");
                }

                prependedAttrs = attrInjector;
            } else {
                if (appendedAttrs != null) {
                    throw new InvalidOperationException("Attributes injector already registered");
                }

                appendedAttrs = attrInjector;
            }
        }

        #endregion

        #region Private APIs

        private void CreateInitCode() {
            // __new__
            object newFunc;
            if (dict.TryGetValue(SymbolTable.NewInst, out newFunc)) {
                // user provided a __new__ method, first argument should be DynamicType
                // We will set our allocator to be a bound-method that passes our type
                // through, and we'll leave __new__ unchanged, other than making sure
                // it's a function, not a method.

                BuiltinFunction bf = newFunc as BuiltinFunction;
                Debug.Assert(bf != null);

                ctor = bf;

                bf.FunctionType = (bf.FunctionType & ~FunctionType.FunctionMethodMask) | FunctionType.Function;
            } else {
                CreateNewMethod();
            }

            // __init__
            if (!dict.ContainsKey(SymbolTable.Init)) {
                dict[SymbolTable.Init] = InstanceOps.Init;
            }
        }

        /// <summary>
        /// Creates a __new__ method for the type.  If the type defines interesting constructors
        /// then the __new__ method will call that.  Otherwise if it has only a single argless
        /// </summary>
        protected virtual void CreateNewMethod() {
            if (!type.IsAbstract) {
                BuiltinFunction reflectedCtors = GetConstructors();
                if (reflectedCtors == null) return; // no ctors, no __new__

                ctor = reflectedCtors;
                if (reflectedCtors.Targets.Length == 1 &&
                    reflectedCtors.Targets[0].GetParameters().Length == 0) {
                    if (IsPythonType) {
                        dict[SymbolTable.NewInst] = InstanceOps.New;
                    } else {
                        dict[SymbolTable.NewInst] = InstanceOps.NewCls;
                    }
                } else {
                    dict[SymbolTable.NewInst] = new ConstructorFunction(InstanceOps.CreateNonDefaultNew(), reflectedCtors.Targets);
                }
            }
        }

        private BuiltinFunction GetConstructors() {
            BuiltinFunction reflectedCtors = null;
            bool hasDefaultConstructor = false;
            foreach (ConstructorInfo ci in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)) {
                if (ci.IsPublic) {
                    if (ci.GetParameters().Length == 0) hasDefaultConstructor = true;
                    reflectedCtors = BuiltinFunction.MakeOrAdd(reflectedCtors, (string)__name__, ci, FunctionType.Function);
                }
            }
            if (type.IsValueType && !hasDefaultConstructor) {
                MethodInfo mi = typeof(Activator).GetMethod("CreateInstance", Type.EmptyTypes).MakeGenericMethod(type);
                reflectedCtors = BuiltinFunction.MakeOrAdd(reflectedCtors, (string)__name__, mi, FunctionType.Function);
            }
            return reflectedCtors;
        }

        private void AddPythonProtocolMethods() {

            if (typeof(IEnumerator).IsAssignableFrom(type)) {
                AddProtocolMethod("next", "NextMethod", NameType.PythonMethod);
            }

            MethodInfo toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);

            if (toStringMethod != null && toStringMethod.DeclaringType != typeof(object)) {
                AddProtocolMethod("__str__", "ToStringMethod", NameType.PythonMethod);
            }

            if (typeof(ICodeFormattable).IsAssignableFrom(type)) {
                // type is specifically implementing __repr__
                if (!dict.ContainsKey(SymbolTable.Repr)) {
                    // add __repr__ automatically if it's not already there.
                    AddProtocolMethod("__repr__", "ReprHelper", NameType.PythonMethod);
                }
            } else if (toStringMethod == null || toStringMethod.DeclaringType != typeof(object)) {
                // type overrides ToString...  for DynamicType's call ToString, for
                // everyone else call FancyRepr
                if (IsPythonType) {
                    AddProtocolMethod("__repr__", "ToStringMethod", NameType.PythonMethod);
                } else {
                    AddProtocolMethod("__repr__", "FancyRepr", NameType.PythonMethod);
                }
            } else {
                // doesn't override ToString, just do a simple repr displaying the
                // type name & id.
                AddProtocolMethod("__repr__", "SimpleRepr", NameType.PythonMethod);
            }

            if (typeof(IDescriptor).IsAssignableFrom(type)) {
                AddProtocolMethod("__get__", "GetMethod", NameType.PythonMethod);
            }
            if (typeof(ICallable).IsAssignableFrom(type) && !dict.ContainsKey(SymbolTable.Call)) {
                AddProtocolMethod("__call__", "CallMethod", NameType.PythonMethod);
            }
        }

        private void AddDocumentation() {
            if (!dict.ContainsKey(SymbolTable.Doc)) {
                dict[SymbolTable.Doc] = GetDocumentation();
            }
        }

        private string GetDocumentation() {
            object[] docAttr = type.GetCustomAttributes(typeof(DocumentationAttribute), false);
            if (docAttr != null && docAttr.Length > 0) {
                return ((DocumentationAttribute)docAttr[0]).Value;
            }

            BuiltinFunction newMeth = ctor as BuiltinFunction;
            if (newMeth == null) {
                if (type.IsEnum) return ReflectionUtil.CreateEnumDoc(type);

                return null;
            }

            // strip off function name & cls parameter and just
            // display as plain type
            return newMeth.Documentation.Replace("__new__(cls)", Name + "()").Replace("__new__(cls, ", Name + "(");
        }

        private string GetName(Type type) {
            if (type == typeof(object)) { return "object"; } //???
            PythonTypeAttribute attr = (PythonTypeAttribute)
                Attribute.GetCustomAttribute(type, typeof(PythonTypeAttribute));
            if (attr != null) {
                if (attr.impersonateType != null) {
                    this.effectivePythonType = (ReflectedType)Ops.GetDynamicTypeFromType(attr.impersonateType);
                }
                return attr.name;
            } else {
                string name;
                NameConverter.TryGetName(this, type, out name);
                return name;
            }
        }

        private void AddEnumOperator(string name, string method) {
            OperatorMethod oper = (OperatorMethod)operatorTable[name];
            if (oper == null) return;

            if (oper.method != null) {
                MethodInfo mi = typeof(EnumOps).GetMethod(method);
                StoreMethod(oper.method,
                    mi,
                    FunctionType.PythonVisible | FunctionType.SkipThisCheck | FunctionType.Method);
            }
        }

        private void AddEnumOperators() {
            if (!type.IsSubclassOf(typeof(System.Enum))) {
                return;
            }
            object[] flags = type.GetCustomAttributes(typeof(System.FlagsAttribute), false);
            if (flags == null || flags.Length == 0) {
                return;
            }

            if (operatorTable == null) {
                InitializeOperatorTable();
            }

            AddEnumOperator("op_BitwiseOr", "BitwiseOr");
            AddEnumOperator("op_BitwiseAnd", "BitwiseAnd");
            AddEnumOperator("op_OnesComplement", "OnesComplement");
            AddEnumOperator("op_ExclusiveOr", "ExclusiveOr");
            AddEnumOperator("op_Equality", "Equal");
            AddEnumOperator("op_Inequality", "NotEqual");
        }

        private PropertyInfo GetPropertyFromMethod(MethodInfo mi, MemberInfo[] defaultMembers) {

            foreach (MemberInfo member in defaultMembers) {
                if (member.MemberType == MemberTypes.Property) {
                    PropertyInfo property = member as PropertyInfo;
                    if (mi == property.GetGetMethod() ||
                        mi == property.GetSetMethod()) {

                        return property;
                    }
                }
            }

            // check both the declaring type, and our type.  We need to check
            // both to deal w/ interfaces & abstract classes where the property
            // is defined in the interface/abstract class, and the methods are
            // declared in the concrete type.
            if (mi.DeclaringType != this.type) {
                PropertyInfo pi = SearchTypeForProperty(mi, mi.DeclaringType);
                if (pi != null) {
                    return pi;
                }
            }

            return SearchTypeForProperty(mi, type);
        }

        private static PropertyInfo SearchTypeForProperty(MethodInfo mi, Type type) {
            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
                if ((prop.GetGetMethod(true) == mi) ||
                    (prop.GetSetMethod(true) == mi)) {
                    return prop;
                }
            }
            return null;
        }

        private class OperatorMethod {
            public string method;
            public string rmethod;
            public int args;

            public OperatorMethod(string method, string rmethod, int args) {
                this.method = method;
                this.rmethod = rmethod;
                this.args = args;
            }
        }

        private static void InitializeOperatorTable() {
            Hashtable ot = new Hashtable(28);
            ot["op_Addition"] = new OperatorMethod("__add__", "__radd__", 2);
            ot["op_Subtraction"] = new OperatorMethod("__sub__", "__rsub__", 2);
            ot["op_Multiply"] = new OperatorMethod("__mul__", "__rmul__", 2);
            ot["op_Division"] = new OperatorMethod("__div__", "__rdiv__", 2);
            ot["op_Modulus"] = new OperatorMethod("__mod__", "__rmod__", 2);
            ot["op_ExclusiveOr"] = new OperatorMethod("__xor__", "__rxor__", 2);
            ot["op_BitwiseAnd"] = new OperatorMethod("__and__", "__rand__", 2);
            ot["op_BitwiseOr"] = new OperatorMethod("__or__", "__ror__", 2);
            ot["op_LeftShift"] = new OperatorMethod("__lshift__", "__rlshift__", 2);
            ot["op_RightShift"] = new OperatorMethod("__rshift__", "__rrshift__", 2);
            ot["op_Equality"] = new OperatorMethod("__eq__", null, 2);
            ot["op_GreaterThan"] = new OperatorMethod("__gt__", null, 2);
            ot["op_LessThan"] = new OperatorMethod("__lt__", null, 2);
            ot["op_Inequality"] = new OperatorMethod("__ne__", null, 2);
            ot["op_GreaterThanOrEqual"] = new OperatorMethod("__ge__", null, 2);
            ot["op_LessThanOrEqual"] = new OperatorMethod("__le__", null, 2);
            ot["op_UnaryNegation"] = new OperatorMethod("__neg__", null, 1);
            ot["op_OnesComplement"] = new OperatorMethod("__invert__", null, 1);
            ot["op_AdditionAssignment"] = new OperatorMethod("__iadd__", null, 2);
            ot["op_SubtractionAssignment"] = new OperatorMethod("__isub__", null, 2);
            ot["op_MultiplicationAssignment"] = new OperatorMethod("__imul__", null, 2);
            ot["op_DivisionAssignment"] = new OperatorMethod("__idiv__", null, 2);
            ot["op_ModulusAssignment"] = new OperatorMethod("__imod__", null, 2);
            ot["op_LeftShiftAssignment"] = new OperatorMethod("__ilshift__", null, 2);
            ot["op_UnsignedRightShift"] = new OperatorMethod("__irshift__", null, 2);
            ot["op_BitwiseAndAssignment"] = new OperatorMethod("__iand__", null, 2);
            ot["op_ExclusiveOrAssignment"] = new OperatorMethod("__ixor__", null, 2);
            ot["op_BitwiseOrAssignment"] = new OperatorMethod("__ior__", null, 2);
            //ot["op_Implicit"] = null;
            //ot["op_Explicit"] = null;
            //ot["op_LogicalAnd"] = null;
            //ot["op_LogicalOr"] = null;
            //ot["op_Assign"] = null;
            //ot["op_SignedRightShift"] = null;
            //ot["op_Comma"] = null;
            //ot["op_Decrement"] = null;
            //ot["op_Increment"] = null;
            //ot["op_UnaryPlus"] = null;
            operatorTable = ot;
        }

        private bool AddOperator(MethodInfo mi) {
            if (operatorTable == null) {
                InitializeOperatorTable();
            }

            OperatorMethod method = (OperatorMethod)operatorTable[mi.Name];
            if (method == null) {
                if (mi.Name == "op_Implicit") {
                    return AddImplicitConversion(mi);
                }
                return false;
            }

            bool instance = !mi.IsStatic;

            ParameterInfo[] parms = mi.GetParameters();
            if (parms.Length + (instance ? 1 : 0) != method.args) return false;

            if (instance) {
                if (method.method != null) {
                    StoreReflectedMethod(method.method, mi, NameType.PythonMethod);
                    return true;
                } else {
                    return false;
                }
            }

            bool regular = parms.Length > 0 && method.method != null && parms[0].ParameterType == type;
            bool reverse = parms.Length > 1 && method.rmethod != null && parms[1].ParameterType == type;

            FunctionType funcType = FunctionType.Method | FunctionType.PythonVisible;
            if (parms.Length == 2) funcType |= FunctionType.BinaryOperator;

            if (regular) {
                StoreMethod(method.method, mi, funcType);
            }
            if (reverse) {
                StoreMethod(method.rmethod, mi, funcType | FunctionType.ReversedOperator);
            }

            return regular || reverse;
        }

        private bool AddImplicitConversion(MethodInfo mi) {
            if (conversions == null) conversions = new List<MethodInfo>();
            conversions.Add(mi);
            return true;
        }

        private void AddReflectedMethod(MethodInfo mi, MemberInfo[] defaultMembers) {
            string name;

            if (IsExplicitInterfaceImpl(mi)) {
                // explicit interface methods will be added independently.
                return;
            }

            if (mi.IsSpecialName) {
                if (AddOperator(mi)) {
                    return;
                } else if (GetPropertyFromMethod(mi, defaultMembers) != null) {
                    return;
                }
            }

            if (Options.Python25 == false) {
                object[] attribute = mi.GetCustomAttributes(typeof(PythonVersionAttribute), false);
                if (attribute.Length > 0) {
                    PythonVersionAttribute attr = attribute[0] as PythonVersionAttribute;
                    if (attr != null && attr.version == ReflectionUtil.pythonVersion25) return;
                }
            }

            NameType nt = NameConverter.TryGetName(this, mi, out name);
            switch (nt) {
                case NameType.None: break;
                case NameType.PythonMethod:
                    StoreReflectedMethod(name, mi, nt);
                    break;
                case NameType.Method:
                    StoreReflectedMethod(name, mi, nt);
                    break;
                case NameType.ClassMethod: StoreClassMethod(name, mi); break;
                default: Debug.Assert(false, "Unexpected name type for reflected method"); break;
            }

            if (name != mi.Name) {
                StoreReflectedMethod(mi.Name, mi, NameType.Method);
            }
        }

        private void AddReflectedField(FieldInfo fi) {
            string name;
            NameType nt = NameConverter.TryGetName(this, fi, out name);
            if (nt != NameType.None) {
                dict[SymbolTable.StringToId(name)] = new ReflectedField(fi, nt);
            }
        }

        private void AddIndexer(PropertyInfo pi) {
            MethodInfo method;

            method = pi.GetGetMethod();
            if (method != null) {
                StoreReflectedMethod("__getitem__", method, NameType.PythonMethod);
            }
            method = pi.GetSetMethod();
            if (method != null) {
                StoreReflectedMethod("__setitem__", method, NameType.PythonMethod);
            }
        }

        internal static bool IsExplicitInterfaceImpl(MethodInfo mi) {
            return mi.IsFinal && mi.IsHideBySig && mi.IsPrivate && mi.IsVirtual;
        }

        private void AddReflectedProperty(PropertyInfo info, MemberInfo[] defaultMembers) {
            if (info.GetIndexParameters().Length > 0) {
                foreach (MemberInfo member in defaultMembers) {
                    if (member == info) {
                        AddIndexer(info);
                        return;
                    }
                }
            } else {
                // properties can have conflicting accessibility.  Generate public or private
                // accessors as necessary.
                string getName = null, setName = null;
                MethodInfo getter = info.GetGetMethod(true), setter = info.GetSetMethod(true);

                // don't add it if we only have explicit interfaces, we'll add
                // those later.
                if (getter != null) {
                    if (IsExplicitInterfaceImpl(getter) &&
                        (setter == null || IsExplicitInterfaceImpl(setter))) {
                        return;
                    }
                } else if (setter != null && IsExplicitInterfaceImpl(setter)) {
                    return;
                }

                NameType getterNt = NameType.None, setterNt = NameType.None;

                if (getter != null) getterNt = NameConverter.TryGetName(this, info, getter, out getName);
                if (setter != null) setterNt = NameConverter.TryGetName(this, info, setter, out setName);
                if (getterNt == NameType.None && setterNt == NameType.None) return; // both private

                if (getName == setName) {
                    // both public or both protected ( we don't support mixing & matching PythonName on properties)
                    dict[SymbolTable.StringToId(getName)] = new ReflectedProperty(info, getter, setter, !IsPythonType ? NameType.PythonProperty : getterNt);
                } else {
                    // one public, one protected, one doesn't exist, etc...
                    if (getterNt != NameType.None) dict[SymbolTable.StringToId(getName)] = new ReflectedProperty(info, getter, null, !IsPythonType ? NameType.PythonProperty : getterNt);
                    if (setterNt != NameType.None) dict[SymbolTable.StringToId(setName)] = new ReflectedProperty(info, null, setter, !IsPythonType ? NameType.PythonProperty : setterNt);
                }
            }
        }

        private void AddReflectedEvent(EventInfo info) {
            dict[SymbolTable.StringToId(info.Name)] = new ReflectedEvent(info, null, IsPythonType);
        }

        private void AddNestedType(Type type) {
            string name;
            NameType nt = NameConverter.TryGetName(this, type, out name);
            if (nt == NameType.None) return;
            else if (nt == NameType.Type) dict[SymbolTable.StringToId(name)] = Ops.GetDynamicTypeFromClsOnlyType(type);
            else dict[SymbolTable.StringToId(name)] = Ops.GetDynamicTypeFromType(type);
        }

        //		//??? don't like this design
        private void AddProtocolMethod(string pythonName, string methodName, NameType nameType) {
            if (dict.ContainsKey(SymbolTable.StringToId(pythonName)))
                return;

            object meth;
            FunctionType functionType = FunctionType.Method | FunctionType.SkipThisCheck;
            MethodInfo methodInfo = typeof(InstanceOps).GetMethod(methodName);
            if (nameType == NameType.PythonMethod) functionType |= FunctionType.PythonVisible;

            meth = BuiltinFunction.MakeMethod(pythonName, methodInfo, functionType).GetDescriptor();

            Debug.Assert(meth != null);

            dict[SymbolTable.StringToId(pythonName)] = meth;
        }

        private Type GetCannonicalType() {

            Type type = this.type;

            OpsReflectedType opsReflectedType = this as OpsReflectedType;

            if (opsReflectedType != null && opsReflectedType.GetTypeToExtend() != null)
                type = opsReflectedType.GetTypeToExtend();

            else if (effectivePythonType != null)
                type = effectivePythonType.GetCannonicalType();

            Debug.Assert(type != null);
            return type;
        }
        #endregion

        #region DynamicType overrides

        protected override Tuple CalculateMro(Tuple baseClasses) {
            // should always be the same for ReflectedTypes
            Debug.Assert(baseClasses.Equals(BaseClasses));

            if (effectivePythonType != null) {
                return effectivePythonType.MethodResolutionOrder;
            } else {
                return base.CalculateMro(baseClasses);
            }
        }

        public override Tuple BaseClasses {
            [PythonName("__bases__")]
            get {
                if (bases == null) {
                    if (type.BaseType == null) {
                        bases = Tuple.MakeTuple();
                    } else if (type.BaseType == typeof(ValueType)) {
                        bases = Tuple.MakeTuple(TypeCache.Object);
                    } else {
                        bases = Tuple.MakeTuple(Ops.GetDynamicTypeFromType(type.BaseType));
                    }
                }
                return bases;
            }
            [PythonName("__bases__")]
            set {
                throw Ops.TypeError("can't set bases for {0} '{1}'", TypeCategoryDescription, __name__);
            }
        }

        public override bool IsPythonType {
            get {
                if (isPythonTypeChecked) {
                    return isPythonType;
                }

                isPythonType = type.IsDefined(typeof(PythonTypeAttribute), false);
                isPythonTypeChecked = true;
                return isPythonType;
            }
        }

        public bool IsClsType {
            get {
                return clsOnly;
            }
        }

        protected override string TypeCategoryDescription {
            get {
                if (IsPythonType) return "built-in type";
                else return "CLI type";
            }
        }

        public override void Initialize() {
            if (!initialized) {
                base.Initialize();

                bool isOps = OpsReflectedType.OpsTypeToType.ContainsKey(type);
                lock (this) {
                    if (initialized) return;

                    dict = new FieldIdDict();

                    // if we're an ops type don't add our members to ourself
                    // (this happens if the user gets ahold of FloatOps and then
                    // trys to call a method on it).
                    if (isOps) {
                        initialized = true;
                        return;
                    }

                    MemberInfo[] defaultMembers = type.GetDefaultMembers();
                    BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

                    foreach (MethodInfo mi in type.GetMethods(bf)) {
                        AddReflectedMethod(mi, defaultMembers);
                    }

                    if (type != typeof(object)) {
                        Type curType = type.BaseType;
                        while (curType != null && curType != typeof(object) && curType != typeof(ValueType)) {
                            foreach (MethodInfo mi in curType.GetMethods(bf & ~BindingFlags.Instance)) {
                                AddReflectedMethod(mi, defaultMembers);
                            }
                            curType = curType.BaseType;
                        }
                    }

                    foreach (FieldInfo fi in type.GetFields(bf)) {
                        AddReflectedField(fi);
                    }

                    foreach (PropertyInfo pi in type.GetProperties(bf)) {
                        AddReflectedProperty(pi, defaultMembers);
                    }

                    foreach (EventInfo pi in type.GetEvents()) {
                        AddReflectedEvent(pi);
                    }

                    foreach (Type ty in type.GetNestedTypes(bf)) {
                        AddNestedType(ty);
                    }

                    AddComInterfaceMethods();

                    AddEnumOperators();

                    AddOps();

                    AddPythonProtocolMethods();

                    CreateInitCode();

                    AddProtocolWrappers();

                    AddDocumentation();

                    if (!dict.ContainsKey(SymbolTable.Dict)) {
                        dict[SymbolTable.Dict] = new DictWrapper(this);
                    }
                    initialized = true;

                    AddModule();
                }
            }
        }

        private void AddComInterfaceMethods() {
            // interfaces can't have explicitly implemented interfaces
            if (type.IsInterface) return;

            foreach (Type ty in type.GetInterfaces()) {
                // GetInterfaceMap fails on array types
                if (!type.IsArray && !ty.IsGenericParameter) {
                    InterfaceMapping mapping = type.GetInterfaceMap(ty);
                    for (int i = 0; i < mapping.TargetMethods.Length; i++) {
                        MethodInfo mi = mapping.TargetMethods[i];

                        if (mi == null) {
                            // COM objects can have interfaces that they don't appear
                            // to implement.  When that happens our target method is 
                            // null, but the interface method actually exists (we just need
                            // to QI for it).  For those we store the interfaces method
                            // directly into our dynamic type so the user can still call
                            // the appropriate method directly from the type.
                            Debug.Assert(type.IsCOMObject);
                            StoreReflectedMethod(mapping.InterfaceMethods[i].Name,
                                mapping.InterfaceMethods[i],
                                NameType.PythonMethod);
                            continue;
                        }
                    }
                }
            }
        }

        #endregion


        #region DynamicType overrides

        /// <summary>
        /// This has Python's semantics that a type issubclassof itself, this is different than Type.IsSubclassOf
        /// </summary>
        public override bool IsSubclassOf(object other) {

            if (other == this)
                return true;

            ReflectedType rtOther = other as ReflectedType;

            if (rtOther == null)
                return false;

            Type otherType = rtOther.GetCannonicalType();
            Type thisType = GetCannonicalType();

            if (thisType == otherType || thisType.IsSubclassOf(otherType))
                return true;

            return false;
        }

        // This is either the CLI type or interface represented by this ReflectedType
        // It can also return null if Python does not allow extending the given DynamicType.
        public virtual Type GetTypeToExtend() {
            Type typeToExtend = type;

            if (typeToExtend == typeof(DynamicType) || typeToExtend == typeof(ReflectedType)) {
                // This is for code like:
                //     class MyMetaType(type): pass
                Debug.Assert(this == Ops.GetDynamicTypeFromType(typeof(DynamicType)) ||
                    this == Ops.GetDynamicTypeFromType(typeof(ReflectedType)));
                typeToExtend = typeof(UserType);
            }

            if (typeToExtend == typeof(PythonFunction) || typeToExtend == typeof(Method)) {
                // Disallow inheriting from "function" or "instance method"
                typeToExtend = null;
            }

            return typeToExtend;
        }

        public override Type GetTypesToExtend(out IList<Type> interfacesToExtend) {
            Type typeToExtend = GetTypeToExtend();

            if (typeToExtend != null && typeToExtend.IsInterface) {
                interfacesToExtend = new List<Type>(1);
                interfacesToExtend.Add(typeToExtend);
                return typeof(object);
            } else {
                interfacesToExtend = DynamicType.EmptyListOfInterfaces;
                return typeToExtend;
            }
        }

        public override bool TryGetAttr(ICallerContext context, object self, SymbolId name, out object ret) {
            if (prependedAttrs != null && prependedAttrs.TryGetAttr(self, name, out ret)) {
                return true;
            }

            if (TryBaseGetAttr(context, self, name, out ret)) return true;

            if (appendedAttrs != null && appendedAttrs.TryGetAttr(self, name, out ret)) {
                return true;
            }

            return false;
        }

        public override List GetAttrNames(ICallerContext context, object self) {
            List ret;

            if (prependedAttrs != null) {
                ret = prependedAttrs.GetAttrNames(self);
                ret.AppendListNoLockNoDups(base.GetAttrNames(context, self));
            } else {
                ret = base.GetAttrNames(context, self);
            }

            if (appendedAttrs != null) {
                ret.AppendListNoLockNoDups(appendedAttrs.GetAttrNames(self));
            }

            return ret;
        }


        protected override void RawSetSlot(SymbolId name, object value) {
            object slot;
            bool slotExists = TryGetAttr(DefaultContext.Default, name, out slot);
            ThrowAttributeError(slotExists, name);
        }

        #endregion

        #region ICustomAttributes helpers
        /// <summary>
        /// Types implementing ICustomAttributes need special handling for attribute access since they have their own dictionary
        /// </summary>

        internal void SetAttrWithCustomDict(ICallerContext context, ICustomAttributes self, IAttributesDictionary selfDict, SymbolId name, object value) {
            Debug.Assert(IsInstanceOfType(self));

            if (name == SymbolTable.Dict)
                throw Ops.AttributeErrorForReadonlyAttribute(__name__.ToString(), name);

            object dummy;
            if (TryLookupSlot(context, name, out dummy)) {
                SetAttr(context, self, name, value);
            } else {
                selfDict[name] = value;
            }
        }

        internal bool DeleteAttrWithCustomDict(ICallerContext context, ICustomAttributes self, IAttributesDictionary selfDict, SymbolId name) {
            Debug.Assert(IsInstanceOfType(self));

            if (name == SymbolTable.Dict)
                throw Ops.AttributeErrorForReadonlyAttribute(__name__.ToString(), name);

            if (selfDict.ContainsKey(name)) {
                selfDict.Remove(name);
                return true;
            }

            object dummy;
            if (TryLookupSlot(context, name, out dummy)) {
                selfDict[name] = Uninitialized.instance;
                return true;
            } else {
                return selfDict.Remove(name);
            }
        }

        internal IDictionary<object, object> GetAttrDictWithCustomDict(ICallerContext context, ICustomAttributes self, IAttributesDictionary selfDict) {
            Debug.Assert(IsInstanceOfType(self));

            // Get the attributes from the instance
            Dict res = new Dict(selfDict);

            // Add the attributes from the type
            Dict typeDict = base.GetAttrDict(context, self);
            foreach (KeyValuePair<object, object> pair in (IDictionary<object, object>)typeDict) {
                res.Add(pair);
            }

            return res;
        }

        #endregion

        #region Object overrides

        [PythonName("__str__")]
        public override string ToString() {
            if (effectivePythonType != null) return effectivePythonType.ToString();
            return string.Format("<type {0}>", Ops.StringRepr(GetTypeDisplayName()));
        }

        public override bool Equals(object obj) {
            if (effectivePythonType != null) {
                if ((object)effectivePythonType == obj) return true;
                if (obj is ReflectedType && (object)((ReflectedType)obj).effectivePythonType == (object)effectivePythonType) return true;
            } else {
                if (obj is ReflectedType && (object)((ReflectedType)obj).effectivePythonType == (object)this) return true;
            }
            return (object)this == obj;
        }

        public override int GetHashCode() {
            if (effectivePythonType != null) return effectivePythonType.GetHashCode();
            else return base.GetHashCode();
        }
        #endregion


        #region ICustomAttributes Overrides

        /// <summary>
        /// Instances of builtin and CLI types do not have a __dict__ attribute.
        /// </summary>
        public override Dict GetAttrDict(ICallerContext context, object self) {
            throw Ops.AttributeErrorForMissingAttribute(__name__.ToString(), SymbolTable.Dict);
        }

        #endregion

        #region IDynamicObject Members

        public override DynamicType GetDynamicType() {
            return TypeCache.ReflectedType;
        }

        #endregion

        public virtual object this[object index] {
            get {
                Type[] types = GetTypesFromTuple(index);

                return Ops.GetDynamicTypeFromType(type.MakeGenericType(types));
            }
        }

        protected static Type[] GetTypesFromTuple(object index) {
            Tuple typesTuple = index as Tuple;
            Type[] types;
            if (typesTuple != null) {
                types = new Type[typesTuple.Count];
                int i = 0;
                foreach (object t in typesTuple) {
                    types[i++] = Converter.ConvertToType(t);
                }
            } else {
                types = new Type[1];
                types[0] = Converter.ConvertToType(index);
            }

            return types;
        }

        #region IContextAwareMember Members

        bool IContextAwareMember.IsVisible(ICallerContext context) {
            return !clsOnly || (context.ContextFlags & CallerContextAttributes.ShowCls) != 0;
        }

        #endregion

        internal string GetTypeDisplayName() {
            if (type.IsGenericTypeDefinition) {
                StringBuilder res = new StringBuilder(__name__.ToString());
                res.Append('[');
                Type[] generics = type.GetGenericArguments();
                string comma = "";
                for (int i = 0; i < generics.Length; i++) {
                    res.Append(comma);
                    res.Append(generics[i].Name);
                    comma = ", ";
                }
                res.Append(']');
                return res.ToString();
            }
            return __name__.ToString();
        }
    }

    /// <summary>
    /// A TypeCollision is used when we have a collsion between
    /// two types with the same name.  Currently this is only possible w/ generic
    /// methods that should logically have arity as a portion of their name.
    /// 
    /// The TypeCollision provides an indexer but also is a real type.  When used
    /// as a real type it is the non-generic form of the type.
    /// 
    /// The indexer allows the user to disambiguate between the generic and
    /// non-generic versions.  Therefore users must always provide additional
    /// information to get the generic version.
    /// </summary>
    [PythonType(typeof(DynamicType))]
    public class TypeCollision : ReflectedType {
        List<ReflectedType> types;

        public TypeCollision(Type t)
            : base(t) {
            types = new List<ReflectedType>();
        }

        /// <summary>
        /// Indexer for generic parameter resolution.  We bind to one of the generic versions
        /// available in this type collision.  A user can also do someType[()] to force to
        /// bind to the non-generic version, but we will always present the non-generic version
        /// when no bindings are available.
        /// </summary>
        public override object this[object index] {
            get {
                Type[] bindRequest = ReflectedType.GetTypesFromTuple(index);

                // Try our base type first (it's the only possible non-generic)
                if (TryGenericBind(bindRequest, type)) {
                    if (bindRequest.Length == 0) return this;

                    return Ops.GetDynamicTypeFromType(type.MakeGenericType(bindRequest));
                }

                for (int i = 0; i < types.Count; i++) {
                    // next try all of our other generics, until we find
                    // an arity that matches.
                    Debug.Assert(types[i].type.ContainsGenericParameters);

                    if (TryGenericBind(bindRequest, types[i].type)) {
                        return Ops.GetDynamicTypeFromType(types[i].type.MakeGenericType(bindRequest));
                    }
                }

                throw Ops.ValueError("could not find compatible generic type for {0} type args", bindRequest.Length);
            }
        }

        internal void UpdateType(Type t) {
            Debug.Assert(t.ContainsGenericParameters,
                String.Format("Expected only generics to be added: {0}, non generics need to CloneWithNewBase", t.Name));

            int genericCount = GetGenericCount(t.GetGenericArguments());
            for (int i = 0; i < types.Count; i++) {
                if (genericCount == GetGenericCount(types[i].type.GetGenericArguments())) {
                    types[i] = (ReflectedType)Ops.GetDynamicTypeFromType(t);
                    return;
                }
            }

            types.Add((ReflectedType)Ops.GetDynamicTypeFromType(t));
        }

        /// <summary> Creates a new TypeCollision using this types generic list w/ the specified
        /// non-generic type as the TypeCollision's ReflectedType.</summary>
        internal TypeCollision CloneWithNewBase(Type newBase) {
            Debug.Assert(!newBase.ContainsGenericParameters);

            TypeCollision res = new TypeCollision(newBase);
            res.types.AddRange(types);

            if (type.ContainsGenericParameters) {
                // if we have a collision between two non-generic
                // types new newer type simply wins.
                res.types.Add((ReflectedType)Ops.GetDynamicTypeFromType(type));
            }
            return res;
        }

        /// <summary> Determines if the bind request matches the arity of the provided type</summary>
        private bool TryGenericBind(Type[] bindRequest, Type t) {
            if (bindRequest.Length == 0 && !t.ContainsGenericParameters)
                return true;

            int genericCount = GetGenericCount(t.GetGenericArguments());

            if (genericCount == bindRequest.Length)
                return true;

            return false;
        }

        /// <summary> Gets the number of unbound generic arguments exist in a type array</summary>
        private int GetGenericCount(Type[] genericArgs) {
            int genericCount = 0;
            for (int i = 0; i < genericArgs.Length; i++)
                if (genericArgs[i].IsGenericParameter)
                    genericCount++;
            return genericCount;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder("<types ");
            sb.Append(Ops.StringRepr(__name__));
            for (int i = 0; i < types.Count; i++) {
                sb.Append(", ");
                sb.Append(Ops.StringRepr(types[i].GetTypeDisplayName()));
            }
            sb.Append(">");

            return sb.ToString();
        }
    }
}
