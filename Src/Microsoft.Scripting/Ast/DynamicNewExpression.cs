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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class DynamicNewExpression : CallExpression {
        internal DynamicNewExpression(SourceSpan span, Expression constructor, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs)
            : base(span, constructor, args, hasArgsTuple, hasKeywordDictionary, keywordCount, extraArgs) {
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
            Target.EmitAsObject(cg);
            cg.EmitArray(typeof(object), Args.Count, delegate(int index) {
                Args[index].Expression.EmitAsObject(cg);
            });
            cg.EmitCall(typeof(RuntimeHelpers), "Construct",
                new Type[] { typeof(CodeContext), typeof(object), typeof(object[]) });
        }
    }

    public static partial class Ast {
        public static DynamicNewExpression DynamicNew(Expression constructor, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs) {
            return DynamicNew(SourceSpan.None, constructor, args, hasArgsTuple, hasKeywordDictionary, keywordCount, extraArgs);
        }
        public static DynamicNewExpression DynamicNew(SourceSpan span, Expression constructor, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs) {
            return new DynamicNewExpression(span, constructor, args, hasArgsTuple, hasKeywordDictionary, keywordCount, extraArgs);
        }
    }
}
