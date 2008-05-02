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

# internal const string AmbiguousFileName = "AmbiguousFileName";
def gen_constant(cw, name):
    cw.write("internal const string " + name + " = \"" + name + "\";")

def gen_constants(cw, const_name_list):
    for const_name in const_name_list:
        gen_constant(cw, const_name)

def get_const_names():
    filename = generate.root_dir + "\\..\\..\\ndp\\fx\\src\\Core\\Microsoft\\Scripting\\Microsoft.Scripting.txt"
    thefile = open(filename)
    text = thefile.read()
    lines = text.splitlines()
    name_list = []
    for  line in lines:
	if line.startswith(";"):
             continue        
	if line.startswith("#"):
             continue
        if line.find("=") > -1:
	     name = line.split("=")[0]
	     name_list.append(name)
    return name_list


def gen_res_contants(cw):
    names = get_const_names()
    gen_constants(cw, names)

def main():
    return generate.generate(
        ("Resource constants", gen_res_contants)
    )

if __name__ == "__main__":
    main()

