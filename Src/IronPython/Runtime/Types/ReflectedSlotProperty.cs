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
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Generation;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Represents a member of a user-defined type which defines __slots__.  The names listed in
    /// __slots__ have storage allocated for them with the type and provide fast get/set access.
    /// </summary>
    [PythonType("member_descriptor")]
    class ReflectedSlotProperty : DynamicTypeSlot, ICodeFormattable {
        private string _name;
        private SlotInfo _slotInfo;

        private static Dictionary<SlotInfo, SlotValue> _methods = new Dictionary<SlotInfo, SlotValue>();
                
        public ReflectedSlotProperty(string name, Type type, int index) {
            _slotInfo = new SlotInfo(index, type);
            _name = name;
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            if (instance != null) {
                value = Getter(instance);
                PythonOps.CheckInitializedAttribute(value, instance, _name);
                return true;
            }

            value = this;
            return true;
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            if (instance != null) {
                Setter(instance, value);
                return true;
            }

            return false;
        }

        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
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
            CodeGen getter = ScriptDomainManager.CurrentManager.Snippets.Assembly.DefineMethod("get_" + _slotInfo.Index.ToString(),
                typeof(object),
                new Type[] { typeof(object) },
                null);


            PropertyInfo slotTuple = _slotInfo.Type.GetProperty("$SlotValues");

            getter.EmitArgGet(0);
            getter.EmitCast(typeof(object), _slotInfo.Type);
            getter.EmitPropertyGet(slotTuple);
            foreach (PropertyInfo pi in NewTuple.GetAccessPath(slotTuple.PropertyType, _slotInfo.Index)) {
                getter.EmitPropertyGet(pi);
            }
            getter.EmitReturn();
            getter.Finish();

            return new KeyValuePair<SlotGetValue, MethodInfo>((SlotGetValue)getter.CreateDelegate(typeof(SlotGetValue)), getter.CreateDelegateMethodInfo());
        }

        private KeyValuePair<SlotSetValue, MethodInfo> CreateSetter() {
            CodeGen setter = ScriptDomainManager.CurrentManager.Snippets.Assembly.DefineMethod("set_" + _slotInfo.Index.ToString(),
                                    typeof(void),
                                    new Type[] { typeof(object), typeof(object) },
                                    null);

            PropertyInfo slotTuple = _slotInfo.Type.GetProperty("$SlotValues");

            setter.EmitArgGet(0);
            setter.EmitCast(typeof(object), _slotInfo.Type);
            setter.EmitPropertyGet(slotTuple);

            List<PropertyInfo> pis = new List<PropertyInfo>(1);
            foreach (PropertyInfo pi in NewTuple.GetAccessPath(slotTuple.PropertyType, _slotInfo.Index)) {
                pis.Add(pi);
            }
            for (int i = 0; i < pis.Count - 1; i++) {
                setter.EmitPropertyGet(pis[i]);
            }
            setter.EmitArgGet(1);
            setter.EmitPropertySet(pis[pis.Count - 1]);

            setter.EmitReturn();
            setter.Finish();

            return new KeyValuePair<SlotSetValue, MethodInfo>((SlotSetValue)setter.CreateDelegate(typeof(SlotSetValue)), setter.CreateDelegateMethodInfo());
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
