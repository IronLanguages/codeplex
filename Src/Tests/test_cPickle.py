#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
#
#  You must not remove this notice, or any other, from this software.
#
######################################################################################

#
# test pickle
#

from lib.assert_util import *

import re
from cStringIO import StringIO

import sys

if sys.platform == 'cli':
    import cPickle
else:
    import pickle as cPickle

def sorted_dict_repr(dict):
    # We do str(k) to force unicode attribute values to be strings. On
    # IronPython this doesn't matter, since everything is unicode. However,
    # this allows us to test against CPython for compatibility.
    return '{%s}' % ', '.join(
        ['%r: %r' % (str(k),v)
            for k,v
            in sorted(dict.items(),
                cmp=lambda a,b: cmp(a[0], b[0])
                )
            ]
        )

class OldClass:
    def __repr__(self):
        if hasattr(self, '__getstate__'):
            state = repr(self.__getstate__())
        else:
            state = sorted_dict_repr(self.__dict__)
        return "<%s instance with state %s>" % (
            self.__class__, state)
class OldClass_GetState(OldClass):
    def __getstate__(self):
        return (u'state1', u'state2')
    def __setstate__(self, state): pass
class OldClass_GetInitArgs(OldClass):
    initargs = (u'arg1', 2, 3)
    def __init__(self, *args):
        AreEqual(args, self.initargs)
    def __getinitargs__(self):
        return self.initargs
class OldClass_GetState_GetInitArgs(OldClass_GetState, OldClass_GetInitArgs): pass

class NewClass(object):
    def __repr__(self):
        if hasattr(self, '__getstate__'):
            state = repr(self.__getstate__())
        else:
            state = sorted_dict_repr(self.__dict__)
        return "<%s instance with state %s>" % (
            type(self).__name__, state)
class NewClass_GetState(NewClass):
    def __getstate__(self):
        return (u'state1', u'state2')
    def __setstate__(self, state): pass
class NewClass_GetNewArgs(NewClass):
    newargs = (u'arg1', 2, 3)
    def __getnewargs__(self):
        return self.newargs
    def __new__(cls, *args):
        AreEqual(args, cls.newargs)
        return NewClass.__new__(cls)
class NewClass_GetState_GetNewArgs(NewClass_GetState, NewClass_GetNewArgs): pass

def global_function(): pass

class NamedObject:
    def __init__(self, name, obj):
        self.name = name
        self.obj = obj

class CLIOnly:
    def __init__(self, obj):
        self.obj = obj

class TestBank:
    def normalize(test):
        obj, expectations = test
        if isinstance(obj, CLIOnly):
            obj = obj.obj
        if isinstance(obj, NamedObject):
            display_name = '<<%s>>' % obj.name
            obj = obj.obj
        else:
            display_name = repr(obj)
        return obj, expectations, display_name
    normalize = staticmethod(normalize)

    hey = u'hey'
    long_smallest_neg = -2**(8*255-1)
    long_largest_pos = 2**(8*255-1)-1
    list_recursive = [1]
    list_recursive.append(list_recursive)
    dict_recursive = {0:u'hey'}
    dict_recursive[1] = dict_recursive
    oldinst0 = OldClass()
    oldinst1 = OldClass()
    oldinst1.name = u'Bob'
    oldinst1.age = 3
    oldinst2 = OldClass_GetState()
    oldinst3 = OldClass_GetState()
    oldinst3.name = u'Bob'
    oldinst4 = OldClass_GetInitArgs(*OldClass_GetInitArgs.initargs)
    oldinst5 = OldClass_GetInitArgs(*OldClass_GetInitArgs.initargs)
    oldinst5.name = u'Bob'
    oldinst6 = OldClass_GetState_GetInitArgs(*OldClass_GetState_GetInitArgs.initargs)
    oldinst7 = OldClass_GetState_GetInitArgs(*OldClass_GetState_GetInitArgs.initargs)
    oldinst7.name = u'Bob'
    newinst0 = NewClass()
    newinst1 = NewClass()
    newinst1.name = u'Bob'
    newinst1.age = 3
    newinst2 = NewClass_GetState()
    newinst3 = NewClass_GetState()
    newinst3.name = u'Bob'
    newinst4 = NewClass_GetNewArgs(*NewClass_GetNewArgs.newargs)
    newinst5 = NewClass_GetNewArgs(*NewClass_GetNewArgs.newargs)
    newinst5.name = u'Bob'
    newinst6 = NewClass_GetState_GetNewArgs(*NewClass_GetState_GetNewArgs.newargs)
    newinst7 = NewClass_GetState_GetNewArgs(*NewClass_GetState_GetNewArgs.newargs)
    newinst7.name = u'Bob'

    # Test description format:
    # (thing_to_pickle, {
    #     protocol_m: expected_output,
    #     protocol_n: (expected_alternative1, expected_alternative2),
    #     })
    # 
    # If thing_to_pickle is a NamedObject instance, then the NamedObject's
    # 'name' attr will be printed and its 'obj' attr will be pickled.
    #
    # If there's only one expected alternative, you can just use that instead
    # of a tuple of possible alternatives.
    #
    # The highest protocol in the test description that is equal to or less than
    # the current protocol is used for matching. For example, the actual output
    # from protocol 2 would be matched with the expected output from protocol 1
    # if only protocols 1 and 0 were specified in the test description.
    #
    # In the expected output strings, any angle bracket-delimited sequence
    # (e.g. "<2>" or "<\x05>") is a wildcard that matches exactly one
    # character when testing the pickler. When testing the unpickler, the angle
    # brackets are stripped and the string is used otherwise unmodified. This
    # is to allow slightly fuzzy matching against memo reference numbers.  As a
    # corrolary, this means that you can't use strings that contain literal
    # angle brackets.

    tests = [
        (None, {
            0:'N.',
            }),
        (True, {
            0:'I01\n.',
            2:'\x88.',
            }),
        (False, {
            0:'I00\n.',
            2:'\x89.',
            }),
        (1, {
            0:'I1\n.',
            1:'K\x01.',
            }),
        (-1, {
            0:'I-1\n.',
            1:'J\xff\xff\xff\xff.',
            }),
        (256, {
            0:'I256\n.',
            1:'M\x00\x01.',
            }),
        (2**30, {
            0:'I1073741824\n.',
            1:'J\x00\x00\x00\x40.',
            }),
        (0L, {
            0:'L0L\n.',
            1:'L0L\n.',
            2:'\x8a\x00.',
            }),
        (1L, {
            0:'L1L\n.',
            1:'L1L\n.',
            2:'\x8a\x01\x01.',
            }),
        (-1L, {
            0:'L-1L\n.',
            1:'L-1L\n.',
            2:'\x8a\x01\xff.',
            }),
        (-1L, {
            0:'L-1L\n.',
            1:'L-1L\n.',
            2:'\x8a\x01\xff.',
            }),
        (NamedObject('smallest negative long', long_smallest_neg), {
            0:'L-63119152483029311134208743532558499922742388026788054750254580913134092068101349400775784006880690358767027267425582069324452263965802580263844047629781802969982182358009757991699604981229789271086050074968881969290609802036366711253590028004836270450354777054758408286889796663166144157436625779538926534222488932401695981290400341380008924794640968818996722769683214178380910532633711551074723814187845931105358601012620815151559279594339152157038471900846264123490479852950820722119447464310412741151715903477845113154386713414751950465264697590604369795983597920768026571572887653525297164440538776584100773888L\n.',
            2:'\x8a\xff\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x80.',
            }),
        (NamedObject('smallest negative long - 1', long_smallest_neg-1), {
            0:'L-63119152483029311134208743532558499922742388026788054750254580913134092068101349400775784006880690358767027267425582069324452263965802580263844047629781802969982182358009757991699604981229789271086050074968881969290609802036366711253590028004836270450354777054758408286889796663166144157436625779538926534222488932401695981290400341380008924794640968818996722769683214178380910532633711551074723814187845931105358601012620815151559279594339152157038471900846264123490479852950820722119447464310412741151715903477845113154386713414751950465264697590604369795983597920768026571572887653525297164440538776584100773889L\n.',
            2:'\x8b\x00\x01\x00\x00\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\x7f\xff.',
            }),
        (NamedObject('largest positive long', long_largest_pos), {
            0:'L63119152483029311134208743532558499922742388026788054750254580913134092068101349400775784006880690358767027267425582069324452263965802580263844047629781802969982182358009757991699604981229789271086050074968881969290609802036366711253590028004836270450354777054758408286889796663166144157436625779538926534222488932401695981290400341380008924794640968818996722769683214178380910532633711551074723814187845931105358601012620815151559279594339152157038471900846264123490479852950820722119447464310412741151715903477845113154386713414751950465264697590604369795983597920768026571572887653525297164440538776584100773887L\n.',
            2:'\x8a\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\x7f.',
            }),
        (NamedObject('largest positive long + 1', long_largest_pos+1), {
            0:'L63119152483029311134208743532558499922742388026788054750254580913134092068101349400775784006880690358767027267425582069324452263965802580263844047629781802969982182358009757991699604981229789271086050074968881969290609802036366711253590028004836270450354777054758408286889796663166144157436625779538926534222488932401695981290400341380008924794640968818996722769683214178380910532633711551074723814187845931105358601012620815151559279594339152157038471900846264123490479852950820722119447464310412741151715903477845113154386713414751950465264697590604369795983597920768026571572887653525297164440538776584100773888L\n.',
            2:'\x8b\x00\x01\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x80\x00.',
            }),
        (0.0, {
            0:( 'F0.0\n.',
                'F0\n.',
                ),
            1:'G\x00\x00\x00\x00\x00\x00\x00\x00.',
            }),
        (1.123, {
            0:'F1.123\n.',
            1:'G?\xf1\xf7\xce\xd9\x16\x87+.',
            }),
        (-1.123, {
            0:'F-1.123\n.',
            1:'G\xbf\xf1\xf7\xce\xd9\x16\x87+.',
            }),
        (500.5, {
            0:'F500.5\n.',
            1:'G@\x7fH\x00\x00\x00\x00\x00.',
            }),
        (1e+017, {
            0:'F1e+017\n.',
            1:'GCv4W\x85\xd8\xa0\x00.',
            }),
        (1e-014, {
            0:'F1e-014\n.',
            1:'G=\x06\x84\x9b\x86\xa1+\x9b.',
            }),
        (u'hey\x00hey', {
            0:'Vhey\x00hey\np<0>\n.',
            1:'X\x07\x00\x00\x00hey\x00heyq<\x00>.',
            }),
        (u'hey\xffhey', {
            0:'Vhey\xffhey\np<0>\n.',
            1:'X\x08\x00\x00\x00hey\xc3\xbfheyq<\x00>.',
            }),
        (u'hey\u0100hey', {
            0:'Vhey\\u0100hey\np<0>\n.',
            1:'X\x08\x00\x00\x00hey\xc4\x80heyq<\x00>.',
            }),
        (u'hey\u07ffhey', {
            0:( 'Vhey\\u07ffhey\np<0>\n.',
                'Vhey\\u07FFhey\np<0>\n.',
                ),
            1:'X\x08\x00\x00\x00hey\xdf\xbfheyq<\x00>.',
            }),
        (u'hey\u0800hey', {
            0:'Vhey\\u0800hey\np<0>\n.',
            1:'X\t\x00\x00\x00hey\xe0\xa0\x80heyq<\x00>.',
            }),
        (u'hey\uffffhey', {
            0:( 'Vhey\\uffffhey\np<0>\n.',
                'Vhey\\uFFFFhey\np<0>\n.',
                ),
            1:'X\t\x00\x00\x00hey\xef\xbf\xbfheyq<\x00>.',
            }),
        ((), {
            0:'(t.',
            1:').',
            }),
        ((7,), {
            0:'(I7\ntp<0>\n.',
            1:'(K\x07tq<\x00>.',
            2:'K\x07\x85q<\x00>.',
            }),
        ((7,8), {
            0:'(I7\nI8\ntp<0>\n.',
            1:'(K\x07K\x08tq<\x00>.',
            2:'K\x07K\x08\x86q<\x00>.',
            }),
        ((7,8,9), {
            0:'(I7\nI8\nI9\ntp<0>\n.',
            1:'(K\x07K\x08K\ttq<\x00>.',
            2:'K\x07K\x08K\t\x87q<\x00>.',
            }),
        ((7,8,9,10), {
            0:'(I7\nI8\nI9\nI10\ntp<0>\n.',
            1:'(K\x07K\x08K\tK\ntq<\x00>.',
            2:'(K\x07K\x08K\tK\ntq<\x00>.',
            }),
        ((hey, hey), {
            0:'(Vhey\np<0>\ng<0>\ntp<1>\n.',
            1:'(X\x03\x00\x00\x00heyq<\x00>h<\x00>tq<\x01>.',
            2:'X\x03\x00\x00\x00heyq<\x00>h<\x00>\x86q<\x01>.',
            }),
        ([], {
            0:'(lp<0>\n.',
            1:']q<\x00>.',
            }),
        ([5], {
            0:'(lp<0>\nI5\na.',
            1:']q<\x00>K\x05a.',
            }),
        ([5,6], {
            0:'(lp<0>\nI5\naI6\na.',
            1:']q<\x00>(K\x05K\x06e.',
            }),
        (CLIOnly(list(range(10))), {
            0:'(lp<0>\nI0\naI1\naI2\naI3\naI4\naI5\naI6\naI7\naI8\naI9\na.',
            1:']q<\x00>(K\x00K\x01K\x02K\x03K\x04K\x05K\x06K\x07K\x08K\te.',
            }),
        (CLIOnly(list(range(11))), {
            0:'(lp<0>\nI0\naI1\naI2\naI3\naI4\naI5\naI6\naI7\naI8\naI9\naI10\na.',
            1:']q<\x00>(K\x00K\x01K\x02K\x03K\x04K\x05K\x06K\x07K\x08K\teK\na.',
            }),
        (CLIOnly(list(range(12))), {
            0:'(lp<0>\nI0\naI1\naI2\naI3\naI4\naI5\naI6\naI7\naI8\naI9\naI10\naI11\na.',
            1:']q<\x00>(K\x00K\x01K\x02K\x03K\x04K\x05K\x06K\x07K\x08K\te(K\nK\x0be.',
            }),
        ([hey, hey], {
            0:'(lp<0>\nVhey\np<1>\nag<1>\na.',
            1:']q<\x00>(X\x03\x00\x00\x00heyq<\x01>h<\x01>e.',
            }),
        ({}, {
            0:'(dp<0>\n.',
            1:'}q<\x00>.',
            }),
        ({1:2}, {
            0:'(dp<0>\nI1\nI2\ns.',
            1:'}q<\x00>K\x01K\x02s.',
            }),
        ({1:2, 3:4}, {
            0:'(dp<0>\nI1\nI2\nsI3\nI4\ns.',
            1:'}q<\x00>(K\x01K\x02K\x03K\x04u.',
            }),
        (dict([(x,x) for x in range(10)]), {
            0:'(dp<0>\nI0\nI0\nsI1\nI1\nsI2\nI2\nsI3\nI3\nsI4\nI4\nsI5\nI5\nsI6\nI6\nsI7\nI7\nsI8\nI8\nsI9\nI9\ns.',
            1:'}q<\x00>(K\x00K\x00K\x01K\x01K\x02K\x02K\x03K\x03K\x04K\x04K\x05K\x05K\x06K\x06K\x07K\x07K\x08K\x08K\tK\tu.',
            }),
        (CLIOnly(dict([(x,x) for x in range(11)])), {
            0:'(dp<0>\nI0\nI0\nsI1\nI1\nsI2\nI2\nsI3\nI3\nsI4\nI4\nsI5\nI5\nsI6\nI6\nsI7\nI7\nsI8\nI8\nsI9\nI9\nsI10\nI10\ns.',
            1:'}q<\x00>(K\x00K\x00K\x01K\x01K\x02K\x02K\x03K\x03K\x04K\x04K\x05K\x05K\x06K\x06K\x07K\x07K\x08K\x08K\tK\tuK\nK\ns.',
            }),
        (CLIOnly(dict([(x,x) for x in range(12)])), {
            0:'(dp<0>\nI0\nI0\nsI1\nI1\nsI2\nI2\nsI3\nI3\nsI4\nI4\nsI5\nI5\nsI6\nI6\nsI7\nI7\nsI8\nI8\nsI9\nI9\nsI10\nI10\nsI11\nI11\ns.',
            1:'}q<\x00>(K\x00K\x00K\x01K\x01K\x02K\x02K\x03K\x03K\x04K\x04K\x05K\x05K\x06K\x06K\x07K\x07K\x08K\x08K\tK\tu(K\nK\nK\x0bK\x0bu.',
            }),
        ({hey: hey}, {
            0:'(dp<0>\nVhey\np<1>\ng<1>\ns.',
            1:'}q<\x00>X\x03\x00\x00\x00heyq<\x01>h<\x01>s.',
            }),
        (list_recursive, {
            0:'(lp<0>\nI1\nag<0>\na.',
            1:']q<\x00>(K\x01h<\x00>e.',
            }),
        (dict_recursive, {
            0:'(dp<0>\nI0\nVhey\np<1>\nsI1\ng<0>\ns.',
            1:'}q<\x00>(K\x00X\x03\x00\x00\x00heyq<\x01>K\x01h<\x00>u.',
            }),

        (OldClass, {
            0:'c%s\nOldClass\np<0>\n.' % (__name__,),
            1:'c%s\nOldClass\nq<\x00>.' % (__name__,),
            }),
        (NewClass, {
            0:'c%s\nNewClass\np<0>\n.' % (__name__,),
            1:'c%s\nNewClass\nq<\x00>.' % (__name__,),
            }),
        (global_function, {
            0:'c%s\nglobal_function\np<0>\n.' % (__name__,),
            1:'c%s\nglobal_function\nq<\x00>.' % (__name__,),
            }),
        (len, {
            0:'c__builtin__\nlen\np<0>\n.',
            1:'c__builtin__\nlen\nq<\x00>.',
            }),

        (oldinst0, {
            0:'(i%s\nOldClass\np<0>\n(dp<1>\nb.' % (__name__,),
            1:'(c%s\nOldClass\nq<\x00>oq<\x01>}q<\x02>b.' % (__name__,),
            }),
        (oldinst1, {
            # variants w/ unicode and string hash keys, in different hash orders
            0:( '(i%s\nOldClass\np<0>\n(dp<1>\nVage\np<2>\nI3\nsVname\np<3>\nVBob\np<4>\nsb.' % (__name__,),
                '(i%s\nOldClass\np<0>\n(dp<1>\nVname\np<2>\nVBob\np<3>\nsVage\np<4>\nI3\nsb.' % (__name__,),
                "(i%s\nOldClass\np<0>\n(dp<1>\nS'age'\np<2>\nI3\nsS'name'\np<3>\nVBob\np<4>\nsb." % (__name__,),
                "(i%s\nOldClass\np<0>\n(dp<1>\nS'name'\np<2>\nVBob\np<3>\nsS'age'\np<4>\nI3\nsb." % (__name__,),
                ),
            1:( '(c%s\nOldClass\nq<\x00>oq<\x01>}q<\x02>(X\x03\x00\x00\x00ageq<\x03>K\x03X\x04\x00\x00\x00nameq<\x04>X\x03\x00\x00\x00Bobq<\x05>ub.' % (__name__,),
                '(c%s\nOldClass\nq<\x00>oq<\x01>}q<\x02>(X\x04\x00\x00\x00nameq<\x03>X\x03\x00\x00\x00Bobq<\x04>X\x03\x00\x00\x00ageq<\x05>K\x03ub.' % (__name__,),
                '(c%s\nOldClass\nq<\x00>oq<\x01>}q<\x02>(U\x04nameq<\x03>X\x03\x00\x00\x00Bobq<\x04>U\x03ageq<\x05>K\x03ub.' % (__name__,),
                '(c%s\nOldClass\nq<\x00>oq<\x01>}q<\x02>(U\x03ageq<\x03>K\x03U\x04nameq<\x04>X\x03\x00\x00\x00Bobq<\x05>ub.' % (__name__,),
                ),
            }),
        (oldinst2, {
            0:'(i%s\nOldClass_GetState\np<0>\n(Vstate1\np<1>\nVstate2\np<2>\ntp<3>\nb.' % (__name__,),
            1:'(c%s\nOldClass_GetState\nq<\x00>oq<\x01>(X\x06\x00\x00\x00state1q<\x02>X\x06\x00\x00\x00state2q<\x03>tq<\x04>b.' % (__name__,),
            2:'(c%s\nOldClass_GetState\nq<\x00>oq<\x01>X\x06\x00\x00\x00state1q<\x02>X\x06\x00\x00\x00state2q<\x03>\x86q<\x04>b.' % (__name__,),
            }),
        (oldinst3, {
            0:'(i%s\nOldClass_GetState\np<0>\n(Vstate1\np<1>\nVstate2\np<2>\ntp<3>\nb.' % (__name__,),
            1:'(c%s\nOldClass_GetState\nq<\x00>oq<\x01>(X\x06\x00\x00\x00state1q<\x02>X\x06\x00\x00\x00state2q<\x03>tq<\x04>b.' % (__name__,),
            2:'(c%s\nOldClass_GetState\nq<\x00>oq<\x01>X\x06\x00\x00\x00state1q<\x02>X\x06\x00\x00\x00state2q<\x03>\x86q<\x04>b.' % (__name__,),
            }),
        (oldinst4, {
            0:'(Varg1\np<0>\nI2\nI3\ni%s\nOldClass_GetInitArgs\np<1>\n(dp<2>\nb.' % (__name__,),
            1:'(c%s\nOldClass_GetInitArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03oq<\x02>}q<\x03>b.' % (__name__,),
            }),
        (oldinst5, {
            0:( "(Varg1\np<0>\nI2\nI3\ni%s\nOldClass_GetInitArgs\np<1>\n(dp<2>\nS'name'\np<3>\nVBob\np<4>\nsb." % (__name__,),
                '(Varg1\np<0>\nI2\nI3\ni%s\nOldClass_GetInitArgs\np<1>\n(dp<2>\nVname\np<3>\nVBob\np<4>\nsb.' % (__name__,),
                ),
            1:( '(c%s\nOldClass_GetInitArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03oq<\x02>}q<\x03>U\x04nameq<\x04>X\x03\x00\x00\x00Bobq<\x05>sb.' % (__name__,),
                '(c%s\nOldClass_GetInitArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03oq<\x02>}q<\x03>X\x04\x00\x00\x00nameq<\x04>X\x03\x00\x00\x00Bobq<\x05>sb.' % (__name__,),
                ),
            }),
        (oldinst6, {
            0:'(Varg1\np<0>\nI2\nI3\ni%s\nOldClass_GetState_GetInitArgs\np<1>\n(Vstate1\np<2>\nVstate2\np<3>\ntp<4>\nb.' % (__name__,),
            1:'(c%s\nOldClass_GetState_GetInitArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03oq<\x02>(X\x06\x00\x00\x00state1q<\x03>X\x06\x00\x00\x00state2q<\x04>tq<\x05>b.' % (__name__,),
            2:'(c%s\nOldClass_GetState_GetInitArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03oq<\x02>X\x06\x00\x00\x00state1q<\x03>X\x06\x00\x00\x00state2q<\x04>\x86q<\x05>b.' % (__name__,),
            }),
        (oldinst7, {
            0:'(Varg1\np<0>\nI2\nI3\ni%s\nOldClass_GetState_GetInitArgs\np<1>\n(Vstate1\np<2>\nVstate2\np<3>\ntp<4>\nb.' % (__name__,),
            1:'(c%s\nOldClass_GetState_GetInitArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03oq<\x02>(X\x06\x00\x00\x00state1q<\x03>X\x06\x00\x00\x00state2q<\x04>tq<\x05>b.' % (__name__,),
            2:'(c%s\nOldClass_GetState_GetInitArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03oq<\x02>X\x06\x00\x00\x00state1q<\x03>X\x06\x00\x00\x00state2q<\x04>\x86q<\x05>b.' % (__name__,),
            }),
        (CLIOnly(newinst0), {
            0:'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n.' % (__name__,),
            1:'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>.' % (__name__,),
            2:'c%s\nNewClass\nq<\x00>)\x81q<\x01>}q<\x02>b.' % (__name__,),
            }),
        (CLIOnly(newinst1), {
            0:( 'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n(dp<5>\nVname\np<6>\nVBob\np<7>\nsVage\np<8>\nI3\nsb.' % (__name__,),
                'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n(dp<5>\nVage\np<6>\nI3\nsVname\np<7>\nVBob\np<8>\nsb.' % (__name__,),
                'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n(dp<5>\nS\'name\'\np<6>\nVBob\np<7>\nsS\'age\'\np<8>\nI3\nsb.' % (__name__,),
                'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n(dp<5>\nS\'age\'\np<6>\nI3\nsS\'name\'\np<7>\nVBob\np<8>\nsb.' % (__name__,),
                ),
            1:( 'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>}q<\x05>(X\x04\x00\x00\x00nameq<\x06>X\x03\x00\x00\x00Bobq<\x07>X\x03\x00\x00\x00ageq<\x08>K\x03ub.' % (__name__,),
                'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>}q<\x05>(X\x03\x00\x00\x00ageq<\x06>K\x03X\x04\x00\x00\x00nameq<\x07>X\x03\x00\x00\x00Bobq<\x08>ub.' % (__name__,),
                'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>}q<\x05>(U\x04nameq<\x06>X\x03\x00\x00\x00Bobq<\x07>U\x03ageq<\x08>K\x03ub.' % (__name__,),
                'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>}q<\x05>(U\x03ageq<\x06>K\x03U\x04nameq<\x07>X\x03\x00\x00\x00Bobq<\x08>ub.' % (__name__,),
                ),
            2:( 'c%s\nNewClass\nq<\x00>)\x81q<\x01>}q<\x02>(X\x04\x00\x00\x00nameq<\x03>X\x03\x00\x00\x00Bobq<\x04>X\x03\x00\x00\x00ageq<\x05>K\x03ub.' % (__name__,),
                'c%s\nNewClass\nq<\x06>)\x81q<\x07>}q<\x08>(X\x03\x00\x00\x00ageq<\x00>K\x03X\x04\x00\x00\x00nameq<\x01>X\x03\x00\x00\x00Bobq<\x02>ub.' % (__name__,),
                'c%s\nNewClass\nq<\x03>)\x81q<\x04>}q<\x05>(U\x04nameq<\x06>X\x03\x00\x00\x00Bobq<\x07>U\x03ageq<\x08>K\x03ub.' % (__name__,),
                'c%s\nNewClass\nq<\x00>)\x81q<\x01>}q<\x02>(U\x03ageq<\x03>K\x03U\x04nameq<\x04>X\x03\x00\x00\x00Bobq<\x05>ub.' % (__name__,),
                ),
            }),
        (CLIOnly(newinst2), {
            0:'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass_GetState\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n(Vstate1\np<5>\nVstate2\np<6>\ntp<7>\nb.' % (__name__,),
            1:'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass_GetState\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>(X\x06\x00\x00\x00state1q<\x05>X\x06\x00\x00\x00state2q<\x06>tq<\x07>b.' % (__name__,),
            2:'c%s\nNewClass_GetState\nq<\x00>)\x81q<\x01>X\x06\x00\x00\x00state1q<\x02>X\x06\x00\x00\x00state2q<\x03>\x86q<\x04>b.' % (__name__,),
            }),
        (CLIOnly(newinst3), {
            0:'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass_GetState\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n(Vstate1\np<5>\nVstate2\np<6>\ntp<7>\nb.' % (__name__,),
            1:'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass_GetState\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>(X\x06\x00\x00\x00state1q<\x05>X\x06\x00\x00\x00state2q<\x06>tq<\x07>b.' % (__name__,),
            2:'c%s\nNewClass_GetState\nq<\x00>)\x81q<\x01>X\x06\x00\x00\x00state1q<\x02>X\x06\x00\x00\x00state2q<\x03>\x86q<\x04>b.' % (__name__,),
            }),
        (CLIOnly(newinst4), {
            0:'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass_GetNewArgs\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n.' % (__name__,),
            1:'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass_GetNewArgs\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>.' % (__name__,),
            2:'c%s\nNewClass_GetNewArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03\x87q<\x02>\x81q<\x03>}q<\x04>b.' % (__name__,),
            }),
        (CLIOnly(newinst5), {
            0:( 'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass_GetNewArgs\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n(dp<5>\nS\'name\'\np<6>\nVBob\np<7>\nsb.' % (__name__,),
                'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass_GetNewArgs\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n(dp<5>\nVname\np<6>\nVBob\np<7>\nsb.' % (__name__,),
                ),
            1:( 'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass_GetNewArgs\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>}q<\x05>U\x04nameq<\x06>X\x03\x00\x00\x00Bobq<\x07>sb.' % (__name__,),
                'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass_GetNewArgs\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>}q<\x05>X\x04\x00\x00\x00nameq<\x06>X\x03\x00\x00\x00Bobq<\x07>sb.' % (__name__,),
                ),
            2:( 'c%s\nNewClass_GetNewArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03\x87q<\x02>\x81q<\x03>}q<\x04>U\x04nameq<\x05>X\x03\x00\x00\x00Bobq<\x06>sb.' % (__name__,),
                'c%s\nNewClass_GetNewArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03\x87q<\x02>\x81q<\x03>}q<\x04>X\x04\x00\x00\x00nameq<\x05>X\x03\x00\x00\x00Bobq<\x06>sb.' % (__name__,),
                ),
            }),
        (CLIOnly(newinst6), {
            0:'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass_GetState_GetNewArgs\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n(Vstate1\np<5>\nVstate2\np<6>\ntp<7>\nb.' % (__name__,),
            1:'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass_GetState_GetNewArgs\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>(X\x06\x00\x00\x00state1q<\x05>X\x06\x00\x00\x00state2q<\x06>tq<\x07>b.' % (__name__,),
            2:'c%s\nNewClass_GetState_GetNewArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03\x87q<\x02>\x81q<\x03>X\x06\x00\x00\x00state1q<\x04>X\x06\x00\x00\x00state2q<\x05>\x86q<\x06>b.' % (__name__,),
            }),
        (CLIOnly(newinst7), {
            0:'ccopy_reg\n_reconstructor\np<0>\n(c%s\nNewClass_GetState_GetNewArgs\np<1>\nc__builtin__\nobject\np<2>\nNtp<3>\nRp<4>\n(Vstate1\np<5>\nVstate2\np<6>\ntp<7>\nb.' % (__name__,),
            1:'ccopy_reg\n_reconstructor\nq<\x00>(c%s\nNewClass_GetState_GetNewArgs\nq<\x01>c__builtin__\nobject\nq<\x02>Ntq<\x03>Rq<\x04>(X\x06\x00\x00\x00state1q<\x05>X\x06\x00\x00\x00state2q<\x06>tq<\x07>b.' % (__name__,),
            2:'c%s\nNewClass_GetState_GetNewArgs\nq<\x00>X\x04\x00\x00\x00arg1q<\x01>K\x02K\x03\x87q<\x02>\x81q<\x03>X\x06\x00\x00\x00state1q<\x04>X\x06\x00\x00\x00state2q<\x05>\x86q<\x06>b.' % (__name__,),
            }),
        ]

def test_pickler(module=cPickle, verbose=True):
    def get_expected(expectations, proto):
        while proto >= 0:
            try:
                return expectations[proto]
            except KeyError:
                proto -= 1

        raise RuntimeError(
            "no expectation found in %r for protocol %d" % (expectations, proto)
            )

    def match(pattern, text):
        patterns = re.split(r'<.*?>', pattern)
        curpos = 0

        if len(text) != sum([len(p) for p in patterns]) + len(patterns) - 1: return False

        for pattern in patterns:
            if text[curpos:curpos+len(pattern)] != pattern:
                return False
            curpos += len(pattern) + 1

        return True

    Assert(match('a', 'a'))
    Assert(not match('a<1>', 'a'))
    Assert(match('a<1>', 'ab'))
    Assert(not match('a', 'b'))
    Assert(match('a<1>c', 'abc'))
    Assert(match('a<1>c', 'abc'))
    Assert(match('<1>', 'a'))
    Assert(match('<1><1>', 'xy'))
    Assert(not match('<1><1>', 'xyz'))

    s = StringIO()

    picklers = [
        module.Pickler(s, protocol=0),
        module.Pickler(s, protocol=1),
        module.Pickler(s, protocol=2),
        ]

    if sys.platform.startswith('cli'):
        for p in picklers:
            # This lets us test batched SETITEMS and APPENDS without generating
            # huge (1000-item) datasets
            p._BATCHSIZE = 10

    for test in TestBank.tests:
        obj, expectations = test
        if isinstance(obj, CLIOnly):
            if not sys.platform.startswith('cli'):
                continue
            else:
                obj = obj.obj

        if isinstance(obj, NamedObject):
            display_name = '<<%s>>' % obj.name
            obj = obj.obj
        else:
            display_name = repr(obj)

        if verbose: print "Testing %s..." % display_name,
        for proto in range(len(picklers)):
            pickler = picklers[proto]

            s.truncate(0)
            pickler.clear_memo()
            pickler.dump(obj)
            if verbose: print proto,
            expected = get_expected(expectations, proto)
            if not isinstance(expected, tuple):
                expected = (expected,)
            actual = s.getvalue()
            if 2 == proto:
                # ignore protocol opcodes so that we can use the same literal
                # representation for protocols 1 and 2 in the test bank
                actual = actual[2:]
            for pattern in expected:
                if match(pattern, actual):
                    break
            else:
                if len(expected) == 1:
                    Fail('expected\n%r, got\n%r' % (expected[0], actual))
                else:
                    Fail('expected one of\n%r, got\n%r' % (expected, actual))

        if verbose: print 'ok'


    if verbose: print "Tests completed"

def test_unpickler(module=cPickle, verbose=True):
    s = StringIO()
    for test in TestBank.tests:
        obj, pickle_lists, display_name = TestBank.normalize(test)
        expected = repr(obj)

        if verbose: print "Testing %s..." % display_name,

        for proto in range(3):
            if proto not in pickle_lists:
                continue

            if verbose: print proto,

            pickles = pickle_lists[proto]
            if not isinstance(pickles, tuple):
                pickles = (pickles,)

            pickle_num = 0

            for pickle in pickles:
                unpickler = module.Unpickler(s)
                s.truncate(0)
                s.write(pickle.replace('<', '').replace('>', ''))
                s.seek(0)
                try:
                    unpickled_obj = unpickler.load()
                except NotImplementedError:
                    print "SKIPPED",
                else:
                    actual = repr(unpickled_obj)

                    if expected != actual:
                        print 
                        Fail('Wrong unpickled value:\n'
                            'with pickle %d %r\n'
                            'expected\n'
                            '%r, got\n'
                            '%r' % (pickle_num, pickle, expected, actual))

                pickle_num += 1

        if verbose: print 'ok'

run_test(__name__)
