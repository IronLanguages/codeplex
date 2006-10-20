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
using System.Text;
using System.CodeDom;
using System.IO;

namespace IronPython.CodeDom {
    public interface IMergableProvider {
        CodeCompileUnit ParseMergable(string text, string filename, IMergeDestination mergeDestination);
        CodeCompileUnit ParseMergable(StreamReader sw, IMergeDestination mergeDestination);
        void MergeCodeFromCompileUnit(CodeCompileUnit compileUnit);
    }

    public interface IMergeDestination {
        /// <summary>
        /// Inserts a block of lines into the text buffer at the specified starting line
        /// </summary>
        void InsertRange(int start, IList<string> lines);

        /// <summary>
        /// Removes a block of count lines from the text buffer at the specified starting line
        /// </summary>
        void RemoveRange(int start, int count);        

        /// <summary>
        /// Returns true if any inserts / removes have been performed, false otherwise
        /// </summary>
        bool HasMerged {
            get;
        }

        /// <summary>
        /// Gets the number of lines currently in the text buffer
        /// </summary>
        int LineCount {
            get;
        }

        /// <summary>
        /// Returns the modified text to be written out to the text buffer, or null if
        /// the destination doesn't require the right
        /// </summary>
        string FinalText {
            get;
        }
    }
}
