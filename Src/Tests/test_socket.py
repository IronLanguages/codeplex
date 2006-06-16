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

#
# test socket
#

from lib.assert_util import *
import sys
import socket
import clr

def test_HandleToSocket():
    s = socket.socket()

    system_socket = socket.socket.HandleToSocket(s.fileno())
    AreEqual(s.fileno(), system_socket.Handle.ToInt64())

    s.close()

run_test(__name__)
