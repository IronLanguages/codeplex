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
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.Calls {
    public delegate object OptimizingCallDelegate(object[] args, out bool shouldOptimize);

    /// <summary>
    /// Encapsulates the result of an attempt to bind to one or methods using the OverloadResolver.
    /// 
    /// Users should first check the Result property to see if the binding was successful or
    /// to determine the specific type of failure that occured.  If the binding was successful
    /// MakeExpression can then be called to create an expression which calls the method.
    /// If the binding was a failure callers can then create a custom error message based upon
    /// the reason the call failed.
    /// </summary>
    public sealed class BindingTarget {
        private readonly BindingResult _result;                                           // the result of the binding
        private readonly string _name;                                                    // the name of the method being bound to
        private readonly MethodCandidate _candidate;                                      // the selected method if the binding was successful 
        private readonly RestrictionInfo _restrictedArgs;                                 // the arguments after they've been restricted to their known types
        private readonly NarrowingLevel _level;                                           // the NarrowingLevel at which the target succeeds on conversion
        private readonly CallFailure[] _callFailures;                                     // if failed on conversion the various conversion failures for all overloads
        private readonly MethodCandidate[] _ambiguousMatches;                             // list of methods which are ambiguous to bind to.
        private readonly int[] _expectedArgs;                                             // gets the acceptable number of parameters which can be passed to the method.
        private readonly int _actualArgs;                                                 // gets the actual number of arguments provided

        /// <summary>
        /// Creates a new BindingTarget when the method binding has succeeded.
        /// </summary>
        internal BindingTarget(string name, int actualArgumentCount, MethodCandidate candidate, NarrowingLevel level, RestrictionInfo restrictedArgs) {
            _name = name;
            _candidate = candidate;
            _restrictedArgs = restrictedArgs;
            _level = level;
            _actualArgs = actualArgumentCount;
        }

        /// <summary>
        /// Creates a new BindingTarget when the method binding has failed due to an incorrect argument count
        /// </summary>
        internal BindingTarget(string name, int actualArgumentCount, int[] expectedArgCount) {
            _name = name;
            _result = BindingResult.IncorrectArgumentCount;
            _expectedArgs = expectedArgCount;
            _actualArgs = actualArgumentCount;
        }

        /// <summary>
        /// Creates a new BindingTarget when the method binding has failued due to 
        /// one or more parameters which could not be converted.
        /// </summary>
        internal BindingTarget(string name, int actualArgumentCount, CallFailure[] failures) {
            _name = name;
            _result = BindingResult.CallFailure;
            _callFailures = failures;
            _actualArgs = actualArgumentCount;
        }

        /// <summary>
        /// Creates a new BindingTarget when the match was ambiguous
        /// </summary>
        internal BindingTarget(string name, int actualArgumentCount, MethodCandidate[] ambiguousMatches) {
            _name = name;
            _result = BindingResult.AmbiguousMatch;
            _ambiguousMatches = ambiguousMatches;
            _actualArgs = actualArgumentCount;
        }

        /// <summary>
        /// Other failure.
        /// </summary>
        internal BindingTarget(string name, BindingResult result) {
            _name = name;
            _result = result;
        }

        /// <summary>
        /// Gets the result of the attempt to bind.
        /// </summary>
        public BindingResult Result {
            get {
                return _result;
            }
        }

        /// <summary>
        /// Gets an Expression which calls the binding target if the method binding succeeded.
        /// 
        /// Throws InvalidOperationException if the binding failed.
        /// </summary>
        public Expression MakeExpression() {
            if (_candidate == null) {
                throw new InvalidOperationException("An expression cannot be produced because the method binding was unsuccessful.");
            } else if (_restrictedArgs == null) {
                throw new InvalidOperationException("An expression cannot be produced because the method binding was done with Expressions, not MetaObject's");
            }

            Expression[] exprs = new Expression[_restrictedArgs.Objects.Length];
            for (int i = 0; i < exprs.Length; i++) {
                exprs[i] = _restrictedArgs.Objects[i].Expression;
            }

            return _candidate.MakeExpression(exprs);
        }

        public OptimizingCallDelegate MakeDelegate() {
            if (_candidate == null) {
                throw new InvalidOperationException("An expression cannot be produced because the method binding was unsuccessful.");
            } else if (_restrictedArgs == null) {
                throw new InvalidOperationException("An expression cannot be produced because the method binding was done with Expressions, not MetaObject's");
            }

            return _candidate.MakeDelegate(_restrictedArgs);
        }

        /// <summary>
        /// Returns the method if the binding succeeded, or null if no method was applicable.
        /// </summary>
        public MethodBase Method {
            get {
                if (_candidate != null) {
                    return _candidate.Method;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the name of the method as supplied to the OverloadResolver.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// Returns the MethodTarget if the binding succeeded, or null if no method was applicable.
        /// </summary>
        public MethodCandidate MethodCandidate {
            get {
                return _candidate;
            }
        }

        /// <summary>
        /// Returns the methods which don't have any matches or null if Result == BindingResult.AmbiguousMatch
        /// </summary>
        public IEnumerable<MethodCandidate> AmbiguousMatches {
            get {
                return _ambiguousMatches;
            }
        }

        /// <summary>
        /// Returns the methods and their associated conversion failures if Result == BindingResult.CallFailure.
        /// </summary>
        public ICollection<CallFailure> CallFailures {
            get {
                return _callFailures;
            }
        }

        /// <summary>
        /// Returns the acceptable number of arguments which can be passed to the method if Result == BindingResult.IncorrectArgumentCount.
        /// </summary>
        public IList<int> ExpectedArgumentCount {
            get {
                return _expectedArgs;
            }
        }

        /// <summary>
        /// Returns the total number of arguments provided to the call. 0 if the call succeeded or failed for a reason other
        /// than argument count mismatch.
        /// </summary>
        public int ActualArgumentCount {
            get {
                return _actualArgs;
            }
        }

        /// <summary>
        /// Gets the MetaObjects which we originally did binding against in their restricted form.
        /// 
        /// The members of the array correspond to each of the arguments.  All members of the array
        /// have a value.
        /// </summary>
        public RestrictionInfo RestrictedArguments {
            get {
                return _restrictedArgs;
            }
        }

        /// <summary>
        /// Returns the return type of the binding, or null if no method was applicable.
        /// </summary>
        public Type ReturnType {
            get {
                if (_candidate != null) {
                    return _candidate.ReturnType;
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the NarrowingLevel of the method if the call succeeded.  If the call
        /// failed returns NarrowingLevel.None.
        /// </summary>
        public NarrowingLevel NarrowingLevel {
            get {
                return _level;
            }
        }

        /// <summary>
        /// Returns true if the binding was succesful, false if it failed.
        /// 
        /// This is an alias for BindingTarget.Result == BindingResult.Success.
        /// </summary>
        public bool Success {
            get {
                return _result == BindingResult.Success;
            }
        }
    }
}
