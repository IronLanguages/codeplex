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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Scripting.Utils;

namespace System.Linq.Expressions {

    // Suppose we have something like:
    //
    //    (string s)=>()=>s.
    //
    // We wish to generate the outer as:
    // 
    //      Func<string> OuterMethod(Closure closure, string s)
    //      {
    //          object[] locals = new object[1];
    //          locals[0] = s;
    //          return ((DynamicMethod)closure.Constants[0]).CreateDelegate(typeof(Func<string>), new Closure(null, locals));
    //      }
    //      
    // ... and the inner as:
    // 
    //      string InnerMethod(Closure closure)
    //      {
    //          object[] locals = closure.Locals;
    //          return (string)locals[0];
    //      }
    //
    // This class tracks that "s" was hoisted into a closure, as the 0th
    // element in the array
    //
    /// <summary>
    /// Stores information about locals and arguments that are hoisted into
    /// the closure array because they're referenced in an inner lambda.
    /// 
    /// This class is sometimes emitted as a runtime constant for internal
    /// use to hoist variables/parameters in quoted expressions
    /// 
    /// Invariant: this class stores no mutable state
    /// </summary>
    internal sealed class HoistedLocals {

        // The parent locals, if any
        private readonly HoistedLocals _parent;

        // A mapping of hoisted variables to their indexes in the array
        private readonly ReadOnlyDictionary<Expression, int> _indexes;

        // The variables, in the order they appear in the array
        private readonly ReadOnlyCollection<Expression> _vars;

        internal HoistedLocals(HoistedLocals parent, ReadOnlyCollection<Expression> vars) {
            if (parent != null) {
                // Add the parent locals array as the 0th element in the array
                Expression parentVar = Expression.Variable(typeof(object[]), "$parentEnv");
                vars = new ReadOnlyCollection<Expression>(ArrayUtils.Insert(parentVar, vars));
            }

            Dictionary<Expression, int> indexes = new Dictionary<Expression, int>(vars.Count);
            for (int i = 0; i < vars.Count; i++) {
                indexes.Add(vars[i], i);
            }

            _vars = vars;
            _indexes = new ReadOnlyDictionary<Expression,int>(indexes);
            _parent = parent;
        }

        internal HoistedLocals Parent {
            get { return _parent; }
        }

        internal Expression ParentVariable {
            get { return _parent != null ? _vars[0] : null; }
        }

        internal ReadOnlyCollection<Expression> Variables {
            get { return _vars; }
        }

        internal ReadOnlyDictionary<Expression, int> Indexes {
            get { return _indexes; }
        }

        internal static object[] GetParent(object[] locals) {
            return (object[])locals[0];
        }
    }
}
