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

using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    [PythonType("generator")]
    public sealed class Generator : IDynamicObject, IEnumerator, IEnumerable, ICustomAttributes {
        private static BuiltinFunction nextFunction = GetNextFunctionTemplate();

        private object current = null;
        private NextTarget generateNext;

        public int location = Int32.MaxValue;
        public FunctionEnvironmentDictionary environment;
        public FunctionEnvironmentDictionary staticLink;

        public delegate bool NextTarget(Generator generator, out object ret);

        public Generator(FunctionEnvironmentDictionary staticLink, FunctionEnvironmentDictionary environment, NextTarget generateNext) {
            this.environment = environment;
            this.staticLink = staticLink;
            this.generateNext = generateNext;
        }

        public object Current {
            get {
                return current;
            }
        }

        public bool MoveNext() {
            return generateNext(this, out current);
        }

        public void Reset() {
            throw new NotImplementedException();
        }

        [PythonName("next")]
        public object Next() {
            object ret;
            if (!generateNext(this, out ret)) throw Ops.StopIteration();
            return ret;
        }

        public override string ToString() {
            return string.Format("<generator object at {0}>", Ops.HexId(this));
        }

        private static BuiltinFunction GetNextFunctionTemplate() {
            BuiltinMethodDescriptor bimd = (BuiltinMethodDescriptor)TypeCache.Generator.GetAttr(
                DefaultContext.Default, null, SymbolTable.GeneratorNext);
            return bimd.template;
        }

        #region IEnumerable Members

        [PythonName("__iter__")]
        public IEnumerator GetEnumerator() {
            return this;
        }

        #endregion

        #region IDynamicObject Members

        private static readonly DynamicType generatorType = Ops.GetDynamicTypeFromType(typeof(Generator));
        public DynamicType GetDynamicType() {
            return generatorType;
        }

        #endregion

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            if (name == SymbolTable.GeneratorNext) {
                // next is the most common call on generators, we optimize that call here.  We get
                // two benefits out of this:
                //      1. Avoid the dictionary lookup for next
                //      2. Avoid the self-check in the method descriptor (because we know we're binding to a generator)
                value = new BoundBuiltinFunction(nextFunction, this);
                return true;
            }
            return generatorType.TryGetAttr(context, this, name, out value);
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            generatorType.SetAttr(context, this, name, value);
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            generatorType.DelAttr(context, this, name);
        }

        public List GetAttrNames(ICallerContext context) {
            return generatorType.GetAttrNames(context, this);
        }

        public System.Collections.Generic.IDictionary<object, object> GetAttrDict(ICallerContext context) {
            return generatorType.GetAttrDict(context, this);
        }

        #endregion
    }

    [PythonType("enum_iter")]
    public class EnumIterator {
        private IEnumerator e;
        public EnumIterator(IEnumerator e) { this.e = e; }

        [PythonName("next")]
        public object Next() {
            if (!e.MoveNext()) throw Ops.StopIteration();
            return e.Current;
        }
    }
}
