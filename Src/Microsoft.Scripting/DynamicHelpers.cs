/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using Microsoft.Scripting.Utils;

using Microsoft.Scripting.Types;

namespace Microsoft.Scripting {
    public static class DynamicHelpers {
        private static TopReflectedPackage _topPackage;

        /// <summary> Table of dynamicly generated delegates which are shared based upon method signature. </summary>
        private static Publisher<DelegateSignatureInfo, DelegateInfo> _dynamicDelegateCache = new Publisher<DelegateSignatureInfo, DelegateInfo>();
        private static Dictionary<Type, List<Type>> _extensionTypes = new Dictionary<Type, List<Type>>();

        public static DynamicType GetDynamicTypeFromType(Type type) {
            Contract.RequiresNotNull(type, "type");

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
            List<DynamicStackFrame> frames = Utils.ExceptionUtils.GetDataDictionary(e)[typeof(DynamicStackFrame)] as List<DynamicStackFrame>;

            if (frames == null) {
                // we may have missed a dynamic catch, and our host is looking
                // for the exception...
                frames = ExceptionHelpers.AssociateDynamicStackFrames(e);
                ExceptionHelpers.ClearDynamicStackFrames();
            }

            if (frames == null) {
                return new DynamicStackFrame[0];
            }

#if !SILVERLIGHT
            frames = new List<DynamicStackFrame>(frames);
            List<DynamicStackFrame> res = new List<DynamicStackFrame>();

            // the list of _stackFrames we build up in RuntimeHelpers can have
            // too many frames if exceptions are thrown from script code and
            // caught outside w/o calling GetDynamicStackFrames.  Therefore we
            // filter down to only script frames which we know are associated
            // w/ the exception here.
            try {
                StackTrace outermostTrace = new StackTrace(e);
                IList<StackTrace> otherTraces = ExceptionHelpers.GetExceptionStackTraces(e) ?? new List<StackTrace>();
                List<StackFrame> clrFrames = new List<StackFrame>();
                foreach (StackTrace trace in otherTraces) {
                    clrFrames.AddRange(trace.GetFrames());
                }
                clrFrames.AddRange(outermostTrace.GetFrames());
                
                int lastFound = 0;
                foreach (StackFrame clrFrame in clrFrames) {
                    MethodBase method = clrFrame.GetMethod();

                    for (int j = lastFound; j < frames.Count; j++) {
                        MethodBase other = frames[j].GetMethod();
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
            Contract.RequiresNotNull(delegateType, "delegateType");

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

        /// <summary>
        /// Registers a set of extension methods from the provided assemly.
        /// </summary>
        public static void RegisterLanguageAssembly(Assembly assembly) {
            object[] attrs = assembly.GetCustomAttributes(typeof(ExtensionTypeAttribute), false);
            foreach (ExtensionTypeAttribute et in attrs) {
                ExtendOneType(et, DynamicHelpers.GetDynamicTypeFromType(et.Extends), false);
            }
        }

        private static void RegisterOneExtension(Type extending, Type extension) {
            lock (_extensionTypes) {
                List<Type> extensions;
                if (!_extensionTypes.TryGetValue(extending, out extensions)) {
                    _extensionTypes[extending] = extensions = new List<Type>();
                }
                extensions.Add(extension);
            }
        }

        public static void ExtendOneType(ExtensionTypeAttribute et, DynamicType dt) {
            ExtendOneType(et, dt, true);
        }

        public static void ExtendOneType(ExtensionTypeAttribute et, DynamicType dt, bool publish) {
            // new-style extensions:
            if(publish) RegisterOneExtension(et.Extends, et.Type);

            ExtensionTypeAttribute.RegisterType(et.Extends, et.Type, dt);

            DynamicTypeExtender.ExtendType(dt, et.Type, et.Transformer);

            if (et.EnableDerivation) {
                DynamicTypeBuilder.GetBuilder(DynamicHelpers.GetDynamicTypeFromType(et.Extends)).SetIsExtensible();
            } else if (et.DerivationType != null) {
                DynamicTypeBuilder.GetBuilder(DynamicHelpers.GetDynamicTypeFromType(et.Extends)).SetExtensionType(et.DerivationType);
            }
        }

        internal static Type[] GetExtensionTypes(Type t) {
            lock (_extensionTypes) {
                List<Type> res;
                if (_extensionTypes.TryGetValue(t, out res)) {
                    return res.ToArray();
                }
            }

            return ArrayUtils.EmptyTypes;
        }
    }
}
