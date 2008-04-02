#####################################################################################
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public
# License. A  copy of the license can be found in the License.html file at the
# root of this distribution. If  you cannot locate the  Microsoft Public
# License, please send an email to  dlr@microsoft.com. By using this source
# code in any fashion, you are agreeing to be bound by the terms of the 
# Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#####################################################################################

import generate
import System

COMPRESS = True
ast_walker = "IAstWalker"

class AstNode:
    def __init__(self, t, p):
        self.t = t
        self.p = p
        self.c = []
    def Walk(self, m, d):
        m(self, d)
        for c in self.c:
            c.Walk(m, d + 1)
    def Get(self, t):
        for c in self.c:
            if c.t == t:
                return c
        else:
            c = AstNode(t, self)
            self.c.append(c)
            return c

class Ast:
    def __init__(self):
        self.root = None

    def Add(self, t):
        if not t:
            return None
        elif t.FullName == "IronPython.Compiler.Ast.Node":
            if self.root == None:
                self.root = AstNode(t, None)
            return self.root
        else:
            parent = self.Add(t.BaseType)
            if parent != None:
                me = parent.Get(t)
                return me

    def Walk(self, m):
        self.root.Walk(m, 0)

def get_ast():
    asm = [].GetType().Assembly

    ast = Ast()

    for t in asm.GetExportedTypes():
        if t.FullName.StartsWith("IronPython.Compiler.Ast"):
            ast.Add(t)

    return ast

def inherits(n, f):
    if not n:
        return False
    elif n.t.FullName == f:
        return True
    else:
        return inherits(n.p, f)

def isexpr(n):
    return inherits(n, "IronPython.Compiler.Ast.Expression")
def isstmt(n):
    return inherits(n, "IronPython.Compiler.Ast.Statement")

def compare(a, b):
    if isexpr(a):
        if isexpr(b):
            return cmp(a.t.FullName, b.t.FullName)
        else:
            return -1
    elif isstmt(a):
        if isexpr(b):
            return 1
        elif isstmt(b):
            return cmp(a.t.FullName, b.t.FullName)
        else:
            return -1
    else:
        if isexpr(b) or isstmt(b):
            return 1
        else:
            return cmp(a.t.FullName, b.t.FullName)

class Leaves:
    leaves = []
    def __call__(self, x, d):
        if (len(x.c) == 0):
            self.leaves.append(x)

def get_sorted_leaves(ast):
    l = Leaves()
    ast.Walk(l)
    l.leaves.sort(compare)
    return l.leaves


def gen_interface(cw, l):
    space = False
    cw.write("/// <summary>")
    cw.write("/// %s interface" % ast_walker)
    cw.write("/// </summary>")
    cw.enter_block("public interface %s" % ast_walker)
    for t in l:
        if space: cw.write("")
        ttname = t.t.Name
        cw.write("bool Walk(%s node);" % ttname)
        cw.write("void PostWalk(%s node);" % ttname)
        space = True
    cw.exit_block()

def gen_astwalker(cw, l, name, value):
    cw.write("/// <summary>")
    cw.write("/// %s abstract class - the powerful walker (default result is %s)" % (name, value))
    cw.write("/// </summary>")
    cw.enter_block("public abstract class %s : %s" % (name, ast_walker))
    space = False
    for t in l:
        if space: cw.write("")
        ttname = t.t.Name
        cw.write("// %s" % ttname)
        if COMPRESS:
            cw.write("public virtual bool Walk(%s node) { return %s; }" % (ttname, value))
            cw.write("public virtual void PostWalk(%s node) { }" % ttname)
        else:        
            cw.enter_block("public virtual bool Walk(%s node)" % ttname)
            cw.write("return %s;" % value)
            cw.exit_block()
            cw.enter_block("public virtual void PostWalk(%s node)" % ttname)
            cw.exit_block()
        space = True
    cw.exit_block()

def gen_bigwalk(cw, l):
    gen_interface(cw, l)
    cw.write("")
    cw.write("")    
    gen_astwalker(cw, l, "AstWalker", "true")
    cw.write("")
    cw.write("")
    gen_astwalker(cw, l, "AstWalkerNonRecursive", "false")

def gen_walker(cw):
    ast = get_ast()
    l = get_sorted_leaves(ast)
    gen_bigwalk(cw, l)

generate.CodeGenerator("AST Walker", gen_walker).doit()

