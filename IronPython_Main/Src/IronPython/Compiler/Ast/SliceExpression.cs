/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class SliceExpression : Expression {
        private readonly Expression _sliceStart;
        private readonly Expression _sliceStop;
        private readonly Expression _sliceStep;

        public SliceExpression(Expression start, Expression stop, Expression step) {
            _sliceStart = start;
            _sliceStop = stop;
            _sliceStep = step;
        }

        public Expression SliceStart {
            get { return _sliceStart; }
        }

        public Expression SliceStop {
            get { return _sliceStop; }
        }

        public Expression SliceStep {
            get { return _sliceStep; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            return Ast.Call(
                Span,
                null,                                                       // instance
                AstGenerator.GetHelperMethod("MakeSlice"),                  // method
                ag.TransformOrConstantNull(_sliceStart, typeof(object)),    // parameters
                ag.TransformOrConstantNull(_sliceStop, typeof(object)),
                ag.TransformOrConstantNull(_sliceStep, typeof(object))
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_sliceStart != null) {
                    _sliceStart.Walk(walker);
                }
                if (_sliceStop != null) {
                    _sliceStop.Walk(walker);
                }
                if (_sliceStep != null) {
                    _sliceStep.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
