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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;   

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;

namespace IronPython.Runtime {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix"), PythonSystemType("generator")]
    public sealed class PythonGenerator : Generator, IEnumerable, IEnumerable<object> {
        private NextTarget _next;

        /// <summary>
        /// True if the generator has finished (is "closed"), else false.
        /// Python language spec mandates that calling Next on a closed generator gracefully throws a StopIterationException.
        /// This can never be reset.
        /// </summary>
        private bool _closed;

        /// <summary>
        /// Initially false, and set to True after the generator body is executed (through MoveNext(), Send, Throw, Close, etc)
        /// This is never reset and remains true even after the generated is closed.
        /// </summary>
        private bool _started;

        /// <summary>
        /// True iff the thread is currently inside the generator (ie, invoking the _next delegate).
        /// This can be used to enforce that a generator does not call back into itself. 
        /// Pep255 says that a generator should throw a ValueError if called reentrantly.
        /// </summary>
        private bool _active;

        /// <summary>
        /// Delegate to generator body. 
        /// </summary>
        /// <param name="generator">instance data for the generator. This contains the current state for the generator's state machine</param>
        /// <param name="ret">next item to be yielded from the generator</param>
        /// <returns>true if ret is valid, false if generator is at end. This provides a way for the delegate 
        /// to signal termination without having to incur the perf hit of throwing a StopIterationException.
        /// Null is a valid yield value</returns>
        /// <exception cref="StopIterationException">Python generators can gracefully abort by throwing a 
        /// StopIteration exception.</exception>
        public delegate bool NextTarget(PythonGenerator generator, out object ret);


        // Fields set by Throw() to communicate an exception to the yield point.
        // These are plumbed through the generator to become parameters to Raise(...) invoked 
        // at the yield suspenion point in the generator.
        private object _throwable;
        private object _value;
        private object _traceback;

        // Value sent by generator.send().
        // Since send() could send an exception, we need to keep this different from throwable's value.
        private object _sendValue;

        public PythonGenerator(CodeContext context, NextTarget next)
            : base(context) {
            _next = next;
        }

        // Silverlight doesn't allow finalizers in user code.
#if !SILVERLIGHT
        // Pep 342 says generators now have finalizers (__del__) that call Close()
        ~PythonGenerator() {
            try {
                // This may run the users generator.
                this.close();

            } catch (Exception e) {
                // An unhandled exceptions on the finalizer could tear down the process, so catch it.

                // PEP says:
                //   If close() raises an exception, a traceback for the exception is printed to sys.stderr
                //   and further ignored; it is not propagated back to the place that
                //   triggered the garbage collection. 

                // Sample error message from CPython 2.5 looks like:
                //     Exception __main__.MyError: MyError() in <generator object at 0x00D7F6E8> ignored
                string message = string.Format("Exception {0} in {1} ignored\n", e.Message, this);
                PythonOps.Write(Context, PythonContext.GetContext(Context).SystemStandardError, message);
            }
        }
#endif // !SILVERLIGHT

        protected override bool MoveNext() {
            _started = true;
            bool ret = false;
            object next = null;

            // Python's language policy on generators is that attempting to access after it's closed (returned)
            // just continues to throw StopIteration exceptions.
            if (_closed) {
                return false;
            }

            bool lastActive = _active;
            // Generators can not be called re-entrantly.
            if (_active) {
                // A generato could catch this exception and continue executing, so this does
                // not necessarily close the generator.
                throw PythonOps.ValueError("generator already executing");
            }
            _active = true;

            Exception save = PythonOps.SaveCurrentException();
            try {
                try {
                    // This calls into the delegate that has the real body of the generator.
                    // The generator body here may:
                    // 1. return an item: _next() returns true and 'next' is set to the next item in the enumeration.
                    // 2. Exit normally: _next returns false.
                    // 3. Exit with a StopIteration exception: for-loops and other enumeration consumers will 
                    //    catch this and terminate the loop without propogating the exception.
                    // 4. Exit via some other unhandled exception: This will close the generator, but the exception still propogates.
                    //    _next does not return, so ret is left assigned to false (closed), which we detect in the finally.
                    ret = _next(this, out next);
                } catch (StopIterationException) {
                    ret = false;
                }
            } finally {
                // A generator restores the sys.exc_info() status after each yield point.
                PythonOps.RestoreCurrentException(save);
                _active = lastActive;

                // If _next() returned false, or did not return (thus leavintg ret assigned to its initial value of false), then 
                // the body of the generator has exited and the generator is now closed.
                if (!ret) {
                    next = null;
                    _closed = true;
                }
            }
            this.Current = next;
            return ret;
        }

        public object next() {
            if (!MoveNext()) {
                throw PythonOps.StopIteration();
            }
            return Current;
        }

        /// <summary>
        /// See PEP 342 (http://python.org/dev/peps/pep-0342/) for details of new methods on Generator.
        /// Full signature including default params for throw is:
        ///    throw(type, value=None, traceback=None)
        /// Use multiple overloads to resolve the default parameters.
        /// </summary>
        public object @throw(object type) {
            return @throw(type, null, null);
        }

        public object @throw(object type, object value) {
            return @throw(type, value, null);
        }

        /// <summary>
        /// Throw(...) is like Raise(...) being called from the yield point within the generator.
        /// Note it must come from inside the generator so that the traceback matches, and so that it can 
        /// properly cooperate with any try/catch/finallys inside the generator body.
        /// 
        /// If the generator catches the exception and yields another value, that is the return value of g.throw().
        /// </summary>
        public object @throw(object type, object value, object traceback) {            
            // The Pep342 explicitly says "The type argument must not be None". 
            // According to CPython 2.5's implementation, a null type argument should:
            // - throw a TypeError exception (just as Raise(None) would) *outside* of the generator's body
            //   (so the generator can't catch it).
            // - not update any other generator state (so future calls to Next() will still work)
            if (type == null) {
                // Create the appropriate exception and throw it.
                throw PythonOps.MakeExceptionTypeError(null);
            }

            // Set fields which will then be used by CheckThrowable.
            // We create the actual exception from inside the generator so that if the exception's __init__ 
            // throws, the traceback matches that which we get from CPython2.5.
            _throwable = type;
            _value = value;
            _traceback = traceback;
            Debug.Assert(_sendValue == null);

            // Pep explicitly says that Throw on a closed generator throws the exception, 
            // and not a StopIteration exception. (This is different than Next()).
            if (_closed) {
                // this will throw the exception that we just set the fields for.
                CheckThrowable();
            }

            if (!MoveNext()) {
                throw PythonOps.StopIteration();
            }
            return Current;
        }

        /// <summary>
        /// send() was added in Pep342. It sends a result back into the generator, and the expression becomes
        /// the result of yield when used as an expression.
        /// </summary>
        public object send(object value) {
            Debug.Assert(_throwable == null);

            // CPython2.5's behavior is that Send(non-null) on unstaretd generator should:
            // - throw a TypeError exception
            // - not change generator state. So leave as unstarted, and allow future calls to succeed.
            if (value != null && !_started) {
                throw PythonOps.TypeErrorForIllegalSend();
            }

            _sendValue = value;
            return next();
        }
        
        /// <summary>
        /// Close introduced in Pep 342.
        /// </summary>
        public void close() {
            // This is nop if the generator is already closed.

            // Optimization to avoid throwing + catching an exception if we're already closed.
            if (_closed) {
                return;
            }

            // This function body is the psuedo code straight from Pep 342.
            try {
                this.@throw(new GeneratorExitException());

                // Generator should not have exited normally. 
                throw new RuntimeException("generator ignored GeneratorExit");
            } catch (StopIterationException) {
                // Ignore
            } catch (GeneratorExitException) {
                // Ignore
            }
        }


        public override string ToString() {
            return string.Format("<generator object at {0}>", PythonOps.HexId(this));
        }
        
        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return this;
        }

        #endregion      

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return this;
        }

        #endregion

        #region Internal implementation details

        /// <summary>
        /// Helper called from PythonOps after the yield statement
        /// Keepin this in a helper method:
        /// - reduces generated code size
        /// - allows better coupling with PythonGenerator.Throw()
        /// - avoids throws from emitted code (which can be harder to debug).
        /// </summary>
        /// <returns></returns>
        internal object CheckThrowableAndReturnSendValue() {
            // Since this method is called from the generator body's execution, the generator must be running 
            // and not closed.
            Debug.Assert(!_closed);
            if (_sendValue != null) {
                // Can't Send() and Throw() at the same time.
                Debug.Assert(_throwable == null);

                object sendValueBackup = _sendValue;
                _sendValue = null;
                return sendValueBackup;
            }
            CheckThrowable();
            return null;
        }

        /// <summary>
        /// Called to throw an exception set by Throw().
        /// </summary>
        private void CheckThrowable() {
            if (this._throwable != null) {
                object throwableBackup = _throwable;

                // Clear it so that any future Next()/MoveNext() call doesn't pick up the exception again.
                _throwable = null;

                // This may invoke user code such as __init__, thus MakeException may throw. 
                // Since this is invoked from the generator's body, the generator can catch this exception. 
                Exception e = PythonOps.MakeException(Context, throwableBackup, _value, _traceback);
                throw e;
            }
        }

        #endregion
    }
}
