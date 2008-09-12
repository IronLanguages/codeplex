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
#if !SILVERLIGHT // ComObject

using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Com {
    // TODO: Can it be made internal?
    public sealed class ComTypeLibInfo : IDynamicObject  {
        private readonly ComTypeLibDesc _typeLibDesc;

        internal ComTypeLibInfo(ComTypeLibDesc typeLibDesc) {
            _typeLibDesc = typeLibDesc;
        }

        public string Name {
            get { return _typeLibDesc.Name; }
        }

        public Guid Guid {
            get { return new Guid(_typeLibDesc.Guid); }
        }

        public short VersionMajor {
            get { return _typeLibDesc.VersionMajor; }
        }

        public short VersionMinor {
            get { return _typeLibDesc.VersionMinor; }
        }

        public ComTypeLibDesc TypeLibDesc {
            get { return _typeLibDesc; }
        }

        public string[] GetMemberNames() {
            return new string[] { this.Name, "Guid", "Name", "VersionMajor", "VersionMinor" };
        }

        #region IDynamicObject Members

        MetaObject IDynamicObject.GetMetaObject(Expression parameter) {
            return new TypeLibInfoMetaObject(parameter, this);
        }

        #endregion
    }
}

#endif
