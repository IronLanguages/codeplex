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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// An expression that will return a reference to a block of code.
    /// Currently these references are created by emitting a delegate of the 
    /// requested type.
    /// </summary>
    public sealed class CodeBlockExpression : Expression {
        private readonly LambdaExpression /*!*/ _block;

        internal CodeBlockExpression(LambdaExpression /*!*/ block, Type/*!*/ type)
            : base(AstNodeType.CodeBlockExpression, type) {
            Assert.NotNull(block);

            _block = block;
        }

        public LambdaExpression Block {
            get { return _block; }
        }
    }

    public static partial class Ast {
        public static CodeBlockExpression CodeBlockExpression(LambdaExpression block, Type type) {
            Contract.RequiresNotNull(block, "block");
            Contract.RequiresNotNull(type, "type");

            ValidateDelegateType(block, type);

            return new CodeBlockExpression(block, type);
        }

        /// <summary>
        /// Validates that the delegate type of the lambda
        /// matches the lambda itself.
        /// 
        /// * Return types of the lambda and the delegate must be identical.
        /// 
        /// * Without parameter array on the delegate type, the signatures must
        ///   match perfectly as to count and types of parameters.
        ///   
        /// * With parameter array on the delegate type, the common subset of
        ///   parameters must match
        /// </summary>
        private static void ValidateDelegateType(LambdaExpression lambda, Type type) {
            Contract.Requires(type != typeof(Delegate), "type", "type must not be System.Delegate.");
            Contract.Requires(TypeUtils.CanAssign(typeof(Delegate), type), "type", "Incorrect delegate type.");

            MethodInfo mi = type.GetMethod("Invoke");
            Contract.RequiresNotNull(mi, "Delegate must have an 'Invoke' method");

            Contract.Requires(mi.ReturnType == lambda.ReturnType, "type", "Delegate type doesn't match LambdaExpression");

            ParameterInfo[] infos = mi.GetParameters();
            ReadOnlyCollection<Variable> parameters = lambda.Parameters;

            if (infos.Length > 0 && CompilerHelpers.IsParamArray(infos[infos.Length - 1])) {
                Contract.Requires(infos.Length - 1 <= parameters.Count, "Delegate and block parameter count mismatch");

                // Parameter array case. The lambda may have more parameters than delegate,
                // and can also have parameter array as its last parameter, however all of the
                // parameters upto delegate's parameter array (excluding) must be identical

                ValidateIdenticalParameters(infos, parameters, infos.Length - 1);
            } else {
                Contract.Requires(infos.Length == parameters.Count, "Delegate and block parameter count mismatch");

                // No parameter array. The lambda must have identical signature to that of the
                // delegate, and it may not be marked as parameter array itself.
                ValidateIdenticalParameters(infos, parameters, infos.Length);

                Contract.Requires(!lambda.ParameterArray, "block", "Parameter array delegate type required for parameter array lambda");
            }
        }

        private static void ValidateIdenticalParameters(ParameterInfo[] infos, ReadOnlyCollection<Variable> parameters, int count) {
            Debug.Assert(count <= infos.Length && count <= parameters.Count);
            while (count-- > 0) {
                Contract.Requires(infos[count].ParameterType == parameters[count].Type, "type");
            }
        }
    }
}
