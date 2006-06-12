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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using System.Threading;
using IronPython.Compiler;

namespace IronPython.Runtime {

    /// <summary>
    /// Represents a piece of code.  This can reference either a CompiledCode
    /// object or a Function.   The user can explicitly call FunctionCode by
    /// passing it into exec or eval.
    /// </summary>
    [PythonType("code")]
    public class FunctionCode {
        #region Private member variables
        private object varnames;
        private CompiledCode compiledCode;
        private PythonFunction func;
        private string filename;
        private int lineNo;
        private FuncCodeFlags flags;      // future division, generator
        #endregion

        #region Flags
        [Flags]
        internal enum FuncCodeFlags {
            VarArgs = 0x04,
            KwArgs = 0x08,
            Generator = 0x20,
            FutureDivision = 0x2000,
        }
        #endregion

        #region Public constructors
        public FunctionCode(CompiledCode code) {
            this.compiledCode = code;
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
                if (compiledCode != null) return 0;
                return func.argNames.Length;
            }
        }

        public object CallVars {
            [PythonName("co_cellvars")]
            get {
                throw Ops.NotImplementedError("");
            }
        }

        public object Code {
            [PythonName("co_code")]
            get {
                throw Ops.NotImplementedError("");
            }
        }

        public object Consts {
            [PythonName("co_consts")]
            get {
                throw Ops.NotImplementedError("");
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
                if (funcN != null) res |= FuncCodeFlags.VarArgs;
                if (funcX != null) {
                    if ((funcX.Flags & FuncDefType.KeywordDict) != 0) res |= FuncCodeFlags.KwArgs;
                    if ((funcX.Flags & FuncDefType.ArgList) != 0) res |= FuncCodeFlags.VarArgs;
                }
                return (int)res;
            }
        }

        public object FreeVars {
            [PythonName("co_freevars")]
            get {
                throw Ops.NotImplementedError("");
            }
        }

        public object LineNumberTab {
            [PythonName("co_lnotab")]
            get {
                throw Ops.NotImplementedError("");
            }
        }

        public object Name {
            [PythonName("co_name")]
            get {
                if (func != null) return func.Name;
                if (compiledCode != null) return compiledCode.GetType().Name;

                throw Ops.NotImplementedError("");
            }
        }

        public object Names {
            [PythonName("co_names")]
            get {
                throw Ops.NotImplementedError("");
            }
        }

        public object NumberLocals {
            [PythonName("co_nlocals")]
            get {
                throw Ops.NotImplementedError("");
            }
        }

        public object StackSize {
            [PythonName("co_stacksize")]
            get {
                throw Ops.NotImplementedError("");
            }
        }
        #endregion

        #region Public setters called by generated code
        public void SetFilename(string sourceFile) {
            filename = sourceFile;
        }

        public void SetLineNumber(int line) {
            lineNo = line;
        }

        // This is only used to set the value of FutureDivision and Generator flags
        public void SetFlags(int value) {
            this.flags = (FuncCodeFlags)value & (FuncCodeFlags.FutureDivision | FuncCodeFlags.Generator);
        }

        #endregion

        #region Internal API Surface
        internal object Call(ModuleScope curFrame) {
            if (compiledCode != null) {
                return compiledCode.Run(curFrame);
            } else if (func != null) {
                return func.Call();
            }

            throw Ops.TypeError("bad code");
        }
        #endregion

        #region Private helper functions
        private Tuple GetArgNames() {
            if (compiledCode != null) return Tuple.MakeTuple();

            List<string> names = new List<string>();
            List<Tuple> nested = new List<Tuple>();


            for (int i = 0; i < func.argNames.Length; i++) {
                if (func.argNames[i].IndexOf('#') != -1 && func.argNames[i].IndexOf('!') != -1) {
                    names.Add("." + (i * 2).ToString());
                    nested.Add(FuncDef.DecodeTupleParamName(func.argNames[i]));
                } else {
                    names.Add(func.argNames[i]);
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

        [PythonName("__eq__")]
        public override bool Equals(object obj) {
            FunctionCode other = obj as FunctionCode;
            if (other == null) return false;

            if (compiledCode != null) {
                return compiledCode == other.compiledCode;
            } else if (func != null) {
                return func == other.func;
            }

            throw Ops.TypeError("bad code");
        }

        [PythonName("__hash__")]
        public override int GetHashCode() {
            if (compiledCode != null) {
                return compiledCode.GetHashCode();
            } else if (func != null) {
                return func.GetHashCode();
            }

            throw Ops.TypeError("bad code");
        }
    }

    /// <summary>
    /// Created for a user-defined function.  
    /// </summary>
    [PythonType("function")]
    public abstract partial class PythonFunction : FastCallable, IFancyCallable, IDescriptor, IDynamicObject, IWeakReferenceable, ICloneable, ICustomAttributes {
        [ThreadStatic]
        private static int depth;                         // call depth (only used when we have lots of threads)
        private static int[] depths = new int[20];        // first 32 threads get tracked in static array for perf, 
        // more threads go to TLS 
        internal static int MaximumDepth = 1001;          // maximum recursion depth allowed before we throw 
        internal static bool EnforceRecursion = false;    // true to enforce maximum depth, false otherwise

        #region instance members
        public string[] argNames;
        // The default values of the last few arguments
        protected readonly object[] defaults;

        private PythonModule module;
        private object doc;
        private FunctionCode code;
        internal IAttributesDictionary dict;
        #endregion

        protected PythonFunction(PythonModule globals, string name, string[] argNames, object[] defaults):base(name) {
            this.argNames = argNames;
            this.defaults = defaults;
            Debug.Assert(defaults.Length <= argNames.Length);
            if (name.IndexOf('#') > 0) {
                // dynamic method, strip the trailing id...
                this.name = name.Substring(0, name.IndexOf('#'));
            } else {
                this.name = name;
            }

            this.module = globals;
            this.FunctionCode = new FunctionCode(this);
        }

        public override int MaximumArgs {
            get { return 0; }
        }

        public override int MinimumArgs {
            get { return 0; }
        }

        #region Public APIs
        public object FunctionGlobals {
            [PythonName("func_globals")]
            get {
                return module.__dict__;
            }
        }

        public Tuple FunctionDefaults {
            [PythonName("func_defaults")]
            get {
                return new Tuple(defaults);
            }
        }

        public string FunctionName {
            [PythonName("func_name")]
            get {
                return name;
            }
        }
        public string Name {
            [PythonName("__name__")]
            get {
                return name;
            }
        }

        public object FunctionDoc {
            [PythonName("func_doc")]
            get {
                return Documentation;
            }
        }

        public PythonModule Module {
            [PythonName("__module__")]
            get {
                return module;
            }
        }

        public virtual object Documentation {
            [PythonName("__doc__")]
            get {
                return doc;
            }
            [PythonName("__doc__")]
            set {
                doc = value;
            }
        }

        public object FunctionCode {
            [PythonName("func_code")]
            get {
                return code;
            }
            [PythonName("func_code")]
            set {
                ///!!! need to do more to be right.
                if (!(value is FunctionCode)) throw Ops.TypeError("func_code must be set to a code object");
                code = (FunctionCode)value;
            }
        }

        public override object CallInstance(ICallerContext context, object instance, params object[] args) {
            return Call(context, PrependInstance(instance, args));
        }

        [PythonName("__call__")]
        public virtual object Call(ICallerContext context, object[] args, string[] kwNames) {
            int nparams = argNames.Length;
            int nargs = args.Length - kwNames.Length;
            if (nargs > nparams) throw BadArgumentError(nparams);

            object[] inArgs = args;
            args = new object[nparams];
            bool[] haveArg = new bool[nparams];
            for (int i = 0; i < nargs; i++) { args[i] = inArgs[i]; haveArg[i] = true; }

            for (int i = 0; i < kwNames.Length; i++) {
                int paramIndex = FindParamIndex(kwNames[i]);
                if (paramIndex == -1) {
                    throw Ops.TypeError("{0}() got an unexpected keyword argument '{1}'", name, kwNames[i]);
                }
                if (haveArg[paramIndex]) {
                    throw Ops.TypeError("multiple values for " + kwNames[i]);
                }
                haveArg[paramIndex] = true;
                args[paramIndex] = inArgs[nargs + i];
            }

            for (int i = 0; i < nparams; i++) {
                if (!haveArg[i]) {
                    int defaultIndex = i - (nparams - defaults.Length);
                    if (defaultIndex < 0) {
                        throw TypeErrorForIncorrectArgumentCount(name, nparams, defaults.Length, nargs, false, true);
                    }
                    args[i] = defaults[defaultIndex];
                    haveArg[i] = true;
                }
            }

            return Call(args);
        }

        [PythonName("__call__")]
        public object Call(ICallerContext context, [ParamDict]Dict dictArgs, params object[] args) {
            object[] realArgs = new object[args.Length + dictArgs.Count];
            string[] argNames = new string[dictArgs.Count];

            Array.Copy(args, realArgs, args.Length);

            int index = 0;
            foreach (KeyValuePair<object, object> kvp in dictArgs) {
                argNames[index] = kvp.Key as string;
                realArgs[index + args.Length] = kvp.Value;
                index++;
            }

            return Ops.Call(context, this, realArgs, argNames);
        }

        public int ArgCount {
            get {
                return argNames.Length;
            }
        }

        #endregion

        #region IDynamicObject Members

        public virtual DynamicType GetDynamicType() {
            return TypeCache.Function;
        }

        #endregion


        #region Protected APIs

        protected void PushFrame() {
            // ManagedThreadId starts at 1 and increases as we get more threads.
            // Therefore we keep track of a limited number of threads in an array
            // that only gets created once, and we access each of the elements
            // from only a single thread.
            int localDepth;
            uint tid = (uint)Thread.CurrentThread.ManagedThreadId;

            if (tid < depths.Length) {
                localDepth = ++depths[tid];
            } else {
                localDepth = ++depth;
            }

            if (localDepth > MaximumDepth) throw Ops.RuntimeError("maximum recursion depth exceeded");
        }

        protected void PopFrame() {
            uint tid = (uint)Thread.CurrentThread.ManagedThreadId;

            if (tid < depths.Length) {
                --depths[tid];
            } else {
                --depth;
            }
        }

        // formalNormalArgumentCount - does not include FuncDefFlags.ArgList and FuncDefFlags.KwDict
        // defaultArgumentCount - How many arguments in the method declaration have a default value?
        // providedArgumentCount - How many arguments are passed in at the call site?
        // hasArgList - Is the method declaration of the form "foo(*argList)"?
        // keywordArgumentsProvided - Does the call site specify keyword arguments?
        internal static Exception TypeErrorForIncorrectArgumentCount(
            string methodName,
            int formalNormalArgumentCount,
            int defaultArgumentCount,
            int providedArgumentCount,
            bool hasArgList,
            bool keywordArgumentsProvided) {

            int formalCount;
            string formalCountQualifier;
            string nonKeyword = keywordArgumentsProvided ? "non-keyword " : "";

            if (defaultArgumentCount > 0 || hasArgList) {
                if (providedArgumentCount < formalNormalArgumentCount) {
                    formalCountQualifier = "at least";
                    formalCount = formalNormalArgumentCount - defaultArgumentCount;
                } else {
                    formalCountQualifier = "at most";
                    formalCount = formalNormalArgumentCount;
                }
            } else {
                formalCountQualifier = "exactly";
                formalCount = formalNormalArgumentCount;
            }

            return Ops.TypeError("{0}() takes {1} {2} {3}argument{4} ({5} given)",
                                methodName, // 0
                                formalCountQualifier, // 1
                                formalCount, // 2
                                nonKeyword, // 3
                                formalCount == 1 ? "" : "s", // 4
                                providedArgumentCount); // 5
        }

        internal static Exception TypeErrorForIncorrectArgumentCount(string name, int formalNormalArgumentCount, int defaultArgumentCount, int providedArgumentCount) {
            return TypeErrorForIncorrectArgumentCount(name, formalNormalArgumentCount, defaultArgumentCount, providedArgumentCount, false, false);
        }

        protected virtual Exception BadArgumentError(int count) {
            return TypeErrorForIncorrectArgumentCount(name, argNames.Length, defaults.Length, count);
        }

        protected int FindParamIndex(string name) {
            for (int i = 0; i < argNames.Length; i++) {
                if (name == argNames[i]) return i;
            }
            return -1;
        }

        protected int FindParamIndexOrError(string name) {
            int ret = FindParamIndex(name);
            if (ret != -1) return ret;
            throw Ops.TypeError("no parameter for " + name);
        }

        #endregion

        #region IDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, null); }

        [PythonName("__get__")]
        public virtual object GetAttribute(object instance, object owner) {

            if (instance != null) return new Method(this, instance, owner);
            else {
                PythonFunction res = Clone() as PythonFunction;
                res.doc = FunctionDoc;
                return res;
            }
        }
        #endregion

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            if (name == SymbolTable.Dict) {
                value = EnsureDict();
                return true;
            }

            if (dict != null) {
                if (dict.TryGetValue(name, out value)) {
                    return true;
                }
            }

            // We check for SymbolTable.Module as the user code can modify it
            if (name == SymbolTable.Module) {
                value = Module.ModuleName;
                return true;
            }

            return GetDynamicType().TryGetAttr(context, this, name, out value);
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            if (name == SymbolTable.Dict) {
                // our Python Dictionaries implement this explicitly
                IAttributesDictionary d = value as IAttributesDictionary;
                if (d == null) {
                    throw Ops.TypeError("__dict__ must be set to dictionary");
                }
                dict = d;
            } else {
                EnsureDict()[name] = value;
            }
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            if (name == SymbolTable.Dict)
                throw Ops.TypeError(name.ToString() + " may not be deleted");

            if (dict == null || !dict.ContainsKey(name)) {
                // We check for SymbolTable.Module as the user code can modify it
                if (name == SymbolTable.Module)
                    throw Ops.TypeError(name.ToString() + " may not be deleted");

                throw Ops.AttributeError("no attribute {0}", name);
            }

            dict.Remove(name);
        }

        public List GetAttrNames(ICallerContext context) {
            List list;
            if (dict == null) {
                list = List.Make();
            } else {
                list = List.Make(dict);
            }
            list.AddNoLock(SymbolTable.Module.ToString());

            List reflectedAttrs = GetDynamicType().GetAttrNames(context, this);
            list.AppendListNoLockNoDups(reflectedAttrs);
            return list;
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            if (dict == null) return new Dict(0);
            return (IDictionary<object, object>)dict;
        }

        #endregion

        #region Object Overrides
        [PythonName("__str__")]
        public override string ToString() {
            return string.Format("<function {0} at {1}>", FunctionName, Ops.HexId(this));
        }
        #endregion

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            if (dict != null) {
                object weakRef;
                if (dict.TryGetValue(SymbolTable.WeakRef, out weakRef)) {
                    return weakRef as WeakRefTracker;
                }
            }
            return null;
        }

        void IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            EnsureDict();
            dict[SymbolTable.WeakRef] = value;
        }

        #endregion

        #region ICloneable Members

        public abstract object Clone();

        #endregion

        private IAttributesDictionary EnsureDict() {
            if (dict == null) {
                dict = new FieldIdDict();
            }
            return dict;
        }
    }

    /// <summary>
    /// Targets a single delegate that takes many arguments.
    /// </summary>
    [PythonType(typeof(PythonFunction))]
    public partial class FunctionN : PythonFunction {
        public CallTargetN target;

        public FunctionN(PythonModule globals, string name, CallTargetN target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }


        #region ICallable Members

        [PythonName("__call__")]
        public override object Call(ICallerContext context, params object[] args) {
            int nparams = argNames.Length;
            int nargs = args.Length;
            if (nargs < nparams) {
                if (nargs + defaults.Length < nparams) {
                    throw BadArgumentError(nargs + defaults.Length);
                }
                object[] inArgs = args;
                args = new object[nparams];
                for (int i = 0; i < nargs; i++) args[i] = inArgs[i];
                object[] defs = defaults;
                int di = defs.Length - 1;
                for (int i = nparams - 1; i >= nargs; i--) {
                    args[i] = defs[di--];
                }
            } else if (nargs > nparams) {
                throw BadArgumentError(nparams);
            }

            if (!EnforceRecursion) return target(args);

            PushFrame();
            try {
                return target(args);
            } finally {
                PopFrame();
            }
        }

        #endregion

        #region Object Overrides
        [PythonName("__eq__")]
        public override bool Equals(object obj) {
            FunctionN other = obj as FunctionN;
            if (other == null) return false;

            return target == other.target;
        }

        [PythonName("__hash__")]
        public override int GetHashCode() {
            return target.GetHashCode();
        }
        #endregion

        public override object Clone() {
            return new FunctionN(Module, Name, target, argNames, defaults);
        }
    }

    /// <summary>
    /// Targets a single delegate that takes a variety of arguments (kw-arg list or arg list)
    /// </summary>
    [PythonType(typeof(PythonFunction))]
    public class FunctionX : FunctionN {
        private FuncDefType flags;

        // Given "foo(a, b, c=3, *argList, **argDist), only a, b, and c are considered "normal" parameters
        private int nparams;

        // Is there any FuncDefFlags.ArgList or FuncDefFlags.KwDict?
        private int extraArgs = 0;

        private int argListPos = -1, kwDictPos = -1;

        public FunctionX(PythonModule globals, string name, CallTargetN target, string[] argNames, object[] defaults, FuncDefType flags)
            :
            base(globals, name, target, argNames, defaults) {
            this.flags = flags;
            nparams = argNames.Length;

            if ((flags & FuncDefType.KeywordDict) != 0) {
                extraArgs++;
                nparams--;
                kwDictPos = nparams;
            }

            if ((flags & FuncDefType.ArgList) != 0) {
                extraArgs++;
                nparams--;
                argListPos = nparams;
            }

            Debug.Assert(defaults.Length <= nparams);
        }

        [PythonName("__call__")]
        public override object Call(ICallerContext context, params object[] args) {
            int nargs = args.Length;
            object argList = null;
            object[] outArgs = new object[argNames.Length];

            if (nargs < nparams) {
                if (nargs + defaults.Length < nparams) {
                    throw BadArgumentError(nargs);
                }
                for (int i = 0; i < nargs; i++) outArgs[i] = args[i];
                object[] defs = defaults;
                int di = defs.Length - 1;
                for (int i = nparams - 1; i >= nargs; i--) {
                    outArgs[i] = defs[di--];
                }
            } else if (nargs > nparams) {
                if (argListPos >= 0) {
                    for (int i = 0; i < nparams; i++) outArgs[i] = args[i];

                    object[] extraArgs = new object[nargs - nparams];
                    for (int i = 0; i < extraArgs.Length; i++) {
                        extraArgs[i] = args[i + nparams];
                    }
                    argList = Tuple.Make(extraArgs);
                } else {
                    throw BadArgumentError(nargs);
                }
            } else {
                for (int i = 0; i < nargs; i++) outArgs[i] = args[i];
            }

            if (argListPos >= 0) {
                if (argList == null) argList = Tuple.MakeTuple();
                outArgs[argListPos] = argList;
            }
            if (kwDictPos >= 0) {
                outArgs[kwDictPos] = new Dict(); //PyDictionary.make();
            }

            if (!EnforceRecursion) return target(outArgs);

            PushFrame();
            try {
                return target(outArgs);
            } finally {
                PopFrame();
            }
        }

        [PythonName("__call__")]
        public override object Call(ICallerContext context, object[] args, string[] names) {
            KwArgBinder argBinder = new KwArgBinder(args, names);
            object[] defaults = this.defaults;
            if (defaults.Length != argNames.Length) {
                // we need a 1<->1 mapping here for kwarg binder.  
                object[] newDefs = new object[argNames.Length];

                for (int i = 0; i < (nparams - defaults.Length); i++) {
                    newDefs[i] = DBNull.Value;
                }

                Array.Copy(defaults, 0, newDefs, (nparams - defaults.Length), defaults.Length);
                defaults = newDefs;
            }

            object[] realArgs = argBinder.DoBind(Name, argNames, defaults, kwDictPos, argListPos);

            if (realArgs != null) {
                if (!EnforceRecursion) return target(realArgs);
                PushFrame();
                try {
                    return target(realArgs);
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
            throw PythonFunction.TypeErrorForIncorrectArgumentCount(Name, nparams, defaults.Length, count, argListPos != -1, false);
        }

        internal FuncDefType Flags {
            get {
                return flags;
            }
        }

        public override object Clone() {
            return new FunctionX(Module, Name, target, argNames, defaults, flags);
        }
    }

    [PythonType("instancemethod")]
    public sealed partial class Method : FastCallable, IFancyCallable, IDescriptor, IWeakReferenceable, IDynamicObject, ICustomAttributes {
        //??? can I type this to Function
        private object func;
        private object inst;
        private object declaringClass;
        private WeakRefTracker weakref;

        public Method(object function, object instance, object @class):base("") {
            this.func = function;
            this.inst = instance;
            this.declaringClass = @class;
        }

        public Method(object function, object instance):base("") {
            this.func = function;
            this.inst = instance;
        }

        public string Name {
            [PythonName("__name__")]
            get { return (string)Ops.GetAttr(DefaultContext.Default, func, SymbolTable.Name); }
        }

        public string Documentation {
            [PythonName("__doc__")]
            get {
                return Ops.GetAttr(DefaultContext.Default, func, SymbolTable.Doc) as string;
            }
        }

        public object Function {
            [PythonName("im_func")]
            get {
                return func;
            }
        }

        public object Self {
            [PythonName("im_self")]
            get {
                return inst;
            }
        }

        public object DeclaringClass {
            [PythonName("im_class")]
            get {
                return declaringClass;
            }
        }

        //!!! These two methods are quite ugly
        public override int MaximumArgs {
            get { return 0; }
        }

        public override int MinimumArgs {
            get { return 0; }
        }

        private Exception BadSelf(object got) {
            DynamicType dt = DeclaringClass as DynamicType;

            string firstArg;
            if (got == null) {
                firstArg = "nothing";
            } else {
                firstArg = Ops.GetClassName(got) + " instance";
            }

            return Ops.TypeError("unbound method {0}() must be called with {1} instance as first argument (got {2} instead)",
                Name,
                (dt != null) ? dt.__name__ : DeclaringClass,
                firstArg);
        }

        private object CheckSelf(object self) {
            if (!Modules.Builtin.IsInstance(self, DeclaringClass)) throw BadSelf(self);
            return self;
        }

        private object[] AddInstToArgs(object[] args) {
            if (inst == null) {
                if (args.Length < 1) throw BadSelf(null);
                CheckSelf(args[0]);
                return args;
            }

            object[] nargs = new object[args.Length + 1];
            args.CopyTo(nargs, 1);
            nargs[0] = inst;
            return nargs;
        }

        [PythonName("__call__")]
        public override object Call(ICallerContext context, params object[] args) {
            FastCallable fc = func as FastCallable;
            if (fc != null) {
                if (inst != null) {
                    return fc.CallInstance(context, inst, args);
                }  else {
                    if (args.Length > 0) CheckSelf(args[0]);
                    return fc.Call(context, args);
                }
            }
            return Ops.CallWithContext(context, func, AddInstToArgs(args));
        }

        public override object CallInstance(ICallerContext context, object instance, params object[] args) {
            FastCallable fc = func as FastCallable;
            if (fc != null) {
                if (inst != null) return fc.CallInstance(context, instance, AddInstToArgs(args));
                else return fc.CallInstance(context, instance, args); //??? check instance type
            }
            return Ops.CallWithContext(context, func, PrependInstance(instance, AddInstToArgs(args)));
        }

        [PythonName("__call__")]
        public object Call(ICallerContext context, object []args, string []names){
            return Ops.Call(context, func, AddInstToArgs(args), names);
        }

        #region Object Overrides
        private string DeclaringClassAsString() {
            if (DeclaringClass == null) return "?";
            DynamicType dt = DeclaringClass as DynamicType;
            if (dt != null) return dt.__name__.ToString();
            return DeclaringClass.ToString();
        }

        [PythonName("__str__")]
        public override string ToString() {
            if (inst != null) {
                return string.Format("<bound method {0}.{1} of {2}>",
                    DeclaringClassAsString(), 
                    Ops.GetAttr(DefaultContext.Default, func, SymbolTable.Name),
                    Ops.StringRepr(inst));
            } else {
                return string.Format("<unbound method {0}.{1}>", DeclaringClassAsString(), Name);
            }
        }

        [PythonName("__eq__")]
        public override bool Equals(object obj) {
            Method other = obj as Method;
            if (other == null) return false;

            return other.inst == inst && other.func == func;
        }

        [PythonName("__hash__")]
        public override int GetHashCode() {
            if (inst == null) return func.GetHashCode();

            return inst.GetHashCode() ^ func.GetHashCode();
        }
        #endregion

        #region IDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, DeclaringClass); }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object context) {
            if (this.Self == null) {
                if (context == DeclaringClass || Modules.Builtin.IsSubClass((DynamicType)context, DeclaringClass)) {
                    return new Method(func, instance, context);
                }
            }
            return this;
        }
        #endregion

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            return weakref;
        }

        void IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            weakref = value;
        }

        #endregion

        #region IDynamicObject Members

        public DynamicType GetDynamicType() {
            return TypeCache.Method;
        }

        #endregion

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            if (name == SymbolTable.Module) {
                value = ((PythonFunction)func).Module.ModuleName;
                return true;
            }

            if (TypeCache.Method.TryGetAttr(context, this, name, out value)) return true;

            // Forward to the func
            return ((PythonFunction)func).TryGetAttr(context, name, out value);
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            TypeCache.Method.SetAttr(context, this, name, value);
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            TypeCache.Method.DelAttr(context, this, name);
        }

        public List GetAttrNames(ICallerContext context) {
            List ret = TypeCache.Method.GetAttrNames(context, this);
            ret = List.Make(ret);
            ret.AddNoLockNoDups(SymbolTable.Module.ToString());

            IAttributesDictionary dict = ((PythonFunction)func).dict;
            if (dict != null) {
                // Check the func
                foreach (KeyValuePair<object, object> kvp in ((PythonFunction)func).dict) {
                    ret.AddNoLockNoDups(kvp.Key);
                }
            }

            return ret;
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            return TypeCache.Method.GetAttrDict(context, this);
        }

        #endregion

    }

    [PythonType(typeof(PythonFunction))]
    public class InterpFunction : ICallable {
        string[] argNames;
        object[] defaults;

        Stmt body;
        PythonModule globals;

        public InterpFunction(string[] argNames, object[] defaults, Stmt body, PythonModule globals) {
            this.argNames = argNames;
            this.defaults = defaults;
            this.body = body;

            this.globals = globals;
        }


        #region ICallable Members

        public object Call(params object[] args) {
            NameEnv env = new NameEnv(globals, new Dict());
            int i = 0;
            for (; i < args.Length; i++) {
                env.Set(argNames[i], args[i]);
            }

            int di = i - (argNames.Length - defaults.Length);

            while (i < argNames.Length) {
                env.Set(argNames[i++], defaults[di++]);
            }

            return body.Execute(env);
        }

        #endregion
    }


}