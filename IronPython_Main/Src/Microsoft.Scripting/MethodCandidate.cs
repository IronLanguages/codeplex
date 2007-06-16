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
using System.Text;
using System.Collections.Generic;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting {
    public class MethodCandidate {
        private MethodTarget _target;
        private List<ParameterWrapper> _parameters;
        private NarrowingLevel _narrowingLevel;

        public MethodCandidate(MethodCandidate previous, NarrowingLevel narrowingLevel) {
            this._target = previous.Target;
            this._parameters = previous._parameters;
            _narrowingLevel = narrowingLevel;
        }

        public MethodCandidate(MethodTarget target, List<ParameterWrapper> parameters) {
            _target = target;
            _parameters = parameters;
            _narrowingLevel = NarrowingLevel.None;
            parameters.TrimExcess();
        }

        public MethodTarget Target {
            get { return _target; }
        }

        public bool IsApplicable(Type[] types, NarrowingLevel allowNarrowing) {
            for (int i = 0; i < types.Length; i++) {
                if (!_parameters[i].HasConversionFrom(types[i], allowNarrowing)) {
                    return false;
                }
            }
            return true;
        }

        public bool CheckArgs(CodeContext context, object[] args) {
            if (this.IsApplicable(CompilerHelpers.GetTypes(args), NarrowingLevel.None)) {
                return true;
            }

            return Target.CheckArgs(context, args);
        }

        public int? CompareParameters(MethodCandidate other) {
            return ParameterWrapper.CompareParameters(this._parameters, other._parameters);
        }

        public int CompareTo(MethodCandidate other, CallType callType) {
            int? cmpParams = CompareParameters(other);
            if (cmpParams == +1 || cmpParams == -1) return (int)cmpParams;

            int ret = Target.CompareEqualParameters(other.Target);
            if (ret != 0) return ret;

            if (CompilerHelpers.IsStatic(Target.Method) && !CompilerHelpers.IsStatic(other.Target.Method)) {
                return callType == CallType.ImplicitInstance ? -1 : +1;
            } else if (!CompilerHelpers.IsStatic(Target.Method) && CompilerHelpers.IsStatic(other.Target.Method)) {
                return callType == CallType.ImplicitInstance ? +1 : -1;
            }

            return 0;
        }

        public MethodCandidate MakeParamsExtended(ActionBinder binder, int count) {
            if (count < _parameters.Count - 1) return null;

            List<ParameterWrapper> newParameters = _parameters.GetRange(0, _parameters.Count - 1);
            Type elementType = _parameters[_parameters.Count - 1].Type.GetElementType();

            while (newParameters.Count < count) {
                ParameterWrapper param = new ParameterWrapper(binder, elementType);
                newParameters.Add(param);
            }

            return new MethodCandidate(_target.MakeParamsExtended(count), newParameters);
        }

        public override string ToString() {
            return string.Format("MethodCandidate({0})", Target);
        }

        public string ToSignatureString(string name, CallType callType) {
            StringBuilder buf = new StringBuilder(name);
            buf.Append("(");
            bool isFirstArg = true;
            int i = 0;
            if (callType == CallType.ImplicitInstance) i = 1;
            for (; i < _parameters.Count; i++) {
                if (isFirstArg) isFirstArg = false;
                else buf.Append(", ");
                buf.Append(_parameters[i].ToSignatureString());
            }
            buf.Append(")");
            return buf.ToString(); //@todo add helper info for more interesting signatures
        }

        public NarrowingLevel NarrowingLevel {
            get {
                return _narrowingLevel;
            }
        }
    }

}
