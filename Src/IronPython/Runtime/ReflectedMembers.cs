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


using IronPython.Compiler;
using IronPython.Modules;
using IronMath;

namespace IronPython.Runtime {  
    public class ReflectedField : IDataDescriptor, IContextAwareMember {
        public readonly FieldInfo info;
        private NameType nameType;

        public ReflectedField(FieldInfo info, NameType nameType) {
            this.nameType = nameType;
            this.info = info;
        }

        public ReflectedField(FieldInfo info) : this(info, NameType.PythonField) {
        }

        [PythonName("__get__")]
        public virtual object GetAttribute(object instance, object context) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance == null) {
                if (info.IsStatic) return Ops.ToPython(info.GetValue(null));
                else return this;
            } else {
                return Ops.ToPython(info.GetValue(Converter.Convert(instance, info.DeclaringType)));
            }
        }

        private void DoSet(object instance, object val) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance != null && instance.GetType().IsValueType)
                throw Ops.ValueError("Attempt to update field '{0}' on value type '{1}'; value type fields cannot be directly modified", info.Name, info.DeclaringType.Name);
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
            throw Ops.TypeError("can't delete field on built-in object");
        }

        [PythonName("__str__")]
        public override string ToString() {
            return string.Format("<field# {0} on {1}>", info.Name, info.DeclaringType.Name);
        }

        #region IContextAwareMember Members

        public bool IsVisible(ICallerContext context) {
            return nameType == NameType.PythonField || (context.ContextFlags & CallerContextFlags.ShowCls) != 0;
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
    class OpsReflectedField<BaseType, ExtensibleType> : ReflectedField 
        where ExtensibleType : IExtensible<BaseType> {
        
        public OpsReflectedField(FieldInfo info, NameType nameType)
            : base(info, nameType) {
        }

        public override object GetAttribute(object instance, object context) {
            if (instance is BaseType) 
                return base.GetAttribute(instance, context);
            if(instance is ExtensibleType) 
                return base.GetAttribute(((ExtensibleType)instance).Value, context);
            
            throw Ops.TypeError("expected {0}, got {1}",
                Ops.GetDynamicTypeFromType(typeof(BaseType)).__name__, 
                Ops.GetDynamicType(instance));
        }

        public override bool SetAttribute(object instance, object value) {
            if (instance is BaseType) 
                return base.SetAttribute(instance, value);
            if (instance is ExtensibleType)
                return base.SetAttribute(((ExtensibleType)instance).Value, value);            
            if (instance == null)
                return false;

            throw Ops.TypeError("expected {0}, got {1}",
                Ops.GetDynamicTypeFromType(typeof(BaseType)).__name__,
                Ops.GetDynamicType(instance));
        }
    }

    /// <summary>
    /// Just like a reflected property, but we also allow deleting of values (setting them to
    /// uninitialized)
    /// </summary>
    public class ReflectedSlotProperty : ReflectedProperty {
        
        public ReflectedSlotProperty(PropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt) :
            base(info, getter, setter, nt) {
        }

        [PythonName("__delete__")]
        public override bool DeleteAttribute(object instance) {
            if (instance != null) {
                SetAttribute(instance, new Uninitialized(info.Name));
                return true;
            }
            return false;
        }
    }

    public class ReflectedProperty : IDataDescriptor, IContextAwareMember {
        public readonly MethodInfo getter, setter;
        public readonly PropertyInfo info;
        private NameType nameType;
        private BuiltinFunction optGetter, optSetter;

        public ReflectedProperty(PropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt) {
            this.info = info;
            this.getter = getter;
            this.setter = setter;
            this.nameType = nt;
        }

        public string Documentation {
            [PythonName("__doc__")]
            get {
                object[] attrs = info.GetCustomAttributes(typeof(DocumentationAttribute), false);
                if (attrs.Length == 0) return null;

                StringBuilder docStr = new StringBuilder();
                for (int i = 0; i < attrs.Length; i++) {
                    docStr.Append(((DocumentationAttribute)attrs[i]).Value);
                    docStr.Append(Environment.NewLine);
                }
                return docStr.ToString();
            }
        }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object context) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Properties, this);
            if (getter == null) throw Ops.TypeError("writeonly attribute");

            OptimizePropertyMethod(ref optGetter, getter);

            if (instance == null && !getter.IsStatic) return this;

            if (optGetter != null) {
                if (instance == null) {
                    return optGetter.Call();
                } else {
                    return optGetter.Call(instance);
                }
            } 

            return DoGet(instance);            
        }

        [PythonName("__set__")]
        public bool SetAttribute(object instance, object value) {
            if(setter != null) OptimizePropertyMethod(ref optSetter, setter);

            if (instance == null && (setter == null || !setter.IsStatic)) return false;

            if (optSetter != null) {
                if (instance == null) {
                    optSetter.Call(value);
                } else {
                    optSetter.Call(instance, value);
                }
                return true;
            }

            DoSet(instance, value);
            return true;
        }

        [PythonName("__delete__")]
        public virtual bool DeleteAttribute(object instance) {
            throw Ops.TypeError("can't delete property on built-in object");
        }

        [PythonName("__str__")]
        public override string ToString() {
            return string.Format("<property# {0} on {1}>", info.Name, info.DeclaringType.Name);
        }

        private void OptimizePropertyMethod(ref BuiltinFunction bf, MethodInfo method) {
            if (Options.OptimizeReflectCalls && bf == null) {
                FunctionType ft = FunctionType.Method;
                if (method.IsStatic) ft = FunctionType.Function;
                bf = ReflectedMethod.MakeMethod(info.Name, method, ft);
            }
        }

        private void DoSet(object instance, object val) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Properties, this);
            if (setter == null) throw Ops.TypeError("readonly attribute");
            try {
                setter.Invoke(instance, new object[] { Ops.ConvertTo(val, info.PropertyType) });
            } catch (TargetInvocationException tie) {
                throw ExceptionConverter.UpdateForRethrow(tie.InnerException);
            }
        }

        private object DoGet(object instance) {
            try {
                return Ops.ToPython(getter.Invoke(instance, new object[0]));
            } catch (TargetInvocationException tie) {
                throw ExceptionConverter.UpdateForRethrow(tie.InnerException);
            }
        }

        #region IContextAwareMember Members

        public bool IsVisible(ICallerContext context) {
            return nameType == NameType.PythonProperty || (context.ContextFlags & CallerContextFlags.ShowCls) != 0;
        }

        #endregion
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
            if (other == null || other.dispatcher.Info != this.dispatcher.Info) throw Ops.TypeError("read-only attribute");
            return true;
        }

        [PythonName("__delete__")]
        public bool DeleteAttribute(object instance) {
            return true;
        }

        [PythonName("__str__")]
        public override string ToString() {
            return string.Format("<event# {0} on {1}>", dispatcher.Info.Name, dispatcher.Info.DeclaringType.Name);
        }

        #region IContextAwareMember Members

        bool IContextAwareMember.IsVisible(ICallerContext context) {
            // events aren't visible w/o importing clr.
            return !clsOnly || (context.ContextFlags & CallerContextFlags.ShowCls) != 0;
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

        class EventDispatcher : ICallable{
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
                info.AddEventHandler(instance, handler); 
            }

            private void Unhook(object instance) {
                Delegate handler = Ops.GetDelegate(this, info.EventHandlerType);
                info.RemoveEventHandler(instance, handler);
            }

            public EventInfo Info {
                get {
                    return info;
                }
            }
        }

    }

    public enum NameType {
        None,
        Method,
        ClassMethod,
        Field,
        Property,
        Event,
        Type,

        // Python versions are marked as exposed to all python code.
        PythonMethod,
        PythonProperty,
        PythonType,
        PythonField,
    }



}
