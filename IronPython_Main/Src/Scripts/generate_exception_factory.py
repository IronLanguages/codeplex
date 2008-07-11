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

import sys
import generate

def get_parent_directory(path, levels=1):
    while levels:
        pos = path[:-1].rfind('\\')
        if pos < 0:
            return ""
        path = path[:pos]
        levels -= 1
    return path

class ExceptionItem:
    def __init__(self, type, ID, text, param_num):
        self.type = type
        self.ID = ID
        self.text = text
        self.param_num = param_num


def make_item(line1, line2):
    type = line1.rsplit('=', 1)[1]
    splitted = line2.split('=', 1)
    ID = splitted[0]
    text = splitted[1]
    num = text.count('{')
    exception_item = ExceptionItem(type, ID, text ,num)
    return exception_item


def collect_exceptions():
	filename = generate.root_dir + "\\..\\..\\ndp\\fx\\src\\Core\\System\\Linq\\Expressions\\System.Linq.Expressions.txt"
	thefile = open(filename)
	text = thefile.read()
	lines = text.splitlines()
	items = []
	for i in range(len(lines)):
		line1 = lines[i]
		if line1.startswith("##"):
			line2 = lines[i+1]
			items.append(make_item(line1, line2))
			
	items.sort(None, lambda n: n.ID)	
	return items

def make_signature(cnt):
	if cnt == 0:
		return "()"
		
	sig = "("
	for i in range(cnt - 1):
		sig += "object p" + str(i) + ", "
	
	sig += "object p" + str(cnt - 1) + ")"
	return sig

def make_format(text, cnt):
	if cnt == 0:
		return '"' + text + '"'
		
	format = 'string.Format("' + text + '", '
	for i in range(cnt - 1):
		format += "p" + str(i) + ", "
	
	format += "p" + str((cnt - 1)) + ")"
	return format

	
def gen_expr_factory(cw):
    for ex in collect_exceptions():
		result = ""
		if (ex.type == "System.Runtime.InteropServices.COMException"):
			result += "#if !SILVERLIGHT" + "\n"
		result += "/// <summary>" + "\n"
		result += "/// " + ex.type + ' with message like "' + ex.text + '"\n'
		result += "/// </summary>" + "\n"
		if (ex.type == "System.Runtime.InteropServices.COMException"):
			result += '[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]'  + '\n'
		result += "internal static Exception " + str(ex.ID) + make_signature(ex.param_num) + " {" + "\n"
		result += "    return new " + ex.type + "(" + make_format(ex.text, ex.param_num)  + ");" + "\n"
		result += "}" + "\n"
		if (ex.type == "System.Runtime.InteropServices.COMException"):
			result += "#endif" + "\n"
		cw.write(result)

def main():
    return generate.generate(
        ("Exception Factory", gen_expr_factory),
    )

if __name__ == "__main__":
    main()
