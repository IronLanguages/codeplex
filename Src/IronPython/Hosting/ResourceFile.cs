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
#if !SILVERLIGHT // files

using System;
using System.Collections.Generic;
using System.Text;

namespace IronPython.Hosting {
    public class ResourceFile {
        private string _name;
        private string _file;
        private bool _publicResource;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public string File {
            get { return _file; }
            set { _file = value; }
        }

        public bool PublicResource {
            get { return _publicResource; }
            set { _publicResource = value; }
        }

        public ResourceFile(string name, string file)
            : this(name, file, true) {
        }

        public ResourceFile(string name, string file, bool publicResource) {
            this._name = name;
            this._file = file;
            this._publicResource = publicResource;
        }
    }
}

#endif