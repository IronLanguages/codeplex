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
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;

    class PythonOperationBinder : OperationBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;

        public PythonOperationBinder(BinderState/*!*/ state, string/*!*/ operation)
            : base(operation) {
            _state = state;
        }

        public override MetaObject/*!*/ FallbackOperation(MetaObject target, MetaObject/*!*/[]/*!*/ args, MetaObject onBindingError) {
            // TODO: until we use the real GetIndex and SetIndex binders, we
            // need to do this for COM interop
            if (Operation == "GetItem") {
                return target.BindGetIndex(new GetIndexAdapter(this), args);
            }            
            if (Operation == "SetItem") {
                MetaObject[] indexes = ArrayUtils.RemoveLast(args);
                MetaObject value = args[args.Length - 1];
                return target.BindSetIndex(new SetIndexAdapter(this), indexes, value);
            }
            return PythonProtocol.Operation(this, ArrayUtils.Insert(target, args));
        }

        public override object CacheIdentity {
            get { return this; }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonOperationBinder ob = obj as PythonOperationBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj);
        }

        public BinderState/*!*/ Binder {
            get {
                return _state;
            }
        }

        public override string ToString() {
            return "Python " + Operation;
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeOperationAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Expression.Constant(Operation)
            );
        }

        #endregion

        #region GetIndex/SetIndex adapters

        // TODO: remove when Python uses the SetIndexBinder for real
        class SetIndexAdapter : SetIndexBinder {
            private readonly PythonOperationBinder _opBinder;

            internal SetIndexAdapter(PythonOperationBinder opBinder) {
                _opBinder = opBinder;
            }

            public override MetaObject FallbackSetIndex(MetaObject target, MetaObject[] indexes, MetaObject value, MetaObject errorSuggestion) {
#if !SILVERLIGHT
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindSetIndex(this, ref target, indexes, value)) {
                    return target;
                }
#endif                
                return PythonProtocol.Operation(_opBinder, ArrayUtils.Append(ArrayUtils.Insert(target, indexes), value));
            }

            public override object CacheIdentity {
                get { return _opBinder; }
            }
        }

        // TODO: remove when Python uses the GetIndexBinder for real
        class GetIndexAdapter : GetIndexBinder {
            private readonly PythonOperationBinder _opBinder;

            internal GetIndexAdapter(PythonOperationBinder opBinder) {
                _opBinder = opBinder;
            }

            public override MetaObject FallbackGetIndex(MetaObject target, MetaObject[] indexes, MetaObject errorSuggestion) {
#if !SILVERLIGHT
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindGetIndex(this, ref target, indexes)) {
                    return target;
                }
#endif
                return PythonProtocol.Operation(_opBinder, ArrayUtils.Insert(target, indexes));
            }

            public override object CacheIdentity {
                get { return _opBinder; }
            }
        }

        #endregion
    }
}
