/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Threading;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

[assembly: PythonModule("time", typeof(IronPython.Modules.PythonTime))]
namespace IronPython.Modules {
    [PythonType("time")]
    public static class PythonTime {
        private const int YearIndex = 0;
        private const int MonthIndex = 1;
        private const int DayIndex = 2;
        private const int HourIndex = 3;
        private const int MinuteIndex = 4;
        private const int SecondIndex = 5;
        private const int WeekdayIndex = 6;
        private const int DayOfYearIndex = 7;
        private const int IsDaylightSavingsIndex = 8;
        private const int MaxIndex = 9;

        private const int minYear = 1900;   // minimum year for python dates (CLS dates are bigger)

        public static int altzone;
        public static int daylight;
        public static int timezone;
        public static string tzname;
        public static bool accept2dyear = true;

#if !SILVERLIGHT    // System.Diagnostics.Stopwatch
        private static Stopwatch sw;
#endif

        static PythonTime() {
            daylight = DateTime.Now.IsDaylightSavingTime() ? 1 : 0;

            // altzone, timezone are offsets from UTC in seconds, so they always fit in the
            // -13*3600 to 13*3600 range and are safe to cast to ints
#if !SILVERLIGHT
            tzname = TimeZone.CurrentTimeZone.StandardName;
            altzone = (int)-TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalSeconds;
            timezone = altzone;
            if (daylight != 0) {
                timezone += (int)TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year).Delta.TotalSeconds;
            }
#else
            tzname = TimeZoneInfo.Local.StandardName;
            timezone = (int)-TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds;
            altzone = (int)-TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
#endif
        }

        [PythonName("asctime")]
        public static string AscTime() {
            return AscTime(null);
        }

        [PythonName("asctime")]
        public static string AscTime(object time) {
            DateTime dt;
            if (time is PythonTuple) {
                dt = GetDateTimeFromTuple(time as PythonTuple);
            } else if (time == null) {
                dt = DateTime.Now;
            } else {
                throw PythonOps.TypeError("expected struct_time or None");
            }

            return dt.ToString("ddd MMM dd HH:mm:ss yyyy", null);
        }

#if !SILVERLIGHT    // System.Diagnostics.Stopwatch
        [PythonName("clock")]
        public static double Clock() {
            InitStopWatch();
            return ((double)sw.ElapsedTicks) / Stopwatch.Frequency;
        }
#endif

        [PythonName("ctime")]
        public static string CTime() {
            return AscTime(LocalTime());
        }

        [PythonName("ctime")]
        public static string CTime(object seconds) {
            if (seconds == null)
                return CTime();
            return AscTime(LocalTime(seconds));
        }

        [PythonName("sleep")]
        public static void Sleep(double tm) {
            Thread.Sleep((int)(tm * 1000));
        }

        [PythonName("time")]
        public static double CurrentTime() {
            return DateTime.Now.Ticks / 1.0e7;
        }

        [PythonName("localtime")]
        public static PythonTuple LocalTime() {
            return GetDateTimeTuple(DateTime.Now);
        }

        [PythonName("localtime")]
        public static PythonTuple LocalTime(object seconds) {
            if (seconds == null) return LocalTime();

            long intSeconds = GetDateTimeFromObject(seconds);

            return GetDateTimeTuple(new DateTime(intSeconds * TimeSpan.TicksPerSecond, DateTimeKind.Local));
        }

        [PythonName("gmtime")]
        public static PythonTuple UniversalTime() {
            return GetDateTimeTuple(DateTime.Now.ToUniversalTime());
        }

        [PythonName("gmtime")]
        public static PythonTuple UniversalTime(object seconds) {
            if (seconds == null) return UniversalTime();

            long intSeconds = GetDateTimeFromObject(seconds);

            return GetDateTimeTuple(new DateTime(intSeconds * TimeSpan.TicksPerSecond).ToUniversalTime());
        }

        [PythonName("mktime")]
        public static double MakeTime(PythonTuple localTime) {
            return GetDateTimeFromTuple(localTime).Ticks / 1.0e7;
        }

        [PythonName("strftime")]
        public static string FormatTime(string format) {
            return FormatTime(format, DateTime.Now);
        }

        [PythonName("strftime")]
        public static string FormatTime(string format, PythonTuple dateTime) {
            return FormatTime(format, GetDateTimeFromTuple(dateTime));
        }

        [PythonName("strptime")]
        public static object ParseTime(string @string) {
            return DateTime.Parse(@string, PythonLocale.currentLocale.Time.DateTimeFormat);
        }

        [PythonName("strptime")]
        public static object ParseString(string @string, string format) {
            bool postProc;
            List<FormatInfo> formatInfo = PythonFormatToCLIFormat(format, true, out postProc);

            DateTime res;
            if (postProc) {
                int doyIndex = FindFormat(formatInfo, "\\%j");
                int dowMIndex = FindFormat(formatInfo, "\\%W");
                int dowSIndex = FindFormat(formatInfo, "\\%U");

                if (doyIndex != -1 && dowMIndex == -1 && dowSIndex == -1) {
                    res = new DateTime(1900, 1, 1);
                    res = res.AddDays(Int32.Parse(@string));
                } else if (dowMIndex != -1 && doyIndex == -1 && dowSIndex == -1) {
                    res = new DateTime(1900, 1, 1);
                    res = res.AddDays(Int32.Parse(@string) * 7);
                } else if (dowSIndex != -1 && doyIndex == -1 && dowMIndex == -1) {
                    res = new DateTime(1900, 1, 1);
                    res = res.AddDays(Int32.Parse(@string) * 7);
                } else {
                    throw PythonOps.ValueError("cannot parse %j, %W, or %U w/ other values");
                }
            } else {
                string[] formats = new string[formatInfo.Count];
                for (int i = 0; i < formatInfo.Count; i++) {
                    switch (formatInfo[i].Type) {
                        case FormatInfoType.UserText: formats[i] = "'" + formatInfo[i].Text + "'"; break;
                        case FormatInfoType.SimpleFormat: formats[i] = formatInfo[i].Text; break;
                        case FormatInfoType.CustomFormat:
                            // include % if we only have one specifier to mark that it's a custom
                            // specifier
                            if (formatInfo.Count == 1 && formatInfo[i].Text.Length == 1) {
                                formats[i] = "%" + formatInfo[i].Text;
                            } else {
                                formats[i] = formatInfo[i].Text;
                            }
                            break;
                    }
                }

                try {
                    if (!StringUtils.TryParseDateTimeExact(@string,
                        String.Join("", formats),
                        PythonLocale.currentLocale.Time.DateTimeFormat,
                        DateTimeStyles.AllowWhiteSpaces,
                        out res)) {
                        // If TryParseExact fails, fall back to DateTime.Parse which does a better job in some cases...
                        res = DateTime.Parse(@string, PythonLocale.currentLocale.Time.DateTimeFormat);
                    }
                } catch (FormatException e) {
                    throw PythonOps.ValueError(e.Message + Environment.NewLine + "data=" + @string + ", fmt=" + format + ", to: " + String.Join("", formats));
                }
            }

            return GetDateTimeTuple(res);
        }

        internal static string FormatTime(string format, DateTime dt) {
            bool postProc;
            List<FormatInfo> formatInfo = PythonFormatToCLIFormat(format, false, out postProc);
            StringBuilder res = new StringBuilder();

            for (int i = 0; i < formatInfo.Count; i++) {
                switch (formatInfo[i].Type) {
                    case FormatInfoType.UserText: res.Append(formatInfo[i].Text); break;
                    case FormatInfoType.SimpleFormat: res.Append(dt.ToString(formatInfo[i].Text, PythonLocale.currentLocale.Time.DateTimeFormat)); break;
                    case FormatInfoType.CustomFormat:
                        // custom format strings need to be at least 2 characters long                        
                        res.Append(dt.ToString("%" + formatInfo[i].Text, PythonLocale.currentLocale.Time.DateTimeFormat));
                        break;
                }
            }

            if (postProc) {
                res = res.Replace("%j", dt.DayOfYear.ToString("D03"));  // day of the year (001 - 366)

                // figure out first day of the year...
                DateTime first = new DateTime(dt.Year, 1, 1);
                int weekOneSunday = (7 - (int)first.DayOfWeek) % 7;
                int dayOffset = (8 - (int)first.DayOfWeek) % 7;

                // week of year  (sunday first day, 0-53), all days before Sunday are 0
                res = res.Replace("%U", (((dt.DayOfYear + 6 - weekOneSunday) / 7)).ToString());
                // week number of year (monday first day, 0-53), all days before Monday are 0
                res = res.Replace("%W", (((dt.DayOfYear + 6 - dayOffset) / 7)).ToString());                
                res = res.Replace("%w", ((int)dt.DayOfWeek).ToString());
            }
            return res.ToString();
        }

        private static long GetDateTimeFromObject(object seconds) {
            int intSeconds;
            if (Converter.TryConvertToInt32(seconds, out intSeconds)) {
                return intSeconds;
            }

            double dblVal;
            if (Converter.TryConvertToDouble(seconds, out dblVal)) {
                if (dblVal > Int64.MaxValue || dblVal < Int64.MinValue) throw PythonOps.ValueError("unreasonable date/time");
                return (long)dblVal;
            }

            throw PythonOps.TypeError("expected int, got {0}", DynamicHelpers.GetDynamicType(seconds));
        }

        enum FormatInfoType {
            UserText,
            SimpleFormat,
            CustomFormat,
        }

        class FormatInfo {
            public FormatInfo(string text) {
                Type = FormatInfoType.SimpleFormat;
                Text = text;
            }

            public FormatInfo(FormatInfoType type, string text) {
                Type = type;
                Text = text;
            }

            public FormatInfoType Type;
            public string Text;

            public override string ToString() {
                return string.Format("{0}:{1}", Type, Text);
            }
        }

        // temporary solution
        private static void AddTime(List<FormatInfo> newFormat) {
            newFormat.Add(new FormatInfo("HH"));
            newFormat.Add(new FormatInfo(FormatInfoType.UserText, ":"));
            newFormat.Add(new FormatInfo("mm"));
            newFormat.Add(new FormatInfo(FormatInfoType.UserText, ":"));
            newFormat.Add(new FormatInfo("ss"));
        }

        private static void AddDate(List<FormatInfo> newFormat) {
            newFormat.Add(new FormatInfo("MM"));
            newFormat.Add(new FormatInfo(FormatInfoType.UserText, "/"));
            newFormat.Add(new FormatInfo("dd"));
            newFormat.Add(new FormatInfo(FormatInfoType.UserText, "/"));
            newFormat.Add(new FormatInfo("yy"));
        }

        private static List<FormatInfo> PythonFormatToCLIFormat(string format, bool forParse, out bool postProcess) {
            postProcess = false;
            List<FormatInfo> newFormat = new List<FormatInfo>();

            for (int i = 0; i < format.Length; i++) {
                if (format[i] == '%') {
                    if (i + 1 == format.Length) throw PythonOps.ValueError("badly formatted string");

                    switch (format[++i]) {
                        case 'a': newFormat.Add(new FormatInfo("ddd")); break;
                        case 'A': newFormat.Add(new FormatInfo("dddd")); break;
                        case 'b': newFormat.Add(new FormatInfo("MMM")); break;
                        case 'B': newFormat.Add(new FormatInfo("MMMM")); break;
                        case 'c':
                            AddDate(newFormat);
                            newFormat.Add(new FormatInfo(FormatInfoType.UserText, " "));
                            AddTime(newFormat);
                            break;
                        case 'd':
                            // if we're parsing we want to use the less-strict
                            // d format and which doesn't require both digits.
                            if (forParse) newFormat.Add(new FormatInfo(FormatInfoType.CustomFormat, "d"));
                            else newFormat.Add(new FormatInfo("dd"));
                            break;
                        case 'H': newFormat.Add(new FormatInfo("HH")); break;
                        case 'I': newFormat.Add(new FormatInfo("hh")); break;
                        case 'm': newFormat.Add(new FormatInfo("MM")); break;
                        case 'M': newFormat.Add(new FormatInfo("mm")); break;
                        case 'p':
                            newFormat.Add(new FormatInfo(FormatInfoType.CustomFormat, "t"));
                            newFormat.Add(new FormatInfo(FormatInfoType.UserText, "M"));
                            break;
                        case 'S': newFormat.Add(new FormatInfo("ss")); break;
                        case 'x':
                            AddDate(newFormat); break;
                        case 'X':
                            AddTime(newFormat);
                            break;
                        case 'y': newFormat.Add(new FormatInfo("yy")); break;
                        case 'Y': newFormat.Add(new FormatInfo("yyyy")); break;
                        case '%': newFormat.Add(new FormatInfo("\\%")); break;

                        // format conversions not defined by the CLR.  We leave
                        // them as \\% and then replace them by hand later
                        case 'j': newFormat.Add(new FormatInfo("\\%j")); postProcess = true; break; // day of year
                        case 'W': newFormat.Add(new FormatInfo("\\%W")); postProcess = true; break;
                        case 'U': newFormat.Add(new FormatInfo("\\%U")); postProcess = true; break; // week number
                        case 'w': newFormat.Add(new FormatInfo("\\%w")); postProcess = true; break; // weekday number
                        case 'z':
                        case 'Z':
                            // !!!TODO: 
                            // 'z' for offset
                            // 'Z' for time zone name; could be from PythonTimeZoneInformation
                            newFormat.Add(new FormatInfo(FormatInfoType.UserText, ""));
                            break;
                        default:
                            newFormat.Add(new FormatInfo(FormatInfoType.UserText, "")); break;
                    }
                } else {
                    if (newFormat.Count == 0 || newFormat[newFormat.Count - 1].Type != FormatInfoType.UserText)
                        newFormat.Add(new FormatInfo(FormatInfoType.UserText, format[i].ToString()));
                    else
                        newFormat[newFormat.Count - 1].Text = newFormat[newFormat.Count - 1].Text + format[i];
                }
            }

            return newFormat;
        }

        // weekday: Monday is 0, Sunday is 6
        internal static int Weekday(DateTime dt) {
            if (dt.DayOfWeek == DayOfWeek.Sunday) return 6;
            else return (int)dt.DayOfWeek - 1;
        }

        // isoweekday: Monday is 1, Sunday is 7
        internal static int IsoWeekday(DateTime dt) {
            if (dt.DayOfWeek == DayOfWeek.Sunday) return 7;
            else return (int)dt.DayOfWeek;
        }

        internal static PythonTuple GetDateTimeTuple(DateTime dt) {
            return GetDateTimeTuple(dt, null);
        }
        internal static PythonTuple GetDateTimeTuple(DateTime dt, PythonDateTime.PythonTimeZoneInformation tz) {
            return GetDateTimeTuple(dt, tz, false);
        }
        internal static StructTime GetDateTimeTuple(DateTime dt, PythonDateTime.PythonTimeZoneInformation tz, bool utc) {
            int last;
            if (utc) {
                last = 0;
            } else {
                if (tz == null) {
                    last = -1;
                } else {
                    PythonDateTime.PythonTimeDelta delta = tz.DaylightSavingTime(dt);
                    PythonDateTime.ThrowIfInvalid(delta, "dst");
                    if (delta == null) {
                        last = -1;
                    } else {
                        last = delta.NonZero() ? 1 : 0;
                    }
                }
            }

            return new StructTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, Weekday(dt), dt.DayOfYear, last);
        }

        private static DateTime GetDateTimeFromTuple(PythonTuple t) {
            if (t == null) return DateTime.Now;

            int[] ints = ValidateDateTimeTuple(t);

            return new DateTime(ints[YearIndex], ints[MonthIndex], ints[DayIndex], ints[HourIndex], ints[MinuteIndex], ints[SecondIndex]);
        }

        private static int[] ValidateDateTimeTuple(PythonTuple t) {
            if (t.Count != MaxIndex) throw PythonOps.TypeError("expected tuple of length {0}", MaxIndex);

            int[] ints = new int[MaxIndex];
            for (int i = 0; i < MaxIndex; i++) {
                ints[i] = Converter.ConvertToInt32(t[i]);
            }

            if (ints[YearIndex] < DateTime.MinValue.Year || ints[YearIndex] <= minYear) throw PythonOps.ValueError("year is too low");
            if (ints[YearIndex] > DateTime.MaxValue.Year) throw PythonOps.ValueError("year is too high");
            if (ints[WeekdayIndex] < 0 || ints[WeekdayIndex] >= 7) throw PythonOps.ValueError("day of week is outside of 0-6 range");
            return ints;
        }

        private static int FindFormat(List<FormatInfo> formatInfo, string format) {
            for (int i = 0; i < formatInfo.Count; i++) {
                if (formatInfo[i].Text == format) return i;
            }
            return -1;
        }

#if !SILVERLIGHT    // Stopwatch
        private static void InitStopWatch() {
            if (sw == null) {
                sw = new Stopwatch();
                sw.Start();
            }
        }
#endif

        [PythonType("struct_time")]
        public class StructTime : PythonTuple {
            private static DynamicType _StructTimeType = DynamicHelpers.GetDynamicTypeFromType(typeof(StructTime));

            public object Year {
                [PythonName("tm_year")]
                get { return data[0]; }
            }
            public object Month {
                [PythonName("tm_mon")]
                get { return data[1]; }
            }
            public object Day {
                [PythonName("tm_mday")]
                get { return data[2]; }
            }
            public object Hour {
                [PythonName("tm_hour")]
                get { return data[3]; }
            }
            public object Minute {
                [PythonName("tm_min")]
                get { return data[4]; }
            }
            public object Second {
                [PythonName("tm_sec")]
                get { return data[5]; }
            }
            public object DayOfWeek {
                [PythonName("tm_wday")]
                get { return data[6]; }
            }
            public object DayOfYear {
                [PythonName("tm_yday")]
                get { return data[7]; }
            }
            public object IsDaylightSavingTime {
                [PythonName("tm_isdst")]
                get { return data[8]; }
            }

            internal StructTime(int year, int month, int day, int hour, int minute, int second, int dayOfWeek, int dayOfYear, int isDst)
                : base(new object[] { year, month, day, hour, minute, second, dayOfWeek, dayOfYear, isDst }) {
            }

            [PythonName("__new__")]
            public static StructTime Make(CodeContext context, DynamicType cls, int year, int month, int day, int hour, int minute, int second, int dayOfWeek, int dayOfYear, int isDst) {
                if (cls == _StructTimeType) {
                    return new StructTime(year, month, day, hour, minute, second, dayOfWeek, dayOfYear, isDst);
                } else {
                    StructTime st = cls.CreateInstance(context, year, month, day, hour, minute, second, dayOfWeek, dayOfYear, isDst) as StructTime;
                    if (st == null)
                        throw PythonOps.TypeError("{0} is not a subclass of time.struct_time", cls);
                    return st;
                }
            }

            [PythonName("__reduce__")]
            public PythonTuple Reduce() {
                return PythonTuple.MakeTuple(_StructTimeType, PythonTuple.MakeTuple(Year, Month, Day, Hour, Minute, Second, DayOfWeek, DayOfYear, IsDaylightSavingTime));
            }

            [PythonName("__getnewargs__")]
            public static object GetNewArgs(CodeContext context, int year, int month, int day, int hour, int minute, int second, int dayOfWeek, int dayOfYear, int isDst) {
                return PythonTuple.MakeTuple(StructTime.Make(context, _StructTimeType, year, month, day, hour, minute, second, dayOfWeek, dayOfYear, isDst));
            }

            public override DynamicType DynamicType {
                get {
                    return _StructTimeType;
                }
            }
        }
    }
}
