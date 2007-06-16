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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using Microsoft.Scripting.Shell;
using System.IO;

namespace Microsoft.Scripting.Hosting {

    public class PlatformAdaptationLayer {

#if SILVERLIGHT
        private Dictionary<string, string> _assemblyFullNames = new Dictionary<string, string>();

        public PlatformAdaptationLayer() {
            LoadSilverlightAssemblyNameMapping();
        }

        // TODO
        private void LoadSilverlightAssemblyNameMapping() {
            _assemblyFullNames.Add("mscorlib", "mscorlib, Version=2.1.0.0, PublicKeyToken=b77a5c561934e089");
            _assemblyFullNames.Add("system", "System, Version=2.1.0.0, PublicKeyToken=b77a5c561934e089");
            _assemblyFullNames.Add("system.core", "System.Core, Version=2.1.0.0, PublicKeyToken=b77a5c561934e089");
            _assemblyFullNames.Add("system.xml.core", "System.Xml.Core, Version=2.1.0.0, PublicKeyToken=b77a5c561934e089");
            _assemblyFullNames.Add("system.silverlight", "System.SilverLight, Version=1.0.0.0, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("agclr", "agclr, Version=0.0.0.0, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("microsoft.scripting", "Microsoft.Scripting, Version=1.0.0.100, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("microsoft.scripting.vestigial", "Microsoft.Scripting.Vestigial, Version=1.0.0.100, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("microsoft.scripting.silverlight", "Microsoft.Scripting.SilverLight, Version=1.0.0.0, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("ironpython", "IronPython, Version=2.0.0.100, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("ironpython.modules", "IronPython.Modules, Version=2.0.0.100, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("ironpythontest", "IronPythonTest, Version=1.0.0.0, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("microsoft.jscript.compiler", "Microsoft.JScript.Compiler, Version=1.0.0.0, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("microsoft.jscript.runtime", "Microsoft.JScript.Runtime, Version=1.0.0.0, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("microsoft.visualbasic.compiler", "Microsoft.VisualBasic.Compiler, Version=1.0.0.0, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("microsoft.visualbasic.scripting", "Microsoft.VisualBasic.Scripting, Version=1.0.0.0, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("microsoft.visualbasic", "Microsoft.VisualBasic, Version=8.1.0.0, PublicKeyToken=b03f5f7f11d50a3a");
            _assemblyFullNames.Add("ruby", "Ruby, Version=1.0.0.0, PublicKeyToken=b03f5f7f11d50a3a");

        }

        protected string LookupFullName(string name) {
            AssemblyName asm = new AssemblyName(name);
            if (asm.Version != null || asm.GetPublicKeyToken() != null || asm.GetPublicKey() != null) {
                return name;
            }
            return _assemblyFullNames.ContainsKey(name.ToLower()) ? _assemblyFullNames[name.ToLower()] : name;
        }
#endif

        public virtual Assembly LoadAssembly(string name) {
#if !SILVERLIGHT
            return Assembly.Load(name);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Assembly LoadAssemblyFromPath(string path) {
#if !SILVERLIGHT
            return Assembly.LoadFile(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual void TerminateScriptExecution(int exitCode) {
#if !SILVERLIGHT
            System.Environment.Exit(exitCode);
#endif
        }

        public virtual bool FileExists(string path) {
#if !SILVERLIGHT
            return File.Exists(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual bool DirectoryExists(string path) {
#if !SILVERLIGHT
            return Directory.Exists(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share) {
#if !SILVERLIGHT
            return new FileStream(path, mode, access, share);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) {
#if !SILVERLIGHT
            return new FileStream(path, mode, access, share, bufferSize);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Stream OpenInputFileStream(string path) {
#if !SILVERLIGHT
            return new FileStream(path, FileMode.Open, FileAccess.Read);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Stream OpenOutputFileStream(string path) {
#if !SILVERLIGHT
            return new FileStream(path, FileMode.Create, FileAccess.Write);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual string[] GetFiles(string path, string searchPattern) {
#if !SILVERLIGHT
            return Directory.GetFiles(path, searchPattern);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual string GetFullPath(string path) {
#if !SILVERLIGHT
            return Path.GetFullPath(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Action<Exception> EventExceptionHandler {
            get { return null; }
        }

    }
}

