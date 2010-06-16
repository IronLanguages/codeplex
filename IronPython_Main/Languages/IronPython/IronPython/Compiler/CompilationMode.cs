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
using System.Dynamic;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

using IronPython.Compiler.Ast;
using IronPython.Runtime;
using System.Collections;

namespace IronPython.Compiler {
    public delegate object LookupCompilationDelegate(CodeContext context, FunctionCode code);

    /// <summary>
    /// Specifies the compilation mode which will be used during the AST transformation
    /// </summary>
    [Serializable]
    internal abstract class CompilationMode {
        /// <summary>
        /// Compilation will proceed in a manner in which the resulting AST can be serialized to disk.
        /// </summary>
        public static readonly CompilationMode ToDisk = new ToDiskCompilationMode();
        /// <summary>
        /// Compilation will use a type and declare static fields for globals.  The resulting type
        /// is uncollectible and therefore extended use of this will cause memory leaks.
        /// </summary>
        public static readonly CompilationMode Uncollectable = new UncollectableCompilationMode();
        /// <summary>
        /// Compilation will use an array for globals.  The resulting code will be fully collectible
        /// and once all references are released will be collected.
        /// </summary>
        public static readonly CompilationMode Collectable = new CollectableCompilationMode();
        /// <summary>
        /// Compilation will force all global accesses to do a full lookup.  This will also happen for
        /// any unbound local references.  This is the slowest form of code generation and is only
        /// used for exec/eval code where we can run against an arbitrary dictionary.
        /// </summary>
        public static readonly CompilationMode Lookup = new LookupCompilationMode();

        public virtual ScriptCode MakeScriptCode(PythonAst ast) {
            return new RuntimeScriptCode(ast, ast.ModuleContext.GlobalContext);
        }

        public virtual MSAst.Expression GetConstant(object value) {
            return MSAst.Expression.Constant(value);
        }

        public virtual Type GetConstantType(object value) {
            if (value == null) {
                return typeof(object);
            }

            return value.GetType();
        }

        public virtual void PrepareScope(PythonAst ast, ReadOnlyCollectionBuilder<MSAst.ParameterExpression> locals, List<MSAst.Expression> init) {
        }

        public virtual Type DelegateType {
            get {
                return typeof(MSAst.Expression<LookupCompilationDelegate>);
            }
        }

        public virtual UncollectableCompilationMode.ConstantInfo GetContext() {
            return null;
        }

        public virtual void PublishContext(CodeContext codeContext, UncollectableCompilationMode.ConstantInfo _contextInfo) {
        }

        public MSAst.Expression/*!*/ Dynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0) {
            if (retType == typeof(object)) {
                return new PythonDynamicExpression1(binder, this, arg0);
            } else if (retType == typeof(bool)) {
                return new PythonDynamicExpression1<bool>(binder, this, arg0);
            }

            return ReduceDynamic(binder, retType, arg0);
        }

        public MSAst.Expression/*!*/ Dynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0, MSAst.Expression/*!*/ arg1) {
            if (retType == typeof(object)) {
                return new PythonDynamicExpression2(binder, this, arg0, arg1);
            } else if (retType == typeof(bool)) {
                return new PythonDynamicExpression2<bool>(binder, this, arg0, arg1);
            }

            return ReduceDynamic(binder, retType, arg0, arg1);
        }

        public MSAst.Expression/*!*/ Dynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0, MSAst.Expression/*!*/ arg1, MSAst.Expression/*!*/ arg2) {
            if (retType == typeof(object)) {
                return new PythonDynamicExpression3(binder, this, arg0, arg1, arg2);
            }
            return ReduceDynamic(binder, retType, arg0, arg1, arg2);
        }

        public MSAst.Expression/*!*/ Dynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0, MSAst.Expression/*!*/ arg1, MSAst.Expression/*!*/ arg2, MSAst.Expression/*!*/ arg3) {
            if (retType == typeof(object)) {
                return new PythonDynamicExpression4(binder, this, arg0, arg1, arg2, arg3);
            }
            return ReduceDynamic(binder, retType, arg0, arg1, arg2, arg3);
        }

        public MSAst.Expression/*!*/ Dynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/[]/*!*/ args) {
            Assert.NotNull(binder, retType, args);
            Assert.NotNullItems(args);
            
            switch (args.Length) {
                case 1: return Dynamic(binder, retType, args[0]);
                case 2: return Dynamic(binder, retType, args[0], args[1]);
                case 3: return Dynamic(binder, retType, args[0], args[1], args[2]);
                case 4: return Dynamic(binder, retType, args[0], args[1], args[2], args[3]);
            }

            if (retType == typeof(object)) {
                return new PythonDynamicExpressionN(binder, this, args);
            }

            return ReduceDynamic(binder, retType, args);
        }

        public virtual MSAst.Expression/*!*/ ReduceDynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0) {
            return MSAst.Expression.Dynamic(binder, retType, arg0);
        }

        public virtual MSAst.Expression/*!*/ ReduceDynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0, MSAst.Expression/*!*/ arg1) {
            return MSAst.Expression.Dynamic(binder, retType, arg0, arg1);
        }

        public virtual MSAst.Expression/*!*/ ReduceDynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0, MSAst.Expression/*!*/ arg1, MSAst.Expression/*!*/ arg2) {
            return MSAst.Expression.Dynamic(binder, retType, arg0, arg1, arg2);
        }

        public virtual MSAst.Expression/*!*/ ReduceDynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0, MSAst.Expression/*!*/ arg1, MSAst.Expression/*!*/ arg2, MSAst.Expression/*!*/ arg3) {
            return MSAst.Expression.Dynamic(binder, retType, arg0, arg1, arg2, arg3);
        }

        public virtual MSAst.Expression/*!*/ ReduceDynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/[]/*!*/ args) {
            Assert.NotNull(binder, retType, args);
            Assert.NotNullItems(args);

            return MSAst.Expression.Dynamic(binder, retType, args);
        }

        public abstract MSAst.Expression GetGlobal(MSAst.Expression globalContext, int arrayIndex, PythonVariable variable, PythonGlobal global);

        public abstract LightLambdaExpression ReduceAst(PythonAst instance, string name);
    }
}
