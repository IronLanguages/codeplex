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
namespace Microsoft.Linq.Expressions {

    public enum ExpressionType {

        #region Generated Expression Tree Node Types

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_tree_nodes from: generate_tree.py

        Add,
        AddChecked,
        And,
        AndAlso,
        ArrayLength,
        ArrayIndex,
        Call,
        Coalesce,
        Conditional,
        Constant,
        Convert,
        ConvertChecked,
        Divide,
        Equal,
        ExclusiveOr,
        GreaterThan,
        GreaterThanOrEqual,
        Invoke,
        Lambda,
        LeftShift,
        LessThan,
        LessThanOrEqual,
        ListInit,
        MemberAccess,
        MemberInit,
        Modulo,
        Multiply,
        MultiplyChecked,
        Negate,
        UnaryPlus,
        NegateChecked,
        New,
        NewArrayInit,
        NewArrayBounds,
        Not,
        NotEqual,
        Or,
        OrElse,
        Parameter,
        Power,
        Quote,
        RightShift,
        Subtract,
        SubtractChecked,
        TypeAs,
        TypeIs,
        Assign,
        Block,
        DebugInfo,
        DoStatement,
        Dynamic,
        EmptyStatement,
        Extension,
        Goto,
        Index,
        Label,
        LocalScope,
        LoopStatement,
        ReturnStatement,
        SwitchStatement,
        ThrowStatement,
        TryStatement,
        Unbox,

        // *** END GENERATED CODE ***

        #endregion
    }
}
