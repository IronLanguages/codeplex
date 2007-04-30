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
using System.Diagnostics;

using Microsoft.Scripting;

namespace Microsoft.Scripting {
    /// <summary>
    /// A TypeCollision is used when we have a collsion between
    /// two types with the same name.  Currently this is only possible w/ generic
    /// methods that should logically have arity as a portion of their name.
    /// 
    /// The TypeCollision provides an indexer but also is a real type.  When used
    /// as a real type it is the non-generic form of the type.
    /// 
    /// The indexer allows the user to disambiguate between the generic and
    /// non-generic versions.  Therefore users must always provide additional
    /// information to get the generic version.
    /// </summary>
    public sealed class TypeCollision : ICallableWithCodeContext, IConstructorWithCodeContext {
        private List<DynamicType> _types;
        private Type _type;

        public TypeCollision(Type t) {
            _types = new List<DynamicType>();
            _type = t;
        }

        /// <summary>
        /// Indexer for generic parameter resolution.  We bind to one of the generic versions
        /// available in this type collision.  A user can also do someType[()] to force to
        /// bind to the non-generic version, but we will always present the non-generic version
        /// when no bindings are available.
        /// </summary>
        public object this[params object []index] {
            get {
                Type[] bindRequest = new Type[index.Length] ;
                for (int i = 0; i < bindRequest.Length; i++) {
                    bindRequest[i] = index[i] as Type ?? ((DynamicType)index[i]).UnderlyingSystemType;
                }

                // Try our base type first (it's the only possible non-generic)
                if (TryGenericBind(bindRequest, _type)) {
                    if (bindRequest.Length == 0) return DynamicHelpers.GetDynamicTypeFromType(_type);

                    return DynamicHelpers.GetDynamicTypeFromType(_type.MakeGenericType(bindRequest));
                }

                for (int i = 0; i < _types.Count; i++) {
                    // next try all of our other generics, until we find
                    // an arity that matches.
                    Debug.Assert(_types[i].UnderlyingSystemType.ContainsGenericParameters);

                    if (TryGenericBind(bindRequest, _types[i].UnderlyingSystemType)) {
                        return DynamicHelpers.GetDynamicTypeFromType(_types[i].UnderlyingSystemType.MakeGenericType(bindRequest));
                    }
                }

                throw new ArgumentException(String.Format("could not find compatible generic type for {0} type args", bindRequest.Length));
            }
        }

        internal void UpdateType(Type t) {
            Debug.Assert(t.ContainsGenericParameters,
                String.Format("Expected only generics to be added: {0}, non generics need to CloneWithNewBase", t.Name));

            int genericCount = GetGenericCount(t.GetGenericArguments());
            for (int i = 0; i < _types.Count; i++) {
                if (genericCount == GetGenericCount(_types[i].UnderlyingSystemType.GetGenericArguments())) {
                    _types[i] = DynamicHelpers.GetDynamicTypeFromType(t);
                    return;
                }
            }

            _types.Add(DynamicHelpers.GetDynamicTypeFromType(t));
        }

        /// <summary> Creates a new TypeCollision using this types generic list w/ the specified
        /// non-generic type as the TypeCollision's ReflectedType.</summary>
        internal TypeCollision CloneWithNewBase(Type newBase) {
            Debug.Assert(!newBase.ContainsGenericParameters);

            TypeCollision res = new TypeCollision(newBase);
            res._types.AddRange(_types);

            if (_type.ContainsGenericParameters) {
                // if we have a collision between two non-generic
                // types new newer type simply wins.
                res._types.Add(DynamicHelpers.GetDynamicTypeFromType(_type));
            }
            return res;
        }

        /// <summary> Determines if the bind request matches the arity of the provided type</summary>
        private bool TryGenericBind(Type[] bindRequest, Type t) {
            if (bindRequest.Length == 0 && !t.ContainsGenericParameters)
                return true;

            int genericCount = GetGenericCount(t.GetGenericArguments());

            if (genericCount == bindRequest.Length)
                return true;

            return false;
        }

        /// <summary> Gets the number of unbound generic arguments exist in a type array</summary>
        private int GetGenericCount(Type[] genericArgs) {
            int genericCount = 0;
            for (int i = 0; i < genericArgs.Length; i++)
                if (genericArgs[i].IsGenericParameter)
                    genericCount++;
            return genericCount;
        }

        public Type DefaultType {
            get {
                return _type;
            }
        }

        public IList<DynamicType> OtherTypes {
            get {
                return _types;
            }
        }


        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, params object[] args) {
            return DynamicHelpers.CallWithContext(context, DynamicHelpers.GetDynamicTypeFromType(_type), args);
        }

        #endregion

        #region IConstructorWithCodeContext Members

        public object Construct(CodeContext context, params object[] args) {
            return RuntimeHelpers.Construct(context, DynamicHelpers.GetDynamicTypeFromType(_type), args);
        }

        #endregion
    }
}
