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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Globalization;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using System.Runtime.CompilerServices;
using System.Reflection;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Scripting {
    // TODO: should move to C#
    public class DefaultLanguageContext : LanguageContext {
        public static CodeContext CreateCodeContext() {
            return CreateCodeContext(Type.EmptyTypes);
        }

        public static CodeContext CreateCodeContext(Type[] extensionTypes) {
            DefaultLanguageContext dlc = new DefaultLanguageContext(null);
            CodeContext cc = new CodeContext(new Scope(dlc), dlc);
            dlc.Binder = new DefaultActionBinder(cc, extensionTypes);
            return cc;
        }

        public DefaultLanguageContext(ScriptDomainManager manager)
            : base(manager) {
        }

        public override CodeBlock ParseSourceCode(CompilerContext context) {
            throw new NotImplementedException();
        }
    }

    // TODO: invariant context
    internal class DefaultActionBinder : ActionBinder {
        private Type[] _extensionTypes;

        public DefaultActionBinder(CodeContext context, Type[] extensionTypes)
            : base(context) {
            this._extensionTypes = extensionTypes;
        }

        protected internal override IList<Type> GetExtensionTypes(Type t) {
            return _extensionTypes;
        }


        // A bunch of conversion code
        public override object Convert(object obj, Type toType) {
            throw new NotImplementedException();
        }

        public override bool CanConvertFrom(Type fromType, Type toType, NarrowingLevel level) {
            return toType.IsAssignableFrom(fromType);
        }

        public override bool PreferConvert(Type t1, Type t2) {
            throw new NotImplementedException();
        }

        public override Expression ConvertExpression(Expression expr, Type toType) {
            if (toType.IsAssignableFrom(expr.Type)) {
                return expr;
            }
            return Ast.Ast.Convert(expr, toType);
        }

        public override Expression CheckExpression(Expression expr, Type toType) {
            throw new NotImplementedException();
        }
    }
}