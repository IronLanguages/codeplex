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

using System;
using System.Diagnostics;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// VariableInfo stores read/write information needed by the compiler to emit a variable/parameter
    /// </summary>
    internal sealed class VariableInfo {
        private readonly Expression/*!*/ _variable;

        // the containing LambdaInfo
        // VariableBinder mutates this as it is resolving which lambda this variable belongs to
        private LambdaInfo _lambdaInfo;

        // parameter index (if variable is a parameter)
        private readonly int _parameterIndex;

        // storage for the variable, used to create slots
        private Storage _storage;

        // Lift variable/parameter to closure
        private bool _lift;

        internal VariableInfo(VariableExpression/*!*/ variable, LambdaInfo lambdaInfo) {
            Debug.Assert(variable != null);
            _variable = variable;
            _lambdaInfo = lambdaInfo;
            _parameterIndex = -1;
        }

        internal VariableInfo(ParameterExpression/*!*/ parameter, LambdaInfo lambdaInfo, int parameterIndex) {
            Debug.Assert(parameter != null);
            Debug.Assert(parameterIndex >= 0);
            _variable = parameter;
            _lambdaInfo = lambdaInfo;
            _parameterIndex = parameterIndex;
        }

        internal LambdaInfo LambdaInfo {
            get { return _lambdaInfo; }
            set { _lambdaInfo = value; }
        }

        internal Expression Variable {
            get { return _variable; }
        }

        internal SymbolId Name {
            get { return GetName(_variable); }
        }

        internal bool Lift {
            get { return _lift; }
        }

        internal Storage Storage {
            get { return _storage; }
            set { _storage = value; }
        }

        internal int ParameterIndex {
            get { return _parameterIndex; }
        }

        // Variable kind is Global or Variable is a local in a lambda marked as global.
        // TODO: remove this, the "local in global lambda" special case needs to go away 
        internal bool IsGlobal {
            get {
                return _variable.NodeType == AstNodeType.GlobalVariable ||
                    _variable.NodeType == AstNodeType.LocalVariable && _lambdaInfo.Lambda.IsGlobal;
            }
        }

        internal void LiftToClosure() {
            switch (_variable.NodeType) {
                case AstNodeType.LocalVariable:
                case AstNodeType.Parameter:
                    _lift = true;
                    break;
                default:
                    throw new InvalidOperationException(String.Format("Cannot lift variable of type {0} to a closure ('{1}')", _variable.NodeType, Name));
            }
        }

        // Gets the name of a VariableExpression or ParameterExpression
        internal static SymbolId GetName(Expression variable) {
            Debug.Assert(variable is VariableExpression || variable is ParameterExpression);
            VariableExpression v = variable as VariableExpression;
            if (v != null) {
                return v.Name;
            }
            return GetName((ParameterExpression)variable);
        }

        internal static SymbolId GetName(ParameterExpression p) {
            return p.Name != null ? SymbolTable.StringToId(p.Name) : SymbolId.Empty;
        }
    }
}
