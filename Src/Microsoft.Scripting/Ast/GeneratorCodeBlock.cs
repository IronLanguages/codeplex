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
using System.Reflection.Emit;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Generator code block (code block with yield statements).
    /// 
    /// To create the generator, the AST node requires 2 types.
    /// First is the type of the generator object. The code generation will emit code to instantiate
    /// this type using constructor GeneratorType(CodeContext, DelegateType)
    /// 
    /// The GeneratorType must inherit from Generator
    /// The second type is the Delegate type in the above constructor call.
    /// 
    /// The inner function of the generator will have the signature:
    /// bool GetNext(GeneratorType, out object value);
    /// </summary>
    public sealed class GeneratorCodeBlock : CodeBlock {
        /// <summary>
        /// The type of the generator instance.
        /// The CodeBlock will emit code to create a new instance of this type, using constructor:
        /// GeneratorType(CodeContext context, Delegate next);
        /// </summary>
        private readonly Type _generator;
        /// <summary>
        /// The type of the delegate to produce the next element.
        /// </summary>
        private readonly Type _next;

        // TODO: Move
        private List<YieldTarget> _topTargets;

        internal GeneratorCodeBlock(SourceSpan span, string name, Type generator, Type next)
            : base(span, name, typeof(object)) {
            _generator = generator;
            Debug.Assert(generator.IsSubclassOf(typeof(Generator)));
            _next = next;
        }

        public Type GeneratorType {
            get { return _generator; }
        }

        public Type DelegateType {
            get { return _next; }
        }

        internal IList<YieldTarget> TopTargets {
            get { return _topTargets; }
        }
        
        internal int BuildYieldTargets() {
            Debug.Assert(_topTargets == null);
            int temps;
            YieldLabelBuilder.BuildYieldTargets(this, out _topTargets, out temps);
            return temps;
        }
    }

    public static partial class Ast {
        public static CodeBlock Generator(SourceSpan span, string name, Type generator, Type next) {
            Contract.RequiresNotNull(name, "name");
            Contract.RequiresNotNull(generator, "generator");
            Contract.RequiresNotNull(next, "next");
            Contract.Requires(TypeUtils.CanAssign(typeof(Generator), generator), "generator", "The generator type must inherit from Generator");

            return new GeneratorCodeBlock(span, name, generator, next);
        }
    }
}
