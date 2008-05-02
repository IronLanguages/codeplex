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
    Expression("AddChecked",                    "",                                     False),
    Expression("And",                           "BinaryExpression",                     True),
    Expression("AndAlso",                       "BinaryExpression",                     True),
    Expression("ArrayLength",                   "",                                     False),
    Expression("ArrayIndex",                    "BinaryExpression",                     True),
    Expression("Call",                          "MethodCallExpression",                 True),
    Expression("Coalesce",                      "",                                     False),
    Expression("Conditional",                   "ConditionalExpression",                True),
    Expression("Constant",                      "ConstantExpression",                   True),
    Expression("Convert",                       "UnaryExpression",                      True),
    Expression("ConvertChecked",                "",                                     False),
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
    Expression("ListInit",                      "",                                     False),
    Expression("MemberAccess",                  "",                                     False),
    Expression("MemberInit",                    "",                                     False),
    Expression("Modulo",                        "BinaryExpression",                     True),
    Expression("Multiply",                      "BinaryExpression",                     True),
    Expression("MultiplyChecked",               "",                                     False),
    Expression("Negate",                        "UnaryExpression",                      True),
    Expression("UnaryPlus",                     "",                                     False),
    Expression("NegateChecked",                 "",                                     False),
    Expression("New",                           "NewExpression",                        True),
    Expression("NewArrayInit",                  "",                                     False),
    Expression("NewArrayBounds",                "NewArrayExpression",                   True),
    Expression("Not",                           "UnaryExpression",                      True),
    Expression("NotEqual",                      "BinaryExpression",                     True),
    Expression("Or",                            "BinaryExpression",                     True),
    Expression("OrElse",                        "BinaryExpression",                     True),
    Expression("Parameter",                     "ParameterExpression",                  True),
    Expression("Power",                         "",                                     False),
    Expression("Quote",                         "",                                     False),
    Expression("RightShift",                    "BinaryExpression",                     True),
    Expression("Subtract",                      "BinaryExpression",                     True),
    Expression("SubtractChecked",               "",                                     False),
    Expression("TypeAs",                        "",                                     False),
    Expression("TypeIs",                        "TypeBinaryExpression",                 True),

    # DLR Added values

    Expression("ActionExpression",              "ActionExpression",                     True),
    Expression("Assign",                        "AssignmentExpression",                 True),
    Expression("Block",                         "Block",                                True),
    Expression("BreakStatement",                "BreakStatement",                       True),
    Expression("CodeContextExpression",         "IntrinsicExpression",                  True),
    Expression("GeneratorIntrinsic",            "IntrinsicExpression",                  True),
    Expression("Generator",                     "GeneratorLambdaExpression",            True),
    Expression("ContinueStatement",             "ContinueStatement",                    True),
    Expression("DeleteStatement",               "DeleteStatement",                      True),
    Expression("DoStatement",                   "DoStatement",                          True),
    Expression("EmptyStatement",                "EmptyStatement",                       True),
    Expression("GlobalVariable",                "VariableExpression",                   True),
    Expression("LabeledStatement",              "LabeledStatement",                     True),
    Expression("LocalVariable",                 "VariableExpression",                   True),
    Expression("LoopStatement",                 "LoopStatement",                        True),
    Expression("MemberExpression",              "MemberExpression",                     True),
    Expression("NewArrayExpression",            "NewArrayExpression",                   True),
    Expression("OnesComplement",                "UnaryExpression",                      True),
    Expression("ReturnStatement",               "ReturnStatement",                      True),
    Expression("ScopeStatement",                "ScopeStatement",                       True),
    Expression("SwitchStatement",               "SwitchStatement",                      True),
    Expression("TemporaryVariable",             "VariableExpression",                   True),
    Expression("ThrowStatement",                "ThrowStatement",                       True),
    Expression("TryStatement",                  "TryStatement",                         True),
    Expression("YieldStatement",                "YieldStatement",                       True),
    Expression("Extension",                     "ExtensionExpression",                  True),
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
        cw.write("protected internal virtual bool Walk(%s node) { return true; }" % node)
        cw.write("protected internal virtual void PostWalk(%s node) { }" % node)
        space = 1

def gen_tree_nodes(cw):
    for node in expressions:
        text = node.kind
        if not node.enabled:
            text = "//    " + text
        cw.write(text + ",")


def gen_ast_rewriter(cw):
    for node in expressions:
        method = "Rewrite"

        #special case AndAlso and OrElse
        if node.kind == "AndAlso" or node.kind == "OrElse":
            method += "Logical"
        
        if node.enabled:
            text = method + node.type + ","
            comment = "//    " + node.kind
        else:
            text = ""
            comment = "// ** " + node.kind

        cw.write(text + (60 - len(text)) * " " + comment)

def gen_ast_interpreter(cw):
    for node in expressions:
        method = "Interpret"

        # special case AndAlso and OrElse
        if node.kind == "AndAlso" or node.kind == "OrElse":
            method += node.kind

        if node.enabled:
            text = method + node.type + ","
            comment = "//    " + node.kind
        else:
            text = ""
            comment = "// ** " + node.kind

        cw.write(text + (60 - len(text)) * " " + comment)

def gen_ast_writer(cw):
    for node in expressions:
        if node.enabled:
            text = "Write" + node.type + ","
            comment = "//    " + node.kind
        else:
            text = ""
            comment = "// ** " + node.kind

        cw.write(text + (40 - len(text)) * " " + comment)

def main():
    return generate(
        ("Expression Tree Node Types", gen_tree_nodes),
        ("DLR AST Walker", gen_scripting_walker),
        ("Ast Rewriter", gen_ast_rewriter),
        ("Ast Interpreter", gen_ast_interpreter),
        ("Ast Writer", gen_ast_writer),
    )

if __name__ == "__main__":
    main()
