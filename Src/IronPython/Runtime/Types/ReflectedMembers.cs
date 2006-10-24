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
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Compiler;
using IronPython.Modules;
using IronMath;

namespace IronPython.Runtime.Types {
    [PythonType("field#")]
    public class ReflectedField : IDataDescriptor, IContextAwareMember {
        public readonly FieldInfo info;
        private NameType nameType;

        public ReflectedField(FieldInfo info, NameType nameType) {
            this.nameType = nameType;
            this.info = info;
        }

        public ReflectedField(FieldInfo info)
            : this(info, NameType.PythonField) {
        }

        [PythonName("__get__")]
        public virtual object GetAttribute(object instance, object context) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance == null) {
                if (info.IsStatic) return info.GetValue(null);
                else return this;
            } else {
                return info.GetValue(Converter.Convert(instance, info.DeclaringType));
            }
        }

        private void DoSet(object instance, object val) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance != null && instance.GetType().IsValueType)
                throw Ops.ValueError("Attempt to update field '{0}' on value type '{1}'; value type fields cannot be directly modified", info.Name, info.DeclaringType.Name);
            if (info.IsInitOnly)
                throw Ops.AttributeErrorForReadonlyAttribute(info.DeclaringType.Name, SymbolTable.StringToId(info.Name));

            info.SetValue(instance, Ops.ConvertTo(val, info.FieldType)); //val.toObject(info.FieldType, "set "));
        }

        [PythonName("__set__")]
        public virtual bool SetAttribute(object instance, object value) {
            if (instance == null) {
                if (info.IsStatic) {
                    DoSet(null, value);
                } else {
                    return false;
                }
            } else {
                DoSet(instance, value); //.toObject(info.DeclaringType, "set "), val);
            }
            return true;
        }

        [PythonName("__delete__")]
        public bool DeleteAttribute(object instance) {
            throw Ops.AttributeErrorForBuiltinAttributeDeletion(info.DeclaringType.Name, SymbolTable.StringToId(info.Name));
        }

        [PythonName("__str__")]
        public override string ToString() {
            return string.Format("<field# {0} on {1}>", info.Name, info.DeclaringType.Name);
        }

        public string Documentation {
            [PythonName("__doc__")]
            get {
                return ReflectionUtil.CreateAutoDoc(info);
            }
        }

        public string Name {
            get {
                return info.Name;
            }
        }

        #region IContextAwareMember Members

        public bool IsVisible(ICallerContext context) {
            return nameType == NameType.PythonField || (context.ContextFlags & CallerContextAttributes.ShowCls) != 0;
        }

        #endregion
    }

    /// <summary>
    /// Used to create overridden fields from a base type to an ops reflected type.  
    /// 
    /// BaseType is the type we're providing overrides for, ExtensibleType is the type that
    /// is used to extend this.  ExtensibleType must implemnt IExtensible of BaseType to
    /// provide access to the base type value.  This value will be updated if the user
    /// sets the value, or wants to get a field from the value.
    /// </summary>
    [PythonType("field#")]
    class OpsReflectedField<BaseType, ExtensibleType> : ReflectedField
        where ExtensibleType : IExtensible<BaseType> {

        public OpsReflectedField(FieldInfo info, NameType nameType)
            : base(info, nameType) {
        }

        public override object GetAttribute(object instance, object context) {
            if (instance is BaseType)
                return base.GetAttribute(instance, context);
            if (instance is ExtensibleType)
                return base.GetAttribute(((ExtensibleType)instance).Value, context);

            throw Ops.TypeErrorForTypeMismatch(Ops.GetDynamicTypeFromType(typeof(BaseType)).__name__.ToString(), instance);
        }

        public override bool SetAttribute(object instance, object value) {
            if (instance is BaseType)
                return base.SetAttribute(instance, value);
            if (instance is ExtensibleType)
                return base.SetAttribute(((ExtensibleType)instance).Value, value);
            if (instance == null)
                return false;

            throw Ops.TypeErrorForTypeMismatch(Ops.GetDynamicTypeFromType(typeof(BaseType)).__name__.ToString(), instance);
        }
    }

    /// <summary>
    /// Base class for properties backed by methods.  These include our slot properties,
    /// indexers, and normal properties.  This class provides the storage of these as well
    /// as the storage of our optimized getter/setter methods, documentation for the property,
    /// etc...
    /// </summary>
    public class ReflectedGetterSetter : IContextAwareMember {
        private readonly MethodInfo getter, setter;
        private readonly PropertyInfo info;
        private FastCallable fcGetter, fcSetter;
        private NameType nameType;

        public ReflectedGetterSetter(PropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt) {
            this.info = info;
            this.getter = getter;
            this.setter = setter;
            this.nameType = nt;
        }

        protected ReflectedGetterSetter(ReflectedGetterSetter from) {
            this.info = from.info;
            this.getter = from.getter;
            this.setter = from.setter;
            from.CreateGetter();
            from.CreateSetter();
            this.fcGetter = from.fcGetter;
            this.fcSetter = from.fcSetter;
            this.nameType = from.nameType;
        }

        private void CreateGetter() {
            if (fcGetter != null) return;

            if (getter != null) this.fcGetter = MethodBinder.MakeFastCallable(getter.Name, getter, false);
        }

        private void CreateSetter() {
            if (fcSetter != null) return;

            if (setter != null) this.fcSetter = MethodBinder.MakeFastCallable(setter.Name, setter, false);
        }

        internal string Name {
            get {
                object[] pnas = null;
                if (getter != null) {
                    pnas = getter.GetCustomAttributes(typeof(PythonNameAttribute), true);
                } else if (setter != null) {
                    pnas = setter.GetCustomAttributes(typeof(PythonNameAttribute), true);
                }

                if (pnas != null && pnas.Length > 0)
                    return ((PythonNameAttribute)pnas[0]).name;

                return info.Name;
            }
        }

        protected MethodInfo Getter {
            get {
                return getter;
            }
        }

        protected MethodInfo Setter {
            get {
                return setter;
            }
        }

        protected PropertyInfo Info {
            get {
                return info;
            }
        }

        public string Documentation {
            [PythonName("__doc__")]
            get {
                return ReflectionUtil.DocOneInfo(Info);
            }
        }

        protected object CallGetter(object instance, object[] args) {
            if (instance == null && (Getter == null || !Getter.IsStatic)) return this;

            CreateGetter();

            if (instance != null)
                return fcGetter.CallInstance(DefaultContext.Default, instance, args);

            return fcGetter.Call(args);
        }

        protected bool CallSetter(object instance, object[] args, object value) {
            if (instance == null && (Setter == null || !Setter.IsStatic)) return false;

            CreateSetter();

            if (args.Length == 0) {
                if (instance != null) fcSetter.CallInstance(DefaultContext.Default, instance, value);
                else fcSetter.Call(value);
            } else {
                object[] nargs = new object[args.Length + 1];
                Array.Copy(args, 0, nargs, 0, args.Length);
                nargs[args.Length] = value;

                if (instance != null) fcSetter.CallInstance(DefaultContext.Default, instance, nargs);
                else fcSetter.Call(nargs);
            }

            return true;
        }

        #region IContextAwareMember Members

        public bool IsVisible(ICallerContext context) {
            return nameType == NameType.PythonProperty || (context.ContextFlags & CallerContextAttributes.ShowCls) != 0;
        }

        #endregion
    }

    /// <summary>
    /// Just like a reflected property, but we also allow deleting of values (setting them to
    /// Uninitialized.instance)
    /// </summary>
    [PythonType("member_descriptor")]
    public class ReflectedSlotProperty : ReflectedProperty, ICodeFormattable {

        public ReflectedSlotProperty(PropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt)
            : base(info, getter, setter, nt) {
        }

        [PythonName("__delete__")]
        public override bool DeleteAttribute(object instance) {
            if (instance != null) {
                SetAttribute(instance, Uninitialized.instance);
                return true;
            }
            return false;
        }

        public override string ToString() {
            return String.Format("<member '{0}'>", Info.Name); // <member '{0}' of '{1}' objects> - but we don't know our type name
        }

        #region ICodeFormattable Members

        public string ToCodeString() {
            return ToString();
        }

        #endregion
    }

    /// <summary>
    /// Provides access to non-default .NET indexers (aka properties w/ parameters).
    /// 
    /// C# doesn't support these, but both COM and VB.NET do.  The types dictionary
    /// gets populated w/a ReflectedGetterSetter indexer which is a descriptor.  Getting
    /// the descriptor returns a bound indexer.  The bound indexer is an IMapping which
    /// supports accessing the index value.  We support multiple indexer parameters
    /// via expandable tuples.
    /// </summary>
    [PythonType("indexer#")]
    public sealed class ReflectedIndexer : ReflectedGetterSetter, IDescriptor, IMapping {
        private object instance;

        public ReflectedIndexer(PropertyInfo info, NameType nt)
            : base(info, info.GetGetMethod(), info.GetSetMethod(), nt) {
        }

        public ReflectedIndexer(ReflectedIndexer from, object instance)
            : base(from) {
            this.instance = instance;
        }

        #region IMapping Members

        public object GetValue(object key) {
            return this[key];
        }

        public object GetValue(object key, object defaultValue) {
            object res;
            if (TryGetValue(key, out res)) return res;

            return defaultValue;
        }


        public bool TryGetValue(object key, out object value) {
            if (Getter == null)
                throw Ops.AttributeError("attribute '{0}' of '{1}' object is write-only",
                     Name,
                     Ops.GetDynamicTypeFromType(Info.DeclaringType).Name);

            try {
                value = this[key];
                return true;
            } catch (MissingMemberException) {
                value = null;
                return false;
            }
        }

        public object this[object key] {
            [PythonName("__getitem__")]
            get {
                Tuple tupleKey = key as Tuple;
                if (tupleKey != null && tupleKey.IsExpandable) {
                    return CallGetter(instance, tupleKey.Expand(null));
                }

                return CallGetter(instance, new object[] { key });
            }
            [PythonName("__setitem__")]
            set {
                Tuple tupleKey = key as Tuple;
                if (tupleKey != null && tupleKey.IsExpandable) {
                    if (!CallSetter(instance, tupleKey.Expand(null), value)) {
                        throw new Exception("The method or operation is not implemented.");
                    }
                    return;
                }

                if (!CallSetter(instance, new object[] { key }, value)) {
                    throw new Exception("The method or operation is not implemented.");
                }
            }
        }

        [PythonName("__delitem__")]
        public void DeleteItem(object key) {
            if (Setter != null)
                throw Ops.AttributeErrorForReadonlyAttribute(
                    Ops.GetDynamicTypeFromType(Info.DeclaringType).Name,
                    SymbolTable.StringToId(Name));
            else
                throw Ops.AttributeErrorForBuiltinAttributeDeletion(
                    Ops.GetDynamicTypeFromType(Info.DeclaringType).Name,
                    SymbolTable.StringToId(Name));
        }

        #endregion

        #region IPythonContainer Members

        int IPythonContainer.GetLength() {
            return 1;
        }

        bool IPythonContainer.ContainsValue(object value) {
            object dummy;
            return TryGetValue(value, out dummy);
        }

        #endregion

        #region IDescriptor Members

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            return new ReflectedIndexer(this, instance);
        }

        #endregion

        public override string ToString() {
            return string.Format("<indexer# {0} on {1}>", Name,
                Ops.GetDynamicTypeFromType(Info.DeclaringType).Name);
        }
    }


    [PythonType("property#")]
    public class ReflectedProperty : ReflectedGetterSetter, IDataDescriptor {
        public ReflectedProperty(PropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt)
            : base(info, getter, setter, nt) {
        }

        public object GetValue(object instance) {
            return GetAttribute(instance, null);
        }

        public void SetValue(object instance, object value) {
            SetAttribute(instance, value);
        }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object context) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Properties, this);
            if (Getter == null)
                throw Ops.AttributeError("attribute '{0}' of '{1}' object is write-only",
                    Name,
                    Ops.GetDynamicTypeFromType(Info.DeclaringType).Name);

            return CallGetter(instance, Ops.EMPTY);
        }


        [PythonName("__set__")]
        public bool SetAttribute(object instance, object value) {
            return CallSetter(instance, Ops.EMPTY, value);
        }

        [PythonName("__delete__")]
        public virtual bool DeleteAttribute(object instance) {
            if (Setter != null)
                throw Ops.AttributeErrorForReadonlyAttribute(
                    Ops.GetDynamicTypeFromType(Info.DeclaringType).Name,
                    SymbolTable.StringToId(Name));
            else
                throw Ops.AttributeErrorForBuiltinAttributeDeletion(
                    Ops.GetDynamicTypeFromType(Info.DeclaringType).Name,
                    SymbolTable.StringToId(Name));
        }

        [PythonName("__str__")]
        public override string ToString() {
            return string.Format("<property# {0} on {1}>", Name,
                Ops.GetDynamicTypeFromType(Info.DeclaringType).Name);
        }

    }

    [PythonType("event#")]
    public class ReflectedEvent : IDataDescriptor, IContextAwareMember {
        public readonly object instance;
        bool clsOnly;
        EventDispatcher dispatcher;
        WeakHash<object, EventDispatcher> dispatchers;
        static object staticTarget = new object();

        public ReflectedEvent(EventInfo info, object instance, bool clsOnly) {
            this.instance = instance;
            this.clsOnly = clsOnly;
            dispatcher = new EventDispatcher(info);
        }

        private ReflectedEvent(EventDispatcher dispatch, WeakHash<object, EventDispatcher> dispatchers, object instance, bool clsOnly) {
            this.dispatcher = dispatch;
            this.instance = instance;
            this.clsOnly = clsOnly;
            this.dispatchers = dispatchers;
        }

        [PythonName("__iadd__")]
        public object __iadd__(object func) {
            GetDispatcher(instance).AddEvent(instance, func);
            return this;
        }

        [PythonName("__isub__")]
        public object __isub__(object func) {
            GetDispatcher(instance).RemoveEvent(instance, func);
            return this;
        }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object context) {
            if (instance != null) {
                EventDispatcher ed = GetDispatcher(instance);
                ReflectedEvent bound = new ReflectedEvent(ed, dispatchers, instance, clsOnly);
                return bound;
            } else {
                return this;
            }
        }

        [PythonName("__set__")]
        public bool SetAttribute(object instance, object value) {
            ReflectedEvent other = value as ReflectedEvent;
            if (other == null || other.dispatcher.Info != this.dispatcher.Info)
                throw Ops.AttributeErrorForReadonlyAttribute(dispatcher.Info.DeclaringType.Name, SymbolTable.StringToId(dispatcher.Info.Name));
            return true;
        }

        [PythonName("__delete__")]
        public bool DeleteAttribute(object instance) {
            throw Ops.AttributeErrorForReadonlyAttribute(dispatcher.Info.DeclaringType.Name, SymbolTable.StringToId(dispatcher.Info.Name));
        }

        [PythonName("__str__")]
        public override string ToString() {
            return string.Format("<event# {0} on {1}>", dispatcher.Info.Name, dispatcher.Info.DeclaringType.Name);
        }

        public string Name {
            get {
                return dispatcher.Info.Name;
            }
        }

        public string Documentation {
            [PythonName("__doc__")]
            get {
                return ReflectionUtil.CreateAutoDoc(dispatcher.Info);
            }
        }
        #region IContextAwareMember Members

        bool IContextAwareMember.IsVisible(ICallerContext context) {
            // events aren't visible w/o importing clr.
            return !clsOnly || (context.ContextFlags & CallerContextAttributes.ShowCls) != 0;
        }

        #endregion

        private EventDispatcher GetDispatcher(object instance) {
            if (dispatchers == null) {
                dispatchers = new WeakHash<object, EventDispatcher>();
            }

            if (instance == null) {
                // targetting a static method, we'll use a random object
                // as our place holder here...
                instance = staticTarget;
            }

            EventDispatcher res;
            if (dispatchers.TryGetValue(instance, out res)) {
                return res;
            }

            return dispatchers[instance] = new EventDispatcher(dispatcher.Info);
        }

        class EventDispatcher : ICallable {
            List<object> events = new List<object>();
            EventInfo info;

            public EventDispatcher(EventInfo eventInfo) {
                info = eventInfo;
            }

            #region ICallable Members

            public object Call(params object[] args) {
                object res = null;
                for (int i = 0; i < events.Count; i++) {
                    Delegate d = events[i] as Delegate;
                    if (d != null) {
                        // user did += SomeDelegateType(SomeFunction), just invoke the 
                        // delegate.
                        res = d.DynamicInvoke(args);
                    } else {
                        res = Ops.Call(events[i], args);
                    }
                }
                return res;
            }

            #endregion

            public void AddEvent(object instance, object func) {
                if (events.Count == 0) {
                    Hook(instance);
                }

                events.Add(func);
            }

            public void RemoveEvent(object instance, object func) {
                for (int i = 0; i < events.Count; i++) {
                    if (Ops.EqualRetBool(events[i], func)) {
                        events.RemoveAt(i);
                        break;
                    }
                }

                if (events.Count == 0) {
                    Unhook(instance);
                }
            }

            private void Hook(object instance) {
                Delegate handler = Ops.GetDelegate(this, info.EventHandlerType);
                try {
                    info.AddEventHandler(instance, handler);
                } catch (TargetInvocationException tie) {
                    throw ExceptionConverter.UpdateForRethrow(tie.InnerException);
                }
            }

            private void Unhook(object instance) {
                Delegate handler = Ops.GetDelegate(this, info.EventHandlerType);
                try {
                    info.RemoveEventHandler(instance, handler);
                } catch (TargetInvocationException tie) {
                    throw ExceptionConverter.UpdateForRethrow(tie.InnerException);
                }
            }

            public EventInfo Info {
                get {
                    return info;
                }
            }
        }
    }

    [Flags]
    public enum NameType {
        None = 0x0000,
        Python = 0x0001,

        Method = 0x0002,
        Field = 0x0004,
        Property = 0x0008,
        Event = 0x0010,
        Type = 0x0020,

        PythonMethod = Method | Python,
        PythonField = Field | Python,
        PythonProperty = Property | Python,
        PythonEvent = Event | Python,
        PythonType = Type | Python,

        ClassMember = 0x0040,
        ClassMethod = ClassMember | PythonMethod,
    }



}
