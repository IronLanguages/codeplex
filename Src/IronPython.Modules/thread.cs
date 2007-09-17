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
using System.Collections;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Microsoft.Scripting;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.Hosting;
using Microsoft.Scripting.Utils;

[assembly: PythonModule("thread", typeof(IronPython.Modules.PythonThread))]
namespace IronPython.Modules {
    [PythonType("thread")]
    public static class PythonThread {
        #region Public API Surface
        public static object LockType = DynamicHelpers.GetDynamicTypeFromType(typeof(Lock));
        public static object error = ExceptionConverter.CreatePythonException("error", "thread");


        [Documentation("start_new_thread(function, [args, [kwDict]]) -> thread id\nCreates a new thread running the given function")]
        [PythonName("start_new_thread")]
        public static object StartNewThread(CodeContext context, object function, object args, object kwDict) {
            PythonTuple tupArgs = args as PythonTuple;
            if (tupArgs == null) throw PythonOps.TypeError("2nd arg must be a tuple");

            Thread t = new Thread(new ThreadObj((CodeContext)context, function, tupArgs, kwDict).Start);
            t.Start();

            return t.ManagedThreadId;
        }

        [Documentation("start_new_thread(function, args, [kwDict]) -> thread id\nCreates a new thread running the given function")]
        [PythonName("start_new_thread")]
        public static object StartNewThread(CodeContext context, object function, object args) {
            PythonTuple tupArgs = args as PythonTuple;
            if (tupArgs == null) throw PythonOps.TypeError("2nd arg must be a tuple");

            Thread t = new Thread(new ThreadObj((CodeContext)context, function, tupArgs, null).Start);
            t.IsBackground = true;
            t.Start();

            return t.ManagedThreadId;
        }

        [PythonName("interrupt_main")]
        public static void InterruptMain() {
            throw PythonOps.NotImplementedError("interrupt_main not implemented");
            //throw new PythonKeyboardInterrupt();
        }

        [PythonName("exit")]
        public static void Exit() {
            PythonOps.SystemExit();
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
        public static object StartNew(CodeContext context, object function, object args) {
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
                return (Acquire(RuntimeHelpers.True));
            }

            [PythonName("acquire")]
            public object Acquire(object waitflag) {
                bool fWait = PythonOps.IsTrue(waitflag);
                for (; ; ) {
                    if (Interlocked.CompareExchange<Thread>(ref curHolder, Thread.CurrentThread, null) == null) {
                        return RuntimeHelpers.True;
                    }
                    if (!fWait) {
                        return RuntimeHelpers.False;
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
                    throw PythonOps.MakeException(error, "lock isn't help", null);
                }
                if (blockEvent != null) {
                    // if this isn't set yet we race, it's handled in Acquire()
                    blockEvent.Set();
                }
            }

            [PythonName("locked")]
            public bool IsLocked() {
                return curHolder != null;
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
            PythonTuple args;
            CodeContext context;
            public ThreadObj(CodeContext context, object function, PythonTuple args, object kwargs) {
                Debug.Assert(args != null);
                func = function;
                this.kwargs = kwargs;
                this.args = args;
                this.context = context;
            }

            public void Start() {
                try {
                    if (kwargs != null) {
                        PythonOps.CallWithArgsTupleAndKeywordDictAndContext(context, func, ArrayUtils.EmptyObjects, ArrayUtils.EmptyStrings, args, kwargs);
                    } else {
                        PythonOps.CallWithArgsTuple(func, ArrayUtils.EmptyObjects, args);
                    }
                } catch (PythonSystemExitException) {
                    // ignore and quit
                } catch (Exception e) {
                    PythonOps.Print("Unhandled exception on thread");
                    string result = PythonEngine.CurrentEngine.FormatException(e);
                    PythonOps.Print(result);
                }
            }
        }
        #endregion
    }
}
