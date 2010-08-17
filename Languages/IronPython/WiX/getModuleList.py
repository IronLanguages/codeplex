#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

# Copies working standard library modules to the provided output directory
# Usage: ipy getModuleList.py <output directory>

#--Imports---------------------------------------------------------------------
import sys, nt, os
import clr
import shutil
clr.AddReference("System.Xml")

from System.Xml import XmlDocument, XmlNamespaceManager
from System import Guid
from System.IO import File, Path, Directory, FileInfo, FileAttributes

#List of predetermined directories and files which should not be included in
#the MSI
excludedDirectories = []
excludedFiles       = []


#Automatically determine what's currently not working under IronPython
sys.path.append(nt.environ["DLR_ROOT"] + r"\Languages\IronPython\Tests\Tools")
base_dir = nt._getfullpathname(nt.environ["DLR_ROOT"] + r"\External.LCA_RESTRICTED\Languages\CPython\27")

import stdmodules
BROKEN_LIST = stdmodules.main(base_dir)

if len(BROKEN_LIST)<10:
    #If there are less than ten modules/directories listed in BROKEN_LIST
    #chances are good stdmodules is broken!
    exc_msg = "It's highly unlikely that only %d CPy standard modules are broken under IP!" % len(BROKEN_LIST)
    print exc_msg
    raise Exception(exc_msg)

#Specify Packages and Modules that should not be included here.
excludedDirectories += [
                        "/Lib/test",
                        ]
excludedDirectories += [x for x in BROKEN_LIST if not x.endswith(".py")]

excludedFiles += [                  
                  #*.py modules IronPython has implemented in *.cs
                  "/Lib/copy_reg.py",
                  "/Lib/socket.py",
                  "/Lib/re.py",
                ]
excludedFiles += [x for x in BROKEN_LIST if x.endswith(".py")]

excludedDirectories = [os.path.join(base_dir, x[1:]).lower().replace('/', '\\') + '\\' for x in excludedDirectories]
excludedFiles = [os.path.join(base_dir, x[1:]).lower().replace('/', '\\') for x in excludedFiles]


for dirpath, dirnames, filenames in os.walk(base_dir):
    for filename in filenames:
        filename = os.path.join(dirpath, filename)
        for excluded in excludedFiles:
            if filename.lower() == excluded:
                break
        else:
            for excluded in excludedDirectories:
                if filename.lower().startswith(excluded):
                    break
            else:
                sub_name = filename[len(base_dir) + 1:]
                if sub_name.endswith('.exe'):    
                    continue
                dest = os.path.join(sys.argv[1], sub_name)
                if not Directory.Exists(Path.GetDirectoryName(dest)):
                    Directory.CreateDirectory(Path.GetDirectoryName(dest))
                if (FileInfo(filename).Attributes & FileAttributes.ReadOnly):
                    File.Copy(filename, dest, True)
