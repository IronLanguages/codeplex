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

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;

using MSAst = Microsoft.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Linq.Expressions.Expression;

    class SavableGlobalAllocator : ArrayGlobalAllocator {
        private readonly MSAst.Expression/*!*/ _constantPool;
        private readonly List<MSAst.Expression/*!*/>/*!*/ _constants;

        public SavableGlobalAllocator(LanguageContext/*!*/ context)
            : base(context) {
            _constantPool = Ast.Variable(typeof(object[]), "$constantPool");
            _constants = new List<MSAst.Expression>();
        }

        public override Microsoft.Linq.Expressions.Expression GetConstant(object value) {
            return Utils.Constant(value);
        }

        public override ScriptCode/*!*/ MakeScriptCode(MSAst.Expression/*!*/ body, CompilerContext/*!*/ context, PythonAst/*!*/ ast) {
            MSAst.ParameterExpression scope = Ast.Parameter(typeof(Scope), "$scope");
            MSAst.ParameterExpression language = Ast.Parameter(typeof(LanguageContext), "$language ");

            // finally build the funcion that's closed over the array and
            var func = Ast.Lambda<Func<Scope, LanguageContext, object>>(
                Ast.Block(
                    new[] { GlobalArray },
                    Ast.Assign(
                        GlobalArray, 
                        Ast.Call(
                            null,
                            typeof(PythonOps).GetMethod("GetGlobalArray"),
                            scope
                        )
                    ),
                    body
                ),
                ((PythonCompilerOptions)context.Options).ModuleName,
                new MSAst.ParameterExpression[] { scope, language }
            );

            return new SavableScriptCode(func, context.SourceUnit, GetNames());
        }
    }
}
