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


""" This provides a more convenient harness for running this
    benchmark and collecting separate timings for each component.
"""

import sys

def test_main(type="short"):
    import pystone
    loops = { "full": 50000, "short" : 50000, "medium" : 250000, "long" : 1000000 }[type]
    pystone.main(loops)

if __name__=="__main__":
    kind = "short"
    if len(sys.argv) > 1: kind = sys.argv[1]
    test_main(kind)
