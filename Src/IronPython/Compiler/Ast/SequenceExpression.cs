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

using System.Collections.Generic;
using System.Scripting;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public abstract class SequenceExpression : Expression {
        private readonly Expression[] _items;

        protected SequenceExpression(Expression[] items) {
            _items = items;
        }

        public Expression[] Items {
            get { return _items; }
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, Operators op) {
            if (op != Operators.None) {
                ag.AddError("augmented assign to sequence prohibited", Span);
                return null;
            }

            // if we just have a simple named multi-assignment  (e.g. a, b = 1,2)
            // then go ahead and step over the entire statement at once.  If we have a 
            // more complex statement (e.g. a.b, c.d = 1, 2) then we'll step over the
            // sets individually as they could be property sets the user wants to step
            // into.  TODO: Enable stepping of the right hand side?
            bool emitIndividualSets = false;
            foreach (Expression e in _items) {
                if (IsComplexAssignment(e)) {
                    emitIndividualSets = true;
                    break;
                }
            }

            SourceSpan rightSpan = SourceSpan.None;
            SourceSpan leftSpan =
                (Span.Start.IsValid && span.IsValid) ?
                    new SourceSpan(Span.Start, span.End) :
                    SourceSpan.None;

            SourceSpan totalSpan = SourceSpan.None;
            if (emitIndividualSets) {
                rightSpan = span;
                leftSpan = SourceSpan.None;
                totalSpan = (Span.Start.IsValid && span.IsValid) ?
                    new SourceSpan(Span.Start, span.End) :
                    SourceSpan.None;
            }

            List<MSAst.Expression> statements = new List<MSAst.Expression>();

            // 1. Evaluate the expression and assign the value to the temp.
            MSAst.VariableExpression right_temp = ag.GetTemporary("unpacking");

            // 2. Add the assignment "right_temp = right" into the suite/block
            statements.Add(
                AstGenerator.MakeAssignment(right_temp, right)
                );

            // 3. Call GetEnumeratorValues on the right side (stored in temp)
            MSAst.Expression enumeratorValues = Ast.Call(
                AstGenerator.GetHelperMethod("GetEnumeratorValues"),    // method
                // arguments
                AstUtils.CodeContext(),
                right_temp,
                Ast.Constant(_items.Length)
            );

            // 4. Create temporary variable for the array
            MSAst.VariableExpression array_temp = ag.GetTemporary("array", typeof(object[]));

            // 5. Assign the value of the method call (mce) into the array temp
            // And add the assignment "array_temp = Ops.GetEnumeratorValues(...)" into the block
            statements.Add(
                AstGenerator.MakeAssignment(
                    array_temp, 
                    enumeratorValues, 
                    rightSpan
                )
            );

            List<MSAst.Expression> sets = new List<MSAst.Expression>();            
            for (int i = 0; i < _items.Length; i ++) {
                // target = array_temp[i]

                Expression target = _items[i];
                if (target == null) {
                    continue;
                }

                // 6. array_temp[i]
                MSAst.Expression element = Ast.ArrayIndex(
                    array_temp,                             // array expression
                    Ast.Constant(i)                         // index
                );

                // 7. target = array_temp[i], and add the transformed assignment into the list of sets
                MSAst.Expression set = target.TransformSet(
                    ag,
                    emitIndividualSets ?                    // span
                        target.Span :
                        SourceSpan.None,
                    element,
                    Operators.None
                );
                if (set == null) {
                    throw PythonOps.SyntaxError(string.Format("can't assign to {0}", target.NodeName), ag.Context.SourceUnit, target.Span, -1);
                }

                sets.Add(set);
            }
            // 9. add the sets as their own block so they cna be marked as a single span, if necessary.
            statements.Add(AstUtils.Block(leftSpan, sets.ToArray()));

            // 10. Free the temps
            ag.FreeTemp(array_temp);
            ag.FreeTemp(right_temp);

            // 11. Return the suite statement (block)
            return AstUtils.Block(totalSpan, statements.ToArray());
        }

        private static bool IsComplexAssignment(Expression expr) {
            return !(expr is NameExpression);
        }

        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            MSAst.Expression[] statements = new MSAst.Expression[_items.Length];
            for (int i = 0; i < statements.Length; i++) {
                statements[i] = _items[i].TransformDelete(ag);
            }
            return AstUtils.Block(Span, statements);
        }

        internal override bool CanThrow {
            get {
                foreach (Expression e in _items) {
                    if (e.CanThrow) {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
