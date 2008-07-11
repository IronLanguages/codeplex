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

using System.Diagnostics;
using System.Reflection.Emit;

namespace System.Linq.Expressions {
    internal sealed class TargetLabel {
        private Label _label;
        private bool _initialized;

        internal TargetLabel() {
        }

        internal Label EnsureLabel(LambdaCompiler cg) {
            if (!_initialized) {
                _label = cg.IL.DefineLabel();
                _initialized = true;
            }
            return _label;
        }

        internal void Clear() {
            _initialized = false;
        }
    }

    internal sealed class YieldTarget {
        private readonly int _index;
        private readonly TargetLabel _target;

        internal YieldTarget(int index, TargetLabel label) {
            Debug.Assert(label != null);
            _index = index;
            _target = label;
        }

        internal int Index {
            get { return _index; }
        }

        internal Label EnsureLabel(LambdaCompiler cg) {
            return _target.EnsureLabel(cg);
        }

        internal void Clear() {
            _target.Clear();
        }
    }
}
