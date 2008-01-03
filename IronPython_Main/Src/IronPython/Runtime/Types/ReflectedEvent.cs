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
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {

    /// <summary>
    /// The unbound representation of an event property
    /// </summary>
    [PythonSystemType("event#")]
    public class ReflectedEvent : PythonTypeSlot, ICodeFormattable {
        private bool _clsOnly;
        private EventInfo/*!*/ _eventInfo;
        private WeakHash<object, HandlerList/*!*/> _handlerLists;
        private static object _staticTarget = new object();

        internal ReflectedEvent(EventInfo eventInfo, bool clsOnly) {
            Assert.NotNull(eventInfo);

            this._clsOnly = clsOnly;
            _eventInfo = eventInfo;
        }

        #region Internal APIs

        internal EventInfo/*!*/ Info {
            get {
                return _eventInfo;
            }
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            Assert.NotNull(context, owner);

            value = new BoundEvent(this, instance, (PythonType)owner);
            return true;
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            Assert.NotNull(context);
            BoundEvent et = value as BoundEvent;

            if (et == null || et.Event.Info != this.Info) {
                BadEventChange bea = value as BadEventChange;

                if (bea != null) {
                    PythonType dt = bea.Owner as PythonType;
                    if (dt != null) {
                        if (bea.Instance == null) {
                            throw new MissingMemberException(String.Format("attribute '{1}' of '{0}' object is read-only", dt.Name, SymbolTable.StringToId(_eventInfo.Name)));
                        } else {
                            throw new MissingMemberException(String.Format("'{0}' object has no attribute '{1}'", dt.Name, SymbolTable.StringToId(_eventInfo.Name)));
                        }
                    }
                }

                throw ReadOnlyException(DynamicHelpers.GetPythonTypeFromType(Info.DeclaringType));
            }

            return true;
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            return true;
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            Assert.NotNull(context, owner);
            throw ReadOnlyException(DynamicHelpers.GetPythonTypeFromType(Info.DeclaringType));
        }

        internal override bool IsVisible(CodeContext context, PythonType owner) {
            // events aren't visible w/o importing clr.
            return !_clsOnly || context.ModuleContext.ShowCls;
        }

        private HandlerList/*!*/ GetStubList(object instance) {
            if (_handlerLists == null) {
                System.Threading.Interlocked.CompareExchange(ref _handlerLists, new WeakHash<object, HandlerList>(), null);
            }

            if (instance == null) {
                // targetting a static method, we'll use a random object
                // as our place holder here...
                instance = _staticTarget;
            }

            lock (_handlerLists) {
                HandlerList result;
                if (_handlerLists.TryGetValue(instance, out result)) {
                    return result;
                }

                result = new HandlerList();
                _handlerLists[instance] = result;
                return result;
            }
        }

        #endregion

        #region Public Python APIs

        /// <summary>
        /// BoundEvent is the object that gets returned when the user gets an event object.  An
        /// BoundEvent tracks where the event was received from and is used to verify we get
        /// a proper add when dealing w/ statics events.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public class BoundEvent {
            private ReflectedEvent _event;
            private object _instance;
            private PythonType _ownerType;

            public ReflectedEvent Event {
                get {
                    return _event;
                }
            }

            public BoundEvent(ReflectedEvent reflectedEvent, object instance, PythonType ownerType) {
                Assert.NotNull(reflectedEvent, ownerType);

                _event = reflectedEvent;
                _instance = instance;
                _ownerType = ownerType;
            }

            // this one's correct, InPlaceAdd is wrong but we still have some dependencies on the wrong name.
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")] // TODO: fix
            [SpecialName]
            public object op_AdditionAssignment(object func) {
                return InPlaceAdd(func);
            }

            [SpecialName]
            public object InPlaceAdd(object func) {
                Assert.NotNull(func);

                MethodInfo add = _event.Info.GetAddMethod(true);
                if (add.IsStatic) {
                    if (_ownerType != DynamicHelpers.GetPythonTypeFromType(_event.Info.DeclaringType)) {
                        // mutating static event, only allow this from the type we're mutating, not sub-types
                        return new BadEventChange(_ownerType, _instance);
                    }
                }

                Delegate handler;
                HandlerList stubs;

                // we can add event directly (signature does match):
                if (_event.Info.EventHandlerType.IsAssignableFrom(func.GetType())) {
                    handler = (Delegate)func;
                    stubs = null;
                } else {
                    // create signature converting stub:
                    handler = Microsoft.Scripting.RuntimeHelpers.GetDelegate(func, _event.Info.EventHandlerType);
                    stubs = _event.GetStubList(_instance);
                }

                // wire the handler up:
                if (!add.DeclaringType.IsPublic) {
                    add = CompilerHelpers.GetCallableMethod(add);
                }

                if ((add.IsPublic && add.DeclaringType.IsPublic) || ScriptDomainManager.Options.PrivateBinding) {
                    add.Invoke(_instance, new object[] { handler });
                } else {
                    throw new ArgumentTypeException("cannot add to private event");
                }

                if (stubs != null) {
                    // remember the stub so that we could search for it on removal:
                    stubs.AddHandler(func, handler);
                }

                return this;
            }

            [SpecialName]
            public object InPlaceSubtract(CodeContext context, object func) {
                Assert.NotNull(context, func);

                MethodInfo remove = _event.Info.GetRemoveMethod(true);
                if (remove.IsStatic) {
                    if (_ownerType != DynamicHelpers.GetPythonTypeFromType(_event.Info.DeclaringType)) {
                        // mutating static event, only allow this from the type we're mutating, not sub-types
                        return new BadEventChange(_ownerType, _instance);
                    }
                }

                if (!remove.DeclaringType.IsPublic) {
                    remove = CompilerHelpers.GetCallableMethod(remove);
                }

                bool isRemovePublic = remove.IsPublic && remove.DeclaringType.IsPublic;
                if (isRemovePublic || ScriptDomainManager.Options.PrivateBinding) {

                    Delegate handler;

                    if (_event.Info.EventHandlerType.IsAssignableFrom(func.GetType())) {
                        handler = (Delegate)func;
                    } else {
                        handler = _event.GetStubList(_instance).RemoveHandler(context, func);
                    }

                    if (handler != null) {
                        remove.Invoke(_instance, new object[] { handler });
                    }
                } else {
                    throw new ArgumentTypeException("cannot subtract from private event");
                }

                return this;
            }
        }

        public void __set__(object instance, object value) {
            TrySetValue(DefaultContext.Default, instance, DynamicHelpers.GetPythonType(instance), value);
        }

        public void __delete__(object instance) {
            TryDeleteValue(DefaultContext.Default, instance, DynamicHelpers.GetPythonType(instance));
        }

        #endregion

        #region Private Helpers

        private class BadEventChange {
            private PythonType _ownerType;
            private object _instance;
            public BadEventChange(PythonType ownerType, object instance) {
                _ownerType = ownerType;
                _instance = instance;
            }

            public PythonType Owner {
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
        /// Holds on a list of delegates hooked to the event. 
        /// We need the list because we cannot enumerate the delegates hooked to CLR event and we need to do so in 
        /// handler removal (we need to do custom delegate comparison there). If BCL enables the enumeration we could remove this.
        /// </summary>
        private sealed class HandlerList {
            private readonly CopyOnWriteList<KeyValuePair<object, Delegate>> _handlers = new CopyOnWriteList<KeyValuePair<object, Delegate>>();

            public HandlerList() {
            }

            public void AddHandler(object callableObject, Delegate handler) {
                Assert.NotNull(handler);
                _handlers.Add(new KeyValuePair<object, Delegate>(callableObject, handler));
            }

            public Delegate RemoveHandler(CodeContext context, object callableObject) {
                Assert.NotNull(context);

                List<KeyValuePair<object, Delegate>> copyOfHandlers = _handlers.GetCopyForRead();
                for (int i = copyOfHandlers.Count - 1; i >= 0; i--) {
                    if (context.LanguageContext.EqualReturnBool(context, copyOfHandlers[i].Key, callableObject)) {
                        Delegate handler = copyOfHandlers[i].Value;
                        _handlers.RemoveAt(i);
                        return handler;
                    }
                }

                return null;
            }
        }

        private MissingMemberException ReadOnlyException(PythonType/*!*/ dt) {
            Assert.NotNull(dt);
            return new MissingMemberException(String.Format("attribute '{1}' of '{0}' object is read-only", dt.Name, SymbolTable.StringToId(_eventInfo.Name)));
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ ToCodeString(CodeContext context) {
            return string.Format("<event# {0} on {1}>", Info.Name, Info.DeclaringType.Name);
        }

        #endregion
    }
}
