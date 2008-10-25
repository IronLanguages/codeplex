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

    Expression("Assign",             "AssignmentExpression"), # TODO: merge to BinaryExpression
    Expression("Block",              "Block"),                # TODO: rename to BlockExpression
    Expression("DebugInfo",          "DebugInfoExpression"),
    Expression("Dynamic",            "DynamicExpression"),
    Expression("EmptyStatement",     "EmptyStatement"),       # TODO: rename to EmptyExpression
    Expression("Extension",          "ExtensionExpression"),
    Expression("Goto",               "GotoExpression"),
    Expression("Index",              "IndexExpression"),
    Expression("Label",              "LabelExpression"),
    Expression("LocalScope",         "LocalScopeExpression"), # TODO: RuntimeVariablesExpression
    Expression("LoopStatement",      "LoopStatement"),        # TODO: LoopExpression
    Expression("ReturnStatement",    "ReturnStatement"),      # TODO: remove
    Expression("SwitchStatement",    "SwitchStatement"),      # TODO: SwitchExpression
    Expression("Throw",              "UnaryExpression"),      
    Expression("TryStatement",       "TryStatement"),         # TODO: TryExpression
    Expression("Unbox",              "UnaryExpression"),
    Expression("AddAssign",          "BinaryExpression"),
    Expression("AndAssign",          "BinaryExpression"),
    Expression("DivideAssign",       "BinaryExpression"),
    Expression("ExclusiveOrAssign",  "BinaryExpression"),
    Expression("LeftShiftAssign",    "BinaryExpression"),
    Expression("ModuloAssign",       "BinaryExpression"),
    Expression("MultiplyAssign",     "BinaryExpression"),
    Expression("OrAssign",           "BinaryExpression"),
    Expression("PowerAssign",        "BinaryExpression"),
    Expression("RightShiftAssign",   "BinaryExpression"),
    Expression("SubtractAssign",     "BinaryExpression"),

]

op_assignments = ["MultiplyAssign", "SubtractAssign", "ExclusiveOrAssign", "LeftShiftAssign", "RightShiftAssign", "ModuloAssign", "AddAssign", "AndAssign", "OrAssign", "DivideAssign", "PowerAssign"]

def get_unique_types():
    return sorted(list(set(filter(None, map(lambda n: n.type, expressions)))))

def gen_tree_nodes(cw):
    for node in expressions:
        cw.write(node.kind + ",")

def gen_stackspiller_switch(cw):
    
    no_spill_node_kinds =  ["Quote", "Parameter", "Constant", "LocalScope", "EmptyStatement"]
    
    # nodes that need spilling
    for node in expressions:
        if node.kind in no_spill_node_kinds:
            continue
    
        method = "Rewrite"
        
        # special case certain unary expressions
        if node.kind == "Quote" or node.kind == "Throw":
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
        if node.kind in ["AndAlso", "OrElse", "Quote", "Coalesce", "Unbox", "Throw"]:
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
        if node.kind in ["AndAlso", "OrElse", "Quote", "Coalesce", "Unbox", "Throw"]:
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

def main():
    return generate(
        ("Expression Tree Node Types", gen_tree_nodes),
        ("StackSpiller Switch", gen_stackspiller_switch),
        ("Ast Interpreter", gen_interpreter),
        ("Expression Compiler", gen_compiler),
    )

if __name__ == "__main__":
    main()
