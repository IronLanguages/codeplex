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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if !SILVERLIGHT3
using System.Dynamic;
#endif
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using Microsoft.Scripting.Generation;
using ETUtils;

namespace AstTest {
    public static class Utils {
        internal static String[] GetArgumentValues(String arg) {
            String[] Parts = arg.Split(':');
            if (Parts.Length > 2) throw new ArgumentException("\":\" appears more than once in argument: " + arg);
            if (Parts.Length == 1) return null;
            if (Parts.Length != 2) throw new ArgumentException("Can't parse argument: " + arg);

            String Values = Parts[1];

            return Values.Split(new char[] { ',', ';' });

        }

        internal static bool ArgumentsHaveTestcaseNames(String[] args) {
            foreach (string s in args) {
                if (!(ArgIsSwitch(s))) return true;
            }
            return false;
        }

        internal static bool ArgIsSwitch(String arg) {
            return arg.Contains("/") || arg.Contains("-");
        }

        internal static bool InsensitiveStringInArray(String name, String[] args) {
            foreach (string s in args) {
#if SILVERLIGHT3
                if (name.ToLower().CompareTo(s.ToLower()) == 0) return true;
#else
                if (name.ToLowerInvariant().CompareTo(s.ToLowerInvariant()) == 0) return true;
#endif
            }

            return false;
        }

        internal static T CompileAndVerify<T>(this Expression<T> lambda) {
#if !SILVERLIGHT
            if (Scenarios.FullTrust) {
                Snippets.SetSaveAssemblies(true, null);
                var type = Snippets.Shared.DefinePublicType(new StackFrame(1).GetMethod().Name, typeof(object));
                var method = type.DefineMethod("Test", MethodAttributes.Public | MethodAttributes.Static);
                lambda.CompileToMethod(method, false);
                var f = (T)(object)Delegate.CreateDelegate(typeof(T), type.CreateType().GetMethod("Test"));
                Snippets.SaveAndVerifyAssemblies();
                return f;
            } else {
                return lambda.Compile();
            }
#else
            return lambda.Compile();

#endif
        }

        internal static void VerifyAssembly() {
#if !SILVERLIGHT
            Utils.PeVerifyAssemblyFile(CompileAsMethodUtils.SaveAssembly());
#endif
        }

        internal static void PeVerifyAssemblyFile(string fileLocation) {
#if !SILVERLIGHT
            string outDir = System.IO.Path.GetDirectoryName(fileLocation);
            string outFileName = System.IO.Path.GetFileName(fileLocation);
            string peverifyPath = FindPeverify();
            if (peverifyPath == null) {
                throw new Exception("Couldn't find PEVerify.exe");
            }

            int exitCode = 0;
            string strOut = null;
            string verifyFile = null;

            try {
                string assemblyFile = System.IO.Path.Combine(outDir, outFileName).ToLower(System.Globalization.CultureInfo.InvariantCulture);
                string assemblyName = System.IO.Path.GetFileNameWithoutExtension(outFileName);
                string assemblyExtension = System.IO.Path.GetExtension(outFileName);
                Random rnd = new System.Random();

                for (int i = 0; ; i++) {
                    string verifyName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}_{2}{3}", assemblyName, i, rnd.Next(1, 100), assemblyExtension);
                    verifyName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), verifyName);

                    try {
                        System.IO.File.Copy(assemblyFile, verifyName);
                        verifyFile = verifyName;
                        break;
                    } catch (System.IO.IOException) {
                    }
                }

                // /IGNORE=80070002 ignores errors related to files we can't find, this happens when we generate assemblies
                // and then peverify the result.  Note if we can't resolve a token thats in an external file we still
                // generate an error.
                ProcessStartInfo psi = new ProcessStartInfo(peverifyPath, "/IGNORE=80070002 \"" + verifyFile + "\"");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                Process proc = Process.Start(psi);
                System.Threading.Thread thread = new System.Threading.Thread(
                    new System.Threading.ThreadStart(
                        delegate {
                            using (System.IO.StreamReader sr = proc.StandardOutput) {
                                strOut = sr.ReadToEnd();
                            }
                        }
                        ));

                thread.Start();
                proc.WaitForExit();
                thread.Join();
                exitCode = proc.ExitCode;
                proc.Close();
            } catch (Exception e) {
                strOut = "Unexpected exception: " + e.ToString();
                exitCode = 1;
            }

            if (exitCode != 0) {
                Console.WriteLine("Verification failed w/ exit code {0}: {1}", exitCode, strOut);
                throw new Exception(
                    outFileName + " " +
                    verifyFile +
                    strOut ?? "");
            }

            if (verifyFile != null) {
                System.IO.File.Delete(verifyFile);
            }
#endif
        }

#if !SILVERLIGHT
        private static string PeverifyLocation;
#endif
        private static string FindPeverify() {
#if !SILVERLIGHT
            if (PeverifyLocation != null) return PeverifyLocation;
            const string peverify_exe = "peverify.exe";

            string path = System.Environment.GetEnvironmentVariable("PATH");
            path = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("PROGRAMFILES"), "Microsoft SDKs\\Windows\\v6.0A\\bin") + ";" + path;
            path = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("PROGRAMFILES"), "Microsoft SDKs\\Windows\\v7.0A\\bin") + ";" + path;

            string[] dirs = path.Split(';');
            foreach (string dir in dirs) {
                string file = System.IO.Path.Combine(dir, peverify_exe);
                if (System.IO.File.Exists(file)) {
                    PeverifyLocation = file;
                    return PeverifyLocation;
                }
            }
#endif
            return null;
        }

#if !SILVERLIGHT3
        internal static DynamicMetaObject CreateDynamicMetaObject(this DynamicMetaObjectBinder binder, Expression expr, BindingRestrictions restrictions) {
            return new DynamicMetaObject(Convert(expr, binder.ReturnType), restrictions);
        }

        internal static Expression Convert(Expression expr, Type to) {
            if (expr.Type == to) {
                return expr;
            }
            if (to == typeof(void)) {
                return Expression.Block(typeof(void), expr);
            }
            if (expr.Type == typeof(void)) {
                return Expression.Block(expr, Expression.Default(to));
            }
            return Expression.Convert(expr, to);
        }
#endif

        internal static U[] Map<T, U>(this ICollection<T> collection, Func<T, U> select) {
            int count = collection.Count;
            U[] result = new U[count];
            count = 0;
            foreach (T t in collection) {
                result[count++] = select(t);
            }
            return result;
        }

#if !SILVERLIGHT3
        public static DynamicMetaObject CreateThrow(this DynamicMetaObjectBinder binder, DynamicMetaObject target, DynamicMetaObject[] args, Type exception, params object[] exceptionArgVals) {
            Expression[] exceptionArgs = exceptionArgVals != null ? exceptionArgVals.Map<object, Expression>((arg) => Expression.Constant(arg)) : null;
            Type[] argTypes = exceptionArgs != null ? exceptionArgs.Map((arg) => arg.Type) : Type.EmptyTypes;
            ConstructorInfo constructor = exception.GetConstructor(argTypes);

            return binder.CreateDynamicMetaObject(
                Expression.Throw(
                    Expression.New(
                        exception.GetConstructor(argTypes),
                        exceptionArgs
                    )
                ),
                target.Restrictions.Merge(BindingRestrictions.Combine(args))
            );
        }
#endif
    }
}
