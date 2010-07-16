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
using System.Diagnostics;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.IronPythonTools.Intellisense {
    internal class LazyCompletion : Completion {
        private readonly string _displayText;
        private readonly Func<string> _insertionText, _description;

        public LazyCompletion(string displayText, Func<string> insertionText, Func<string> description, ImageSource glyph) {
            Debug.Assert(displayText != null);
            Debug.Assert(insertionText != null);
            Debug.Assert(description != null);

            _displayText = displayText;
            _insertionText = insertionText;
            _description = description;
            IconSource = glyph;
            IconAutomationText = "";
        }

        public override string DisplayText {
            get {
                return _displayText;
            }
        }

        public override string InsertionText {
            get {
                return _insertionText();
            }
        }

        public override string Description {
            get {
                return _description();
            }
        }
    }
}
