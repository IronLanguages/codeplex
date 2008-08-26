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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Actions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {

    /// <summary>
    /// The Action used for Python call sites.  This supports both splatting of position and keyword arguments.
    /// 
    /// When a foreign object is encountered the arguments are expanded into normal position/keyword arguments.
    /// </summary>
    class InvokeBinder : MetaAction, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;
        private readonly CallSignature _signature;

        public InvokeBinder(BinderState/*!*/ binder, CallSignature signature) {
            _state = binder;
            _signature = signature;
        }

        #region MetaAction overrides

        /// <summary>
        /// Python's Invoke is a non-standard action.  Here we first try to bind through a Python
        /// internal interface (IPythonInvokable) which supports CallSigantures.  If that fails
        /// and we have an IDO then we translate to the DLR protocol through a nested dynamic site -
        /// this includes unsplatting any keyword / position arguments.  Finally if it's just a plain
        /// old .NET type we use the default binder which supports CallSignatures.
        /// </summary>
        public override MetaObject/*!*/ Bind(MetaObject/*!*/[]/*!*/ args) {
            Debug.Assert(args[0].LimitType == typeof(CodeContext));

            const int codeContext = 0, callTarget = 1;

            // we don't have CodeContext if an IDO falls back to us when we ask them to produce the Call
            MetaObject cc = args[codeContext];            
            MetaObject[] callargs = ArrayUtils.RemoveFirst(args);
            IPythonInvokable icc = args[callTarget] as IPythonInvokable;

            if (icc != null) {
                // call it and provide the context
                return icc.Invoke(
                    this,
                    cc.Expression,
                    callargs
                );
            } else if (args[callTarget].IsDynamicObject) {
                return InvokeForeignObject(callargs);
            }

            return Fallback(cc.Expression, callargs);
        }

        /// <summary>
        /// Fallback - performs the default binding operation if the object isn't recognized
        /// as being invokable.
        /// </summary>
        internal MetaObject/*!*/ Fallback(Expression codeContext, MetaObject/*!*/[]/*!*/ args) {
            if (args[0].NeedsDeferral) {
                return Defer(args);
            }

            return PythonProtocol.Call(this, args) ??
                Binder.Binder.Create(Signature, codeContext, args) ??
                Binder.Binder.Call(Signature, codeContext, args);
        }

        public override object/*!*/ HashCookie {
            get { return this; }
        }

        #endregion

        #region Object Overrides

        public override int GetHashCode() {
            return _signature.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            InvokeBinder ob = obj as InvokeBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder &&
                _signature == ob._signature;
        }

        public override string ToString() {
            return "Python Invoke " + Signature.ToString();
        }

        #endregion

        #region Public API Surface

        /// <summary>
        /// Gets the CallSignature for this invocation which describes how the MetaObject array
        /// is to be mapped.
        /// </summary>
        public CallSignature Signature {
            get {
                return _signature;
            }
        }

        #endregion

        #region Implementation Details

        /// <summary>
        /// Creates a nested dynamic site which uses the unpacked arguments.
        /// </summary>
        protected MetaObject InvokeForeignObject(MetaObject[] args) {
            // need to unpack any dict / list arguments...
            List<Argument> newArgs;
            List<Expression> metaArgs;
            Expression test;
            Restrictions restrictions;
            TranslateArguments(args, out newArgs, out metaArgs, out test, out restrictions);

            Debug.Assert(metaArgs.Count > 0);

            return BindingHelpers.AddDynamicTestAndDefer(
                this,
                new MetaObject(
                    Expression.Dynamic(
                        new CompatibilityInvokeBinder(_state, newArgs.ToArray()),
                        typeof(object),
                        metaArgs.ToArray()
                    ),
                    restrictions.Merge(Restrictions.TypeRestriction(args[0].Expression, args[0].LimitType))
                ),
                args,
                new ValidationInfo(test, null)
            );
        }

        /// <summary>
        /// Translates our CallSignature into a DLR Argument list and gives the simple MetaObject's which are extracted
        /// from the tuple or dictionary parameters being splatted.
        /// </summary>
        private void TranslateArguments(MetaObject/*!*/[]/*!*/ args, out List<Argument/*!*/>/*!*/ newArgs, out List<Expression/*!*/>/*!*/ metaArgs, out Expression test, out Restrictions restrictions) {
            ArgumentInfo[] argInfo = _signature.GetArgumentInfos();

            newArgs = new List<Argument>();
            metaArgs = new List<Expression>();
            metaArgs.Add(args[0].Expression);
            Expression splatArgTest = null;
            Expression splatKwArgTest = null;
            restrictions = Restrictions.Empty;

            for (int i = 0; i < argInfo.Length; i++) {
                ArgumentInfo ai = argInfo[i];

                switch (ai.Kind) {
                    case ArgumentKind.Dictionary:
                        IAttributesCollection iac = (IAttributesCollection)args[i + 1].Value;
                        List<string> argNames = new List<string>();

                        foreach (KeyValuePair<object, object> kvp in iac) {
                            string key = (string)kvp.Key;
                            newArgs.Add(Expression.NamedArg(key));
                            argNames.Add(key);

                            metaArgs.Add(
                                Expression.Call(
                                    Expression.ConvertHelper(args[i + 1].Expression, typeof(IAttributesCollection)),
                                    typeof(IAttributesCollection).GetMethod("get_Item"),
                                    AstUtils.Constant(SymbolTable.StringToId(key))
                                )
                            );
                        }

                        restrictions = restrictions.Merge(Restrictions.TypeRestriction(args[i + 1].Expression, args[i + 1].LimitType));
                        splatKwArgTest = Expression.Call(
                            typeof(PythonOps).GetMethod("CheckDictionaryMembers"),
                            Expression.ConvertHelper(args[i + 1].Expression, typeof(IAttributesCollection)),
                            Expression.Constant(argNames.ToArray())
                        );
                        break;
                    case ArgumentKind.List:
                        IList<object> splattedArgs = (IList<object>)args[i + 1].Value;
                        splatArgTest = Expression.Equal(
                            Expression.Property(Expression.ConvertHelper(args[i + 1].Expression, args[i + 1].LimitType), typeof(ICollection<object>).GetProperty("Count")),
                            Expression.Constant(splattedArgs.Count)
                        );

                        for (int splattedArg = 0; splattedArg < splattedArgs.Count; splattedArg++) {
                            newArgs.Add(Expression.PositionalArg(splattedArg + i));
                            metaArgs.Add(
                                Expression.Call(
                                    Expression.ConvertHelper(args[i + 1].Expression, typeof(IList<object>)),
                                    typeof(IList<object>).GetMethod("get_Item"),
                                    Expression.Constant(splattedArg)
                                )
                            );
                        }

                        restrictions = restrictions.Merge(Restrictions.TypeRestriction(args[i + 1].Expression, args[i + 1].LimitType));
                        break;
                    case ArgumentKind.Named:
                        newArgs.Add(Expression.NamedArg(SymbolTable.IdToString(ai.Name)));
                        metaArgs.Add(args[i + 1].Expression);
                        break;
                    case ArgumentKind.Simple:
                        newArgs.Add(Expression.PositionalArg(i));
                        metaArgs.Add(args[i + 1].Expression);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            test = splatArgTest;
            if (splatKwArgTest != null) {
                if (test != null) {
                    test = Expression.AndAlso(test, splatKwArgTest);
                } else {
                    test = splatKwArgTest;
                }
            }
        }

        #endregion

        #region IPythonSite Members

        public BinderState Binder {
            get { return _state; }
        }

        #endregion

        #region Helper MetaActions

        /// <summary>
        /// Fallback action for performing a new() on a foreign IDynamicObject.  used
        /// when call falls back.
        /// </summary>
        class CreateFallback : CreateAction {
            private readonly CompatibilityInvokeBinder/*!*/ _fallback;

            public CreateFallback(CompatibilityInvokeBinder/*!*/ realFallback, IEnumerable<Argument/*!*/>/*!*/ arguments)
                : base(arguments) {
                _fallback = realFallback;
            }

            public override MetaObject/*!*/ Fallback(MetaObject/*!*/[]/*!*/ args, MetaObject onBindingError) {
                return _fallback.InvokeFallback(args, BindingHelpers.GetCallSignature(this));
            }

            public override object HashCookie {
                get { return this; }
            }
        }

        /// <summary>
        /// Fallback action for performing an invoke from Python.  We translate the
        /// CallSignature which supports splatting position and keyword args into
        /// their expanded form.
        /// </summary>
        class CompatibilityInvokeBinder : InvokeAction, IPythonSite {
            private readonly BinderState/*!*/ _state;

            public CompatibilityInvokeBinder(BinderState/*!*/ state, params Argument/*!*/[]/*!*/ args)
                : base(args) {
                _state = state;
            }

            public override MetaObject/*!*/ Fallback(MetaObject/*!*/[]/*!*/ args, MetaObject onBindingError) {
                if (args[0].IsDynamicObject) {
                    // try creating an instance...
                    return args[0].Create(
                        new CreateFallback(this, Arguments),
                        args
                    );
                }

                return InvokeFallback(args, BindingHelpers.ArgumentArrayToSignature(Arguments));
            }

            internal MetaObject/*!*/ InvokeFallback(MetaObject/*!*/[]/*!*/ args, CallSignature sig) {                
                return PythonProtocol.Call(this, args) ??
                   Binder.Binder.Create(sig, Expression.Constant(_state.Context), args) ??
                   Binder.Binder.Call(sig, Expression.Constant(_state.Context), args);
            }

            public override int GetHashCode() {
                return base.GetHashCode() ^ _state.Binder.GetHashCode();
            }

            public override bool Equals(object obj) {
                CompatibilityInvokeBinder ob = obj as CompatibilityInvokeBinder;
                if (ob == null) {
                    return false;
                }

                return ob._state.Binder == _state.Binder && base.Equals(obj);
            }

            public override object/*!*/ HashCookie {
                get { return this; }
            }

            #region IPythonSite Members

            public BinderState/*!*/ Binder {
                get { return _state; }
            }

            #endregion
        }

        #endregion

        #region IExpressionSerializable Members

        public virtual Expression CreateExpression() {
            return Expression.Call(
                typeof(PythonOps).GetMethod("MakeInvokeAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Signature.CreateExpression()
            );
        }

        #endregion
    }
}
