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
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    [PythonSystemType("builtin_function_or_method")]
    public class ConstructorFunction : BuiltinFunction {
        private MethodBase[] _ctors;

        internal ConstructorFunction(BuiltinFunction realTarget, IList<MethodBase> constructors)
            : base(GetTargetsValidateFunction(realTarget)) {

            base.Name = realTarget.Name;
            base.FunctionType = realTarget.FunctionType;
            this._ctors = ArrayUtils.ToArray(constructors);
        }

        internal IList<MethodBase> ConstructorTargets {
            get {
                return _ctors;
            }
        }

        public override BuiltinFunctionOverloadMapper Overloads {
            get {
                return new ConstructorOverloadMapper(this, null);
            }
        }

        private static IList<MethodBase> GetTargetsValidateFunction(BuiltinFunction realTarget) {
            Contract.RequiresNotNull(realTarget, "realTarget");
            return realTarget.Targets;
        }

        public new string __name__ {
            get {
                return "__new__";
            }
        }

        public override string __doc__ {
            get {
                StringBuilder sb = new StringBuilder();
                IList<MethodBase> targets = ConstructorTargets;

                foreach (MethodBase mb in ConstructorTargets) {
                    if (mb != null) sb.AppendLine(DocBuilder.DocOneInfo(mb, "__new__"));
                }
                return sb.ToString();
            }
        }


    }
}