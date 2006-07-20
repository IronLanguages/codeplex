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

##
## Test the binascii module
##

from lib.assert_util import *

import binascii

# verify extra characters are ignored, and that we require padding.
def test_negative():
    # the native implementation throws a binascii.Error---we throw a TypeError
    if is_cli:
        for x in ('A', 'AB', '%%%A', 'A%%%', '%A%%', '%AA%' ):
            AssertError(TypeError, binascii.a2b_base64, x)     # Type Error , incorrect padding

def test_positive():
    AreEqual(binascii.a2b_base64(''), '')
    AreEqual(binascii.a2b_base64('AAA='), '\x00\x00')
    AreEqual(binascii.a2b_base64('%%^^&&A%%&&**A**#%&A='), '\x00\x00')

def test_zeros():
    """verify zeros don't show up as being only a single character"""
    AreEqual(binascii.b2a_hex('\x00\x00\x10\x00'), '00001000')


run_test(__name__)