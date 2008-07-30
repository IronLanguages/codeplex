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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Runtime;
using System.Scripting.Actions;
using System.Text;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    class BinderState : IExpressionSerializable {
        private readonly PythonBinder/*!*/ _binder;
        private CodeContext _context;
        private static readonly BinderState Default = new BinderState(DefaultContext.DefaultPythonBinder, DefaultContext.Default);

        public BinderState(PythonBinder/*!*/ binder) {
            Debug.Assert(binder != null);

            _binder = binder;
        }

        public BinderState(PythonBinder/*!*/ binder, CodeContext context) {
            Debug.Assert(binder != null);

            _binder = binder;
            _context = context;
        }

        public CodeContext Context {
            get {
                return _context;
            }
            set {
                _context = value;
            }
        }

        public PythonBinder/*!*/ Binder {
            get {
                return _binder;
            }
        }

        public static BinderState/*!*/ GetBinderState(MetaAction/*!*/ action) {
            IPythonSite pySite = action as IPythonSite;
            if (pySite != null) {
                return pySite.Binder;
            }

            Debug.Assert(Default != null);
            return Default;
        }

        public static Expression/*!*/ GetCodeContext(MetaAction/*!*/ action) {
            return Ast.Constant(BinderState.GetBinderState(action).Context);
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Expression.Call(
                null,
                typeof(PythonOps).GetMethod("GetInitialBinderState"),
                Expression.CodeContext()
            );
        }

        #endregion
    }
}
