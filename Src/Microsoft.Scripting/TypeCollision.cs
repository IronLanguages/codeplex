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
        private Dictionary<int, Type> _typesByArity;

        private TypeCollision(Type t1, Type t2) {
            Debug.Assert(GetNormalizedTypeName(t1) == GetNormalizedTypeName(t2));
            _typesByArity = new Dictionary<int, Type>();

            Debug.Assert(GetGenericArity(t1) != GetGenericArity(t2));
            _typesByArity[GetGenericArity(t1)] = t1;
            _typesByArity[GetGenericArity(t2)] = t2;
        }

        private TypeCollision(Type t1, TypeCollision tc2) {
            Debug.Assert(GetNormalizedTypeName(t1) == tc2.NormalizedName);
            // We grab _typesByArity of tc2, which means that tc2 is effectively dead. This is OK
            // since UpdateTypeEntity can mutate the incoming TypeCollisions or return a new copy.
            _typesByArity = tc2._typesByArity;

            if (!_typesByArity.ContainsKey(GetGenericArity(t1))) {
                _typesByArity[GetGenericArity(t1)] = t1;
            }
        }

        public override string ToString() {
            StringBuilder repr = new StringBuilder(base.ToString());
            repr.Append(":" + NormalizedName + "(");

            bool pastFirstType = false;
            foreach (Type type in Types) {
                if (pastFirstType) {
                    repr.Append(", ");
                }
                repr.Append(type.Name);
                pastFirstType = true;
            }
            repr.Append(")");

            return repr.ToString();
        }

        /// <summary>
        /// Indexer for generic parameter resolution.  We bind to one of the generic versions
        /// available in this type collision.  A user can also do someType[()] to force to
        /// bind to the non-generic version, but we will always present the non-generic version
        /// when no bindings are available.
        /// </summary>
        public object this[params object [] index] {
            get {
                Type[] bindRequest = new Type[index.Length] ;
                for (int i = 0; i < bindRequest.Length; i++) {
                    bindRequest[i] = index[i] as Type ?? ((DynamicType)index[i]).UnderlyingSystemType;
                }

                Type typeWithMatchingArity;
                if (!_typesByArity.TryGetValue(bindRequest.Length, out typeWithMatchingArity)) {
                    throw new ArgumentException(String.Format("could not find compatible generic type for {0} type arguments", bindRequest.Length));
                }

                Type ret;
                if (bindRequest.Length == 0) {
                    ret = typeWithMatchingArity;
                } else {
                    ret = typeWithMatchingArity.MakeGenericType(bindRequest);
                }

                return DynamicHelpers.GetDynamicTypeFromType(ret);
            }
        }

        void AddType(Type t) {
            Debug.Assert(NormalizedName == GetNormalizedTypeName(t.Name));

            _typesByArity[GetGenericArity(t)] = t;
        }

        /// <param name="existingTypeEntity">The merged list so far. Could be null</param>
        /// <param name="newTypeEntity">The new type(s) to add to the merged list</param>
        /// <returns>The merged list. Could be a DynamicType or a TypeCollision</returns>
        public static IConstructorWithCodeContext UpdateTypeEntity(
            IConstructorWithCodeContext existingTypeEntity, 
            IConstructorWithCodeContext newTypeEntity) {

            Debug.Assert(newTypeEntity != null && ((newTypeEntity is DynamicType) || (newTypeEntity is TypeCollision)));
            Debug.Assert(existingTypeEntity == null || (existingTypeEntity is DynamicType) || (existingTypeEntity is TypeCollision));

            if (existingTypeEntity == null) {
                return newTypeEntity;
            }

            DynamicType existingDynamicType = existingTypeEntity as DynamicType;
            DynamicType newDynamicType = newTypeEntity as DynamicType;
            Type existingType = (existingDynamicType != null) ? existingDynamicType.UnderlyingSystemType : null;
            Type newType = (newDynamicType != null) ? newDynamicType.UnderlyingSystemType : null;
            TypeCollision existingTypeCollision = existingTypeEntity as TypeCollision;
            TypeCollision newTypeCollision = newTypeEntity as TypeCollision;
#if DEBUG
            string existingEntityNormalizedName = (existingType != null) ? GetNormalizedTypeName(existingType)
                                                                         : existingTypeCollision.NormalizedName;
            string newEntityNormalizedName = (newType != null) ? GetNormalizedTypeName(newType)
                                                               : newTypeCollision.NormalizedName;
            Debug.Assert(existingEntityNormalizedName == newEntityNormalizedName);
#endif

            if (existingType != null) {
                if (newType != null) {
                    if (GetGenericArity(existingType) == GetGenericArity(newType)) {
                        return newDynamicType;
                    } else {
                        return new TypeCollision(existingType, newType);
                    }
                } else {
                    return new TypeCollision(existingType, newTypeCollision);
                }
            } else {
                if (newType != null) {
                    existingTypeCollision.AddType(newType);
                    return existingTypeCollision;
                } else {
                    foreach (Type type in newTypeCollision.Types) {
                        existingTypeCollision.AddType(type);
                    }
                    return existingTypeCollision;
                }
            }
        }

        /// <summary> Gets the arity of generic parameters</summary>
        private static int GetGenericArity(Type type) {
            if (!type.IsGenericType) {
                return 0;
            }

            Debug.Assert(type.IsGenericTypeDefinition);
            return type.GetGenericArguments().Length;
        }

        /// <summary>
        /// This will throw an exception if all the colliding types are generic
        /// </summary>
        public Type NonGenericType {
            get {
                Type nonGenericType;
                if (TryGetNonGenericType(out nonGenericType)) {
                    return nonGenericType;
                }

                throw new TypeLoadException("The operation requires a non-generic type for " + NormalizedName + ", but this represents generic types only");
            }
        }

        public bool TryGetNonGenericType(out Type nonGenericType) {
            return _typesByArity.TryGetValue(0, out nonGenericType);
        }

        private Type SampleType {
            get {
                IEnumerator<Type> e = Types.GetEnumerator();
                e.MoveNext();
                return e.Current;
            }
        }

        public IEnumerable<Type> Types {
            get {
                return _typesByArity.Values;
            }
        }

        internal string NormalizedName {
            get {
                return GetNormalizedTypeName(SampleType);
            }
        }

        internal static string GetNormalizedTypeName(Type type) {
            string name = type.Name;
            if (type.IsGenericType) {
                return GetNormalizedTypeName(name);
            }
            return name;
        }

        internal static string GetNormalizedTypeName(string typeName) {
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

            return DynamicHelpers.GetDynamicTypeFromType(NonGenericType).TryGetCustomMember(context, name, out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetCustomMember(context, name, out value);
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            DynamicHelpers.GetDynamicTypeFromType(NonGenericType).SetCustomMember(context, name, value);
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            return DynamicHelpers.GetDynamicTypeFromType(NonGenericType).DeleteCustomMember(context, name);
        }

        public IList<object> GetCustomMemberNames(CodeContext context) {
            return DynamicHelpers.GetDynamicTypeFromType(NonGenericType).GetCustomMemberNames(context);
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            return DynamicHelpers.GetDynamicTypeFromType(NonGenericType).GetCustomMemberDictionary(context);
        }

        #endregion

        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, params object[] args) {
            return DynamicHelpers.CallWithContext(context, DynamicHelpers.GetDynamicTypeFromType(NonGenericType), args);
        }

        #endregion

        #region IConstructorWithCodeContext Members

        public object Construct(CodeContext context, params object[] args) {
            return RuntimeHelpers.Construct(context, DynamicHelpers.GetDynamicTypeFromType(NonGenericType), args);
        }

        #endregion
    }
}
