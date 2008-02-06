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

#if !SILVERLIGHT // ComObject

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.ComDispatch {
    public class BoundDispEvent {
        object _rcw;
        Guid _sourceIid;
        int _dispid;

        public BoundDispEvent(object rcw, Guid sourceIid, int dispid) {
            this._rcw = rcw;
            this._sourceIid = sourceIid;
            this._dispid = dispid;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates"), SpecialName] // TODO: fix
        public object op_AdditionAssignment(CodeContext context, object func) {
            return InPlaceAdd(context, func);
        }

        [SpecialName]
        public object InPlaceAdd(CodeContext context, object func) {
            ComEventSink comEventSink = ComEventSink.FromRCW(this._rcw, this._sourceIid, true);

            comEventSink.AddHandler(this._dispid, context, func);
            return this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context"), SpecialName]
        public object InPlaceSubtract(CodeContext context, object func) {
            ComEventSink comEventSink = ComEventSink.FromRCW(this._rcw, this._sourceIid, false);
            if (comEventSink == null) {
                throw new System.InvalidOperationException("removing an event handler that is not registered");
            }

            comEventSink.RemoveHandler(this._dispid, func);
            return this;
        }
    }
}

#endif