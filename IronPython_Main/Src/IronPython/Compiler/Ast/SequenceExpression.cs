/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public abstract class SequenceExpression : Expression {
        private readonly Expression[] _items;

        protected SequenceExpression(Expression[] items) {
            _items = items;
        }

        public Expression[] Items {
            get { return _items; }
        }

        internal override MSAst.Statement TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, Operators op) {
            if (op != Operators.None) {
                ag.AddError("augmented assign to sequence prohibited", Span);
                return null;
            }

            if (_items.Length == 0) {
                ag.AddError("can't assign to empty sequence", Span);
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
                leftSpan = Microsoft.Scripting.SourceSpan.None;
                totalSpan = (Span.Start.IsValid && span.IsValid) ?
                    new SourceSpan(Span.Start, span.End) :
                    SourceSpan.None;
            }

            List<MSAst.Statement> statements = new List<MSAst.Statement>();

            // 1. Evaluate the expression and assign the value to the temp.
            MSAst.BoundExpression right_temp = ag.MakeTempExpression("unpacking");

            // 2. Add the assignment "right_temp = right" into the suite/block
            statements.Add(
                AstGenerator.MakeAssignment(right_temp.Variable, right)
                );

            // 3. Call GetEnumeratorValues on the right side (stored in temp)
            MSAst.Expression enumeratorValues = Ast.Call(
                null,                                                   // instance
                AstGenerator.GetHelperMethod("GetEnumeratorValues"),    // method
                // arguments
                right_temp,
                Ast.Constant(_items.Length)
            );

            // 4. Create temporary variable for the array
            MSAst.BoundExpression array_temp = ag.MakeTempExpression("array", typeof(object[]));

            // 5. Assign the value of the method call (mce) into the array temp
            // And add the assignment "array_temp = Ops.GetEnumeratorValues(...)" into the block
            statements.Add(
                AstGenerator.MakeAssignment(array_temp.Variable, enumeratorValues, rightSpan)
                );

            List<MSAst.Statement> sets = new List<MSAst.Statement>();            
            for (int i = 0; i < _items.Length; i ++) {
                // target = array_temp[i]

                Expression target = _items[i];
                if (target == null) {
                    continue;
                }

                // 6. array_temp[i]
                MSAst.ArrayIndexExpression element = Ast.ArrayIndex(
                    array_temp,                             // array expression
                    Ast.Constant(i)                         // index
                );

                // 7. target = array_temp[i], and add the transformed assignment into the list of sets
                sets.Add(
                    target.TransformSet(
                        ag,
                        emitIndividualSets ?                    // span
                            target.Span :
                            SourceSpan.None,
                        element,
                        Operators.None
                    )
                );
            }
            // 9. add the sets as their own block so they cna be marked as a single span, if necessary.
            statements.Add(Ast.Block(leftSpan, sets.ToArray()));

            // 10. Free the temps
            ag.FreeTemp(array_temp);
            ag.FreeTemp(right_temp);

            // 11. Return the suite statement (block)
            return Ast.Block(totalSpan, statements.ToArray());
        }

        private static bool IsComplexAssignment(Expression expr) {
            return !(expr is NameExpression);
        }

        internal override MSAst.Statement TransformDelete(AstGenerator ag) {
            MSAst.Statement[] statements = new MSAst.Statement[_items.Length];
            for (int i = 0; i < statements.Length; i++) {
                statements[i] = _items[i].TransformDelete(ag);
            }
            return Ast.Block(Span, statements);
        }
    }
}
