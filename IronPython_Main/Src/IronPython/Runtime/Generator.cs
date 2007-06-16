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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;   
using Microsoft.Scripting;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;

namespace IronPython.Runtime {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix"), PythonType("generator")]
    public sealed class PythonGenerator : Generator, IEnumerable, ICustomMembers, IEnumerable<object> {
        private static BuiltinFunction nextFunction = GetNextFunctionTemplate();
        private NextTarget _next;

        public delegate bool NextTarget(PythonGenerator generator, out object ret);

        public PythonGenerator(CodeContext context, NextTarget next)
        : base(context) {
            _next = next;
        }

        public override bool MoveNext() {
            bool ret;
            object next;

            try {
                ret = _next(this, out next);
            } catch (StopIterationException) {
                next = null;
                ret = false;
            }
            this.Current = next;
            return ret;
        }

        [PythonName("next")]
        public object Next() {
            if (!MoveNext()) {
                throw PythonOps.StopIteration();
            }
            return Current;
        }

        public override string ToString() {
            return string.Format("<generator object at {0}>", PythonOps.HexId(this));
        }

        private static BuiltinFunction GetNextFunctionTemplate() {
            BuiltinMethodDescriptor bimd = (BuiltinMethodDescriptor)TypeCache.Generator.GetMember(
                DefaultContext.Default, null, Symbols.GeneratorNext);
            return bimd.Template;
        }

        #region IEnumerable Members

        [PythonName("__iter__")]
        public IEnumerator GetEnumerator() {
            return this;
        }

        #endregion

        private static DynamicType generatorType = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonGenerator));


        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            return generatorType.TryGetMember(context, this, name, out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            if (name == Symbols.GeneratorNext) {
                // next is the most common call on generators, we optimize that call here.  We get
                // two benefits out of this:
                //      1. Avoid the dictionary lookup for next
                //      2. Avoid the self-check in the method descriptor (because we know we're binding to a generator)
                value = new BoundBuiltinFunction(nextFunction, this);
                return true;
            }
            return generatorType.TryGetBoundMember(context, this, name, out value);
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            generatorType.SetMember(context, this, name, value);
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            generatorType.DeleteMember(context, this, name);
            return true;
        }

        public IList<object> GetCustomMemberNames(CodeContext context) {
            return new List(generatorType.GetMemberNames(context, this));
        }

        public System.Collections.Generic.IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            return generatorType.GetMemberDictionary(context, this).AsObjectKeyedDictionary();
        }

        #endregion

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return this;
        }

        #endregion
    }
}
