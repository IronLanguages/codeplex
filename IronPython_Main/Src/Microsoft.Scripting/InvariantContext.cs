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
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Scripting {
    /// <summary>
    /// Singleton LanguageContext which represents a language-neutral LanguageContext
    /// </summary>
    public class InvariantContext : LanguageContext {
        public static InvariantContext Instance = new InvariantContext();
        public static CodeContext CodeContext = new CodeContext(new Scope(new SymbolDictionary()), Instance);

        public override bool ShowCls {
            get {
                return true;
            }
            set {
                throw new InvalidOperationException();
            }
        }
        
        private InvariantContext() : base(null) {
        }

    }
}
