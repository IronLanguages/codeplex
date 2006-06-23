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
# test assert
#

from lib.assert_util import *

def test_positive():
    ok = True
    try:
        assert True, 'this should always pass'
    except AssertionError:
        ok = False
    Assert(ok)
    
def test_negative():
    ok = False
    try:
        assert False, 'this should never pass'
    except AssertionError:
        ok = True
    Assert(ok)
    
def test_doesnt_fail_on_curly():
    """Ensures that asserting a string with a curly brace doesn't choke up the
    string formatter."""

    ok = False
    try:
        assert False, '}'
    except AssertionError:
        ok = True
    Assert(ok)
    
run_test(__name__)
