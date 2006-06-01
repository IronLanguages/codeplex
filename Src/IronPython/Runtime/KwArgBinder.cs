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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Diagnostics;

namespace IronPython.Runtime {
    /// <summary>
    /// Helper class for performing keyword argument binding over multiple methods.
    /// 
    /// Class is constructed w/ args & keynames.  Binds can then be attempted for multiple
    /// delegates or function information by calling the DoBind overrides.  If a bind is 
    /// successful the .NET object array for calling the parameter is returned.  On failure
    /// null is returned and GetError() can be called to get the binding failure.
    /// </summary>
    class KwArgBinder {
        private object[] arguments;
        private string[] kwNames;       // keyword argument names provided at the call site
        private object[] realArgs;
        private bool[] haveArg;
        private int kwDictIndex = -1;
        private int paramArrayIndex = -1;
        private bool targetsCls;       // true if we target a CLS method, false if we target a Python function
        private string methodName = "unknown";
        private bool fAllowUnboundArgs; 
        private List<UnboundArgument> unboundArgs;
        private Exception error;

        public KwArgBinder(object[] args, string[] keyNames)
            : this(args, keyNames, false) {
        }

        public KwArgBinder(object[] args, string[] keyNames, bool allowUnboundArgs) {
            arguments = args;
            kwNames = keyNames;
            Debug.Assert(keyNames.Length <= args.Length);
            fAllowUnboundArgs = allowUnboundArgs;
        }

        /// <summary>
        /// Bind a MethodBase using the args & keyword names specified in the constructor
        /// </summary>
        public object[] DoBind(MethodBase target, string name) {
            ParameterInfo[] pis = target.GetParameters();
            string[] argNames = new string[pis.Length];
            object[] defaultVals = new object[pis.Length];
            methodName = target.Name;
            targetsCls = true;
            int kwDict = -1, paramsArray = -1;

            if (pis.Length > 0) {
                // populate argument information
                for (int i = 0; i < pis.Length; i++) {
                    argNames[i] = pis[i].Name;
                    defaultVals[i] = pis[i].DefaultValue;
                }

                if (pis[pis.Length - 1].IsDefined(typeof(ParamArrayAttribute), false)) {
                    paramsArray = pis.Length - 1;
                    if (pis.Length > 1 &&
                        pis[pis.Length - 2].IsDefined(typeof(ParamDictAttribute), false)) {
                        kwDict = pis.Length - 2;
                    }
                }else if (pis[pis.Length - 1].IsDefined(typeof(ParamDictAttribute), false)) {
                    paramsArray = pis.Length - 1;
                }  
            }

            return DoBind(name, argNames, defaultVals, kwDict, paramsArray);
        }

        /// <summary>
        /// provide the binding result for the specified arguments, values, and positions for kw dict & param array.
        /// </summary>
        /// argNames - Names of all arguments, including abnormal arguments like FuncDefFlags.ArgList or FuncDefFlags.KwDict
        /// defaultVals - There is one entry per argument. The entry will be DBNull.Value if there is no default value for the argument
        public object[] DoBind(string methName, string[] argNames, object[] defaultVals, int kwDict, int paramArray) {
            methodName = methName;
            realArgs = new object[argNames.Length];
            haveArg = new bool[argNames.Length];
            unboundArgs = null;
            kwDictIndex = kwDict;
            paramArrayIndex = paramArray;

            Debug.Assert(kwDict == -1 || kwDict < argNames.Length);
            Debug.Assert(paramArray == -1 || paramArray < argNames.Length);
            Debug.Assert(defaultVals.Length == argNames.Length);

            if (BindNormalArgs(argNames, kwDict, paramArray) &&
                BindKWArgs(argNames, kwDict, paramArray) &&
                IsValidCall(argNames, defaultVals)) {
                return realArgs;
            }

            return null;
        }

        private int GetNormalArgumentsCount() {
            int normalArgumentsCount = realArgs.Length;
            if (kwDictIndex != -1) normalArgumentsCount--;
            if (paramArrayIndex != -1) normalArgumentsCount--;
            return normalArgumentsCount;
        }

        /// <summary>
        /// Gets the error that caused the bind to fail, or null
        /// if no errors occured during the binding process.
        /// </summary>
        public Exception GetError() {
            return (error);
        }

        public bool AllowUnboundArgs {
            get {
                return (fAllowUnboundArgs);
            }
        }

        public List<UnboundArgument> UnboundArgs {
            get {
                return (unboundArgs);
            }
        }

        private static int FindParamIndex(string[] argNames, string name) {
            for (int i = 0; i < argNames.Length; i++) {
                if (argNames[i] == name) {
                    return (i);
                }
            }
            return (-1);
        }

        private bool BindNormalArgs(string[] argNames, int kwDict, int paramArrayIndex) {
            int maxNormalArgs = arguments.Length - kwNames.Length;

            for (int i = 0; i < maxNormalArgs; i++) {
                if (i == paramArrayIndex) {
                    haveArg[i] = true;
                    object[] paramArray = new object[maxNormalArgs - i];
                    for (int j = i; j < maxNormalArgs; j++) {
                        paramArray[j - i] = arguments[j];
                    }
                    realArgs[i] = targetsCls ? (object)paramArray : (object)Tuple.MakeTuple(paramArray);
                    return true;
                } else if (i == kwDict) {
                    // we shouldn't bind to the kwDict during normal arg binding
                    error = Ops.TypeError("{0}() takes exactly {1} argument ({2} given)", methodName, i, maxNormalArgs);
                    return false;
                } else if (i >= realArgs.Length) {
                    error = Ops.TypeError("{0}() takes exactly {1} argument ({2} given)", methodName, i, maxNormalArgs);
                    return false;
                }

                haveArg[i] = true;
                realArgs[i] = arguments[i];
            }
            if (paramArrayIndex != -1 && !haveArg[paramArrayIndex]) {
                realArgs[paramArrayIndex] = targetsCls ? (object)new object[0] : (object)Tuple.MakeTuple(new object[] { });
                haveArg[paramArrayIndex] = true;
            }
            return true;
        }

        private bool BindKWArgs(string[] argNames, int kwDictIndex, int paramArray) {
            bool fHasDict = false;
            Dict kwDict = null;
            if (kwDictIndex != -1) {
                // append kw value to dictionary
                realArgs[kwDictIndex] = kwDict = new Dict();
                haveArg[kwDictIndex] = true;
                fHasDict = true;
            }
            for (int i = 0; i < kwNames.Length; i++) {
                int index = FindParamIndex(argNames, kwNames[i]);
                int argumentsIndex = i + arguments.Length - kwNames.Length;
                if (index != -1 && index != kwDictIndex) {
                    // attempt to bind to a real arg
                    if (haveArg[index]) {
                        if (index == paramArray)
                            error = Ops.TypeError("{0}() got an unexpected keyword argument '{1}'", methodName, kwNames[i]);
                        else
                            error = Ops.TypeError("got multiple values for keyword argument {0}", kwNames[i]);
                        return false;
                    }

                    haveArg[index] = true;
                    realArgs[index] = arguments[argumentsIndex];
                } else if (fHasDict) {
                    // append kw value to dictionary
                    kwDict[kwNames[i]] = arguments[argumentsIndex];
                } else if (AllowUnboundArgs) {
                    if (unboundArgs == null) {
                        unboundArgs = new List<UnboundArgument>();
                    }
                    unboundArgs.Add(new UnboundArgument(kwNames[i], arguments[argumentsIndex]));
                } else {
                    error = Ops.TypeError("{0}() got an unexpected keyword argument '{1}'", methodName, kwNames[i]);
                    return false;
                }
            }
            return true;
        }

        private bool IsValidCall(string[] argNames, object[] defaultVals) {
            for (int i = 0; i < argNames.Length; i++) {
                if (!haveArg[i]) {
                    if (defaultVals != null && i < defaultVals.Length && defaultVals[i] != DBNull.Value) {
                        realArgs[i] = defaultVals[i];
                        haveArg[i] = true;
                    } else {
                        int realDefaultsCount = 0;
                        for (int d = 0; d < GetNormalArgumentsCount(); d++ ) {
                            if (defaultVals[d] != DBNull.Value) realDefaultsCount++;
                        }

                        error = PythonFunction.TypeErrorForIncorrectArgumentCount(methodName, GetNormalArgumentsCount(), realDefaultsCount, arguments.Length - kwNames.Length, paramArrayIndex != -1, true);
                        return false;
                    }
                }
            }
            return true;
        }
    }

    public class UnboundArgument {
        public UnboundArgument(string name, object value) {
            Name = name;
            Value = value;
        }

        public string Name;
        public object Value;
    }

}
