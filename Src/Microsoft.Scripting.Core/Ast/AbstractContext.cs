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
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// An abstraction of an evaluation CodeContext
    /// </summary>
    public class AbstractContext {
        private ActionBinder _binder;

        private Dictionary<VariableExpression, VariableExpression> _variableMap;

        public AbstractContext(ActionBinder binder) {
            _binder = binder;
        }

        public void SetupToLambda() {
            _variableMap = new Dictionary<VariableExpression, VariableExpression>();
        }

        public AbstractValue Lookup(VariableExpression variable) {
            VariableExpression newVar = _variableMap[variable];
            return AbstractValue.LimitType(newVar.Type, newVar);
        }

        public ActionBinder Binder {
            get { return _binder;  }
        }
    }
}
