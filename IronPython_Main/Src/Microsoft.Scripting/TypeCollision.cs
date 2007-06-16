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
    /// methods that should logically have arity as a portion of their name. For eg:
    ///      System.EventHandler and System.EventHandler[T]
    ///      System.Nullable and System.Nullable[T]
    ///      System.IComparable and System.IComparable[T]
    /// 
    /// The TypeCollision provides an indexer but also is a real type.  When used
    /// as a real type it is the non-generic form of the type.
    /// 
    /// The indexer allows the user to disambiguate between the generic and
    /// non-generic versions.  Therefore users must always provide additional
    /// information to get the generic version.
    /// </summary>
    public sealed class TypeCollision : ICallableWithCodeContext, ICustomMembers, IConstructorWithCodeContext {
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
            Debug.Assert(GetNormalizedTypeName(_type.Name) == GetNormalizedTypeName(t.Name));

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

        /// <param name="existingTypeEntity">The merged list so far. Could be null</param>
        /// <param name="newTypeEntity">The new type(s) to add to the merged list</param>
        /// <returns>The merged list. Could be a DynamicType or a TypeCollision</returns>
        public static IConstructorWithCodeContext UpdateTypeEntity(IConstructorWithCodeContext existingTypeEntity, IConstructorWithCodeContext newTypeEntity) {
            Debug.Assert(newTypeEntity != null && ((newTypeEntity is DynamicType) || (newTypeEntity is TypeCollision)));
            Debug.Assert(existingTypeEntity == null || (existingTypeEntity is DynamicType) || (existingTypeEntity is TypeCollision));

            if (existingTypeEntity == null) {
                return newTypeEntity;
            }

            TypeCollision exitingTypeCollision = existingTypeEntity as TypeCollision;
            TypeCollision newTypeCollision = newTypeEntity as TypeCollision;
            Type newRootType;
            if (newTypeCollision != null) newRootType = newTypeCollision._type;
            else newRootType = (newTypeEntity as DynamicType).UnderlyingSystemType;
            TypeCollision typeCollision = exitingTypeCollision;

            if (exitingTypeCollision != null) {
                // we've collided names before...
                Debug.Assert(GetNormalizedTypeName(exitingTypeCollision._type.Name) == GetNormalizedTypeName(newRootType.Name));

                if (!newRootType.ContainsGenericParameters) {
                    // we're replacing the existing non-generic type or moving some random generic type 
                    // from the "base" reflected type into the list of generics.
                    typeCollision = exitingTypeCollision.CloneWithNewBase(newRootType);
                } else {
                    // newType is a generic type.  we just need to add it to the list, or replace an existing type 
                    // of the same arity.
                    exitingTypeCollision.UpdateType(newRootType);
                }

            } else {
                // first time collision on this name. We need to provide the type collision to disambiguate.  
                // The non-generic is exposed by default, and the generic gets added to the list to disambiguate.
                Type existingType = (existingTypeEntity as DynamicType).UnderlyingSystemType;
                Debug.Assert(GetNormalizedTypeName(existingType.Name) == GetNormalizedTypeName(newRootType.Name));

                if (existingType.ContainsGenericParameters) {
                    // existing type has generics, append it.
                    typeCollision = new TypeCollision(newRootType);
                    typeCollision.UpdateType(existingType);
                } else if (newRootType.ContainsGenericParameters) {
                    // new type has generics, append it.
                    typeCollision = new TypeCollision(existingType);
                    typeCollision.UpdateType(newRootType);
                } else {
                    // neither type has generics, replace the old
                    // non-generic type w/ the new non-generic type.
                    return DynamicHelpers.GetDynamicTypeFromType(newRootType);
                }
            }

            if (newTypeCollision != null) {
                // We have processed newTypeCollision._type. Now, we need to process newTypeCollision._types
                foreach (DynamicType t in newTypeCollision._types) {
                    typeCollision.UpdateType(t.UnderlyingSystemType);
                }
            }

            return typeCollision;
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

        /// <summary>
        /// This will return null if all the colliding types are generic
        /// </summary>
        public Type NonGenericType {
            get {
                if (_type.ContainsGenericParameters) {
                    return null;
                }
                return _type;
            }
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

        public static string GetNormalizedTypeName(Type type) {
            string name = type.Name;
            if (type.IsGenericType) {
                return GetNormalizedTypeName(name);
            }
            return name;
        }

        public static string GetNormalizedTypeName(string typeName) {
            Debug.Assert(typeName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
            int backtick = typeName.IndexOf(Utils.Reflection.GenericArityDelimiter);
            if (backtick != -1) return typeName.Substring(0, backtick);
            return typeName;
        }

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            if (DynamicHelpers.GetDynamicTypeFromType(typeof(TypeCollision)).TryGetMember(context, this, name, out value)) {
                return true;
            }

            return DynamicHelpers.GetDynamicTypeFromType(_type).TryGetCustomMember(context, name, out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetCustomMember(context, name, out value);
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            DynamicHelpers.GetDynamicTypeFromType(_type).SetCustomMember(context, name, value);
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            return DynamicHelpers.GetDynamicTypeFromType(_type).DeleteCustomMember(context, name);
        }

        public IList<object> GetCustomMemberNames(CodeContext context) {
            return DynamicHelpers.GetDynamicTypeFromType(_type).GetCustomMemberNames(context);
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            return DynamicHelpers.GetDynamicTypeFromType(_type).GetCustomMemberDictionary(context);
        }

        #endregion

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
