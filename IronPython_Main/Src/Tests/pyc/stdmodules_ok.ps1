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

#List of CPython standard modules that should be import'able by IronPython
#This should be manually updated every so often.

$STDMODULES = @("bsddb\test\__init__.py",
     "bsddb\dbrecio.py",
     "compiler\consts.py",
     "compiler\misc.py",
     "ctypes\__init__.py",
     "ctypes\macholib\__init__.py",
     "ctypes\macholib\dyld.py",
     "ctypes\macholib\dylib.py",
     "ctypes\macholib\framework.py",
     "ctypes\test\__init__.py",
     "ctypes\test\runtests.py",
     "ctypes\test\test_anon.py",
     "ctypes\test\test_arrays.py",
     "ctypes\test\test_array_in_pointer.py",
     "ctypes\test\test_as_parameter.py",
     "ctypes\test\test_bitfields.py",
     "ctypes\test\test_buffers.py",
     "ctypes\test\test_byteswap.py",
     "ctypes\test\test_callbacks.py",
     "ctypes\test\test_cast.py",
     "ctypes\test\test_cfuncs.py",
     "ctypes\test\test_checkretval.py",
     "ctypes\test\test_delattr.py",
     "ctypes\test\test_errcheck.py",
     "ctypes\test\test_errno.py",
     "ctypes\test\test_find.py",
     "ctypes\test\test_frombuffer.py",
     "ctypes\test\test_funcptr.py",
     "ctypes\test\test_functions.py",
     "ctypes\test\test_incomplete.py",
     "ctypes\test\test_init.py",
     "ctypes\test\test_integers.py",
     "ctypes\test\test_internals.py",
     "ctypes\test\test_keeprefs.py",
     "ctypes\test\test_libc.py",
     "ctypes\test\test_loading.py",
     "ctypes\test\test_macholib.py",
     "ctypes\test\test_memfunctions.py",
     "ctypes\test\test_numbers.py",
     "ctypes\test\test_objects.py",
     "ctypes\test\test_parameters.py",
     "ctypes\test\test_pep3118.py",
     "ctypes\test\test_pickling.py",
     "ctypes\test\test_pointers.py",
     "ctypes\test\test_prototypes.py",
     "ctypes\test\test_python_api.py",
     "ctypes\test\test_random_things.py",
     "ctypes\test\test_refcounts.py",
     "ctypes\test\test_repr.py",
     "ctypes\test\test_returnfuncptrs.py",
     "ctypes\test\test_simplesubclasses.py",
     "ctypes\test\test_sizes.py",
     "ctypes\test\test_slicing.py",
     "ctypes\test\test_stringptr.py",
     "ctypes\test\test_strings.py",
     "ctypes\test\test_structures.py",
     "ctypes\test\test_struct_fields.py",
     "ctypes\test\test_unaligned_structures.py",
     "ctypes\test\test_unicode.py",
     "ctypes\test\test_values.py",
     "ctypes\test\test_varsize_struct.py",
     "ctypes\test\test_win32.py",
     "ctypes\util.py",
     "ctypes\wintypes.py",
     "ctypes\_endian.py",
     "curses\ascii.py",
     "distutils\__init__.py",
     "distutils\command\__init__.py",
     "distutils\command\bdist.py", 
     "distutils\command\bdist_dumb.py", 
     "distutils\command\bdist_rpm.py",
     "distutils\command\bdist_wininst.py",
     "distutils\command\build.py",
     "distutils\command\build_clib.py",
     "distutils\command\build_ext.py",
     "distutils\command\build_py.py",
     "distutils\command\build_scripts.py",
     "distutils\command\clean.py",
     "distutils\command\config.py",
     "distutils\command\install.py",
     "distutils\command\install_data.py",
     "distutils\command\install_egg_info.py",
     "distutils\command\install_headers.py",
     "distutils\command\install_lib.py",
     "distutils\command\install_scripts.py",
     "distutils\command\register.py",
     "distutils\command\sdist.py",
     "distutils\command\upload.py",
     "distutils\tests\__init__.py",
     "distutils\tests\support.py",
     "distutils\tests\test_build_py.py",
     "distutils\tests\test_build_scripts.py",
     "distutils\tests\test_config.py",
     "distutils\tests\test_install.py",
     "distutils\tests\test_install_scripts.py",
     "distutils\tests\test_upload.py",
     "distutils\tests\test_versionpredicate.py",
     "distutils\archive_util.py",
     "distutils\bcppcompiler.py",
     "distutils\ccompiler.py",
     "distutils\cmd.py",
     "distutils\config.py",
     "distutils\core.py",
     "distutils\cygwinccompiler.py",
     "distutils\debug.py",
     "distutils\dep_util.py",
     "distutils\dir_util.py",
     "distutils\dist.py",
     "distutils\emxccompiler.py",
     "distutils\errors.py",
     "distutils\extension.py",
     "distutils\fancy_getopt.py",
     "distutils\filelist.py",
     "distutils\file_util.py",
     "distutils\log.py",
     "distutils\msvccompiler.py",
     "distutils\mwerkscompiler.py",
     "distutils\spawn.py",
     "distutils\sysconfig.py",
     "distutils\text_file.py",
     "distutils\unixccompiler.py",
     "distutils\util.py",
     "distutils\version.py",
     "distutils\versionpredicate.py",
     "email\__init__.py",
     "email\mime\__init__.py",
     "email\mime\application.py",
     "email\mime\audio.py",
     "email\mime\base.py",
     "email\mime\image.py",
     "email\mime\message.py",
     "email\mime\multipart.py",
     "email\mime\nonmultipart.py",
     "email\mime\text.py",
     "email\test\__init__.py",
     "email\base64mime.py",
     "email\charset.py",
     "email\encoders.py",
     "email\errors.py",
     "email\feedparser.py",
     "email\generator.py",
     "email\header.py",
     "email\iterators.py",
     "email\message.py",
     "email\parser.py",
     "email\quoprimime.py",
     "email\utils.py",
     "email\_parseaddr.py",
     "encodings\__init__.py",
     "encodings\aliases.py",
     "encodings\ascii.py",
     "encodings\base64_codec.py",
     "encodings\charmap.py",
     "encodings\cp037.py",
     "encodings\cp1006.py",
     "encodings\cp1026.py",
     "encodings\cp1140.py",
     "encodings\cp1250.py",
     "encodings\cp1251.py",
     "encodings\cp1252.py",
     "encodings\cp1253.py",
     "encodings\cp1254.py",
     "encodings\cp1255.py",
     "encodings\cp1256.py",
     "encodings\cp1257.py",
     "encodings\cp1258.py",
     "encodings\cp424.py",
     "encodings\cp437.py",
     "encodings\cp500.py",
     "encodings\cp737.py",
     "encodings\cp775.py",
     "encodings\cp850.py",
     "encodings\cp852.py",
     "encodings\cp855.py",
     "encodings\cp856.py",
     "encodings\cp857.py",
     "encodings\cp860.py",
     "encodings\cp861.py",
     "encodings\cp862.py",
     "encodings\cp863.py",
     "encodings\cp864.py",
     "encodings\cp865.py",
     "encodings\cp866.py",
     "encodings\cp869.py",
     "encodings\cp874.py",
     "encodings\cp875.py",
     "encodings\hex_codec.py",
     "encodings\hp_roman8.py",
     "encodings\iso8859_1.py",
     "encodings\iso8859_10.py",
     "encodings\iso8859_11.py",
     "encodings\iso8859_13.py",
     "encodings\iso8859_14.py",
     "encodings\iso8859_15.py",
     "encodings\iso8859_16.py",
     "encodings\iso8859_2.py",
     "encodings\iso8859_3.py",
     "encodings\iso8859_4.py",
     "encodings\iso8859_5.py",
     "encodings\iso8859_6.py",
     "encodings\iso8859_7.py",
     "encodings\iso8859_8.py",
     "encodings\iso8859_9.py",
     "encodings\koi8_r.py",
     "encodings\koi8_u.py",
     "encodings\latin_1.py",
     "encodings\mac_arabic.py",
     "encodings\mac_centeuro.py",
     "encodings\mac_croatian.py",
     "encodings\mac_cyrillic.py",
     "encodings\mac_farsi.py",
     "encodings\mac_greek.py",
     "encodings\mac_iceland.py",
     "encodings\mac_latin2.py",
     "encodings\mac_roman.py",
     "encodings\mac_romanian.py",
     "encodings\mac_turkish.py",
     "encodings\mbcs.py",
     "encodings\palmos.py",
     "encodings\ptcp154.py",
     "encodings\punycode.py",
     "encodings\quopri_codec.py",
     "encodings\raw_unicode_escape.py",
     "encodings\rot_13.py",
     "encodings\string_escape.py",
     "encodings\tis_620.py",
     "encodings\undefined.py",
     "encodings\unicode_escape.py",
     "encodings\unicode_internal.py",
     "encodings\utf_16.py",
     "encodings\utf_16_be.py",
     "encodings\utf_16_le.py",
     "encodings\utf_32.py",
     "encodings\utf_32_le.py",
     "encodings\utf_7.py",
     "encodings\utf_8.py",
     "encodings\utf_8_sig.py",
     "encodings\uu_codec.py",
     "hotshot\log.py",
     "idlelib\__init__.py",
     "idlelib\AutoExpand.py",
     "idlelib\Delegator.py",
     "idlelib\HyperParser.py",
     "idlelib\idlever.py",
     "idlelib\PyParse.py",
     "idlelib\RemoteObjectBrowser.py",
     "idlelib\rpc.py",
     "json\__init__.py",
     "json\tests\__init__.py",
     "json\tests\test_decode.py",
     "json\tests\test_default.py",
     "json\tests\test_dump.py",
     "json\tests\test_encode_basestring_ascii.py",
     "json\tests\test_fail.py",
     "json\tests\test_float.py",
     "json\tests\test_indent.py",
     "json\tests\test_pass1.py",
     "json\tests\test_pass2.py",
     "json\tests\test_pass3.py",
     "json\tests\test_recursion.py",
     "json\tests\test_scanstring.py",
     "json\tests\test_separators.py",
     "json\tests\test_speedups.py",
     "json\tests\test_unicode.py",
     "json\decoder.py",
     "json\encoder.py",
     "json\scanner.py",
     "json\tool.py",
     "lib2to3\__init__.py",
     "lib2to3\fixes\__init__.py",
     "lib2to3\pgen2\__init__.py",
     "lib2to3\pgen2\conv.py",
     "lib2to3\pgen2\literals.py",
     "lib2to3\pgen2\token.py",
     "lib2to3\pgen2\tokenize.py",
     "lib2to3\tests\__init__.py",
     "lib2to3\tests\support.py",
     "lib2to3\pytree.py",
     "logging\__init__.py",
     "logging\config.py",
     "logging\handlers.py",
     "msilib\sequence.py",
     "msilib\text.py",
     "multiprocessing\dummy\connection.py",
     "multiprocessing\connection.py",
     "multiprocessing\util.py",
     "sqlite3\test\__init__.py",
     "sqlite3\test\types.py",
     "sqlite3\dump.py",
     "wsgiref\__init__.py",
     "wsgiref\handlers.py",
     "wsgiref\headers.py",
     "wsgiref\simple_server.py",
     "wsgiref\util.py",
     "wsgiref\validate.py",
     "xml\__init__.py",
     "xml\dom\__init__.py",
     "xml\dom\domreg.py",
     "xml\dom\minicompat.py",
     "xml\dom\minidom.py",
     "xml\dom\NodeFilter.py",
     "xml\dom\pulldom.py",
     "xml\dom\xmlbuilder.py",
     "xml\etree\__init__.py",
     "xml\etree\ElementInclude.py",
     "xml\etree\ElementPath.py",
     "xml\etree\ElementTree.py",
     "xml\parsers\__init__.py",
     "xml\sax\__init__.py",
     "xml\sax\handler.py",
     "xml\sax\saxutils.py",
     "xml\sax\xmlreader.py",
     "xml\sax\_exceptions.py",
     "abc.py",
     "aifc.py",
     "anydbm.py",
     "asynchat.py",
     "asyncore.py",
     "atexit.py",
     "audiodev.py",
     "base64.py",
     "BaseHTTPServer.py",
     "Bastion.py",
     "bdb.py",
     "binhex.py",
     "bisect.py",
     "calendar.py",
     "cgi.py",
     "CGIHTTPServer.py",
     "cgitb.py",
     "chunk.py",
     "cmd.py",
     "code.py",
     "codecs.py",
     "codeop.py",
     "collections.py",
     "colorsys.py",
     "commands.py",
     "compileall.py",
     "ConfigParser.py",
     "contextlib.py",
     "Cookie.py",
     "cookielib.py",
     "copy.py",
     "copy_reg.py",
     "decimal.py",
     "difflib.py",
     "dircache.py",
     "dis.py",
     "doctest.py",
     "DocXMLRPCServer.py",
     "dumbdbm.py",
     "dummy_thread.py",
     "dummy_threading.py",
     "filecmp.py",
     "fileinput.py",
     "fnmatch.py",
     "formatter.py",
     "fpformat.py",
     "fractions.py",
     "ftplib.py",
     "functools.py",
     "genericpath.py",
     "getopt.py",
     "getpass.py",
     "gettext.py",
     "glob.py",
     "hashlib.py",
     "heapq.py",
     "hmac.py",
     "htmlentitydefs.py",
     "htmllib.py",
     "HTMLParser.py",
     "httplib.py",
     "ihooks.py",
     "imaplib.py",
     "imghdr.py",
     "imputil.py",
     "inspect.py",
     "io.py",
     "keyword.py",
     "linecache.py",
     "locale.py",
     "macpath.py",
     "macurl2path.py",
     "mailbox.py",
     "mailcap.py",
     "markupbase.py",
     "md5.py",
     "mhlib.py",
     "mimetools.py",
     "mimetypes.py",
     "MimeWriter.py",
     "mimify.py",
     "modulefinder.py",
     "multifile.py",
     "mutex.py",
     "netrc.py",
     "new.py",
     "nntplib.py",
     "ntpath.py",
     "nturl2path.py",
     "numbers.py",
     "opcode.py",
     "optparse.py",
     "os.py",
     "os2emxpath.py",
     "pdb.py",
     "pickle.py",
     "pickletools.py",
     "pipes.py",
     "pkgutil.py",
     "platform.py",
     "plistlib.py",
     "popen2.py",
     "poplib.py",
     "posixfile.py",
     "posixpath.py",
     "pprint.py",
     "profile.py",
     "pstats.py",
     "pyclbr.py",
     "pydoc.py",
     "py_compile.py",
     "Queue.py",
     "quopri.py",
     "random.py",
     "re.py",
     "repr.py",
     "rexec.py",
     "rfc822.py",
     "rlcompleter.py",
     "robotparser.py",
     "runpy.py",
     "sched.py",
     "sets.py",
     "sgmllib.py",
     "sha.py",
     "shelve.py",
     "shlex.py",
     "shutil.py",
     "SimpleHTTPServer.py",
     "SimpleXMLRPCServer.py",
     "site.py",
     "smtpd.py",
     "smtplib.py",
     "sndhdr.py",
     "socket.py",
     "SocketServer.py",
     "sre_compile.py",
     "sre_constants.py",
     "sre_parse.py",
     "ssl.py",
     "stat.py",
     "statvfs.py",
     "string.py",
     "StringIO.py",
     "stringold.py",
     "struct.py",
     "sunau.py",
     "sunaudio.py",
     "symbol.py",
     "tabnanny.py",
     "tarfile.py",
     "telnetlib.py",
     "tempfile.py",
     "textwrap.py",
     "this.py",
     "threading.py",
     "timeit.py",
     "toaiff.py",
     "token.py",
     "tokenize.py",
     "trace.py",
     "traceback.py",
     "types.py",
     "unittest.py",
     "urllib.py",
     "urllib2.py",
     "urlparse.py",
     "user.py",
     "UserDict.py",
     "UserList.py",
     "UserString.py",
     "uu.py",
     "uuid.py",
     "warnings.py",
     "wave.py",
     "weakref.py",
     "whichdb.py",
     "xdrlib.py",
     "xmllib.py",
     "xmlrpclib.py",
     "zipfile.py",
     "_abcoll.py",
     "_LWPCookieJar.py",
     "_MozillaCookieJar.py",
     "_strptime.py",
     "_threading_local.py",
     "__future__.py")
