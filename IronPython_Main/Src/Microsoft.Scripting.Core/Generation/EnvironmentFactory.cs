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
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Creates FunctionEnvironments which are used for closures and exposing locals or arguments as a collection object to the user.
    /// 
    /// FunctionEnvironments are typically composed of two parts (but these can be the same objects):
    ///     1. The collection object exposed to the user (which implements IFunctionEnvironment)
    ///     2. The underlying storage object which generated code accesses for gets/sets.
    /// 
    /// The collection object exposed to the user is stored as the Locals property of the CodeContext.  The underlying
    /// storage can be of any data type.  The EnvironmentReferences that the EnvironmentFactory creates will create
    /// Slot's that access the storage in an appropriate manner.  These slots are passed the underlying storage object
    /// for their instance.
    /// 
    /// Creation of the environment factory consists of first creating the storage and then creating the collection object.
    /// </summary>
    internal abstract class EnvironmentFactory {
        /// <summary>
        /// Creates a reference within the environment with the specified name typed to object.
        /// </summary>
        public Storage MakeEnvironmentReference(SymbolId name) {
            return MakeEnvironmentReference(name, typeof(object));
        }

        /// <summary>
        /// Gets the underlying storage type for the environment, can be any type
        /// </summary>
        public abstract Type StorageType { get; }

        /// <summary>
        /// Creates a reference within the environment of the specified type and name.
        /// </summary>
        public abstract Storage MakeEnvironmentReference(SymbolId name, Type variableType);

        /// <summary>
        /// Emits the creation of the underlying storage object.
        /// </summary>
        /// <param name="cg"></param>
        public abstract void EmitStorage(ILGen cg);

        public abstract void EmitGetStorageFromContext(LambdaCompiler cg);

        /// <summary>
        /// Creates the slot that holds onto the environment for the specified Compiler.
        /// </summary>
        public abstract EnvironmentSlot CreateEnvironmentSlot(LambdaCompiler cg);        
    }
}
