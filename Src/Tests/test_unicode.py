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

from lib.assert_util import *

def test_raw_unicode_escape():
    # verify raw-unicode-escape works properly
    s = unicode('\u0663\u0661\u0664 ','raw-unicode-escape')
    AreEqual(len(s), 4)
    AreEqual(int(s), 314)

def test_raw_unicode_escape_dashes():
    """Make sure that either dashes or underscores work in raw encoding name"""
    ok = True
    try:
        unicode('hey', 'raw_unicode-escape')
    except LookupError:
        ok = False

    Assert(ok, "dashes and underscores should be interchangable")

run_test(__name__)
