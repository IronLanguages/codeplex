/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System; using Microsoft;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting;
using System.Text;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Generation;
using System.Collections;
using Microsoft.Scripting.Utils;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Reflection;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// MethodCandidate represents the different possible ways of calling a method or a set of method overloads.
    /// A single method can result in multiple MethodCandidates. Some reasons include:
    /// - Every optional parameter or parameter with a default value will result in a candidate
    /// - The presence of ref and out parameters will add a candidate for languages which want to return the updated values as return values.
    /// - ArgumentKind.List and ArgumentKind.Dictionary can result in a new candidate per invocation since the list might be different every time.
    ///
    /// Each MethodCandidate represents the parameter type for the candidate using ParameterWrapper.
    /// 
    /// Contrast this with MethodTarget which represents the real physical invocation of a method
    /// </summary>
    public sealed class MethodCandidate {
        // TODO: merge with MethodTarget

        private readonly MethodTarget _target;
        private readonly List<ParameterWrapper> _parameters;
        private readonly ParameterWrapper _paramsDict;
        private readonly int _paramsArrayIndex;

        internal MethodCandidate(MethodTarget target, List<ParameterWrapper> parameters, ParameterWrapper paramsDict) {
            Assert.NotNull(target);
            Assert.NotNullItems(parameters);

            _target = target;
            _parameters = parameters;
            _paramsDict = paramsDict;

            _paramsArrayIndex = ParameterWrapper.IndexOfParamsArray(parameters);

            parameters.TrimExcess();
        }

        public int ParamsArrayIndex {
            get { return _paramsArrayIndex; }
        }

        public bool HasParamsArray {
            get { return _paramsArrayIndex != -1; }
        }

        public bool HasParamsDictionary {
            get { return _paramsDict != null; }
        }

        public MethodTarget Target {
            get { return _target; }
        }

        public OverloadResolver MethodBinder {
            get { return _target.MethodBinder; }
        }

        public ActionBinder Binder {
            get { return _target.MethodBinder.Binder; }
        }

        internal ParameterWrapper GetParameter(int argumentIndex, ArgumentBinding namesBinding) {
            return _parameters[namesBinding.ArgumentToParameter(argumentIndex)];
        }

        internal ParameterWrapper GetParameter(int parameterIndex) {
            return _parameters[parameterIndex];
        }

        internal int ParameterCount {
            get { return _parameters.Count; }
        }

        internal int IndexOfParameter(string name) {
            for (int i = 0; i < _parameters.Count; i++) {
                if (_parameters[i].Name == name) {
                    return i;
                }
            }
            return -1;
        }

        public int GetVisibleParameterCount() {
            int result = 0;
            foreach (var parameter in _parameters) {
                if (!parameter.IsHidden) {
                    result++;
                }
            }
            return result;
        }

        internal bool TryConvertArguments(ArgumentBinding namesBinding, NarrowingLevel narrowingLevel, out CallFailure failure) {
            var args = MethodBinder.GetActualArguments();
            Debug.Assert(args.Count == ParameterCount);

            BitArray hasConversion = new BitArray(args.Count);

            bool success = true;
            for (int i = 0; i < args.Count; i++) {
                success &= (hasConversion[i] = MethodBinder.CanConvertFrom(args[i].GetLimitType(), GetParameter(i, namesBinding), narrowingLevel));
            }

            if (!success) {
                var conversionResults = new ConversionResult[args.Count];
                for (int i = 0; i < args.Count; i++) {
                    conversionResults[i] = new ConversionResult(args[i].GetLimitType(), GetParameter(i, namesBinding).Type, !hasConversion[i]);
                }
                failure = new CallFailure(_target, conversionResults);
            } else {
                failure = null;
            }

            return success;
        }

        internal bool TryConvertCollapsedArguments(NarrowingLevel narrowingLevel, out CallFailure failure) {
            var args = MethodBinder.GetActualArguments();
            Debug.Assert(args.CollapsedCount > 0);

            // There must be at least one expanded parameter preceding splat index (see MethodBinder.GetSplatLimits):
            ParameterWrapper parameter = GetParameter(args.SplatIndex - 1);
            Debug.Assert(parameter.ParameterInfo != null && CompilerHelpers.IsParamArray(parameter.ParameterInfo));

            for (int i = 0; i < args.CollapsedCount; i++) {
                Type argType = CompilerHelpers.GetType(MethodBinder.GetCollapsedArgumentValue(i));

                if (!MethodBinder.CanConvertFrom(argType, parameter, narrowingLevel)) {
                    failure = new CallFailure(_target, new[] { new ConversionResult(argType, parameter.Type, false) });
                    return false;
                }
            }

            failure = null;
            return true;
        }

        internal static Candidate GetPreferredParameters(ApplicableCandidate one, ApplicableCandidate two) {
            Debug.Assert(one.Method.ParameterCount == two.Method.ParameterCount);
            var binder = one.Method.MethodBinder;
            var args = binder.GetActualArguments();

            Candidate result = Candidate.Equivalent;
            for (int i = 0; i < args.Count; i++) {
                Candidate preferred = ParameterWrapper.GetPreferredParameter(binder, one.GetParameter(i), two.GetParameter(i), args[i].GetLimitType());

                switch (result) {
                    case Candidate.Equivalent:
                        result = preferred;
                        break;

                    case Candidate.One:
                        if (preferred == Candidate.Two) return Candidate.Ambiguous;
                        break;

                    case Candidate.Two:
                        if (preferred == Candidate.One) return Candidate.Ambiguous;
                        break;

                    case Candidate.Ambiguous:
                        if (preferred != Candidate.Equivalent) {
                            result = preferred;
                        }
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            // TODO: process collapsed arguments:

            return result;
        }


        /// <summary>
        /// Builds a new MethodCandidate which takes count arguments and the provided list of keyword arguments.
        /// 
        /// The basic idea here is to figure out which parameters map to params or a dictionary params and
        /// fill in those spots w/ extra ParameterWrapper's.  
        /// </summary>
        internal MethodCandidate MakeParamsExtended(int count, IList<string> names) {
            Debug.Assert(BinderHelpers.IsParamsMethod(_target.Method));

            List<ParameterWrapper> newParameters = new List<ParameterWrapper>(count);
            
            // keep track of which named args map to a real argument, and which ones
            // map to the params dictionary.
            List<string> unusedNames = new List<string>(names);
            List<int> unusedNameIndexes = new List<int>();
            for (int i = 0; i < unusedNames.Count; i++) {
                unusedNameIndexes.Add(i);
            }

            // if we don't have a param array we'll have a param dict which is type object
            ParameterWrapper paramsArrayParameter = null;
            int paramsArrayIndex = -1;

            for (int i = 0; i < _parameters.Count; i++) {
                ParameterWrapper parameter = _parameters[i];

                if (parameter.IsParamsArray) {
                    paramsArrayParameter = parameter;
                    paramsArrayIndex = i;
                } else {
                    int j = unusedNames.IndexOf(parameter.Name);
                    if (j != -1) {
                        unusedNames.RemoveAt(j);
                        unusedNameIndexes.RemoveAt(j);
                    }
                    newParameters.Add(parameter);
                }
            }

            if (paramsArrayIndex != -1) {
                ParameterWrapper expanded = paramsArrayParameter.Expand();
                while (newParameters.Count < (count - unusedNames.Count)) {
                    newParameters.Insert(System.Math.Min(paramsArrayIndex, newParameters.Count), expanded);
                }
            }

            if (_paramsDict != null) {
                bool nonNullItems = CompilerHelpers.ProhibitsNullItems(_paramsDict.ParameterInfo);

                foreach (string name in unusedNames) {
                    newParameters.Add(new ParameterWrapper(_paramsDict.ParameterInfo, typeof(object), name, nonNullItems, false, false, _paramsDict.IsHidden));
                }
            } else if (unusedNames.Count != 0) {
                // unbound kw args and no where to put them, can't call...
                // TODO: We could do better here because this results in an incorrect arg # error message.
                return null;
            }

            // if we have too many or too few args we also can't call
            if (count != newParameters.Count) {
                return null;
            }

            return new MethodCandidate(_target.MakeParamsExtended(count, unusedNames.ToArray(), unusedNameIndexes.ToArray()), newParameters, null);
        }

        [Confined]
        public override string ToString() {
            return string.Format("MethodCandidate({0})", Target);
        }
    }
}
