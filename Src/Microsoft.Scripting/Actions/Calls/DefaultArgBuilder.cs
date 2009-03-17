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
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Generation;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {

    /// <summary>
    /// ArgBuilder which provides a default parameter value for a method call.
    /// </summary>
    internal sealed class DefaultArgBuilder : ArgBuilder {
        public DefaultArgBuilder(ParameterInfo info) 
            : base(info) {
        }

        public override int Priority {
            get { return 2; }
        }

        internal protected override Expression ToExpression(ParameterBinder parameterBinder, IList<Expression> parameters, bool[] hasBeenUsed) {
            object val = ParameterInfo.DefaultValue;
            if (val is Missing) {
                val = CompilerHelpers.GetMissingValue(ParameterInfo.ParameterType);
            }

            if (ParameterInfo.ParameterType.IsByRef) {
                return AstUtils.Constant(val, ParameterInfo.ParameterType.GetElementType());
            }

            return parameterBinder.ConvertExpression(AstUtils.Constant(val), ParameterInfo, ParameterInfo.ParameterType);            
        }

        protected internal override Func<object[], object> ToDelegate(ParameterBinder parameterBinder, IList<DynamicMetaObject> knownTypes, bool[] hasBeenUsed) {
            object val = ParameterInfo.DefaultValue;
            if (val is Missing) {
                val = CompilerHelpers.GetMissingValue(ParameterInfo.ParameterType);
            }
            Debug.Assert(val != Missing.Value);
            return (args) => val;
        }

        internal override bool CanGenerateDelegate {
            get {
                if (ParameterInfo.ParameterType.IsByRef) {
                    return false;
                } else if (ParameterInfo.DefaultValue is Missing && CompilerHelpers.GetMissingValue(ParameterInfo.ParameterType) is Missing) {
                    // reflection throws when we do this
                    return false;
                }
                return true;
            }
        }
    }
}
