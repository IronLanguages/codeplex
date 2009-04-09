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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions.Calls {
    public partial class OverloadResolver {
        /// <summary>
        /// Represents a collection of MethodCandidate's which all accept the
        /// same number of logical parameters.  For example a params method
        /// and a method with 3 parameters would both be a TargetSet for 3 parameters.
        /// </summary>
        internal sealed class TargetSet {
            private readonly OverloadResolver _binder;
            private readonly int _count;
            private readonly List<MethodCandidate> _candidates;

            internal TargetSet(OverloadResolver binder, int count) {
                _count = count;
                _candidates = new List<MethodCandidate>();
                _binder = binder;
            }

            internal List<MethodCandidate> Candidates {
                get { return _candidates; }
            }

            internal int Count {
                get { return _count; }
            }

            internal bool IsParamsDictionaryOnly() {
                foreach (MethodCandidate candidate in _candidates) {
                    if (!candidate.HasParamsDictionary) {
                        return false;
                    }
                }
                return true;
            }

            internal bool HasParamsArrayCandidate() {
                foreach (MethodCandidate candidate in _candidates) {
                    if (candidate.HasParamsArray) {
                        return true;
                    }
                }
                return false;
            }

            internal void Add(MethodCandidate target) {
                Debug.Assert(target.ParameterCount == _count);
                _candidates.Add(target);
            }

            [Confined]
            public override string ToString() {
                return string.Format("TargetSet({0} on {1}, nargs={2})", _candidates[0].Target.Method.Name, _candidates[0].Target.Method.DeclaringType.FullName, _count);
            }
        }
    }
}
