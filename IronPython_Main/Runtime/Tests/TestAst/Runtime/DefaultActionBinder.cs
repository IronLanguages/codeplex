/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions.Calls;

namespace TestAst.Runtime {

    public class DefaultActionBinder : DefaultBinder {
        public DefaultActionBinder() {
        }

        public override Expression ConvertExpression(Expression expr, Type toType, ConversionResultKind kind, OverloadResolverFactory factory) {
            if (toType.IsAssignableFrom(expr.Type)) {
                return expr;
            }

            
            return Expression.Convert(expr, toType);
        }

        public override bool CanConvertFrom(Type fromType, Type toType, bool toNotNullable, NarrowingLevel level) {
            return Converter.CanConvertFrom(fromType, toType, level);
        }

        public override Candidate PreferConvert(Type t1, Type t2) {
            return Converter.PreferConvert(t1, t2);
        }

        public override object Convert(object obj, Type toType) {
            return Converter.Convert(obj, toType);
        }
    }
}
