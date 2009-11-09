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

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

using AstUtils = Microsoft.Scripting.Ast.Utils;


namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    class ToDiskCompilationMode : CollectableCompilationMode {
        public override MSAst.Expression GetConstant(object value) {
            return AstUtils.Constant(value);
        }

        public override void PrepareScope(PythonAst ast, System.Runtime.CompilerServices.ReadOnlyCollectionBuilder<MSAst.ParameterExpression> locals, List<MSAst.Expression> init) {
            locals.Add(PythonAst._globalArray);
            init.Add(
                Ast.Assign(
                    PythonAst._globalArray,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("GetGlobalArrayFromContext"),
                        PythonAst._globalContext
                    )
                )
            );
        }

        public override MSAst.LambdaExpression ReduceAst(PythonAst instance, string name) {
            return Ast.Lambda<Func<CodeContext, FunctionCode, object>>(
                Ast.Block(
                    new[] { PythonAst._globalArray },
                    Ast.Assign(
                        PythonAst._globalArray,
                        Ast.Call(
                            null,
                            typeof(PythonOps).GetMethod("GetGlobalArrayFromContext"),
                            IronPython.Compiler.Ast.PythonAst._globalContext
                        )
                    ),
                    AstUtils.Convert(instance.ReduceWorker(), typeof(object))
                ),
                name,
                PythonAst._arrayFuncParams
            );
        }

        public override ScriptCode MakeScriptCode(PythonAst ast) {
            PythonCompilerOptions pco = ast.CompilerContext.Options as PythonCompilerOptions;
            var code = (MSAst.Expression<Func<CodeContext/*!*/, FunctionCode/*!*/, object>>)ast.Reduce();

            return new PythonSavableScriptCode(code, ast.SourceUnit, ast.GetNames(), pco.ModuleName);
        }

    }
}
