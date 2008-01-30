/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;

using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Utils;
using IronPython.Runtime.Types;

[assembly: PythonModule("re", typeof(IronPython.Modules.PythonRegex))]
namespace IronPython.Modules {

    /// <summary>
    /// Python regular expression module.
    /// </summary>
    public static class PythonRegex {
        #region CONSTANTS

        // short forms
        public static object I = 0x02;
        public static object L = 0x04;
        public static object M = 0x08;
        public static object S = 0x10;
        public static object U = 0x20;
        public static object X = 0x40;

        // long forms
        public static object IGNORECASE = 0x02;
        public static object LOCALE = 0x04;
        public static object MULTILINE = 0x08;
        public static object DOTALL = 0x10;
        public static object UNICODE = 0x20;
        public static object VERBOSE = 0x40;

        #endregion

        #region Public API Surface

        [PythonName("compile")]
        public static RE_Pattern Compile(object pattern) {
            try {
                return new RE_Pattern(ValidatePattern(pattern));
            } catch (ArgumentException e) {
                throw PythonExceptions.CreateThrowable(error, e.Message);
            }
        }

        [PythonName("compile")]
        public static RE_Pattern Compile(object pattern, object flags) {
            try {
                return new RE_Pattern(ValidatePattern(pattern), Converter.ConvertToInt32(flags));
            } catch (ArgumentException e) {
                throw PythonExceptions.CreateThrowable(error, e.Message);
            }
        }
        public static string engine = "cli reg ex";

        public static PythonType error = PythonExceptions.CreateSubType(PythonExceptions.Exception, "error", "re", "");

        [PythonName("escape")]
        public static string Escape(string text) {
            if (text == null) throw PythonOps.TypeError("text must not be None");

            for (int i = 0; i < text.Length; i++) {
                if (!Char.IsLetterOrDigit(text[i])) {
                    StringBuilder sb = new StringBuilder(text, 0, i, text.Length);

                    char ch = text[i];
                    do {
                        sb.Append('\\');
                        sb.Append(ch);
                        i++;

                        int last = i;
                        while (i < text.Length) {
                            ch = text[i];
                            if (!Char.IsLetterOrDigit(ch)) {
                                break;
                            }
                            i++;
                        }
                        sb.Append(text, last, i - last);
                    } while (i < text.Length);

                    return sb.ToString();
                }
            }
            return text;
        }

        [PythonName("findall")]
        public static object FindAll(object pattern, string @string) {
            return FindAll(pattern, @string, 0);
        }

        [PythonName("findall")]
        public static object FindAll(object pattern, string @string, int flags) {
            RE_Pattern pat = new RE_Pattern(ValidatePattern(pattern), flags);
            ValidateString(@string, "string");

            MatchCollection mc = pat.FindAllWorker(@string, 0, @string.Length);
            return FixFindAllMatch(pat, mc);
        }

        private static object FixFindAllMatch(RE_Pattern pat, MatchCollection mc) {
            object[] matches = new object[mc.Count];
            int numgrps = pat._re.GetGroupNumbers().Length;
            for (int i = 0; i < mc.Count; i++) {
                if (numgrps > 2) { // CLR gives us a "bonus" group of 0 - the entire expression
                    //  at this point we have more than one group in the pattern;
                    //  need to return a list of tuples in this case

                    //  for each match item in the matchcollection, create a tuple representing what was matched
                    //  e.g. findall("(\d+)|(\w+)", "x = 99y") == [('', 'x'), ('99', ''), ('', 'y')]
                    //  in the example above, ('', 'x') did not match (\d+) as indicated by '' but did 
                    //  match (\w+) as indicated by 'x' and so on...
                    int k = 0;
                    List<object> tpl = new List<object>();
                    foreach (Group g in mc[i].Groups) {
                        //  here also the CLR gives us a "bonus" match as the first item which is the 
                        //  group that was actually matched in the tuple e.g. we get 'x', '', 'x' for 
                        //  the first match object...so we'll skip the first item when creating the 
                        //  tuple
                        if (k++ != 0) {
                            tpl.Add(g.Value);
                        }
                    }
                    matches[i] = PythonTuple.Make(tpl);
                } else if (numgrps == 2) {
                    //  at this point we have exactly one group in the pattern (including the "bonus" one given 
                    //  by the CLR 
                    //  skip the first match since that contains the entire match and not the group match
                    //  e.g. re.findall(r"(\w+)\s+fish\b", "green fish") will have "green fish" in the 0 
                    //  index and "green" as the (\w+) group match
                    matches[i] = mc[i].Groups[1].Value;
                } else {
                    matches[i] = mc[i].Value;
                }
            }

            return new List(matches);
        }

        [PythonName("finditer")]
        public static object FindIter(object pattern, object @string) {
            return FindIter(pattern, @string, 0);
        }

        [PythonName("finditer")]
        public static object FindIter(object pattern, object @string, int flags) {
            RE_Pattern pat = new RE_Pattern(ValidatePattern(pattern), flags);

            string str = ValidateString(@string, "string");
            return MatchIterator(pat.FindAllWorker(str, 0, str.Length), pat, str);
        }

        [PythonName("match")]
        public static object Match(object pattern, object @string) {
            return Match(pattern, @string, 0);
        }

        [PythonName("match")]
        public static object Match(object pattern, object @string, int flags) {
            return new RE_Pattern(ValidatePattern(pattern), flags).Match(ValidateString(@string, "string"));
        }

        [PythonName("search")]
        public static object Search(object pattern, object @string) {
            return Search(pattern, @string, 0);
        }
        [PythonName("search")]
        public static object Search(object pattern, object @string, int flags) {
            return new RE_Pattern(ValidatePattern(pattern), flags).Search(ValidateString(@string, "string"));
        }

        [PythonName("split")]
        public static object Split(object pattern, object @string) {
            return Split(ValidatePattern(pattern), ValidateString(@string, "string"), 0);
        }
        [PythonName("split")]
        public static object Split(object pattern, object @string, int maxSplit) {
            return new RE_Pattern(ValidatePattern(pattern)).Split(ValidateString(@string, "string"),
                maxSplit);
        }

        [PythonName("sub")]
        public static object Substitute(object pattern, object repl, object @string) {
            return Substitute(pattern, repl, @string, Int32.MaxValue);
        }

        [PythonName("sub")]
        public static object Substitute(object pattern, object repl, object @string, int count) {
            return new RE_Pattern(ValidatePattern(pattern)).Substitute(repl, ValidateString(@string, "string"), count);
        }

        [PythonName("subn")]
        public static object SubGetCount(object pattern, object repl, object @string) {
            return SubGetCount(pattern, repl, @string, Int32.MaxValue);
        }

        [PythonName("subn")]
        public static object SubGetCount(object pattern, object repl, object @string, int count) {
            return new RE_Pattern(ValidatePattern(pattern)).SubGetCount(repl, ValidateString(@string, "string"), count);

        }

        #endregion

        #region Public classes
        /// <summary>
        /// Compiled reg-ex pattern
        /// </summary>
        [PythonType("RE_Pattern")]
        public class RE_Pattern : IWeakReferenceable {
            internal Regex _re;
            private PythonDictionary _groups;
            private int _compileFlags;
            private WeakRefTracker _weakRefTracker;
            internal ParsedRegex _pre;

            public RE_Pattern(object pattern)
                : this(pattern, 0) {
            }

            public RE_Pattern(object pattern, int flags) {
                _pre = PreParseRegex(ValidatePattern(pattern));
                try {
                    RegexOptions opts = FlagsToOption(flags);
                    this._re = new Regex(_pre.Pattern, opts);
                } catch (ArgumentException e) {
                    throw PythonExceptions.CreateThrowable(error, e.Message);
                }
                this._compileFlags = flags;
            }

            [PythonName("match")]
            public RE_Match Match(object text) {
                string input = ValidateString(text, "text");
                return RE_Match.makeMatch(_re.Match(input), this, input, 0, input.Length);
            }

            private static int FixPosition(string text, int position) {
                if (position < 0) return 0;
                if (position > text.Length) return text.Length;

                return position;
            }

            [PythonName("match")]
            public RE_Match Match(object text, int pos) {
                string input = ValidateString(text, "text");
                pos = FixPosition(input, pos);
                return RE_Match.makeMatch(_re.Match(input, pos), this, input, pos, input.Length);
            }

            [PythonName("match")]
            public RE_Match Match(object text, [DefaultParameterValue(0)]int pos, int endpos) {
                string input = ValidateString(text, "text");
                pos = FixPosition(input, pos);
                endpos = FixPosition(input, endpos);
                return RE_Match.makeMatch(
                    _re.Match(input.Substring(0, endpos), pos),
                    this,
                    input,
                    pos,
                    endpos);
            }

            [PythonName("search")]
            public RE_Match Search(object text) {
                string input = ValidateString(text, "text");
                return RE_Match.make(_re.Match(input), this, input);
            }

            [PythonName("search")]
            public RE_Match Search(object text, int pos) {
                string input = ValidateString(text, "text");
                return RE_Match.make(_re.Match(input, pos, input.Length - pos), this, input);
            }

            [PythonName("search")]
            public RE_Match Search(object text, int pos, int endpos) {
                string input = ValidateString(text, "text");
                return RE_Match.make(_re.Match(input, pos, Math.Max(endpos - pos, 0)), this, input);
            }

            [PythonName("findall")]
            public object FindAll(string @string) {
                return FindAll(@string, 0, null);
            }

            [PythonName("findall")]
            public object FindAll(string @string, int pos) {
                return FindAll(@string, pos, null);
            }

            [PythonName("findall")]
            public object FindAll(object @string, int pos, object endpos) {
                MatchCollection mc = FindAllWorker(ValidateString(@string, "text"), pos, endpos);

                return FixFindAllMatch(this, mc);
            }

            internal MatchCollection FindAllWorker(string str, int pos, object endpos) {
                string against = str;
                if (endpos != null) {
                    int end = Converter.ConvertToInt32(endpos);
                    against = against.Substring(0, Math.Max(end, 0));
                }
                return _re.Matches(against, pos);
            }

            [PythonName("finditer")]
            public object FindIter(object @string) {
                string input = ValidateString(@string, "string");
                return MatchIterator(FindAllWorker(input, 0, input.Length), this, input);
            }

            [PythonName("finditer")]
            public object FindIter(object @string, int pos) {
                string input = ValidateString(@string, "string");
                return MatchIterator(FindAllWorker(input, pos, input.Length), this, input);
            }

            [PythonName("finditer")]
            public object FindIter(object @string, int pos, int endpos) {
                string input = ValidateString(@string, "string");
                return MatchIterator(FindAllWorker(input, pos, endpos), this, input);
            }

            [PythonName("split")]
            public object Split(object @string) {
                return Split(@string, 0);
            }

            [PythonName("split")]
            public object Split(object @string, int maxSplit) {
                List result = new List();
                // fast path for negative maxSplit ( == "make no splits")
                if (maxSplit < 0)
                    result.AddNoLock(@string);
                else {
                    // iterate over all matches
                    string theStr = ValidateString(@string, "string");
                    MatchCollection matches = _re.Matches(theStr);
                    int lastPos = 0; // is either start of the string, or first position *after* the last match
                    int nSplits = 0; // how many splits have occurred?
                    foreach (Match m in matches) {
                        if (m.Length > 0) {
                            // add substring from lastPos to beginning of current match
                            result.AddNoLock(theStr.Substring(lastPos, m.Index - lastPos));
                            // if there are subgroups of the match, add their match or None
                            if (m.Groups.Count > 1)
                                for (int i = 1; i < m.Groups.Count; i++)
                                    if (m.Groups[i].Success)
                                        result.AddNoLock(m.Groups[i].Value);
                                    else
                                        result.AddNoLock(null);
                            // update lastPos, nSplits
                            lastPos = m.Index + m.Length;
                            nSplits++;
                            if (nSplits == maxSplit)
                                break;
                        }
                    }
                    // add tail following last match
                    result.AddNoLock(theStr.Substring(lastPos));
                }
                return result;
            }

            [PythonName("sub")]
            public string Substitute(object repl, object @string) {
                return Substitute(repl, ValidateString(@string, "string"), Int32.MaxValue);
            }

            [PythonName("sub")]
            public string Substitute(object repl, object @string, int count) {
                if (repl == null) throw PythonOps.TypeError("NoneType is not valid repl");
                //  if 'count' is omitted or 0, all occurrences are replaced
                if (count == 0) count = Int32.MaxValue;

                string replacement = repl as string;
                if (replacement == null) {
                    if (repl is ExtensibleString) {
                        replacement = (repl as ExtensibleString).Value;
                    }
                }

                Match prev = null;
                string input = ValidateString(@string, "string");
                return _re.Replace(
                    input,
                    delegate(Match match) {
                        //  from the docs: Empty matches for the pattern are replaced 
                        //  only when not adjacent to a previous match
                        if (String.IsNullOrEmpty(match.Value) && prev != null &&
                                        (prev.Index + prev.Length) == match.Index) {
                            return "";
                        };
                        prev = match;

                        if (replacement != null) return UnescapeGroups(match, replacement);
                        return PythonCalls.Call(repl, RE_Match.make(match, this, input)) as string;
                    },
                    count);
            }

            [PythonName("subn")]
            public object SubGetCount(object repl, string @string) {
                return SubGetCount(repl, @string, Int32.MaxValue);
            }

            [PythonName("subn")]
            public object SubGetCount(object repl, object @string, int count) {
                if (repl == null) throw PythonOps.TypeError("NoneType is not valid repl");
                //  if 'count' is omitted or 0, all occurrences are replaced
                if (count == 0) count = Int32.MaxValue;

                int totalCount = 0;
                string res;
                string replacement = repl as string;

                if (replacement == null) {
                    if (repl is ExtensibleString) {
                        replacement = (repl as ExtensibleString).Value;
                    }
                }

                Match prev = null;
                string input = ValidateString(@string, "string");
                res = _re.Replace(
                    input,
                    delegate(Match match) {
                        //  from the docs: Empty matches for the pattern are replaced 
                        //  only when not adjacent to a previous match
                        if (String.IsNullOrEmpty(match.Value) && prev != null &&
                            (prev.Index + prev.Length) == match.Index) {
                            return "";
                        };
                        prev = match;

                        totalCount++;
                        if (replacement != null) return UnescapeGroups(match, replacement);

                        return PythonCalls.Call(repl, RE_Match.make(match, this, input)) as string;
                    },
                    count);

                return PythonTuple.MakeTuple(res, totalCount);
            }

            public int Flags {
                [PythonName("flags")]
                get {
                    return _compileFlags;
                }
            }

            public PythonDictionary GroupIndex {
                [PythonName("groupindex")]
                get {
                    if (_groups == null) {
                        PythonDictionary d = new PythonDictionary();
                        string[] names = _re.GetGroupNames();
                        int[] nums = _re.GetGroupNumbers();
                        for (int i = 1; i < names.Length; i++) {
                            if (Char.IsDigit(names[i][0])) continue;    // skip numeric names

                            d[names[i]] = nums[i];
                        }
                        _groups = d;
                    }
                    return _groups;
                }
            }

            public string Pattern {
                [PythonName("pattern")]
                get {
                    return _pre.UserPattern;
                }
            }


            #region IWeakReferenceable Members

            public WeakRefTracker GetWeakRef() {
                return _weakRefTracker;
            }

            public bool SetWeakRef(WeakRefTracker value) {
                _weakRefTracker = value;
                return true;
            }

            public void SetFinalizer(WeakRefTracker value) {
                SetWeakRef(value);
            }

            #endregion
        }

        [PythonType("RE_Match")]
        public class RE_Match {
            RE_Pattern _pattern;
            private Match _m;
            private string _text;
            private int _lastindex = -1;
            private int _endPos;

            #region Internal makers
            internal static RE_Match make(Match m, RE_Pattern pattern, string input) {
                if (m.Success) return new RE_Match(m, pattern, input);
                return null;
            }

            internal static RE_Match makeMatch(Match m, RE_Pattern pattern, string input, int offset, int endpos) {
                if (m.Success && m.Index == offset) return new RE_Match(m, pattern, input, endpos);
                return null;
            }
            #endregion

            #region Public ctors

            public RE_Match(Match m, RE_Pattern pattern, string text) {
                _m = m;
                _pattern = pattern;
                _text = text;
            }

            public RE_Match(Match m, RE_Pattern pattern, string text, int endpos) {
                _m = m;
                _pattern = pattern;
                _text = text;
                _endPos = endpos;
            }

            #endregion

            //			public override bool __nonzero__() {
            //				return m.Success;
            //			}

            #region Public API Surface

            [PythonName("end")]
            public int End() {
                return _m.Index + _m.Length;
            }

            [PythonName("start")]
            public int Start() {
                return _m.Index;
            }

            [PythonName("start")]
            public int Start(object group) {
                int grpIndex = GetGroupIndex(group);
                if (!_m.Groups[grpIndex].Success) {
                    return -1;
                }
                return _m.Groups[grpIndex].Index;
            }

            [PythonName("end")]
            public int End(object group) {
                int grpIndex = GetGroupIndex(group);
                if (!_m.Groups[grpIndex].Success) {
                    return -1;
                }
                return _m.Groups[grpIndex].Index + _m.Groups[grpIndex].Length;
            }

            [PythonName("group")]
            public object Group(object index, params object[] additional) {
                if (additional.Length == 0) return Group(index);

                object[] res = new object[additional.Length + 1];
                res[0] = _m.Groups[GetGroupIndex(index)].Success ? _m.Groups[GetGroupIndex(index)].Value : null;
                for (int i = 1; i < res.Length; i++) {
                    int grpIndex = GetGroupIndex(additional[i - 1]);
                    res[i] = _m.Groups[grpIndex].Success ? _m.Groups[grpIndex].Value : null;
                }
                return PythonTuple.MakeTuple(res);
            }

            [PythonName("group")]
            public object Group(object index) {
                int pos = GetGroupIndex(index);
                Group g = _m.Groups[pos];
                return g.Success ? g.Value : null;

            }

            [PythonName("group")]
            public object Group() {
                return Group(0);
            }

            [PythonName("groups")]
            public object Groups() {
                return Groups(null);
            }

            [PythonName("groups")]
            public object Groups(object @default) {
                object[] ret = new object[_m.Groups.Count - 1];
                for (int i = 1; i < _m.Groups.Count; i++) {
                    if (!_m.Groups[i].Success) {
                        ret[i - 1] = @default;
                    } else {
                        ret[i - 1] = _m.Groups[i].Value;
                    }
                }
                return PythonTuple.MakeTuple(ret);
            }

            [PythonName("expand")]
            public object Expand(object template) {
                string strTmp = ValidateString(template, "template");

                StringBuilder res = new StringBuilder();
                for (int i = 0; i < strTmp.Length; i++) {
                    if (strTmp[i] != '\\') { res.Append(strTmp[i]); continue; }
                    if (++i == strTmp.Length) { res.Append(strTmp[i - 1]); continue; }

                    if (Char.IsDigit(strTmp[i])) {
                        AppendGroup(res, (int)(strTmp[i] - '0'));
                    } else if (strTmp[i] == 'g') {
                        if (++i == strTmp.Length) { res.Append("\\g"); return res.ToString(); }
                        if (strTmp[i] != '<') {
                            res.Append("\\g<"); continue;
                        } else { // '<'
                            StringBuilder name = new StringBuilder();
                            i++;
                            while (strTmp[i] != '>' && i < strTmp.Length) {
                                name.Append(strTmp[i++]);
                            }
                            AppendGroup(res, _pattern._re.GroupNumberFromName(name.ToString()));
                        }
                    } else {
                        switch (strTmp[i]) {
                            case 'n': res.Append('\n'); break;
                            case 'r': res.Append('\r'); break;
                            case 't': res.Append('\t'); break;
                            case '\\': res.Append('\\'); break;
                        }
                    }

                }
                return res.ToString();
            }

            [PythonName("groupdict")]
            public object GroupDict() {
                return GroupDict(null);
            }
            
            private static bool IsGroupNumber(string name) {
                foreach (char c in name) {
                    if (!Char.IsNumber(c)) return false;
                }
                return true;
            }

            [PythonName("groupdict")]
            public object GroupDict(object value) {
                string[] groupNames = this._pattern._re.GetGroupNames();
                Debug.Assert(groupNames.Length == this._m.Groups.Count);
                PythonDictionary d = new PythonDictionary();
                for (int i = 0; i < groupNames.Length; i++) {
                    if (IsGroupNumber(groupNames[i])) continue; // python doesn't report group numbers

                    if (_m.Groups[i].Captures.Count != 0) {
                        d[groupNames[i]] = _m.Groups[i].Value;
                    } else {
                        d[groupNames[i]] = value;
                    }
                }
                return d;
            }

            [PythonName("span")]
            public object Span() {
                return PythonTuple.MakeTuple(this.Start(), this.End());
            }

            [PythonName("span")]
            public object Span(object group) {
                return PythonTuple.MakeTuple(this.Start(group), this.End(group));
            }

            public int Position {
                [PythonName("pos")]
                get {
                    return _m.Index;
                }
            }

            public int EndPosition {
                [PythonName("endpos")]
                get {
                    return _endPos;
                }
            }

            public string SearchValue {
                [PythonName("string")]
                get {
                    return _text;
                }
            }

            public object Regs {
                [PythonName("regs")]
                get {
                    object[] res = new object[_m.Groups.Count];
                    for (int i = 0; i < res.Length; i++) {
                        res[i] = PythonTuple.MakeTuple(Start(i), End(i));
                    }

                    return PythonTuple.MakeTuple(res);
                }
            }
            public object Pattern {
                [PythonName("re")]
                get {
                    return _pattern;
                }
            }

            public object LastIndex {
                [PythonName("lastindex")]
                get {
                    //   -1 : initial value of lastindex
                    //    0 : no match found
                    //other : the true lastindex

                    // Match.Groups contains "lower" level matched groups, which has to be removed
                    if (_lastindex == -1) {
                        int i = 1;
                        while (i < _m.Groups.Count) {
                            if (_m.Groups[i].Success) {
                                _lastindex = i;
                                int start = _m.Groups[i].Index;
                                int end = start + _m.Groups[i].Length;
                                i++;

                                // skip any group which fall into the range [start, end], 
                                // no matter match succeed or fail
                                while (i < _m.Groups.Count && (_m.Groups[i].Index < end)) {
                                    i++;
                                }
                            } else {
                                i++;
                            }
                        }

                        if (_lastindex == -1) {
                            _lastindex = 0;
                        }
                    }

                    if (_lastindex == 0) {
                        return null;
                    } else {
                        return _lastindex;
                    }
                }
            }

            public object LastGroup {
                [PythonName("lastgroup")]
                get {
                    if (LastIndex == null) return null;

                    // when group was not explicitly named, RegEx assigns the number as name
                    // This is different from C-Python, which returns None in such cases

                    return this._pattern._re.GroupNameFromNumber((int)LastIndex);
                }
            }

            #endregion

            #region Private helper functions
            private void AppendGroup(StringBuilder sb, int index) {
                sb.Append(_m.Groups[index].Value);
            }

            private int GetGroupIndex(object group) {
                int grpIndex;
                if (!Converter.TryConvertToInt32(group, out grpIndex)) {
                    grpIndex = _pattern._re.GroupNumberFromName(ValidateString(group, "group"));
                }
                if (grpIndex < 0 || grpIndex >= _m.Groups.Count) {
                    throw PythonOps.IndexError("no such group");
                }
                return grpIndex;
            }
            #endregion
        }

        #endregion

        #region Private helper functions
        private static IEnumerator MatchIterator(MatchCollection matches, RE_Pattern pattern, string input) {
            for (int i = 0; i < matches.Count; i++) {
                yield return RE_Match.make(matches[i], pattern, input);
            }
        }

        private static RegexOptions FlagsToOption(int flags) {
            RegexOptions opts = RegexOptions.None;
            if ((flags & (int)IGNORECASE) != 0) opts |= RegexOptions.IgnoreCase;
            if ((flags & (int)MULTILINE) != 0) opts |= RegexOptions.Multiline;
            if (((flags & (int)LOCALE)) == 0) opts &= (~RegexOptions.CultureInvariant);
            if ((flags & (int)DOTALL) != 0) opts |= RegexOptions.Singleline;
            if ((flags & (int)VERBOSE) != 0) opts |= RegexOptions.IgnorePatternWhitespace;

            return opts;
        }

        internal class ParsedRegex {
            public ParsedRegex(string pattern) {
                this.UserPattern = pattern;
            }

            public string UserPattern;
            public string Pattern;
            public RegexOptions Options = RegexOptions.CultureInvariant;
        }

        /// <summary>
        /// Preparses a regular expression text returning a ParsedRegex class
        /// that can be used for further regular expressions.
        /// </summary>
        private static ParsedRegex PreParseRegex(string pattern) {
            ParsedRegex res = new ParsedRegex(pattern);

            //string newPattern;
            int cur = 0, nameIndex;
            int curGroup = 0;
            bool containsNamedGroup = false;

            for (; ; ) {
                nameIndex = pattern.IndexOf("(", cur);
                if (nameIndex > 0 && pattern[nameIndex - 1] == '\\') {
                    int curIndex = nameIndex - 2;
                    int backslashCount = 1;
                    while (curIndex >= 0 && pattern[curIndex] == '\\') {
                        backslashCount++;
                        curIndex--;
                    }
                    // odd number of back slashes, this is an optional
                    // paren that we should ignore.
                    if ((backslashCount & 0x01) != 0) {
                        cur++;
                        continue;
                    }
                }

                if (nameIndex == -1) break;
                if (nameIndex == pattern.Length - 1) break;

                switch (pattern[++nameIndex]) {
                    case '?':
                        // extension syntax
                        if (nameIndex == pattern.Length - 1) throw PythonExceptions.CreateThrowable(error, "unexpected end of regex");
                        switch (pattern[++nameIndex]) {
                            case 'P':
                                //  named regex, .NET doesn't expect the P so we'll remove it;
                                //  also, once we see a named group i.e. ?P then we need to start artificially 
                                //  naming all unnamed groups from then on---this is to get around the fact that 
                                //  the CLR RegEx support orders all the unnamed groups before all the named 
                                //  groups, even if the named groups are before the unnamed ones in the pattern;
                                //  the artificial naming preserves the order of the groups and thus the order of
                                //  the matches
                                if (nameIndex + 1 < pattern.Length && pattern[nameIndex + 1] == '=') {
                                    // match whatever was previously matched by the named group

                                    // remove the (?P=
                                    pattern = pattern.Remove(nameIndex - 2, 4);
                                    pattern = pattern.Insert(nameIndex - 2, "\\\\k<");
                                    int tmpIndex = nameIndex;
                                    while (tmpIndex < pattern.Length && pattern[tmpIndex] != ')')
                                        tmpIndex++;

                                    if (tmpIndex == pattern.Length) throw PythonExceptions.CreateThrowable(error, "unexpected end of regex");

                                    pattern = pattern.Substring(0, tmpIndex) + ">" + pattern.Substring(tmpIndex + 1);
                                } else {
                                    containsNamedGroup = true;
                                    pattern = pattern.Remove(nameIndex, 1);
                                }
                                break;
                            case 'i': res.Options |= RegexOptions.IgnoreCase; break;
                            case 'L': res.Options &= ~(RegexOptions.CultureInvariant); break;
                            case 'm': res.Options |= RegexOptions.Multiline; break;
                            case 's': res.Options |= RegexOptions.Singleline; break;
                            case 'u': break;
                            case 'x': res.Options |= RegexOptions.IgnorePatternWhitespace; break;
                            case ':': break; // non-capturing
                            case '=': break; // look ahead assertion
                            case '<': break; // positive look behind assertion
                            case '!': break; // negative look ahead assertion
                            case '#': break; // inline comment
                            case '(':  // yes/no if group exists, we don't support this
                            default: throw PythonExceptions.CreateThrowable(error, "Unrecognized extension " + pattern[nameIndex]);
                        }
                        break;
                    default:
                        // just another group
                        curGroup++;
                        if (containsNamedGroup) {
                            // need to name this unnamed group
                            pattern = pattern.Insert(nameIndex, "?<Named" + GetRandomString() + ">");
                        }
                        break;
                }

                cur = nameIndex;
            }

            cur = 0;
            for (; ; ) {
                nameIndex = pattern.IndexOf('\\', cur);

                if (nameIndex == -1 || nameIndex == pattern.Length - 1) break;
                char curChar = pattern[++nameIndex];
                switch (curChar) {
                    case 'x':
                    case 'u':
                    case 'a':
                    case 'b':
                    case 'e':
                    case 'f':
                    case 'n':
                    case 'r':
                    case 't':
                    case 'v':
                    case 'c':
                    case 's':
                    case 'W':
                    case 'w':
                    case 'p':
                    case 'P':
                    case 'S':
                    case 'd':
                    case 'D':
                    case 'Z':
                        // known escape sequences, leave escaped.
                        break;
                    case '\\':
                        // escaping a \\
                        cur += 2;
                        break;
                    default:
                        System.Globalization.UnicodeCategory charClass = Char.GetUnicodeCategory(curChar);
                        switch (charClass) {
                            // recognized word characters, always unescape.
                            case System.Globalization.UnicodeCategory.ModifierLetter:
                            case System.Globalization.UnicodeCategory.LowercaseLetter:
                            case System.Globalization.UnicodeCategory.UppercaseLetter:
                            case System.Globalization.UnicodeCategory.TitlecaseLetter:
                            case System.Globalization.UnicodeCategory.OtherLetter:
                            case System.Globalization.UnicodeCategory.LetterNumber:
                            case System.Globalization.UnicodeCategory.OtherNumber:
                            case System.Globalization.UnicodeCategory.ConnectorPunctuation:
                                pattern = pattern.Remove(nameIndex - 1, 1);
                                break;
                            case System.Globalization.UnicodeCategory.DecimalDigitNumber:
                                //  actually don't want to unescape '\1', '\2' etc. which are references to groups
                                break;
                        }
                        break;
                }
                cur++;
            }

            res.Pattern = pattern;
            return res;
        }

        static Random r = new Random(DateTime.Now.Millisecond);
        private static string GetRandomString() {
            return r.Next(Int32.MaxValue / 2, Int32.MaxValue).ToString();
        }

        private static string UnescapeGroups(Match m, string text) {
            for (int i = 0; i < text.Length; i++) {
                if (text[i] == '\\') {
                    StringBuilder sb = new StringBuilder(text, 0, i, text.Length);

                    do {
                        if (text[i] == '\\') {
                            i++;
                            if (i == text.Length) { sb.Append('\\'); break; }

                            switch (text[i]) {
                                case 'n': sb.Append('\n'); break;
                                case 'r': sb.Append('\r'); break;
                                case 't': sb.Append('\t'); break;
                                case '\\': sb.Append('\\'); break;
                                case '\'': sb.Append('\''); break;
                                case '"': sb.Append('"'); break;
                                case 'b': sb.Append('\b'); break;
                                case 'g':
                                    //  \g<#>, \g<name> need to be substituted by the groups they 
                                    //  matched
                                    if (text[i + 1] == '<') {
                                        int anglebrkStart = i + 1;
                                        int anglebrkEnd = text.IndexOf('>', i + 2);
                                        if (anglebrkEnd != -1) {
                                            //  grab the # or 'name' of the group between '< >'
                                            int lengrp = anglebrkEnd - (anglebrkStart + 1);
                                            string grp = text.Substring(anglebrkStart + 1, lengrp);
                                            int num;
                                            Group g;

                                            if (StringUtils.TryParseInt32(grp, out num)) {
                                                g = m.Groups[num];
                                                if (String.IsNullOrEmpty(g.Value)) {
                                                    throw PythonOps.IndexError("unknown group reference");
                                                }
                                                sb.Append(g.Value);
                                            } else {
                                                g = m.Groups[grp];
                                                if (String.IsNullOrEmpty(g.Value)) {
                                                    throw PythonOps.IndexError("unknown group reference");
                                                }
                                                sb.Append(g.Value);
                                            }
                                            i = anglebrkEnd;
                                        }
                                        break;
                                    }
                                    sb.Append('\\');
                                    sb.Append((char)text[i]);
                                    break;
                                default:
                                    if (Char.IsDigit(text[i]) && text[i] <= '7') {
                                        int val = 0;
                                        int digitCount = 0;
                                        while (i < text.Length && Char.IsDigit(text[i]) && text[i] <= '7') {
                                            digitCount++;
                                            val += val * 8 + (text[i] - '0');
                                            i++;
                                        }
                                        i--;

                                        if (digitCount == 1 && val > 0 && val < m.Groups.Count) {
                                            sb.Append(m.Groups[val].Value);
                                        } else {
                                            sb.Append((char)val);
                                        }
                                    } else {
                                        sb.Append('\\');
                                        sb.Append((char)text[i]);
                                    }
                                    break;
                            }
                        } else {
                            sb.Append(text[i]);
                        }
                    } while (++i < text.Length);
                    return sb.ToString();
                }
            }
            return text;
        }

        private static string ValidatePattern(object pattern) {
            if (pattern is string) return pattern as string;

            ExtensibleString es = pattern as ExtensibleString;
            if (es != null) return es.Value;

            RE_Pattern rep = pattern as RE_Pattern;
            if (rep != null) return rep._pre.UserPattern;

            throw PythonOps.TypeError("pattern must be a string or compiled pattern");
        }

        private static string ValidateString(object str, string param) {
            if (str is string) return str as string;

            ExtensibleString es = str as ExtensibleString;
            if (es != null) return es.Value;

            throw PythonOps.TypeError("expected string for parameter '{0}' but got '{1}'", param, PythonOps.GetPythonTypeName(str));
        }

        #endregion
    }
}
