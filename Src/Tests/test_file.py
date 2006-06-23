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
import sys

# This module tests operations on the builtin file object. It is not yet complete, the tests cover read(),
# read(size), readline() and write() for binary, text and universal newline modes.
def test_sanity():
    for i in range(5):
	    ### general file robustness tests
	    f = file("onlyread.tmp", "w")
	    f.write("will only be read")
	    f.flush()
	    f.close()
	    sin = file("onlyread.tmp", "r")
	    sout = file("onlywrite.tmp", "w")

	    # writer is null for sin
	    AssertError(IOError, sin.write, "abc")
	    AssertError(IOError, sin.writelines, ["abc","def"])

	    # reader is null for sout
	    AssertError(IOError, sout.read)
	    AssertError(IOError, sout.read, 10)
	    AssertError(IOError, sout.readline)
	    AssertError(IOError, sout.readline, 10)
	    AssertError(IOError, sout.readlines)
	    AssertError(IOError, sout.readlines, 10)
    	
	    sin.close()
	    sout.close()

	    # now close a file and try to perform other I/O operations on it...
	    # should throw ValueError according to docs
	    f = file("onlywrite.tmp", "w")
	    f.close()
	    f.close()
	    AssertError(ValueError, f.__iter__)
	    AssertError(ValueError, f.flush)
	    AssertError(ValueError, f.fileno)
	    AssertError(ValueError, f.next)
	    AssertError(ValueError, f.read)
	    AssertError(ValueError, f.read, 10)
	    AssertError(ValueError, f.readline)
	    AssertError(ValueError, f.readline, 10)
	    AssertError(ValueError, f.readlines)
	    AssertError(ValueError, f.readlines, 10)
	    AssertError(ValueError, f.seek, 10)
	    AssertError(ValueError, f.seek, 10, 10)
	    AssertError(ValueError, f.write, "abc")
	    AssertError(ValueError, f.writelines, ["abc","def"])

	###

# The name of a temporary test data file that will be used for the following
# file tests.
temp_file = path_combine(testpath.temporary_dir, "temp.dat")

# Test binary reading and writing fidelity using a round trip method. First
# construct some pseudo random binary data in a string (making it long enough
# that it's likely we'd show up any problems with the data being passed through
# a character encoding/decoding scheme). Then write this data to disk (in binary
# mode), read it back again (in binary) and check that no changes have occured.

# Construct the binary data. We want the test to be repeatable so seed the
# random number generator with a fixed value. Use a simple linear congruential
# method to generate the random byte values.

rng_seed = 0

def test_read_write_fidelity():
    def randbyte():
        global rng_seed
        rng_seed = (1664525 * rng_seed) + 1013904223
        return (rng_seed >> 8) & 0xff

    data = ""
    for i in range(10 * 1024):
        data += chr(randbyte())

    # Keep a copy of the data safe.
    orig_data = data;

    # Write the data to disk in binary mode.
    f = file(temp_file, "wb")
    f.write(data)
    f.close()

    # And read it back in again.
    f = file(temp_file, "rb")
    data = f.read()
    f.close()

    # Check nothing changed.
    Assert(data == orig_data)

# Helper used to format newline characters into a visible format.
def format_newlines(string):
    out = ""
    for char in string:
        if char == '\r':
            out += "\\r"
        elif char == '\n':
            out += "\\n"
        else:
            out += char
    return out

# The set of read modes we wish to test. Each tuple consists of a human readable
# name for the mode followed by the corresponding mode string that will be
# passed to the file constructor.
read_modes = (("binary", "rb"), ("text", "r"), ("universal", "rU"))

# Same deal as above but for write modes. Note that writing doesn't support a
# universal newline mode.
write_modes = (("binary", "wb"), ("text", "w"))

# The following is the setup for a set of pattern mode tests that will check
# some tricky edge cases for newline translation for both reading and writing.
# The entry point is the test_patterns() function.
def test_newlines():

    # Read mode test cases. Each tuple has three values; the raw on-disk value we
    # start with (which also doubles as the value we should get back when we read in
    # binary mode) then the value we expect to get when reading in text mode and
    # finally the value we expect to get in universal newline mode.
    read_patterns = (("\r", "\r", "\n"),
                     ("\n", "\n", "\n"),
                     ("\r\n", "\n", "\n"),
                     ("\n\r", "\n\r", "\n\n"),
                     ("\r\r", "\r\r", "\n\n"),
                     ("\n\n", "\n\n", "\n\n"),
                     ("\r\n\r\n", "\n\n", "\n\n"),
                     ("\n\r\n\r", "\n\n\r", "\n\n\n"),
                     ("The quick brown fox", "The quick brown fox", "The quick brown fox"),
                     ("The \rquick\n brown fox\r\n", "The \rquick\n brown fox\n", "The \nquick\n brown fox\n"),
                     ("The \r\rquick\r\n\r\n brown fox", "The \r\rquick\n\n brown fox", "The \n\nquick\n\n brown fox"))

    # Write mode test cases. Same deal as above but with one less member in each
    # tuple due to the lack of a universal newline write mode. The first value
    # represents the in-memory value we start with (and expect to write in binary
    # write mode) and the next value indicates the value we expect to end up on disk
    # in text mode.
    write_patterns = (("\r", "\r"),
                      ("\n", "\r\n"),
                      ("\r\n", "\r\r\n"),
                      ("\n\r", "\r\n\r"),
                      ("\r\r", "\r\r"),
                      ("\n\n", "\r\n\r\n"),
                      ("\r\n\r\n", "\r\r\n\r\r\n"),
                      ("\n\r\n\r", "\r\n\r\r\n\r"),
                      ("The quick brown fox", "The quick brown fox"),
                      ("The \rquick\n brown fox\r\n", "The \rquick\r\n brown fox\r\r\n"),
                      ("The \r\rquick\r\n\r\n brown fox", "The \r\rquick\r\r\n\r\r\n brown fox"))

    # Test a specific read mode pattern.
    def test_read_pattern(pattern):
        # Write the initial data to disk using binary mode (we test this
        # functionality earlier so we're satisfied it gets there unaltered).
        f = file(temp_file, "wb")
        f.write(pattern[0])
        f.close()

        # Read the data back in each read mode, checking that we get the correct
        # transform each time.
        for mode in range(3):
            test_read_mode(pattern, mode);

    # Test a specific read mode pattern for a given reading mode.
    def test_read_mode(pattern, mode):
        # Read the data back from disk using the given read mode.
        f = file(temp_file, read_modes[mode][1])
        contents = f.read()
        f.close()

        # Check it equals what we expected for this mode.
        Assert(contents == pattern[mode])

    # Test a specific write mode pattern.
    def test_write_pattern(pattern):
        for mode in range(2):
            test_write_mode(pattern, mode);

    # Test a specific write mode pattern for a given write mode.
    def test_write_mode(pattern, mode):
        # Write the raw data using the given mode.
        f = file(temp_file, write_modes[mode][1])
        f.write(pattern[0])
        f.close()

        # Read the data back in using binary mode (we tested this gets us back
        # unaltered data earlier).
        f = file(temp_file, "rb")
        contents = f.read()
        f.close()

        # Check it equals what we expected for this mode.
        Assert(contents == pattern[mode])

    # Run through the read and write mode tests for all patterns.
    def test_patterns():
        for pattern in read_patterns:
            test_read_pattern(pattern)
        for pattern in write_patterns:
            test_write_pattern(pattern)

    # Actually run the pattern mode tests.
    test_patterns()

# Now some tests of read(size).
# Test data is in the following format: ("raw data", read_size, (binary mode result strings) (binary mode result tell() result)
#                                                               (text mode result strings) (text mode result tell() result)
#                                                               (universal mode result strings) (univermose mode result tell() results)

def test_read_size():
    read_size_tests = (("Hello", 1, ("H", "e", "l", "l", "o"), (1,2,3,4,5),
                                    ("H", "e", "l", "l", "o"), (1,2,3,4,5),
                                    ("H", "e", "l", "l", "o"), (1,2,3,4,5)),
                       ("Hello", 2, ("He", "ll", "o"), (2,4,5),
                                    ("He", "ll", "o"), (2,4,5),
                                    ("He", "ll", "o"), (2,4,5)),
                       ("H\re\n\r\nllo", 1, ("H", "\r", "e", "\n", "\r", "\n", "l", "l", "o"), (1,2,3,4,5,6,7, 8, 9),
                                            ("H", "\r", "e", "\n", "\n", "l", "l", "o"), (1,2,3,4,6,7,8,9),
                                            ("H", "\n", "e", "\n", "\n", "l", "l", "o"), (1,2,3,4,6,7,8,9)),
                       ("H\re\n\r\nllo", 2, ("H\r", "e\n", "\r\n", "ll", "o"), (2, 4, 6, 8, 9), 
                                            ("H\r", "e\n", "\nl", "lo"), (2,4,7, 9),
                                            ("H\n", "e\n", "\nl", "lo"), (2,4,7, 9)))

    for test in read_size_tests:
        # Write the test pattern to disk in binary mode.
        f = file(temp_file, "wb")
        f.write(test[0])
        f.close()

        # Read the data back in each of the read modes we test.
        for mode in range(3):
            f = file(temp_file, read_modes[mode][1])
            AreEqual(f.closed, False)

            # We read the data in the size specified by the test and expect to get
            # the set of strings given for this specific mode.
            size = test[1]
            strings = test[2 + mode*2]
            lengths = test[3 + mode*2]
            count = 0
            while True:
                data = f.read(size)
                if data == "":
                    Assert(count == len(strings))
                    break
                count = count + 1
                Assert(count <= len(strings))
                Assert(data == strings[count - 1])
                AreEqual(f.tell(), lengths[count-1])

            f.close()
            AreEqual(f.closed, True)

# And some readline tests.
# Test data is in the following format: ("raw data", (binary mode result strings)
#                                                    (text mode result strings)
#                                                    (universal mode result strings))
def test_readline():
    readline_tests = (("Mary had a little lamb", ("Mary had a little lamb", ),
                                                 ("Mary had a little lamb", ),
                                                 ("Mary had a little lamb", )),
                      ("Mary had a little lamb\r", ("Mary had a little lamb\r", ),
                                                   ("Mary had a little lamb\r", ),
                                                   ("Mary had a little lamb\n", )),
                      ("Mary had a \rlittle lamb\r", ("Mary had a \rlittle lamb\r", ),
                                                     ("Mary had a \rlittle lamb\r", ),
                                                     ("Mary had a \n", "little lamb\n")),
                      ("Mary \r\nhad \na little lamb", ("Mary \r\n", "had \n", "a little lamb"),
                                                       ("Mary \n", "had \n", "a little lamb"),
                                                       ("Mary \n", "had \n", "a little lamb")))
    for test in readline_tests:
        # Write the test pattern to disk in binary mode.
        f = file(temp_file, "wb")
        f.write(test[0])
        f.close()

        # Read the data back in each of the read modes we test.
        for mode in range(3):
            f = file(temp_file, read_modes[mode][1])

            # We read the data by line and expect to get a specific sets of lines back.
            strings = test[1 + mode]
            count = 0
            while True:
                data = f.readline()
                if data == "":
                    Assert(count == len(strings))
                    break
                count = count + 1
                Assert(count <= len(strings))
                Assert(data == strings[count - 1])

            f.close()

def format_tuple(tup):
    if tup == None:
        return "None"
    if (isinstance(tup, str)):
        return format_newlines(tup)
    out = "("
    for entry in tup:
        out += format_newlines(entry) + ", "
    out += ")"
    return out

# Test the 'newlines' attribute.
# Format of the test data is the raw data written to the test file followed by a tuple representing the values
# of newlines expected after each line is read from the file in universal newline mode.
def test_newlines_attribute():
    newlines_tests = (("123", (None, )),
                      ("1\r\n2\r3\n", ("\r\n", ("\r\n", "\r"), ("\r\n", "\r", "\n"))),
                      ("1\r2\n3\r\n", ("\r", ("\r", "\n"), ("\r\n", "\r", "\n"))),
                      ("1\n2\r\n3\r", ("\n", ("\r\n", "\n"), ("\r\n", "\r", "\n"))),
                      ("1\r\n2\r\n3\r\n", ("\r\n", "\r\n", "\r\n")),
                      ("1\r2\r3\r", ("\r", "\r", "\r")),
                      ("1\n2\n3\n", ("\n", "\n", "\n")))

    for test in newlines_tests:
        # Write the test pattern to disk in binary mode.
        f = file(temp_file, "wb")
        f.write(test[0])
        # Verify newlines isn't set while writing.
        Assert(f.newlines == None)
        f.close()

        # Verify that reading the file in binary or text mode won't set newlines.
        f = file(temp_file, "rb")
        data = f.read()
        Assert(f.newlines == None)
        f.close()

        f = file(temp_file, "r")
        data = f.read()
        Assert(f.newlines == None)
        f.close()

        # Read file in universal mode line by line and verify we see the expected output at each stage.
        expected = test[1]
        f = file(temp_file, "rU")
        Assert(f.newlines == None)
        count = 0
        while True:
            data = f.readline()
            if data == "":
                break
            Assert(count < len(expected))
            Assert(f.newlines == expected[count])
            count = count + 1
        f.close()
    
## coverage: a sequence of file operation
def test_coverage():
    f = file(temp_file, 'w')
    Assert(str(f).startswith("<open file '%s', mode 'w'" % temp_file))
    Assert(f.fileno() <> -1)
    Assert(f.fileno() <> 0)

    # write
    AssertError(TypeError, f.writelines, [3])
    f.writelines(["firstline\n"])

    f.close()
    Assert(str(f).startswith("<closed file '%s', mode 'w'" % temp_file))

    # append
    f = file(temp_file, 'a+')
    f.writelines(['\n', 'secondline\n'])

    pos = len('secondline\n') + 1
    f.seek(-1 * pos, 1)

    f.writelines(['thirdline\n'])
    f.close()

    # read
    f = file(temp_file, 'r+', 512)
    f.seek(-1 * pos - 2, 2)

    AreEqual(f.readline(), '\n')
    AreEqual(f.readline(5), 'third')
    AreEqual(f.read(-1), 'line\n\n')
    AreEqual(f.read(-1), '')
    f.close()

    # read
    f = file(temp_file, 'rb', 512)
    f.seek(-1 * pos - 2, 2)

    AreEqual(f.readline(), '\r\n')
    AreEqual(f.readline(5), 'third')
    AreEqual(f.read(-1), 'line\r\n\n')
    AreEqual(f.read(-1), '')
    f.close()

    ## file op in nt
    import nt
    nt.unlink(temp_file)

    fd = nt.open(temp_file, nt.O_CREAT | nt.O_WRONLY)
    nt.write(fd, "hello ")
    nt.close(fd)

    fd = nt.open(temp_file, nt.O_APPEND | nt.O_WRONLY)
    nt.write(fd, "world")
    nt.close(fd)

    fd = nt.open(temp_file, 0)
    AreEqual(nt.read(fd, 1024), "hello world")
    nt.close(fd)

    nt.unlink(temp_file)

def test_encoding():
    #verify we start w/ ASCII
    import sys

    f = file(temp_file, 'w')
    f.write(u'\u6211')
    f.close()

    f = file(temp_file, 'r')
    txt = f.read()
    f.close()
    Assert(txt != u'\u6211')


    #and verify UTF8 round trips correctly
    saved = sys.getdefaultencoding()
    
    try:
        setenc = sys.setdefaultencoding
        setenc('utf8')

        f = file(temp_file, 'w')
        f.write(u'\u6211')
        f.close()

        f = file(temp_file, 'r')
        txt = f.read()
        f.close()
        AreEqual(txt, u'\u6211')
    finally: 
        setenc(saved)

if is_cli:
    def test_net_stream():
        import System
        fs = System.IO.FileStream(temp_file, System.IO.FileMode.Create, System.IO.FileAccess.Write)
        f = file(fs, "wb")
        f.write('hello\rworld\ngoodbye\r\n')
        f.close()
        
        f = file(temp_file, 'rb')
        AreEqual(f.read(), 'hello\rworld\ngoodbye\r\n')
        f.close()
        
        f = file(temp_file, 'rU')
        AreEqual(f.read(), 'hello\nworld\ngoodbye\n')
        f.close()
    
    def test_file_manager():
        def return_fd1():
            f = file(temp_file, 'w')
            return f.fileno()
            
        def return_fd2():
            return nt.open(temp_file, 0)
        
        import nt
        import System

        fd = return_fd1()
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()
        AssertError(OSError, nt.fdopen, fd)

        fd = return_fd2()
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()
        f = nt.fdopen(fd)
        f.close()
        AssertError(OSError, nt.fdopen, fd)


run_test(__name__)