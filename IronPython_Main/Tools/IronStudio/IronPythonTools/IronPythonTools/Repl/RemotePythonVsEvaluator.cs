/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using Microsoft.IronPythonTools.Library.Repl;
using Microsoft.IronStudio.Core.Repl;
using Microsoft.IronStudio;
using Microsoft.IronPythonTools.Language;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Editor;
using Microsoft.IronStudio.Repl;

namespace Microsoft.IronPythonTools.Repl {
    public sealed class RemotePythonVsEvaluator : RemotePythonEvaluator {
        // Constructed via reflection when deserialized from the registry.
        public RemotePythonVsEvaluator() {
        }

        public override void TextViewCreated(IReplWindow window, VisualStudio.Text.Editor.ITextView view) {
            var adapterFactory = IronPythonToolsPackage.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
            new EditFilter((IWpfTextView)view, adapterFactory.GetViewAdapter(view));
            window.UseSmartUpDown = IronPythonToolsPackage.Instance.OptionsPage.ReplSmartHistory;
            base.TextViewCreated(window, view);
        }

        public override void Reset() {
            base.Reset(); 
            Initialize();
        }

        public void Initialize() {
            string filename, dir;
            if (CommonPackage.TryGetStartupFileAndDirectory(out filename, out dir)) {
                RemoteScriptFactory.SetCurrentDirectory(dir);
            }
        }

        protected override bool ShouldEvaluateForCompletion(string source) {
            switch (IronPythonToolsPackage.Instance.OptionsPage.ReplIntellisenseMode) {
                case ReplIntellisenseMode.AlwaysEvaluate: return true;
                case ReplIntellisenseMode.DontEvaluateCalls: return base.ShouldEvaluateForCompletion(source);
                case ReplIntellisenseMode.NeverEvaluate: return false;
                default: throw new InvalidOperationException();
            }
        }
    }
}
