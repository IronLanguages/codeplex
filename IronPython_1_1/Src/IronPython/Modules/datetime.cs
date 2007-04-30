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
using IronPython.Runtime.Types;

[assembly: PythonModule("datetime", typeof(IronPython.Modules.PythonDateTime))]
namespace IronPython.Modules {
    [PythonType("datetime")]
    public class PythonDateTime {
        public static object MAXYEAR = DateTime.MaxValue.Year;
        public static object MINYEAR = DateTime.MinValue.Year;

        [PythonType("timedelta")]
        public class PythonTimeDelta : IRichComparable, ICodeFormattable {
            internal int m_days;
            internal int m_seconds;
            internal int m_microseconds;

            bool m_fWithDaysAndSeconds = false;
            bool m_fWithSeconds = false;
            TimeSpan m_tsWithDaysAndSeconds, m_tsWithSeconds; // value type
            internal TimeSpan TimeSpanWithDaysAndSeconds {
                get {
                    if (!m_fWithDaysAndSeconds) {
                        m_tsWithDaysAndSeconds = new TimeSpan(m_days, 0, 0, m_seconds);
                        m_fWithDaysAndSeconds = true;
                    }
                    return m_tsWithDaysAndSeconds;
                }
            }
            internal TimeSpan TimeSpanWithSeconds {
                get {
                    if (!m_fWithSeconds) {
                        m_tsWithSeconds = TimeSpan.FromSeconds(m_seconds);
                        m_fWithSeconds = true;
                    }
                    return m_tsWithSeconds;
                }
            }

            internal static PythonTimeDelta s_dayResolution = new PythonTimeDelta(1, 0, 0);

            private const int MAXDAYS = 999999999;
            private const double SECONDSPERDAY = 24 * 60 * 60;

            internal PythonTimeDelta(double days, double seconds, double microsecond)
                : this(days, seconds, microsecond, 0, 0, 0, 0) {
            }
            internal PythonTimeDelta(TimeSpan ts, double microsecond)
                : this(ts.Days, ts.Seconds, microsecond, ts.Milliseconds, ts.Minutes, ts.Hours, 0) {
            }

            public PythonTimeDelta(double days, double seconds, double microseconds, double milliseconds, double minutes, double hours, double weeks) {
                double totalDays = weeks * 7 + days;
                double totalSeconds = ((totalDays * 24 + hours) * 60 + minutes) * 60 + seconds;

                double totalSecondsSharp = Math.Floor(totalSeconds);
                double totalSecondsFloat = totalSeconds - totalSecondsSharp;

                double totalMicroseconds = Math.Round(totalSecondsFloat * 1e6 + milliseconds * 1000 + microseconds);
                double otherSecondsFromMicroseconds = Math.Floor(totalMicroseconds / 1e6);

                totalSecondsSharp += otherSecondsFromMicroseconds;
                totalMicroseconds -= otherSecondsFromMicroseconds * 1e6;

                if (totalSecondsSharp > 0 && totalMicroseconds < 0) {
                    totalSecondsSharp -= 1;
                    totalMicroseconds += 1e6;
                }

                m_days = (int)(totalSecondsSharp / SECONDSPERDAY);
                m_seconds = (int)(totalSecondsSharp - m_days * SECONDSPERDAY);

                if (m_seconds < 0) {
                    m_days--;
                    m_seconds += (int)SECONDSPERDAY;
                }
                m_microseconds = (int)(totalMicroseconds);

                if (Math.Abs(m_days) > MAXDAYS) {
                    throw Ops.OverflowError("days={0}; must have magnitude <= 999999999", m_days);
                }
            }

            [PythonName("__new__")]
            public static PythonTimeDelta Make(DynamicType cls,
                [DefaultParameterValue(0D)] double days,
                [DefaultParameterValue(0D)] double seconds,
                [DefaultParameterValue(0D)] double microseconds,
                [DefaultParameterValue(0D)] double milliseconds,
                [DefaultParameterValue(0D)] double minutes,
                [DefaultParameterValue(0D)] double hours,
                [DefaultParameterValue(0D)] double weeks) {
                if (cls == Ops.GetDynamicTypeFromType(typeof(PythonTimeDelta))) {
                    return new PythonTimeDelta(days, seconds, microseconds, milliseconds, minutes, hours, weeks);
                } else {
                    PythonTimeDelta delta = cls.ctor.Call(cls, days, seconds, microseconds, milliseconds, minutes, hours, weeks) as PythonTimeDelta;
                    if (delta == null) throw Ops.TypeError("{0} is not a subclass of datetime.timedelta", cls);
                    return delta;
                }
            }

            // class attributes:
            public static PythonTimeDelta resolution = new PythonTimeDelta(0, 0, 1);
            public static PythonTimeDelta min = new PythonTimeDelta(-MAXDAYS, 0, 0);
            public static PythonTimeDelta max = new PythonTimeDelta(MAXDAYS, 86399, 999999);

            // instance attributes:
            public int Days {
                [PythonName("days")]
                get { return m_days; }
            }
            public int Seconds {
                [PythonName("seconds")]
                get { return m_seconds; }
            }
            public int MicroSeconds {
                [PythonName("microseconds")]
                get { return m_microseconds; }
            }

            // supported operations:
            public static PythonTimeDelta operator +(PythonTimeDelta self, PythonTimeDelta other) {
                return new PythonTimeDelta(self.m_days + other.m_days, self.m_seconds + other.m_seconds, self.m_microseconds + other.m_microseconds);
            }
            public static PythonTimeDelta operator -(PythonTimeDelta self, PythonTimeDelta other) {
                return new PythonTimeDelta(self.m_days - other.m_days, self.m_seconds - other.m_seconds, self.m_microseconds - other.m_microseconds);
            }
            public static PythonTimeDelta operator -(PythonTimeDelta self) {
                return new PythonTimeDelta(-self.m_days, -self.m_seconds, -self.m_microseconds);
            }
            public static PythonTimeDelta operator +(PythonTimeDelta self) {
                return new PythonTimeDelta(self.m_days, self.m_seconds, self.m_microseconds);
            }
            public static PythonTimeDelta operator *(PythonTimeDelta self, int other) {
                return new PythonTimeDelta(self.m_days * other, self.m_seconds * other, self.m_microseconds * other);
            }
            public static PythonTimeDelta operator /(PythonTimeDelta self, int other) {
                return new PythonTimeDelta((double)self.m_days / other, (double)self.m_seconds / other, (double)self.m_microseconds / other);
            }

            [PythonName("__pos__")]
            public PythonTimeDelta Positive() { return +this; }
            [PythonName("__neg__")]
            public PythonTimeDelta Negate() { return -this; }
            [PythonName("__abs__")]
            public PythonTimeDelta Abs() { return (m_days > 0) ? this : -this; }
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

            [PythonName("__nonzero__")]
            public bool NonZero() {
                return this.m_days != 0 || this.m_seconds != 0 || this.m_microseconds != 0;
            }

            [PythonName("__reduce__")]
            public Tuple Reduce() {
                return Tuple.MakeTuple(Ops.GetDynamicTypeFromType(this.GetType()), Tuple.MakeTuple(m_days, m_seconds, m_microseconds));
            }
            [PythonName("__getnewargs__")]
            public static object GetNewArgs(int days, int seconds, int microseconds) {
                return Tuple.MakeTuple(new PythonTimeDelta(days, seconds, microseconds, 0, 0, 0, 0));
            }

            public override bool Equals(object obj) {
                PythonTimeDelta delta = obj as PythonTimeDelta;
                if (delta == null) return false;

                return this.m_days == delta.m_days && this.m_seconds == delta.m_seconds && this.m_microseconds == delta.m_microseconds;
            }
            public override int GetHashCode() {
                return this.m_days ^ this.m_seconds ^ this.m_microseconds;
            }

            [PythonName("__str__")]
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                if (m_days != 0) {
                    sb.Append(m_days);
                    if (Math.Abs(m_days) == 1)
                        sb.Append(" day, ");
                    else
                        sb.Append(" days, ");
                }

                sb.AppendFormat("{0}:{1:d2}:{2:d2}", TimeSpanWithSeconds.Hours, TimeSpanWithSeconds.Minutes, TimeSpanWithSeconds.Seconds);

                if (m_microseconds != 0)
                    sb.AppendFormat(".{0:d6}", m_microseconds);

                return sb.ToString();
            }

            #region IRichComparable Members

            [PythonName("__cmp__")]
            public object CompareTo(object other) {
                PythonTimeDelta delta = other as PythonTimeDelta;
                if (delta == null)
                    throw Ops.TypeError("can't compare datetime.timedelta to {0}", Ops.GetDynamicType(other));

                int res = this.m_days - delta.m_days;
                if (res != 0) return res;

                res = this.m_seconds - delta.m_seconds;
                if (res != 0) return res;

                return this.m_microseconds - delta.m_microseconds;
            }

            [PythonName("__gt__")]
            public object GreaterThan(object other) {
                return (int)CompareTo(other) > 0;
            }

            [PythonName("__lt__")]
            public object LessThan(object other) {
                return (int)CompareTo(other) < 0;
            }

            [PythonName("__ge__")]
            public object GreaterThanOrEqual(object other) {
                return (int)CompareTo(other) >= 0;
            }

            [PythonName("__le__")]
            public object LessThanOrEqual(object other) {
                return (int)CompareTo(other) <= 0;
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

            #region ICodeFormattable Members
            public string ToCodeString() {
                if (m_seconds == 0 && m_microseconds == 0) {
                    return String.Format("datetime.timedelta({0})", m_days);
                } else if (m_microseconds == 0) {
                    return String.Format("datetime.timedelta({0}, {1})", m_days, m_seconds);
                } else {
                    return String.Format("datetime.timedelta({0}, {1}, {2})", m_days, m_seconds, m_microseconds);
                }
            }
            #endregion
        }

        internal static void ThrowIfInvalid(PythonTimeDelta delta, string funcname) {
            if (delta != null) {
                if (delta.m_microseconds != 0 || delta.m_seconds % 60 != 0) {
                    throw Ops.ValueError("tzinfo.{0}() must return a whole number of minutes", funcname);
                }

                int minutes = (int)(delta.TimeSpanWithDaysAndSeconds.TotalSeconds / 60);
                if (Math.Abs(minutes) >= 1440) {
                    throw Ops.ValueError("tzinfo.{0}() returned {1}; must be in -1439 .. 1439", funcname, minutes);
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

        internal static bool IsNaiveTimeZone(PythonTimeZoneInformation tz) {
            if (tz == null) return true;
            if (tz.UtcOffset(null) == null) return true;
            return false;
        }

        [PythonType("date")]
        public class PythonDate : ICodeFormattable, IRichEquality, IRichComparable {
            protected DateTime m_dateTime;
            protected PythonDate() { }

            public PythonDate(int year, int month, int day) {
                PythonDateTime.ValidateInput(InputKind.Year, year);
                PythonDateTime.ValidateInput(InputKind.Month, month);
                PythonDateTime.ValidateInput(InputKind.Day, day);

                m_dateTime = new DateTime(year, month, day);
            }

            internal PythonDate(DateTime value) {
                m_dateTime = value.Date; // no hour, minute, second
            }

            [PythonName("__new__")]
            public static PythonDate Make(DynamicType cls, int year, int month, int day) {
                if (cls == Ops.GetDynamicTypeFromType(typeof(PythonDate))) {
                    return new PythonDate(year, month, day);
                } else {
                    PythonDate date = cls.ctor.Call(cls, year, month, day) as PythonDate;
                    if (date == null) throw Ops.TypeError("{0} is not a subclass of datetime.date", cls);
                    return date;
                }
            }

            // other constructors, all class methods
            [PythonName("today")]
            public static object Today() {
                return new PythonDate(DateTime.Today);
            }

            [PythonName("fromordinal")]
            public static PythonDate FromOrdinal(int d) {
                return new PythonDate(min.m_dateTime.AddDays(d - 1));
            }

            [PythonName("fromtimestamp")]
            public static PythonDate FromTimestamp(double timestamp) {
                DateTime dt = new DateTime((long)(timestamp * 1e7));
                return new PythonDate(dt.Year, dt.Month, dt.Day);
            }

            // class attributes
            public static PythonDate min = new PythonDate(new DateTime(1, 1, 1));
            public static PythonDate max = new PythonDate(new DateTime(9999, 12, 31));
            public static PythonTimeDelta resolution = PythonTimeDelta.s_dayResolution;

            // instance attributes
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

            // supported operations
            public static PythonDate operator +(PythonDate self, PythonTimeDelta other) {
                try {
                    return new PythonDate(self.m_dateTime.AddDays(other.Days));
                } catch {
                    throw Ops.OverflowError("date value out of range");
                }
            }
            [PythonName("__radd__")]
            public object ReverseAdd(PythonTimeDelta delta) { return this + delta; }
            public static PythonDate operator -(PythonDate self, PythonTimeDelta delta) {
                try {
                    return new PythonDate(self.m_dateTime.AddDays(-1 * delta.Days));
                } catch {
                    throw Ops.OverflowError("date value out of range");
                }
            }
            public static PythonTimeDelta operator -(PythonDate self, PythonDate other) {
                TimeSpan ts = self.m_dateTime - other.m_dateTime;
                return new PythonTimeDelta(0, ts.TotalSeconds, ts.Milliseconds * 1000);
            }

            [PythonName("__nonzero__")]
            public bool NonZero() { return true; }

            [PythonName("__reduce__")]
            public virtual Tuple Reduce() {
                return Tuple.MakeTuple(Ops.GetDynamicTypeFromType(this.GetType()), Tuple.MakeTuple(m_dateTime.Year, m_dateTime.Month, m_dateTime.Day));
            }

            [PythonName("__getnewargs__")]
            public static object GetNewArgs(int year, int month, int day) {
                return Tuple.MakeTuple(PythonDate.Make(Ops.GetDynamicTypeFromType(typeof(PythonDate)), year, month, day));
            }

            // instance methods
            [PythonName("replace")]
            public PythonDate Replace([DefaultParameterValue(null)] object year,
                [DefaultParameterValue(null)]object month,
                [DefaultParameterValue(null)]object day) {
                int year2 = m_dateTime.Year;
                int month2 = m_dateTime.Month;
                int day2 = m_dateTime.Day;

                if (year != null)
                    year2 = Converter.ConvertToInt32(year);

                if (month != null)
                    month2 = Converter.ConvertToInt32(month);

                if (day != null)
                    day2 = Converter.ConvertToInt32(day);

                return new PythonDate(year2, month2, day2);
            }

            [PythonName("timetuple")]
            public virtual object GetTimeTuple() {
                return PythonTime.GetDateTimeTuple(m_dateTime);
            }

            [PythonName("toordinal")]
            public int ToOrdinal() {
                return (m_dateTime - min.m_dateTime).Days + 1;
            }

            [PythonName("weekday")]
            public int Weekday() { return PythonTime.Weekday(m_dateTime); }

            [PythonName("isoweekday")]
            public int IsoWeekday() { return PythonTime.IsoWeekday(m_dateTime); }

            private DateTime FirstDayOfIsoYear(int year) {
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

            [PythonName("isocalendar")]
            public Tuple GetIsoCalendar() {
                DateTime firstDayOfLastIsoYear = FirstDayOfIsoYear(m_dateTime.Year - 1);
                DateTime firstDayOfThisIsoYear = FirstDayOfIsoYear(m_dateTime.Year);
                DateTime firstDayOfNextIsoYear = FirstDayOfIsoYear(m_dateTime.Year + 1);

                int year, days;
                if (firstDayOfThisIsoYear <= m_dateTime && m_dateTime < firstDayOfNextIsoYear) {
                    year = m_dateTime.Year;
                    days = (m_dateTime - firstDayOfThisIsoYear).Days;
                } else if (m_dateTime < firstDayOfThisIsoYear) {
                    year = m_dateTime.Year - 1;
                    days = (m_dateTime - firstDayOfLastIsoYear).Days;
                } else {
                    year = m_dateTime.Year + 1;
                    days = (m_dateTime - firstDayOfNextIsoYear).Days;
                }

                return Tuple.MakeTuple(year, days / 7 + 1, days % 7 + 1);
            }

            [PythonName("isoformat")]
            public string IsoFormat() {
                return m_dateTime.ToString("yyyy-MM-dd");
            }

            [PythonName("__str__")]
            public override string ToString() {
                return IsoFormat();
            }

            [PythonName("ctime")]
            public string GetCTime() {
                return m_dateTime.ToString("ddd MMM ") + string.Format("{0,2}", m_dateTime.Day) + m_dateTime.ToString(" HH:mm:ss yyyy");
            }

            [PythonName("strftime")]
            public string Format(string dateFormat) {
                return PythonTime.FormatTime(dateFormat, m_dateTime);
            }

            public override bool Equals(object obj) {
                if (obj == null) return false;

                if (obj.GetType() == typeof(PythonDate)) {
                    PythonDate other = (PythonDate)obj;
                    return this.m_dateTime == other.m_dateTime;
                } else {
                    return false;
                }
            }

            public override int GetHashCode() {
                return m_dateTime.GetHashCode();
            }

            #region IRichComparable Members

            [PythonName("__cmp__")]
            public virtual object CompareTo(object other) {
                if (other == null)
                    throw Ops.TypeError("can't compare datetime.date to NoneType");

                if (other.GetType() != typeof(PythonDate))
                    throw Ops.TypeError("can't compare datetime.date to {0}", Ops.GetDynamicType(other));

                PythonDate date = other as PythonDate;
                return this.m_dateTime.CompareTo(date.m_dateTime);
            }

            [PythonName("__gt__")]
            public object GreaterThan(object other) {
                return (int)CompareTo(other) > 0;
            }

            [PythonName("__lt__")]
            public object LessThan(object other) {
                return (int)CompareTo(other) < 0;
            }

            [PythonName("__ge__")]
            public object GreaterThanOrEqual(object other) {
                return (int)CompareTo(other) >= 0;
            }

            [PythonName("__le__")]
            public object LessThanOrEqual(object other) {
                return (int)CompareTo(other) <= 0;
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

            #region ICodeFormattable Members

            public virtual string ToCodeString() {
                return string.Format("datetime.date({0}, {1}, {2})", m_dateTime.Year, m_dateTime.Month, m_dateTime.Day);
            }

            #endregion
        }

        [PythonType("datetime")]
        public class PythonDateTimeCombo : PythonDate {
            internal int m_lostMicroseconds;
            internal PythonTimeZoneInformation m_tz;

            class UnifiedDateTime {
                public DateTime DateTime;
                public int LostMicroseconds;

                public override bool Equals(object obj) {
                    UnifiedDateTime other = obj as UnifiedDateTime;
                    if (other == null) return false;

                    return this.DateTime == other.DateTime && this.LostMicroseconds == other.LostMicroseconds;
                }

                public override int GetHashCode() {
                    return DateTime.GetHashCode() ^ LostMicroseconds;
                }

                public int CompareTo(UnifiedDateTime other) {
                    int res = this.DateTime.CompareTo(other.DateTime);

                    if (res != 0) return res;

                    return this.LostMicroseconds - other.LostMicroseconds;
                }
            }
            UnifiedDateTime m_utcDateTime;
            UnifiedDateTime UtcDateTime {
                get {
                    if (m_utcDateTime == null) {
                        m_utcDateTime = new UnifiedDateTime();

                        m_utcDateTime.DateTime = m_dateTime;
                        m_utcDateTime.LostMicroseconds = m_lostMicroseconds;

                        PythonTimeDelta delta = this.GetUtcOffset();
                        if (delta != null) {
                            PythonDateTimeCombo utced = this - delta;
                            m_utcDateTime.DateTime = utced.m_dateTime;
                            m_utcDateTime.LostMicroseconds = utced.m_lostMicroseconds;
                        }
                    }
                    return m_utcDateTime;
                }
            }

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

            // other constructors, all class methods:
            [PythonName("today")]
            public new static object Today() {
                return new PythonDateTimeCombo(DateTime.Now, 0, null);
            }

            [PythonName("now")]
            public static object Now([DefaultParameterValue(null)]PythonTimeZoneInformation tz) {
                if (tz != null) {
                    return tz.FromUtc(new PythonDateTimeCombo(DateTime.UtcNow, 0, tz));
                } else {
                    return new PythonDateTimeCombo(DateTime.Now, 0, null);
                }
            }

            [PythonName("utcnow")]
            public static object UtcNow() {
                return new PythonDateTimeCombo(DateTime.UtcNow, 0, null);
            }

            [PythonName("fromtimestamp")]
            public static object FromTimeStamp(double timestamp, [DefaultParameterValue(null)] PythonTimeZoneInformation tz) {
                DateTime dt = new DateTime((long)(timestamp * 1e7));

                if (tz != null) {
                    dt = dt.ToUniversalTime();
                    PythonDateTimeCombo pdtc = new PythonDateTimeCombo(dt, 0, tz);
                    return tz.FromUtc(pdtc);
                } else {
                    return new PythonDateTimeCombo(dt, 0, null);
                }
            }

            [PythonName("utcfromtimestamp")]
            public static PythonDateTimeCombo UtcFromTimestamp(double timestamp) {
                DateTime dt = new DateTime((long)(timestamp * 1e7));
                dt = dt = dt.ToUniversalTime();
                return new PythonDateTimeCombo(dt, 0, null);
            }

            [PythonName("fromordinal")]
            public new static PythonDateTimeCombo FromOrdinal(int d) {
                return new PythonDateTimeCombo(DateTime.MinValue + new TimeSpan(d - 1, 0, 0, 0), 0, null);
            }

            [PythonName("combine")]
            public static object Combine(PythonDate date, PythonDateTimeTime time) {
                return new PythonDateTimeCombo(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Microsecond, time.TimeZoneInfo);
            }

            // class attributes
            public new static object max = new PythonDateTimeCombo(DateTime.MaxValue, 999, null);
            public new static object min = new PythonDateTimeCombo(DateTime.MinValue, 0, null);
            public new static object resolution = PythonTimeDelta.resolution;

            // instance attributes
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

            public object TimeZoneInformation {
                [PythonName("tzinfo")]
                get { return m_tz; }
            }

            // supported operations
            public static PythonDateTimeCombo operator +(PythonDateTimeCombo date, PythonTimeDelta delta) {
                return new PythonDateTimeCombo(date.m_dateTime.Add(delta.TimeSpanWithDaysAndSeconds), delta.m_microseconds + date.m_lostMicroseconds, date.m_tz);
            }
            [PythonName("__radd__")]
            public new PythonDateTimeCombo ReverseAdd(PythonTimeDelta delta) { return this + delta; }
            public static PythonDateTimeCombo operator -(PythonDateTimeCombo date, PythonTimeDelta delta) {
                return new PythonDateTimeCombo(date.m_dateTime.Subtract(delta.TimeSpanWithDaysAndSeconds), date.m_lostMicroseconds - delta.m_microseconds, date.m_tz);
            }

            public static PythonTimeDelta operator -(PythonDateTimeCombo date, PythonDateTimeCombo other) {
                if (CheckTzInfoBeforeCompare(date, other)) {
                    return new PythonTimeDelta(date.m_dateTime - other.m_dateTime, date.m_lostMicroseconds - other.m_lostMicroseconds);
                } else {
                    return new PythonTimeDelta(date.UtcDateTime.DateTime - other.UtcDateTime.DateTime, date.UtcDateTime.LostMicroseconds - other.UtcDateTime.LostMicroseconds);
                }
            }

            // instance methods
            [PythonName("date")]
            public PythonDate ToDate() {
                return new PythonDate(Year, Month, Day);
            }

            [PythonName("time")]
            [Documentation("gets the datetime w/o the time zone component")]
            public PythonDateTimeTime GetTime() {
                return new PythonDateTimeTime(Hour, Minute, Second, Microsecond, null);
            }

            [PythonName("timetz")]
            public object GetTimeWithTimeZone() {
                return new PythonDateTimeTime(Hour, Minute, Second, Microsecond, m_tz);
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

            [PythonName("astimezone")]
            public object AsTimeZone(PythonTimeZoneInformation tz) {
                if (tz == null)
                    throw Ops.TypeError("astimezone() argument 1 must be datetime.tzinfo, not None");

                if (m_tz == null)
                    throw Ops.ValueError("astimezone() cannot be applied to a naive datetime");

                if (tz == m_tz)
                    return this;

                PythonDateTimeCombo utc = this - GetUtcOffset();
                utc.m_tz = tz;
                return tz.FromUtc(utc);
            }

            [PythonName("utcoffset")]
            public PythonTimeDelta GetUtcOffset() {
                if (m_tz == null) return null;
                PythonTimeDelta delta = m_tz.UtcOffset(this);
                PythonDateTime.ThrowIfInvalid(delta, "utcoffset");
                return delta;
            }

            [PythonName("dst")]
            public PythonTimeDelta dst() {
                if (m_tz == null) return null;
                PythonTimeDelta delta = m_tz.DaylightSavingTime(this);
                PythonDateTime.ThrowIfInvalid(delta, "dst");
                return delta;
            }

            [PythonName("tzname")]
            public object GetTimeZoneName() {
                if (m_tz == null) return null;
                return m_tz.TimeZoneName(this);
            }

            [PythonName("timetuple")]
            public override object GetTimeTuple() {
                return PythonTime.GetDateTimeTuple(m_dateTime, m_tz);
            }

            [PythonName("utctimetuple")]
            public object GetUtcTimeTuple() {
                if (m_tz == null)
                    return PythonTime.GetDateTimeTuple(m_dateTime, null, true);
                else {
                    PythonDateTimeCombo dtc = this - GetUtcOffset();
                    return PythonTime.GetDateTimeTuple(dtc.m_dateTime, null, true);
                }
            }

            [PythonName("isoformat")]
            public string IsoFormat([DefaultParameterValue('T')]char sep) {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0:d4}-{1:d2}-{2:d2}{3}{4:d2}:{5:d2}:{6:d2}", Year, Month, Day, sep, Hour, Minute, Second);

                if (Microsecond != 0) sb.AppendFormat(".{0:d6}", Microsecond);

                PythonTimeDelta delta = GetUtcOffset();
                if (delta != null) {
                    if (delta.TimeSpanWithDaysAndSeconds >= TimeSpan.Zero) {
                        sb.AppendFormat("+{0:d2}:{1:d2}", delta.TimeSpanWithDaysAndSeconds.Hours, delta.TimeSpanWithDaysAndSeconds.Minutes);
                    } else {
                        sb.AppendFormat("-{0:d2}:{1:d2}", -delta.TimeSpanWithDaysAndSeconds.Hours, -delta.TimeSpanWithDaysAndSeconds.Minutes);
                    }
                }

                return sb.ToString();
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
                return this.UtcDateTime.DateTime.GetHashCode() ^ this.UtcDateTime.LostMicroseconds;
            }

            public override string ToString() {
                return IsoFormat(' ');
            }

            #region IRichComparable Members

            [PythonName("__cmp__")]
            public override object CompareTo(object other) {
                if (other == null)
                    throw Ops.TypeError("can't compare datetime.datetime to NoneType");

                PythonDateTimeCombo combo = other as PythonDateTimeCombo;
                if (combo == null)
                    throw Ops.TypeError("can't compare datetime.datetime to {0}", Ops.GetDynamicType(other));

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


            #endregion

            #region ICodeFormattable Members

            public override string ToCodeString() {
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

        [PythonType("time")]
        public class PythonDateTimeTime : ICodeFormattable, IRichComparable {
            internal TimeSpan m_timeSpan;
            internal int m_lostMicroseconds;
            internal PythonTimeZoneInformation m_tz;

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
                        }
                    }
                    return m_utcTime;
                }
            }

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

            // class attributes:
            public static object max = new PythonDateTimeTime(23, 59, 59, 999999, null);
            public static object min = new PythonDateTimeTime(0, 0, 0, 0, null);
            public static object resolution = PythonTimeDelta.resolution;

            // instance attributes:
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

            // supported operations
            public static PythonDateTimeTime operator +(PythonDateTimeTime date, PythonTimeDelta delta) {
                return new PythonDateTimeTime(date.m_timeSpan.Add(delta.TimeSpanWithDaysAndSeconds), delta.m_microseconds + date.m_lostMicroseconds, date.m_tz);
            }

            public static PythonDateTimeTime operator -(PythonDateTimeTime date, PythonTimeDelta delta) {
                return new PythonDateTimeTime(date.m_timeSpan.Subtract(delta.TimeSpanWithDaysAndSeconds), date.m_lostMicroseconds - delta.m_microseconds, date.m_tz);
            }

            [PythonName("__nonzero__")]
            public bool NonZero() {
                return this.UtcTime.TimeSpan.Ticks != 0 || this.UtcTime.LostMicroseconds != 0;
            }

            // instance methods
            [PythonName("replace")]
            public object Replace() {
                return this;
            }

            [PythonName("replace")]
            public object Replace([ParamDict]Dict dict) {
                int hour = Hour;
                int minute = Minute;
                int second = Second;
                int microsecond = Microsecond;
                PythonTimeZoneInformation tz = TimeZoneInfo;

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
            [PythonName("isoformat")]
            public object GetIsoFormat() {
                return ToString();
            }

            [PythonName("__str__")]
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0:d2}:{1:d2}:{2:d2}", Hour, Minute, Second);

                if (Microsecond != 0) sb.AppendFormat(".{0:d6}", Microsecond);

                PythonTimeDelta delta = GetUtcOffset();
                if (delta != null) {
                    if (delta.TimeSpanWithDaysAndSeconds >= TimeSpan.Zero) {
                        sb.AppendFormat("+{0:d2}:{1:d2}", delta.TimeSpanWithDaysAndSeconds.Hours, delta.TimeSpanWithDaysAndSeconds.Minutes);
                    } else {
                        sb.AppendFormat("-{0:d2}:{1:d2}", -delta.TimeSpanWithDaysAndSeconds.Hours, -delta.TimeSpanWithDaysAndSeconds.Minutes);
                    }
                }

                return sb.ToString();
            }

            [PythonName("strftime")]
            public object FormatTime(string format) {
                return PythonTime.FormatTime(format,
                    new DateTime(1900, 1, 1, m_timeSpan.Hours, m_timeSpan.Minutes, m_timeSpan.Seconds, m_timeSpan.Milliseconds));
            }

            [PythonName("utcoffset")]
            public PythonTimeDelta GetUtcOffset() {
                if (m_tz == null) return null;
                PythonTimeDelta delta = m_tz.UtcOffset(null);
                PythonDateTime.ThrowIfInvalid(delta, "utcoffset");
                return delta;
            }

            [PythonName("dst")]
            public object DaylightSavingsTime() {
                if (m_tz == null) return null;
                PythonTimeDelta delta = m_tz.DaylightSavingTime(null);
                PythonDateTime.ThrowIfInvalid(delta, "dst");
                return delta;
            }

            [PythonName("tzname")]
            public object TimeZoneName() {
                if (m_tz == null) return null;
                return m_tz.TimeZoneName(null);
            }

            public override int GetHashCode() {
                return this.UtcTime.GetHashCode();
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
                if (other2 == null)
                    throw Ops.TypeError("can't compare datetime.time to {0}", Ops.GetDynamicType(other));

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
                return (int)CompareTo(other) > 0;
            }

            [PythonName("__lt__")]
            public object LessThan(object other) {
                return (int)CompareTo(other) < 0;
            }

            [PythonName("__ge__")]
            public object GreaterThanOrEqual(object other) {
                return (int)CompareTo(other) >= 0;
            }

            [PythonName("__le__")]
            public object LessThanOrEqual(object other) {
                return (int)CompareTo(other) <= 0;
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

        [PythonType("tzinfo")]
        public class PythonTimeZoneInformation {
            [PythonName("fromutc")]
            public virtual object FromUtc(PythonDateTimeCombo dt) {
                PythonTimeDelta dtOffset = UtcOffset(dt);
                if (dtOffset == null)
                    throw Ops.ValueError("fromutc: non-None utcoffset() result required");

                PythonTimeDelta dtDst = DaylightSavingTime(dt);
                if (dtDst == null)
                    throw Ops.ValueError("fromutc: non-None dst() result required");

                PythonTimeDelta delta = dtOffset - dtDst;
                dt = dt + delta; // convert to standard LOCAL time
                dtDst = dt.dst();

                return dt + dtDst;
            }
            [PythonName("dst")]
            public virtual PythonTimeDelta DaylightSavingTime(object dt) {
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
