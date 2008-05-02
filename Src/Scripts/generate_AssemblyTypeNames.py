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

from generate import generate

import sys
import clr
import System

def SortTypesByNamespace(types):
    typesByNamespace = {}
    for t in types:
        # We ignore nested types. They will be loaded when the containing type is loaded
        # t.DeclaringType is failing on Silverlight (DDB 76340)
        if not System.Type.get_DeclaringType(t) == None:
            continue

        namespace = System.Type.get_Namespace(t)
        if not typesByNamespace.has_key(namespace):
            # This is the first type we are seeing from this namespace
            typesByNamespace[namespace] = []

        # t.Name is failing on Silverlight (DDB 76340)
        typeName = System.Reflection.MemberInfo.get_Name(t)
        typesByNamespace[namespace].append(typeName)
        
    for namespace in typesByNamespace.keys():
        typesByNamespace[namespace].sort()

    return typesByNamespace

def CountTypes(typesByNamespace):
    count = 0

    for namespace in typesByNamespace.keys():
        count += len(typesByNamespace[namespace])

    return count

# Try loading an assembly in different ways

def LoadAssembly(assemName):
    try:
        return clr.LoadAssemblyByName(assemName)
    except IOError:
        return clr.LoadAssemblyByPartialName(assemName)
        
    return System.Reflection.Assembly.ReflectionOnlyLoadFrom(assemName)

# The assembly names in Whidbey and Orcas are the same, even though the Orcas platform assemblies
# have new types. Hence, we need to deal with such types specially

def PrintOrcasTypes(cw, assem):
    cw.write('    TypeName [] orcasTypes = {')

    assemName = assem.GetName().Name
    
    if assemName == "mscorlib":
        cw.write('        new TypeName("System", "DateTimeOffset"),')
        cw.write('        new TypeName("System", "GCCollectionMode"),')
        cw.write('        new TypeName("System.Runtime", "GCLatencyMode"),')
    elif assemName == "System":
        cw.write('        new TypeName("System.ComponentModel.Design.Serialization", "IDesignerLoaderHost2"),')
        cw.write('        new TypeName("System.ComponentModel", "INotifyPropertyChanging"),')
        cw.write('        new TypeName("System.ComponentModel", "PropertyChangingEventArgs"),')
        cw.write('        new TypeName("System.ComponentModel", "PropertyChangingEventHandler"),')
        cw.write('        new TypeName("System.Configuration", "IdnElement"),')
        cw.write('        new TypeName("System.Configuration", "IriParsingElement"),')
        cw.write('        new TypeName("System.Configuration", "UriSection"),')
        cw.write('        new TypeName("System.Net.Sockets", "SendPacketsElement"),')
        cw.write('        new TypeName("System.Net.Sockets", "SocketAsyncEventArgs"),')
        cw.write('        new TypeName("System.Net.Sockets", "SocketAsyncOperation"),')
        cw.write('        new TypeName("System", "UriIdnScope"),')
    elif assemName == "System.Windows.Forms":
        cw.write('        new TypeName("System.Windows.Forms", "FileDialogCustomPlace"),')
        cw.write('        new TypeName("System.Windows.Forms", "FileDialogCustomPlacesCollection"),')
    elif assemName == "System.Xml":
        cw.write('        new TypeName("System.Xml.Serialization.Configuration", "RootedPathValidator"),')

    cw.write('    };')
    cw.write('')

# Get all public non-nested types in the assembly and list their names

def PrintTypeNames(cw, assemName):
    assem = LoadAssembly(assemName)
    types = assem.GetExportedTypes()
    typesByNamespace = SortTypesByNamespace(types)

    namespacesCount = len(typesByNamespace)
    sortedNamespaces = typesByNamespace.keys()
    sortedNamespaces.sort()

    cw.write('static IEnumerable<TypeName> Get_%s_TypeNames() {' % assemName.replace(".", ""))
    cw.write('    // %s' % assem.FullName)
    cw.write('    // Total number of types = %s' % CountTypes(typesByNamespace))
    cw.write('')
    cw.write('    string [] namespaces = new string[%i] {' % namespacesCount)
    for namespace in sortedNamespaces:
        cw.write('        "%s",' % namespace)
    cw.write('    };')
    cw.write('')
    cw.write('    string [][] types = new string[%i][] {' % namespacesCount)
    cw.write('')
    for namespace in sortedNamespaces:
        cw.write('        new string[%i] { // %s' % (len(typesByNamespace[namespace]), namespace))
        for t in typesByNamespace[namespace]:
            cw.write('            "%s",' % t)
        cw.write('        },')
        cw.write('')
    cw.write('    };')
    
    PrintOrcasTypes(cw, assem)
    
    cw.write('    return GetTypeNames(namespaces, types, orcasTypes);')
    cw.write('}')
    cw.write('')

# This is the set of assemblies used by a WinForms app. We can add more assemblies depending on the 
# priority of scenarios

default_assemblies = ["mscorlib", "System", "System.Drawing", "System.Windows.Forms", "System.Xml"]

def do_generate(cw):
    for assemName in default_assemblies:
        PrintTypeNames(cw, assemName)

def GetGeneratorList():
    is_orcas = System.Type.GetType("System.Object").Assembly.GetType("System.DateTimeOffset", False) != None
    if is_orcas:
        print "Skipping generate_AssemblyTypeNames on Orcas"
        return []
    elif  'checkonly' in sys.argv:
        # skip checking - even if we're out of date (new types added) the code still does the right thing.
        # If we are out of date then we shouldn't fail tests.
        print "Skipping generate_AssemblyTypeNames in check only mode"
        return []
    else:
        return [("Well-known assembly type names", do_generate)]

def main():
    return generate(*GetGeneratorList())

if __name__ == "__main__":
    main()

