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

using System.Scripting.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// Used by BreakStatement and ContinueStatement to specify the target of
    /// the break/continue. The label object is shared with the enclosing
    /// LabeledStatement, LoopStatement, SwitchStatement, or DoStatement, 
    /// indicating which statement to break/continue out of
    /// </summary>
    public sealed class LabelTarget {
        private readonly string _name;

        internal LabelTarget(string name) {
            _name = name;
        }

        /// <summary>
        /// The name of the label, possibly empty.
        /// The name is purely descriptive, so it doesn't have to be unique
        /// </summary>
        public string Name {
            get { return _name; }
        }
    }

    public partial class Expression {
        public static LabelTarget Label() {
            return new LabelTarget(null);
        }

        public static LabelTarget Label(string name) {
            return new LabelTarget(name);
        }
    }
}
