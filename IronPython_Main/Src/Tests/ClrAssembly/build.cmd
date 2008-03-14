csc /nologo /t:library /out:bin\loadorder_1a.dll src\loadorder_1a.cs
csc /nologo /t:library /out:bin\loadorder_1b.dll src\loadorder_1b.cs
csc /nologo /t:library /out:bin\loadorder_1c.dll src\loadorder_1c.cs

csc /nologo /t:library /out:bin\loadorder_2.dll src\loadorder_2.cs
csc /nologo /t:library /out:bin\loadorder_2a.dll src\loadorder_2a.cs
csc /nologo /t:library /out:bin\loadorder_2b.dll src\loadorder_2b.cs
csc /nologo /t:library /out:bin\loadorder_2c.dll src\loadorder_2c.cs
csc /nologo /t:library /out:bin\loadorder_2d.dll src\loadorder_2d.cs
csc /nologo /t:library /out:bin\loadorder_2e.dll src\loadorder_2e.cs
csc /nologo /t:library /out:bin\loadorder_2f.dll src\loadorder_2f.cs
csc /nologo /t:library /out:bin\loadorder_2g.dll src\loadorder_2g.cs
csc /nologo /t:library /out:bin\loadorder_2h.dll src\loadorder_2h.cs

csc /nologo /t:library /out:bin\loadorder_3.dll src\loadorder_3.cs
csc /nologo /t:library /out:bin\loadorder_3a.dll src\loadorder_3a.cs
csc /nologo /t:library /out:bin\loadorder_3b.dll src\loadorder_3b.cs
csc /nologo /t:library /out:bin\loadorder_3c.dll src\loadorder_3c.cs
csc /nologo /t:library /out:bin\loadorder_3d.dll src\loadorder_3d.cs
csc /nologo /t:library /out:bin\loadorder_3e.dll src\loadorder_3e.cs
csc /nologo /t:library /out:bin\loadorder_3f.dll src\loadorder_3f.cs
csc /nologo /t:library /out:bin\loadorder_3g.dll src\loadorder_3g.cs
csc /nologo /t:library /out:bin\loadorder_3h.dll src\loadorder_3h.cs
csc /nologo /t:library /out:bin\loadorder_3i.dll src\loadorder_3i.cs

csc /nologo /t:library /out:bin\loadorder_4.dll src\loadorder_4.cs
csc /nologo /t:library /out:bin\loadorder_4a.dll src\loadorder_4a.cs
csc /nologo /t:library /out:bin\loadorder_4b.dll src\loadorder_4b.cs
csc /nologo /t:library /out:bin\loadorder_4c.dll src\loadorder_4c.cs

csc /nologo /t:library /out:bin\loadorder_5.dll src\loadorder_5.cs
csc /nologo /t:library /out:bin\loadorder_5a.dll src\loadorder_5a.cs
csc /nologo /t:library /out:bin\loadorder_5b.dll src\loadorder_5b.cs
csc /nologo /t:library /out:bin\loadorder_5c.dll src\loadorder_5c.cs

csc /nologo /t:library /out:bin\folder1\loadorder_6.dll src\loadorder_6a.cs
csc /nologo /t:library /out:bin\folder2\loadorder_6.dll src\loadorder_6b.cs

csc /nologo /t:library /out:bin\loadorder_7a.dll src\loadorder_7a.cs
csc /nologo /t:library /out:bin\loadorder_7b.dll src\loadorder_7b.cs
csc /nologo /t:library /out:bin\loadorder_7c.dll src\loadorder_7c.cs

csc /nologo /t:library /out:bin\loadtypesample.dll src\loadtypesample.cs

csc /nologo /t:library /out:bin\missingtype.dll src\missingtype.cs
csc /nologo /t:library /r:bin\missingtype.dll /out:bin\loadexception.dll src\loadexception.cs
del bin\missingtype.dll

csc /nologo /t:library /out:bin\typeforwardee.dll src\typeforwardee.cs
csc /nologo /t:library /r:bin\typeforwardee.dll /out:bin\typeforwarder.dll src\typeforwarder.cs

csc /nologo /t:library /out:bin\typeforwardee2.dll src\typeforwardee2.cs
ilasm /dll /out=bin\typeforwarder2.dll src\typeforwarder2.il

csc /nologo /t:library /out:bin\typeforwardee3.dll src\typeforwardee3.cs
csc /nologo /t:library /r:bin\typeforwardee3.dll /out:bin\typeforwarder3.dll src\typeforwarder3.cs

csc /nologo /t:library /out:bin\typesamples.dll src\typesamples.cs src\testsupport.cs
csc /nologo /t:library /r:bin\typesamples.dll /out:bin\fieldtests.dll src\fieldtests.cs
csc /nologo /t:library /r:bin\typesamples.dll /out:bin\methodargs.dll src\methodargs.cs
csc /nologo /t:library /r:bin\typesamples.dll /out:bin\returnvalues.dll src\returnvalues.cs
csc /nologo /t:library /r:bin\typesamples.dll /out:bin\operators.dll src\operators.cs
csc /nologo /t:library /r:bin\typesamples.dll /out:bin\userdefinedconversions.dll src\userdefinedconversions.cs
csc /nologo /t:library /r:bin\typesamples.dll /out:bin\delegatedefinitions.dll src\delegatedefinitions.cs

csc /nologo /t:library /r:bin\typesamples.dll /out:bin\propertydefinitions.dll src\propertydefinitions.cs

csc /nologo /t:library /r:bin\typesamples.dll /out:bin\indexerdefinitionscs.dll src\indexerdefinitionscs.cs
vbc /nologo /t:library /r:bin\typesamples.dll /out:bin\indexerdefinitionsvb.dll src\indexerdefinitionsvb.vb

csc /nologo /t:library /r:bin\typesamples.dll /out:bin\defaultmemberscs.dll src\defaultmemberscs.cs
vbc /nologo /t:library /r:bin\typesamples.dll /out:bin\defaultmembersvb.dll src\defaultmembersvb.vb
