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
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Utils;
using Microsoft.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Scripting.Binders {
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
                    Type ct = Expression.Type;
                    // valuetype at compile tyme, type cannot change.
                    if (ct.IsValueType) {
                        return ct;
                    }
                    if (_value != null) {
                        return _value.GetType();
                    } else {
                        return typeof(Null);    // TODO: Return something else ???
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
 || ComInterop.ComObject.IsComObject(_value)
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

        public virtual MetaObject BindConvert(ConvertBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackConvert(this);
        }

        public virtual MetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetMember(this);
        }

        public virtual MetaObject BindSetMember(SetMemberBinder binder, MetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetMember(this, value);
        }

        public virtual MetaObject BindDeleteMember(DeleteMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackDeleteMember(this);
        }

        public virtual MetaObject BindGetIndex(GetIndexBinder binder, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetIndex(this, args);
        }

        public virtual MetaObject BindSetIndex(SetIndexBinder binder, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetIndex(this, args);
        }

        public virtual MetaObject BindDeleteIndex(DeleteIndexBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackDeleteIndex(this, args);
        }

        public virtual MetaObject BindInvokeMember(InvokeMemberBinder binder, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvokeMember(this, args);
        }

        public virtual MetaObject BindInvoke(InvokeBinder binder, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvoke(this, args);
        }

        public virtual MetaObject BindCreateInstance(CreateInstanceBinder binder, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackCreateInstance(this, args);
        }

        public virtual MetaObject BindUnaryOperation(UnaryOperationBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackUnaryOperation(this);
        }

        public virtual MetaObject BindBinaryOperation(BinaryOperationBinder binder, MetaObject arg) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackBinaryOperation(this, arg);
        }

        /// <summary>
        /// Binds an operation a.b (op)= c
        /// </summary>
        /// <param name="binder">Binder implementing the language semantics.</param>
        /// <param name="value">Meta Object representing the right-side argument.</param>
        /// <returns>MetaObject representing the result of the binding.</returns>
        public virtual MetaObject BindOperationOnMember(OperationOnMemberBinder binder, MetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackOperationOnMember(this, value);
        }

        /// <summary>
        /// Binds an operation a[i,j,k] (op)= c
        /// </summary>
        /// <param name="binder">Binder implementing the language semantics.</param>
        /// <param name="args">MetaObjects representing the indexes and the right-hand value.</param>
        /// <returns>MetaObject representing the result of the binding.</returns>
        public virtual MetaObject BindOperationOnIndex(OperationOnIndexBinder binder, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackOperationOnIndex(this, args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IEnumerable<string> GetDynamicMemberNames() {
            return EmptyArray<string>.Instance;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual IEnumerable<KeyValuePair<string, object>> GetDynamicDataMembers() {
            return EmptyArray<KeyValuePair<string, object>>.Instance;
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
            } else if (ComObject.IsComObject(argValue)) {
                return ComMetaObject.GetComMetaObject(parameterExpression, argValue);
#endif
            } else {
                return new ParameterMetaObject(parameterExpression, argValue);
            }
        }

        public static MetaObject CreateThrow(MetaObject target, MetaObject[] args, Type exception, params object[] exceptionArgs) {
            return CreateThrow(
                target,
                args,
                exception,
                exceptionArgs != null ? exceptionArgs.Map<object, Expression>((arg) => Expression.Constant(arg)) : null
            );
        }

        public static MetaObject CreateThrow(MetaObject target, MetaObject[] args, Type exception, params Expression[] exceptionArgs) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.RequiresNotNull(exception, "exception");

            Type[] argTypes = exceptionArgs != null ? exceptionArgs.Map((arg) => arg.Type) : Type.EmptyTypes;
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