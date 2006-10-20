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
using System.Collections.Generic;
using System.Diagnostics;

namespace IronPython.Compiler {
    // Name is a symbol used to refer to a variable or an attribute.
    // A Name is looked up in the Namespace to determine the actual Slot that it refers to.

    public class Name {
        public static Name Make(String source) {
            return Make(source.ToCharArray(), 0, source.Length);
        }

        public static string[] ToStrings(Name[] names) {
            string[] ret = new string[names.Length];
            for (int i = 0; i < names.Length; i++) {
                if (names[i] == null) ret[i] = null;
                else ret[i] = names[i].GetString();
            }
            return ret;
        }

        public static Name Make(char[] source, int start, int end) {
            NameKey key = new NameKey(source, start, end);
            Name ret;
            if(!names.TryGetValue(key, out ret)) {
                ret = key.makeName();
                key = new NameKey(ret.name, 0, ret.name.Length);
                names[key] = ret;
            }
            return ret;
        }

        private static int hash(char[] source, int start, int end) {
            int ret = 0;
            for (int i = start; i < end; i++) {
                ret = 31 * ret + source[i];
            }
            return ret;
        }

        private static Dictionary<NameKey, Name> names = new Dictionary<NameKey, Name>();
        public static Name None = Name.Make("None");

        // in the current design, these will hold on the too much text
        private class NameKey {
            private char[] name;
            private int start, end, length;
            private int hashcode;
            public NameKey(char[] name, int start, int end) {
                this.name = name;
                this.start = start;
                this.end = end;
                this.length = end - start;
                this.hashcode = hash(name, start, end);
            }

            public Name makeName() {
                char[] chs = new char[length];
                for (int i = chs.Length - 1, j = end - 1; i >= 0; i--, j--) {
                    chs[i] = name[j];
                }
                // need to truncate name here...
                return new Name(chs, hashcode);
            }

            public override int GetHashCode() {
                return hashcode;
            }

            public override bool Equals(object other) {
                NameKey okey = other as NameKey;
                if (okey == null) return false;

                if (length != okey.length) return false;
                // TODO check performance of making locals of fields
                for (int i = 0; i < length; i++) {
                    if (name[i + start] != okey.name[i + okey.start]) return false;
                }
                return true;
            }
        }


        private char[] name;
        private int hashcode;
        private int stringHashCode = -1;
        private string str = null;

        private Name(char[] name, int hashcode) {
            this.name = name;
            this.hashcode = hashcode;
        }
        public override int GetHashCode() {
            return hashcode;
        }
        public override bool Equals(object obj) {
            Debug.Assert(obj is Name ? ((Name)obj).GetString().Equals(this.GetString()) == (obj == (object)this)
                : true, "two Names for same string: " + GetString());
            return (object)this == obj;
        }

        public String GetString() {
            string ret = str;
            if (ret == null) {
                ret = new string(name);
                str = ret;
                //Console.WriteLine("string: " + ret);
            }
            return ret;
        }

        public int GetStringHashCode() {
            int ret = stringHashCode;
            if (ret == -1) {
                ret = GetString().GetHashCode();
                stringHashCode = ret;
            }
            return ret;
        }

        public override string ToString() {
            return "Name(" + new String(name) + ")";
        }
    }
}
