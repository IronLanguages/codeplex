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
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

using IronPython.Runtime.Types;
    
namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;

    partial class MetaPythonType : MetaPythonObject, IPythonConvertible {
        public MetaPythonType(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, PythonType/*!*/ value)
            : base(expression, BindingRestrictions.Empty, value) {
            Assert.NotNull(value);
        }

        public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder create, params DynamicMetaObject[] args) {
            return InvokeWorker(create, args, AstUtils.Constant(PythonContext.GetPythonContext(create).SharedContext));
        }

        public override DynamicMetaObject BindConvert(ConvertBinder/*!*/ conversion) {
            return ConvertWorker(conversion, conversion.Type, conversion.Explicit ? ConversionResultKind.ExplicitCast : ConversionResultKind.ImplicitCast);
        }

        public DynamicMetaObject BindConvert(PythonConversionBinder binder) {
            return ConvertWorker(binder, binder.Type, binder.ResultKind);
        }

        public DynamicMetaObject ConvertWorker(DynamicMetaObjectBinder binder, Type type, ConversionResultKind kind) {
            if (type.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateTarget(binder, type, Restrict(Value.GetType()));
            }
            return FallbackConvert(binder);
        }

        public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames() {
            PythonContext pc = Value.PythonContext ?? DefaultContext.DefaultPythonContext;

            foreach (object o in Value.GetMemberNames(pc.SharedContext)) {
                if (o is string) {
                    yield return (string)o;
                }
            }
        }

        public new PythonType/*!*/ Value {
            get {
                return (PythonType)base.Value;
            }
        }
    }
}
