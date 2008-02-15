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
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.IO;
using System.Diagnostics;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Contracts;

using IronPython.Runtime.Types;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

[assembly: PythonModule("clr", typeof(IronPython.Modules.ClrModule))]
namespace IronPython.Modules {
    /// <summary>
    /// this class contains objecs and static methods used for
    /// .NET/CLS interop with Python.  
    /// </summary>
    public static class ClrModule {
        public static ReferencesList References = new ReferencesList();

        #region Public methods

        // TODO: should be a property
        public static IScriptEnvironment/*!*/ GetCurrentRuntime(CodeContext/*!*/ context) {
            return context.LanguageContext.DomainManager.Environment;
        }

        public static void AddReference(CodeContext/*!*/ context, params object[] references) {
            if (references == null) throw new ArgumentTypeException("Expected string or Assembly, got NoneType");
            if (references.Length == 0) throw new ArgumentException("Expected at least one name, got none");
            Contract.RequiresNotNull(context, "context");

            foreach (object reference in references) {
                AddReference(context, reference);
            }
        }

        public static void AddReferenceToFile(CodeContext/*!*/ context, params string[] files) {
            if (files == null) throw new ArgumentTypeException("Expected string, got NoneType");
            if (files.Length == 0) throw new ArgumentException("Expected at least one name, got none");
            Contract.RequiresNotNull(context, "context");

            foreach (string file in files) {
                AddReferenceToFile(context, file);
            }
        }

        public static void AddReferenceByName(CodeContext/*!*/ context, params string[] names) {
            if (names == null) throw new ArgumentTypeException("Expected string, got NoneType");
            if (names.Length == 0) throw new ArgumentException("Expected at least one name, got none");
            Contract.RequiresNotNull(context, "context");

            foreach (string name in names) {
                AddReferenceByName(context, name);
            }
        }

#if !SILVERLIGHT // files, paths
        public static void AddReferenceByPartialName(CodeContext/*!*/ context, params string[] names) {
            if (names == null) throw new ArgumentTypeException("Expected string, got NoneType");
            if (names.Length == 0) throw new ArgumentException("Expected at least one name, got none");
            Contract.RequiresNotNull(context, "context");

            foreach (string name in names) {
                AddReferenceByPartialName(context, name);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        public static Assembly/*!*/ LoadAssemblyFromFileWithPath(string/*!*/ file) {
            if (file == null) throw new ArgumentTypeException("LoadAssemblyFromFileWithPath: arg 1 must be a string.");
            // We use Assembly.LoadFile instead of Assembly.LoadFrom as the latter first tries to use Assembly.Load
            return Assembly.LoadFile(file);
        }

        public static Assembly/*!*/ LoadAssemblyFromFile(CodeContext/*!*/ context, string/*!*/ file) {
            if (file == null) throw new ArgumentTypeException("Expected string, got NoneType");
            if (file.Length == 0) throw new ArgumentException("assembly name must not be empty string");
            Contract.RequiresNotNull(context, "context");

            if (file.IndexOf(System.IO.Path.DirectorySeparatorChar) != -1) {
                throw new ArgumentException("filenames must not contain full paths, first add the path to sys.path");
            }

            return context.LanguageContext.LoadAssemblyFromFile(file);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadWithPartialName")]
        public static Assembly/*!*/ LoadAssemblyByPartialName(string/*!*/ name) {
            if (name == null) {
                throw new ArgumentTypeException("LoadAssemblyByPartialName: arg 1 must be a string");
            }

#pragma warning disable 618
            return Assembly.LoadWithPartialName(name);
#pragma warning restore 618
        }
#endif

        public static Assembly/*!*/ LoadAssemblyByName(CodeContext/*!*/ context, string/*!*/ name) {
            if (name == null) {
                throw new ArgumentTypeException("LoadAssemblyByName: arg 1 must be a string");
            }

            return PythonContext.GetContext(context).DomainManager.PAL.LoadAssembly(name);
        }

        public static object Use(CodeContext context, string/*!*/ name) {
            Contract.RequiresNotNull(context, "context");

            if (name == null) {
                throw new ArgumentTypeException("Use: arg 1 must be a string");
            }

            object res = context.LanguageContext.DomainManager.UseModule(name);
            if (res == null) {
                throw new ArgumentException(String.Format("couldn't find module {0} to use", name));
            }

            return res;
        }

        public static object/*!*/ Use(CodeContext/*!*/ context, string/*!*/ path, string/*!*/ language) {
            Contract.RequiresNotNull(context, "context"); 
            
            if (path == null) {
                throw new ArgumentTypeException("Use: arg 1 must be a string");
            } 
            if (language == null) {
                throw new ArgumentTypeException("Use: arg 2 must be a string");
            }

            object res = context.LanguageContext.DomainManager.UseModule(path, language);
            if (res == null) {
                throw new ArgumentException(String.Format("couldn't load module at path '{0}' in language '{1}'", path, language));
            }

            return res;
        }

        public static CommandDispatcher SetCommandDispatcher(CodeContext/*!*/ context, CommandDispatcher dispatcher) {
            Contract.RequiresNotNull(context, "context"); 
            
            return context.LanguageContext.DomainManager.SetCommandDispatcher(dispatcher);
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
            // Load the assembly into IronPython
            if (context.LanguageContext.DomainManager.LoadAssembly(assembly)) {
                // Add it to the references tuple if we
                // loaded a new assembly.
                References.Add(assembly);
            }
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
            Assembly asm = context.LanguageContext.DomainManager.PAL.LoadAssemblyFromPath(file);
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
            Contract.RequiresNotNull(context, "context");

            Assembly asm = LoadAssemblyByPartialName(name);
            if (asm == null) {
                throw new IOException(String.Format("Could not add reference to assembly {0}", name));
            }

            AddReference(context, asm);
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
        public class ReferencesList : List<Assembly>, ICodeFormattable {

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

            [SpecialName]
            public string ToCodeString(CodeContext context) {
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

        public static void AddReferenceToFileAndPath(CodeContext/*!*/ context, params string[] files) {
            if (files == null) throw new ArgumentTypeException("Expected string, got NoneType");
            Contract.RequiresNotNull(context, "context");

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

            list.Add(path);

            Assembly asm = DefaultContext.Default.LanguageContext.LoadAssemblyFromFile(file);
            if (asm == null) throw PythonOps.IOError("file does not exist: {0}", file);
            AddReference(context, asm);
        }

#endif

        public static Type GetClrType(Type type) {
            return type;
        }

        /// <summary>
        /// TODO: Remove me before 2.0 ships (not necessary for backwards compatibility except for w/ alpha 2.0 builds)... 
        /// </summary>
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

            #region IFancyCallable Members
            [SpecialName]
            public object Call(CodeContext context, [ParamDictionary] IAttributesCollection dict, params object[] args) {
                ValidateArgs(args);

                if (_inst != null) {
                    return PythonCalls.CallWithKeywordArgs(_func, ArrayUtils.Insert(_inst, args), dict);
                } else {
                    return PythonCalls.CallWithKeywordArgs(_func, args, dict);
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

            #region IFancyCallable Members
            [SpecialName]
            public object Call(CodeContext context, [ParamDictionary] IAttributesCollection dict, params object[] args) {
                object ret;
                if (_inst != null) {
                    ret = PythonCalls.CallWithKeywordArgs(_func, ArrayUtils.Insert(_inst, args), dict);
                } else {
                    return PythonCalls.CallWithKeywordArgs(_func, args, dict);
                }
                ValidateReturn(ret);
                return ret;
            }

            #endregion
        }

        // backwards compatibility w/ IronPython v1.x
        public static PythonType GetPythonType(Type t) {
            return DynamicHelpers.GetPythonTypeFromType(t);
        }

    }
}
