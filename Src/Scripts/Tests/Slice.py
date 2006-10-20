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

x="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
Assert( x[10] == 'k')
Assert( x[20] == 'u')
Assert( x[30] == 'E')
Assert( x[-10] == 'Q')
Assert(x[-3] == 'X')
Assert(x[14:20] == 'opqrst')
Assert(x[20:14] == '')
Assert(x[-30:-5] == 'wxyzABCDEFGHIJKLMNOPQRSTU')
Assert(x[-5:-30] == '')
Assert(x[3:40:2] == 'dfhjlnprtvxzBDFHJLN')
Assert(x[40:3:2] == '')
Assert(x[3:40:-2] == '')
Assert(x[40:3:-2] == 'OMKIGECAywusqomkige')
Assert(x[-40:-4:-2] == '')
Assert(x[-4:-40:-2] == 'WUSQOMKIGECAywusqo')
Assert(x[-40:-4:2] == 'moqsuwyACEGIKMOQSU')
Assert(x[-4:-40:2] == '')
Assert(x[-40:-5:-2] == '')
Assert(x[-5:-40:-2] == 'VTRPNLJHFDBzxvtrpn')
Assert(x[-40:-5:2] == 'moqsuwyACEGIKMOQSU')
Assert(x[-5:-40:2] == '')
Assert(x[-40:-6:-2] == '')
Assert(x[-6:-40:-2] == 'USQOMKIGECAywusqo')
Assert(x[-40:-6:2] == 'moqsuwyACEGIKMOQS')
Assert(x[-6:-40:2] == '')
Assert(x[-49:-5:-3] == '')
Assert(x[-5:-49:-3] == 'VSPMJGDAxurolif')
Assert(x[-49:-5:3] == 'dgjmpsvyBEHKNQT')
Assert(x[-5:-49:3] == '')
Assert(x[-50:-5:-3] == '')
Assert(x[-5:-50:-3] == 'VSPMJGDAxurolif')
Assert(x[-50:-5:3] == 'cfiloruxADGJMPS')
Assert(x[-5:-50:3] == '')
Assert(x[-51:-5:-3] == '')
Assert(x[-5:-51:-3] == 'VSPMJGDAxurolifc')
Assert(x[-51:-5:3] == 'behknqtwzCFILORU')
Assert(x[-5:-51:3] == '')

Assert((1, 2, 3, 4, 5)[1:-1][::-1] == (4, 3, 2))
Assert([1, 2, 3, 4, 5][1:-1][::-1] == [4, 3, 2])
Assert((9, 7, 5, 3) == (1, 2, 3, 4, 5, 6, 7, 8, 9, 0)[1:-1][::-2])
Assert([9, 7, 5, 3] == [1, 2, 3, 4, 5, 6, 7, 8, 9, 0][1:-1][::-2])
Assert((2, 4, 6, 8) == (1, 2, 3, 4, 5, 6, 7, 8, 9, 0)[1:-1][::2])
Assert([2, 4, 6, 8] == [1, 2, 3, 4, 5, 6, 7, 8, 9, 0][1:-1][::2])
Assert((2, 5, 8) == (1, 2, 3, 4, 5, 6, 7, 8, 9, 0)[1:-1][::3])
Assert([2, 5, 8] == [1, 2, 3, 4, 5, 6, 7, 8, 9, 0][1:-1][::3])

l = list(x)
l[2:50] = "10"
Assert(l == list("ab10YZ"))
l = list(x)
l[2:50:2] = "~!@#$%^&*()-=_+[]{}|;:/?"
Assert(l == list("ab~d!f@h#j$l%n^p&r*t(v)x-z=B_D+F[H]J{L}N|P;R:T/V?XYZ"))

# check for good behaviour of slices on lists in general
l=list(x)
Assert( l[10] == 'k')
Assert( l[20] == 'u')
Assert( l[30] == 'E')
Assert( l[-10] == 'Q')
Assert(l[-3] == 'X')
Assert(l[14:20] == list('opqrst'))
Assert(l[20:14] == [])
Assert(l[-51:-5:-3] == [])
Assert(l[-5:-51:3] == [])

# more coverage for slice
arr = range(10)
# BUG 614
AreEqual(slice(3), slice(None, 3, None))
# /BUG
AreEqual(arr[slice(None,None,2)], [0,2,4,6,8])
AreEqual(str(slice(1,2,3)), 'slice(1, 2, 3)')

# negative 
l = range(10)
def f1(): l[::3] = [1]
def f2(): l[::3] = range(5)
def f3(): l[::3] = (1,)
def f4(): l[::3] = (1, 2, 3, 4, 5, 6)
for f in (f1, f2, f3, f4):
    AssertError(ValueError, f)

# coverage
AreEqual(slice(3) == slice(0, 3, 1), False)
AreEqual(slice(3) == slice(None, 3, None), True)
AreEqual(slice(3) == 3, False)
