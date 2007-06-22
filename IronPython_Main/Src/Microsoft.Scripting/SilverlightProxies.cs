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
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Collections;

#if SILVERLIGHT // Proxies

namespace System {

    namespace Diagnostics {

        namespace CodeAnalysis {
            [AttributeUsageAttribute(AttributeTargets.All, AllowMultiple = true)]
            public class SuppressMessageAttribute : Attribute {
                public SuppressMessageAttribute(string category, string checkId) { }
                public string Scope { get { return null; } set { } }
                public string Target { get { return null; } set { } }
                public string MessageId { get { return null; } set { } }
            }
        }
    }

    namespace Runtime.InteropServices {
        public sealed class DefaultParameterValueAttribute : Attribute {
            public DefaultParameterValueAttribute(object value) { }
        }
    }

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

        public enum EditorBrowsableState {
            Advanced
        }

        public class EditorBrowsableAttribute : Attribute {
            public EditorBrowsableAttribute(EditorBrowsableState state) { }
        }
    }

    namespace CodeDom.Compiler {
        public class GeneratedCodeAttribute : Attribute {
            public GeneratedCodeAttribute(string tool, string version) { }
        }
    }

    public class SerializableAttribute : Attribute {
    }

    public class NonSerializedAttribute : Attribute {
    }

    public interface ISerializable {
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


    // BitArray was removed from CoreCLR, recreate a simple version here
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
                throw new IndexOutOfRangeException();
            }
            int elem = index / 32, mask = 1 << (index % 32);
            return (_data[elem] & mask) != 0;
        }

        public void Set(int index, bool value) {
            if (index < 0 || index >= _count) {
                throw new IndexOutOfRangeException();
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

        public void And(BitArray bits) {
            if (bits == null) {
                throw new ArgumentNullException();
            } else if (bits._count != _count) {
                throw new ArgumentException("Array lengths differ");
            }
            for (int i = 0; i < _data.Length; ++i) {
                _data[i] &= bits._data[i];
            }
        }

        public void Or(BitArray bits) {
            if (bits == null) {
                throw new ArgumentNullException();
            } else if (bits._count != _count) {
                throw new ArgumentException("Array lengths differ");
            }
            for (int i = 0; i < _data.Length; ++i) {
                _data[i] |= bits._data[i];
            }
        }

        public BitArray Not() {
            for (int i = 0; i < _data.Length; ++i) {
                _data[i] = ~_data[i];
            }
            return this;
        }
    }

}

#endif