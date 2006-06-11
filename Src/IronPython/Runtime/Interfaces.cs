/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace IronPython.Runtime {
    public interface ICallable {
        object Call(params object[] args);
    }

    public interface ITryCallable : ICallable {
        bool TryCall(object[] args, out object ret);
    }

    public interface IFancyCallable {
        object Call(ICallerContext context, object[] args, string[] names);
    }

    public interface ICallableWithCallerContext {
        object Call(ICallerContext context, object[] args);
    }

    public interface IExtensible<T> {
        T Value {
            get;
        }
    }

    public interface IContextAwareMember {
        bool IsVisible(ICallerContext context);
    }

    public interface ICallerContext {
        PythonModule Module { get; }
        SystemState SystemState { get;} 
        object Locals { get; }
        IAttributesDictionary Globals { get; }
        object GetStaticData(int index);
        bool TrueDivision { get; set; }
        CallerContextFlags ContextFlags { get; set; }
        IronPython.Compiler.CompilerContext CreateCompilerContext();
    }

    public interface IModuleScope {
        object GetGlobal(SymbolId symbol);
        bool TryGetGlobal(SymbolId name, out object value);
        void SetGlobal(SymbolId symbol, object value);
        void DelGlobal(SymbolId symbol);
    }

    public interface IModuleEnvironment : ICallerContext, IModuleScope {
    }

    public interface IDynamicObject {
        DynamicType GetDynamicType();
    }

    public interface ISuperDynamicObject : IDynamicObject {
        /// <returns>This can return null if the object has no attributes.</returns>
        IAttributesDictionary GetDict();
        bool SetDict(IAttributesDictionary dict);

        void SetDynamicType(UserType newType);  //??? maybe PythonType
    }

    public interface ICustomBaseAccess {
        bool TryGetBaseAttr(ICallerContext context, SymbolId name, out object value);
    }

    /// <summary>
    /// This interface objects to specify how to look up attributes (for code like "obj.attr").
    /// If an object does not implement this interface, its DynamicType is then asked to find the attribute.
    /// See Ops.GetAttrNames() for how this works.
    /// </summary>
    public interface ICustomAttributes {
        bool TryGetAttr(ICallerContext context, SymbolId name, out object value);
        void SetAttr(ICallerContext context, SymbolId name, object value);
        void DeleteAttr(ICallerContext context, SymbolId name);

        /// <returns>The returned List contains all the attributes of the instance. ie. all the keys in the 
        /// dictionary of the object. Note that it can contain objects that are not strings. Such keys can be
        /// added using syntax like:
        ///     obj.__dict__[100] = someOtherObject
        /// </returns>
        List GetAttrNames(ICallerContext context);  
        IDictionary<object, object> GetAttrDict(ICallerContext context);
    }

    public interface IAttributesInjector {
        bool TryGetAttr(object self, SymbolId name, out object value);
        List GetAttrNames(object self);
    }

    /// <summary>
    /// This interface represents a dictionary that can be accessed using symbols and also arbitrary objects.
    /// This should conceptually inherit from IDictionary<object, object>, but we do not do that as we want the default indexer
    /// property to be indexed by SymbolId, not by object.
    /// </summary>
    public interface IAttributesDictionary : IEnumerable<KeyValuePair<object, object>> {
        ///
        /// Access using SymbolId keys
        ///
        void Add(SymbolId name, object value);
        bool TryGetValue(SymbolId name, out object value);
        bool Remove(SymbolId name);
        bool ContainsKey(SymbolId name);
        object this[SymbolId name] { get; set; }
        // This returns just the attributes that are keyed using SymbolIds. It will ignore any object-keyed attributes
        IDictionary<SymbolId, object> SymbolAttributes { get; }

        ///
        // Access using object keys
        ///
        void AddObjectKey(object name, object value);
        bool TryGetObjectValue(object name, out object value);
        bool RemoveObjectKey(object name);
        bool ContainsObjectKey(object name);
        IDictionary<object, object> AsObjectKeyedDictionary();

        int Count { get; }
        ICollection<object> Keys { get; }
    }

    public interface ICodeFormattable {
        string ToCodeString();
    }

    public interface IDescriptor {
        [PythonName("__get__")]
        object GetAttribute(object instance, object owner);
    }

    /// <summary>
    /// Implements the IDataDescriptor interface.  Descriptors provide an opportunity
    /// for attribute access to be intercepted on a per-attribute basis.  Data descriptors
    /// provide this functionlity for writable attributes.
    /// 
    /// Internally we also support "static" descriptors which act at a class level instead of 
    /// just an instance level - these are ReflectedMethods and ReflectedPropertys that are static.  
    /// This is the reason for the bool return value, and is a feature not available to pure-Python code.
    /// If the descriptor returns true then the set has been handled, otherwise it has not.
    /// </summary>
    public interface IDataDescriptor : IDescriptor {
        [PythonName("__set__")]
        bool SetAttribute(object instance, object value);
        [PythonName("__delete__")]
        bool DeleteAttribute(object instance);
    }

    public interface IPythonContainer {
        [PythonName("__len__")]
        int GetLength();
        [PythonName("__contains__")]
        bool ContainsValue(object value);
        //???object __iter__(); //??? this vs. IEnumerable
    }


    public interface ISequence : IPythonContainer {
        [PythonName("__add__")]
        object AddSequence(object other);  //??? require that other be ISequence
        [PythonName("__mul__")]
        object MultiplySequence(object count);

        object this[int index] {
            get;
        }
        object this[Slice slice] {
            get;
        }

        // deprecated __getslice__ method
        [PythonName("__getslice__")]
        object GetSlice(int start, int stop);
    }

    public interface IMutableSequence : ISequence {
        new object this[int index] {
            set;
        }
        new object this[Slice slice] {
            set;
        }

        [PythonName("__delitem__")]
        void DeleteItem(int index);
        [PythonName("__delitem__")]
        void DeleteItem(Slice slice);

        // deprecated __setslice__ and __delslice__ methods
        [PythonName("__setslice__")]
        void SetSlice(int start, int stop, object value);
        [PythonName("__delslice__")]
        void DeleteSlice(int start, int stop);
    }

    public interface IMapping : IPythonContainer {
        [PythonName("get")]
        object GetValue(object key);
        [PythonName("get")]
        object GetValue(object key, object defaultValue);

        bool TryGetValue(object key, out object value);

        object this[object key] {
            get;
            set;
        }
        [PythonName("__delitem__")]
        void DeleteItem(object key);
    }

    public interface IRichComparable : IRichEquality {
        object CompareTo(object other);
        object GreaterThan(object other);
        object LessThan(object other);
        object GreaterThanOrEqual(object other);
        object LessThanOrEqual(object other);
    }

    // __hash__ in Python does not work for mutable objects, and __eq__ implements deep comparion for 
    // many of the built-in types like "list". However, object.GetHashCode needs to work for all objects
    // for interoperability with non-Python code as System.Collections.Hashtable uses it.
    // (Note that this works because the default implementation of object.Equals implements 
    // shallow comparison). 
    // IRichEquality allows different implementation for the two versions of hashing/equality.
    // By default, Python hooks up GetHashCode and __hash__ for Python types. However,
    // a mutable Python type can explicitly throw a TypeError from __hash__/PythonGetHashCode
    // (by calling Ops.TypeErrorForUnhashableType) but it should return some hash value from object.GetHashCode. 
    // This allows hashing behavior consistent with the environment the type is used in (ie. Python vs. non-Python).
    public interface IRichEquality {
        object RichGetHashCode();
        object RichEquals(object other);
        object RichNotEquals(object other);
    }

    public interface IWeakReferenceable {
        WeakRefTracker GetWeakRef();
        void SetWeakRef(WeakRefTracker value);
    }

    public interface IProxyObject {
        object Target { get; }
    }

    public interface ICustomExceptionConversion {
        object ToPythonException();
    }
}
