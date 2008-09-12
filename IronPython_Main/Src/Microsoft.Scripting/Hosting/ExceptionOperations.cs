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
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Hosting {
    public sealed class ExceptionOperations
#if !SILVERLIGHT
 : MarshalByRefObject
#endif
 {
        private readonly LanguageContext _context;

        internal ExceptionOperations(LanguageContext context) {
            _context = context;
        }

        public string FormatException(Exception exception) {
            return _context.FormatException(exception);
        }

        public void GetExceptionMessage(Exception exception, out string message, out string errorTypeName) {
            _context.GetExceptionMessage(exception, out message, out errorTypeName);
        }

        public bool HandleException(Exception exception) {
            ContractUtils.RequiresNotNull(exception, "exception");
            return false;
        }
    }
}
