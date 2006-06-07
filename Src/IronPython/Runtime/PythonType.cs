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

using System.Diagnostics;
using System.Reflection;

namespace IronPython.Runtime {
    // PythonType is the base of the hierarchy of new-style types in the Python world, and builtins.
    // 
    // It is extended by:
    //   ReflectedType
    //   UserType

    [PythonType("type")]
    public abstract class PythonType : DynamicType, IDynamicObject, ICallable, ICustomAttributes {
        public ICallable ctor;
        private List<WeakReference> subclasses = new List<WeakReference>();

        // The CLI type represented by this PythonType
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

            DynamicType meta = cls as PythonType;
            foreach (DynamicType dt in bases) {
                DynamicType metaCls = Ops.GetDynamicType(dt);

                if (metaCls == OldInstanceType.Instance) continue;

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
                    if(newFunc != DefaultNewInst) return Ops.Call(newFunc, ut, name, bases, dict);
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

        protected PythonType(Type type) {
            this.type = type;
        }

        internal static int GetMaxArgs(ICallable c) {
            if (c is ReflectedMethodBase) return ((ReflectedMethodBase)c).GetMaximumArguments();
            else if (c is BuiltinFunction) return ((BuiltinFunction)c).GetMaximumArguments();
            else return 10;
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
        }

        protected void AddModule() {
            if (type.Assembly == typeof(PythonType).Assembly) {
                dict[SymbolTable.Module] = "__builtin__";
            } else {
                dict[SymbolTable.Module] = type.Namespace + " in " + type.Assembly.FullName;
            }
        }

        protected void AddProtocolWrappersForObject() {
            __getitem__F = MethodWrapper.MakeUndefined(this, SymbolTable.GetItem);
            __setitem__F = MethodWrapper.MakeUndefined(this, SymbolTable.SetItem);

            __getattribute__F = MethodWrapper.MakeForObject(this, SymbolTable.GetAttribute, new CallTarget2(GetAttributeMethod));
            __getattr__F = MethodWrapper.MakeUndefined(this, SymbolTable.GetAttr);
            __setattr__F = MethodWrapper.MakeForObject(this, SymbolTable.SetAttr, new CallTarget3(SetAttrMethod));
            __delattr__F = MethodWrapper.MakeForObject(this, SymbolTable.DelAttr, new CallTarget2(DelAttrMethod));

            __cmp__F = MethodWrapper.MakeUndefined(this, SymbolTable.Cmp); //!!!
            __str__F = MethodWrapper.MakeForObject(this, SymbolTable.String, new CallTarget1(StrMethod));
            __repr__F = MethodWrapper.MakeForObject(this, SymbolTable.Repr, new CallTarget1(ReprMethod));
        }

        public static object ReprMethod(object self) {
            return string.Format("<{0} object at {1}>", Ops.GetDynamicType(self).__name__, Ops.HexId(self));
        }

        public static object StrMethod(object self) {
            return Ops.Repr(self);
        }

        // These are the default base implementations of attribute-access.
        abstract internal bool TryBaseGetAttr(ICallerContext context, object self, SymbolId name, out object ret);
        abstract internal void BaseSetAttr(ICallerContext context, object self, SymbolId name, object value);
        abstract internal void BaseDelAttr(ICallerContext context, object self, SymbolId name);

        public static object GetAttributeMethod(object self, object name) {
            string strName = name as string;
            if (strName == null) throw Ops.TypeError("attribute name must be a string");

            PythonType pythonType = Ops.GetDynamicType(self) as PythonType;
            object ret;
            if (pythonType.TryBaseGetAttr(DefaultContext.Default, self, SymbolTable.StringToId(strName), out ret)) return ret;
            throw Ops.AttributeError("no attribute {0}", name); //??? better message
        }

        public static object SetAttrMethod(object self, object name, object value) {
            string strName = name as string;
            if (strName == null) throw Ops.TypeError("attribute name must be a string");

            PythonType pythonType = Ops.GetDynamicType(self) as PythonType;
            pythonType.BaseSetAttr(DefaultContext.Default, self, SymbolTable.StringToId(strName), value);
            return null;
        }

        public static object DelAttrMethod(object self, object name) {
            string strName = name as string;
            if (strName == null) throw Ops.TypeError("attribute name must be a string");

            PythonType pythonType = Ops.GetDynamicType(self) as PythonType;
            pythonType.BaseDelAttr(DefaultContext.Default, self, SymbolTable.StringToId(strName));
            return null;
        }

        /// <summary>
        /// Looks up a __xyz__ method in slots only (because we should never lookup
        /// in instance members for these)
        /// </summary>
        internal static bool TryLookupSpecialMethod(object self, SymbolId symbol, out object ret) {
            return TryLookupSpecialMethod(DefaultContext.Default, self, symbol, out ret);
        }

        /// <summary>
        /// Looks up a __xyz__ method in slots only (because we should never lookup
        /// in instance members for these)
        /// </summary>
        internal static bool TryLookupSpecialMethod(ICallerContext context, object self, SymbolId symbol, out object ret) {
            IDynamicObject ido = self as IDynamicObject;
            if (ido != null) {
                PythonType pt = (PythonType)ido.GetDynamicType();

                if (pt.TryLookupSlot(context, symbol, out ret)) {
                    ret = Ops.GetDescriptor(ret, self, pt);
                    return true;
                }
            }
            ret = null;
            return false;
        }



        public List __subclasses__() {
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

        protected static readonly List<Type> EmptyListOfInterfaces = new List<Type>();

        /// <summary>
        /// Python has different rules for which types may or may not be extended.
        /// For eg, Python allows primitives to be extended. Hence, GetTypesToExtend() will not always 
        /// be equal to PythonType.type, and it could instead return a proxy CLI type.
        /// </summary>
        /// <param name="interfacesToExtend">Interfaces to be extended are returned in the out argument "interfaces". The return type could
        /// be typeof(object) in such cases. This will be an empty list, not null, if there are no interfaces.</param>
        /// <returns>Returns the CLI type to extend for implementing extending of a Python type. 
        /// This will be typeof(Object) for pure Python types.
        /// It can also return null if Python does not allow extending the given PythonType.
        /// </returns>
        public abstract Type GetTypesToExtend(out IList<Type> interfacesToExtend);

        public void AddSubclass(PythonType subclass) {
            // stored as a weak ref so when GC collects the subtypes we can
            // get rid of our reference.
            lock (subclass) {
                subclasses.Add(new WeakReference(subclass, true));
            }
            //!!! propogation is needed
        }

        public void RemoveSubclass(PythonType subclass) {
            lock (subclass) {
                foreach(WeakReference subType in subclasses) {
                    if (subclass == (subType.Target as PythonType)) {
                        subclasses.Remove(subType);
                        return;
                    }
                }
                Debug.Assert(false, "subclass was not found");
            }
        }

        //!!! this should be field specific
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
        }

        public void UpdateSubclasses() {
            int i = 0;
            lock (subclasses) {
                while (i < subclasses.Count) {
                    if (subclasses[i].IsAlive) {
                        object target = subclasses[i].Target;
                        if (target != null) {
                            PythonType pt = target as PythonType;

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

            foreach (UserType subUserType in __subclasses__()) {
                subUserType.ReinitializeHierarchy();
            }
        }

        internal bool TryLookupBoundSlot(ICallerContext context, object inst, SymbolId name, out object ret) {
            if (TryLookupSlot(context, name, out ret)) {
                ret = Ops.GetDescriptor(ret, inst, this);
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

                PythonType pt = type as PythonType;
                if (pt != null) {
                    if (pt.TryGetSlot(context, name, out ret)) {
                        // respect MRO for base class lookups: method wrappers are
                        // logically a member of a super type, but are available on
                        // derived types for quick access.  We only want to return a method
                        // wrapper here if it's actually exposed on our type.

                        MethodWrapper mw = ret as MethodWrapper;
                        if (mw == null || !mw.IsSuperTypeMethod()) return true;
                    } 
                } else if (Ops.TryGetAttr(context, type, name, out ret)) {
                    return true;
                }
            }
            ret = null;
            return false;
        }
        
        internal bool TryGetSlot(ICallerContext context, SymbolId name, out object ret) {
            Initialize();

            switch(name.Id){
                case SymbolTable.DictId: ret = new DictWrapper(this); return true;
                case SymbolTable.GetAttributeId:  ret = __getattribute__F;  return true;                    
                case SymbolTable.GetAttrId: ret = __getattr__F; return true;
                case SymbolTable.SetAttrId: ret = __setattr__F; return true;
                case SymbolTable.DelAttrId: ret = __delattr__F; return true;
                case SymbolTable.ReprId: ret = __repr__F; return true;
                case SymbolTable.StringId: ret = __str__F; return true;
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

        public virtual object Call(params object[] args) { throw new NotImplementedException(); }
        public virtual DynamicType GetDynamicType() { throw new NotImplementedException(); }

        internal bool IsInstanceOfType(object o) {
            return Ops.GetDynamicType(o).IsSubclassOf(this);
        }

        public override string Repr(object self) {
            //!!!
            Initialize();
            return (string)__repr__F.Invoke(self);
        }

        protected object[] PrependThis(object[] args) {
            object[] newArgs = new object[args.Length + 1];
            Array.Copy(args, 0, newArgs, 1, args.Length);
            newArgs[0] = this;
            return (newArgs);
        }

        public void InvokeInit(object inst, object[] args) {            
            object initFunc;

            if (Ops.GetDynamicType(inst).IsSubclassOf(this)) {
                if (TryLookupBoundSlot(DefaultContext.Default, inst, SymbolTable.Init, out initFunc)) {
                    //!!!initFunc = Ops.GetDescriptor(initFunc, newObject, this);
                    switch (args.Length) {
                        case 0: Ops.Call(initFunc); break;
                        case 1: Ops.Call(initFunc, args[0]); break;
                        case 2: Ops.Call(initFunc, args[0], args[1]); break;
                        default:
                            Ops.Call(initFunc, args);
                            break;
                    }
                }
            }
        }

        public void InvokeInit(object inst, object[] args, string[] names) {
            if (Ops.GetDynamicType(inst).IsSubclassOf(this)) {
                object initFunc;
                if (TryLookupBoundSlot(DefaultContext.Default, inst, SymbolTable.Init, out initFunc)) {
                    IFancyCallable ifc = initFunc as IFancyCallable;
                    if (ifc != null) {
                        ifc.Call(DefaultContext.Default, args, names);
                    } else {
                        throw Ops.TypeError("__init__ cannot be called with keyword arguments");
                    }
                }
            }
        }

        public virtual bool IsPythonType {
            get {
                return true;
            }
        }

        [PythonName("mro")]
        public static object GetMethodResolutionOrder(object type) {
            throw new NotSupportedException("type.mro() is not yet supported");
        }

        // What kind of a class is it? Built-in, CLI, etc?
        protected abstract string TypeCategoryDescription {
            get; 
        }

        #region ICustomAttributes Members

        public virtual bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            switch(name.Id){
                case SymbolTable.NameId: value = __name__; return true;
                case SymbolTable.BasesId: value = BaseClasses; return true;
                case SymbolTable.ClassId: value = Ops.GetDynamicType(this); return true;
                case SymbolTable.SubclassesId:
                    ReflectedMethod rm = new ReflectedMethod("__subclasses__", typeof(ReflectedType).GetMethod("__subclasses__"), FunctionType.PythonVisible | FunctionType.Method); ;
                    rm.inst = this;
                    value = rm;
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
            if ((context.ContextFlags & CallerContextFlags.ShowCls) == 0) {
                // Filter out the non-CLS attribute names
                foreach (KeyValuePair<object, object> kvp in dict) {
                    IContextAwareMember icaa = kvp.Value as IContextAwareMember;
                    if (icaa == null || icaa.IsVisible(context)) {
                        // This is a non-CLS attribute. Include it.
                        names.AddNoLock(kvp.Key.ToString());
                    }
                }
            } else {
                // Add all the attribute names
                names.AddRange(dict.Keys);
            }

            foreach (DynamicType dt in BaseClasses) {
                if (dt != TypeCache.Object) {
                    foreach (string name in Ops.GetAttrNames(context, dt)) {
                        if (name[0] == '_' && name[1] != '_') continue;
                        if (names.Contains(name)) continue;

                        names.AddNoLock(name);
                    }
                }
            }

            if (!names.Contains("__class__"))
                names.AddNoLock("__class__");
            return names;
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            Initialize();

            if ((context.ContextFlags & CallerContextFlags.ShowCls) != 0) {
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
        
        public override List GetAttrNames(ICallerContext context, object self) {
            // Get the entries from the type
            List ret = GetAttrNames(context);

            // Add the entries from the instance
            ISuperDynamicObject sdo = self as ISuperDynamicObject;
            if (sdo != null) {
                if (sdo.GetDict() != null) {
                    ICollection<object> keys = sdo.GetDict().Keys;
                    foreach (object key in keys) {
                        if (!ret.Contains(key)) {
                            ret.AddNoLock(key);
                        }
                    }
                }
            }
            return ret;
        }

        public override Dict GetAttrDict(ICallerContext context, object self) {
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

        #region Method Resolution Order Generation

        private class MroRatingInfo : List<DynamicType> {
            public int Order;
            public bool Processing;

            public MroRatingInfo()
                : base() {
            }
        }

        protected virtual Tuple CalculateMro(Tuple bases) {
            // the rules are:
            //      If A is a subtype of B, then A has precedence (A >B)
            //      If C appears before D in the list of bases then C > D
            //      If E > F in one __mro__ then E > F in all __mro__'s for our subtype
            // 
            // class A(object): pass
            // class B(object): pass
            // class C(B): pass
            // class N(A,B,C): pass         # illegal
            //
            // This is because:
            //      C.__mro__ == (C, B, object)
            //      N.__mro__ == (N, A, B, C, object)
            // which would conflict, but:
            //
            // N(B,A) is ok  (N, B, a, object)
            // N(C, B, A) is ok (N, C, B, A, object)
            //
            // To calculate this we build a graph that is based upon the first two
            // properties: the base classes, and the order in which the base classes
            // are ordered.  At the same time of building the graph we store the
            // order the classes occur so that classes that come earlier in the
            // precedence list but at the same depth from the newly defined type have
            // lower values.  In the case of two classes that have the same weight
            // we use this distance to disambiguate, and select the class that appeared
            // earliest in the inheritance hierachy.
            // 
            // Once we've built the graph we recursively walk it, and if we detect
            // any cycles the MRO is invalid.  The graph is stored using a dictionary
            // of dynamic types mapping to the types that are less than it.  If there
            // are no cycles we propagate the types in the lists up the graph so that
            // the top most type contains all the types less than it.  
            // 
            // Finally we sort that list w/ the largest # of types winning, and that
            // sorted order is our MRO.

            Dictionary<DynamicType, MroRatingInfo> classes = new Dictionary<DynamicType, MroRatingInfo>();

            // start w/ the provided bases, and build the graph that merges inheritance
            // and linear order as defined by the base classes
            int count = 1;
            GenerateMroGraph(classes, this, bases, ref count);

            // then propagate all the classes reachable from one node into that node
            // so we get the overall weight of each node
            foreach (DynamicType dt in classes.Keys) {
                PropagateBases(classes, dt);
            }

            // get the sorted keys, based upon weight, ties decided by order
            KeyValuePair<DynamicType, MroRatingInfo>[] kvps = GetSortedRatings(classes);
            DynamicType[] mro = new DynamicType[classes.Count];

            // and then we have our mro...
            for (int i = 0; i < mro.Length; i++) {
                mro[i] = kvps[i].Key;
            }

            return new Tuple(false, mro);
        }

        /// <summary>
        /// Gets the keys sorted by their weight & ties settled by the order in which
        /// classes appear in the class hierarchy given the calculated graph
        /// </summary>
        private static KeyValuePair<DynamicType, MroRatingInfo>[] GetSortedRatings(Dictionary<DynamicType, MroRatingInfo> classes) {
            KeyValuePair<DynamicType, MroRatingInfo>[] kvps = new KeyValuePair<DynamicType, MroRatingInfo>[classes.Count];
            ((ICollection<KeyValuePair<DynamicType, MroRatingInfo>>)classes).CopyTo(kvps, 0);

            // sort the array, so the largest lists are on top
            Array.Sort<KeyValuePair<DynamicType, MroRatingInfo>>(kvps,
                delegate(KeyValuePair<DynamicType, MroRatingInfo> x, KeyValuePair<DynamicType, MroRatingInfo> y) {
                    if (x.Key == y.Key) return 0;

                    int res = y.Value.Count - x.Value.Count;
                    if (res != 0) return res;

                    // two classes are of the same precedence, 
                    // we need to look at which one is left
                    // most - luckily we already stored that info
                    return x.Value.Order - y.Value.Order;
                });
            return kvps;
        }

        /// <summary>
        /// Updates our classes dictionary from a type's base classes, updating for both
        /// the inheritance hierachy as well as the order the types appear as bases.
        /// </summary>
        private static void GenerateMroGraph(Dictionary<DynamicType, MroRatingInfo> classes, DynamicType parent, Tuple bases, ref int count) {
            MroRatingInfo parentList, innerList;
            if (!classes.TryGetValue(parent, out parentList))
                parentList = classes[parent] = new MroRatingInfo();

            // only set order once - if we see a type multiple times,
            // the earliest one is the most important.
            if (parentList.Order == 0) parentList.Order = count++;

            DynamicType prevType = null;
            foreach (DynamicType baseType in bases) {
                if (!parentList.Contains(baseType)) parentList.Add(baseType);

                if (prevType != null) {
                    if (!classes.TryGetValue(prevType, out innerList))
                        innerList = classes[prevType] = new MroRatingInfo();

                    if (!innerList.Contains(baseType)) innerList.Add(baseType);
                }

                prevType = baseType;

                GenerateMroGraph(classes, baseType, baseType.BaseClasses, ref count);
            }
        }

        private static void PropagateBases(Dictionary<DynamicType, MroRatingInfo> classes, DynamicType dt) {
            MroRatingInfo innerInfo, mroInfo = classes[dt];
            mroInfo.Processing = true;

            // recurse down to the bottom of the tree
            foreach (DynamicType lesser in mroInfo) {
                DynamicType lesserType = lesser;

                if (classes[lesserType].Processing) throw Ops.TypeError("invalid order for base classes: {0} and {1}", dt.__name__, lesserType.__name__);

                PropagateBases(classes, lesserType);
            }

            // then propagate the bases up the tree as we go.
            int startingCount = mroInfo.Count;
            for (int i = 0; i < startingCount; i++) {
                DynamicType lesser = mroInfo[i];

                if (!classes.TryGetValue(lesser, out innerInfo)) continue;

                foreach (DynamicType newList in innerInfo) {
                    if (!mroInfo.Contains(newList)) mroInfo.Add(newList);
                }
            }

            mroInfo.Processing = false;
        }

        #endregion
    }

    /// <summary>
    /// Provides a slot object for the dictionary to allow setting of the dictionary.
    /// </summary>
    public sealed class DictWrapper : IDataDescriptor {
        PythonType type;

        public DictWrapper(PythonType pt) {
            type = pt;
        }

        #region IDataDescriptor Members

        public bool SetAttribute(object instance, object value) {
            ISuperDynamicObject sdo = instance as ISuperDynamicObject;
            if (sdo != null) {
                return sdo.SetDict((IAttributesDictionary)value);
            }

            return false;
        }

        public bool DeleteAttribute(object instance) {
            ISuperDynamicObject sdo = instance as ISuperDynamicObject;
            if (sdo != null) {
                return sdo.SetDict(null);
            }

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
    }
    /// <summary>
    /// Method wrappers provide quick access to commonly used methods that
    /// short-circuit walking the entire inheritance chain.  When a method wrapper
    /// is added to a type we calculate it's underlying value.  When the value (or
    /// base classes) changes we update the value.  That way we can always quickly
    /// get the correct value.
    /// </summary>
    public sealed class MethodWrapper : ICallable, IFancyCallable, IDataDescriptor {
        FieldInfo myField;
        private PythonType pythonType;
        internal SymbolId name;

        private bool isObjectMethod, isBuiltinMethod, isSuperTypeMethod;
        private object func = null;
        private BuiltinFunction funcAsFunc = null;

        public static MethodWrapper Make(PythonType pt, SymbolId name) {
            MethodWrapper ret = new MethodWrapper(pt, name);
            object meth;            
            if (pt.dict.TryGetValue(name, out meth)) {
                object otherMeth;
                if (!pt.TryLookupSlotInBases(DefaultContext.Default, name, out otherMeth) || otherMeth != meth) {
                    ret.SetDeclaredMethod(meth);
                } else {
                    // they did __foo__ = myBase.__foo__, we'll just ignore it...
                    ret.UpdateFromBases(pt.MethodResolutionOrder);
                }
            } else {
                ret.UpdateFromBases(pt.MethodResolutionOrder);
            }

            //pt.dict[name] = ret; //???

            return ret;
        }

        public static MethodWrapper MakeUndefined(PythonType pt, SymbolId name) {
            return new MethodWrapper(pt, name);
        }

        public static MethodWrapper MakeForObject(PythonType pt, SymbolId name, Delegate func) {
            MethodWrapper ret = new MethodWrapper(pt, name);
            ret.isObjectMethod = true;
            ret.isBuiltinMethod = true;
            ret.isSuperTypeMethod = false;

            ret.func = BuiltinFunction.Make((string)SymbolTable.IdToString(name), func, new MethodBase[] { func.Method });
            ret.funcAsFunc = ret.func as BuiltinFunction;

            //pt.dict[name] = ret;

            return ret;
        }

        //public static MethodWrapper MakeDefault() { return new MethodWrapper(null, true, true); }

        public MethodWrapper(PythonType pt, SymbolId name) {
            this.pythonType = pt;
            this.name = name;
            //!!! need to remove IdToField call here...
            string fieldname = SymbolTable.IdToString(name) + "F";
            this.myField = typeof(PythonType).GetField(fieldname);
            this.isObjectMethod = true;
            this.isBuiltinMethod = true;
            this.isSuperTypeMethod = true;
        }

        public override string ToString() {
            return String.Format("MethodWrapper for {0}.{1} => {2}>", pythonType, name, func);
        }

        public void SetDeclaredMethod(object m) {
            this.func = m;
            this.funcAsFunc = m as BuiltinFunction;
            this.isObjectMethod = pythonType.type == typeof(object);
            this.isBuiltinMethod = pythonType is ReflectedType;
            this.isSuperTypeMethod = false;

            //!!! the dictionary should be bound to this in a more sophisticated way
            pythonType.dict[this.name] = m;
        }

        public void UpdateFromBases(Tuple mro) {
            if (!isSuperTypeMethod) return;

            Debug.Assert(mro.Count > 0);
            MethodWrapper current = (MethodWrapper)myField.GetValue(mro[0]);

            for(int i = 1; i<mro.Count; i++){
                if (current != null && !current.isSuperTypeMethod) {
                    break;
                }

                object baseTypeObj = mro[i];

                PythonType baseType = baseTypeObj as PythonType;
                if (baseType == null) {
                    System.Diagnostics.Debug.Assert(baseTypeObj is DynamicType);
                    continue;
                }
                baseType.Initialize();
                current = (MethodWrapper)myField.GetValue(baseType);
            }

            if(current != null) UpdateFromBase(current);
        }

        private void UpdateFromBase(MethodWrapper mw) {
            func = mw.func;
            funcAsFunc = mw.func as BuiltinFunction;
            isObjectMethod = mw.isObjectMethod;
            isBuiltinMethod = mw.isBuiltinMethod;
            isSuperTypeMethod = true;
        }

        public object Invoke(object self) {
            if (func == null) throw Ops.AttributeError("{0} not defined on instance of {1}", name, pythonType.__name__);
            if (funcAsFunc != null) return funcAsFunc.Call(self);
            return Ops.Call(Ops.GetDescriptor(func, self, pythonType));
        }
        public object Invoke(object self, object arg1) {
            if (func == null) throw Ops.AttributeError("{0} not defined on instance of {1}", name, pythonType.__name__);
            if (funcAsFunc != null) return funcAsFunc.Call(self, arg1);
            return Ops.Call(Ops.GetDescriptor(func, self, pythonType), arg1);
        }
        public object Invoke(object self, object arg1, object arg2) {
            if (func == null) throw Ops.AttributeError("{0} not defined on instance of {1}", name, pythonType.__name__);
            if (funcAsFunc != null) return funcAsFunc.Call(self, arg1, arg2);
            return Ops.Call(Ops.GetDescriptor(func, self, pythonType), arg1, arg2);
        }
        public object Invoke(object self, object arg1, object arg2, object arg3) {
            if (func == null) throw Ops.AttributeError("{0} not defined on instance of {1}", name, pythonType.__name__);
            if (funcAsFunc != null) return funcAsFunc.Call(self, arg1, arg2, arg3);
            return Ops.Call(Ops.GetDescriptor(func, self, pythonType), arg1, arg2, arg3);
        }

        public bool IsObjectMethod() {
            return isObjectMethod;
        }

        /// <summary>
        /// Called from generated code
        /// </summary>
        public bool IsBuiltinMethod() {
            return isBuiltinMethod;
        }

        public bool IsSuperTypeMethod() {
            return isSuperTypeMethod;
        }


        #region ICallable Members

        [PythonName("__call__")]
        public object Call(params object[] args) {
            if (func == null)
                throw Ops.AttributeErrorForMissingAttribute(pythonType.__name__.ToString(), name);
            return Ops.Call(func, args);
        }

        #endregion

        #region IFancyCallable Members

        [PythonName("__call__")]
        public object Call(ICallerContext context, object[] args, string[] names) {
            if (func == null) throw Ops.AttributeError("{0} not defined on instance of {1}", name, pythonType.__name__);
            return Ops.Call(context, func, args, names);
        }

        #endregion

        #region IDataDescriptor Members

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            if (func == null) 
                throw Ops.AttributeErrorForMissingAttribute(pythonType.__name__.ToString(), name);
            if (instance != null) return new Method(func, instance, owner);
            else return func;
        }

        [PythonName("__set__")]
        public bool SetAttribute(object instance, object value) {
            SetDeclaredMethod(value);
            pythonType.UpdateSubclasses();
            return true;
        }
        [PythonName("__delete__")]
        public bool DeleteAttribute(object instance) {
            if (isSuperTypeMethod) {
                throw new NotImplementedException();
            }

            func = null;
            funcAsFunc = null;
            isSuperTypeMethod = true;
            UpdateFromBases(pythonType.MethodResolutionOrder);
            pythonType.UpdateSubclasses();
            return true;
        }

        #endregion        
    }

}
