/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types {
    internal delegate void TypeInitializer(PythonTypeBuilder builder);

    internal class PythonTypeBuilder {
        private PythonType _building;
        private List<TypeInitializer> _inits;
        private static List<EventHandler<TypeCreatedEventArgs>> _notifications = new List<EventHandler<TypeCreatedEventArgs>>();

        /// <summary>
        /// Creates a new PythonTypeBuilder for a PythonType with the specified name
        /// </summary>
        public PythonTypeBuilder(string name, Type underlyingSystemType) {
            Contract.RequiresNotNull(name, "name");
            Contract.RequiresNotNull(underlyingSystemType, "underlyingSystemType");

            _building = new PythonType(underlyingSystemType);
            _building.Name = name;
            _building.Builder = this;
        }

        public PythonTypeBuilder(string name, Type underlyingSystemType, Type extensionType)
            : this(name, underlyingSystemType) {
            _building.ExtensionType = extensionType;
        }

        internal PythonTypeBuilder(PythonType type) {
            _building = type;
        }

        /// <summary>
        /// Gets the PythonTypeBuilder for a pre-existing PythonType.
        /// </summary>
        public static PythonTypeBuilder GetBuilder(PythonType type) {
            Contract.RequiresNotNull(type, "type");

            lock (type.SyncRoot) {
                if (type.Builder == null) {
                    type.Builder = new PythonTypeBuilder(type);
                    return type.Builder;
                }

                return type.Builder;
            }
        }

        /// <summary>
        /// Returns the PythonType that is being built by this PythonTypeBuilder.
        /// </summary>
        public PythonType UnfinishedType {
            get {
                return _building;
            }
        }

        /// <summary>
        /// True if the type is a system type, false if the type is a user type.
        /// </summary>
        public bool IsSystemType {
            get {
                return _building.IsSystemType;
            }
            set {
                _building.IsSystemType = value;
            }
        }

        /// <summary>
        /// Adds a conversion from one type to another.
        /// </summary>
        public void AddConversion(Type from, Type to, CallTarget1 converter) {
            _building.AddConversion(from, to, converter);
        }
        
        /// <summary>
        /// Sets the type the dynamic tyep should impersonate
        /// </summary>
        public void SetImpersonationType(Type impersonate) {
            _building.ImpersonationType = impersonate;
        }

        /// <summary>
        /// Adds a new base type to the type being constructed
        /// </summary>
        public void AddBaseType(PythonType baseType) {
            Contract.RequiresNotNull(baseType, "baseType");

            _building.AddBaseType(baseType);
        }

        public void SetBases(IList<PythonType> bases) {
            _building.BaseTypes = new List<PythonType>(bases);
        }

        /// <summary>
        /// Sets the interface which can be used for constructing instances of this object
        /// </summary>
        public void SetConstructor(object callable) {
            _building.SetConstructor(callable);
        }

        public void SetIsExtensible() {
            SetExtensionType(typeof(Extensible<>).MakeGenericType(_building.UnderlyingSystemType));
        }

        public void SetExtensionType(Type type) {
            _building.ExtensionType = type;
        }

        /// <summary>
        /// Adds a new slot to the PythonType being constructed
        /// </summary>
        public void AddSlot(SymbolId name, PythonTypeSlot slot) {
            Contract.RequiresNotNull(slot, "slot");
            Debug.Assert(name != SymbolId.Empty);

            _building.AddSlot(name, slot);
        }

        public void AddSlot(ContextId context, SymbolId name, PythonTypeSlot slot) {
            Contract.RequiresNotNull(slot, "slot");
            Debug.Assert(name != SymbolId.Empty);

            _building.AddSlot(context, name, slot);
        }

        /// <summary>
        /// Removes a slot from the PythonType. Returns true if the slot is removed, false if the
        /// slot doesn't exist.
        /// </summary>
        public bool RemoveSlot(ContextId context, SymbolId name) {
            return _building.RemoveSlot(context, name);
        }

        public void SetHasGetAttribute(bool value) {
            _building.HasGetAttribute = value;
        }

        /// <summary>
        /// Sets a delegate this is used to intercept all member lookups.
        /// </summary>
        public void SetCustomBoundGetter(TryGetMemberCustomizer customizer) {
            _building.CustomBoundGetter = customizer;
        }

        /// <summary>
        /// Sets a delegate this is used to intercept all member sets.
        /// </summary>
        public void SetCustomSetter(SetMemberCustomizer customizer) {
            _building.CustomSetter = customizer;
        }

        /// <summary>
        /// Sets a delegate this is used to intercept all member deletes.
        /// </summary>
        public void SetCustomDeleter(DeleteMemberCustomizer customizer) {
            _building.CustomDeleter = customizer;
        }

        /// <summary>
        /// Sets a delegate that creates the default PythonTypeSlot type
        /// for this type.
        /// </summary>
        public void SetDefaultSlotType(CreateTypeSlot creator) {
            _building.SlotCreator = creator;
        }

        /// <summary>
        /// Sets the name of the type.
        /// </summary>
        public void SetName(string name) {
            _building.Name = name;
        }

        /// <summary>
        /// Adds a unary operator to the type being built.
        /// </summary>
        public void AddOperator(Operators op, UnaryOperator target) {
            _building.AddOperator(op, target);
        }

        /// <summary>
        /// Adds a binary operator to the type
        /// </summary>
        public void AddOperator(Operators op, BinaryOperator target) {
            _building.AddOperator(op, target);
        }

        /// <summary>
        /// Adds a ternary operator to the type.
        /// </summary>
        public void AddOperator(Operators op, TernaryOperator target) {
            _building.AddOperator(op, target);
        }


        /// <summary>
        /// Adds a unary operator for a specific context to the type being built.  
        /// </summary>
        public void AddOperator(ContextId context, Operators op, UnaryOperator target) {
            _building.AddOperator(context, op, target);
        }

        /// <summary>
        /// Adds a binary operator for a specific context to the type
        /// </summary>
        public void AddOperator(ContextId context, Operators op, BinaryOperator target) {
            _building.AddOperator(context, op, target);
        }

        /// <summary>
        /// Adds a ternary operator for a specific context to the type.
        /// </summary>
        public void AddOperator(ContextId context, Operators op, TernaryOperator target) {
            _building.AddOperator(context, op, target);
        }

        /// <summary>
        /// Sets the method resolution order that will be used for the type.
        /// </summary>
        public void SetResolutionOrder(IList<PythonType> resolutionOrder) {
            Contract.RequiresNotNull(resolutionOrder, "resolutionOrder");

            _building.ResolutionOrder = resolutionOrder;
        }

        /// <summary>
        /// Sets the ContextId that this type was created from and belongs to.
        /// </summary>
        public void SetTypeContext(ContextId id) {
            _building.TypeContext = id;
        }

        /// <summary>
        /// Finishes constructing the type and returns a newly created and immutable PythonType object
        /// </summary>
        public PythonType Finish() {
            return Finish(true);
        }

        /// <summary>
        /// Finishes constructing the type and returns the newly created PythonType object
        /// </summary>
        public PythonType Finish(bool frozen) {
            if (frozen) _building.IsImmutable = true;

            _building.Commit(); //TODO: Thread Safety?
            return _building;
        }

        /// <summary>
        /// Adds an initializer that runs the first time the type is used in a 
        /// substantial way
        /// </summary>
        public void AddInitializer(TypeInitializer init) {
            Contract.RequiresNotNull(init, "init");

            if (_inits == null) _inits = new List<TypeInitializer>();

            _inits.Add(init);
        }

        /// <summary>
        /// Called to perform the lazy initialization from a type.
        /// </summary>
        internal void Initialize() {
            if (_inits != null) {

                for (int i = 0; i < _inits.Count; i++) {
                    PerfTrack.NoteEvent(PerfTrack.Categories.OverAllocate, "PythonTypeInit " + _building.Name);
                    _inits[i](this);
                }
                _inits = null;


                PythonType dt = _building as PythonType;
                EventHandler<TypeCreatedEventArgs>[] notifys;
                lock (_notifications) notifys = _notifications.ToArray();
                foreach (EventHandler<TypeCreatedEventArgs> init in notifys) {
                    init(this, new TypeCreatedEventArgs(dt));
                }
            }
        }

        /// <summary>
        /// Fired when a new .NET type is initialialzed.
        /// </summary>
        public static event EventHandler<TypeCreatedEventArgs> TypeInitialized {
            add {
                List<PythonType> inited = new List<PythonType>();
                lock (_notifications) {
                    _notifications.Add(value);

                    int current = 0;
                    inited.Add(DynamicHelpers.GetPythonTypeFromType(typeof(object)));

                    while (current < inited.Count) {
                        PythonType dt = inited[current++];

                        IList<WeakReference> types = dt.SubTypes;
                        if (types != null) {
                            foreach (WeakReference wr in types) {
                                if (wr.IsAlive) {
                                    PythonType wrtype = (PythonType)wr.Target;

                                    if (wrtype != null) {
                                        inited.Add(wrtype);
                                    }
                                }
                            }
                        }
                    }

                }

                foreach (PythonType dt in inited) {
                    value(typeof(PythonTypeBuilder), new TypeCreatedEventArgs(dt));
                }
            }
            remove {
                lock (_notifications) {
                    _notifications.Remove(value);
                }
            }
        }

        /// <summary>
        /// Forces initialization of the build type and releases the builder
        /// object.  This should be called if a builder is created for a 
        /// pre-existing type and no new initializers were added.
        /// </summary>
        public void ReleaseBuilder() {
            lock (_building.SyncRoot) {
                if (_inits != null) {
                    _building.Initialize();
                }

                _building.Builder = null;
            }
        }
    }
}
