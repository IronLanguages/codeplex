using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.Scripting.Ast;

namespace Samples {
    class Program {

        static void Main(string[] args) {

            Samples.CAdd.Add1();
            Samples.CAdd.Add2();
            Samples.CAdd.AddChecked1();
            Samples.CAdd.AddChecked2();
            Samples.CAdd.AddAssign1();
            Samples.CAdd.AddAssign2();
            Samples.CAdd.AddAssign3();
            Samples.CAdd.AddAssignChecked1();
            Samples.CAdd.AddAssignChecked2();
            Samples.CAdd.AddAssignChecked3();

            Samples.CAnd.And1();
            Samples.CAnd.And2();
            Samples.CAnd.AndAlso1();
            Samples.CAnd.AndAlso2();
            Samples.CAnd.AndAssign1();
            Samples.CAnd.AndAssign2();
            Samples.CAnd.AndAssign3();

            Samples.CArray.ArrayAccess1();
            Samples.CArray.ArrayAccess2();
            Samples.CArray.ArrayAccess3();
            Samples.CArray.ArrayAccess4();
            Samples.CArray.ArrayAccess5();
            Samples.CArray.ArrayAccess6();

            Samples.CAssign.Assign1();

            Samples.CBind.Bind1();
            Samples.CBind.Bind2();

            Samples.CBlock.Block1();
            Samples.CBlock.Block2();
            Samples.CBlock.Block3();
            Samples.CBlock.Block4();
            Samples.CBlock.Block5();
            Samples.CBlock.Block6();
            Samples.CBlock.Block7();
            Samples.CBlock.Block8();
            Samples.CBlock.Block9();
            Samples.CBlock.Block10();
            Samples.CBlock.Block11();
            Samples.CBlock.Block12();

            Samples.CBreak.Break1();
            Samples.CBreak.Break2();
            Samples.CBreak.Break3();
            Samples.CBreak.Break4();

            Samples.CCall.Call1();
            Samples.CCall.Call2();
            Samples.CCall.Call3();
            Samples.CCall.Call4();
            Samples.CCall.Call5();
            Samples.CCall.Call6();
            Samples.CCall.Call7();
            Samples.CCall.Call8();
            Samples.CCall.Call9();
            Samples.CCall.Call10();
            Samples.CCall.Call11();
            Samples.CCall.Call12();
            Samples.CCall.Call13();
            Samples.CCall.Call14();

            /*Samples.CCatch.Catch1();
            Samples.CCatch.Catch2();
            Samples.CCatch.Catch3();
            Samples.CCatch.Catch4();*/

            Samples.CClearDebugInfo.ClearDebugInfo1();

            Samples.CCoalesce.Coalesce1();
            Samples.CCoalesce.Coalesce2();

            Samples.CCondition.Condition1();
            Samples.CCondition.Condition2();

            Samples.CConstant.Constant1();
            Samples.CConstant.Constant2();

            Samples.CContinue.Continue1();
            Samples.CContinue.Continue2();

            Samples.CConvert.Convert1();
            Samples.CConvert.Convert2();

            Samples.CConvertChecked.ConvertChecked1();
            Samples.CConvertChecked.ConvertChecked2();

            Samples.CDebugInfo.DebugInfo1();

            Samples.CDecrement.Decrement1();
            Samples.CDecrement.Decrement2();

            Samples.CDefault.Default1();

            Samples.CDivide.Divide1();
            Samples.CDivide.Divide2();

            Samples.CDivide.DivideAssign1();
            Samples.CDivide.DivideAssign2();
            Samples.CDivide.DivideAssign3();

            Samples.CDynamic.Dynamic1();
            Samples.CDynamic.Dynamic2();
            Samples.CDynamic.Dynamic3();
            Samples.CDynamic.Dynamic4();
            Samples.CDynamic.Dynamic5();
            Samples.CDynamic.Dynamic6();

            Samples.CElementInit.ElementInit1();

            Samples.CEmpty.Empty1();

            Samples.CEqual.Equal1();
            Samples.CEqual.Equal2();

            Samples.CExclusiveOr.ExclusiveOr1();
            Samples.CExclusiveOr.ExclusiveOr2();
            Samples.CExclusiveOr.ExclusiveOrAssign1();
            Samples.CExclusiveOr.ExclusiveOrAssign2();
            Samples.CExclusiveOr.ExclusiveOrAssign3();

            Samples.CField.Field1();
            Samples.CField.Field2();
            Samples.CField.Field3();


            Samples.CGreaterThan.GreaterThan1();
            Samples.CGreaterThan.GreaterThan2();

            Samples.CGreaterThanOrEqual.GreaterThanOrEqual1();
            Samples.CGreaterThanOrEqual.GreaterThanOrEqual2();

            Samples.CGetActionType.Action1();

            Samples.CGetDelegateType.Delegate1();

            Samples.CGetFuncType.Func1();

            Samples.CGoto.Goto1();
            Samples.CGoto.Goto2();
            Samples.CGoto.Goto3();
            Samples.CGoto.Goto4();

            Samples.CIncrement.Increment1();
            Samples.CIncrement.Increment2();

            Samples.CIfThen.IfThen1();

            Samples.CIfThenElse.IfThenElse1();

            Samples.CInvoke.Invoke1();
            Samples.CInvoke.Invoke2();

            Samples.CIsFalse.IsFalse1();
            Samples.CIsFalse.IsFalse2();

            Samples.CIsTrue.IsTrue1();
            Samples.CIsTrue.IsTrue2();

            Samples.CLabel.Label1();
            Samples.CLabel.Label2();
            Samples.CLabel.Label3();
            Samples.CLabel.Label4();
            Samples.CLabel.Label5();
            Samples.CLabel.Label6();

            Samples.CLambda.Lambda1();
            Samples.CLambda.Lambda2();
            Samples.CLambda.Lambda3();
            Samples.CLambda.Lambda4();
            Samples.CLambda.Lambda5();
            Samples.CLambda.Lambda6();
            Samples.CLambda.Lambda7();
            Samples.CLambda.Lambda8();
            Samples.CLambda.Lambda9();

            Samples.CLeftShift.LeftShift1();
            Samples.CLeftShift.LeftShift2();
            Samples.CLeftShift.LeftShiftAssign1();
            Samples.CLeftShift.LeftShiftAssign2();
            Samples.CLeftShift.LeftShiftAssign3();

            Samples.CLessThan.LessThan1();
            Samples.CLessThan.LessThan2();

            Samples.CLessThanOrEqual.LessThanOrEqual1();
            Samples.CLessThanOrEqual.LessThanOrEqual2();

            Samples.CListBind.ListBind1();
            Samples.CListBind.ListBind2();
            Samples.CListBind.ListBind3();
            Samples.CListBind.ListBind4();

            Samples.CListInit.ListInit1();
            Samples.CListInit.ListInit2();
            Samples.CListInit.ListInit3();
            Samples.CListInit.ListInit4();
            Samples.CListInit.ListInit5();
            Samples.CListInit.ListInit6();

            Samples.CLoop.Loop1();
            Samples.CLoop.Loop2();
            Samples.CLoop.Loop3();
            Samples.CLoop.Loop4();

            Samples.CMakeBinary.MakeBinary1();
            Samples.CMakeBinary.MakeBinary2();
            Samples.CMakeBinary.MakeBinary3();

            //Samples.CMakeCatchBlock.MakeCatchBlock1();

            Samples.CMakeDynamic.MakeDynamic1();
            Samples.CMakeDynamic.MakeDynamic2();
            Samples.CMakeDynamic.MakeDynamic3();
            Samples.CMakeDynamic.MakeDynamic4();
            Samples.CMakeDynamic.MakeDynamic5();
            Samples.CMakeDynamic.MakeDynamic6();

            Samples.CMakeGoto.MakeGoto1();

            Samples.CMakeIndex.MakeIndex1();
            
            Samples.CMakeMemberAccess.MakeMemberAccess1();

            Samples.CMakeTry.MakeTry1();

            Samples.CMakeUnary.MakeUnary1();
            Samples.CMakeUnary.MakeUnary2();

            Samples.CNotEqual.NotEqual1();
            Samples.CNotEqual.NotEqual2();

            Samples.CQuote.Quote1();

            Samples.CRuntimeVariables.RuntimeVariables1();
            Samples.CRuntimeVariables.RuntimeVariables2();

            Samples.CSymbolDocument.SymbolDocument1();
            Samples.CSymbolDocument.SymbolDocument2();
            Samples.CSymbolDocument.SymbolDocument3();
            Samples.CSymbolDocument.SymbolDocument4();

            Samples.CTryGetActionType.Action1();
            Samples.CTryGetFuncType.Func1();



            // I named all my samples with Sample in the method name so this will run them all
            // I just comment out the Sample part of the name to disable any that don't work
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type t in types) {
                MethodInfo[] samples = t.GetMethods();
                foreach (var s in samples) {
                    if (s.Name.Contains("Sample")) {
                        s.Invoke((object)null, new object[] { });
                    }
                }
            }
        }

        
    }
}

