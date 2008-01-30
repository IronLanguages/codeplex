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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// An expression that will return a reference to a block of code.
    /// Currently these references are created by emitting a delegate of the 
    /// requested type.
    /// </summary>
    public sealed class CodeBlockExpression : Expression {
        private readonly CodeBlock /*!*/ _block;
        private readonly bool _forceWrapperMethod;
        private readonly bool _stronglyTyped;
        private readonly Type _delegateType;
        
        internal bool ForceWrapperMethod {
            get { return _forceWrapperMethod; }
        }

        internal bool IsStronglyTyped {
            get { return _stronglyTyped; }
        }

        internal Type DelegateType {
            get { return _delegateType; }
        }

        internal CodeBlockExpression(CodeBlock /*!*/ block, bool forceWrapperMethod, bool stronglyTyped, Type delegateType)
            : base(AstNodeType.CodeBlockExpression, typeof(Delegate)) {
            Assert.NotNull(block);

            _block = block;
            _forceWrapperMethod = forceWrapperMethod;
            _stronglyTyped = stronglyTyped;
            _delegateType = delegateType;
        }

        public CodeBlock Block {
            get { return _block; }
        }
    }

    public static partial class Ast {
        public static CodeBlockExpression CodeBlockExpression(CodeBlock block, bool forceWrapper) {
            return new CodeBlockExpression(block, forceWrapper, false, null);
        }

        public static CodeBlockExpression CodeBlockExpression(CodeBlock block, bool forceWrapper, bool stronglyTyped) {
            return new CodeBlockExpression(block, forceWrapper, stronglyTyped, null);
        }

        public static CodeBlockExpression CodeBlockExpression(CodeBlock block, Type delegateType) {
            return new CodeBlockExpression(block, false, true, delegateType);
        }
    }
}
