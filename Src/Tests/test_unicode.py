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

from lib.assert_util import *

def test_raw_unicode_escape():
    s = unicode('\u0663\u0661\u0664 ','raw-unicode-escape')
    AreEqual(len(s), 4)
    AreEqual(int(s), 314)

def test_raw_unicode_escape_noescape_lowchars():
    for i in range(0x100):
        AreEqual(unichr(i).encode('raw-unicode-escape'), chr(i))

    AreEqual(unichr(0x100).encode('raw-unicode-escape'), r'\u0100')

def test_raw_unicode_escape_dashes():
    """Make sure that either dashes or underscores work in raw encoding name"""
    ok = True
    try:
        unicode('hey', 'raw_unicode-escape')
    except LookupError:
        ok = False

    Assert(ok, "dashes and underscores should be interchangable")

@skip("silverlight")
def test_unicode_error():
    
    from _codecs import register_error
    def handler(ex): 
        AreEqual(ex.object, u'\uac00')
        return (u"", ex.end)
    register_error("test_unicode_error", handler)
    
    supported_modes = [ 'cp1252','ascii', 'utf-8', 'utf-16', 'latin-1', 'iso-8859-1', 'utf-16-le', 'utf-16-be', 'unicode-escape', 'raw-unicode-escape']
                        
    for mode in supported_modes:  unichr(0xac00).encode(mode, "test_unicode_error")




run_test(__name__)
