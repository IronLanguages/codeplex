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
import re

# None tests
def test_none():
    AssertError(TypeError, re.compile, None)
    AssertError(TypeError, re.compile, None, None)

    AssertError(TypeError, re.search, None, 'abc')
    AssertError(TypeError, re.search, 'abc', None)

    AssertError(TypeError, re.match, None, 'abc')
    AssertError(TypeError, re.match, 'abc', None)

    AssertError(TypeError, re.split, None, 'abc')
    AssertError(TypeError, re.split, 'abc', None)

    AssertError(TypeError, re.findall, None, 'abc')
    AssertError(TypeError, re.findall, 'abc', None)

    AssertError(TypeError, re.finditer,None, 'abc')
    AssertError(TypeError, re.finditer, 'abc', None)

    # Other exceptional input tests

    AssertError(TypeError, re.sub, 'abc', None, 'abc')
    AssertError(TypeError, re.sub, 'abc', None, None)
    AssertError(TypeError, re.sub, None, 'abc', 'abc')
    AssertError(TypeError, re.sub, 'abc', 'abc', None)

    AssertError(TypeError, re.subn, 'abc', None, 'abc')
    AssertError(TypeError, re.subn, 'abc', None, None)
    AssertError(TypeError, re.subn, None, 'abc', 'abc')
    AssertError(TypeError, re.subn, 'abc', 'abc', None)

    AssertError(TypeError, re.escape, None)
    
def test_comment():
    pattern = "a(?#foo)bc"
    c = re.compile(pattern)
    AreEqual(c.findall("abc"), ['abc'])

def test_optional_paren():
     pattern = r"""\(?\w+\)?"""
     c = re.compile(pattern, re.X)
     AreEqual(c.findall('abc'), ['abc'])
     
def test_back_match():
    p = re.compile('(?P<grp>.+?)(?P=grp)')
    AreEqual(p.match('abcabc').groupdict(), {'grp':'abc'})

def test_expand():
	AreEqual(re.match("(a)(b)", "ab").expand("blah\g<1>\g<2>"), "blahab")

def test_sub():    
    x = '\n   #region Generated Foo\nblah\nblah#end region'
    a = re.compile("^([ \t]+)#region Generated Foo.*?#end region", re.MULTILINE|re.DOTALL)
    
    AreEqual(a.sub("xx", x), "\nxx")            # should match successfully
    AreEqual(a.sub("\\x12", x), "\n\\x12")      # should match, but shouldn't un-escape for \x

    #if optional count arg is 0 then all occurrences should be replaced
    AreEqual('bbbb', re.sub("a","b","abab", 0)) 

    AreEqual(re.sub(r'(?P<id>b)', '\g<id>\g<id>yadayada', 'bb'), 'bbyadayadabbyadayada')
    AreEqual(re.sub(r'(?P<id>b)', '\g<1>\g<id>yadayada', 'bb'), 'bbyadayadabbyadayada')
    AssertError(IndexError, re.sub, r'(?P<id>b)', '\g<1>\g<i2>yadayada', 'bb')
    AssertError(IndexError, re.sub, r'(?P<id>b)', '\g<1>\g<30>yadayada', 'bb')
    
    AreEqual(re.sub('x*', '-', 'abc'), '-a-b-c-')
    AreEqual(re.subn('x*', '-', 'abc'), ('-a-b-c-', 4))
    AreEqual(re.sub('a*', '-', 'abc'), '-b-c-')
    AreEqual(re.subn('a*', '-', 'abc'), ('-b-c-', 3))
    AreEqual(re.sub('a*', '-', 'a'), '-')
    AreEqual(re.subn('a*', '-', 'a'), ('-', 1))
    AreEqual(re.sub("a*", "-", "abaabb"), '-b-b-b-')
    AreEqual(re.subn("a*", "-", "abaabb"), ('-b-b-b-', 4))
    AreEqual(re.sub("(a*)b", "-", "abaabb"), '---')
    AreEqual(re.subn("(a*)b", "-", "abaabb"), ('---', 3))
    
    AreEqual(re.subn("(ab)*", "cd", "abababababab", 10), ('cd', 1))
    
    AreEqual(re.sub('x*', '-', 'abxd'), '-a-b-d-')
    AreEqual(re.subn('x*', '-', 'abxd'), ('-a-b-d-', 4))

    Assert(re.sub('([^aeiou])y$', r'\lies', 'vacancy') == 'vacan\\lies')
    Assert(re.sub('([^aeiou])y$', r'\1ies', 'vacancy') == 'vacancies')

def test_dot():
    a = re.compile('.')
    AreEqual(a.groupindex, {})
    
    p = re.compile('.')
    z = []
    for c in p.finditer('abc'):  z.append((c.start(), c.end()))
    z.sort()
    AreEqual(z, [(0,1), (1,2), (2,3)])

def test_x():
    nonmatchingp = re.compile('x')
    AreEqual(nonmatchingp.search('ecks', 1, 4), None)

def test_match():
    p = re.compile('.')
    AreEqual(p.match('foobar', 1,2).span(), (1,2))
 
def test_startandend():
    m = re.match(r'(a)|(b)', 'b')
    AreEqual(m.groups(), (None, 'b'))
    AreEqual(m.group(0), "b")
    AreEqual(m.start(0), 0)
    AreEqual(m.end(0), 1)
    AreEqual(m.start(1), -1)
    AreEqual(m.end(1), -1)
    m = re.match(".*", '')
    AreEqual(m.groups(), ())
    AreEqual(m.start(0), 0)
    AreEqual(m.end(0), 0)
    AssertError(IndexError, m.group, "112")
    AssertError(IndexError, m.group, 112)
    AssertError(IndexError, m.group, "-1")
    AssertError(IndexError, m.group, -1)
    AssertError(IndexError, m.start, 112)
    AssertError(IndexError, m.start, -1)
    AssertError(IndexError, m.end, "112")
    AssertError(IndexError, m.end, 112)
    AssertError(IndexError, m.end, "-1")
    AssertError(IndexError, m.end, -1)

def test_start_of_str():
    startOfStr = re.compile('^')
    AreEqual(startOfStr.match('foobar', 1), None)
    AreEqual(startOfStr.match('foobar', 0,0).span(), (0,0))
    # BUG 674
    #AreEqual(startOfStr.match('foobar', 1,2), None)
    # /BUG

    # BUG
    #AreEqual(startOfStr.match('foobar', endpos=3).span(), (0,0))
    #/BUG

# check that groups in split RE are added properly
def test_split():
    AreEqual(re.split('{(,)?}', '1 {} 2 {,} 3 {} 4'), ['1 ', None, ' 2 ', ',', ' 3 ', None, ' 4'])

    # BUG 637
    
    pnogrp = ','
    
    ptwogrp = '((,))'
    csv = '0,1,1,2,3,5,8,13,21,44'
    AreEqual(re.split(pnogrp, csv, 1), ['0', csv[2:]])
    AreEqual(re.split(pnogrp, csv, 2), ['0','1', csv[4:]])
    AreEqual(re.split(pnogrp, csv, 1000), re.split(pnogrp, csv))
    AreEqual(re.split(pnogrp, csv, 0), re.split(pnogrp, csv))
    AreEqual(re.split(pnogrp, csv, -1), [csv])
    
    ponegrp = '(,)'
    AreEqual(re.split(ponegrp, csv, 1), ['0', ',', csv[2:]])
    # /BUG

def test_escape():
    compiled = re.compile(re.escape("hi_"))
    
    all = re.compile('(.*)')
    AreEqual(all.search('abcdef', 3).group(0), 'def')
    
    AssertError(IndexError, re.match("a[bcd]*b", 'abcbd').group, 1)
    AreEqual(re.match('(a[bcd]*b)', 'abcbd').group(1), 'abcb')

    s = ''
    for i in range(32, 128):
        if not chr(i).isalnum():
            s = s + chr(i)
    x = re.escape(s)
    Assert(x == '\\ \\!\\"\\#\\$\\%\\&\\\'\\(\\)\\*\\+\\,\\-\\.\\/\\:\\;\\<\\=\\>\\?\\@\\[\\\\\\]\\^\\_\\`\\{\\|\\}\\~\\\x7f')

# From the docs: "^" matches only at the start of the string, or in MULTILINE mode also immediately 
# following a newline.
# bug 827 
#m = re.compile("a").match("ba", 1)  # succeed
#AreEqual('a', m.group(0)) 
#AreEqual(re.compile("^a").search("ba", 1), None)		# fails; 'a' not at start   
#AreEqual(re.compile("^a").search("\na", 1), None)		# fails; 'a' not at start
#m = re.compile("^a", re.M).search("\na", 1)				# succeed (multiline)
#AreEqual('a', m.group(0))

# bug 938
#AreEqual(re.compile("^a", re.M).search("ba", 1), None)	# fails; no preceding \n



# findall
def test_findall():
    l = re.findall('\d+', '99 blahblahblah 183 blah 12 blah 7777 yada yada')
    Assert(l == ['99', '183', '12', '7777'])
    l =re.findall('^\d+', '0blahblahblah blah blah yada yada1')
    Assert(l == ['0'])
    l =re.findall('^\d+', 'blahblahblah blah blah yada yada1')
    Assert(l == [])
    
    expr = "x = 999y + 23"
    l = re.findall("(\d+)|(\w+)", expr)
    Assert(l == [('', 'x'), ('999', ''), ('', 'y'), ('23', '')])
    
    digits = "123456789123456789"
    l = re.findall("(\d)(\d\d)(\d\d\d)", digits)
    Assert(l == [('1', '23', '456'), ('7', '89', '123'), ('4', '56', '789')])
    
    sentence = "green fish black fish red fish blue fish"
    l = re.findall(r"(?i)(\w+)\s+fish\b",sentence)
    Assert(l == ['green', 'black', 'red', 'blue'])

def test_match_groups():
    m = re.match('(?P<test>a)(b)', 'ab')
    Assert(m.groups() == ('a', 'b'))
    m = re.match('(u)(?P<test>v)(b)(?P<Named2>w)(x)(y)', 'uvbwxy')
    Assert(m.groups() == ('u', 'v', 'b', 'w', 'x', 'y'))



def test_options():
    # coverage for ?iLmsux options in re.compile path
    c = re.compile("(?i:foo)") # ignorecase
    l = c.findall("fooFoo FOO fOo fo oFO O\n\t\nFo ofO O")
    Assert(l == ['foo', 'Foo', 'FOO', 'fOo'])
    
    c = re.compile("(?im:^foo)") # ignorecase, multiline (matches at beginning of string and at each newline)
    l = c.findall("fooFoo FOO fOo\n\t\nFoo\nFOO")
    Assert(l == ['foo', 'Foo', 'FOO'])
    
    c = re.compile("(?s:foo.*bar)") # dotall (make "." match any chr, including a newline)
    l = c.findall("foo yadayadayada\nyadayadayada bar")
    Assert(l == ['foo yadayadayada\nyadayadayada bar'])
    
    c = re.compile("(?x:foo  bar)") #verbose (ignore whitespace)
    l = c.findall("foobar foo bar      foobar \n\n\tfoobar")
    Assert(l == ['foobar', 'foobar', 'foobar'])
    
    pattern = "t(?=s)"
    c = re.compile(pattern)
    l = c.findall("atreftsadbeatwttta")
    Assert(l == ['t'])
    
    pattern = "t(?!s)"
    c = re.compile(pattern)
    l = c.findall("atreftsadbeatststs")
    Assert(l == ['t'])

# bug 858
#pattern = r"""\(? #optional paren
#...   \)? #optional paren
#...   \d+ """
#c = re.compile(pattern, re.X)
#l = c.findall("989")
#Assert(l == ['989'])

def test_finditer():
    # finditer 
    matches = re.finditer("foo","barfoobarfoobar")
    num = 0
    for m in matches:
        num = num + 1
        AreEqual("foo", m.group(0))
    Assert(num == 2)


def test_search():
    # search
    sp = re.search('super', 'blahsupersuper').span()
    Assert(sp == (4, 9))
    
    sp = re.search('super', 'superblahsuper').span()
    Assert(sp == (0, 5))

    #bug 850
    #re.search.group() index error
    
    AreEqual(re.search("z.*z", "az123za").group(),'z123z')
    AreEqual(re.search("z.*z", "az12za").group(),'z12z')
    AreEqual(re.search("z.*z", "azza").group(),'zz')
    AreEqual(re.search("z123p+z", "az123ppppppppppza").group(),'z123ppppppppppz')
    AreEqual(re.search("z123p+z", "az123pza").group(),'z123pz')
    AreEqual(re.search("z123p?z", "az123pza").group(),'z123pz')
    AreEqual(re.search("z123p?z", "az123za").group(),'z123z')
    
    # bug 870
    AreEqual(re.search('b', 'abc').string, 'abc')
    
def test_subn():
    # subn
    tup = re.subn("ab", "cd", "abababababab")
    Assert(tup == ('cdcdcdcdcdcd', 6))
    tup = re.subn("ab", "cd", "abababababab", 0)
    Assert(tup == ('abababababab', 0))
    tup = re.subn("ab", "cd", "abababababab", 1)
    Assert(tup == ('cdababababab', 1))
    tup = re.subn("ab", "cd", "abababababab", 10)
    Assert(tup == ('cdcdcdcdcdcd', 6))
    tup = re.subn("ababab", "cd", "ab", 10)
    Assert(tup == ('ab', 0))
    tup = re.subn("ababab", "cd", "ab")
    Assert(tup == ('ab', 0))
    
    tup = re.subn("(ab)*", "cd", "abababababab", 10)
    Assert(tup == ('cd', 1))
    tup = re.subn("(ab)?", "cd", "abababababab", 10)
    Assert(tup == ('cdcdcdcdcdcd', 6))


def test_groups():
    reg = re.compile("\[(?P<header>.*?)\]")
    m = reg.search("[DEFAULT]")
    Assert( m.groups() == ('DEFAULT',))
    Assert( m.group('header') == 'DEFAULT' )
    
    reg2 = re.compile("(?P<grp>\S+)?")
    m2 = reg2.search("")
    Assert ( m2.groups() == (None,))
    Assert ( m2.groups('Default') == ('Default',))


def test_end():
    ex = re.compile(r'\s+')
    
    m = ex.match('(object Petal', 7)
    Assert (m.end(0) == 8)


def test_lone_hat():
    """Single ^ reg-ex shouldn't match w/ a sub-set of a string"""
    sol = re.compile('^')

    AreEqual(sol.match('foobar', 1, 2), None)

run_test(__name__)
