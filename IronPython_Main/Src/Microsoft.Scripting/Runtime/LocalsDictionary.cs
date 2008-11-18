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
using System; using Microsoft;


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {

    /// <summary>
    /// Creates a dictionary of locals in this scope
    /// </summary>
    public sealed class LocalsDictionary : CustomSymbolDictionary {
        private readonly IList<IStrongBox> _locals;
        private readonly SymbolId[] _symbols;
        private Dictionary<SymbolId, IStrongBox> _boxes;

        public LocalsDictionary(IList<IStrongBox> locals, SymbolId[] symbols) {
            Assert.NotNull(locals, symbols);
            _locals = locals;
            _symbols = symbols;
        }
        
        private void EnsureBoxes() {
            if (_boxes == null) {
                int count = _symbols.Length;
                Dictionary<SymbolId, IStrongBox> boxes = new Dictionary<SymbolId, IStrongBox>(count);
                for (int i = 0; i < count; i++) {
                    boxes[_symbols[i]] = _locals[i];
                }
                _boxes = boxes;
            }
        }

        public override SymbolId[] GetExtraKeys() {
            return _symbols;
        }

        protected internal override bool TrySetExtraValue(SymbolId key, object value) {
            EnsureBoxes();

            IStrongBox box;
            if (_boxes.TryGetValue(key, out box)) {
                box.Value = value;
                return true;
            }

            return false;
        }

        protected internal override bool TryGetExtraValue(SymbolId key, out object value) {
            EnsureBoxes();

            IStrongBox box;
            if (_boxes.TryGetValue(key, out box)) {
                value = box.Value;
                return true;
            }
            value = null;
            return false;
        }
    }
}
