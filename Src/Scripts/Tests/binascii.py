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



import binascii

# verify extra characters are ignored, and that we require padding.

AssertError(TypeError, binascii.a2b_base64, 'AB')     # Type Error , incorrect padding
AssertError(TypeError, binascii.a2b_base64, 'A')     # Type Error , incorrect padding
AssertError(TypeError, binascii.a2b_base64, '%%%A')     # Type Error , incorrect padding
AssertError(TypeError, binascii.a2b_base64, 'A%%%')     # Type Error , incorrect padding
AssertError(TypeError, binascii.a2b_base64, '%A%%')     # Type Error , incorrect padding

AssertError(TypeError, binascii.a2b_base64, '%AA%')     # Type Error , incorrect padding
AssertError(TypeError, binascii.a2b_base64, 'AAA=')     # Type Error , incorrect padding


AreEqual(binascii.a2b_base64(''), '')

AreEqual(binascii.a2b_base64('AAA='), '\x00\x00')

AreEqual(binascii.a2b_base64('%%^^&&A%%&&**A**#%&A='), '\x00\x00')



