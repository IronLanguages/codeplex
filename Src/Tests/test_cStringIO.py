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

from lib.assert_util import *
import cStringIO

text = "Line 1\nLine 2\nLine 3\nLine 4\nLine 5"

# close
def test_close(i):
    AreEqual(i.closed, False)
    i.close()
    AreEqual(i.closed, True)
    i.close()
    AreEqual(i.closed, True)
    i.close()
    AreEqual(i.closed, True)

# read
def test_read(i):
    AreEqual(i.read(), text)
    AreEqual(i.read(), "")
    AreEqual(i.read(), "")
    i.close()
    i.close()
    AssertError(ValueError, i.read)

# readline
def test_readline(i):
    AreEqual(i.readline(), "Line 1\n")
    AreEqual(i.readline(), "Line 2\n")
    AreEqual(i.readline(), "Line 3\n")
    AreEqual(i.readline(), "Line 4\n")
    AreEqual(i.readline(), "Line 5")
    AreEqual(i.readline(), "")
    i.close()
    AssertError(ValueError, i.readline)

# readlines
def test_readlines(i):
    AreEqual(i.readlines(), ["Line 1\n", "Line 2\n", "Line 3\n", "Line 4\n", "Line 5"])
    AreEqual(i.readlines(), [])
    i.close()
    AssertError(ValueError, i.readlines)

# getvalue
def test_getvalue(i):
    AreEqual(i.getvalue(), text)
    AreEqual(i.read(6), "Line 1")
    AreEqual(i.getvalue(True), "Line 1")
    AreEqual(i.getvalue(), text)
    i.close()
    AssertError(ValueError, i.getvalue)

# isatty
def test_isatty(i):
    AreEqual(i.isatty(), 0)
    i.close()
    AreEqual(i.isatty(), 0)

# __iter__, next
def test_next(i):
    AreEqual(i.__iter__(), i)
    AreEqual(i.next(), "Line 1\n")
    AreEqual(i.next(), "Line 2\n")
    AreEqual([l for l in i], ["Line 3\n", "Line 4\n", "Line 5"])
    i.close()
    AssertError(ValueError, i.readlines)

# read, readline, reset
def test_reset(i):
    AreEqual(i.read(4), "Line")
    AreEqual(i.readline(), " 1\n")
    i.reset()
    AreEqual(i.read(4), "Line")
    AreEqual(i.readline(), " 1\n")
    i.close()
    AssertError(ValueError, i.read, 5)
    AssertError(ValueError, i.readline)

# seek, tell, read
def test_seek_tell(i):
    AreEqual(i.read(4), "Line")
    AreEqual(i.tell(), 4)
    i.seek(10)
    AreEqual(i.tell(), 10)
    AreEqual(i.read(3), "e 2")
    i.seek(15, 0)
    AreEqual(i.tell(), 15)
    AreEqual(i.read(5), "ine 3")
    i.seek(3, 1)
    AreEqual(i.read(4), "ne 4")
    i.seek(-5, 2)
    AreEqual(i.tell(), len(text) - 5)
    AreEqual(i.read(), "ine 5")
    i.seek(1000)
    AreEqual(i.tell(), 1000)
    AreEqual(i.read(), "")
    i.seek(2000, 0)
    AreEqual(i.tell(), 2000)
    AreEqual(i.read(), "")
    i.seek(400, 1)
    AreEqual(i.tell(), 2400)
    AreEqual(i.read(), "")
    i.seek(100, 2)
    AreEqual(i.tell(), len(text) + 100)
    AreEqual(i.read(), "")
    i.close()
    AssertError(ValueError, i.tell)
    AssertError(ValueError, i.seek, 0)
    AssertError(ValueError, i.seek, 0, 2)

# truncate
def test_truncate(i):
    AreEqual(i.read(6), "Line 1")
    i.truncate(20)
    AreEqual(i.tell(), 20)
    AreEqual(i.getvalue(), "Line 1\nLine 2\nLine 3")
    i.reset()
    AreEqual(i.tell(), 0)
    AreEqual(i.read(6), "Line 1")
    i.truncate()
    AreEqual(i.getvalue(), "Line 1")
    i.close()
    AssertError(ValueError, i.truncate)
    AssertError(ValueError, i.truncate, 10)

def test_write(o):
    AreEqual(o.getvalue(), text)
    o.write("Data 1")
    AreEqual(o.read(7), "\nLine 2")
    AreEqual(o.getvalue(), "Data 1\nLine 2\nLine 3\nLine 4\nLine 5")
    o.close()
    AssertError(ValueError, o.write, "Hello")

def test_writelines(o):
    AreEqual(o.getvalue(), text)
    o.writelines(["Data 1", "Data 2"])
    AreEqual(o.read(8), "2\nLine 3")
    AreEqual(o.getvalue(), "Data 1Data 22\nLine 3\nLine 4\nLine 5")
    o.close()
    AssertError(ValueError, o.writelines, "Hello")

def test_softspace(o):
    o.write("Hello")
    o.write("Hi")
    o.softspace = 1
    AreEqual(o.softspace, 1)
    AreEqual(o.getvalue(), "HelloHiLine 2\nLine 3\nLine 4\nLine 5")

def init_StringI():
    return cStringIO.StringIO(text)

def init_StringO():
    o = cStringIO.StringIO()
    o.write(text)
    o.reset()
    return o

def test_i(init):
    for t in [
        test_close,
        test_read,
        test_readline,
        test_readlines,
        test_getvalue,
        test_next,
        test_reset,
        test_seek_tell,
        test_truncate
        ]:
        i = init()
        t(i)

def test_o(init):
    for t in [ test_write, test_writelines, test_softspace ]:
        i = init()
        t(i)

test_i(init_StringI)
test_i(init_StringO)
test_o(init_StringO)
