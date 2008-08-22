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
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// Updates fields/properties of the returned value with unused keyword parameters.
    /// </summary>
    class KeywordConstructorReturnBuilder : ReturnBuilder {
        private readonly ReturnBuilder _builder;
        private readonly int _kwArgCount;
        private readonly int[] _indexesUsed;
        private readonly MemberInfo[] _membersSet;
        private readonly bool _privateBinding;

        public KeywordConstructorReturnBuilder(ReturnBuilder builder, int kwArgCount, int[] indexesUsed, MemberInfo[] membersSet,
            bool privateBinding)
            : base(builder.ReturnType) {
            _builder = builder;
            _kwArgCount = kwArgCount;
            _indexesUsed = indexesUsed;
            _membersSet = membersSet;
            _privateBinding = privateBinding;
        }

        public override object Build(CodeContext context, object[] args, object[] parameters, object ret) {
            for (int i = 0; i < _indexesUsed.Length; i++) {
                object value = parameters[parameters.Length - _kwArgCount + _indexesUsed[i]];
                switch(_membersSet[i].MemberType) {
                    case MemberTypes.Field:
                        ((FieldInfo)_membersSet[i]).SetValue(ret, value);
                        break;
                    case MemberTypes.Property:
                        ((PropertyInfo)_membersSet[i]).SetValue(ret, value, ArrayUtils.EmptyObjects);
                        break;
                }
            }

            return _builder.Build(context, args, parameters, ret);
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<ArgBuilder> args, IList<Expression> parameters, Expression ret) {
            List<Expression> sets = new List<Expression>();

            VariableExpression tmp = context.GetTemporary(ret.Type, "val");
            sets.Add(
                Ast.Assign(tmp, ret)
            );

            for (int i = 0; i < _indexesUsed.Length; i++) {
                Expression value = parameters[parameters.Count - _kwArgCount + _indexesUsed[i]];
                switch(_membersSet[i].MemberType) {
                    case MemberTypes.Field:
                        FieldInfo fi = (FieldInfo)_membersSet[i];
                        if (!fi.IsLiteral && !fi.IsInitOnly) {
                            sets.Add(
                                Ast.AssignField(
                                    tmp,
                                    fi,
                                    ConvertToHelper(context, value, fi.FieldType)
                                )
                            );
                        } else {
                            // call a helper which throws the error but "returns object"
                            sets.Add(
                                Ast.Convert(
                                    Ast.Call(
                                        typeof(RuntimeHelpers).GetMethod("ReadOnlyAssignError"),
                                        Ast.Constant(true),
                                        Ast.Constant(fi.Name)
                                    ),
                                    fi.FieldType
                                )
                            );
                        }                        
                        break;

                    case MemberTypes.Property:
                        PropertyInfo pi = (PropertyInfo)_membersSet[i];
                        if (pi.GetSetMethod(_privateBinding) != null) {
                            sets.Add(
                                Ast.AssignProperty(
                                    tmp,
                                    pi,
                                    ConvertToHelper(context, value, pi.PropertyType)
                                )
                            );
                        } else {
                            // call a helper which throws the error but "returns object"
                            sets.Add(
                                Ast.Convert(
                                    Ast.Call(
                                        typeof(RuntimeHelpers).GetMethod("ReadOnlyAssignError"),
                                        Ast.Constant(false),
                                        Ast.Constant(pi.Name)
                                    ),
                                    pi.PropertyType
                                )
                            );
                        }
                        break;
                }
            }

            sets.Add(
                tmp
            );

            Expression newCall = Ast.Comma(
                sets.ToArray()
            );

            return _builder.ToExpression(context, args, parameters, newCall);
        }

        private static Expression ConvertToHelper(MethodBinderContext context, Expression value, Type type) {
            if (type == value.Type) {
                return value;
            }

            if (type.IsAssignableFrom(value.Type)) {
                return Ast.ConvertHelper(value, type);
            }

            return Expression.Dynamic(OldConvertToAction.Make(context.Binder, type), type, context.ContextExpression, value);
        }
    }
}
