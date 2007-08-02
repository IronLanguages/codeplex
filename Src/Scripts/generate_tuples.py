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
from generate import CodeGenerator, CodeWriter

def make_arg_list(size, name ='T%(id)d'):
    return ', '.join([name % {'id': x} for x in range(size)])

def get_base(size):
    if size: return 'Tuple<' + make_arg_list(size) + '>'
    return 'NewTuple'

def gen_generic_args(i, type='object'):
    return ', '.join([type]*i)

def gen_tuple(cw, size, prevSize):
	cw.enter_block('public class Tuple<%s> : %s' % (make_arg_list(size), get_base(prevSize)))
	
	cw.write('public Tuple() { }')
	cw.write('')
	cw.write('public Tuple(' + make_arg_list(size, 'T%(id)d item%(id)d') + ')')
	cw.enter_block('  : base(' + make_arg_list(prevSize, 'item%(id)d') + ')')
	
	for i in range(prevSize, size):
		cw.write('_item%d = item%d;' % (i, i))
    
	cw.exit_block()
	cw.write('')
	for i in range(prevSize, size):
	    cw.write("private T%d _item%d;" % (i, i))
        cw.write('')
	for i in range(prevSize, size):
            cw.enter_block('public T%d Item%03d' % (i, i))
            cw.write('get { return _item%d; }' % i)
            cw.write('set { _item%d = value; }' % (i))
            cw.exit_block()
        
        cw.write('')
	cw.enter_block('public override object GetValue(int index)')
	cw.enter_block('switch(index)')
	for i in range(0, size):
		cw.write('case %d: return Item%03d;' % (i, i))
	cw.write('default: throw new ArgumentException("index");')
	cw.exit_block()
	cw.exit_block()

        cw.write('')
	cw.enter_block('public override void SetValue(int index, object value)')
	cw.enter_block('switch(index)')
	for i in range(0, size):
		cw.write('case %d: Item%03d = (T%d)value; break;' % (i, i, i))
	cw.write('default: throw new ArgumentException("index");')
	cw.exit_block()
	cw.exit_block()

	cw.enter_block('public override int Capacity')
	cw.enter_block('get')
	cw.write('return %d;' % size)
	cw.exit_block()
	cw.exit_block()
    
	cw.exit_block()
	
tuples = [1, 2, 4, 8, 16, 32, 64, 128]


def gen_tuples(cw):
    prevSize = 0
    for size in tuples:
        gen_tuple(cw, size, prevSize)
        prevSize = size

def gen_one_pgf(cw, i, first, last=False):
    if first: cw.enter_block("if (size <= %i)" %  i)
    elif not last: cw.else_block("if (size <= %i)" %  i)
    cw.writeline("return typeof(Tuple<%s>);" % gen_generic_args(i, ''))

def gen_get_size(cw):
    ssizes = sorted(tuples)

    first = True
    cw.enter_block("if (size <= %i)" % ssizes[-1])
    for i in ssizes[:-1]:
        gen_one_pgf(cw, i, first)
        first = False
    cw.else_block()
    gen_one_pgf(cw, ssizes[-1], False, True)
    cw.exit_block()
    cw.exit_block()


CodeGenerator("Tuples", gen_tuples).doit()

generate.CodeGenerator("Tuple Get From Size", gen_get_size).doit()