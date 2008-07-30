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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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

        public int Count {
            get { return _indexes.Length; }
        }

        public ReadOnlyCollection<string> Names {
            get { return _names; }
        }

        public IStrongBox this[int index] {
            get {
                // We lookup the closure using two ints:
                // 1. The high dword is the number of parents to go up
                // 2. The low dword is the index into that array
                long closureKey = _indexes[index];

                // walk up the parent chain to find the real environment
                object[] result = _data;
                for (int parents = (int)(closureKey >> 32); parents > 0; parents--) {
                    result = HoistedLocals.GetParent(result);
                }

                // Return the variable storage
                return (IStrongBox)result[(int)closureKey];
            }
        }
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
