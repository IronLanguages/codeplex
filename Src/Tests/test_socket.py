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
import _socket
import clr

def is_open(sock):
    try:
        dummy = sock.gettimeout() # this will raise if s is closed
    except _socket.error:
        return False
    return True

def is_closed(sock):
    return not is_open(sock)
    
def test_socket_refcount():
    s = _socket.socket()
    AreEqual(s.RefCount, 0)
    Assert(is_open(s), "refcounted socket closed prematurely")
    
    s.AcquireRef()
    AreEqual(s.RefCount, 1)
    Assert(is_open(s), "refcounted socket closed prematurely")

    s.AcquireRef()
    AreEqual(s.RefCount, 2)
    Assert(is_open(s), "refcounted socket closed prematurely")

    s.ReleaseRef()
    AreEqual(s.RefCount, 1)
    Assert(is_open(s), "refcounted socket closed prematurely")

    s.ReleaseRef()
    AreEqual(s.RefCount, 0)
    Assert(is_closed(s), "socket not closed when refcount reached zero")

    ok = False
    try:
        s.ReleaseRef()
    except RuntimeError:
        ok = True
    Assert(ok, "no error raised when refcount decremented past zero")

def test_socketobject_refcounted_descriptor():
    sys.path.append(testpath.lib_testdir)
    import socket

    s1 = socket._socketobject()

    internal_sock = s1._sock

    AreEqual(internal_sock.RefCount, 1)
    Assert(is_open(internal_sock))

    s2 = s1.dup()
    AreEqual(s2._sock, internal_sock)
    AreEqual(internal_sock.RefCount, 2)
    Assert(is_open(internal_sock))

    s1.close()
    AreEqual(internal_sock.RefCount, 1)
    Assert(is_open(internal_sock))

    s2.close()
    AreEqual(internal_sock.RefCount, 0)
    Assert(is_closed(s2._sock))

def test_HandleToSocket():
    s = _socket.socket()

    system_socket = _socket.socket.HandleToSocket(s.fileno())
    AreEqual(s.fileno(), system_socket.Handle.ToInt64())

    s.close()

run_test(__name__)
