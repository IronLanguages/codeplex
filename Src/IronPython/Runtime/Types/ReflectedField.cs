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
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;


using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Binding;

using Ast = Microsoft.Linq.Expressions.Expression;

namespace IronPython.Runtime.Types {
    [PythonType("field#")]
    public sealed class ReflectedField : PythonTypeSlot, ICodeFormattable {
        private readonly NameType _nameType;
        internal readonly FieldInfo/*!*/ info;

        public ReflectedField(FieldInfo/*!*/ info, NameType nameType) {
            Debug.Assert(info != null);

            this._nameType = nameType;
            this.info = info;
        }

        public ReflectedField(FieldInfo/*!*/ info)
            : this(info, NameType.PythonField) {
        }

        #region Public Python APIs

        /// <summary>
        /// Convenience function for users to call directly
        /// </summary>
        public object GetValue(CodeContext context, object instance) {
            object value;
            if (TryGetValue(context, instance, DynamicHelpers.GetPythonType(instance), out value)) {
                return value;
            }
            throw new InvalidOperationException("cannot get field");
        }

        /// <summary>
        /// Convenience function for users to call directly
        /// </summary>
        public void SetValue(CodeContext context, object instance, object value) {
            if (!TrySetValue(context, instance, DynamicHelpers.GetPythonType(instance), value)) {
                throw new InvalidOperationException("cannot set field");
            }            
        }

        public void __set__(object instance, object value) {
            if (instance == null && info.IsStatic) {
                DoSet(null, value);
            } else if (!info.IsStatic) {
                DoSet(instance, value);
            } else {
                throw PythonOps.AttributeErrorForReadonlyAttribute(info.DeclaringType.Name, SymbolTable.StringToId(info.Name));
            }
        }

        [SpecialName]
        public void __delete__(object instance) {
            throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(info.DeclaringType.Name, SymbolTable.StringToId(info.Name));
        }

        public string __doc__ {
            get {
                return DocBuilder.DocOneInfo(info);
            }
        }

        public PythonType FieldType {
            [PythonHidden]
            get {
                return DynamicHelpers.GetPythonTypeFromType(info.FieldType);
            }
        }

        #endregion

        #region Internal APIs

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance == null) {
                if (info.IsStatic) {
                    value = info.GetValue(null);
                } else {
                    value = this;
                }
            } else {
                value = info.GetValue(context.LanguageContext.Binder.Convert(instance, info.DeclaringType));
            }

            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            if (ShouldSetOrDelete(owner)) {
                DoSet(context, instance, value);
                return true;
            }

            return false;
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            // field is settable if it is not readonly
            return (info.Attributes & FieldAttributes.InitOnly) == 0 && !info.IsLiteral;
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            if (ShouldSetOrDelete(owner)) {
                throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(info.DeclaringType.Name, SymbolTable.StringToId(info.Name));
            }
            return false;
        }

        internal override bool IsAlwaysVisible {
            get {
                return _nameType == NameType.PythonField;
            }
        }

        internal override Expression/*!*/ MakeGetExpression(PythonBinder/*!*/ binder, Expression/*!*/ codeContext, Expression instance, Expression/*!*/ owner, Expression/*!*/ error) {
            if (!info.IsPublic || info.DeclaringType.ContainsGenericParameters) {
                // fallback to reflection
                return base.MakeGetExpression(binder, codeContext, instance, owner, error);
            }
            
            if (instance == null) {
                if (info.IsStatic) {
                    return Ast.Field(null, info);
                } else {
                    return Ast.Constant(this);
                }
            } else {
                return Ast.Field(
                    binder.ConvertExpression(
                        instance,
                        info.DeclaringType,
                        ConversionResultKind.ExplicitCast,
                        codeContext
                    ),
                    info
                );
            }
        }

        #endregion

        #region Private helpers

        private void DoSet(CodeContext context, object instance, object val) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance != null && instance.GetType().IsValueType)
                throw new ArgumentException(String.Format("Attempt to update field '{0}' on value type '{1}'; value type fields cannot be directly modified", info.Name, info.DeclaringType.Name));
            if (info.IsInitOnly || info.IsLiteral)
                throw new MissingFieldException(String.Format("Cannot set field {1} on type {0}", info.DeclaringType.Name, SymbolTable.StringToId(info.Name)));

            info.SetValue(instance, context.LanguageContext.Binder.Convert(val, info.FieldType));
        }

        private void DoSet(object instance, object val) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance != null && instance.GetType().IsValueType)
                throw PythonOps.ValueError("Attempt to update field '{0}' on value type '{1}'; value type fields cannot be directly modified", info.Name, info.DeclaringType.Name);
            if (info.IsInitOnly || info.IsLiteral)
                throw PythonOps.AttributeErrorForReadonlyAttribute(info.DeclaringType.Name, SymbolTable.StringToId(info.Name));

            info.SetValue(instance, Converter.Convert(val, info.FieldType));
        }

        private bool ShouldSetOrDelete(PythonType type) {
            PythonType dt = type as PythonType;

            // statics must be assigned through their type, not a derived type.  Non-statics can
            // be assigned through their instances.
            return (dt != null && info.DeclaringType == dt.UnderlyingSystemType) || !info.IsStatic || info.IsLiteral || info.IsInitOnly;
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return string.Format("<field# {0} on {1}>", info.Name, info.DeclaringType.Name);
        }

        #endregion
    }
}
