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
using System.Diagnostics;
using System.Linq.Expressions;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// ArgBuilder which provides a value for a keyword argument.  
    /// 
    /// The KeywordArgBuilder calculates its position at emit time using it's initial 
    /// offset within the keyword arguments, the number of keyword arguments, and the 
    /// total number of arguments provided by the user.  It then delegates to an 
    /// underlying ArgBuilder which only receives the single correct argument.
    /// 
    /// Delaying the calculation of the position to emit time allows the method binding to be 
    /// done without knowing the exact the number of arguments provided by the user. Hence,
    /// the method binder can be dependent only on the set of method overloads and keyword names,
    /// but not the user arguments. While the number of user arguments could be determined
    /// upfront, the current MethodBinder does not have this design.
    /// </summary>
    class KeywordArgBuilder : ArgBuilder {
        private int _kwArgCount, _kwArgIndex;
        private ArgBuilder _builder;

        public KeywordArgBuilder(ArgBuilder builder, int kwArgCount, int kwArgIndex) {
            Debug.Assert(BuilderExpectsSingleParameter(builder));
            _builder = builder;

            Debug.Assert(kwArgIndex < kwArgCount);
            _kwArgCount = kwArgCount;
            _kwArgIndex = kwArgIndex;
        }

        public override int Priority {
            get { return _builder.Priority; }
        }

        /// <summary>
        /// The underlying builder should expect a single parameter as KeywordArgBuilder is responsible
        /// for calculating the correct parameter to use
        /// </summary>
        /// <param name="builder"></param>
        internal static bool BuilderExpectsSingleParameter(ArgBuilder builder) {
            return (((SimpleArgBuilder)builder).Index == 0);
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters, bool[] hasBeenUsed) {
            Debug.Assert(BuilderExpectsSingleParameter(_builder));
            int index = GetKeywordIndex(parameters.Count);
            hasBeenUsed[index] = true;
            return _builder.ToExpression(context, new Expression[] { parameters[index] }, new bool[1]);
        }

        public override Type Type {
            get {
                return _builder.Type;
            }
        }

        internal override Expression ToReturnExpression(MethodBinderContext context) {
            return _builder.ToReturnExpression(context);
        }

        internal override Expression UpdateFromReturn(MethodBinderContext context, IList<Expression> parameters) {
            return _builder.UpdateFromReturn(context, new Expression[] { parameters[GetKeywordIndex(parameters.Count)] });
        }

        private int GetKeywordIndex(int paramCount) {
            return paramCount - _kwArgCount + _kwArgIndex;
        }
    }
}
