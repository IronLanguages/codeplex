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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IronStudio.Repl;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Threading;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Microsoft.IronStudio.Core.Repl {
    [Export(typeof(IReplCommand))]
    class CancelExecutionCommand : IReplCommand {
        #region IReplCommand Members

        public void Execute(IReplWindow window, string arguments) {
            window.AbortCommand();
        }

        public string Description {
            get { return "Stops execution of the current command."; }
        }

        public string Command {
            get { return null; }
        }

        public object ButtonContent {
            get {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.IronStudio.Core.Resources.CancelEvaluation.gif");
                image.EndInit();
                var res = new Image();
                res.Source = image;
                res.Width = res.Height = 16;
                return res;
            }
        }

        #endregion
    }
}
