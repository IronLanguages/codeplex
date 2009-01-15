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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    public abstract class CodeSenseProvider 
#if !SILVERLIGHT 
        : MarshalByRefObject
#endif
    {
        private readonly ScriptEngine _engine;
        private readonly ScriptScope _module;

        protected ScriptEngine Engine { get { return _engine; } }
        protected ScriptScope Module { get { return _module; } }

        protected CodeSenseProvider(ScriptEngine engine, ScriptScope module) {
            CodeContract.RequiresNotNull(engine, "engine");
            _engine = engine;
            _module = module;
        }

        public abstract string GetFunctionSignature(string name);
        public abstract string[] GetMemberNames(string name);
        public abstract string GetFunctionDoc(string name);
    }
}
