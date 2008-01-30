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
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Collections;
using Microsoft.Scripting.Utils;
using System.Diagnostics;

#if SILVERLIGHT // Stubs

namespace System {

    namespace Runtime.InteropServices {
        public sealed class DefaultParameterValueAttribute : Attribute {
            public DefaultParameterValueAttribute(object value) { }
        }
    }

    // We reference these namespaces via "using"
    // We don't actually use them because the code is #if !SILVERLIGHT
    // Rather than fix the usings all over the place, just define these here
    namespace Runtime.Remoting { class Dummy {} }
    namespace Security.Policy { class Dummy {} }
    namespace Xml.XPath { class Dummy {} }

    namespace Reflection {
        public enum PortableExecutableKinds {
            ILOnly = 0
        }

        public enum ImageFileMachine {
            I386 = 1
        }
    }

    namespace ComponentModel {

        public class WarningException : SystemException {
            public WarningException(string message) : base(message) { }
        }
    }

    public class SerializableAttribute : Attribute {
    }

    public class NonSerializedAttribute : Attribute {
    }

    namespace Runtime.Serialization {
        public interface ISerializable {
        }
    }

    [Flags]
    public enum StringSplitOptions {
        None = 0,
        RemoveEmptyEntries = 1,
    }

    public enum ConsoleColor {
        Black = 0,
        DarkBlue = 1,
        DarkGreen = 2,
        DarkCyan = 3,
        DarkRed = 4,
        DarkMagenta = 5,
        DarkYellow = 6,
        Gray = 7,
        DarkGray = 8,
        Blue = 9,
        Green = 10,
        Cyan = 11,
        Red = 12,
        Magenta = 13,
        Yellow = 14,
        White = 15,
    }

    // BitArray, LinkedList<T> and LinkedListNode<T> were removed from CoreCLR
    // Recreating simple versions here.

    namespace Collections {
        #region BitArray
        
        public class BitArray {
            readonly int[] _data;
            readonly int _count;

            public int Length {
                get { return _count; }
            }

            public int Count {
                get { return _count; }
            }

            public BitArray(int count)
                : this(count, false) {
            }

            public BitArray(int count, bool value) {
                this._count = count;
                this._data = new int[(count + 31) / 32];
                if (value) {
                    Not();
                }
            }

            public BitArray(BitArray bits) {
                _count = bits._count;
                _data = (int[])bits._data.Clone();
            }

            public bool Get(int index) {
                if (index < 0 || index >= _count) {
                    throw new ArgumentOutOfRangeException();
                }
                int elem = index / 32, mask = 1 << (index % 32);
                return (_data[elem] & mask) != 0;
            }

            public void Set(int index, bool value) {
                if (index < 0 || index >= _count) {
                    throw new ArgumentOutOfRangeException();
                }
                int elem = index / 32, mask = 1 << (index % 32);
                if (value) {
                    _data[elem] |= mask;
                } else {
                    _data[elem] &= ~mask;
                }
            }

            public void SetAll(bool value) {
                int set = value ? -1 : 0;
                for (int i = 0; i < _data.Length; ++i) {
                    _data[i] = set;
                }
            }

            public BitArray And(BitArray bits) {
                if (bits == null) {
                    throw new ArgumentNullException();
                } else if (bits._count != _count) {
                    throw new ArgumentException("Array lengths differ");
                }
                for (int i = 0; i < _data.Length; ++i) {
                    _data[i] &= bits._data[i];
                }

                return this;
            }

            public BitArray Or(BitArray bits) {
                if (bits == null) {
                    throw new ArgumentNullException();
                } else if (bits._count != _count) {
                    throw new ArgumentException("Array lengths differ");
                }
                for (int i = 0; i < _data.Length; ++i) {
                    _data[i] |= bits._data[i];
                }

                return this;
            }

            public BitArray Not() {
                for (int i = 0; i < _data.Length; ++i) {
                    _data[i] = ~_data[i];
                }
                return this;
            }
        }
        #endregion
    }

    namespace Collections.Generic {
        #region LinkedList<T>, LinkedListNode<T>

        public class LinkedListNode<T> {
            internal LinkedList<T> _list;
            internal LinkedListNode<T> _previous, _next;
            internal T _value;

            internal LinkedListNode(LinkedList<T> list, T value) {
                _list = list;
                _value = value;
            }

            public LinkedListNode(T value) {
                _value = value;
            }

            public LinkedList<T> List {
                get { return _list; }
            }

            public LinkedListNode<T> Previous {
                get { return _previous; }
            }

            public LinkedListNode<T> Next {
                get { return _next; }
            }

            public T Value {
                get { return _value; }
            }

        }

        public class LinkedList<T> {
            private LinkedListNode<T> _first;
            private LinkedListNode<T> _last;

            public LinkedList() { }

            public LinkedListNode<T> Last {
                get { return _last; }
            }

            public LinkedListNode<T> First {
                get { return _first; }
            }

            public void AddFirst(T value) {
                AddFirst(new LinkedListNode<T>(value));
            }

            public void AddFirst(LinkedListNode<T> node) {
                CheckInvariants();

                if (node == null) {
                    throw new ArgumentNullException("node");
                }
                if (node._list != null) {
                    throw new InvalidOperationException("node is already a member of another list");
                }

                node._list = this;
                node._next = _first;
                if (_first != null) {
                    _first._previous = node;
                }
                _first = node;
                if (_last == null) {
                    _last = node;
                }

                CheckInvariants();
            }

            public void AddLast(T value) {
                AddLast(new LinkedListNode<T>(value));
            }

            public void AddLast(LinkedListNode<T> node) {
                CheckInvariants();

                if (node == null) {
                    throw new ArgumentNullException("node");
                }
                if (node._list != null) {
                    throw new InvalidOperationException("node is already a member of another list");
                }

                node._list = this;
                node._previous = _last;
                if (_last != null) {
                    _last._next = node;
                }
                _last = node;
                if (_first == null) {
                    _first = node;
                }

                CheckInvariants();
            }

            public void Remove(LinkedListNode<T> node) {
                CheckInvariants();

                if (node == null) {
                    throw new ArgumentNullException("node");
                }
                if (node._list != this) {
                    throw new InvalidOperationException("node is not a member of this list");
                }

                if (node._previous == null) {
                    _first = node._next;
                } else {
                    node._previous._next = node._next;
                }

                if (node._next == null) {
                    _last = node._previous;
                } else {
                    node._next._previous = node._previous;
                }

                node._list = null;
                node._previous = null;
                node._next = null;

                CheckInvariants();
            }

            [Conditional("DEBUG")]
            private void CheckInvariants() {
                if (_first == null || _last == null) {
                    // empty list
                    Debug.Assert(_first == null && _last == null);
                } else if (_first == _last) {
                    // one element
                    Debug.Assert(_first._next == null && _first._previous == null && _first._list == this);
                } else {
                    Debug.Assert(_first._previous == null && _first._list == this);
                    Debug.Assert(_last._next == null && _last._list == this);
                    if (_first._next == _last || _last._previous == _first) {
                        // two elements
                        Debug.Assert(_first._next == _last && _last._previous == _first);
                    } else if (_first._next == _last._previous) {
                        // three elements
                        Debug.Assert(_first._next._next == _last && _last._previous._previous == _first);
                    }
                }
            }
        }

        #endregion
    }
}

#endif

#if !SPECSHARP

namespace Microsoft.Contracts {
    [Conditional("SPECSHARP"), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    internal sealed class StateIndependentAttribute : Attribute {
    }

    [Conditional("SPECSHARP"), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    internal sealed class PureAttribute : Attribute {
    }

    [Conditional("SPECSHARP"), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    internal sealed class ConfinedAttribute : Attribute {
    }

    [Conditional("SPECSHARP"), AttributeUsage(AttributeTargets.Field)]
    internal sealed class StrictReadonlyAttribute : Attribute {
    }

    internal static class NonNullType {
        [DebuggerStepThrough]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public static void AssertInitialized<T>(T[] array) where T : class {
            Assert.NotNullItems<T>(array);
        }
    }
}

#endif