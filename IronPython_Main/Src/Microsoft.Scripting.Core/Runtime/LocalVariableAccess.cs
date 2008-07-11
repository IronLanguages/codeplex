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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Utils;

namespace System.Scripting.Runtime {

    /// <summary>
    /// Provides a list of local variables, supporing read/write of elements
    /// Exposed via LocalScopeExpression
    /// </summary>
    internal sealed class LocalVariableAccess : ILocalVariables {
        // The names of the variables
        private readonly ReadOnlyCollection<string> _names;

        // The top level environment. It contains pointers to parent
        // environments, which are always in the first element
        private readonly object[] _data;

        // An array of (int, int) pairs, each representing how to find a
        // variable in the environment data struction.
        //
        // The first integer indicates the number of times to go up in the
        // closure chain, the second integer indicates the index into that
        // closure chain.
        private readonly long[] _indexes;

        internal static readonly ILocalVariables Empty = new LocalVariableAccess(new object[0], new string[0], new long[0]);

        internal LocalVariableAccess(object[] data, string[] names, long[] indexes) {
            Assert.NotNull(names, data, indexes);
            Debug.Assert(names.Length == indexes.Length);

            _names = new ReadOnlyCollection<string>(names);
            _data = data;
            _indexes = indexes;
        }

        private System.Runtime.CompilerServices.IStrongBox GetVariable(int i) {
            long index = _indexes[i];

            // walk up the parent chain to find the real environment
            object[] result = _data;
            for (int parents = (int)(index >> 32); parents > 0; parents--) {
                result = HoistedLocals.GetParent(result);
            }

            // Get the variable storage
            return (System.Runtime.CompilerServices.IStrongBox)result[(int)index];
        }

        #region ILocalVariables Members

        public ReadOnlyCollection<string> Names {
            get { return _names; }
        }

        #endregion

        #region IList<object> Members

        public int IndexOf(object item) {
            int i = 0;
            foreach (object element in this) {
                if (object.Equals(element, item)) {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public void Insert(int index, object item) {
            throw new NotSupportedException("variables cannot be added/removed");
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException("variables cannot be added/removed");
        }

        public object this[int index] {
            get {
                ContractUtils.RequiresArrayIndex(this, index, "index");
                return GetVariable(index).Value;
            }
            set {
                ContractUtils.RequiresArrayIndex(this, index, "index");
                GetVariable(index).Value = value;
            }
        }

        #endregion

        #region ICollection<object> Members

        public void Add(object item) {
            throw new NotSupportedException("variables cannot be added/removed");
        }

        public void Clear() {
            throw new NotSupportedException("variables cannot be added/removed");
        }

        public bool Contains(object item) {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(object[] array, int arrayIndex) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresArrayRange(array, arrayIndex, Count, "arrayIndex", "array");

            foreach (object value in this) {
                array[arrayIndex++] = value;
            }
        }

        public int Count {
            get { return _indexes.Length; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(object item) {
            throw new NotSupportedException("variables cannot be added/removed");
        }

        #endregion

        #region IEnumerable<object> Members

        public IEnumerator<object> GetEnumerator() {
            for (int i = 0, count = Count; i < count; i++) {
                yield return GetVariable(i).Value;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

    }
    
    public static partial class RuntimeHelpers {
        // creates access for local variables in scope
        [Obsolete("used by generated code", true)]
        public static ILocalVariables CreateVariableAccess(object[] data, string[] names, long[] indexes) {
            return new LocalVariableAccess(data, names, indexes);
        }

        // creates access when there are no variables in scope
        [Obsolete("used by generated code", true)]
        public static ILocalVariables CreateEmptyVariableAccess() {
            return LocalVariableAccess.Empty;
        }
    }
}
