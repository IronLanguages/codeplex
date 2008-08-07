#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

from generate import generate

class Expression:
    def __init__(self, kind, type, enabled):
        self.kind = kind
        self.type = type
        self.enabled = enabled

expressions = [
    #
    #          enum kind                        tree node type                          enabled
    #

    #
    #   DO NOT REORDER THESE, THEY COME FROM THE LINQ ENUM
    #

    Expression("Add",                           "BinaryExpression",                     True),
    Expression("AddChecked",                    "BinaryExpression",                     True),
    Expression("And",                           "BinaryExpression",                     True),
    Expression("AndAlso",                       "BinaryExpression",                     True),
    Expression("ArrayLength",                   "UnaryExpression",                      True),
    Expression("ArrayIndex",                    "BinaryExpression",                     True),
    Expression("Call",                          "MethodCallExpression",                 True),
    Expression("Coalesce",                      "BinaryExpression",                     True),
    Expression("Conditional",                   "ConditionalExpression",                True),
    Expression("Constant",                      "ConstantExpression",                   True),
    Expression("Convert",                       "UnaryExpression",                      True),
    Expression("ConvertChecked",                "UnaryExpression",                      True),
    Expression("Divide",                        "BinaryExpression",                     True),
    Expression("Equal",                         "BinaryExpression",                     True),
    Expression("ExclusiveOr",                   "BinaryExpression",                     True),
    Expression("GreaterThan",                   "BinaryExpression",                     True),
    Expression("GreaterThanOrEqual",            "BinaryExpression",                     True),
    Expression("Invoke",                        "InvocationExpression",                 True),
    Expression("Lambda",                        "LambdaExpression",                     True),
    Expression("LeftShift",                     "BinaryExpression",                     True),
    Expression("LessThan",                      "BinaryExpression",                     True),
    Expression("LessThanOrEqual",               "BinaryExpression",                     True),
    Expression("ListInit",                      "ListInitExpression",                   True),
    Expression("MemberAccess",                  "MemberExpression",                     True),
    Expression("MemberInit",                    "MemberInitExpression",                 True),
    Expression("Modulo",                        "BinaryExpression",                     True),
    Expression("Multiply",                      "BinaryExpression",                     True),
    Expression("MultiplyChecked",               "BinaryExpression",                     True),
    Expression("Negate",                        "UnaryExpression",                      True),
    Expression("UnaryPlus",                     "UnaryExpression",                      True),
    Expression("NegateChecked",                 "UnaryExpression",                      True),
    Expression("New",                           "NewExpression",                        True),
    Expression("NewArrayInit",                  "NewArrayExpression",                   True),
    Expression("NewArrayBounds",                "NewArrayExpression",                   True),
    Expression("Not",                           "UnaryExpression",                      True),
    Expression("NotEqual",                      "BinaryExpression",                     True),
    Expression("Or",                            "BinaryExpression",                     True),
    Expression("OrElse",                        "BinaryExpression",                     True),
    Expression("Parameter",                     "ParameterExpression",                  True),
    Expression("Power",                         "BinaryExpression",                     True),
    Expression("Quote",                         "UnaryExpression",                      True),
    Expression("RightShift",                    "BinaryExpression",                     True),
    Expression("Subtract",                      "BinaryExpression",                     True),
    Expression("SubtractChecked",               "BinaryExpression",                     True),
    Expression("TypeAs",                        "UnaryExpression",                      True),
    Expression("TypeIs",                        "TypeBinaryExpression",                 True),

    # New types in LINQ V2

    Expression("ActionExpression",              "ActionExpression",                     True),
    Expression("Assign",                        "AssignmentExpression",                 True),
    Expression("Block",                         "Block",                                True),
    Expression("BreakStatement",                "BreakStatement",                       True),
    Expression("Generator",                     "LambdaExpression",                     True),
    Expression("ContinueStatement",             "ContinueStatement",                    True),
    Expression("Delete",                        "DeleteExpression",                     True),
    Expression("DoStatement",                   "DoStatement",                          True),
    Expression("EmptyStatement",                "EmptyStatement",                       True),
    Expression("Extension",                     "ExtensionExpression",                  True),
    Expression("IndexedProperty",               "IndexedPropertyExpression",            True),
    Expression("LabeledStatement",              "LabeledStatement",                     True),
    Expression("LocalScope",                    "LocalScopeExpression",                 True),
    Expression("LoopStatement",                 "LoopStatement",                        True),
    Expression("OnesComplement",                "UnaryExpression",                      True),
    Expression("ReturnStatement",               "ReturnStatement",                      True),
    Expression("Scope",                         "ScopeExpression",                      True),
    Expression("SwitchStatement",               "SwitchStatement",                      True),
    Expression("ThrowStatement",                "ThrowStatement",                       True),
    Expression("TryStatement",                  "TryStatement",                         True),
    Expression("Unbox",                         "UnaryExpression",                      True),
    Expression("Variable",                      "VariableExpression",                   True),
    Expression("YieldStatement",                "YieldStatement",                       True),
]

def get_unique_types():
    return sorted(list(set(filter(None, map(lambda n: n.type, expressions)))))

def gen_scripting_walker(cw):
    nodes = get_unique_types() + [
            "CatchBlock",
            "IfStatementTest",
            "SwitchCase"
        ]
    
    nodes.remove("ExtensionExpression")

    space = 0
    for node in nodes:
        if space: cw.write("")
        cw.write("// %s" % node)
        cw.write("protected virtual bool Walk(%s node) { return true; }" % node)
        cw.write("protected virtual void PostWalk(%s node) { }" % node)
        space = 1

default_visit = """// %(type)s
private Expression DefaultVisit%(type)s(Expression node) {
    return Visit((%(type)s)node);
}"""

def gen_visitor_methods(cw):
    nodes = get_unique_types()
    nodes.remove("ExtensionExpression")
    
    space = 0
    for node in nodes:
        if space: cw.write("")
        cw.write(default_visit, type = node)
        space = 1

def gen_visitor_switch(cw):
    for node in expressions:
        method = "DefaultVisit"

        if node.enabled:
            cw.write("// " + node.kind)
            cw.write("case ExpressionType." + node.kind + ":")
            cw.write("    return " + method + node.type + "(node);")
        else:
            cw.write("// ** " + node.kind)        

def gen_tree_nodes(cw):
    for node in expressions:
        text = node.kind
        if not node.enabled:
            text = "//    " + text
        cw.write(text + ",")

def gen_stackspiller_switch(cw):
    for node in expressions:
        method = "Rewrite"

        #special case AndAlso and OrElse
        if node.kind == "AndAlso" or node.kind == "OrElse":
            method += "Logical"

        if node.enabled:
            cw.write("// " + node.kind)
            cw.write("case ExpressionType." + node.kind + ":")
            cw.write("    result = " + method + node.type + "(self, node, stack);")
            cw.write("    break;")
        else:
            cw.write("// ** " + node.kind)

def gen_compiler(cw):
    for node in expressions:
        method = "Emit"

        # special case certain unary/binary expressions
        if node.kind in ["AndAlso", "OrElse", "Quote", "Coalesce", "Unbox"]:
            method += node.kind
        elif node.kind in ["Convert", "ConvertChecked"]:
            method += "Convert"

        if node.enabled:
            cw.write("// " + node.kind)
            cw.write("case ExpressionType." + node.kind + ":")
            cw.write("    " + method + node.type + "(this, node);")
            cw.write("    break;")
        else:
            cw.write("// ** " + node.kind)

def gen_interpreter(cw):
   for node in expressions:
        method = "Interpret"

        # special case AndAlso and OrElse
        if node.kind in ["AndAlso", "OrElse", "Quote", "Coalesce", "Unbox"]:
            method += node.kind
        elif node.kind in ["Convert", "ConvertChecked"]:
            method += "Convert"

        method += node.type           
        
        if node.enabled:
            comment = ""
        else:
            comment = "//"            
			
        cw.write("%s case ExpressionType.%s: return %s(state, expr);" % (comment, node.kind, method))

def gen_ast_dispatch(cw, name):
    for node in expressions:
        if node.enabled:
            text = name + node.type + ","
            comment = "//    " + node.kind
        else:
            text = ""
            comment = "// ** " + node.kind

        cw.write(text + (40 - len(text)) * " " + comment)

def gen_ast_writer(cw):
    gen_ast_dispatch(cw, "Write")

def main():
    return generate(
        ("Expression Tree Node Types", gen_tree_nodes),
        ("ExpressionVisitor Switch", gen_visitor_switch),
        ('ExpressionVisitor Methods', gen_visitor_methods),
        ("StackSpiller Switch", gen_stackspiller_switch),
        ("Ast Interpreter", gen_interpreter),
        ("Expression Compiler", gen_compiler),
    )

if __name__ == "__main__":
    main()
