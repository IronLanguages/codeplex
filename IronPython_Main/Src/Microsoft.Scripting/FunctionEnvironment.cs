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
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting {
    /// <summary>
    /// Base class for FunctionEnvironment's which use a Tuple for the underlying storage.
    /// </summary>
    /// <typeparam name="TupleType"></typeparam>
    public sealed class FunctionEnvironmentDictionary<TupleType> : TupleDictionary<TupleType> where TupleType : NewTuple {
        public FunctionEnvironmentDictionary(TupleType data, SymbolId[] names) :
            base(data) {
            Extra = names;
        }

        protected internal override bool TrySetExtraValue(SymbolId key, object value) {
            for (int i = 0; i < Extra.Length; i++) {
                if (Extra[i] == key) {
                    Tuple.SetValue(i + 1, value);
                    return true;
                }
            }
            return false;
        }

        protected internal override bool TryGetExtraValue(SymbolId key, out object value) {
            for (int index = 0; index < Extra.Length; index++) {
                if (Extra[index] == key) {
                    value = Tuple.GetValue(index + 1);
                    return true;
                }
            }
            value = null;
            return false;
        }
    }
    
    /// <summary>
    /// The environment for closures. The environment provides access to the variables
    /// defined in the enclosing lexical scopes.
    /// </summary>
    public sealed class FunctionEnvironmentNDictionary : CustomSymbolDictionary {
        // Array of the variables in the environment
        private object[] _environmentValues;
        private SymbolId[] _names;

        public FunctionEnvironmentNDictionary() {
        }

        public FunctionEnvironmentNDictionary(object [] envValues, SymbolId[] names) {
            if (envValues == null) throw new ArgumentNullException("envValues");

            PerfTrack.NoteEvent(PerfTrack.Categories.Temporary, "FuncEnv " + envValues.Length);
            Debug.Assert(names.Length <= envValues.Length);

            _names = names;
            _environmentValues = envValues;            
        }

        public object[] EnvironmentValues {
            get { return _environmentValues; }
        }

        protected internal override bool TrySetExtraValue(SymbolId key, object value) {
            for (int i = 0; i < _names.Length; i++) {
                if (_names[i] == key) {
                    _environmentValues[i] = value;
                    return true;
                }
            }
            return false;
        }

        protected internal override bool TryGetExtraValue(SymbolId key, out object value) {
            for (int index = 0; index < _names.Length; index++) {
                if (_names[index] == key) {
                    value = _environmentValues[index];
                    return true;
                }
            }

            value = null;
            return false;
        }

        public override SymbolId[] GetExtraKeys() {
            return _names;
        }
    }
}
