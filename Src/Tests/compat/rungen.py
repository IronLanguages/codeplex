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
import runsbs
   
if __name__ == "__main__":
    args = sys.argv
    
    if len(args) == 1: 
        ret = runsbs.run_gen()
    else :
        ret = runsbs.run_gen([x for x in args[1:]])
        
    sys.exit(ret)
