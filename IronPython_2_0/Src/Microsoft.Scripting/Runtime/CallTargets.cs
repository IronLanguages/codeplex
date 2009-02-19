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

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// The delegate representing the DLR Main function
    /// </summary>
    public delegate object DlrMainCallTarget(Scope scope, LanguageContext context);

    /// <summary>
    /// VB Doesn't allow params array parameters so for languages implemented in VB
    /// this useful delegate is here for now.
    /// </summary>
    [CLSCompliant(true)]
    public delegate object ParamsCallTarget(params object[] args);
}