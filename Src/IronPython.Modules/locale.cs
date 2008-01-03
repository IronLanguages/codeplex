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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

[assembly: PythonModule("_locale", typeof(IronPython.Modules.PythonLocale))]
namespace IronPython.Modules {
    public static class PythonLocale {
        internal static LocaleInfo currentLocale = new LocaleInfo();

        public static object CHAR_MAX = 127;
        public static object Error = ExceptionConverter.CreatePythonException("Error", "_locale");
        public static object LC_ALL = (int)LocaleCategories.All;
        public static object LC_COLLATE = (int)LocaleCategories.Collate;
        public static object LC_CTYPE = (int)LocaleCategories.CType;
        public static object LC_MONETARY = (int)LocaleCategories.Monetary;
        public static object LC_NUMERIC = (int)LocaleCategories.Numeric;
        public static object LC_TIME = (int)LocaleCategories.Time;

        [Documentation("gets the default locale tuple")]
        [PythonName("_getdefaultlocale")]
        public static object GetDefaultLocale() {
            return PythonTuple.MakeTuple(new object[] { CultureInfo.CurrentCulture.Name, "" });
        }

        [Documentation(@"gets the locale's convetions table.  

The conventions table is a dictionary that contains information on how to use 
the locale for numeric and monetary formatting")]
        [PythonName("localeconv")]
        public static object LocaleConventions() {
            return currentLocale.GetConventionsTable();
        }

        [Documentation(@"Sets the current locale for the given category.

LC_ALL:       sets locale for all options below
LC_COLLATE:   sets locale for collation (strcoll and strxfrm) only
LC_CTYPE:     sets locale for CType [unused]
LC_MONETARY:  sets locale for the monetary functions (localeconv())
LC_NUMERIC:   sets the locale for numeric functions (slocaleconv())
LC_TIME:      sets the locale for time functions [unused]

If locale is None then the current setting is returned.
")]
        [PythonName("setlocale")]
        public static object SetLocale(object category, [DefaultParameterValue(null)]string locale) {
            if (locale == null) {
                return currentLocale.GetLocale(category);
            }

            currentLocale.SetLocale(category, locale);
            return null;
        }

        [Documentation("compares two strings using the current locale")]
        [PythonName("strcoll")]
        public static int StringCollate(string string1, string string2) {
            return currentLocale.Collate.CompareInfo.Compare(string1, string2, CompareOptions.None);
        }

        [Documentation(@"returns a transformed string that can be compared using the built-in cmp.
        
Currently returns the string unmodified")]
        [PythonName("strxfrm")]
        public static object StringTransform(string @string) {
            return @string;
        }

        private enum LocaleCategories {
            All = 0,
            Collate = 1,
            CType = 2,
            Monetary = 3,
            Numeric = 4,
            Time = 5,
        }

        internal class LocaleInfo {
            public LocaleInfo() {
                Collate = CultureInfo.CurrentCulture;
                CType = CultureInfo.CurrentCulture;
                Time = CultureInfo.CurrentCulture;
                Monetary = CultureInfo.CurrentCulture;
                Numeric = CultureInfo.CurrentCulture;
            }

            public CultureInfo Collate;
            public CultureInfo CType;
            public CultureInfo Time;

            public CultureInfo Monetary;
            public CultureInfo Numeric;

            private PythonDictionary conv;

            public override string ToString() {
                return base.ToString();
            }

            public PythonDictionary GetConventionsTable() {
                CreateConventionsDict();

                return conv;
            }

            public void SetLocale(object category, string locale) {
                switch ((LocaleCategories)(int)category) {
                    case LocaleCategories.All:
                        SetLocale(LC_COLLATE, locale);
                        SetLocale(LC_CTYPE, locale);
                        SetLocale(LC_MONETARY, locale);
                        SetLocale(LC_NUMERIC, locale);
                        SetLocale(LC_TIME, locale);
                        break;
                    case LocaleCategories.Collate: Collate = LocaleToCulture(locale); break;
                    case LocaleCategories.CType: CType = LocaleToCulture(locale); break;
                    case LocaleCategories.Time: Time = LocaleToCulture(locale); break;
                    case LocaleCategories.Monetary:
                        Monetary = LocaleToCulture(locale);
                        conv = null;
                        break;
                    case LocaleCategories.Numeric:
                        Numeric = LocaleToCulture(locale);
                        conv = null;
                        break;
                    default:
                        throw ExceptionConverter.CreateThrowable(Error, "unknown locale category");
                }
            }

            public string GetLocale(object category) {
                switch ((LocaleCategories)(int)category) {
                    case LocaleCategories.All:
                        if (Collate == CType &&
                            Collate == Time &&
                            Collate == Monetary &&
                            Collate == Numeric) {
                            // they're all the same, return only 1 name
                            goto case LocaleCategories.Collate;
                        }

                        // return them all...
                        return String.Format("LC_COLLATE={0};LC_CTYPE={1};LC_MONETARY={2};LC_NUMERIC={3};LC_TIME={4}",
                            GetLocale(LC_COLLATE),
                            GetLocale(LC_CTYPE),
                            GetLocale(LC_MONETARY),
                            GetLocale(LC_NUMERIC),
                            GetLocale(LC_TIME));
                    case LocaleCategories.Collate: return CultureToName(Collate);
                    case LocaleCategories.CType: return CultureToName(CType);
                    case LocaleCategories.Time: return CultureToName(Time);
                    case LocaleCategories.Monetary: return CultureToName(Monetary);
                    case LocaleCategories.Numeric: return CultureToName(Numeric);
                    default:
                        throw ExceptionConverter.CreateThrowable(Error, "unknown locale category");
                }
            }

            public string CultureToName(CultureInfo culture) {
                return culture.Name.Replace('-', '_');
            }

            private CultureInfo LocaleToCulture(string locale) {
                locale = locale.Replace('_', '-');

                try {
                    return StringUtils.GetCultureInfo(locale);
                } catch (ArgumentException) {
                    throw ExceptionConverter.CreateThrowable(Error, String.Format("unknown locale: {0}", locale));
                }
            }

            /// <summary>
            /// Popupates the given directory w/ the locale information from the given
            /// CultureInfo.
            /// </summary>
            private void CreateConventionsDict() {
                conv = new PythonDictionary();

                conv["decimal_point"] = Numeric.NumberFormat.NumberDecimalSeparator;
                conv["grouping"] = GroupsToList(Numeric.NumberFormat.NumberGroupSizes);
                conv["thousands_sep"] = Numeric.NumberFormat.NumberGroupSeparator;

                conv["mon_decimal_point"] = Monetary.NumberFormat.CurrencyDecimalSeparator;
                conv["mon_thousands_sep"] = Monetary.NumberFormat.CurrencyGroupSeparator;
                conv["mon_grouping"] = GroupsToList(Monetary.NumberFormat.CurrencyGroupSizes);
                conv["int_curr_symbol"] = Monetary.NumberFormat.CurrencySymbol;
                conv["currency_symbol"] = Monetary.NumberFormat.CurrencySymbol;
                conv["frac_digits"] = Monetary.NumberFormat.CurrencyDecimalDigits;
                conv["int_frac_digits"] = Monetary.NumberFormat.CurrencyDecimalDigits;
                conv["positive_sign"] = Monetary.NumberFormat.PositiveSign;
                conv["negative_sign"] = Monetary.NumberFormat.NegativeSign;

                conv["p_sign_posn"] = Monetary.NumberFormat.CurrencyPositivePattern;
                conv["n_sign_posn"] = Monetary.NumberFormat.CurrencyNegativePattern;
            }

            private static List GroupsToList(int[] groups) {
                // .NET: values from 0-9, if the last digit is zero, remaining digits
                // go ungrouped, otherwise they're grouped based upon the last value.

                // locale: ends in CHAR_MAX if no further grouping is performed, ends in
                // zero if the last group size is repeatedly used.
                List res = new List(groups);
                if (groups.Length > 0 && groups[groups.Length - 1] == 0) {
                    // replace zero w/ CHAR_MAX, no further grouping is performed
                    res[res.Count - 1] = CHAR_MAX;
                } else {
                    // append 0 to indicate we should repeatedly use the last one
                    res.AddNoLock(0);
                }

                return res;
            }
        }
    }
}
