/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public class TryStatementBuilder {
        private Statement _tryStatement;
        private List<CatchBlock> _catchBlocks;
        private Statement _finallyStatement;
        private bool _skipNext;
        private SourceSpan _statementSpan;
        private SourceLocation _header;

        internal TryStatementBuilder(SourceSpan statementSpan, SourceLocation bodyLocation, Statement body) {
            if (body == null) throw new ArgumentNullException("body");

            _tryStatement = body;
            _header = bodyLocation;
            _statementSpan = statementSpan;
        }

        public TryStatementBuilder Catch(Type type, Statement body) {
            return Catch(type, (Variable)null, body);
        }

        public TryStatementBuilder Catch(Type type, params Statement[] body) {
            return Catch(type, null, body);
        }

        public TryStatementBuilder Catch(Type type, Variable holder, params Statement[] body) {
            return Catch(type, holder, Ast.Block(body));
        }

        public TryStatementBuilder Catch(Type type, Variable holder, Statement body) {
            if (type == null) throw new ArgumentNullException("type");
            if (body == null) throw new ArgumentNullException("body");

            if (_skipNext) {
                _skipNext = false;
                return this;
            }

            if (_finallyStatement != null) throw new InvalidOperationException("Finally statement already defined");
            
            if (_catchBlocks == null) {
                _catchBlocks = new List<CatchBlock>();
            }

            _catchBlocks.Add(Ast.Catch(type, holder, body));
            return this;
        }

        public TryStatementBuilder Filter(Type type, Variable holder, Expression condition, params Statement[] body) {
            return Filter(type, holder, condition, Ast.Block(body));
        }

        public TryStatementBuilder Filter(Type type, Variable holder, Expression condition, Statement body) {
            if (type == null) throw new ArgumentNullException("type");
            if (condition == null) throw new ArgumentNullException("condition");
            if (holder == null) throw new ArgumentNullException("holder");
            if (body == null) throw new ArgumentNullException("body");

            if (_skipNext) {
                _skipNext = false;
                return this;
            }

            if (_catchBlocks == null) {
                _catchBlocks = new List<CatchBlock>();
            }

            _catchBlocks.Add(Ast.Catch(type, holder, Ast.IfThenElse(condition, body, Ast.Statement(Ast.Rethrow()))));
            return this;
        }

        public TryStatementBuilder Finally(params Statement[] body) {
            return Finally(Ast.Block(body));
        }

        public TryStatementBuilder Finally(Statement body) {
            if (body == null) throw new ArgumentNullException("body");
            
            if (_skipNext) {
                _skipNext = false;
                return this;
            }
            
            if (_finallyStatement != null) throw new InvalidOperationException("Finally statement already defined");

            _finallyStatement = body;
            return this;
        }

        public TryStatementBuilder SkipIf(bool condition) {
            _skipNext = condition;
            return this;
        }

        public static implicit operator TryStatement(TryStatementBuilder builder) {
            Assert.NotNull(builder);
            return new TryStatement(
                builder._statementSpan,
                builder._header,
                builder._tryStatement, 
                (builder._catchBlocks != null) ? builder._catchBlocks.ToArray() : null, 
                builder._finallyStatement
            ); 
        }
    }

    public static partial class Ast {

        public static TryStatementBuilder Try(params Statement[] body) {
            return new TryStatementBuilder(SourceSpan.None, SourceLocation.None, new BlockStatement(SourceSpan.None, body));
        }
        
        public static TryStatementBuilder Try(Statement body) {
            return new TryStatementBuilder(SourceSpan.None, SourceLocation.None, body);
        }

        public static TryStatementBuilder Try(SourceSpan statementSpan, SourceLocation bodyLocation, Statement body) {
            return new TryStatementBuilder(statementSpan, bodyLocation, body);
        }


        public static TryStatement TryCatch(SourceSpan span, SourceLocation header, Statement body, params CatchBlock[] handlers) {
            return TryCatchFinally(span, header, body, handlers, null);
        }

        public static TryStatement TryFinally(Statement body, Statement @finally) {
            return TryCatchFinally(SourceSpan.None, SourceLocation.None, body, null, @finally);
        }

        public static TryStatement TryFinally(SourceSpan span, SourceLocation header, Statement body, Statement @finally) {
            return TryCatchFinally(span, header, body, null, @finally);
        }

        public static TryStatement TryCatchFinally(Statement body, CatchBlock[] handlers, Statement @finally) {
            return new TryStatement(SourceSpan.None, SourceLocation.None, body, handlers, @finally);
        }

        public static TryStatement TryCatchFinally(SourceSpan span, SourceLocation header, Statement body, CatchBlock[] handlers, Statement @finally) {
            return new TryStatement(span, header, body, handlers, @finally);
        }
    }
}
