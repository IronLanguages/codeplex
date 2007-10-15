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

#
# test codecs
#

'''
TODO - essentially all the tests currently here are barebones sanity checks
to ensure a minimal level of functionality exists. In other words, there are 
many special cases that are not being covered *yet*.

Disabled Silverlight tests are due to Rowan #304084
'''

from lib.assert_util import *

if is_cli or is_silverlight:
    import _codecs as codecs
else:
    import codecs


#-----------------------
#--GLOBALS

def test_escape_decode():
    '''
    '''
    #sanity checks

    value, length = codecs.escape_decode("ab\a\b\t\n\r\f\vba")
    AreEqual(value, 'ab\x07\x08\t\n\r\x0c\x0bba')
    AreEqual(length, 11)
    
    value, length = codecs.escape_decode("\\a")
    AreEqual(value, '\x07')
    AreEqual(length, 2)
    
    #BUG
    #value, length = codecs.escape_decode("\\\a")
    #AreEqual(value, '\\\x07')
    #AreEqual(length, 2)

    #BUG
    #AreEqual("abc", codecs.escape_decode("abc", None)[0])
    
def test_escape_encode():
    '''
    '''
    #sanity checks

    #DWF-this function is totally broken
    #BUG
    #value, length = codecs.escape_encode("abba")
    #AreEqual(value, "abba")
    #AreEqual(length, 4)
    
    #BUG
    #value, length = codecs.escape_encode("ab\a\b\t\n\r\f\vba")
    #AreEqual(value, 'ab\\x07\\x08\\t\\n\\r\\x0c\\x0bba')
    #AreEqual(length, 26)
    
    #BUG
    #value, length = codecs.escape_encode("\\a")
    #AreEqual(value, "\\\\a")
    #AreEqual(length, 3)


@skip('silverlight')
def test_register_error():
        '''
        TODO: test that these are actually used.
        '''
        #Sanity
        def garbage_error0(): print "garbage_error0"
        def garbage_error1(param1): print "garbage_error1:", param1
        def garbage_error2(param1, param2): print "garbage_error2:", param1, "; ", param2
        
        codecs.register_error("garbage0", garbage_error0)
        codecs.register_error("garbage1", garbage_error1)
        codecs.register_error("garbage2", garbage_error2)
        codecs.register_error("garbage1dup", garbage_error1)

@skip('silverlight') # different result on Silverlight
def test_utf_16_ex_decode():
    '''
    '''
    #sanity
    new_str, size, zero = codecs.utf_16_ex_decode("abc")
    AreEqual(new_str, u'\u6261')
    AreEqual(size, 2)
    AreEqual(zero, 0)
    
def test_charmap_decode():
    '''
    '''
    new_str, size = codecs.charmap_decode("abc")
    AreEqual(new_str, u'abc')
    AreEqual(size, 3)
    
def test_decode():
    '''
    '''
    #sanity
    #BUG - LookupError: unknown encoding: us_ascii
    #new_str = codecs.decode("abc")
    #AreEqual(new_str, u'abc')
    
def test_encode():
    '''
    '''
    #sanity
    #BUG - LookupError: unknown encoding: us_ascii
    #new_str = codecs.encode("abc")
    #AreEqual(new_str, 'abc')

def test_raw_unicode_escape_decode():
    '''
    '''
    #sanity
    new_str, size = codecs.raw_unicode_escape_decode("abc")
    AreEqual(new_str, u'abc')
    AreEqual(size, 3)

def test_raw_unicode_escape_encode():
    '''
    '''
    #sanity
    new_str, size = codecs.raw_unicode_escape_encode("abc")
    AreEqual(new_str, 'abc')
    AreEqual(size, 3)

@skip('silverlight')
def test_utf_7_decode():
    '''
    '''
    #sanity
    new_str, size = codecs.utf_7_decode("abc")
    AreEqual(new_str, u'abc')
    AreEqual(size, 3)

@skip('silverlight')
def test_utf_7_encode():
    '''
    '''
    #sanity
    new_str, size = codecs.utf_7_encode("abc")
    AreEqual(new_str, 'abc')
    AreEqual(size, 3)

def test_ascii_decode():
    '''
    '''
    #sanity
    new_str, size = codecs.ascii_decode("abc")
    AreEqual(new_str, u'abc')
    AreEqual(size, 3)

def test_ascii_encode():
    '''
    '''
    #sanity
    new_str, size = codecs.ascii_encode("abc")
    AreEqual(new_str, 'abc')
    AreEqual(size, 3)

@skip('silverlight')
def test_latin_1_decode():
    '''
    '''
    #sanity
    new_str, size = codecs.latin_1_decode("abc")
    AreEqual(new_str, u'abc')
    AreEqual(size, 3)

@skip('silverlight')
def test_latin_1_encode():
    '''
    '''
    #sanity
    new_str, size = codecs.latin_1_encode("abc")
    AreEqual(new_str, 'abc')
    AreEqual(size, 3)

@skip('silverlight')
def test_lookup_error():
    '''
    '''
    #sanity
    AssertError(LookupError, codecs.lookup_error, "blah garbage xyz")
    def garbage_error1(someError): pass
    codecs.register_error("blah garbage xyz", garbage_error1)
    AreEqual(codecs.lookup_error("blah garbage xyz"), garbage_error1)
    def garbage_error2(someError): pass
    codecs.register_error("some other", garbage_error2)
    AreEqual(codecs.lookup_error("some other"), garbage_error2)


def test_register():
    '''
    TODO: test that functions passed in are actually used
    '''
    #sanity check - basically just ensure that functions can be registered
    def garbage_func0(): pass
    def garbage_func1(param1): pass
    codecs.register(garbage_func0)
    codecs.register(garbage_func1)
    
    #negative cases
    AssertError(TypeError, codecs.register)
    AssertError(TypeError, codecs.register, None)
    AssertError(TypeError, codecs.register, ())
    AssertError(TypeError, codecs.register, [])
    AssertError(TypeError, codecs.register, 1)
    AssertError(TypeError, codecs.register, "abc")
    AssertError(TypeError, codecs.register, 3.14)

def test_unicode_internal_encode():
    '''
    '''
    # takes one or two parameters, not zero or three
    AssertError(TypeError, codecs.unicode_internal_encode)
    AssertError(TypeError, codecs.unicode_internal_encode, 'abc', 'def', 'qrt')

@skip('silverlight')
def test_utf_16_be_decode():
    '''
    '''
    #sanity
    new_str, size = codecs.utf_16_be_decode("abc")
    AreEqual(new_str, u'\u6162')
    AreEqual(size, 2)

def test_utf_16_be_encode():
    '''
    '''
    #sanity
    new_str, size = codecs.utf_16_be_encode("abc")
    AreEqual(new_str, '\x00a\x00b\x00c')
    AreEqual(size, 3)
    
@skip('silverlight')
def test_utf_16_decode():
    '''
    '''
    #sanity
    new_str, size = codecs.utf_16_decode("abc")
    AreEqual(new_str, u'\u6261')
    AreEqual(size, 2)

@skip('silverlight')
def test_utf_16_le_decode():
    '''
    '''
    #sanity
    new_str, size = codecs.utf_16_le_decode("abc")
    AreEqual(new_str, u'\u6261')
    AreEqual(size, 2)

def test_utf_16_le_encode():
    '''
    '''
    #sanity
    new_str, size = codecs.utf_16_le_encode("abc")
    AreEqual(new_str, 'a\x00b\x00c\x00')
    AreEqual(size, 3)

def test_utf_8_decode():
    '''
    '''
    #sanity
    new_str, size = codecs.utf_8_decode("abc")
    AreEqual(new_str, u'abc')
    AreEqual(size, 3)

def test_utf_8_encode():
    '''
    '''
    #sanity
    new_str, size = codecs.utf_8_encode("abc")
    AreEqual(new_str, 'abc')
    AreEqual(size, 3)

def test_charbuffer_encode():
    '''
    '''
    #BUG - function takes one parameter, not 0
    #if is_cli:
    #    AssertError(NotImplementedError, codecs.charbuffer_encode, "abc")

def test_mbcs_decode():
    '''
    '''
    #BUG - function takes one parameter, not 0
    #if is_cli:
    #    AssertError(NotImplementedError, codecs.mbcs_decode, "abc")

def test_mbcs_encode():
    '''
    '''
    #BUG - function takes one parameter, not 0
    #if is_cli:
    #    AssertError(NotImplementedError, codecs.mbcs_encode, "abc")

def test_readbuffer_encode():
    '''
    '''
    #BUG - function takes one parameter, not 0
    #if is_cli:
    #    AssertError(NotImplementedError, codecs.readbuffer_encode, "abc")

def test_unicode_escape_decode():
    '''
    '''
    #BUG - function takes one parameter
    #if is_cli:
    #    AssertError(NotImplementedError, codecs.unicode_escape_decode, "abc")

def test_unicode_escape_encode():
    '''
    '''
    #BUG - function takes one parameter, not 0
    #if is_cli:
    #    AssertError(NotImplementedError, codecs.unicode_escape_encode, "abc")

def test_misc_encodings():
    if not is_silverlight:
        # codec not available on silverlight
        AreEqual('abc'.encode('utf-16'), '\xff\xfea\x00b\x00c\x00')
        AreEqual('abc'.encode('utf-16-be'), '\x00a\x00b\x00c')
    AreEqual('abc'.encode('unicode-escape'), 'abc')
    AreEqual('abc\u1234'.encode('unicode-escape'), 'abc\\\\u1234')

@disabled("CodePlex Work Item 3094")
def test_file_encodings():
    '''
    Once this gets fixed, we should use *.py files in the correct encoding instead
    of dynamically generating ASCII files.  Also, need variations on the encoding 
    names.
    '''
    import sys
    import nt
    sys.path.append(nt.getcwd() + "\\tmp_encodings")
    try:
        nt.mkdir(nt.getcwd() + "\\tmp_encodings")
    except:
        pass
    
    #positive cases
    for coding in [ 'cp1252','ascii', 'utf-8', 'utf-16', 'latin-1', 'iso-8859-1', 'utf-16-le', 'utf-16-be', 'unicode-escape', 'raw-unicode-escape']:
        temp_mod_name = "test_encoding_" + coding.replace("-", "_")
        f = open(nt.getcwd() + "\\tmp_encodings\\" + temp_mod_name + ".py", 
                 "w")
        f.write("# coding: %s" % (coding))
        f.close()
        __import__(temp_mod_name)
               
    #negative case               
    f = open(nt.getcwd() + "\\tmp_encodings\\" + "bad_encoding.py", "w")
    f.write("# coding: bad")
    f.close() 
    AssertError(SyntaxError, __import__, "bad_encoding")
    
    #cleanup
    sys.path.remove(nt.getcwd() + "\\tmp_encodings")           
    
run_test(__name__)
