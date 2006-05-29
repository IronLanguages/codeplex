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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;

using IronPython.Runtime;

namespace IronPython.Modules {
    /// <summary>
    /// this class contains objecs and static methods used for
    /// .NET/CLS interop with Python.  
    /// </summary>
    //[PythonType(typeof(PythonModule))]
    public class ClrModule : IDisposable {
        SystemState state;
        public ClrModule(SystemState systemState) {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            state = systemState;
        }

        [ThreadStatic]
        private static Dictionary<string, Assembly> loading;    // list of assemblies currently loading on this thread

        public override string ToString() {
            return "<module 'clr' (built-in)>";
        }

        public object References = new ReferencesTuple();

        public object Path {
            get {
                return List.MakeList("clr.Path has been deprecated and will be removed in the 1.0 Beta 7 release. Please use sys.path instead.");
            }
            set {
                throw Ops.Warning("clr.Path has been deprecated and will be removed in the 1.0 Beta 7 release. Please use sys.path instead.");
            }
        }

        #region Public methods

        public void AddReference(params object[] references) {
            foreach (object reference in references) {
                AddReference(reference);
            }
        }

        public void AddReferenceToFile(params string[] files) {
            foreach (string file in files) {
                AddReferenceToFile(file);
            }
        }

        public void AddReferenceToFileAndPath(params string[] files) {
            foreach (string file in files) {
                AddReferenceToFileAndPath(file);
            }
        }

        public void AddReferenceByName(params string[] names) {
            foreach (string name in names) {
                AddReferenceByName(name);
            }
        }

        public void AddReferenceByPartialName(params string[] names) {
            foreach (string name in names) {
                AddReferenceByPartialName(name);
            }
        }

        public Assembly LoadAssemblyFromFileWithPath(string file) {
            Assembly asm = null;
            try {

                asm = Assembly.LoadFile(file);
            } catch (Exception) {
            }
            return asm;
        }

        public Assembly LoadAssemblyFromFile(string file) {
            if (file.IndexOf(System.IO.Path.DirectorySeparatorChar) != -1) {
                throw Ops.ValueError("filenames must not contain full paths, first add the path to sys.path");
            }

            // check all files in the path...
            IEnumerator ie = Ops.GetEnumerator(state.path);
            while (ie.MoveNext()) {
                Conversion conv;
                string str = Converter.TryConvertToString(ie.Current, out conv);
                if (conv != Conversion.None) {
                    string fullName = System.IO.Path.Combine(str, file);
                    Assembly res;

                    if (File.Exists(fullName)) {
                        res = LoadAssemblyFromFileWithPath(fullName);
                        if (res != null) return res;
                    }

                    string trying = fullName + ".EXE";
                    if (File.Exists(trying)) {
                        res = LoadAssemblyFromFileWithPath(trying);
                        if (res != null) return res;
                    }

                    trying = fullName + ".DLL";
                    if (File.Exists(trying)) {
                        res = LoadAssemblyFromFileWithPath(trying);
                        if (res != null) return res;
                    }                    
                }
            }

            return null;
        }

        public Assembly LoadAssemblyByName(string name) {
            Assembly asm = null;
            try {
                asm = Assembly.Load(name);
            } catch (Exception) {
            }
            return asm;
        }

        public Assembly LoadAssemblyByPartialName(string name) {
            Assembly asm = null;
            try {
#pragma warning disable 618
                asm = Assembly.LoadWithPartialName(name);
#pragma warning restore 618
            } catch (Exception) {
            }
            return asm;
        }

        public Type GetClrType(Type type) {
            return type;
        }

        #endregion


        #region Private implementation methods

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            AssemblyName an = new AssemblyName(args.Name);
            Assembly res;

            // check to see if we're currently adding a reference to this assembly.  If we
            // are we don't want to try loading it again.  This occurs w/ assemblies that have
            // PythonModuleAttribute because when we create an instace of the attribute we also need 
            // to create the corresponding Type associated w/ the module. That triggers another 
            // assembly load because our assembly is not finished loading yet.  In that case we just 
            // return the assembly that is currently being loaded that we know about, but the loader
            // hasn't yet cached.
            if (loading == null) loading = new Dictionary<string,Assembly>();
            if (loading.TryGetValue(an.Name, out res)) return res;
            
            res = LoadAssemblyFromFile(an.Name);

            if (res != null) {
                loading[an.Name] = res;
                try {
                    AddReference(res);
                } finally {
                    loading.Remove(an.Name);
                }
            }

            return res;
        }

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

            throw Ops.TypeError("invalid assembly type.  expected string or Assembly, got {0}", Ops.GetDynamicType(reference).__name__);
        }

        private void AddReference(Assembly assembly) {
            Tuple referencesTuple = References as Tuple;
            if (referencesTuple == null) {
                throw Ops.TypeError("cannot add reference");
            }

            // Load the assembly into IronPython
            if (state.TopPackage.LoadAssembly(state, assembly)) {
                // Add it to the references tuple if we
                // loaded a new assembly.
                References = referencesTuple.AddSequence(Tuple.MakeTuple(assembly));
            }
        }

        private void AddReference(string name) {
            Assembly asm;

            asm = LoadAssemblyByName(name);
            
            // note we don't explicit call to get the file version
            // here because the assembly resolve event will do it for us.

            if (asm == null) {
                asm = LoadAssemblyByPartialName(name);
            }
            if (asm == null) {
                throw Ops.RuntimeError("Could not add reference to assembly {0}", name);
            }
            AddReference(asm);
        }

        private void AddReferenceToFileAndPath(string file) {
            // update our path w/ the path of this file...
            string path = System.IO.Path.GetDirectoryName(file);
            List list = state.path;
            if (list == null) throw Ops.TypeError("cannot update path, it is not a list");

            list.Add(path);

            // then fall through to the normal loading process
            AddReferenceToFile(System.IO.Path.GetFileName(file));
        }

        private void AddReferenceToFile(string file) {
            Assembly asm = LoadAssemblyFromFile(file);
            if (asm == null) {
                throw Ops.RuntimeError("Could not add reference to assembly {0}", file);
            }

            AddReference(asm);
        }

        private void AddReferenceByName(string name) {
            Assembly asm = LoadAssemblyByName(name);

            if (asm == null) {
                throw Ops.RuntimeError("Could not add reference to assembly {0}", name);
            }

            AddReference(asm);
        }

        private void AddReferenceByPartialName(string name) {
            Assembly asm = LoadAssemblyByPartialName(name);
            if (asm == null) {
                throw Ops.RuntimeError("Could not add reference to assembly {0}", name);
            }

            AddReference(asm);
        }

        #endregion


        #region Runtime Type Checking support

        // Supports runtime type checking.  These decorators are currently primarily
        // used by the codedom implementation to carry
        // extra information regarding types at runtime.

        [PythonName("accepts")]
        public object Accepts(params object[] types) {
            return new ArgChecker(types);
        }

        [PythonName("returns")]
        public object Returns(object type) {
            return new ReturnChecker(type);
        }

        [PythonName("Self")]
        public object Self() {
            return null;
        }

        [PythonType("accepts_checker")]
        public class ArgChecker : ICallable {
            private object[] expected;

            public ArgChecker(object[] prms) {
                expected = prms;
            }

            #region ICallable Members

            public object Call(params object[] args) {
                // expect only to receive the function we'll call here.
                if (args.Length != 1) throw Ops.TypeError("bad arg count");

                return new RuntimeChecker(args[0], expected);
            }

            #endregion

           
            [PythonType("checker")]
            public class RuntimeChecker : ICallable, IDescriptor{
                private object[] expected;
                private object func;
                private object inst;

                public RuntimeChecker(object function, object[] expectedArgs) {
                    expected = expectedArgs;
                    func = function;
                }

                public RuntimeChecker(object instance, object function, object[] expectedArgs) 
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
                        if (dt != expected[i] && !dt.IsSubclassOf(expected[i])) {
                            throw Ops.AssertionError("argument {0} has bad value (got {1}, expected {2})", i, dt, expected[i]);
                        }
                    }
                }

                #region ICallable Members

                public object Call(params object[] args) {
                    ValidateArgs(args);

                    if (inst != null) {
                        object[] realArgs = new object[args.Length + 1];
                        realArgs[0] = inst;
                        Array.Copy(args, 0, realArgs, 1, args.Length);
                        return Ops.Call(func, realArgs);
                    } else {
                        return Ops.Call(func, args);
                    }
                }

                #endregion

                #region IDescriptor Members

                public object GetAttribute(object instance, object owner) {
                    return new RuntimeChecker(instance, func, expected);
                }

                #endregion
            }
        }

        [PythonType("return_checker")]
        public class ReturnChecker : ICallable {
            public object retType;

            public ReturnChecker(object returnType) {
                retType = returnType;
            }

            #region ICallable Members

            public object Call(params object[] args) {
                // expect only to receive the function we'll call here.
                if (args.Length != 1) throw Ops.TypeError("bad arg count");

                return new RuntimeChecker(args[0], retType);
            }

            #endregion

            [PythonType("checker")]
            public class RuntimeChecker : ICallable, IDescriptor {
                private object retType;
                private object func;
                private object inst;

                public RuntimeChecker(object function, object expectedReturn) {
                    retType = expectedReturn;
                    func = function;
                }

                public RuntimeChecker(object instance, object function, object expectedReturn)
                    : this(function, expectedReturn) {
                    inst = instance;
                }

                private void ValidateReturn(object ret) {
                    // we return void...
                    if (ret == null && retType == null) return;

                    DynamicType dt = Ops.GetDynamicType(ret);
                    if (dt != retType) {
                        if (!dt.IsSubclassOf(retType))
                            throw Ops.AssertionError("bad return value returned (expected {0}, got {1})", retType, dt);
                    }
                }

                #region ICallable Members

                public object Call(params object[] args) {
                    object ret;
                    if (inst != null) {
                        object[] realArgs = new object[args.Length + 1];
                        realArgs[0] = inst;
                        Array.Copy(args, 0, realArgs, 1, args.Length);
                        ret = Ops.Call(func, realArgs);
                    } else {
                        ret = Ops.Call(func, args);
                    }
                    ValidateReturn(ret);
                    return ret;
                }

                #endregion

                #region IDescriptor Members

                public object GetAttribute(object instance, object owner) {
                    return new RuntimeChecker(instance, func, retType);
                }

                #endregion
            }
            
        }
        #endregion

        #region IDisposable Members

        public void Dispose() {
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        #endregion
    }

        
    /// <summary>
    /// Special subclass of Tuple to provide improved formatting when outputting the Tuple.
    /// </summary>
    [PythonType("references_tuple")]  
    class ReferencesTuple : Tuple {

        public ReferencesTuple()
            : base(Tuple.MakeTuple()) {
        }

        public ReferencesTuple(object data) : base(data) { 
        }

        [PythonName("__add__")]
        public override object AddSequence(object other) {
            Tuple o = other as Tuple;
            if (o == null) throw Ops.TypeError("can only concatenate tuple (not \"{0}\") to tuple", Ops.GetDynamicType(other).__name__);

            List newData = new List(this);
            foreach (object item in o) {
                newData.AddNoLock(item);
            }
            return new ReferencesTuple(newData);
        }

        [PythonName("__str__")]
        public override string ToString() {
            StringBuilder buf = new StringBuilder();
            buf.Append("(");
            for(int i = 0; i<this.GetLength(); i++){
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
