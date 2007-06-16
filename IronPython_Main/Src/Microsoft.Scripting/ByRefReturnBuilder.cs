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

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting {
    public class ByRefReturnBuilder : ReturnBuilder {
        private IList<int> _returnArgs;
        private ActionBinder _binder;

        public ByRefReturnBuilder(ActionBinder binder, Type returnType, IList<int> returnArgs)
            : base(returnType) {
            _returnArgs = returnArgs;
            _binder = binder;
        }

        private static object GetValue(object[] args, object ret, int index) {
            if (index == -1) return ConvertToObject(ret);
            return ConvertToObject(args[index]);
        }

        public override object Build(CodeContext context, object[] args, object ret) {
            if (_returnArgs.Count == 1) {
                return GetValue(args, ret, _returnArgs[0]);
            } else {
                object[] retValues = new object[_returnArgs.Count];
                int rIndex = 0;
                foreach (int index in _returnArgs) {
                    retValues[rIndex++] = GetValue(args, ret, index);
                }
                return _binder.GetByRefArray(retValues);
            }
        }

        public override int CountOutParams {
            get { return _returnArgs.Count; }
        }

        public override bool CanGenerate {
            get {
                return false;
            }
        }

        public override void Generate(CodeGen cg, IList<Slot> argSlots) {
            throw new NotImplementedException();
        }
    }
}
