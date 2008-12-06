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

using System; using Microsoft;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Runtime.InteropServices;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

[assembly: PythonModule("_locale", typeof(IronPython.Modules.PythonLocale))]
namespace IronPython.Modules {
    public static class PythonLocale {
        private static readonly object _localeKey = new object();

        [SpecialName]
        public static void PerformModuleReload(PythonContext/*!*/ context, IAttributesCollection/*!*/ dict) {
            EnsureLocaleInitialized(context);
            context.EnsureModuleException("_localeerror", dict, "Error", "_locale");
        }

        internal static void EnsureLocaleInitialized(PythonContext context) {
            if (!context.HasModuleState(_localeKey)) {
                context.SetModuleState(_localeKey, new LocaleInfo(context));
            }
        }

        public const int CHAR_MAX = 127;
        public const int LC_ALL = (int)LocaleCategories.All;
        public const int LC_COLLATE = (int)LocaleCategories.Collate;
        public const int LC_CTYPE = (int)LocaleCategories.CType;
        public const int LC_MONETARY = (int)LocaleCategories.Monetary;
        public const int LC_NUMERIC = (int)LocaleCategories.Numeric;
        public const int LC_TIME = (int)LocaleCategories.Time;

        [Documentation("gets the default locale tuple")]
        public static object _getdefaultlocale() {            
            return PythonTuple.MakeTuple(
                CultureInfo.CurrentCulture.Name.Replace('-', '_').Replace(' ', '_'), 
#if !SILVERLIGHT    // No ANSICodePage in Silverlight
                "cp" + CultureInfo.CurrentCulture.TextInfo.ANSICodePage.ToString()
#else
                ""
#endif
            );
        }

        [Documentation(@"gets the locale's convetions table.  

The conventions table is a dictionary that contains information on how to use 
the locale for numeric and monetary formatting")]
        public static object localeconv(CodeContext/*!*/ context) {
            return GetLocaleInfo(context).GetConventionsTable();
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
        public static object setlocale(CodeContext/*!*/ context, object category, [DefaultParameterValue(null)]string locale) {
            LocaleInfo li = GetLocaleInfo(context);
            if (locale == null) {
                return li.GetLocale(context, category);
            }

            return li.SetLocale(context, category, locale);
        }

        [Documentation("compares two strings using the current locale")]
        public static int strcoll(CodeContext/*!*/ context, string string1, string string2) {
            return GetLocaleInfo(context).Collate.CompareInfo.Compare(string1, string2, CompareOptions.None);
        }

        [Documentation(@"returns a transformed string that can be compared using the built-in cmp.
        
Currently returns the string unmodified")]
        public static object strxfrm(string @string) {
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
            private readonly PythonContext _context;
            private PythonDictionary conv;

            public LocaleInfo(PythonContext context) {
                _context = context;
            }

            public CultureInfo Collate {
                get { return _context.CollateCulture; }
                set { _context.CollateCulture = value; }
            }

            public CultureInfo CType {
                get { return _context.CTypeCulture; }
                set { _context.CTypeCulture= value; }
            }
            
            public CultureInfo Time {
                get { return _context.TimeCulture; }
                set { _context.TimeCulture = value; }
            }

            public CultureInfo Monetary {
                get { return _context.MonetaryCulture; }
                set { _context.MonetaryCulture = value; }
            }
            
            public CultureInfo Numeric {
                get { return _context.NumericCulture; }
                set { _context.NumericCulture = value; }
            }

            public override string ToString() {
                return base.ToString();
            }

            public PythonDictionary GetConventionsTable() {
                CreateConventionsDict();

                return conv;
            }

            public string SetLocale(CodeContext/*!*/ context, object category, string locale) {
                switch ((LocaleCategories)(int)category) {
                    case LocaleCategories.All:
                        SetLocale(context, LC_COLLATE, locale);
                        SetLocale(context, LC_CTYPE, locale);
                        SetLocale(context, LC_MONETARY, locale);
                        SetLocale(context, LC_NUMERIC, locale);
                        return SetLocale(context, LC_TIME, locale);
                    case LocaleCategories.Collate:
                        return CultureToName(Collate = LocaleToCulture(context, locale));
                    case LocaleCategories.CType:
                        return CultureToName(CType = LocaleToCulture(context, locale));                        
                    case LocaleCategories.Time:
                        return CultureToName(Time = LocaleToCulture(context, locale));                        
                    case LocaleCategories.Monetary:
                        Monetary = LocaleToCulture(context, locale);
                        conv = null;
                        return CultureToName(Monetary);
                    case LocaleCategories.Numeric:
                        Numeric = LocaleToCulture(context, locale);
                        conv = null;
                        return CultureToName(Numeric);
                    default:
                        throw PythonExceptions.CreateThrowable(_localeerror(context), "unknown locale category");
                }

            }

            public string GetLocale(CodeContext/*!*/ context, object category) {
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
                            GetLocale(context, LC_COLLATE),
                            GetLocale(context, LC_CTYPE),
                            GetLocale(context, LC_MONETARY),
                            GetLocale(context, LC_NUMERIC),
                            GetLocale(context, LC_TIME));
                    case LocaleCategories.Collate: return CultureToName(Collate);
                    case LocaleCategories.CType: return CultureToName(CType);
                    case LocaleCategories.Time: return CultureToName(Time);
                    case LocaleCategories.Monetary: return CultureToName(Monetary);
                    case LocaleCategories.Numeric: return CultureToName(Numeric);
                    default:
                        throw PythonExceptions.CreateThrowable(_localeerror(context), "unknown locale category");
                }
            }

            public string CultureToName(CultureInfo culture) {
                if (culture == CultureInfo.InvariantCulture) {
                    return "C";
                }
                
                return culture.Name.Replace('-', '_');
            }

            private CultureInfo LocaleToCulture(CodeContext/*!*/ context, string locale) {
                if (locale == "C") {
                    return CultureInfo.InvariantCulture;
                }

                locale = locale.Replace('_', '-');

                try {
                    return StringUtils.GetCultureInfo(locale);
                } catch (ArgumentException) {
                    throw PythonExceptions.CreateThrowable(_localeerror(context), String.Format("unknown locale: {0}", locale));
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
                    res[res.__len__() - 1] = CHAR_MAX;
                } else {
                    // append 0 to indicate we should repeatedly use the last one
                    res.AddNoLock(0);
                }

                return res;
            }
        }

        internal static LocaleInfo/*!*/ GetLocaleInfo(CodeContext/*!*/ context) {
            EnsureLocaleInitialized(PythonContext.GetContext(context));

            return (LocaleInfo)PythonContext.GetContext(context).GetModuleState(_localeKey);
        }        

        private static PythonType _localeerror(CodeContext/*!*/ context) {
            return (PythonType)PythonContext.GetContext(context).GetModuleState("_localeerror");
        }
    }
}
