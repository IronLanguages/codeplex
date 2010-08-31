/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;

namespace TestAst.Runtime {
    //
    // The delegate type for the test functions
    //
    public delegate object TestCallTarget();

    public class DefaultFunction {
        public static DefaultFunction Create(string name, string[] parameterNames, TestCallTarget target) {
            return new DefaultFunction(name, parameterNames, target);
        }

        private readonly string _name;
        private readonly string[] _parameterNames;
        private readonly TestCallTarget _target;

        private DefaultFunction(string name, string[] parameterNames, TestCallTarget target) {
            _name = name;
            _parameterNames = parameterNames;
            _target = target;
        }

        public string Name {
            get { return _name; }
        }

        public Delegate Target {
            get { return _target; }
        }

        [SpecialName]
        public object Call(params object[] arguments) {
            if (arguments != null && arguments.Length > 0) {
                throw new InvalidOperationException();
            }
            return _target();
        }

        public override string ToString() {
            return string.Format("DefaultFunction({0})", _name);
        }
    }
}
