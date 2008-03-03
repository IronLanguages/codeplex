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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IronPython.Runtime {
    /// <summary>
    /// Abstract base class for all PythonDictionary storage.
    /// 
    /// Defined as a class instead of an interface for performance reasons.  Also not
    /// using IDictionary* for keeping a simple interface.
    /// </summary>
    public abstract class DictionaryStorage  {
        public abstract void Add(object key, object value);
        public abstract bool Contains(object key);
        public abstract bool Remove(object key);
        public abstract bool TryGetValue(object key, out object value);
        public abstract int Count { get; }
        public abstract void Clear();
        public abstract List<KeyValuePair<object, object>> GetItems();
        public virtual DictionaryStorage Clone() {
            CommonDictionaryStorage storage = new CommonDictionaryStorage();
            foreach (KeyValuePair<object, object> kvp in GetItems()) {
                storage.Add(kvp.Key, kvp.Value);
            }
            return storage;
        }
    }

}
