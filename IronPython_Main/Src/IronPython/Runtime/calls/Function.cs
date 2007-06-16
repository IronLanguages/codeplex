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
    [Flags]
    public enum CompileFlags {
        CO_NESTED = 0x0010,              //  nested_scopes
        CO_GENERATOR_ALLOWED = 0x1000,   //  generators
        CO_FUTURE_DIVISION = 0x2000,   //  division
    }

    [FlagsAttribute]
    public enum FunctionAttributes { None = 0, ArgumentList, KeywordDictionary }

    /// <summary>
    /// Represents a piece of code.  This can reference either a CompiledCode
    /// object or a Function.   The user can explicitly call FunctionCode by
    /// passing it into exec or eval.
    /// </summary>
    [PythonType("code")]
    public class FunctionCode {
        #region Private member variables
        private object varnames;
        private ScriptCode _code;
        private PythonFunction func;
        private string filename;
        private int lineNo;
        private FuncCodeFlags flags;      // future division, generator
        #endregion

        #region Flags
        [Flags]
        public enum FuncCodeFlags {
            VarArgs = 0x04,
            KwArgs = 0x08,
            Generator = 0x20,
            FutureDivision = 0x2000,
        }
        #endregion

        internal FunctionCode(ScriptCode code, CompileFlags compilerFlags)
            : this(code) {

            if ((compilerFlags & CompileFlags.CO_FUTURE_DIVISION) != 0)
                flags |= FuncCodeFlags.FutureDivision;
        }

        #region Public constructors
        public FunctionCode(ScriptCode code) {
            this._code = code;
        }

        public FunctionCode(PythonFunction f) {
            this.func = f;
        }
        #endregion

        #region Public Python API Surface
        public object VarNames {
            [PythonName("co_varnames")]
            get {
                if (varnames == null) {
                    varnames = GetArgNames();
                }
                return varnames;
            }
        }

        public object ArgCount {
            [PythonName("co_argcount")]
            get {
                if (_code != null) return 0;
                return func.ArgNames.Length;
            }
        }

        public object CallVars {
            [PythonName("co_cellvars")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object Code {
            [PythonName("co_code")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object Consts {
            [PythonName("co_consts")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object Filename {
            [PythonName("co_filename")]
            get {
                return filename;
            }
        }

        public object FirstLineNumber {
            [PythonName("co_firstlineno")]
            get {
                return lineNo;
            }
        }

        public object Flags {
            [PythonName("co_flags")]
            get {
                FuncCodeFlags res = flags;
                FunctionN funcN = func as FunctionN;
                FunctionX funcX = func as FunctionX;
                if (funcX != null) {
                    if ((funcX.Flags & FunctionAttributes.KeywordDictionary) != 0) res |= FuncCodeFlags.KwArgs;
                    if ((funcX.Flags & FunctionAttributes.ArgumentList) != 0) res |= FuncCodeFlags.VarArgs;
                } 
                else if (funcN != null) res |= FuncCodeFlags.VarArgs;
                
                return (int)res;
            }
        }

        public object FreeVars {
            [PythonName("co_freevars")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object LineNumberTab {
            [PythonName("co_lnotab")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object Name {
            [PythonName("co_name")]
            get {
                if (func != null) return func.Name;
                if (_code != null) return _code.GetType().Name;

                throw PythonOps.NotImplementedError("");
            }
        }

        public object Names {
            [PythonName("co_names")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object NumberLocals {
            [PythonName("co_nlocals")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object StackSize {
            [PythonName("co_stacksize")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }
        #endregion

        #region Public setters called from PythonFunction factory method
        internal void SetFilename(string sourceFile) {
            filename = sourceFile;
        }

        internal void SetLineNumber(int line) {
            lineNo = line;
        }

        // This is only used to set the value of FutureDivision and Generator flags
        internal void SetFlags(CodeContext context, int value) {
            this.flags = (FuncCodeFlags)value & (FuncCodeFlags.FutureDivision | FuncCodeFlags.Generator);
            if (((PythonContext)context.LanguageContext).TrueDivision) this.flags |= FuncCodeFlags.FutureDivision;
        }

        #endregion

        #region Internal API Surface

        public object Call(CodeContext context, Microsoft.Scripting.Scope scope) {
            if (_code != null) {
                return _code.Run(scope, context.ModuleContext);
            } else if (func != null) {
                return func.Call(context);
            }

            throw PythonOps.TypeError("bad code");
        }

        #endregion

        #region Private helper functions
        private Tuple GetArgNames() {
            if (_code != null) return Tuple.MakeTuple();

            List<string> names = new List<string>();
            List<Tuple> nested = new List<Tuple>();


            for (int i = 0; i < func.ArgNames.Length; i++) {
                if (func.ArgNames[i].IndexOf('#') != -1 && func.ArgNames[i].IndexOf('!') != -1) {
                    names.Add("." + (i * 2));
                    // TODO: need to get local variable names here!!!
                    //nested.Add(FunctionDefinition.DecodeTupleParamName(func.ArgNames[i]));
                } else {
                    names.Add(func.ArgNames[i]);
                }
            }

            for (int i = 0; i < nested.Count; i++) {
                ExpandArgsTuple(names, nested[i]);
            }
            return Tuple.Make(names);
        }

        private void ExpandArgsTuple(List<string> names, Tuple toExpand) {
            for (int i = 0; i < toExpand.Count; i++) {
                if (toExpand[i] is Tuple) {
                    ExpandArgsTuple(names, toExpand[i] as Tuple);
                } else {
                    names.Add(toExpand[i] as string);
                }
            }
        }
        #endregion

        public override bool Equals(object obj) {
            FunctionCode other = obj as FunctionCode;
            if (other == null) return false;

            if (_code != null) {
                return _code == other._code;
            } else if (func != null) {
                return func == other.func;
            }

            throw PythonOps.TypeError("bad code");
        }

        public override int GetHashCode() {
            if (_code != null) {
                return _code.GetHashCode();
            } else if (func != null) {
                return func.GetHashCode();
            }

            throw PythonOps.TypeError("bad code");
        }
    }

    /// <summary>
    /// Created for a user-defined function.  
    /// </summary>
    [PythonType("function")]
    public abstract partial class PythonFunction : FastCallable, IFancyCallable, IWeakReferenceable, ICustomMembers, IDynamicObject {

        // hi-perf thread static data:
        private static int[] _depth_fast = new int[20];
        private static int DepthSlow { get { return ThreadStatics.PythonFunction_Depth; } set { ThreadStatics.PythonFunction_Depth = value;  } }

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
        
        internal static int _MaximumDepth = 1001;          // maximum recursion depth allowed before we throw 
        internal static bool EnforceRecursion = false;    // true to enforce maximum depth, false otherwise

        #region instance members
        private readonly string _name;
        private readonly string[] _argNames;
        private readonly object[] _defaults;
        private object _module;

        private CodeContext _context;
        private object _doc;
        private FunctionCode _code;
        internal IAttributesCollection _dict;
        #endregion

        public static PythonFunction MakeFunction(CodeContext context, string name, Delegate target, string[] argNames, object[] defaults,
            FunctionAttributes attributes, string docString, int lineNumber, string fileName, int flags) {
            PythonFunction ret = MakeFunction(context, name, target, argNames, defaults, attributes);
            if (docString != null) ret.Documentation = docString;
            ret.FunctionCode.SetLineNumber(lineNumber);
            ret.FunctionCode.SetFilename(fileName);
            ret.FunctionCode.SetFlags(context, flags);
            return ret;
        }


        protected PythonFunction(CodeContext context, string name, string[] argNames, object[] defaults) {
            if (name == null) throw new ArgumentNullException("name");
            if (context == null) throw new ArgumentNullException("context");

            this._name = name;
            this._argNames = argNames;
            this._defaults = defaults;
            Debug.Assert(defaults.Length <= argNames.Length);
            if (name.IndexOf('#') > 0) {
                // dynamic method, strip the trailing id...
                this._name = name.Substring(0, name.IndexOf('#'));
            } else {
                this._name = name;
            }

            this._context = context;
            object modName;
            Debug.Assert(context.Scope != null, "null scope?");
            if (context.Scope.TryLookupName(context.LanguageContext, Symbols.Name, out modName)) {
                _module = modName;
            }
            this.FunctionCode = new FunctionCode(this);
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

        public int ArgCount {
            get {
                return _argNames.Length;
            }
        }

        #endregion

        public CodeContext Context {
            get {
                return _context;
            }
        }

        public object GetDefaultValue(int index) {
            return _defaults[index];
        }

        #region Protected APIs


        /// <summary>
        /// Captures a compatibility encoding w/ other functions.
        /// </summary>
        public int FunctionCompatibility {
            get {
                // TODO: Invalidate sites when EnforceRecursion changes instead of 
                // tracking this info in a compat flag.
                // TODO: Replace w/ GetDefaultValue function instead.
                return Defaults.Length << 14 | (EnforceRecursion ? 1 : 0);
            }
        }
        public object[] Defaults
        {
            get { return _defaults; }
        }

        public static void PushFrame() {
            if (++Depth > _MaximumDepth) 
                throw PythonOps.RuntimeError("maximum recursion depth exceeded");
        }

        public static void PopFrame() {
            --Depth;
        }

        
        protected virtual Exception BadArgumentError(int count) {
            return RuntimeHelpers.TypeErrorForIncorrectArgumentCount(_name, _argNames.Length, _defaults.Length, count);
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

        public abstract Delegate Target { get; }

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

        #region IDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, null); }

        [PythonName("__get__")]
        public virtual object GetAttribute(object instance, object owner) {
            return new Method(this, instance, owner);
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

        private IAttributesCollection EnsureDict() {
            if (_dict == null) {
                _dict = new SymbolDictionary();
            }
            return _dict;
        }

        #region DynamicTypeSlot Overrides

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = new Method(this, instance, owner);
            return true;
        }

        #endregion

        #region IDynamicObject Members

        public LanguageContext LanguageContext {
            get {
                return _context.LanguageContext;
            }
        }

        public virtual StandardRule<T> GetRule<T>(Action action, CodeContext context, object[] args) {
            if (action.Kind != ActionKind.Call)
                return null;

            object[] dest = new object[args.Length - 1];
            Array.Copy(args, 1, dest, 0, args.Length - 1);
            args = dest;

            //Console.Error.Write("Function Rule: {0} ", action, this.Name);
            StandardRule<T> rule = new StandardRule<T>();
            rule.SetTest(
                BinaryExpression.AndAlso(
                    rule.MakeTypeTestExpression(GetType(), 0),
                    BinaryExpression.Equal(
                        MemberExpression.Property(
                            StaticUnaryExpression.Convert(rule.GetParameterExpression(0), GetType()),
                            GetType().GetProperty("FunctionCompatibility")),
                        new ConstantExpression(FunctionCompatibility))
                )
             );
            Expression target;


            ParameterInfo[] pis = Target.Method.GetParameters();
            if ((args.Length < ArgCount && (_defaults == null || (_defaults.Length < (ArgCount - args.Length))))
                || args.Length > ArgCount
                || action != CallAction.Simple) {
                // Calling Target directly might not work, so fall back to slower code path
                target = CallBinderHelper<T>.MakeDynamicTarget(rule, (CallAction)action);
            } else {
                target = MethodCallExpression.Call(
                    StaticUnaryExpression.Convert(
                        MemberExpression.Property(
                            StaticUnaryExpression.Convert(rule.GetParameterExpression(0), GetType()),
                            GetType().GetProperty("Target")),
                        Target.GetType()),
                    Target.GetType().GetMethod("Invoke"),
                    GetArgumentsForRule<T>(rule, (CallAction)action, args));
            }

            Statement ret = rule.MakeReturn(context.LanguageContext.Binder, target);
            if (EnforceRecursion) {
                ret = BlockStatement.Block(
                    new ExpressionStatement(MethodCallExpression.Call(null, typeof(PythonFunction).GetMethod("PushFrame"))),
                    TryFinallyStatement.TryFinally(
                        ret,
                        new ExpressionStatement(
                            MethodCallExpression.Call(null, typeof(PythonFunction).GetMethod("PopFrame")))));
            } 
            rule.SetTarget(ret);
                        
            return rule;
        }

        private Expression[] GetArgumentsForRule<T>(StandardRule<T> rule, CallAction action, object[] args) {
            Expression[] exprArgs;
            ParameterInfo[] pis = Target.Method.GetParameters();
            int ctxIndex = -1;
            int startIndex = 0; // first index *after* the CodeContext (if any)
            // check for CodeContext in first two parameters (in case of instance method)
            for (int i = 0; i < Math.Min(2, pis.Length); i++) {
                if (pis[i].ParameterType == typeof(CodeContext)) {
                    ctxIndex = i;
                    break;
                }
            }
            if (ctxIndex != -1) {
                startIndex = 1;
                exprArgs = new Expression[ArgCount + 1];
                exprArgs[0] = MemberExpression.Property(
                    StaticUnaryExpression.Convert(
                        rule.GetParameterExpression(0),
                        GetType()),
                    GetType().GetProperty("Context"));

                Debug.Assert(args.Length <= exprArgs.Length - 1);
                for (int i = 0; i < args.Length; i++) {
                    exprArgs[i + 1] = rule.GetParameterExpression(i + 1);
                }
            } else {
                exprArgs = new Expression[ArgCount];
                Debug.Assert(args.Length <= exprArgs.Length);
                for (int i = 0; i < args.Length; i++) {
                    exprArgs[i] = rule.GetParameterExpression(i + 1);
                }
            }
            for (int j = args.Length; j < ArgCount; j++) {
                // default values
                                // offset       + # of defaults not consumed
                int dfltIndex = j - args.Length + (Defaults.Length - (ArgCount - args.Length));
                Debug.Assert(dfltIndex < Defaults.Length);
                Debug.Assert(dfltIndex >= 0);
                exprArgs[j+startIndex] = MethodCallExpression.Call(
                    StaticUnaryExpression.Convert(
                        rule.GetParameterExpression(0),
                        GetType()),
                    GetType().GetMethod("GetDefaultValue"),                    
                    new ConstantExpression(dfltIndex));
            }
#if DEBUG
            for (int i = 0; i < exprArgs.Length; i++) {
                Debug.Assert(exprArgs[i] != null);
            }
#endif
            return exprArgs;
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
            : base(context, name, argNames, defaults) {
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
        private FunctionAttributes _flags;
        // Given "foo(a, b, c=3, *argList, **argDist), only a, b, and c are considered "normal" parameters
        private int _nparams;
        // Is there any FuncDefFlags.ArgList or FuncDefFlags.KwDict?
        private int _extraArgs = 0;
        private int _argListPos = -1, _kwDictPos = -1;

        public FunctionX(CodeContext context, string name, CallTargetWithContextN target, string[] argNames, object[] defaults, FunctionAttributes flags)
            : base(context, name, target, argNames, defaults) {
            this._flags = flags;
            _nparams = argNames.Length;

            if ((flags & FunctionAttributes.KeywordDictionary) != 0) {
                _extraArgs++;
                _nparams--;
                _kwDictPos = _nparams;
            }

            if ((flags & FunctionAttributes.ArgumentList) != 0) {
                _extraArgs++;
                _nparams--;
                _argListPos = _nparams;
            }

            Debug.Assert(defaults.Length <= _nparams);
        }

        [OperatorMethod]
        public override object Call(CodeContext context, params object[] args) {
            int nargs = args.Length;
            object argList = null;
            object[] outArgs = new object[ArgNames.Length];

            if (nargs < _nparams) {
                if (nargs + Defaults.Length < _nparams) {
                    throw BadArgumentError(nargs);
                }
                for (int i = 0; i < nargs; i++) outArgs[i] = args[i];
                object[] defs = Defaults;
                int di = defs.Length - 1;
                for (int i = _nparams - 1; i >= nargs; i--) {
                    outArgs[i] = defs[di--];
                }
            } else if (nargs > _nparams) {
                if (_argListPos >= 0) {
                    for (int i = 0; i < _nparams; i++) outArgs[i] = args[i];

                    object[] extraArgs = new object[nargs - _nparams];
                    for (int i = 0; i < extraArgs.Length; i++) {
                        extraArgs[i] = args[i + _nparams];
                    }
                    argList = Tuple.Make(extraArgs);
                } else {
                    throw BadArgumentError(nargs);
                }
            } else {
                for (int i = 0; i < nargs; i++) outArgs[i] = args[i];
            }

            if (_argListPos >= 0) {
                if (argList == null) argList = Tuple.MakeTuple();
                outArgs[_argListPos] = argList;
            }
            if (_kwDictPos >= 0) {
                outArgs[_kwDictPos] = new PythonDictionary(); //PyDictionary.make();
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

                for (int i = 0; i < (_nparams - defaults.Length); i++) {
                    newDefs[i] = DBNull.Value;
                }

                Array.Copy(defaults, 0, newDefs, (_nparams - defaults.Length), defaults.Length);
                defaults = newDefs;
            }

            object[] realArgs = argBinder.DoBind(Name, ArgNames, defaults, _kwDictPos, _argListPos);

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

        protected override Exception BadArgumentError(int count) {
            return RuntimeHelpers.TypeErrorForIncorrectArgumentCount(Name, _nparams, Defaults.Length, count, _argListPos != -1, false);
        }

        internal FunctionAttributes Flags {
            get {
                return _flags;
            }
        }

        #region Rule Production

        /// <summary>
        /// Captures the # of args and whether we have kw / arg lists.  This
        /// enables us to share sites for simple calls (calls that don't directly
        /// provide named arguments or the list/dict params).
        /// </summary>
        public new int FunctionCompatibility {
            get {
                return _nparams |
                    Defaults.Length << 14 |
                    ((_kwDictPos != -1) ? 0x40000000 : 0) |
                    ((_argListPos != -1) ? 0x20000000 : 0);
            }
        }

        public override StandardRule<T> GetRule<T>(Action action, CodeContext context, object[] args) {
            if (action.Kind != ActionKind.Call)
                return null;

            object[] dest = new object[args.Length - 1];
            Array.Copy(args, 1, dest, 0, args.Length - 1);

            //Console.Error.Write("Function Rule: {0} ", action, this.Name);
            return MakeRule<T>(context, (CallAction)action, dest);
        }

        private StandardRule<T> MakeRule<T>(CodeContext context, CallAction action, object[] args) {
            StandardRule<T> rule = new StandardRule<T>();
            
            if (IsSimple(action)) {
                rule.SetTest(MakeSimpleTest<T>(rule));
            } else {
                rule.SetTest(MakeKeywordTest<T>(rule));
            }

            Expression target = null;

            // TODO: Remove this check and enable dynamic actions for other scenarios.
            if (action.IsSimple && 
                _nparams >= args.Length && !(_nparams != args.Length && (Defaults == null || (Defaults.Length < (_nparams - args.Length))))) {
                target = MakeTarget<T>(rule, action, args);
            } 

            target = target ?? CallBinderHelper<T>.MakeDynamicTarget(rule, action);

            rule.SetTarget(rule.MakeReturn(
                context.LanguageContext.Binder,
                target));
        
            return rule;
        }

        private bool IsSimple(CallAction action) {
            if (action.IsSimple) return true;

            foreach (ArgumentKind ac in action.ArgumentKinds) {
                if (ac.Name != SymbolId.Empty) return false;
            }

            return true;
        }

        private Expression MakeSimpleTest<T>(StandardRule<T> rule) {
            Debug.Assert(Target is CallTargetWithContextN);

            return BinaryExpression.AndAlso(
                rule.MakeTypeTestExpression(GetType(), 0),
                BinaryExpression.Equal(
                    MemberExpression.Property(
                        StaticUnaryExpression.Convert(rule.GetParameterExpression(0), GetType()),
                        GetType().GetProperty("FunctionCompatibility")),
                    new ConstantExpression(FunctionCompatibility))
            );
        }

        private Expression MakeKeywordTest<T>(StandardRule<T> rule) {
            // THIS ISN'T RIGHT, need to test instance equality
            return BinaryExpression.AndAlso(
                BinaryExpression.AndAlso(
                    rule.MakeTypeTestExpression(GetType(), 0),
                    TypeBinaryExpression.TypeIs(
                        MemberExpression.Property(
                            StaticUnaryExpression.Convert(rule.GetParameterExpression(0), GetType()),
                            GetType().GetProperty("Target")),
                        Target.GetType())),                        
                BinaryExpression.Equal(
                    MemberExpression.Property(
                        StaticUnaryExpression.Convert(rule.GetParameterExpression(0), GetType()),
                        GetType().GetProperty("FunctionCompatibility")),
                    new ConstantExpression(FunctionCompatibility))
            );
        }

        private Expression MakeTarget<T>(StandardRule<T> rule, CallAction action, object[] args) {
            Expression[] invokeArgs = GetArgumentsForRule<T>(rule, action, args);
            if (invokeArgs == null) return null;

            return MethodCallExpression.Call(
                StaticUnaryExpression.Convert(
                    MemberExpression.Property(
                        StaticUnaryExpression.Convert(rule.GetParameterExpression(0), GetType()),
                        GetType().GetProperty("Target")),
                    Target.GetType()),
                Target.GetType().GetMethod("Invoke"),
                invokeArgs);
        }

        private Expression[] GetArgumentsForRule<T>(StandardRule<T> rule, CallAction action, object[] args) {
            Expression[] exprArgs = new Expression[_nparams + _extraArgs];
            List<Expression> extraArgs = null;
            Dictionary<SymbolId, Expression> namedArgs = null;

            // walk all the provided args and find out where they go...
            for (int i = 0; i < args.Length; i++) {
                if (!action.IsSimple) {
                    if (action.ArgumentKinds[i].ExpandDictionary) {
                        //if (_argListPos == -1) TypeError;
                        //if (exprArgs[_argListPos] != null) TypeError; ?
                        exprArgs[_kwDictPos] = rule.GetParameterExpression(i + 1);
                        continue;
                    } else if (action.ArgumentKinds[i].ExpandList) {
                        //if (_kwDictPos == -1) TypeError;
                        //if (exprArgs[_kwDictPos] != null) TypeError; ?
                        bool emptyArg = false;
                        for (int j = 0; j < _nparams; j++) {
                            if (exprArgs[j] == null) {
                                return null;
                            // TODO: GetEnumerator & unpack
                                /*exprArgs[j] = rule.GetParameterExpression(i + 1);
                                emptyArg = true;
                                break;*/
                            }
                        }
                        if (!emptyArg) {
                            exprArgs[_argListPos] = rule.GetParameterExpression(i + 1);
                        }
                        continue;
                    } else if (action.ArgumentKinds[i].Name != SymbolId.Empty) {
                        bool foundName = false;
                        for (int j = 0; j < ArgNames.Length; j++) {
                            if (ArgNames[j] == SymbolTable.IdToString(action.ArgumentKinds[j].Name)) {
                                //if(exprArgs[j] != null) TypeError
                                exprArgs[j] = rule.GetParameterExpression(i + 1);
                                foundName = true;
                                break;
                            }
                        }

                        if (!foundName) {
                            if (namedArgs == null) namedArgs = new Dictionary<SymbolId, Expression>();
                            namedArgs[action.ArgumentKinds[i].Name] = rule.GetParameterExpression(i + 1);
                        }
                        continue;
                    }
                }

                if (i < _nparams) {
                    exprArgs[i] = rule.GetParameterExpression(i + 1);
                } else {
                    if (extraArgs == null) extraArgs = new List<Expression>();
                    extraArgs.Add(rule.GetParameterExpression(i + 1));
                }
            }
            for (int j = args.Length; j < _nparams; j++) {
                // default values
                // TODO: Can do better than IndexExpression here
                // offset       + # of defaults not consumed
                int dfltIndex = j - args.Length + (Defaults.Length - (_nparams - args.Length));
                Debug.Assert(dfltIndex < Defaults.Length);
                Debug.Assert(dfltIndex >= 0);
                exprArgs[j] = new IndexExpression(
                    MemberExpression.Property(
                        rule.GetParameterExpression(0),
                        GetType().GetProperty("Defaults")),
                    new ConstantExpression(dfltIndex),
                    SourceSpan.None);
            }

            AssignDictionary<T>(rule, exprArgs, namedArgs);
            AssignList(exprArgs, extraArgs);

            return new Expression[] {
                MemberExpression.Property(
                    StaticUnaryExpression.Convert(
                        rule.GetParameterExpression(0),
                        GetType()),
                    GetType().GetProperty("Context")),
                NewArrayExpression.NewArrayInit(typeof(object[]), exprArgs) 
            };
        }

        private void AssignList(Expression[] exprArgs, List<Expression> extraArgs) {
            if (_argListPos != -1) {
                // TODO: Call PythonOps.MakeTuple
                if (exprArgs[_argListPos] == null) {
                    if (extraArgs != null) {
                        exprArgs[_argListPos] = MethodCallExpression.Call(null, typeof(Tuple).GetMethod("MakeTuple", new Type[] { typeof(object[]) }), extraArgs.ToArray());
                    } else {
                        exprArgs[_argListPos] = MethodCallExpression.Call(null, typeof(Tuple).GetMethod("MakeTuple", new Type[] { typeof(object[]) }));
                    }
                } else if (extraArgs != null) {
                    // TODO: Type error
                }
            }
        }

        private void AssignDictionary<T>(StandardRule<T> rule, Expression[] exprArgs, Dictionary<SymbolId, Expression> namedArgs) {
            if (_kwDictPos != -1) {
                // TODO: Call PythonOps.MakeDict               
                if (exprArgs[_kwDictPos] == null) {
                    if (namedArgs != null) {
                        exprArgs[_kwDictPos] = MakeDictionary<T>(rule, namedArgs);
                    } else {
                        exprArgs[_kwDictPos] = MethodCallExpression.Call(null, typeof(PythonOps).GetMethod("MakeDict"), new ConstantExpression(0));
                    }
                } else if (namedArgs != null) {
                    // TODO: TypeError
                }
            }
        }

        private static Expression MakeDictionary<T>(StandardRule<T> rule, Dictionary<SymbolId, Expression> namedArgs) {
            Variable vr = rule.GetTemporary(typeof(PythonDictionary), "$dict");
            Expression[] dictCreator = new Expression[namedArgs.Count + 2];
            BoundExpression dictRef = BoundExpression.Defined(vr);

            dictCreator[0] = BoundAssignment.Assign(vr, MethodCallExpression.Call(null, typeof(PythonOps).GetMethod("MakeDict"), new ConstantExpression(0)));
            dictCreator[1] = dictRef;
            int index = 2;
            foreach (KeyValuePair<SymbolId, Expression> kvp in namedArgs) {
                dictCreator[index++] = MethodCallExpression.Call(
                    dictRef,
                    typeof(PythonDictionary).GetMethod("set_Item"),
                    new ConstantExpression(SymbolTable.IdToString(kvp.Key)),
                    kvp.Value);
            }
            return new CommaExpression(dictCreator, 1);
        }

        #endregion
    }

    [PythonType("instancemethod")]
    public sealed partial class Method : FastCallable, IFancyCallable, IWeakReferenceable, ICustomMembers, IDynamicObject {
        //??? can I type this to Function
        private object _func;
        private object _inst;
        private object _declaringClass;
        private WeakRefTracker _weakref;

        public Method(object function, object instance, object @class) {
            this._func = function;
            this._inst = instance;
            this._declaringClass = @class;
        }

        public Method(object function, object instance) {
            this._func = function;
            this._inst = instance;
        }

        public string Name {
            [PythonName("__name__")]
            get { return (string)PythonOps.GetBoundAttr(DefaultContext.Default, _func, Symbols.Name); }
        }

        public string Documentation {
            [PythonName("__doc__")]
            get {
                return PythonOps.GetBoundAttr(DefaultContext.Default, _func, Symbols.Doc) as string;
            }
        }

        public object Function {
            [PythonName("im_func")]
            get {
                return _func;
            }
        }

        public object Self {
            [PythonName("im_self")]
            get {
                return _inst;
            }
        }

        public object DeclaringClass {
            [PythonName("im_class")]
            get {
                return PythonOps.ToPythonType((DynamicType)_declaringClass);
            }
        }

        private Exception BadSelf(object got) {
            OldClass dt = DeclaringClass as OldClass;            

            string firstArg;
            if (got == null) {
                firstArg = "nothing";
            } else {
                firstArg = PythonOps.GetPythonTypeName(got) + " instance";
            }

            return PythonOps.TypeError("unbound method {0}() must be called with {1} instance as first argument (got {2} instead)",
                Name,
                (dt != null) ? dt.Name : DeclaringClass,
                firstArg);
        }

        private object CheckSelf(object self) {
            if (!PythonOps.IsInstance(self, DeclaringClass)) throw BadSelf(self);
            return self;
        }

        private object[] AddInstToArgs(object[] args) {
            if (_inst == null) {
                if (args.Length < 1) throw BadSelf(null);
                CheckSelf(args[0]);
                return args;
            }

            object[] nargs = new object[args.Length + 1];
            args.CopyTo(nargs, 1);
            nargs[0] = _inst;
            return nargs;
        }

        [OperatorMethod]
        public override object Call(CodeContext context, params object[] args) {
            FastCallable fc = _func as FastCallable;
            if (fc != null) {
                if (_inst != null) {
                    return fc.CallInstance(context, _inst, args);
                } else {
                    if (args.Length > 0) CheckSelf(args[0]);
                    return fc.Call(context, args);
                }
            }
            return PythonOps.CallWithContext(context, _func, AddInstToArgs(args));
        }

        public override object CallInstance(CodeContext context, object instance, params object[] args) {
            FastCallable fc = _func as FastCallable;
            if (fc != null) {
                if (_inst != null) return fc.CallInstance(context, instance, AddInstToArgs(args));
                else return fc.CallInstance(context, instance, args); //??? check instance type
            }
            return PythonOps.CallWithContext(context, _func, PrependInstance(instance, AddInstToArgs(args)));
        }

        [OperatorMethod]
        public object Call(CodeContext context, object[] args, string[] names) {
            return PythonOps.CallWithKeywordArgs(context, _func, AddInstToArgs(args), names);
        }

        #region Object Overrides
        private string DeclaringClassAsString() {
            if (DeclaringClass == null) return "?";
            DynamicType dt = DeclaringClass as DynamicType;
            if (dt != null) return DynamicTypeOps.GetName(dt);
            OldClass oc = DeclaringClass as OldClass;
            if (oc != null) return oc.Name;
            return DeclaringClass.ToString();
        }

        public override string ToString() {
            if (_inst != null) {
                return string.Format("<bound method {0}.{1} of {2}>",
                    DeclaringClassAsString(),
                    PythonOps.GetBoundAttr(DefaultContext.Default, _func, Symbols.Name),
                    PythonOps.StringRepr(_inst));
            } else {
                return string.Format("<unbound method {0}.{1}>", DeclaringClassAsString(), Name);
            }
        }

        public override bool Equals(object obj) {
            Method other = obj as Method;
            if (other == null) return false;

            return other._inst == _inst && other._func == _func;
        }

        public override int GetHashCode() {
            if (_inst == null) return _func.GetHashCode();

            return _inst.GetHashCode() ^ _func.GetHashCode();
        }
        #endregion

        #region IDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, DeclaringClass); }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            if (this.Self == null) {
                if (owner == DeclaringClass || PythonOps.IsSubClass((DynamicType)owner, DeclaringClass)) {
                    return new Method(_func, instance, owner);
                }
            }
            return this;
        }
        #endregion

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            return _weakref;
        }

        bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            _weakref = value;
            return true;
        }

        void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
            ((IWeakReferenceable)this).SetWeakRef(value);
        }

        #endregion

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetBoundCustomMember(context, name, out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            if (name == Symbols.Module) {
                // Get the module name from the function and pass that out.  Note that CPython's method has
                // no __module__ attribute and this value can be gotten via a call to method.__getattribute__ 
                // there as well.
                value = PythonOps.GetBoundAttr(context, _func, Symbols.Module);
                return true;
            }

            if (TypeCache.Method.TryGetBoundMember(context, this, name, out value)) return true;

            // Forward to the func
            return ((PythonFunction)_func).TryGetBoundCustomMember(context, name, out value);
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            TypeCache.Method.SetMember(context, this, name, value);
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            TypeCache.Method.DeleteMember(context, this, name);
            return true;
        }

        public IList<object> GetCustomMemberNames(CodeContext context) {
            List ret = new List();
            foreach(SymbolId si in TypeCache.Method.GetMemberNames(context, this)) {
                ret.AddNoLock(SymbolTable.IdToString(si));
            }

            ret.AddNoLockNoDups(SymbolTable.IdToString(Symbols.Module));

            IAttributesCollection dict = ((PythonFunction)_func)._dict;
            if (dict != null) {
                // Check the func
                foreach (KeyValuePair<object, object> kvp in ((PythonFunction)_func)._dict) {
                    ret.AddNoLockNoDups(kvp.Key);
                }
            }

            return ret;
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            return TypeCache.Method.GetMemberDictionary(context, this).AsObjectKeyedDictionary();
        }

        #endregion

        #region DynamicTypeSlot Overrides

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = GetAttribute(instance, owner);
            return true;
        }

        #endregion

        #region IDynamicObject Members

        LanguageContext IDynamicObject.LanguageContext {
            get { return DefaultContext.Default.LanguageContext; }
        }

        StandardRule<T> IDynamicObject.GetRule<T>(Action action, CodeContext context, object[] args) {
            Utils.Assert.NotNull(action, context, args);
            
            // get default rule:
            return null;
        }

        #endregion
    }

    [PythonType(typeof(PythonFunction))]
    public class InterpFunction : ICallableWithCodeContext {
        private SymbolId[] _argNames;
        private object[] _defaults;
        private Statement _body;
        private CodeContext _context;

        public InterpFunction(SymbolId[] argNames, object[] defaults, Statement body, CodeContext context) {
            this._argNames = argNames;
            this._defaults = defaults;
            this._body = body;

            this._context = context;
        }


        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, params object[] args) {
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
        }

        #endregion
    }
}
