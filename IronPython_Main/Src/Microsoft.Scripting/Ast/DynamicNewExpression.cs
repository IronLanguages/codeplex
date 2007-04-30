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
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class DynamicNewExpression : CallExpression {
        public DynamicNewExpression(Expression constructor, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs)
            : base(constructor, args, hasArgsTuple, hasKeywordDictionary, keywordCount, extraArgs) {
        }

        public DynamicNewExpression(Expression constructor, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs, SourceSpan span)
            : base(constructor, args, hasArgsTuple, hasKeywordDictionary, keywordCount, extraArgs, span) {
        }

        public override object Evaluate(CodeContext context) {
            object callee = Target.Evaluate(context);

            object[] cargs = new object[Args.Count];
            int index = 0;
            foreach (Arg arg in Args) {
                cargs[index++] = arg.Expression.Evaluate(context);
            }

            return RuntimeHelpers.Construct(context, callee, cargs);
        }

        public override void Emit(CodeGen cg) {
            cg.EmitCodeContext();
            Target.Emit(cg);
            cg.EmitArray(typeof(object), Args.Count, delegate(int index) {
                Args[index].Expression.EmitAs(cg, typeof(object));
            });
            cg.EmitCall(typeof(RuntimeHelpers), "Construct",
                new Type[] { typeof(CodeContext), typeof(object), typeof(object[]) });
        }
    }
}
