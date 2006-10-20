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

        [PythonType("date")]
        public class PythonDate {
            private DateTime _value;

            public static object min = new PythonDate(DateTime.MinValue);
            public static object max = new PythonDate(DateTime.MaxValue);

            public PythonDate(int year, int month, int day) {
                _value = new DateTime(year, month, day);
            }

            internal PythonDate(DateTime value) {
                _value = value;
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

            [PythonName("today")]
            public static object Today() {
                return new PythonDate(DateTime.Today);
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

            [PythonName("strftime")]
            public string Format(string dateFormat) {
                return PythonTime.FormatTime(dateFormat, _value);
            }

            [PythonName("__str__")]
            public override string ToString() {
                return string.Format("datetime.date({0}, {1}, {2})", _value.Year, _value.Month, _value.Day);
            }

            #region IRichComparable Members

            [PythonName("__cmp__")]
            public object CompareTo(object other) {
                PythonDate time = other as PythonDate;
                if (time == null) return Ops.NotImplemented;

                return this._value.CompareTo(time._value);
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
        }

        [PythonType("time")]
        public class PythonDateTimeTime : ICodeFormattable, IRichComparable {
            TimeSpan time;
            int lostMicroseconds;
            PythonTimeZoneInformation tz;

            public PythonDateTimeTime([DefaultParameterValue(0)]int hour,
                [DefaultParameterValue(0)]int minute,
                [DefaultParameterValue(0)]int second,
                [DefaultParameterValue(0)]int microsecond,
                [DefaultParameterValue(null)]PythonTimeZoneInformation tzInfo) {
                time = new TimeSpan(0, hour, minute, second, microsecond / 1000);
                lostMicroseconds = microsecond % 1000;
                tz = tzInfo;
            }

            internal PythonDateTimeTime(TimeSpan timeSpan, int lostMicroseconds) {
                time = timeSpan;
                this.lostMicroseconds = lostMicroseconds;
            }

            public static object max = new PythonDateTimeTime(23, 59, 59, 999999, null);
            public static object min = new PythonDateTimeTime(0, 0, 0, 0, null);
            public static object resolution = new PythonDateTimeTime(0, 0, 0, 1, null);

            [PythonName("dst")]
            public object DaylightSavingsTime() {
                //Return self.tzinfo.dst(self).
                if (tz == null) return null;

                return tz.DaylightSavingsTime(this);
            }

            [PythonName("isoformat")]
            public object GetIsoFormat() {
                //Return string in ISO 8601 format, HH:MM:SS[.mmmmmm][+HH:MM].

                return String.Format("{0,##}:{1,##}:{2,##}", time.Hours, time.Seconds, time.Minutes);
            }

            [PythonName("replace")]
            public object Replace() {
                return new PythonDateTimeTime(time, lostMicroseconds);
            }
            

            [PythonName("replace")]
            public object Replace([ParamDict]Dict dict) {
                //Return time with new specified fields.
                TimeSpan res = time;
                int ms = lostMicroseconds;

                foreach (KeyValuePair<object, object> kvp in (IDictionary<object, object>)dict) {
                    string key = kvp.Key as string;
                    if (key == null) continue;

                    switch (key) {
                        case "hour":
                            
                            res = new TimeSpan(0, (int)kvp.Value, res.Minutes, res.Seconds, res.Milliseconds);
                            break;
                        case "minute":
                            res = new TimeSpan(0, res.Hours, (int)kvp.Value, res.Seconds, res.Milliseconds);
                            break;
                        case "second":
                            res = new TimeSpan(0, res.Hours, res.Minutes, (int)kvp.Value, res.Milliseconds);
                            break;
                        case "microsecond":
                            res = new TimeSpan(0, res.Hours, res.Minutes, res.Seconds, (int)kvp.Value / 1000);
                            ms = (int)kvp.Value % 1000;
                            break;
                    }
                }
                return new PythonDateTimeTime(res, ms);
            }

            [PythonName("strftime")]
            public object FormatTime(string format) {
                return PythonTime.FormatTime(format,
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, time.Hours, time.Minutes, time.Seconds, time.Milliseconds));
            }

            [PythonName("tzname")]
            public object GetTimeZoneName() {
                if (tz == null) return String.Empty;

                return tz.TimeZoneName(this);
            }

            [PythonName("utcoffset")]
            public object GetUtcOffset() {
                if (tz == null) return 0;

                return tz.UtfOffset(this);
            }

            public int Hour {
                [PythonName("hour")]
                get {
                    return time.Hours;
                }
            }

            public int Minute {
                [PythonName("minute")]
                get {
                    return time.Minutes;
                }
            }

            public int Second {
                [PythonName("second")]
                get {
                    return time.Seconds;
                }
            }

            public int Microsecond {
                [PythonName("microsecond")]
                get {
                    return time.Milliseconds * 1000 + lostMicroseconds;
                }
            }

            public override bool Equals(object obj) {
                PythonDateTimeTime time = obj as PythonDateTimeTime;
                if (time == null) return false;

                return this.time == time.time &&
                    tz == time.tz &&
                    lostMicroseconds == time.lostMicroseconds;
            }

            public override int GetHashCode() {
                return time.GetHashCode() ^ ((tz == null) ? 0 : tz.GetHashCode()) ^ lostMicroseconds;
            }

            public override string ToString() {
                if (Microsecond != 0)
                    return String.Format("{0,##}:{1,))}:{2,##}:{3,##}", Hour, Minute, Second, Microsecond);

                return String.Format("{0,))}:{1,##}:{2,##}", Hour, Minute, Second);
            }

            public static PythonDateTimeTime operator +(PythonDateTimeTime date, PythonTimeDelta delta) {
                return new PythonDateTimeTime(date.time.Add(delta.timeSpan), delta.lostMicroseconds + date.lostMicroseconds);
            }

            public static PythonDateTimeTime operator -(PythonDateTimeTime date, PythonTimeDelta delta) {
                return new PythonDateTimeTime(date.time.Subtract(delta.timeSpan), delta.lostMicroseconds - date.lostMicroseconds);
            }

            #region IRichComparable Members

            [PythonName("__cmp__")]
            public object CompareTo(object other) {
                PythonDateTimeTime time = other as PythonDateTimeTime;
                if (time == null) return Ops.NotImplemented;

                int res = this.time.CompareTo(time.time);

                if (res == 0) {
                    res = lostMicroseconds - time.lostMicroseconds;
                    if (res == 0)
                        return Ops.Compare(this.tz, time.tz);
                }

                return res;
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
                if (Microsecond != 0)
                    return String.Format("datetime.time({0}, {1}, {2}, {3})", Hour, Minute, Second, Microsecond);

                return String.Format("datetime.time({0}, {1}, {2})", Hour, Minute, Second);
            }

            #endregion
        }

        [PythonType("datetime")]
        public class PythonDateTimeCombo : IRichComparable, ICodeFormattable {
            internal DateTime dateTime;
            internal int lostMicroseconds;
            private PythonTimeZoneInformation tz;

            public static object max = new PythonDateTimeCombo(DateTime.MaxValue, 0);
            public static object min = new PythonDateTimeCombo(DateTime.MinValue, 0);
            public static object resolution = new PythonDateTimeCombo(DateTime.MinValue, 0);

            public PythonDateTimeCombo(int year,
                int month,
                int day,
               [DefaultParameterValue(1)]int hour,
               [DefaultParameterValue(0)]int minute,
               [DefaultParameterValue(0)]int second,
               [DefaultParameterValue(0)]int microsecond,
               [DefaultParameterValue(null)]PythonTimeZoneInformation tzinfo) {

                dateTime = new DateTime(year, month, day, hour, minute, second, microsecond / 1000);
                lostMicroseconds = microsecond % 1000;
                tz = tzinfo;
            }

            internal PythonDateTimeCombo(DateTime dt, int lostMicroseconds) {
                dateTime = dt;
                this.lostMicroseconds = lostMicroseconds;
            }

            [PythonName("now")]
            public static object Now([DefaultParameterValue(null)]PythonTimeZoneInformation tz) {
                if (tz == null) return new PythonDateTimeCombo(DateTime.Now, 0);

                return tz.FromUtc(new PythonDateTimeCombo(DateTime.Now.ToUniversalTime(), 0));
            }

            [PythonName("today")]
            public static object Today([DefaultParameterValue(null)]PythonTimeZoneInformation tz) {
                if (tz == null) return new PythonDateTimeCombo(DateTime.Today, 0);

                return tz.FromUtc(new PythonDateTimeCombo(DateTime.Today.ToUniversalTime(), 0));
            }

            [PythonName("strftime")]
            public object FormatTime(string format) {
                return PythonTime.FormatTime(format, dateTime);
            }

            [PythonName("utcnow")]
            public static object UtcNow() {
                return new PythonDateTimeCombo(DateTime.Now.ToUniversalTime(), 0);
            }

            [PythonName("astimezone")]
            public object AsTimeZone(PythonTimeZoneInformation tz) {
                return tz.FromUtc(new PythonDateTimeCombo(dateTime.ToUniversalTime(), lostMicroseconds));
            }

            [PythonName("ctime")]
            [Documentation("converts the time into a string DOW Mon ## hh:mm:ss year")]
            public object ToCTime() {
                return dateTime.ToString("ddd MMM d hh:mm:ss yyyy");
            }

            [PythonName("date")]
            public PythonDate ToDate() {
                return new PythonDate(dateTime.Year, dateTime.Month, dateTime.Day);
            }

            [PythonName("dst")]
            public object dst() {
                //Return self.tzinfo.dst(self).
                if (tz == null) return null;

                return tz.DaylightSavingsTime(this);
            }

            [PythonName("fromtimestamp")]
            public static object FromTimeStamp(long timestamp) {
                return new PythonDateTimeCombo(DateTime.FromFileTime(timestamp), 0);
            }

            [PythonName("isoformat")]
            public string IsoFormat() {
                // ISO 8601 format, "yyyy'-'MM'-'dd'T'HH':'mm':'ss". 

                return dateTime.ToString("s");
            }

            [PythonName("replace")]
            [Documentation("gets a new datetime object with the fields provided as keyword arguments replaced.")]
            public object Replace([ParamDict]Dict dict) {
                DateTime curTime = dateTime;
                int ms = lostMicroseconds;
                foreach (KeyValuePair<object, object> kvp in (IDictionary<object, object>)dict) {
                    string key = kvp.Key as string;
                    if (key == null) continue;

                    switch (key) {
                        case "year":
                            curTime = new DateTime((int)kvp.Value, curTime.Month, curTime.Day, curTime.Hour, curTime.Minute, curTime.Second, curTime.Millisecond);
                            break;
                        case "month":
                            curTime = new DateTime(curTime.Year, (int)kvp.Value, curTime.Day, curTime.Hour, curTime.Minute, curTime.Second, curTime.Millisecond);
                            break;
                        case "day":
                            curTime = new DateTime(curTime.Year, curTime.Month, (int)kvp.Value, curTime.Hour, curTime.Minute, curTime.Second, curTime.Millisecond);
                            break;
                        case "hour":
                            curTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, (int)kvp.Value, curTime.Minute, curTime.Second, curTime.Millisecond);
                            break;
                        case "minute":
                            curTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, curTime.Hour, (int)kvp.Value, curTime.Second, curTime.Millisecond);
                            break;
                        case "second":
                            curTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, curTime.Hour, curTime.Minute, (int)kvp.Value, curTime.Millisecond);
                            break;
                        case "microseconds":
                            curTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, curTime.Hour, curTime.Minute, curTime.Second, (int)kvp.Value / 1000);
                            ms = (int)kvp.Value % 1000;
                            break;
                    }
                }
                return new PythonDateTimeCombo(curTime, ms);
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
                    null);
            }

            [PythonName("time")]
            [Documentation("gets the datetime w/o the time zone component")]
            public PythonDateTimeTime GetTime() {
                return new PythonDateTimeTime(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond * 1000, null);
            }

            [PythonName("timetuple")]
            public object GetTimeTuple() {
                return PythonTime.GetDateTimeTuple(dateTime);
            }

            [PythonName("timetz")]
            public object GetTimeWithTimeZone() {
                //Return time object with same time and tzinfo.
                return new PythonDateTimeTime(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond * 1000, tz);
            }

            [PythonName("tzname")]
            public object GetTimeZoneName() {
                if (tz == null) return String.Empty;
                return tz.StandardName;
            }

            public object TimeZoneInformation {
                [PythonName("tzinfo")]
                get {
                    return tz;
                }
            }

            [PythonName("utcoffset")]
            public object GetUtcOffset() {
                if (tz == null) return null;
                return tz.UtfOffset(this);
            }

            [PythonName("utctimetuple")]
            public object GetUtcTimeTuple() {
                // Return UTC time tuple, compatible with time.localtime().
                if (tz == null) return GetTimeTuple();

                return new PythonDateTimeCombo(tz.ToUniversalTime(dateTime), lostMicroseconds).GetTimeTuple();
            }

            [PythonName("weekday")]
            public object WeekDay() {
                return (int)dateTime.DayOfWeek;
            }

            public object Year {
                [PythonName("year")]
                get {
                    return dateTime.Year;
                }
            }

            public object Month {
                [PythonName("month")]
                get {
                    return dateTime.Month;
                }
            }

            public object Day {
                [PythonName("day")]
                get {
                    return dateTime.Day;
                }
            }


            public object Hour {
                [PythonName("hour")]
                get {
                    return dateTime.Hour;
                }
            }

            public object Minute {
                [PythonName("minute")]
                get {
                    return dateTime.Minute;
                }
            }

            public object Second {
                [PythonName("second")]
                get {
                    return dateTime.Second;
                }
            }

            public object MicroSecond {
                [PythonName("microsecond")]
                get {
                    return dateTime.Millisecond * 1000 + lostMicroseconds;
                }
            }

            public static PythonDateTimeCombo operator +(PythonDateTimeCombo date, PythonTimeDelta delta) {
                return new PythonDateTimeCombo(date.dateTime.Add(delta.timeSpan), delta.lostMicroseconds + date.lostMicroseconds);
            }

            public static PythonDateTimeCombo operator -(PythonDateTimeCombo date, PythonTimeDelta delta) {
                return new PythonDateTimeCombo(date.dateTime.Subtract(delta.timeSpan), delta.lostMicroseconds - date.lostMicroseconds);
            }

            public static PythonTimeDelta operator -(PythonDateTimeCombo date, PythonDateTimeCombo other) {                
                return new PythonTimeDelta(date.dateTime.Subtract(other.dateTime), date.lostMicroseconds - other.lostMicroseconds);
            }

            public override bool Equals(object obj) {
                PythonDateTimeCombo pdtc = obj as PythonDateTimeCombo;
                if (pdtc == null) return false;

                return this.dateTime == pdtc.dateTime && this.tz == pdtc.tz;
            }

            public override int GetHashCode() {
                return dateTime.GetHashCode() ^ (tz == null ? 0 : tz.GetHashCode());
            }

            public override string ToString() {
                return IsoFormat();
            }

            #region IRichComparable Members

            [PythonName("__cmp__")]
            public object CompareTo(object other) {
                PythonDateTimeCombo combo = other as PythonDateTimeCombo;
                if (combo == null) return Ops.NotImplemented;

                int res = this.dateTime.CompareTo(combo.dateTime);

                if (res == 0)
                    return Ops.Compare(this.tz, combo.tz);

                return res;
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
                return String.Format("datetime.datetime({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                    dateTime.Year,
                    dateTime.Month,
                    dateTime.Day,
                    dateTime.Hour,
                    dateTime.Minute,
                    dateTime.Second,
                    dateTime.Millisecond * 1000);
            }

            #endregion
        }

        [PythonType("timedelta")]
        public class PythonTimeDelta : IRichComparable, ICodeFormattable {
            internal TimeSpan timeSpan;
            internal int lostMicroseconds;

            public static object resolution = new PythonTimeDelta(new TimeSpan(1));
            public static object minimum = new PythonTimeDelta(TimeSpan.MinValue);
            public static object maximum = new PythonTimeDelta(TimeSpan.MaxValue);

            public PythonTimeDelta([DefaultParameterValue(0D)]double days,
                [DefaultParameterValue(0D)]double seconds,
                [DefaultParameterValue(0D)]double microseconds)

                : this(TimeSpan.FromDays(days).Add(
                    TimeSpan.FromSeconds(seconds)).Add(
                    TimeSpan.FromMilliseconds(microseconds / 1000)), (int)(microseconds % 1000)) {
            }

            internal PythonTimeDelta(TimeSpan ts) {
                timeSpan = ts;
            }

            internal PythonTimeDelta(TimeSpan ts, int extramicros) {
                timeSpan = ts;
                while (extramicros < 0) {
                    timeSpan = timeSpan.Subtract(new TimeSpan(0, 0, 0, 0, 1));
                    extramicros = extramicros + 1000;
                }

                while (extramicros > 1000) {
                    timeSpan = timeSpan.Add(new TimeSpan(0, 0, 0, 0, 1));
                    extramicros = extramicros - 1000;
                }

                lostMicroseconds = extramicros;
            }

            public static PythonTimeDelta operator +(PythonTimeDelta self, PythonTimeDelta other) {
                return new PythonTimeDelta(self.timeSpan.Add(other.timeSpan), self.lostMicroseconds + other.lostMicroseconds);
            }

            public static PythonTimeDelta operator -(PythonTimeDelta self, PythonTimeDelta other) {
                return new PythonTimeDelta(self.timeSpan.Subtract(other.timeSpan), self.lostMicroseconds - other.lostMicroseconds);
            }

            public static PythonTimeDelta operator -(PythonTimeDelta self) {
                return new PythonTimeDelta(self.timeSpan.Negate(), -self.lostMicroseconds);
            }

            public static PythonTimeDelta operator +(PythonTimeDelta self) {
                return new PythonTimeDelta(self.timeSpan, -self.lostMicroseconds);
            }

            public object Seconds {
                [PythonName("seconds")]
                get {
                    return timeSpan.Seconds + (timeSpan.Minutes * 60) + (timeSpan.Hours * 60 * 60);
                }
            }

            public object MicroSeconds {
                [PythonName("microseconds")]
                get {
                    return timeSpan.Milliseconds * 1000 + lostMicroseconds;
                }
            }

            public object Days {
                [PythonName("days")]
                get {
                    return timeSpan.Days;
                }
            }

            public override bool Equals(object obj) {
                PythonTimeDelta delta = obj as PythonTimeDelta;
                if (delta == null) return false;

                return this.timeSpan == delta.timeSpan && this.lostMicroseconds == delta.lostMicroseconds;
            }

            public override int GetHashCode() {
                return timeSpan.GetHashCode() ^ (int)lostMicroseconds;
            }

            public override string ToString() {
                StringBuilder res = new StringBuilder();
                if (timeSpan.Days > 0) {
                    res.Append(timeSpan.Days);
                    if (timeSpan.Days == 1) res.Append(" day, ");
                    else res.Append(" days, ");
                }

                res.AppendFormat("{0}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

                return res.ToString();
            }

            #region IRichComparable Members

            [PythonName("__cmp__")]
            public object CompareTo(object other) {
                PythonTimeDelta delta = other as PythonTimeDelta;
                if (delta == null) return Ops.NotImplemented;

                int res = timeSpan.CompareTo(delta.timeSpan);
                if (res == 0) res = this.lostMicroseconds - delta.lostMicroseconds;

                return res;
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
                return String.Format("datetime.timedelta({0},{1},{2})", Days, Seconds, MicroSeconds);
            }

            #endregion
        }

        [PythonType("tzinfo")]
        public class PythonTimeZoneInformation : TimeZone {
            public override TimeSpan GetUtcOffset(DateTime time) {
                return TimeSpan.FromMinutes((int)UtfOffset(new PythonDateTimeCombo(time, 0)));
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
            public virtual object DaylightSavingsTime(object dt) {
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
            public virtual object UtfOffset(object dt) {
                throw new NotImplementedException();
            }


        }
    }
}
