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
using MSAst = Microsoft.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Linq.Expressions.Expression;

    public class SliceExpression : Expression {
        private readonly Expression _sliceStart;
        private readonly Expression _sliceStop;
        private readonly Expression _sliceStep;
        private readonly bool _stepProvided;

        public SliceExpression(Expression start, Expression stop, Expression step, bool stepProvided) {
            _sliceStart = start;
            _sliceStop = stop;
            _sliceStep = step;
            _stepProvided = stepProvided;
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

        /// <summary>
        /// True if the user provided a step parameter (either providing an explicit parameter
        /// or providing an empty step parameter) false if only start and stop were provided.
        /// </summary>
        public bool StepProvided {
            get {
                return _stepProvided;
            }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            return Ast.Call(
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
