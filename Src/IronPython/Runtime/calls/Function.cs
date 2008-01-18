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
using System.Reflection;
using System.Text;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

using IronPython.Hosting;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Calls {
    /// <summary>
    /// Created for a user-defined function.  
    /// </summary>
    [PythonSystemType("function")]
    public sealed class PythonFunction : PythonTypeSlot, IWeakReferenceable, IMembersList, IDynamicObject, ICodeFormattable {
        private FunctionCode _code;
        private FunctionAttributes _flags;
        // Given "foo(a, b, c=3, *argList, **argDist), only a, b, and c are considered "normal" parameters
        private int _nparams;

        // Is there any FuncDefFlags.ArgList or FuncDefFlags.KwDict?
        private string _name;
        private readonly string[] _argNames;
        private object[] _defaults;
        private object _module;

        private CodeContext _context;
        private object _doc;
        private IAttributesCollection _dict;
        private int _id;
        private Delegate _target;

        // hi-perf thread static data:
        private static int[] _depth_fast = new int[20];
        [ThreadStatic]
        private static int DepthSlow;
        internal static int _MaximumDepth = 1001;          // maximum recursion depth allowed before we throw 
        internal static bool EnforceRecursion = false;    // true to enforce maximum depth, false otherwise
        private static int _CurrentId = 1;

        /// <summary>
        /// Python ctor - maps to function.__new__
        /// </summary>
        public PythonFunction(CodeContext context, FunctionCode code, IAttributesCollection globals) {
            throw new NotImplementedException();
        }

        internal PythonFunction(CodeContext context, string name, Delegate target, string[] argNames, object[] defaults, FunctionAttributes flags) {
            Contract.RequiresNotNull(name, "name");
            Contract.RequiresNotNull(context, "context");

            _name = name;
            _argNames = argNames;
            _defaults = defaults;
            _flags = flags;
            _nparams = argNames.Length;
            _context = context;
            _target = target;

            if ((flags & FunctionAttributes.KeywordDictionary) != 0) {
                _nparams--;
            }

            if ((flags & FunctionAttributes.ArgumentList) != 0) {
                _nparams--;
            }

            Debug.Assert(defaults.Length <= _nparams);

            if (name.IndexOf('#') > 0) {
                // dynamic method, strip the trailing id...
                this._name = name.Substring(0, name.IndexOf('#'));
            } else {
                this._name = name;
            }

            object modName;
            Debug.Assert(context.Scope != null, "null scope?");
            if (context.Scope.TryLookupName(context.LanguageContext, Symbols.Name, out modName)) {
                _module = modName;
            }
            func_code = new FunctionCode(this);
        }

        #region Public APIs

        public object func_globals {
            get {
                return new GlobalsDictionary(_context.Scope);
            }
        }

        public PythonTuple func_defaults {
            get {
                if (_defaults.Length == 0) return null;

                return new PythonTuple(_defaults);
            }
            set {
                if (value == null) {
                    _defaults = ArrayUtils.EmptyObjects;
                } else {
                    _defaults = value.ToArray();
                }
            }
        }

        public PythonTuple func_closure {
            get {
                Scope curScope = Context.Scope;
                List<ClosureCell> cells = new List<ClosureCell>();
                while (curScope != null) {
                    IFunctionEnvironment funcEnv = curScope.Dict as IFunctionEnvironment;
                    if (funcEnv != null) {
                        foreach (SymbolId si in funcEnv.Names) {
                            cells.Add(new ClosureCell(curScope.Dict[si]));
                        }
                    }

                    curScope = curScope.Parent;
                }

                if (cells.Count != 0) {
                    return PythonTuple.MakeTuple(cells.ToArray());
                }
                return null;
            }
            set {
                throw PythonOps.TypeError("readonly attribute");
            }
        }

        public string __name__ {
            get { return func_name; }
            set { func_name = value; }
        }

        public string func_name {
            get { return _name; }
            set {
                if (_name == null) throw PythonOps.TypeError("func_name must be set to a string object");
                _name = value;
            }
        }

        public IAttributesCollection __dict__ {
            get { return func_dict; }
            set { func_dict = value; }
        }

        public IAttributesCollection func_dict {
            get { return EnsureDict(); }
            set {
                if (value == null) throw PythonOps.TypeError("setting function's dictionary to non-dict");

                _dict = value;
            }
        }

        public object __doc__ {
            get { return _doc; }
            set { _doc = value; }
        }

        public object func_doc {
            get { return __doc__; }
            set { __doc__ = value; }
        }

        public object __module__ {
            get { return _module; }
            set { _module = value; }
        }

        public FunctionCode func_code {
            get { return _code; }
            set {
                if (value == null) throw PythonOps.TypeError("func_code must be set to a code object");
                _code = value;
            }
        }

        public object __call__(params object[] args) {
            return PythonCalls.Call(this, args);
        }

        public object __call__([ParamDictionary]IAttributesCollection dict, params object[] args) {
            return PythonCalls.CallWithKeywordArgs(this, args, dict);
        }

        #endregion

        #region Internal APIs

        internal string[] ArgNames {
            get { return _argNames; }
        }

        internal CodeContext Context {
            get {
                return _context;
            }
        }

        internal string GetSignatureString() {
            StringBuilder sb = new StringBuilder(__name__);
            sb.Append('(');
            for (int i = 0; i < _argNames.Length; i++) {
                if (i != 0) sb.Append(", ");

                if (i == ExpandDictPosition) {
                    sb.Append("**");
                } else if (i == ExpandListPosition) {
                    sb.Append("*");
                }

                sb.Append(ArgNames[i]);

                if (i < NormalArgumentCount) {
                    int noDefaults = NormalArgumentCount - Defaults.Length; // number of args w/o defaults
                    if (i - noDefaults >= 0) {
                        sb.Append('=');
                        sb.Append(PythonOps.Repr(Defaults[i - noDefaults]));
                    }
                }
            }
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// Captures the # of args and whether we have kw / arg lists.  This
        /// enables us to share sites for simple calls (calls that don't directly
        /// provide named arguments or the list/dict params).
        /// </summary>
        internal int FunctionCompatibility {
            get {
                // TODO: Invalidate sites when EnforceRecursion changes instead of 
                // tracking this info in a compat flag.
                return NormalArgumentCount |
                    Defaults.Length << 14 |
                    ((ExpandDictPosition != -1) ? 0x40000000 : 0) |
                    ((ExpandListPosition != -1) ? 0x20000000 : 0) |
                    (EnforceRecursion ? (unchecked((int)0x80000000)) : 0);
            }
        }

        /// <summary>
        /// Returns an ID for the function if one has been assigned, or zero if the
        /// function has not yet required the use of an ID.
        /// </summary>
        internal int FunctionID {
            get {
                return _id;
            }
        }

        /// <summary>
        /// Gets the position for the expand list argument or -1 if the function doesn't have an expand list parameter.
        /// </summary>
        internal int ExpandListPosition {
            get {
                if ((_flags & FunctionAttributes.ArgumentList) != 0) {
                    return _nparams;
                }

                return -1;
            }
        }

        /// <summary>
        /// Gets the position for the expand dictionary argument or -1 if the function doesn't have an expand dictionary parameter.
        /// </summary>
        internal int ExpandDictPosition {
            get {
                if ((_flags & FunctionAttributes.KeywordDictionary) != 0) {
                    if ((_flags & FunctionAttributes.ArgumentList) != 0) {
                        return _nparams + 1;
                    }
                    return _nparams;
                }
                return -1;
            }
        }

        /// <summary>
        /// Gets the number of normal (not params or kw-params) parameters.
        /// </summary>
        internal int NormalArgumentCount {
            get {
                return _nparams;
            }
        }

        /// <summary>
        /// Gets the number of extra arguments (params or kw-params)
        /// </summary>
        internal int ExtraArguments {
            get {
                if ((_flags & FunctionAttributes.ArgumentList) != 0) {
                    if ((_flags & FunctionAttributes.KeywordDictionary) != 0) {
                        return 2;
                    }
                    return 1;

                } else if ((_flags & FunctionAttributes.KeywordDictionary) != 0) {
                    return 1;
                }
                return 0;
            }
        }

        internal FunctionAttributes Flags {
            get {
                return _flags;
            }
        }

        internal Delegate Target { get { return _target; } }

        internal object[] Defaults {
            get { return _defaults; }
        }

        internal Exception BadArgumentError(int count) {
            return RuntimeHelpers.TypeErrorForIncorrectArgumentCount(__name__, NormalArgumentCount, Defaults.Length, count, ExpandListPosition != -1, false);
        }

        internal Exception BadKeywordArgumentError(int count) {
            return RuntimeHelpers.TypeErrorForIncorrectArgumentCount(__name__, NormalArgumentCount, Defaults.Length, count, ExpandListPosition != -1, true);
        }

        #endregion

        #region Private APIs

        private int FindParamIndex(string name) {
            for (int i = 0; i < _argNames.Length; i++) {
                if (name == _argNames[i]) return i;
            }
            return -1;
        }

        private int FindParamIndexOrError(string name) {
            int ret = FindParamIndex(name);
            if (ret != -1) return ret;
            throw PythonOps.TypeError("no parameter for " + name);
        }

        #endregion

        #region Custom member lookup operators

        [SpecialName, PythonHidden]
        public void SetMemberAfter(CodeContext context, string name, object value) {
            EnsureDict();

            _dict[SymbolTable.StringToId(name)] = value;
        }

        [SpecialName, PythonHidden]
        public object GetBoundMember(CodeContext context, string name) {
            object value;
            if (_dict != null && _dict.TryGetValue(SymbolTable.StringToId(name), out value)) {
                return value;
            }
            return OperationFailed.Value;
        }

        [SpecialNameAttribute, PythonHidden]
        public bool DeleteMember(CodeContext context, string name) {
            switch (name) {
                case "func_dict":
                case "__dict__":
                    throw PythonOps.TypeError("function's dictionary may not be deleted");
                case "__doc__":
                case "func_doc":
                    _doc = null;
                    return true;
                case "func_defaults":
                    _defaults = ArrayUtils.EmptyObjects;
                    return true;
            }

            if (_dict == null) return false;

            return _dict.Remove(SymbolTable.StringToId(name));
        }

        IList<object> IMembersList.GetMemberNames(CodeContext context) {
            List list;
            if (_dict == null) {
                list = List.Make();
            } else {
                list = List.Make(_dict);
            }
            list.AddNoLock(SymbolTable.IdToString(Symbols.Module));

            foreach (SymbolId id in TypeCache.Function.GetMemberNames(context, this)) {
                list.AddNoLockNoDups(id.ToString());
            }
            return list;
        }

        #endregion

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            if (_dict != null) {
                object weakRef;
                if (_dict.TryGetValue(Symbols.WeakRef, out weakRef)) {
                    return weakRef as WeakRefTracker;
                }
            }
            return null;
        }

        bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            EnsureDict();
            _dict[Symbols.WeakRef] = value;
            return true;
        }

        void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
            ((IWeakReferenceable)this).SetWeakRef(value);
        }

        #endregion

        #region Private APIs

        private IAttributesCollection EnsureDict() {
            if (_dict == null) {
                _dict = new SymbolDictionary();
            }
            return _dict;
        }

        internal static int Depth {
            get {
                // ManagedThreadId starts at 1 and increases as we get more threads.
                // Therefore we keep track of a limited number of threads in an array
                // that only gets created once, and we access each of the elements
                // from only a single thread.
                uint tid = (uint)Thread.CurrentThread.ManagedThreadId;

                return (tid < _depth_fast.Length) ? _depth_fast[tid] : DepthSlow;
            }
            set {
                uint tid = (uint)Thread.CurrentThread.ManagedThreadId;

                if (tid < _depth_fast.Length)
                    _depth_fast[tid] = value;
                else
                    DepthSlow = value;
            }
        }

        private void EnsureID() {
            if (_id == 0) {
                Interlocked.CompareExchange(ref _id, Interlocked.Increment(ref _CurrentId), 0);
            }
        }

        #endregion

        #region PythonTypeSlot Overrides

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = new Method(this, instance, owner);
            return true;
        }

        #endregion

        #region IDynamicObject Members

        LanguageContext IDynamicObject.LanguageContext {
            get {
                return _context.LanguageContext;
            }
        }

        StandardRule<T> IDynamicObject.GetRule<T>(DynamicAction action, CodeContext context, object[] args) {
            switch (action.Kind) {
                case DynamicActionKind.Call:
                    return new FunctionBinderHelper<T>(context, (CallAction)action, this).MakeRule(ArrayUtils.RemoveFirst(args));
                case DynamicActionKind.DoOperation:
                    return MakeDoOperationRule<T>((DoOperationAction)action, context, args);
            }
            return null;
        }

        private StandardRule<T> MakeDoOperationRule<T>(DoOperationAction doOperationAction, CodeContext context, object[] args) {
            switch (doOperationAction.Operation) {
                case Operators.IsCallable:
                    return PythonBinderHelper.MakeIsCallableRule<T>(context, this, true);
                case Operators.CallSignatures:
                    return MakeCallSignatureRule<T>(context);
            }
            return null;
        }

        private StandardRule<T> MakeCallSignatureRule<T>(CodeContext context) {
            string data = GetSignatureString();
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(PythonFunction));
            rule.SetTarget(
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("GetFunctionSignature"),
                        Ast.ConvertHelper(
                            rule.Parameters[0],
                            typeof(PythonFunction)
                        )
                    )
                )
            );
            return rule;
        }

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
        class FunctionBinderHelper<T> : BinderHelper<T, CallAction> {
            private PythonFunction _func;                           // the function we're calling
            private StandardRule<T> _rule = new StandardRule<T>();  // the rule we're producing
            private Variable _dict, _params, _paramsLen;            // splatted dictionary & params + the initial length of the params array, null if not provided.
            private List<Expression> _init;                          // a set of initialization code (e.g. creating a list for the params array)
            private Expression _error;                              // a custom error expression if the default needs to be overridden.
            private bool _extractedParams;                          // true if we needed to extract a parameter from the parameter list.
            private bool _extractedKeyword;                         // true if we needed to extract a parameter from the kw list.
            private Expression _userProvidedParams;                 // expression the user provided that should be expanded for params.
            private Expression _paramlessCheck;                     // tests when we have no parameters

            public FunctionBinderHelper(CodeContext context, CallAction action, PythonFunction function)
                : base(context, action) {
                _func = function;
            }

            public StandardRule<T> MakeRule(object[] args) {
                //Remove the passed in instance argument if present
                int instanceIndex = Action.Signature.IndexOf(ArgumentKind.Instance);
                if (instanceIndex > -1) {
                    args = ArrayUtils.RemoveAt(args, instanceIndex);
                }

                _rule.SetTarget(MakeTarget(args));
                _rule.SetTest(MakeTest());

                return _rule;
            }

            /// <summary>
            /// Makes the target for our rule.
            /// </summary>
            private Expression MakeTarget(object[] args) {
                Expression[] invokeArgs = GetArgumentsForRule(args);

                if (invokeArgs != null) {
                    return AddRecursionCheck(AddInitialization(MakeFunctionInvoke(invokeArgs)));
                }

                return MakeBadArgumentRule();
            }

            /// <summary>
            /// Makes the test for our rule.
            /// </summary>
            private Expression MakeTest() {
                if (!Action.Signature.HasKeywordArgument()) {
                    return MakeSimpleTest();
                }

                return MakeComplexTest();
            }

            /// <summary>
            /// Makes the test when we just have simple positional arguments.
            /// </summary>
            private Expression MakeSimpleTest() {
                return Ast.AndAlso(
                    _rule.MakeTypeTestExpression(_func.GetType(), 0),
                    Ast.AndAlso(
                        Ast.TypeIs(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("FunctionGetTarget"),
                                Ast.Convert(_rule.Parameters[0], typeof(PythonFunction))
                            ),
                            _func.Target.GetType()
                        ),
                        Ast.Equal(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("FunctionGetCompatibility"),
                                Ast.Convert(_rule.Parameters[0], typeof(PythonFunction))
                            ),
                            Ast.Constant(_func.FunctionCompatibility))
                    )
                );
            }

            /// <summary>
            /// Makes the test when we have a keyword argument call or splatting.
            /// </summary>
            /// <returns></returns>
            private Expression MakeComplexTest() {
                if (_extractedKeyword) {
                    _func.EnsureID();

                    return Ast.AndAlso(
                        _rule.MakeTypeTestExpression(_func.GetType(), 0),
                        Ast.Equal(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("FunctionGetID"),
                                Ast.Convert(_rule.Parameters[0], typeof(PythonFunction))
                            ),
                            Ast.Constant(_func.FunctionID))
                    );
                }

                return Ast.AndAlso(
                    MakeSimpleTest(),
                    Ast.TypeIs(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("FunctionGetTarget"),
                            GetFunctionParam()
                        ),
                        _func.Target.GetType()
                    )
                );
            }

            /// <summary>
            /// Gets the array of expressions which correspond to each argument for the function.  These
            /// correspond with the function as it's defined in Python and must be transformed for our
            /// delegate type before being used.
            /// </summary>
            private Expression[] GetArgumentsForRule(object[] args) {
                Expression[] exprArgs = new Expression[_func.NormalArgumentCount + _func.ExtraArguments];
                List<Expression> extraArgs = null;
                Dictionary<SymbolId, Expression> namedArgs = null;
                int instanceIndex = Action.Signature.IndexOf(ArgumentKind.Instance);

                // walk all the provided args and find out where they go...
                for (int i = 0; i < args.Length; i++) {
                    int parameterIndex = (instanceIndex == -1 || i < instanceIndex) ? i + 1 : i + 2;

                    switch (Action.Signature.GetArgumentKind(i)) {
                        case ArgumentKind.Dictionary:
                            MakeDictionaryCopy(_rule.Parameters[parameterIndex]);
                            continue;

                        case ArgumentKind.List:
                            _userProvidedParams = _rule.Parameters[parameterIndex];
                            continue;

                        case ArgumentKind.Named:
                            _extractedKeyword = true;
                            bool foundName = false;
                            for (int j = 0; j < _func.NormalArgumentCount; j++) {
                                if (_func.ArgNames[j] == SymbolTable.IdToString(Action.Signature.GetArgumentName(i))) {
                                    if (exprArgs[j] != null) {
                                        // kw-argument provided for already provided normal argument.
                                        return null;
                                    }

                                    exprArgs[j] = _rule.Parameters[parameterIndex];
                                    foundName = true;
                                    break;
                                }
                            }

                            if (!foundName) {
                                if (namedArgs == null) namedArgs = new Dictionary<SymbolId, Expression>();
                                namedArgs[Action.Signature.GetArgumentName(i)] = _rule.Parameters[parameterIndex];
                            }
                            continue;
                    }

                    if (i < _func.NormalArgumentCount) {
                        exprArgs[i] = _rule.Parameters[parameterIndex];
                    } else {
                        if (extraArgs == null) extraArgs = new List<Expression>();
                        extraArgs.Add(_rule.Parameters[parameterIndex]);
                    }
                }

                if (!FinishArguments(exprArgs, extraArgs, namedArgs)) {
                    if (namedArgs != null && _func.ExpandDictPosition == -1) {
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
                int noDefaults = _func.NormalArgumentCount - _func.Defaults.Length; // number of args w/o defaults

                for (int i = 0; i < _func.NormalArgumentCount; i++) {
                    if (exprArgs[i] != null) continue;

                    if (i < noDefaults) {
                        exprArgs[i] = ExtractNonDefaultValue(_func.ArgNames[i]);
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
                if (_func.ExpandListPosition != -1) {
                    if (_userProvidedParams != null) {
                        if (_params == null && paramsArgs == null) {
                            // we didn't extract any params, we can re-use a Tuple or
                            // make a single copy.
                            exprArgs[_func.ExpandListPosition] = Ast.Call(
                                typeof(PythonOps).GetMethod("GetOrCopyParamsTuple"),
                                Ast.ConvertHelper(_userProvidedParams, typeof(object))
                            );
                        } else {
                            // user provided a sequence to be expanded, and we may have used it,
                            // or we have extra args.
                            EnsureParams();

                            exprArgs[_func.ExpandListPosition] = Ast.Call(
                                typeof(PythonTuple).GetMethod("Make"),
                                Ast.ConvertHelper(Ast.ReadDefined(_params), typeof(object))
                            );

                            if (paramsArgs != null) {
                                MakeParamsAddition(paramsArgs);
                            }
                        }
                    } else {
                        exprArgs[_func.ExpandListPosition] = MakeParamsTuple(paramsArgs);
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
                args.Add(Ast.ReadDefined(_params));
                args.AddRange(paramsArgs);

                EnsureInit();

                _init.Add(
                    Ast.Statement(
                        Ast.ComplexCallHelper(
                            typeof(PythonOps).GetMethod("AddParamsArguments"),
                            args.ToArray()
                        )
                    )
                );
            }

            /// <summary>
            /// Creates the argument for the dictionary expansion parameter.
            /// </summary>
            private bool TryFinishDictionary(Expression[] exprArgs, Dictionary<SymbolId, Expression> namedArgs) {
                if (_func.ExpandDictPosition != -1) {
                    if (_dict != null) {
                        // used provided a dictionary to be expanded
                        exprArgs[_func.ExpandDictPosition] = Ast.ReadDefined(_dict);
                        if (namedArgs != null) {
                            foreach (KeyValuePair<SymbolId, Expression> kvp in namedArgs) {
                                MakeDictionaryAddition(kvp);
                            }
                        }
                    } else {
                        exprArgs[_func.ExpandDictPosition] = MakeDictionary(namedArgs);
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
                    Ast.Statement(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("AddDictionaryArgument"),
                            Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                            Ast.Constant(SymbolTable.IdToString(kvp.Key)),
                            Ast.ConvertHelper(kvp.Value, typeof(object)),
                            Ast.ConvertHelper(Ast.ReadDefined(_dict), typeof(IAttributesCollection))
                        )
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
                if (_func.ExpandListPosition == -1) {
                    if (_params != null) {
                        // we used some params, they should have gone down to zero...
                        tests.Add(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("CheckParamsZero"),
                                Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                                Ast.ReadDefined(_params)
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
                if (_func.ExpandDictPosition == -1 && _dict != null) {
                    tests.Add(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CheckDictionaryZero"),
                            Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                            Ast.ConvertHelper(Ast.ReadDefined(_dict), typeof(IDictionary))
                        )
                    );
                }

                if (tests.Count != 0) {
                    if (exprArgs.Length != 0) {
                        // if we have arguments run the tests after the last arg is evaluated.
                        Expression last = exprArgs[exprArgs.Length - 1];
                        Variable temp = _rule.GetTemporary(last.Type, "$temp");
                        tests.Insert(0, Ast.Assign(temp, last));
                        tests.Add(Ast.Read(temp));
                        exprArgs[exprArgs.Length - 1] = Ast.Comma(tests.ToArray());
                    } else {
                        // otherwise run them right before the method call
                        _paramlessCheck = Ast.Comma(tests.ToArray());
                    }
                }
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
                    Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),              // function
                    Ast.Constant(name, typeof(string)),                                         // name
                    Ast.Constant(Action.Signature.ArgumentCount),                               // arg count
                    Ast.ConvertHelper(Ast.ReadDefined(_dict), typeof(IAttributesCollection))    // dictionary
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
                        Ast.Constant(_func.ArgNames[index], typeof(string)),
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
                    Ast.ReadDefined(_paramsLen),                                    // arg count
                    Ast.ReadDefined(_params),                                       // params list
                    Ast.ConvertHelper(Ast.ReadDefined(_dict), typeof(IDictionary))  // dictionary
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
                    Ast.Constant(Action.Signature.ArgumentCount),                   // arg count
                    Ast.ReadDefined(_params)                                        // list
                );
            }

            private Expression VariableOrNull(Variable var, Type type) {
                if (var != null) {
                    return Ast.ConvertHelper(
                        Ast.ReadDefined(var),
                        type
                    );
                }
                return Ast.Null(type);
            }

            /// <summary>
            /// Fixes up the argument list for the appropriate target delegate type.
            /// </summary>
            private Expression[] GetArgumentsForTargetType(Expression[] exprArgs) {
                if (_func.Target.GetType() == typeof(CallTargetWithContextN)) {
                    exprArgs = new Expression[] {
                        GetContextExpression(),
                        Ast.NewArrayHelper(typeof(object[]), exprArgs) 
                    };
                } else if (_func.Target.GetType() == typeof(CallTargetN)) {
                    exprArgs = new Expression[] {
                        Ast.NewArrayHelper(typeof(object[]), exprArgs) 
                    };
                } else if (NeedsContext) {
                    exprArgs = ArrayUtils.Insert(GetContextExpression(), exprArgs);
                }

                return exprArgs;
            }

            /// <summary>
            /// Helper function to get the function argument strongly typed.
            /// </summary>
            private UnaryExpression GetFunctionParam() {
                return Ast.Convert(_rule.Parameters[0], _func.GetType());
            }

            /// <summary>
            /// Helper function to get the functions CodeContext.
            /// </summary>
            private Expression GetContextExpression() {
                return Ast.Call(typeof(PythonOps).GetMethod("FunctionGetContext"), GetFunctionParam());
            }

            /// <summary>
            /// Called when the user is expanding a dictionary - we copy the user
            /// dictionary and verify that it contains only valid string names.
            /// </summary>
            private void MakeDictionaryCopy(Expression userDict) {
                Debug.Assert(_dict == null);

                _dict = _rule.GetTemporary(typeof(PythonDictionary), "$dict");

                EnsureInit();
                _init.Add(Ast.Statement(
                        Ast.Assign(
                            _dict,
                            Ast.Call(
                                typeof(PythonOps).GetMethod("CopyAndVerifyDictionary"),
                                Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                                Ast.ConvertHelper(userDict, typeof(IDictionary))
                            )
                        )
                    )
                );
            }

            /// <summary>
            /// Called when the user is expanding a params argument
            /// </summary>
            private void MakeParamsCopy(Expression userList) {
                Debug.Assert(_params == null);

                _params = _rule.GetTemporary(typeof(List), "$list");
                _paramsLen = _rule.GetTemporary(typeof(int), "$paramsLen");

                EnsureInit();

                _init.Add(
                    Ast.Statement(
                        Ast.Assign(
                            _params,
                            Ast.Call(
                                typeof(PythonOps).GetMethod("CopyAndVerifyParamsList"),
                                Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                                Ast.ConvertHelper(userList, typeof(object))
                            )
                        )
                    )
                );

                _init.Add(Ast.Statement(
                        Ast.Assign(_paramsLen,
                            Ast.Add(
                                Ast.ReadProperty(Ast.ReadDefined(_params), typeof(List).GetProperty("Count")),
                                Ast.Constant(Action.Signature.GetProvidedPositionalArgumentCount())
                            )
                        )
                    )
                );
            }

            /// <summary>
            /// Called when the user hasn't supplied a dictionary to be expanded but the
            /// function takes a dictionary to be expanded.
            /// </summary>
            private Expression MakeDictionary(Dictionary<SymbolId, Expression> namedArgs) {
                Debug.Assert(_dict == null);
                _dict = _rule.GetTemporary(typeof(PythonDictionary), "$dict");

                int count = 2;
                if (namedArgs != null) {
                    count += namedArgs.Count;
                }

                Expression[] dictCreator = new Expression[count];
                BoundExpression dictRef = Ast.ReadDefined(_dict);

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
            private Expression MakeParamsTuple(List<Expression> extraArgs) {
                if (extraArgs != null) {
                    return Ast.ComplexCallHelper(
                        typeof(PythonOps).GetMethod("MakeTuple"),
                        extraArgs.ToArray()
                    );
                }
                return Ast.Call(
                    typeof(PythonOps).GetMethod("MakeTuple"),
                    Ast.NewArray(typeof(object[]))
                );
            }

            /// <summary>
            /// Creates the code to invoke the target delegate function w/ the specified arguments.
            /// </summary>
            private Expression MakeFunctionInvoke(Expression[] invokeArgs) {
                Expression invoke = Ast.SimpleCallHelper(
                    Ast.Convert(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("FunctionGetTarget"),
                            GetFunctionParam()
                        ),
                        _func.Target.GetType()
                    ),
                    _func.Target.GetType().GetMethod("Invoke"),
                    invokeArgs
                );

                if (_paramlessCheck != null) {
                    invoke = Ast.Comma(_paramlessCheck, invoke);
                }
                return _rule.MakeReturn(Context.LanguageContext.Binder, invoke);
            }

            /// <summary>
            /// Appends the initialization code for the call to the function if any exists.
            /// </summary>
            private Expression AddInitialization(Expression body) {
                if (_init == null) return body;

                List<Expression> res = new List<Expression>(_init);
                res.Add(body);
                return Ast.Block(res);
            }

            /// <summary>
            /// Determines if our target method takes CodeContext for it's first parameter (the
            /// method may be static & closed over the 1st parameter so we also check the 2nd).
            /// </summary>
            private bool NeedsContext {
                get {
                    ParameterInfo[] pi = _func.Target.Method.GetParameters();
                    if (pi.Length > 0 && pi[0].ParameterType == typeof(CodeContext) ||
                        (_func.Target.Target != null && _func.Target.Method.IsStatic && pi.Length > 1 && pi[1].ParameterType == typeof(CodeContext))) {
                        return true;
                    }
                    return false;
                }
            }

            /// <summary>
            /// Adds a try/finally which enforces recursion limits around the target method.
            /// </summary>
            private Expression AddRecursionCheck(Expression stmt) {
                if (EnforceRecursion) {
                    stmt = Ast.Block(
                        Ast.Statement(
                            Ast.Call(typeof(PythonOps).GetMethod("FunctionPushFrame"))
                        ),
                        Ast.TryFinally(
                            stmt,
                            Ast.Statement(
                                Ast.Call(typeof(PythonOps).GetMethod("FunctionPopFrame"))
                            )
                        )
                    );
                }
                return stmt;
            }

            private void MakeUnexpectedKeywordError(Dictionary<SymbolId, Expression> namedArgs) {
                string name = null;
                foreach (SymbolId id in namedArgs.Keys) {
                    name = SymbolTable.IdToString(id);
                    break;
                }

                _error = Ast.Call(
                    typeof(PythonOps).GetMethod("UnexpectedKeywordArgumentError"),
                    Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                    Ast.Constant(name, typeof(string))
                );
            }

            private Expression MakeBadArgumentRule() {
                if (_error != null) {
                    return _rule.MakeError(_error);
                }

                return _rule.MakeError(
                    Ast.Call(
                        typeof(PythonOps).GetMethod(Action.Signature.HasKeywordArgument() ? "BadKeywordArgumentError" : "FunctionBadArgumentError"),
                        Ast.ConvertHelper(GetFunctionParam(), typeof(PythonFunction)),
                        Ast.Constant(Action.Signature.GetProvidedPositionalArgumentCount())
                    )
                );
            }

            private void EnsureInit() {
                if (_init == null) _init = new List<Expression>();
            }
        }

        #endregion

        #region ICodeFormattable Members

        string ICodeFormattable.ToCodeString(CodeContext context) {
            return string.Format("<function {0} at {1}>", func_name, PythonOps.HexId(this));
        }

        #endregion
    }

    [PythonType("cell")]
    public class ClosureCell : ICodeFormattable, IValueEquality {
        private object _value;

        internal ClosureCell(object value) {
            _value = value;
        }

        public object Contents {
            [PythonName("cell_contents")]
            get {
                return _value;
            }
        }

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return String.Format("<cell at {0}: {1} object at {2}>",
                IdDispenser.GetId(this),
                PythonTypeOps.GetName(_value),
                IdDispenser.GetId(_value));
        }

        #endregion

        #region IValueEquality Members

        [SpecialNameAttribute]
        public int GetValueHashCode() {
            throw PythonOps.TypeError("unhashable type: cell");
        }

        public bool ValueEquals(object other) {
            return CompareTo(other) == 0;
        }

        public bool ValueNotEquals(object other) {
            return CompareTo(other) != 0;
        }

        #endregion

        [SpecialNameAttribute]
        public int CompareTo(object other) {
            ClosureCell cc = other as ClosureCell;
            if (cc == null) throw PythonOps.TypeError("cell.__cmp__(x,y) expected cell, got {0}", PythonTypeOps.GetName(other));

            return PythonOps.Compare(_value, cc._value);
        }
    }
}
