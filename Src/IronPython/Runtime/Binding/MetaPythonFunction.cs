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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Scripting.Actions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    class MetaPythonFunction : MetaPythonObject, IPythonInvokable {
        public MetaPythonFunction(Expression/*!*/ expression, Restrictions/*!*/ restrictions, PythonFunction/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(InvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args) {
            return new FunctionBinderHelper(pythonInvoke, this, args).MakeMetaObject();
        }

        #endregion
        
        #region MetaObject Overrides

        public override MetaObject/*!*/ Call(CallAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, args);
        }

        public override MetaObject/*!*/ Invoke(InvokeAction/*!*/ call, params MetaObject[] args) {
            return new FunctionBinderHelper(call, this, args).MakeMetaObject();
        }

        public override MetaObject/*!*/ Convert(ConvertAction/*!*/ conversion, MetaObject/*!*/[]/*!*/ args) {
            if (conversion.ToType.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateTarget(conversion, conversion.ToType, Restrict(typeof(PythonFunction)));
            }
            return conversion.Fallback(args);
        }

        #endregion

        #region Calls

        /// <summary>
        /// Performs the actual work of binding to the function.
        /// 
        /// Overall this works by going through the arguments and attempting to bind all the outstanding known
        /// arguments - position arguments and named arguments which map to parameters are easy and handled
        /// in the 1st pass for GetArgumentsForRule.  We also pick up any extra named or position arguments which
        /// will need to be passed off to a kw argument or a params array.
        /// 
        /// After all the normal args have been assigned to do a 2nd pass in FinishArguments.  Here we assign
        /// a value to either a value from the params list, kw-dict, or defaults.  If there is ambiguity between
        /// this (e.g. we have a splatted params list, kw-dict, and defaults) we call a helper which extracts them
        /// in the proper order (first try the list, then the dict, then the defaults).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class FunctionBinderHelper {
            private readonly MetaPythonFunction/*!*/ _func;         // the meta object for the function we're calling
            private readonly MetaObject/*!*/[]/*!*/ _args;          // the arguments for the function
            private readonly MetaObject/*!*/[]/*!*/ _originalArgs;  // the original arguments for the function
            private readonly MetaAction/*!*/ _call;               // the signature for the method call

            private List<VariableExpression>/*!*/ _temps;           // temporary variables allocated to create the rule
            private VariableExpression _dict, _params, _paramsLen;  // splatted dictionary & params + the initial length of the params array, null if not provided.
            private List<Expression> _init;                         // a set of initialization code (e.g. creating a list for the params array)
            private Expression _error;                              // a custom error expression if the default needs to be overridden.
            private bool _extractedParams;                          // true if we needed to extract a parameter from the parameter list.
            private bool _extractedKeyword;                         // true if we needed to extract a parameter from the kw list.
            private Expression _deferTest;                          // non-null if we have a test which could fail at runtime and we need to fallback to deferal
            private Expression _userProvidedParams;                 // expression the user provided that should be expanded for params.
            private Expression _paramlessCheck;                     // tests when we have no parameters

            public FunctionBinderHelper(MetaAction/*!*/ call, MetaPythonFunction/*!*/ function, MetaObject/*!*/[]/*!*/ args) {
                _call = call;
                _func = function;
                _args = ArrayUtils.RemoveFirst(args);
                _originalArgs = args;
                _temps = new List<VariableExpression>();

                // Remove the passed in instance argument if present
                int instanceIndex = Signature.IndexOf(ArgumentKind.Instance);
                if (instanceIndex > -1) {
                    _args = ArrayUtils.RemoveAt(_args, instanceIndex);
                }
            }

            public MetaObject/*!*/ MakeMetaObject() {
                Expression[] invokeArgs = GetArgumentsForRule();
                Restrictions restrict = _func.Restrictions.Merge(GetRestrictions().Merge(Restrictions.Combine(_args)));
                MetaObject res;

                if (invokeArgs != null) {
                    // successful call
                    Expression target = BindingHelpers.AddRecursionCheck(AddInitialization(MakeFunctionInvoke(invokeArgs)));

                    if (_temps.Count > 0) {
                        target = Ast.Scope(
                            target,
                            _temps
                        );
                    }

                    res = new MetaObject(
                        target,
                        restrict
                    );
                } else if (_error != null) {
                    // custom error generated while figuring out the call
                    res = new MetaObject(_error, restrict);
                } else {
                    // generic error
                    res = new MetaObject(
                        Ast.Throw(
                            Ast.Call(
                                typeof(PythonOps).GetMethod(Signature.HasKeywordArgument() ? "BadKeywordArgumentError" : "FunctionBadArgumentError"),
                                Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                                Ast.Constant(Signature.GetProvidedPositionalArgumentCount())
                            )
                        ),
                        restrict
                    );
                }

                
                return BindingHelpers.AddDynamicTestAndDefer(
                    _call, 
                    res, 
                    _originalArgs, 
                    new ValidationInfo(_deferTest, null),
                    res.Expression.Type     // force defer to our return type, our restrictions guarantee this to be true (only defaults can change, and we restrict to the delegate type)
                );
            }


            private CallSignature Signature {
                get {
                    return BindingHelpers.GetCallSignature(_call);
                }
            }

            /// <summary>
            /// Makes the test for our rule.
            /// </summary>
            private Restrictions/*!*/ GetRestrictions() {
                if (!Signature.HasKeywordArgument()) {
                    return GetSimpleRestriction();
                }

                return GetComplexRestriction();
            }

            /// <summary>
            /// Makes the test when we just have simple positional arguments.
            /// </summary>
            private Restrictions/*!*/ GetSimpleRestriction() {
                _deferTest = Ast.Equal(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("FunctionGetCompatibility"),
                        Ast.Convert(_func.Expression, typeof(PythonFunction))
                    ),
                    Ast.Constant(_func.Value.FunctionCompatibility)
                );

                return Restrictions.TypeRestriction(
                    _func.Expression, typeof(PythonFunction)
                ).Merge(
                    Restrictions.TypeRestriction(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("FunctionGetTarget"),
                            Ast.Convert(_func.Expression, typeof(PythonFunction))
                        ),
                        _func.Value.Target.GetType()
                    )
                );
            }

            /// <summary>
            /// Makes the test when we have a keyword argument call or splatting.
            /// </summary>
            /// <returns></returns>
            private Restrictions/*!*/ GetComplexRestriction() {
                if (_extractedKeyword) {
                    return Restrictions.InstanceRestriction(_func.Expression, _func.Value);
                }

                return GetSimpleRestriction();
            }

            /// <summary>
            /// Gets the array of expressions which correspond to each argument for the function.  These
            /// correspond with the function as it's defined in Python and must be transformed for our
            /// delegate type before being used.
            /// </summary>
            private Expression/*!*/[]/*!*/ GetArgumentsForRule() {
                Expression[] exprArgs = new Expression[_func.Value.NormalArgumentCount + _func.Value.ExtraArguments];
                List<Expression> extraArgs = null;
                Dictionary<SymbolId, Expression> namedArgs = null;
                int instanceIndex = Signature.IndexOf(ArgumentKind.Instance);

                // walk all the provided args and find out where they go...
                for (int i = 0; i < _args.Length; i++) {
                    int parameterIndex = (instanceIndex == -1 || i < instanceIndex) ? i : i + 1;

                    switch (Signature.GetArgumentKind(i)) {
                        case ArgumentKind.Dictionary:
                            MakeDictionaryCopy(_args[parameterIndex].Expression);
                            continue;

                        case ArgumentKind.List:
                            _userProvidedParams = _args[parameterIndex].Expression;
                            continue;

                        case ArgumentKind.Named:
                            _extractedKeyword = true;
                            bool foundName = false;
                            for (int j = 0; j < _func.Value.NormalArgumentCount; j++) {
                                if (_func.Value.ArgNames[j] == SymbolTable.IdToString(Signature.GetArgumentName(i))) {
                                    if (exprArgs[j] != null) {
                                        // kw-argument provided for already provided normal argument.
                                        return null;
                                    }

                                    exprArgs[j] = _args[parameterIndex].Expression;
                                    foundName = true;
                                    break;
                                }
                            }

                            if (!foundName) {
                                if (namedArgs == null) {
                                    namedArgs = new Dictionary<SymbolId, Expression>();
                                }
                                namedArgs[Signature.GetArgumentName(i)] = _args[parameterIndex].Expression;
                            }
                            continue;
                    }

                    if (i < _func.Value.NormalArgumentCount) {
                        exprArgs[i] = _args[parameterIndex].Expression;
                    } else {
                        if (extraArgs == null) {
                            extraArgs = new List<Expression>();
                        }
                        extraArgs.Add(_args[parameterIndex].Expression);
                    }
                }

                if (!FinishArguments(exprArgs, extraArgs, namedArgs)) {
                    if (namedArgs != null && _func.Value.ExpandDictPosition == -1) {
                        MakeUnexpectedKeywordError(namedArgs);
                    }

                    return null;
                }

                return GetArgumentsForTargetType(exprArgs);
            }

            /// <summary>
            /// Binds any missing arguments to values from params array, kw dictionary, or default values.
            /// </summary>
            private bool FinishArguments(Expression[] exprArgs, List<Expression> paramsArgs, Dictionary<SymbolId, Expression> namedArgs) {
                int noDefaults = _func.Value.NormalArgumentCount - _func.Value.Defaults.Length; // number of args w/o defaults

                for (int i = 0; i < _func.Value.NormalArgumentCount; i++) {
                    if (exprArgs[i] != null) {
                        if (_userProvidedParams != null && i >= Signature.GetProvidedPositionalArgumentCount()) {
                            exprArgs[i] = ValidateNotDuplicate(exprArgs[i], _func.Value.ArgNames[i], i);
                        }
                        continue;
                    }

                    if (i < noDefaults) {
                        exprArgs[i] = ExtractNonDefaultValue(_func.Value.ArgNames[i]);
                        if (exprArgs[i] == null) {
                            // can't get a value, this is an invalid call.
                            return false;
                        }
                    } else {
                        exprArgs[i] = ExtractDefaultValue(i, i - noDefaults);
                    }
                }

                if (!TryFinishList(exprArgs, paramsArgs) ||
                    !TryFinishDictionary(exprArgs, namedArgs))
                    return false;

                // add check for extra parameters.
                AddCheckForNoExtraParameters(exprArgs);

                return true;
            }

            /// <summary>
            /// Creates the argument for the list expansion parameter.
            /// </summary>
            private bool TryFinishList(Expression[] exprArgs, List<Expression> paramsArgs) {
                if (_func.Value.ExpandListPosition != -1) {
                    if (_userProvidedParams != null) {
                        if (_params == null && paramsArgs == null) {
                            // we didn't extract any params, we can re-use a Tuple or
                            // make a single copy.
                            exprArgs[_func.Value.ExpandListPosition] = Ast.Call(
                                typeof(PythonOps).GetMethod("GetOrCopyParamsTuple"),
                                Ast.ConvertHelper(_userProvidedParams, typeof(object))
                            );
                        } else {
                            // user provided a sequence to be expanded, and we may have used it,
                            // or we have extra args.
                            EnsureParams();

                            exprArgs[_func.Value.ExpandListPosition] = Ast.Call(
                                typeof(PythonOps).GetMethod("MakeTupleFromSequence"),
                                Ast.ConvertHelper(_params, typeof(object))
                            );

                            if (paramsArgs != null) {
                                MakeParamsAddition(paramsArgs);
                            }
                        }
                    } else {
                        exprArgs[_func.Value.ExpandListPosition] = MakeParamsTuple(paramsArgs);
                    }
                } else if (paramsArgs != null) {
                    // extra position args which are unused and no where to put them.
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Adds extra positional arguments to the start of the expanded list.
            /// </summary>
            private void MakeParamsAddition(List<Expression> paramsArgs) {
                _extractedParams = true;

                List<Expression> args = new List<Expression>(paramsArgs.Count + 1);
                args.Add(_params);
                args.AddRange(paramsArgs);

                EnsureInit();

                _init.Add(
                    Ast.ComplexCallHelper(
                        typeof(PythonOps).GetMethod("AddParamsArguments"),
                        args.ToArray()
                    )
                );
            }

            /// <summary>
            /// Creates the argument for the dictionary expansion parameter.
            /// </summary>
            private bool TryFinishDictionary(Expression[] exprArgs, Dictionary<SymbolId, Expression> namedArgs) {
                if (_func.Value.ExpandDictPosition != -1) {
                    if (_dict != null) {
                        // used provided a dictionary to be expanded
                        exprArgs[_func.Value.ExpandDictPosition] = _dict;
                        if (namedArgs != null) {
                            foreach (KeyValuePair<SymbolId, Expression> kvp in namedArgs) {
                                MakeDictionaryAddition(kvp);
                            }
                        }
                    } else {
                        exprArgs[_func.Value.ExpandDictPosition] = MakeDictionary(namedArgs);
                    }
                } else if (namedArgs != null) {
                    // extra named args which are unused and no where to put them.
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Adds an unbound keyword argument into the dictionary.
            /// </summary>
            /// <param name="kvp"></param>
            private void MakeDictionaryAddition(KeyValuePair<SymbolId, Expression> kvp) {
                _init.Add(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("AddDictionaryArgument"),
                        Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                        Ast.Constant(SymbolTable.IdToString(kvp.Key)),
                        Ast.ConvertHelper(kvp.Value, typeof(object)),
                        Ast.ConvertHelper(_dict, typeof(IAttributesCollection))
                    )
                );
            }

            /// <summary>
            /// Adds a check to the last parameter (so it's evaluated after we've extracted
            /// all the parameters) to ensure that we don't have any extra params or kw-params
            /// when we don't have a params array or params dict to expand them into.
            /// </summary>
            private void AddCheckForNoExtraParameters(Expression[] exprArgs) {
                List<Expression> tests = new List<Expression>(3);

                // test we've used all of the extra parameters
                if (_func.Value.ExpandListPosition == -1) {
                    if (_params != null) {
                        // we used some params, they should have gone down to zero...
                        tests.Add(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("CheckParamsZero"),
                                Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                                _params
                            )
                        );
                    } else if (_userProvidedParams != null) {
                        // the user provided params, we didn't need any, and they should be zero
                        tests.Add(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("CheckUserParamsZero"),
                                Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                                Ast.ConvertHelper(_userProvidedParams, typeof(object))
                            )
                        );
                    }
                }

                // test that we've used all the extra named arguments
                if (_func.Value.ExpandDictPosition == -1 && _dict != null) {
                    tests.Add(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CheckDictionaryZero"),
                            Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                            Ast.ConvertHelper(_dict, typeof(IDictionary))
                        )
                    );
                }

                if (tests.Count != 0) {
                    if (exprArgs.Length != 0) {
                        // if we have arguments run the tests after the last arg is evaluated.
                        Expression last = exprArgs[exprArgs.Length - 1];

                        VariableExpression temp;

                        _temps.Add(temp = Ast.Variable(last.Type, "$temp"));

                        tests.Insert(0, Ast.Assign(temp, last));
                        tests.Add(temp);
                        exprArgs[exprArgs.Length - 1] = Ast.Comma(tests.ToArray());
                    } else {
                        // otherwise run them right before the method call
                        _paramlessCheck = Ast.Comma(tests.ToArray());
                    }
                }
            }

            /// <summary>
            /// Helper function to validate that a named arg isn't duplicated with by
            /// a params list or the dictionary (or both).
            /// </summary>
            private Expression ValidateNotDuplicate(Expression value, string name, int position) {
                EnsureParams();

                return Ast.Comma(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("VerifyUnduplicatedByPosition"),
                        Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),    // function
                        Ast.Constant(name, typeof(string)),                               // name
                        Ast.Constant(position),                                           // position
                        _paramsLen                                                        // params list length
                        ),
                    value
                    );
            }

            /// <summary>
            /// Helper function to get a value (which has no default) from either the 
            /// params list or the dictionary (or both).
            /// </summary>
            private Expression ExtractNonDefaultValue(string name) {
                if (_userProvidedParams != null) {
                    // expanded params
                    if (_dict != null) {
                        // expanded params & dict
                        return ExtractFromListOrDictionary(name);
                    } else {
                        return ExtractNextParamsArg();
                    }
                } else if (_dict != null) {
                    // expanded dict
                    return ExtractDictionaryArgument(name);
                }

                // missing argument, no default, no expanded params or dict.
                return null;
            }

            /// <summary>
            /// Helper function to get the specified variable from the dictionary.
            /// </summary>
            private Expression ExtractDictionaryArgument(string name) {
                _extractedKeyword = true;

                return Ast.Call(
                        typeof(PythonOps).GetMethod("ExtractDictionaryArgument"),
                        Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),        // function
                        Ast.Constant(name, typeof(string)),                                   // name
                        Ast.Constant(Signature.ArgumentCount),                               // arg count
                        Ast.ConvertHelper(_dict, typeof(IAttributesCollection))               // dictionary
                    );
            }

            /// <summary>
            /// Helper function to extract the variable from defaults, or to call a helper
            /// to check params / kw-dict / defaults to see which one contains the actual value.
            /// </summary>
            private Expression ExtractDefaultValue(int index, int dfltIndex) {
                if (_dict == null && _userProvidedParams == null) {
                    // we can pull the default directly
                    return Ast.Call(
                      typeof(PythonOps).GetMethod("FunctionGetDefaultValue"),
                      Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                      Ast.Constant(dfltIndex)
                  );
                } else {
                    // we might have a conflict, check the default last.
                    if (_userProvidedParams != null) {
                        EnsureParams();
                    }
                    _extractedKeyword = true;
                    return Ast.Call(
                        typeof(PythonOps).GetMethod("GetFunctionParameterValue"),
                        Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                        Ast.Constant(dfltIndex),
                        Ast.Constant(_func.Value.ArgNames[index], typeof(string)),
                        VariableOrNull(_params, typeof(List)),
                        VariableOrNull(_dict, typeof(IAttributesCollection))
                    );
                }
            }

            /// <summary>
            /// Helper function to extract from the params list or dictionary depending upon
            /// which one has an available value.
            /// </summary>
            private Expression ExtractFromListOrDictionary(string name) {
                EnsureParams();

                _extractedKeyword = true;

                return Ast.Call(
                    typeof(PythonOps).GetMethod("ExtractAnyArgument"),
                    Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),  // function
                    Ast.Constant(name, typeof(string)),                             // name
                    _paramsLen,                                    // arg count
                    _params,                                       // params list
                    Ast.ConvertHelper(_dict, typeof(IDictionary))  // dictionary
                );
            }

            private void EnsureParams() {
                if (!_extractedParams) {
                    Debug.Assert(_userProvidedParams != null);
                    MakeParamsCopy(_userProvidedParams);
                    _extractedParams = true;
                }
            }

            /// <summary>
            /// Helper function to extract the next argument from the params list.
            /// </summary>
            private Expression ExtractNextParamsArg() {
                if (!_extractedParams) {
                    MakeParamsCopy(_userProvidedParams);

                    _extractedParams = true;
                }

                return Ast.Call(
                    typeof(PythonOps).GetMethod("ExtractParamsArgument"),
                    Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),  // function
                    Ast.Constant(Signature.ArgumentCount),                   // arg count
                    _params                                        // list
                );
            }

            private Expression VariableOrNull(VariableExpression var, Type type) {
                if (var != null) {
                    return Ast.ConvertHelper(
                        var,
                        type
                    );
                }
                return Ast.Null(type);
            }

            /// <summary>
            /// Fixes up the argument list for the appropriate target delegate type.
            /// </summary>
            private Expression/*!*/[]/*!*/ GetArgumentsForTargetType(Expression[] exprArgs) {
                Type target = _func.Value.Target.GetType();
                if (target == typeof(IronPython.Compiler.CallTargetN) || target == typeof(IronPython.Compiler.GeneratorTargetN)) {
                    exprArgs = new Expression[] {
                        Ast.NewArrayHelper(typeof(object), exprArgs) 
                    };
                }

                return exprArgs;
            }

            /// <summary>
            /// Helper function to get the function argument strongly typed.
            /// </summary>
            private UnaryExpression/*!*/ GetFunctionParam() {
                return Ast.Convert(_func.Expression, typeof(PythonFunction));
            }

            /// <summary>
            /// Called when the user is expanding a dictionary - we copy the user
            /// dictionary and verify that it contains only valid string names.
            /// </summary>
            private void MakeDictionaryCopy(Expression/*!*/ userDict) {
                Debug.Assert(_dict == null);

                _temps.Add(_dict = Ast.Variable(typeof(PythonDictionary), "$dict"));

                EnsureInit();
                _init.Add(
                    Ast.Assign(
                        _dict,
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CopyAndVerifyDictionary"),
                            Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                            Ast.ConvertHelper(userDict, typeof(IDictionary))
                        )
                    )
                );
            }

            /// <summary>
            /// Called when the user is expanding a params argument
            /// </summary>
            private void MakeParamsCopy(Expression/*!*/ userList) {
                Debug.Assert(_params == null);

                _temps.Add(_params = Ast.Variable(typeof(List), "$list"));
                _temps.Add(_paramsLen = Ast.Variable(typeof(int), "$paramsLen"));

                EnsureInit();

                _init.Add(
                    Ast.Assign(
                        _params,
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CopyAndVerifyParamsList"),
                            Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                            Ast.ConvertHelper(userList, typeof(object))
                        )
                    )
                );

                _init.Add(
                    Ast.Assign(_paramsLen,
                        Ast.Add(
                            Ast.Call(_params, typeof(List).GetMethod("__len__")),
                            Ast.Constant(Signature.GetProvidedPositionalArgumentCount())
                        )
                    )
                );
            }

            /// <summary>
            /// Called when the user hasn't supplied a dictionary to be expanded but the
            /// function takes a dictionary to be expanded.
            /// </summary>
            private Expression MakeDictionary(Dictionary<SymbolId, Expression/*!*/> namedArgs) {
                Debug.Assert(_dict == null);
                _temps.Add(_dict = Ast.Variable(typeof(PythonDictionary), "$dict"));

                int count = 2;
                if (namedArgs != null) {
                    count += namedArgs.Count;
                }

                Expression[] dictCreator = new Expression[count];
                VariableExpression dictRef = _dict;

                count = 0;
                dictCreator[count++] = Ast.Assign(
                    _dict,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("MakeDict"),
                        Ast.Zero()
                    )
                );

                if (namedArgs != null) {
                    foreach (KeyValuePair<SymbolId, Expression> kvp in namedArgs) {
                        dictCreator[count++] = Ast.Call(
                            dictRef,
                            typeof(PythonDictionary).GetMethod("set_Item", new Type[] { typeof(object), typeof(object) }),
                            Ast.Constant(SymbolTable.IdToString(kvp.Key), typeof(object)),
                            Ast.ConvertHelper(kvp.Value, typeof(object))
                        );
                    }
                }

                dictCreator[count] = dictRef;

                return Ast.Comma(dictCreator);
            }

            /// <summary>
            /// Helper function to create the expression for creating the actual tuple passed through.
            /// </summary>
            private Expression/*!*/ MakeParamsTuple(List<Expression> extraArgs) {
                if (extraArgs != null) {
                    return Ast.ComplexCallHelper(
                        typeof(PythonOps).GetMethod("MakeTuple"),
                        extraArgs.ToArray()
                    );
                }
                return Ast.Call(
                    typeof(PythonOps).GetMethod("MakeTuple"),
                    Ast.NewArrayInit(typeof(object[]))
                );
            }

            /// <summary>
            /// Generators follow different calling convention than 'regular' Python functions.
            /// When calling generator, we need to insert an additional argument to the call:
            /// an instance of the PythonGenerator. This is also result of the call.
            /// 
            /// DLR only provides generator lambdas that return IEnumerator so we are essentially
            /// wrapping it inside PythonGenerator.
            /// </summary>
            private bool IsGenerator(MethodInfo invoke) {
                ParameterInfo[] pis = invoke.GetParameters();
                return pis.Length > 0 && pis[0].ParameterType == typeof(PythonGenerator);
            }

            /// <summary>
            /// Creates the code to invoke the target delegate function w/ the specified arguments.
            /// </summary>
            private Expression/*!*/ MakeFunctionInvoke(Expression[] invokeArgs) {
                Type targetType = _func.Value.Target.GetType();
                MethodInfo method = targetType.GetMethod("Invoke");

                // If calling generator, create the instance of PythonGenerator first
                // and add it into the list of arguments
                Expression pg = null;
                if (IsGenerator(method)) {
                    _temps.Add((VariableExpression)(pg = Ast.Variable(typeof(PythonGenerator), "$gen")));

                    invokeArgs = ArrayUtils.Insert(
                        pg,
                        invokeArgs
                    );
                }

                Expression invoke = Ast.SimpleCallHelper(
                    Ast.Convert(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("FunctionGetTarget"),
                            GetFunctionParam()
                        ),
                        targetType
                    ),
                    method,
                    invokeArgs
                );

                int count = 1;
                if (_paramlessCheck != null) count++;
                if (pg != null) count += 2;

                if (count > 1) {
                    Expression[] comma = new Expression[count];

                    // Reset count for array fill
                    count = 0;

                    if (_paramlessCheck != null) {
                        comma[count++] = _paramlessCheck;
                    }

                    if (pg != null) {
                        // $gen = new PythonGenerator(context);
                        comma[count++] = Expression.Assign(
                            pg,
                            Expression.New(
                                TypeInfo._PythonGenerator.Ctor, 
                                Ast.Constant(BinderState.GetBinderState(_call).Context)
                            )
                        );
                        // $gen.Next = <call DLR generator method>($gen, ....)
                        comma[count++] = Expression.Call(typeof(PythonOps).GetMethod("InitializePythonGenerator"), pg, invoke);

                        if (_func.Value.IsGeneratorWithExceptionHandling) {
                            // function is a generator and it has try/except blocks.  We
                            // need to save/restore exception info but want to skip it
                            // for simple generators
                            pg = Ast.Call(
                                typeof(PythonOps).GetMethod("MarkGeneratorWithExceptionHandling"),
                                pg
                            );
                        }

                        // $gen is the value
                        comma[count++] = pg;
                    } else {
                        comma[count++] = invoke;
                    }

                    invoke = Expression.Comma(comma);
                }

                return invoke;
            }

            /// <summary>
            /// Appends the initialization code for the call to the function if any exists.
            /// </summary>
            private Expression/*!*/ AddInitialization(Expression body) {
                if (_init == null) return body;

                List<Expression> res = new List<Expression>(_init);
                res.Add(body);
                return Ast.Comma(res);
            }

            private void MakeUnexpectedKeywordError(Dictionary<SymbolId, Expression> namedArgs) {
                string name = null;
                foreach (SymbolId id in namedArgs.Keys) {
                    name = SymbolTable.IdToString(id);
                    break;
                }

                _error = Ast.Throw(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("UnexpectedKeywordArgumentError"),
                        Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                        Ast.Constant(name, typeof(string))
                    )
                );
            }            

            private void EnsureInit() {
                if (_init == null) _init = new List<Expression>();
            }
        }

        #endregion

        #region Helpers

        public new PythonFunction/*!*/ Value {
            get {
                return (PythonFunction)base.Value;
            }
        }

        #endregion
    }
}
