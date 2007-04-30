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
#if !SILVERLIGHT // files

using System;
using System.Collections.Generic;
using System.Text;

namespace IronPython.Hosting {
    public class ResourceFile {
        private string name;
        private string file;
        private bool publicResource;

        public string Name {
            get { return name; }
            set { name = value; }
        }

        public string File {
            get { return file; }
            set { file = value; }
        }

        public bool PublicResource {
            get { return publicResource; }
            set { publicResource = value; }
        }

        public ResourceFile(string name, string file)
            : this(name, file, true) {
        }

        public ResourceFile(string name, string file, bool publicResource) {
            this.name = name;
            this.file = file;
            this.publicResource = publicResource;
        }
    }
}

#endif