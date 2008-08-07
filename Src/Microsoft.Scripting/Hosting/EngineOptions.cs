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
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {

    [Serializable]
    public class EngineOptions {
        private bool _exceptionDetail;
        private bool _showClrExceptions;
        private bool _interpretedMode;
        private readonly bool _perfStats;
        private readonly ReadOnlyCollection<string> _searchPaths;

        /// <summary>
        /// Interpret code instead of emitting it.
        /// </summary>
        public bool InterpretedMode {
            get { return _interpretedMode; }
            set { _interpretedMode = value; }
        }

        /// <summary>
        ///  Display exception detail (callstack) when exception gets caught
        /// </summary>
        public bool ExceptionDetail {
            get { return _exceptionDetail; }
            set { _exceptionDetail = value; }
        }

        public bool ShowClrExceptions {
            get { return _showClrExceptions; }
            set { _showClrExceptions = value; }
        }

        /// <summary>
        /// Whether to gather performance statistics.
        /// </summary>
        public bool PerfStats {
            get { return _perfStats; }
        }

        /// <summary>
        /// Initial file search paths provided by the host.
        /// </summary>
        public ReadOnlyCollection<string> SearchPaths {
            get { return _searchPaths; }
        }

        public EngineOptions() 
            : this(null) {
        }

        public EngineOptions(IDictionary<string, object> options) {
            _interpretedMode = GetOption(options, "InterpretedMode", false);
            _exceptionDetail = GetOption(options, "ExceptionDetail", false);
            _showClrExceptions = GetOption(options, "ShowClrExceptions", false);
            _perfStats = GetOption(options, "PerfStats", false);
            _searchPaths = GetStringCollectionOption(options, "SearchPaths") ?? new ReadOnlyCollection<string>(new string[] { "." });
        }

        protected static T GetOption<T>(IDictionary<string, object> options, string name, T defaultValue) {
            object value;
            if (options != null && options.TryGetValue(name, out value)) {
                if (value is T) {
                    return (T)value;
                }
                throw new ArgumentException(String.Format("Invalid value for option {0}", name));
            }
            return defaultValue;
        }

        /// <summary>
        /// Reads an option whose value is expected to be a collection of non-null strings.
        /// Reaturns a read-only copy of the option's value.
        /// </summary>
        protected static ReadOnlyCollection<string> GetStringCollectionOption(IDictionary<string, object> options, string name) {
            var collection = GetOption<ICollection<string>>(options, name, null);

            if (collection == null) {
                return null;
            }

            foreach (var item in collection) {
                if (item == null) {
                    throw new ArgumentException(String.Format("Invalid value for option {0}: collection shouldn't containt null items", name));
                }
            }

            return new ReadOnlyCollection<string>(ArrayUtils.MakeArray(collection));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        protected static readonly ReadOnlyCollection<string> EmptyStringCollection = new ReadOnlyCollection<string>(ArrayUtils.EmptyStrings);
    }
}
