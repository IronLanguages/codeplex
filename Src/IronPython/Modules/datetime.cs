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
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

[assembly: PythonModule("datetime", typeof(IronPython.Modules.PythonDateTime))]
namespace IronPython.Modules {
    [PythonType("datetime")]
    public class PythonDateTime {
        public static object MAXYEAR = DateTime.MaxValue.Year;
        public static object MINYEAR = DateTime.MinValue.Year;

        internal static void ThrowIfInvalid(PythonTimeDelta delta, string funcname) {
            if (delta != null) {
                if (delta.m_timeSpan.Seconds != 0 || delta.m_timeSpan.Milliseconds != 0 || delta.m_lostMicroseconds != 0) {
                    throw Ops.ValueError("tzinfo.{0}() must return a whole number of minutes", funcname);
                }
                double tm = delta.m_timeSpan.TotalMinutes;
                if (Math.Abs(tm) >= 24 * 60) {
                    throw Ops.ValueError("tzinfo.{0}() returned {1}; must be in -1439 .. 1439", funcname, Math.Floor(tm));
                }
            }
        }
        internal enum InputKind { Year, Month, Day, Hour, Minute, Second, Microsecond }
        internal static void ValidateInput(InputKind kind, int value) {
            switch (kind) {
                case InputKind.Year:
                    if (value > DateTime.MaxValue.Year || value < DateTime.MinValue.Year) {
                        throw Ops.ValueError("year is out of range");
                    }
                    break;
                case InputKind.Month:
                    if (value > 12 || value < 1) {
                        throw Ops.ValueError("month must be in 1..12");
                    }
                    break;
                case InputKind.Day:
                    // TODO: changing upper bound
                    if (value > 31 || value < 1) {
                        throw Ops.ValueError("day is out of range for month");
                    }
                    break;
                case InputKind.Hour:
                    if (value > 23 || value < 0) {
                        throw Ops.ValueError("hour must be in 0..23");
                    }
                    break;
                case InputKind.Minute:
                    if (value > 59 || value < 0) {
                        throw Ops.ValueError("minute must be in 0..59");
                    }
                    break;
                case InputKind.Second:
                    if (value > 59 || value < 0) {
                        throw Ops.ValueError("second must be in 0..59");
                    }
                    break;
                case InputKind.Microsecond:
                    if (value > 999999 || value < 0) {
                        throw Ops.ValueError("microsecond must be in 0..999999");
                    }
                    break;
            }
        }

        private static DateTime FirstDayOfIsoYear(int year) {
            DateTime firstDay = new DateTime(year, 1, 1);
            DateTime firstIsoDay = firstDay;

            switch (firstDay.DayOfWeek) {
                case DayOfWeek.Sunday:
                    firstIsoDay = firstDay.AddDays(1);
                    break;
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                    firstIsoDay = firstDay.AddDays(-1 * ((int)firstDay.DayOfWeek - 1));
                    break;
                case DayOfWeek.Friday:
                    firstIsoDay = firstDay.AddDays(3);
                    break;
                case DayOfWeek.Saturday:
                    firstIsoDay = firstDay.AddDays(2);
                    break;
            }
            return firstIsoDay;
        }
        internal static Tuple GetIsoCalendarTuple(DateTime dt) {
            DateTime firstDayOfLastIsoYear = FirstDayOfIsoYear(dt.Year - 1);
            DateTime firstDayOfThisIsoYear = FirstDayOfIsoYear(dt.Year);
            DateTime firstDayOfNextIsoYear = FirstDayOfIsoYear(dt.Year + 1);

            int year, days;
            if (firstDayOfThisIsoYear <= dt && dt < firstDayOfNextIsoYear) {
                year = dt.Year;
                days = (dt - firstDayOfThisIsoYear).Days;
            } else if (dt < firstDayOfThisIsoYear) {
                year = dt.Year - 1;
                days = (dt - firstDayOfLastIsoYear).Days;
            } else {
                year = dt.Year + 1;
                days = (dt - firstDayOfNextIsoYear).Days;
            }

            return Tuple.MakeTuple(year, days / 7 + 1, days % 7 + 1);
        }

        [PythonType("date")]
        public class PythonDate : ICodeFormattable, IRichEquality, IRichComparable {
            internal DateTime m_value;

            public static PythonDate min = new PythonDate(new DateTime(1, 1, 1));
            public static PythonDate max = new PythonDate(new DateTime(9999, 12, 31));

            public PythonDate(int year, int month, int day) {
                m_value = new DateTime(year, month, day);
            }

            internal PythonDate(DateTime value) {
                m_value = value;
            }

            [PythonName("weekday")]
            public int Weekday() {
                return PythonTime.Weekday(m_value);
            }

            [PythonName("isoweekday")]
            public int IsoWeekday() {
                return PythonTime.IsoWeekday(m_value);
            }

            [PythonName("isocalendar")]
            public Tuple IsoCalendar() {
                return GetIsoCalendarTuple(m_value);
            }

            [PythonName("timetuple")]
            public Tuple GetTimeTuple() {
                return PythonTime.GetDateTimeTuple(m_value, null);
            }

            [PythonName("toordinal")]
            public int ToOrdinal() {
                return (m_value - min.m_value).Days + 1;
            }

            [PythonName("fromordinal")]
            public static PythonDate FromOrdinal(int d) {
                return new PythonDate(min.m_value.AddDays(d - 1));
            }

            [PythonName("fromtimestamp")]
            public static PythonDate FromTimestamp(double timestamp) {
                DateTime dt = new DateTime((long)(timestamp * 1e7));
                return new PythonDate(dt.Year, dt.Month, dt.Day);
            }

            [PythonName("today")]
            public static object Today() {
                return new PythonDate(DateTime.Today);
            }

            public static object Resolution {
                [PythonName("resolution")]
                get { return PythonTimeDelta.s_dayResolution; }
            }

            public int Year {
                [PythonName("year")]
                get { return m_value.Year; }
            }

            public int Month {
                [PythonName("month")]
                get { return m_value.Month; }
            }

            public int Day {
                [PythonName("day")]
                get { return m_value.Day; }
            }

            [PythonName("replace")]
            public PythonDate Replace([DefaultParameterValue(null)] object year,
                [DefaultParameterValue(null)]object month,
                [DefaultParameterValue(null)]object day) {
                int iYear, iMonth, iDay;
                if (year == null) {
                    iYear = m_value.Year;
                } else {
                    iYear = Converter.ConvertToInt32(year);
                }
                if (month == null) {
                    iMonth = m_value.Month;
                } else {
                    iMonth = Converter.ConvertToInt32(month);
                }
                if (day == null) {
                    iDay = m_value.Day;
                } else {
                    iDay = Converter.ConvertToInt32(day);
                }

                return new PythonDate(iYear, iMonth, iDay);
            }


            [PythonName("strftime")]
            public string Format(string dateFormat) {
                return PythonTime.FormatTime(dateFormat, m_value);
            }

            [PythonName("isoformat")]
            public string IsoFormat() {
                return m_value.ToString("yyyy-MM-dd");
            }

            [PythonName("ctime")]
            public string ToCTime() {
                return m_value.ToString("ddd MMM ") + string.Format("{0,2}", m_value.Day) + m_value.ToString(" 00:00:00 yyyy");
            }

            [PythonName("__str__")]
            public override string ToString() {
                return IsoFormat();
            }

            public override bool Equals(object obj) {
                PythonDate other = obj as PythonDate;
                if (other == null) return false;
                return this.m_value == other.m_value;
            }

            public override int GetHashCode() {
                return m_value.GetHashCode();
            }

            [PythonName("__add__")]
            public PythonDate Add(PythonTimeDelta delta) {
                try {
                    return new PythonDate(m_value.AddDays(delta.Days));
                } catch {
                    throw Ops.OverflowError("date value out of range");
                }
            }
            [PythonName("__radd__")]
            public PythonDate ReverseAdd(PythonTimeDelta delta) { return this.Add(delta); }

            [PythonName("__sub__")]
            public PythonDate Subtract(PythonTimeDelta delta) {
                try {
                    return new PythonDate(m_value.AddDays(-1 * delta.Days));
                } catch {
                    throw Ops.OverflowError("date value out of range");
                }

            }
            [PythonName("__sub__")]
            public PythonTimeDelta Subtract(PythonDate other) {
                return PythonTimeDelta.Make(this.m_value - other.m_value, 0);
            }

            #region ICodeFormattable Members

            public string ToCodeString() {
                return string.Format("datetime.date({0}, {1}, {2})", m_value.Year, m_value.Month, m_value.Day);
            }

            #endregion

            #region IRichComparable Members

            [PythonName("__cmp__")]
            public object CompareTo(object other) {
                PythonDate time = other as PythonDate;
                if (time == null) return Ops.NotImplemented;

                return this.m_value.CompareTo(time.m_value);
            }

            [PythonName("__gt__")]
            public object GreaterThan(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res > 0;
            }

            [PythonName("__lt__")]
            public object LessThan(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res < 0;
            }

            [PythonName("__ge__")]
            public object GreaterThanOrEqual(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res >= 0;
            }

            [PythonName("__le__")]
            public object LessThanOrEqual(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res <= 0;
            }

            #endregion

            #region IRichEquality Members

            [PythonName("__hash__")]
            public object RichGetHashCode() {
                return GetHashCode();
            }

            [PythonName("__eq__")]
            public object RichEquals(object other) {
                return Ops.Bool2Object(Equals(other));
            }

            [PythonName("__ne__")]
            public object RichNotEquals(object other) {
                return Ops.Bool2Object(!Equals(other));
            }
            #endregion
        }

        [PythonType("time")]
        public class PythonDateTimeTime : ICodeFormattable, IRichComparable {
            public static object max = new PythonDateTimeTime(23, 59, 59, 999999, null);
            public static object min = new PythonDateTimeTime(0, 0, 0, 0, null);
            public static object resolution = PythonTimeDelta.resolution;

            internal TimeSpan m_timeSpan;
            internal int m_lostMicroseconds;
            internal PythonTimeZoneInformation m_tz;

            public PythonDateTimeTime([DefaultParameterValue(0)]int hour,
                [DefaultParameterValue(0)]int minute,
                [DefaultParameterValue(0)]int second,
                [DefaultParameterValue(0)]int microsecond,
                [DefaultParameterValue(null)]PythonTimeZoneInformation tzinfo) {

                PythonDateTime.ValidateInput(InputKind.Hour, hour);
                PythonDateTime.ValidateInput(InputKind.Minute, minute);
                PythonDateTime.ValidateInput(InputKind.Second, second);
                PythonDateTime.ValidateInput(InputKind.Microsecond, microsecond);

                // all inputs are positive
                this.m_timeSpan = new TimeSpan(0, hour, minute, second, microsecond / 1000);
                this.m_lostMicroseconds = microsecond % 1000;
                this.m_tz = tzinfo;
            }

            internal PythonDateTimeTime(TimeSpan timeSpan, int lostMicroseconds, PythonTimeZoneInformation tzinfo) {
                this.m_timeSpan = timeSpan;
                this.m_lostMicroseconds = lostMicroseconds;
                this.m_tz = tzinfo;
            }

            [PythonName("replace")]
            public object Replace() {
                return this;
            }

            [PythonName("replace")]
            public object Replace([ParamDict]Dict dict) {
                int hour = this.Hour;
                int minute = this.Minute;
                int second = this.Second;
                int microsecond = this.Microsecond;
                PythonTimeZoneInformation tz = this.TimeZoneInfo;

                foreach (KeyValuePair<object, object> kvp in (IDictionary<object, object>)dict) {
                    string key = kvp.Key as string;
                    if (key == null) continue;

                    switch (key) {
                        case "hour":
                            hour = (int)kvp.Value;
                            break;
                        case "minute":
                            minute = (int)kvp.Value;
                            break;
                        case "second":
                            second = (int)kvp.Value;
                            break;
                        case "microsecond":
                            microsecond = (int)kvp.Value;
                            break;
                        case "tzinfo":
                            tz = kvp.Value as PythonTimeZoneInformation;
                            break;
                    }
                }
                return new PythonDateTimeTime(hour, minute, second, microsecond, tz);
            }

            [PythonName("strftime")]
            public object FormatTime(string format) {
                return PythonTime.FormatTime(format,
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, m_timeSpan.Hours, m_timeSpan.Minutes, m_timeSpan.Seconds, m_timeSpan.Milliseconds));
            }

            public int Hour {
                [PythonName("hour")]
                get { return m_timeSpan.Hours; }
            }

            public int Minute {
                [PythonName("minute")]
                get { return m_timeSpan.Minutes; }
            }

            public int Second {
                [PythonName("second")]
                get { return m_timeSpan.Seconds; }
            }

            public int Microsecond {
                [PythonName("microsecond")]
                get { return m_timeSpan.Milliseconds * 1000 + m_lostMicroseconds; }
            }

            public PythonTimeZoneInformation TimeZoneInfo {
                [PythonName("tzinfo")]
                get { return m_tz; }
            }


            [PythonName("dst")]
            public object DaylightSavingsTime() {
                if (m_tz == null) return null;
                PythonTimeDelta delta = m_tz.DaylightSavingsTime(null);
                PythonDateTime.ThrowIfInvalid(delta, "dst");
                return delta;
            }

            [PythonName("utcoffset")]
            public PythonTimeDelta GetUtcOffset() {
                if (m_tz == null) return null;
                PythonTimeDelta delta = m_tz.UtcOffset(null);
                PythonDateTime.ThrowIfInvalid(delta, "utcoffset");
                return delta;
            }

            [PythonName("tzname")]
            public object TimeZoneName() {
                if (m_tz == null) return null;
                return m_tz.TimeZoneName(null);
            }

            class UnifiedTime {
                public TimeSpan TimeSpan;
                public int LostMicroseconds;

                public override bool Equals(object obj) {
                    UnifiedTime other = obj as UnifiedTime;
                    if (other == null) return false;
                    return this.TimeSpan == other.TimeSpan && this.LostMicroseconds == other.LostMicroseconds;
                }

                public override int GetHashCode() {
                    return TimeSpan.GetHashCode() ^ LostMicroseconds;
                }

                public int CompareTo(UnifiedTime other) {
                    int res = this.TimeSpan.CompareTo(other.TimeSpan);
                    if (res != 0) return res;
                    return this.LostMicroseconds - other.LostMicroseconds;
                }
            }

            UnifiedTime m_utcTime;
            UnifiedTime UtcTime {
                get {
                    if (m_utcTime == null) {
                        m_utcTime = new UnifiedTime();

                        m_utcTime.TimeSpan = m_timeSpan;
                        m_utcTime.LostMicroseconds = m_lostMicroseconds;

                        PythonTimeDelta delta = this.GetUtcOffset();
                        if (delta != null) {
                            PythonDateTimeTime utced = this - delta;
                            m_utcTime.TimeSpan = utced.m_timeSpan;
                            m_utcTime.LostMicroseconds = utced.m_lostMicroseconds;
                            // TODO
                        }
                    }
                    return m_utcTime;
                }
            }

            [PythonName("__nonzero__")]
            public bool NonZero() {
                return this.UtcTime.TimeSpan.Ticks != 0 || this.UtcTime.LostMicroseconds != 0;
            }

            [PythonName("isoformat")]
            public object GetIsoFormat() {
                return ToString();
            }

            public override int GetHashCode() {
                return this.UtcTime.GetHashCode();
            }

            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0:d2}:{1:d2}:{2:d2}", Hour, Minute, Second);

                if (Microsecond != 0) sb.AppendFormat(".{0:d6}", Microsecond);

                PythonTimeDelta delta = GetUtcOffset();
                if (delta != null) {
                    if (delta.m_timeSpan >= TimeSpan.Zero) {
                        sb.AppendFormat("+{0:d2}:{1:d2}", delta.m_timeSpan.Hours, delta.m_timeSpan.Minutes);
                    } else {
                        sb.AppendFormat("-{0:d2}:{1:d2}", -delta.m_timeSpan.Hours, -delta.m_timeSpan.Minutes);
                    }
                }

                return sb.ToString();
            }

            public static PythonDateTimeTime operator +(PythonDateTimeTime date, PythonTimeDelta delta) {
                return new PythonDateTimeTime(date.m_timeSpan.Add(delta.m_timeSpan), delta.m_lostMicroseconds + date.m_lostMicroseconds, date.m_tz);
            }

            public static PythonDateTimeTime operator -(PythonDateTimeTime date, PythonTimeDelta delta) {
                return new PythonDateTimeTime(date.m_timeSpan.Subtract(delta.m_timeSpan), delta.m_lostMicroseconds - date.m_lostMicroseconds, date.m_tz);
            }

            internal static bool CheckTzInfoBeforeCompare(PythonDateTimeTime self, PythonDateTimeTime other) {
                if (self.m_tz != other.m_tz) {
                    PythonTimeDelta offset1 = self.GetUtcOffset();
                    PythonTimeDelta offset2 = other.GetUtcOffset();

                    if ((offset1 == null && offset2 != null) || (offset1 != null && offset2 == null))
                        throw Ops.TypeError("can't compare offset-naive and offset-aware times");

                    return false;
                } else {
                    return true; // has the same TzInfo, Utcoffset will be skipped
                }
            }

            public override bool Equals(object obj) {
                PythonDateTimeTime other = obj as PythonDateTimeTime;
                if (other == null) return false;

                if (CheckTzInfoBeforeCompare(this, other)) {
                    return this.m_timeSpan == other.m_timeSpan && this.m_lostMicroseconds == other.m_lostMicroseconds;
                } else {
                    return this.UtcTime.Equals(other.UtcTime);
                }
            }

            #region IRichComparable Members

            [PythonName("__cmp__")]
            public object CompareTo(object other) {
                PythonDateTimeTime other2 = other as PythonDateTimeTime;
                if (other2 == null) return Ops.NotImplemented;

                if (CheckTzInfoBeforeCompare(this, other2)) {
                    int res = this.m_timeSpan.CompareTo(other2.m_timeSpan);
                    if (res != 0) return res;
                    return this.m_lostMicroseconds - other2.m_lostMicroseconds;
                } else {
                    return this.UtcTime.CompareTo(other2.UtcTime);
                }
            }

            [PythonName("__gt__")]
            public object GreaterThan(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res > 0;
            }

            [PythonName("__lt__")]
            public object LessThan(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res < 0;
            }

            [PythonName("__ge__")]
            public object GreaterThanOrEqual(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res >= 0;
            }

            [PythonName("__le__")]
            public object LessThanOrEqual(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res <= 0;
            }

            #endregion

            #region IRichEquality Members

            [PythonName("__hash__")]
            public object RichGetHashCode() {
                return GetHashCode();
            }

            [PythonName("__eq__")]
            public object RichEquals(object other) {
                if (other is PythonDateTimeTime)
                    return Ops.Bool2Object(Equals(other));

                return Ops.NotImplemented;
            }

            [PythonName("__ne__")]
            public object RichNotEquals(object other) {
                if (other is PythonDateTimeTime)
                    return Ops.Bool2Object(!Equals(other));

                return Ops.NotImplemented;
            }

            #endregion

            #region ICodeFormattable Members

            public string ToCodeString() {
                StringBuilder sb = new StringBuilder();
                if (Microsecond != 0)
                    sb.AppendFormat("datetime.time({0}, {1}, {2}, {3}", Hour, Minute, Second, Microsecond);
                else if (Second != 0)
                    sb.AppendFormat("datetime.time({0}, {1}, {2}", Hour, Minute, Second);
                else
                    sb.AppendFormat("datetime.time({0}, {1}", Hour, Minute);

                string tzname = TimeZoneName() as string;
                if (tzname != null) {
                    // TODO: calling __repr__?
                    sb.AppendFormat(", tzinfo={0}", tzname.ToLower());
                }

                sb.AppendFormat(")");

                return sb.ToString();
            }

            #endregion
        }

        [PythonType("datetime")]
        public class PythonDateTimeCombo : IRichComparable, ICodeFormattable {
            internal DateTime m_dateTime;
            internal int m_lostMicroseconds;
            internal PythonTimeZoneInformation m_tz;

            public static object max = new PythonDateTimeCombo(DateTime.MaxValue, 999, null);
            public static object min = new PythonDateTimeCombo(DateTime.MinValue, 0, null);
            public static object resolution = PythonTimeDelta.resolution;

            public PythonDateTimeCombo(int year,
                int month,
                int day,
               [DefaultParameterValue(0)]int hour,
               [DefaultParameterValue(0)]int minute,
               [DefaultParameterValue(0)]int second,
               [DefaultParameterValue(0)]int microsecond,
               [DefaultParameterValue(null)]PythonTimeZoneInformation tzinfo) {

                PythonDateTime.ValidateInput(InputKind.Year, year);
                PythonDateTime.ValidateInput(InputKind.Month, month);
                PythonDateTime.ValidateInput(InputKind.Day, day);
                PythonDateTime.ValidateInput(InputKind.Hour, hour);
                PythonDateTime.ValidateInput(InputKind.Minute, minute);
                PythonDateTime.ValidateInput(InputKind.Second, second);
                PythonDateTime.ValidateInput(InputKind.Microsecond, microsecond);

                m_dateTime = new DateTime(year, month, day, hour, minute, second, microsecond / 1000);
                m_lostMicroseconds = microsecond % 1000;
                m_tz = tzinfo;
            }

            internal PythonDateTimeCombo(DateTime dt, int lostMicroseconds, PythonTimeZoneInformation tzinfo) {
                this.m_dateTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                this.m_lostMicroseconds = dt.Millisecond * 1000 + lostMicroseconds;
                this.m_tz = tzinfo;

                Adjust();
            }

            internal void Adjust() {
                // make sure both are positive, and lostMicroseconds < 1000
                if (m_lostMicroseconds < 0) {
                    try {
                        m_dateTime = m_dateTime.AddMilliseconds(-1);
                    } catch {
                        throw Ops.OverflowError("date value out of range");
                    }
                    m_lostMicroseconds += 1000;
                }

                if (m_lostMicroseconds > 999) {
                    try {
                        m_dateTime = m_dateTime.AddMilliseconds(m_lostMicroseconds / 1000);
                    } catch {
                        throw Ops.OverflowError("date value out of range");
                    }
                    m_lostMicroseconds = m_lostMicroseconds % 1000;
                }
            }

            [PythonName("now")]
            public static object Now([DefaultParameterValue(null)]PythonTimeZoneInformation tz) {
                if (tz != null) {
                    DateTime dt = DateTime.UtcNow;
                    PythonDateTimeCombo pdtc = new PythonDateTimeCombo(dt, 0, tz);
                    return pdtc + tz.UtcOffset(dt);
                } else {
                    return new PythonDateTimeCombo(DateTime.Now, 0, tz);
                }
            }

            [PythonName("today")]
            public static object Today() {
                return new PythonDateTimeCombo(DateTime.Now, 0, null);
            }

            [PythonName("strftime")]
            public object FormatTime(string format) {
                return PythonTime.FormatTime(format, m_dateTime);
            }

            [PythonName("utcnow")]
            public static object UtcNow() {
                return new PythonDateTimeCombo(DateTime.UtcNow, 0, null);
            }

            [PythonName("astimezone")]
            public object AsTimeZone(PythonTimeZoneInformation tz) {
                if (tz == null)
                    throw Ops.TypeError("astimezone() argument 1 must be datetime.tzinfo, not None");

                if (tz == m_tz) return this;

                PythonTimeDelta newDelta = tz.UtcOffset(m_dateTime);
                if (newDelta == null)
                    throw Ops.ValueError("astimezone() cannot be applied to a naive datetime");

                PythonTimeDelta curDelta = m_tz.UtcOffset(m_dateTime);

                PythonDateTimeCombo pdtc = this + newDelta - curDelta;
                pdtc.m_tz = tz;
                return pdtc;
            }

            [PythonName("ctime")]
            [Documentation("converts the time into a string DOW Mon ## hh:mm:ss year")]
            public object ToCTime() {
                return m_dateTime.ToString("ddd MMM ") + string.Format("{0,2}", m_dateTime.Day) + m_dateTime.ToString(" HH:mm:ss yyyy");
            }

            [PythonName("date")]
            public PythonDate ToDate() {
                return new PythonDate(m_dateTime.Year, m_dateTime.Month, m_dateTime.Day);
            }

            [PythonName("replace")]
            public object Replace() {
                return this;
            }

            [PythonName("replace")]
            [Documentation("gets a new datetime object with the fields provided as keyword arguments replaced.")]
            public object Replace([ParamDict]Dict dict) {
                int year = Year;
                int month = Month;
                int day = Day;
                int hour = Hour;
                int minute = Minute;
                int second = Second;
                int microsecond = Microsecond;
                PythonTimeZoneInformation tz = m_tz;

                foreach (KeyValuePair<object, object> kvp in (IDictionary<object, object>)dict) {
                    string key = kvp.Key as string;
                    if (key == null) continue;

                    switch (key) {
                        case "year":
                            year = (int)kvp.Value;
                            break;
                        case "month":
                            month = (int)kvp.Value;
                            break;
                        case "day":
                            day = (int)kvp.Value;
                            break;
                        case "hour":
                            hour = (int)kvp.Value;
                            break;
                        case "minute":
                            minute = (int)kvp.Value;
                            break;
                        case "second":
                            second = (int)kvp.Value;
                            break;
                        case "microsecond":
                            microsecond = (int)kvp.Value;
                            break;
                        case "tzinfo":
                            tz = kvp.Value as PythonTimeZoneInformation;
                            break;
                    }
                }
                return new PythonDateTimeCombo(year, month, day, hour, minute, second, microsecond, tz);
            }

            [PythonName("toordinal")]
            public int ToOrdinal() {
                TimeSpan t = m_dateTime - DateTime.MinValue;
                return t.Days + 1;
            }
            [PythonName("fromordinal")]
            public static PythonDateTimeCombo FromOrdinal(int d) {
                return new PythonDateTimeCombo(DateTime.MinValue + new TimeSpan(d - 1, 0, 0, 0), 0, null);
            }
            [PythonName("combine")]
            public static object Combine(PythonDate date, PythonDateTimeTime time) {
                return new PythonDateTimeCombo(date.Year,
                    date.Month,
                    date.Day,
                    time.Hour,
                    time.Minute,
                    time.Second,
                    time.Microsecond,
                    time.TimeZoneInfo);
            }

            [PythonName("time")]
            [Documentation("gets the datetime w/o the time zone component")]
            public PythonDateTimeTime GetTime() {
                return new PythonDateTimeTime(m_dateTime.Hour, m_dateTime.Minute, m_dateTime.Second, m_dateTime.Millisecond * 1000 + m_lostMicroseconds, null);
            }

            [PythonName("timetuple")]
            public object GetTimeTuple() {
                return PythonTime.GetDateTimeTuple(m_dateTime, m_tz);
            }

            [PythonName("timetz")]
            public object GetTimeWithTimeZone() {
                //Return time object with same time and tzinfo.
                return new PythonDateTimeTime(m_dateTime.Hour, m_dateTime.Minute, m_dateTime.Second, m_dateTime.Millisecond * 1000 + m_lostMicroseconds, m_tz);
            }

            [PythonName("tzname")]
            public object GetTimeZoneName() {
                if (m_tz == null) return null;
                return m_tz.TimeZoneName(this);
            }

            public object TimeZoneInformation {
                [PythonName("tzinfo")]
                get { return m_tz; }
            }

            [PythonName("dst")]
            public PythonTimeDelta dst() {
                if (m_tz == null) return null;
                PythonTimeDelta delta = m_tz.DaylightSavingsTime(this);
                PythonDateTime.ThrowIfInvalid(delta, "dst");
                return delta;
            }

            [PythonName("utcoffset")]
            public PythonTimeDelta GetUtcOffset() {
                if (m_tz == null) return null;
                PythonTimeDelta delta = m_tz.UtcOffset(this);
                PythonDateTime.ThrowIfInvalid(delta, "utcoffset");
                return delta;
            }

            [PythonName("utctimetuple")]
            public object GetUtcTimeTuple() {
                if (m_tz == null) return GetTimeTuple();
                return new PythonDateTimeCombo(m_tz.ToUniversalTime(m_dateTime), m_lostMicroseconds, m_tz).GetTimeTuple();
            }

            [PythonName("fromtimestamp")]
            public static PythonDateTimeCombo FromTimeStamp(double timestamp, [DefaultParameterValue(null)] PythonTimeZoneInformation tz) {
                DateTime dt = new DateTime((long)(timestamp * 1e7));

                if (tz != null) {
                    dt = TimeZone.CurrentTimeZone.ToUniversalTime(dt);
                    PythonDateTimeCombo pdtc = new PythonDateTimeCombo(dt, 0, tz);
                    pdtc = pdtc + tz.UtcOffset(dt);
                    return pdtc;
                } else {
                    return new PythonDateTimeCombo(dt, 0, tz);
                }
            }

            [PythonName("utcfromtimestamp")]
            public static PythonDateTimeCombo UtcFromTimestamp(double timestamp) {
                DateTime dt = new DateTime((long)(timestamp * 1e7));
                dt = TimeZone.CurrentTimeZone.ToUniversalTime(dt);
                return new PythonDateTimeCombo(dt, 0, null);
            }

            [PythonName("weekday")]
            public object WeekDay() {
                return PythonTime.Weekday(m_dateTime);
            }

            [PythonName("isoweekday")]
            public object IsoWeekDay() {
                return PythonTime.IsoWeekday(m_dateTime);
            }

            [PythonName("isocalendar")]
            public Tuple IsoCalendar() {
                return GetIsoCalendarTuple(m_dateTime);
            }

            public int Year {
                [PythonName("year")]
                get { return m_dateTime.Year; }
            }

            public int Month {
                [PythonName("month")]
                get { return m_dateTime.Month; }
            }

            public int Day {
                [PythonName("day")]
                get { return m_dateTime.Day; }
            }

            public int Hour {
                [PythonName("hour")]
                get { return m_dateTime.Hour; }
            }

            public int Minute {
                [PythonName("minute")]
                get { return m_dateTime.Minute; }
            }

            public int Second {
                [PythonName("second")]
                get { return m_dateTime.Second; }
            }

            public int Microsecond {
                [PythonName("microsecond")]
                get { return m_dateTime.Millisecond * 1000 + m_lostMicroseconds; }
            }

            public static PythonDateTimeCombo operator +(PythonDateTimeCombo date, PythonTimeDelta delta) {
                return new PythonDateTimeCombo(date.m_dateTime.Add(delta.m_timeSpan), delta.m_lostMicroseconds + date.m_lostMicroseconds, date.m_tz);
            }
            [PythonName("__radd__")]
            public PythonDateTimeCombo ReverseAdd(PythonTimeDelta delta) {
                return this + delta;
            }
            public static PythonDateTimeCombo operator -(PythonDateTimeCombo date, PythonTimeDelta delta) {
                return new PythonDateTimeCombo(date.m_dateTime.Subtract(delta.m_timeSpan), date.m_lostMicroseconds - delta.m_lostMicroseconds, date.m_tz);
            }

            public static PythonTimeDelta operator -(PythonDateTimeCombo date, PythonDateTimeCombo other) {
                if (CheckTzInfoBeforeCompare(date, other)) {
                    return PythonTimeDelta.Make(date.m_dateTime - other.m_dateTime, date.m_lostMicroseconds - other.m_lostMicroseconds);
                } else {
                    return PythonTimeDelta.Make(date.UtcDateTime.Period - other.UtcDateTime.Period, date.UtcDateTime.LostMicroseconds - other.UtcDateTime.LostMicroseconds);
                }
            }

            class UnifiedDateTime {
                public DateTime Period;
                public int LostMicroseconds;

                public override bool Equals(object obj) {
                    UnifiedDateTime other = obj as UnifiedDateTime;
                    if (other == null) return false;

                    return this.Period == other.Period && this.LostMicroseconds == other.LostMicroseconds;
                }

                public override int GetHashCode() {
                    return Period.GetHashCode() ^ LostMicroseconds;
                }

                public int CompareTo(UnifiedDateTime other) {
                    int res = this.Period.CompareTo(other.Period);

                    if (res != 0) return res;

                    return this.LostMicroseconds - other.LostMicroseconds;
                }
            }

            UnifiedDateTime m_utcDateTime;
            UnifiedDateTime UtcDateTime {
                get {
                    if (m_utcDateTime == null) {
                        m_utcDateTime = new UnifiedDateTime();

                        m_utcDateTime.Period = m_dateTime;
                        m_utcDateTime.LostMicroseconds = m_lostMicroseconds;

                        PythonTimeDelta delta = this.GetUtcOffset();
                        if (delta != null) {
                            PythonDateTimeCombo utced = this - delta;
                            m_utcDateTime.Period = utced.m_dateTime;
                            m_utcDateTime.LostMicroseconds = utced.m_lostMicroseconds;
                        }
                    }
                    return m_utcDateTime;
                }
            }

            internal static bool CheckTzInfoBeforeCompare(PythonDateTimeCombo self, PythonDateTimeCombo other) {
                if (self.m_tz != other.m_tz) {
                    PythonTimeDelta offset1 = self.GetUtcOffset();
                    PythonTimeDelta offset2 = other.GetUtcOffset();

                    if ((offset1 == null && offset2 != null) || (offset1 != null && offset2 == null))
                        throw Ops.TypeError("can't compare offset-naive and offset-aware times");

                    return false;
                } else {
                    return true; // has the same TzInfo, Utcoffset will be skipped
                }
            }

            public override bool Equals(object obj) {
                PythonDateTimeCombo other = obj as PythonDateTimeCombo;
                if (other == null) return false;

                if (CheckTzInfoBeforeCompare(this, other)) {
                    return this.m_dateTime.Equals(other.m_dateTime) && this.m_lostMicroseconds == other.m_lostMicroseconds;
                } else {
                    // hack
                    TimeSpan delta = this.m_dateTime - other.m_dateTime;
                    if (Math.Abs(delta.TotalHours) > 24 * 2) {
                        return false;
                    } else {
                        return this.UtcDateTime.Equals(other.UtcDateTime);
                    }
                }
            }

            public override int GetHashCode() {
                return this.UtcDateTime.Period.GetHashCode() ^ this.UtcDateTime.LostMicroseconds;
            }

            [PythonName("isoformat")]
            public string IsoFormat([DefaultParameterValue('T')]char sep) {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0:d4}-{1:d2}-{2:d2}{3}{4:d2}:{5:d2}:{6:d2}", Year, Month, Day, sep, Hour, Minute, Second);

                if (Microsecond != 0) sb.AppendFormat(".{0:d6}", Microsecond);

                PythonTimeDelta delta = GetUtcOffset();
                if (delta != null) {
                    if (delta.m_timeSpan >= TimeSpan.Zero) {
                        sb.AppendFormat("+{0:d2}:{1:d2}", delta.m_timeSpan.Hours, delta.m_timeSpan.Minutes);
                    } else {
                        sb.AppendFormat("-{0:d2}:{1:d2}", -delta.m_timeSpan.Hours, -delta.m_timeSpan.Minutes);
                    }
                }

                return sb.ToString();
            }


            public override string ToString() {
                return IsoFormat(' ');
            }

            #region IRichComparable Members

            [PythonName("__cmp__")]
            public object CompareTo(object other) {
                PythonDateTimeCombo combo = other as PythonDateTimeCombo;
                if (combo == null) return Ops.NotImplemented;

                if (CheckTzInfoBeforeCompare(this, combo)) {
                    int res = this.m_dateTime.CompareTo(combo.m_dateTime);

                    if (res != 0) return res;

                    return this.m_lostMicroseconds - combo.m_lostMicroseconds;
                } else {
                    TimeSpan delta = this.m_dateTime - combo.m_dateTime;
                    // hack
                    if (Math.Abs(delta.TotalHours) > 24 * 2) {
                        return delta > TimeSpan.Zero ? 1 : -1;
                    } else {
                        return this.UtcDateTime.CompareTo(combo.UtcDateTime);
                    }
                }
            }

            [PythonName("__gt__")]
            public object GreaterThan(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res > 0;
            }

            [PythonName("__lt__")]
            public object LessThan(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res < 0;
            }

            [PythonName("__ge__")]
            public object GreaterThanOrEqual(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res >= 0;
            }

            [PythonName("__le__")]
            public object LessThanOrEqual(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res <= 0;
            }

            #endregion

            #region IRichEquality Members

            [PythonName("__hash__")]
            public object RichGetHashCode() {
                return GetHashCode();
            }

            [PythonName("__eq__")]
            public object RichEquals(object other) {
                if (other is PythonDateTimeCombo)
                    return Ops.Bool2Object(Equals(other));

                return Ops.NotImplemented;
            }

            [PythonName("__ne__")]
            public object RichNotEquals(object other) {
                if (other is PythonDateTimeCombo)
                    return Ops.Bool2Object(!Equals(other));

                return Ops.NotImplemented;
            }

            #endregion

            #region ICodeFormattable Members

            public string ToCodeString() {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("datetime.datetime({0}, {1}, {2}, {3}, {4}",
                    m_dateTime.Year,
                    m_dateTime.Month,
                    m_dateTime.Day,
                    m_dateTime.Hour,
                    m_dateTime.Minute);

                if (Microsecond != 0) {
                    sb.AppendFormat(", {0}, {1}", Second, Microsecond);
                } else {
                    if (Second != 0) {
                        sb.AppendFormat(", {0}", Second);
                    }
                }

                if (m_tz != null) {
                    sb.AppendFormat(", tzinfo={0}", m_tz.TimeZoneName(this).ToLower());
                }
                sb.AppendFormat(")");
                return sb.ToString();
            }

            #endregion
        }

        [PythonType("timedelta")]
        public class PythonTimeDelta : IRichComparable, ICodeFormattable {
            internal TimeSpan m_timeSpan;
            internal int m_lostMicroseconds;

            internal static PythonTimeDelta s_dayResolution = new PythonTimeDelta(1, 0, 0, 0, 0, 0, 0);
            public static PythonTimeDelta resolution = new PythonTimeDelta(0, 0, 1, 0, 0, 0, 0);
            public static PythonTimeDelta min = PythonTimeDelta.Make(TimeSpan.MinValue, -999);
            public static PythonTimeDelta max = PythonTimeDelta.Make(TimeSpan.MaxValue, 999);

            private const double SECONDSPERDAY = 24 * 60 * 60;

            public PythonTimeDelta([DefaultParameterValue(0D)] double days,
                [DefaultParameterValue(0D)]double seconds,
                [DefaultParameterValue(0D)]double microseconds,
                [DefaultParameterValue(0D)]double milliseconds,
                [DefaultParameterValue(0D)]double minutes,
                [DefaultParameterValue(0D)]double hours,
                [DefaultParameterValue(0D)]double weeks
                ) {
                double totalDays = weeks * 7 + days;
                double temp = ((totalDays * 24 + hours) * 60 + minutes) * 60 + seconds; // could have floating points
                double totalSeconds = Math.Floor(temp); // no .xxxx
                double totalMicroseconds = Math.Round(milliseconds * 1000 + microseconds + (temp - totalSeconds) * 1000000);

                // ensure both have the same sign, otherwise, the comparison could be wrong
                if (totalSeconds > 0 && totalMicroseconds < 0) {
                    totalSeconds -= 1;
                    totalMicroseconds += 1e6;
                } else if (temp < 0 && totalMicroseconds > 0) {
                    totalSeconds += 1;
                    totalMicroseconds -= 1e6;
                }

                m_timeSpan = TimeSpan.FromSeconds(totalSeconds).Add(TimeSpan.FromMilliseconds((long)(totalMicroseconds / 1000)));
                m_lostMicroseconds = (int)(totalMicroseconds % 1000);
            }

            internal static PythonTimeDelta Make(TimeSpan ts, int extramicros) {
                return new PythonTimeDelta(ts.Days, ts.Seconds, extramicros, ts.Milliseconds, ts.Minutes, ts.Hours, 0);
            }

            public static PythonTimeDelta operator +(PythonTimeDelta self, PythonTimeDelta other) {
                return PythonTimeDelta.Make(self.m_timeSpan.Add(other.m_timeSpan), self.m_lostMicroseconds + other.m_lostMicroseconds);
            }
            public static PythonTimeDelta operator -(PythonTimeDelta self, PythonTimeDelta other) {
                return PythonTimeDelta.Make(self.m_timeSpan.Subtract(other.m_timeSpan), self.m_lostMicroseconds - other.m_lostMicroseconds);
            }
            public static PythonTimeDelta operator -(PythonTimeDelta self) {
                return PythonTimeDelta.Make(self.m_timeSpan.Negate(), -self.m_lostMicroseconds);
            }
            public static PythonTimeDelta operator +(PythonTimeDelta self) {
                return PythonTimeDelta.Make(self.m_timeSpan, self.m_lostMicroseconds);
            }
            public static PythonTimeDelta operator *(PythonTimeDelta self, int other) {
                return PythonTimeDelta.Make(new TimeSpan(self.m_timeSpan.Ticks * other), self.m_lostMicroseconds * other);
            }

            public static PythonTimeDelta operator /(PythonTimeDelta self, int other) {
                return PythonTimeDelta.Make(
                    TimeSpan.FromMilliseconds(self.m_timeSpan.TotalMilliseconds / other),
                    (int)((self.m_timeSpan.TotalMilliseconds % other * 1000 + self.m_lostMicroseconds) / other));
            }

            [PythonName("__pos__")]
            public PythonTimeDelta Positive() { return +this; }
            [PythonName("__neg__")]
            public PythonTimeDelta Negate() { return -this; }
            [PythonName("__abs__")]
            public PythonTimeDelta Abs() { return (this.m_timeSpan > TimeSpan.Zero) ? this : -this; }
            [PythonName("__mul__")]
            public PythonTimeDelta Mulitply(object y) {
                return this * Converter.ConvertToInt32(y);
            }
            [PythonName("__rmul__")]
            public PythonTimeDelta ReverseMulitply(object y) {
                return this * Converter.ConvertToInt32(y);
            }
            [PythonName("__floordiv__")]
            public PythonTimeDelta Divide(object y) {
                return this / Converter.ConvertToInt32(y);
            }
            [PythonName("__rfloordiv__")]
            public PythonTimeDelta ReverseDivide(object y) {
                return this / Converter.ConvertToInt32(y);
            }

            class NormalizedTimeDelta {
                public int days;
                public TimeSpan timespan;
                public int lostMicroseconds;
            }

            NormalizedTimeDelta m_normalizedTimeDelta;
            NormalizedTimeDelta Normalized {
                get {
                    if (m_normalizedTimeDelta == null) {
                        m_normalizedTimeDelta = new NormalizedTimeDelta();

                        // totalSeconds: no .xxxx
                        double totalSeconds = ((m_timeSpan.Days * 24.0 + m_timeSpan.Hours) * 60.0 + m_timeSpan.Minutes) * 60.0 + m_timeSpan.Seconds;

                        if (totalSeconds < 0) {
                            if (totalSeconds % SECONDSPERDAY == 0 && m_lostMicroseconds == 0) {
                                m_normalizedTimeDelta.days = (int)(totalSeconds / SECONDSPERDAY);
                                totalSeconds = 0;
                            } else {
                                m_normalizedTimeDelta.days = (int)(totalSeconds / SECONDSPERDAY) - 1;
                                totalSeconds = (totalSeconds - m_normalizedTimeDelta.days * SECONDSPERDAY) % SECONDSPERDAY;
                            }
                        } else {
                            m_normalizedTimeDelta.days = (int)(totalSeconds / SECONDSPERDAY);
                            totalSeconds = totalSeconds % SECONDSPERDAY;
                        }

                        totalSeconds = Math.Floor(totalSeconds);

                        // must less than 1 second, or 999,999
                        double totalMicroseconds = m_timeSpan.Milliseconds * 1000 + m_lostMicroseconds;

                        if (totalMicroseconds < 0) {
                            totalSeconds--;
                            if (totalSeconds < 0) {
                                m_normalizedTimeDelta.days--;
                                totalSeconds += SECONDSPERDAY;
                            }
                            totalMicroseconds += 1e6;
                        }

                        m_normalizedTimeDelta.timespan = TimeSpan.FromSeconds(totalSeconds).Add(TimeSpan.FromMilliseconds((long)(totalMicroseconds / 1000)));
                        m_normalizedTimeDelta.lostMicroseconds = (int)(totalMicroseconds % 1000);
                    }
                    return m_normalizedTimeDelta;
                }
            }

            public int Days {
                [PythonName("days")]
                get { return Normalized.days; }
            }
            public int Seconds {
                [PythonName("seconds")]
                get { return (int)Normalized.timespan.TotalSeconds; }
            }
            public int MicroSeconds {
                [PythonName("microseconds")]
                get { return Normalized.timespan.Milliseconds * 1000 + Normalized.lostMicroseconds; }
            }

            public override bool Equals(object obj) {
                PythonTimeDelta delta = obj as PythonTimeDelta;
                if (delta == null) return false;

                return this.m_timeSpan == delta.m_timeSpan && this.m_lostMicroseconds == delta.m_lostMicroseconds;
            }

            public override int GetHashCode() {
                return m_timeSpan.GetHashCode() ^ (int)m_lostMicroseconds;
            }

            public override string ToString() {
                StringBuilder res = new StringBuilder();
                int d = Normalized.days;
                if (d != 0) {
                    res.Append(d);
                    if (Math.Abs(d) == 1) res.Append(" day, ");
                    else res.Append(" days, ");
                }

                TimeSpan ts = Normalized.timespan;
                res.AppendFormat("{0}:{1:d2}:{2:d2}", ts.Hours, ts.Minutes, ts.Seconds);

                int ms = ts.Milliseconds * 1000 + Normalized.lostMicroseconds;
                if (ms != 0) res.AppendFormat(".{0:d6}", ms);

                return res.ToString();
            }

            [PythonName("__nonzero__")]
            public bool ToBoolean() {
                return m_timeSpan != TimeSpan.Zero || m_lostMicroseconds != 0;
            }

            #region IRichComparable Members

            [PythonName("__cmp__")]
            public object CompareTo(object other) {
                PythonTimeDelta delta = other as PythonTimeDelta;
                if (delta == null) return Ops.NotImplemented;

                int res = m_timeSpan.CompareTo(delta.m_timeSpan);
                if (res != 0) return res;

                return this.m_lostMicroseconds - delta.m_lostMicroseconds;
            }

            [PythonName("__gt__")]
            public object GreaterThan(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res > 0;
            }

            [PythonName("__lt__")]
            public object LessThan(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res < 0;
            }

            [PythonName("__ge__")]
            public object GreaterThanOrEqual(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res >= 0;
            }

            [PythonName("__le__")]
            public object LessThanOrEqual(object other) {
                object res = CompareTo(other);
                if (res == Ops.NotImplemented) return res;

                return (int)res <= 0;
            }

            #endregion

            #region IRichEquality Members

            [PythonName("__hash__")]
            public object RichGetHashCode() {
                return GetHashCode();
            }

            [PythonName("__eq__")]
            public object RichEquals(object other) {
                if (other is PythonTimeDelta)
                    return Ops.Bool2Object(Equals(other));

                return Ops.NotImplemented;
            }

            [PythonName("__ne__")]
            public object RichNotEquals(object other) {
                if (other is PythonTimeDelta)
                    return Ops.Bool2Object(!Equals(other));

                return Ops.NotImplemented;
            }

            #endregion

            #region ICodeFormattable Members

            public string ToCodeString() {
                if (Seconds == 0 && MicroSeconds == 0) {
                    return String.Format("datetime.timedelta({0})", Days);
                } else if (MicroSeconds == 0) {
                    return String.Format("datetime.timedelta({0}, {1})", Days, Seconds);
                } else {
                    return String.Format("datetime.timedelta({0}, {1}, {2})", Days, Seconds, MicroSeconds);
                }
            }

            #endregion
        }

        [PythonType("tzinfo")]
        public class PythonTimeZoneInformation : TimeZone {
            public override TimeSpan GetUtcOffset(DateTime time) {
                throw new NotImplementedException();
            }

            public override string StandardName {
                get { return TimeZoneName(PythonDateTimeCombo.Now(null)); }
            }

            public override string DaylightName {
                get { return String.Empty; }
            }

            public override DaylightTime GetDaylightChanges(int year) {
                throw new NotImplementedException();
            }

            [PythonName("dst")]
            public virtual PythonTimeDelta DaylightSavingsTime(object dt) {
                throw new NotImplementedException();
            }
            [PythonName("fromutc")]
            public virtual object FromUtc(object dt) {
                throw new NotImplementedException();
            }
            [PythonName("tzname")]
            public virtual string TimeZoneName(object dt) {
                throw new NotImplementedException();
            }
            [PythonName("utcoffset")]
            public virtual PythonTimeDelta UtcOffset(object dt) {
                throw new NotImplementedException();
            }
        }

    }
}
