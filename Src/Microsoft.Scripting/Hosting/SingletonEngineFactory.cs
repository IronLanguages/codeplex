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
using System.Text;
using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public sealed class SingletonEngineFactory<EngineType, EngineOptionsType, LanguageContextType>
        where EngineType : ScriptEngine
        where EngineOptionsType : EngineOptions
        where LanguageContextType : LanguageContext {

        private readonly object _singletonLock = new object();
        private bool _initializing = false;
        private EngineType _singleton;

        private Function<LanguageContextType, EngineOptionsType, EngineType> _createEngine;
        private Function<EngineOptionsType> _getSetupInformation;

        public SingletonEngineFactory(
            Function<LanguageContextType, EngineOptionsType, EngineType> createEngine, 
            Function<EngineOptionsType> getSetupInformation) {

            _createEngine = createEngine;
            _getSetupInformation = getSetupInformation;
        }

        public EngineType GetInstance() {
            if (_singleton == null) {
                GetInstance((LanguageContextType)ScriptDomainManager.CurrentManager.GetLanguageContext(typeof(LanguageContextType)), null);
            }
            return _singleton;
        }

        public EngineType GetInstance(LanguageContextType provider, EngineOptionsType options) {
            Contract.RequiresNotNull(provider, "provider");

            if (_singleton == null) {

                if (options == null) {
                    options = _getSetupInformation();
                }

                lock (_singletonLock) {
                    if (_singleton == null) {

                        if (_initializing) {
                            throw new InvalidOperationException("Singleton engine accessed during its initialization");
                        }

                        _initializing = true;

                        EngineType singleton = _createEngine(provider, options);

                        Utilities.MemoryBarrier();

                        _singleton = singleton;
                        _getSetupInformation = null;
                        _createEngine = null;
                    }
                }
            }
            return _singleton;
        }
	}
}
