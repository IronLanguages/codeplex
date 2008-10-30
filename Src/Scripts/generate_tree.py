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
    def __init__(self, kind, type, interop=False):
        self.kind = kind
        self.type = type
        self.interop = interop

expressions = [
    #
    #          enum kind                        tree node type
    #

    #
    #   DO NOT REORDER THESE, THEY COME FROM THE LINQ V1 ENUM
    #

    #          Enum Value               Expression Class                   Interop

    Expression("Add",                   "BinaryExpression",                True),
    Expression("AddChecked",            "BinaryExpression"),
    Expression("And",                   "BinaryExpression",                True),
    Expression("AndAlso",               "BinaryExpression"),
    Expression("ArrayLength",           "UnaryExpression"),
    Expression("ArrayIndex",            "BinaryExpression"),
    Expression("Call",                  "MethodCallExpression"),
    Expression("Coalesce",              "BinaryExpression"),
    Expression("Conditional",           "ConditionalExpression"),
    Expression("Constant",              "ConstantExpression"),
    Expression("Convert",               "UnaryExpression"),
    Expression("ConvertChecked",        "UnaryExpression"),
    Expression("Divide",                "BinaryExpression",                True),
    Expression("Equal",                 "BinaryExpression",                True),
    Expression("ExclusiveOr",           "BinaryExpression",                True),
    Expression("GreaterThan",           "BinaryExpression",                True),
    Expression("GreaterThanOrEqual",    "BinaryExpression",                True),
    Expression("Invoke",                "InvocationExpression"),
    Expression("Lambda",                "LambdaExpression"),
    Expression("LeftShift",             "BinaryExpression",                True),
    Expression("LessThan",              "BinaryExpression",                True),
    Expression("LessThanOrEqual",       "BinaryExpression",                True),
    Expression("ListInit",              "ListInitExpression"),
    Expression("MemberAccess",          "MemberExpression"),
    Expression("MemberInit",            "MemberInitExpression"),
    Expression("Modulo",                "BinaryExpression",                True),
    Expression("Multiply",              "BinaryExpression",                True),
    Expression("MultiplyChecked",       "BinaryExpression"),
    Expression("Negate",                "UnaryExpression",                 True),
    Expression("UnaryPlus",             "UnaryExpression",                 True),
    Expression("NegateChecked",         "UnaryExpression"),
    Expression("New",                   "NewExpression"),
    Expression("NewArrayInit",          "NewArrayExpression"),
    Expression("NewArrayBounds",        "NewArrayExpression"),
    Expression("Not",                   "UnaryExpression",                 True),
    Expression("NotEqual",              "BinaryExpression",                True),
    Expression("Or",                    "BinaryExpression",                True),
    Expression("OrElse",                "BinaryExpression"),
    Expression("Parameter",             "ParameterExpression"),
    Expression("Power",                 "BinaryExpression",                True),
    Expression("Quote",                 "UnaryExpression"),
    Expression("RightShift",            "BinaryExpression",                True),
    Expression("Subtract",              "BinaryExpression",                True),
    Expression("SubtractChecked",       "BinaryExpression"),
    Expression("TypeAs",                "UnaryExpression"),
    Expression("TypeIs",                "TypeBinaryExpression"),

    # New types in LINQ V2

    Expression("Assign",                "BinaryExpression"),
    Expression("Block",                 "BlockExpression"),
    Expression("DebugInfo",             "DebugInfoExpression"),
    Expression("Dynamic",               "DynamicExpression"),
    Expression("Default",               "EmptyExpression"),
    Expression("Extension",             "ExtensionExpression"),
    Expression("Goto",                  "GotoExpression"),
    Expression("Index",                 "IndexExpression"),
    Expression("Label",                 "LabelExpression"),
    Expression("RuntimeVariables",      "RuntimeVariablesExpression"),
    Expression("Loop",                  "LoopExpression"),
    Expression("ReturnStatement",       "ReturnStatement"),      # TODO: remove
    Expression("Switch",                "SwitchExpression"),
    Expression("Throw",                 "UnaryExpression"),
    Expression("Try",                   "TryExpression"),
    Expression("Unbox",                 "UnaryExpression"),
    Expression("AddAssign",             "BinaryExpression",                True),
    Expression("AndAssign",             "BinaryExpression",                True),
    Expression("DivideAssign",          "BinaryExpression",                True),
    Expression("ExclusiveOrAssign",     "BinaryExpression",                True),
    Expression("LeftShiftAssign",       "BinaryExpression",                True),
    Expression("ModuloAssign",          "BinaryExpression",                True),
    Expression("MultiplyAssign",        "BinaryExpression",                True),
    Expression("OrAssign",              "BinaryExpression",                True),
    Expression("PowerAssign",           "BinaryExpression",                True),
    Expression("RightShiftAssign",      "BinaryExpression",                True),
    Expression("SubtractAssign",        "BinaryExpression",                True),
    Expression("AddAssignChecked",      "BinaryExpression",                True),
    Expression("MultiplyAssignChecked", "BinaryExpression",                True),
    Expression("SubtractAssignChecked", "BinaryExpression",                True),
]

op_assignments = ["MultiplyAssign", "MultiplyAssignChecked", "SubtractAssign", "SubtractAssignChecked", "ExclusiveOrAssign", "LeftShiftAssign", "RightShiftAssign", "ModuloAssign", "AddAssign", "AddAssignChecked", "AndAssign", "OrAssign", "DivideAssign", "PowerAssign"]

def get_unique_types():
    return sorted(list(set(filter(None, map(lambda n: n.type, expressions)))))

def gen_tree_nodes(cw):
    for node in expressions:
        cw.write(node.kind + ",")

def gen_stackspiller_switch(cw):
    
    no_spill_node_kinds =  ["Quote", "Parameter", "Constant", "RuntimeVariables", "Default"]
    
    # nodes that need spilling
    for node in expressions:
        if node.kind in no_spill_node_kinds:
            continue
    
        method = "Rewrite"
        
        # special case certain expressions
        if node.kind in ["Quote", "Throw", "Assign"]:
            method += node.kind
        
        #special case AndAlso and OrElse
        if node.kind == "AndAlso" or node.kind == "OrElse":
            method += "Logical"

        #special case OpAssign
        elif node.kind in op_assignments:
            method += "OpAssign"

        cw.write("// " + node.kind)
        cw.write("case ExpressionType." + node.kind + ":")
        cw.write("    result = " + method + node.type + "(node, stack);")           
        cw.write("    break;")
    
    # no spill nodes
    for kind in no_spill_node_kinds:
        cw.write("// " + kind)
        cw.write("case ExpressionType." + kind + ":")

    cw.write("    return new Result(RewriteAction.None, node);")


def gen_compiler(cw):
    for node in expressions:
        method = "Emit"

        # special case certain unary/binary expressions
        if node.kind in ["AndAlso", "OrElse", "Quote", "Coalesce", "Unbox", "Throw", "Assign"]:
            method += node.kind
        elif node.kind in ["Convert", "ConvertChecked"]:
            method += "Convert"
        #special case OpAssign
        elif node.kind in op_assignments:
            method += "OpAssign"

        cw.write("// " + node.kind)
        cw.write("case ExpressionType." + node.kind + ":")
        cw.write("    " + method + node.type + "(node);")
        cw.write("    break;")

def gen_interpreter(cw):
   for node in expressions:
        method = "Interpret"

        # special case AndAlso and OrElse
        if node.kind in ["AndAlso", "OrElse", "Quote", "Coalesce", "Unbox", "Throw", "Assign"]:
            method += node.kind
        elif node.kind in ["Convert", "ConvertChecked"]:
            method += "Convert"
        #special case OpAssign
        elif node.kind in op_assignments:
            method += "OpAssign"

        method += node.type           
			
        cw.write(" case ExpressionType.%s: return %s(state, expr);" % (node.kind, method))

def gen_ast_dispatch(cw, name):
    for node in expressions:
        text = name + node.type + ","
        comment = "//    " + node.kind

        cw.write(text + (40 - len(text)) * " " + comment)

def gen_ast_writer(cw):
    gen_ast_dispatch(cw, "Write")

def gen_op_validator(type, cw):
    for node in expressions:
        if node.interop and node.type == type:
             cw.write("case ExpressionType.%s:" % node.kind)

def gen_binop_validator(cw):
    gen_op_validator("BinaryExpression", cw)
    
def gen_unop_validator(cw):
    gen_op_validator("UnaryExpression", cw)

def main():
    return generate(
        ("Binary Operation Binder Validator", gen_binop_validator),
        ("Unary Operation Binder Validator", gen_unop_validator),
        ("Expression Tree Node Types", gen_tree_nodes),
        ("StackSpiller Switch", gen_stackspiller_switch),
        ("Ast Interpreter", gen_interpreter),
        ("Expression Compiler", gen_compiler)
    )

if __name__ == "__main__":
    main()
