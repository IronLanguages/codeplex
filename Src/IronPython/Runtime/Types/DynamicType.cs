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
using System.Diagnostics;
using System.Collections.Generic;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    // DynamicType is the base of the hierarchy of new-style types in the Python world, and builtins.
    // 
    // It is extended by ReflectedType and UserType
    [PythonType("type")]
    public abstract partial class DynamicType : IPythonType, IDynamicObject, IFancyCallable, ICallableWithCallerContext, ICustomAttributes, IRichEquality, IPythonContainer {
        public object __name__;
        protected Tuple bases;

        /// <summary>
        /// allocates space for the object.  The ctor takes the type as the first parameter
        /// and any additional arguments after that.  Arguments are typically used for immutable types.
        /// </summary>
        internal FastCallable ctor;
        private List<WeakReference> subclasses = new List<WeakReference>();

        // The CLI type represented by this DynamicType
        public readonly Type type;

        public IAttributesDictionary dict;
        private Tuple methodResolutionOrder;

        // Cached attribute fields
        public MethodWrapper __getitem__F;
        public MethodWrapper __setitem__F;
        public MethodWrapper __cmp__F;

        public MethodWrapper __repr__F;
        public MethodWrapper __str__F;

        public MethodWrapper __getattribute__F;
        public MethodWrapper __getattr__F;
        public MethodWrapper __setattr__F;
        public MethodWrapper __delattr__F;
        public MethodWrapper __hash__F;

        private static object DefaultNewInst;

        [PythonName("__new__")]
        public static object Make(ICallerContext context, object cls, string name, Tuple bases, IDictionary<object, object> dict) {
            if (name == null) {
                throw Ops.TypeError("type() argument 1 must be string, not None");
            }
            if (bases == null) {
                throw Ops.TypeError("type() argument 2 must be tuple, not None");
            }
            if (dict == null) {
                throw Ops.TypeError("TypeError: type() argument 3 must be dict, not None");
            }

            if (!dict.ContainsKey("__module__")) {
                if (context.Module.ModuleName == null) {
                    dict["__module__"] = "__builtin__";
                } else {
                    dict["__module__"] = context.Module.ModuleName;
                }
            }

            DynamicType meta = cls as DynamicType;
            foreach (IPythonType dt in bases) {
                DynamicType metaCls = Ops.GetDynamicType(dt);

                if (metaCls == TypeCache.OldClass) continue;

                if (meta.IsSubclassOf(metaCls)) continue;

                if (metaCls.IsSubclassOf(meta)) {
                    meta = metaCls;
                    continue;
                }
                throw Ops.TypeError("metaclass conflict {0} and {1}", metaCls.__name__, meta.__name__);
            }


            UserType ut = meta as UserType;
            if (ut != null) {
                object newFunc = Ops.GetAttr(context, ut, SymbolTable.NewInst);

                if (meta != cls) {
                    if (DefaultNewInst == null) DefaultNewInst = Ops.GetAttr(context, Modules.Builtin.type, SymbolTable.NewInst);

                    // the user has a custom __new__ which picked the wrong meta class, call __new__ again
                    if (newFunc != DefaultNewInst) return Ops.Call(newFunc, ut, name, bases, dict);
                }

                // we have the right user __new__, call our ctor method which will do the actual
                // creation.
                return ut.ctor.Call(ut, name, bases, dict);
            }

            // no custom user type for __new__
            return UserType.MakeClass(name, bases, dict);
        }

        [PythonName("__new__")]
        public static object Make(ICallerContext context, object cls, object o) {
            return Ops.GetDynamicType(o);
        }

        public virtual object AllocateObject(params object[] args) {
            Initialize();
            if (ctor == null) throw Ops.TypeError("Cannot create instances of {0}", this);
            return ctor.Call(args);
        }

        public virtual object AllocateObject(Dict dict, params object[] args) {
            Initialize();
            if (ctor == null) throw Ops.TypeError("Cannot create instances of {0}", this);

            object[] finalArgs;
            string[] names;
            GetKeywordArgs(dict, args, out finalArgs, out names);

            return ((BuiltinFunction)ctor).CallHelper(DefaultContext.Default, finalArgs, names, null);
        }

        internal static void GetKeywordArgs(Dict dict, object[] args, out object[] finalArgs, out string[] names) {
            finalArgs = new object[args.Length + dict.Count];
            Array.Copy(args, finalArgs, args.Length);
            names = new string[dict.Count];
            int i = 0;
            foreach (KeyValuePair<object, object> kvp in (IDictionary<object, object>)dict) {
                names[i] = (string)kvp.Key;
                finalArgs[i + args.Length] = kvp.Value;
                i++;
            }
        }

        protected DynamicType(Type type) {
            this.type = type;
        }

        public virtual void Initialize() {
            if (methodResolutionOrder == null) methodResolutionOrder = CalculateMro(BaseClasses);
        }

        protected void AddProtocolWrappers() {
            if (type == typeof(object)) {
                AddProtocolWrappersForObject();
                return;
            }

            __getitem__F = MethodWrapper.Make(this, SymbolTable.GetItem);
            __setitem__F = MethodWrapper.Make(this, SymbolTable.SetItem);

            __getattribute__F = MethodWrapper.Make(this, SymbolTable.GetAttribute);
            __getattr__F = MethodWrapper.Make(this, SymbolTable.GetAttr);
            __setattr__F = MethodWrapper.Make(this, SymbolTable.SetAttr);
            __delattr__F = MethodWrapper.Make(this, SymbolTable.DelAttr);

            __cmp__F = MethodWrapper.Make(this, SymbolTable.Cmp);
            __repr__F = MethodWrapper.Make(this, SymbolTable.Repr);
            __str__F = MethodWrapper.Make(this, SymbolTable.String);
            __hash__F = MethodWrapper.Make(this, SymbolTable.Hash);
        }

        protected void AddModule() {
            if (!dict.ContainsKey(SymbolTable.Module)) {
                if (type.Assembly == typeof(DynamicType).Assembly ||
                    type.Assembly == typeof(IronMath.BigInteger).Assembly ||
                    this is OpsReflectedType
                ) {
                    string moduleName = null;
                    Type curType = type;
                    while (curType != null) {
                        TopReflectedPackage.builtinModuleNames.TryGetValue(curType, out moduleName);
                        curType = curType.DeclaringType;
                    }
                    dict[SymbolTable.Module] = moduleName ?? "__builtin__";
                } else {
                    dict[SymbolTable.Module] = type.Namespace + " in " + type.Assembly.FullName;
                }
            }
        }

        protected void AddProtocolWrappersForObject() {
            __getitem__F = MethodWrapper.MakeUndefined(this, SymbolTable.GetItem);
            __setitem__F = MethodWrapper.MakeUndefined(this, SymbolTable.SetItem);

            __getattribute__F = MethodWrapper.MakeForObject(this, SymbolTable.GetAttribute, new CallTarget2(GetAttributeMethod));
            __getattr__F = MethodWrapper.MakeUndefined(this, SymbolTable.GetAttr);
            __setattr__F = MethodWrapper.MakeForObject(this, SymbolTable.SetAttr, new CallTarget3(SetAttrMethod));
            __delattr__F = MethodWrapper.MakeForObject(this, SymbolTable.DelAttr, new CallTarget2(DelAttrMethod));

            __cmp__F = MethodWrapper.MakeUndefined(this, SymbolTable.Cmp);
            __str__F = MethodWrapper.MakeForObject(this, SymbolTable.String, new CallTarget1(StrMethod));
            __repr__F = MethodWrapper.MakeForObject(this, SymbolTable.Repr, new CallTarget1(ReprMethod));
            __hash__F = MethodWrapper.MakeForObject(this, SymbolTable.Hash, new CallTarget1(HashMethod));
        }

        public static object HashMethod(object self) {
            return Ops.SimpleHash(self);
        }

        public static object ReprMethod(object self) {
            return string.Format("<{0} object at {1}>", Ops.GetDynamicType(self).__name__, Ops.HexId(self));
        }

        public static object StrMethod(object self) {
            return Ops.Repr(self);
        }

        public static object GetAttributeMethod(object self, object name) {
            string strName = name as string;
            if (strName == null) throw Ops.TypeError("attribute name must be a string");

            object ret;
            SymbolId symbol = SymbolTable.StringToId(strName);
            ICustomAttributes ica = self as ICustomAttributes;
            if (ica != null) if (ica.TryGetAttr(DefaultContext.Default, symbol, out ret)) return ret;

            DynamicType pythonType = Ops.GetDynamicType(self);
            if (pythonType.TryBaseGetAttr(DefaultContext.Default, self, symbol, out ret)) return ret;
            throw Ops.AttributeError("no attribute {0}", name); //??? better message
        }

        public static object SetAttrMethod(object self, object name, object value) {
            string strName = name as string;
            if (strName == null) throw Ops.TypeError("attribute name must be a string");

            SymbolId symbol = SymbolTable.StringToId(strName);
            ICustomAttributes ica = self as ICustomAttributes;
            if (ica != null) {
                ica.SetAttr(DefaultContext.Default, symbol, value);
                return null;
            }

            DynamicType pythonType = Ops.GetDynamicType(self);
            pythonType.BaseSetAttr(DefaultContext.Default, self, symbol, value);
            return null;
        }

        public static object DelAttrMethod(object self, object name) {
            string strName = name as string;
            if (strName == null) throw Ops.TypeError("attribute name must be a string");

            SymbolId symbol = SymbolTable.StringToId(strName);
            ICustomAttributes ica = self as ICustomAttributes;
            if (ica != null) {
                ica.DeleteAttr(DefaultContext.Default, symbol);
                return null;
            }

            DynamicType pythonType = Ops.GetDynamicType(self);
            pythonType.BaseDelAttr(DefaultContext.Default, self, symbol);
            return null;
        }

        [PythonName("__subclasses__")]
        public List LookupSubclasses() {
            List l = new List();
            int i = 0;

            lock (subclasses) {
                while (i < subclasses.Count) {
                    if (subclasses[i].IsAlive && subclasses[i].Target != null) {
                        l.AddNoLock(subclasses[i].Target);
                        i++;
                    } else {
                        // class has been collected
                        subclasses.RemoveAt(i);
                    }
                }
            }
            return l;
        }

        internal static readonly List<Type> EmptyListOfInterfaces = new List<Type>();

        /// <summary>
        /// Python has different rules for which types may or may not be extended.
        /// For eg, Python allows primitives to be extended. Hence, GetTypesToExtend() will not always 
        /// be equal to DynamicType.type, and it could instead return a proxy CLI type.
        /// </summary>
        /// <param name="interfacesToExtend">Interfaces to be extended are returned in the out argument "interfaces". The return type could
        /// be typeof(object) in such cases. This will be an empty list, not null, if there are no interfaces.</param>
        /// <returns>Returns the CLI type to extend for implementing extending of a Python type. 
        /// This will be typeof(Object) for pure Python types.
        /// It can also return null if Python does not allow extending the given DynamicType.
        /// </returns>
        public abstract Type GetTypesToExtend(out IList<Type> interfacesToExtend);

        public void AddSubclass(DynamicType subclass) {
            // stored as a weak ref so when GC collects the subtypes we can
            // get rid of our reference.
            lock (subclass) {
                subclasses.Add(new WeakReference(subclass, true));
            }
        }

        public void RemoveSubclass(DynamicType subclass) {
            lock (subclass) {
                foreach (WeakReference subType in subclasses) {
                    if (subclass == (subType.Target as DynamicType)) {
                        subclasses.Remove(subType);
                        return;
                    }
                }
                Debug.Assert(false, "subclass was not found");
            }
        }

        public void UpdateFromBases() {
            __getitem__F.UpdateFromBases(MethodResolutionOrder);
            __setitem__F.UpdateFromBases(MethodResolutionOrder);

            __getattribute__F.UpdateFromBases(MethodResolutionOrder);
            __getattr__F.UpdateFromBases(MethodResolutionOrder);
            __setattr__F.UpdateFromBases(MethodResolutionOrder);
            __delattr__F.UpdateFromBases(MethodResolutionOrder);

            __cmp__F.UpdateFromBases(MethodResolutionOrder);
            __repr__F.UpdateFromBases(MethodResolutionOrder);
            __str__F.UpdateFromBases(MethodResolutionOrder);
            __hash__F.UpdateFromBases(MethodResolutionOrder);
        }

        public void UpdateSubclasses() {
            int i = 0;
            lock (subclasses) {
                while (i < subclasses.Count) {
                    if (subclasses[i].IsAlive) {
                        object target = subclasses[i].Target;
                        if (target != null) {
                            DynamicType pt = target as DynamicType;

                            System.Diagnostics.Debug.Assert(pt != null);

                            pt.UpdateFromBases();
                            pt.UpdateSubclasses();
                            i++;
                        } else {
                            subclasses.RemoveAt(i);
                        }
                    } else {
                        subclasses.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// This is called when __bases__ changes. We need to initialize all the state of the type and its subtypes
        /// </summary>
        protected void ReinitializeHierarchy() {
            MethodResolutionOrder = CalculateMro(BaseClasses);

            UpdateFromBases();

            foreach (UserType subUserType in LookupSubclasses()) {
                subUserType.ReinitializeHierarchy();
            }
        }

        internal virtual bool TryLookupBoundSlot(ICallerContext context, object inst, SymbolId name, out object ret) {
            if (TryLookupSlot(context, name, out ret)) {
                ret = UncheckedGetDescriptor(ret, inst, this);
                return true;
            }
            return false;
        }

        protected bool TryLookupSlot(ICallerContext context, SymbolId name, out object ret) {
            if (TryGetSlot(context, name, out ret)) return true;
            return TryLookupSlotInBases(context, name, out ret);
        }

        internal bool TryLookupSlotInBases(ICallerContext context, SymbolId name, out object ret) {
            Tuple resOrder = MethodResolutionOrder;

            for (int i = 1; i < resOrder.Count; i++) {  // skip our own type...
                object type = resOrder[i];

                DynamicType pt = type as DynamicType;
                if (pt != null) {
                    if (pt.TryGetSlot(context, name, out ret)) {
                        // respect MRO for base class lookups: method wrappers are
                        // logically a member of a super type, but are available on
                        // derived types for quick access.  We only want to return a method
                        // wrapper here if it's actually exposed on our type.

                        MethodWrapper mw = ret as MethodWrapper;
                        if (mw == null || !mw.IsSuperTypeMethod()) return true;
                    }
                } else {
                    // need to access OldClass's dict directly for this lookup because
                    // TryGetAttr will search subclasses first, which is wrong.
                    OldClass oc = type as OldClass;
                    Debug.Assert(oc != null);

                    if (oc.__dict__.TryGetValue(name, out ret)) return true;
                }
            }
            ret = null;
            return false;
        }

        internal bool TryGetSlot(ICallerContext context, SymbolId name, out object ret) {
            Initialize();

            switch (name.Id) {
                case SymbolTable.GetAttributeId: ret = __getattribute__F; return true;
                case SymbolTable.GetAttrId: ret = __getattr__F; return true;
                case SymbolTable.SetAttrId: ret = __setattr__F; return true;
                case SymbolTable.DelAttrId: ret = __delattr__F; return true;
                case SymbolTable.ReprId: ret = __repr__F; return true;
                case SymbolTable.StringId: ret = __str__F; return true;
                case SymbolTable.HashId: ret = __hash__F; return true;
                case SymbolTable.CmpId:
                    if (!__cmp__F.IsSuperTypeMethod()) {
                        ret = __cmp__F;
                        return true;
                    }
                    break;
                case SymbolTable.MethodResolutionOrderId:
                    if (methodResolutionOrder == null)
                        methodResolutionOrder = CalculateMro(BaseClasses);
                    ret = methodResolutionOrder;
                    return true;
            }

            if (dict.TryGetValue(name, out ret)) {
                IContextAwareMember icam = ret as IContextAwareMember;
                if (icam != null && !icam.IsVisible(context)) {
                    ret = null;
                    return false;
                }
                return true;
            }
            return false;
        }

        internal Tuple MethodResolutionOrder {
            get {
                if (methodResolutionOrder == null) methodResolutionOrder = CalculateMro(BaseClasses);
                return methodResolutionOrder;
            }
            set {
                methodResolutionOrder = value;
            }
        }
        protected virtual void RawSetSlot(SymbolId name, object value) {
            if (name == SymbolTable.Name) {
                __name__ = value;
                return;
            }
            dict[name] = value;
        }

        protected virtual void RawDeleteSlot(SymbolId name) {
            if (dict.ContainsKey(name)) {
                dict.Remove(name);
            } else {
                throw Ops.AttributeError("No attribute {0}.", name);
            }
        }

        public abstract DynamicType GetDynamicType();

        public virtual string Repr(object self) {
            Initialize();
            return (string)__repr__F.Invoke(self);
        }

        protected object[] PrependThis(object[] args) {
            object[] newArgs = new object[args.Length + 1];
            Array.Copy(args, 0, newArgs, 1, args.Length);
            newArgs[0] = this;
            return (newArgs);
        }

        public virtual bool IsPythonType {
            get {
                return true;
            }
        }

        [PythonName("mro")]
        public static object GetMethodResolutionOrder(object type) {
            throw new NotImplementedException("type.mro() is not yet supported");
        }

        // What kind of a class is it? Built-in, CLI, etc?
        protected abstract string TypeCategoryDescription {
            get;
        }

        #region ICallableWithCallerContext Members

        [PythonName("__call__")]
        public virtual object Call(ICallerContext context, params object[] args) {
            return Call(context, args, null);
        }
        #endregion

        #region IFancyCallable Members

        public object Call(ICallerContext context, object[] args, string[] names) {
            object newObject = CreateInstance(context, args, names);
            if (newObject == null) return newObject;

            DynamicType newObjectType = Ops.GetDynamicType(newObject);
            if (ShouldInvokeInit(newObjectType, args.Length)) {
                object init;
                if (newObjectType.TryLookupBoundSlot(context, newObject, SymbolTable.Init, out init)) {
                    if (names != null) Ops.CallWithContext(context, init, args, names);
                    else Ops.CallWithContext(context, init, args);
                }

                if (HasFinalizer) {
                    IWeakReferenceable iwr = newObject as IWeakReferenceable;
                    Debug.Assert(iwr != null);

                    InstanceFinalizer nif = new InstanceFinalizer(newObject);
                    iwr.SetFinalizer(new WeakRefTracker(nif, nif));
                }
            }

            return newObject;
        }

        internal object CreateInstance(ICallerContext context, object[] args, string[] names) {
            Initialize();

            object newFunc, newObject;

            // object always has __new__, so we'll always find at least one            
            TryLookupBoundSlot(context, this, SymbolTable.NewInst, out newFunc);
            Debug.Assert(newFunc != null);

            if (names != null) newObject = Ops.CallWithContext(context, newFunc, PrependThis(args), names);
            else newObject = Ops.CallWithContext(context, newFunc, PrependThis(args));

            return newObject;
        }

        private bool ShouldInvokeInit(DynamicType newObjectType, int argCnt) {
            // don't run __init__ if it's not a subclass of ourselves,
            // or if this is the user doing type(x)
            return newObjectType.IsSubclassOf(this) &&
                (this != TypeCache.DynamicType ||
                argCnt > 1);
        }

        #endregion

        protected virtual object InvokeSpecialMethod(SymbolId op, object self) {
            object func;
            if (TryLookupBoundSlot(DefaultContext.Default, self, op, out func)) {
                return Ops.Call(func);
            }
            return Ops.NotImplemented;
        }

        internal virtual object InvokeSpecialMethod(SymbolId op, object self, object arg0) {
            object func;
            if (TryLookupBoundSlot(DefaultContext.Default, self, op, out func)) {
                return Ops.Call(func, arg0);
            }
            return Ops.NotImplemented;
        }

        internal virtual object InvokeSpecialMethod(SymbolId op, object self, object arg0, object arg1) {
            object func;
            if (TryLookupBoundSlot(DefaultContext.Default, self, op, out func)) {
                return Ops.Call(func, arg0, arg1);
            }
            return Ops.NotImplemented;
        }

        internal bool TryInvokeSpecialMethod(object target, SymbolId name, out object ret, params object[] args) {
            object meth;
            if (TryLookupBoundSlot(DefaultContext.Default, target, name, out meth)) {
                ret = Ops.Call(meth, args);
                return true;
            } else {
                ret = null;
                return false;
            }
        }

        internal virtual object InvokeBinaryOperator(SymbolId op, object self, object arg0) {
            return InvokeSpecialMethod(op, self, arg0);
        }

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            switch (name.Id) {
                case SymbolTable.NameId: value = __name__; return true;
                case SymbolTable.BasesId: value = BaseClasses; return true;
                case SymbolTable.ClassId: value = ((IDynamicObject)this).GetDynamicType(); return true;
                case SymbolTable.SubclassesId:
                    BuiltinFunction rm = BuiltinFunction.MakeMethod("__subclasses__", typeof(DynamicType).GetMethod("LookupSubclasses"), FunctionType.PythonVisible | FunctionType.Method); ;
                    value = new BoundBuiltinFunction(rm, this);
                    return true;
                case SymbolTable.CallId:
                    //@todo We should provide a method wrapper object rather than directly returning ourselves
                    value = this;
                    return true;
                default:
                    if (TryLookupSlot(context, name, out value)) {
                        value = Ops.GetDescriptor(value, null, this);
                        return true;
                    }
                    break;
            }

            return false;
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            if (name == SymbolTable.Bases) BaseClasses = (Tuple)value;

            object slot;
            if (TryGetSlot(context, name, out slot)) {
                if (Ops.SetDescriptor(slot, null, value)) return;
            }

            RawSetSlot(name, value);
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            object slot;
            if (TryGetSlot(context, name, out slot)) {
                if (Ops.DelDescriptor(slot, null)) {
                    return;
                }
            }

            RawDeleteSlot(name);
        }

        public virtual List GetAttrNames(ICallerContext context) {
            Initialize();

            List names = new List();
            if ((context.ContextFlags & CallerContextAttributes.ShowCls) == 0) {
                // Filter out the non-CLS attribute names
                foreach (KeyValuePair<object, object> kvp in dict) {
                    if (kvp.Key is string && ((string)kvp.Key) == "__dict__") continue;

                    IContextAwareMember icaa = kvp.Value as IContextAwareMember;
                    if (icaa == null || icaa.IsVisible(context)) {
                        // This is a non-CLS attribute. Include it.
                        names.AddNoLock(kvp.Key);
                    }
                }
            } else {
                // Add all the attribute names
                names.AddRange(dict.Keys);

                // don't display dict on built-in types
                int index = names.IndexOf("__dict__");
                if (index != -1) names.RemoveAt(index);
            }

            foreach (IPythonType dt in BaseClasses) {
                if (dt is DynamicType && ((DynamicType)dt).type == typeof(DynamicType)) continue;

                if (dt != this) {
                    foreach (string name in Ops.GetAttrNames(context, dt)) {
                        if (name[0] == '_' && name[1] != '_') continue;
                        if (names.Contains(name)) continue;

                        names.AddNoLock(name);
                    }
                }
            }

            names.AddNoLockNoDups("__class__");
            return names;
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            Initialize();

            if ((context.ContextFlags & CallerContextAttributes.ShowCls) != 0) {
                // All the attributes should be displayed. Just return 'dict'
                return dict.AsObjectKeyedDictionary();
            }

            // We need to filter out the non-CLS attributes. So we create a new Dict, and
            // add just the non-CLS attributes
            Dict res = new Dict();

            foreach (KeyValuePair<object, object> kvp in dict) {
                IContextAwareMember icaa = kvp.Value as IContextAwareMember;
                if (icaa == null || icaa.IsVisible(context)) {
                    // This is a non-CLS attribute. Include it.
                    res[kvp.Key.ToString()] = kvp.Value;
                }
            }

            return res;
        }

        #endregion

        public virtual List GetAttrNames(ICallerContext context, object self) {
            // Get the entries from the type
            List ret = GetAttrNames(context);

            // Add the entries from the instance
            ISuperDynamicObject sdo = self as ISuperDynamicObject;
            if (sdo != null) {
                if (sdo.GetDict() != null) {
                    ICollection<object> keys = sdo.GetDict().Keys;
                    foreach (object key in keys) {
                        ret.AddNoLockNoDups(key);
                    }
                }
            }
            return ret;
        }

        public virtual Dict GetAttrDict(ICallerContext context, object self) {
            // Get the entries from the type
            Dict res = new Dict(GetAttrDict(context));

            // Add the entries from the instance
            ISuperDynamicObject sdo = self as ISuperDynamicObject;
            if (sdo != null) {
                IAttributesDictionary dict = sdo.GetDict();
                if (dict != null) {
                    foreach (KeyValuePair<object, object> val in dict) {
                        object fieldName = val.Key;
                        if (!res.ContainsKey(fieldName)) {
                            res.Add(new KeyValuePair<object, object>(fieldName, val.Value));
                        }
                    }
                }
            }
            return res;
        }

        protected virtual Tuple CalculateMro(Tuple bases) {
            return Mro.Calculate(this, bases);
        }

        public string Name {
            get { return __name__.ToString(); }
        }

        protected virtual bool HasFinalizer {
            get {
                return false;
            }
            set {
                throw new InvalidOperationException("only UserTypes can have a finalizer");
            }
        }

        #region Overloaded Unary/Binary operators

        public object Negate(object self) {
            return InvokeSpecialMethod(SymbolTable.OpNegate, self);
        }

        public object Positive(object self) {
            return InvokeSpecialMethod(SymbolTable.Positive, self);
        }

        public object OnesComplement(object self) {
            return InvokeSpecialMethod(SymbolTable.OpOnesComplement, self);
        }

        public virtual object CompareTo(object self, object other) {
            return InvokeSpecialMethod(SymbolTable.Cmp, self, other);
        }

        public virtual int GetInstanceLength(object self) {
            object ret;
            if (Ops.TryInvokeSpecialMethod(self, SymbolTable.Length, out ret)) {
                return Converter.ConvertToInt32(ret);
            }

            throw Ops.TypeError("len() of unsized object of type {0}", Ops.StringRepr(Ops.GetDynamicType(self)));
        }

        #endregion

        public object PowerMod(object self, object other, object mod) {
            return InvokeSpecialMethod(SymbolTable.OpPower, self, other, mod);
        }

        public virtual object CallOnInstance(object func, object[] args) {
            object ret;
            if (TryInvokeSpecialMethod(func, SymbolTable.Call, out ret, args)) return ret;
            throw Ops.TypeErrorForBadInstance("{0} object is not callable", func);
        }

        public object CallOnInstance(object func, object[] args, string[] names) {
            object meth;
            if (TryLookupBoundSlot(DefaultContext.Default, func, SymbolTable.Call, out meth)) {
                return Ops.Call(DefaultContext.Default, meth, args, names);
            } else {
                throw Ops.TypeErrorForBadInstance("{0} object is not callable with keyword arguments", func);
            }
        }

        public abstract Tuple BaseClasses {
            get;
            set;
        }

        [PythonName("__getitem__")]
        public virtual object GetIndex(object self, object index) {
            return GetIndexHelper(self, index);
        }

        internal static object GetIndexHelper(object self, object index) {
            Slice slice = index as Slice;
            if (slice != null && slice.Step == null) {
                object getSlice;
                if (Ops.TryGetAttr(DefaultContext.Default, self, SymbolTable.GetSlice, out getSlice)) {
                    int start, stop;
                    slice.DeprecatedFixed(self, out start, out stop);
                    return Ops.Call(getSlice, start, stop);
                }
            }

            return Ops.Invoke(self, SymbolTable.GetItem, index);
        }

        [PythonName("__setitem__")]
        public virtual void SetIndex(object self, object index, object value) {
            SetIndexHelper(self, index, value);
        }

        internal static void SetIndexHelper(object self, object index, object value) {
            Slice slice = index as Slice;
            if (slice != null && slice.Step == null) {
                object setSlice;
                if (Ops.TryGetAttr(DefaultContext.Default, self, SymbolTable.SetSlice, out setSlice)) {
                    int start, stop;
                    slice.DeprecatedFixed(self, out start, out stop);
                    Ops.Call(setSlice, start, stop, value);
                    return;
                }
            }

            Ops.Invoke(self, SymbolTable.SetItem, index, value);
        }

        [PythonName("__delitem__")]
        public void DelIndex(object self, object index) {
            Slice slice = index as Slice;
            if (slice != null && slice.Step == null) {
                object delSlice;
                if (Ops.TryGetAttr(DefaultContext.Default, self, SymbolTable.DeleteSlice, out delSlice)) {
                    int start, stop;
                    slice.DeprecatedFixed(self, out start, out stop);

                    Ops.Call(delSlice, start, stop);
                    return;
                }
            }

            Ops.Invoke(self, SymbolTable.DelItem, index);
        }

        public virtual bool TryGetAttr(ICallerContext context, object self, SymbolId name, out object ret) {
            return TryBaseGetAttr(context, self, name, out ret);
        }

        public virtual object GetAttr(ICallerContext context, object self, SymbolId name) {
            object ret;
            if (TryGetAttr(context, self, name, out ret)) return ret;
            throw Ops.AttributeErrorForMissingAttribute(this.Name, name);
        }

        public virtual void SetAttr(ICallerContext context, object self, SymbolId name, object value) {
            BaseSetAttr(context, self, name, value);
        }

        public virtual void DelAttr(ICallerContext context, object self, SymbolId name) {
            BaseDelAttr(context, self, name);
        }

        internal object UncheckedGetDescriptor(object o, object instance, object context) {
            BuiltinMethodDescriptor md = o as BuiltinMethodDescriptor;
            if (md != null) return md.UncheckedGetAttribute(instance);

            return Ops.GetDescriptor(o, instance, context);
        }

        // These are the default base implementations of attribute-access.
        internal virtual bool TryBaseGetAttr(ICallerContext context, object self, SymbolId name, out object ret) {
            if (name == SymbolTable.Dict) {
                // Instances of builtin types do not have "__dict__"
                ret = null;
                return false;
            }

            if (TryLookupSlot(context, name, out ret)) {
                ret = UncheckedGetDescriptor(ret, self, this);
                return true;
            }

            if (name == SymbolTable.Class) { ret = this; return true; }

            return false;
        }

        protected void ThrowAttributeError(bool slotExists, SymbolId attributeName) {
            if (slotExists) {
                throw Ops.AttributeErrorForReadonlyAttribute(Name, attributeName);
            } else {
                throw Ops.AttributeErrorForMissingAttribute(Name, attributeName);
            }
        }

        internal virtual void BaseSetAttr(ICallerContext context, object self, SymbolId name, object value) {
            if (name == SymbolTable.Class) throw Ops.AttributeErrorForReadonlyAttribute(Name, name);

            object slot;
            if (TryGetSlot(context, name, out slot)) {
                if (!Ops.SetDescriptor(slot, self, value)) {
                    throw Ops.AttributeErrorForReadonlyAttribute(Name, name);
                }
            } else {
                throw Ops.AttributeErrorForMissingAttribute(Name, name);
            }
        }
        internal virtual void BaseDelAttr(ICallerContext context, object self, SymbolId name) {
            if (name == SymbolTable.Class) throw Ops.AttributeErrorForReadonlyAttribute(Name, name);

            object slot;
            if (TryGetSlot(context, name, out slot)) {
                if (!Ops.DelDescriptor(slot, self)) {
                    throw Ops.AttributeErrorForReadonlyAttribute(Name, name);
                }
            } else {
                throw Ops.AttributeErrorForMissingAttribute(Name, name);
            }
        }

        /// <summary>
        /// Is "other" a base type?
        /// </summary>
        /// <param name="other">This is usually a DynamicType. However, it only really needs to satisfy 
        /// "inspect.isclass(other)" which holds for DynamicTypes as well any object with a "__bases__" attribute.</param>
        public virtual bool IsSubclassOf(object other) {
            throw new NotImplementedException();
        }

        internal bool IsInstanceOfType(object o) {
            return Ops.GetDynamicType(o).IsSubclassOf(this);
        }

        internal static DynamicType GetDeclaringType(MemberInfo member) {
            Type declaringType = member.DeclaringType;
            if (OpsReflectedType.OpsTypeToType.ContainsKey(declaringType)) {
                // declaringType is an Ops type
                return OpsReflectedType.OpsTypeToType[declaringType];
            } else {
                return Ops.GetDynamicTypeFromType(declaringType);
            }
        }

        public object Equal(object self, object other) {
            object ret = InvokeBinaryOperator(SymbolTable.OpEqual, self, other);
            if (ret != Ops.NotImplemented) return ret;

            ret = InvokeBinaryOperator(SymbolTable.Cmp, self, other);
            if (ret != Ops.NotImplemented) return Ops.Bool2Object(Ops.CompareToZero(ret) == 0);

            return Ops.NotImplemented;
        }

        public object NotEqual(object self, object other) {
            object ret = InvokeBinaryOperator(SymbolTable.OpNotEqual, self, other);
            if (ret != Ops.NotImplemented) return ret;

            ret = InvokeBinaryOperator(SymbolTable.Cmp, self, other);
            if (ret != Ops.NotImplemented) return Ops.Bool2Object(Ops.CompareToZero(ret) != 0);

            return Ops.NotImplemented;
        }

        public virtual object Coerce(object self, object other) {
            return Ops.NotImplemented;
        }

        public object Invoke(object target, SymbolId name, params object[] args) {
            return Ops.Call(Ops.GetAttr(DefaultContext.Default, target, name), args);
        }

        public object InvokeSpecialMethod(object target, SymbolId name, params object[] args) {
            object ret;
            if (TryInvokeSpecialMethod(target, name, out ret, args)) return ret;

            throw Ops.TypeError("{0} object has no attribute '{1}'",
                Ops.StringRepr(Ops.GetDynamicType(target)),
                name.ToString());
        }

        #region IRichEquality Members

        public object RichGetHashCode() {
            return this.GetHashCode();
        }

        public object RichEquals(object other) {
            return this.Equals(other);
        }

        public object RichNotEquals(object other) {
            return !this.Equals(other);
        }

        #endregion

        #region IPythonContainer Members

        public int GetLength() {
            return 1;
        }

        public bool ContainsValue(object value) {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion


        /// <summary>
        /// Provides a slot object for the dictionary to allow setting of the dictionary.
        /// </summary>
        [PythonType("getset_descriptor")]
        public sealed class DictWrapper : IDataDescriptor, ICodeFormattable {
            DynamicType type;

            public DictWrapper(DynamicType pt) {
                type = pt;
            }

            #region IDataDescriptor Members

            public bool SetAttribute(object instance, object value) {
                ISuperDynamicObject sdo = instance as ISuperDynamicObject;
                if (sdo != null) {
                    if (!(value is IAttributesDictionary))
                        throw Ops.TypeError("__dict__ must be set to a dictionary, not '{0}'", Ops.GetDynamicType(value).__name__);

                    return sdo.SetDict((IAttributesDictionary)value);
                }

                if (instance == null) throw Ops.TypeError("'__dict__' of '{0}' objects is not writable", Ops.GetDynamicType(type).__name__);
                return false;
            }

            public bool DeleteAttribute(object instance) {
                ISuperDynamicObject sdo = instance as ISuperDynamicObject;
                if (sdo != null) {
                    return sdo.SetDict(null);
                }

                if (instance == null) throw Ops.TypeError("'__dict__' of '{0}' objects is not writable", Ops.GetDynamicType(type).__name__);
                return false;
            }

            #endregion

            #region IDescriptor Members

            public object GetAttribute(object instance, object owner) {
                ISuperDynamicObject sdo = instance as ISuperDynamicObject;
                if (sdo != null) {
                    return sdo.GetDict();
                }

                if (instance == null) return type.dict;

                throw Ops.TypeError("type {0} has no dict", Ops.StringRepr(type));
            }

            #endregion

            #region ICodeFormattable Members

            public string ToCodeString() {
                return String.Format("<attribute '__dict__' of {0} objects",
                    Ops.StringRepr(type));
            }

            #endregion
        }

        [PythonType("getset_descriptor")]
        public sealed class WeakRefWrapper : IDataDescriptor, ICodeFormattable {
            DynamicType parentType;

            public WeakRefWrapper(DynamicType parent) {
                this.parentType = parent;
            }

            #region IDataDescriptor Members

            public bool SetAttribute(object instance, object value) {
                IWeakReferenceable reference = instance as IWeakReferenceable;
                if (reference != null) {
                    return reference.SetWeakRef(new WeakRefTracker(value, instance));
                }
                return false;
            }

            public bool DeleteAttribute(object instance) {
                throw Ops.TypeError("__weakref__ attribute cannot be deleted");
            }

            #endregion

            #region IDescriptor Members

            public object GetAttribute(object instance, object owner) {
                if (instance == null) return this;

                IWeakReferenceable reference = instance as IWeakReferenceable;
                if (reference != null) {
                    WeakRefTracker tracker = reference.GetWeakRef();
                    if (tracker == null || tracker.HandlerCount == 0) return null;

                    return tracker.GetHandlerCallback(0);
                }
                throw Ops.TypeError("'{0}' has no attribute __weakref__", Ops.GetDynamicType(instance));
            }

            #endregion

            public override string ToString() {
                return String.Format("<attribute '__weakref__' of '{0}' objects>", parentType.__name__);
            }

            #region ICodeFormattable Members

            public string ToCodeString() {
                return String.Format("<attribute '__weakref__' of {0} objects",
                    Ops.StringRepr(parentType));
            }

            #endregion
        }
    }
}
