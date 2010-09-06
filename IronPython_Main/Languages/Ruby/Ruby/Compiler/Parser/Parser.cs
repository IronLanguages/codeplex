/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler.Ast;
using IronRuby.Runtime;
using IronRuby.Builtins;
using System.Globalization;

namespace IronRuby.Compiler {

    public interface ILexicalVariableResolver {
        bool IsLocalVariable(string/*!*/ identifier);
    }

    internal sealed class DummyVariableResolver : ILexicalVariableResolver {
        public static readonly ILexicalVariableResolver AllVariableNames = new DummyVariableResolver(true);
        public static readonly ILexicalVariableResolver AllMethodNames = new DummyVariableResolver(false);
        private readonly bool _isVariable;

        private DummyVariableResolver(bool isVariable) {
            _isVariable = isVariable;
        }

        public bool IsLocalVariable(string/*!*/ identifier) {
            return _isVariable;
        }
    }

    // The non-autogenerated part of the Parser class.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class Parser : ILexicalVariableResolver {
        internal sealed class InternalSyntaxError : Exception {
        }

        private int _inSingletonMethodDefinition = 0;
        private int _inInstanceMethodDefinition = 0;

        private SourceUnitTree _ast;
        private List<FileInitializerStatement> _initializers; // lazy

        private Stack<LexicalScope>/*!*/ _lexicalScopes = new Stack<LexicalScope>();
        private SourceUnit _sourceUnit;
        private readonly Tokenizer/*!*/ _tokenizer;
        private Action<Tokens, SourceSpan> _tokenSink;
        private int _generatedNameId;

        // current encoding (used for __ENCODING__ pseudo-constant, literal string, symbol, regex encodings):
        internal RubyEncoding/*!*/ Encoding {
            get { return _tokenizer.Encoding; }
        }

        private bool InMethod {
            get { return _inSingletonMethodDefinition > 0 || _inInstanceMethodDefinition > 0; }
        }

        public Tokenizer/*!*/ Tokenizer {
            get { return _tokenizer; }
        }

        public ErrorSink/*!*/ ErrorSink {
            get { return _tokenizer.ErrorSink; }
        }

        public Action<Tokens, SourceSpan> TokenSink {
            get { return _tokenSink; }
            set { _tokenSink = value; }
        }

        private SourceSpan GetTokenSpan() {
            return _tokenizer.TokenSpan;
        }

        private TokenValue GetTokenValue() {
            return _tokenizer.TokenValue;
        }

        private int GetNextToken() {
            Tokens token = _tokenizer.GetNextToken();
            if (_tokenSink != null) {
                _tokenSink(token, _tokenizer.TokenSpan);
            }
            return (int)token;
        }

        private void ReportSyntaxError(string message) {
            ErrorSink.Add(_sourceUnit, message, GetTokenSpan(), -1, Severity.FatalError);
            throw new InternalSyntaxError();
        }

        internal void ReportSyntaxError(ErrorInfo error) {
            ErrorSink.Add(_sourceUnit, error.GetMessage(), GetTokenSpan(), error.Code, Severity.FatalError);
            throw new InternalSyntaxError();
        }

        private string/*!*/ GenerateErrorLocalName() {
            return "error#" + _generatedNameId++;
        }

        private string/*!*/ GenerateErrorConstantName() {
            return "Error#" + _generatedNameId++;
        }

        public Parser() 
            : this(ErrorSink.Default) {
        }

        public Parser(ErrorSink/*!*/ errorSink) {
            _tokenizer = new Tokenizer(false, this) { 
                ErrorSink = errorSink 
            };
            InitializeTables();
        }

        public SourceUnitTree Parse(SourceUnit/*!*/ sourceUnit, RubyCompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink) {
            Assert.NotNull(sourceUnit, options, errorSink);

            ErrorCounter counter = new ErrorCounter(errorSink);
            _tokenizer.ErrorSink = counter;
            _tokenizer.Compatibility = options.Compatibility;

            _lexicalScopes.Clear();

            EnterScope(CreateTopScope(options.LocalNames));

            using (SourceCodeReader reader = sourceUnit.GetReader()) {
                _sourceUnit = sourceUnit;
                _tokenizer.Initialize(null, reader, sourceUnit, options.InitialLocation);

                // Default encoding when hosted:
                _tokenizer.Encoding = (reader.Encoding != null) ? RubyEncoding.GetRubyEncoding(reader.Encoding) : RubyEncoding.UTF8;
                _tokenizer.AllowNonAsciiIdentifiers = _tokenizer.Encoding != RubyEncoding.Binary;
                
                try {
                    Parse();
                    LeaveScope();
                } catch (InternalSyntaxError) {
                    _ast = null;
                    _lexicalScopes.Clear();
                } finally {
                    ScriptCodeParseResult props;
                    if (counter.AnyError) {
                        _ast = null;

                        if (_tokenizer.UnterminatedToken) {
                            props = ScriptCodeParseResult.IncompleteToken;
                        } else if (_tokenizer.EndOfFileReached) {
                            props = ScriptCodeParseResult.IncompleteStatement;
                        } else {
                            props = ScriptCodeParseResult.Invalid;
                        }
                        
                    } else {
                        props = ScriptCodeParseResult.Complete;
                    }

                    sourceUnit.CodeProperties = props;
                }

                return _ast;
            }
        }

        // Top level scope is created for top level code. 
        // Variables defined outside of compilation unit (we are compiling eval) are stored in "outer scope", 
        // to which the top level scope is nested in such case.
        private static LexicalScope/*!*/ CreateTopScope(List<string> localVariableNames) {
            LexicalScope outer;
            if (localVariableNames != null) {
                outer = new RuntimeLexicalScope(localVariableNames);
            } else {
                outer = null;
            }

            return new TopStaticLexicalScope(outer);
        }

        private LocalVariable/*!*/ DefineParameter(string/*!*/ name, SourceSpan location) {
            // we are in a method or a block:
            Debug.Assert(CurrentScope.IsTop && !(CurrentScope is TopStaticLexicalScope) || CurrentScope is BlockLexicalScope);

            LocalVariable variable;
            if (CurrentScope.TryGetValue(name, out variable)) {
                if (name != "_") {
                    _tokenizer.ReportError(Errors.DuplicateParameterName);
                }
                return variable;
            }

            return CurrentScope.AddVariable(name, location);
        }

        private FileInitializerStatement/*!*/ AddInitializer(FileInitializerStatement/*!*/ initializer) {
            if (_initializers == null) {
                _initializers = new List<FileInitializerStatement>();
            }

            _initializers.Add(initializer);

            return initializer;
        }
        
        private SourceSpan MergeLocations(SourceSpan start, SourceSpan end) {
            Debug.Assert(start.IsValid && end.IsValid);

            return new SourceSpan(start.Start, end.End);
        }

        private LexicalScope/*!*/ EnterScope(LexicalScope/*!*/ scope) {
            Assert.NotNull(scope);
            _lexicalScopes.Push(scope);
            return scope;
        }

        /// <summary>
        /// Block scope.
        /// </summary>
        private LexicalScope/*!*/ EnterNestedScope() {
            LexicalScope result = new BlockLexicalScope(CurrentScope);
            _lexicalScopes.Push(result);
            return result;
        }

        /// <summary>
        /// for-loop scope.
        /// </summary>
        private LexicalScope/*!*/ EnterPaddingScope() {
            LexicalScope result = new PaddingLexicalScope(CurrentScope);
            _lexicalScopes.Push(result);
            return result;
        }

        /// <summary>
        /// BEGIN block.
        /// </summary>
        private LexicalScope/*!*/ EnterFileInitializerScope() {
            LexicalScope result = new TopStaticLexicalScope(null);
            _lexicalScopes.Push(result);
            return result;
        }

        /// <summary>
        /// Source unit scope.
        /// </summary>
        private LexicalScope/*!*/ EnterTopStaticScope() {
            LexicalScope result = new TopStaticLexicalScope(CurrentScope);
            _lexicalScopes.Push(result);
            return result;
        }

        private LexicalScope/*!*/ EnterModuleDefinitionScope() {
            LexicalScope result = new TopLocalDefinitionLexicalScope(CurrentScope);
            _lexicalScopes.Push(result);
            return result;
        }

        private LexicalScope/*!*/ EnterClassDefinitionScope() {
            LexicalScope result = new ClassLexicalScope(CurrentScope);
            _lexicalScopes.Push(result);
            return result;
        }

        private LexicalScope/*!*/ EnterSingletonClassDefinitionScope() {
            LexicalScope result = new TopLocalDefinitionLexicalScope(CurrentScope);
            _lexicalScopes.Push(result);
            return result;
        }

        private LexicalScope/*!*/ EnterMethodDefinitionScope() {
            LexicalScope result = new MethodLexicalScope(CurrentScope);
            _lexicalScopes.Push(result);
            return result;
        }

        private LexicalScope/*!*/ EnterSingletonMethodDefinitionScope() {
            LexicalScope result = new TopLocalDefinitionLexicalScope(CurrentScope);
            _lexicalScopes.Push(result);
            return result;
        }

        private LexicalScope LeaveScope() {
            return _lexicalScopes.Pop();
        }

        public LexicalScope CurrentScope {
            get {
                Debug.Assert(_lexicalScopes.Count > 0);
                return _lexicalScopes.Peek();
            }
        }

        public bool IsLocalVariable(string/*!*/ identifier) {
            return CurrentScope.ResolveVariable(identifier) != null;
        }

        private Expression/*!*/ ToCondition(Expression/*!*/ expression) {            
            return expression.ToCondition(CurrentScope);
        }

        private Body/*!*/ MakeBody(Statements/*!*/ statements, List<RescueClause> rescueClauses, ElseIfClause elseIf,
            SourceSpan elseIfLocation, Statements ensureStatements, SourceSpan location) {
            Debug.Assert(elseIf == null || elseIf.Condition == null);

            if (elseIf != null && rescueClauses == null) {
                ErrorSink.Add(_sourceUnit, "else without rescue is useless", elseIfLocation, -1, Severity.Warning);
            }

            return new Body(
                statements,
                rescueClauses,
                (elseIf != null) ? elseIf.Statements : null,
                ensureStatements,
                location
            );
        }

        internal LeftValue/*!*/ CannotAssignError(string/*!*/ constantName, SourceSpan location) {
            Tokenizer.ReportError(Errors.CannotAssignTo, constantName);
            return CurrentScope.ResolveOrAddVariable(GenerateErrorLocalName(), location);
        }

        private void MatchReferenceReadOnlyError(RegexMatchReference/*!*/ matchRef) {
            Tokenizer.ReportError(Errors.MatchGroupReferenceReadOnly, matchRef.VariableName);
        }

        private AliasStatement/*!*/ MakeGlobalAlias(string/*!*/ newVar, string/*!*/ existingVar, SourceSpan location) {
            return new AliasStatement(false, newVar, existingVar, location);
        }

        private Expression/*!*/ MakeGlobalAlias(string/*!*/ newVar, RegexMatchReference/*!*/ existingVar, SourceSpan location) {
            if (existingVar.CanAlias) {
                return new AliasStatement(false, newVar, existingVar.VariableName, location);
            } else {
                _tokenizer.ReportError(Errors.CannotAliasGroupMatchVariable);
                return new ErrorExpression(location);
            }
        }

        private AliasStatement/*!*/ MakeGlobalAlias(RegexMatchReference/*!*/ newVar, string/*!*/ existingVar, SourceSpan location) {
            return new AliasStatement(false, newVar.VariableName, existingVar, location);
        }

        private Expression/*!*/ MakeGlobalAlias(RegexMatchReference/*!*/ newVar, RegexMatchReference/*!*/ existingVar, SourceSpan location) {
            if (existingVar.CanAlias) {
                return new AliasStatement(false, newVar.VariableName, existingVar.VariableName, location);
            } else {
                _tokenizer.ReportError(Errors.CannotAliasGroupMatchVariable);
                return new ErrorExpression(location);
            }
        }

        private List<T>/*!*/ MakeListAddOpt<T>(T item) {
            List<T> result = new List<T>();
            if (item != null) {
                result.Add(item);
            }
            return result;
        }

        // BlockExpression behaves like an expression, so we don't need to create one that comprises of a single expression:
        private Expression/*!*/ MakeBlockExpression(Statements/*!*/ statements, SourceSpan location) {
            if (statements.Count == 0) {
                return BlockExpression.Empty;
            } else if (statements.Count == 1) {
                return statements.First;
            } else {
                return new BlockExpression(statements, location);
            }
        }

        private ForLoopExpression/*!*/ MakeForLoopExpression(CompoundLeftValue/*!*/ lvalue, Expression/*!*/ list, Statements/*!*/ body, SourceSpan location) {
            // MRI allows this
            // CheckForLoopVariables(lvalue.LeftValues);

            Parameters parameters;
            if (lvalue.HasUnsplattedValue) {
                parameters = new Parameters(
                    ArrayUtils.RemoveAt(lvalue.LeftValues, lvalue.UnsplattedValueIndex),
                    lvalue.UnsplattedValueIndex, 
                    null,
                    lvalue.UnsplattedValue, 
                    null, 
                    SourceSpan.None
                );
            } else {
                parameters = new Parameters(lvalue.LeftValues, lvalue.LeftValues.Length, null, null, null, SourceSpan.None);
            }

            return new ForLoopExpression(CurrentScope, parameters, list, body, location);
        }
#if TODO // ?
        private bool CheckForLoopVariables(LeftValue/*!*/[]/*!*/ lvalues) {
            for (int i = 0; i < lvalues.Length; i++) {
                switch (lvalues[i].NodeType) {
                    case NodeTypes.LocalVariable:
                    case NodeTypes.CompoundLeftValue:
                    case NodeTypes.Placeholder:
                        break;

                    case NodeTypes.ConstantVariable:
                        _tokenizer.ReportError(Errors.ForLoopVariableIsConstantVariable, lvalues[i].Location);
                        return false;

                    case NodeTypes.InstanceVariable:
                        _tokenizer.ReportError(Errors.ForLoopVariableIsInstanceVariable, lvalues[i].Location);
                        return false;

                    case NodeTypes.GlobalVariable:
                        _tokenizer.ReportError(Errors.ForLoopVariableIsGlobalVariable, lvalues[i].Location);
                        return false;

                    case NodeTypes.ClassVariable:
                        _tokenizer.ReportError(Errors.ForLoopVariableIsClassVariable, lvalues[i].Location);
                        return false;

                    default:
                        throw Assert.Unreachable;
                }
            }
            return true;
        }
#endif
        private IfExpression/*!*/ MakeIfExpression(Expression/*!*/ condition, Statements/*!*/ body, List<ElseIfClause>/*!*/ elseIfClauses, SourceSpan location) {
            // last else-if/else clause is the first one in the list:            
            elseIfClauses.Reverse();
            return new IfExpression(condition, body, elseIfClauses, location);
        }

        private ArrayConstructor/*!*/ MakeVerbatimWords(List<Expression>/*!*/ words, SourceSpan location) {
            Debug.Assert(CollectionUtils.TrueForAll(words, (word) => word is StringLiteral), "all words are string literals");

            return new ArrayConstructor(new Arguments(words.ToArray()), location);
        }

        private Expression/*!*/[]/*!*/ PopHashArguments(int argumentCount, SourceSpan location) {
            if (argumentCount % 2 != 0) {
                ErrorSink.Add(_sourceUnit, "odd number list for Hash", location, -1, Severity.Error);
                return PopArguments(argumentCount, Literal.Nil(SourceSpan.None));
            } else {
                return PopArguments(argumentCount);
            }
        }

        private Arguments/*!*/ RequireNoBlockArg(TokenValue arguments) {
            if (arguments.Block != null) {
                ErrorSink.Add(_sourceUnit, "block argument should not be given", arguments.Block.Location, -1, Severity.Error);
                arguments.Block = null;
            }

            return arguments.Arguments;
        }

        private static MethodCall/*!*/ MakeMethodCall(Expression target, string/*!*/ methodName, TokenValue args, SourceSpan location) {
            return new MethodCall(target, methodName, args.Arguments, args.Block, location);
        }

        private static MethodCall/*!*/ MakeMethodCall(Expression target, string/*!*/ methodName, TokenValue args, Block block, SourceSpan location) {
            Debug.Assert(args.Block == null);
            return new MethodCall(target, methodName, args.Arguments, block, location);
        }

        private static ArrayItemAccess/*!*/ MakeArrayItemAccess(Expression/*!*/ array, TokenValue args, SourceSpan location) {
            return new ArrayItemAccess(array, args.Arguments, args.Block, location);
        }

        private static Expression/*!*/ MakeMatch(Expression/*!*/ left, Expression/*!*/ right, SourceSpan location) {
            var regex = left as RegularExpression;
            if (regex != null) {
                return new MatchExpression(regex, right, location);
            } else {
                return new MethodCall(left, Symbols.Match, new Arguments(right), location);
            }
        }

        private static SuperCall/*!*/ MakeSuperCall(TokenValue args, SourceSpan location) {
            return new SuperCall(args.Arguments, args.Block, location);
        }

        private static CompoundLeftValue/*!*/ MakeCompoundLeftValue(List<LeftValue> leading, LeftValue/*!*/ unsplat, List<LeftValue> trailing) {
            int leadingCount = (leading != null ? leading.Count : 0);
            int trailingCount = (trailing != null ? trailing.Count : 0);
            var array = new LeftValue[leadingCount + 1 + trailingCount];
            if (leadingCount > 0) {
                leading.CopyTo(array, 0);
            }
            array[leadingCount] = unsplat;
            if (trailingCount > 0) {
                trailing.CopyTo(array, leadingCount + 1);
            }
            return new CompoundLeftValue(array, leadingCount);
        }

        private static T[]/*!*/ MakeArray<T>(List<T>/*!*/ left, List<T>/*!*/ right) {
            var array = new T[left.Count + right.Count];
            left.CopyTo(array, 0);
            right.CopyTo(array, left.Count);
            return array;
        }

        private static T[]/*!*/ MakeArray<T>(List<T>/*!*/ left, T/*!*/ right) {
            var array = new T[left.Count + 1];
            left.CopyTo(array, 0);
            array[array.Length - 1] = right;
            return array;
        }

        private Expression[]/*!*/ _argumentStack = new Expression[10];
        private int _argumentCount;

        private void PushArgument(int argumentCount, Expression/*!*/ argument) {
            Assert.NotNull(argument);
            if (_argumentCount == _argumentStack.Length) {
                Array.Resize(ref _argumentStack, _argumentCount * 2);
            }
            _argumentStack[_argumentCount++] = argument;
            yyval.ArgumentCount = argumentCount + 1;
        }

        private Expression/*!*/[]/*!*/ PopArguments(int count) {
            var result = new Expression[count];
            _argumentCount -= count;
            Array.Copy(_argumentStack, _argumentCount, result, 0, count);
            return result;
        }

        private Expression/*!*/[]/*!*/ PopArguments(int count, Expression/*!*/ additionalArgument) {
            var result = new Expression[count + 1];
            _argumentCount -= count;
            Array.Copy(_argumentStack, _argumentCount, result, 0, count);
            result[count] = additionalArgument;
            return result;
        }

        private Expression/*!*/[]/*!*/ PopArguments(Expression/*!*/ additionalArgument, int count) {
            var result = new Expression[count + 1];
            _argumentCount -= count;
            Array.Copy(_argumentStack, _argumentCount, result, 1, count);
            result[0] = additionalArgument;
            return result;
        }

        // foo 
        // foo {}
        private void SetNoArguments(Block block) {
            yyval.Arguments = null;
            yyval.Block = block;
        }

        // foo()
        private void SetArguments() {
            yyval.Arguments = Arguments.Empty;
            yyval.Block = null;
        }
        
        // foo()
        // foo() {}
        // foo(&p)
        // foo &p
        private void SetArguments(Block block) {
            yyval.Arguments = Arguments.Empty;
            yyval.Block = block;
        }

        // foo(expr)
        private void SetArguments(Expression/*!*/ expression) {
            yyval.Arguments = new Arguments(expression);
            yyval.Block = null;
        }

        // foo(expr)
        // foo(expr) {}
        // foo(expr, &p)
        private void SetArguments(Expression/*!*/ expression, Block block) {
            yyval.Arguments = new Arguments(expression);
            yyval.Block = block;
        }

        // foo([*]?expr1, ..., [*]?exprN, k1 => v1, ..., kN => vN)
        // foo([*]?expr1, ..., [*]?exprN, k1 => v1, ..., kN => vN) {}
        // foo([*]?expr1, ..., [*]?exprN, k1 => v1, ..., kN => vN, &p)
        private void PopAndSetArguments(int argumentCount, Block block) {
            yyval.Arguments = new Arguments(PopArguments(argumentCount));
            yyval.Block = block;
        }

        private void SetArguments(Expression/*!*/[] arguments, Block block) {
            yyval.Arguments = new Arguments(arguments);
            yyval.Block = block;
        }

        private void SetBlock(CallExpression/*!*/ expression, BlockDefinition/*!*/ block) {
            if (expression.Block != null) {
                ReportSyntaxError(Errors.BothBlockDefAndBlockRefGiven);
            } else {
                expression.Block = block;
            }
        }

        private LambdaDefinition/*!*/ MakeLambdaDefinition(Parameters parameters, Statements body, SourceSpan location) {
            return new LambdaDefinition(new BlockDefinition(CurrentScope, parameters, body, location));
        }

        private BlockDefinition/*!*/ MakeBlockDefinition(Parameters parameters, Statements body, SourceSpan location) {
            return new BlockDefinition(CurrentScope, parameters, body, location);
        }

        private static Expression/*!*/ MakeLoopStatement(Expression/*!*/ statement, Expression/*!*/ condition, bool isWhileLoop, SourceSpan location) {
            return new WhileLoopExpression(condition, isWhileLoop, statement is Body, new Statements(statement), location);
        }

        public static string/*!*/ GetTerminalName(int terminal) {
            return RubyUtils.TryMangleName(((Tokens)terminal).ToString()).ToUpperInvariant();
        }

        public static StringLiteral/*!*/ MakeStringLiteral(TokenValue token, SourceSpan location) {
            return new StringLiteral(token.StringContent, token.Encoding, location);
        }

        public SymbolLiteral/*!*/ MakeSymbolLiteral(string/*!*/ symbol, SourceSpan location) {
            return new SymbolLiteral(symbol, symbol.IsAscii() ? RubyEncoding.Ascii : Encoding, location);
        }

        private StringConstructor/*!*/ MakeSymbolConstructor(List<Expression>/*!*/ content, SourceSpan location) {
            if (content.Count == 0 && _tokenizer.Compatibility < RubyCompatibility.Ruby19) {
                _tokenizer.ReportError(Errors.EmptySymbolLiteral);
            }
            return new StringConstructor(content, StringKind.Symbol, location);
        }

        // TODO: utils

        public static string/*!*/ CharToString(char ch) {
            switch (ch) {
                case '\a': return @"'\a'";
                case '\b': return @"'\b'";
                case '\f': return @"'\f'";
                case '\n': return @"'\n'";
                case '\r': return @"'\r'";
                case '\t': return @"'\t'";
                case '\v': return @"'\v'";
                case '\0': return @"'\0'";
                default: return String.Concat("'", ch.ToString(), "'");
            }
        }

        public static string/*!*/ EscapeString(string str) {
            return (str == null) ? String.Empty :
                new StringBuilder(str).
                Replace(@"\", @"\\").
                Replace(@"""", @"\""").
                Replace("\a", @"\a").
                Replace("\b", @"\b").
                Replace("\f", @"\f").
                Replace("\n", @"\n").
                Replace("\r", @"\r").
                Replace("\t", @"\t").
                Replace("\v", @"\v").
                Replace("\0", @"\0").ToString();
        }
    }
}


