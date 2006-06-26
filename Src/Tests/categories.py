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

# make sure all characters in dictionarys lowercase: press Ctrl+U in VS

IronPythonTests = {
            'applications':
                '''
                ''',
            'builtinfuncs':
                '''
                test_builtinfunc
                test_isinstance
                test_execfile
                ''',
            'builtintypes':
                '''
                test_bigint
                test_bool
                test_buffer
                test_complex
                test_dict
                test_file
                test_list
                test_nonetype
                test_set
                test_slice
                test_str
                test_tuple
                test_xrange
                test_iterator   
                test_unicode            
                ''',
            'codedom':
                '''
                test_codedom
                ''',
            'console':
                '''
                test_interactive
                test_stdconsole
                test_superconsole
                ''',
            'hosting':
                '''
                test_ipyc
                test_ipye
                ''',
            'modules':
                '''
                test_binascii
                test_cStringIO
                test_codecs
                test_collections
                test_datetime
                test_errno
                test_gc
                test_imp
                test_itertools
                test_marshal
                test_math
                test_nt
                test_operator
                test_re
                test_socket
                test_struct
                test_sys
                test_thread
                test_time
                test_weakref
                ''',
            'netinterop':
                '''
                test_array
                test_weakref
                test_assembly
                test_callback
                test_clrload
                test_cominterop
                test_delegate
                test_enum
                test_event
                test_genericmeth
                test_generictype
                test_inheritance
                test_methoddispatch
                test_namespace
                test_protected
                test_specialcontext
                test_clrexception
                test_exceptionconverter
                test_privateBinding
                ''',
            'standard':
                '''
                test_assert
                test_attr
                test_class
                test_cliclass
                test_closure
                test_decorator
                test_doc
                test_exceptions
                test_exec
                test_formatting
                test_function
                test_future
                test_generator
                test_importpkg
                test_index
                test_ironmath
                test_kwarg
                test_lambda
                test_listcomp
                test_namebinding
                test_nofuture
                test_number
                test_numtypes
                test_property
                test_syntax
                ''',
            'stress':
                '''
                test_coredata
                test_threadsafety
                test_memory
                ''',
            'perf':
                '''
                ''',
        }
        
MiscTests = {
            'parrot':
                '''
                test_parrot
                ''',
            'pystone': 
                '''
                test_pystone
                ''',
            'cgcheck':
                '''
                test_cgcheck
                ''',
        } 
        
LibraryTests = {
            'library':
                '''
                aifc
                anydbm
                asynchat
                asyncore
                atexit
                audiodev
                base64
                BaseHTTPServer
                Bastion
                bdb
                binhex
                bisect
                calendar
                cgi
                CGIHTTPServer
                cgitb
                chunk
                cmd
                code
                codecs
                codeop
                colorsys
                commands
                compileall
                ConfigParser
                Cookie
                cookielib
                copy
                copy_reg
                csv
                dbhash
                decimal
                difflib
                dircache
                dis
                doctest
                DocXMLRPCServer
                dumbdbm
                dummy_thread
                dummy_threading
                filecmp
                fileinput
                fnmatch
                formatter
                fpformat
                ftplib
                getopt
                getpass
                gettext
                glob
                gopherlib
                gzip
                heapq
                hmac
                htmlentitydefs
                htmllib
                HTMLParser
                httplib
                ihooks
                imaplib
                imghdr
                imputil
                inspect
                keyword
                linecache
                locale
                macpath
                macurl2path
                mailbox
                mailcap
                markupbase
                mhlib
                mimetools
                mimetypes
                MimeWriter
                mimify
                modulefinder
                multifile
                mutex
                netrc
                new
                nntplib
                ntpath
                nturl2path
                opcode
                optparse
                os
                os2emxpath
                pdb
                pickle
                pickletools
                pipes
                pkgutil
                platform
                popen2
                poplib
                posixfile
                posixpath
                pprint
                profile
                pstats
                pty
                pyclbr
                pydoc
                py_compile
                Queue
                quopri
                random
                re
                reconvert
                regex_syntax
                regsub
                repr
                rexec
                rfc822
                rlcompleter
                robotparser
                sched
                sets
                sgmllib
                shelve
                shlex
                shutil
                SimpleHTTPServer
                SimpleXMLRPCServer
                site
                smtpd
                smtplib
                sndhdr
                socket
                SocketServer
                sre
                sre_compile
                sre_constants
                sre_parse
                stat
                statcache
                statvfs
                string
                StringIO
                stringold
                stringprep
                subprocess
                sunau
                sunaudio
                symbol
                symtable
                tabnanny
                tarfile
                telnetlib
                tempfile
                textwrap
                this
                threading
                timeit
                toaiff
                token
                tokenize
                trace
                traceback
                tty
                types
                tzparse
                unittest
                urllib
                urllib2
                urlparse
                user
                UserDict
                UserList
                UserString
                uu
                warnings
                wave
                weakref
                webbrowser
                whichdb
                whrandom
                xdrlib
                xmllib
                xmlrpclib
                zipfile
                _LWPCookieJar
                _MozillaCookieJar
                _strptime
                _threading_local
                __future__
                __phello__.foo
                '''
        }      

RegressionTests = {
            'regression-nochange': 
                '''
                test.test_atexit
                test.test_augassign
                test.test_binop
                test.test_bufio
                test.test_call
                test.test_calendar
                test.test_colorsys
                test.test_contains
                test.test_compare
                test.test_complex
                test.test_decorators
                test.test_dict
                test.test_dircache
                test.test_dummy_thread
                test.test_dummy_threading
                test.test_enumerate
                test.test_errno
                test.test_filecmp
                test.test_fileinput
                test.test_fnmatch
                test.test_fpformat
                test.test_format
                test.test_grammar
                test.test_hexoct
                test.test_htmllib
                test.test_imp
                test.test_list
                test.test_macpath
                test.test_math
                test.test_ntpath
                test.test_operations
                test.test_operator
                test.test_opcodes
                test.test_pep263
                test.test_pkg
                test.test_pkgimport
                test.test_popen
                test.test_popen2
                test.test_queue
                test.test_rfc822
                test.test_urlparse
                test.test_sgmllib
                test.test_shlex
                test.test_slice
                test.test_string
                test.test_struct
                test.test_syntax
                test.test_textwrap
                test.test_thread
                test.test_threading
                test.test_time
                test.test_types
                test.test_unary
                test.test_univnewlines
                test.test_userdict
                test.test_userstring
                test.test_warnings
                test.test_xrange
                ''',  
            'regression-withchange':
            [
    "test.test_bisect",     # doctest support
    "test.test_bool",       # 4 scenarios disabled due to pickle
    "test.test_codecs",     # Pyunycode, Nameprep, and idna not implemented, need to manually import encodings
    "test.test_coercion",   # no copy module
    "test.test_decimal",    # Bugs 972, 975, 973; another 2 cases disabled due to pickle
    "test.test_deque",      # weakref, pickle, itertools not implemented
    "test.test_eof",        # tests for the whole exception string verbatim, changed to test for substring
    "test.test_exceptions", # warnings module
    "test.test_iter",       # reference counter behavior
    "test.test_itertools",
    "test.test_long",       # test_logs() - log of big int's unconvertible to floats
    "test.test_marshal",    # code not implemented, file() operations need to be explicitly closed
    "test.test_repr",       # repr for array module commentted out
    "test.test_richcmp",    # VectorTest disabled (due to __cast?), Also "False is False" == False(rarely)
    "test.test_pow",        # BUG# 884
    "test.test_scope",      # Bugs 961, 962
    "test.test_set",        # weakref, itertools, and pickling not supported
    "test.test_sort",       # finalizer (__del__)
    "test.test_str",        # formatting disabled in string_tests, need to import encodings manually
    "test.test_stringio",   # IP doesn't support buffer, iter(StringIO()) is wrapped IEnumerator
    "test.test_traceback",  # generates files aren't collected, need to close manually
    "test.test_weakref",    # various tests disabled due to collection not being eager enough, additional gc.collect calls
    "test.test_builtin",    # various tests disabled - locals(), dir(), unicode strings
            ],
        }     

CompatTests = {
    "builtin" : 
        '''
        sbs_builtin
        sbs_parse_string
        ''',
    "compare": 
        '''
        sbs_class_compare
        sbs_simple_compare
        ''', 
    "ops": 
        '''
        sbs_simple_ops
        sbs_true_division
        '''
}             
        
LibraryExpectedFailures = {
        "CGIHTTPServer"      : "No module named select",
        "_LWPCookieJar"      : "Cannot import name LWPCookieJar",
        "_MozillaCookieJar"  : "Cannot import name MozillaCookieJar",
        "__phello__.foo"     : "No module named __phello__.foo",
        "_strptime"          : "'module' object has no attribute 'struct_time'",
        "asynchat"           : "No module named select",
        "asyncore"           : "No module named select",
        "csv"                : "No module named _csv",
        "dbhash"             : "No module named _bsddb",
        "gzip"               : "No module named zlib",
        "pty"                : "No module named select",
        "reconvert"          : "No module named regex", 
        "regsub"             : "No module named regex", 
        "rlcompleter"        : "No module named readline",
        "smtpd"              : "No module named select",
        "stringprep"         : "No module named unicodedata",
        "subprocess"         : "No module named select",
        "symtable"           : "No module named _symtable",
        "telnetlib"          : "No module named select",
        "tty"                : "No module named termios",
        "tzparse"            : "'TZ'",
        "urllib2"            : "No module named md5",
    }

if __name__ == '__main__':
    import sys

    thisModule = sys.modules['__main__']
    testList = [ x for x in dir(thisModule) if x.endswith('Tests') ] 
    mapping = {}
    for x in testList:
        mapping[x] = getattr(thisModule, x).keys()

    def printAllCategories():
        print 'Available categories:'
        for x in mapping.iterkeys():
            print ' +', x
            for y in sorted(mapping[x]):
                print ' |--', y
        print 

    def printTestUnderCategory(cat):
        print 'Available tests'
        for x in mapping.iterkeys():
            y = mapping[x]
            if cat in y: 
                print ' +', x
                print ' |-+', cat
                for z in getattr(thisModule, x)[cat].split():
                    print '   |--', z
        print 

    def usage():
        print '''To show all categories, 
        %s -show
    To show one particular category, 
        %s -show <category>
    ''' % ((sys.argv[0], ) * 2)
        sys.exit(1)

    # TODO: support ...
                
    def main(args):
        print 
        if args: 
            if '-show' in args: 
                pos = args.index('-show')
                try: 
                    cat = args[pos+1]
                    printTestUnderCategory(cat)
                except IndexError:
                    printAllCategories()
            else: 
                usage()
        else: 
            printAllCategories()
            usage()

    main([x.lower() for x in sys.argv[1:]]) 
