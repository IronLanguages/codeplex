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
using System.Text;
using System.Runtime.InteropServices;

using System.Reflection;

using System.Globalization;
using System.Threading;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using IronPython.Compiler;

namespace IronPython.Runtime.Operations {
    /// <summary>
    /// ExtensibleString is the base class that is used for types the user defines
    /// that derive from string.  It carries along with it the string's value and
    /// our converter recognizes it as a string.
    /// </summary>
    public class ExtensibleString : ICodeFormattable, IRichComparable, ISequence {
        private string self;

        public ExtensibleString() { this.self = String.Empty; }
        public ExtensibleString(string self) { this.self = self; }

        public string Value {
            get { return self; }
        }

        [PythonName("__str__")]
        public override string ToString() {
            return self;
        }


        public override int GetHashCode() {
            return self.GetHashCode();
        }

        #region ICodeFormattable Members

        [PythonName("__repr__")]
        public virtual string ToCodeString() {
            return StringOps.Quote(self);
        }

        #endregion

        #region IRichComparable Members
        public object CompareTo(object other) {
            return StringOps.Compare(self, other);
        }

        public object GreaterThan(object other) {
            object res = StringOps.Compare(self, other);
            if (res is int) return ((int)res) > 0;
            return Ops.NotImplemented;
        }

        public object LessThan(object other) {
            object res = StringOps.Compare(self, other);
            if (res is int) return ((int)res) < 0;
            return Ops.NotImplemented;
        }

        public object GreaterThanOrEqual(object other) {
            object res = StringOps.Compare(self, other);
            if (res is int) return ((int)res) >= 0;
            return Ops.NotImplemented;
        }

        public object LessThanOrEqual(object other) {
            object res = StringOps.Compare(self, other);
            if (res is int) return ((int)res) <= 0;
            return Ops.NotImplemented;
        }

        #endregion

        #region IRichComparable

        [PythonName("__hash__")]
        public virtual object RichGetHashCode() {
            return Ops.Int2Object(GetHashCode());
        }


        [PythonName("__eq__")]
        public virtual object RichEquals(object other) {
            if (other == null) return Ops.FALSE;

            ExtensibleString es = other as ExtensibleString;
            if (es != null) return Ops.Bool2Object(self == es.self);
            string os = other as string;
            if (os != null) return Ops.Bool2Object(self == os);

            return Ops.NotImplemented;
        }

        [PythonName("__ne__")]
        public virtual object RichNotEquals(object other) {
            object res = RichEquals(other);
            if (res != Ops.NotImplemented) return Ops.Not(res);

            return Ops.NotImplemented;
        }

        #endregion

        #region ISequence Members

        [PythonName("__add__")]
        public virtual object AddSequence(object other) {
            if (other is string) return self + (string)other;
            else if (other is ExtensibleString) return self + ((ExtensibleString)other).self;

            throw Ops.TypeErrorForBadInstance("cannot add string and {0}", other);
        }

        [PythonName("__mul__")]
        public virtual object MultiplySequence(object count) {
            return Ops.MultiplySequence<string>(StringOps.Multiply, self, count);
        }

        public virtual object this[int index] {
            get { return Ops.Char2String(self[index]); }
        }

        public object this[Slice slice] {
            get { return StringOps.__getitem__(self, slice); }
        }

        public object GetSlice(int start, int stop) {
            return StringOps.GetSlice(self, start, stop);
        }

        #endregion

        #region IPythonContainer Members

        [PythonName("__len__")]
        public virtual int GetLength() {
            return self.Length;
        }

        [PythonName("__contains__")]
        public virtual bool ContainsValue(object value) {
            if (value is string) return self.Contains((string)value);
            else if (value is ExtensibleString) return self.Contains(((ExtensibleString)value).self);

            throw Ops.TypeErrorForBadInstance("expected string, got {0}", value);
        }

        #endregion

    }

    /// <summary>
    /// StringOps is the static class that contains the methods defined on strings, i.e. 'abc'
    /// 
    /// Here we define all of the methods that a Python user would see when doing dir('abc').
    /// If the user is running in a CLS aware context they will also see all of the methods
    /// defined in the CLS System.String type.
    /// </summary>
    public static class StringOps {

        internal const int LowestUnicodeValue = 0x7f;

        static ReflectedType StringType;
        public static ReflectedType MakeDynamicType() {
            if (StringType == null) {
                OpsReflectedType ret = new OpsReflectedType("str", typeof(string), typeof(StringOps), typeof(ExtensibleString), new CallTarget2(FastNew));
                if (Interlocked.CompareExchange<ReflectedType>(ref StringType, ret, null) == null)
                    return ret;
            }
            return StringType;
        }

        internal static object FastNew(object context, object x) {
            if (x == null) {
                return "None";
            }
            if (x is string) {
                // check ascii
                string s = (string)x;
                for (int i = 0; i < s.Length; i++) {
                    if (s[i] > '\x80')
                        return StringOps.Make(
                            (ICallerContext)context,
                            (DynamicType)Ops.GetDynamicTypeFromType(typeof(String)),
                            s,
                            null,
                            "strict"
                            );
                }
                return s;
            }
            return Ops.ToString(x);
        }

        private static Dictionary<string, EncodingInfo> codecs;

        #region Python Constructors

        [PythonName("__new__")]
        public static object Make(ICallerContext context, DynamicType cls) {
            if (cls == TypeCache.String) {
                return "";
            } else {
                return cls.ctor.Call(cls);
            }
        }

        [PythonName("__new__")]
        public static object Make(ICallerContext context, DynamicType cls, object @object) {
            if (cls == TypeCache.String) {
                return FastNew(context, @object);
            } else {
                return cls.ctor.Call(cls, @object);
            }
        }

        [PythonName("__new__")]
        public static object Make(ICallerContext context, DynamicType cls, string @string) {
            if (cls == TypeCache.String) {
                return Decode(context, @string);
            } else {
                return cls.ctor.Call(cls, @string);
            }
        }

        [PythonName("__new__")]
        public static object Make(ICallerContext context, DynamicType cls,
            string @string,
            [DefaultParameterValue(null)] string encoding,
            [DefaultParameterValue("strict")]string errors) {

            if (cls == TypeCache.String) {
                return Decode(context, @string, encoding, errors);
            } else {
                return cls.ctor.Call(cls, @string, encoding, errors);
            }
        }
        #endregion

        #region Python __ methods

        [PythonName("__contains__")]
        public static bool Contains(string s, string item) {
            return s.Contains(item);
        }

        [PythonName("__contains__")]
        public static bool Contains(string s, char item) {
            return s.IndexOf(item) != -1;
        }

        [PythonName("__len__")]
        public static int GetLength(string s) {
            return s.Length;
        }

        [PythonName("__getitem__")]
        public static string __getitem__(string s, int index) {
            return Ops.Char2String(s[Ops.FixIndex(index, s.Length)]);
        }

        [PythonName("__getitem__")]
        public static string __getitem__(string s, Slice slice) {
            if (slice == null) throw Ops.TypeError("string indicies must be slices or integers");
            int start, stop, step;
            slice.indices(s.Length, out start, out stop, out step);
            if (step == 1) {
                return stop > start ? s.Substring(start, stop - start) : String.Empty;
            } else {
                int index = 0;
                char[] newData;
                if (step > 0) {
                    if (start > stop) return String.Empty;

                    int icnt = (stop - start + step - 1) / step;
                    newData = new char[icnt];
                    for (int i = start; i < stop; i += step) {
                        newData[index++] = s[i];
                    }
                } else {
                    if (start < stop) return String.Empty;

                    int icnt = (stop - start + step + 1) / step;
                    newData = new char[icnt];
                    for (int i = start; i > stop; i += step) {
                        newData[index++] = s[i];
                    }
                }
                return new string(newData);
            }
        }

        [PythonName("__getslice__")]
        public static string GetSlice(string self, int x, int y) {
            return __getitem__(self, new Slice(x, y));
        }

        [PythonName("__add__")]
        public static object Add(string self, string other) {
            return self + other;
        }

        [PythonName("__mod__")]
        public static string Modulus(string self, object other) {
            return new StringFormatter(self, other).Format();
        }

        [PythonName("__mul__")]
        public static string Multiply(string s, int count) {
            if (count <= 0) return String.Empty;

            long size = (long)s.Length * (long)count;
            if (size > Int32.MaxValue) throw Ops.OverflowError("repeated string is too long");

            int sz = s.Length;
            if (sz == 1) return new string(s[0], count);

            StringBuilder ret = new StringBuilder(sz * count);
            ret.Insert(0, s, count);
            // the above code is MUCH faster than the simple loop
            //for (int i=0; i < count; i++) ret.Append(s);
            return ret.ToString();
        }

        [PythonName("__rmul__")]
        public static object ReverseMultiply(string self, int other) {
            return Multiply(self, other);
        }

        #endregion

        #region Public Python methods

        [PythonName("capitalize")]
        public static string Capitalize(string self) {
            if (self.Length == 0) return self;
            return Char.ToUpper(self[0]) + self.Substring(1).ToLower();
        }

        //  default fillchar (padding char) is a space
        [PythonName("center")]
        public static string Center(string self, int width) {
            return Center(self, width, ' ');
        }

        [PythonName("center")]
        public static string Center(string self, int width, char fillchar) {
            int spaces = width - self.Length;
            if (spaces <= 0) return self;

            StringBuilder ret = new StringBuilder(width);
            ret.Append(fillchar, spaces / 2);
            ret.Append(self);
            ret.Append(fillchar, (spaces + 1) / 2);
            return ret.ToString();
        }

        [PythonName("count")]
        public static int Count(string self, string sub) {
            return Count(self, sub, 0, self.Length);
        }

        [PythonName("count")]
        public static int Count(string self, string sub, int start) {
            return Count(self, sub, start, self.Length);
        }

        [PythonName("count")]
        public static int Count(string self, string ssub, int start, int end) {
            if (ssub == null) throw Ops.TypeError("expected string for 'sub' argument, got NoneType");
            string v = self;
            if (v.Length == 0) return 0;

            start = Ops.FixSliceIndex(start, self.Length);
            end = Ops.FixSliceIndex(end, self.Length);

            int count = 0;
            while (true) {
                if (end <= start) break;
                int index = v.IndexOf(ssub, start, end - start);
                if (index == -1) break;
                count++;
                start = index + ssub.Length;
            }
            return count;
        }

        [PythonName("decode")]
        public static string Decode(ICallerContext context, string s) {
            return Decode(context, s, null, "strict");
        }

        [PythonName("decode")]
        public static string Decode(ICallerContext context, string s, string encoding, [DefaultParameterValue("strict")]string errors) {
            return RawDecode(context.SystemState, s, encoding, errors);
        }

        [PythonName("encode")]
        public static string Encode(ICallerContext context, string s, string encoding, [DefaultParameterValue("strict")]string errors) {
            return RawEncode(context.SystemState, s, encoding, errors);
        }

        [PythonName("endswith")]
        public static object EndsWith(string self, object suffix) {
            TryStringOrTuple(suffix);
            if (suffix is string)
                return EndsWith(self, suffix as string);
            else
                return EndsWith(self, suffix as Tuple);
        }

        [PythonName("endswith")]
        public static object EndsWith(string self, object suffix, int start) {
            TryStringOrTuple(suffix);
            if (suffix is string)
                return EndsWith(self, suffix as string, start);
            else
                return EndsWith(self, suffix as Tuple, start);
        }

        [PythonName("endswith")]
        public static object EndsWith(string self, object suffix, int start, int end) {
            TryStringOrTuple(suffix);
            if (suffix is string)
                return EndsWith(self, suffix as string, start, end);
            else
                return EndsWith(self, suffix as Tuple, start, end);
        }

        [PythonName("expandtabs")]
        public static string ExpandTabs(string self) {
            return ExpandTabs(self, 8);
        }

        [PythonName("expandtabs")]
        public static string ExpandTabs(string self, int tabsize) {
            StringBuilder ret = new StringBuilder(self.Length * 2);
            string v = self;
            int col = 0;
            for (int i = 0; i < v.Length; i++) {
                char ch = v[i];
                switch (ch) {
                    case '\n':
                    case '\r': col = 0; ret.Append(ch); break;
                    case '\t':
                        if (tabsize > 0) {
                            int tabs = tabsize - (col % tabsize);
                            ret.Append(' ', tabs);
                        }
                        break;
                    default:
                        col++;
                        ret.Append(ch);
                        break;
                }
            }
            return ret.ToString();
        }

        [PythonName("find")]
        public static int Find(string self, string sub) {
            if (sub == null) throw Ops.TypeError("expected string, got NoneType");
            if (sub.Length == 1) return self.IndexOf(sub[0]);
            CompareInfo c = CultureInfo.InvariantCulture.CompareInfo;
            return c.IndexOf(self, sub, CompareOptions.Ordinal);

        }

        [PythonName("find")]
        public static int Find(string self, string sub, int start) {
            if (sub == null) throw Ops.TypeError("expected string, got NoneType");
            return self.IndexOf(sub, Ops.FixSliceIndex(start, self.Length));
        }

        [PythonName("find")]
        public static int Find(string self, string sub, int start, int end) {
            if (sub == null) throw Ops.TypeError("expected string, got NoneType");
            start = Ops.FixSliceIndex(start, self.Length);
            end = Ops.FixSliceIndex(end, self.Length);

            return self.IndexOf(sub, start, end - start);
        }

        [PythonName("index")]
        public static int Index(string self, string sub) {
            if (sub == null) throw Ops.TypeError("expected string, got NoneType");
            return Index(self, sub, 0, self.Length);
        }

        [PythonName("index")]
        public static int Index(string self, string sub, int start) {
            if (sub == null) throw Ops.TypeError("expected string, got NoneType");
            return Index(self, sub, start, self.Length);
        }

        [PythonName("index")]
        public static int Index(string self, string sub, int start, int end) {
            if (sub == null) throw Ops.TypeError("expected string, got NoneType");
            int ret = Find(self, sub, start, end);
            if (ret == -1) throw Ops.ValueError("substring {0} not found in {1}", sub, self);
            return ret;
        }

        [PythonName("isalnum")]
        public static object IsAlnum(string self) {
            if (self.Length == 0) return Ops.FALSE;
            string v = self;
            for (int i = v.Length - 1; i >= 0; i--) {
                if (!Char.IsLetterOrDigit(v, i)) return Ops.FALSE;
            }
            return Ops.TRUE;
        }

        [PythonName("isalpha")]
        public static object IsAlpha(string self) {
            if (self.Length == 0) return Ops.FALSE;
            string v = self;
            for (int i = v.Length - 1; i >= 0; i--) {
                if (!Char.IsLetter(v, i)) return Ops.FALSE;
            }
            return Ops.TRUE;
        }

        [PythonName("isdigit")]
        public static object IsDigit(string self) {
            if (self.Length == 0) return Ops.FALSE;
            string v = self;
            for (int i = v.Length - 1; i >= 0; i--) {
                if (!Char.IsDigit(v, i)) return Ops.FALSE;
            }
            return Ops.TRUE;
        }

        [PythonName("isspace")]
        public static object IsSpace(string self) {
            if (self.Length == 0) return Ops.FALSE;
            string v = self;
            for (int i = v.Length - 1; i >= 0; i--) {
                if (!Char.IsWhiteSpace(v, i)) return Ops.FALSE;
            }
            return Ops.TRUE;
        }

        [PythonName("isdecimal")]
        public static object IsDecimal(string self) {
            return IsNumeric(self);
        }

        [PythonName("isnumeric")]
        public static object IsNumeric(string self) {
            foreach (char c in self) {
                if (!Char.IsDigit(c)) return Ops.FALSE;
            }
            return Ops.TRUE;
        }

        [PythonName("islower")]
        public static object IsLower(string self) {
            if (self.Length == 0) return Ops.FALSE;
            string v = self;
            bool hasLower = false;
            for (int i = v.Length - 1; i >= 0; i--) {
                if (!hasLower && Char.IsLower(v, i)) hasLower = true;
                if (Char.IsUpper(v, i)) return Ops.FALSE;
            }
            return Ops.Bool2Object(hasLower);
        }

        [PythonName("isupper")]
        public static object IsUpper(string self) {
            if (self.Length == 0) return Ops.FALSE;
            string v = self;
            bool hasUpper = false;
            for (int i = v.Length - 1; i >= 0; i--) {
                if (!hasUpper && Char.IsUpper(v, i)) hasUpper = true;
                if (Char.IsLower(v, i)) return Ops.FALSE;
            }
            return Ops.Bool2Object(hasUpper);
        }

        //  return Ops.TRUE if self is a titlecased string and there is at least one
        //  character in self; also, uppercase characters may only follow uncased
        //  characters (e.g. whitespace) and lowercase characters only cased ones. 
        //  return Ops.FALSE otherwise.
        [PythonName("istitle")]
        public static object IsTitle(string self) {
            if (self == null || self.Length == 0) return Ops.FALSE;

            string v = self;
            bool prevCharCased = false, currCharCased = false, containsUpper = false;
            for (int i = 0; i < v.Length; i++) {
                if (Char.IsUpper(v, i)) {
                    containsUpper = true;
                    if (prevCharCased)
                        return Ops.FALSE;
                    else
                        currCharCased = true;
                } else if (Char.IsLower(v, i))
                    if (!prevCharCased)
                        return Ops.FALSE;
                    else
                        currCharCased = true;
                else
                    currCharCased = false;
                prevCharCased = currCharCased;
            }

            //  if we've gone through the whole string and haven't encountered any rule 
            //  violations but also haven't seen an Uppercased char, then this is not a 
            //  title e.g. '\n', all whitespace etc.
            if (!containsUpper) return Ops.FALSE;

            return Ops.TRUE;
        }

        [PythonName("isunicode")]
        public static object IsUnicode(string self) {
            foreach (char c in self) {
                if (c >= LowestUnicodeValue) return Ops.TRUE;
            }
            return Ops.FALSE;
        }

        //  Return a string which is the concatenation of the strings 
        //  in the sequence seq. The separator between elements is the 
        //  string providing this method
        [PythonName("join")]
        public static object Join(string self, object sequence) {
            IEnumerator seq = Ops.GetEnumerator(sequence);
            if (!seq.MoveNext()) return "";

            StringBuilder ret = new StringBuilder();
            // check if we have just a sequnce of just one value - if so just
            // return that value.
            object curVal = seq.Current;
            if (!seq.MoveNext()) return Converter.ConvertToString(curVal);

            AppendJoin(curVal, 0, ret);

            int index = 1;
            do {
                ret.Append(self);

                AppendJoin(seq.Current, index, ret);

                index++;
            } while (seq.MoveNext());

            return ret.ToString();
        }

        [PythonName("ljust")]
        public static string LJust(string self, int width) {
            return LJust(self, width, ' ');
        }

        [PythonName("ljust")]
        public static string LJust(string self, int width, char fillchar) {
            int spaces = width - self.Length;
            if (spaces <= 0) return self;

            StringBuilder ret = new StringBuilder(width);
            ret.Append(self);
            ret.Append(fillchar, spaces);
            return ret.ToString();
        }

        [PythonName("lower")]
        public static string Lower(string self) {
            return self.ToLower();
        }

        private static readonly char[] Whitespace = new char[] { ' ', '\t', '\n', '\r', '\f' };
        [PythonName("lstrip")]
        public static string LStrip(string self) {
            return self.TrimStart(Whitespace);
        }

        [PythonName("lstrip")]
        public static string LStrip(string self, string chars) {
            if (chars == null) return LStrip(self);
            return self.TrimStart(chars.ToCharArray());
        }

        [PythonVersion(2, 5)]
        [PythonName("partition")]
        public static Tuple Partition(string self, string sep) {
            if (sep == null)
                throw Ops.TypeError("expected string, got NoneType");
            if (sep.Length == 0)
                throw Ops.ValueError("empty separator");

            object[] obj = new object[3] { "", "", "" };

            if (self.Length != 0) {
                int index = Find(self, sep);
                if (index == -1) {
                    obj[0] = self;
                } else {
                    obj[0] = self.Substring(0, index);
                    obj[1] = sep;
                    obj[2] = self.Substring(index + sep.Length, self.Length - index - sep.Length);
                }
            }
            return new Tuple(obj);
        }

        [PythonName("replace")]
        public static string Replace(string self, string old, string new_) {
            if (old == null) throw Ops.TypeError("expected string for 'old' argument, got NoneType");
            if (old.Length == 0) return ReplaceEmpty(self, new_, self.Length + 1);
            return self.Replace(old, new_);
        }

        [PythonName("replace")]
        public static string Replace(string self, string old, string new_, int maxsplit) {
            if (old == null) throw Ops.TypeError("expected string for 'old' argument, got NoneType");
            if (maxsplit == -1) return Replace(self, old, new_);
            if (old.Length == 0) return ReplaceEmpty(self, new_, maxsplit);

            string v = self;
            StringBuilder ret = new StringBuilder(v.Length);

            int index;
            int start = 0;

            while (maxsplit > 0 && (index = v.IndexOf(old, start)) != -1) {
                ret.Append(v.Substring(start, index - start));
                ret.Append(new_);
                start = index + old.Length;
                maxsplit--;
            }
            ret.Append(v.Substring(start));

            return ret.ToString();
        }

        [PythonName("rfind")]
        public static int RFind(string self, string sub) {
            if (sub == null) throw Ops.TypeError("expected string, got NoneType");
            return RFind(self, sub, 0, self.Length);
        }

        [PythonName("rfind")]
        public static int RFind(string self, string sub, int start) {
            if (sub == null) throw Ops.TypeError("expected string, got NoneType");
            return RFind(self, sub, start, self.Length);
        }

        [PythonName("rfind")]
        public static int RFind(string self, string sub, int start, int end) {
            if (sub == null) throw Ops.TypeError("expected string, got NoneType");

            start = Ops.FixSliceIndex(start, self.Length);
            end = Ops.FixSliceIndex(end, self.Length);

            if (start > end) return -1;     // can't possibly match anything, not even an empty string
            if (sub.Length == 0) return end;    // match at the end
            if (end == 0) return -1;    // can't possibly find anything

            return self.LastIndexOf(sub, end - 1, end - start);
        }

        [PythonName("rindex")]
        public static int RIndex(string self, string sub) {
            return RIndex(self, sub, 0, self.Length);
        }

        [PythonName("rindex")]
        public static int RIndex(string self, string sub, int start) {
            return RIndex(self, sub, start, self.Length);
        }

        [PythonName("rindex")]
        public static int RIndex(string self, string sub, int start, int end) {
            int ret = RFind(self, sub, start, end);
            if (ret == -1) throw Ops.ValueError("substring {0} not found in {1}", sub, self);
            return ret;
        }

        [PythonName("rjust")]
        public static string RJust(string self, int width) {
            return RJust(self, width, ' ');
        }

        [PythonName("rjust")]
        public static string RJust(string self, int width, char fillchar) {
            int spaces = width - self.Length;
            if (spaces <= 0) return self;

            StringBuilder ret = new StringBuilder(width);
            ret.Append(fillchar, spaces);
            ret.Append(self);
            return ret.ToString();
        }

        [PythonVersion(2, 5)]
        [PythonName("rpartition")]
        public static Tuple Rpartition(string self, string sep) {
            if (sep == null)
                throw Ops.TypeError("expected string, got NoneType");
            if (sep.Length == 0)
                throw Ops.ValueError("empty separator");

            object[] obj = new object[3] { "", "", "" };
            if (self.Length != 0) {
                int index = RFind(self, sep);
                if (index == -1) {
                    obj[0] = self;
                } else {
                    obj[0] = self.Substring(0, index);
                    obj[1] = sep;
                    obj[2] = self.Substring(index + sep.Length, self.Length - index - sep.Length);
                }
            }
            return new Tuple(obj);
        }


        //  when no maxsplit arg is given then just use split
        [PythonName("rsplit")]
        public static List RSplit(string self) {
            return Split(self, (char[])null, -1);
        }

        [PythonName("rsplit")]
        public static List RSplit(string self, string sep) {
            return Split(self, sep, -1);
        }

        [PythonName("rsplit")]
        public static List RSplit(string self, string sep, int maxsplit) {

            //  rsplit works like split but needs to split from the right;
            //  reverse the original string (and the sep), split, reverse 
            //  the split list and finally reverse each element of the list
            string reversed = Reverse(self);
            if (sep != null) sep = Reverse(sep);
            List temp = null, ret = null;
            temp = Split(reversed, sep, maxsplit);
            temp.Reverse();
            if (temp.Count != 0)
                ret = new List();
            foreach (string s in temp)
                ret.AddNoLock(Reverse(s));
            return ret;
        }

        [PythonName("rstrip")]
        public static string RStrip(string self) {
            return self.TrimEnd(Whitespace);
        }

        [PythonName("rstrip")]
        public static string RStrip(string self, string chars) {
            if (chars == null) return RStrip(self);
            return self.TrimEnd(chars.ToCharArray());
        }

        [PythonName("split")]
        public static List Split(string self) {
            return Split(self, (char[])null, -1);
        }

        [PythonName("split")]
        public static List Split(string self, string sep) {
            return Split(self, sep, -1);
        }

        [PythonName("split")]
        public static List Split(string self, string sep, int maxsplit) {
            if (sep == null) return Split(self, (char[])null, maxsplit);

            if (sep.Length == 0) {
                throw Ops.ValueError("empty separator");
            } else if (sep.Length == 1) {
                return Split(self, new char[] { sep[0] }, maxsplit);
            } else {
                return Split(self, new string[] { sep }, maxsplit);
            }
        }

        [PythonName("splitlines")]
        public static List SplitLines(string self) {
            return SplitLines(self, false);
        }

        [PythonName("splitlines")]
        public static List SplitLines(string self, bool keepends) {
            List ret = new List();
            int i, linestart;
            for (i = 0, linestart = 0; i < self.Length; i++) {
                if (self[i] == '\n' || self[i] == '\r' || self[i] == '\x2028') {
                    //  special case of "\r\n" as end of line marker
                    if (i < self.Length - 1 && self[i] == '\r' && self[i + 1] == '\n') {
                        if (keepends)
                            ret.AddNoLock(self.Substring(linestart, i - linestart + 2));
                        else
                            ret.AddNoLock(self.Substring(linestart, i - linestart));
                        linestart = i + 2;
                        i++;
                    } else { //'\r', '\n', or unicode new line as end of line marker
                        if (keepends)
                            ret.AddNoLock(self.Substring(linestart, i - linestart + 1));
                        else
                            ret.AddNoLock(self.Substring(linestart, i - linestart));
                        linestart = i + 1;
                    }
                }
            }
            //  the last line needs to be accounted for if it is not empty
            if (i - linestart != 0)
                ret.AddNoLock(self.Substring(linestart, i - linestart));
            return ret;
        }


        [PythonName("startswith")]
        public static object StartsWith(string self, object prefix) {
            TryStringOrTuple(prefix);
            if (prefix is string)
                return StartsWith(self, prefix as string);
            else
                return StartsWith(self, prefix as Tuple);

        }

        [PythonName("startswith")]
        public static object StartsWith(string self, object prefix, int start) {
            TryStringOrTuple(prefix);
            if (prefix is string)
                return StartsWith(self, prefix as string, start);
            else
                return StartsWith(self, prefix as Tuple, start);
        }


        [PythonName("startswith")]
        public static object StartsWith(string self, object prefix, int start, int end) {
            TryStringOrTuple(prefix);
            if (prefix is string)
                return StartsWith(self, prefix as string, start, end);
            else
                return StartsWith(self, prefix as Tuple, start, end);
        }


        [PythonName("strip")]
        public static string Strip(string self) {
            return self.Trim();
        }

        [PythonName("strip")]
        public static string Strip(string self, string chars) {
            if (chars == null) return Strip(self);
            return self.Trim(chars.ToCharArray());
        }

        [PythonName("swapcase")]
        public static string SwapCase(string self) {
            StringBuilder ret = new StringBuilder(self);
            for (int i = 0; i < ret.Length; i++) {
                char ch = ret[i];
                if (Char.IsUpper(ch)) ret[i] = Char.ToLower(ch);
                else if (Char.IsLower(ch)) ret[i] = Char.ToUpper(ch);
            }
            return ret.ToString();
        }

        [PythonName("title")]
        public static string Title(string self) {
            if (self == null || self.Length == 0) return self;

            char[] retchars = self.ToCharArray();
            bool prevCharCased = false;
            bool currCharCased = false;
            int i = 0;
            do {
                if (Char.IsUpper(retchars[i]) || Char.IsLower(retchars[i])) {
                    if (!prevCharCased)
                        retchars[i] = Char.ToUpper(retchars[i]);
                    else
                        retchars[i] = Char.ToLower(retchars[i]);
                    currCharCased = true;
                } else {
                    currCharCased = false;
                }
                i++;
                prevCharCased = currCharCased;
            }
            while (i < retchars.Length);
            return new string(retchars);
        }

        //translate on a unicode string differs from that on an ascii
        //for unicode, the table argument is actually a dictionary with
        //character ordinals as keys and the replacement strings as values
        [PythonName("translate")]
        public static string Translate(string self, Dict table) {
            if (table == null) throw Ops.TypeError("expected dictionary or string, got NoneType");
            if (self.Length == 0) return self;
            StringBuilder ret = new StringBuilder();
            for (int i = 0, idx = 0; i < self.Length; i++) {
                idx = (int)self[i];
                if (table.ContainsKey(idx))
                    ret.Append((string)table[idx]);
                else
                    ret.Append(self[i]);
            }
            return ret.ToString();
        }

        [PythonName("translate")]
        public static string Translate(string self, string table) {
            return Translate(self, table, (string)null);
        }

        [PythonName("translate")]
        public static string Translate(string self, string table, string deletechars) {
            if (table == null) throw Ops.TypeError("expected string, got NoneType");
            if (table.Length != 256)
                throw Ops.ValueError("translation table must be 256 characters long");
            if (self.Length == 0) return self;
            StringBuilder ret = new StringBuilder();
            for (int i = 0, idx = 0; i < self.Length; i++) {
                if (deletechars == null || !deletechars.Contains(Char.ToString(self[i]))) {
                    idx = (int)self[i];
                    if (idx >= 0 && idx < 256) ret.Append(table[idx]);
                }
            }
            return ret.ToString();
        }

        [PythonName("upper")]
        public static string Upper(string self) {
            return self.ToUpper();
        }

        [PythonName("zfill")]
        public static string ZFill(string self, int width) {
            int spaces = width - self.Length;
            if (spaces <= 0) return self;

            StringBuilder ret = new StringBuilder(width);
            if (self.Length > 0 && IsSign(self[0])) {
                ret.Append(self[0]);
                ret.Append('0', spaces);
                ret.Append(self.Substring(1));
            } else {
                ret.Append('0', spaces);
                ret.Append(self);
            }
            return ret.ToString();
        }
        #endregion

        [PythonName("__eq__")]
        public static object Equals(string x, object other) {
            if (other is string) {
                return String.Equals(x, other as string);
            } else if (other is ExtensibleString) {
                return String.Equals(x, ((ExtensibleString)other).Value);
            } else if (other is char && x.Length == 1) {
                return (char)other == x[0];
            }
            return Ops.NotImplemented;
        }

        public static bool EqualsRetBool(string x, object other) {
            if (other is string) {
                return string.Equals(x, other as string);
            } else if (other is ExtensibleString) {
                return string.Equals(x, ((ExtensibleString)other).Value);
            } else if (other is char && x.Length == 1) {
                return (char)other == x[0];
            }
            return Ops.DynamicEqualRetBool(x, other);
        }

        [PythonName("__ne__")]
        public static object NotEquals(string x, object other) {
            if (other is string) {
                return !String.Equals(x, other as string);
            } else if (other is ExtensibleString) {
                return !String.Equals(x, ((ExtensibleString)other).Value);
            } else if (other is char && x.Length == 1) {
                return (char)other != x[0];
            }
            return Ops.NotImplemented;
        }

        [PythonName("__cmp__")]
        public static object Compare(string self, object obj) {
            if (obj == null) return 1;

            string otherStr;

            if (obj is string) {
                otherStr = (string)obj;
            } else if (obj is ExtensibleString) {
                otherStr = ((ExtensibleString)obj).Value;
            } else if (obj is char && self.Length == 1) {
                return (int)(self[0] - (char)obj);
            } else {
                return Ops.NotImplemented;
            }

            int ret = string.CompareOrdinal(self, otherStr);
            return ret == 0 ? 0 : (ret < 0 ? -1 : +1);
        }

        [PythonName("__getnewargs__")]
        public static object GetNewArgs(string self) {
            if (!Object.ReferenceEquals(self, null)) {
                // Cast self to object to avoid exception caused by trying to access SystemState on DefaultContext
                return Tuple.MakeTuple(StringOps.Make(DefaultContext.Default, TypeCache.String, (object)self));
            }
            throw Ops.TypeErrorForBadInstance("__getnewargs__ requires a 'str' object but received a '{0}'", self);
        }

        #region Internal implementation details

        internal static IEnumerator GetEnumerator(string s) {
            // make an enumerator that produces strings instead of chars
            return new PythonStringEnumerator(s);
        }

        internal static string Quote(string s) {

            bool isUnicode = false;
            StringBuilder b = new StringBuilder(s.Length + 5);
            char quote = '\'';
            if (s.IndexOf('\'') != -1 && s.IndexOf('\"') == -1) {
                quote = '\"';
            }
            b.Append(quote);
            b.Append(ReprEncode(s, quote, ref isUnicode));
            b.Append(quote);
            if (isUnicode) return "u" + b.ToString();
            return b.ToString();
        }

        internal static string ReprEncode(string s, ref bool isUnicode) {
            return ReprEncode(s, (char)0, ref isUnicode);
        }

        internal static bool TryGetEncoding(SystemState state, string name, out Encoding encoding) {
            if (name == null) {
                encoding = state.DefaultEncoding;
                return true;
            }

            if (codecs == null) MakeCodecsDict();

            name = NormalizeEncodingName(name);

            EncodingInfo encInfo;
            if (codecs.TryGetValue(name, out encInfo)) {
                encoding = encInfo.GetEncoding();
                return true;
            }

            encoding = null;
            return false;
        }

        internal static byte[] ToByteArray(string s) {
            byte[] ret = new byte[s.Length];
            for (int i = 0; i < s.Length; i++) {
                if (s[i] < 0x100) ret[i] = (byte)s[i];
                else throw Ops.UnicodeDecodeError("'ascii' codec can't decode byte {0:X} in position {1}: ordinal not in range", (int)ret[i], i);
            }
            return ret;
        }

        internal static string FromByteArray(byte[] bytes) {
            return FromByteArray(bytes, bytes.Length);
        }

        internal static string FromByteArray(byte[] bytes, int maxBytes) {
            int bytesToCopy = Math.Min(bytes.Length, maxBytes);
            StringBuilder b = new StringBuilder(bytesToCopy);
            for (int i = 0; i < bytesToCopy; i++) {
                b.Append((char)bytes[i]);
            }
            return b.ToString();
        }


        #endregion

        #region Private implementation details

        private static void AppendJoin(object value, int index, StringBuilder sb) {
            string strVal;

            if ((strVal = value as string) != null) {
                sb.Append(strVal.ToString());
            } else if (Converter.TryConvertToString(value, out strVal) && strVal != null) {
                sb.Append(strVal);
            } else {
                throw Ops.TypeError("sequence item {0}: expected string, {1} found", index.ToString(), Ops.GetPythonTypeName(value));
            }
        }

        private static string ReplaceEmpty(string self, string new_, int maxsplit) {
            if (maxsplit == 0) return self;

            string v = self;
            int max = maxsplit > v.Length ? v.Length : maxsplit;
            StringBuilder ret = new StringBuilder(v.Length * (new_.Length + 1));
            for (int i = 0; i < max; i++) {
                ret.Append(new_);
                ret.Append(v[i]);
            }
            if (maxsplit > max) {
                ret.Append(new_);
            }

            return ret.ToString();
        }

        private static string Reverse(string s) {
            if (s.Length == 0 || s.Length == 1) return s;
            char[] chars = s.ToCharArray();
            char[] rchars = new char[s.Length];
            for (int i = s.Length - 1, j = 0; i >= 0; i--, j++) {
                rchars[j] = chars[i];
            }
            return new string(rchars);
        }

        private static string ReprEncode(string s, char quote, ref bool isUnicode) {
            // in the common case we don't need to encode anything, so we
            // lazily create the StringBuilder only if necessary.
            StringBuilder b = null;
            for (int i = 0; i < s.Length; i++) {
                char ch = s[i];

                if (ch >= LowestUnicodeValue) isUnicode = true;
                switch (ch) {
                    case '\\': ReprInit(ref b, s, i); b.Append("\\\\"); break;
                    case '\t': ReprInit(ref b, s, i); b.Append("\\t"); break;
                    case '\n': ReprInit(ref b, s, i); b.Append("\\n"); break;
                    case '\r': ReprInit(ref b, s, i); b.Append("\\r"); break;
                    default:
                        if (quote != 0 && ch == quote) {
                            ReprInit(ref b, s, i);
                            b.Append('\\'); b.Append(ch);
                        } else if (ch < ' ' || (ch >= 0x7f && ch <= 0xff)) {
                            ReprInit(ref b, s, i);
                            b.AppendFormat("\\x{0:x2}", (int)ch);
                        } else if (ch > 0xff) {
                            ReprInit(ref b, s, i);
                            b.AppendFormat("\\u{0:x4}", (int)ch);
                        } else if (b != null) {
                            b.Append(ch);
                        }
                        break;
                }
            }

            if (b == null) return s;
            return b.ToString();
        }

        private static void ReprInit(ref StringBuilder sb, string s, int c) {
            if (sb != null) return;

            sb = new StringBuilder(s, 0, c, s.Length);
        }

        private static string RawUnicodeEscapeEncode(string s) {
            // in the common case we don't need to encode anything, so we
            // lazily create the StringBuilder only if necessary.
            StringBuilder b = null;
            for (int i = 0; i < s.Length; i++) {
                char ch = s[i];
                if (ch > 0xff) {
                    ReprInit(ref b, s, i);
                    b.AppendFormat("\\u{0:x4}", (int)ch);
                } else if (b != null) {
                    b.Append(ch);
                }
            }

            if (b == null) return s;
            return b.ToString();
        }

        private static bool IsSign(char ch) {
            return ch == '+' || ch == '-';
        }

        private static string NormalizeEncodingName(string name) {
            return name.ToLower().Replace('-', '_');
        }

        private static string RawDecode(SystemState state, string s, string encoding, string errors) {
            if (encoding != null) {
                string normalizedName = NormalizeEncodingName(encoding);
                if ("raw_unicode_escape" == normalizedName) {
                    return LiteralParser.ParseString(s, true, true);
                } else if ("string_escape" == normalizedName) {
                    return LiteralParser.ParseString(s, false, false);
                }
            }

            Encoding e = state.DefaultEncoding;

            if (encoding == null || TryGetEncoding(state, encoding, out e)) {
                // CLR's encoder exceptions have a 1-1 mapping w/ Python's encoder exceptions
                // so we just clone the encoding & set the fallback to throw in strict mode.
                e = (Encoding)e.Clone();

                switch (errors) {
                    case "strict": e.DecoderFallback = DecoderFallback.ExceptionFallback; break;
                    case "replace": e.DecoderFallback = DecoderFallback.ReplacementFallback; break;
                    default:
                        e.DecoderFallback = new PythonDecoderFallback(encoding,
                            s,
                            Modules.PythonCodecs.LookupError(errors));
                        break;
                }
                return e.GetString(ToByteArray(s));
            }

            // look for user-registered codecs
            Tuple codecTuple = Modules.PythonCodecs.Lookup(encoding);
            if (codecTuple != null) {
                return UserDecodeOrEncode(codecTuple[Modules.PythonCodecs.DecoderIndex], s);
            }

            throw Ops.LookupError("unknown encoding: {0}", encoding);

        }

        private static string RawEncode(SystemState state, string s, string encoding, string errors) {
            if (encoding != null) {
                string normalizedName = NormalizeEncodingName(encoding);
                if ("raw_unicode_escape" == normalizedName) {
                    return RawUnicodeEscapeEncode(s);
                } else if ("string_escape" == normalizedName) {
                    bool dummy = false;
                    return ReprEncode(s, '\'', ref dummy);
                }
            }

            Encoding e = state.DefaultEncoding;
            if (encoding == null || TryGetEncoding(state, encoding, out e)) {
                // CLR's encoder exceptions have a 1-1 mapping w/ Python's encoder exceptions
                // so we just clone the encoding & set the fallback to throw in strict mode
                e = (Encoding)e.Clone();

                switch (errors) {
                    case "strict": e.EncoderFallback = EncoderFallback.ExceptionFallback; break;
                    case "replace": e.EncoderFallback = EncoderFallback.ReplacementFallback; break;
                    default:
                        e.EncoderFallback = new PythonEncoderFallback(encoding,
                            s,
                            Modules.PythonCodecs.LookupError(errors));
                        break;
                }

                return FromByteArray(e.GetBytes(s));
            }

            // look for user-registered codecs
            Tuple codecTuple = Modules.PythonCodecs.Lookup(encoding);
            if (codecTuple != null) {
                return UserDecodeOrEncode(codecTuple[Modules.PythonCodecs.EncoderIndex], s);
            }

            throw Ops.LookupError("unknown encoding: {0}", encoding);
        }

        private static string UserDecodeOrEncode(object function, string data) {
            object res = Ops.Call(function, data);

            string strRes = res as string;
            if (strRes != null) return strRes;

            // tuple is string, bytes used, we just want the string...
            Tuple t = res as Tuple;
            if (t == null) throw Ops.TypeErrorForBadInstance("expected tuple, but found {0}", res);

            return Converter.ConvertToString(t[0]);
        }

        private static void MakeCodecsDict() {
            Dictionary<string, EncodingInfo> d = new Dictionary<string, EncodingInfo>();
            EncodingInfo[] encs = Encoding.GetEncodings();
            for (int i = 0; i < encs.Length; i++) {
                string normalizedName = NormalizeEncodingName(encs[i].Name);
                // setup well-known mappings, for everything
                // else we'll store as lower case w/ _                
                switch (normalizedName) {
                    case "us_ascii":
                        d["ascii"] = encs[i];
                        d["646"] = encs[i];
                        d["us"] = encs[i];
                        break;
                    case "iso_8859_1":
                        d["latin_1"] = encs[i];
                        d["latin1"] = encs[i];
                        break;
                    case "utf_8":
                        d["utf8"] = encs[i];
                        break;
                    case "utf_16":
                        d["utf_16_le"] = encs[i];
                        // utf-16 in CPython already writes the BOM, so we don't 
                        // want to replace it w/ ours that doesn't.
                        continue;
                    case "unicodeFFFE": // big endian unicode                    
                        d["utf_16_be"] = encs[i];
                        break;
                }

                d[normalizedName] = encs[i];
            }

            codecs = d;
        }

        private static List SplitEmptyString(bool separators) {
            List ret = List.MakeEmptyList(1);
            if (separators) {
                ret.AddNoLock(String.Empty);
            }
            return ret;
        }

        private static List Split(string self, char[] seps, int maxsplit) {
            if (self == String.Empty) {
                return SplitEmptyString(seps != null);
            } else {
                string[] r = null;
                //  If the optional second argument sep is absent or None, the words are separated 
                //  by arbitrary strings of whitespace characters (space, tab, newline, return, formfeed);
                if (seps == null)
                    r = maxsplit == -1 ? self.Split(seps, StringSplitOptions.RemoveEmptyEntries)
                                            : self.Split(seps, maxsplit + 1, StringSplitOptions.RemoveEmptyEntries);
                else
                    r = maxsplit == -1 ? self.Split(seps)
                                            : self.Split(seps, maxsplit + 1);
                List ret = List.MakeEmptyList(r.Length);
                foreach (string s in r) ret.AddNoLock(s);
                return ret;
            }
        }

        private static List Split(string self, string[] seps, int maxsplit) {
            if (self == String.Empty) {
                return SplitEmptyString(seps != null);
            } else {
                string[] r = maxsplit == -1 ? self.Split(seps, StringSplitOptions.None)
                                            : self.Split(seps, maxsplit + 1, StringSplitOptions.None);
                List ret = List.MakeEmptyList(r.Length);
                foreach (string s in r) ret.AddNoLock(s);
                return ret;
            }
        }

        private static void TryStringOrTuple(object prefix) {
            if (Options.Python25 == true) {
                if (prefix == null) throw Ops.TypeError("expected string or Tuple, got NoneType");
                if (!(prefix is string) && !(prefix is Tuple))
                    throw Ops.TypeError("expected string or Tuple, got {0} Type", prefix.GetType());
            } else {
                if (prefix == null) throw Ops.TypeError("expected string, got NoneType");
                if (!(prefix is string))
                    throw Ops.TypeError("expected string, got {0} Type", prefix.GetType());
            }
        }

        private static Boolean TryString(object obj) {
            if (obj == null)
                throw Ops.TypeError("expected string , got NoneType");
            if ((obj as string) == null)
                throw Ops.TypeError("expected string , got {0} Type", obj.GetType());
            return true;
        }

        private static object EndsWith(string self, string suffix) {
            return Ops.Bool2Object(self.EndsWith(suffix));
        }

        //  Indexing is 0-based. Need to deal with negative indices
        //  (which mean count backwards from end of sequence)
        //  +---+---+---+---+---+ 
        //  | a | b | c | d | e |
        //  +---+---+---+---+---+ 
        //    0   1   2   3   4    
        //   -5  -4  -3  -2  -1

        private static object EndsWith(string self, string suffix, int start) {
            int len = self.Length;
            if (start > len) return Ops.FALSE;
            // map the negative indice to its positive counterpart
            if (start < 0) {
                start += len;
                if (start < 0) start = 0;
            }
            return Ops.Bool2Object(self.Substring(start).EndsWith(suffix));
        }

        //  With optional start, test beginning at that position (the char at that index is
        //  included in the test). With optional end, stop comparing at that position (the 
        //  char at that index is not included in the test)
        private static object EndsWith(string self, string suffix, int start, int end) {
            int len = self.Length;
            if (start > len) return Ops.FALSE;
            // map the negative indices to their positive counterparts
            else if (start < 0) {
                start += len;
                if (start < 0) start = 0;
            }
            if (end >= len) return Ops.Bool2Object(self.Substring(start).EndsWith(suffix));
            else if (end < 0) {
                end += len;
                if (end < 0) return Ops.FALSE;
            }
            if (end < start) return Ops.FALSE;
            return Ops.Bool2Object(self.Substring(start, end - start).EndsWith(suffix));
        }


        private static object EndsWith(string self, Tuple suffix) {
            foreach (object obj in suffix) {
                if (TryString(obj)) {
                    if (self.EndsWith(obj as string) == true) {
                        return Ops.Bool2Object(true);
                    }
                }
            }
            return Ops.Bool2Object(false);
        }

        private static object EndsWith(string self, Tuple suffix, int start) {
            int len = self.Length;
            if (start > len) return Ops.FALSE;
            // map the negative indice to its positive counterpart
            if (start < 0) {
                start += len;
                if (start < 0) start = 0;
            }
            foreach (object obj in suffix) {
                if (TryString(obj)) {
                    if (self.Substring(start).EndsWith(obj as string) == true) {
                        return Ops.Bool2Object(true);
                    }
                }
            }
            return Ops.Bool2Object(false);
        }

        private static object EndsWith(string self, Tuple suffix, int start, int end) {
            int len = self.Length;
            if (start > len) return Ops.FALSE;
            // map the negative indices to their positive counterparts
            else if (start < 0) {
                start += len;
                if (start < 0) start = 0;
            }
            if (end >= len) end = len;
            else if (end < 0) {
                end += len;
                if (end < 0) return Ops.FALSE;
            }
            if (end < start) return Ops.FALSE;

            foreach (object obj in suffix) {
                if (TryString(obj)) {
                    if (self.Substring(start, end - start).EndsWith(obj as string) == true) {
                        return Ops.Bool2Object(true);
                    }
                }
            }
            return Ops.Bool2Object(false);
        }

        private static object StartsWith(string self, string prefix) {
            return Ops.Bool2Object(self.StartsWith(prefix));
        }

        private static object StartsWith(string self, string prefix, int start) {
            int len = self.Length;
            if (start > len) return Ops.FALSE;
            if (start < 0) {
                start += len;
                if (start < 0) start = 0;
            }
            return Ops.Bool2Object(self.Substring(start).StartsWith(prefix));
        }

        private static object StartsWith(string self, string prefix, int start, int end) {
            int len = self.Length;
            if (start > len) return Ops.FALSE;
            // map the negative indices to their positive counterparts
            else if (start < 0) {
                start += len;
                if (start < 0) start = 0;
            }
            if (end >= len) return Ops.Bool2Object(self.Substring(start).StartsWith(prefix));
            else if (end < 0) {
                end += len;
                if (end < 0) return Ops.FALSE;
            }
            if (end < start) return Ops.FALSE;
            return Ops.Bool2Object(self.Substring(start, end - start).StartsWith(prefix));
        }

        private static object StartsWith(string self, Tuple prefix) {
            foreach (object obj in prefix) {
                if (TryString(obj)) {
                    if (self.StartsWith(obj as string) == true) {
                        return Ops.Bool2Object(true);
                    }
                }
            }
            return Ops.Bool2Object(false);
        }

        private static object StartsWith(string self, Tuple prefix, int start) {
            int len = self.Length;
            if (start > len) return Ops.FALSE;
            if (start < 0) {
                start += len;
                if (start < 0) start = 0;
            }
            foreach (object obj in prefix) {
                if (TryString(obj)) {
                    if (self.Substring(start).StartsWith(obj as string) == true) {
                        return Ops.Bool2Object(true);
                    }
                }
            }
            return Ops.Bool2Object(false);
        }

        private static object StartsWith(string self, Tuple prefix, int start, int end) {
            int len = self.Length;
            if (start > len) return Ops.FALSE;
            // map the negative indices to their positive counterparts
            else if (start < 0) {
                start += len;
                if (start < 0) start = 0;
            }
            if (end >= len) end = len;
            else if (end < 0) {
                end += len;
                if (end < 0) return Ops.FALSE;
            }
            if (end < start) return Ops.FALSE;

            foreach (object obj in prefix) {
                if (TryString(obj)) {
                    if (self.Substring(start, end - start).StartsWith(obj as string) == true) {
                        return Ops.Bool2Object(true);
                    }
                }
            }
            return Ops.Bool2Object(false);
        }

        private class PythonStringEnumerator : IEnumerator {
            private string s;
            private int i;

            public PythonStringEnumerator(string s) {
                this.s = s;
                this.Reset();
            }
            #region IEnumerator Members

            public void Reset() {
                i = -1;
            }

            public object Current {
                get {
                    return Ops.Char2String(s[i]);
                }
            }

            public bool MoveNext() {
                i++;
                return i < s.Length;
            }

            #endregion

        }

        #endregion

        #region  Unicode Encode/Decode Fallback Support

        /// When encoding or decoding strings if an error occurs CPython supports several different
        /// behaviors, in addition it supports user-extensible behaviors as well.  For the default
        /// behavior we're ok - both of us support throwing & replacing.  For custom behaviors
        /// we define a single fallback for decoding & encoding that calls the python function to do
        /// the replacement.
        /// 
        /// When we do the replacement we call the provided handler w/ a UnicodeEncodeError or UnicodeDecodeError
        /// object which contains:
        ///         encoding    (string, the encoding the user requested)
        ///         end         (the end of the invalid characters)
        ///         object      (the original string being decoded)
        ///         reason      (the error, e.g. 'unexpected byte code', not sure of others)
        ///         start       (the start of the invalid sequence)
        ///         
        /// The decoder returns a tuple of (unicode, int) where unicode is the replacement string
        /// and int is an index where encoding should continue.

        public class PythonEncoderFallbackBuffer : EncoderFallbackBuffer {
            private object function;
            private string encoding, strData;
            private string buffer;
            private int bufferIndex;

            public PythonEncoderFallbackBuffer(string encoding, string str, object callable) {
                function = callable;
                strData = str;
                this.encoding = encoding;
            }

            public override bool Fallback(char charUnknown, int index) {
                return DoPythonFallback(index, 1);
            }

            public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index) {
                return DoPythonFallback(index, 2);
            }

            public override char GetNextChar() {
                if (buffer == null || bufferIndex >= buffer.Length) return Char.MinValue;

                return buffer[bufferIndex++];
            }

            public override bool MovePrevious() {
                if (bufferIndex > 0) {
                    bufferIndex--;
                    return true;
                }
                return false;
            }

            public override int Remaining {
                get {
                    if (buffer == null) return 0;
                    return buffer.Length - bufferIndex;
                }
            }

            public override void Reset() {
                buffer = null;
                bufferIndex = 0;
                base.Reset();
            }

            private bool DoPythonFallback(int index, int length) {
                // create the exception object to hand to the user-function...
                object exObj = Ops.Call(ExceptionConverter.GetPythonException("UnicodeEncodeError"),
                    encoding,
                    strData,
                    index,
                    index + length,
                    "unexpected code byte");

                // call the user function...
                object res = Ops.Call(function, exObj);

                string replacement = PythonDecoderFallbackBuffer.CheckReplacementTuple(res, "encoding");

                // finally process the user's request.
                buffer = replacement;
                bufferIndex = 0;
                return true;
            }

        }

        class PythonEncoderFallback : EncoderFallback {
            private object function;
            private string str;
            private string enc;

            public PythonEncoderFallback(string encoding, string data, object callable) {
                function = callable;
                str = data;
                enc = encoding;
            }

            public override EncoderFallbackBuffer CreateFallbackBuffer() {
                return new PythonEncoderFallbackBuffer(enc, str, function);
            }

            public override int MaxCharCount {
                get { return Int32.MaxValue; }
            }
        }

        public class PythonDecoderFallbackBuffer : DecoderFallbackBuffer {
            private object function;
            private string encoding, strData;
            private string buffer;
            private int bufferIndex;

            public PythonDecoderFallbackBuffer(string encoding, string str, object callable) {
                this.encoding = encoding;
                this.strData = str;
                this.function = callable;
            }

            public override int Remaining {
                get {
                    if (buffer == null) return 0;
                    return buffer.Length - bufferIndex;
                }
            }

            public override char GetNextChar() {
                if (buffer == null || bufferIndex >= buffer.Length) return Char.MinValue;

                return buffer[bufferIndex++];
            }

            public override bool MovePrevious() {
                if (bufferIndex > 0) {
                    bufferIndex--;
                    return true;
                }
                return false;
            }

            public override void Reset() {
                buffer = null;
                bufferIndex = 0;
                base.Reset();
            }

            public override bool Fallback(byte[] bytesUnknown, int index) {
                // create the exception object to hand to the user-function...
                object exObj = Ops.Call(ExceptionConverter.GetPythonException("UnicodeDecodeError"),
                    encoding,
                    strData,
                    index,
                    index + bytesUnknown.Length,
                    "unexpected code byte");

                // call the user function...
                object res = Ops.Call(function, exObj);

                string replacement = CheckReplacementTuple(res, "decoding");

                // finally process the user's request.
                buffer = replacement;
                bufferIndex = 0;
                return true;
            }

            internal static string CheckReplacementTuple(object res, string encodeOrDecode) {
                bool ok = true;
                string replacement = null;
                Tuple tres = res as Tuple;

                // verify the result is sane...
                if (tres != null && tres.Count == 2) {
                    if (!Converter.TryConvertToString(tres[0], out replacement)) ok = false;
                    if (ok) {
                        int bytesSkipped;
                        if (!Converter.TryConvertToInt32(tres[1], out bytesSkipped)) ok = false;
                    }
                } else {
                    ok = false;
                }

                if (!ok) throw Ops.TypeError("{1} error handler must return tuple containing (str, int), got {0}", Ops.GetPythonTypeName(res), encodeOrDecode);
                return replacement;
            }
        }

        class PythonDecoderFallback : DecoderFallback {
            private object function;
            private string str;
            private string enc;

            public PythonDecoderFallback(string encoding, string data, object callable) {
                function = callable;
                str = data;
                enc = encoding;
            }

            public override DecoderFallbackBuffer CreateFallbackBuffer() {
                return new PythonDecoderFallbackBuffer(enc, str, function);
            }

            public override int MaxCharCount {
                get { throw new Exception("The method or operation is not implemented."); }
            }

        }

        #endregion


    }
}
