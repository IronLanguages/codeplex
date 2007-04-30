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

using System.Reflection;

namespace Microsoft.Scripting.Internal.Ast {
    public sealed class UnaryOperator {
        private readonly CallTarget1 _target;
        private readonly Operators _op;

        public UnaryOperator(Operators op, CallTarget1 target) {
            _op = op;            
            _target = target;
        }

        public Operators Operator {
            get {
                return _op;
            }
        }

        public CallTarget1 Target {
            get {
                return _target;
            }
        }

        public MethodInfo TargetMethod {
            get {
                return Target.Method;
            }
        }

        public int Precedence {
            get { return -1; }
        }

        public object Evaluate(object value) {
            return _target(value);
        }
    }
}
