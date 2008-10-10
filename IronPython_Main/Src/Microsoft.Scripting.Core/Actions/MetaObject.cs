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
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Com;
using System.Reflection;

namespace Microsoft.Scripting.Actions {
    public class MetaObject {
        private readonly Expression _expression;
        private readonly Restrictions _restrictions;
        private readonly object _value;
        private readonly bool _hasValue;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly MetaObject[] EmptyMetaObjects = new MetaObject[0];

        public MetaObject(Expression expression, Restrictions restrictions) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(restrictions, "restrictions");

            _expression = expression;
            _restrictions = restrictions;
        }

        public MetaObject(Expression expression, Restrictions restrictions, object value)
            : this(expression, restrictions) {
            _value = value;
            _hasValue = true;
        }

        public Expression Expression {
            get {
                return _expression;
            }
        }

        public Restrictions Restrictions {
            get {
                return _restrictions;
            }
        }

        public object Value {
            get {
                return _value;
            }
        }

        public bool HasValue {
            get {
                return _hasValue;
            }
        }

        public Type RuntimeType {
            get {
                if (_hasValue) {
                    if (_value != null) {
                        return _value.GetType();
                    } else {
                        return typeof(None);    // TODO: Return something else ???
                    }
                } else {
                    return null;                // TODO: Return something else ???
                }
            }
        }
        
        public Type LimitType {
            get {
                return RuntimeType ?? Expression.Type;
            }
        }

        public bool IsDynamicObject {
            get {
                // We can skip _hasValue check as it implies _value == null
                return _value is IDynamicObject 
#if !SILVERLIGHT
                    || Com.ComMetaObject.IsComObject(_value)
#endif
                ;
            }
        }

        public bool IsByRef {
            get {
                ParameterExpression pe = _expression as ParameterExpression;
                return pe != null && pe.IsByRef;
            }
        }

        public virtual MetaObject Convert(ConvertAction action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(this);
        }

        public virtual MetaObject GetMember(GetMemberAction action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(this);
        }

        public virtual MetaObject SetMember(SetMemberAction action, MetaObject value) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(this, value);
        }

        public virtual MetaObject DeleteMember(DeleteMemberAction action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(this);
        }

        public virtual MetaObject GetIndex(GetIndexAction action, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(this, args);
        }

        public virtual MetaObject SetIndex(SetIndexAction action, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(this, args);
        }

        public virtual MetaObject DeleteIndex(DeleteIndexAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(this, args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Call")]
        public virtual MetaObject Call(CallAction action, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(this, args);
        }

        public virtual MetaObject Invoke(InvokeAction action, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(this, args);
        }

        public virtual MetaObject Create(CreateAction action, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(this, args);
        }

        public virtual MetaObject Operation(OperationAction action, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(this, args);
        }

        // Internal helpers

        internal static Type[] GetTypes(MetaObject[] objects) {
            Type[] res = new Type[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                res[i] = objects[i].RuntimeType ?? objects[i].Expression.Type;
            }

            return res;
        }

        public static Expression[] GetExpressions(MetaObject[] objects) {
            ContractUtils.RequiresNotNull(objects, "objects");

            Expression[] res = new Expression[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                MetaObject mo = objects[i];
                ContractUtils.RequiresNotNull(mo, "objects");
                Expression expr = mo.Expression;
                ContractUtils.RequiresNotNull(expr, "objects");
                res[i] = expr;
            }

            return res;
        }

        public static MetaObject ObjectToMetaObject(object argValue, Expression parameterExpression) {
            IDynamicObject ido = argValue as IDynamicObject;
            if (ido != null) {
                return ido.GetMetaObject(parameterExpression);
#if !SILVERLIGHT
            } else if (ComMetaObject.IsComObject(argValue)) {
                return ComMetaObject.GetComMetaObject(parameterExpression, argValue);
#endif
            } else {
                return new ParameterMetaObject(parameterExpression, argValue);
            }
        }

        public static MetaObject Throw(MetaObject target, MetaObject[] args, Type exception, params object[] exceptionArgs) {
            ContractUtils.RequiresNotNull(exceptionArgs, "exceptionArgs");

            return Throw(
                target,
                args,
                exception,
                exceptionArgs.Map<object, Expression>((arg) => Expression.Constant(arg))
            );
        }

        public static MetaObject Throw(MetaObject target, MetaObject[] args, Type exception, params Expression[] exceptionArgs) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.RequiresNotNull(exception, "exception");
            ContractUtils.RequiresNotNull(exceptionArgs, "exceptionArgs");

            Type[] argTypes = exceptionArgs.Map((arg) => arg.Type);
            ConstructorInfo constructor = exception.GetConstructor(argTypes);

            if (constructor == null) {
                throw new ArgumentException(Strings.TypeDoesNotHaveConstructorForTheSignature);
            }

            return new MetaObject(
                Expression.Throw(
                    Expression.New(
                        exception.GetConstructor(argTypes),
                        exceptionArgs
                    )
                ),
                target.Restrictions.Merge(Restrictions.Combine(args))
            );
        }
    }
}
