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
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Types {
    using Ast = Microsoft.Scripting.Ast.Ast;

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

    [PythonType("classobj")]
    [Serializable]
    public sealed class OldClass :
#if !SILVERLIGHT // ICustomTypeDescriptor
 ICustomTypeDescriptor,
#endif
 ICodeFormattable,
        ICustomMembers,
        IDynamicObject {

        [NonSerialized]
        private List<OldClass> _bases;
        private PythonType _type = null;

        public IAttributesCollection __dict__;
        private int _attrs;  // actually OldClassAttributes - losing type safety for thread safety
        internal object __name__;

        private static int _namesVersion;
        private int _optimizedInstanceNamesVersion;
        private SymbolId[] _optimizedInstanceNames;

        public OldClass(string name, PythonTuple bases, IAttributesCollection dict)
            : this(name, bases, dict, "") {
        }

        internal OldClass(string name, PythonTuple bases, IAttributesCollection dict, string instanceNames) {
            _bases = ValidateBases(bases);

            Init(name, dict, instanceNames);
        }

        internal OldClass(string name, List<OldClass> bases, IAttributesCollection dict, string instanceNames) {
            Assert.NotNullItems(bases);
            _bases = bases;
            Init(name, dict, instanceNames);
        }

        private void Init(string name, IAttributesCollection dict, string instanceNames) {
            __name__ = name;

            InitializeInstanceNames(instanceNames);

            __dict__ = new WrapperDictionary(dict);

            if (!__dict__.ContainsKey(Symbols.Doc)) {
                __dict__[Symbols.Doc] = null;
            }

            if (__dict__.ContainsKey(Symbols.Unassign)) {
                HasFinalizer = true;
            }

            if (__dict__.ContainsKey(Symbols.SetAttr)) {
                HasSetAttr = true;
            }

            if (__dict__.ContainsKey(Symbols.DelAttr)) {
                HasDelAttr = true;
            }
        }

#if !SILVERLIGHT // SerializationInfo
        private OldClass(SerializationInfo info, StreamingContext context) {
            _bases = (List<OldClass>)info.GetValue("__class__", typeof(List<OldClass>));
            __name__ = info.GetValue("__name__", typeof(object));
            __dict__ = new SymbolDictionary();

            InitializeInstanceNames(""); //TODO should we serialize the optimization data

            List<object> keys = (List<object>)info.GetValue("keys", typeof(List<object>));
            List<object> values = (List<object>)info.GetValue("values", typeof(List<object>));
            for (int i = 0; i < keys.Count; i++) {
                __dict__.AddObjectKey(keys[i], values[i]);
            }

            if (__dict__.ContainsKey(Symbols.Unassign)) HasFinalizer = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context")]
        private void GetObjectData(SerializationInfo info, StreamingContext context) {
            Contract.RequiresNotNull(info, "info");

            info.AddValue("__bases__", _bases);
            info.AddValue("__name__", __name__);

            List<object> keys = new List<object>();
            List<object> values = new List<object>();
            foreach (object o in __dict__.Keys) {
                keys.Add(o);
                object value;

                bool res = __dict__.TryGetObjectValue(o, out value);

                Debug.Assert(res);

                values.Add(value);
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
        public bool TryLookupSlot(SymbolId name, out object ret) {
            if (__dict__.TryGetValue(name, out ret)) {
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

        internal string FullName {
            get { return __dict__[Symbols.Module].ToString() + '.' + __name__; }
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

        // OldClass impls IDynamicObject. But May wind up here still if IDynamicObj doesn't provide a rule (such as for list sigs).
        // If our IDynamicObject implementation is complete, we can then remove these Call methods.
        [SpecialName]
        public object Call(CodeContext context, params object[] args\u00F8) {
            OldInstance inst = new OldInstance(this);
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
        public object Call(CodeContext context, [ParamDictionary] IAttributesCollection dict\u00F8, params object[] args\u00F8) {
            OldInstance inst = new OldInstance(this);
            object meth;
            if (PythonOps.TryGetBoundAttr(inst, Symbols.Init, out meth)) {
                PythonCalls.CallWithKeywordArgs(meth, args\u00F8, dict\u00F8);
            } else if (dict\u00F8.Count > 0 || args\u00F8.Length > 0) {
                MakeCallError();
            }
            return inst;
        }
        #endregion // calls

        
        internal PythonType TypeObject {
            get {
                if (_type == null) {
                    _type = OldInstanceTypeBuilder.Build(this);
                }
                return _type;
            }
        }

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetBoundMember(context, name, null, out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetBoundMember(context, name, null, out value);
        }

        public bool TryGetBoundMember(CodeContext context, SymbolId name, object instance, out object value) {
            if (name == Symbols.Bases) { value = PythonTuple.Make(_bases); return true; }
            if (name == Symbols.Name) { value = __name__; return true; }
            if (name == Symbols.Dict) {
                //!!! user code can modify __del__ property of __dict__ behind our back
                HasDelAttr = HasSetAttr = true;  // pessimisticlly assume the user is setting __setattr__ in the dict
                value = __dict__; return true;
            }

            if (TryLookupSlot(name, out value)) {
                value = GetOldStyleDescriptor(context, value, instance, this);
                return true;
            }
            return false;
        }

        private List<OldClass> ValidateBases(object value) {
            PythonTuple t = value as PythonTuple;
            if (t == null) throw PythonOps.TypeError("__bases__ must be a tuple object");

            List<OldClass> res = new List<OldClass>(t.Count);
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

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            if (name == Symbols.Bases) {
                _bases = ValidateBases(value);
                return;
            } else if (name == Symbols.Name) {
                string n = value as string;
                if (n == null) throw PythonOps.TypeError("TypeError: __name__ must be a string object");
                __name__ = n;
                return;
            } else if (name == Symbols.Dict) {
                IAttributesCollection d = value as IAttributesCollection;
                if (d == null) throw PythonOps.TypeError("__dict__ must be set to dictionary");
                __dict__ = d;
                return;
            }

            __dict__[name] = value;

            if (name == Symbols.Unassign) {
                HasFinalizer = true;
            } else if (name == Symbols.SetAttr) {
                HasSetAttr = true;
            } else if (name == Symbols.DelAttr) {
                HasDelAttr = true;
            }
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            if (!__dict__.Remove(name)) {
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

        #endregion

        internal static void RecurseAttrHierarchy(OldClass oc, IDictionary<object, object> attrs) {
            foreach (object key in oc.__dict__.Keys) {
                if (!attrs.ContainsKey(key)) {
                    attrs.Add(key, key);
                }
            }

            //  recursively get attrs in parent hierarchy
            if (oc._bases.Count != 0) {
                foreach (OldClass parent in oc._bases) {
                    RecurseAttrHierarchy(parent, attrs);
                }
            }
        }

        #region ICustomMembers Members

        public IList<object> GetMemberNames(CodeContext context) {
            SymbolDictionary attrs = new SymbolDictionary(__dict__);
            RecurseAttrHierarchy(this, attrs);
            return List.Make(attrs);
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            return (IDictionary<object, object>)__dict__;
        }

        #endregion

        public bool IsSubclassOf(object other) {
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

        string ICodeFormattable.ToCodeString(CodeContext context) {
            return string.Format("<class {0} at {1}>", FullName, PythonOps.HexId(this));
        }

        #endregion

        #region IDynamicObject Members

        LanguageContext IDynamicObject.LanguageContext {
            get { return DefaultContext.Default.LanguageContext; }
        }

        public StandardRule<T> GetRule<T>(DynamicAction action, CodeContext context, object[] args) {
            switch (action.Kind) {
                case DynamicActionKind.GetMember:
                    return MakeGetMemberRule<T>((GetMemberAction)action, context, args);
                case DynamicActionKind.SetMember:
                    return MakeSetMemberRule<T>((SetMemberAction)action, context, args);
                case DynamicActionKind.DeleteMember:
                    return MakeDelMemberRule<T>((DeleteMemberAction)action, context, args);
                case DynamicActionKind.CreateInstance:
                case DynamicActionKind.Call:
                    return MakeCallRule<T>((CallAction)action, context, args);
                case DynamicActionKind.DoOperation:
                    return MakeDoOperationRule<T>((DoOperationAction)action, context, args);
                default: return null;
            }
        }

        private StandardRule<T> MakeDoOperationRule<T>(DoOperationAction doOperationAction, CodeContext context, object[] args) {
            switch (doOperationAction.Operation) {
                case Operators.IsCallable:
                    return PythonBinderHelper.MakeIsCallableRule<T>(context, this, true);
            }
            return null;
        }

        private static StandardRule<T> MakeCallRule<T>(CallAction action, CodeContext context, object[] args) {
            // This rule only handles simple signatures. Fallback on MethodBinder to handle complex signatures 
            // such as keyword args.
            if (!action.Signature.IsSimple) return null; 

            StandardRule<T> rule = new StandardRule<T>();

            Expression[] exprArgs = new Expression[args.Length - 1];
            for (int i = 0; i < args.Length - 1; i++) {
                exprArgs[i] = rule.Parameters[i + 1];
            }

            // TODO: If we know __init__ wasn't present we could construct the OldInstance directly.
            Variable tmp = rule.GetTemporary(typeof(object), "init");
            Variable instTmp = rule.GetTemporary(typeof(object), "inst");
            rule.Test = rule.MakeTypeTest(typeof(OldClass), 0);
            rule.Target =
                rule.MakeReturn(context.LanguageContext.Binder,
                    Ast.Comma(
                        Ast.Assign(
                            instTmp,
                            Ast.New(
                                typeof(OldInstance).GetConstructor(new Type[] { typeof(OldClass) }),
                                Ast.ConvertHelper(rule.Parameters[0], typeof(OldClass))
                            )
                        ),
                        Ast.Condition(
                            Ast.Call(
                                Ast.ConvertHelper(rule.Parameters[0], typeof(OldClass)),
                                typeof(OldClass).GetMethod("TryLookupInit"),
                                Ast.Read(instTmp),
                                Ast.Read(tmp)
                            ),
                            Ast.Action.Call(
                                action,
                                typeof(object),
                                ArrayUtils.Insert((Expression)Ast.Read(tmp), ArrayUtils.RemoveFirst(rule.Parameters))
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
                        Ast.Read(instTmp)
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

        private static StandardRule<T> MakeSetMemberRule<T>(SetMemberAction action, CodeContext context, object[] args) {
            StandardRule<T> rule = new StandardRule<T>();
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
            IAttributesCollection d = value as IAttributesCollection;
            if (d == null) throw PythonOps.TypeError("__dict__ must be set to dictionary");
            __dict__ = d;
        }

        public void SetNameHelper(SymbolId name, object value) {
            __dict__[name] = value;

            if (name == Symbols.Unassign) {
                HasFinalizer = true;
            } else if (name == Symbols.SetAttr) {
                HasSetAttr = true;
            } else if (name == Symbols.DelAttr) {
                HasDelAttr = true;
            }
        }

        private static StandardRule<T> MakeDelMemberRule<T>(DeleteMemberAction action, CodeContext context, object[] args) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(OldClass));
            rule.Target = rule.MakeReturn(context.LanguageContext.Binder,
                Ast.Call(
                    Ast.ConvertHelper(rule.Parameters[0], typeof(OldClass)),
                    typeof(OldClass).GetMethod("DeleteCustomMember"),
                    Ast.CodeContext(),
                    Ast.Constant(action.Name)
                )
            );
            return rule;
        }

        private static StandardRule<T> MakeGetMemberRule<T>(GetMemberAction action, CodeContext context, object[] args) {
            StandardRule<T> rule = new StandardRule<T>();

            rule.MakeTest(typeof(OldClass));
            Expression target;

            if (action.Name == Symbols.Dict) {
                target = Ast.Comma(
                    Ast.Call(
                        Ast.Convert(rule.Parameters[0], typeof(OldClass)),
                        typeof(OldClass).GetMethod("DictionaryIsPublic")
                    ),
                    Ast.ReadField(
                        Ast.Convert(rule.Parameters[0], typeof(OldClass)),
                        typeof(OldClass).GetField("__dict__")
                    )
                );
            } else if (action.Name == Symbols.Bases) {
                target = Ast.Call(
                    typeof(PythonTuple).GetMethod("Make"),
                    Ast.ConvertHelper(
                        Ast.ReadProperty(
                            Ast.Convert(rule.Parameters[0], typeof(OldClass)),
                            typeof(OldClass).GetProperty("BaseClasses")
                        ),
                        typeof(object)
                    )
                );
            } else if (action.Name == Symbols.Name) {
                target = Ast.ReadProperty(
                    Ast.Convert(rule.Parameters[0], typeof(OldClass)),
                    typeof(OldClass).GetProperty("Name")
                );
            } else {
                if (action.IsNoThrow) {
                    Variable tmp = rule.GetTemporary(typeof(object), "lookupVal");
                    target =
                        Ast.Condition(
                            Ast.Call(
                                Ast.Convert(rule.Parameters[0], typeof(OldClass)),
                                typeof(OldClass).GetMethod("TryLookupValue"),
                                Ast.CodeContext(),
                                Ast.Constant(action.Name),
                                Ast.Read(tmp)
                            ),
                            Ast.Read(tmp),
                            Ast.Convert(
                                Ast.ReadField(null, typeof(OperationFailed).GetField("Value")),
                                typeof(object)
                            )
                        );
                } else {
                    target = Ast.Call(
                        Ast.Convert(rule.Parameters[0], typeof(OldClass)),
                        typeof(OldClass).GetMethod("LookupValue"),
                        Ast.CodeContext(),
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
    }
}
