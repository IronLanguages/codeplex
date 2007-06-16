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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Scripting.Generation {

    /// <summary>
    /// Base class for all other slot factories.  Supports creating either strongly typed
    /// slots or slots that are always of type object.
    /// </summary>
    public abstract class SlotFactory {
        private Dictionary<SymbolId, Slot> _fields = new Dictionary<SymbolId, Slot>();

        /// <summary>
        /// Gets or creates a new slot with the given name of the specified type.
        /// </summary>
        public Slot MakeSlot(SymbolId name, Type type) {
            Slot res;

            if (!_fields.TryGetValue(name, out res)) {
                _fields[name] = res = CreateSlot(name, type);
            }

            return res;
        }
 
        /// <summary>
        /// Overriden by the base type.  Creates a new slot of the given name and type.  Only called once for each name.
        /// </summary>
        protected abstract Slot CreateSlot(SymbolId name, Type type);

        /// <summary>
        /// Called before emitting code into the specified CodeGen.  Provides an opportunity to setup any
        /// method-local state for the slot factory.
        /// </summary>
        /// <param name="cg"></param>
        public virtual void PrepareForEmit(CodeGen cg) {
        }

        /// <summary>
        /// Provides all the fields created by the SlotFactory.
        /// </summary>
        public Dictionary<SymbolId, Slot> Fields {
            get {
                return _fields;
            }
        }
    }

    /// <summary>
    /// Creates slots that are backed by local variables.
    /// </summary>
    public class LocalSlotFactory : SlotFactory {
        private CodeGen codeGen;

        public LocalSlotFactory(CodeGen codeGen) {
            this.codeGen = codeGen;
        }

        protected override Slot CreateSlot(SymbolId name, Type type) {
            LocalBuilder b = codeGen.DeclareLocal(type);
            if (codeGen.EmitDebugInfo) b.SetLocalSymInfo(SymbolTable.IdToString(name));
            return new LocalSlot(b, codeGen);
        }
    }

    /// <summary>
    /// Creates slots that are backed by fields in a type.
    /// </summary>
    public class FieldSlotFactory : SlotFactory {
        private TypeGen typeGen;
        private Slot instance;

        public FieldSlotFactory(TypeGen typeGen, Slot instance) {
            this.typeGen = typeGen;
            this.instance = instance;
        }
        protected override Slot CreateSlot(SymbolId name, Type type) {
            FieldBuilder fb = typeGen.TypeBuilder.DefineField(SymbolTable.IdToString(name), type, FieldAttributes.Public);
            return new FieldSlot(instance, fb);
        }
    }

    public class StaticFieldSlotFactory : SlotFactory {
        private TypeGen _typeGen;

        public StaticFieldSlotFactory(TypeGen typeGen) {
            _typeGen = typeGen;
        }

        protected override Slot CreateSlot(SymbolId name, Type type) {
            FieldBuilder fb = _typeGen.TypeBuilder.DefineField(SymbolTable.IdToString(name), type, FieldAttributes.Assembly | FieldAttributes.Static);
            return new StaticFieldSlot(fb);
        }
    }

    public class LocalFrameSlotFactory : SlotFactory {
        protected Slot frame;

        public LocalFrameSlotFactory(Slot frame) {
            this.frame = frame;
        }

        protected override Slot CreateSlot(SymbolId name, Type type) {
            return new LocalNamedFrameSlot(frame, name);
        }
    }     
 
    /// <summary>
    /// Slot factory that indexes into a tuple.  The slot factory will generate a TupleDictionary 
    /// and a NewTuple object that backs it with the types generated from requests to CreateSlot.
    /// 
    /// All slots need to be created before accessing the TupleType property.
    /// </summary>
    public class TupleSlotFactory : SlotFactory, ILazySlotFactory<int> {
        private List<Slot> _slots = new List<Slot>();
        private List<SymbolId> _names = new List<SymbolId>();
        private Type _dictType, _tupleType;
        private Dictionary<CodeGen, List<Slot>> _concreteSlots;

        public TupleSlotFactory(Type dictType) {
            _dictType = dictType;
        }

        protected virtual Slot PrepareSlotForEmit(CodeGen cg) {
            // Emit globals from context and cast to tuple type            

            // tmpLocal = ((tupleDictType)codeContext.Scope.GlobalScope.GetDictionary(context)).Tuple
            cg.EmitCodeContext();
            cg.EmitPropertyGet(typeof(CodeContext), "Scope");
            cg.EmitPropertyGet(typeof(Scope), "ModuleScope");
            cg.EmitCall(typeof(RuntimeHelpers).GetMethod("GetTupleDictionaryData").MakeGenericMethod(TupleType));

            Slot tmpLocal = cg.GetLocalTmp(TupleType);
            tmpLocal.EmitSet(cg);

            return tmpLocal;
        }

        public override void PrepareForEmit(CodeGen cg) {
            if (_concreteSlots == null) _concreteSlots = new Dictionary<CodeGen, List<Slot>>();
            if (_concreteSlots.ContainsKey(cg)) return;

            Slot tmpLocal = PrepareSlotForEmit(cg);

            List<Slot> concreteSlots = new List<Slot>(_slots.Count);
            for (int i = 0; i < _slots.Count; i++ ) {
                concreteSlots.Add(CreateConcreteSlot(tmpLocal, i));
            }
            _concreteSlots[cg] = concreteSlots;
        }

        public Slot GetConcreteSlot(CodeGen cg, int data) {
            return _concreteSlots[cg][data];
        }

        protected override Slot CreateSlot(SymbolId name, Type type) {
            if (_tupleType != null) throw new InvalidOperationException("cannot add slots after tuple type has been determined");

            Slot res = new LazySlot<int>(this, type, _slots.Count);

            _slots.Add(res);
            _names.Add(name);

            return res;
        }

        internal Slot CreateConcreteSlot(Slot instance, int index) {
            // We get the final index by breaking the index into groups of bits.  The more significant bits
            // represent the indexes into the outermost tuples and the least significant bits index into the
            // inner most tuples.  The mask is initialized to mask the upper bits and adjust is initialized
            // and adjust is the value we need to divide by to get the index in the least significant bits.
            // As we go through we shift the mask and adjust down each loop to pull out the inner slot.  Logically
            // everything in here is shifting bits (not multiplying or dividing) because NewTuple.MaxSize is a 
            // power of 2.
            int depth = 0;
            int mask = NewTuple.MaxSize - 1;
            int adjust = 1;
            int count = _slots.Count;
            while (count > NewTuple.MaxSize) {
                depth++;
                count /= NewTuple.MaxSize;
                mask *= NewTuple.MaxSize;
                adjust *= NewTuple.MaxSize;
            }

            while(depth-- >= 0) {
                Debug.Assert(mask != 0);

                int curIndex = (index & mask) / adjust;
                instance = new PropertySlot(instance, instance.Type.GetProperty("Item" + String.Format("{0:D3}", curIndex)));

                mask /= NewTuple.MaxSize;
                adjust /= NewTuple.MaxSize;
            }
            return instance;            
        }

        public Type TupleType {
            get {
                if (_tupleType != null) return _tupleType;

                _tupleType = MakeTupleType(0, _slots.Count);
                return _tupleType;
            }
        }

        /// <summary>
        /// Creates the type used for the tuple.  If the number of slots fits within the maximum tuple size then we simply 
        /// create a single tuple.  If it's greater then we create nested tuples (e.g. a Tuple`2 which contains a Tuple`128
        /// and a Tuple`8 if we had a size of 136).
        /// </summary>
        private Type MakeTupleType(int start, int end) {
            int size = end - start;

            Type type = NewTuple.GetTupleType(size);
            if (type != null) {
                Type[] typeArr = new Type[type.GetGenericArguments().Length];
                int index = 0;
                for (int i = start; i < end; i++) {
                    typeArr[index++] = _slots[i].Type;
                }
                while (index < typeArr.Length) {
                    typeArr[index++] = typeof(object);
                }
                return type.MakeGenericType(typeArr);
            }

            int multiplier = 1;
            while (size > NewTuple.MaxSize) {
                size = (size + NewTuple.MaxSize - 1) / NewTuple.MaxSize;
                multiplier *= NewTuple.MaxSize;
            }

            type = NewTuple.GetTupleType(size);
            Debug.Assert(type != null);
            Type[] nestedTypes = new Type[type.GetGenericArguments().Length];
            for (int i = 0; i < size; i++) {
                
                int newStart = start + (i * multiplier);
                int newEnd   = System.Math.Min(end, start + ((i + 1) * multiplier));
                nestedTypes[i] = MakeTupleType(newStart, newEnd);
            }
            for (int i = size; i < nestedTypes.Length; i++) {
                nestedTypes[i] = typeof(object);
            }

            return type.MakeGenericType(nestedTypes);
        }

        public object CreateTupleInstance() {
            return CreateTupleInstance(TupleType, 0, _slots.Count);
        }

        /// <summary>
        /// Creates tupleType and any nested tuple types (which belong to this slot factory) stored with in it.  
        /// </summary>
        private object CreateTupleInstance(Type tupleType, int start, int end) {
            int size = end - start;

            object res = Activator.CreateInstance(tupleType);
            if (size > NewTuple.MaxSize) {
                int multiplier = 1;
                while (size > NewTuple.MaxSize) {
                    size = (size + NewTuple.MaxSize - 1) / NewTuple.MaxSize;
                    multiplier *= NewTuple.MaxSize;
                }
                for (int i = 0; i < size; i++) {
                    int newStart = start + (i * multiplier);
                    int newEnd = System.Math.Min(end, start + ((i + 1) * multiplier));

                    PropertyInfo pi = tupleType.GetProperty("Item" + String.Format("{0:D3}", i));
                    Debug.Assert(pi != null);
                    pi.SetValue(res, CreateTupleInstance(pi.PropertyType, newStart, newEnd), null);
                }
                
            }
            return res;
        }

        public Type DictionaryType {
            get {
                return _dictType;
            }
        }

        public IList<SymbolId> Names {
            get {
                return _names.ToArray();
            }
        }
    }



}
