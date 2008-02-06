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
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    
    /// <summary>
    /// Represents a member of a user-defined type which defines __slots__.  The names listed in
    /// __slots__ have storage allocated for them with the type and provide fast get/set access.
    /// </summary>
    [PythonType("member_descriptor")]
    class ReflectedSlotProperty : PythonTypeSlot, ICodeFormattable {
        private string _name;
        private SlotInfo _slotInfo;

        private static Dictionary<SlotInfo, SlotValue> _methods = new Dictionary<SlotInfo, SlotValue>();
                
        public ReflectedSlotProperty(string name, Type type, int index) {
            _slotInfo = new SlotInfo(index, type);
            _name = name;
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            if (instance != null) {
                value = Getter(instance);
                PythonOps.CheckInitializedAttribute(value, instance, _name);
                return true;
            }

            value = this;
            return true;
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            if (instance != null) {
                Setter(instance, value);
                return true;
            }

            return false;
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            return TrySetValue(context, instance, owner, Uninitialized.Instance);
        }

        public override string ToString() {
            return String.Format("<member '{0}'>", _name); // <member '{0}' of '{1}' objects> - but we don't know our type name
        }

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return ToString();
        }

        #endregion

        private SlotValue Value {
            get {
                SlotValue res;
                lock (_methods) {
                    if (!_methods.TryGetValue(_slotInfo, out res)) {
                        res = _methods[_slotInfo] = new SlotValue();
                    }
                }
                return res;
            }
        }

        internal SlotGetValue Getter {
            get {
                SlotValue value = Value;
                lock (value) {
                    EnsureGetter(value);
                    return value.Getter;
                }
            }
        }

        internal SlotSetValue Setter {
            get {
                SlotValue value = Value;
                lock (value) {
                    EnsureSetter(value);
                    return value.Setter;
                }

            }
        }

        internal MethodInfo GetterMethod {
            get {
                SlotValue value = Value;
                lock (value) {
                    EnsureGetter(value);
                    return value.GetterMethod;
                }
            }
        }

        internal MethodInfo SetterMethod {
            get {
                SlotValue value = Value;
                lock (value) {
                    EnsureSetter(value);
                    return value.SetterMethod;
                }
            }
        }

        private void EnsureGetter(SlotValue value) {
            if (value.Getter == null) {
                KeyValuePair<SlotGetValue, MethodInfo> kvp = CreateGetter();
                value.Getter = kvp.Key;
                value.GetterMethod = kvp.Value;
            }
        }

        private void EnsureSetter(SlotValue value) {
            if (value.Setter == null) {
                KeyValuePair<SlotSetValue, MethodInfo> kvp = CreateSetter();
                value.Setter = kvp.Key;
                value.SetterMethod = kvp.Value;
            }
        }

        private KeyValuePair<SlotGetValue, MethodInfo> CreateGetter() {
            DynamicILGen getter = CompilerHelpers.CreateDynamicMethod(
                "get_" + _slotInfo.Index.ToString(),
                typeof(object),
                new Type[] { typeof(object) }
            );

            PropertyInfo slotTuple = _slotInfo.Type.GetProperty("$SlotValues");

            getter.EmitLoadArg(0);
            getter.EmitExplicitCast(typeof(object), _slotInfo.Type);
            getter.EmitPropertyGet(slotTuple);
            foreach (PropertyInfo pi in Tuple.GetAccessPath(slotTuple.PropertyType, _slotInfo.Index)) {
                getter.EmitPropertyGet(pi);
            }
            getter.Emit(OpCodes.Ret);
            getter.CreateDelegate<SlotGetValue>();

            MethodInfo mi;
            SlotGetValue sgv = getter.CreateDelegate<SlotGetValue>(out mi);

            return new KeyValuePair<SlotGetValue, MethodInfo>(sgv, mi);
        }

        private KeyValuePair<SlotSetValue, MethodInfo> CreateSetter() {
            DynamicILGen setter = CompilerHelpers.CreateDynamicMethod(
                "set_" + _slotInfo.Index.ToString(),
                typeof(void),
                new Type[] { typeof(object), typeof(object) }
            );

            PropertyInfo slotTuple = _slotInfo.Type.GetProperty("$SlotValues");

            setter.EmitLoadArg(0);
            setter.EmitExplicitCast(typeof(object), _slotInfo.Type);
            setter.EmitPropertyGet(slotTuple);

            List<PropertyInfo> pis = new List<PropertyInfo>(1);
            foreach (PropertyInfo pi in Tuple.GetAccessPath(slotTuple.PropertyType, _slotInfo.Index)) {
                pis.Add(pi);
            }
            for (int i = 0; i < pis.Count - 1; i++) {
                setter.EmitPropertyGet(pis[i]);
            }
            setter.EmitLoadArg(1);
            setter.EmitPropertySet(pis[pis.Count - 1]);

            setter.Emit(OpCodes.Ret);

            MethodInfo mi;
            SlotSetValue ssv = setter.CreateDelegate<SlotSetValue>(out mi);

            return new KeyValuePair<SlotSetValue, MethodInfo>(ssv, mi);
        }

        /// <summary>
        /// Provides hashing based upon inherited type and indexing enabling us to share
        /// getter/setters for a slot amongst multiple types which use slots.
        /// </summary>
        private class SlotInfo : IEquatable<SlotInfo> {
            public int Index;
            public Type Type;

            public SlotInfo(int index, Type type) {
                Index = index;
                Type = type;
            }

            public override int GetHashCode() {
                return Type.GetHashCode() ^ Index;
            }

            public override bool Equals(object obj) {
                SlotInfo si = obj as SlotInfo;
                if (si == null) return false;

                return Equals(si);
            }

            #region IEquatable<SlotInfo> Members

            public bool Equals(SlotInfo other) {
                return Index == other.Index && Type == other.Type;
            }

            #endregion
        }

        private class SlotValue {
            public SlotGetValue Getter;
            public SlotSetValue Setter;
            public MethodInfo GetterMethod;
            public MethodInfo SetterMethod;
        }
    }

    delegate object SlotGetValue(object instance);
    delegate void   SlotSetValue(object instance, object value);
}
