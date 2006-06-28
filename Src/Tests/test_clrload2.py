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

import sys
from lib.assert_util import *

"""Test cases for CLR types that don't involve actually loading CLR into the module
using the CLR types"""

if is_cli:
    sys.path.append(testpath.test_inputs_dir)
    import UseCLI

    UseCLI.Form().Controls.Add(UseCLI.Control())

