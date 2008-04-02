#####################################################################################
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public
# License. A  copy of the license can be found in the License.html file at the
# root of this distribution. If  you cannot locate the  Microsoft Public
# License, please send an email to  dlr@microsoft.com. By using this source
# code in any fashion, you are agreeing to be bound by the terms of the 
# Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#####################################################################################


""" This provides a more convenient harness for running this
    benchmark and collecting separate timings for each component.
"""

import sys, nt
sys.path.append(nt.environ['merlin_root'] + "\\..\\External\\Languages\\IronPython11\\24\\Lib\\Test")


def test_main(type="short"):
    import pystone
    loops = { "full": 50000, "short" : 50000, "medium" : 250000, "long" : 1000000 }[type]
    pystone.main(loops)

if __name__=="__main__":
    kind = "short"
    if len(sys.argv) > 1: kind = sys.argv[1]
    test_main(kind)
