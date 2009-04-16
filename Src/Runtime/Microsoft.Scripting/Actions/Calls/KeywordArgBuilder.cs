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

namespace Microsoft.Scripting.Actions.Calls {
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
    internal sealed class KeywordArgBuilder : ArgBuilder {
        private readonly int _kwArgCount, _kwArgIndex;
        private readonly ArgBuilder _builder;

        public KeywordArgBuilder(ArgBuilder builder, int kwArgCount, int kwArgIndex) 
            : base(builder.ParameterInfo) {

            Debug.Assert(BuilderExpectsSingleParameter(builder));
            Debug.Assert(builder.ConsumedArgumentCount == 1);
            _builder = builder;

            Debug.Assert(kwArgIndex < kwArgCount);
            _kwArgCount = kwArgCount;
            _kwArgIndex = kwArgIndex;
        }

        public override int Priority {
            get { return _builder.Priority; }
        }

        public override int ConsumedArgumentCount {
            get { return 1; }
        }

        /// <summary>
        /// The underlying builder should expect a single parameter as KeywordArgBuilder is responsible
        /// for calculating the correct parameter to use
        /// </summary>
        /// <param name="builder"></param>
        internal static bool BuilderExpectsSingleParameter(ArgBuilder builder) {
            return (((SimpleArgBuilder)builder).Index == 0);
        }

        internal protected override Expression ToExpression(OverloadResolver resolver, IList<Expression> parameters, bool[] hasBeenUsed) {
            Debug.Assert(BuilderExpectsSingleParameter(_builder));

            int index = GetKeywordIndex(parameters.Count);
            Debug.Assert(!hasBeenUsed[index]);
            hasBeenUsed[index] = true;
            return _builder.ToExpression(resolver, new Expression[] { parameters[index] }, new bool[1]);
        }

        protected internal override Func<object[], object> ToDelegate(OverloadResolver resolver, IList<DynamicMetaObject> knownTypes, bool[] hasBeenUsed) {
            return null;
        }

        public override Type Type {
            get {
                return _builder.Type;
            }
        }

        internal override Expression ToReturnExpression(OverloadResolver resolver) {
            return _builder.ToReturnExpression(resolver);
        }

        internal override Expression UpdateFromReturn(OverloadResolver resolver, IList<Expression> parameters) {
            return _builder.UpdateFromReturn(resolver, new Expression[] { parameters[GetKeywordIndex(parameters.Count)] });
        }

        private int GetKeywordIndex(int paramCount) {
            return paramCount - _kwArgCount + _kwArgIndex;
        }

        internal override Expression ByRefArgument {
            get { return _builder.ByRefArgument; }
        }
    }
}
