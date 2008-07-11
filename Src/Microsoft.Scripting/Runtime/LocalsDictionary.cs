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
using System.Linq.Expressions;
using System.Scripting;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {

    /// <summary>
    /// Wraps ILocalVariables in a dictionary
    /// </summary>
    public sealed class LocalsDictionary : CustomSymbolDictionary {
        private readonly ILocalVariables _locals;

        // TODO: remove, lazily created stuff for CustomSymbolDictionary
        private Dictionary<SymbolId, int> _indexes;
        private SymbolId[] _symbols;

        public LocalsDictionary(ILocalVariables locals) {
            Assert.NotNull(locals);
            _locals = locals;
        }

        public ReadOnlyCollection<string> Names {
            get { return _locals.Names; }
        }

        private void EnsureSymbols() {
            if (_symbols == null) {
                int count = _locals.Names.Count;
                SymbolId[] symbols = new SymbolId[count];
                for (int i = 0; i < count; i++) {
                    symbols[i] = SymbolTable.StringToId(_locals.Names[i]);
                }
                _symbols = symbols;
            }
        }
        
        private void EnsureIndexes() {
            if (_indexes == null) {
                EnsureSymbols();

                int count = _symbols.Length;
                Dictionary<SymbolId, int> indexes = new Dictionary<SymbolId, int>(count);
                for (int index = 0; index < count; index++) {
                    indexes[_symbols[index]] = index;
                }
                _indexes = indexes;
            }
        }


        public override SymbolId[] GetExtraKeys() {
            EnsureSymbols();
            return _symbols;
        }

        protected override bool TrySetExtraValue(SymbolId key, object value) {
            EnsureIndexes();

            int index;
            if (_indexes.TryGetValue(key, out index)) {
                _locals[index] = value;
                return true;
            }

            return false;
        }

        protected override bool TryGetExtraValue(SymbolId key, out object value) {
            EnsureIndexes();

            int index;
            if (_indexes.TryGetValue(key, out index)) {
                value = _locals[index];
                return true;
            }
            value = null;
            return false;
        }
    }
}
