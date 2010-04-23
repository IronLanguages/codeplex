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
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;

using AstUtils = Microsoft.Scripting.Ast.Utils;


namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;
    /// <summary>
    /// A global allocator that puts all of the globals into an array access.  The array is an
    /// array of PythonGlobal objects.  We then just close over the array for any inner functions.
    /// 
    /// Once compiled a RuntimeScriptCode is produced which is closed over the entire execution
    /// environment.
    /// </summary>
    class CollectableCompilationMode : CompilationMode {

        public override MSAst.LambdaExpression ReduceAst(PythonAst instance, string name) {
            return Ast.Lambda<Func<FunctionCode, object>>(
                    Ast.Block(
                        new[] { PythonAst._globalArray, PythonAst._globalContext },
                        Ast.Assign(PythonAst._globalArray, instance.GlobalArrayInstance),
                        Ast.Assign(PythonAst._globalContext, Ast.Constant(instance.ModuleContext.GlobalContext)),
                        AstUtils.Convert(instance.ReduceWorker(), typeof(object))
                    ),
                    name,
                    new[] { PythonAst._functionCode }
                );
        }

        public override void PrepareScope(PythonAst ast, ReadOnlyCollectionBuilder<MSAst.ParameterExpression> locals, List<MSAst.Expression> init) {
            locals.Add(PythonAst._globalArray);
            init.Add(Ast.Assign(PythonAst._globalArray, ast._arrayExpression));
        }


        public override MSAst.Expression GetGlobal(MSAst.Expression globalContext, int arrayIndex, PythonVariable variable, PythonGlobal global) {
            Assert.NotNull(global);

            return new PythonGlobalVariableExpression(
                Ast.ArrayIndex(
                    PythonAst._globalArray,
                    Ast.Constant(arrayIndex)
                ),
                variable,
                global
            );
        }

        public override Type DelegateType {
            get {
                return typeof(MSAst.Expression<Func<FunctionCode, object>>);
            }
        }
    }
}
