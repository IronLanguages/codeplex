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
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Types;

// TODO: Remove remaining dependencies
using Ops = IronPython.Runtime.Operations.Ops;
using MethodBinder = IronPython.Compiler.MethodBinder;
using PythonBinder = IronPython.Runtime.Calls.PythonBinder;
using BinderType = IronPython.Compiler.BinderType;

namespace Microsoft.Scripting {
    public class ReflectedField : DynamicTypeSlot, IContextAwareMember {
        public readonly FieldInfo info;
        private NameType nameType;

        public ReflectedField(FieldInfo info, NameType nameType) {
            this.nameType = nameType;
            this.info = info;
        }

        public ReflectedField(FieldInfo info)
            : this(info, NameType.PythonField) {
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance == null) {
                if (info.IsStatic) { 
                    value = info.GetValue(null); 
                } else { 
                    value = this; 
                }
            } else {
                value = info.GetValue(context.LanguageContext.Binder.Convert(instance, info.DeclaringType));
            }

            return true;
        }
        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            if (instance == null && info.IsStatic && info.DeclaringType == ((DynamicType)owner).UnderlyingSystemType) {
                DoSet(context, null, value);
            } else if (!info.IsStatic || info.DeclaringType == ((DynamicType)owner).UnderlyingSystemType) {
                DoSet(context, instance, value);
            } else {
                return false;
            }
            return true;
        }

        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
            if (info.IsStatic && info.DeclaringType != ((DynamicType)owner).UnderlyingSystemType)
                return false;

            throw Ops.AttributeErrorForBuiltinAttributeDeletion(info.DeclaringType.Name, SymbolTable.StringToId(info.Name));
        }

        private void DoSet(CodeContext context, object instance, object val) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance != null && instance.GetType().IsValueType)
                throw Ops.ValueError("Attempt to update field '{0}' on value type '{1}'; value type fields cannot be directly modified", info.Name, info.DeclaringType.Name);
            if (info.IsInitOnly)
                throw Ops.AttributeErrorForReadonlyAttribute(info.DeclaringType.Name, SymbolTable.StringToId(info.Name));

            info.SetValue(instance, context.LanguageContext.Binder.Convert(val, info.FieldType)); 
        }

        #region IContextAwareMember Members

        public override bool IsVisible(CodeContext context, DynamicMixin owner) {
            return nameType == NameType.PythonField || context.LanguageContext.ShowCls;
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
    class OpsReflectedProperty<BaseType, ExtensibleType> : ReflectedProperty
        where ExtensibleType : Extensible<BaseType> {

        public OpsReflectedProperty(PropertyInfo info, NameType nameType)
            : base(info, info.GetGetMethod(), info.GetSetMethod(), nameType) {
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            if (instance is BaseType)
                return base.TryGetValue(context, instance, owner, out value);
            if (instance is ExtensibleType)
                return base.TryGetValue(context, ((ExtensibleType)instance).Value, owner, out value);
            if (instance == null) {
                value = this;
                return true; 
            }

            throw Ops.TypeErrorForTypeMismatch(Ops.GetDynamicTypeFromType(typeof(BaseType)).Name.ToString(), instance);
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            if (instance is BaseType)
                return base.TrySetValue(context, instance, owner, value);
            if (instance is ExtensibleType)
                return base.TrySetValue(context, ((ExtensibleType)instance).Value, owner, value);
            if (instance == null)
                return false;

            throw Ops.TypeErrorForTypeMismatch(Ops.GetDynamicTypeFromType(typeof(BaseType)).Name.ToString(), instance);
        }
    }

    /// <summary>
    /// Base class for properties backed by methods.  These include our slot properties,
    /// indexers, and normal properties.  This class provides the storage of these as well
    /// as the storage of our optimized getter/setter methods, documentation for the property,
    /// etc...
    /// </summary>
    public abstract class ReflectedGetterSetter : DynamicTypeSlot, IContextAwareMember {
        private readonly MethodInfo getter, setter;
        private readonly PropertyInfo _info;
        private FastCallable fcGetter, fcSetter;
        private NameType nameType;

        public ReflectedGetterSetter(PropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt) {
            this._info = info;
            this.getter = getter;
            this.setter = setter;
            this.nameType = nt;
        }

        protected ReflectedGetterSetter(ReflectedGetterSetter from) {
            this._info = from._info;
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

            if (getter != null) this.fcGetter = MethodBinder.MakeFastCallable(PythonBinder.Default, getter.Name, getter, BinderType.Normal);
        }

        private void CreateSetter() {
            if (fcSetter != null) return;

            if (setter != null) this.fcSetter = MethodBinder.MakeFastCallable(PythonBinder.Default, setter.Name, setter, BinderType.Normal);
        }

        internal virtual string Name {
            get {
                if (_info != null) {
                    return _info.Name;
                }

                return null;
            }
        }

        internal MethodInfo Getter {
            get {
                return getter;
            }
        }

        internal MethodInfo Setter {
            get {
                return setter;
            }
        }

        public PropertyInfo Info {
            get {
                return _info;
            }
        }

        public virtual Type DeclaringType {
            get {
                return _info.DeclaringType;
            }
        }

        protected NameType NameType {
            get {
                return nameType;
            }
        }

        protected object CallGetter(CodeContext context, object instance, object[] args) {
            if (instance == null && (Getter == null || !Getter.IsStatic)) return this;

            CreateGetter();

            if (instance != null)
                return fcGetter.CallInstance(context, instance, args);

            return fcGetter.Call(context, args);
        }

        protected bool CallSetter(CodeContext context, object instance, object[] args, object value) {
            if (instance == null && (Setter == null || !Setter.IsStatic)) return false;

            CreateSetter();

            if (args.Length == 0) {
                if (instance != null) fcSetter.CallInstance(context, instance, value);
                else fcSetter.Call(context, value);
            } else {
                object[] nargs = new object[args.Length + 1];
                Array.Copy(args, 0, nargs, 0, args.Length);
                nargs[args.Length] = value;

                if (instance != null) fcSetter.CallInstance(context, instance, nargs);
                else fcSetter.Call(context, nargs);
            }

            return true;
        }

        #region IContextAwareMember Members

        public override bool IsVisible(CodeContext context, DynamicMixin owner) {
            return nameType == NameType.PythonProperty || context.LanguageContext.ShowCls;
        }

        #endregion
    }

    /// <summary>
    /// Provides access to non-default .NET indexers (aka properties w/ parameters).
    /// 
    /// C# doesn't support these, but both COM and VB.NET do.  The types dictionary
    /// gets populated w/a ReflectedGetterSetter indexer which is a descriptor.  Getting
    /// the descriptor returns a bound indexer.  The bound indexer supports indexing.
    /// We support multiple indexer parameters via expandable tuples.
    /// </summary>
    public sealed class ReflectedIndexer : ReflectedGetterSetter {
        private object _instance;

        public ReflectedIndexer(PropertyInfo info, NameType nt)
            : base(info, info.GetGetMethod(), info.GetSetMethod(), nt) {
        }

        public ReflectedIndexer(ReflectedIndexer from, object instance)
            : base(from) {
            this._instance = instance;
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = new ReflectedIndexer(this, instance);
            return true;
        }

        // TODO: params object[] keys ? and get rid of IParameterSequence?
        [OperatorMethod]
        public object GetItem(CodeContext context, object key) {
            IParameterSequence tupleKey = key as IParameterSequence;
            if (tupleKey != null && tupleKey.IsExpandable) {
                return CallGetter(context, _instance, tupleKey.Expand(null));
            }

            return CallGetter(context, _instance, new object[] { key });
        }

        [OperatorMethod]
        public void SetItem(CodeContext context, object key, object value) {
            IParameterSequence tupleKey = key as IParameterSequence;
            if (tupleKey != null && tupleKey.IsExpandable) {
                if (!CallSetter(context, _instance, tupleKey.Expand(null), value)) {
                    throw new Exception("The method or operation is not implemented.");
                }
                return;
            }

            if (!CallSetter(context, _instance, new object[] { key }, value)) {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        [OperatorMethod]
        public void DeleteItem(object key) {
            if (Setter != null)
                throw Ops.AttributeErrorForReadonlyAttribute(
                    DynamicHelpers.GetDynamicTypeFromType(DeclaringType).Name,
                    SymbolTable.StringToId(Name));
            else
                throw Ops.AttributeErrorForBuiltinAttributeDeletion(
                    DynamicHelpers.GetDynamicTypeFromType(DeclaringType).Name,
                    SymbolTable.StringToId(Name));
        }
    }

    public class ReflectedProperty : ReflectedGetterSetter {
        public ReflectedProperty(PropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt)
            : base(info, getter, setter, nt) {
        }

        /// <summary>
        /// Convenience function for users to call directly
        /// </summary>
        public object GetValue(CodeContext context, object instance) {
            object value;
            if (TryGetValue(context, instance, DynamicHelpers.GetDynamicType(instance), out value)) {
                return value;
            }
            return null;
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            if (Setter == null) return false;

            if (instance == null) {
                if (Setter.IsStatic && DeclaringType != ((DynamicType)owner).UnderlyingSystemType)
                    return false;
            } else if (instance != null) {
                if (Setter.IsStatic)
                    return false;
            }

            return CallSetter(context, instance, Ops.EmptyObjectArray, value);
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Properties, this);
            
            value = CallGetter(context, instance, Ops.EmptyObjectArray);
            return true;
        }

        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
            if (Setter != null)
                throw Ops.AttributeErrorForReadonlyAttribute(
                    DynamicHelpers.GetDynamicTypeFromType(DeclaringType).Name,
                    SymbolTable.StringToId(Name));
            else
                throw Ops.AttributeErrorForBuiltinAttributeDeletion(
                    DynamicHelpers.GetDynamicTypeFromType(DeclaringType).Name,
                    SymbolTable.StringToId(Name));
        }

        public sealed override bool IsVisible(CodeContext context, DynamicMixin owner) {
            if (context.LanguageContext.ShowCls)
                return true;

            return NameType == NameType.PythonProperty;
        }

    }

    /// <summary>
    /// Represents a ReflectedProperty created for an extension method.  Logically the property is an
    /// instance property but the method implementing it is static.
    /// </summary>
    public class ReflectedExtensionProperty : ReflectedProperty {
        private MethodInfo _deleter;
        private FastCallable _fcDeleter;
        private ExtensionPropertyInfo _extInfo;

        public ReflectedExtensionProperty(ExtensionPropertyInfo info, NameType nt)
            : this(info, info.Getter, info.Setter, nt) {
        }

        public ReflectedExtensionProperty(ExtensionPropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt)
            : base(null, getter, setter, nt) {
            Debug.Assert(Getter == null || Getter.IsStatic);
            Debug.Assert(Setter == null || Setter.IsStatic);

            _extInfo = info;
            _deleter = info.Deleter;
        }


        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            if (Getter == null || instance == null) {
                value = null;
                return false;
            }

            return base.TryGetValue(context, instance, owner, out value);
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            if (Setter == null || instance == null) return false;

            return CallSetter(context, instance, Ops.EmptyObjectArray, value);
        }

        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
            if (_deleter == null || instance == null) {
                return base.TryDeleteValue(context, instance, owner);
            }

            CreateDeleter();
            _fcDeleter.Call(context, instance);
            return true;
        }

        private void CreateDeleter() {
            if (_fcDeleter != null) return;

            _fcDeleter = MethodBinder.MakeFastCallable(PythonBinder.Default, _deleter.Name, _deleter, BinderType.Normal);
        }

        public override Type DeclaringType {
            get {
                return _extInfo.DeclaringType;
            }
        }

        public ExtensionPropertyInfo ExtInfo {
            get {
                return _extInfo;
            }
        }

        internal override string Name {
            get {
                return base.Name ?? _extInfo.Name;                
            }
        }

    }

    /// <summary>
    /// The unbound representation of an event property
    /// </summary>
    public class ReflectedEvent : DynamicTypeSlot, IContextAwareMember {
        private bool _clsOnly;
        private EventInfo _eventInfo;
        private WeakHash<object, EventDispatcher> _dispatchers;
        private static object _staticTarget = new object();

        public ReflectedEvent(EventInfo eventInfo, bool clsOnly) {
            this._clsOnly = clsOnly;
            _eventInfo = eventInfo;
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = new BoundEvent(this, instance, (DynamicType)owner);            
            return true;
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            BoundEvent et = value as BoundEvent;

            if (et == null || et.Event.Info != this.Info) {
                BadEventChange bea = value as BadEventChange;

                if (bea != null) {
                    DynamicType dt = bea.Owner as DynamicType;
                    if (dt != null) {
                        if (bea.Instance == null) {
                            throw Ops.AttributeErrorForReadonlyAttribute(dt.Name, SymbolTable.StringToId(_eventInfo.Name));
                        } else {
                            throw Ops.AttributeErrorForMissingAttribute(dt.Name, SymbolTable.StringToId(_eventInfo.Name));
                        }
                    }
                }

                throw Ops.AttributeErrorForReadonlyAttribute(_eventInfo.DeclaringType.Name, SymbolTable.StringToId(_eventInfo.Name));
            }

            return true;
        }

        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
            throw Ops.AttributeErrorForReadonlyAttribute(_eventInfo.DeclaringType.Name, SymbolTable.StringToId(_eventInfo.Name));
        }
        
        #region IContextAwareMember Members

        public override bool IsVisible(CodeContext context, DynamicMixin owner) {
            // events aren't visible w/o importing clr.
            return !_clsOnly || context.LanguageContext.ShowCls;
        }

        #endregion

        private EventDispatcher GetDispatcher(object instance) {
            if (_dispatchers == null) {
                System.Threading.Interlocked.CompareExchange(ref _dispatchers, new WeakHash<object, EventDispatcher>(), null);
            }

            if (instance == null) {
                // targetting a static method, we'll use a random object
                // as our place holder here...
                instance = _staticTarget;
            }

            lock (_dispatchers) {
                EventDispatcher dispatcher;
                if (_dispatchers.TryGetValue(instance, out dispatcher)) {
                    return dispatcher;
                }

                dispatcher = new EventDispatcher(_eventInfo);
                _dispatchers[instance] = dispatcher;
                return dispatcher;
            }
        }

        public EventInfo Info {
            get {
                return _eventInfo;
            }
        }

        /// <summary>
        /// BoundEvent is the object that gets returned when the user gets an event object.  An
        /// BoundEvent tracks where the event was received from and is used to verify we get
        /// a proper add when dealing w/ statics events.
        /// </summary>
        public class BoundEvent {
            private ReflectedEvent _event;
            private object _instance;
            private DynamicType _ownerType;

            public BoundEvent(ReflectedEvent reflectedEvent, object instance, DynamicType ownerType) {
                _event = reflectedEvent;
                _instance = instance;
                _ownerType = ownerType;
            }

            [OperatorMethod]
            public object InPlaceAdd(object func) {
                MethodInfo add = _event.Info.GetAddMethod(true);
                if (add.IsStatic) {
                    if (_ownerType != DynamicHelpers.GetDynamicTypeFromType(_event.Info.DeclaringType)) {
                        // mutating static event, only allow this from the type we're mutating, not sub-types
                        return new BadEventChange(_ownerType, _instance);
                    }
                }

                if (_event.Info.EventHandlerType.IsAssignableFrom(func.GetType())) {
                    // We add the handler directly to the event source if possible
                    if (add.IsPublic && add.DeclaringType.IsPublic) {
                        _event.Info.AddEventHandler(_instance, (Delegate)func);
                    } else if (ScriptDomainManager.Options.PrivateBinding) {
                        add.Invoke(_instance, new object[] { func });
                    } else {
                        throw Ops.TypeError("cannot add to private event");
                    }
                } else {
                    _event.GetDispatcher(_instance).AddEvent(_instance, func);
                }
                return this;
            }

            [OperatorMethod]
            public object InPlaceSubtract(object func) {
                MethodInfo remove = _event.Info.GetRemoveMethod(true);
                if (remove.IsStatic) {
                    if (_ownerType != DynamicHelpers.GetDynamicTypeFromType(_event.Info.DeclaringType)) {
                        // mutating static event, only allow this from the type we're mutating, not sub-types
                        return new BadEventChange(_ownerType, _instance);
                    }
                }

                if (_event.Info.EventHandlerType.IsAssignableFrom(func.GetType())) {
                    if (remove.IsPublic && remove.DeclaringType.IsPublic) {
                        _event.Info.RemoveEventHandler(_instance, (Delegate)func);
                    } else if (ScriptDomainManager.Options.PrivateBinding) {
                        remove.Invoke(_instance, new object[] { func });
                    } else {
                        throw Ops.TypeError("cannot subtract from private event");
                    }
                } else {
                    _event.GetDispatcher(_instance).RemoveEvent(_instance, func);
                }
                return this;
            }

            public ReflectedEvent Event {
                get {
                    return _event;
                }
            }
        }

        private class BadEventChange {
            private DynamicType _ownerType;
            private object _instance;
            public BadEventChange(DynamicType ownerType, object instance) {
                _ownerType = ownerType;
                _instance = instance;
            }

            public DynamicType Owner {
                get {
                    return _ownerType;
                }
            }
            public object Instance {
                get {
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Responsible for dispatching multiple python event hooks from a single event invocation.
        /// 
        /// We add a dynamicly generated delegate to call the EventDispatcher when the event fires. 
        /// The event dispatcher then calls each of the Python delegates that have been hooked from Python
        /// code.
        /// 
        /// Strongly-typed delegates are directly added to the event source, and will not use EventDispatcher
        /// </summary>
        class EventDispatcher : ICallableWithCodeContext {
            // We use CopyOnWriteList so that Call does not need to allocate any memory. This is at the 
            // expense of AddEvent and RemoveEvent which will now always allocate memory
            CopyOnWriteList<object> _handlers = new CopyOnWriteList<object>();

            EventInfo _info;

            public EventDispatcher(EventInfo eventInfo) {
                _info = eventInfo;
            }

            #region ICallableWithCodeContext Members

            public object Call(CodeContext context, params object[] args) {
                // Get a copy of this.events so that it can be accessed in a thread-safe way without a lock.
                List<object> copyOfHandlers = _handlers.GetCopyForRead();

                object res = null;
                for (int i = 0; i < copyOfHandlers.Count; i++) {                    
                    res = DynamicHelpers.CallWithContext(context, copyOfHandlers[i], args);
                }
                return res;
            }

            #endregion

            public void AddEvent(object instance, object func) {
                // A strongly-typed delegate should get directly added to the event source
                Debug.Assert(!_info.EventHandlerType.IsAssignableFrom(func.GetType()));

                if (_handlers.Count == 0) {
                    lock (this) {
                        if (_handlers.Count == 0) {
                            Hook(instance);
                        }
                    }
                }

                _handlers.Add(func);
            }

            public void RemoveEvent(object instance, object func) {
                // A strongly-typed delegate should get directly added to the event source
                Debug.Assert(!_info.EventHandlerType.IsAssignableFrom(func.GetType()));

                List<object> copyOfHandlers = _handlers.GetCopyForRead();
                for (int i = 0; i < copyOfHandlers.Count; i++) {
                    if (Ops.EqualRetBool(copyOfHandlers[i], func)) {
                        _handlers.Remove(copyOfHandlers[i]);
                        break;
                    }
                }

                if (_handlers.Count == 0) {
                    lock (this) {
                        if (_handlers.Count == 0) {
                            Unhook(instance);
                        }
                    }
                }
            }

            private void Hook(object instance) {
                Delegate handler = Ops.GetDelegate(this, _info.EventHandlerType, ScriptDomainManager.CurrentManager.PAL.EventExceptionHandler);
                try {
                    MethodInfo add = _info.GetAddMethod(true);
                    if (!add.IsPrivate && add.DeclaringType.IsPublic) {
                        _info.AddEventHandler(instance, handler);
                    } else {
                        add.Invoke(instance, new object[] { handler });
                    }
                } catch (TargetInvocationException tie) {
                    throw ExceptionHelpers.UpdateForRethrow(tie.InnerException);
                }
            }

            private void Unhook(object instance) {
                Delegate handler = Ops.GetDelegate(this, _info.EventHandlerType, ScriptDomainManager.CurrentManager.PAL.EventExceptionHandler);
                try {
                    MethodInfo remove = _info.GetRemoveMethod(true);
                    if (!remove.IsPrivate && remove.DeclaringType.IsPublic) {
                        _info.RemoveEventHandler(instance, handler);
                    } else {
                        remove.Invoke(instance, new object[] { handler });
                    }
                } catch (TargetInvocationException tie) {
                    throw ExceptionHelpers.UpdateForRethrow(tie.InnerException);
                }
            }

            EventInfo Info {
                get {
                    return _info;
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
        BaseTypeMask = 0x003e,

        PythonMethod = Method | Python,
        PythonField = Field | Python,
        PythonProperty = Property | Python,
        PythonEvent = Event | Python,
        PythonType = Type | Python,

        ClassMember = 0x0040,
        ClassMethod = ClassMember | PythonMethod,
    }
}
