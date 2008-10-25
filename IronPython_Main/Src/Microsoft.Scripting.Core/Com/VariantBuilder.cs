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

namespace Microsoft.Scripting.ComInterop {

    /// <summary>
    /// VariantBuilder handles packaging of arguments into a Variant for a call to IDispatch.Invoke
    /// </summary>
    internal class VariantBuilder {

        private readonly ArgBuilder _argBuilder;
        private readonly VarEnum _targetComType;
        internal ParameterExpression TempVariable { get; private set; }

        internal VariantBuilder(VarEnum targetComType, ArgBuilder builder) {
            _targetComType = targetComType;
            _argBuilder = builder;
        }

        internal bool IsByRef {
            get { return (_targetComType & VarEnum.VT_BYREF) != 0; }
        }

        internal Expression WriteArgumentVariant(MemberExpression variant, Expression parameter) {
            if (IsByRef) {
                // temp = argument
                // paramVariants._elementN.SetAsByrefT(ref temp)
                Debug.Assert(TempVariable == null);
                var argExpr = _argBuilder.MarshalToRef(parameter);

                TempVariable = Expression.Variable(argExpr.Type, null);
                return Expression.Block(
                    Expression.Assign(TempVariable, argExpr),
                    Expression.Call(
                        variant,
                        Variant.GetByrefSetter(_targetComType & ~VarEnum.VT_BYREF),
                        TempVariable
                    )
                );
            }

            Expression argument = _argBuilder.Marshal(parameter);

            //TODO: we need to make this cleaner.
            // it is not nice that we need a special case for IConvertible.
            // we are forced to special case it now since it does not have 
            // a corresponding _targetComType.
            if (_argBuilder is ConvertibleArgBuilder) {
                return Expression.Call(
                    variant,
                    typeof(Variant).GetMethod("SetAsIConvertible"),
                    argument
                );
            }

            if (Variant.IsPrimitiveType(_targetComType) ||
                (_targetComType == VarEnum.VT_UNKNOWN) ||
                (_targetComType == VarEnum.VT_DISPATCH)) {
                // paramVariants._elementN.AsT = (cast)argN
                return Expression.AssignProperty(
                    variant,
                    Variant.GetAccessor(_targetComType),
                    argument
                );
            }

            switch (_targetComType) {
                case VarEnum.VT_EMPTY:
                    return null;

                case VarEnum.VT_NULL:
                    // paramVariants._elementN.SetAsNull();
                    return Expression.Call(variant, typeof(Variant).GetMethod("SetAsNull"));

                default:
                    Debug.Assert(false, "Unexpected VarEnum");
                    return null;
            }
        }

        internal Expression Clear(MemberExpression variant) {
            if (IsByRef) {
                if (_argBuilder is StringArgBuilder) {
                    Debug.Assert(TempVariable != null);
                    return Expression.Call(typeof(Marshal).GetMethod("FreeBSTR"), TempVariable);
                }
                return null;
            }

            switch (_targetComType) {
                case VarEnum.VT_EMPTY:
                case VarEnum.VT_NULL:
                    return null;

                case VarEnum.VT_BSTR:
                case VarEnum.VT_UNKNOWN:
                case VarEnum.VT_DISPATCH:
                    // paramVariants._elementN.Clear()
                    return Expression.Call(variant, typeof(Variant).GetMethod("Clear"));

                default:
                    Debug.Assert(Variant.IsPrimitiveType(_targetComType), "Unexpected VarEnum");
                    return null;
            }
        }

        internal object Build(object arg) {
            object result = _argBuilder.UnwrapForReflection(arg);
            if (_targetComType == VarEnum.VT_DISPATCH) {
                // Ensure that the object supports IDispatch to match how WriteArgumentVariant would work
                // (it would call the Variant.AsDispatch setter). Otherwise, Type.InvokeMember might decide
                // to marshal it as IUknown which would result in a difference in behavior.
                IntPtr dispatch = Marshal.GetIDispatchForObject(result);
                Marshal.Release(dispatch);
            }

            return result;
        }

        internal Expression UpdateFromReturn(Expression parameter) {
            if (TempVariable == null) {
                return null;
            }
            return _argBuilder.UpdateFromReturn(parameter, TempVariable);
        }

        internal void UpdateFromReturn(object originalArg, object updatedArg) {
            _argBuilder.UpdateFromReturn(originalArg, updatedArg);
        }
    }
}

#endif
