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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Scripting;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using System.Threading;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Types {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    // OldClass represents the type of old-style Python classes (which could not inherit from 
    // built-in Python types). 
    // 
    // Object instances of old-style Python classes are represented by OldInstance.
    // 
    // UserType is the equivalent of OldClass for new-style Python classes (which can inherit 
    // from built-in types).

    [Flags]
    enum OldClassAttributes {
        None = 0x00,
        HasFinalizer = 0x01,
        HasSetAttr = 0x02,
        HasDelAttr = 0x04,
    }

    [PythonSystemType("classobj")]
    [Serializable]
    public sealed class OldClass :
#if !SILVERLIGHT // ICustomTypeDescriptor
 ICustomTypeDescriptor,
#endif
 ICodeFormattable,
        IMembersList,
        IDynamicObject,
        IOldDynamicObject {

        [NonSerialized]
        private List<OldClass> _bases;
        private PythonType _type = null;

        public PythonDictionary __dict__;
        private int _attrs;  // actually OldClassAttributes - losing type safety for thread safety
        internal object __name__;

        [MultiRuntimeAware]
        private static int _namesVersion;
        private int _optimizedInstanceNamesVersion;
        private SymbolId[] _optimizedInstanceNames;

        public static string __doc__ = "classobj(name, bases, dict)";

        public static object __new__(CodeContext/*!*/ context, [NotNull]PythonType cls, string name, PythonTuple bases, IAttributesCollection dict) {
            if (cls != TypeCache.OldClass) throw PythonOps.TypeError("{0} is not a subtype of classobj", cls.Name);

            if (!dict.ContainsKey(Symbols.Module)) {
                object moduleValue;
                if (context.GlobalScope.TryGetName(Symbols.Name, out moduleValue)) {
                    dict[Symbols.Module] = moduleValue;
                }
            }

            foreach (object o in bases) {
                if (o is PythonType) {
                    return PythonOps.MakeClass(context, name, bases._data, String.Empty, dict);
                }
            }

            return new OldClass(name, bases, dict, String.Empty);
        }

        internal OldClass(string name, PythonTuple bases, IAttributesCollection dict)
            : this(name, bases, dict, "") {
        }

        internal OldClass(string name, PythonTuple bases, IAttributesCollection dict, string instanceNames) {
            _bases = ValidateBases(bases);

            Init(name, dict, instanceNames);
        }

        private void Init(string name, IAttributesCollection dict, string instanceNames) {
            __name__ = name;

            InitializeInstanceNames(instanceNames);

            __dict__ = dict as PythonDictionary ?? new PythonDictionary(new WrapperDictionaryStorage(dict));

            if (!__dict__._storage.Contains(Symbols.Doc)) {
                __dict__._storage.Add(Symbols.Doc, null);
            }

            if (__dict__._storage.Contains(Symbols.Unassign)) {
                HasFinalizer = true;
            }

            if (__dict__._storage.Contains(Symbols.SetAttr)) {
                HasSetAttr = true;
            }

            if (__dict__._storage.Contains(Symbols.DelAttr)) {
                HasDelAttr = true;
            }
        }

#if !SILVERLIGHT // SerializationInfo
        private OldClass(SerializationInfo info, StreamingContext context) {
            _bases = (List<OldClass>)info.GetValue("__class__", typeof(List<OldClass>));
            __name__ = info.GetValue("__name__", typeof(object));
            __dict__ = new PythonDictionary();

            InitializeInstanceNames(""); //TODO should we serialize the optimization data

            List<object> keys = (List<object>)info.GetValue("keys", typeof(List<object>));
            List<object> values = (List<object>)info.GetValue("values", typeof(List<object>));
            for (int i = 0; i < keys.Count; i++) {
                __dict__[keys[i]] = values[i];
            }

            if (__dict__.has_key("__del__")) HasFinalizer = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context")]
        private void GetObjectData(SerializationInfo info, StreamingContext context) {
            ContractUtils.RequiresNotNull(info, "info");

            info.AddValue("__bases__", _bases);
            info.AddValue("__name__", __name__);

            List<object> keys = new List<object>();
            List<object> values = new List<object>();
            foreach (KeyValuePair<object, object> kvp in __dict__._storage.GetItems()) {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }

            info.AddValue("keys", keys);
            info.AddValue("values", values);
        }
#endif

        private void InitializeInstanceNames(string instanceNames) {
            if (instanceNames.Length == 0) {
                _optimizedInstanceNames = SymbolId.EmptySymbols;
                _optimizedInstanceNamesVersion = 0;
                return;
            }

            string[] names = instanceNames.Split(',');
            _optimizedInstanceNames = new SymbolId[names.Length];
            for (int i = 0; i < names.Length; i++) {
                _optimizedInstanceNames[i] = SymbolTable.StringToId(names[i]);
            }
            _optimizedInstanceNamesVersion = Interlocked.Increment(ref _namesVersion);
        }

        internal SymbolId[] OptimizedInstanceNames {
            get { return _optimizedInstanceNames; }
        }

        internal int OptimizedInstanceNamesVersion {
            get { return _optimizedInstanceNamesVersion; }
        }

        public string Name {
            get { return __name__.ToString(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        internal bool TryLookupSlot(SymbolId name, out object ret) {
            if (__dict__._storage.TryGetValue(name, out ret)) {
                return true;
            }

            // bases only ever contains OldClasses (tuples are immutable, and when assigned
            // to we verify the types in the Tuple)
            foreach (OldClass c in _bases) {
                if (c.TryLookupSlot(name, out ret)) return true;
            }

            ret = null;
            return false;
        }

        internal bool TryLookupOneSlot(SymbolId name, out object ret) {
            return __dict__._storage.TryGetValue(name, out ret);
        }

        internal string FullName {
            get { return __dict__["__module__"].ToString() + '.' + __name__; }
        }


        public List<OldClass> BaseClasses {
            get {
                return _bases;
            }
        }

        internal object GetOldStyleDescriptor(CodeContext context, object self, object instance, object type) {
            PythonTypeSlot dts = self as PythonTypeSlot;
            object callable;
            if (dts != null && dts.TryGetValue(context, instance, TypeObject, out callable)) {
                return callable;
            }

            return PythonOps.GetUserDescriptor(self, instance, type);
        }

        public bool HasFinalizer {
            get {
                return (_attrs & (int)OldClassAttributes.HasFinalizer) != 0;
            }
            internal set {
                int oldAttrs, newAttrs;
                do {
                    oldAttrs = _attrs;
                    newAttrs = value ? oldAttrs | ((int)OldClassAttributes.HasFinalizer) : oldAttrs & ((int)~OldClassAttributes.HasFinalizer);
                } while (Interlocked.CompareExchange(ref _attrs, newAttrs, oldAttrs) != oldAttrs);
            }
        }

        internal bool HasSetAttr {
            get {
                return (_attrs & (int)OldClassAttributes.HasSetAttr) != 0;
            }
            set {
                int oldAttrs, newAttrs;
                do {
                    oldAttrs = _attrs;
                    newAttrs = value ? oldAttrs | ((int)OldClassAttributes.HasSetAttr) : oldAttrs & ((int)~OldClassAttributes.HasSetAttr);
                } while (Interlocked.CompareExchange(ref _attrs, newAttrs, oldAttrs) != oldAttrs);
            }
        }

        internal bool HasDelAttr {
            get {
                return (_attrs & (int)OldClassAttributes.HasDelAttr) != 0;
            }
            set {
                int oldAttrs, newAttrs;
                do {
                    oldAttrs = _attrs;
                    newAttrs = value ? oldAttrs | ((int)OldClassAttributes.HasDelAttr) : oldAttrs & ((int)~OldClassAttributes.HasDelAttr);
                } while (Interlocked.CompareExchange(ref _attrs, newAttrs, oldAttrs) != oldAttrs);
            }
        }

        public override string ToString() {
            return FullName;
        }

        #region Calls

        // Calling an OldClass instance means instantiating that class and invoking the __init__() member if 
        // it's defined.

        // OldClass impls IOldDynamicObject. But May wind up here still if IDynamicObj doesn't provide a rule (such as for list sigs).
        // If our IOldDynamicObject implementation is complete, we can then remove these Call methods.
        [SpecialName]
        public object Call(CodeContext context, [NotNull]params object[] args\u00F8) {
            OldInstance inst = new OldInstance(context, this);
            object value;
            // lookup the slot directly - we don't go through __getattr__
            // until after the instance is created.
            if (TryLookupSlot(Symbols.Init, out value)) {
                PythonOps.CallWithContext(context, GetOldStyleDescriptor(context, value, inst, this), args\u00F8);
            } else if (args\u00F8.Length > 0) {
                MakeCallError();
            }
            return inst;
        }

        [SpecialName]
        public object Call(CodeContext context, [ParamDictionary] IAttributesCollection dict\u00F8, [NotNull]params object[] args\u00F8) {
            OldInstance inst = new OldInstance(context, this);
            object meth;
            if (PythonOps.TryGetBoundAttr(inst, Symbols.Init, out meth)) {
                PythonCalls.CallWithKeywordArgs(context, meth, args\u00F8, dict\u00F8);
            } else if (dict\u00F8.Count > 0 || args\u00F8.Length > 0) {
                MakeCallError();
            }
            return inst;
        }

        #endregion // calls
        
        internal PythonType TypeObject {
            get {
                if (_type == null) {
                    Interlocked.CompareExchange(ref _type, new PythonType(this), null);
                }
                return _type;
            }
        }

        private List<OldClass> ValidateBases(object value) {
            PythonTuple t = value as PythonTuple;
            if (t == null) throw PythonOps.TypeError("__bases__ must be a tuple object");

            List<OldClass> res = new List<OldClass>(t.__len__());
            foreach (object o in t) {
                OldClass oc = o as OldClass;
                if (oc == null) throw PythonOps.TypeError("__bases__ items must be classes (got {0})", PythonTypeOps.GetName(o));

                if (oc.IsSubclassOf(this)) {
                    throw PythonOps.TypeError("a __bases__ item causes an inheritance cycle");
                }

                res.Add(oc);
            }
            return res;
        }

        internal object GetMember(CodeContext context, SymbolId name) {
            object value;

            if (!TryGetBoundCustomMember(context, name, out value)) {
                throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'", Name, SymbolTable.IdToString(name));
            }

            return value;
        }

        internal bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            if (name == Symbols.Bases) { value = PythonTuple.Make(_bases); return true; }
            if (name == Symbols.Name) { value = __name__; return true; }
            if (name == Symbols.Dict) {
                //!!! user code can modify __del__ property of __dict__ behind our back
                HasDelAttr = HasSetAttr = true;  // pessimisticlly assume the user is setting __setattr__ in the dict
                value = __dict__; return true;
            }

            if (TryLookupSlot(name, out value)) {
                value = GetOldStyleDescriptor(context, value, null, this);
                return true;
            }
            return false;
        }

        internal bool DeleteCustomMember(CodeContext context, SymbolId name) {
            if (!__dict__._storage.Remove(SymbolTable.IdToString(name))) {
                throw PythonOps.AttributeError("{0} is not a valid attribute", SymbolTable.IdToString(name));
            }

            if (name == Symbols.Unassign) {
                HasFinalizer = false;
            }
            if (name == Symbols.SetAttr) {
                HasSetAttr = false;
            }
            if (name == Symbols.DelAttr) {
                HasDelAttr = false;
            }

            return true;
        }

        internal static void RecurseAttrHierarchy(OldClass oc, IDictionary<object, object> attrs) {
            foreach (KeyValuePair<object, object> kvp in oc.__dict__._storage.GetItems()) {
                if (!attrs.ContainsKey(kvp.Key)) {
                    attrs.Add(kvp.Key, kvp.Key);
                }
            }

            //  recursively get attrs in parent hierarchy
            if (oc._bases.Count != 0) {
                foreach (OldClass parent in oc._bases) {
                    RecurseAttrHierarchy(parent, attrs);
                }
            }
        }

        #region IMembersList Members

        IList<object> IMembersList.GetMemberNames(CodeContext context) {
            PythonDictionary attrs = new PythonDictionary(__dict__);
            RecurseAttrHierarchy(this, attrs);
            return PythonOps.MakeListFromSequence(attrs);
        }

        #endregion

        internal bool IsSubclassOf(object other) {
            if (this == other) return true;

            OldClass dt = other as OldClass;
            if (dt == null) return false;

            List<OldClass> bases = _bases;
            foreach (OldClass bc in bases) {
                if (bc.IsSubclassOf(other)) {
                    return true;
                }
            }
            return false;
        }

        #region ICustomTypeDescriptor Members
#if !SILVERLIGHT // ICustomTypeDescriptor

        AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return CustomTypeDescHelpers.GetAttributes(this);
        }

        string ICustomTypeDescriptor.GetClassName() {
            return CustomTypeDescHelpers.GetClassName(this);
        }

        string ICustomTypeDescriptor.GetComponentName() {
            return CustomTypeDescHelpers.GetComponentName(this);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return CustomTypeDescHelpers.GetConverter(this);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return CustomTypeDescHelpers.GetDefaultEvent(this);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return CustomTypeDescHelpers.GetDefaultProperty(this);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return CustomTypeDescHelpers.GetEditor(this, editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
            return CustomTypeDescHelpers.GetEvents(attributes);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return CustomTypeDescHelpers.GetEvents(this);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
            return CustomTypeDescHelpers.GetProperties(attributes);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            return CustomTypeDescHelpers.GetProperties(this);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            return CustomTypeDescHelpers.GetPropertyOwner(this, pd);
        }
#endif
        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return string.Format("<class {0} at {1}>", FullName, PythonOps.HexId(this));
        }

        #endregion

        #region IOldDynamicObject Members

        public RuleBuilder<T> GetRule<T>(OldDynamicAction action, CodeContext context, object[] args) where T : class {
            switch (action.Kind) {
                case DynamicActionKind.GetMember:
                    return MakeGetMemberRule<T>((OldGetMemberAction)action, context, args);
                case DynamicActionKind.SetMember:
                    return MakeSetMemberRule<T>((OldSetMemberAction)action, context, args);
                case DynamicActionKind.DeleteMember:
                    return MakeDelMemberRule<T>((OldDeleteMemberAction)action, context, args);
                case DynamicActionKind.CreateInstance:
                case DynamicActionKind.Call:
                    return MakeCallRule<T>((OldCallAction)action, context, args);
                case DynamicActionKind.DoOperation:
                    return MakeDoOperationRule<T>((OldDoOperationAction)action, context, args);
                default: return null;
            }
        }

        private RuleBuilder<T> MakeDoOperationRule<T>(OldDoOperationAction doOperationAction, CodeContext context, object[] args) where T : class {
            switch (doOperationAction.Operation) {
                case Operators.IsCallable:
                    return PythonBinderHelper.MakeIsCallableRule<T>(context, this, true);
            }
            return null;
        }

        private static RuleBuilder<T> MakeCallRule<T>(OldCallAction action, CodeContext context, object[] args) where T : class {
            // This rule only handles simple signatures. Fallback on MethodBinder to handle complex signatures 
            // such as keyword args.
            if (!action.Signature.IsSimple) return null; 

            RuleBuilder<T> rule = new RuleBuilder<T>();

            Expression[] exprArgs = new Expression[args.Length - 1];
            for (int i = 0; i < args.Length - 1; i++) {
                exprArgs[i] = rule.Parameters[i + 1];
            }

            // TODO: If we know __init__ wasn't present we could construct the OldInstance directly.
            VariableExpression tmp = rule.GetTemporary(typeof(object), "init");
            VariableExpression instTmp = rule.GetTemporary(typeof(object), "inst");
            rule.Test = rule.MakeTypeTest(typeof(OldClass), 0);
            rule.Target =
                rule.MakeReturn(context.LanguageContext.Binder,
                    Ast.Comma(
                        Ast.Assign(
                            instTmp,
                            Ast.New(
                                typeof(OldInstance).GetConstructor(new Type[] { typeof(CodeContext), typeof(OldClass) }),
                                rule.Context,
                                Ast.ConvertHelper(rule.Parameters[0], typeof(OldClass))
                            )
                        ),
                        Ast.Condition(
                            Ast.Call(
                                Ast.ConvertHelper(rule.Parameters[0], typeof(OldClass)),
                                typeof(OldClass).GetMethod("TryLookupInit"),
                                instTmp,
                                tmp
                            ),
                            AstUtils.Call(
                                action,
                                typeof(object),
                                ArrayUtils.Insert<Expression>(rule.Context, tmp, ArrayUtils.RemoveFirst(rule.Parameters))
                            ),
                            // Checking the Parameter array directly here only works for simple signatures.
                            // It would get confused by cases like C(*()), C(**{}), or C(*E(), **{}), which could all
                            // bind against C(). 
                            rule.Parameters.Count != 1 ?
                                (Expression)Ast.Call(
                                    Ast.ConvertHelper(rule.Parameters[0], typeof(OldClass)),
                                    typeof(OldClass).GetMethod("MakeCallError")
                                    ) :
                                Ast.Null()
                        ),
                        instTmp
                    )
                );

            return rule;
        }

        public bool TryLookupInit(object inst, out object ret) {
            if (TryLookupSlot(Symbols.Init, out ret)) {
                ret = GetOldStyleDescriptor(DefaultContext.Default, ret, inst, this);
                return true;
            }

            return false;
        }

        public object MakeCallError() {
            // Normally, if we have an __init__ method, the method binder detects signature mismatches.
            // This can happen when a class does not define __init__ and therefore does not take any arguments.
            // Beware that calls like F(*(), **{}) have 2 arguments but they're empty and so it should still
            // match against def F(). 
            throw PythonOps.TypeError("this constructor takes no arguments");
        }

        private static RuleBuilder<T> MakeSetMemberRule<T>(OldSetMemberAction action, CodeContext context, object[] args) where T : class {
            RuleBuilder<T> rule = new RuleBuilder<T>();
            rule.MakeTest(typeof(OldClass));
            Expression call;

            if (action.Name == Symbols.Bases) {
                call = Ast.Call(
                    Ast.ConvertHelper(rule.Parameters[0], typeof(OldClass)),
                    typeof(OldClass).GetMethod("SetBases"),
                    Ast.ConvertHelper(rule.Parameters[1], typeof(object))
                );
            } else if (action.Name == Symbols.Name) {
                call = Ast.Call(
                    Ast.ConvertHelper(rule.Parameters[0], typeof(OldClass)),
                    typeof(OldClass).GetMethod("SetName"),
                    Ast.ConvertHelper(rule.Parameters[1], typeof(object))
                );
            } else if (action.Name == Symbols.Dict) {
                call = Ast.Call(
                    Ast.ConvertHelper(rule.Parameters[0], typeof(OldClass)),
                    typeof(OldClass).GetMethod("SetDictionary"),
                    Ast.ConvertHelper(rule.Parameters[1], typeof(object))
                );
            } else {
                call = Ast.Call(
                    Ast.ConvertHelper(rule.Parameters[0], typeof(OldClass)),
                    typeof(OldClass).GetMethod("SetNameHelper"),
                    Ast.Constant(action.Name),
                    Ast.ConvertHelper(rule.Parameters[1], typeof(object))
                );
            }

            rule.Target = rule.MakeReturn(context.LanguageContext.Binder, call);
            return rule;
        }

        public void SetBases(object value) {
            _bases = ValidateBases(value);
        }

        public void SetName(object value) {
            string n = value as string;
            if (n == null) throw PythonOps.TypeError("TypeError: __name__ must be a string object");
            __name__ = n;
        }

        public void SetDictionary(object value) {
            PythonDictionary d = value as PythonDictionary;
            if (d == null) throw PythonOps.TypeError("__dict__ must be set to dictionary");
            __dict__ = d;
        }

        public void SetNameHelper(SymbolId name, object value) {
            __dict__._storage.Add(name, value);

            if (name == Symbols.Unassign) {
                HasFinalizer = true;
            } else if (name == Symbols.SetAttr) {
                HasSetAttr = true;
            } else if (name == Symbols.DelAttr) {
                HasDelAttr = true;
            }
        }

        private static RuleBuilder<T> MakeDelMemberRule<T>(OldDeleteMemberAction action, CodeContext context, object[] args) where T : class {
            RuleBuilder<T> rule = new RuleBuilder<T>();
            rule.MakeTest(typeof(OldClass));
            rule.Target = rule.MakeReturn(context.LanguageContext.Binder,
                Ast.Call(
                    typeof(PythonOps).GetMethod("OldClassDeleteMember"),
                    rule.Context,
                    Ast.ConvertHelper(rule.Parameters[0], typeof(OldClass)),
                    Ast.Constant(action.Name)
                )
            );
            return rule;
        }

        private static RuleBuilder<T> MakeGetMemberRule<T>(OldGetMemberAction action, CodeContext context, object[] args) where T : class {
            RuleBuilder<T> rule = new RuleBuilder<T>();

            rule.MakeTest(typeof(OldClass));
            Expression target;

            if (action.Name == Symbols.Dict) {
                target = Ast.Comma(
                    Ast.Call(
                        Ast.Convert(rule.Parameters[0], typeof(OldClass)),
                        typeof(OldClass).GetMethod("DictionaryIsPublic")
                    ),
                    Ast.Field(
                        Ast.Convert(rule.Parameters[0], typeof(OldClass)),
                        typeof(OldClass).GetField("__dict__")
                    )
                );
            } else if (action.Name == Symbols.Bases) {
                target = Ast.Call(
                    typeof(PythonOps).GetMethod("OldClassGetBaseClasses"),
                    Ast.Convert(rule.Parameters[0], typeof(OldClass))
                );
            } else if (action.Name == Symbols.Name) {
                target = Ast.Property(
                    Ast.Convert(rule.Parameters[0], typeof(OldClass)),
                    typeof(OldClass).GetProperty("Name")
                );
            } else {
                if (action.IsNoThrow) {
                    VariableExpression tmp = rule.GetTemporary(typeof(object), "lookupVal");
                    target =
                        Ast.Condition(
                            Ast.Call(
                                Ast.Convert(rule.Parameters[0], typeof(OldClass)),
                                typeof(OldClass).GetMethod("TryLookupValue"),
                                rule.Context,
                                Ast.Constant(action.Name),
                                tmp
                            ),
                            tmp,
                            Ast.Convert(
                                Ast.Field(null, typeof(OperationFailed).GetField("Value")),
                                typeof(object)
                            )
                        );
                } else {
                    target = Ast.Call(
                        Ast.Convert(rule.Parameters[0], typeof(OldClass)),
                        typeof(OldClass).GetMethod("LookupValue"),
                        rule.Context,
                        Ast.Constant(action.Name)
                    );
                }
            }

            rule.Target = rule.MakeReturn(context.LanguageContext.Binder, target);

            return rule;
        }

        public object LookupValue(CodeContext context, SymbolId name) {
            object value;
            if (TryLookupValue(context, name, out value)) {
                return value;
            }

            throw PythonOps.AttributeErrorForMissingAttribute(this, name);
        }

        public bool TryLookupValue(CodeContext context, SymbolId name, out object value) {
            if (TryLookupSlot(name, out value)) {
                value = GetOldStyleDescriptor(context, value, null, this);
                return true;
            }

            return false;
        }

        public void DictionaryIsPublic() {
            HasDelAttr = true;
            HasSetAttr = true;
        }
        #endregion

        #region IDynamicObject Members

        MetaObject/*!*/ IDynamicObject.GetMetaObject(Expression/*!*/ parameter) {
            return new Binding.MetaOldClass(parameter, Restrictions.Empty, this);
        }

        #endregion
    }
}
