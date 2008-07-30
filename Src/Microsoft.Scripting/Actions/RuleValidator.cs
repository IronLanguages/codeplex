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
using System.Collections.Generic;

namespace Microsoft.Scripting.Actions {
    internal sealed class RuleValidator {
        private readonly Func<bool>[] _validators;

        private RuleValidator(Func<bool>[] validators) {
            _validators = validators;
        }

        private bool Valid() {
            foreach (Func<bool> f in _validators) {
                if (!f()) {
                    return false;
                }
            }
            return true;
        }

        internal static Func<bool> Create(List<Func<bool>> validators) {
            if (validators == null || validators.Count == 0) {
                return null;
            } else if (validators.Count == 1) {
                return validators[0];
            } else {
                return new RuleValidator(validators.ToArray()).Valid;
            }
        }
    }
}
