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
using System.Diagnostics;
using System.Dynamic;
using ETUtils;

namespace TestAst {
    /// <summary>
    /// 
    /// </summary>
    static class TestSpan {
        /// <summary>
        /// Returns the full path to TestScenarios.Tests.cs
        /// </summary>
        public static string SourceFile {
            get {
                string setting = System.Environment.GetEnvironmentVariable("DLR_ROOT");
                if (setting == null) {
                    setting = System.Environment.GetEnvironmentVariable("dlr_root");
                }
                if (setting == null) {
                    string path;
                    path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    path = System.IO.Path.GetDirectoryName(path);
                    int length = path.LastIndexOf('\\');
                    if (length > 0) {
                        length = path.LastIndexOf('\\', length - 1);
                    }
                    if (length > 0) {
                        setting = path.Substring(0, length);
                    }
                }
                return System.IO.Path.Combine(setting, "Runtime\\Tests\\TestAst\\TestScenarios.Tests.cs");
            }
        }

        /// <summary>
        /// Wraps an expression with DebugInfo pointing to the callers location or the location
        /// of any deeper frame in the current stack.
        /// </summary>
        /// <param name="index">0 for the current frame, 1 for the previous frame, etc.</param>
        /// <returns></returns>
        public static Expression GetDebugInfoForFrame(Expression var, int index) {
            //Skip this frame too
            index = index + 1;
            StackTrace t = new StackTrace(true);
            if (index > t.FrameCount - 1)
                throw new ArgumentException("There aren't that many frames currently.");
            StackFrame frame = t.GetFrame(index);

            //If the source file is not available for the frame, don't attach
            //the debug information to the expression.
            if (String.IsNullOrEmpty(frame.GetFileName())) {
                return var;
            }

            return ExpressionUtils.AddDebugInfo(
                var,
                Expression.SymbolDocument(SourceFile),
                frame.GetFileLineNumber(), 
                frame.GetFileColumnNumber(), 
                frame.GetFileLineNumber(), 
                frame.GetFileColumnNumber()
            );
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static Expression GetDebugInfoForFrame(Expression var) {
            return GetDebugInfoForFrame(var, 1);
        }
    }
}
