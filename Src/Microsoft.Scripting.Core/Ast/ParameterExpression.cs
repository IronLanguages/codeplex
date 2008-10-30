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
using Microsoft.Scripting.Utils;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    public class ParameterExpression : Expression {
        private readonly string _name;
        private readonly Type _paramType;

        internal ParameterExpression(Type type, string name, Annotations annotations)
            : base(annotations) {
            _name = name;
            _paramType = type;
        }

        internal static ParameterExpression Make(Type type, string name, Annotations annotations, bool isByRef) {
            if (isByRef) {
                return new ByRefParameterExpression(type, name, annotations);
            }

            return new ParameterExpression(type, name, annotations);            
        }

        protected override Type GetExpressionType() {
            return _paramType;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Parameter;
        }

        public string Name {
            get { return _name; }
        }

        public bool IsByRef {
            get {
                return GetIsByRef();
            }
        }

        internal virtual bool GetIsByRef() {
            return false;
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitParameter(this);
        }
    }

    internal sealed class ByRefParameterExpression : ParameterExpression {
        internal ByRefParameterExpression(Type type, string name, Annotations annotations)
            : base(type, name, annotations) {
        }

        internal override bool GetIsByRef() {
            return true;
        }
    }

    public partial class Expression {
        public static ParameterExpression Parameter(Type type, string name) {
            return Parameter(type, name, Annotations.Empty);
        }

        //CONFORMING
        public static ParameterExpression Parameter(Type type, string name, Annotations annotations) {
            ContractUtils.RequiresNotNull(type, "type");

            if (type == typeof(void)) {
                throw Error.ArgumentCannotBeOfTypeVoid();
            }

            bool byref = type.IsByRef;
            if (byref) {
                type = type.GetElementType();
            }

            return ParameterExpression.Make(type, name, annotations, byref);
        }

        public static ParameterExpression Variable(Type type, string name) {
            return Variable(type, name, Annotations.Empty);
        }
        public static ParameterExpression Variable(Type type, string name, Annotations annotations) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(type != typeof(void), "type", Strings.ArgumentCannotBeOfTypeVoid);
            ContractUtils.Requires(!type.IsByRef, "type", Strings.TypeMustNotBeByRef);
            return ParameterExpression.Make(type, name, annotations, false);
        }

        //Variables must not be ByRef.
        internal static void RequireVariableNotByRef(ParameterExpression v, string varName) {
            Assert.NotNull(varName);
            if (v != null) {
                ContractUtils.Requires(!v.IsByRef, varName, Strings.VariableMustNotBeByRef);
            }
        }

        internal static void RequireVariablesNotByRef(ReadOnlyCollection<ParameterExpression> vs, string collectionName) {
            Assert.NotNull(vs);
            Assert.NotNull(collectionName);
            for (int i = 0; i < vs.Count; i++) {
                if (vs[i] != null && vs[i].IsByRef) {
                    // TODO: Just throw, don't call ContractUtils
                    ContractUtils.Requires(!vs[i].IsByRef, string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0}[{1}]", collectionName, i), Strings.VariableMustNotBeByRef);
                }
            }
        }
    }
}
