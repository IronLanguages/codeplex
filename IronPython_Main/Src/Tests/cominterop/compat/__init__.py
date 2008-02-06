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

import sys
import nt

if sys.platform=="win32":

    #Make sure we'll have access to pywin32
    if sys.prefix + "\\Lib" not in sys.path:
        sys.path.append(sys.prefix + "\\Lib")
    
    #Make sure we'll have access to cominterop_util
    if "." not in sys.path: sys.path.append(".")
    
#Next make sure pywintypes25.dll is in %Path%
cpy_location = nt.environ["SystemDrive"] + "\\Python" + sys.winver.replace(".", "")
if cpy_location not in nt.environ["Path"]:
    nt.putenv("Path", nt.environ["Path"] + ";" + cpy_location)

if sys.platform=="win32":
    #At this point it should be possible to install the pywin32com server
    from hw import install_pywin32_server
    install_pywin32_server()


#--Run tests-------------------------------------------------------------------
from exceptions import SystemExit

for test_module in ["hw_client"]:
    print "--------------------------------------------------------------------"
    print "Importing", test_module, "..."
    try:
        __import__(test_module)
    except SystemExit, e:
        if e.code!=0: 
            raise Exception("Importing '%s' caused an unexpected exit code: %s" % (test_module, str(e.code)))
    print ""

