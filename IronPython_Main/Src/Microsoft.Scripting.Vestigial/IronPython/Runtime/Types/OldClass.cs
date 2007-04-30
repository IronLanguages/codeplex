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
using System.Runtime.Serialization;
using Microsoft.Scripting;

using System.Threading;
using System.Diagnostics;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Math;
using System.ComponentModel;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Types {
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
        ICallableWithCodeContext,
        IConstructorWithCodeContext,
        IFancyCallable,
#if !SILVERLIGHT // ICustomTypeDescriptor
        ICustomTypeDescriptor,
#endif
        ICodeFormattable,
        ICustomMembers, 
        IActionable {

        [NonSerialized]
        private List<OldClass> _bases;
        private DynamicType _type = null;

        public IAttributesCollection __dict__;
        private int attrs;  // actually OldClassAttributes - losing type safety for thread safety
        internal object __name__;

        private static int _namesVersion;
        private int _optimizedInstanceNamesVersion;
        private SymbolId[] _optimizedInstanceNames;

        public OldClass(string name, Tuple bases, IAttributesCollection dict) : this(name, bases, dict, "") {
        }

        public OldClass(string name, Tuple bases, IAttributesCollection dict, string instanceNames) {
            _bases = ValidateBases(bases);
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
            if (info == null) throw new ArgumentNullException("info");

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
                _optimizedInstanceNames = new SymbolId[0];
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
            DynamicTypeSlot dts = self as DynamicTypeSlot;
            object callable;
            if (dts != null && dts.TryGetValue(context, instance, TypeObject, out callable)) {
                return callable;
            }

            return Ops.GetUserDescriptor(self, instance, type);
        }

        public bool HasFinalizer {
            get {
                return (attrs & (int)OldClassAttributes.HasFinalizer) != 0;
            }
            internal set {
                int oldAttrs, newAttrs;
                do {
                    oldAttrs = attrs;
                    newAttrs = value ? oldAttrs | ((int)OldClassAttributes.HasFinalizer) : oldAttrs & ((int)~OldClassAttributes.HasFinalizer);
                } while (Interlocked.CompareExchange(ref attrs, newAttrs, oldAttrs) != oldAttrs);
            }
        }

        internal bool HasSetAttr {
            get {
                return (attrs & (int)OldClassAttributes.HasSetAttr) != 0;
            }
            set {
                int oldAttrs, newAttrs;
                do {
                    oldAttrs = attrs;
                    newAttrs = value ? oldAttrs | ((int)OldClassAttributes.HasSetAttr) : oldAttrs & ((int)~OldClassAttributes.HasSetAttr);
                } while (Interlocked.CompareExchange(ref attrs, newAttrs, oldAttrs) != oldAttrs);
            }
        }

        internal bool HasDelAttr {
            get {
                return (attrs & (int)OldClassAttributes.HasDelAttr) != 0;
            }
            set {
                int oldAttrs, newAttrs;
                do {
                    oldAttrs = attrs;
                    newAttrs = value ? oldAttrs | ((int)OldClassAttributes.HasDelAttr) : oldAttrs & ((int)~OldClassAttributes.HasDelAttr);
                } while (Interlocked.CompareExchange(ref attrs, newAttrs, oldAttrs) != oldAttrs);
            }
        }
        public override string ToString() {
            return FullName;
        }

        #region ICallableWithCodeContext Members
        [OperatorMethod]
        public object Call(CodeContext context, params object[] args) {
            OldInstance inst = new OldInstance(this);
            object value;
            // lookup the slot directly - we don't go through __getattr__
            // until after the instance is created.
            if (TryLookupSlot(Symbols.Init, out value)) {
                Ops.CallWithContext(context, GetOldStyleDescriptor(context, value, inst, this), args);
            } else if (args.Length > 0) {
                throw Ops.TypeError("this constructor takes no arguments");
            }
            return inst;
        }

        #endregion

        #region IFancyCallable Members

        public object Call(CodeContext context, object[] args, string[] names) {
            OldInstance inst = new OldInstance(this);
            object meth;
            if (Ops.TryGetBoundAttr(inst, Symbols.Init, out meth)) {
                Ops.CallWithKeywordArgs(context, meth, args, names);
            } else {
                Debug.Assert(names.Length != 0);
                throw Ops.TypeError("this constructor takes no arguments");
            }
            return inst;
        }

        #endregion

        internal DynamicType TypeObject {
            get {
                if (_type == null) {
                    _type = OldInstanceTypeBuilder.Build(this);
                }
                return _type;
            }
        }

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            if (name == Symbols.Bases) { value = Tuple.Make(_bases); return true; }
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

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetCustomMember(context, name, out value);
        }

        private List<OldClass> ValidateBases(object value) {
            Tuple t = value as Tuple;
            if (t == null) throw Ops.TypeError("__bases__ must be a tuple object");

            List<OldClass> res = new List<OldClass>(t.Count);
            foreach (object o in t) {
                OldClass oc = o as OldClass;
                if (oc == null) throw Ops.TypeError("__bases__ items must be classes (got {0})", Ops.GetDynamicType(o).Name);

                if (oc.IsSubclassOf(this)) {
                    throw Ops.TypeError("a __bases__ item causes an inheritance cycle");
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
                if (n == null) throw Ops.TypeError("TypeError: __name__ must be a string object");
                __name__ = n;
                return;
            } else if (name == Symbols.Dict) {
                IAttributesCollection d = value as IAttributesCollection;
                if (d == null) throw Ops.TypeError("__dict__ must be set to dictionary");
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
                throw Ops.AttributeError("{0} is not a valid attribute", SymbolTable.IdToString(name));
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

        public IList<object> GetCustomMemberNames(CodeContext context) {
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
            return string.Format("<class {0} at {1}>", FullName, Ops.HexId(this));
        }

        #endregion

        #region IConstructorWithCodeContext Members

        object IConstructorWithCodeContext.Construct(CodeContext context, params object[] args) {
            return Call(context, args);
        }

        #endregion

        #region IActionable Members

        public StandardRule<T> GetRule<T>(Action action, CodeContext context, object[] args) {
            if (action != CallAction.Simple) return null;

            StandardRule<T> rule = new StandardRule<T>();

            Expression[] exprArgs = new Expression[args.Length - 1];
            for (int i = 0; i < args.Length - 1; i++) {
                exprArgs[i] = rule.GetParameterExpression(i + 1);
            }

            // TODO: If we know __init__ wasn't present we could construct the OldInstance directly.
            rule.SetTest(rule.MakeTypeTest(TypeCache.OldClass, 0));
            rule.SetTarget(rule.MakeReturn(context.LanguageContext.Binder,
                MethodCallExpression.Call(
                    StaticUnaryExpression.Convert(
                        rule.GetParameterExpression(0),
                        typeof(OldClass)),
                    typeof(OldClass).GetMethod("Call", new Type[] { typeof(CodeContext), typeof(object[]) }),
                    new CodeContextExpression(),
                    NewArrayExpression.NewArrayInit(typeof(object[]), exprArgs))));

            return rule;
        }

        #endregion
    }

    /// <summary>
    /// Custom dictionary use for old class instances so commonly used
    /// items can be accessed quickly w/o requiring dictionary access.
    /// 
    /// Keys are only added to the dictionary, once added they are never
    /// removed.
    /// 
    /// TODO Merge this with TupleDictionary
    /// </summary>
    [PythonType(typeof(PythonDictionary))]
    public class CustomOldClassDictionary : CustomSymbolDictionary {
        private int _keyVersion;
        private SymbolId[] _extraKeys;
        private object[] _values;

        public CustomOldClassDictionary(SymbolId[] extraKeys, int keyVersion) {
            _extraKeys = extraKeys;
            _keyVersion = keyVersion;
            _values = new object[extraKeys.Length];
            for (int i = 0; i < _values.Length; i++) {
                _values[i] = Uninitialized.Instance;
            }
        }

        public int KeyVersion {
            get {
                return _keyVersion;
            }
        }

        public override SymbolId[] GetExtraKeys() {
            return _extraKeys;
        }

        public int FindKey(SymbolId key) {
            for (int i = 0; i < _extraKeys.Length; i++) {
                if (_extraKeys[i] == key) {
                    return i;
                }
            }
            return -1;
        }

        public object GetExtraValue(int index) {
            return _values[index];
        }

        public object GetValueHelper(int index, object oldInstance) {
            object ret = _values[index];
            if (ret != Uninitialized.Instance) return ret;
            //TODO this should go to a faster path since we know it's not in the dict
            return ((OldInstance)oldInstance).GetBoundMember(null, _extraKeys[index]);
        }

        public void SetExtraValue(int index, object value) {
            _values[index] = value;
        }

        protected override bool TrySetExtraValue(SymbolId keyId, object value) {
            int key = keyId.Id;
            for (int i = 0; i < _extraKeys.Length; i++) {
                // see if we already have a key (once keys are assigned
                // they never change) that matches this ID.
                if (_extraKeys[i].Id == key) {
                    _values[i] = value;
                    return true;
                }
            }
            return false;
        }

        protected override bool TryGetExtraValue(SymbolId keyId, out object value) {
            int key = keyId.Id;
            for (int i = 0; i < _extraKeys.Length; i++) {
                if (_extraKeys[i].Id == key) {
                    value = _values[i];
                    return true;
                }
            }
            value = null;
            return false;
        }

        [PythonClassMethod("fromkeys")]
        public static object FromKeys(CodeContext context, DynamicType cls, object seq) {
            return PythonDictionary.FromKeys(context, cls, seq, null);
        }

        [PythonClassMethod("fromkeys")]
        public static object FromKeys(CodeContext context, DynamicType cls, object seq, object value) {
            return PythonDictionary.FromKeys(context, cls, seq, value);
        }

        public override string ToString() {
            return DictionaryOps.ToString(this);
        }
    }

    [PythonType("instance")]
    [Serializable]
    public sealed partial class OldInstance :
        ICodeFormattable,
        IValueEquality,
#if !SILVERLIGHT // ICustomTypeDescriptor
        ICustomTypeDescriptor,
#endif
        ISerializable,
        IWeakReferenceable,
        ICustomMembers,
        IActionable
    {

        private IAttributesCollection __dict__;
        internal OldClass __class__;
        private WeakRefTracker weakRef;       // initialized if user defines finalizer on class or instance

        private IAttributesCollection MakeDictionary(OldClass oldClass) {
            //if (oldClass.OptimizedInstanceNames.Length == 0) {
            //    return new CustomOldClassDictionar();
            //}
            return new CustomOldClassDictionary(oldClass.OptimizedInstanceNames, oldClass.OptimizedInstanceNamesVersion);
        }


        public OldInstance(OldClass _class) {
            __class__ = _class;
            __dict__ = MakeDictionary(_class);
            if (__class__.HasFinalizer) {
                // class defines finalizer, we get it automatically.
                AddFinalizer();
            }
        }

#if !SILVERLIGHT // SerializationInfo
        private OldInstance(SerializationInfo info, StreamingContext context) {
            __class__ = (OldClass)info.GetValue("__class__", typeof(OldClass));
            __dict__ = MakeDictionary(__class__);

            List<object> keys = (List<object>)info.GetValue("keys", typeof(List<object>));
            List<object> values = (List<object>)info.GetValue("values", typeof(List<object>));
            for (int i = 0; i < keys.Count; i++) {
                __dict__.AddObjectKey(keys[i], values[i]);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context")]
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) throw new ArgumentNullException("info");

            info.AddValue("__class__", __class__);
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

        /// <summary>
        /// Returns the dictionary used to store state for this object
        /// </summary>
        public IAttributesCollection Dictionary {
            get { return __dict__; }
        }

        public CustomOldClassDictionary GetOptimizedDictionary(int keyVersion) {
            CustomOldClassDictionary dict = __dict__ as CustomOldClassDictionary;
            if (dict == null || __class__.HasSetAttr || dict.KeyVersion != keyVersion) {
                return null;
            }
            return dict;
        }


        StandardRule<T> IActionable.GetRule<T>(Action action, CodeContext context, object[] args)  {
            switch (action.Kind) {
                case ActionKind.GetMember:
                case ActionKind.SetMember:
                    return MakeMemberRule<T>((MemberAction)action, context, args);
                default:
                    return null;
            }
        }

        private StandardRule<T> MakeMemberRule<T>(MemberAction action, CodeContext context, object[] args) {
            CustomOldClassDictionary dict = this.Dictionary as CustomOldClassDictionary;
            if (dict == null || __class__.HasSetAttr) {
                return MakeDynamicOldInstanceRule<T>(action, context);
            }

            int key = dict.FindKey(action.Name);
            if (key == -1) {
                return MakeDynamicOldInstanceRule<T>(action, context);
            }

            StandardRule<T> rule = new StandardRule<T>();

            VariableReference tmp = rule.GetTemporary(typeof(CustomOldClassDictionary), "dict");
            Expression tryGetValue = MethodCallExpression.Call(
                StaticUnaryExpression.Convert(rule.GetParameterExpression(0), typeof(OldInstance)),
                typeof(OldInstance).GetMethod("GetOptimizedDictionary"),
                ConstantExpression.Constant(dict.KeyVersion));
            tryGetValue = BoundAssignment.Assign(tmp, tryGetValue);

            Expression test = BinaryExpression.AndAlso(
                BinaryExpression.NotEqual(
                    rule.GetParameterExpression(0),
                    ConstantExpression.Constant(null)),
                BinaryExpression.Equal(
                    MethodCallExpression.Call(
                        rule.GetParameterExpression(0), typeof(object).GetMethod("GetType")),
                        ConstantExpression.Constant(typeof(OldInstance))
                ));
            test = BinaryExpression.AndAlso(test,
                BinaryExpression.NotEqual(
                    tryGetValue, ConstantExpression.Constant(null)));

            rule.SetTest(test);
            Expression target;

            switch (action.Kind) {
                case ActionKind.GetMember:
                    target = MethodCallExpression.Call(
                                BoundExpression.Defined(tmp),
                                typeof(CustomOldClassDictionary).GetMethod("GetValueHelper"),
                                ConstantExpression.Constant(key),
                                rule.GetParameterExpression(0));
                    break;
                case ActionKind.SetMember:
                    target = MethodCallExpression.Call(
                                BoundExpression.Defined(tmp),
                                typeof(CustomOldClassDictionary).GetMethod("SetExtraValue"),
                                ConstantExpression.Constant(key),
                                rule.GetParameterExpression(1));
                    break;
                default:
                    throw new InvalidOperationException();
            }

            rule.SetTarget(rule.MakeReturn(context.LanguageContext.Binder, target));
            return rule;
        }

        private StandardRule<T> MakeDynamicOldInstanceRule<T>(MemberAction action, CodeContext context) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(new DynamicType[] { Ops.GetDynamicTypeFromType(typeof(OldInstance)) });
            Expression instance = StaticUnaryExpression.Convert(
                    rule.GetParameterExpression(0), typeof(OldInstance));

            Expression target;
             switch (action.Kind) {
                case ActionKind.GetMember:
                    target = MethodCallExpression.Call(instance,
                        typeof(OldInstance).GetMethod("GetBoundMember"),
                            new CodeContextExpression(),
                            ConstantExpression.Constant(action.Name));
                    break;
                case ActionKind.SetMember:
                    target = MethodCallExpression.Call(instance,
                        typeof(OldInstance).GetMethod("SetCustomMember"),
                            new CodeContextExpression(),
                            ConstantExpression.Constant(action.Name),
                            rule.GetParameterExpression(1));
                    break;
                default:
                    throw new InvalidOperationException();
            }

            rule.SetTarget(rule.MakeReturn(context.LanguageContext.Binder, target));
            return rule;
        }

        #region Object overrides

        public override string ToString() {
            object ret;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.String, out ret)) {
                ret = PythonCalls.Call(ret);
                string strRet;
                if (Converter.TryConvertToString(ret, out strRet) && strRet != null) {
                    return strRet;
                }
                throw Ops.TypeError("__str__ returned non-string type ({0})", Ops.GetDynamicType(ret).Name);
            }

            return ToCodeString(DefaultContext.Default);
        }

        #endregion

        #region ICodeFormattable Members

        [OperatorMethod, PythonName("__repr__")]
        public string ToCodeString(CodeContext context) {
            object ret;

            if (TryGetBoundCustomMember(context, Symbols.Repr, out ret)) {
                ret = Ops.CallWithContext(context, ret);
                string strRet;
                if (Converter.TryConvertToString(ret, out strRet) && strRet != null) {
                    return strRet;
                }
                throw Ops.TypeError("__repr__ returned non-string type ({0})", Ops.GetDynamicType(ret).Name);
            }

            return string.Format("<{0} instance at {1}>", __class__.FullName, Ops.HexId(this));
        }

        #endregion

        [OperatorMethod, PythonName("__divmod__")]
        public object DivMod(CodeContext context, object divmod) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.DivMod, out value)) {
                return PythonCalls.Call(value, divmod);
            }


            return Ops.NotImplemented;
        }
        [OperatorMethod, PythonName("__rdivmod__")]
        public object ReverseDivMod(CodeContext context, object divmod) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ReverseDivMod, out value)) {
                return PythonCalls.Call(value, divmod);
            }

            return Ops.NotImplemented;
        }


        [OperatorMethod, PythonName("__coerce__")]
        public object Coerce(CodeContext context, object other) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Coerce, out value)) {
                return PythonCalls.Call(value, other);
            }

            return Ops.NotImplemented;
        }

        [OperatorMethod, PythonName("__len__")]
        public object GetLength(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Length, out value)) {
                return Ops.CallWithContext(context, value);
            }

            throw Ops.AttributeErrorForMissingAttribute(__class__.Name, Symbols.Length);
        }

        [OperatorMethod, PythonName("__pos__")]
        public object Positive(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Positive, out value)) {
                return Ops.CallWithContext(context, value);
            }

            throw Ops.AttributeErrorForMissingAttribute(__class__.Name, Symbols.Positive);
        }

        [OperatorMethod, PythonName("__getitem__")]
        public object GetItem(CodeContext context, object item) {
            Slice slice = item as Slice;
            if (slice != null && slice.Step == null) {
                object getSlice;
                if (Ops.TryGetBoundAttr(context, this, Symbols.GetSlice, out getSlice)) {
                    int start, stop;
                    slice.DeprecatedFixed(this, out start, out stop);
                    return Ops.CallWithContext(context, getSlice, start, stop);
                }
            }

            return Ops.InvokeWithContext(context, this, Symbols.GetItem, item);
        }

        [OperatorMethod, PythonName("__setitem__")]
        public void SetItem(CodeContext context, object item, object value) {
            Slice slice = item as Slice;
            if (slice != null && slice.Step == null) {
                object setSlice;
                if (Ops.TryGetBoundAttr(context, this, Symbols.SetSlice, out setSlice)) {
                    int start, stop;
                    slice.DeprecatedFixed(this, out start, out stop);
                    Ops.CallWithContext(context, setSlice, start, stop, value);
                    return;
                }
            }

            Ops.InvokeWithContext(context, this, Symbols.SetItem, item, value);
        }

        [OperatorMethod, PythonName("__delitem__")]
        public object DeleteItem(CodeContext context, object item) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.DelItem, out value)) {
                return PythonCalls.Call(value, item);
            }

            throw Ops.AttributeErrorForMissingAttribute(__class__.Name, Symbols.DelItem);
        }

        [OperatorMethod, PythonName("__neg__")]
        public object Negate(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.OperatorNegate, out value)) {
                return Ops.CallWithContext(context, value);
            }

            throw Ops.AttributeErrorForMissingAttribute(__class__.Name, Symbols.OperatorNegate);
        }


        [OperatorMethod, PythonName("__abs__")]
        public object Absolute(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.AbsoluteValue, out value)) {
                return Ops.CallWithContext(context, value);
            }

            throw Ops.AttributeErrorForMissingAttribute(__class__.Name, Symbols.AbsoluteValue);
        }

        [OperatorMethod, PythonName("__invert__")]
        public object Invert(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.OperatorOnesComplement, out value)) {
                return Ops.CallWithContext(context, value);
            }

            throw Ops.AttributeErrorForMissingAttribute(__class__.Name, Symbols.OperatorOnesComplement);
        }

        [OperatorMethod, PythonName("__contains__")]
        public object Contains(CodeContext context, object index) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Contains, out value)) {
                return PythonCalls.Call(value, index);
            }

            IEnumerator ie = Ops.GetEnumerator(this);
            while (ie.MoveNext()) {
                if (Ops.EqualRetBool(ie.Current, index)) return Ops.True;
            }

            return Ops.False;
        }

        [OperatorMethod, PythonName("__pow__")]
        public object Power(CodeContext context, object exp, object mod) {
            object value;
            if (TryGetBoundCustomMember(context, Symbols.OperatorPower, out value)) {
                return PythonCalls.Call(value, exp, mod);
            }

            return Ops.NotImplemented;
        }

        [OperatorMethod]
        public object Call(CodeContext context) {
            return Call(context, Ops.EmptyObjectArray);
        }

        [OperatorMethod]
        public object Call(CodeContext context, object args) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                KwCallInfo kwInfo;

                if (args is object[])
                    return Ops.CallWithContext(context, value, (object[])args);
                else if ((kwInfo = args as KwCallInfo) != null)
                    return Ops.CallWithKeywordArgs(context, value, kwInfo.Arguments, kwInfo.Names);

                return Ops.CallWithContext(context, value, args);
            }

            throw Ops.AttributeError("{0} instance has no __call__ method", __class__.Name);
        }

        [OperatorMethod]
        public object Call(CodeContext context, params object[] args) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                return Ops.CallWithContext(context, value, args);
            }

            throw Ops.AttributeError("{0} instance has no __call__ method", __class__.Name);
        }

        [OperatorMethod]
        public object Call(CodeContext context, [ParamDictionary]PythonDictionary dict, params object[] args) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                return Ops.CallWithArgsTupleAndKeywordDictAndContext(context, value, args, new string[0], null, dict);
            }

            throw Ops.AttributeError("{0} instance has no __call__ method", __class__.Name);
        }

        [OperatorMethod, PythonName("__nonzero__")]
        public object IsNonZero(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.NonZero, out value)) {
                return Ops.CallWithContext(context, value);
            }

            if (TryGetBoundCustomMember(context, Symbols.Length, out value)) {
                value = Ops.CallWithContext(context, value);
                // Convert resulting object to the desired type
                if (value is Int32 || value is BigInteger) {
                    return Ops.Bool2Object(Converter.ConvertToBoolean(value));
                }
                throw Ops.TypeError("an integer is required, got {0}", Ops.GetDynamicType(value).Name);
            }

            return Ops.True;
        }

        [OperatorMethod, PythonName("__hex__")]
        public object ConvertToHex(CodeContext context) {
            object value;
            if (TryGetBoundCustomMember(context, Symbols.ConvertToHex, out value)) {
                return Ops.CallWithContext(context, value);
            }

            throw Ops.AttributeErrorForMissingAttribute(__class__.Name, Symbols.ConvertToHex);
        }

        [OperatorMethod, PythonName("__oct__")]
        public object ConvertToOctal(CodeContext context) {
            object value;
            if (TryGetBoundCustomMember(context, Symbols.ConvertToOctal, out value)) {
                return Ops.CallWithContext(context, value);
            }

            throw Ops.AttributeErrorForMissingAttribute(__class__.Name, Symbols.ConvertToOctal);
        }

        [OperatorMethod, PythonName("__int__")]
        public object ConvertToInt(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ConvertToInt, out value)) {
                return Ops.CallWithContext(context, value);
            }

            return Ops.NotImplemented;
        }

        [OperatorMethod, PythonName("__long__")]
        public object ConvertToLong(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ConvertToLong, out value)) {
                return Ops.CallWithContext(context, value);
            }

            return Ops.NotImplemented;
        }

        [OperatorMethod, PythonName("__float__")]
        public object ConvertToFloat(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ConvertToFloat, out value)) {
                return Ops.CallWithContext(context, value);
            }

            return Ops.NotImplemented;
        }

        [OperatorMethod, PythonName("__complex__")]
        public object ConvertToComplex(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ConvertToComplex, out value)) {
                return Ops.CallWithContext(context, value);
            }

            return Ops.NotImplemented;
        }

        public object GetBoundMember(CodeContext context, SymbolId name) {
            object ret;
            if (TryGetBoundCustomMember(context, name, out ret)) {
                return ret;
            }
            throw Ops.AttributeError("'{0}' object has no attribute '{1}'",
                Ops.GetDynamicType(this).Name, SymbolTable.IdToString(name));
        }

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetBoundCustomMember(context, name, out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            int nameId = name.Id;
            if (nameId == Symbols.Dict.Id) {
                //!!! user code can modify __del__ property of __dict__ behind our back
                value = __dict__;
                return true;
            } else if (nameId == Symbols.Class.Id) {
                value = __class__;
                return true;
            }

            if (TryRawGetAttr(context, name, out value)) return true;

            if (nameId != Symbols.GetBoundAttr.Id) {
                object getattr;
                if (TryRawGetAttr(context, Symbols.GetBoundAttr, out getattr)) {
                    try {
                        value = PythonCalls.Call(getattr, SymbolTable.IdToString(name));
                        return true;
                    } catch (MissingMemberException) {
                        // __getattr__ raised AttributeError, return false.
                    }
                }
            }

            return false;
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            object setFunc;
            int nameId = name.Id;
            if (nameId == Symbols.Class.Id) {
                SetClass(value);
            } else if (nameId == Symbols.Dict.Id) {
                SetDict(value);
            } else if (__class__.HasSetAttr && __class__.TryLookupSlot(Symbols.SetAttr, out setFunc)) {
                PythonCalls.Call(__class__.GetOldStyleDescriptor(context, setFunc, this, __class__), name.ToString(), value);
            } else if (nameId == Symbols.Unassign.Id) {
                SetFinalizer(name, value);
            } else {
                __dict__[name] = value;
            }
        }

        private void SetFinalizer(SymbolId name, object value) {
            if (!HasFinalizer()) {
                // user is defining __del__ late bound for the 1st time
                AddFinalizer();
            }

            __dict__[name] = value;
        }

        private void SetDict(object value) {
            IAttributesCollection dict = value as IAttributesCollection;
            if (dict == null) {
                throw Ops.TypeError("__dict__ must be set to a dictionary");
            }
            if (HasFinalizer() && !__class__.HasFinalizer) {
                if (!dict.ContainsKey(Symbols.Unassign)) {
                    ClearFinalizer();
                }
            } else if (dict.ContainsKey(Symbols.Unassign)) {
                AddFinalizer();
            }


            __dict__ = dict;
        }

        private void SetClass(object value) {
            OldClass oc = value as OldClass;
            if (oc == null) {
                throw Ops.TypeError("__class__ must be set to class");
            }
            __class__ = oc;
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            if (name == Symbols.Class) throw Ops.TypeError("__class__ must be set to class");
            if (name == Symbols.Dict) throw Ops.TypeError("__dict__ must be set to a dictionary");

            object delFunc;
            if (__class__.HasDelAttr && __class__.TryLookupSlot(Symbols.DelAttr, out delFunc)) {
                PythonCalls.Call(__class__.GetOldStyleDescriptor(context, delFunc, this, __class__), name.ToString());
                return true;
            }


            if (name == Symbols.Unassign) {
                // removing finalizer
                if (HasFinalizer() && !__class__.HasFinalizer) {
                    ClearFinalizer();
                }
            }

            if (!__dict__.Remove(name)) {
                throw Ops.AttributeError("{0} is not a valid attribute", SymbolTable.IdToString(name));
            }
            return true;
        }

        #endregion

        #region ICustomAttributes Members

        public IList<object> GetCustomMemberNames(CodeContext context) {
            SymbolDictionary attrs = new SymbolDictionary(__dict__);
            OldClass.RecurseAttrHierarchy(this.__class__, attrs);
            return List.Make(attrs);
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            return (IDictionary<object, object>)__dict__;
        }

        #endregion

        [OperatorMethod, PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public object CompareTo(CodeContext context, object other) {
            OldInstance oiOther = other as OldInstance;
            // CPython raises this if called directly, but not via cmp(os,ns) which still calls the user __cmp__
            //if(!(oiOther is OldInstance)) 
            //    throw Ops.TypeError("instance.cmp(x,y) -> y must be an instance, got {0}", Ops.StringRepr(Ops.GetDynamicType(other)));

            object res = InternalCompare(Symbols.Cmp, other);
            if (res != Ops.NotImplemented) return res;
            if (oiOther != null) {
                res = oiOther.InternalCompare(Symbols.Cmp, this);
                if (res != Ops.NotImplemented) return ((int)res) * -1;
            }

            return Ops.NotImplemented;
        }

        private object CompareForwardReverse(object other, SymbolId forward, SymbolId reverse) {
            object res = InternalCompare(forward, other);
            if (res != Ops.NotImplemented) return res;

            OldInstance oi = other as OldInstance;
            if (oi != null) {
                // comparison operators are reflexive
                return oi.InternalCompare(reverse, this);
            }

            return Ops.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator >([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorGreaterThan, Symbols.OperatorLessThan);
        }

        [return: MaybeNotImplemented]
        public static object operator <([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorLessThan, Symbols.OperatorGreaterThan);
        }

        [return: MaybeNotImplemented]
        public static object operator >=([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorGreaterThanOrEqual, Symbols.OperatorLessThanOrEqual);
        }

        [return: MaybeNotImplemented]
        public static object operator <=([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorLessThanOrEqual, Symbols.OperatorGreaterThanOrEqual);
        }

        private object InternalCompare(SymbolId cmp, object other) {
            object meth;
            if (TryGetBoundCustomMember(DefaultContext.Default, cmp, out meth)) 
                return Ops.CallWithContext(DefaultContext.Default, meth, other);
            return Ops.NotImplemented;
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

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            return weakRef;
        }

        bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            weakRef = value;
            return true;
        }

        void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
            ((IWeakReferenceable)this).SetWeakRef(value);
        }

        #endregion

        #region Rich Equality
        // Specific rich equality support for when the user calls directly from oldinstance type.

        [OperatorMethod, PythonName("__hash__")]
        public object RichGetHashCode() {
            object func;
            if (Ops.TryGetBoundAttr(DefaultContext.Default, this, Symbols.Hash, out func)) {
                object res = PythonCalls.Call(func);
                if (!(res is int))
                    throw Ops.TypeError("expected int from __hash__, got {0}", Ops.StringRepr(Ops.GetDynamicType(res)));

                return (int)res;
            }

            if (Ops.TryGetBoundAttr(DefaultContext.Default, this, Symbols.Cmp, out func) ||
                Ops.TryGetBoundAttr(DefaultContext.Default, this, Symbols.OperatorEqual, out func)) {
                throw Ops.TypeError("unhashable instance");
            }

            return Ops.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [OperatorMethod, PythonName("__eq__")]
        public object RichEquals(object other) {
            object func;
            if (Ops.TryGetBoundAttr(DefaultContext.Default, this, Symbols.OperatorEqual, out func)) {
                object res = Ops.CallWithContext(DefaultContext.Default, func, other);
                if (res != Ops.NotImplemented) {
                    return res;
                }
            }

            if (Ops.TryGetBoundAttr(DefaultContext.Default, other, Symbols.OperatorEqual, out func)) {
                object res = Ops.CallWithContext(DefaultContext.Default, func, this);
                if (res != Ops.NotImplemented) {
                    return res;
                }
            }

            object coerce;
            if (TypeCache.OldInstance.TryInvokeBinaryOperator(DefaultContext.Default, Operators.Coerce, this, other, out coerce) &&
                coerce != Ops.NotImplemented &&
                !(coerce is OldInstance)) {
                return Ops.Equal(((Tuple)coerce)[0], ((Tuple)coerce)[1]);
            }

            return Ops.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [OperatorMethod, PythonName("__ne__")]
        public object RichNotEquals(object other) {
            object func;
            if (Ops.TryGetBoundAttr(DefaultContext.Default, this, Symbols.OperatorNotEqual, out func)) {
                return Ops.CallWithContext(DefaultContext.Default, func, other);
            }

            object res = RichEquals(other);
            if (res != Ops.NotImplemented) return Ops.Not(res);

            return Ops.NotImplemented;
        }

        #endregion

        #region ISerializable Members
#if !SILVERLIGHT // SerializationInfo

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("__class__", __class__);
            info.AddValue("__dict__", __dict__);
        }

#endif
        #endregion

        #region Private Implementation Details

        private void RecurseAttrHierarchyInt(OldClass oc, IDictionary<SymbolId, object> attrs) {
            foreach (SymbolId key in oc.__dict__.Keys) {
                if (!attrs.ContainsKey(key)) {
                    attrs.Add(key, key);
                }
            }
            //  recursively get attrs in parent hierarchy
            if (oc.BaseClasses.Count != 0) {
                foreach (OldClass parent in oc.BaseClasses) {
                    RecurseAttrHierarchyInt(parent, attrs);
                }
            }
        }

        private void AddFinalizer() {
            InstanceFinalizer oif = new InstanceFinalizer(this);
            weakRef = new WeakRefTracker(oif, oif);
        }

        private void ClearFinalizer() {
            if (weakRef == null) return;

            WeakRefTracker wrt = weakRef;
            if (wrt != null) {
                // find our handler and remove it (other users could have created weak refs to us)
                for (int i = 0; i < wrt.HandlerCount; i++) {
                    if (wrt.GetHandlerCallback(i) is InstanceFinalizer) {
                        wrt.RemoveHandlerAt(i);
                        break;
                    }
                }

                // we removed the last handler
                if (wrt.HandlerCount == 0) {
                    GC.SuppressFinalize(wrt);
                    weakRef = null;
                }
            }
        }

        private bool HasFinalizer() {
            if (weakRef != null) {
                WeakRefTracker wrt = weakRef;
                if (wrt != null) {
                    for (int i = 0; i < wrt.HandlerCount; i++) {
                        if (wrt.GetHandlerCallback(i) is InstanceFinalizer) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool TryRawGetAttr(CodeContext context, SymbolId name, out object ret) {
            if (__dict__.TryGetValue(name, out ret)) return true;

            if (__class__.TryLookupSlot(name, out ret)) {
                ret = __class__.GetOldStyleDescriptor(context, ret, this, __class__);
                return true;
            }

            return false;
        }

        #endregion

        #region IValueEquality Members

        public int GetValueHashCode() {
            object res = RichGetHashCode();
            if (res is int) {
                return (int)res;
            }
            return base.GetHashCode();
        }

        public bool ValueEquals(object other) {
            return Equals(other);
        }

        public bool ValueNotEquals(object other) {
            return !Equals(other);
        }

        #endregion
    }
}
