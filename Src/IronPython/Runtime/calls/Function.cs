/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
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
using System.Threading;

using IronPython.Runtime.Operations;
using IronPython.Compiler;
using IronPython.Runtime.Types;
using IronPython.Hosting;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace IronPython.Runtime.Calls {
    /// <summary>
    /// Created for a user-defined function.  
    /// </summary>
    [PythonType("function")]
    public abstract partial class PythonFunction : FastCallable, IFancyCallable, IWeakReferenceable, ICustomMembers, IDynamicObject {
        private FunctionCode _code;
        private FunctionAttributes _flags;
        // Given "foo(a, b, c=3, *argList, **argDist), only a, b, and c are considered "normal" parameters
        private int _nparams;

        // Is there any FuncDefFlags.ArgList or FuncDefFlags.KwDict?
        private readonly string _name;
        private readonly string[] _argNames;
        private readonly object[] _defaults;
        private object _module;

        private CodeContext _context;
        private object _doc;
        private IAttributesCollection _dict;
        private int _id;

        // hi-perf thread static data:
        private static int[] _depth_fast = new int[20];
        private static int DepthSlow { get { return ThreadStatics.PythonFunction_Depth; } set { ThreadStatics.PythonFunction_Depth = value;  } }
        internal static int _MaximumDepth = 1001;          // maximum recursion depth allowed before we throw 
        internal static bool EnforceRecursion = false;    // true to enforce maximum depth, false otherwise
        private static int _CurrentId = 1;
                
        protected PythonFunction(CodeContext context, string name, string[] argNames, object[] defaults)
            : this(context, name, argNames, defaults, FunctionAttributes.None) {
        }

        protected PythonFunction(CodeContext context, string name, string[] argNames, object[] defaults, FunctionAttributes flags) {
            if (name == null) throw new ArgumentNullException("name");
            if (context == null) throw new ArgumentNullException("context");

            _name = name;
            _argNames = argNames;
            _defaults = defaults;
            _flags = flags;
            _nparams = argNames.Length;
            _context = context;

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
            FunctionCode = new FunctionCode(this);
        }

        public static PythonFunction MakeFunction(CodeContext context, string name, Delegate target, string[] argNames, object[] defaults,
            FunctionAttributes attributes, string docString, int lineNumber, string fileName, int flags) {
            PythonFunction ret = MakeFunction(context, name, target, argNames, defaults, attributes);
            if (docString != null) ret.Documentation = docString;
            ret.FunctionCode.SetLineNumber(lineNumber);
            ret.FunctionCode.SetFilename(fileName);
            ret.FunctionCode.SetFlags(context, flags);
            return ret;
        }
                       
        #region Public APIs

        public string[] ArgNames {
            get { return _argNames; }
        }

        public object FunctionGlobals {
            [PythonName("func_globals")]
            get {
                return new GlobalsDictionary(_context.Scope);
            }
        }

        public Tuple FunctionDefaults {
            [PythonName("func_defaults")]
            get {
                return new Tuple(_defaults);
            }
        }

        public string FunctionName {
            [PythonName("func_name")]
            get {
                return _name;
            }
        }
        public string Name {
            [PythonName("__name__")]
            get {
                return _name;
            }
        }

        public object FunctionDoc {
            [PythonName("func_doc")]
            get {
                return Documentation;
            }
        }

        public IAttributesCollection Dictionary {
            [PythonName("func_dict")]
            get {
                return EnsureDict();
            }
            [PythonName("func_dict")]
            set {
                _dict = value;
            }
        }

        public object Module {
            [PythonName("__module__")]
            get {
                return _module;
            }
            [PythonName("__module__")]
            set {
                _module = value;
            }
        }

        public virtual object Documentation {
            [PythonName("__doc__")]
            get {
                return _doc;
            }
            [PythonName("__doc__")]
            set {
                _doc = value;
            }
        }

        public FunctionCode FunctionCode {
            [PythonName("func_code")]
            get {
                return _code;
            }
            [PythonName("func_code")]
            set {
                _code = value;
            }
        }

        /*
        public object CallInterpreted(CodeContext context, params object[] args) {
            CodeContext funcContext = new CodeContext(
                new Microsoft.Scripting.Scope(context.Scope.ModuleScope, new PythonDictionary()),
                context.LanguageContext,
                context.ModuleContext);

            int i = 0;
            for (; i < args.Length; i++) {
                funcContext.Scope.SetName(_argNames[i], args[i]);
            }

            int di = i - (_argNames.Length - _defaults.Length);

            while (i < _argNames.Length) {
                funcContext.Scope.SetName(_argNames[i++], _defaults[di++]);
            }

            return _body.Execute(funcContext);
        }*/


        public override object CallInstance(CodeContext context, object instance, params object[] args) {
            return Call(context, PrependInstance(instance, args));
        }

        [OperatorMethod]
        public override object Call(CodeContext context, params object[] args) {            
            throw new NotImplementedException();
        }

        [OperatorMethod]
        public object Call(CodeContext context, [ParamDictionary]IAttributesCollection dictArgs, params object[] args) {
            object[] realArgs = new object[args.Length + dictArgs.Count];
            string[] argNames = new string[dictArgs.Count];

            Array.Copy(args, realArgs, args.Length);

            int index = 0;
            foreach (KeyValuePair<object, object> kvp in (IDictionary<object, object>)dictArgs) {
                argNames[index] = kvp.Key as string;
                realArgs[index + args.Length] = kvp.Value;
                index++;
            }

            return Call(context, realArgs, argNames);
        }

        public CodeContext Context {
            get {
                return _context;
            }
        }

        public object GetDefaultValue(int index) {
            return _defaults[index];
        }

        /// <summary>
        /// Captures the # of args and whether we have kw / arg lists.  This
        /// enables us to share sites for simple calls (calls that don't directly
        /// provide named arguments or the list/dict params).
        /// </summary>
        public int FunctionCompatibility {
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
        public int FunctionID {
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

        public static void PushFrame() {
            if (++Depth > _MaximumDepth)
                throw PythonOps.RuntimeError("maximum recursion depth exceeded");
        }

        public static void PopFrame() {
            --Depth;
        }

        public abstract Delegate Target { get; }

        #endregion

        #region Protected APIs

        protected internal object[] Defaults {
            get { return _defaults; }
        }

        public Exception BadArgumentError(int count) {
            return RuntimeHelpers.TypeErrorForIncorrectArgumentCount(Name, NormalArgumentCount, Defaults.Length, count, ExpandListPosition != -1, false);
        }

        public Exception BadKeywordArgumentError(int count) {
            return RuntimeHelpers.TypeErrorForIncorrectArgumentCount(Name, NormalArgumentCount, Defaults.Length, count, ExpandListPosition != -1, true);
        }

        protected int FindParamIndex(string name) {
            for (int i = 0; i < _argNames.Length; i++) {
                if (name == _argNames[i]) return i;
            }
            return -1;
        }

        protected int FindParamIndexOrError(string name) {
            int ret = FindParamIndex(name);
            if (ret != -1) return ret;
            throw PythonOps.TypeError("no parameter for " + name);
        }

        #endregion

        #region IFancyCallable Members

        public virtual object Call(CodeContext context, object[] args, string[] names) {
            int nparams = _argNames.Length;
            int nargs = args.Length - names.Length;
            if (nargs > nparams) throw BadArgumentError(nparams);

            object[] inArgs = args;
            args = new object[nparams];
            bool[] haveArg = new bool[nparams];
            for (int i = 0; i < nargs; i++) { args[i] = inArgs[i]; haveArg[i] = true; }

            for (int i = 0; i < names.Length; i++) {
                int paramIndex = FindParamIndex(names[i]);
                if (paramIndex == -1) {
                    throw PythonOps.TypeError("{0}() got an unexpected keyword argument '{1}'", _name, names[i]);
                }
                if (haveArg[paramIndex]) {
                    throw PythonOps.TypeError("multiple values for " + names[i]);
                }
                haveArg[paramIndex] = true;
                args[paramIndex] = inArgs[nargs + i];
            }

            for (int i = 0; i < nparams; i++) {
                if (!haveArg[i]) {
                    int defaultIndex = i - (nparams - _defaults.Length);
                    if (defaultIndex < 0) {
                        throw RuntimeHelpers.TypeErrorForIncorrectArgumentCount(_name, nparams, _defaults.Length, nargs, false, true);
                    }
                    args[i] = _defaults[defaultIndex];
                    haveArg[i] = true;
                }
            }

            return Call(context, args);
        }

        #endregion        

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetBoundCustomMember(context, name, out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            if (name == Symbols.Dict) {
                value = EnsureDict();
                return true;
            }

            if (_dict != null) {
                if (_dict.TryGetValue(name, out value)) {
                    return true;
                }
            }

            return TypeCache.Function.TryGetBoundMember(context, this, name, out value);
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            if (name == Symbols.Dict) {
                // our Python Dictionaries implement this explicitly
                IAttributesCollection d = value as IAttributesCollection;
                if (d == null) {
                    throw PythonOps.TypeError("__dict__ must be set to dictionary");
                }
                _dict = d;
            } else {
                EnsureDict()[name] = value;
            }
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            if (name == Symbols.Dict)
                throw PythonOps.TypeError(SymbolTable.IdToString(name) + " may not be deleted");

            if (_dict == null || !_dict.ContainsKey(name)) {
                // We check for Symbols.Module as the user code can modify it
                if (name == Symbols.Module)
                    throw PythonOps.TypeError(SymbolTable.IdToString(name) + " may not be deleted");

                throw PythonOps.AttributeError("no attribute {0}", name);
            }

            return _dict.Remove(name);
        }

        public IList<object> GetCustomMemberNames(CodeContext context) {
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

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            if (_dict == null) return new PythonDictionary(0);
            return (IDictionary<object, object>)_dict;
        }

        #endregion

        #region Object Overrides

        public override string ToString() {
            return string.Format("<function {0} at {1}>", FunctionName, PythonOps.HexId(this));
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
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

        private static int Depth {
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

        #region DynamicTypeSlot Overrides

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
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

        StandardRule<T> IDynamicObject.GetRule<T>(Action action, CodeContext context, object[] args) {
            if (action.Kind != ActionKind.Call)
                return null;

            return new FunctionBinderHelper<T>(context, (CallAction)action, this).MakeRule(CompilerHelpers.RemoveFirst(args));
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
            private List<Statement> _init;                          // a set of initialization code (e.g. creating a list for the params array)
            private Expression _error;                              // a custom error expression if the default needs to be overridden.
            private bool _extractedParams;                          // true if we needed to extract a parameter from the parameter list.
            private bool _extractedKeyword;                         // true if we needed to extract a parameter from the kw list.
            private Expression _userProvidedParams;                 // expression the user provided that should be expanded for params.

            public FunctionBinderHelper(CodeContext context, CallAction action, PythonFunction function) 
                : base(context, action) {
                _func = function;
            }

            public StandardRule<T> MakeRule(object[] args) {
                _rule.SetTarget(MakeTarget(args));
                _rule.SetTest(MakeTest());

                return _rule;
            }

            /// <summary>
            /// Makes the target for our rule.
            /// </summary>
            private Statement MakeTarget(object[] args) {
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
                if (IsSimpleAction) {
                    return MakeSimpleTest();
                } 

                return MakeComplexTest();                
            }

            /// <summary>
            /// Makes the test when we just have simple positional arguments.
            /// </summary>
            private Expression MakeSimpleTest() {
                return BinaryExpression.AndAlso(
                    _rule.MakeTypeTestExpression(_func.GetType(), 0),
                    BinaryExpression.Equal(
                        MemberExpression.Property(
                            GetFunctionParam(),
                            typeof(PythonFunction).GetProperty("FunctionCompatibility")),
                        new ConstantExpression(_func.FunctionCompatibility))
                );
            }

            /// <summary>
            /// Makes the test when we have a keyword argument call or splatting.
            /// </summary>
            /// <returns></returns>
            private Expression MakeComplexTest() {
                if (_extractedKeyword) {
                    _func.EnsureID();

                    return BinaryExpression.AndAlso(
                        _rule.MakeTypeTestExpression(_func.GetType(), 0),
                        BinaryExpression.Equal(
                            MemberExpression.Property(
                                GetFunctionParam(),
                                typeof(PythonFunction).GetProperty("FunctionID")),
                            new ConstantExpression(_func.FunctionID))
                    );
                }

                return BinaryExpression.AndAlso(
                    MakeSimpleTest(),
                    TypeBinaryExpression.TypeIs(
                        MemberExpression.Property(
                            GetFunctionParam(),
                            _func.GetType().GetProperty("Target")),
                        _func.Target.GetType())
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

                // walk all the provided args and find out where they go...
                for (int i = 0; i < args.Length; i++) {
                    if (!Action.IsSimple) {
                        if (Action.ArgumentKinds[i].ExpandDictionary) {
                            MakeDictionaryCopy(_rule.GetParameterExpression(i + 1)); 
                            continue;
                        } else if (Action.ArgumentKinds[i].ExpandList) {
                            _userProvidedParams = _rule.GetParameterExpression(i + 1);
                            continue;
                        } else if (Action.ArgumentKinds[i].Name != SymbolId.Empty) {
                            bool foundName = false;
                            for (int j = 0; j < _func.NormalArgumentCount; j++) {
                                if (_func.ArgNames[j] == SymbolTable.IdToString(Action.ArgumentKinds[i].Name)) {
                                    if (exprArgs[j] != null) {
                                        // kw-argument provided for already provided normal argument.
                                        return null;
                                    }

                                    exprArgs[j] = _rule.GetParameterExpression(i + 1);
                                    foundName = true;
                                    break;
                                }
                            }

                            if (!foundName) {
                                if (namedArgs == null) namedArgs = new Dictionary<SymbolId, Expression>();
                                namedArgs[Action.ArgumentKinds[i].Name] = _rule.GetParameterExpression(i + 1);
                            }
                            continue;
                        }
                    }

                    if (i < _func.NormalArgumentCount) {
                        exprArgs[i] = _rule.GetParameterExpression(i + 1);
                    } else {
                        if (extraArgs == null) extraArgs = new List<Expression>();
                        extraArgs.Add(_rule.GetParameterExpression(i + 1));
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
                            exprArgs[_func.ExpandListPosition] = MethodCallExpression.Call(
                                null,
                                typeof(PythonOps).GetMethod("GetOrCopyParamsTuple"),
                                _userProvidedParams);
                        } else {
                            // user provided a sequence to be expanded, and we may have used it,
                            // or we have extra args.
                            EnsureParams();

                            exprArgs[_func.ExpandListPosition] = MethodCallExpression.Call(
                                null,
                                typeof(Tuple).GetMethod("Make"),
                                BoundExpression.Defined(_params));

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
                args.Add(BoundExpression.Defined(_params));
                args.AddRange(paramsArgs);

                EnsureInit();

                _init.Add(
                    new ExpressionStatement(
                        MethodCallExpression.Call(null,
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
                        exprArgs[_func.ExpandDictPosition] = BoundExpression.Defined(_dict);
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
                _init.Add(new ExpressionStatement(
                    MethodCallExpression.Call(null,
                        typeof(PythonOps).GetMethod("AddDictionaryArgument"),
                        GetFunctionParam(),
                        ConstantExpression.Constant(SymbolTable.IdToString(kvp.Key)),
                        kvp.Value,
                        BoundExpression.Defined(_dict))));
            }

            /// <summary>
            /// Adds a check to the last parameter (so it's evaluated after we've extracted
            /// all the parameters) to ensure that we don't have any extra params or kw-params
            /// when we don't have a params array or params dict to expand them into.
            /// </summary>
            private void AddCheckForNoExtraParameters(Expression[] exprArgs) {
                if (exprArgs.Length > 0) {
                    List<Expression> tests = new List<Expression>(3);
                    tests.Add(exprArgs[exprArgs.Length - 1]);

                    // test we've used all of the extra parameters
                    if (_func.ExpandListPosition == -1 && _params != null) {
                        tests.Add(MethodCallExpression.Call(null,
                            typeof(PythonOps).GetMethod("CheckParamsZero"),
                            GetFunctionParam(),
                            BoundExpression.Defined(_params)));
                    }

                    // test that we've used all the extra named arguments
                    if (_func.ExpandDictPosition == -1 && _dict != null) {
                        tests.Add(MethodCallExpression.Call(null,
                            typeof(PythonOps).GetMethod("CheckDictionaryZero"),
                            GetFunctionParam(),
                            BoundExpression.Defined(_dict)));
                    }

                    // if we needed any tests have them run after the last argument.
                    if (tests.Count != 1) {
                        exprArgs[exprArgs.Length - 1] = CommaExpression.Comma(0, tests.ToArray());
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

                return MethodCallExpression.Call(null,
                    typeof(PythonOps).GetMethod("ExtractDictionaryArgument"),
                    GetFunctionParam(),                            // function
                    ConstantExpression.Constant(name),             // name
                    ConstantExpression.Constant(Action.ArgumentKinds.Length),   // arg count
                    BoundExpression.Defined(_dict)                              // dictionary
                );
            }

            /// <summary>
            /// Helper function to extract the variable from defaults, or to call a helper
            /// to check params / kw-dict / defaults to see which one contains the actual value.
            /// </summary>
            private Expression ExtractDefaultValue(int index, int dfltIndex) {
                if (_dict == null && _userProvidedParams == null) {
                    // we can pull the default directly
                    return MethodCallExpression.Call(
                      GetFunctionParam(),
                      typeof(PythonFunction).GetMethod("GetDefaultValue"),
                      new ConstantExpression(dfltIndex));
                } else {
                    // we might have a conflict, check the default last.
                    if (_userProvidedParams != null) {
                        EnsureParams();
                    }

                    return MethodCallExpression.Call(
                        null,
                        typeof(PythonOps).GetMethod("GetFunctionParameterValue"),
                        GetFunctionParam(),
                        ConstantExpression.Constant(dfltIndex),
                        ConstantExpression.Constant(_func.ArgNames[index]),
                        VariableOrNull(_params),
                        VariableOrNull(_dict)
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

                return MethodCallExpression.Call(null,
                    typeof(PythonOps).GetMethod("ExtractAnyArgument"),
                    GetFunctionParam(),                                         // function
                    ConstantExpression.Constant(name),             // name
                    BoundExpression.Defined(_paramsLen),   // arg count
                    BoundExpression.Defined(_params),                           // params list
                    BoundExpression.Defined(_dict)                              // dictionary
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

                return MethodCallExpression.Call(null,
                    typeof(PythonOps).GetMethod("ExtractParamsArgument"),
                    GetFunctionParam(),                                         // function
                    ConstantExpression.Constant(Action.ArgumentKinds.Length),   // arg count
                    BoundExpression.Defined(_params)                            // list
                );
            }

            private Expression VariableOrNull(Variable var) {                
                if (var != null) {
                    return BoundExpression.Defined(var);
                }
                return ConstantExpression.Constant(null);
            }

            /// <summary>
            /// Fixes up the argument list for the appropriate target delegate type.
            /// </summary>
            private Expression[] GetArgumentsForTargetType(Expression[] exprArgs) {
                if (_func.Target.GetType() == typeof(CallTargetWithContextN)) {
                    exprArgs = new Expression[] {
                        GetContextExpression(),
                        NewArrayExpression.NewArrayInit(typeof(object[]), exprArgs) 
                    };
                } else if (_func.Target.GetType() == typeof(CallTargetN)) {
                    exprArgs = new Expression[] {
                        NewArrayExpression.NewArrayInit(typeof(object[]), exprArgs) 
                    };
                } else if (NeedsContext) {
                    exprArgs = CompilerHelpers.PrependArray(GetContextExpression(), exprArgs);
                }

                return exprArgs;
            }
             
            /// <summary>
            /// Gets the number of positional arguments the user provided at the call site.
            /// </summary>
            /// <returns></returns>          
            private int GetProvidedPositionalArgumentCount() {
                if (Action.IsSimple) return _rule.Parameters.Length - 1;

                int cnt = Action.ArgumentKinds.Length;
                for (int i = 0; i < Action.ArgumentKinds.Length; i++) {
                    if (Action.ArgumentKinds[i].ExpandDictionary || 
                        Action.ArgumentKinds[i].ExpandList ||
                        Action.ArgumentKinds[i].Name != SymbolId.Empty) {
                        cnt--;
                    }
                }
                return cnt;
            }

            /// <summary>
            /// Helper function to get the function argument strongly typed.
            /// </summary>
            private StaticUnaryExpression GetFunctionParam() {
                return StaticUnaryExpression.Convert(_rule.GetParameterExpression(0), _func.GetType());
            }

            /// <summary>
            /// Helper function to get the functions CodeContext.
            /// </summary>
            private Expression GetContextExpression() {
                return MemberExpression.Property(GetFunctionParam(), _func.GetType().GetProperty("Context"));
            }
            
            /// <summary>
            /// Called when the user is expanding a dictionary - we copy the user
            /// dictionary and verify that it contains only valid string names.
            /// </summary>
            private void MakeDictionaryCopy(Expression userDict) {
                Debug.Assert(_dict == null);

                _dict = _rule.GetTemporary(typeof(PythonDictionary), "$dict");

                EnsureInit();
                _init.Add(new ExpressionStatement(
                        BoundAssignment.Assign(_dict,
                            MethodCallExpression.Call(
                                null,
                                typeof(PythonOps).GetMethod("CopyAndVerifyDictionary"),
                                GetFunctionParam(),
                                userDict
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

                _init.Add(new ExpressionStatement(
                        BoundAssignment.Assign(_params,
                            MethodCallExpression.Call(
                                null,
                                typeof(PythonOps).GetMethod("CopyAndVerifyParamsList"),
                                GetFunctionParam(),
                                userList
                            )
                        )
                    )
                );

                _init.Add(new ExpressionStatement(
                        BoundAssignment.Assign(_paramsLen,
                            BinaryExpression.Add(
                                MemberExpression.Property(BoundExpression.Defined(_params), typeof(List).GetProperty("Count")),
                                ConstantExpression.Constant(GetProvidedPositionalArgumentCount())
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

                int argCount = 2;
                if (namedArgs != null) {
                    argCount += namedArgs.Count;
                }

                Expression[] dictCreator = new Expression[argCount];
                BoundExpression dictRef = BoundExpression.Defined(_dict);

                dictCreator[0] = BoundAssignment.Assign(_dict,
                    MethodCallExpression.Call(null,
                        typeof(PythonOps).GetMethod("MakeDict"),
                        new ConstantExpression(0)
                    )
                );
                
                dictCreator[1] = dictRef;
                
                if (namedArgs != null) {
                    int index = 2;
                    foreach (KeyValuePair<SymbolId, Expression> kvp in namedArgs) {
                        dictCreator[index++] = MethodCallExpression.Call(
                            dictRef,
                            typeof(PythonDictionary).GetMethod("set_Item"),
                            new ConstantExpression(SymbolTable.IdToString(kvp.Key)),
                            kvp.Value);
                    }
                }

                return new CommaExpression(dictCreator, 1);
            }

            /// <summary>
            /// Helper function to create the expression for creating the actual tuple passed through.
            /// </summary>
            private Expression MakeParamsTuple(List<Expression> extraArgs) {
                if (extraArgs != null) {
                    return MethodCallExpression.Call(null, typeof(PythonOps).GetMethod("MakeTuple"), extraArgs.ToArray());
                } 
                return MethodCallExpression.Call(null, typeof(PythonOps).GetMethod("MakeTuple"));                
            }

            /// <summary>
            /// Creates the code to invoke the target delegate function w/ the specified arguments.
            /// </summary>
            private Statement MakeFunctionInvoke(Expression[] invokeArgs) {
                return _rule.MakeReturn(Context.LanguageContext.Binder,
                    MethodCallExpression.Call(
                        StaticUnaryExpression.Convert(
                            MemberExpression.Property(
                                GetFunctionParam(),
                                _func.GetType().GetProperty("Target")),
                            _func.Target.GetType()
                        ),
                        _func.Target.GetType().GetMethod("Invoke"),
                        invokeArgs
                    )
                );
            }

            /// <summary>
            /// Appends the initialization code for the call to the function if any exists.
            /// </summary>
            private Statement AddInitialization(Statement body) {
                if (_init == null) return body;

                List<Statement> res = new List<Statement>(_init);
                res.Add(body);
                return BlockStatement.Block(res);
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
            private Statement AddRecursionCheck(Statement stmt) {
                if (EnforceRecursion) {                    
                    stmt = BlockStatement.Block(
                        new ExpressionStatement(MethodCallExpression.Call(null, typeof(PythonFunction).GetMethod("PushFrame"))),
                        TryFinallyStatement.TryFinally(
                            stmt,
                            new ExpressionStatement(
                                MethodCallExpression.Call(null, typeof(PythonFunction).GetMethod("PopFrame")))));
                }
                return stmt;
            }

            private bool IsSimpleAction {
                get {
                    if (Action.IsSimple) return true;

                    foreach (ArgumentKind ac in Action.ArgumentKinds) {
                        if (ac.Name != SymbolId.Empty) return false;
                    }

                    return true;
                }
            }

            private bool KeywordCall {
                get {
                    if (Action.IsSimple) return false;

                    foreach (ArgumentKind ak in Action.ArgumentKinds) {
                        if (ak.ExpandDictionary || ak.Name != SymbolId.Empty) {
                            return true;
                        }
                    }
                    return false;
                }
            }

            private void MakeUnexpectedKeywordError(Dictionary<SymbolId, Expression> namedArgs) {
                string name = null;
                foreach (SymbolId id in namedArgs.Keys) {
                    name = SymbolTable.IdToString(id);
                    break;
                }

                _error = MethodCallExpression.Call(null,
                    typeof(PythonOps).GetMethod("UnexpectedKeywordArgumentError"),
                    GetFunctionParam(),
                    ConstantExpression.Constant(name));
            }

            private Statement MakeBadArgumentRule() {
                if (_error != null) {
                    return _rule.MakeError(Context.LanguageContext.Binder, _error);
                }

                return _rule.MakeError(Context.LanguageContext.Binder,
                    MethodCallExpression.Call(
                        GetFunctionParam(),
                        typeof(PythonFunction).GetMethod(KeywordCall ? "BadKeywordArgumentError" : "BadArgumentError"),
                        ConstantExpression.Constant(GetProvidedPositionalArgumentCount())
                    )
                );
            }

            private void EnsureInit() {
                if (_init == null) _init = new List<Statement>();
            }
        }

        #endregion
    }

    /// <summary>
    /// Targets a single delegate that takes many arguments.
    /// </summary>
    [PythonType(typeof(PythonFunction))]
    public partial class FunctionN : PythonFunction {
        public CallTargetWithContextN target;

        public FunctionN(CodeContext context, string name, CallTargetWithContextN target, string[] argNames, object[] defaults)
            : this(context, name, target, argNames, defaults, FunctionAttributes.None) {
        }

        public FunctionN(CodeContext context, string name, CallTargetWithContextN target, string[] argNames, object[] defaults, FunctionAttributes flags)
            : base(context, name, argNames, defaults, flags) {
            this.target = target;            
        }

        [OperatorMethod]
        public override object Call(CodeContext context, params object[] args) {
            int nparams = ArgNames.Length;
            int nargs = args.Length;
            if (nargs < nparams) {
                if (nargs + Defaults.Length < nparams) {
                    throw BadArgumentError(nargs);
                }
                object[] inArgs = args;
                args = new object[nparams];
                for (int i = 0; i < nargs; i++) args[i] = inArgs[i];
                object[] defs = Defaults;
                int di = defs.Length - 1;
                for (int i = nparams - 1; i >= nargs; i--) {
                    args[i] = defs[di--];
                }
            } else if (nargs > nparams) {
                throw BadArgumentError(nargs);
            }

            if (!EnforceRecursion) return target(Context, args);

            PushFrame();
            try {
                return target(Context, args);
            } finally {
                PopFrame();
            }
        }

        public override Delegate Target {
            get { return target; }
        }
    }

    /// <summary>
    /// Targets a single delegate that takes a variety of arguments (kw-arg list or arg list)
    /// </summary>
    [PythonType(typeof(PythonFunction))]
    public class FunctionX : FunctionN {
        public FunctionX(CodeContext context, string name, CallTargetWithContextN target, string[] argNames, object[] defaults, FunctionAttributes flags)
            : base(context, name, target, argNames, defaults, flags) {
        }

        [OperatorMethod]
        public override object Call(CodeContext context, params object[] args) {
            int nargs = args.Length;
            object argList = null;
            object[] outArgs = new object[ArgNames.Length];

            if (nargs < NormalArgumentCount) {
                if (nargs + Defaults.Length < NormalArgumentCount) {
                    throw BadArgumentError(nargs);
                }
                for (int i = 0; i < nargs; i++) outArgs[i] = args[i];
                object[] defs = Defaults;
                int di = defs.Length - 1;
                for (int i = NormalArgumentCount - 1; i >= nargs; i--) {
                    outArgs[i] = defs[di--];
                }
            } else if (nargs > NormalArgumentCount) {
                if (ExpandListPosition >= 0) {
                    for (int i = 0; i < NormalArgumentCount; i++) outArgs[i] = args[i];

                    object[] extraArgs = new object[nargs - NormalArgumentCount];
                    for (int i = 0; i < extraArgs.Length; i++) {
                        extraArgs[i] = args[i + NormalArgumentCount];
                    }
                    argList = Tuple.Make(extraArgs);
                } else {
                    throw BadArgumentError(nargs);
                }
            } else {
                for (int i = 0; i < nargs; i++) outArgs[i] = args[i];
            }

            if (ExpandListPosition >= 0) {
                if (argList == null) argList = Tuple.MakeTuple();
                outArgs[ExpandListPosition] = argList;
            }
            if (ExpandDictPosition >= 0) {
                outArgs[ExpandDictPosition] = new PythonDictionary(); //PyDictionary.make();
            }

            if (!EnforceRecursion) return target(Context, outArgs);

            PushFrame();
            try {
                return target(Context, outArgs);
            } finally {
                PopFrame();
            }
        }

        [OperatorMethod]
        public override object Call(CodeContext context, object[] args, string[] names) {
            KwArgBinder argBinder = new KwArgBinder(context, null, args, names);
            object[] defaults = this.Defaults;
            if (defaults.Length != ArgNames.Length) {
                // we need a 1<->1 mapping here for kwarg binder.  
                object[] newDefs = new object[ArgNames.Length];

                for (int i = 0; i < (NormalArgumentCount - defaults.Length); i++) {
                    newDefs[i] = DBNull.Value;
                }

                Array.Copy(defaults, 0, newDefs, (NormalArgumentCount - defaults.Length), defaults.Length);
                defaults = newDefs;
            }

            object[] realArgs = argBinder.DoBind(Name, ArgNames, defaults, ExpandDictPosition, ExpandListPosition);

            if (realArgs != null) {
                if (!EnforceRecursion) return target(Context, realArgs);
                PushFrame();
                try {
                    return target(Context, realArgs);
                } finally {
                    PopFrame();
                }
            } else if (argBinder.GetError() != null) {
                throw argBinder.GetError();
            } else {
                throw BadArgumentError(args.Length);
            }
        }
    }
}
