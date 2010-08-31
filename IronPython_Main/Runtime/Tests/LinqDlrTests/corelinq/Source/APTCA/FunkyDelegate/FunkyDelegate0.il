
//  Microsoft (R) .NET Framework IL Disassembler.  Version 3.5.20706.1
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v2.0.50727
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 2:0:0:0
}
.assembly FunkyDelegate
{

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 07 01 00 00 00 00 ) 

  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module ConsoleApplication5.dll
// MVID: {88EFB8D6-05C3-4BC3-8B4C-3CE4E0182A15}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x002A0000


// =============== CLASS MEMBERS DECLARATION ===================

.class public auto ansi sealed System.Action`5<T1,T2,T3,T4,
                                               T5>
       extends [mscorlib]System.MulticastDelegate
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(object 'object',
                               native int 'method') runtime managed
  {
  } // end of method Action`5::.ctor

  .method public hidebysig newslot virtual 
          instance void  Invoke(!T1 arg1,
                                !T2 arg2,
                                !T3 arg3,
                                !T4 arg4,
                                !T5 arg5) runtime managed
  {
  } // end of method Action`5::Invoke

  .method public hidebysig newslot virtual 
          instance class [mscorlib]System.IAsyncResult 
          BeginInvoke(!T1 arg1,
                      !T2 arg2,
                      !T3 arg3,
                      !T4 arg4,
                      !T5 arg5,
                      class [mscorlib]System.AsyncCallback callback,
                      object 'object') runtime managed
  {
  } // end of method Action`5::BeginInvoke

  .method public hidebysig newslot virtual 
          instance void  EndInvoke(class [mscorlib]System.IAsyncResult result) runtime managed
  {
  } // end of method Action`5::EndInvoke

} // end of class System.Action`5


// =============================================================

// *********** DISASSEMBLY COMPLETE ***********************
// WARNING: Created Win32 resource file il.res
