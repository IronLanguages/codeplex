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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Types;

using IronPython.Hosting;
using IronPython.Compiler;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Utils;


namespace IronPython.Runtime {

    [PythonType(typeof(ScriptModule))]
    public class SystemState : ICustomMembers {
        private Encoding _defaultEncoding;
        private IAttributesCollection _dict;
        private object _exception_type, _exception_value, _exception_traceback;
        private string _initialVersionString;
        private string _initialExecutable;
        private static Dictionary<SymbolId, ModuleGlobalCache> _builtinCache = new Dictionary<SymbolId,ModuleGlobalCache>();
        private Dictionary<string, Type> _builtinsDict = new Dictionary<string, Type>();
        private Dictionary<Type, string> _builtinModuleNames = new Dictionary<Type, string>();

        private ScriptModule _builtins;

        internal SystemState() {
            InitializeBuiltins();
        }

        public static SystemState Instance {
            get {
                return PythonEngine.CurrentEngine.SystemState;
            }
        }

        /// <summary>
        /// TODO: Remove me, or stop caching built-ins.  This is broken if the user changes __builtin__
        /// </summary>
        public ScriptModule BuiltinModuleInstance {
            get {
                lock (this) {
                    ScriptModule res = _builtins;
                    if (res == null) {
                        res = _builtins = (ScriptModule)modules["__builtin__"];
                        _builtins.ModuleChanged += new EventHandler<ModuleChangeEventArgs>(BuiltinsChanged);
                    }
                    return res;
                }
            }
        }

        internal bool TryGetModuleGlobalCache(SymbolId name, out ModuleGlobalCache cache) {
            lock (_builtinCache) {
                if (!_builtinCache.TryGetValue(name, out cache)) {
                    // only cache values currently in built-ins, everything else will have
                    // no caching policy and will fall back to the LanguageContext.
                    object value;
                    if (BuiltinModuleInstance.Scope.TryGetName(DefaultContext.Default.LanguageContext, name, out value)) {
                        _builtinCache[name] = cache = new ModuleGlobalCache(value);
                    }
                }
            }
            return cache != null;
        }

        void BuiltinsChanged(object sender, ModuleChangeEventArgs e) {
            ModuleGlobalCache mgc;
            lock (_builtinCache) {
                if (_builtinCache.TryGetValue(e.Name, out mgc)) {
                    switch (e.ChangeType) {
                        case ModuleChangeType.Delete: mgc.Value = Uninitialized.Instance; break;
                        case ModuleChangeType.Set: mgc.Value = e.Value; break;
                    }
                } else {
                    // shouldn't be able to delete before it was set
                    object value = e.ChangeType == ModuleChangeType.Set ? e.Value : Uninitialized.Instance;
                    _builtinCache[e.Name] = new ModuleGlobalCache(value);
                }
            }
        }

        [StaticExtensionMethod("__new__")]
        public static object MakeModule(CodeContext context, DynamicType cls, params object[] args\u00F8) {
            return PythonModuleOps.MakeModule(context, cls);
        }

        [StaticExtensionMethod("__new__")]
        public static object MakeModule(CodeContext context, DynamicType cls, [ParamDictionary] IAttributesCollection kwDict\u00F8, params object[] args\u00F8) {
            return MakeModule(context, cls, args\u00F8);
        }

        /// <summary>
        /// Performs sys's initialization
        /// It is in it's own function so we can do reload(sys). On reload(sys), most of the attributes need to be
        /// reset. The following are left as they are - argv, exc_type, modules, path, path_hooks, path_importer_cache, ps1, ps2.
        /// </summary>
        public void Initialize() {

            if (_dict == null) {
                _dict = new SymbolDictionary();

                // These fields do not get reset on "reload(sys)"
                argv = List.Make();
                modules = new SymbolDictionary();
                modules["sys"] = this;

                modules["__builtin__"] = PythonModuleOps.MakePythonModule("__builtin__", typeof(Builtin));

                path = List.Make();
                ps1 = ">>> ";
                ps2 = "... ";

                SetStandardIO();
            }

            _dict[Symbols.Name] = "sys";

            stdin = __stdin__;
            stdout = __stdout__;
            stderr = __stderr__;

            // removed from dictionary after the first call to set it.
            MethodInfo mi = typeof(SystemState).GetMethod("SetDefaultEncodingImplementation",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            BuiltinMethodDescriptor descr = (BuiltinMethodDescriptor)BuiltinFunction.MakeMethod(
                "setdefaultencoding", mi, FunctionType.AlwaysVisible | FunctionType.Method).GetDescriptor();


            _dict[Symbols.SetDefaultEncoding] = descr.UncheckedGetAttribute(this);

            _defaultEncoding = StringUtils.AsciiEncoding;
            byteorder = BitConverter.IsLittleEndian ? "little" : "big";
            copyright = "Copyright (c) Microsoft Corporation. All rights reserved.";
            maxint = Int32.MaxValue;
            maxunicode = (int)ushort.MaxValue;
#if SILVERLIGHT
            platform = "silverlight";
#else
            platform = "cli";
#endif
            winver = "2.4";

            // !!! These fields do need to be reset on "reload(sys)". However, the initial value is specified by the 
            // engine elsewhere. For now, we initialize them just once to some default value
            warnoptions = List.Make();
            executable = _initialExecutable ?? "";
            SetVersionVariables(_initialVersionString);
        }

        private void SetVersionVariables(string versionString) {
            SetVersionVariables(2, 5, 0, "release", versionString);
        }

        private void SetVersionVariables(byte major, byte minor, byte build, string level, string versionString) {
            hexversion = ((int)major << 24) + ((int)minor << 16) + ((int)build << 8);
            version_info = Tuple.MakeTuple((int)major, (int)minor, (int)build, level, 0);
            version = String.Format("{0}.{1}.{2} ({3})", major, minor, build, versionString);
        }

        public void SetHostVariables(string prefix, string executable, string versionString) {

            SetVersionVariables(versionString);
            
            this._initialVersionString = versionString;
            this.executable = this._initialExecutable = executable;
            this.exec_prefix = this.prefix = prefix;
        }

        public override string ToString() {
            return "<module 'sys' (built-in)>";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public object ApiVersion {
            [PythonName("api_version")]
            get { return "IronPython does not support the C APIs, the api_version is not supported"; }
            [PythonName("api_version")]
            set { throw PythonOps.NotImplementedError("IronPython does not support the C APIs, the api_version is not supported"); }
        }

        [PythonName("argv")]
        public object argv;

        public string byteorder;

        [PythonName("builtin_module_names")]
        public Tuple builtin_module_names;

        [PythonName("copyright")]
        public string copyright;

        /// <summary>
        /// Gets or sets the default encoding for this system state / engine.
        /// </summary>
        public Encoding DefaultEncoding {
            get { return _defaultEncoding; }
            set { _defaultEncoding = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public object DisplayHook {
            [PythonName("displayhook")]
            get { return "IronPython does not support sys.displayhook"; }
            [PythonName("displayhook")]
            set { throw PythonOps.NotImplementedError("IronPython does not support sys.displayhook"); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public object ExceptionHook {
            [PythonName("excepthook")]
            get { return "IronPython does not support sys.excepthook"; }
            [PythonName("excepthook")]
            set { throw PythonOps.NotImplementedError("IronPython does not support sys.excepthook"); }
        }

        [PythonName("getcheckinterval")]
        public int GetCheckInterval() {
            throw PythonOps.NotImplementedError("IronPython does not support sys.getcheckinterval");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value"), PythonName("setcheckinterval")]
        public void SetCheckInterval(int value) {
            throw PythonOps.NotImplementedError("IronPython does not support sys.setcheckinterval");
        }

        [PythonName("warnoptions")]
        public List warnoptions;

        // as of 1.5 preferred access is exc_info, these may be null.
        public object ExceptionType {
            [PythonName("exc_type")]
            get {
                return _exception_type;
            }
            [PythonName("exc_type")]
            set {
                _exception_type = value;
            }
        }

        public object ExceptionValue {
            [PythonName("exc_value")]
            get {
                return _exception_value;
            }
            [PythonName("exc_value")]
            set {
                _exception_value = value;
            }
        }

        public object ExceptionTraceBack {
            [PythonName("exc_traceback")]
            get {
                return _exception_traceback;
            }
            [PythonName("exc_traceback")]
            set {
                _exception_traceback = value;
            }
        }

        [ThreadStatic]
        internal static TraceBack RawTraceBack;

        [PythonName("exc_clear")]
        public void ClearException() {
            PythonOps.PopExceptionHandler();
        }

        [PythonName("exc_info")]
        public Tuple ExceptionInfo() {
            return PythonOps.GetExceptionInfo();
        }

        [PythonName("exec_prefix")]
        public string exec_prefix;
        [PythonName("executable")]
        public object executable;

        [PythonName("exit")]
        public void Exit() {
            Exit(null);
        }

        [PythonName("exit")]
        public void Exit(object code) {
            if (code == null) {
                throw ExceptionConverter.CreateThrowable(ExceptionConverter.GetPythonException("SystemExit"));
            } else {
                // throw as a python exception here to get the args set.
                throw ExceptionConverter.CreateThrowable(ExceptionConverter.GetPythonException("SystemExit"), code);
            }
        }

        [PythonName("getdefaultencoding")]
        public string GetDefaultEncoding() {
            return _defaultEncoding.WebName.ToLower().Replace('-', '_');
        }

        [PythonName("getfilesystemencoding")]
        public object GetFileSystemEncoding() {
            return null;
        }

        [PythonName("_getframe")]
        public object GetPythonStackFrame() {
            throw PythonOps.ValueError("_getframe is not implemented");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "depth"), PythonName("_getframe")]
        public object GetPythonStackFrame(int depth) {
            throw PythonOps.ValueError("_getframe is not implemented");
        }

        [PythonName("hexversion")]
        public int hexversion;

        [PythonName("maxint")]
        public int maxint;
        [PythonName("maxunicode")]
        public object maxunicode;

        [PythonName("modules")]
        public IDictionary<object, object> modules;

        [PythonName("path")]
        public List path;

        [PythonName("platform")]
        public object platform;

        [PythonName("prefix")]
        public string prefix;

        [PythonName("ps1")]
        public object ps1;
        [PythonName("ps2")]
        public object ps2;

        public object SetDefaultEncodingImplementation(object name) {
            if (name == null) throw PythonOps.TypeError("name cannot be None");
            string strName = name as string;
            if (strName == null) throw PythonOps.TypeError("name must be a string");

            Encoding enc;
            if (!StringOps.TryGetEncoding(_defaultEncoding, strName, out enc)) {
                throw PythonOps.LookupError("'{0}' does not match any available encodings", strName);
            }

            _defaultEncoding = enc;

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o"), PythonName("settrace")]
        public void SetTrace(object o) {
            throw PythonOps.NotImplementedError("sys.settrace is not yet supported by IronPython");
        }

        [PythonName("setrecursionlimit")]
        public void SetRecursionLimit(int limit) {
            if (limit < 0) throw PythonOps.ValueError("recursion limit must be positive");
            PythonFunction.EnforceRecursion = (limit != Int32.MaxValue);
            PythonFunction._MaximumDepth = limit;
        }

        [PythonName("getrecursionlimit")]
        public int GetRecursionLimit() {
            return PythonFunction._MaximumDepth;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o"), PythonName("getrefcount")]
        public int GetReferenceCount(object o) {
            throw PythonOps.NotImplementedError("IronPython uses mark and sweep garbage collector, getrefcount is not supported");
        }

        #region IO

        [PythonName("stdin")]
        public object stdin;
        [PythonName("stdout")]
        public object stdout;
        [PythonName("stderr")]
        public object stderr;

        [PythonName("__stdin__")]
        public object __stdin__;
        [PythonName("__stdout__")]
        public object __stdout__;
        [PythonName("__stderr__")]
        public object __stderr__;

        private void SetStandardIO() {
            ConsoleStream s;
            bool buffered = ScriptDomainManager.Options.BufferedStandardOutAndError;

            s = new ConsoleStream(ConsoleStreamType.Input);
            __stdin__ = stdin = new PythonFile(s, s.Encoding, "<stdin>", "w");
            
            s = new ConsoleStream(ConsoleStreamType.Output, buffered);
             __stdout__ = stdout = new PythonFile(s, s.Encoding, "<stdout>", "w");
           
            s = new ConsoleStream(ConsoleStreamType.ErrorOutput, buffered);
            __stderr__ = stderr = new PythonFile(s, s.Encoding, "<stderr>", "w");
        }

        internal void CloseStandardIOStreams() {
            IDisposable disp;
            if ((disp = stdin as IDisposable) != null) disp.Dispose(); 
            if ((disp = stderr as IDisposable) != null) disp.Dispose();
            if ((disp = stdout as IDisposable) != null) disp.Dispose();
            if ((disp = __stdin__ as IDisposable) != null) disp.Dispose();
            if ((disp = __stderr__ as IDisposable) != null) disp.Dispose();
            if ((disp = __stdout__ as IDisposable) != null) disp.Dispose();

            stdin = stderr = stdout = __stdin__ = __stderr__ = __stdout__ = null;
        }

        #endregion
       
        [PythonName("version")]
        public string version;

        [PythonName("version_info")]
        public object version_info;

        [PythonName("winver")]
        public object winver;
        

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetBoundCustomMember(context, name, out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            if (name == Symbols.Dict) {
                value = _dict;
                return true;
            }

            if (_dict.TryGetValue(name, out value)) {
                return value != Uninitialized.Instance;
            }

            if (TypeCache.SystemState.TryGetBoundMember(context, this, name, out value)) return true;

            return false;
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            TypeHelpers.SetAttrWithCustomDict(TypeCache.SystemState, context, this, _dict, name, value);
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            if (!TypeHelpers.DeleteAttrWithCustomDict(TypeCache.SystemState, context, this, _dict, name)) {
                throw PythonOps.AttributeErrorForMissingAttribute("sys", name);
            }
            return true;
        }

        public IList<object> GetCustomMemberNames(CodeContext context) {
            List ret = new List(((IDictionary<object, object>)_dict).Keys);
            foreach (SymbolId id in TypeCache.SystemState.GetMemberNames(context, this))
                ret.AddNoLock(id.ToString());

            return ret;
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            return TypeHelpers.GetAttrDictWithCustomDict(TypeCache.SystemState, context, this, _dict);
        }

        #endregion

        public ClrModule ClrModule {
            get {
                return ClrModule.GetInstance();
            }
        }
        
        /// <summary>
        /// Dictionary from name to type of all known built-in module names.
        /// </summary>
        public Dictionary<string, Type> Builtins {
            get {
                return _builtinsDict;
            }
        }

        /// <summary>
        /// Dictionary from type to name of all built-in modules.
        /// </summary>
        public Dictionary<Type, string> BuiltinModuleNames {
            get {
                return _builtinModuleNames;
            }
        }

        List<Assembly> _autoLoadBuiltins = new List<Assembly>();

        private void InitializeBuiltins() {
            // We should register builtins, if any, from IronPython.dll
            _autoLoadBuiltins.Add(typeof(SystemState).Assembly);

            DynamicHelpers.TopPackage.AssemblyLoaded += new EventHandler<AssemblyLoadedEventArgs>(TopPackage_AssemblyLoaded);

            PythonExtensionTypeAttribute._sysState = this;
            // Load builtins from IronPython.Modules
            Assembly ironPythonModules;
            ironPythonModules = ScriptDomainManager.CurrentManager.PAL.LoadAssembly(GetIronPythonAssembly("IronPython.Modules"));
            _autoLoadBuiltins.Add(ironPythonModules);

            foreach(Assembly builtinsAssembly in _autoLoadBuiltins) {
                LoadBuiltins(builtinsAssembly);
            }

            if (Environment.OSVersion.Platform == PlatformID.Unix) {
                // we make our nt package show up as a posix package
                // on unix platforms.  Because we build on top of the 
                // CLI for all file operations we should be good from
                // there, but modules that check for the presence of
                // names (e.g. os) will do the right thing.
                Debug.Assert(_builtinsDict.ContainsKey("nt"));
                _builtinsDict["posix"] = _builtinsDict["nt"];
                _builtinsDict.Remove("nt");
            }            
        }

        private void TopPackage_AssemblyLoaded(object sender, AssemblyLoadedEventArgs e) {
            Assembly assem = e.Assembly;
            if (_autoLoadBuiltins.Contains(assem)) {
                // We add builtins from these assemblies at startup
                return;
            }

            LoadBuiltins(e.Assembly);
        }

        private void LoadBuiltins(Assembly assem) {
            object [] attrs = assem.GetCustomAttributes(typeof(PythonModuleAttribute), false);
            if (attrs.Length > 0) {
                foreach (PythonModuleAttribute pma in attrs) {
                    Builtins.Add(pma.Name, pma.Type);
                    BuiltinModuleNames[pma.Type] = pma.Name;
                }

                object[] keys = new object[_builtinsDict.Keys.Count];
                int index = 0;
                foreach (object key in _builtinsDict.Keys) {
                    keys[index++] = key;
                }
                builtin_module_names = Tuple.MakeTuple(keys);
            }
        }

        public static string GetIronPythonAssembly(string baseName) {
#if SIGNED
            return baseName + ", Version=" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ", Culture=neutral, PublicKeyToken=31bf3856ad364e35";
#else
        return baseName;
#endif
        }
    }
}
