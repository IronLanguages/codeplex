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

using System; using Microsoft;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;


using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.ComDispatch {

    public class BoundDispEvent {

        private object _rcw;
        private Guid _sourceIid;
        private int _dispid;

        public BoundDispEvent(object rcw, Guid sourceIid, int dispid) {
            _rcw = rcw;
            _sourceIid = sourceIid;
            _dispid = dispid;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates"), SpecialName] // TODO: fix
        public object op_AdditionAssignment(CodeContext context, object func) {
            return InPlaceAdd(context, func);
        }

        [SpecialName]
        public object InPlaceAdd(CodeContext context, object func) {
            ComEventSink comEventSink = ComEventSink.FromRuntimeCallableWrapper(_rcw, _sourceIid, true);

            comEventSink.AddHandler(_dispid, context, func);
            return this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context"), SpecialName]
        public object InPlaceSubtract(CodeContext context, object func) {
            ComEventSink comEventSink = ComEventSink.FromRuntimeCallableWrapper(_rcw, _sourceIid, false);
            if (comEventSink == null) {
                throw new System.InvalidOperationException("removing an event handler that is not registered");
            }

            comEventSink.RemoveHandler(_dispid, func);
            return this;
        }
    }
}

#endif
