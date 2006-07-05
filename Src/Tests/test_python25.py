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
# test python25
#

from lib.assert_util import *
from System import Environment
isPython25 = "-X:Python25" in System.Environment.GetCommandLineArgs()

if isPython25:
	def test_string_partition():
		AreEqual('http://www.codeplex.com/WorkItem/List.aspx?ProjectName=IronPython'.partition('://'), ('http','://','www.codeplex.com/WorkItem/List.aspx?ProjectName=IronPython'))
		AreEqual('http://www.codeplex.com/WorkItem/List.aspx?ProjectName=IronPython'.partition('stringnotpresent'), ('http://www.codeplex.com/WorkItem/List.aspx?ProjectName=IronPython','',''))
		AreEqual('stringisnotpresent'.partition('presentofcoursenot'), ('stringisnotpresent','',''))
		AreEqual(''.partition('stringnotpresent'), ('','',''))
		AreEqual('onlymatchingtext'.partition('onlymatchingtext'), ('','onlymatchingtext',''))
		AreEqual('alotoftextherethatisapartofprefixonlyprefix_nosuffix'.partition('_nosuffix'), ('alotoftextherethatisapartofprefixonlyprefix','_nosuffix',''))
		AreEqual('noprefix_alotoftextherethatisapartofsuffixonlysuffix'.partition('noprefix_'), ('','noprefix_','alotoftextherethatisapartofsuffixonlysuffix'))
		AreEqual('\0'.partition('\0'), ('','\0',''))
		AreEqual('\00\ff\67\56\d8\89\33\09\99\ee\20\00\56\78\45\77\e9'.partition('\00\56\78'), ('\00\ff\67\56\d8\89\33\09\99\ee\20','\00\56\78','\45\77\e9'))
		AreEqual('\00\ff\67\56\d8\89\33\09\99\ee\20\00\56\78\45\77\e9'.partition('\78\45\77\e9'), ('\00\ff\67\56\d8\89\33\09\99\ee\20\00\56','\78\45\77\e9',''))
		AreEqual('\ff\67\56\d8\89\33\09\99\ee\20\00\56\78\45\77\e9'.partition('\ff\67\56\d8\89\33\09\99'), ('','\ff\67\56\d8\89\33\09\99','\ee\20\00\56\78\45\77\e9'))
		AreEqual(u'\ff\67\56\d8\89\33\09\99some random 8-bit text here \ee\20\00\56\78\45\77\e9'.partition('random'), (u'\ff\67\56\d8\89\33\09\99some ','random',' 8-bit text here \ee\20\00\56\78\45\77\e9'))
		AreEqual(u'\ff\67\56\d8\89\33\09\99some random 8-bit text here \ee\20\00\56\78\45\77\e9'.partition(u'\33\09\99some r'), (u'\ff\67\56\d8\89','\33\09\99some r','andom 8-bit text here \ee\20\00\56\78\45\77\e9'))
		AssertError(ValueError,'sometextheretocauseanexeption'.partition,'')    
		AssertError(ValueError,''.partition,'')    
		AssertError(TypeError,'some\90text\ffhere\78to\88causeanexeption'.partition,None)    
		AssertError(TypeError,''.partition,None)

		prefix = """ this is some random text
		and it has lots of text 
		"""

		sep = """
    			that is multilined
				and includes unicode \00 \56
				\01 \02 \06 \12\33\67\33\ff \ee also"""
		suffix = """
				\78\ff\43\12\23ok"""
		
		str = prefix + sep + suffix				

		AreEqual(str.partition(sep),(prefix,sep,suffix))			
		AreEqual(str.partition('nomatch'),(str,'',''))			
		AssertError(TypeError,str.partition,None)
		AssertError(ValueError,str.partition,'')

	def test_string_rpartition():
		AreEqual('http://www.codeplex.com/WorkItem/List.aspx?Project://Name=IronPython'.rpartition('://'), ('http://www.codeplex.com/WorkItem/List.aspx?Project','://','Name=IronPython'))
		AreEqual('http://www.codeplex.com/WorkItem/List.aspx?ProjectName=IronPython'.rpartition('stringnotpresent'), ('http://www.codeplex.com/WorkItem/List.aspx?ProjectName=IronPython','',''))
		AreEqual('stringisnotpresent'.rpartition('presentofcoursenot'), ('stringisnotpresent','',''))
		AreEqual(''.rpartition('stringnotpresent'), ('','',''))
		AreEqual('onlymatchingtext'.rpartition('onlymatchingtext'), ('','onlymatchingtext',''))
		AreEqual('alotoftextherethatisapartofprefixonlyprefix_nosuffix'.rpartition('_nosuffix'), ('alotoftextherethatisapartofprefixonlyprefix','_nosuffix',''))
		AreEqual('noprefix_alotoftextherethatisapartofsuffixonlysuffix'.rpartition('noprefix_'), ('','noprefix_','alotoftextherethatisapartofsuffixonlysuffix'))
		AreEqual('\0'.partition('\0'), ('','\0',''))
		AreEqual('\00\ff\67\56\d8\89\33\09\99\ee\20\00\56\78\00\56\78\45\77\e9'.rpartition('\00\56\78'), ('\00\ff\67\56\d8\89\33\09\99\ee\20\00\56\78','\00\56\78','\45\77\e9'))
		AreEqual('\00\ff\67\56\d8\89\33\09\99\ee\20\00\56\78\45\77\e9\78\45\77\e9'.rpartition('\78\45\77\e9'), ('\00\ff\67\56\d8\89\33\09\99\ee\20\00\56\78\45\77\e9','\78\45\77\e9',''))
		AreEqual('\ff\67\56\d8\89\33\09\99\ee\20\00\56\78\45\77\e9'.rpartition('\ff\67\56\d8\89\33\09\99'), ('','\ff\67\56\d8\89\33\09\99','\ee\20\00\56\78\45\77\e9'))
		AreEqual(u'\ff\67\56\d8\89\33\09\99some random 8-bit text here \ee\20\00\56\78\45\77\e9'.rpartition('random'), (u'\ff\67\56\d8\89\33\09\99some ','random',' 8-bit text here \ee\20\00\56\78\45\77\e9'))
		AreEqual(u'\ff\67\56\d8\89\33\09\99some random 8-bit text here \ee\20\00\56\78\45\77\e9'.rpartition(u'\33\09\99some r'), (u'\ff\67\56\d8\89','\33\09\99some r','andom 8-bit text here \ee\20\00\56\78\45\77\e9'))
		AssertError(ValueError,'sometextheretocauseanexeption'.rpartition,'')    
		AssertError(ValueError,''.rpartition,'')    
		AssertError(TypeError,'some\90text\ffhere\78to\88causeanexeption'.rpartition,None)    
		AssertError(TypeError,''.rpartition,None)

		prefix = """ this is some random text
		and it has lots of text 
		"""

		sep = """
    			that is multilined
				and includes unicode \00 \56
				\01 \02 \06 \12\33\67\33\ff \ee also"""
		suffix = """
				\78\ff\43\12\23ok"""
		
		str = prefix + sep + suffix				

		AreEqual(str.rpartition(sep),(prefix,sep,suffix))			
		AreEqual(str.rpartition('nomatch'),(str,'',''))			
		AssertError(TypeError,str.rpartition,None)
		AssertError(ValueError,str.rpartition,'')

				                
	def test_string_startswith():
		class A:pass
		# failure scenarios
		AssertError(TypeError,'string'.startswith,None)
		AssertError(TypeError,'string'.startswith,(None,"strin","str"))
		AssertError(TypeError,'string'.startswith,(None,))
		AssertError(TypeError,'string'.startswith,(["this","is","invalid"],"str","stri"))
		AssertError(TypeError,'string'.startswith,(("string","this is invalid","this is also invalid",),))
		AssertError(TypeError,''.startswith,None)
		AssertError(TypeError,''.startswith,(None,"strin","str"))
		AssertError(TypeError,''.startswith,(None,))
		AssertError(TypeError,''.startswith,(["this","is","invalid"],"str","stri"))
		AssertError(TypeError,''.startswith,(("string","this is invalid","this is also invalid",),))

		# success scenarios
		AreEqual('no matching string'.startswith(("matching","string","here")),False)
		AreEqual('here matching string'.startswith(("matching","string","here")), True)
		AreEqual('here matching string'.startswith(("here", "matching","string","here")), True)
		AreEqual('here matching string'.startswith(("matching","here","string",)), True)
		AreEqual('here matching string'.startswith(("here matching string","here matching string","here matching string",)), True)

		s = 'here \12 \34 \ff \e5 \45 matching string'
		m = "here \12 \34 \ff \e5 \45 "
		m1 = " \12 \34 \ff \e5 \45 "
		n = "here \12 \34 \ff \e5 \46 "
		n1 = " \12 \34 \ff \e5 \46 "
	    
		AreEqual(s.startswith((m,None)), True)
		AreEqual(s.startswith((m,123, ["here","good"])), True)
		AreEqual(s.startswith(("nomatch",m,123, ["here","good"])), True)


		# with start parameter  = 0
		AreEqual(s.startswith((m,None),0), True)
		AreEqual(s.startswith((n,"nomatch"),0), False)
		AreEqual(s.startswith((s,"nomatch"),0), True)
		AreEqual(s.startswith((s + "a","nomatch"),0), False)
		AssertError(TypeError, s.startswith,(n,None),0)
		AssertError(TypeError, s.startswith,(None, n),0)
		AssertError(TypeError, s.startswith,(A, None, m),0)

		# with start parameter  > 0
		AreEqual(s.startswith((m1,None),4), True)
		AreEqual(s.startswith((m,"nomatch"),4), False)
		AreEqual(s.startswith((n1,"nomatch"),4), False)
		AreEqual(s.startswith((" \12 \34 \fd \e5 \45 ","nomatch"),4), False)
		AreEqual(s.startswith((s," \12 \34 \ff \e5 \45 matching string"),4), True)
		AreEqual(s.startswith((" \12 \34 \ff \e5 \45 matching string" + "a","nomatch"),4), False)
		AssertError(TypeError, s.startswith,(n1,None),4)
		AssertError(TypeError, s.startswith,(None, n1),4)
		AssertError(TypeError, s.startswith,(A, None, m1),4)

		AreEqual(s.startswith(("g",None),len(s) - 1), True)
		AreEqual(s.startswith(("g","nomatch"),len(s)), False)
		AreEqual(s.startswith(("g","nomatch"),len(s) + 400), False)

		# with start parameter  < 0
		AreEqual(s.startswith(("string",None),-6), True)
		AreEqual(s.startswith(("stro","nomatch"),-6), False)
		AreEqual(s.startswith(("strong","nomatch"),-6), False)
		AreEqual(s.startswith(("stringandmore","nomatch"),-6), False)
		AreEqual(s.startswith(("prefixandstring","nomatch"),-6), False)
		AssertError(TypeError, s.startswith,("string000",None),-6)
		AssertError(TypeError, s.startswith,(None, "string"),-6)
		AssertError(TypeError, s.startswith,(A, None, "string"),-6)

		AreEqual(s.startswith(("here",None),-len(s)), True)
		AreEqual(s.startswith((s,None),-len(s) - 1 ), True)
		AreEqual(s.startswith(("here",None),-len(s) - 400), True)

		# with start and end parameters  
		  # with +ve start , +ve end
			# end > start
		AreEqual(s.startswith((m1,None),4,len(s)), True)
		AreEqual(s.startswith((m1,None),4,len(s) + 100), True)
		AreEqual(s.startswith((n1,"nomatch"),len(s)), False)
		AssertError(TypeError, s.startswith,(n1,None),4, len(s))
		AssertError(TypeError, s.startswith,(None, n1),4 , len(s) + 100)
		AssertError(TypeError, s.startswith,(A, None, m1),4, len(s))

			# end < start
		AreEqual(s.startswith((m1,None),4,3), False)
		AreEqual(s.startswith((m1,None),4,2), False)
		AreEqual(s.startswith((n1,None),4, 3),False)
		AreEqual(s.startswith((None, n1),4 , 3),False)
		AreEqual(s.startswith((A, None, m1),4, 0),False)
	    
			# end == start
		AreEqual(s.startswith(("",None),4,4), True)
		AreEqual(s.startswith((m1,),4,4), False)
		AssertError(TypeError, s.startswith,(n1,None),4, 4)
		AssertError(TypeError, s.startswith,(None, n1),4 , 4)
		AssertError(TypeError, s.startswith,(A, None, m1),4, 4)

		 # with -ve start , +ve end
		   # end > start
		AreEqual(s.startswith(("string",None),-6, len(s)), True)
		AreEqual(s.startswith(("string",None),-6, len(s) + 100), True)
		AreEqual(s.startswith(("string","nomatch"),-6, len(s) -2), False)

		AreEqual(s.startswith(("stro","nomatch"),-6, len(s)-1), False)
		AreEqual(s.startswith(("strong","nomatch"),-6,len(s)), False)
		AssertError(TypeError, s.startswith,("string000",None),-6,len(s) + 3)
		AssertError(TypeError, s.startswith,(None, "string"),-6, len(s))
		AssertError(TypeError, s.startswith,(A, None, "string"),-6,len(s))

		AreEqual(s.startswith(("here",None),-len(s), 5), True)
		AreEqual(s.startswith(("here","nomatch"),-len(s), 2), False)
		AreEqual(s.startswith(("here",None),-len(s) - 1, 4 ), True)
		AreEqual(s.startswith(("here","nomatch"),-len(s) - 1, 2 ), False)

			# end < start
		AreEqual(s.startswith(("string",None),-6, 10), False)
		AreEqual(s.startswith(("stro","nomatch"),-6, 10), False)
		AreEqual(s.startswith(("strong","nomatch"),-6,10), False)
		AreEqual(s.startswith(("string000",None),-6,10),False)
		AreEqual(s.startswith((None, "string"),-6, 10),False)
		AreEqual(s.startswith((A, None, "string"),-6,10),False)

			# end == start
		AssertError(TypeError,s.startswith, ("string",None),-6, len(s) -6)
		AreEqual(s.startswith(("",None),-6, len(s) -6), True)


		  # with +ve start , -ve end
			# end > start
		AreEqual(s.startswith((m1,None),4,-5 ), True)
		AreEqual(s.startswith((m1,"nomatch"),4,-(4  + len(m) +1) ), False)
		AssertError(TypeError, s.startswith,(n1,None),4, -5)
		AssertError(TypeError, s.startswith,(None, n1),4 , -5)
		AssertError(TypeError, s.startswith,(A, None, m1),4, -5)

			# end < start
		AreEqual(s.startswith((m1,None),4,-len(s) + 1), False)
		AreEqual(s.startswith((m1,),4,-len(s) + 1), False)
		AreEqual(s.startswith((m1,),4,-500), False)

		AreEqual(s.startswith((n1,None),4, -len(s)),False)
		AreEqual(s.startswith((None, n1),4 , -len(s)),False)
		AreEqual(s.startswith((A, None, m1),4, -len(s)),False)
	    
			# end == start
		AreEqual(s.startswith(("",None),4,-len(s)  + 4), True)
		AreEqual(s.startswith((m1,"nomatch"),4,-len(s)  + 4), False)
		AssertError(TypeError, s.startswith,(n1,None),4, -len(s)  + 4)
		AssertError(TypeError, s.startswith,(None, n1),4 , -len(s)  + 4)
		AssertError(TypeError, s.startswith,(A, None, m1),4, -len(s)  + 4)


		  # with -ve start , -ve end
			# end > start
		AreEqual(s.startswith(("stri",None),-6, -2), True)
		AreEqual(s.startswith(("string","nomatch"),-6, -1), False)

		AreEqual(s.startswith(("stro","nomatch"),-6, -1), False)
		AreEqual(s.startswith(("strong","nomatch"),-6,-1), False)
		AreEqual(s.startswith(("stringand","nomatch"),-6,-1), False)

		AssertError(TypeError, s.startswith,("string000",None),-6, -1)
		AssertError(TypeError, s.startswith,(None, "string"),-6, -1)
		AssertError(TypeError, s.startswith,(A, None, "string"),-6,-1)

		AreEqual(s.startswith(("here","nomatch"),-len(s), -5), True)
		AreEqual(s.startswith(("here","nomatch"),-len(s), -len(s) + 2), False)
		AreEqual(s.startswith(("here","nomatch"),-len(s) - 1, -5 ), True)
		AreEqual(s.startswith(("here","nomatch"),-len(s) - 1,  -len(s) + 2), False)

			# end < start
		AreEqual(s.startswith(("string",None),-6, -7), False)
	  
		AreEqual(s.startswith(("stro","nomatch"),-6, -8), False)
		AreEqual(s.startswith(("strong","nomatch"),-6,-8), False)
		AreEqual(s.startswith(("string000",None),-6,-8),False)
		AreEqual(s.startswith((None, "string"),-6, -8),False)
		AreEqual(s.startswith((A, None, "string"),-6,-8),False)

			# end == start
		AreEqual(s.startswith(("string","nomatch"),-6, -6), False)
		AreEqual(s.startswith(("",None),-6, -6), True)



	def test_string_endswith():
		#failue scenarios
		class A:pass
		AssertError(TypeError,'string'.endswith,None)
		AssertError(TypeError,'string'.endswith,(None,"tring","ing"))
		AssertError(TypeError,'string'.endswith,(None,))
		AssertError(TypeError,'string'.endswith,(["this","is","invalid"],"ring","ing"))
		AssertError(TypeError,'string'.endswith,(("string","this is invalid","this is also invalid",),))
		AssertError(TypeError,''.endswith,None)
		AssertError(TypeError,''.endswith,(None,"tring","ring"))
		AssertError(TypeError,''.endswith,(None,))
		AssertError(TypeError,''.endswith,(["this","is","invalid"],"tring","ring"))
		AssertError(TypeError,''.endswith,(("string","this is invalid","this is also invalid",),))
	    
		#Positive scenarios
		AreEqual('no matching string'.endswith(("matching","no","here")),False)
		AreEqual('here matching string'.endswith(("string", "matching","nomatch")), True)
		AreEqual('here matching string'.endswith(("string", "matching","here","string")), True)
		AreEqual('here matching string'.endswith(("matching","here","string",)), True)
		AreEqual('here matching string'.endswith(("here matching string","here matching string","here matching string",)), True)

		s = 'here \12 \34 \ff \e5 \45 matching string'
		m = "\e5 \45 matching string"
		m1 = "\e5 \45 matching "
		n = "\e5 \45 matching strinh"
		n1 = "\e5 \45 matching_"
	    
		AreEqual(s.endswith((m,None)), True)
		AreEqual(s.endswith((m,123, ["string","good"])), True)
		AreEqual(s.endswith(("nomatch",m,123, ["here","string"])), True)

		#With starts parameter = 0
		AreEqual(s.endswith((m,None),0), True)
		AreEqual(s.endswith((n,"nomatch"),0), False)
		AreEqual(s.endswith((s,"nomatch"),0), True)
		AreEqual(s.endswith((s + "a","nomatch"),0), False)
		AssertError(TypeError, s.endswith,(n,None),0)
		AssertError(TypeError, s.endswith,(None, n),0)
		AssertError(TypeError, s.endswith,(A, None, m),0)

		#With starts parameter > 0
		AreEqual(s.endswith((m,None),4), True)
		AreEqual(s.endswith((m,"nomatch"),4), True)
		AreEqual(s.endswith((n1,"nomatch"),4), False)
		AreEqual(s.endswith((" \12 \34 \fd \e5 \45 ","nomatch"),4), False)
		AreEqual(s.endswith((s," \12 \34 \ff \e5 \45 matching string"),4), True)
		AreEqual(s.endswith((" \12 \34 \ff \e5 \45 matching string" + "a","nomatch"),4), False)
		AssertError(TypeError, s.endswith,(n1,None),4)
		AssertError(TypeError, s.endswith,(None, n1),4)
		AssertError(TypeError, s.endswith,(A, None, m1),4)

		AreEqual(s.endswith(("g",None),len(s) - 1), True)
		AreEqual(s.endswith(("g","nomatch"),len(s)), False)
		AreEqual(s.endswith(("g","nomatch"),len(s) + 400), False)

		#With starts parameter < 0
		AreEqual(s.endswith(("string",None),-6), True)
		AreEqual(s.endswith(("ring",None),-6), True)
		AreEqual(s.endswith(("rong","nomatch"),-6), False)
		AreEqual(s.endswith(("strong","nomatch"),-6), False)
		AreEqual(s.endswith(("stringandmore","nomatch"),-6), False)
		AreEqual(s.endswith(("prefixandstring","nomatch"),-6), False)
		AssertError(TypeError, s.endswith,("string000",None),-6)
		AssertError(TypeError, s.endswith,(None, "string"),-6)
		AssertError(TypeError, s.endswith,(A, None, "string"),-6)

		AreEqual(s.endswith(("string",None),-len(s)), True)
		AreEqual(s.endswith((s,None),-len(s) - 1 ), True)
		AreEqual(s.endswith(("string",None),-len(s) - 400), True)

		#With starts , end parameter 
		  # with +ve start , +ve end
			# end > start
		AreEqual(s.endswith((m1,"nomatch"),4,len(s)), False)
		AreEqual(s.endswith((m1,"nomatch"),4,len(s) - 6), True)
		AreEqual(s.endswith((m1,"nomatch"),4,len(s) - 8), False)
		AreEqual(s.endswith((n1,"nomatch"),4,len(s) - 6), False)
		AssertError(TypeError, s.endswith,(n1,None),4, len(s)-6)
		AssertError(TypeError, s.endswith,(None, n1),4 , len(s)-6)
		AssertError(TypeError, s.endswith,(A, None, m1),4, len(s)-6)

			# end < start
		AreEqual(s.endswith((m1,None),4,3), False)
		AreEqual(s.endswith((n1,None),4, 3),False)
		AreEqual(s.endswith((None, n1),4 , 3),False)
		AreEqual(s.endswith((A, None, m1),4, 0),False)
	    
			# end == start
		AreEqual(s.endswith(("",None),4,4), True)
		AreEqual(s.endswith((m1,),4,4), False)
		AssertError(TypeError, s.endswith,(n1,None),4, 4)
		AssertError(TypeError, s.endswith,(None, n1),4 , 4)
		AssertError(TypeError, s.endswith,(A, None, m1),4, 4)

		  # with -ve start , +ve end
			# end > start
		AreEqual(s.endswith((m1,None),-30, len(s) -6), True)
		AreEqual(s.endswith((m1,None),-300, len(s) -6 ), True)
		AreEqual(s.endswith((m1,"nomatch"),-5, len(s) -6), False)

		AreEqual(s.endswith(("string",None),-30, len(s) + 6), True)
		AreEqual(s.endswith(("string",None),-300, len(s) + 6 ), True)

		AreEqual(s.endswith(("here",None),-len(s), 4), True)
		AreEqual(s.endswith(("here",None),-300, 4 ), True)
		AreEqual(s.endswith(("hera","nomatch"),-len(s), 4), False)
		AreEqual(s.endswith(("hera","nomatch"),-300, 4 ), False)


		AssertError(TypeError, s.endswith,("here000",None),-len(s),4)
		AssertError(TypeError, s.endswith,(None, "here"),-len(s),4)
		AssertError(TypeError, s.endswith,(A, None, "here"),-len(s),4)

			# end < start
		AreEqual(s.endswith(("here",None),-len(s) + 4, 2), False)
	  
		AreEqual(s.endswith(("hera","nomatch"),-len(s) + 4, 2), False)
		AreEqual(s.endswith(("here000",None),-len(s) + 4, 2),False)
		AreEqual(s.endswith((None, "he"),-len(s) + 4, 2),False)
		AreEqual(s.endswith((A, None, "string"),-len(s) + 4, 2),False)

			# end == start
		AssertError(TypeError,s.endswith, ("here",None),-6, len(s) -6)
		AreEqual(s.endswith(("",None),-6, len(s) -6), True)


		  # with +ve start , -ve end
			# end > start
		AreEqual(s.endswith((m1,None),4,-6 ), True)
		AreEqual(s.endswith((m1,"nomatch"),4,-7), False)
		AssertError(TypeError, s.endswith,(n1,None),4, -6)
		AssertError(TypeError, s.endswith,(None, n1),4 , -6)
		AssertError(TypeError, s.endswith,(A, None, m1),4, -6)

			# end < start
		AreEqual(s.endswith((m1,None),4,-len(s) + 1), False)
		AreEqual(s.endswith((m1,),4,-len(s) + 1), False)
		AreEqual(s.endswith((m1,),4,-500), False)

		AreEqual(s.endswith((n1,None),4, -len(s)),False)
		AreEqual(s.endswith((None, n1),4 , -len(s)),False)
		AreEqual(s.endswith((A, None, m1),4, -len(s)),False)
	    
			# end == start
		AreEqual(s.endswith(("",None),4,-len(s)  + 4), True)
		AreEqual(s.endswith((m1,"nomatch"),4,-len(s)  + 4), False)
		AssertError(TypeError, s.endswith,(n1,None),4, -len(s)  + 4)
		AssertError(TypeError, s.endswith,(None, n1),4 , -len(s)  + 4)
		AssertError(TypeError, s.endswith,(A, None, m1),4, -len(s)  + 4)


		  # with -ve start , -ve end
			# end > start
		AreEqual(s.endswith(("stri",None),-6, -2), True)
		AreEqual(s.endswith(("string","nomatch"),-6, -1), False)

		AreEqual(s.endswith(("stro","nomatch"),-6, -2), False)
		AreEqual(s.endswith(("stron","nomatch"),-6,-1), False)
		AreEqual(s.endswith(("stringand","nomatch"),-6,-1), False)

		AssertError(TypeError, s.endswith,("string000",None),-6, -1)
		AssertError(TypeError, s.endswith,(None, "string"),-6, -1)
		AssertError(TypeError, s.endswith,(A, None, "string"),-6,-1)

		AreEqual(s.endswith(("here","nomatch"),-len(s), -len(s)+4), True)
		AreEqual(s.endswith(("here","nomatch"),-len(s), -len(s) + 2), False)
		AreEqual(s.endswith(("here","nomatch"),-len(s) - 1, -len(s)+4 ), True)
		AreEqual(s.endswith(("here","nomatch"),-len(s) - 1,  -len(s) + 2), False)

			# end < start
		AreEqual(s.endswith(("here",None),-len(s) + 5, -len(s) + 4), False)
		AreEqual(s.endswith(("hera","nomatch"),-len(s) + 5, -len(s) + 4), False)
		AreEqual(s.endswith(("here000",None),-len(s) + 5, -len(s) + 4),False)
		AreEqual(s.endswith((None, "here"),-len(s) + 5, -len(s) + 4),False)
		AreEqual(s.endswith((A, None, "here"),-len(s) + 5, -len(s) + 4),False)

			# end == start
		AreEqual(s.endswith(("here","nomatch"),-6, -6), False)
		AreEqual(s.endswith(("",None),-6, -6), True)
    
run_test(__name__)

if not isPython25: 
    from lib.process_util import *
    result = launch_ironpython_changing_extensions(path_combine(testpath.public_testdir, "test_python25.py"), ["-X:Python25"])
    AreEqual(result, 0)

