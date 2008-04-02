/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.CodeDom;

using IronPython.Hosting;
using IronPython.CodeDom;


namespace IronPython.Compiler.Ast {
    public abstract class Node {
        private Location start;
        private Location end;
        private ExternalLineMapping externalMapping;

        protected Node() {
            start.Line = -1;
            start.Column = -1;
            end.Line = -1;
            end.Column = -1;
        }


        public void SetLoc(Location start, Location end) {
            SetLoc(null, start, end);
        }

        public void SetLoc(CodeSpan span) {
            SetLoc(null, span);
        }

        internal void SetLoc(ExternalLineMapping externalInfo, Location start, Location end) {
            this.start = start;
            this.end = end;
            this.externalMapping = externalInfo;
        }

        internal void SetLoc(ExternalLineMapping externalInfo, CodeSpan span) {
            start.Line = span.StartLine;
            start.Column = span.StartColumn;
            end.Line = span.EndLine;
            end.Column = span.EndColumn;
            externalMapping = externalInfo;
        }

        internal CodeSpan Span {
            get {
                return new CodeSpan(start.Line, start.Column, end.Line, end.Column);
            }
        }

        public abstract void Walk(IAstWalker walker);

        public Location Start {
            get { return start; }
            set { start = value; }
        }
        public Location End {
            get { return end; }
            set { end = value; }
        }

        /// <summary>
        /// True if the node's code lives in an external file.
        /// </summary>
        internal bool IsExternal {
            get {
                return externalMapping != null;
            }
        }

        internal ExternalLineMapping ExternalInfo {
            get {
                return externalMapping;
            }
        }

        internal Location ExternalStart {
            get {
                if (!IsExternal) throw new InvalidOperationException("get ExternalStart on non-external method");

                int lineDelta = (Start.Line) - externalMapping.Start.Line;
                return new Location(externalMapping.ExternalLine + lineDelta - 2, start.Column);
            }
        }

        internal Location ExternalEnd {
            get {
                if (!IsExternal) throw new InvalidOperationException("get ExternalEnd on non-external method");

                int lineDelta = (End.Line) - externalMapping.Start.Line;
                return new Location(externalMapping.ExternalLine + lineDelta - 2, end.Column);
            }
        }
    }
}
