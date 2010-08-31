/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SiteTest {
    /// <summary>
    /// SiteLog logs the various events of a dyn site scenario.
    /// 
    /// This class is still being designed.  It should be thread-safe.
    /// Need to better handle performance metrics
    /// </summary>
    public class SiteLog {
        public delegate void Func();
        List<Event> _log = new List<Event>();
        Stopwatch logtimer = new Stopwatch();

        Stopwatch scenarioTimer = new Stopwatch();
        string scenarioDescription = null;

        public SiteLog() {
            logtimer.Start();
        }

        /// <summary>
        /// Removes all recorded events from the log
        /// and resets the timer to 0.  This invalidates
        /// all pre-existing event ids.
        /// </summary>
        public void Reset() {
            lock (_log) {
                _log.Clear();
                logtimer.Reset();
                logtimer.Start();
            }
        }

        /// <summary>
        /// Retrieves the log event matching
        /// the given event id.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Event this[int index] {
            get {
                return _log[index];
            }
        }

        /// <summary>
        /// Returns the number of entries currently in the log.
        /// </summary>
        public int Length {
            get {
                return _log.Count;
            }
        }

        /// <summary>
        /// Delegate to be used by sites to Target the Log call
        /// </summary>
        /// <param name="site"></param>
        /// <param name="eType"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public delegate int LogDel(System.Runtime.CompilerServices.CallSite site, EventType eType, string text);

        /// <summary>
        /// Adds a new event to the log with the given
        /// description using the current time and thread
        /// information.
        /// </summary>
        /// <param name="eType">The type of this event</param>
        /// <param name="text">Description of the new event</param>
        /// <returns></returns>
        public int Log(EventType eType, string text) {
            Event ev = new Event();
            ev.Time = logtimer.ElapsedMilliseconds;
            ev.EType = eType;
            ev.ThreadId = Thread.CurrentThread.ManagedThreadId;
            ev.Description = text;
            lock (_log) {
                _log.Add(ev);
                return _log.Count - 1 ;
            }
        }

        /// <summary>
        /// Adds a new event with the given type and
        /// an empty description.
        /// </summary>
        /// <param name="eType">The type of this event</param>
        /// <returns></returns>
        public int Log(EventType eType) {
            return Log(eType, null);
        }

        /// <summary>
        /// Logs a ScenarioBegin event, invokes f,
        /// logs a ScenarioEnd event and returns
        /// the elapsed time in milliseconds.
        /// </summary>
        /// <param name="text">Description of the event</param>
        /// <param name="f"></param>
        /// <returns></returns>
        public long TimeScenario(string text, Func f) {
            StartTimer(text);
            f();
            return StopTimer();
        }

        /// <summary>
        /// Logs a ScenarioBegin event, invokes f,
        /// logs a ScenarioEnd event and returns
        /// the elapsed time in milliseconds.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public long TimeScenario(Func f) {
            return TimeScenario(null, f);
        }

        /// <summary>
        /// Logs a ScenarioBegin event and starts
        /// a separate stopwatch timer.  The given
        /// text is used as the event's long description.
        /// </summary>
        /// <param name="text"></param>
        public void StartTimer(string text) {
            if(scenarioTimer.IsRunning)
                throw new ApplicationException("The scenario timer is already running");

            scenarioDescription = text;
            scenarioTimer.Reset();
            Log(EventType.ScenarioBegin, text);
            scenarioTimer.Start();
        }

        /// <summary>
        /// Logs a ScenarioBegin event and starts
        /// a separate stopwatch timer.
        /// </summary>
        public void StartTimer() {
            StartTimer(null);
        }

        /// <summary>
        /// Stops a running scenario stopwatch timer
        /// logs an event and returns the number of
        /// milliseconds elapsed since the timer was
        /// started.
        /// </summary>
        /// <returns>Elapsed time in milliseconds</returns>
        public long StopTimer() {
            if (!scenarioTimer.IsRunning)
                throw new ApplicationException("The scenario timer is not running");
            Log(EventType.ScenarioEnd, scenarioDescription);
            scenarioTimer.Stop();
            scenarioDescription = null;
            return scenarioTimer.ElapsedMilliseconds;
        }

        /// <summary>
        /// Returns a pretty-printable view of the log contents
        /// 
        /// Don't want to override ToString because it impacts how
        /// -X:ShowRules works.
        /// </summary>
        /// <returns></returns>
        public string ToPrintableString() {
            lock (_log) {
                return ToPrintableString(_log.ToArray());
            }
        }

        /// <summary>
        /// Returns a pretty-printable view of a given list of events
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        private string ToPrintableString(Event[] events) {
            StringBuilder repr = new StringBuilder();
            repr.AppendLine("TID\tTime\tEvent Type\t\tDescription");
            foreach (Event e in events) {
                repr.AppendLine(e.ToString());
            }
            return repr.ToString();
        }

        /// <summary>
        /// Generates an Expression tree that, when invoked,
        /// will append a new event to this log with the
        /// given description and the current (at invocation time)
        /// time and thread information.
        /// </summary>
        /// <param name="textExp">An Expression that returns a string that describes the new event</param>
        /// <param name="eType"></param>
        /// <returns></returns>
        public Expression GenLog(EventType eType, Expression textExp) {
            return GenLog(Expression.Constant(eType), textExp);
        }

        /// <summary>
        /// Generates an Expression tree that, when invoked,
        /// will append a new event to this log with the
        /// given description and the current (at invocation time)
        /// time and thread information.
        /// </summary>
        /// <param name="text">Description of the new event</param>
        /// <param name="eType"></param>
        /// <returns></returns>
        public Expression GenLog(EventType eType, string text) {
            return GenLog(eType, Expression.Constant(text, typeof(string)));
        }

        /// <summary>
        /// Generates an Expression tree that, when invoked,
        /// will append a new event to this log with the
        /// given description and the current (at invocation time)
        /// time and thread information.
        /// </summary>
        /// <param name="eTypeExp"></param>
        /// <param name="textExp"></param>
        /// <returns></returns>
        public Expression GenLog(Expression eTypeExp, Expression textExp) {
            return Expression.Call(Expression.Constant(this),
                typeof(SiteLog).GetMethod("Log", new Type[] { typeof(EventType), typeof(string) }),
                eTypeExp,
                textExp);
        }

        /// <summary>
        /// Compares the current log against the given
        /// sequence of events, ignoring all extended information
        /// (long description, threadid, timings).
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool MatchesEventSequence(params Event[] args) {
            bool result = true;
            lock (_log) {
                if (args.Length != _log.Count)
                    result = false;
                else {
                    for (int i = 0; i < _log.Count; i++) {
                        if (args[i].EType != _log[i].EType) {
                            result = false;
                            break;
                        }

                        //If the baseline sequence didn't give an event description then don't compare
                        if (args[i].Description != null) {
                            if (args[i].Description != _log[i].Description) {
                                result = false;
                                break;
                            }
                        }
                    }
                }

                if (!result) {
                    Console.WriteLine("Expected:\n{0}", ToPrintableString(args));
                    Console.WriteLine("Actual:\n{0}", ToPrintableString());
                }

                return result;
            }
        }

        public Event CreateEvent(EventType eType) {
            return CreateEvent(eType, null);
        }

        public Event CreateEvent(EventType eType, string descr) {
            Event e = new Event();
            e.EType = eType;
            e.Description = descr;
            return e;
        }

        /// <summary>
        /// Represents a single log event
        /// </summary>
        public struct Event {
            public int ThreadId;
            public long Time;
            public string Description;
            public EventType EType;

            public override string ToString() {
                return String.Format("{0}\t{1}\t{2}\t\t{3}", ThreadId, Time, EType, Description);
            }
        }

        /// <summary>
        /// Enumeration of the different types of dyn site
        /// events we care to track.  This makes it easier
        /// to compare against an expected baseline.
        /// </summary>
        public enum EventType {
            MakeRule,
            ValidatorInvocation,
            TestInvocation,
            TargetInvocation,
            CachePrune_L2,
            CacheOverflow_L1,
            ScenarioBegin,
            ScenarioEnd,
            //@TODO - Add any others...
        }
    }
}
