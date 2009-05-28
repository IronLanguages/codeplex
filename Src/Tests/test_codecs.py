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

#
# test codecs
#

'''
TODO - essentially all the tests currently here are barebones sanity checks
to ensure a minimal level of functionality exists. In other words, there are
many special cases that are not being covered *yet*.

Disabled Silverlight tests are due to Rowan #304084
'''

from iptest.assert_util import *
from iptest.misc_util import ip_supported_encodings

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
    
    
    value, length = codecs.escape_decode("ab\a\b\t\n\r\f\vbaab\\a\\b\\t\\n\\r\\f\\vbaab\\\a\\\b\\\t\\\n\\\r\\\f\\\vba")
    if is_cpython:
        AreEqual(value, 'ab\x07\x08\t\n\r\x0c\x0bbaab\x07\x08\t\n\r\x0c\x0bbaab\\\x07\\\x08\\\t\\\r\\\x0c\\\x0bba')
    else:
        #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22743
        AreEqual(value, 'ab\x07\x08\t\n\r\x0c\x0bbaab\x07\x08\t\n\r\x0c\x0bbaab\\\\\\\\\\\\\\\\\\\\\\\\\\\\ba')
    AreEqual(length, 47)
    
    value, length = codecs.escape_decode("\\\a")
    if is_cpython:
        AreEqual(value, '\\\x07')
    else:
        #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22743
        AreEqual(value, '\\\\')
    AreEqual(length, 2)

    if is_cpython:
        AreEqual("abc", codecs.escape_decode("abc", None)[0])
    else:
        #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22742
        AssertError(TypeError, codecs.escape_decode, "abc", None)
    
def test_escape_encode():
    '''
    '''
    #sanity checks
    if is_cpython:
        value, length = codecs.escape_encode("abba")
        AreEqual(value, "abba")
        AreEqual(length, 4)
    
        value, length = codecs.escape_encode("ab\a\b\t\n\r\f\vba")
        AreEqual(value, 'ab\\x07\\x08\\t\\n\\r\\x0c\\x0bba')
        AreEqual(length, 26)
    
        value, length = codecs.escape_encode("\\a")
        AreEqual(value, "\\\\a")
        AreEqual(length, 3)
    
    else:
        #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=4566
        value = codecs.escape_encode("abba")
        AreEqual(value, "abba")
        
        value = codecs.escape_encode("ab\a\b\t\n\r\f\vba")
        #We get this wrong too.
        #AreEqual(value, 'ab\\x07\\x08\\t\\n\\r\\x0c\\x0bba')
        
        value = codecs.escape_encode("\\a")
        AreEqual(value, "\\\\a")

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
    #Sanity
    new_str, size = codecs.charmap_decode("abc")
    AreEqual(new_str, u'abc')
    AreEqual(size, 3)
    
    if is_cpython:
        AreEqual(codecs.charmap_decode(""),
                 (u'', 0))
    else:
        #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22744
        AreEqual(codecs.charmap_decode(""),
                 '')
    
    #Negative
    if is_cpython:
        AssertError(UnicodeDecodeError, codecs.charmap_decode, "a", "strict", {})
    else:
        #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22741
        AssertError(LookupError, codecs.charmap_decode, "a", "strict", {})
    
@skip("silverlight") # no std lib
def test_decode():
    '''
    '''
    #sanity
    new_str = codecs.decode("abc")
    AreEqual(new_str, u'abc')
        
    
@skip("silverlight") # no std lib
def test_encode():
    '''
    '''
    #sanity
    new_str = codecs.encode("abc")
    AreEqual(new_str, 'abc')

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
    
    # so many ways to express latin 1...
    for x in ['iso-8859-1', 'iso8859-1', '8859', 'cp819', 'latin', 'latin1', 'L1']:
        AreEqual('abc'.encode(x), 'abc')
        

@skip('silverlight', "multiple_execute")
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

@skip("multiple_execute")
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
    AreEqual(codecs.unicode_internal_encode(u'abc'), ('a\x00b\x00c\x00', 6))

def test_unicode_internal_decode():
    '''
    '''
    # takes one or two parameters, not zero or three
    AssertError(TypeError, codecs.unicode_internal_decode)
    AssertError(TypeError, codecs.unicode_internal_decode, 'abc', 'def', 'qrt')
    AreEqual(codecs.unicode_internal_decode('ab'), (u'\u6261', 2))

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

@skip('silverlight')
def test_utf_16_le_str_encode():
    for x in ('utf_16_le', 'UTF-16LE', 'utf-16le'):
        AreEqual('abc'.encode(x), 'a\x00b\x00c\x00')

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
    #function takes one parameter, not 0
    if is_cli:
        #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=4563
        #AssertError(NotImplementedError, codecs.charbuffer_encode, "abc")
        AssertError(NotImplementedError, codecs.charbuffer_encode)

def test_charmap_encode():
    #Sanity
    if is_cpython:
        #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22738
        AreEqual(codecs.charmap_encode("abc"), 
                 ('abc', 3))
        AreEqual(codecs.charmap_encode("abc", "strict"), 
                 ('abc', 3))
    
    if is_cpython:
        AreEqual(codecs.charmap_encode("", "strict", {}),
                 ('', 0))
    else:
        #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22740
        AreEqual(codecs.charmap_encode("", "strict", {}),
                 '')
                 
    #Sanity Negative
    if is_cpython:
        AssertError(UnicodeEncodeError, codecs.charmap_encode, "abc", "strict", {})
    else:
        #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22741
        AssertError(LookupError, codecs.charmap_encode, "abc", "strict", {})


def test_mbcs_decode():
    '''
    '''
    #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=4563
    if is_cli:
        #AssertError(NotImplementedError, codecs.mbcs_decode, "abc")
        AssertError(NotImplementedError, codecs.mbcs_decode)

def test_mbcs_encode():
    '''
    '''
    #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=4563
    if is_cli:
        #AssertError(NotImplementedError, codecs.mbcs_encode, "abc")
        AssertError(NotImplementedError, codecs.mbcs_encode)

def test_readbuffer_encode():
    '''
    '''
    #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=4563
    if is_cli:
        #AssertError(NotImplementedError, codecs.readbuffer_encode, "abc")
        AssertError(NotImplementedError, codecs.readbuffer_encode)

def test_unicode_escape_decode():
    '''
    '''
    #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=4563
    if is_cli:
        #AssertError(NotImplementedError, codecs.unicode_escape_decode, "abc")
        AssertError(NotImplementedError, codecs.unicode_escape_decode)

def test_unicode_escape_encode():
    '''
    '''
    #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=4563
    if is_cli:
        #AssertError(NotImplementedError, codecs.unicode_escape_encode, "abc")
        AssertError(NotImplementedError, codecs.unicode_escape_encode)

def test_utf_16_encode():
    #Sanity
    AreEqual(codecs.utf_16_encode("abc"), ('\xff\xfea\x00b\x00c\x00', 3))


def test_misc_encodings():
    if not is_silverlight:
        # codec not available on silverlight
        AreEqual('abc'.encode('utf-16'), '\xff\xfea\x00b\x00c\x00')
        AreEqual('abc'.encode('utf-16-be'), '\x00a\x00b\x00c')
    for unicode_escape in ['unicode-escape', 'unicode escape']:
        AreEqual('abc'.encode('unicode-escape'), 'abc')
        AreEqual('abc\u1234'.encode('unicode-escape'), 'abc\\\\u1234')

@skip("silverlight")
def test_file_encodings():
    '''
    Once this gets fixed, we should use *.py files in the correct encoding instead
    of dynamically generating ASCII files.  Also, need variations on the encoding
    names.
    '''
    
    sys.path.append(nt.getcwd() + "\\tmp_encodings")
    try:
        nt.mkdir(nt.getcwd() + "\\tmp_encodings")
    except:
        pass
    
    try:
        #positive cases
        for coding in ip_supported_encodings:
            temp_mod_name = "test_encoding_" + coding.replace("-", "_").replace(" ", "_")
            f = open(nt.getcwd() + "\\tmp_encodings\\" + temp_mod_name + ".py",
                    "w")
            f.write("# coding: %s" % (coding))
            f.close()
            __import__(temp_mod_name)
            nt.remove(nt.getcwd() + "\\tmp_encodings\\" + temp_mod_name + ".py")
            
    finally:
        #cleanup
        sys.path.remove(nt.getcwd() + "\\tmp_encodings")

@skip("silverlight")
def test_cp11334():
    
    #--Test that not using "# coding ..." results in a warning
    t_in, t_out, t_err = nt.popen3(sys.executable + " " + nt.getcwd() + r"\encoded_files\cp11334_warn.py")
    t_err_lines = t_err.readlines()
    t_out_lines = t_out.readlines()
    t_err.close()
    t_out.close()
    t_in.close()
    
    AreEqual(len(t_out_lines), 0)
    Assert(t_err_lines[0].startswith("  File"))
    Assert(t_err_lines[1].startswith("SyntaxError: Non-ASCII character '\\xb5' in file"))
    
    #--Test that using "# coding ..." is OK
    t_in, t_out, t_err = nt.popen3(sys.executable + " " + nt.getcwd() + r"\encoded_files\cp11334_ok.py")
    t_err_lines = t_err.readlines()
    t_out_lines = t_out.readlines()
    t_err.close()
    t_out.close()
    t_in.close()
    
    AreEqual(len(t_err_lines), 0)
    if not is_cli:
        AreEqual(t_out_lines[0], "\xb5ble\n")
    else:
        print "CodePlex 11334"
        AreEqual(t_out_lines[0], "\xe6ble\n")
    AreEqual(len(t_out_lines), 1)


@skip("silverlight", "multiple_execute")
def test_file_encodings_negative():
    '''
    TODO:
    - we should use *.py files in the correct encoding instead
    of dynamically generating ASCII files
    - need variations on the encoding names
    '''
    import sys
    import nt
    sys.path.append(nt.getcwd() + "\\tmp_encodings")
    try:
        nt.mkdir(nt.getcwd() + "\\tmp_encodings")
    except:
        pass
             
    try:
        #negative case
        f = open(nt.getcwd() + "\\tmp_encodings\\" + "bad_encoding.py", "w")
        f.write("# coding: bad")
        f.close()
        AssertError(SyntaxError, __import__, "bad_encoding")
    finally:
        #cleanup
        sys.path.remove(nt.getcwd() + "\\tmp_encodings")

@disabled
def test_cp1214():
    """
    TODO: extend this a great deal
    """
    AreEqual('7FF80000000000007FF0000000000000'.decode('hex'),
             '\x7f\xf8\x00\x00\x00\x00\x00\x00\x7f\xf0\x00\x00\x00\x00\x00\x00')


def test_codecs_lookup():
    l = []
    def my_func(encoding, cache = l):
        l.append(encoding)
    
    codecs.register(my_func)
    allchars = ''.join([chr(i) for i in xrange(1, 256)])
    try:
        codecs.lookup(allchars)
        AssertUnreachable()
    except LookupError:
        pass
        
    lowerchars = allchars.lower().replace(' ', '-')
    for i in xrange(1, 255):
        if l[0][i] != lowerchars[i]:
            Assert(False, 'bad chars at index %d: %r %r' % (i, l[0][i], lowerchars[i]))
            
    AssertError(TypeError, codecs.lookup, '\0')
    AssertError(TypeError, codecs.lookup, 'abc\0')
    AreEqual(len(l), 1)


def test_lookup_encodings():
    try:
        AreEqual('07FF'.decode('hex')  , '\x07\xff')
    except LookupError:
        # if we don't have encodings then this will fail so
        # make sure we're failing because we don't have encodings
        AssertError(ImportError, __import__, 'encodings')
        
run_test(__name__)
