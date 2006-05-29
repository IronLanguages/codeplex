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
using System.Text.RegularExpressions;

using IronPython.Runtime;

[assembly: PythonModule("datetime", typeof(IronPython.Modules.PythonDateTime))]
namespace IronPython.Modules {
    public class PythonDateTime {
        [PythonType("date")]
        public class PythonDate {
            private DateTime _value;
            
            public PythonDate(int year, int month, int day) {
                _value = new DateTime(year, month, day);
            }

            [PythonName("weekday")]
            public int Weekday() {
                int ret = (int)_value.DayOfWeek - 1;
                if (ret < 0) return 6;
                else return ret;
            }

            [PythonName("toordinal")]
            public int ToOrdinal() {
                TimeSpan t = _value - new DateTime(1, 1, 1);
                return t.Days + 1;
            }


            public int Year {
                [PythonName("year")]
                get { return _value.Year; }
            }

            public int Month {
                [PythonName("month")]
                get { return _value.Month; }
            }

            public int Day {
                [PythonName("day")]
                get { return _value.Day; }
            }

            private static Hashtable strftimeMap;
            
            static PythonDate() {
                strftimeMap = new Hashtable();
                strftimeMap["%A"] = "dddd";
                strftimeMap["%a"] = "ddd";
                strftimeMap["%B"] = "MMMM";
                strftimeMap["%b"] = "MMM";
                strftimeMap["%Y"] = "yyyy";
                strftimeMap["%y"] = "yy";
                //!!! many more to add
            }

            static string ReplaceFormat(Match m) {
                string ret = (string)strftimeMap[m.Value];
                if (ret != null) return ret;
                else return m.Value;
            }

            [PythonName("strftime")]
            public string Format(string dateFormat) {
                Regex r = new Regex("%[a-zA-Z]");
                string mappedFormat = r.Replace(dateFormat, ReplaceFormat);
                return _value.ToString(mappedFormat);
            }

            [PythonName("__str__")]
            public override string ToString() {
                return string.Format("datetime.date({0}, {1}, {2})", _value.Year, _value.Month, _value.Day);
            }
        }
    }
}
