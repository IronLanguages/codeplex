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
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;

using Microsoft.Scripting;

using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Hosting;
using System.Threading;
using IronPython.Runtime;

using Ops = IronPython.Runtime.Operations.Ops;
using IronPython.Runtime.Types;

namespace Microsoft.Scripting {
    /// <summary>
    /// this class contains objecs and static methods used for
    /// .NET/CLS interop with Python.  
    /// </summary>
    public sealed class ClrModule : IDisposable {
        private static ClrModule _module;

        public static ClrModule GetInstance() {
            if (_module == null) {
                if (Interlocked.CompareExchange<ClrModule>(ref _module, new ClrModule(), null) == null) {
#if !SILVERLIGHT // AssemblyResolve
                    try {
                        _module.HookAssemblyResolve();
                    } catch (System.Security.SecurityException) {
                        // We may not have SecurityPermissionFlag.ControlAppDomain. 
                        // If so, we will not look up sys.path for module loads
                    }
#endif
                }
            }
            return _module;
        }

        private ClrModule() {
        }

        public override string ToString() {
            return "<module 'clr' (built-in)>";
        }

        public object References = new ReferencesTuple();

        #region Public methods

        public void AddReference(params object[] references) {
            if (references == null) throw Ops.TypeError("Expected string or Assembly, got NoneType");

            foreach (object reference in references) {
                AddReference(reference);
            }
        }

#if !SILVERLIGHT // files, paths

        public void AddReferenceToFileAndPath(params string[] files) {
            if (files == null) throw Ops.TypeError("Expected string, got NoneType");

            foreach (string file in files) {
                AddReferenceToFileAndPath(file);
            }
        }
#endif
        public void AddReferenceToFile(params string[] files) {
            if (files == null) throw Ops.TypeError("Expected string, got NoneType");

            foreach (string file in files) {
                AddReferenceToFile(file);
            }
        }

        public void AddReferenceByName(params string[] names) {
            if (names == null) throw Ops.TypeError("Expected string, got NoneType");

            foreach (string name in names) {
                AddReferenceByName(name);
            }
        }

#if !SILVERLIGHT // files, paths
        public void AddReferenceByPartialName(params string[] names) {
            if (names == null) throw Ops.TypeError("Expected string, got NoneType");

            foreach (string name in names) {
                AddReferenceByPartialName(name);
            }
        }

        public Assembly LoadAssemblyFromFileWithPath(string file) {
            if (file == null) throw Ops.TypeError("LoadAssemblyFromFileWithPath: arg 1 must be a string.");
            return Assembly.LoadFile(file);
        }

        public Assembly LoadAssemblyFromFile(string file) {
            if (file == null) throw Ops.TypeError("Expected string, got NoneType");

            if (file.IndexOf(System.IO.Path.DirectorySeparatorChar) != -1) {
                throw Ops.ValueError("filenames must not contain full paths, first add the path to sys.path");
            }

            // check all files in the path...
            IEnumerator ie = Ops.GetEnumerator(SystemState.Instance.path);
            while (ie.MoveNext()) {
                string str;
                if (Converter.TryConvertToString(ie.Current, out str)) {
                    string fullName = Path.Combine(str, file);
                    Assembly res;

                    if (TryLoadAssemblyFromFileWithPath(fullName, out res)) return res;
                    if (TryLoadAssemblyFromFileWithPath(fullName + ".EXE", out res)) return res;
                    if (TryLoadAssemblyFromFileWithPath(fullName + ".DLL", out res)) return res;
                }
            }

            return null;
        }

        public Assembly LoadAssemblyByPartialName(string name) {
            if (name == null) throw Ops.TypeError("LoadAssemblyByPartialName: arg 1 must be a string");
#pragma warning disable 618
            return Assembly.LoadWithPartialName(name);
#pragma warning restore 618
        }
#endif

        public Assembly LoadAssemblyByName(string name) {
            if (name == null) throw Ops.TypeError("LoadAssemblyByName: arg 1 must be a string");
            return ScriptDomainManager.CurrentManager.PAL.LoadAssembly(name);
        }

        public Type GetClrType(Type type) {
            return type;
        }

        public DynamicType GetPythonType(Type t) {
            return Ops.GetDynamicTypeFromType(t);
        }

        [Documentation("Loads a module from a dynamic language")]
        public ScriptModule Use(string name) {
            ScriptModule res = ScriptDomainManager.CurrentManager.UseModule(name);
            if (res == null) throw Ops.ValueError("couldn't find module {0} to use", name);

            SystemState.Instance.modules[name] = res;

            return res;
        }

        [Documentation("Loads a module from a dynamic language, given a path and language name")]
        public ScriptModule Use(string path, string language) {
            ScriptModule res = ScriptDomainManager.CurrentManager.UseModule(path, language);
            if (res == null) throw Ops.ValueError("couldn't load module at path '{0}' in language '{1}'", path, language);

            SystemState.Instance.modules[res.ModuleName] = res;

            return res;
        }

        [Documentation("Sets a function that executes commands on behalf of an engine. Returns previous dispatcher.")]
        public CommandDispatcher SetCommandDispatcher(CommandDispatcher dispatcher) {
            return ScriptDomainManager.CurrentManager.SetCommandDispatcher(dispatcher);
        }

        #endregion

        public static object Reference = Ops.GetDynamicTypeFromType(typeof(Reference<>));

        #region Private implementation methods

#if !SILVERLIGHT // AssemblyResolve, files, path
        bool _hookedAssemblyResolve = false;

        void HookAssemblyResolve() {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            _hookedAssemblyResolve = true;
        }

        void UnhookAssemblyResolve() {
            if (_hookedAssemblyResolve)
                AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            AssemblyName an = new AssemblyName(args.Name);
            return LoadAssemblyFromFile(an.Name);
        }
#endif

        private void AddReference(object reference) {
            Assembly asmRef = reference as Assembly;
            if (asmRef != null) {
                AddReference(asmRef);
                return;
            }

            string strRef = reference as string;
            if (strRef != null) {
                AddReference(strRef);
                return;
            }

            throw Ops.TypeError("invalid assembly type. expected string or Assembly, got {0}", Ops.GetPythonTypeName(reference));
        }

        private void AddReference(Assembly assembly) {
            ReferencesTuple referencesTuple = References as ReferencesTuple;
            if (referencesTuple == null) {
                throw Ops.TypeError("cannot add reference");
            }

            // Load the assembly into IronPython
            if (Ops.TopPackage.LoadAssembly(assembly)) {
                // Add it to the references tuple if we
                // loaded a new assembly.
                References = referencesTuple.AddSequence(Tuple.MakeTuple(assembly));
            }
        }

        private void AddReference(string name) {
            if (name == null) throw Ops.TypeError("Expected string, got NoneType");

            Assembly asm = null;

            try {
                asm = LoadAssemblyByName(name);
            } catch { }

            // note we don't explicit call to get the file version
            // here because the assembly resolve event will do it for us.

#if !SILVERLIGHT // files, paths
            if (asm == null) {
                asm = LoadAssemblyByPartialName(name);
            }
#endif

            if (asm == null) {
                throw Ops.IOError("Could not add reference to assembly {0}", name);
            }
            AddReference(asm);
        }

        private void AddReferenceToFile(string file) {
            if (file == null) throw Ops.TypeError("Expected string, got NoneType");

#if SILVERLIGHT
            Assembly asm = ScriptDomainManager.CurrentManager.PAL.LoadAssemblyFromPath(file);
#else
            Assembly asm = LoadAssemblyFromFile(file);
#endif
            if (asm == null) {
                throw Ops.IOError("Could not add reference to assembly {0}", file);
            }

            AddReference(asm);
        }

#if !SILVERLIGHT // files, paths
        private void AddReferenceToFileAndPath(string file) {
            if (file == null) throw Ops.TypeError("Expected string, got NoneType");

            // update our path w/ the path of this file...
            string path = System.IO.Path.GetDirectoryName(file);
            List list = SystemState.Instance.path;
            if (list == null) throw Ops.TypeError("cannot update path, it is not a list");

            list.Add(path);

            // then fall through to the normal loading process
            AddReferenceToFile(System.IO.Path.GetFileName(file));
        }

        private void AddReferenceByPartialName(string name) {
            if (name == null) throw Ops.TypeError("Expected string, got NoneType");

            Assembly asm = LoadAssemblyByPartialName(name);
            if (asm == null) {
                throw Ops.IOError("Could not add reference to assembly {0}", name);
            }

            AddReference(asm);
        }

        private bool TryLoadAssemblyFromFileWithPath(string path, out Assembly res) {
            if (File.Exists(path)) {
                try {
                    res = LoadAssemblyFromFileWithPath(path);
                    if (res != null) return true;
                } catch { }
            }
            res = null;
            return false;
        }
#endif
        private void AddReferenceByName(string name) {
            if (name == null) throw Ops.TypeError("Expected string, got NoneType");

            Assembly asm = LoadAssemblyByName(name);

            if (asm == null) {
                throw Ops.IOError("Could not add reference to assembly {0}", name);
            }

            AddReference(asm);
        }

        #endregion



        #region IDisposable Members

        public void Dispose() {
#if !SILVERLIGHT // AssemblyResolve
            UnhookAssemblyResolve();
#endif
        }

        #endregion
    }

    public class ArgChecker : ICallableWithCodeContext {
        private object[] expected;

        public ArgChecker(object[] prms) {
            expected = prms;
        }

        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, params object[] args) {
            // expect only to receive the function we'll call here.
            if (args.Length != 1) throw Ops.TypeError("bad arg count");

            return new RuntimeArgChecker(args[0], expected);
        }

        #endregion
    }

    public class RuntimeArgChecker : DynamicTypeSlot, ICallableWithCodeContext {
        private object[] expected;
        private object func;
        private object inst;

        public RuntimeArgChecker(object function, object[] expectedArgs) {
            expected = expectedArgs;
            func = function;
        }

        public RuntimeArgChecker(object instance, object function, object[] expectedArgs)
            : this(function, expectedArgs) {
            inst = instance;
        }

        private void ValidateArgs(object[] args) {
            int start = 0;

            if (inst != null) {
                start = 1;
            }


            // no need to validate self... the method should handle it.
            for (int i = start; i < args.Length + start; i++) {
                DynamicType dt = Ops.GetDynamicType(args[i - start]);

                DynamicType expct = expected[i] as DynamicType;
                if(expct == null) expct = ((OldClass)expected[i]).TypeObject;
                if (dt != expected[i] && !dt.IsSubclassOf(expct)) {
                    throw Ops.AssertionError("argument {0} has bad value (got {1}, expected {2})", i, dt, expected[i]);
                }
            }
        }

        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, params object[] args) {
            ValidateArgs(args);

            if (inst != null) {
                object[] realArgs = new object[args.Length + 1];
                realArgs[0] = inst;
                Array.Copy(args, 0, realArgs, 1, args.Length);
                return Ops.CallWithContext(context, func, realArgs);
            } else {
                return Ops.CallWithContext(context, func, args);
            }
        }

        #endregion

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = new RuntimeArgChecker(instance, func, expected);
            return true;
        }
    }

    public class ReturnChecker : ICallableWithCodeContext {
        public object retType;

        public ReturnChecker(object returnType) {
            retType = returnType;
        }

        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, params object[] args) {
            // expect only to receive the function we'll call here.
            if (args.Length != 1) throw Ops.TypeError("bad arg count");

            return new RuntimeReturnChecker(args[0], retType);
        }

        #endregion
    }

    public class RuntimeReturnChecker : DynamicTypeSlot, ICallableWithCodeContext {
        private object retType;
        private object func;
        private object inst;

        public RuntimeReturnChecker(object function, object expectedReturn) {
            retType = expectedReturn;
            func = function;
        }

        public RuntimeReturnChecker(object instance, object function, object expectedReturn)
            : this(function, expectedReturn) {
            inst = instance;
        }

        private void ValidateReturn(object ret) {
            // we return void...
            if (ret == null && retType == null) return;

            DynamicType dt = Ops.GetDynamicType(ret);
            if (dt != retType) {
                DynamicType expct = retType as DynamicType;
                if (expct == null) expct = ((OldClass)retType).TypeObject;

                if (!dt.IsSubclassOf(expct))
                    throw Ops.AssertionError("bad return value returned (expected {0}, got {1})", retType, dt);
            }
        }

        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, params object[] args) {
            object ret;
            if (inst != null) {
                object[] realArgs = new object[args.Length + 1];
                realArgs[0] = inst;
                Array.Copy(args, 0, realArgs, 1, args.Length);
                ret = Ops.CallWithContext(context, func, realArgs);
            } else {
                ret = Ops.CallWithContext(context, func, args);
            }
            ValidateReturn(ret);
            return ret;
        }

        #endregion

        #region IDescriptor Members

        public object GetAttribute(object instance, object owner) {
            return new RuntimeReturnChecker(instance, func, retType);
        }

        #endregion

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = GetAttribute(instance, owner);
            return true;
        }
    }

    /// <summary>
    /// Special subclass of Tuple to provide improved formatting when outputting the Tuple.
    /// </summary>
    public class ReferencesTuple : Tuple {

        public ReferencesTuple()
            : base(Tuple.MakeTuple()) {
        }

        public ReferencesTuple(object data)
            : base(data) {
        }

        //[PythonName("__add__")]
        public ReferencesTuple AddSequence(Tuple o) {
            List newData = new List(this);
            foreach (object item in o) {
                newData.AddNoLock(item);
            }
            return new ReferencesTuple(newData);
        }

        public override string ToString() {
            StringBuilder buf = new StringBuilder();
            buf.Append("(");
            for (int i = 0; i < this.GetLength(); i++) {
                if (i != 0) buf.AppendLine(",");
                buf.Append('<');
                buf.Append(this[i].ToString());
                buf.Append('>');
            }
            buf.AppendLine(")");
            return buf.ToString();
        }
    }
}
