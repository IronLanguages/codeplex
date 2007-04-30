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

namespace Microsoft.Scripting.Internal.Generation {
    public class ConstantPool {
        private List<object> _data;

        private List<Type> _types;
        private Slot _dataSlot;
        private CodeGen _cg;

        public ConstantPool() {
            this._data = new List<object>();
            this._types = new List<Type>();
        }

        public Type SlotType {
            get { return typeof(object[]); }
        }

        public object[] Data {
            get { return _data.ToArray(); }
        }

        public Slot Slot {
            get { return _dataSlot; }
        }

        public void SetCodeGen(CodeGen cg, Slot dataSlot) {
            this._cg = cg;
            this._dataSlot = dataSlot;
        }

        public Slot AddData(object data) {
            _data.Add(data);
            _types.Add(data.GetType());

            // Use a CastSlot around an IndexSlot since we just want a cast and not a full conversion
            return new CastSlot(new IndexSlot(_dataSlot, _data.Count - 1), data.GetType());
        }

        public ConstantPool CopyData() {
            ConstantPool ret = new ConstantPool();
            ret._data = this._data;
            ret._types = this._types;
            return ret;
        }


    }

}
