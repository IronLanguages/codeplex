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
#if !SILVERLIGHT // ComObject

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.Com {

    /// <summary>
    /// VariantBuilder handles packaging of arguments into a Variant for a call to IDispatch.Invoke
    /// </summary>
    internal class VariantBuilder {

        private ArgBuilder _builder;
        private int _variantIndex;
        private VarEnum _targetComType;

        internal VariantBuilder(VarEnum targetComType, ArgBuilder builder) {
            _targetComType = targetComType;
            _builder = builder;
        }

        internal ArgBuilder ArgBuilder {
            get { return _builder; }
        }

        internal bool IsByRef {
            get { return (_targetComType & VarEnum.VT_BYREF) != 0; }
        }

        internal List<Expression> WriteArgumentVariant(
            ParameterExpression paramVariants, 
            int variantIndex,
            Expression parameter) {
            List<Expression> exprs = new List<Expression>();
            Expression expr;

            _variantIndex = variantIndex;
            FieldInfo variantArrayField = VariantArray.GetField(variantIndex);

            if (IsByRef) {
                // paramVariants._elementN.SetAsByrefT(ref argument)
                expr = Expression.Call(
                    Expression.Field(
                        paramVariants,
                        variantArrayField),
                    Variant.GetByrefSetter(_targetComType & ~VarEnum.VT_BYREF),
                    _builder.UnwrapByRef(parameter)
                );
                exprs.Add(expr);
                return exprs;
            }

            Expression argument = _builder.Unwrap(parameter);
            if (Variant.IsPrimitiveType(_targetComType) ||
                (_targetComType == VarEnum.VT_UNKNOWN) ||
                (_targetComType == VarEnum.VT_DISPATCH)) {
                // paramVariants._elementN.AsT = (cast)argN
                expr = Expression.AssignProperty(
                    Expression.Field(
                        paramVariants,
                        variantArrayField),
                    Variant.GetAccessor(_targetComType),
                    argument
                );
                exprs.Add(expr);
                return exprs;
            }

            switch (_targetComType) {
                case VarEnum.VT_EMPTY:
                    return exprs;

                case VarEnum.VT_NULL:
                    // paramVariants._elementN.SetAsNull();

                    expr = Expression.Call(
                        Expression.Field(
                            paramVariants,
                            variantArrayField),
                        typeof(Variant).GetMethod("SetAsNull")
                    );
                    exprs.Add(expr);
                    return exprs;

                default:
                    Debug.Assert(false, "Unexpected VarEnum");
                    return exprs;
            }
        }

        internal List<Expression> Clear(ParameterExpression paramVariants) {
            List<Expression> exprs = new List<Expression>();
            Expression expr;

            if (IsByRef) {
                StringArgBuilder comReferenceArgBuilder = _builder as StringArgBuilder;
                if (comReferenceArgBuilder != null) {
                    return comReferenceArgBuilder.Clear();
                }
                return exprs;
            }

            FieldInfo variantArrayField = VariantArray.GetField(_variantIndex);

            switch (_targetComType) {
                case VarEnum.VT_EMPTY:
                case VarEnum.VT_NULL:
                    return exprs;

                case VarEnum.VT_BSTR:
                case VarEnum.VT_UNKNOWN:
                case VarEnum.VT_DISPATCH:
                    // paramVariants._elementN.Clear()
                    expr = Expression.Call(
                        Expression.Field(
                            paramVariants,
                            variantArrayField),
                        typeof(Variant).GetMethod("Clear")
                    );
                    exprs.Add(expr);
                    return exprs;

                default:
                    if (Variant.IsPrimitiveType(_targetComType)) {
                        return exprs;
                    }
                    Debug.Assert(false, "Unexpected VarEnum");
                    return exprs;
            }
        }

        internal object Build(object arg) {
            object result = _builder.UnwrapForReflection(arg);
            if (_targetComType == VarEnum.VT_DISPATCH) {
                // Ensure that the object supports IDispatch to match how WriteArgumentVariant would work
                // (it would call the Variant.AsDispatch setter). Otherwise, Type.InvokeMember might decide
                // to marshal it as IUknown which would result in a difference in behavior.
                IntPtr dispatch = Marshal.GetIDispatchForObject(result);
                Marshal.Release(dispatch);
            }

            return result;
        }
    }
}

#endif