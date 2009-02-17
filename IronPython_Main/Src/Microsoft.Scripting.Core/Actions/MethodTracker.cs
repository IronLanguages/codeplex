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
using System.Text;
using System.Reflection;

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Ast;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Expression;

    public class MethodTracker : MemberTracker {
        private readonly MethodInfo _method;
        private readonly bool _isStatic;

        internal MethodTracker(MethodInfo method) {
            ContractUtils.RequiresNotNull(method, "method");
            _method = method;
            _isStatic = method.IsStatic;
        }

        internal MethodTracker(MethodInfo method, bool isStatic) {
            ContractUtils.RequiresNotNull(method, "method");
            _method = method;
            _isStatic = isStatic;
        }

        public override Type DeclaringType {
            get { return _method.DeclaringType; }
        }

        public override TrackerTypes MemberType {
            get { return TrackerTypes.Method; }
        }

        public override string Name {
            get { return _method.Name; }
        }

        public MethodInfo Method {
            get {
                return _method;
            }
        }

        public bool IsPublic {
            get {
                return _method.IsPublic;
            }
        }

        public bool IsStatic {
            get {
                return _isStatic;
            }
        }

        [Confined]
        public override string/*!*/ ToString() {
            return _method.ToString();
        }

        public override MemberTracker BindToInstance(Expression instance) {
            if (IsStatic) {
                return this;
            }

            return new BoundMemberTracker(this, instance);
        }

        protected internal override Expression GetBoundValue(Expression context, ActionBinder binder, Type type, Expression instance) {
            return binder.ReturnMemberTracker(type, BindToInstance(instance));
        }

        public override Microsoft.Scripting.Ast.Expression Call(Expression context, ActionBinder binder, params Expression[] arguments) {
            if (Method.IsPublic && Method.DeclaringType.IsVisible) {
                // TODO: Need to use MethodBinder in here to make this right.
                return binder.MakeCallExpression(context, Method, arguments);
            }

            //methodInfo.Invoke(obj, object[] params)
            if (Method.IsStatic) {
                return Ast.Convert(
                    Ast.Call(
                        Ast.RuntimeConstant(Method),
                        typeof(MethodInfo).GetMethod("Invoke", new Type[] { typeof(object), typeof(object[]) }),
                        Ast.Null(),
                        Ast.NewArrayHelper(typeof(object[]), arguments)),
                    Method.ReturnType);
            } 

            if (arguments.Length == 0) throw new InvalidOperationException("no instance for call");

            return Ast.Convert(
                Ast.Call(
                    Ast.RuntimeConstant(Method),
                    typeof(MethodInfo).GetMethod("Invoke", new Type[] { typeof(object), typeof(object[]) }),
                    arguments[0],
                    Ast.NewArrayHelper(typeof(object[]), ArrayUtils.RemoveFirst(arguments))),
                Method.ReturnType);                       
        }
    }
}
