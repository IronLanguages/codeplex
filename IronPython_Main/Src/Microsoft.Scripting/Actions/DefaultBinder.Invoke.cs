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
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions.Calls;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Linq.Expressions.Expression;
    
    public partial class DefaultBinder : ActionBinder {

        /// <summary>
        /// Provides default binding for performing a call on the specified meta objects.
        /// </summary>
        /// <param name="signature">The signature describing the call</param>
        /// <param name="target">The object to be called</param>
        /// <param name="args">
        /// Additional meta objects are the parameters for the call as specified by the CallSignature in the CallAction.
        /// </param>
        /// <returns>A MetaObject representing the call or the error.</returns>
        public MetaObject Call(CallSignature signature, MetaObject target, params MetaObject[] args) {
            return Call(signature, new ParameterBinder(this), target, args);
        }

        /// <summary>
        /// Provides default binding for performing a call on the specified meta objects.
        /// </summary>
        /// <param name="signature">The signature describing the call</param>
        /// <param name="target">The meta object to be called.</param>
        /// <param name="args">
        /// Additional meta objects are the parameters for the call as specified by the CallSignature in the CallAction.
        /// </param>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <returns>A MetaObject representing the call or the error.</returns>
        public MetaObject Call(CallSignature signature, ParameterBinder parameterBinder, MetaObject target, params MetaObject[] args) {
            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.RequiresNotNull(parameterBinder, "parameterBinder");

            TargetInfo targetInfo = GetTargetInfo(signature, target, args);

            if (targetInfo != null) {
                // we're calling a well-known MethodBase
                return MakeMetaMethodCall(signature, parameterBinder, targetInfo);
            } else {
                // we can't call this object
                return MakeCannotCallRule(target, target.LimitType);
            }
        }

        #region Method Call Rule

        private MetaObject MakeMetaMethodCall(CallSignature signature, ParameterBinder parameterBinder, TargetInfo targetInfo) {
            Restrictions restrictions = Restrictions.Combine(targetInfo.Arguments).Merge(targetInfo.Restrictions);
            if (targetInfo.Instance != null) {
                restrictions = targetInfo.Instance.Restrictions.Merge(restrictions);
            }

            if (targetInfo.Instance != null) {
                return CallInstanceMethod(
                    parameterBinder,
                    targetInfo.Targets,
                    targetInfo.Instance,
                    targetInfo.Arguments,
                    signature,
                    restrictions
                );
            }

            return CallMethod(
                parameterBinder,
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
        private TargetInfo GetTargetInfo(CallSignature signature, MetaObject target, MetaObject[] args) {
            Debug.Assert(target.HasValue);
            object objTarget = target.Value;

            return
                TryGetDelegateTargets(target, args, objTarget as Delegate) ??
                TryGetMemberGroupTargets(target, args, objTarget as MemberGroup) ??
                TryGetMethodGroupTargets(target, args, objTarget as MethodGroup) ??
                TryGetBoundMemberTargets(target, args, objTarget as BoundMemberTracker) ??
                TryGetOperatorTargets(target, args, target, signature);
        }

        /// <summary>
        /// Binds to the methods in a method group.
        /// </summary>
        private static TargetInfo TryGetMethodGroupTargets(MetaObject target, MetaObject[] args, MethodGroup mthgrp) {
            if (mthgrp != null) {
                List<MethodBase> foundTargets = new List<MethodBase>();

                foreach (MethodTracker mt in mthgrp.Methods) {
                    foundTargets.Add(mt.Method);
                }

                return new TargetInfo(null, ArrayUtils.Insert(target, args), Restrictions.GetInstanceRestriction(target.Expression, mthgrp), foundTargets.ToArray());
            }
            return null;
        }

        /// <summary>
        /// Binds to the methods in a member group.  
        /// 
        /// TODO: We should really only have either MemberGroup or MethodGroup, not both.
        /// </summary>
        private static TargetInfo TryGetMemberGroupTargets(MetaObject target, MetaObject[] args, MemberGroup mg) {
            if (mg != null) {
                MethodBase[] targets;
                List<MethodInfo> foundTargets = new List<MethodInfo>();
                foreach (MemberTracker mt in mg) {
                    if (mt.MemberType == TrackerTypes.Method) {
                        foundTargets.Add(((MethodTracker)mt).Method);
                    }
                }
                targets = foundTargets.ToArray();
                return new TargetInfo(null, ArrayUtils.Insert(target, args), targets);
            }
            return null;
        }

        /// <summary>
        /// Binds to the BoundMemberTracker and uses the instance in the tracker and restricts
        /// based upon the object instance type.
        /// </summary>
        private TargetInfo TryGetBoundMemberTargets(MetaObject self, MetaObject[] args, BoundMemberTracker bmt) {
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
                Restrictions restrictions = Restrictions.GetExpressionRestriction(
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

                return new TargetInfo(instance, args, restrictions, targets);
            }
            return null;
        }

        /// <summary>
        /// Binds to the Invoke method on a delegate if this is a delegate type.
        /// </summary>
        private static TargetInfo TryGetDelegateTargets(MetaObject target, MetaObject[] args, Delegate d) {
            if (d != null) {
                return new TargetInfo(target, args, d.GetType().GetMethod("Invoke"));
            }
            return null;
        }

        /// <summary>
        /// Attempts to bind to an operator Call method.
        /// </summary>
        private TargetInfo TryGetOperatorTargets(MetaObject self, MetaObject[] args, object target, CallSignature signature) {
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
                return new TargetInfo(null, ArrayUtils.Insert(self, args), targets);
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
                self.Restrictions.Merge(Restrictions.GetTypeRestriction(self.Expression, type))
            );
        }


        #endregion


        /// <summary>
        /// Encapsulates information about the target of the call.  This includes an implicit instance for the call,
        /// the methods that we'll be calling as well as any restrictions required to perform the call.
        /// </summary>
        class TargetInfo {
            public readonly MetaObject Instance;
            public readonly MetaObject[] Arguments;
            public readonly MethodBase[] Targets;
            public readonly Restrictions Restrictions;

            public TargetInfo(MetaObject instance, MetaObject[] arguments, params MethodBase[] args) :
                this(instance, arguments, Restrictions.Empty, args) {
            }

            public TargetInfo(MetaObject instance, MetaObject[] arguments, Restrictions restrictions, params MethodBase[] targets) {
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
