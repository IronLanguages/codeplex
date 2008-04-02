/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Compiler.Generation;

namespace IronPython.Compiler.Ast {
    public abstract partial class ScopeStatement {
        private static Type GetEnvironmentType(int size, CodeGen cg, out ConstructorInfo ctor, out EnvironmentFactory ef) {
            Type envType;

            #region Generated partial factories

            // *** BEGIN GENERATED CODE ***

            if (size <= 32 && Options.OptimizeEnvironments) {
                if (size <= 2) {
                    envType = typeof(FunctionEnvironment2Dictionary);
                } else if (size <= 4) {
                    envType = typeof(FunctionEnvironment4Dictionary);
                } else if (size <= 8) {
                    envType = typeof(FunctionEnvironment8Dictionary);
                } else if (size <= 16) {
                    envType = typeof(FunctionEnvironment16Dictionary);
                } else {
                    envType = typeof(FunctionEnvironment32Dictionary);
                }
                ctor = envType.GetConstructor(new Type[] { typeof(FunctionEnvironmentDictionary), typeof(IModuleEnvironment), typeof(SymbolId[]), typeof(SymbolId[]) });
                ef = new FieldEnvironmentFactory(envType);
            } else {
                cg.EmitInt(size);
                envType = typeof(FunctionEnvironmentNDictionary);
                ctor = envType.GetConstructor(new Type[] { typeof(int), typeof(FunctionEnvironmentDictionary), typeof(IModuleEnvironment), typeof(SymbolId[]), typeof(SymbolId[]) });
                ef = new IndexEnvironmentFactory(size);
            }

            // *** END GENERATED CODE ***

            #endregion

            return envType;
        }
    }
}
