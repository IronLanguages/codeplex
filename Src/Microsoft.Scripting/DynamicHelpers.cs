/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Microsoft.Scripting {
    public static class DynamicHelpers {
        private static TopReflectedPackage _topPackage;

        /// <summary> Table of dynamicly generated delegates which are shared based upon method signature. </summary>
        private static Publisher<DelegateSignatureInfo, DelegateInfo> _dynamicDelegateCache = new Publisher<DelegateSignatureInfo, DelegateInfo>();

        public static DynamicType GetDynamicTypeFromType(Type type) {
            if (type == null) throw new ArgumentNullException("type");

            PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "TypeLookup " + type.FullName);

            DynamicType ret = DynamicType.GetDynamicType(type);
            if (ret != null) return ret;

            ret = ReflectedTypeBuilder.Build(type);

            return DynamicType.SetDynamicType(type, ret);
        }

        public static StackFrame[] GetStackFrames(Exception e, bool includeNonDynamicFrames) {
            return null;
        }

        public static DynamicStackFrame[] GetDynamicStackFrames(Exception e) {
            List<DynamicStackFrame> frames = Utils.GetDataDictionary(e)[typeof(DynamicStackFrame)] as List<DynamicStackFrame>;

            if (frames == null) {
                // we may have missed a dynamic catch, and our host is looking
                // for the exception...
                frames = RuntimeHelpers._stackFrames;
                Utils.GetDataDictionary(e)[typeof(DynamicStackFrame)] = frames;
                RuntimeHelpers._stackFrames = null;
            }

            if (frames == null) {
                return new DynamicStackFrame[0];
            }

#if !SILVERLIGHT
            frames = new List<DynamicStackFrame>(frames);
            List<DynamicStackFrame> res = new List<DynamicStackFrame>();

            // the list of _stackFraames we build up in RuntimeHelpers can have
            // too many frames if exceptions are thrown from script code and
            // caught outside w/o calling GetDynamicStackFrames.  Therefore we
            // filter down to only script frames which we know are associated
            // w/ the exception here.
            try {
                StackTrace clrFrames = new StackTrace(e);
                int lastFound = 0;
                for (int i = 0; i < clrFrames.FrameCount; i++) {
                    MethodBase method = clrFrames.GetFrame(i).GetMethod();

                    for (int j = lastFound; j < frames.Count; j++) {
                        MethodBase other = frames[j].GetMethod();
                        if (other == null) {
                            // GetMethod() can return null for CodeBlocks executed in interpreted mode
                            continue;
                        }

                        // method info's don't always compare equal, check based
                        // upon name/module/declaring type which will always be a correct
                        // check for dynamic methods.
                        if (method.Module == other.Module &&
                            method.DeclaringType == other.DeclaringType &&
                            method.Name == other.Name) {
                            res.Add(frames[j]);
                            frames.RemoveAt(j);
                            lastFound = j;
                            break;
                        }
                    }
                }
            } catch (MemberAccessException) {
                // can't access new StackTrace(e) due to security
            }
            return res.ToArray();
#else 
            return frames.ToArray();
#endif
        }
        public static DynamicType GetDynamicType(object o) {
            ISuperDynamicObject dt = o as ISuperDynamicObject;
            if (dt != null) return dt.DynamicType;
            
            if (o == null) return DynamicType.NullType;

            return GetDynamicTypeFromType(o.GetType());
        }       

        public static object CallWithContext(CodeContext context, object func, params object[] args) {
            ICallableWithCodeContext icc = func as ICallableWithCodeContext;
            if (icc != null) return icc.Call(context, args);

            return SlowCallWithContext(context, func, args);
        }

        private static object SlowCallWithContext(CodeContext context, object func, object[] args) {
            PerfTrack.NoteEvent(PerfTrack.Categories.OperatorInvoke, new KeyValuePair<Operators, DynamicType>(Operators.Call, GetDynamicType(func)));

            object res;
            if (!GetDynamicType(func).TryInvokeBinaryOperator(context, Operators.Call, func, args, out res))
                throw RuntimeHelpers.SimpleTypeError(String.Format("{0} is not callable", GetDynamicType(func)));

            return res;
        }

        public static TopReflectedPackage TopPackage {
            get {
                if (_topPackage == null)
                    Interlocked.CompareExchange<TopReflectedPackage>(ref _topPackage, new TopReflectedPackage(), null);

                return _topPackage;
            }
        }



        // TODO: remove exceptionHandler param (Silverlight hack):
        /// <summary>
        /// Creates a delegate with a given signature that could be used to invoke this object from non-dynamic code (w/o code context).
        /// A stub is created that makes appropriate conversions/boxing and calls the object.
        /// The stub should be executed within a context of this object's language.
        /// </summary>
        /// <returns>The delegate or a <c>null</c> reference if the object is not callable.</returns>
        public static Delegate GetDelegate(object callableObject, Type delegateType, Action<Exception> exceptionHandler) {
            if (delegateType == null) throw new ArgumentNullException("delegateType");

            Delegate result = callableObject as Delegate;
            if (result != null) {
                if (!delegateType.IsAssignableFrom(result.GetType())) {
                    throw RuntimeHelpers.SimpleTypeError(String.Format("Cannot cast {0} to {1}.", result.GetType(), delegateType));
                }

                return result;
            }

            IDynamicObject dynamicObject = callableObject as IDynamicObject;
            if (dynamicObject != null) {

                MethodInfo invoke;
                
                if (!typeof(Delegate).IsAssignableFrom(delegateType) || (invoke = delegateType.GetMethod("Invoke")) == null) {
                    throw RuntimeHelpers.SimpleTypeError("A specific delegate type is required.");
                }

// using IDynamicObject.LanguageContext for now, we need todo better
                Debug.Assert(dynamicObject.LanguageContext != null, "Invalid implementation");

                ParameterInfo[] parameters = invoke.GetParameters();

                dynamicObject.LanguageContext.CheckCallable(dynamicObject, parameters.Length);
                
                DelegateSignatureInfo signatureInfo = new DelegateSignatureInfo(
                    dynamicObject.LanguageContext.Binder,
                    invoke.ReturnType,
                    parameters,
                    exceptionHandler
                );

                DelegateInfo delegateInfo = _dynamicDelegateCache.GetOrCreateValue(signatureInfo,
                    delegate() {
                        // creation code
                        return signatureInfo.GenerateDelegateStub();
                    });


                result = delegateInfo.CreateDelegate(delegateType, dynamicObject);
                if (result != null) {
                    return result;
                }
            }

            throw RuntimeHelpers.SimpleTypeError("Object is not callable.");
        }

        /// <summary>
        /// Registers a set of extension methods from the provided assemly.
        /// </summary>
        public static void RegisterAssembly(Assembly assembly) {
            object[] attrs = assembly.GetCustomAttributes(typeof(ExtensionTypeAttribute), false);
            foreach (ExtensionTypeAttribute et in attrs) {
                ExtendOneType(et, DynamicHelpers.GetDynamicTypeFromType(et.Extends));
            }
        }

        public static void ExtendOneType(ExtensionTypeAttribute et, DynamicType dt) {
            ExtensionTypeAttribute.RegisterType(et.Extends, et.Type, dt);

            DynamicTypeExtender.ExtendType(dt, et.Type, et.Transformer);

            if (et.EnableDerivation) {
                DynamicTypeBuilder.GetBuilder(DynamicHelpers.GetDynamicTypeFromType(et.Extends)).SetIsExtensible();
            } else if (et.DerivationType != null) {
                DynamicTypeBuilder.GetBuilder(DynamicHelpers.GetDynamicTypeFromType(et.Extends)).SetExtensionType(et.DerivationType);
            }
        }
    }
}
