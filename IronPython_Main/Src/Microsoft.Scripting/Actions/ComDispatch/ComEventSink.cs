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
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Reflection;
using System.Globalization;
using Microsoft.Scripting;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace Microsoft.Scripting.Actions.ComDispatch {
    /// <summary>
    /// This class implements an event sink for a particular RCW.
    /// Unlike the implementation of events in TlbImp'd assemblies,
    /// we will create only one event sink per RCW (theoretically RCW might have
    /// several ComEventSink evenk sinks - but all these implement different source intefaces).
    /// Each ComEventSink contains a list of ComEventSinkMethod objects - which represent
    /// a single method on the source interface an a multicast delegate to redirect 
    /// the calls. Notice that we are chaining multicast delegates so that same 
    /// ComEventSinkMedhod can invoke multiple event handlers).
    /// 
    /// ComEventSink implements an IDisposable pattern to Unadvise from the connection point.
    /// Typically, when RCW is finalized the corresponding Dispose will be triggered by 
    /// ComEventSinksContainer finalizer. Notice that lifetime of ComEventSinksContainer
    /// is bound to the lifetime of the RCW. 
    /// </summary>
    public class ComEventSink : MarshalByRefObject, IReflect, IDisposable {
        #region private fields

        private Guid _sourceIid;
        private ComTypes.IConnectionPoint _connectionPoint;
        private int _adviseCookie;
        private List<ComEventSinkMethod> _comEventSinkMethods;
        private object _lockObject = new object(); // We cannot lock on ComEventSink since it causes a DoNotLockOnObjectsWithWeakIdentity warning

        #endregion

        #region private classes

        /// <summary>
        /// Contains a methods DISPID (in a string formatted of "[DISPID=N]"
        /// and a chained list of delegates to invoke
        /// </summary>
        private class ComEventSinkMethod {
            public string _name;
            public Delegate _target;
        }

        delegate object ComEventCallHandler(object[] args);

        private class ComEventCallContext {
            public CodeContext _context;
            public object _func;

            public ComEventCallContext(CodeContext context, object func) {
                _context = context;
                _func = func;
            }

            public object Call(object[] args) {
                return _context.LanguageContext.Call(_context, _func, args);
            }
        }

        #endregion

        private const int CONNECT_E_NOCONNECTION = unchecked((int)0x80040200);

        #region ctor

        private ComEventSink(object rcw, Guid sourceIid) {
            Initialize(rcw, sourceIid);
        }

        #endregion

        private void Initialize(object rcw, Guid sourceIid) {
            this._sourceIid = sourceIid;
            this._adviseCookie = -1;

            Debug.Assert(this._connectionPoint == null, "re-initializing event sink w/o unadvising from connection point");

            ComTypes.IConnectionPointContainer cpc = rcw as ComTypes.IConnectionPointContainer;
            if (cpc == null)
                throw new ArgumentException("COM object does not support events");

            cpc.FindConnectionPoint(ref _sourceIid, out _connectionPoint);
            if (_connectionPoint == null)
                throw new ArgumentException("COM object does not support specified source interface");

            // Read the comments for ComEventSinkProxy about why we need it
            ComEventSinkProxy proxy = new ComEventSinkProxy(this, _sourceIid);
            _connectionPoint.Advise(proxy.GetTransparentProxy(), out _adviseCookie);
        }

        #region static methods

        public static ComEventSink FromRCW(object rcw, Guid sourceIid, bool createIfNotFound) {
            List<ComEventSink> comEventSinks = ComEventSinksContainer.FromRCW(rcw, createIfNotFound);
            ComEventSink comEventSink = null;

            lock (comEventSinks) {

                foreach (ComEventSink sink in comEventSinks) {
                    if (sink._sourceIid == sourceIid) {
                        comEventSink = sink;
                        break;
                    } else if (sink._sourceIid == Guid.Empty) {
                        // we found a ComEventSink object that 
                        // was previously disposed. Now we will reuse it.
                        sink.Initialize(rcw, sourceIid);
                        comEventSink = sink;
                    }
                }

                if (comEventSink == null && createIfNotFound == true) {
                    comEventSink = new ComEventSink(rcw, sourceIid);
                    comEventSinks.Add(comEventSink);
                }
            }

            return comEventSink;
        }

        #endregion

        public void AddHandler(int dispid, CodeContext context, object func) {

            string name = String.Format(CultureInfo.InvariantCulture, "[DISPID={0}]", dispid);
            ComEventCallContext callContext = new ComEventCallContext(context, func);
            Delegate handler = Delegate.CreateDelegate(typeof(ComEventCallHandler), callContext, typeof(ComEventCallContext).GetMethod("Call"));

            lock (_lockObject) {

                ComEventSinkMethod sinkMethod;
                sinkMethod = FindSinkMethod(name);

                if (sinkMethod == null) {
                    if (_comEventSinkMethods == null) {
                        _comEventSinkMethods = new List<ComEventSinkMethod>();
                    }

                    sinkMethod = new ComEventSinkMethod();
                    sinkMethod._name = name;
                    _comEventSinkMethods.Add(sinkMethod);
                }

                sinkMethod._target = Delegate.Combine(sinkMethod._target, handler);
            }
        }

        public void RemoveHandler(int dispid, object func) {

            string name = String.Format(CultureInfo.InvariantCulture, "[DISPID={0}]", dispid);

            lock (_lockObject) {

                ComEventSinkMethod sinkEntry = FindSinkMethod(name);
                if (sinkEntry == null)
                    throw new InvalidOperationException("removing not registered handler");

                // Remove the delegate from multicast delegate chain.
                // We will need to find the delegate that corresponds
                // to the func handler we want to remove. This will be
                // easy since we Target property of the delegate object
                // is a ComEventCallContext object.
                Delegate[] delegates = sinkEntry._target.GetInvocationList();
                foreach (Delegate d in delegates) {
                    ComEventCallContext callContext = d.Target as ComEventCallContext;
                    if (callContext != null && callContext._func.Equals(func)) {
                        sinkEntry._target = Delegate.Remove(sinkEntry._target, d);
                        break;
                    }
                }

                // If the delegates chain is empty - we can remove 
                // corresponding ComEvenSinkEntry
                if (sinkEntry._target == null)
                    this._comEventSinkMethods.Remove(sinkEntry);

                // We can Unadvise from the ConnectionPoint if no more sink entries
                // are registered for this interface 
                //(calling Dispose will call IConnectionPoint.Unadvise).
                if (this._comEventSinkMethods.Count == 0) {
                    // notice that we do not remove 
                    // ComEventSinkEntry from the list, we will re-use this data structure
                    // if a new handler needs to be attached.
                    this.Dispose();
                }
            }
        }

        public object ExecuteHandler(string name, object[] args) {
            ComEventSinkMethod site;
            site = FindSinkMethod(name);

            if (site != null && site._target != null) {
                // TODO: currently we only pass parameters by value
                // TODO: however modifiers might specify that some params
                // TODO: are by ref. Should we wrap those into IStrongBox-like objects?
                return site._target.DynamicInvoke(new object[] { args });
            }

            return null;
        }

        #region IReflect

        #region Unimplemented members

        public FieldInfo GetField(string name, BindingFlags bindingFlags) {
            return null;
        }

        public FieldInfo[] GetFields(BindingFlags bindingFlags) {
            return new FieldInfo[0];
        }

        public MemberInfo[] GetMember(string name, BindingFlags bindingFlags) {
            return new MemberInfo[0];
        }

        public MemberInfo[] GetMembers(BindingFlags bindingFlags) {
            return new MemberInfo[0];
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingFlags) {
            return null;
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingFlags, Binder binder, Type[] types, ParameterModifier[] modifiers) {
            return null;
        }

        public MethodInfo[] GetMethods(BindingFlags bindingFlags) {
            return new MethodInfo[0];
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingFlags, Binder binder, Type returnType, Type[] argumentTypes, ParameterModifier[] modifiers) {
            return null;
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingFlags) {
            return null;
        }

        public PropertyInfo[] GetProperties(BindingFlags bindingFlags) {
            return new PropertyInfo[0];
        }

        #endregion

        public Type UnderlyingSystemType {
            get {
                return typeof(object);
            }
        }

        public object InvokeMember(
            string name, 
            BindingFlags bindingFlags, 
            Binder binder, 
            object target, 
            object[] args, 
            ParameterModifier[] modifiers, 
            CultureInfo culture, 
            string[] namedParameters) {

            return this.ExecuteHandler(name, args);
        }

        #endregion

        #region IDisposable

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        ~ComEventSink() {
            this.Dispose(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
        private void Dispose(bool disposing) {
            if (_connectionPoint == null) {
                return;
            }

            if (_adviseCookie == -1) {
                return;
            }

            try {
                _connectionPoint.Unadvise(_adviseCookie);

                // _connectionPoint has entered the CLR in the constructor
                // for this object and hence its ref counter has been increased
                // by us. We have not exposed it to other components and
                // hence it is safe to call RCO on it w/o worrying about
                // killing the RCW for other objects that link to it.
                Marshal.ReleaseComObject(_connectionPoint);
            } catch (Exception ex) {
                // if something has gone wrong, and the object is no longer attached to the CLR,
                // the Unadvise is going to throw.  In this case, since we're going away anyway,
                // we'll ignore the failure and quietly go on our merry way.
                COMException exCOM = ex as COMException;
                if (exCOM != null && exCOM.ErrorCode == CONNECT_E_NOCONNECTION) {
                    Debug.Assert(false, "IConnectionPoint::Unadvise returned CONNECT_E_NOCONNECTION.");
                    throw;
                }
            } finally {
                _connectionPoint = null;
                _adviseCookie = -1;
                _sourceIid = Guid.Empty;
            }
        }

        private ComEventSinkMethod FindSinkMethod(string name) {
            if (_comEventSinkMethods == null)
                return null;

            ComEventSinkMethod site;
            site = _comEventSinkMethods.Find(
                delegate(ComEventSinkMethod element) { return element._name == name; });

            return site;
        }
    }
}

#endif