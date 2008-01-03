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

import generate
reload(generate)
from generate import CodeGenerator, CodeWriter

class TypeData(object):
	"""
data structure used for tracking type data.  

name - the name of the 	backing storage, a private static variable
type - the type name for the dynamic type that we're creating
typeType - the specific DynamicType type that we're creating (e.g. DynamicType, PythonType, etc)
entryName - the public entry name in the type cache.

Constructed as:

TypeData(type, name=None, typeType='DynamicType', entryName=None)


by simply providing the type name you get a DynamicType entry, name same as the the type, with
the private storage as a lower-case named version of the type.
	"""
	__slots__ = ['name', 'type', 'typeType', 'entryName']
	def __init__(self, type, name=None, typeType='DynamicType', entryName=None):
		self.type = type
		if name != None: self.name = name
		else: self.name = type.lower()

		if entryName != None: self.entryName = entryName
		else: self.entryName = self.type
		
		self.typeType = typeType
		
# list of all the types we auto-generate
data = [
	TypeData('Array'),
	TypeData('BuiltinFunction'),
	TypeData('PythonDictionary', entryName='Dict'),
	TypeData('FrozenSetCollection', entryName='FrozenSet'),
	TypeData('PythonFunction', entryName='Function'),
	TypeData('Builtin'),
	TypeData('Generator'),
	TypeData('Object', 'obj'),
	TypeData('SetCollection', entryName='Set'),
	TypeData('DynamicType'),
	TypeData('String', 'str'),
	TypeData('SystemState'),
	TypeData('PythonTuple'),
	TypeData('WeakReference'),
	TypeData('List'),
	TypeData('PythonFile'),
	TypeData('Scope', entryName='Module'),
	TypeData('Method'),
	TypeData('Enumerate'),
	TypeData('Int32', 'intType'),
	TypeData('Double', 'doubleType'),
	TypeData('BigInteger'),
	TypeData('Complex64'),
	TypeData('Super'),
	TypeData('OldClass'),
	TypeData('OldInstance'),
	TypeData('None', 'noneType', entryName='None'),
	TypeData('Boolean', 'boolType'),
]

# groups all of our type data by type, and outputs
# a variable for each one.
def gen_typecache_storage(cw):
	types = {}
	for x in data:
		if(types.has_key(x.typeType)):
			types[x.typeType].append(x)
		else:
			types[x.typeType] = [x]
	for type in types:		
		cw.write('private static %s %s;' % (type, ', '.join(map(lambda t: t.name, types[type]))))
	
# outputs the public getters for each cached type	
def gen_typecache(cw):
	for x in data:
		cw.enter_block("public static %s %s" % (x.typeType, x.entryName))
		cw.enter_block("get")
		
		if x.typeType != 'DynamicType': cast = '(%s)' % x.typeType
		else: cast = ""
		
		cw.write("if (%s == null) %s = %sDynamicHelpers.GetDynamicTypeFromType(typeof(%s));" % (x.name, x.name, cast, x.type))
		cw.write("return %s;" % x.name)
		cw.exit_block()
		cw.exit_block()
		cw.write("")


# do it!
CodeGenerator("TypeCache Storage", gen_typecache_storage).doit()

CodeGenerator("TypeCache Entries", gen_typecache).doit()
