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

namespace Microsoft.Scripting {
    public static class DynamicHelpers {
        public delegate DynamicType GetDynamicTypeDelegate(Type t); // HACKHACK: remove when ReflectedTypeBuilder moves down.

        public static GetDynamicTypeDelegate GetDynamicTypeFromType;   // HACKHACK: Make a function

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
            IDynamicObject dt = o as IDynamicObject;
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

    }
}
