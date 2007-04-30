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
import System
import clr

ast_walker = "PythonWalker"

def inherits(t, p):
    if not t:
        return False
    elif t.FullName == p:
        return True
    else:
        return inherits(t.BaseType, p)

def get_ast(assembly, roots):
    import clr

    sets = {}
    for root in roots:
        sets[root] = set()

    for node in assembly.GetTypes():
        # skip abstract types
        if node.IsAbstract: continue

        for root in roots:
            if inherits(node, root):
                sets[root].add(node.Name)
                break

    result = []
    for root in roots:
        result.extend(sorted(list(sets[root])))

    return result
    
def gen_walker(cw, nodes, name, base, ast, value):
    cw.write("/// <summary>")
    cw.write("/// %s class - The %s AST Walker (default result is %s)" % (name, ast, value))
    cw.write("/// </summary>")
    if base:
        cw.enter_block("public class %s : %s" % (name, base))
        method = "override"
    else:
        cw.enter_block("public class %s" % name)
        method = "virtual"

    space = 0
    for node in nodes:
        if space: cw.write("")
        cw.write("// %s" % node)
        cw.write("public %s bool Walk(%s node) { return %s; }" % (method, node, value))
        cw.write("public %s void PostWalk(%s node) { }" % (method, node))
        space = 1
    cw.exit_block()

def gen_newline(cw):
    cw.write("")
    cw.write("")

def gen_scripting_walker(cw):
    nodes = get_ast(
        clr.LoadAssemblyByPartialName("Microsoft.Scripting"),
        [
            "Microsoft.Scripting.Internal.Ast.Expression",
            "Microsoft.Scripting.Internal.Ast.Statement",
            "Microsoft.Scripting.Internal.Ast.Node"
        ]
    )
    gen_walker(cw, nodes, "Walker", None, "Scripting", "true")
    gen_newline(cw)
    gen_walker(cw, nodes, "WalkerNonRecursive", "Walker", "Scripting", "false")


def gen_python_walker(cw):
    nodes = get_ast(
        clr.LoadAssemblyByPartialName("IronPython"),
        [
            "IronPython.Compiler.Ast.Expression",
            "IronPython.Compiler.Ast.Statement",
            "IronPython.Compiler.Ast.Node"
        ]
    )
    gen_walker(cw, nodes, "PythonWalker", None, "Python", "true")
    gen_newline(cw)
    gen_walker(cw, nodes, "PythonWalkerNonRecursive", "PythonWalker", "Python", "false")

generate.CodeGenerator("Scripting AST Walker", gen_scripting_walker).doit()
generate.CodeGenerator("Python AST Walker", gen_python_walker).doit()
