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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Microsoft.Scripting;

[assembly: PythonModule("datetime", typeof(IronPython.Modules.PythonDateTime))]
namespace IronPython.Modules {
    [PythonType("datetime")]
    public class PythonDateTime {
        public static object MAXYEAR = DateTime.MaxValue.Year;
        public static object MINYEAR = DateTime.MinValue.Year;

        [PythonType("timedelta")]
        public class PythonTimeDelta : ICodeFormattable {
            internal int _days;
            internal int _seconds;
            internal int _microseconds;

            private TimeSpan _tsWithDaysAndSeconds, _tsWithSeconds; // value type
            private bool _fWithDaysAndSeconds = false; // whether _tsWithDaysAndSeconds initialized
            private bool _fWithSeconds = false;

            internal static PythonTimeDelta _DayResolution = new PythonTimeDelta(1, 0, 0);

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

                _days = (int)(totalSecondsSharp / SECONDSPERDAY);
                _seconds = (int)(totalSecondsSharp - _days * SECONDSPERDAY);

                if (_seconds < 0) {
                    _days--;
                    _seconds += (int)SECONDSPERDAY;
                }
                _microseconds = (int)(totalMicroseconds);

                if (Math.Abs(_days) > MAXDAYS) {
                    throw PythonOps.OverflowError("days={0}; must have magnitude <= 999999999", _days);
                }
            }

            [PythonName("__new__")]
            public static PythonTimeDelta Make(CodeContext context, DynamicType cls,
                [DefaultParameterValue(0D)] double days,
                [DefaultParameterValue(0D)] double seconds,
                [DefaultParameterValue(0D)] double microseconds,
                [DefaultParameterValue(0D)] double milliseconds,
                [DefaultParameterValue(0D)] double minutes,
                [DefaultParameterValue(0D)] double hours,
                [DefaultParameterValue(0D)] double weeks) {
                if (cls == DynamicHelpers.GetDynamicTypeFromType(typeof(PythonTimeDelta))) {
                    return new PythonTimeDelta(days, seconds, microseconds, milliseconds, minutes, hours, weeks);
                } else {
                    PythonTimeDelta delta = cls.CreateInstance(context, days, seconds, microseconds, milliseconds, minutes, hours, weeks) as PythonTimeDelta;
                    if (delta == null) throw PythonOps.TypeError("{0} is not a subclass of datetime.timedelta", cls);
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
                get { return _days; }
            }
            public int Seconds {
                [PythonName("seconds")]
                get { return _seconds; }
            }
            public int MicroSeconds {
                [PythonName("microseconds")]
                get { return _microseconds; }
            }

            internal TimeSpan TimeSpanWithDaysAndSeconds {
                get {
                    if (!_fWithDaysAndSeconds) {
                        _tsWithDaysAndSeconds = new TimeSpan(_days, 0, 0, _seconds);
                        _fWithDaysAndSeconds = true;
                    }
                    return _tsWithDaysAndSeconds;
                }
            }
            internal TimeSpan TimeSpanWithSeconds {
                get {
                    if (!_fWithSeconds) {
                        _tsWithSeconds = TimeSpan.FromSeconds(_seconds);
                        _fWithSeconds = true;
                    }
                    return _tsWithSeconds;
                }
            }

            // supported operations:
            public static PythonTimeDelta operator +(PythonTimeDelta self, PythonTimeDelta other) {
                return new PythonTimeDelta(self._days + other._days, self._seconds + other._seconds, self._microseconds + other._microseconds);
            }

            public static PythonTimeDelta operator -(PythonTimeDelta self, PythonTimeDelta other) {
                return new PythonTimeDelta(self._days - other._days, self._seconds - other._seconds, self._microseconds - other._microseconds);
            }

            public static PythonTimeDelta operator -(PythonTimeDelta self) {
                return new PythonTimeDelta(-self._days, -self._seconds, -self._microseconds);
            }

            public static PythonTimeDelta operator +(PythonTimeDelta self) {
                return new PythonTimeDelta(self._days, self._seconds, self._microseconds);
            }

            public static PythonTimeDelta operator *(PythonTimeDelta self, int other) {
                return new PythonTimeDelta(self._days * other, self._seconds * other, self._microseconds * other);
            }

            public static PythonTimeDelta operator *(int other, PythonTimeDelta self) {
                return new PythonTimeDelta(self._days * other, self._seconds * other, self._microseconds * other);
            }

            public static PythonTimeDelta operator /(PythonTimeDelta self, int other) {
                return new PythonTimeDelta((double)self._days / other, (double)self._seconds / other, (double)self._microseconds / other);
            }

            [OperatorMethod, PythonName("__pos__")]
            public PythonTimeDelta Positive() { return +this; }
            [OperatorMethod, PythonName("__neg__")]
            public PythonTimeDelta Negate() { return -this; }
            [OperatorMethod, PythonName("__abs__")]
            public PythonTimeDelta Abs() { return (_days > 0) ? this : -this; }
            [OperatorMethod, PythonName("__mul__")]
            public PythonTimeDelta Mulitply(object y) {
                return this * Converter.ConvertToInt32(y);
            }
            [OperatorMethod, PythonName("__rmul__")]
            public PythonTimeDelta ReverseMulitply(object y) {
                return this * Converter.ConvertToInt32(y);
            }
            [OperatorMethod, PythonName("__floordiv__")]
            public PythonTimeDelta FloorDivide(object y) {
                return this / Converter.ConvertToInt32(y);
            }
            [OperatorMethod, PythonName("__rfloordiv__")]
            public PythonTimeDelta ReverseFloorDivide(object y) {
                return this / Converter.ConvertToInt32(y);
            }

            [OperatorMethod, PythonName("__nonzero__")]
            public bool NonZero() {
                return this._days != 0 || this._seconds != 0 || this._microseconds != 0;
            }

            [PythonName("__reduce__")]
            public Tuple Reduce() {
                return Tuple.MakeTuple(DynamicHelpers.GetDynamicTypeFromType(this.GetType()), Tuple.MakeTuple(_days, _seconds, _microseconds));
            }
            [PythonName("__getnewargs__")]
            public static object GetNewArgs(int days, int seconds, int microseconds) {
                return Tuple.MakeTuple(new PythonTimeDelta(days, seconds, microseconds, 0, 0, 0, 0));
            }

            public override bool Equals(object obj) {
                PythonTimeDelta delta = obj as PythonTimeDelta;
                if (delta == null) return false;

                return this._days == delta._days && this._seconds == delta._seconds && this._microseconds == delta._microseconds;
            }
            public override int GetHashCode() {
                return this._days ^ this._seconds ^ this._microseconds;
            }

            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                if (_days != 0) {
                    sb.Append(_days);
                    if (Math.Abs(_days) == 1)
                        sb.Append(" day, ");
                    else
                        sb.Append(" days, ");
                }

                sb.AppendFormat("{0}:{1:d2}:{2:d2}", TimeSpanWithSeconds.Hours, TimeSpanWithSeconds.Minutes, TimeSpanWithSeconds.Seconds);

                if (_microseconds != 0)
                    sb.AppendFormat(".{0:d6}", _microseconds);

                return sb.ToString();
            }

            #region Rich Comparison Members

            private int CompareTo(object other) {
                PythonTimeDelta delta = other as PythonTimeDelta;
                if (delta == null)
                    throw PythonOps.TypeError("can't compare datetime.timedelta to {0}", DynamicTypeOps.GetName(other));

                int res = this._days - delta._days;
                if (res != 0) return res;

                res = this._seconds - delta._seconds;
                if (res != 0) return res;

                return this._microseconds - delta._microseconds;
            }

            public static bool operator >(PythonTimeDelta self, object other) {
                return self.CompareTo(other) > 0;
            }

            public static bool operator <(PythonTimeDelta self, object other) {
                return self.CompareTo(other) < 0;
            }

            public static bool operator >=(PythonTimeDelta self, object other) {
                return self.CompareTo(other) >= 0;
            }

            public static bool operator <=(PythonTimeDelta self, object other) {
                return self.CompareTo(other) <= 0;
            }

            #endregion

            #region ICodeFormattable Members

            public string ToCodeString(CodeContext context) {
                if (_seconds == 0 && _microseconds == 0) {
                    return String.Format("datetime.timedelta({0})", _days);
                } else if (_microseconds == 0) {
                    return String.Format("datetime.timedelta({0}, {1})", _days, _seconds);
                } else {
                    return String.Format("datetime.timedelta({0}, {1}, {2})", _days, _seconds, _microseconds);
                }
            }

            #endregion
        }

        internal static void ThrowIfInvalid(PythonTimeDelta delta, string funcname) {
            if (delta != null) {
                if (delta._microseconds != 0 || delta._seconds % 60 != 0) {
                    throw PythonOps.ValueError("tzinfo.{0}() must return a whole number of minutes", funcname);
                }

                int minutes = (int)(delta.TimeSpanWithDaysAndSeconds.TotalSeconds / 60);
                if (Math.Abs(minutes) >= 1440) {
                    throw PythonOps.ValueError("tzinfo.{0}() returned {1}; must be in -1439 .. 1439", funcname, minutes);
                }
            }
        }
        internal enum InputKind { Year, Month, Day, Hour, Minute, Second, Microsecond }
        internal static void ValidateInput(InputKind kind, int value) {
            switch (kind) {
                case InputKind.Year:
                    if (value > DateTime.MaxValue.Year || value < DateTime.MinValue.Year) {
                        throw PythonOps.ValueError("year is out of range");
                    }
                    break;
                case InputKind.Month:
                    if (value > 12 || value < 1) {
                        throw PythonOps.ValueError("month must be in 1..12");
                    }
                    break;
                case InputKind.Day:
                    // TODO: changing upper bound
                    if (value > 31 || value < 1) {
                        throw PythonOps.ValueError("day is out of range for month");
                    }
                    break;
                case InputKind.Hour:
                    if (value > 23 || value < 0) {
                        throw PythonOps.ValueError("hour must be in 0..23");
                    }
                    break;
                case InputKind.Minute:
                    if (value > 59 || value < 0) {
                        throw PythonOps.ValueError("minute must be in 0..59");
                    }
                    break;
                case InputKind.Second:
                    if (value > 59 || value < 0) {
                        throw PythonOps.ValueError("second must be in 0..59");
                    }
                    break;
                case InputKind.Microsecond:
                    if (value > 999999 || value < 0) {
                        throw PythonOps.ValueError("microsecond must be in 0..999999");
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
        public class PythonDate : ICodeFormattable {
            private DateTime _dateTime;
            protected PythonDate() { }

            public PythonDate(int year, int month, int day) {
                PythonDateTime.ValidateInput(InputKind.Year, year);
                PythonDateTime.ValidateInput(InputKind.Month, month);
                PythonDateTime.ValidateInput(InputKind.Day, day);

                _dateTime = new DateTime(year, month, day);
            }

            internal PythonDate(DateTime value) {
                _dateTime = value.Date; // no hour, minute, second
            }

            [PythonName("__new__")]
            public static PythonDate Make(CodeContext context, DynamicType cls, int year, int month, int day) {
                if (cls == DynamicHelpers.GetDynamicTypeFromType(typeof(PythonDate))) {
                    return new PythonDate(year, month, day);
                } else {
                    PythonDate date = cls.CreateInstance(context, year, month, day) as PythonDate;
                    if (date == null) throw PythonOps.TypeError("{0} is not a subclass of datetime.date", cls);
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
                return new PythonDate(min._dateTime.AddDays(d - 1));
            }

            [PythonName("fromtimestamp")]
            public static PythonDate FromTimestamp(double timestamp) {
                DateTime dt = new DateTime((long)(timestamp * 1e7));
                return new PythonDate(dt.Year, dt.Month, dt.Day);
            }

            // class attributes
            public static PythonDate min = new PythonDate(new DateTime(1, 1, 1));
            public static PythonDate max = new PythonDate(new DateTime(9999, 12, 31));
            public static PythonTimeDelta resolution = PythonTimeDelta._DayResolution;

            // instance attributes
            public int Year {
                [PythonName("year")]
                get { return _dateTime.Year; }
            }
            public int Month {
                [PythonName("month")]
                get { return _dateTime.Month; }
            }
            public int Day {
                [PythonName("day")]
                get { return _dateTime.Day; }
            }

            protected DateTime InternalDateTime {
                get { return _dateTime; }
                set { _dateTime = value; }
            }

            // supported operations
            public static PythonDate operator +([NotNull]PythonDate self, [NotNull]PythonTimeDelta other) {
                try {
                    return new PythonDate(self._dateTime.AddDays(other.Days));
                } catch {
                    throw PythonOps.OverflowError("date value out of range");
                }
            }
            public static PythonDate operator +([NotNull]PythonTimeDelta other, [NotNull]PythonDate self) {
                try {
                    return new PythonDate(self._dateTime.AddDays(other.Days));
                } catch {
                    throw PythonOps.OverflowError("date value out of range");
                }
            }

            [OperatorMethod, PythonName("__radd__")]
            public object ReverseAdd(PythonTimeDelta delta) { return this + delta; }
            public static PythonDate operator -(PythonDate self, PythonTimeDelta delta) {
                try {
                    return new PythonDate(self._dateTime.AddDays(-1 * delta.Days));
                } catch {
                    throw PythonOps.OverflowError("date value out of range");
                }
            }
            public static PythonTimeDelta operator -(PythonDate self, PythonDate other) {
                TimeSpan ts = self._dateTime - other._dateTime;
                return new PythonTimeDelta(0, ts.TotalSeconds, ts.Milliseconds * 1000);
            }

            [OperatorMethod, PythonName("__nonzero__")]
            public bool NonZero() { return true; }

            [PythonName("__reduce__")]
            public virtual Tuple Reduce() {
                return Tuple.MakeTuple(DynamicHelpers.GetDynamicTypeFromType(this.GetType()), Tuple.MakeTuple(_dateTime.Year, _dateTime.Month, _dateTime.Day));
            }

            [PythonName("__getnewargs__")]
            public static object GetNewArgs(CodeContext context, int year, int month, int day) {
                return Tuple.MakeTuple(PythonDate.Make(context, DynamicHelpers.GetDynamicTypeFromType(typeof(PythonDate)), year, month, day));
            }

            [PythonName("replace")]
            public object Replace() {
                return this;
            }

            // instance methods
            [PythonName("replace")]
            public virtual PythonDate Replace([ParamDictionary]IAttributesCollection dict) {
                int year2 = _dateTime.Year;
                int month2 = _dateTime.Month;
                int day2 = _dateTime.Day;

                foreach (KeyValuePair<object, object> kvp in (IDictionary<object, object>)dict) {
                    string strVal = kvp.Key as string;
                    if (strVal == null) continue;

                    switch (strVal) {
                        case "year": year2 = Converter.ConvertToInt32(kvp.Value); break;
                        case "month": month2 = Converter.ConvertToInt32(kvp.Value); break;
                        case "day": day2 = Converter.ConvertToInt32(kvp.Value); break;
                        default: throw PythonOps.TypeError("{0} is an invalid keyword argument for this function", kvp.Key);
                    }
                }

                return new PythonDate(year2, month2, day2);
            }

            [PythonName("timetuple")]
            public virtual object GetTimeTuple() {
                return PythonTime.GetDateTimeTuple(_dateTime);
            }

            [PythonName("toordinal")]
            public int ToOrdinal() {
                return (_dateTime - min._dateTime).Days + 1;
            }

            [PythonName("weekday")]
            public int Weekday() { return PythonTime.Weekday(_dateTime); }

            [PythonName("isoweekday")]
            public int IsoWeekday() { return PythonTime.IsoWeekday(_dateTime); }

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
                DateTime firstDayOfLastIsoYear = FirstDayOfIsoYear(_dateTime.Year - 1);
                DateTime firstDayOfThisIsoYear = FirstDayOfIsoYear(_dateTime.Year);
                DateTime firstDayOfNextIsoYear = FirstDayOfIsoYear(_dateTime.Year + 1);

                int year, days;
                if (firstDayOfThisIsoYear <= _dateTime && _dateTime < firstDayOfNextIsoYear) {
                    year = _dateTime.Year;
                    days = (_dateTime - firstDayOfThisIsoYear).Days;
                } else if (_dateTime < firstDayOfThisIsoYear) {
                    year = _dateTime.Year - 1;
                    days = (_dateTime - firstDayOfLastIsoYear).Days;
                } else {
                    year = _dateTime.Year + 1;
                    days = (_dateTime - firstDayOfNextIsoYear).Days;
                }

                return Tuple.MakeTuple(year, days / 7 + 1, days % 7 + 1);
            }

            [PythonName("isoformat")]
            public string IsoFormat() {
                return _dateTime.ToString("yyyy-MM-dd");
            }

            public override string ToString() {
                return IsoFormat();
            }

            [PythonName("ctime")]
            public string GetCTime() {
                return _dateTime.ToString("ddd MMM ") + string.Format("{0,2}", _dateTime.Day) + _dateTime.ToString(" HH:mm:ss yyyy");
            }

            [PythonName("strftime")]
            public string Format(string dateFormat) {
                return PythonTime.FormatTime(dateFormat, _dateTime);
            }

            public override bool Equals(object obj) {
                if (obj == null) return false;

                if (obj.GetType() == typeof(PythonDate)) {
                    PythonDate other = (PythonDate)obj;
                    return this._dateTime == other._dateTime;
                } else {
                    return false;
                }
            }

            public override int GetHashCode() {
                return _dateTime.GetHashCode();
            }

            #region Rich Comparison Members

            protected virtual int CompareTo(object other) {
                if (other == null)
                    throw PythonOps.TypeError("can't compare datetime.date to NoneType");

                if (other.GetType() != typeof(PythonDate))
                    throw PythonOps.TypeError("can't compare datetime.date to {0}", DynamicTypeOps.GetName(other));

                PythonDate date = other as PythonDate;
                return this._dateTime.CompareTo(date._dateTime);
            }

            public static bool operator >(PythonDate self, object other) {
                return self.CompareTo(other) > 0;
            }

            public static bool operator <(PythonDate self, object other) {
                return self.CompareTo(other) < 0;
            }

            public static bool operator >=(PythonDate self, object other) {
                return self.CompareTo(other) >= 0;
            }

            public static bool operator <=(PythonDate self, object other) {
                return self.CompareTo(other) <= 0;
            }

            [OperatorMethod, PythonName("__eq__")]
            public bool RichEquals(object other) {
                return Equals(other);
            }

            [OperatorMethod, PythonName("__ne__")]
            public bool RichNotEquals(object other) {
                return !Equals(other);
            }

            #endregion

            #region ICodeFormattable Members

            public virtual string ToCodeString(CodeContext context) {
                return string.Format("datetime.date({0}, {1}, {2})", _dateTime.Year, _dateTime.Month, _dateTime.Day);
            }

            #endregion
        }

        [PythonType("datetime")]
        public class PythonDateTimeCombo : PythonDate {
            internal int _lostMicroseconds;
            internal PythonTimeZoneInformation _tz;

            private UnifiedDateTime _utcDateTime;

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

                InternalDateTime = new DateTime(year, month, day, hour, minute, second, microsecond / 1000);
                _lostMicroseconds = microsecond % 1000;
                _tz = tzinfo;
            }

            internal PythonDateTimeCombo(DateTime dt, int lostMicroseconds, PythonTimeZoneInformation tzinfo) {
                this.InternalDateTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                this._lostMicroseconds = dt.Millisecond * 1000 + lostMicroseconds;
                this._tz = tzinfo;

                // make sure both are positive, and lostMicroseconds < 1000
                if (_lostMicroseconds < 0) {
                    try {
                        InternalDateTime = InternalDateTime.AddMilliseconds(_lostMicroseconds / 1000 - 1);
                    } catch {
                        throw PythonOps.OverflowError("date value out of range");
                    }
                    _lostMicroseconds = _lostMicroseconds % 1000 + 1000;
                }

                if (_lostMicroseconds > 999) {
                    try {
                        InternalDateTime = InternalDateTime.AddMilliseconds(_lostMicroseconds / 1000);
                    } catch {
                        throw PythonOps.OverflowError("date value out of range");
                    }
                    _lostMicroseconds = _lostMicroseconds % 1000;
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
                get { return InternalDateTime.Hour; }
            }

            public int Minute {
                [PythonName("minute")]
                get { return InternalDateTime.Minute; }
            }

            public int Second {
                [PythonName("second")]
                get { return InternalDateTime.Second; }
            }

            public int Microsecond {
                [PythonName("microsecond")]
                get { return InternalDateTime.Millisecond * 1000 + _lostMicroseconds; }
            }

            public object TimeZoneInformation {
                [PythonName("tzinfo")]
                get { return _tz; }
            }

            private UnifiedDateTime UtcDateTime {
                get {
                    if (_utcDateTime == null) {
                        _utcDateTime = new UnifiedDateTime();

                        _utcDateTime.DateTime = InternalDateTime;
                        _utcDateTime.LostMicroseconds = _lostMicroseconds;

                        PythonTimeDelta delta = this.GetUtcOffset();
                        if (delta != null) {
                            PythonDateTimeCombo utced = this - delta;
                            _utcDateTime.DateTime = utced.InternalDateTime;
                            _utcDateTime.LostMicroseconds = utced._lostMicroseconds;
                        }
                    }
                    return _utcDateTime;
                }
            }

            // supported operations
            public static PythonDateTimeCombo operator +([NotNull]PythonDateTimeCombo date, [NotNull]PythonTimeDelta delta) {
                return new PythonDateTimeCombo(date.InternalDateTime.Add(delta.TimeSpanWithDaysAndSeconds), delta._microseconds + date._lostMicroseconds, date._tz);
            }
            public static PythonDateTimeCombo operator +([NotNull]PythonTimeDelta delta, [NotNull]PythonDateTimeCombo date) {
                return new PythonDateTimeCombo(date.InternalDateTime.Add(delta.TimeSpanWithDaysAndSeconds), delta._microseconds + date._lostMicroseconds, date._tz);
            }
            [OperatorMethod, PythonName("__radd__")]
            public new PythonDateTimeCombo ReverseAdd(PythonTimeDelta delta) { return this + delta; }
            public static PythonDateTimeCombo operator -(PythonDateTimeCombo date, PythonTimeDelta delta) {
                return new PythonDateTimeCombo(date.InternalDateTime.Subtract(delta.TimeSpanWithDaysAndSeconds), date._lostMicroseconds - delta._microseconds, date._tz);
            }

            public static PythonTimeDelta operator -(PythonDateTimeCombo date, PythonDateTimeCombo other) {
                if (CheckTzInfoBeforeCompare(date, other)) {
                    return new PythonTimeDelta(date.InternalDateTime - other.InternalDateTime, date._lostMicroseconds - other._lostMicroseconds);
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
                return new PythonDateTimeTime(Hour, Minute, Second, Microsecond, _tz);
            }

            [PythonName("replace")]
            [Documentation("gets a new datetime object with the fields provided as keyword arguments replaced.")]
            public override PythonDate Replace([ParamDictionary]IAttributesCollection dict) {
                int year = Year;
                int month = Month;
                int day = Day;
                int hour = Hour;
                int minute = Minute;
                int second = Second;
                int microsecond = Microsecond;
                PythonTimeZoneInformation tz = _tz;

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
                        default:
                            throw PythonOps.TypeError("{0} is an invalid keyword argument for this function", kvp.Key);
                    }
                }
                return new PythonDateTimeCombo(year, month, day, hour, minute, second, microsecond, tz);
            }

            [PythonName("astimezone")]
            public object AsTimeZone(PythonTimeZoneInformation tz) {
                if (tz == null)
                    throw PythonOps.TypeError("astimezone() argument 1 must be datetime.tzinfo, not None");

                if (_tz == null)
                    throw PythonOps.ValueError("astimezone() cannot be applied to a naive datetime");

                if (tz == _tz)
                    return this;

                PythonDateTimeCombo utc = this - GetUtcOffset();
                utc._tz = tz;
                return tz.FromUtc(utc);
            }

            [PythonName("utcoffset")]
            public PythonTimeDelta GetUtcOffset() {
                if (_tz == null) return null;
                PythonTimeDelta delta = _tz.UtcOffset(this);
                PythonDateTime.ThrowIfInvalid(delta, "utcoffset");
                return delta;
            }

            [PythonName("dst")]
            public PythonTimeDelta dst() {
                if (_tz == null) return null;
                PythonTimeDelta delta = _tz.DaylightSavingTime(this);
                PythonDateTime.ThrowIfInvalid(delta, "dst");
                return delta;
            }

            [PythonName("tzname")]
            public object GetTimeZoneName() {
                if (_tz == null) return null;
                return _tz.TimeZoneName(this);
            }

            [PythonName("timetuple")]
            public override object GetTimeTuple() {
                return PythonTime.GetDateTimeTuple(InternalDateTime, _tz);
            }

            [PythonName("utctimetuple")]
            public object GetUtcTimeTuple() {
                if (_tz == null)
                    return PythonTime.GetDateTimeTuple(InternalDateTime, null, true);
                else {
                    PythonDateTimeCombo dtc = this - GetUtcOffset();
                    return PythonTime.GetDateTimeTuple(dtc.InternalDateTime, null, true);
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
                if (self._tz != other._tz) {
                    PythonTimeDelta offset1 = self.GetUtcOffset();
                    PythonTimeDelta offset2 = other.GetUtcOffset();

                    if ((offset1 == null && offset2 != null) || (offset1 != null && offset2 == null))
                        throw PythonOps.TypeError("can't compare offset-naive and offset-aware times");

                    return false;
                } else {
                    return true; // has the same TzInfo, Utcoffset will be skipped
                }
            }

            public override bool Equals(object obj) {
                PythonDateTimeCombo other = obj as PythonDateTimeCombo;
                if (other == null) return false;

                if (CheckTzInfoBeforeCompare(this, other)) {
                    return this.InternalDateTime.Equals(other.InternalDateTime) && this._lostMicroseconds == other._lostMicroseconds;
                } else {
                    // hack
                    TimeSpan delta = this.InternalDateTime - other.InternalDateTime;
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

            protected override int CompareTo(object other) {
                if (other == null)
                    throw PythonOps.TypeError("can't compare datetime.datetime to NoneType");

                PythonDateTimeCombo combo = other as PythonDateTimeCombo;
                if (combo == null)
                    throw PythonOps.TypeError("can't compare datetime.datetime to {0}", DynamicTypeOps.GetName(other));

                if (CheckTzInfoBeforeCompare(this, combo)) {
                    int res = this.InternalDateTime.CompareTo(combo.InternalDateTime);

                    if (res != 0) return res;

                    return this._lostMicroseconds - combo._lostMicroseconds;
                } else {
                    TimeSpan delta = this.InternalDateTime - combo.InternalDateTime;
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

            public override string ToCodeString(CodeContext context) {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("datetime.datetime({0}, {1}, {2}, {3}, {4}",
                    InternalDateTime.Year,
                    InternalDateTime.Month,
                    InternalDateTime.Day,
                    InternalDateTime.Hour,
                    InternalDateTime.Minute);

                if (Microsecond != 0) {
                    sb.AppendFormat(", {0}, {1}", Second, Microsecond);
                } else {
                    if (Second != 0) {
                        sb.AppendFormat(", {0}", Second);
                    }
                }

                if (_tz != null) {
                    sb.AppendFormat(", tzinfo={0}", _tz.TimeZoneName(this).ToLower());
                }
                sb.AppendFormat(")");
                return sb.ToString();
            }
            #endregion

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
        }

        [PythonType("time")]
        public class PythonDateTimeTime : ICodeFormattable {
            internal TimeSpan _timeSpan;
            internal int _lostMicroseconds;
            internal PythonTimeZoneInformation _tz;

            private UnifiedTime _utcTime;

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
                this._timeSpan = new TimeSpan(0, hour, minute, second, microsecond / 1000);
                this._lostMicroseconds = microsecond % 1000;
                this._tz = tzinfo;
            }

            internal PythonDateTimeTime(TimeSpan timeSpan, int lostMicroseconds, PythonTimeZoneInformation tzinfo) {
                this._timeSpan = timeSpan;
                this._lostMicroseconds = lostMicroseconds;
                this._tz = tzinfo;
            }

            // class attributes:
            public static object max = new PythonDateTimeTime(23, 59, 59, 999999, null);
            public static object min = new PythonDateTimeTime(0, 0, 0, 0, null);
            public static object resolution = PythonTimeDelta.resolution;

            // instance attributes:
            public int Hour {
                [PythonName("hour")]
                get { return _timeSpan.Hours; }
            }

            public int Minute {
                [PythonName("minute")]
                get { return _timeSpan.Minutes; }
            }

            public int Second {
                [PythonName("second")]
                get { return _timeSpan.Seconds; }
            }

            public int Microsecond {
                [PythonName("microsecond")]
                get { return _timeSpan.Milliseconds * 1000 + _lostMicroseconds; }
            }

            public PythonTimeZoneInformation TimeZoneInfo {
                [PythonName("tzinfo")]
                get { return _tz; }
            }

            private UnifiedTime UtcTime {
                get {
                    if (_utcTime == null) {
                        _utcTime = new UnifiedTime();

                        _utcTime.TimeSpan = _timeSpan;
                        _utcTime.LostMicroseconds = _lostMicroseconds;

                        PythonTimeDelta delta = this.GetUtcOffset();
                        if (delta != null) {
                            PythonDateTimeTime utced = this - delta;
                            _utcTime.TimeSpan = utced._timeSpan;
                            _utcTime.LostMicroseconds = utced._lostMicroseconds;
                        }
                    }
                    return _utcTime;
                }
            }

            // supported operations
            public static PythonDateTimeTime operator +(PythonDateTimeTime date, PythonTimeDelta delta) {
                return new PythonDateTimeTime(date._timeSpan.Add(delta.TimeSpanWithDaysAndSeconds), delta._microseconds + date._lostMicroseconds, date._tz);
            }

            public static PythonDateTimeTime operator -(PythonDateTimeTime date, PythonTimeDelta delta) {
                return new PythonDateTimeTime(date._timeSpan.Subtract(delta.TimeSpanWithDaysAndSeconds), date._lostMicroseconds - delta._microseconds, date._tz);
            }

            [OperatorMethod, PythonName("__nonzero__")]
            public bool NonZero() {
                return this.UtcTime.TimeSpan.Ticks != 0 || this.UtcTime.LostMicroseconds != 0;
            }

            // instance methods
            [PythonName("replace")]
            public object Replace() {
                return this;
            }

            [PythonName("replace")]
            public object Replace([ParamDictionary]IAttributesCollection dict) {
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
                    new DateTime(1900, 1, 1, _timeSpan.Hours, _timeSpan.Minutes, _timeSpan.Seconds, _timeSpan.Milliseconds));
            }

            [PythonName("utcoffset")]
            public PythonTimeDelta GetUtcOffset() {
                if (_tz == null) return null;
                PythonTimeDelta delta = _tz.UtcOffset(null);
                PythonDateTime.ThrowIfInvalid(delta, "utcoffset");
                return delta;
            }

            [PythonName("dst")]
            public object DaylightSavingsTime() {
                if (_tz == null) return null;
                PythonTimeDelta delta = _tz.DaylightSavingTime(null);
                PythonDateTime.ThrowIfInvalid(delta, "dst");
                return delta;
            }

            [PythonName("tzname")]
            public object TimeZoneName() {
                if (_tz == null) return null;
                return _tz.TimeZoneName(null);
            }

            public override int GetHashCode() {
                return this.UtcTime.GetHashCode();
            }

            internal static bool CheckTzInfoBeforeCompare(PythonDateTimeTime self, PythonDateTimeTime other) {
                if (self._tz != other._tz) {
                    PythonTimeDelta offset1 = self.GetUtcOffset();
                    PythonTimeDelta offset2 = other.GetUtcOffset();

                    if ((offset1 == null && offset2 != null) || (offset1 != null && offset2 == null))
                        throw PythonOps.TypeError("can't compare offset-naive and offset-aware times");

                    return false;
                } else {
                    return true; // has the same TzInfo, Utcoffset will be skipped
                }
            }

            public override bool Equals(object obj) {
                PythonDateTimeTime other = obj as PythonDateTimeTime;
                if (other == null) return false;

                if (CheckTzInfoBeforeCompare(this, other)) {
                    return this._timeSpan == other._timeSpan && this._lostMicroseconds == other._lostMicroseconds;
                } else {
                    return this.UtcTime.Equals(other.UtcTime);
                }
            }

            #region Rich Comparison Members

            /// <summary>
            /// Helper function for doing the comparisons.  time has no __cmp__ method
            /// </summary>
            private int CompareTo(object other) {
                PythonDateTimeTime other2 = other as PythonDateTimeTime;
                if (other2 == null)
                    throw PythonOps.TypeError("can't compare datetime.time to {0}", DynamicTypeOps.GetName(other));

                if (CheckTzInfoBeforeCompare(this, other2)) {
                    int res = this._timeSpan.CompareTo(other2._timeSpan);
                    if (res != 0) return res;
                    return this._lostMicroseconds - other2._lostMicroseconds;
                } else {
                    return this.UtcTime.CompareTo(other2.UtcTime);
                }
            }

            public static bool operator >(PythonDateTimeTime self, object other) {
                return self.CompareTo(other) > 0;
            }

            public static bool operator <(PythonDateTimeTime self, object other) {
                return self.CompareTo(other) < 0;
            }

            public static bool operator >=(PythonDateTimeTime self, object other) {
                return self.CompareTo(other) >= 0;
            }

            public static bool operator <=(PythonDateTimeTime self, object other) {
                return self.CompareTo(other) <= 0;
            }

            #endregion

            #region ICodeFormattable Members

            public string ToCodeString(CodeContext context) {
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
        }

        [PythonType("tzinfo")]
        public class PythonTimeZoneInformation {
            [PythonName("fromutc")]
            public virtual object FromUtc(PythonDateTimeCombo dt) {
                PythonTimeDelta dtOffset = UtcOffset(dt);
                if (dtOffset == null)
                    throw PythonOps.ValueError("fromutc: non-None utcoffset() result required");

                PythonTimeDelta dtDst = DaylightSavingTime(dt);
                if (dtDst == null)
                    throw PythonOps.ValueError("fromutc: non-None dst() result required");

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
