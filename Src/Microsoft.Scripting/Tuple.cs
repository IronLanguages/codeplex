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
using System.Diagnostics;

namespace Microsoft.Scripting {
    public abstract class NewTuple {
        public const int MaxSize = 128;
        public abstract object GetValue(int index);
        public abstract void SetValue(int index, object value);
        /// <summary>
        /// Gets the unbound generic Tuple type which has at lease size slots or null if a large enough tuple is not available.
        /// </summary>
        public static Type GetTupleType(int size) {
            #region Generated Tuple Get From Size

            // *** BEGIN GENERATED CODE ***

            if (size <= 128) {
                if (size <= 1) {
                    return typeof(Tuple<>);
                } else if (size <= 2) {
                    return typeof(Tuple<, >);
                } else if (size <= 4) {
                    return typeof(Tuple<, , , >);
                } else if (size <= 8) {
                    return typeof(Tuple<, , , , , , , >);
                } else if (size <= 16) {
                    return typeof(Tuple<, , , , , , , , , , , , , , , >);
                } else if (size <= 32) {
                    return typeof(Tuple<, , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , >);
                } else if (size <= 64) {
                    return typeof(Tuple<, , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , >);
                } else {
                    return typeof(Tuple<, , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , >);
                }
            }

            // *** END GENERATED CODE ***

            #endregion

            return null;
        }
    }

    #region Generated Tuples

    // *** BEGIN GENERATED CODE ***

    public class Tuple<T0> : NewTuple {
        private T0 _item0;

        public T0 Item000 {
            get { return _item0; }
            set { _item0 = value; }
        }

        public override object GetValue(int index) {
            switch(index) {
                case 0: return Item000;
                default: throw new ArgumentException("index");
            }
        }

        public override void SetValue(int index, object value) {
            switch(index) {
                case 0: Item000 = (T0)value; break;
                default: throw new ArgumentException("index");
            }
        }
    }
    public class Tuple<T0, T1> : Tuple<T0> {
        private T1 _item1;

        public T1 Item001 {
            get { return _item1; }
            set { _item1 = value; }
        }

        public override object GetValue(int index) {
            switch(index) {
                case 0: return Item000;
                case 1: return Item001;
                default: throw new ArgumentException("index");
            }
        }

        public override void SetValue(int index, object value) {
            switch(index) {
                case 0: Item000 = (T0)value; break;
                case 1: Item001 = (T1)value; break;
                default: throw new ArgumentException("index");
            }
        }
    }
    public class Tuple<T0, T1, T2, T3> : Tuple<T0, T1> {
        private T2 _item2;
        private T3 _item3;

        public T2 Item002 {
            get { return _item2; }
            set { _item2 = value; }
        }
        public T3 Item003 {
            get { return _item3; }
            set { _item3 = value; }
        }

        public override object GetValue(int index) {
            switch(index) {
                case 0: return Item000;
                case 1: return Item001;
                case 2: return Item002;
                case 3: return Item003;
                default: throw new ArgumentException("index");
            }
        }

        public override void SetValue(int index, object value) {
            switch(index) {
                case 0: Item000 = (T0)value; break;
                case 1: Item001 = (T1)value; break;
                case 2: Item002 = (T2)value; break;
                case 3: Item003 = (T3)value; break;
                default: throw new ArgumentException("index");
            }
        }
    }
    public class Tuple<T0, T1, T2, T3, T4, T5, T6, T7> : Tuple<T0, T1, T2, T3> {
        private T4 _item4;
        private T5 _item5;
        private T6 _item6;
        private T7 _item7;

        public T4 Item004 {
            get { return _item4; }
            set { _item4 = value; }
        }
        public T5 Item005 {
            get { return _item5; }
            set { _item5 = value; }
        }
        public T6 Item006 {
            get { return _item6; }
            set { _item6 = value; }
        }
        public T7 Item007 {
            get { return _item7; }
            set { _item7 = value; }
        }

        public override object GetValue(int index) {
            switch(index) {
                case 0: return Item000;
                case 1: return Item001;
                case 2: return Item002;
                case 3: return Item003;
                case 4: return Item004;
                case 5: return Item005;
                case 6: return Item006;
                case 7: return Item007;
                default: throw new ArgumentException("index");
            }
        }

        public override void SetValue(int index, object value) {
            switch(index) {
                case 0: Item000 = (T0)value; break;
                case 1: Item001 = (T1)value; break;
                case 2: Item002 = (T2)value; break;
                case 3: Item003 = (T3)value; break;
                case 4: Item004 = (T4)value; break;
                case 5: Item005 = (T5)value; break;
                case 6: Item006 = (T6)value; break;
                case 7: Item007 = (T7)value; break;
                default: throw new ArgumentException("index");
            }
        }
    }
    public class Tuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : Tuple<T0, T1, T2, T3, T4, T5, T6, T7> {
        private T8 _item8;
        private T9 _item9;
        private T10 _item10;
        private T11 _item11;
        private T12 _item12;
        private T13 _item13;
        private T14 _item14;
        private T15 _item15;

        public T8 Item008 {
            get { return _item8; }
            set { _item8 = value; }
        }
        public T9 Item009 {
            get { return _item9; }
            set { _item9 = value; }
        }
        public T10 Item010 {
            get { return _item10; }
            set { _item10 = value; }
        }
        public T11 Item011 {
            get { return _item11; }
            set { _item11 = value; }
        }
        public T12 Item012 {
            get { return _item12; }
            set { _item12 = value; }
        }
        public T13 Item013 {
            get { return _item13; }
            set { _item13 = value; }
        }
        public T14 Item014 {
            get { return _item14; }
            set { _item14 = value; }
        }
        public T15 Item015 {
            get { return _item15; }
            set { _item15 = value; }
        }

        public override object GetValue(int index) {
            switch(index) {
                case 0: return Item000;
                case 1: return Item001;
                case 2: return Item002;
                case 3: return Item003;
                case 4: return Item004;
                case 5: return Item005;
                case 6: return Item006;
                case 7: return Item007;
                case 8: return Item008;
                case 9: return Item009;
                case 10: return Item010;
                case 11: return Item011;
                case 12: return Item012;
                case 13: return Item013;
                case 14: return Item014;
                case 15: return Item015;
                default: throw new ArgumentException("index");
            }
        }

        public override void SetValue(int index, object value) {
            switch(index) {
                case 0: Item000 = (T0)value; break;
                case 1: Item001 = (T1)value; break;
                case 2: Item002 = (T2)value; break;
                case 3: Item003 = (T3)value; break;
                case 4: Item004 = (T4)value; break;
                case 5: Item005 = (T5)value; break;
                case 6: Item006 = (T6)value; break;
                case 7: Item007 = (T7)value; break;
                case 8: Item008 = (T8)value; break;
                case 9: Item009 = (T9)value; break;
                case 10: Item010 = (T10)value; break;
                case 11: Item011 = (T11)value; break;
                case 12: Item012 = (T12)value; break;
                case 13: Item013 = (T13)value; break;
                case 14: Item014 = (T14)value; break;
                case 15: Item015 = (T15)value; break;
                default: throw new ArgumentException("index");
            }
        }
    }
    public class Tuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31> : Tuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> {
        private T16 _item16;
        private T17 _item17;
        private T18 _item18;
        private T19 _item19;
        private T20 _item20;
        private T21 _item21;
        private T22 _item22;
        private T23 _item23;
        private T24 _item24;
        private T25 _item25;
        private T26 _item26;
        private T27 _item27;
        private T28 _item28;
        private T29 _item29;
        private T30 _item30;
        private T31 _item31;

        public T16 Item016 {
            get { return _item16; }
            set { _item16 = value; }
        }
        public T17 Item017 {
            get { return _item17; }
            set { _item17 = value; }
        }
        public T18 Item018 {
            get { return _item18; }
            set { _item18 = value; }
        }
        public T19 Item019 {
            get { return _item19; }
            set { _item19 = value; }
        }
        public T20 Item020 {
            get { return _item20; }
            set { _item20 = value; }
        }
        public T21 Item021 {
            get { return _item21; }
            set { _item21 = value; }
        }
        public T22 Item022 {
            get { return _item22; }
            set { _item22 = value; }
        }
        public T23 Item023 {
            get { return _item23; }
            set { _item23 = value; }
        }
        public T24 Item024 {
            get { return _item24; }
            set { _item24 = value; }
        }
        public T25 Item025 {
            get { return _item25; }
            set { _item25 = value; }
        }
        public T26 Item026 {
            get { return _item26; }
            set { _item26 = value; }
        }
        public T27 Item027 {
            get { return _item27; }
            set { _item27 = value; }
        }
        public T28 Item028 {
            get { return _item28; }
            set { _item28 = value; }
        }
        public T29 Item029 {
            get { return _item29; }
            set { _item29 = value; }
        }
        public T30 Item030 {
            get { return _item30; }
            set { _item30 = value; }
        }
        public T31 Item031 {
            get { return _item31; }
            set { _item31 = value; }
        }

        public override object GetValue(int index) {
            switch(index) {
                case 0: return Item000;
                case 1: return Item001;
                case 2: return Item002;
                case 3: return Item003;
                case 4: return Item004;
                case 5: return Item005;
                case 6: return Item006;
                case 7: return Item007;
                case 8: return Item008;
                case 9: return Item009;
                case 10: return Item010;
                case 11: return Item011;
                case 12: return Item012;
                case 13: return Item013;
                case 14: return Item014;
                case 15: return Item015;
                case 16: return Item016;
                case 17: return Item017;
                case 18: return Item018;
                case 19: return Item019;
                case 20: return Item020;
                case 21: return Item021;
                case 22: return Item022;
                case 23: return Item023;
                case 24: return Item024;
                case 25: return Item025;
                case 26: return Item026;
                case 27: return Item027;
                case 28: return Item028;
                case 29: return Item029;
                case 30: return Item030;
                case 31: return Item031;
                default: throw new ArgumentException("index");
            }
        }

        public override void SetValue(int index, object value) {
            switch(index) {
                case 0: Item000 = (T0)value; break;
                case 1: Item001 = (T1)value; break;
                case 2: Item002 = (T2)value; break;
                case 3: Item003 = (T3)value; break;
                case 4: Item004 = (T4)value; break;
                case 5: Item005 = (T5)value; break;
                case 6: Item006 = (T6)value; break;
                case 7: Item007 = (T7)value; break;
                case 8: Item008 = (T8)value; break;
                case 9: Item009 = (T9)value; break;
                case 10: Item010 = (T10)value; break;
                case 11: Item011 = (T11)value; break;
                case 12: Item012 = (T12)value; break;
                case 13: Item013 = (T13)value; break;
                case 14: Item014 = (T14)value; break;
                case 15: Item015 = (T15)value; break;
                case 16: Item016 = (T16)value; break;
                case 17: Item017 = (T17)value; break;
                case 18: Item018 = (T18)value; break;
                case 19: Item019 = (T19)value; break;
                case 20: Item020 = (T20)value; break;
                case 21: Item021 = (T21)value; break;
                case 22: Item022 = (T22)value; break;
                case 23: Item023 = (T23)value; break;
                case 24: Item024 = (T24)value; break;
                case 25: Item025 = (T25)value; break;
                case 26: Item026 = (T26)value; break;
                case 27: Item027 = (T27)value; break;
                case 28: Item028 = (T28)value; break;
                case 29: Item029 = (T29)value; break;
                case 30: Item030 = (T30)value; break;
                case 31: Item031 = (T31)value; break;
                default: throw new ArgumentException("index");
            }
        }
    }
    public class Tuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60, T61, T62, T63> : Tuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31> {
        private T32 _item32;
        private T33 _item33;
        private T34 _item34;
        private T35 _item35;
        private T36 _item36;
        private T37 _item37;
        private T38 _item38;
        private T39 _item39;
        private T40 _item40;
        private T41 _item41;
        private T42 _item42;
        private T43 _item43;
        private T44 _item44;
        private T45 _item45;
        private T46 _item46;
        private T47 _item47;
        private T48 _item48;
        private T49 _item49;
        private T50 _item50;
        private T51 _item51;
        private T52 _item52;
        private T53 _item53;
        private T54 _item54;
        private T55 _item55;
        private T56 _item56;
        private T57 _item57;
        private T58 _item58;
        private T59 _item59;
        private T60 _item60;
        private T61 _item61;
        private T62 _item62;
        private T63 _item63;

        public T32 Item032 {
            get { return _item32; }
            set { _item32 = value; }
        }
        public T33 Item033 {
            get { return _item33; }
            set { _item33 = value; }
        }
        public T34 Item034 {
            get { return _item34; }
            set { _item34 = value; }
        }
        public T35 Item035 {
            get { return _item35; }
            set { _item35 = value; }
        }
        public T36 Item036 {
            get { return _item36; }
            set { _item36 = value; }
        }
        public T37 Item037 {
            get { return _item37; }
            set { _item37 = value; }
        }
        public T38 Item038 {
            get { return _item38; }
            set { _item38 = value; }
        }
        public T39 Item039 {
            get { return _item39; }
            set { _item39 = value; }
        }
        public T40 Item040 {
            get { return _item40; }
            set { _item40 = value; }
        }
        public T41 Item041 {
            get { return _item41; }
            set { _item41 = value; }
        }
        public T42 Item042 {
            get { return _item42; }
            set { _item42 = value; }
        }
        public T43 Item043 {
            get { return _item43; }
            set { _item43 = value; }
        }
        public T44 Item044 {
            get { return _item44; }
            set { _item44 = value; }
        }
        public T45 Item045 {
            get { return _item45; }
            set { _item45 = value; }
        }
        public T46 Item046 {
            get { return _item46; }
            set { _item46 = value; }
        }
        public T47 Item047 {
            get { return _item47; }
            set { _item47 = value; }
        }
        public T48 Item048 {
            get { return _item48; }
            set { _item48 = value; }
        }
        public T49 Item049 {
            get { return _item49; }
            set { _item49 = value; }
        }
        public T50 Item050 {
            get { return _item50; }
            set { _item50 = value; }
        }
        public T51 Item051 {
            get { return _item51; }
            set { _item51 = value; }
        }
        public T52 Item052 {
            get { return _item52; }
            set { _item52 = value; }
        }
        public T53 Item053 {
            get { return _item53; }
            set { _item53 = value; }
        }
        public T54 Item054 {
            get { return _item54; }
            set { _item54 = value; }
        }
        public T55 Item055 {
            get { return _item55; }
            set { _item55 = value; }
        }
        public T56 Item056 {
            get { return _item56; }
            set { _item56 = value; }
        }
        public T57 Item057 {
            get { return _item57; }
            set { _item57 = value; }
        }
        public T58 Item058 {
            get { return _item58; }
            set { _item58 = value; }
        }
        public T59 Item059 {
            get { return _item59; }
            set { _item59 = value; }
        }
        public T60 Item060 {
            get { return _item60; }
            set { _item60 = value; }
        }
        public T61 Item061 {
            get { return _item61; }
            set { _item61 = value; }
        }
        public T62 Item062 {
            get { return _item62; }
            set { _item62 = value; }
        }
        public T63 Item063 {
            get { return _item63; }
            set { _item63 = value; }
        }

        public override object GetValue(int index) {
            switch(index) {
                case 0: return Item000;
                case 1: return Item001;
                case 2: return Item002;
                case 3: return Item003;
                case 4: return Item004;
                case 5: return Item005;
                case 6: return Item006;
                case 7: return Item007;
                case 8: return Item008;
                case 9: return Item009;
                case 10: return Item010;
                case 11: return Item011;
                case 12: return Item012;
                case 13: return Item013;
                case 14: return Item014;
                case 15: return Item015;
                case 16: return Item016;
                case 17: return Item017;
                case 18: return Item018;
                case 19: return Item019;
                case 20: return Item020;
                case 21: return Item021;
                case 22: return Item022;
                case 23: return Item023;
                case 24: return Item024;
                case 25: return Item025;
                case 26: return Item026;
                case 27: return Item027;
                case 28: return Item028;
                case 29: return Item029;
                case 30: return Item030;
                case 31: return Item031;
                case 32: return Item032;
                case 33: return Item033;
                case 34: return Item034;
                case 35: return Item035;
                case 36: return Item036;
                case 37: return Item037;
                case 38: return Item038;
                case 39: return Item039;
                case 40: return Item040;
                case 41: return Item041;
                case 42: return Item042;
                case 43: return Item043;
                case 44: return Item044;
                case 45: return Item045;
                case 46: return Item046;
                case 47: return Item047;
                case 48: return Item048;
                case 49: return Item049;
                case 50: return Item050;
                case 51: return Item051;
                case 52: return Item052;
                case 53: return Item053;
                case 54: return Item054;
                case 55: return Item055;
                case 56: return Item056;
                case 57: return Item057;
                case 58: return Item058;
                case 59: return Item059;
                case 60: return Item060;
                case 61: return Item061;
                case 62: return Item062;
                case 63: return Item063;
                default: throw new ArgumentException("index");
            }
        }

        public override void SetValue(int index, object value) {
            switch(index) {
                case 0: Item000 = (T0)value; break;
                case 1: Item001 = (T1)value; break;
                case 2: Item002 = (T2)value; break;
                case 3: Item003 = (T3)value; break;
                case 4: Item004 = (T4)value; break;
                case 5: Item005 = (T5)value; break;
                case 6: Item006 = (T6)value; break;
                case 7: Item007 = (T7)value; break;
                case 8: Item008 = (T8)value; break;
                case 9: Item009 = (T9)value; break;
                case 10: Item010 = (T10)value; break;
                case 11: Item011 = (T11)value; break;
                case 12: Item012 = (T12)value; break;
                case 13: Item013 = (T13)value; break;
                case 14: Item014 = (T14)value; break;
                case 15: Item015 = (T15)value; break;
                case 16: Item016 = (T16)value; break;
                case 17: Item017 = (T17)value; break;
                case 18: Item018 = (T18)value; break;
                case 19: Item019 = (T19)value; break;
                case 20: Item020 = (T20)value; break;
                case 21: Item021 = (T21)value; break;
                case 22: Item022 = (T22)value; break;
                case 23: Item023 = (T23)value; break;
                case 24: Item024 = (T24)value; break;
                case 25: Item025 = (T25)value; break;
                case 26: Item026 = (T26)value; break;
                case 27: Item027 = (T27)value; break;
                case 28: Item028 = (T28)value; break;
                case 29: Item029 = (T29)value; break;
                case 30: Item030 = (T30)value; break;
                case 31: Item031 = (T31)value; break;
                case 32: Item032 = (T32)value; break;
                case 33: Item033 = (T33)value; break;
                case 34: Item034 = (T34)value; break;
                case 35: Item035 = (T35)value; break;
                case 36: Item036 = (T36)value; break;
                case 37: Item037 = (T37)value; break;
                case 38: Item038 = (T38)value; break;
                case 39: Item039 = (T39)value; break;
                case 40: Item040 = (T40)value; break;
                case 41: Item041 = (T41)value; break;
                case 42: Item042 = (T42)value; break;
                case 43: Item043 = (T43)value; break;
                case 44: Item044 = (T44)value; break;
                case 45: Item045 = (T45)value; break;
                case 46: Item046 = (T46)value; break;
                case 47: Item047 = (T47)value; break;
                case 48: Item048 = (T48)value; break;
                case 49: Item049 = (T49)value; break;
                case 50: Item050 = (T50)value; break;
                case 51: Item051 = (T51)value; break;
                case 52: Item052 = (T52)value; break;
                case 53: Item053 = (T53)value; break;
                case 54: Item054 = (T54)value; break;
                case 55: Item055 = (T55)value; break;
                case 56: Item056 = (T56)value; break;
                case 57: Item057 = (T57)value; break;
                case 58: Item058 = (T58)value; break;
                case 59: Item059 = (T59)value; break;
                case 60: Item060 = (T60)value; break;
                case 61: Item061 = (T61)value; break;
                case 62: Item062 = (T62)value; break;
                case 63: Item063 = (T63)value; break;
                default: throw new ArgumentException("index");
            }
        }
    }
    public class Tuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60, T61, T62, T63, T64, T65, T66, T67, T68, T69, T70, T71, T72, T73, T74, T75, T76, T77, T78, T79, T80, T81, T82, T83, T84, T85, T86, T87, T88, T89, T90, T91, T92, T93, T94, T95, T96, T97, T98, T99, T100, T101, T102, T103, T104, T105, T106, T107, T108, T109, T110, T111, T112, T113, T114, T115, T116, T117, T118, T119, T120, T121, T122, T123, T124, T125, T126, T127> : Tuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60, T61, T62, T63> {
        private T64 _item64;
        private T65 _item65;
        private T66 _item66;
        private T67 _item67;
        private T68 _item68;
        private T69 _item69;
        private T70 _item70;
        private T71 _item71;
        private T72 _item72;
        private T73 _item73;
        private T74 _item74;
        private T75 _item75;
        private T76 _item76;
        private T77 _item77;
        private T78 _item78;
        private T79 _item79;
        private T80 _item80;
        private T81 _item81;
        private T82 _item82;
        private T83 _item83;
        private T84 _item84;
        private T85 _item85;
        private T86 _item86;
        private T87 _item87;
        private T88 _item88;
        private T89 _item89;
        private T90 _item90;
        private T91 _item91;
        private T92 _item92;
        private T93 _item93;
        private T94 _item94;
        private T95 _item95;
        private T96 _item96;
        private T97 _item97;
        private T98 _item98;
        private T99 _item99;
        private T100 _item100;
        private T101 _item101;
        private T102 _item102;
        private T103 _item103;
        private T104 _item104;
        private T105 _item105;
        private T106 _item106;
        private T107 _item107;
        private T108 _item108;
        private T109 _item109;
        private T110 _item110;
        private T111 _item111;
        private T112 _item112;
        private T113 _item113;
        private T114 _item114;
        private T115 _item115;
        private T116 _item116;
        private T117 _item117;
        private T118 _item118;
        private T119 _item119;
        private T120 _item120;
        private T121 _item121;
        private T122 _item122;
        private T123 _item123;
        private T124 _item124;
        private T125 _item125;
        private T126 _item126;
        private T127 _item127;

        public T64 Item064 {
            get { return _item64; }
            set { _item64 = value; }
        }
        public T65 Item065 {
            get { return _item65; }
            set { _item65 = value; }
        }
        public T66 Item066 {
            get { return _item66; }
            set { _item66 = value; }
        }
        public T67 Item067 {
            get { return _item67; }
            set { _item67 = value; }
        }
        public T68 Item068 {
            get { return _item68; }
            set { _item68 = value; }
        }
        public T69 Item069 {
            get { return _item69; }
            set { _item69 = value; }
        }
        public T70 Item070 {
            get { return _item70; }
            set { _item70 = value; }
        }
        public T71 Item071 {
            get { return _item71; }
            set { _item71 = value; }
        }
        public T72 Item072 {
            get { return _item72; }
            set { _item72 = value; }
        }
        public T73 Item073 {
            get { return _item73; }
            set { _item73 = value; }
        }
        public T74 Item074 {
            get { return _item74; }
            set { _item74 = value; }
        }
        public T75 Item075 {
            get { return _item75; }
            set { _item75 = value; }
        }
        public T76 Item076 {
            get { return _item76; }
            set { _item76 = value; }
        }
        public T77 Item077 {
            get { return _item77; }
            set { _item77 = value; }
        }
        public T78 Item078 {
            get { return _item78; }
            set { _item78 = value; }
        }
        public T79 Item079 {
            get { return _item79; }
            set { _item79 = value; }
        }
        public T80 Item080 {
            get { return _item80; }
            set { _item80 = value; }
        }
        public T81 Item081 {
            get { return _item81; }
            set { _item81 = value; }
        }
        public T82 Item082 {
            get { return _item82; }
            set { _item82 = value; }
        }
        public T83 Item083 {
            get { return _item83; }
            set { _item83 = value; }
        }
        public T84 Item084 {
            get { return _item84; }
            set { _item84 = value; }
        }
        public T85 Item085 {
            get { return _item85; }
            set { _item85 = value; }
        }
        public T86 Item086 {
            get { return _item86; }
            set { _item86 = value; }
        }
        public T87 Item087 {
            get { return _item87; }
            set { _item87 = value; }
        }
        public T88 Item088 {
            get { return _item88; }
            set { _item88 = value; }
        }
        public T89 Item089 {
            get { return _item89; }
            set { _item89 = value; }
        }
        public T90 Item090 {
            get { return _item90; }
            set { _item90 = value; }
        }
        public T91 Item091 {
            get { return _item91; }
            set { _item91 = value; }
        }
        public T92 Item092 {
            get { return _item92; }
            set { _item92 = value; }
        }
        public T93 Item093 {
            get { return _item93; }
            set { _item93 = value; }
        }
        public T94 Item094 {
            get { return _item94; }
            set { _item94 = value; }
        }
        public T95 Item095 {
            get { return _item95; }
            set { _item95 = value; }
        }
        public T96 Item096 {
            get { return _item96; }
            set { _item96 = value; }
        }
        public T97 Item097 {
            get { return _item97; }
            set { _item97 = value; }
        }
        public T98 Item098 {
            get { return _item98; }
            set { _item98 = value; }
        }
        public T99 Item099 {
            get { return _item99; }
            set { _item99 = value; }
        }
        public T100 Item100 {
            get { return _item100; }
            set { _item100 = value; }
        }
        public T101 Item101 {
            get { return _item101; }
            set { _item101 = value; }
        }
        public T102 Item102 {
            get { return _item102; }
            set { _item102 = value; }
        }
        public T103 Item103 {
            get { return _item103; }
            set { _item103 = value; }
        }
        public T104 Item104 {
            get { return _item104; }
            set { _item104 = value; }
        }
        public T105 Item105 {
            get { return _item105; }
            set { _item105 = value; }
        }
        public T106 Item106 {
            get { return _item106; }
            set { _item106 = value; }
        }
        public T107 Item107 {
            get { return _item107; }
            set { _item107 = value; }
        }
        public T108 Item108 {
            get { return _item108; }
            set { _item108 = value; }
        }
        public T109 Item109 {
            get { return _item109; }
            set { _item109 = value; }
        }
        public T110 Item110 {
            get { return _item110; }
            set { _item110 = value; }
        }
        public T111 Item111 {
            get { return _item111; }
            set { _item111 = value; }
        }
        public T112 Item112 {
            get { return _item112; }
            set { _item112 = value; }
        }
        public T113 Item113 {
            get { return _item113; }
            set { _item113 = value; }
        }
        public T114 Item114 {
            get { return _item114; }
            set { _item114 = value; }
        }
        public T115 Item115 {
            get { return _item115; }
            set { _item115 = value; }
        }
        public T116 Item116 {
            get { return _item116; }
            set { _item116 = value; }
        }
        public T117 Item117 {
            get { return _item117; }
            set { _item117 = value; }
        }
        public T118 Item118 {
            get { return _item118; }
            set { _item118 = value; }
        }
        public T119 Item119 {
            get { return _item119; }
            set { _item119 = value; }
        }
        public T120 Item120 {
            get { return _item120; }
            set { _item120 = value; }
        }
        public T121 Item121 {
            get { return _item121; }
            set { _item121 = value; }
        }
        public T122 Item122 {
            get { return _item122; }
            set { _item122 = value; }
        }
        public T123 Item123 {
            get { return _item123; }
            set { _item123 = value; }
        }
        public T124 Item124 {
            get { return _item124; }
            set { _item124 = value; }
        }
        public T125 Item125 {
            get { return _item125; }
            set { _item125 = value; }
        }
        public T126 Item126 {
            get { return _item126; }
            set { _item126 = value; }
        }
        public T127 Item127 {
            get { return _item127; }
            set { _item127 = value; }
        }

        public override object GetValue(int index) {
            switch(index) {
                case 0: return Item000;
                case 1: return Item001;
                case 2: return Item002;
                case 3: return Item003;
                case 4: return Item004;
                case 5: return Item005;
                case 6: return Item006;
                case 7: return Item007;
                case 8: return Item008;
                case 9: return Item009;
                case 10: return Item010;
                case 11: return Item011;
                case 12: return Item012;
                case 13: return Item013;
                case 14: return Item014;
                case 15: return Item015;
                case 16: return Item016;
                case 17: return Item017;
                case 18: return Item018;
                case 19: return Item019;
                case 20: return Item020;
                case 21: return Item021;
                case 22: return Item022;
                case 23: return Item023;
                case 24: return Item024;
                case 25: return Item025;
                case 26: return Item026;
                case 27: return Item027;
                case 28: return Item028;
                case 29: return Item029;
                case 30: return Item030;
                case 31: return Item031;
                case 32: return Item032;
                case 33: return Item033;
                case 34: return Item034;
                case 35: return Item035;
                case 36: return Item036;
                case 37: return Item037;
                case 38: return Item038;
                case 39: return Item039;
                case 40: return Item040;
                case 41: return Item041;
                case 42: return Item042;
                case 43: return Item043;
                case 44: return Item044;
                case 45: return Item045;
                case 46: return Item046;
                case 47: return Item047;
                case 48: return Item048;
                case 49: return Item049;
                case 50: return Item050;
                case 51: return Item051;
                case 52: return Item052;
                case 53: return Item053;
                case 54: return Item054;
                case 55: return Item055;
                case 56: return Item056;
                case 57: return Item057;
                case 58: return Item058;
                case 59: return Item059;
                case 60: return Item060;
                case 61: return Item061;
                case 62: return Item062;
                case 63: return Item063;
                case 64: return Item064;
                case 65: return Item065;
                case 66: return Item066;
                case 67: return Item067;
                case 68: return Item068;
                case 69: return Item069;
                case 70: return Item070;
                case 71: return Item071;
                case 72: return Item072;
                case 73: return Item073;
                case 74: return Item074;
                case 75: return Item075;
                case 76: return Item076;
                case 77: return Item077;
                case 78: return Item078;
                case 79: return Item079;
                case 80: return Item080;
                case 81: return Item081;
                case 82: return Item082;
                case 83: return Item083;
                case 84: return Item084;
                case 85: return Item085;
                case 86: return Item086;
                case 87: return Item087;
                case 88: return Item088;
                case 89: return Item089;
                case 90: return Item090;
                case 91: return Item091;
                case 92: return Item092;
                case 93: return Item093;
                case 94: return Item094;
                case 95: return Item095;
                case 96: return Item096;
                case 97: return Item097;
                case 98: return Item098;
                case 99: return Item099;
                case 100: return Item100;
                case 101: return Item101;
                case 102: return Item102;
                case 103: return Item103;
                case 104: return Item104;
                case 105: return Item105;
                case 106: return Item106;
                case 107: return Item107;
                case 108: return Item108;
                case 109: return Item109;
                case 110: return Item110;
                case 111: return Item111;
                case 112: return Item112;
                case 113: return Item113;
                case 114: return Item114;
                case 115: return Item115;
                case 116: return Item116;
                case 117: return Item117;
                case 118: return Item118;
                case 119: return Item119;
                case 120: return Item120;
                case 121: return Item121;
                case 122: return Item122;
                case 123: return Item123;
                case 124: return Item124;
                case 125: return Item125;
                case 126: return Item126;
                case 127: return Item127;
                default: throw new ArgumentException("index");
            }
        }

        public override void SetValue(int index, object value) {
            switch(index) {
                case 0: Item000 = (T0)value; break;
                case 1: Item001 = (T1)value; break;
                case 2: Item002 = (T2)value; break;
                case 3: Item003 = (T3)value; break;
                case 4: Item004 = (T4)value; break;
                case 5: Item005 = (T5)value; break;
                case 6: Item006 = (T6)value; break;
                case 7: Item007 = (T7)value; break;
                case 8: Item008 = (T8)value; break;
                case 9: Item009 = (T9)value; break;
                case 10: Item010 = (T10)value; break;
                case 11: Item011 = (T11)value; break;
                case 12: Item012 = (T12)value; break;
                case 13: Item013 = (T13)value; break;
                case 14: Item014 = (T14)value; break;
                case 15: Item015 = (T15)value; break;
                case 16: Item016 = (T16)value; break;
                case 17: Item017 = (T17)value; break;
                case 18: Item018 = (T18)value; break;
                case 19: Item019 = (T19)value; break;
                case 20: Item020 = (T20)value; break;
                case 21: Item021 = (T21)value; break;
                case 22: Item022 = (T22)value; break;
                case 23: Item023 = (T23)value; break;
                case 24: Item024 = (T24)value; break;
                case 25: Item025 = (T25)value; break;
                case 26: Item026 = (T26)value; break;
                case 27: Item027 = (T27)value; break;
                case 28: Item028 = (T28)value; break;
                case 29: Item029 = (T29)value; break;
                case 30: Item030 = (T30)value; break;
                case 31: Item031 = (T31)value; break;
                case 32: Item032 = (T32)value; break;
                case 33: Item033 = (T33)value; break;
                case 34: Item034 = (T34)value; break;
                case 35: Item035 = (T35)value; break;
                case 36: Item036 = (T36)value; break;
                case 37: Item037 = (T37)value; break;
                case 38: Item038 = (T38)value; break;
                case 39: Item039 = (T39)value; break;
                case 40: Item040 = (T40)value; break;
                case 41: Item041 = (T41)value; break;
                case 42: Item042 = (T42)value; break;
                case 43: Item043 = (T43)value; break;
                case 44: Item044 = (T44)value; break;
                case 45: Item045 = (T45)value; break;
                case 46: Item046 = (T46)value; break;
                case 47: Item047 = (T47)value; break;
                case 48: Item048 = (T48)value; break;
                case 49: Item049 = (T49)value; break;
                case 50: Item050 = (T50)value; break;
                case 51: Item051 = (T51)value; break;
                case 52: Item052 = (T52)value; break;
                case 53: Item053 = (T53)value; break;
                case 54: Item054 = (T54)value; break;
                case 55: Item055 = (T55)value; break;
                case 56: Item056 = (T56)value; break;
                case 57: Item057 = (T57)value; break;
                case 58: Item058 = (T58)value; break;
                case 59: Item059 = (T59)value; break;
                case 60: Item060 = (T60)value; break;
                case 61: Item061 = (T61)value; break;
                case 62: Item062 = (T62)value; break;
                case 63: Item063 = (T63)value; break;
                case 64: Item064 = (T64)value; break;
                case 65: Item065 = (T65)value; break;
                case 66: Item066 = (T66)value; break;
                case 67: Item067 = (T67)value; break;
                case 68: Item068 = (T68)value; break;
                case 69: Item069 = (T69)value; break;
                case 70: Item070 = (T70)value; break;
                case 71: Item071 = (T71)value; break;
                case 72: Item072 = (T72)value; break;
                case 73: Item073 = (T73)value; break;
                case 74: Item074 = (T74)value; break;
                case 75: Item075 = (T75)value; break;
                case 76: Item076 = (T76)value; break;
                case 77: Item077 = (T77)value; break;
                case 78: Item078 = (T78)value; break;
                case 79: Item079 = (T79)value; break;
                case 80: Item080 = (T80)value; break;
                case 81: Item081 = (T81)value; break;
                case 82: Item082 = (T82)value; break;
                case 83: Item083 = (T83)value; break;
                case 84: Item084 = (T84)value; break;
                case 85: Item085 = (T85)value; break;
                case 86: Item086 = (T86)value; break;
                case 87: Item087 = (T87)value; break;
                case 88: Item088 = (T88)value; break;
                case 89: Item089 = (T89)value; break;
                case 90: Item090 = (T90)value; break;
                case 91: Item091 = (T91)value; break;
                case 92: Item092 = (T92)value; break;
                case 93: Item093 = (T93)value; break;
                case 94: Item094 = (T94)value; break;
                case 95: Item095 = (T95)value; break;
                case 96: Item096 = (T96)value; break;
                case 97: Item097 = (T97)value; break;
                case 98: Item098 = (T98)value; break;
                case 99: Item099 = (T99)value; break;
                case 100: Item100 = (T100)value; break;
                case 101: Item101 = (T101)value; break;
                case 102: Item102 = (T102)value; break;
                case 103: Item103 = (T103)value; break;
                case 104: Item104 = (T104)value; break;
                case 105: Item105 = (T105)value; break;
                case 106: Item106 = (T106)value; break;
                case 107: Item107 = (T107)value; break;
                case 108: Item108 = (T108)value; break;
                case 109: Item109 = (T109)value; break;
                case 110: Item110 = (T110)value; break;
                case 111: Item111 = (T111)value; break;
                case 112: Item112 = (T112)value; break;
                case 113: Item113 = (T113)value; break;
                case 114: Item114 = (T114)value; break;
                case 115: Item115 = (T115)value; break;
                case 116: Item116 = (T116)value; break;
                case 117: Item117 = (T117)value; break;
                case 118: Item118 = (T118)value; break;
                case 119: Item119 = (T119)value; break;
                case 120: Item120 = (T120)value; break;
                case 121: Item121 = (T121)value; break;
                case 122: Item122 = (T122)value; break;
                case 123: Item123 = (T123)value; break;
                case 124: Item124 = (T124)value; break;
                case 125: Item125 = (T125)value; break;
                case 126: Item126 = (T126)value; break;
                case 127: Item127 = (T127)value; break;
                default: throw new ArgumentException("index");
            }
        }
    }

    // *** END GENERATED CODE ***

    #endregion
}
