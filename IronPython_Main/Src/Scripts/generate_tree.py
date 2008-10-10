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
    def __init__(self, kind, type):
        self.kind = kind
        self.type = type

expressions = [
    #
    #          enum kind                        tree node type
    #

    #
    #   DO NOT REORDER THESE, THEY COME FROM THE LINQ V1 ENUM
    #

    Expression("Add",                "BinaryExpression"),
    Expression("AddChecked",         "BinaryExpression"),
    Expression("And",                "BinaryExpression"),
    Expression("AndAlso",            "BinaryExpression"),
    Expression("ArrayLength",        "UnaryExpression"),
    Expression("ArrayIndex",         "BinaryExpression"),
    Expression("Call",               "MethodCallExpression"),
    Expression("Coalesce",           "BinaryExpression"),
    Expression("Conditional",        "ConditionalExpression"),
    Expression("Constant",           "ConstantExpression"),
    Expression("Convert",            "UnaryExpression"),
    Expression("ConvertChecked",     "UnaryExpression"),
    Expression("Divide",             "BinaryExpression"),
    Expression("Equal",              "BinaryExpression"),
    Expression("ExclusiveOr",        "BinaryExpression"),
    Expression("GreaterThan",        "BinaryExpression"),
    Expression("GreaterThanOrEqual", "BinaryExpression"),
    Expression("Invoke",             "InvocationExpression"),
    Expression("Lambda",             "LambdaExpression"),
    Expression("LeftShift",          "BinaryExpression"),
    Expression("LessThan",           "BinaryExpression"),
    Expression("LessThanOrEqual",    "BinaryExpression"),
    Expression("ListInit",           "ListInitExpression"),
    Expression("MemberAccess",       "MemberExpression"),
    Expression("MemberInit",         "MemberInitExpression"),
    Expression("Modulo",             "BinaryExpression"),
    Expression("Multiply",           "BinaryExpression"),
    Expression("MultiplyChecked",    "BinaryExpression"),
    Expression("Negate",             "UnaryExpression"),
    Expression("UnaryPlus",          "UnaryExpression"),
    Expression("NegateChecked",      "UnaryExpression"),
    Expression("New",                "NewExpression"),
    Expression("NewArrayInit",       "NewArrayExpression"),
    Expression("NewArrayBounds",     "NewArrayExpression"),
    Expression("Not",                "UnaryExpression"),
    Expression("NotEqual",           "BinaryExpression"),
    Expression("Or",                 "BinaryExpression"),
    Expression("OrElse",             "BinaryExpression"),
    Expression("Parameter",          "ParameterExpression"),
    Expression("Power",              "BinaryExpression"),
    Expression("Quote",              "UnaryExpression"),
    Expression("RightShift",         "BinaryExpression"),
    Expression("Subtract",           "BinaryExpression"),
    Expression("SubtractChecked",    "BinaryExpression"),
    Expression("TypeAs",             "UnaryExpression"),
    Expression("TypeIs",             "TypeBinaryExpression"),

    # New types in LINQ V2

    Expression("Assign",             "AssignmentExpression"),
    Expression("Block",              "Block"),
    Expression("BreakStatement",     "BreakStatement"),
    Expression("Generator",          "LambdaExpression"),
    Expression("ContinueStatement",  "ContinueStatement"),
    Expression("DoStatement",        "DoStatement"),
    Expression("Dynamic",            "DynamicExpression"),
    Expression("EmptyStatement",     "EmptyStatement"),
    Expression("Extension",          "ExtensionExpression"),
    Expression("Index",              "IndexExpression"),
    Expression("LabeledStatement",   "LabeledStatement"),
    Expression("LocalScope",         "LocalScopeExpression"),
    Expression("LoopStatement",      "LoopStatement"),
    Expression("ReturnStatement",    "ReturnStatement"),
    Expression("Scope",              "ScopeExpression"),
    Expression("SwitchStatement",    "SwitchStatement"),
    Expression("ThrowStatement",     "ThrowStatement"),
    Expression("TryStatement",       "TryStatement"),
    Expression("Unbox",              "UnaryExpression"),
    Expression("Variable",           "VariableExpression"),
    Expression("YieldStatement",     "YieldStatement"),
]

def get_unique_types():
    return sorted(list(set(filter(None, map(lambda n: n.type, expressions)))))

def gen_tree_nodes(cw):
    for node in expressions:
        cw.write(node.kind + ",")

def gen_stackspiller_switch(cw):
    for node in expressions:
        method = "Rewrite"

        #special case AndAlso and OrElse
        if node.kind == "AndAlso" or node.kind == "OrElse":
            method += "Logical"

        cw.write("// " + node.kind)
        cw.write("case ExpressionType." + node.kind + ":")
        cw.write("    result = " + method + node.type + "(node, stack);")
        cw.write("    break;")

def gen_compiler(cw):
    for node in expressions:
        method = "Emit"

        # special case certain unary/binary expressions
        if node.kind in ["AndAlso", "OrElse", "Quote", "Coalesce", "Unbox"]:
            method += node.kind
        elif node.kind in ["Convert", "ConvertChecked"]:
            method += "Convert"

        cw.write("// " + node.kind)
        cw.write("case ExpressionType." + node.kind + ":")
        cw.write("    " + method + node.type + "(node);")
        cw.write("    break;")

def gen_interpreter(cw):
   for node in expressions:
        method = "Interpret"

        # special case AndAlso and OrElse
        if node.kind in ["AndAlso", "OrElse", "Quote", "Coalesce", "Unbox"]:
            method += node.kind
        elif node.kind in ["Convert", "ConvertChecked"]:
            method += "Convert"

        method += node.type           
			
        cw.write(" case ExpressionType.%s: return %s(state, expr);" % (node.kind, method))

def gen_ast_dispatch(cw, name):
    for node in expressions:
        text = name + node.type + ","
        comment = "//    " + node.kind

        cw.write(text + (40 - len(text)) * " " + comment)

def gen_ast_writer(cw):
    gen_ast_dispatch(cw, "Write")

def main():
    return generate(
        ("Expression Tree Node Types", gen_tree_nodes),
        ("StackSpiller Switch", gen_stackspiller_switch),
        ("Ast Interpreter", gen_interpreter),
        ("Expression Compiler", gen_compiler),
    )

if __name__ == "__main__":
    main()
