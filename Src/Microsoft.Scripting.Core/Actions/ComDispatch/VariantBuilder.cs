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

#if !SILVERLIGHT // ComObject

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.ComDispatch {
    
    using Ast = Microsoft.Scripting.Ast.Expression;

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
            MethodBinderContext context, 
            VariableExpression paramVariants, 
            int variantIndex,
            IList<Expression> parameters) {
            List<Expression> exprs = new List<Expression>();
            Expression expr;

            _variantIndex = variantIndex;
            FieldInfo variantArrayField = VariantArray.GetField(variantIndex);

            Expression argument = _builder.ToExpression(context, parameters);
            if (IsByRef) {
                // paramVariants._elementN.SetAsByrefT(ref argument)
                expr = Ast.Call(
                    Ast.ReadField(
                        Ast.ReadDefined(paramVariants),
                        variantArrayField),
                    Variant.GetByrefSetter(_targetComType & ~VarEnum.VT_BYREF),
                    argument
                );
                exprs.Add(expr);
                return exprs;
            }

            if (Variant.IsPrimitiveType(_targetComType) ||
                (_targetComType == VarEnum.VT_UNKNOWN) ||
                (_targetComType == VarEnum.VT_DISPATCH)) {
                // paramVariants._elementN.AsT = (cast)argN
                expr = Ast.AssignProperty(
                    Ast.ReadField(
                        Ast.ReadDefined(paramVariants),
                        variantArrayField),
                    Variant.GetAccessor(_targetComType),
                    argument
                );
                exprs.Add(expr);
                return exprs;
            }

            switch(_targetComType) {
                case VarEnum.VT_EMPTY:
                    return exprs;

                case VarEnum.VT_NULL:
                    // paramVariants._elementN.SetAsNULL();

                    expr = Ast.Call(
                        Ast.ReadField(
                            Ast.ReadDefined(paramVariants),
                            variantArrayField),
                        typeof(Variant).GetMethod("SetAsNULL")
                    );
                    exprs.Add(expr);
                    return exprs;

                default:
                    Debug.Assert(false, "Unexpected VarEnum");
                    return exprs;
            }
        }

        internal List<Expression> Clear(VariableExpression paramVariants) {
            List<Expression> exprs = new List<Expression>();
            Expression expr;

            if (IsByRef) {
                ComReferenceArgBuilder comReferenceArgBuilder = _builder as ComReferenceArgBuilder;
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
                    expr = Ast.Call(
                        Ast.ReadField(
                            Ast.ReadDefined(paramVariants),
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

        internal object Build(CodeContext context, object[] args) {
            object result = _builder.Build(context, args);
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
