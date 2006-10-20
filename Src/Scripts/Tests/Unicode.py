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
import sys


setenc = sys.setdefaultencoding

#verify we start w/ ASCII

f = file('testfile.tmp', 'w')
f.write(u'\u6211')
f.close()

f = file('testfile.tmp', 'r')
txt = f.read()
f.close()
Assert(txt != u'\u6211')


#and verify UTF8 round trips correctly

setenc('utf8')

f = file('testfile.tmp', 'w')
f.write(u'\u6211')
f.close()

f = file('testfile.tmp', 'r')
txt = f.read()
f.close()
AreEqual(txt, u'\u6211')


