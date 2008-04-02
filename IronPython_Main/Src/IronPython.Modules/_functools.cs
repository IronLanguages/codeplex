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
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

[assembly: PythonModule("_functools", typeof(IronPython.Modules.FunctionTools))]
namespace IronPython.Modules {
    public static class FunctionTools {
        /// <summary>
        /// Returns a new callable object with the provided initial set of arguments
        /// bound to it.  Calling the new function then appends to the additional
        /// user provided arguments.
        /// </summary>
        [PythonSystemType]
        public class partial : IWeakReferenceable {
            private object/*!*/ _function;                                                  // the callable function to dispatch to
            private object[]/*!*/ _args;                                                    // the initially provided arguments
            private IAttributesCollection _keywordArgs;                                     // the initially provided keyword arguments or null
            private DynamicSite<object, object[], IAttributesCollection, object> _dictSite; // the dictionary call site if ever called w/ keyword args
            private DynamicSite<object, object[], object> _splatSite;                       // the position only call site
            private IAttributesCollection _dict;                                            // dictionary for storing extra attributes
            private WeakRefTracker _tracker;                                                // tracker so users can use Python weak references
                
            #region Constructors

            /// <summary>
            /// Creates a new partial object with the provided positional arguments.
            /// </summary>
            public partial(object func, [NotNull]params object[]/*!*/ args)
                : this(func, null, args) {
            }

            /// <summary>
            /// Creates a new partial object with the provided positional and keyword arguments.
            /// </summary>
            public partial(object func, [ParamDictionary]IAttributesCollection keywords, [NotNull]params object[]/*!*/ args) {
                if (!PythonOps.IsCallable(func)) {
                    throw PythonOps.TypeError("the first argument must be callable");
                }

                _function = func;
                _keywordArgs = keywords;
                _args = args;
            }

            #endregion

            #region Public Python API

            /// <summary>
            /// Gets the function which will be called
            /// </summary>
            public object func {
                get {
                    return _function;
                }
            }

            /// <summary>
            /// Gets the initially provided positional arguments.
            /// </summary>
            public object args {
                get {
                    return PythonTuple.MakeTuple(_args);
                }
            }

            /// <summary>
            /// Gets the initially provided keyword arguments or None.
            /// </summary>
            public object keywords {
                get {
                    return _keywordArgs;
                }
            }

            /// <summary>
            /// Gets or sets the dictionary used for storing extra attributes on the partial object.
            /// </summary>
            public IAttributesCollection __dict__ {
                get {
                    return EnsureDict();
                }
                set {
                    _dict = value;
                }
            }

            [PropertyMethod]
            public void Delete__dict__() {
                throw PythonOps.TypeError("partial's dictionary may not be deleted");
            }

            // This exists for subtypes because we don't yet automap DeleteMember onto __delattr__
            public void __delattr__(string name) {
                if (name == "__dict__") Delete__dict__();

                if (_dict != null) {
                    _dict.Remove(SymbolTable.StringToId(name));
                }
            }

            #endregion

            #region Operator methods

            /// <summary>
            /// Calls func with the previously provided arguments and more positional arguments.
            /// </summary>
            [SpecialName]
            public object Call(CodeContext/*!*/ context, params object [] args) {
                if (_keywordArgs == null) {
                    EnsureSplatSite();

                    return _splatSite.Invoke(context, _function, ArrayUtils.AppendRange(_args, args));
                }

                EnsureDictSplatSite();
                return _dictSite.Invoke(context, _function, ArrayUtils.AppendRange(_args, args), _keywordArgs);
            }

            /// <summary>
            /// Calls func with the previously provided arguments and more positional arguments and keyword arguments.
            /// </summary>
            [SpecialName]
            public object Call(CodeContext/*!*/ context, [ParamDictionary]IAttributesCollection dict, params object[] args) {
                EnsureDictSplatSite();

                IAttributesCollection finalDict;
                if (_keywordArgs != null) {
                    PythonDictionary pd = new PythonDictionary();
                    pd.update(context, _keywordArgs);
                    pd.update(context, dict);

                    finalDict = pd;
                } else {
                    finalDict = dict;
                }

                return _dictSite.Invoke(context, _function, ArrayUtils.AppendRange(_args, args), finalDict);
            }
            
            /// <summary>
            /// Operator method to set arbitrary members on the partial object.
            /// </summary>
            [SpecialName, PythonHidden]
            public void SetMemberAfter(CodeContext/*!*/ context, string name, object value) {
                EnsureDict();

                _dict[SymbolTable.StringToId(name)] = value;
            }

            /// <summary>
            /// Operator method to get additional arbitrary members defined on the partial object.
            /// </summary>
            [SpecialName, PythonHidden]
            public object GetBoundMember(CodeContext/*!*/ context, string name) {
                object value;
                if (_dict != null && _dict.TryGetValue(SymbolTable.StringToId(name), out value)) {
                    return value;
                }
                return OperationFailed.Value;
            }

            /// <summary>
            /// Operator method to delete arbitrary members defined in the partial object.
            /// </summary>
            [SpecialName, PythonHidden]
            public bool DeleteMember(CodeContext/*!*/ context, string name) {
                switch (name) {
                    case "__dict__":
                        Delete__dict__();
                        break;
                }

                if (_dict == null) return false;

                return _dict.Remove(SymbolTable.StringToId(name));
            }

            #endregion

            #region Internal implementation details

            private void EnsureSplatSite() {
                if (!_splatSite.IsInitialized) {
                    _splatSite.EnsureInitialized(PythonCalls.MakeSplatAction());
                }
            }

            private void EnsureDictSplatSite() {
                if (!_dictSite.IsInitialized) {
                    _dictSite.EnsureInitialized(PythonCalls.MakeDictSplatAction());
                }
            }

            private IAttributesCollection EnsureDict() {
                if (_dict == null) {
                    _dict = PythonDictionary.MakeSymbolDictionary();
                }
                return _dict;
            }

            #endregion

            #region IWeakReferenceable Members

            WeakRefTracker IWeakReferenceable.GetWeakRef() {
                return _tracker;
            }

            bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
                return Interlocked.CompareExchange<WeakRefTracker>(ref _tracker, value, null) == null;
            }

            void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
                _tracker = value;
            }

            #endregion
        }
    }
}
