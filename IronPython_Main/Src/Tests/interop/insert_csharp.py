#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################
import nt, sys
import System

pattern = sys.argv[1]

for x in System.IO.Directory.GetFiles(".", pattern):
    f = file(x)
    lines = f.readlines()
    f.close()
    
    nl = []
    for l in lines:
        insert_file = ""
        if "clr.AddReference" in l:
            left = l.index('(')
            right = l.index(')')
            insert_file = nt.environ["MERLIN_ROOT"] + "\\Test\\ClrAssembly\\" + l[left+2:right-1] + ".cs"
            
        nl.append(l.rstrip()) # no matter what, print the current line
        
        if insert_file:
            nl.append("")
            f = file(insert_file)
            for l2 in f.readlines():
                if l2.strip():
                    nl.append("# " + l2.rstrip())
            f.close()
            nl.append("")
    
    f = file(x, "w")
    for l in nl:
        f.write(l + "\n")
    f.close()