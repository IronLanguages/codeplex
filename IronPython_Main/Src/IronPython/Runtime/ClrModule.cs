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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

#if !SILVERLIGHT
using ComTypeLibInfo = Microsoft.Scripting.ComInterop.ComTypeLibInfo;
using ComTypeLibDesc = Microsoft.Scripting.ComInterop.ComTypeLibDesc;
using System.Runtime.Serialization.Formatters.Binary;
#endif

[assembly: PythonModule("clr", typeof(IronPython.Runtime.ClrModule))]
namespace IronPython.Runtime {
    /// <summary>
    /// this class contains objecs and static methods used for
    /// .NET/CLS interop with Python.  
    /// </summary>
    public static class ClrModule {
        [SpecialName]
        public static void PerformModuleReload(PythonContext/*!*/ context, IAttributesCollection/*!*/ dict) {
            if (!dict.ContainsKey(SymbolTable.StringToId("References"))) {
                dict[SymbolTable.StringToId("References")] = context.ReferencedAssemblies;
            }
        }

        #region Public methods

        // TODO: should be a property
        public static ScriptDomainManager/*!*/ GetCurrentRuntime(CodeContext/*!*/ context) {
            return context.LanguageContext.DomainManager;
        }

        [Documentation(@"Adds a reference to a .NET assembly.  Parameters can be an already loaded
Assembly object, a full assembly name, or a partial assembly name. After the
load the assemblies namespaces and top-level types will be available via 
import Namespace.")]
        public static void AddReference(CodeContext/*!*/ context, params object[] references) {
            if (references == null) throw new ArgumentTypeException("Expected string or Assembly, got NoneType");
            if (references.Length == 0) throw new ArgumentException("Expected at least one name, got none");
            ContractUtils.RequiresNotNull(context, "context");

            foreach (object reference in references) {
                AddReference(context, reference);
            }
        }

        [Documentation(@"Adds a reference to a .NET assembly.  One or more assembly names can
be provided.  The assembly is searched for in the directories specified in 
sys.path and dependencies will be loaded from sys.path as well.  The assembly 
name should be the filename on disk without a directory specifier and 
optionally including the .EXE or .DLL extension. After the load the assemblies 
namespaces and top-level types will be available via import Namespace.")]
        public static void AddReferenceToFile(CodeContext/*!*/ context, params string[] files) {
            if (files == null) throw new ArgumentTypeException("Expected string, got NoneType");
            if (files.Length == 0) throw new ArgumentException("Expected at least one name, got none");
            ContractUtils.RequiresNotNull(context, "context");

            foreach (string file in files) {
                AddReferenceToFile(context, file);
            }
        }

        [Documentation(@"Adds a reference to a .NET assembly.  Parameters are an assembly name. 
After the load the assemblies namespaces and top-level types will be available via 
import Namespace.")]
        public static void AddReferenceByName(CodeContext/*!*/ context, params string[] names) {
            if (names == null) throw new ArgumentTypeException("Expected string, got NoneType");
            if (names.Length == 0) throw new ArgumentException("Expected at least one name, got none");
            ContractUtils.RequiresNotNull(context, "context");

            foreach (string name in names) {
                AddReferenceByName(context, name);
            }
        }

#if !SILVERLIGHT // files, paths

        public static ComTypeLibInfo LoadTypeLibrary(CodeContext/*!*/ context, object rcw) {
            return ComTypeLibDesc.CreateFromObject(rcw);
        }

        public static ComTypeLibInfo LoadTypeLibrary(CodeContext/*!*/ context, Guid typeLibGuid) {
            return ComTypeLibDesc.CreateFromGuid(typeLibGuid);
        }

        public static void AddReferenceToTypeLibrary(CodeContext/*!*/ context, object rcw) {
            ComTypeLibInfo typeLibInfo;
            typeLibInfo = ComTypeLibDesc.CreateFromObject(rcw);
            PublishTypeLibDesc(context, typeLibInfo.TypeLibDesc);
        }

        public static void AddReferenceToTypeLibrary(CodeContext/*!*/ context, Guid typeLibGuid) {
            ComTypeLibInfo typeLibInfo;
            typeLibInfo = ComTypeLibDesc.CreateFromGuid(typeLibGuid);
            PublishTypeLibDesc(context, typeLibInfo.TypeLibDesc);
        }

        [Documentation(@"Adds a reference to a .NET assembly.  Parameters are a partial assembly name. 
After the load the assemblies namespaces and top-level types will be available via 
import Namespace.")]
        public static void AddReferenceByPartialName(CodeContext/*!*/ context, params string[] names) {
            if (names == null) throw new ArgumentTypeException("Expected string, got NoneType");
            if (names.Length == 0) throw new ArgumentException("Expected at least one name, got none");
            ContractUtils.RequiresNotNull(context, "context");

            foreach (string name in names) {
                AddReferenceByPartialName(context, name);
            }
        }

        [Documentation(@"Adds a reference to a .NET assembly.  Parameters are a full path to an. 
assembly on disk. After the load the assemblies namespaces and top-level types 
will be available via import Namespace.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        public static Assembly/*!*/ LoadAssemblyFromFileWithPath(string/*!*/ file) {
            if (file == null) throw new ArgumentTypeException("LoadAssemblyFromFileWithPath: arg 1 must be a string.");
            // We use Assembly.LoadFile instead of Assembly.LoadFrom as the latter first tries to use Assembly.Load
            return Assembly.LoadFile(file);
        }

        [Documentation(@"Loads an assembly from the specified filename and returns the assembly
object.  Namespaces or types in the assembly can be accessed directly from 
the assembly object.")]
        public static Assembly/*!*/ LoadAssemblyFromFile(CodeContext/*!*/ context, string/*!*/ file) {
            if (file == null) throw new ArgumentTypeException("Expected string, got NoneType");
            if (file.Length == 0) throw new ArgumentException("assembly name must not be empty string");
            ContractUtils.RequiresNotNull(context, "context");

            if (file.IndexOf(System.IO.Path.DirectorySeparatorChar) != -1) {
                throw new ArgumentException("filenames must not contain full paths, first add the path to sys.path");
            }

            return context.LanguageContext.LoadAssemblyFromFile(file);
        }

        [Documentation(@"Loads an assembly from the specified partial assembly name and returns the 
assembly object.  Namespaces or types in the assembly can be accessed directly 
from the assembly object.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadWithPartialName")]
        public static Assembly/*!*/ LoadAssemblyByPartialName(string/*!*/ name) {
            if (name == null) {
                throw new ArgumentTypeException("LoadAssemblyByPartialName: arg 1 must be a string");
            }

#pragma warning disable 618 // csc
#pragma warning disable 612 // gmcs
            return Assembly.LoadWithPartialName(name);
#pragma warning restore 618
#pragma warning restore 612
        }
#endif

        [Documentation(@"Loads an assembly from the specified assembly name and returns the assembly
object.  Namespaces or types in the assembly can be accessed directly from 
the assembly object.")]
        public static Assembly/*!*/ LoadAssemblyByName(CodeContext/*!*/ context, string/*!*/ name) {
            if (name == null) {
                throw new ArgumentTypeException("LoadAssemblyByName: arg 1 must be a string");
            }

            return PythonContext.GetContext(context).DomainManager.Platform.LoadAssembly(name);
        }

        public static object Use(CodeContext/*!*/ context, string/*!*/ name) {
            ContractUtils.RequiresNotNull(context, "context");

            if (name == null) {
                throw new ArgumentTypeException("Use: arg 1 must be a string");
            }

            var scope = Importer.TryImportSourceFile(PythonContext.GetContext(context), name);
            if (scope == null) {
                throw new ArgumentException(String.Format("couldn't find module {0} to use", name));
            }
            return scope;
        }

        public static object/*!*/ Use(CodeContext/*!*/ context, string/*!*/ path, string/*!*/ language) {
            ContractUtils.RequiresNotNull(context, "context");

            if (path == null) {
                throw new ArgumentTypeException("Use: arg 1 must be a string");
            }

            if (language == null) {
                throw new ArgumentTypeException("Use: arg 2 must be a string");
            }

            var manager = context.LanguageContext.DomainManager;
            if (!manager.Platform.FileExists(path)) {
                throw new ArgumentException(String.Format("couldn't load module at path '{0}' in language '{1}'", path, language));
            }

            var sourceUnit = manager.GetLanguageByName(language).CreateFileUnit(path);
            return Importer.ExecuteSourceUnit(sourceUnit);
        }

        public static CommandDispatcher SetCommandDispatcher(CodeContext/*!*/ context, CommandDispatcher dispatcher) {
            ContractUtils.RequiresNotNull(context, "context");

            return ((PythonContext)context.LanguageContext).GetSetCommandDispatcher(dispatcher);
        }

        #endregion

        #region Private implementation methods

        private static void AddReference(CodeContext/*!*/ context, object reference) {
            Assembly asmRef = reference as Assembly;
            if (asmRef != null) {
                AddReference(context, asmRef);
                return;
            }

            string strRef = reference as string;
            if (strRef != null) {
                AddReference(context, strRef);
                return;
            }

            throw new ArgumentTypeException(String.Format("Invalid assembly type. Expected string or Assembly, got {0}.", reference));
        }

        private static void AddReference(CodeContext/*!*/ context, Assembly assembly) {
            context.LanguageContext.DomainManager.LoadAssembly(assembly);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // TODO: fix
        private static void AddReference(CodeContext/*!*/ context, string name) {
            if (name == null) throw new ArgumentTypeException("Expected string, got NoneType");

            Assembly asm = null;

            try {
                asm = LoadAssemblyByName(context, name);
            } catch { }

            // note we don't explicit call to get the file version
            // here because the assembly resolve event will do it for us.

#if !SILVERLIGHT // files, paths
            if (asm == null) {
                asm = LoadAssemblyByPartialName(name);
            }
#endif

            if (asm == null) {
                throw new IOException(String.Format("Could not add reference to assembly {0}", name));
            }
            AddReference(context, asm);
        }

        private static void AddReferenceToFile(CodeContext/*!*/ context, string file) {
            if (file == null) throw new ArgumentTypeException("Expected string, got NoneType");

#if SILVERLIGHT
            Assembly asm = context.LanguageContext.DomainManager.Platform.LoadAssemblyFromPath(file);
#else
            Assembly asm = LoadAssemblyFromFile(context, file);
#endif
            if (asm == null) {
                throw new IOException(String.Format("Could not add reference to assembly {0}", file));
            }

            AddReference(context, asm);
        }

#if !SILVERLIGHT // files, paths
        private static void AddReferenceByPartialName(CodeContext/*!*/ context, string name) {
            if (name == null) throw new ArgumentTypeException("Expected string, got NoneType");
            ContractUtils.RequiresNotNull(context, "context");

            Assembly asm = LoadAssemblyByPartialName(name);
            if (asm == null) {
                throw new IOException(String.Format("Could not add reference to assembly {0}", name));
            }

            AddReference(context, asm);
        }

        private static void PublishTypeLibDesc(CodeContext context, ComTypeLibDesc typeLibDesc) {
            SymbolId symbol = SymbolTable.StringToId(typeLibDesc.Name);
            context.LanguageContext.DomainManager.Globals.SetName(symbol, typeLibDesc);
        }
#endif
        private static void AddReferenceByName(CodeContext/*!*/ context, string name) {
            if (name == null) throw new ArgumentTypeException("Expected string, got NoneType");

            Assembly asm = LoadAssemblyByName(context, name);

            if (asm == null) {
                throw new IOException(String.Format("Could not add reference to assembly {0}", name));
            }

            AddReference(context, asm);
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public sealed class ReferencesList : List<Assembly>, ICodeFormattable {

            public new void Add(Assembly other) {
                base.Add(other);
            }

            [SpecialName]
            public ClrModule.ReferencesList Add(object other) {
                IEnumerator ie = PythonOps.GetEnumerator(other);
                while (ie.MoveNext()) {
                    Assembly cur = ie.Current as Assembly;
                    if (cur == null) throw PythonOps.TypeError("non-assembly added to references list");

                    base.Add(cur);
                }
                return this;
            }

            public string/*!*/ __repr__(CodeContext/*!*/ context) {
                StringBuilder res = new StringBuilder("(");
                string comma = "";
                foreach (Assembly asm in this) {
                    res.Append(comma);
                    res.Append('<');
                    res.Append(asm.FullName);
                    res.Append('>');
                    comma = "," + Environment.NewLine;
                }

                res.AppendLine(")");
                return res.ToString();
            }
        }

        private static PythonType _strongBoxType;

        #region Runtime Type Checking support
#if !SILVERLIGHT // files, paths

        [Documentation(@"Adds a reference to a .NET assembly.  One or more assembly names can
be provided which are fully qualified names to the file on disk.  The 
directory is added to sys.path and AddReferenceToFile is then called. After the 
load the assemblies namespaces and top-level types will be available via 
import Namespace.")]
        public static void AddReferenceToFileAndPath(CodeContext/*!*/ context, params string[] files) {
            if (files == null) throw new ArgumentTypeException("Expected string, got NoneType");
            ContractUtils.RequiresNotNull(context, "context");

            foreach (string file in files) {
                AddReferenceToFileAndPath(context, file);
            }
        }

        private static void AddReferenceToFileAndPath(CodeContext/*!*/ context, string file) {
            if (file == null) throw PythonOps.TypeError("Expected string, got NoneType");

            // update our path w/ the path of this file...
            string path = System.IO.Path.GetDirectoryName(file);
            List list;

            if (!PythonContext.GetContext(context).TryGetSystemPath(out list)) {
                throw PythonOps.TypeError("cannot update path, it is not a list");
            }

            list.append(path);

            Assembly asm = DefaultContext.Default.LanguageContext.LoadAssemblyFromFile(file);
            if (asm == null) throw PythonOps.IOError("file does not exist: {0}", file);
            AddReference(context, asm);
        }

#endif

        public static Type GetClrType(Type type) {
            return type;
        }

        /// <summary>
        /// TODO: Remove me before 3.0 ships (not necessary for backwards compatibility except for w/ alpha 2.0 builds)... 
        /// </summary>
        [Obsolete("Call clr.GetPythonType instead")]
        public static PythonType GetDynamicType(Type t) {
            return DynamicHelpers.GetPythonTypeFromType(t);
        }

        public static PythonType Reference {
            get {
                return StrongBox;
            }
        }


        public static PythonType StrongBox {
            get {
                if (_strongBoxType == null) {
                    _strongBoxType = DynamicHelpers.GetPythonTypeFromType(typeof(StrongBox<>));
                }
                return _strongBoxType;
            }
        }

        public static object accepts(params object[] types) {
            return new ArgChecker(types);
        }

        public static object returns(object type) {
            return new ReturnChecker(type);
        }

        public static object Self() {
            return null;
        }

        #endregion

        public class ArgChecker {
            private object[] expected;

            public ArgChecker(object[] prms) {
                expected = prms;
            }

            #region ICallableWithCodeContext Members

            [SpecialName]
            public object Call(CodeContext context, object func) {
                // expect only to receive the function we'll call here.

                return new RuntimeArgChecker(func, expected);
            }

            #endregion
        }

        public class RuntimeArgChecker : PythonTypeSlot {
            private object[] _expected;
            private object _func;
            private object _inst;

            public RuntimeArgChecker(object function, object[] expectedArgs) {
                _expected = expectedArgs;
                _func = function;
            }

            public RuntimeArgChecker(object instance, object function, object[] expectedArgs)
                : this(function, expectedArgs) {
                _inst = instance;
            }

            private void ValidateArgs(object[] args) {
                int start = 0;

                if (_inst != null) {
                    start = 1;
                }


                // no need to validate self... the method should handle it.
                for (int i = start; i < args.Length + start; i++) {
                    PythonType dt = DynamicHelpers.GetPythonType(args[i - start]);

                    PythonType expct = _expected[i] as PythonType;
                    if (expct == null) expct = ((OldClass)_expected[i]).TypeObject;
                    if (dt != _expected[i] && !dt.IsSubclassOf(expct)) {
                        throw PythonOps.AssertionError("argument {0} has bad value (got {1}, expected {2})", i, dt, _expected[i]);
                    }
                }
            }

            #region ICallableWithCodeContext Members
            [SpecialName]
            public object Call(CodeContext context, params object[] args) {
                ValidateArgs(args);

                if (_inst != null) {
                    return PythonOps.CallWithContext(context, _func, ArrayUtils.Insert(_inst, args));
                } else {
                    return PythonOps.CallWithContext(context, _func, args);
                }
            }

            #endregion

            internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
                value = new RuntimeArgChecker(instance, _func, _expected);
                return true;
            }

            internal override bool GetAlwaysSucceeds {
                get {
                    return true;
                }
            }

            #region IFancyCallable Members
            [SpecialName]
            public object Call(CodeContext context, [ParamDictionary] IAttributesCollection dict, params object[] args) {
                ValidateArgs(args);

                if (_inst != null) {
                    return PythonCalls.CallWithKeywordArgs(context, _func, ArrayUtils.Insert(_inst, args), dict);
                } else {
                    return PythonCalls.CallWithKeywordArgs(context, _func, args, dict);
                }
            }

            #endregion
        }

        public class ReturnChecker {
            public object retType;

            public ReturnChecker(object returnType) {
                retType = returnType;
            }

            #region ICallableWithCodeContext Members
            [SpecialName]
            public object Call(CodeContext context, object func) {
                // expect only to receive the function we'll call here.
                return new RuntimeReturnChecker(func, retType);
            }

            #endregion
        }

        public class RuntimeReturnChecker : PythonTypeSlot {
            private object _retType;
            private object _func;
            private object _inst;

            public RuntimeReturnChecker(object function, object expectedReturn) {
                _retType = expectedReturn;
                _func = function;
            }

            public RuntimeReturnChecker(object instance, object function, object expectedReturn)
                : this(function, expectedReturn) {
                _inst = instance;
            }

            private void ValidateReturn(object ret) {
                // we return void...
                if (ret == null && _retType == null) return;

                PythonType dt = DynamicHelpers.GetPythonType(ret);
                if (dt != _retType) {
                    PythonType expct = _retType as PythonType;
                    if (expct == null) expct = ((OldClass)_retType).TypeObject;

                    if (!dt.IsSubclassOf(expct))
                        throw PythonOps.AssertionError("bad return value returned (expected {0}, got {1})", _retType, dt);
                }
            }

            #region ICallableWithCodeContext Members
            [SpecialName]
            public object Call(CodeContext context, params object[] args) {
                object ret;
                if (_inst != null) {
                    ret = PythonOps.CallWithContext(context, _func, ArrayUtils.Insert(_inst, args));
                } else {
                    ret = PythonOps.CallWithContext(context, _func, args);
                }
                ValidateReturn(ret);
                return ret;
            }

            #endregion

            #region IDescriptor Members

            public object GetAttribute(object instance, object owner) {
                return new RuntimeReturnChecker(instance, _func, _retType);
            }


            #endregion

            internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
                value = GetAttribute(instance, owner);
                return true;
            }

            internal override bool GetAlwaysSucceeds {
                get {
                    return true;
                }
            }

            #region IFancyCallable Members
            [SpecialName]
            public object Call(CodeContext context, [ParamDictionary] IAttributesCollection dict, params object[] args) {
                object ret;
                if (_inst != null) {
                    ret = PythonCalls.CallWithKeywordArgs(context, _func, ArrayUtils.Insert(_inst, args), dict);
                } else {
                    return PythonCalls.CallWithKeywordArgs(context, _func, args, dict);
                }
                ValidateReturn(ret);
                return ret;
            }

            #endregion
        }

        public static PythonType GetPythonType(Type t) {
            return DynamicHelpers.GetPythonTypeFromType(t);
        }

        /// <summary>
        /// returns the result of dir(o) as-if "import clr" has not been performed.
        /// </summary>
        public static List Dir(object o) {
            IList<object> ret = PythonOps.GetAttrNames(DefaultContext.Default, o);
            List lret = new List(ret);
            lret.sort(DefaultContext.Default);
            return lret;
        }

        /// <summary>
        /// Returns the result of dir(o) as-if "import clr" has been performed.
        /// </summary>
        public static List DirClr(object o) {
            IList<object> ret = PythonOps.GetAttrNames(DefaultContext.DefaultCLS, o);
            List lret = new List(ret);
            lret.sort(DefaultContext.DefaultCLS);
            return lret;
        }

        /// <summary>
        /// Provides a helper for compiling a group of modules into a single assembly.  The assembly can later be
        /// reloaded using the clr.AddReference API.
        /// </summary>
        public static void CompileModules(CodeContext/*!*/ context, string/*!*/ assemblyName, [ParamDictionary]IAttributesCollection kwArgs, params string/*!*/[]/*!*/ filenames) {
            ContractUtils.RequiresNotNull(assemblyName, "assemblyName");
            ContractUtils.RequiresNotNullItems(filenames, "filenames");

            PythonContext pc = PythonContext.GetContext(context);

            for (int i = 0; i < filenames.Length; i++) {
                filenames[i] = Path.GetFullPath(filenames[i]);
            }

            Dictionary<string, string> packageMap = BuildPackageMap(filenames);

            List<ScriptCode> code = new List<ScriptCode>();
            foreach (string filename in filenames) {
                if (!pc.DomainManager.Platform.FileExists(filename)) {
                    throw PythonOps.IOError("Couldn't find file for compilation: {0}", filename);
                }

                ScriptCode sc;

                string modName;
                string dname = Path.GetDirectoryName(filename);
                string outFilename = "";
                if (Path.GetFileName(filename) == "__init__.py") {
                    // remove __init__.py to get package name
                    dname = Path.GetDirectoryName(dname);
                    if (String.IsNullOrEmpty(dname)) {
                        modName = Path.GetDirectoryName(filename);
                    } else {
                        modName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(filename));
                    }
                    outFilename = Path.DirectorySeparatorChar + "__init__.py";
                } else {
                    modName = Path.GetFileNameWithoutExtension(filename);
                }
                
                // see if we have a parent package, if so incorporate it into
                // our name
                string parentPackage;
                if (packageMap.TryGetValue(dname, out parentPackage)) {
                    modName = parentPackage + "." + modName;
                }

                outFilename = modName.Replace('.', Path.DirectorySeparatorChar) + outFilename;

                SourceUnit su = pc.CreateSourceUnit(
                    new FileStreamContentProvider(
                        context.LanguageContext.DomainManager.Platform,
                        filename
                    ),
                    outFilename,
                    pc.DefaultEncoding,
                    SourceCodeKind.File
                );

                sc = PythonContext.GetContext(context).GetScriptCode(su, modName, ModuleOptions.Initialize, CompilationMode.ToDisk);

                code.Add(sc);
            }

            object mainModule;
            if (kwArgs != null && kwArgs.TryGetValue(SymbolTable.StringToId("mainModule"), out mainModule)) {
                string strModule = mainModule as string;
                if (strModule != null) {
                    if (!pc.DomainManager.Platform.FileExists(strModule)) {
                        throw PythonOps.IOError("Couldn't find main file for compilation: {0}", strModule);
                    }
                    
                    SourceUnit su = pc.CreateFileUnit(strModule, pc.DefaultEncoding, SourceCodeKind.File);
                    code.Add(PythonContext.GetContext(context).GetScriptCode(su, "__main__", ModuleOptions.Initialize, CompilationMode.ToDisk));
                }
            }

            ScriptCode.SaveToAssembly(assemblyName, code.ToArray());
        }

        /// <summary>
        /// clr.CompileSubclassTypes(assemblyName, *typeDescription)
        /// 
        /// Provides a helper for creating an assembly which contains pre-generated .NET 
        /// base types for new-style types.
        /// 
        /// This assembly can then be AddReferenced or put sys.prefix\DLLs and the cached 
        /// types will be used instead of generating the types at runtime.
        /// 
        /// This function takes the name of the assembly to save to and then an arbitrary 
        /// number of parameters describing the types to be created.  Each of those
        /// parameter can either be a plain type or a sequence of base types.
        /// 
        /// clr.CompileSubclassTypes(object) -> create a base type for object
        /// clr.CompileSubclassTypes(object, str, System.Collections.ArrayList) -> create 
        ///     base  types for both object and ArrayList.
        ///     
        /// clr.CompileSubclassTypes(object, (object, IComparable)) -> create base types for 
        ///     object and an object which implements IComparable.
        /// 
        /// </summary>
        public static void CompileSubclassTypes(string/*!*/ assemblyName, params object[] newTypes) {
            if (assemblyName == null) {
                throw PythonOps.TypeError("CompileTypes expected str for assemblyName, got NoneType");
            }

            var typesToCreate = new List<PythonTuple>();
            foreach (object o in newTypes) {
                if (o is PythonType) {
                    typesToCreate.Add(PythonTuple.MakeTuple(o));
                } else {
                    typesToCreate.Add(PythonTuple.Make(o));
                }
            }

            NewTypeMaker.SaveNewTypes(assemblyName, typesToCreate);
        }

        /// <summary>
        /// clr.GetSubclassedTypes() -> tuple
        /// 
        /// Returns a tuple of information about the types which have been subclassed. 
        /// 
        /// This tuple can be passed to clr.CompileSubclassTypes to cache these
        /// types on disk such as:
        /// 
        /// clr.CompileSubclassTypes('assembly', *clr.GetSubclassedTypes())
        /// </summary>
        public static PythonTuple GetSubclassedTypes() {
            List<object> res = new List<object>();
            
            foreach (NewTypeInfo info in NewTypeMaker._newTypes.Keys) {
                Type clrBaseType = info.BaseType;
                Type tempType = clrBaseType;
                while (tempType != null) {
                    if (tempType.IsGenericType && tempType.GetGenericTypeDefinition() == typeof(Extensible<>)) {
                        clrBaseType = tempType.GetGenericArguments()[0];
                        break;
                    }
                    tempType = tempType.BaseType;
                }

                PythonType baseType = DynamicHelpers.GetPythonTypeFromType(clrBaseType);
                if (info.InterfaceTypes.Count == 0) {
                    res.Add(baseType);
                } else if (info.InterfaceTypes.Count > 0) {
                    PythonType[] types = new PythonType[info.InterfaceTypes.Count + 1];
                    types[0] = baseType;
                    for (int i = 0; i < info.InterfaceTypes.Count; i++) {
                        types[i + 1] = DynamicHelpers.GetPythonTypeFromType(info.InterfaceTypes[i]);
                    }
                    res.Add(PythonTuple.MakeTuple(types));
                }
            }

            return PythonTuple.MakeTuple(res.ToArray());
        }

        /// <summary>
        /// Provides a StreamContentProvider for a stream of content backed by a file on disk.
        /// </summary>
        [Serializable]
        internal sealed class FileStreamContentProvider : StreamContentProvider {
            private readonly string _path;
            private readonly PALHolder _pal;

            internal string Path {
                get { return _path; }
            }

            #region Construction

            internal FileStreamContentProvider(PlatformAdaptationLayer manager, string path) {
                ContractUtils.RequiresNotNull(path, "path");

                _path = path;
                _pal = new PALHolder(manager);
            }

            #endregion

            public override Stream GetStream() {
                return _pal.GetStream(Path);
            }

            [Serializable]
            private class PALHolder
#if !SILVERLIGHT
 : MarshalByRefObject
#endif
 {
                [NonSerialized]
                private readonly PlatformAdaptationLayer _pal;

                internal PALHolder(PlatformAdaptationLayer pal) {
                    _pal = pal;
                }

                internal Stream GetStream(string path) {
                    return _pal.OpenInputFileStream(path);
                }
            }
        }

        /// <summary>
        /// Goes through the list of files identifying the relationship between packages
        /// and subpackages.  Returns a dictionary with all of the package filenames (minus __init__.py)
        /// mapping to their full name.  For example given a structure:
        /// 
        /// C:\
        ///     someDir\
        ///         package\
        ///             __init__.py
        ///             a.py
        ///             b\
        ///                 __init.py
        ///                 c.py
        ///         
        /// Returns:
        ///     {r'C:\somedir\package' : 'package', r'C:\somedir\package\b', 'package.b'}
        ///     
        /// This can then be used for calculating the full module name of individual files
        /// and packages.  For example a's full name is "package.a" and c's full name is
        /// "package.b.c".
        /// </summary>
        private static Dictionary<string/*!*/, string/*!*/>/*!*/ BuildPackageMap(string/*!*/[]/*!*/ filenames) {
            // modules which are the children of packages should have the __name__
            // package.subpackage.modulename, not just modulename.  So first
            // we need to get a list of all the modules...
            List<string> modules = new List<string>();
            foreach (string filename in filenames) {
                if (filename.EndsWith("__init__.py")) {
                    // this is a package
                    modules.Add(filename);
                }
            }

            // next we need to understand the relationship between the packages so
            // if we have package.subpackage1 and package.subpackage2 we know
            // both of these are children of the package.  So sort the module names,
            // shortest name first...
            SortModules(modules);

            // finally build up the package names for the dirs...
            Dictionary<string, string> packageMap = new Dictionary<string, string>();
            foreach (string packageName in modules) {
                string dirName = Path.GetDirectoryName(packageName);    // remove __init__.py
                string pkgName = String.Empty;
                string fullName = Path.GetFileName(Path.GetDirectoryName(packageName));

                if (packageMap.TryGetValue(Path.GetDirectoryName(dirName), out pkgName)) {   // remove directory name
                    fullName = pkgName + "." + fullName;
                }

                packageMap[Path.GetDirectoryName(packageName)] = fullName;
            }
            return packageMap;
        }

        private static void SortModules(List<string> modules) {
            modules.Sort((string x, string y) => x.Length - y.Length);
        }
        
#if !SILVERLIGHT
        /// <summary>
        /// Serializes data using the .NET serialization formatter for complex
        /// types.  Returns a tuple identifying the serialization format and the serialized 
        /// data which can be fed back into clr.Deserialize.
        /// 
        /// Current serialization formats include custom formats for primitive .NET
        /// types which aren't already recognized as tuples.  None is used to indicate
        /// that the Binary .NET formatter is used.
        /// </summary>
        public static PythonTuple/*!*/ Serialize(object self) {
            if (self == null) {
                return PythonTuple.MakeTuple(null, String.Empty);
            }

            string data, format;
            switch (Type.GetTypeCode(CompilerHelpers.GetType(self))) {
                // for the primitive non-python types just do a simple
                // serialization
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.DBNull:
                case TypeCode.Decimal:
                case TypeCode.Int16:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    data = self.ToString();
                    format = CompilerHelpers.GetType(self).FullName;
                    break;
                default:
                    // something more complex, let the binary formatter handle it                    
                    BinaryFormatter bf = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream();
                    bf.Serialize(stream, self);
                    data = stream.ToArray().MakeString();
                    format = null;
                    break;
            }

            return PythonTuple.MakeTuple(format, data);
        }

        /// <summary>
        /// Deserializes the result of a Serialize call.  This can be used to perform serialization
        /// for .NET types which are serializable.  This method is the callable object provided
        /// from __reduce_ex__ for .serializable .NET types.
        /// 
        /// The first parameter indicates the serialization format and is the first tuple element
        /// returned from the Serialize call.  
        /// 
        /// The second parameter is the serialized data.
        /// </summary>
        public static object Deserialize(string serializationFormat, [NotNull]string/*!*/ data) {
            if (serializationFormat != null) {
                switch (serializationFormat) {
                    case "System.Byte": return Byte.Parse(data);
                    case "System.Char": return Char.Parse(data);
                    case "System.DBNull": return DBNull.Value;
                    case "System.Decimal": return Decimal.Parse(data);
                    case "System.Int16": return Int16.Parse(data);
                    case "System.Int64": return Int64.Parse(data);
                    case "System.SByte": return SByte.Parse(data);
                    case "System.Single": return Single.Parse(data);
                    case "System.UInt16": return UInt16.Parse(data);
                    case "System.UInt32": return UInt32.Parse(data);
                    case "System.UInt64": return UInt64.Parse(data);
                    default:
                        throw PythonOps.ValueError("unknown serialization format: {0}", serializationFormat);
                }
            } else if (data == String.Empty) {
                return null;
            }

            MemoryStream stream = new MemoryStream(data.MakeByteArray());
            BinaryFormatter bf = new BinaryFormatter();
            return bf.Deserialize(stream);
        }
#endif
    }
}
