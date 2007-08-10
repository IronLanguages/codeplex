#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

import generate
reload(generate)
from generate import CodeGenerator
import operator

kwlist = ['and', 'assert', 'break', 'class', 'continue', 'def', 'del', 'elif', 'else', 'except', 'exec', 'finally', 'for', 'from', 'global', 'if', 'import', 'in', 'is', 'lambda', 'not', 'or', 'pass', 'print', 'raise', 'return', 'try', 'while', 'yield']

class Symbol:
    def __init__(self, symbol, name, titleName = None):
        self.symbol = symbol
        self.name = name
        self.titleName = titleName

    def cap_name(self):
        return self.name.title().replace(' ', '')

    def upper_name(self):
        return self.name.upper().replace(' ', '_')

    def title_name(self):
        if not self.titleName: return self.name.title().replace(' ', '')
        return self.titleName

    def simple_name(self):
        return self.name[0] + self.cap_name()[1:]

    def symbol_name(self):
        return "Operator" + self.title_name()

    def reverse_symbol_name(self):
        return "OperatorReverse" + self.title_name()

    def inplace_symbol_name(self):
        return "OperatorInPlace" + self.title_name()

    def __repr__(self):
        return 'Symbol(%s)' % self.symbol

class Operator(Symbol):
    def __init__(self, symbol, name, rname, clrName=None,prec=-1, opposite=None, bool1=None, bool2=None, bool3=None, dotnetOp=False):
        Symbol.__init__(self, symbol, name)
        self.rname = rname
        self.clrName = clrName
        self.dotnetOp = dotnetOp
        self.meth_name = "__" + name + "__"
        self.rmeth_name = "__" + rname + "__"
        if name in kwlist:
            name = name + "_"
        #self.op = getattr(operator, name)
        self.prec = prec
        self.opposite = opposite
        self.bool1 = bool1
        self.bool2 = bool2
        self.bool3 = bool3

    def clrInPlaceName(self):
        return "InPlace" + self.clrName

    def title_name(self):
        return self.clrName

    def isCompare(self):
        return self.prec == -1

    def __repr__(self):
        return 'Operator(%s,%s,%s)' % (self.symbol, self.name, self.rname)

    def generate_binop(self, cw):
        if self.isCompare():
            cw.write(CMPOP, name=self.clrName)
        else:
            cw.write(BINOP, name=self.clrName)
    
    def genOperatorTable_Mapping(self, cw):
        cw.writeline("pyOp[Symbols.Operator%s] = new OperatorMapping(Operators.%s, false, true, false, true);" % (self.title_name(), self.title_name()))        
        
        if self.isCompare(): return

        cw.writeline("pyOp[Symbols.OperatorReverse%s] = new OperatorMapping(Operators.Reverse%s, false, true, false, true);" % (self.title_name(), self.title_name()))        
        cw.writeline("pyOp[Symbols.OperatorInPlace%s] = new OperatorMapping(Operators.InPlace%s, false, true, false);" % (self.title_name(), self.title_name()))

    def genOperatorTable_Normal(self, cw):
        cw.writeline("///<summary>Operator for performing %s</summary>" % self.name)
        cw.writeline("%s," % (self.title_name()))        
        
    def genOperatorTable_Reverse(self, cw):
        if self.isCompare(): return
        
        cw.writeline("///<summary>Operator for performing reverse %s</summary>" % self.name)
        cw.writeline("Reverse%s," % (self.title_name()))

    def genOperatorTable_InPlace(self, cw):
        if self.isCompare(): return
        
        cw.writeline("///<summary>Operator for performing in-place %s</summary>" % self.name)
        cw.writeline("InPlace%s," % (self.title_name()))
    
    def genOperatorToSymbol(self, cw): 
        cw.writeline("case Operators.%s: return Symbols.Operator%s;" % (self.title_name(), self.title_name()))
        
        if self.isCompare(): return
        
        cw.writeline("case Operators.Reverse%s: return Symbols.OperatorReverse%s;" % (self.title_name(), self.title_name()))
        cw.writeline("case Operators.InPlace%s: return Symbols.OperatorInPlace%s;" % (self.title_name(), self.title_name()))

    def genOldStyleOp(self, cw):
        if self.isCompare(): return

        cw.writeline('[return: MaybeNotImplemented]')
        if self.dotnetOp:
            cw.enter_block("public static object operator %s([NotNull]OldInstance self, object other)" % self.symbol)            
        else:
            cw.writeline('[SpecialName]')
            cw.enter_block("public static object %s([NotNull]OldInstance self, object other)" % self.title_name())
        cw.writeline("object value;")
        cw.writeline()
        cw.enter_block("if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.Operator%s, out value))" % self.title_name())
        cw.writeline("object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);")
        cw.writeline("if(res != PythonOps.NotImplemented) return res;")
        cw.exit_block()	# end of TryGetBoundCustomMember

        cw.writeline()
        cw.writeline("OldInstance otherOc = other as OldInstance;")
        cw.enter_block("if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverse%s, out value))" % self.title_name())
        cw.writeline("return PythonOps.CallWithContext(DefaultContext.Default, value, self);")
        cw.exit_block() # end of PythonOps.TryGetBoundCustomMember
        
        cw.writeline("return PythonOps.NotImplemented;")
        cw.exit_block() # end method
        cw.writeline()
        
        cw.writeline('[return: MaybeNotImplemented]')
        if self.dotnetOp:
            cw.enter_block("public static object operator %s(object other, [NotNull]OldInstance self)" % self.symbol)            
        else:
            cw.writeline('[SpecialName]')
            cw.enter_block("public static object %s(object other, [NotNull]OldInstance self)" % self.title_name())
        cw.writeline("object value;")
        cw.writeline()
        cw.enter_block("if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverse%s, out value))" % self.title_name())
        cw.writeline("object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);")
        cw.writeline("if (res != PythonOps.NotImplemented) return res;")
        cw.exit_block()	# end of TryGetBoundCustomMember
        cw.writeline("return PythonOps.NotImplemented;")
        cw.exit_block() # end method
        cw.writeline()
        
        cw.writeline('[return: MaybeNotImplemented]')
        cw.writeline('[SpecialName, PythonName("__i%s__")]' % self.name)
        cw.enter_block("public object InPlace%s(object other)" % self.title_name())
        cw.writeline("object value;")
        cw.writeline()
        cw.enter_block("if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlace%s, out value))" % self.title_name())
        cw.writeline("object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);")
        cw.writeline("if (res != PythonOps.NotImplemented) return res;")
        cw.exit_block()	# end of TryGetBoundCustomMember
        cw.writeline("return PythonOps.NotImplemented;")
        cw.exit_block() # end method
        cw.writeline()

    def genSymbolTableSymbols(self, cw):
        def gen_one_symbol(titleName, name, op):
            cw.writeline("private static SymbolId _Operator%s;" % (titleName,))
            
            cw.writeline("///<summary>SymbolId for '%s'</summary>" % (op, ))
            cw.enter_block("public static SymbolId Operator%s" % (titleName, ))
            cw.enter_block('get')
            cw.writeline("if (_Operator%s == SymbolId.Empty) _Operator%s = MakeSymbolId(\"%s\");" % (titleName, titleName, op))
            cw.writeline("return _Operator%s;" % (titleName,))
            cw.exit_block()
            cw.exit_block()

        gen_one_symbol(self.title_name(), self.name, '__' + self.name + '__')
        
        if self.isCompare(): return
        
        gen_one_symbol('Reverse' + self.title_name(), self.rname, '__' + self.rname + '__')
        gen_one_symbol('InPlace' + self.title_name(), self.name, '__i' + self.name + '__')

    def genWeakRefOperatorNames(self, cw):
        cw.writeline("AddWrapperOperator(Symbols.%s, res);" % (self.symbol_name()))
        
        if self.isCompare(): return

        cw.writeline("AddWrapperOperator(Symbols.%s, res);" % (self.reverse_symbol_name()))
        cw.writeline("AddWrapperOperator(Symbols.%s, res);" % (self.inplace_symbol_name()))

class Grouping(Symbol):
    def __init__(self, symbol, name, side, titleName=None):
        Symbol.__init__(self, symbol, side+" "+name, titleName)
        self.base_name = name
        self.side = side

ops = []

"""
0 expr: xor_expr ('|' xor_expr)*
1 xor_expr: and_expr ('^' and_expr)*
2 and_expr: shift_expr ('&' shift_expr)*
3 shift_expr: arith_expr (('<<'|'>>') arith_expr)*
4 arith_expr: term (('+'|'-') term)*
5 term: factor (('*'|'/'|'%'|'//') factor)*
"""
            # op,  pyname,   prec, .NET name, .NET op      op,    pyname,   prec, .NET name,  .NET op overload
binaries = [('+',  'add',      4, 'Add',         True),   ('-',  'sub',    4, 'Subtract',   True), 
            ('**', 'pow',      6, 'Power',       False),  ('*',  'mul',    5, 'Multiply',   True),
            ('//', 'floordiv', 5, 'FloorDivide', False),  ('/',  'div',    5, 'Divide',     True), 
            ('/',  'truediv',  5, 'TrueDivide',  False),  ('%',  'mod',    5, 'Mod',        True), 
            ('<<', 'lshift',   3, 'LeftShift',   False),   ('>>', 'rshift', 3, 'RightShift', False),
            ('&',  'and',      2, 'BitwiseAnd',  True),   ('|',  'or',     0, 'BitwiseOr',  True), 
            ('^',  'xor',      1, 'Xor',         True)]

def add_binaries(list):
    for sym, name, prec,clrName, netOp in list:
        ops.append(Operator(sym, name, 'r'+name, clrName, prec, dotnetOp = netOp))
        ops.append(Symbol(sym+"=", name+" Equal", clrName+ "Equal"))

add_binaries(binaries)

compares = [('<', 'lt', 'ge', 'LessThan', 'GreaterThan', "false", "true", "false"), 
            ('>', 'gt', 'le', 'GreaterThan', 'LessThan', "false", "false", "true"),
            ('<=', 'le', 'gt', 'LessThanOrEqual', 'GreaterThanOrEqual', "true", "true", "false"), 
            ('>=', 'ge', 'lt', 'GreaterThanOrEqual', 'LessThanOrEqual', "true", "false", "true"),
            ('==', 'eq', 'eq', 'Equal', 'NotEqual', 'a', 'a', 'a'), 
            ('!=', 'ne', 'ne', 'NotEqual', 'Equal', 'a', 'a', 'a'), 
            ('<>', 'lg', 'lg', 'LessThanGreaterThan', 'Equal', 'a', 'a', 'a')]
for sym, name, rname,clrName,opposite, bool1, bool2, bool3 in compares:
    ops.append(Operator(sym, name, rname,clrName, opposite=opposite, bool1=bool1, bool2=bool2, bool3=bool3))

groupings = [('(', ')', 'Paren', 'Parenthesis'), ('[', ']', 'Bracket', 'Bracket'), ('{', '}', 'Brace', 'Brace')]
for sym, rsym, name, fullName in groupings:
    ops.append(Grouping(sym, name, 'l', 'Left' + fullName))
    ops.append(Grouping(rsym, name, 'r', 'Right' + fullName))

simple = [(',', 'comma'), (':', 'colon'), ('`', 'backquote', 'BackQuote'), (';', 'semicolon'),
          ('=', 'assign'), ('~', 'twiddle'), ('@', 'at')]
for info in simple:
    if len(info) == 2:
        sym, name = info
        title = None
    else:
        sym, name, title = info
    
    ops.append(Symbol(sym, name, title))

start_symbols = {}
for op in ops:
    ss = op.symbol[0]
    if not start_symbols.has_key(ss): start_symbols[ss] = []
    start_symbols[ss].append(op)


def gen_tests(ops, pos, indent=1):
    ret = []

    default_match = []
    future_matches = {}
    for sop in ops:
        if len(sop.symbol) == pos:
            default_match.append(sop)
        elif len(sop.symbol) > pos:
            ch = sop.symbol[pos]
            if future_matches.has_key(ch):
                future_matches[ch].append(sop)
            else:
                future_matches[ch] = [sop]
    #assert len(default_match) <= 1
    for ch, sops in future_matches.items():
        ret.append("if (NextChar('%s')) {" % ch)
        ret.extend(gen_tests(sops, pos+1))
        ret.append("}")
    if default_match:
        op = default_match[0]
        if isinstance(op, Grouping):
            if op.side == 'l':
                ret.append("_state.%sLevel++;" % op.base_name);
            else:
                ret.append("_state.%sLevel--;" % op.base_name);
        ret.append("return Tokens.%sToken;" %
                   op.title_name())
    else:
        ret.append("return BadChar(ch);")

    return ["    "*indent + l for l in ret]

def tokenize_generator(cw):
    ret = []
    done = {}
    for op in ops:
        ch = op.symbol[0]
        if done.has_key(ch): continue
        sops = start_symbols[ch]
        cw.write("case '%s':" % ch)
        for t in gen_tests(sops, 1):
            cw.write(t)
        done[ch] = True
    return ret


CodeGenerator("Tokenize Ops", tokenize_generator).doit()

friendlyOverload = {'elif':"ElseIf"}
def keywordToFriendly(kw):
    if friendlyOverload.has_key(kw):
        return friendlyOverload[kw]
    
    return kw.title()

class unique_checker:
    def __init__(self):
        self.__unique = {}
    def unique(self, op):
        if not op.symbol in self.__unique:
            self.__unique[op.symbol] = op
            return True
        else: return False

def tokenkinds_generator(cw):
    i = 32
    uc = unique_checker()
    for op in ops:
        if not uc.unique(op): continue
        cw.write("%s = %d," % (op.title_name(), i))
        i += 1

    cw.writeline()
    keyword_list = list(kwlist)
    keyword_list.sort()
    
    cw.write("FirstKeyword = Keyword%s," % keywordToFriendly(keyword_list[0]));
    cw.write("LastKeyword = Keyword%s," % keywordToFriendly(keyword_list[len(keyword_list) - 1]));
    
    for kw in keyword_list:
        cw.write("Keyword%s = %d," % (keywordToFriendly(kw), i))
        i += 1

CodeGenerator("Token Kinds", tokenkinds_generator).doit()

def tokens_generator(cw):
    uc = unique_checker()
    for op in ops:
        if not uc.unique(op): continue

        if isinstance(op, Operator) and op.name != "lg":
            creator = 'new OperatorToken(TokenKind.%s, "%s", %d)' % (
                op.title_name(), op.symbol, op.prec)
        else:
            creator = 'new SymbolToken(TokenKind.%s, "%s")' % (
                op.title_name(), op.symbol)
        cw.write("private static readonly Token sym%sToken = %s;" %
                   (op.title_name(), creator))
                       

    cw.writeline()
    
    uc = unique_checker()
    for op in ops:
        if not uc.unique(op): continue

        cw.enter_block("public static Token %sToken" % op.title_name())
        cw.write("get { return sym%sToken; }" % op.title_name())
        cw.exit_block()
        cw.write("")
    
    keyword_list = list(kwlist)
    keyword_list.sort()

    dict_init = []

    for kw in keyword_list:
        creator = 'new SymbolToken(TokenKind.Keyword%s, "%s")' % (
            keywordToFriendly(kw), kw)
        cw.write("private static readonly Token kw%sToken = %s;" %
               (keywordToFriendly(kw), creator))

        dict_init.append("Keywords[SymbolTable.StringToId(\"%s\")] = kw%sToken;" %
                         (kw, keywordToFriendly(kw)))

    cw.write("")
    cw.write("")
    for kw in keyword_list:
        cw.enter_block("public static Token Keyword%sToken" % keywordToFriendly(kw))
        cw.write("get { return kw%sToken; }" % keywordToFriendly(kw))
        cw.exit_block()
        cw.write("")
        
    cw.writeline()
    cw.write("private static readonly Dictionary<SymbolId, Token> kws = new Dictionary<SymbolId, Token>();");
    cw.writeline()
    cw.enter_block("public static IDictionary<SymbolId, Token> Keywords")
    cw.write("get { return kws; }")
    cw.exit_block()

    cw.enter_block("static Tokens()")
    for l in dict_init: cw.write(l)
    cw.exit_block()

CodeGenerator("Tokens", tokens_generator).doit()

IBINOP = """
private static FastDynamicSite<object, object, object> %(name)sSharedSite =
    FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.%(name)s));

public static object %(name)s(object x, object y) {
    return %(name)sSharedSite.Invoke(x, y);
}"""

BINOP = IBINOP

CMPOP2 = """
private static FastDynamicSite<object, object, bool> %(name)sBooleanSharedSite =
    FastDynamicSite<object, object, bool>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.%(name)s));

public static bool %(name)sRetBool(object x, object y) {
    return %(name)sBooleanSharedSite.Invoke(x, y);
}"""

CMPOP = BINOP + CMPOP2

UBINOP = """
public static object %(name)s(object x, object y) {
    throw new NotImplementedException("%(name)s");
}
"""
ADD_EXTRA_VARS = """        string sx, sy;
        ExtensibleString es = null;
"""

ADD_EXTRA_EARLY = """       } else if ((sx = x as string) != null && ((sy = y as string) != null || (es = y as ExtensibleString) != null)) {
            if (sy != null) return sx + sy;
            return sx + es.Value;"""

ADD_EXTRA = """                
        ISequence seq = x as ISequence;
        if (seq != null) { return seq.AddSequence(y); }
"""

INPLACE_ADD_EXTRA = """
        if (x is string && y is string) {
            return ((string)x) + ((string)y);
        }
    
        if (x is ReflectedEvent.EventTarget) {
            return ((ReflectedEvent.EventTarget)x).InPlaceAdd(y);
        }
"""


MUL_EXTRA = """
        if (x is ISequence) {
            return ((ISequence)x).MultiplySequence(y);
        } else if (y is ISequence) {
            return ((ISequence)y).MultiplySequence(x);
        }
"""

def python_sites_generator(cw):
    for op in ops:
        if not isinstance(op, Operator): continue
        if op.symbol in ["<>", "=="]: continue
        op.generate_binop(cw)

CodeGenerator("Python Sites", python_sites_generator).doit()

def gen_SymbolTable_ops_symbols(cw):
    for op in ops:
        if not isinstance(op, Operator): continue
        op.genSymbolTableSymbols(cw)
        
CodeGenerator("Symbols - Ops Symbols", gen_SymbolTable_ops_symbols).doit()

def gen_OperatorTable(cw):
    for op in ops:
        if not isinstance(op, Operator): continue
        op.genOperatorTable_Normal(cw)
    for op in ops:
        if not isinstance(op, Operator): continue
        op.genOperatorTable_InPlace(cw)
    for op in ops:
        if not isinstance(op, Operator): continue
        op.genOperatorTable_Reverse(cw)
        
CodeGenerator("Table of Operators", gen_OperatorTable).doit()

def gen_operatorMapping(cw):
    for op in ops:
        if isinstance(op, Operator): op.genOperatorTable_Mapping(cw)

CodeGenerator("PythonOperator Mapping", gen_operatorMapping).doit()

def gen_OperatorToSymbol(cw):
    for op in ops:
        if not isinstance(op, Operator): continue
        op.genOperatorToSymbol(cw)
        
CodeGenerator("OperatorToSymbol", gen_OperatorToSymbol).doit()

def weakref_operators(cw):
    for op in ops:
        if not isinstance(op, Operator): continue
        op.genWeakRefOperatorNames(cw)

CodeGenerator("WeakRef Operators Initialization", weakref_operators).doit()

def oldinstance_operators(cw):
    for op in ops:
        if not isinstance(op, Operator): continue
        op.genOldStyleOp(cw)

CodeGenerator("OldInstance Operators", oldinstance_operators).doit()
