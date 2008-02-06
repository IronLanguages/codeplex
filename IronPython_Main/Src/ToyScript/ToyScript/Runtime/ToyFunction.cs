/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Runtime.CompilerServices;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace ToyScript.Runtime {
    public class ToyFunction {
        public static ToyFunction Create(string name, string[] parameterNames, Delegate target) {
            return new ToyFunction(name, parameterNames, target);
        }

        private readonly string _name;
        private readonly string[] _parameterNames;
        private readonly Delegate _target;

        private ToyFunction(string name, string[] parameterNames, Delegate target) {
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
        public object Call(CodeContext context, params object[] arguments) {
            ParameterInfo[] parameters = _target.Method.GetParameters();
            if (parameters.Length > arguments.Length) {
                if ((parameters.Length > 0 && parameters[0].ParameterType == typeof(CodeContext)) ||
                    (_target.Target != null && _target.Method.IsStatic && parameters.Length > 1 && parameters[1].ParameterType == typeof(CodeContext))) {
                    arguments = ArrayUtils.Insert<object>(context, arguments);
                }
            }
            return ReflectionUtils.InvokeDelegate(_target, arguments);
        }

        public override string ToString() {
            return string.Format("DefaultFunction({0})", _name);
        }
    }
}
