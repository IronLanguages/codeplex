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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Cached CallSites.  User types are cached on the PythonType and System types are cached on the
    /// PythonContext to avoid cross-runtime contamination due to the binder on the site.
    /// </summary>
    internal class PythonSiteCache {
        private Dictionary<SymbolId, CallSite<Func<CallSite, object, CodeContext, object>>> _tryGetMemSite;
        private Dictionary<SymbolId, CallSite<Func<CallSite, object, CodeContext, object>>> _tryGetMemSiteShowCls;
        private CallSite<Func<CallSite, CodeContext, object, object>> _dirSite;
        private CallSite<Func<CallSite, CodeContext, object, string, object>> _getAttributeSite;
        private CallSite<Func<CallSite, CodeContext, object, object, string, object, object>> _setAttrSite;
        private CallSite<Func<CallSite, CodeContext, object, object>> _lenSite;

        internal CallSite<Func<CallSite, object, CodeContext, object>> GetTryGetMemberSite(CodeContext context, SymbolId name) {
            CallSite<Func<CallSite, object, CodeContext, object>> site;
            if (PythonOps.IsClsVisible(context)) {
                if (_tryGetMemSiteShowCls == null) {
                    Interlocked.CompareExchange(
                        ref _tryGetMemSiteShowCls,
                        new Dictionary<SymbolId, CallSite<Func<CallSite, object, CodeContext, object>>>(),
                        null
                    );
                }

                lock (_tryGetMemSiteShowCls) {
                    if (!_tryGetMemSiteShowCls.TryGetValue(name, out site)) {
                        _tryGetMemSiteShowCls[name] = site = CallSite<Func<CallSite, object, CodeContext, object>>.Create(
                            PythonContext.GetContext(context).GetMember(
                                SymbolTable.IdToString(name),
                                true
                            )
                        );
                    }
                }
            } else {
                if (_tryGetMemSite == null) {
                    Interlocked.CompareExchange(
                        ref _tryGetMemSite,
                        new Dictionary<SymbolId, CallSite<Func<CallSite, object, CodeContext, object>>>(),
                        null
                    );
                }

                lock (_tryGetMemSite) {
                    if (!_tryGetMemSite.TryGetValue(name, out site)) {
                        _tryGetMemSite[name] = site = CallSite<Func<CallSite, object, CodeContext, object>>.Create(
                            PythonContext.GetContext(context).GetMember(
                                SymbolTable.IdToString(name),
                                true
                            )
                        );
                    }
                }
            }
            return site;
        }

        internal CallSite<Func<CallSite, CodeContext, object, object>> GetDirSite(CodeContext context) {
            if (_dirSite == null) {
                Interlocked.CompareExchange(
                    ref _dirSite,
                    CallSite<Func<CallSite, CodeContext, object, object>>.Create(
                        PythonContext.GetContext(context).InvokeNone
                    ),
                    null);
            }
            return _dirSite;
        }

        internal CallSite<Func<CallSite, CodeContext, object, string, object>> GetGetAttributeSite(CodeContext context) {
            if (_getAttributeSite == null) {
                Interlocked.CompareExchange(
                    ref _getAttributeSite,
                    CallSite<Func<CallSite, CodeContext, object, string, object>>.Create(
                        PythonContext.GetContext(context).InvokeOne
                    ),
                    null
                );
            }
            return _getAttributeSite;
        }

        internal CallSite<Func<CallSite, CodeContext, object, object, string, object, object>> GetSetAttrSite(CodeContext context) {
            if (_setAttrSite == null) {
                Interlocked.CompareExchange(
                    ref _setAttrSite,
                    CallSite<Func<CallSite, CodeContext, object, object, string, object, object>>.Create(
                        PythonContext.GetContext(context).Invoke(
                            new CallSignature(4)
                        )
                    ),
                    null
                );
            }
            return _setAttrSite;
        }

        internal CallSite<Func<CallSite, CodeContext, object, object>> GetLenSite(CodeContext context) {
            if (_lenSite == null) {
                Interlocked.CompareExchange(
                    ref _lenSite,
                    CallSite<Func<CallSite, CodeContext, object, object>>.Create(
                        PythonContext.GetContext(context).InvokeNone
                    ),
                    null
                );
            }
            return _lenSite;
        }
    }
}