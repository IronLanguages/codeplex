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

from Util.Debug import *

Assert(min.__doc__ <> None)

if is_cli: 
    import System

    Assert(System.Collections.ArrayList.__new__.__doc__[:7] == "__new__")
    Assert(System.Collections.ArrayList.Repeat.__doc__.startswith("static "))

    # static (bool, float) TryParse(str s)
    Assert(System.Double.TryParse.__doc__.index('(bool, float)') > 0)
    Assert(System.Double.TryParse.__doc__.index('(str s)') > 0)
