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

from lib.assert_util import *
skiptest("silverlight")
from lib.process_util import *

quote = '\"'

def msbuild(projectFile, configuration):
    AreEqual(file_exists(projectFile), True)
    return run_tool("msbuild.exe", str(projectFile) + ' /t:Rebuild /p:Platform=\"Any CPU\" /p:Configuration=' + quote + configuration + quote)


def test_silverlightDebugBuild():
    result = msbuild(testpath.public_testdir + "\\..\\..\\..\\Rowan.sln", "Silverlight Debug")
    AreEqual(result, 0)
    
def test_silverlightReleaseBuild():
    result = msbuild(testpath.public_testdir + "\\..\\..\\..\\Rowan.sln", "Silverlight Release")
    AreEqual(result, 0)

run_test(__name__)
