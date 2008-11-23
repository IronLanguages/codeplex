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

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Com {
    /// <summary>
    /// This is a helper class for runtime-callable-wrappers of COM instances. We create one instance of this type
    /// for every generic RCW instance.
    /// </summary>
    public abstract class ComObject : IDynamicObject {
        /// <summary>
        /// The runtime-callable wrapper
        /// </summary>
        private readonly object _rcw;

        #region Constructor(s)/Initialization

        protected ComObject(object rcw) {
            Debug.Assert(ComMetaObject.IsComObject(rcw));
            _rcw = rcw;
        }

        #endregion

        #region Public Members

        public object Obj {
            get {
                return _rcw;
            }
        }

        #endregion

        #region Static Members

        private readonly static object _ComObjectInfoKey = new object();

        /// <summary>
        /// This is the factory method to get the ComObject corresponding to an RCW
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public static ComObject ObjectToComObject(object rcw) {
            Debug.Assert(ComMetaObject.IsComObject(rcw));

            // Marshal.Get/SetComObjectData has a LinkDemand for UnmanagedCode which will turn into
            // a full demand. We could avoid this by making this method SecurityCritical
            object data = Marshal.GetComObjectData(rcw, _ComObjectInfoKey);
            if (data != null) {
                return (ComObject)data;
            }

            lock (_ComObjectInfoKey) {
                data = Marshal.GetComObjectData(rcw, _ComObjectInfoKey);
                if (data != null) {
                    return (ComObject)data;
                }

                ComObject comObjectInfo = CreateComObject(rcw);
                if (!Marshal.SetComObjectData(rcw, _ComObjectInfoKey, comObjectInfo)) {
                    throw Error.SetComObjectDataFailed();
                }

                return comObjectInfo;
            }
        }

        private static ComObject CreateComObject(object rcw) {
            IDispatch dispatchObject = rcw as IDispatch;
            if (!DebugOptions.PreferComInteropAssembly && (dispatchObject != null)) {
                // We can do method invocations on IDispatch objects
                return new IDispatchComObject(dispatchObject);
            }

            // First check if we can associate metadata with the COM object
            ComObject comObject = ComObjectWithTypeInfo.CheckClassInfo(rcw);
            if (comObject != null) {
                return comObject;
            }

            if (dispatchObject != null) {
                // We can do method invocations on IDispatch objects
                return new IDispatchComObject(dispatchObject);
            }

            // There is not much we can do in this case
            return new GenericComObject(rcw);
        }

        #endregion

        #region Abstract Members

        public abstract IList<string> MemberNames {
            get;
        }

        public abstract string Documentation {
            get;
        }

        public abstract MetaObject GetMetaObject(Expression parameter);

        #endregion
    }
}

#endif
