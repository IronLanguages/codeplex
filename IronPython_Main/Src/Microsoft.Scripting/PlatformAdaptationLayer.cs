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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.IO;
using Microsoft.Scripting;
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {

#if SILVERLIGHT
    public class ExitProcessException : Exception {

        public int ExitCode { get { return exitCode; } }
        int exitCode;

        public ExitProcessException(int exitCode) {
            this.exitCode = exitCode;
        }
    }
#endif

    [Serializable]
    public class PlatformAdaptationLayer {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlatformAdaptationLayer/*!*/ Default = new PlatformAdaptationLayer();

        // TODO: dictionary should be static?
#if SILVERLIGHT
        private Dictionary<string, string> _assemblyFullNames = new Dictionary<string, string>();

        public PlatformAdaptationLayer() {
            LoadSilverlightAssemblyNameMapping();
        }

        // TODO: remove the need for this
        // TODO: does this list need to be complete?
        private void LoadSilverlightAssemblyNameMapping() {
            // non-trasparent assemblies
            AssemblyName clrAssembly = new AssemblyName(typeof(object).Assembly.FullName);
            foreach (string asm in new string[] {
                "mscorlib",
                "System",
                "System.Core",
                "System.Net",
                "System.Runtime.Serialization",
                "System.ServiceModel.Web",
                "System.Windows",
                "System.Windows.Browser",
                "System.Xml",
                "System.Xml.Dtd",
                "System.Xml.Serialization",
                "Microsoft.VisualBasic",
            }) {
                clrAssembly.Name = asm;
                _assemblyFullNames.Add(asm.ToLower(), clrAssembly.FullName);
            }

            // transparent assemblies
            AssemblyName dlrAssembly = new AssemblyName(typeof(PlatformAdaptationLayer).Assembly.FullName);            
            foreach (string asm in new string[] {
                "Microsoft.Scripting",
                "Microsoft.Scripting.Silverlight",
                "IronPython",
                "IronPython.Modules",
                "IronRuby",
                "IronRuby.Libraries",
                "Microsoft.JScript.Compiler",
                "Microsoft.JScript.Runtime",
                "Microsoft.VisualBasic.Compiler",
                "Microsoft.VisualBasic.Scripting",
                "System.ServiceModel",
                "System.ServiceModel.Syndication",
                "System.Xml.Linq",
            }) {
                dlrAssembly.Name = asm;
                _assemblyFullNames.Add(asm.ToLower(), dlrAssembly.FullName);
            }
        }

        protected string LookupFullName(string name) {
            AssemblyName asm = new AssemblyName(name);
            if (asm.Version != null || asm.GetPublicKeyToken() != null || asm.GetPublicKey() != null) {
                return name;
            }
            return _assemblyFullNames.ContainsKey(name.ToLower()) ? _assemblyFullNames[name.ToLower()] : name;
        }
#endif
        #region Assembly Loading

        public virtual Assembly/*!*/ LoadAssembly(string/*!*/ name) {
#if !SILVERLIGHT
            return Assembly.Load(name);
#else
            return Assembly.Load(LookupFullName(name));
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        public virtual Assembly/*!*/ LoadAssemblyFromPath(string/*!*/ path) {
#if !SILVERLIGHT
            return Assembly.LoadFile(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual void TerminateScriptExecution(int exitCode) {
#if !SILVERLIGHT
            System.Environment.Exit(exitCode);
#else
            throw new ExitProcessException(exitCode);
#endif
        }

        #endregion

        #region Virtual File System

        /// <summary>
        /// Normalizes a specified path.
        /// </summary>
        /// <param name="path">Path to normalize.</param>
        /// <returns>Normalized path.</returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is not a valid path.</exception>
        /// <remarks>
        /// Normalization should be idempotent, i.e. NormalizePath(NormalizePath(path)) == NormalizePath(path) for any valid path.
        /// </remarks>
        public virtual string/*!*/ NormalizePath(string/*!*/ path) {
            Contract.RequiresNotNull(path, "path");

            return (path.Length > 0) ? GetFullPath(path) : "";
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

        public virtual Stream/*!*/ OpenInputFileStream(string/*!*/ path, FileMode mode, FileAccess access, FileShare share) {
#if !SILVERLIGHT
            return new FileStream(path, mode, access, share);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Stream/*!*/ OpenInputFileStream(string/*!*/ path, FileMode mode, FileAccess access, FileShare share, int bufferSize) {
#if !SILVERLIGHT
            return new FileStream(path, mode, access, share, bufferSize);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Stream/*!*/ OpenInputFileStream(string/*!*/ path) {
#if !SILVERLIGHT
            return new FileStream(path, FileMode.Open, FileAccess.Read);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Stream/*!*/ OpenOutputFileStream(string/*!*/ path) {
#if !SILVERLIGHT
            return new FileStream(path, FileMode.Create, FileAccess.Write);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual string/*!*/[]/*!*/ GetFiles(string/*!*/ path, string searchPattern) {
#if !SILVERLIGHT
            return Directory.GetFiles(path, searchPattern);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual string/*!*/ GetFullPath(string/*!*/ path) {
#if !SILVERLIGHT
            return Path.GetFullPath(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual string/*!*/ CurrentDirectory {
            get {
#if !SILVERLIGHT
                return Environment.CurrentDirectory;
#else
                throw new NotImplementedException();
#endif
            }
        }

        #endregion
    }
}
