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
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public partial class CodeBlock {
        public static EnvironmentFactory CreateEnvironmentFactory(int size) {
            size++; // +1 for the FunctionEnvironmentDictionary 
            bool optimized = ScriptDomainManager.Options.OptimizeEnvironments;
            
            #region Generated partial factories

            // *** BEGIN GENERATED CODE ***

            if (size <= 128 && optimized) {
                Type envType, tupleType;
                if (size <= 2) {
                    envType = typeof(FunctionEnvironmentDictionary<Tuple<IAttributesCollection, object>>);
                    tupleType = typeof(Tuple<IAttributesCollection, object>);
                } else if (size <= 4) {
                    envType = typeof(FunctionEnvironmentDictionary<Tuple<IAttributesCollection, object, object, object>>);
                    tupleType = typeof(Tuple<IAttributesCollection, object, object, object>);
                } else if (size <= 8) {
                    envType = typeof(FunctionEnvironmentDictionary<Tuple<IAttributesCollection, object, object, object, object, object, object, object>>);
                    tupleType = typeof(Tuple<IAttributesCollection, object, object, object, object, object, object, object>);
                } else if (size <= 16) {
                    envType = typeof(FunctionEnvironmentDictionary<Tuple<IAttributesCollection, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>);
                    tupleType = typeof(Tuple<IAttributesCollection, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                } else if (size <= 32) {
                    envType = typeof(FunctionEnvironmentDictionary<Tuple<IAttributesCollection, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>);
                    tupleType = typeof(Tuple<IAttributesCollection, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                } else if (size <= 64) {
                    envType = typeof(FunctionEnvironmentDictionary<Tuple<IAttributesCollection, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>);
                    tupleType = typeof(Tuple<IAttributesCollection, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                } else {
                    envType = typeof(FunctionEnvironmentDictionary<Tuple<IAttributesCollection, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>);
                    tupleType = typeof(Tuple<IAttributesCollection, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                }
                return new PropertyEnvironmentFactory(tupleType, envType);
            } else {
                return new IndexEnvironmentFactory(size);
            }

            // *** END GENERATED CODE ***

            #endregion
        }

        
    }
}
