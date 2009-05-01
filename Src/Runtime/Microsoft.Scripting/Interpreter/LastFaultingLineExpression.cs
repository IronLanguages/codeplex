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
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;


using Microsoft.Scripting.Runtime;
using System.Threading;


namespace Microsoft.Scripting.Interpreter {
    public class LastFaultingLineExpression : Expression {
        private readonly Expression _lineNumberExpression;
        
        public LastFaultingLineExpression(Expression lineNumberExpression) {
            _lineNumberExpression = lineNumberExpression;
        }

        protected override ExpressionType NodeTypeImpl() {
            return ExpressionType.Extension;
        }

        protected override Type/*!*/ TypeImpl() {
            return typeof(int);
        }

        public override bool CanReduce {
            get {
                return true;
            }
        }

        public override Expression/*!*/ Reduce() {
            return _lineNumberExpression;
        }

        protected override Expression VisitChildren(Func<Expression, Expression> visitor) {
            Expression lineNo = visitor(_lineNumberExpression);
            if (lineNo != _lineNumberExpression) {
                return new LastFaultingLineExpression(lineNo);
            }

            return this;
        }
    }

    class UpdateStackTraceInstruction : Instruction {
        internal DebugInfo[] _debugInfos;

        public override int ProducedStack {
            get {
                return 1;
            }
        }

        public override int Run(InterpretedFrame frame) {
            DebugInfo info = DebugInfo.GetMatchingDebugInfo(_debugInfos, frame.FaultingInstruction);
            if (info != null && !info.IsClear) {
                frame.Push(info.StartLine);
            }else{
                frame.Push(-1);
            }

            return +1;
        }
    }
}
