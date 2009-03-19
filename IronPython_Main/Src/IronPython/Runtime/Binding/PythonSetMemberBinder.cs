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
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;
    using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;


    class PythonSetMemberBinder : SetMemberBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;

        public PythonSetMemberBinder(BinderState/*!*/ binder, string/*!*/ name)
            : base(name, false) {
            _state = binder;
        }

        public PythonSetMemberBinder(BinderState/*!*/ binder, string/*!*/ name, bool ignoreCase)
            : base(name, ignoreCase) {
            _state = binder;
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject self, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
            if (self.NeedsDeferral()) {
                return Defer(self, value);
            }
#if !SILVERLIGHT
            DynamicMetaObject com;
            if (Microsoft.Scripting.ComBinder.TryBindSetMember(this, self, value, out com)) {
                return com;
            }
#endif
            return Binder.Binder.SetMember(Name, self, value, AstUtils.Constant(Binder.Context));
        }

        public override T BindDelegate<T>(CallSite<T> site, object[] args) {
            IPythonObject ipo = args[0] as IPythonObject;
            if (ipo != null && !(ipo is IProxyObject)) {
                FastBindResult<T> res = UserTypeOps.MakeSetBinding<T>(Binder.Context, site, ipo, args[1], this);

                if (res.Target != null) {
                    PerfTrack.NoteEvent(PerfTrack.Categories.BindingFast, "IPythonObject");

                    if (res.ShouldCache) {
                        CacheTarget(res.Target);
                    }
                    return res.Target;
                }

                PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "IPythonObject");
            }

            return base.BindDelegate(site, args);
        }

        internal Func<CallSite, object, TValue, object> OptimizeDelegate<TValue>(CallSite<Func<CallSite, object, TValue, object>> site, object self, TValue value) {
            return base.BindDelegate<Func<CallSite, object, TValue, object>>(site, new object[] { self, value });
        }

        public BinderState/*!*/ Binder {
            get {
                return _state;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonSetMemberBinder ob = obj as PythonSetMemberBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj);
        }

        public override string ToString() {
            return "Python SetMember " + Name;
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeSetAction"),
                BindingHelpers.CreateBinderStateExpression(),
                AstUtils.Constant(Name)
            );
        }

        #endregion

    }
}

