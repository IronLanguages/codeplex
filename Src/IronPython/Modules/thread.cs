/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

[assembly: PythonModule("thread", typeof(IronPython.Modules.PythonThread))]
namespace IronPython.Modules {
    [PythonType("thread")]
    public static class PythonThread {
        #region Public API Surface
        public static object LockType = Ops.GetDynamicTypeFromType(typeof(Lock));
        public static object error = ExceptionConverter.CreatePythonException("error", "thread");


        [Documentation("start_new_thread(function, [args, [kwDict]]) -> thread id\nCreates a new thread running the given function")]
        [PythonName("start_new_thread")]
        public static object StartNewThread(ICallerContext context, object function, object args, [ParamDict]object kwDict) {
            Tuple tupArgs = args as Tuple;
            if (tupArgs == null) throw Ops.TypeError("2nd arg must be a tuple");

            Thread t = new Thread(new ThreadObj(context, function, tupArgs, kwDict).Start);
            t.Start();

            return t.ManagedThreadId;
        }

        [Documentation("start_new_thread(function, [args, [kwDict]]) -> thread id\nCreates a new thread running the given function")]
        [PythonName("start_new_thread")]
        public static object StartNewThread(ICallerContext context, object function, object args) {
            Tuple tupArgs = args as Tuple;
            if (tupArgs == null) throw Ops.TypeError("2nd arg must be a tuple");

            Thread t = new Thread(new ThreadObj(context, function, tupArgs, null).Start);
            t.Start();

            return t.ManagedThreadId;
        }

        [PythonName("interrupt_main")]
        public static void InterruptMain() {
            throw Ops.NotImplementedError("interrupt_main not implemented");
            //throw new PythonKeyboardInterrupt();
        }

        [PythonName("exit")]
        public static void Exit() {
            throw new PythonSystemExitException();
        }

        [Documentation("allocate_lock() -> lock object\nAllocates a new lock object that can be used for synchronization")]
        [PythonName("allocate_lock")]
        public static object AllocateLock() {
            return new Lock();
        }

        [PythonName("get_ident")]
        public static object GetIdentity() {
            return Thread.CurrentThread.ManagedThreadId;
        }

        // deprecated synonyms, wrappers over preferred names...
        [Documentation("start_new(function, [args, [kwDict]]) -> thread id\nCreates a new thread running the given function")]
        [PythonName("start_new")]
        public static object StartNew(ICallerContext context, object function, object args) {
            return StartNewThread(context, function, args);
        }

        [PythonName("exit_thread")]
        public static void ExitThread() {
            Exit();
        }

        [PythonName("allocate")]
        public static object Allocate() {
            return AllocateLock();
        }

        #endregion

        [PythonType("lock")]
        public class Lock {
            AutoResetEvent blockEvent;
            Thread curHolder;

            [PythonName("acquire")]
            public object Acquire() {
                return (Acquire(Ops.TRUE));
            }

            [PythonName("acquire")]
            public object Acquire(object waitflag) {
                bool fWait = Ops.IsTrue(waitflag);
                for (; ; ) {
                    if (Interlocked.CompareExchange<Thread>(ref curHolder, Thread.CurrentThread, null) == null) {
                        return Ops.TRUE;
                    }
                    if (!fWait) {
                        return Ops.FALSE;
                    }
                    if (blockEvent == null) {
                        // try again in case someone released us, checked the block
                        // event and discovered it was null so they didn't set it.
                        CreateBlockEvent();
                        continue;
                    }
                    blockEvent.WaitOne();
                }
            }

            [PythonName("release")]
            public void Release(params object[] param) {
                Release();
            }

            [PythonName("release")]
            public void Release() {
                if (Interlocked.Exchange<Thread>(ref curHolder, null) == null) {
                    Ops.Raise(error, "lock isn't held", null);
                }
                if (blockEvent != null) {
                    // if this isn't set yet we race, it's handled in Acquire()
                    blockEvent.Set();
                }
            }

            [PythonName("locked")]
            public object IsLocked() {
                return (Ops.Bool2Object(curHolder != null));
            }

            void CreateBlockEvent() {
                AutoResetEvent are = new AutoResetEvent(false);
                if (Interlocked.CompareExchange<AutoResetEvent>(ref blockEvent, are, null) != null) {
                    are.Close();
                }
            }
        }

        #region Internal Implementation details
        private class ThreadObj {
            object func, kwargs;
            Tuple args;
            ICallerContext context;
            public ThreadObj(ICallerContext context, object function, Tuple args, object kwargs) {
                Debug.Assert(args != null);
                func = function;
                this.kwargs = kwargs;
                this.args = args;
                this.context = context;
            }

            public void Start() {
                try {
                    if (kwargs != null) {
                        Ops.CallWithArgsTupleAndKeywordDictAndContext(context, func, Ops.EMPTY, new string[0], args, kwargs);
                    } else {
                        Ops.CallWithArgsTuple(func, Ops.EMPTY, args);
                    }
                } catch (PythonSystemExitException) {
                    // ignore and quit
                } catch (Exception e) {
                    Ops.Print(context.SystemState, "Unhandled exception on thread");

                    object pythonEx = ExceptionConverter.ToPython(e);

                    bool dummy = false;
                    string result = Hosting.PythonEngine.FormatStackTraceNoDetail(e, null, ref dummy);
                    result += Hosting.PythonEngine.FormatPythonException(pythonEx);

                    Ops.Print(context.SystemState, result);
                }
            }
        }
        #endregion
    }
}
