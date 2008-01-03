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

namespace Microsoft.Scripting.Ast {
    public class CodeBlockInvoker {
        private CodeBlock _block;
        private CodeContext _context;

        public CodeBlockInvoker(CodeBlock block, CodeContext context) {
            _block = block;
            _context = context;
        }

        public TRet InvokeWithChildContext<TRet>() {
            return (TRet)Interpreter.ExecuteWithChildContext(_context, _block);
        }

        public TRet InvokeWithChildContext<T0, TRet>(T0 arg0) {
            return (TRet)Interpreter.ExecuteWithChildContext(_context, _block, arg0);
        }

        public TRet InvokeWithChildContext<T0, T1, TRet>(T0 arg0, T1 arg1) {
            return (TRet)Interpreter.ExecuteWithChildContext(_context, _block, arg0, arg1);
        }

        public TRet InvokeWithChildContext<T0, T1, T2, TRet>(T0 arg0, T1 arg1, T2 arg2) {
            return (TRet)Interpreter.ExecuteWithChildContext(_context, _block, arg0, arg1, arg2);
        }

        public TRet InvokeWithChildContext<T0, T1, T2, T3, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            return (TRet)Interpreter.ExecuteWithChildContext(_context, _block, arg0, arg1, arg2, arg3);
        }

        public TRet InvokeWithChildContext<T0, T1, T2, T3, T4, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            return (TRet)Interpreter.ExecuteWithChildContext(_context, _block, arg0, arg1, arg2, arg3, arg4);
        }

        public TRet InvokeWithChildContext<T0, T1, T2, T3, T4, T5, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            return (TRet)Interpreter.ExecuteWithChildContext(_context, _block, arg0, arg1, arg2, arg3, arg4, arg5);
        }

        public void ExecuteWithChildContext() {
            Interpreter.ExecuteWithChildContext(_context, _block);
        }

        public void ExecuteWithChildContext<T0>(T0 arg0) {
            Interpreter.ExecuteWithChildContext(_context, _block, arg0);
        }

        public void ExecuteWithChildContext<T0, T1>(T0 arg0, T1 arg1) {
            Interpreter.ExecuteWithChildContext(_context, _block, arg0, arg1);
        }

        public void ExecuteWithChildContext<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2) {
            Interpreter.ExecuteWithChildContext(_context, _block, arg0, arg1, arg2);
        }

        public void ExecuteWithChildContext<T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            Interpreter.ExecuteWithChildContext(_context, _block, arg0, arg1, arg2, arg3);
        }

        public void ExecuteWithChildContext<T0, T1, T2, T3, T4>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            Interpreter.ExecuteWithChildContext(_context, _block, arg0, arg1, arg2, arg3, arg4);
        }

        public void ExecuteWithChildContext<T0, T1, T2, T3, T4, T5>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            Interpreter.ExecuteWithChildContext(_context, _block, arg0, arg1, arg2, arg3, arg4, arg5);
        }

        public object ExecuteWithChildContext(CodeContext parent, object[] args) {
            return Interpreter.ExecuteWithChildContext(parent, _block, args);
        }

        public object ExecuteWithChildContextAndThis(CodeContext parent, object @this, object[] args) {
            return Interpreter.ExecuteWithChildContextAndThis(parent, _block, @this, args);
        }
    }
}
