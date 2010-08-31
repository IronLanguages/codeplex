/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace SiteTest
{
    class Assert
    {
        static public void AreEqual<T>(T expected, T actual)
        {
            Assert.AreEqual(expected, actual, string.Empty, null);
        }

        static public void AreEqual<T>(T expected, T actual, string message)
        {
            Assert.AreEqual(expected, actual, message, null);
        }

        static public void AreEqual<T>(T expected, T actual, string message, params object[] parameters)
        {
            if (!Object.Equals(expected, actual))
            {
                string finalMessage;
                if (actual != null && expected != null && !actual.GetType().Equals(expected.GetType()))
                {
                    // This is for cases like: Assert.AreEqual(42L, 42) -> Expected: <42>, Actual: <42>
                    finalMessage = String.Format("{0}  Expected: {1} ({2}), Actual: {3} ({4})",
                        message == null ? String.Empty : ReplaceNulls(message),
                        ReplaceNulls(expected),
                        expected.GetType().FullName,
                        ReplaceNulls(actual),
                        actual.GetType().FullName);
                }
                else
                {
                    finalMessage = String.Format("{0}  Expected: {1}, Actual: {2}",
                        message == null ? String.Empty : ReplaceNulls(message),
                        ReplaceNulls(expected),
                        ReplaceNulls(actual));
                }
                Assert.HandleFail("Assert.AreEqual", finalMessage, parameters);
            }
        }

        static public void IsTrue(bool condition)
        {
            Assert.IsTrue(condition, string.Empty, null);
        }

        static public void IsTrue(bool condition, string message)
        {
            Assert.IsTrue(condition, message, null);
        }

        static public void IsTrue(bool condition, string message, params object[] parameters)
        {
            if (!condition)
            {
                Assert.HandleFail("Assert.IsTrue", message, parameters);
            }
        }

        static public void IsFalse(bool condition)
        {
            Assert.IsFalse(condition, string.Empty, null);
        }

        static public void IsFalse(bool condition, string message)
        {
            Assert.IsFalse(condition, message, null);
        }

        static public void IsFalse(bool condition, string message, params object[] parameters)
        {
            if (condition)
            {
                Assert.HandleFail("Assert.IsFalse", message, parameters);
            }
        }

        static public void IsNull(object value)
        {
            Assert.IsNull(value, string.Empty, null);
        }

        static public void IsNull(object value, string message)
        {
            Assert.IsNull(value, message, null);
        }

        static public void IsNull(object value, string message, params object[] parameters)
        {
            if (value != null)
            {
                Assert.HandleFail("Assert.IsNull", message, parameters);
            }
        }

        static public void IsNotNull(object value)
        {
            Assert.IsNotNull(value, string.Empty, null);
        }

        static public void IsNotNull(object value, string message)
        {
            Assert.IsNotNull(value, message, null);
        }

        static public void IsNotNull(object value, string message, params object[] parameters)
        {
            if (value == null)
            {
                Assert.HandleFail("Assert.IsNotNull", message, parameters);
            }
        }

        static public void Fail()
        {
            Assert.Fail(string.Empty, null);
        }

        static public void Fail(string message)
        {
            Assert.Fail(message, null);
        }

        static public void Fail(string message, params object[] parameters)
        {
            Assert.HandleFail("Assert.Fail", message, parameters);
        }

        static internal void HandleFail(string assertionName, string message, params object[] parameters)
        {
            string finalMessage = string.Empty;
            if (!string.IsNullOrEmpty(message))
            {
                if (parameters == null)
                {
                    finalMessage = ReplaceNulls(message);
                }
                else
                {
                    finalMessage = string.Format(ReplaceNulls(message), parameters);
                }
            }
            throw new Exception(String.Format("{0} - {1}", assertionName, finalMessage));
        }

        static internal string ReplaceNulls(object input)
        {
            // Use the localized "(null)" string for null values.
            if (null == input)
            {
                return "(null)";
            }
            else
            {
                // Convert it to a string.
                string inputString = input.ToString();

                // Make sure the class didn't override ToString and return null.
                if (inputString == null)
                {
                    return input.GetType().FullName;
                }

                return ReplaceNullChars(inputString);
            }
        }

        /// <summary>
        /// Replaces null characters ('\0') with "\\0".
        /// </summary>
        /// <param name="input">The string to search.</param>
        /// <returns>The converted string with null characters replaced by "\\0".</returns>
        public static string ReplaceNullChars(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Check for \0 in the middle of the display string.
            // Unfortunately we cannot use String.Replace or a regular expression
            // because both of those functions stop when they see a \0.

            // Count the zeros.
            List<int> zeroPos = new List<int>();
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\0')
                {
                    zeroPos.Add(i);
                }
            }

            if (zeroPos.Count > 0)
            {
                StringBuilder sb = new StringBuilder(input.Length + zeroPos.Count);

                // For each zero, add the string from the previous zero up to this one,
                // then add "\\0".
                int start = 0;
                foreach (int index in zeroPos)
                {
                    sb.Append(input.Substring(start, index - start));
                    sb.Append("\\0");
                    start = index + 1;
                }

                // Add the remainder of the string after the last zero
                sb.Append(input.Substring(start));
                return sb.ToString();
            }
            else
            {
                return input;
            }
        }

    }
}
