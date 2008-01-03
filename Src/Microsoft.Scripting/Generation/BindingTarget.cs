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

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using System.Reflection;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Encapsulates the result of an attempt to bind to one or methods using the MethodBinder.
    /// 
    /// Users should first check the Result property to see if the binding was successful or
    /// to determine the specific type of failure that occured.  If the binding was successful
    /// MakeExpression can then be called to create an expression which calls the method.
    /// If the binding was a failure callers can then create a custom error message based upon
    /// the reason the call failed.
    /// </summary>
    public sealed class BindingTarget {
        private readonly BindingResult _result;                                           // the result of the binding
        private readonly MethodTarget _target;                                            // the MethodTarget if the binding was successful 
        private readonly Type[] _argTests;                                                // if successful tests needed to disambiguate between overloads
        private readonly NarrowingLevel _level;                                           // the NarrowingLevel at which the target succeeds on conversion
        private readonly Dictionary<MethodBase, ConversionFailure[]> _conversionFailures; // if failed on conversion the various conversion failures for all overloads
        private readonly MethodBase[] _ambigiousMatches;                                  // list of methods which are ambigious to bind to.
        private readonly int[] _expectedArgs;                                             // gets the acceptable number of parameters which can be passed to the method.

        /// <summary>
        /// Creates a new BindingTarget when the method binding has succeeded
        /// </summary>
        internal BindingTarget(MethodTarget target, NarrowingLevel level, Type[] argTests) {
            _target = target;
            _argTests = argTests;
            _level = level;
        }

        /// <summary>
        /// Creates a new BindingTarget when the method binding has failed due to an incorrect argument count
        /// </summary>
        internal BindingTarget(int[] expectedArgCount) {
            _result = BindingResult.IncorrectArgumentCount;
            _expectedArgs = expectedArgCount;
        }

        /// <summary>
        /// Creates a new BindingTarget when the method binding has failued due to 
        /// one or more parameters which could not be converted.
        /// </summary>
        internal BindingTarget(Dictionary<MethodBase, ConversionFailure[]> failures) {
            _result = BindingResult.ConversionFailure;
            _conversionFailures = failures;
        }

        /// <summary>
        /// Creates a new BindingTarget when the match was ambigious
        /// </summary>
        internal BindingTarget(MethodBase[] ambigiousMatches) {
            _result = BindingResult.AmbigiousMatch;
            _ambigiousMatches = ambigiousMatches;
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
        public Expression/*!*/ MakeExpression(StandardRule/*!*/ rule, Expression/*!*/[]/*!*/ parameters, Type[] knownTypes) {
            if (_target == null) {
                throw new InvalidOperationException();
            }

            return _target.MakeExpression(rule, parameters, knownTypes);
        }

        /// <summary>
        /// Returns the method if the binding succeeded, or null if no method was applicable.
        /// </summary>
        public MethodBase Method {
            get {
                if (_target != null) {
                    return _target.Method;
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the methods which don't have any matches or null if Result == BindingResult.AmbigiousMatch
        /// </summary>
        public IEnumerable<MethodBase> AmbigiousMatches {
            get {
                return _ambigiousMatches;
            }
        }

        /// <summary>
        /// Returns the methods and their associated conversion failures if Result == BindingResult.ConversionFailure.
        /// </summary>
        public IEnumerable<KeyValuePair<MethodBase, ConversionFailure[]>> ConversionFailures {
            get {
                return _conversionFailures;
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
        /// Gets the type tests that need to be performed to ensure that a call is
        /// not applicable for an overload.
        /// 
        /// The members of the array correspond to each of the arguments.  An element is 
        /// null if no test is necessary.
        /// </summary>
        public IList<Type> ArgumentTests {
            get {
                return _argTests;
            }
        }

        /// <summary>
        /// Returns the return type of the binding, or null if no method was applicable.
        /// </summary>
        public Type ReturnType {
            get {
                if (_target != null) {
                    return _target.ReturnType;
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
    }
}
