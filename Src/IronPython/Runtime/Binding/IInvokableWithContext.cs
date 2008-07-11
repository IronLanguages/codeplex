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

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using System.Scripting.Actions;
using System.Linq.Expressions;

namespace IronPython.Runtime.Binding {
    /// <summary>
    /// Provides an interface for MetaObject's which support being called and passing through a CodeContext.
    /// 
    /// This only gets used when a site has our special CallWithContextBinder.
    /// </summary>
    interface IInvokableWithContext {
        MetaObject/*!*/ InvokeWithContext(InvokeAction/*!*/ call, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args);
    }
}
