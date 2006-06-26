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

## BE PLATFORM NETURAL

import nt
import sys
from assert_util import testpath, is_cli

one_arg_params = ("-X:Optimize", "-W", "-c", "-X:MaxRecursion", "-X:AssembliesDir")

def launch(executable, *params):
    if is_cli:
        return nt.spawnl(0, executable, *params)
    else:
        l = [ executable ]
        for param in params: l.append(param)
        return nt.spawnv(0, executable, l)

def launch_ironpython(pyfile, *args):
    t = (pyfile, )
    for arg in args: t += (arg, )
    return launch(testpath.ipython_executable, *t)

def launch_cpython(pyfile, *args):
    t = (pyfile, )
    for arg in args: t += (arg, )
    return launch(testpath.cpython_executable, *t)

def launch_ironpython_with_extensions(pyfile, extensions, args):
    t = tuple(extensions)
    t += (pyfile, )
    for arg in args: t += (arg, )
    return launch(testpath.ipython_executable, *t)

def _get_ip_testmode():
    import System
    lastConsumesNext = False
    switches = []
    for param in System.Environment.GetCommandLineArgs():
        if param.startswith('-T:') or param.startswith('-O:'): 
            continue
        if param.startswith("-"):
            switches.append(param)
            if param in one_arg_params:
                lastConsumesNext = True
        else:
            if lastConsumesNext:
                 switches.append(param)   
            lastConsumesNext = False
    return switches

def launch_ironpython_changing_extensions(test, add=[], remove=[]):
    final = _get_ip_testmode()
    for param in add:
        if param not in final: final.append(param)
        
    for param in remove:
        if param in final:
            pos = final.index(param)
            if pos != -1:
                if param in one_arg_params:
                    del final[pos:pos+2]
                else :
                    del final[pos]
        
    params = tuple(final)
    params += (test,)
    
    return nt.spawnl(0, sys.executable, *params)
