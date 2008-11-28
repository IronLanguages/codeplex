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
    def __init__(self, kind, type, interop = False, reducible = False):
        self.kind = kind
        self.type = type
        self.interop = interop
        self.reducible = reducible

expressions = [
    #
    #          enum kind                        tree node type
    #

    #
    #   DO NOT REORDER THESE, THEY COME FROM THE LINQ V1 ENUM
    #

    #          Enum Value               Expression Class                   Flags

    Expression("Add",                   "BinaryExpression",                interop = True),
    Expression("AddChecked",            "BinaryExpression"),
    Expression("And",                   "BinaryExpression",                interop = True),
    Expression("AndAlso",               "BinaryExpression"),
    Expression("ArrayLength",           "UnaryExpression"),
    Expression("ArrayIndex",            "BinaryExpression"),
    Expression("Call",                  "MethodCallExpression"),
    Expression("Coalesce",              "BinaryExpression"),
    Expression("Conditional",           "ConditionalExpression"),
    Expression("Constant",              "ConstantExpression"),
    Expression("Convert",               "UnaryExpression"),
    Expression("ConvertChecked",        "UnaryExpression"),
    Expression("Divide",                "BinaryExpression",                interop = True),
    Expression("Equal",                 "BinaryExpression",                interop = True),
    Expression("ExclusiveOr",           "BinaryExpression",                interop = True),
    Expression("GreaterThan",           "BinaryExpression",                interop = True),
    Expression("GreaterThanOrEqual",    "BinaryExpression",                interop = True),
    Expression("Invoke",                "InvocationExpression"),
    Expression("Lambda",                "LambdaExpression"),
    Expression("LeftShift",             "BinaryExpression",                interop = True),
    Expression("LessThan",              "BinaryExpression",                interop = True),
    Expression("LessThanOrEqual",       "BinaryExpression",                interop = True),
    Expression("ListInit",              "ListInitExpression"),
    Expression("MemberAccess",          "MemberExpression"),
    Expression("MemberInit",            "MemberInitExpression"),
    Expression("Modulo",                "BinaryExpression",                interop = True),
    Expression("Multiply",              "BinaryExpression",                interop = True),
    Expression("MultiplyChecked",       "BinaryExpression"),
    Expression("Negate",                "UnaryExpression",                 interop = True),
    Expression("UnaryPlus",             "UnaryExpression",                 interop = True),
    Expression("NegateChecked",         "UnaryExpression"),
    Expression("New",                   "NewExpression"),
    Expression("NewArrayInit",          "NewArrayExpression"),
    Expression("NewArrayBounds",        "NewArrayExpression"),
    Expression("Not",                   "UnaryExpression",                 interop = True),
    Expression("NotEqual",              "BinaryExpression",                interop = True),
    Expression("Or",                    "BinaryExpression",                interop = True),
    Expression("OrElse",                "BinaryExpression"),
    Expression("Parameter",             "ParameterExpression"),
    Expression("Power",                 "BinaryExpression",                interop = True),
    Expression("Quote",                 "UnaryExpression"),
    Expression("RightShift",            "BinaryExpression",                interop = True),
    Expression("Subtract",              "BinaryExpression",                interop = True),
    Expression("SubtractChecked",       "BinaryExpression"),
    Expression("TypeAs",                "UnaryExpression"),
    Expression("TypeIs",                "TypeBinaryExpression"),

    # New types in LINQ V2

    Expression("Assign",                "BinaryExpression"),
    Expression("Block",                 "BlockExpression"),
    Expression("DebugInfo",             "DebugInfoExpression"),
    Expression("Decrement",             "UnaryExpression",                 interop = True),
    Expression("Dynamic",               "DynamicExpression"),
    Expression("Default",               "DefaultExpression"),
    Expression("Extension",             "ExtensionExpression"),
    Expression("Goto",                  "GotoExpression"),
    Expression("Increment",             "UnaryExpression",                 interop = True),
    Expression("Index",                 "IndexExpression"),
    Expression("Label",                 "LabelExpression"),
    Expression("RuntimeVariables",      "RuntimeVariablesExpression"),
    Expression("Loop",                  "LoopExpression"),
    Expression("Switch",                "SwitchExpression"),
    Expression("Throw",                 "UnaryExpression"),
    Expression("Try",                   "TryExpression"),
    Expression("Unbox",                 "UnaryExpression"),
    Expression("AddAssign",             "BinaryExpression",                interop = True, reducible = True),
    Expression("AndAssign",             "BinaryExpression",                interop = True, reducible = True),
    Expression("DivideAssign",          "BinaryExpression",                interop = True, reducible = True),
    Expression("ExclusiveOrAssign",     "BinaryExpression",                interop = True, reducible = True),
    Expression("LeftShiftAssign",       "BinaryExpression",                interop = True, reducible = True),
    Expression("ModuloAssign",          "BinaryExpression",                interop = True, reducible = True),
    Expression("MultiplyAssign",        "BinaryExpression",                interop = True, reducible = True),
    Expression("OrAssign",              "BinaryExpression",                interop = True, reducible = True),
    Expression("PowerAssign",           "BinaryExpression",                interop = True, reducible = True),
    Expression("RightShiftAssign",      "BinaryExpression",                interop = True, reducible = True),
    Expression("SubtractAssign",        "BinaryExpression",                interop = True, reducible = True),
    Expression("AddAssignChecked",      "BinaryExpression",                reducible = True),
    Expression("MultiplyAssignChecked", "BinaryExpression",                reducible = True),
    Expression("SubtractAssignChecked", "BinaryExpression",                reducible = True),
    Expression("PreIncrementAssign",    "UnaryExpression",                 reducible = True),
    Expression("PreDecrementAssign",    "UnaryExpression",                 reducible = True),
    Expression("PostIncrementAssign",   "UnaryExpression",                 reducible = True),
    Expression("PostDecrementAssign",   "UnaryExpression",                 reducible = True),
    Expression("TypeEqual",             "TypeBinaryExpression",            reducible = True),
]

def get_unique_types():
    return sorted(list(set(filter(None, map(lambda n: n.type, expressions)))))

def gen_tree_nodes(cw):
    for node in expressions:
        cw.write(node.kind + ",")

def gen_stackspiller_switch(cw):
    
    no_spill_node_kinds =  ["Quote", "Parameter", "Constant", "RuntimeVariables", "Default"]
    
    # nodes that need spilling
    for node in expressions:
        if node.kind in no_spill_node_kinds or node.reducible:
            continue
    
        method = "Rewrite"
        
        # special case certain expressions
        if node.kind in ["Quote", "Throw", "Assign"]:
            method += node.kind
        
        #special case AndAlso and OrElse
        if node.kind == "AndAlso" or node.kind == "OrElse":
            method += "Logical"

        cw.write("case ExpressionType." + node.kind + ":")
        cw.write("    result = " + method + node.type + "(node, stack);")           
        cw.write("    break;")

    # core reducible nodes
    for node in expressions:
        if node.reducible:
            cw.write("case ExpressionType." + node.kind + ":")

    cw.write("    result = RewriteReducibleExpression(node, stack);")
    cw.write("    break;")
    
    # no spill nodes
    for kind in no_spill_node_kinds:
        cw.write("case ExpressionType." + kind + ":")

    cw.write("    return new Result(RewriteAction.None, node);")


def gen_compiler(cw):
    for node in expressions:
        if node.reducible:
            continue
        
        method = "Emit"

        # special case certain unary/binary expressions
        if node.kind in ["AndAlso", "OrElse", "Quote", "Coalesce", "Unbox", "Throw", "Assign"]:
            method += node.kind
        elif node.kind in ["Convert", "ConvertChecked"]:
            method += "Convert"

        cw.write("case ExpressionType." + node.kind + ":")
        cw.write("    " + method + node.type + "(node);")
        cw.write("    break;")
    
def gen_interpreter(cw):
    for node in expressions:
        if node.reducible:
            continue
   
        method = "Interpret"

        # special case AndAlso and OrElse
        if node.kind in ["AndAlso", "OrElse", "Quote", "Coalesce", "Unbox", "Throw", "Assign"]:
            method += node.kind
        elif node.kind in ["Convert", "ConvertChecked"]:
            method += "Convert"

        method += node.type           
			
        cw.write(" case ExpressionType.%s: return %s(state, expr);" % (node.kind, method))
    
    # core reducible nodes
    for node in expressions:
        if node.reducible:
            cw.write("case ExpressionType." + node.kind + ":")

    cw.write("    return InterpretReducibleExpression(state, expr);")

def gen_op_validator(type, cw):
    for node in expressions:
        if node.interop and node.type == type:
             cw.write("case ExpressionType.%s:" % node.kind)

def gen_binop_validator(cw):
    gen_op_validator("BinaryExpression", cw)
    
def gen_unop_validator(cw):
    gen_op_validator("UnaryExpression", cw)

def gen_checked_ops(cw):
    for node in expressions:
        if node.kind.endswith("Checked"):
            cw.write("case ExpressionType.%s:" % node.kind)

def main():
    return generate(
        ("Checked Operations", gen_checked_ops),
        ("Binary Operation Binder Validator", gen_binop_validator),
        ("Unary Operation Binder Validator", gen_unop_validator),
        ("Expression Tree Node Types", gen_tree_nodes),
        ("StackSpiller Switch", gen_stackspiller_switch),
        ("Ast Interpreter", gen_interpreter),
        ("Expression Compiler", gen_compiler)
    )

if __name__ == "__main__":
    main()
