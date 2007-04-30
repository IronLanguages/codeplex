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

def gen_generic_args(i, type='object'):
    return ', '.join([type]*i)

sizes = [2, 4, 8, 16, 32, 64, 128]

def gen_one_pf(cw, i, first, last=False):
    if first: cw.enter_block("if (size <= %i)" %  i)
    elif not last: cw.else_block("if (size <= %i)" %  i)
    cw.writeline("envType = typeof(FunctionEnvironmentDictionary<Tuple<IAttributesCollection, %s>>);" % (gen_generic_args(i-1), ))
    cw.writeline("tupleType = typeof(Tuple<IAttributesCollection, %s>);" % (gen_generic_args(i-1)))

def gen_pf(cw):
    ssizes = sorted(sizes)

    first = True
    cw.enter_block("if (size <= %i && optimized)" % ssizes[-1])
    cw.writeline("Type envType, tupleType;")
    for i in ssizes[:-1]:
        gen_one_pf(cw, i, first)
        first = False
    cw.else_block()
    gen_one_pf(cw, ssizes[-1], False, True)
    cw.exit_block()
    cw.writeline("return new PropertyEnvironmentFactory(tupleType, envType);")

    cw.else_block()
    cw.writeline("return new IndexEnvironmentFactory(size);")
    cw.exit_block()



generate.CodeGenerator("partial factories", gen_pf).doit()
