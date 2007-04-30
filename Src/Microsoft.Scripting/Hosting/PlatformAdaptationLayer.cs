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

        public PlatformAdaptationLayer() {
        }

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

