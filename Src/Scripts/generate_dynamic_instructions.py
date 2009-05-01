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

import sys
from generate import generate

MAX_TYPES = 16

MAX_ARGS = 3
MAX_HELPERS = 7
TYPE_CODE_TYPES = ['Int16', 'Int32', 'Int64', 'Boolean', 'Char', 'Byte', 'Decimal', 'DateTime', 'Double', 'Single', 'UInt16', 'UInt32', 'UInt64', 'String', 'SByte']

def get_args(i):
    return ['arg' + str(x) for x in xrange(i)]

def get_arr_args(i):
    return ['args[' + str(x) + ']' for x in xrange(i)]

def get_object_args(i):
    return ['object arg' + str(x) for x in xrange(i)]

def get_type_names(i):
    if i == 1: return ['T0']
    return ['T' + str(x) for x in xrange(i)]    

def get_invoke_type_names(i):
    return get_type_names(i - 1) + ['TRet']

def get_cast_args(i):
    return ['(%s)%s' % (x[0], x[1]) for x in zip(get_type_names(i), get_args(i))]

def get_type_params(i):
    if i == 0: return ''
    return '<' + ', '.join(get_type_names(i)) + '>'
    
    
def gen_instruction(cw, n):
    type_names = get_type_names(n)
    class_type_params = ','.join(type_names + ['TRet'])
    func_type_params = ','.join(['CallSite'] + type_names + ['TRet'])
    func_type = 'Func<%s>' % func_type_params
  
    cw.enter_block('internal class DynamicInstruction<%s> : Instruction' % class_type_params)
    cw.write('private CallSite<%s> _site;' % func_type)
    cw.enter_block('public static Instruction Factory(CallSiteBinder binder)')
    cw.write('return new DynamicInstruction<%s>(CallSite<%s>.Create(binder));' % (class_type_params, func_type))
    cw.exit_block()
    
    cw.enter_block('private DynamicInstruction(CallSite<%s> site)' % func_type)
    cw.write('this._site = site;')
    cw.exit_block()
    
    cw.write('public override int ProducedStack { get { return 1; } }')
    cw.write('public override int ConsumedStack { get { return %d; } }' % n)
    
    cw.enter_block('public override int Run(InterpretedFrame frame)')
    args = get_args(n)
    for arg in args[::-1]:
        cw.write('object %s = frame.Pop();' % arg)
    
        
    args = ['_site'] + get_cast_args(n)
    cw.write('frame.Push(_site.Target(')
    
    for arg in args[:-1]:
        cw.write('    ' + arg + ',')
    cw.write('    ' + args[-1]+'));')
        
    cw.write('return +1;')
    cw.exit_block()
    
    cw.enter_block('public override string ToString()')
    cw.write('return "Dynamic(" + _site.Binder.ToString() + ")";')
    cw.exit_block()
    
    cw.exit_block()
    
def gen_types(cw):
    for i in xrange(MAX_TYPES):
        cw.write('case %d: genericType = typeof(DynamicInstruction<%s>); break;' %
                  (i+1, ''.join([',']*i)))
                  
    
    
def gen_instructions(cw):
    for i in xrange(MAX_TYPES):
        gen_instruction(cw, i)
    

def gen_run_method(cw, n, is_void):
    type_params = ['T%d' % i for i in xrange(n)]
    param_names = ['T%d arg%d' % (i,i) for i in xrange(n)] 
    if is_void:
        ret_type = 'void'
        name_extra = 'Void'
    else:
        ret_type = 'TRet'
        name_extra = ''
        type_params.append(ret_type)
        
    if type_params: types = '<' + ','.join(type_params) + '>'
    else: types = ''
    
    cw.enter_block('internal %s Run%s%d%s(%s)' % (ret_type, name_extra, n,
                                                types, 
                                                ','.join(param_names)))
        
    cw.enter_block('if (_compiled != null)')
    args = ', '.join(['arg%d' % i for i in xrange(n)])
    if is_void:
        cw.write('((Action%s)_compiled)(%s);' % (types, args))
        cw.write('return;')
    else:
        cw.write('return ((Func%s)_compiled)(%s);' % (types, args))
    cw.exit_block()
    
    cw.write('var frame = PrepareToRun();')
    for i in xrange(n):
        cw.write('frame.Data[%d] = arg%d;' % (i,i))
    
    if n > 0:
        cw.write('frame.BoxLocals();')
    
    if is_void: cw.write('_interpreter.Run(frame);')
    else: cw.write('return (TRet)_interpreter.Run(frame);')
    cw.exit_block()
    
    
def gen_run_methods(cw):
    cw.write('internal const int MaxParameters = %d;' % MAX_TYPES)
    for i in xrange(MAX_TYPES):
        gen_run_method(cw, i, False)
        gen_run_method(cw, i, True)
        


def main():
    return generate(
        ("Dynamic Instructions", gen_instructions),
        ("Dynamic Instruction Types", gen_types),
        ("LightLambda Run Methods", gen_run_methods),
    )

if __name__ == "__main__":
    main()
