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
using System.Reflection;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    public partial class DefaultBinder : ActionBinder {

        /// <summary>
        /// Provides default binding for performing a call on the specified meta objects.
        /// </summary>
        /// <param name="signature">The signature describing the call</param>
        /// <param name="args">
        /// The first meta object in the array is the object to be called.
        /// 
        /// Additional meta objects are the parameters for the call as specified by the CallSignature in the CallAction.
        /// </param>
        /// <returns>A MetaObject representing the call or the error.</returns>
        public MetaObject/*!*/ Call(CallSignature/*!*/ signature, params MetaObject/*!*/[]/*!*/ args) {            
            return Call(signature, Ast.Null(typeof(CodeContext)), args);
        }

        /// <summary>
        /// Provides default binding for performing a call on the specified meta objects.
        /// </summary>
        /// <param name="signature">The signature describing the call</param>
        /// <param name="args">
        /// The first meta object in the array is the object to be called.
        /// 
        /// Additional meta objects are the parameters for the call as specified by the CallSignature in the CallAction.
        /// </param>
        /// <param name="codeContext">Provides an expression which will be provided as the CodeContext if the target object
        /// receives CodeContext as a parameter.</param>
        /// <returns>A MetaObject representing the call or the error.</returns>
        public MetaObject/*!*/ Call(CallSignature/*!*/ signature, Expression/*!*/ codeContext, params MetaObject/*!*/[]/*!*/ args) {
            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.RequiresNotNull(codeContext, "codeContext");

            TargetInfo targetInfo = GetTargetInfo(signature, args);

            if (targetInfo != null) {
                // we're calling a well-known MethodBase
                return MakeMetaMethodCall(signature, codeContext, targetInfo);
            } else {
                // we can't call this object
                return MakeCannotCallRule(args[0], args[0].LimitType);
            }
        }

        #region Method Call Rule

        private MetaObject MakeMetaMethodCall(CallSignature signature, Expression/*!*/ codeContext, TargetInfo/*!*/ targetInfo) {
            Restrictions restrictions = Restrictions.Combine(targetInfo.Arguments).Merge(targetInfo.Restrictions);
            if (targetInfo.Instance != null) {
                restrictions = targetInfo.Instance.Restrictions.Merge(restrictions);
            }

            if (targetInfo.Instance != null) {
                return CallInstanceMethod(
                    codeContext, 
                    targetInfo.Targets, 
                    targetInfo.Instance, 
                    targetInfo.Arguments, 
                    signature, 
                    restrictions
                );
            }

            return CallMethod(
                codeContext, 
                targetInfo.Targets, 
                targetInfo.Arguments, 
                signature, 
                restrictions);
        }

        #endregion

        #region Target acquisition

        /// <summary>
        /// Gets a TargetInfo object for performing a call on this object.  
        /// 
        /// If this object is a delegate we bind to the Invoke method.
        /// If this object is a MemberGroup or MethodGroup we bind to the methods in the member group.
        /// If this object is a BoundMemberTracker we bind to the methods with the bound instance.
        /// If the underlying type has defined an operator Call method we'll bind to that method.
        /// </summary>
        private TargetInfo GetTargetInfo(CallSignature signature, MetaObject/*!*/[]/*!*/ args) {
            Debug.Assert(args[0].HasValue);
            object target = args[0].Value;

            return
                TryGetDelegateTargets(args, target as Delegate) ??
                TryGetMemberGroupTargets(args, target as MemberGroup) ??
                TryGetMethodGroupTargets(args, target as MethodGroup) ??
                TryGetBoundMemberTargets(args, target as BoundMemberTracker) ??
                TryGetOperatorTargets(args, target, signature);
        }

        /// <summary>
        /// Binds to the methods in a method group.
        /// </summary>
        private static TargetInfo TryGetMethodGroupTargets(MetaObject/*!*/[]/*!*/ args, MethodGroup mthgrp) {
            if (mthgrp != null) {
                List<MethodBase> foundTargets = new List<MethodBase>();

                foreach (MethodTracker mt in mthgrp.Methods) {
                    foundTargets.Add(mt.Method);
                }

                return new TargetInfo(null, args, Restrictions.InstanceRestriction(args[0].Expression, mthgrp), foundTargets.ToArray());
            }
            return null;
        }

        /// <summary>
        /// Binds to the methods in a member group.  
        /// 
        /// TODO: We should really only have either MemberGroup or MethodGroup, not both.
        /// </summary>
        private static TargetInfo TryGetMemberGroupTargets(MetaObject/*!*/[]/*!*/ args, MemberGroup mg) {
            if (mg != null) {
                MethodBase[] targets;
                List<MethodInfo> foundTargets = new List<MethodInfo>();
                foreach (MemberTracker mt in mg) {
                    if (mt.MemberType == TrackerTypes.Method) {
                        foundTargets.Add(((MethodTracker)mt).Method);
                    }
                }
                targets = foundTargets.ToArray();
                return new TargetInfo(null, args, targets);
            }
            return null;
        }

        /// <summary>
        /// Binds to the BoundMemberTracker and uses the instance in the tracker and restricts
        /// based upon the object instance type.
        /// </summary>
        private TargetInfo TryGetBoundMemberTargets(MetaObject/*!*/[]/*!*/ args, BoundMemberTracker bmt) {
            MetaObject self = args[0];
            if (bmt != null) {
                Debug.Assert(bmt.Instance == null); // should be null for trackers that leak to user code

                MethodBase[] targets;

                // instance is pulled from the BoundMemberTracker and restricted to the correct
                // type.
                MetaObject instance = new MetaObject(
                    Ast.Convert(
                        Ast.Property(
                            Ast.Convert(self.Expression, typeof(BoundMemberTracker)),
                            typeof(BoundMemberTracker).GetProperty("ObjectInstance")
                        ),
                        bmt.BoundTo.DeclaringType
                    ),
                    self.Restrictions
                ).Restrict(CompilerHelpers.GetType(bmt.ObjectInstance));

                // we also add a restriction to make sure we're going to the same BoundMemberTracker
                Restrictions restrictions = Restrictions.ExpressionRestriction(
                    Ast.Equal(
                        Ast.Property(
                            Ast.Convert(self.Expression, typeof(BoundMemberTracker)),
                            typeof(BoundMemberTracker).GetProperty("BoundTo")
                        ),
                        Ast.Constant(bmt.BoundTo)
                    )
                );

                switch (bmt.BoundTo.MemberType) {
                    case TrackerTypes.MethodGroup:
                        targets = ((MethodGroup)bmt.BoundTo).GetMethodBases();
                        break;
                    case TrackerTypes.Method:
                        targets = new MethodBase[] { ((MethodTracker)bmt.BoundTo).Method };
                        break;
                    default:
                        throw new InvalidOperationException(); // nothing else binds yet
                }

                return new TargetInfo(instance, ArrayUtils.RemoveFirst(args), restrictions, targets);
            }
            return null;
        }

        /// <summary>
        /// Binds to the Invoke method on a delegate if this is a delegate type.
        /// </summary>
        private static TargetInfo TryGetDelegateTargets(MetaObject/*!*/[]/*!*/ args, Delegate d) {
            if (d != null) {
                return new TargetInfo(args[0], ArrayUtils.RemoveFirst(args), d.GetType().GetMethod("Invoke"));
            }
            return null;
        }

        /// <summary>
        /// Attempts to bind to an operator Call method.
        /// </summary>
        private TargetInfo TryGetOperatorTargets(MetaObject/*!*/[]/*!*/ args, object target, CallSignature signature) {
            MetaObject self = args[0];
            MethodBase[] targets;

            Type targetType = CompilerHelpers.GetType(target);

            MemberGroup callMembers = GetMember(OldCallAction.Make(this, signature), targetType, "Call");
            List<MethodBase> callTargets = new List<MethodBase>();
            foreach (MemberTracker mi in callMembers) {
                if (mi.MemberType == TrackerTypes.Method) {
                    MethodInfo method = ((MethodTracker)mi).Method;
                    if (method.IsSpecialName) {
                        callTargets.Add(method);
                    }
                }
            }

            Expression instance = null;
            if (callTargets.Count > 0) {
                targets = callTargets.ToArray();
                instance = Ast.Convert(self.Expression, CompilerHelpers.GetType(target));
                return new TargetInfo(null, args, targets);
            }

            return null;
        }

        #endregion

        #region Error support

        private MetaObject MakeCannotCallRule(MetaObject self, Type type) {
            return MakeError(
                ErrorInfo.FromException(
                    Ast.New(
                        typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                        Ast.Constant(type.Name + " is not callable")
                    )
                ),
                self.Restrictions.Merge(Restrictions.TypeRestriction(self.Expression, type))
            );
        }


        #endregion


        /// <summary>
        /// Encapsulates information about the target of the call.  This includes an implicit instance for the call,
        /// the methods that we'll be calling as well as any restrictions required to perform the call.
        /// </summary>
        class TargetInfo {
            public readonly MetaObject Instance;
            public readonly MetaObject/*!*/[]/*!*/ Arguments;
            public readonly MethodBase/*!*/[]/*!*/ Targets;
            public readonly Restrictions/*!*/ Restrictions;

            public TargetInfo(MetaObject instance, MetaObject/*!*/[]/*!*/ arguments, params MethodBase/*!*/[]/*!*/args) :
                this(instance, arguments, Restrictions.Empty, args) {
            }

            public TargetInfo(MetaObject instance, MetaObject/*!*/[]/*!*/ arguments, Restrictions restrictions, params MethodBase/*!*/[]/*!*/targets) {
                Assert.NotNullItems(targets);
                Assert.NotNull(restrictions);

                Instance = instance;
                Arguments = arguments;
                Targets = targets;
                Restrictions = restrictions;
            }
        }

    }
}
