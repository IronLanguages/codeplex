#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
				
				//-------- Scenario 381
				namespace Scenario381{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Add__2() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ushortq_Add() &
					                check_shortq_Add() &
					                check_uintq_Add() &
					                check_intq_Add() &
					                check_ulongq_Add() &
					                check_longq_Add() &
					                check_floatq_Add() &
					                check_doubleq_Add() &
					                check_decimalq_Add();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushortq_Add() {
					        ushort?[] svals = new ushort?[] { 0, 1, ushort.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_ushortq_Add(svals[i], svals[j])) {
					                    Console.WriteLine("ushortq_Add failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushortq_Add(ushort? val0, ushort? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(ushort?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(ushort?), "p1");
					        Expression<Func<ushort?>> e1 = Expression.Lambda<Func<ushort?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(ushort?)),
					                    Expression.Constant(val1, typeof(ushort?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?> f1 = e1.Compile();
					
					        ushort? f1Result = default(ushort?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<ushort?, ushort?, Func<ushort?>>> e2 = Expression.Lambda<Func<ushort?, ushort?, Func<ushort?>>>(
					            Expression.Lambda<Func<ushort?>>(Expression.Add(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<ushort?, ushort?, Func<ushort?>> f2 = e2.Compile();
					
					        ushort? f2Result = default(ushort?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?, ushort?>>> e3 = Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					                    Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?, ushort?, ushort?> f3 = e3.Compile()();
					
					        ushort? f3Result = default(ushort?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?, ushort?>>> e4 = Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					            Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<ushort?, ushort?, ushort?>> f4 = e4.Compile();
					
					        ushort? f4Result = default(ushort?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<ushort?, Func<ushort?, ushort?>>> e5 =
					            Expression.Lambda<Func<ushort?, Func<ushort?, ushort?>>>(
					                Expression.Lambda<Func<ushort?, ushort?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<ushort?, Func<ushort?, ushort?>> f5 = e5.Compile();
					
					        ushort? f5Result = default(ushort?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?>>> e6 = Expression.Lambda<Func<Func<ushort?, ushort?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ushort?, Func<ushort?, ushort?>>>(
					                    Expression.Lambda<Func<ushort?, ushort?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(ushort?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?, ushort?> f6 = e6.Compile()();
					
					        ushort? f6Result = default(ushort?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        ushort? csResult = default(ushort?);
					        Exception csEx = null;
					        try {
					            csResult = (ushort?) (val0 + val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_shortq_Add() {
					        short?[] svals = new short?[] { 0, 1, -1, short.MinValue, short.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_shortq_Add(svals[i], svals[j])) {
					                    Console.WriteLine("shortq_Add failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_shortq_Add(short? val0, short? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(short?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(short?), "p1");
					        Expression<Func<short?>> e1 = Expression.Lambda<Func<short?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<short?, short?, short?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(short?)),
					                    Expression.Constant(val1, typeof(short?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?> f1 = e1.Compile();
					
					        short? f1Result = default(short?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<short?, short?, Func<short?>>> e2 = Expression.Lambda<Func<short?, short?, Func<short?>>>(
					            Expression.Lambda<Func<short?>>(Expression.Add(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<short?, short?, Func<short?>> f2 = e2.Compile();
					
					        short? f2Result = default(short?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?, short?>>> e3 = Expression.Lambda<Func<Func<short?, short?, short?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<short?, short?, short?>>>(
					                    Expression.Lambda<Func<short?, short?, short?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?, short?, short?> f3 = e3.Compile()();
					
					        short? f3Result = default(short?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?, short?>>> e4 = Expression.Lambda<Func<Func<short?, short?, short?>>>(
					            Expression.Lambda<Func<short?, short?, short?>>(
					                Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<short?, short?, short?>> f4 = e4.Compile();
					
					        short? f4Result = default(short?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<short?, Func<short?, short?>>> e5 =
					            Expression.Lambda<Func<short?, Func<short?, short?>>>(
					                Expression.Lambda<Func<short?, short?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<short?, Func<short?, short?>> f5 = e5.Compile();
					
					        short? f5Result = default(short?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?>>> e6 = Expression.Lambda<Func<Func<short?, short?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<short?, Func<short?, short?>>>(
					                    Expression.Lambda<Func<short?, short?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(short?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?, short?> f6 = e6.Compile()();
					
					        short? f6Result = default(short?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        short? csResult = default(short?);
					        Exception csEx = null;
					        try {
					            csResult = (short?) (val0 + val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_uintq_Add() {
					        uint?[] svals = new uint?[] { 0, 1, uint.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_uintq_Add(svals[i], svals[j])) {
					                    Console.WriteLine("uintq_Add failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_uintq_Add(uint? val0, uint? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(uint?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(uint?), "p1");
					        Expression<Func<uint?>> e1 = Expression.Lambda<Func<uint?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<uint?, uint?, uint?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(uint?)),
					                    Expression.Constant(val1, typeof(uint?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?> f1 = e1.Compile();
					
					        uint? f1Result = default(uint?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<uint?, uint?, Func<uint?>>> e2 = Expression.Lambda<Func<uint?, uint?, Func<uint?>>>(
					            Expression.Lambda<Func<uint?>>(Expression.Add(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<uint?, uint?, Func<uint?>> f2 = e2.Compile();
					
					        uint? f2Result = default(uint?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?, uint?>>> e3 = Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					                    Expression.Lambda<Func<uint?, uint?, uint?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?, uint?, uint?> f3 = e3.Compile()();
					
					        uint? f3Result = default(uint?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?, uint?>>> e4 = Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					            Expression.Lambda<Func<uint?, uint?, uint?>>(
					                Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<uint?, uint?, uint?>> f4 = e4.Compile();
					
					        uint? f4Result = default(uint?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<uint?, Func<uint?, uint?>>> e5 =
					            Expression.Lambda<Func<uint?, Func<uint?, uint?>>>(
					                Expression.Lambda<Func<uint?, uint?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<uint?, Func<uint?, uint?>> f5 = e5.Compile();
					
					        uint? f5Result = default(uint?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?>>> e6 = Expression.Lambda<Func<Func<uint?, uint?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<uint?, Func<uint?, uint?>>>(
					                    Expression.Lambda<Func<uint?, uint?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(uint?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?, uint?> f6 = e6.Compile()();
					
					        uint? f6Result = default(uint?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        uint? csResult = default(uint?);
					        Exception csEx = null;
					        try {
					            csResult = (uint?) (val0 + val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_intq_Add() {
					        int?[] svals = new int?[] { 0, 1, -1, int.MinValue, int.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_intq_Add(svals[i], svals[j])) {
					                    Console.WriteLine("intq_Add failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_intq_Add(int? val0, int? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(int?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(int?), "p1");
					        Expression<Func<int?>> e1 = Expression.Lambda<Func<int?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<int?, int?, int?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(int?)),
					                    Expression.Constant(val1, typeof(int?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?> f1 = e1.Compile();
					
					        int? f1Result = default(int?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<int?, int?, Func<int?>>> e2 = Expression.Lambda<Func<int?, int?, Func<int?>>>(
					            Expression.Lambda<Func<int?>>(Expression.Add(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<int?, int?, Func<int?>> f2 = e2.Compile();
					
					        int? f2Result = default(int?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?, int?>>> e3 = Expression.Lambda<Func<Func<int?, int?, int?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<int?, int?, int?>>>(
					                    Expression.Lambda<Func<int?, int?, int?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?, int?, int?> f3 = e3.Compile()();
					
					        int? f3Result = default(int?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?, int?>>> e4 = Expression.Lambda<Func<Func<int?, int?, int?>>>(
					            Expression.Lambda<Func<int?, int?, int?>>(
					                Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<int?, int?, int?>> f4 = e4.Compile();
					
					        int? f4Result = default(int?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<int?, Func<int?, int?>>> e5 =
					            Expression.Lambda<Func<int?, Func<int?, int?>>>(
					                Expression.Lambda<Func<int?, int?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<int?, Func<int?, int?>> f5 = e5.Compile();
					
					        int? f5Result = default(int?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?>>> e6 = Expression.Lambda<Func<Func<int?, int?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<int?, Func<int?, int?>>>(
					                    Expression.Lambda<Func<int?, int?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(int?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?, int?> f6 = e6.Compile()();
					
					        int? f6Result = default(int?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        int? csResult = default(int?);
					        Exception csEx = null;
					        try {
					            csResult = (int?) (val0 + val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_ulongq_Add() {
					        ulong?[] svals = new ulong?[] { 0, 1, ulong.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_ulongq_Add(svals[i], svals[j])) {
					                    Console.WriteLine("ulongq_Add failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulongq_Add(ulong? val0, ulong? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(ulong?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(ulong?), "p1");
					        Expression<Func<ulong?>> e1 = Expression.Lambda<Func<ulong?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(ulong?)),
					                    Expression.Constant(val1, typeof(ulong?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?> f1 = e1.Compile();
					
					        ulong? f1Result = default(ulong?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<ulong?, ulong?, Func<ulong?>>> e2 = Expression.Lambda<Func<ulong?, ulong?, Func<ulong?>>>(
					            Expression.Lambda<Func<ulong?>>(Expression.Add(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<ulong?, ulong?, Func<ulong?>> f2 = e2.Compile();
					
					        ulong? f2Result = default(ulong?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?, ulong?>>> e3 = Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					                    Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?, ulong?, ulong?> f3 = e3.Compile()();
					
					        ulong? f3Result = default(ulong?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?, ulong?>>> e4 = Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					            Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<ulong?, ulong?, ulong?>> f4 = e4.Compile();
					
					        ulong? f4Result = default(ulong?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<ulong?, Func<ulong?, ulong?>>> e5 =
					            Expression.Lambda<Func<ulong?, Func<ulong?, ulong?>>>(
					                Expression.Lambda<Func<ulong?, ulong?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<ulong?, Func<ulong?, ulong?>> f5 = e5.Compile();
					
					        ulong? f5Result = default(ulong?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?>>> e6 = Expression.Lambda<Func<Func<ulong?, ulong?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ulong?, Func<ulong?, ulong?>>>(
					                    Expression.Lambda<Func<ulong?, ulong?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(ulong?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?, ulong?> f6 = e6.Compile()();
					
					        ulong? f6Result = default(ulong?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        ulong? csResult = default(ulong?);
					        Exception csEx = null;
					        try {
					            csResult = (ulong?) (val0 + val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_longq_Add() {
					        long?[] svals = new long?[] { 0, 1, -1, long.MinValue, long.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_longq_Add(svals[i], svals[j])) {
					                    Console.WriteLine("longq_Add failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_longq_Add(long? val0, long? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(long?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(long?), "p1");
					        Expression<Func<long?>> e1 = Expression.Lambda<Func<long?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<long?, long?, long?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(long?)),
					                    Expression.Constant(val1, typeof(long?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?> f1 = e1.Compile();
					
					        long? f1Result = default(long?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<long?, long?, Func<long?>>> e2 = Expression.Lambda<Func<long?, long?, Func<long?>>>(
					            Expression.Lambda<Func<long?>>(Expression.Add(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<long?, long?, Func<long?>> f2 = e2.Compile();
					
					        long? f2Result = default(long?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?, long?>>> e3 = Expression.Lambda<Func<Func<long?, long?, long?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<long?, long?, long?>>>(
					                    Expression.Lambda<Func<long?, long?, long?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?, long?, long?> f3 = e3.Compile()();
					
					        long? f3Result = default(long?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?, long?>>> e4 = Expression.Lambda<Func<Func<long?, long?, long?>>>(
					            Expression.Lambda<Func<long?, long?, long?>>(
					                Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<long?, long?, long?>> f4 = e4.Compile();
					
					        long? f4Result = default(long?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<long?, Func<long?, long?>>> e5 =
					            Expression.Lambda<Func<long?, Func<long?, long?>>>(
					                Expression.Lambda<Func<long?, long?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<long?, Func<long?, long?>> f5 = e5.Compile();
					
					        long? f5Result = default(long?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?>>> e6 = Expression.Lambda<Func<Func<long?, long?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<long?, Func<long?, long?>>>(
					                    Expression.Lambda<Func<long?, long?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(long?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?, long?> f6 = e6.Compile()();
					
					        long? f6Result = default(long?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        long? csResult = default(long?);
					        Exception csEx = null;
					        try {
					            csResult = (long?) (val0 + val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_floatq_Add() {
					        float?[] svals = new float?[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_floatq_Add(svals[i], svals[j])) {
					                    Console.WriteLine("floatq_Add failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_floatq_Add(float? val0, float? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(float?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(float?), "p1");
					        Expression<Func<float?>> e1 = Expression.Lambda<Func<float?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<float?, float?, float?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(float?)),
					                    Expression.Constant(val1, typeof(float?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?> f1 = e1.Compile();
					
					        float? f1Result = default(float?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<float?, float?, Func<float?>>> e2 = Expression.Lambda<Func<float?, float?, Func<float?>>>(
					            Expression.Lambda<Func<float?>>(Expression.Add(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<float?, float?, Func<float?>> f2 = e2.Compile();
					
					        float? f2Result = default(float?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?, float?>>> e3 = Expression.Lambda<Func<Func<float?, float?, float?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<float?, float?, float?>>>(
					                    Expression.Lambda<Func<float?, float?, float?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?, float?, float?> f3 = e3.Compile()();
					
					        float? f3Result = default(float?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?, float?>>> e4 = Expression.Lambda<Func<Func<float?, float?, float?>>>(
					            Expression.Lambda<Func<float?, float?, float?>>(
					                Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<float?, float?, float?>> f4 = e4.Compile();
					
					        float? f4Result = default(float?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<float?, Func<float?, float?>>> e5 =
					            Expression.Lambda<Func<float?, Func<float?, float?>>>(
					                Expression.Lambda<Func<float?, float?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<float?, Func<float?, float?>> f5 = e5.Compile();
					
					        float? f5Result = default(float?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?>>> e6 = Expression.Lambda<Func<Func<float?, float?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<float?, Func<float?, float?>>>(
					                    Expression.Lambda<Func<float?, float?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(float?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?, float?> f6 = e6.Compile()();
					
					        float? f6Result = default(float?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        float? csResult = default(float?);
					        Exception csEx = null;
					        try {
					            csResult = (float?) (val0 + val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_doubleq_Add() {
					        double?[] svals = new double?[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_doubleq_Add(svals[i], svals[j])) {
					                    Console.WriteLine("doubleq_Add failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_doubleq_Add(double? val0, double? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(double?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(double?), "p1");
					        Expression<Func<double?>> e1 = Expression.Lambda<Func<double?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<double?, double?, double?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(double?)),
					                    Expression.Constant(val1, typeof(double?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?> f1 = e1.Compile();
					
					        double? f1Result = default(double?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<double?, double?, Func<double?>>> e2 = Expression.Lambda<Func<double?, double?, Func<double?>>>(
					            Expression.Lambda<Func<double?>>(Expression.Add(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<double?, double?, Func<double?>> f2 = e2.Compile();
					
					        double? f2Result = default(double?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?, double?>>> e3 = Expression.Lambda<Func<Func<double?, double?, double?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<double?, double?, double?>>>(
					                    Expression.Lambda<Func<double?, double?, double?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?, double?, double?> f3 = e3.Compile()();
					
					        double? f3Result = default(double?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?, double?>>> e4 = Expression.Lambda<Func<Func<double?, double?, double?>>>(
					            Expression.Lambda<Func<double?, double?, double?>>(
					                Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<double?, double?, double?>> f4 = e4.Compile();
					
					        double? f4Result = default(double?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<double?, Func<double?, double?>>> e5 =
					            Expression.Lambda<Func<double?, Func<double?, double?>>>(
					                Expression.Lambda<Func<double?, double?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<double?, Func<double?, double?>> f5 = e5.Compile();
					
					        double? f5Result = default(double?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?>>> e6 = Expression.Lambda<Func<Func<double?, double?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<double?, Func<double?, double?>>>(
					                    Expression.Lambda<Func<double?, double?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(double?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?, double?> f6 = e6.Compile()();
					
					        double? f6Result = default(double?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        double? csResult = default(double?);
					        Exception csEx = null;
					        try {
					            csResult = (double?) (val0 + val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_decimalq_Add() {
					        decimal?[] svals = new decimal?[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_decimalq_Add(svals[i], svals[j])) {
					                    Console.WriteLine("decimalq_Add failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_decimalq_Add(decimal? val0, decimal? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(decimal?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(decimal?), "p1");
					        Expression<Func<decimal?>> e1 = Expression.Lambda<Func<decimal?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(decimal?)),
					                    Expression.Constant(val1, typeof(decimal?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?> f1 = e1.Compile();
					
					        decimal? f1Result = default(decimal?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<decimal?, decimal?, Func<decimal?>>> e2 = Expression.Lambda<Func<decimal?, decimal?, Func<decimal?>>>(
					            Expression.Lambda<Func<decimal?>>(Expression.Add(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<decimal?, decimal?, Func<decimal?>> f2 = e2.Compile();
					
					        decimal? f2Result = default(decimal?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?, decimal?>>> e3 = Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					                    Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?, decimal?, decimal?> f3 = e3.Compile()();
					
					        decimal? f3Result = default(decimal?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?, decimal?>>> e4 = Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					            Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                Expression.Add(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<decimal?, decimal?, decimal?>> f4 = e4.Compile();
					
					        decimal? f4Result = default(decimal?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<decimal?, Func<decimal?, decimal?>>> e5 =
					            Expression.Lambda<Func<decimal?, Func<decimal?, decimal?>>>(
					                Expression.Lambda<Func<decimal?, decimal?>>(
					                    Expression.Add(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<decimal?, Func<decimal?, decimal?>> f5 = e5.Compile();
					
					        decimal? f5Result = default(decimal?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?>>> e6 = Expression.Lambda<Func<Func<decimal?, decimal?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<decimal?, Func<decimal?, decimal?>>>(
					                    Expression.Lambda<Func<decimal?, decimal?>>(
					                        Expression.Add(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(decimal?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?, decimal?> f6 = e6.Compile()();
					
					        decimal? f6Result = default(decimal?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        decimal? csResult = default(decimal?);
					        Exception csEx = null;
					        try {
					            csResult = (decimal?) (val0 + val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static Type GetNonNullableType(Type type) {
				        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
				                type.GetGenericArguments()[0] :
				                type;
				    }
				}
				
			
			
			public  class Helper {
			    public static bool verifyException<T>(T fResult, Exception fEx, T csResult, Exception csEx) {
			        if (fEx != null || csEx != null) {
			            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
			        }
			        else {
			            return object.Equals(fResult, csResult);
			        }
			    }
			}
		
	}
				
				//-------- Scenario 382
				namespace Scenario382{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Subtract__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Subtract__2() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ushortq_Subtract() &
					                check_shortq_Subtract() &
					                check_uintq_Subtract() &
					                check_intq_Subtract() &
					                check_ulongq_Subtract() &
					                check_longq_Subtract() &
					                check_floatq_Subtract() &
					                check_doubleq_Subtract() &
					                check_decimalq_Subtract();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushortq_Subtract() {
					        ushort?[] svals = new ushort?[] { 0, 1, ushort.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_ushortq_Subtract(svals[i], svals[j])) {
					                    Console.WriteLine("ushortq_Subtract failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushortq_Subtract(ushort? val0, ushort? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(ushort?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(ushort?), "p1");
					        Expression<Func<ushort?>> e1 = Expression.Lambda<Func<ushort?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(ushort?)),
					                    Expression.Constant(val1, typeof(ushort?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?> f1 = e1.Compile();
					
					        ushort? f1Result = default(ushort?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<ushort?, ushort?, Func<ushort?>>> e2 = Expression.Lambda<Func<ushort?, ushort?, Func<ushort?>>>(
					            Expression.Lambda<Func<ushort?>>(Expression.Subtract(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<ushort?, ushort?, Func<ushort?>> f2 = e2.Compile();
					
					        ushort? f2Result = default(ushort?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?, ushort?>>> e3 = Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					                    Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?, ushort?, ushort?> f3 = e3.Compile()();
					
					        ushort? f3Result = default(ushort?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?, ushort?>>> e4 = Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					            Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<ushort?, ushort?, ushort?>> f4 = e4.Compile();
					
					        ushort? f4Result = default(ushort?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<ushort?, Func<ushort?, ushort?>>> e5 =
					            Expression.Lambda<Func<ushort?, Func<ushort?, ushort?>>>(
					                Expression.Lambda<Func<ushort?, ushort?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<ushort?, Func<ushort?, ushort?>> f5 = e5.Compile();
					
					        ushort? f5Result = default(ushort?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?>>> e6 = Expression.Lambda<Func<Func<ushort?, ushort?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ushort?, Func<ushort?, ushort?>>>(
					                    Expression.Lambda<Func<ushort?, ushort?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(ushort?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?, ushort?> f6 = e6.Compile()();
					
					        ushort? f6Result = default(ushort?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        ushort? csResult = default(ushort?);
					        Exception csEx = null;
					        try {
					            csResult = (ushort?) (val0 - val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_shortq_Subtract() {
					        short?[] svals = new short?[] { 0, 1, -1, short.MinValue, short.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_shortq_Subtract(svals[i], svals[j])) {
					                    Console.WriteLine("shortq_Subtract failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_shortq_Subtract(short? val0, short? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(short?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(short?), "p1");
					        Expression<Func<short?>> e1 = Expression.Lambda<Func<short?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<short?, short?, short?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(short?)),
					                    Expression.Constant(val1, typeof(short?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?> f1 = e1.Compile();
					
					        short? f1Result = default(short?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<short?, short?, Func<short?>>> e2 = Expression.Lambda<Func<short?, short?, Func<short?>>>(
					            Expression.Lambda<Func<short?>>(Expression.Subtract(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<short?, short?, Func<short?>> f2 = e2.Compile();
					
					        short? f2Result = default(short?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?, short?>>> e3 = Expression.Lambda<Func<Func<short?, short?, short?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<short?, short?, short?>>>(
					                    Expression.Lambda<Func<short?, short?, short?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?, short?, short?> f3 = e3.Compile()();
					
					        short? f3Result = default(short?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?, short?>>> e4 = Expression.Lambda<Func<Func<short?, short?, short?>>>(
					            Expression.Lambda<Func<short?, short?, short?>>(
					                Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<short?, short?, short?>> f4 = e4.Compile();
					
					        short? f4Result = default(short?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<short?, Func<short?, short?>>> e5 =
					            Expression.Lambda<Func<short?, Func<short?, short?>>>(
					                Expression.Lambda<Func<short?, short?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<short?, Func<short?, short?>> f5 = e5.Compile();
					
					        short? f5Result = default(short?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?>>> e6 = Expression.Lambda<Func<Func<short?, short?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<short?, Func<short?, short?>>>(
					                    Expression.Lambda<Func<short?, short?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(short?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?, short?> f6 = e6.Compile()();
					
					        short? f6Result = default(short?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        short? csResult = default(short?);
					        Exception csEx = null;
					        try {
					            csResult = (short?) (val0 - val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_uintq_Subtract() {
					        uint?[] svals = new uint?[] { 0, 1, uint.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_uintq_Subtract(svals[i], svals[j])) {
					                    Console.WriteLine("uintq_Subtract failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_uintq_Subtract(uint? val0, uint? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(uint?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(uint?), "p1");
					        Expression<Func<uint?>> e1 = Expression.Lambda<Func<uint?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<uint?, uint?, uint?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(uint?)),
					                    Expression.Constant(val1, typeof(uint?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?> f1 = e1.Compile();
					
					        uint? f1Result = default(uint?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<uint?, uint?, Func<uint?>>> e2 = Expression.Lambda<Func<uint?, uint?, Func<uint?>>>(
					            Expression.Lambda<Func<uint?>>(Expression.Subtract(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<uint?, uint?, Func<uint?>> f2 = e2.Compile();
					
					        uint? f2Result = default(uint?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?, uint?>>> e3 = Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					                    Expression.Lambda<Func<uint?, uint?, uint?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?, uint?, uint?> f3 = e3.Compile()();
					
					        uint? f3Result = default(uint?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?, uint?>>> e4 = Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					            Expression.Lambda<Func<uint?, uint?, uint?>>(
					                Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<uint?, uint?, uint?>> f4 = e4.Compile();
					
					        uint? f4Result = default(uint?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<uint?, Func<uint?, uint?>>> e5 =
					            Expression.Lambda<Func<uint?, Func<uint?, uint?>>>(
					                Expression.Lambda<Func<uint?, uint?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<uint?, Func<uint?, uint?>> f5 = e5.Compile();
					
					        uint? f5Result = default(uint?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?>>> e6 = Expression.Lambda<Func<Func<uint?, uint?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<uint?, Func<uint?, uint?>>>(
					                    Expression.Lambda<Func<uint?, uint?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(uint?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?, uint?> f6 = e6.Compile()();
					
					        uint? f6Result = default(uint?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        uint? csResult = default(uint?);
					        Exception csEx = null;
					        try {
					            csResult = (uint?) (val0 - val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_intq_Subtract() {
					        int?[] svals = new int?[] { 0, 1, -1, int.MinValue, int.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_intq_Subtract(svals[i], svals[j])) {
					                    Console.WriteLine("intq_Subtract failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_intq_Subtract(int? val0, int? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(int?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(int?), "p1");
					        Expression<Func<int?>> e1 = Expression.Lambda<Func<int?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<int?, int?, int?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(int?)),
					                    Expression.Constant(val1, typeof(int?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?> f1 = e1.Compile();
					
					        int? f1Result = default(int?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<int?, int?, Func<int?>>> e2 = Expression.Lambda<Func<int?, int?, Func<int?>>>(
					            Expression.Lambda<Func<int?>>(Expression.Subtract(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<int?, int?, Func<int?>> f2 = e2.Compile();
					
					        int? f2Result = default(int?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?, int?>>> e3 = Expression.Lambda<Func<Func<int?, int?, int?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<int?, int?, int?>>>(
					                    Expression.Lambda<Func<int?, int?, int?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?, int?, int?> f3 = e3.Compile()();
					
					        int? f3Result = default(int?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?, int?>>> e4 = Expression.Lambda<Func<Func<int?, int?, int?>>>(
					            Expression.Lambda<Func<int?, int?, int?>>(
					                Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<int?, int?, int?>> f4 = e4.Compile();
					
					        int? f4Result = default(int?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<int?, Func<int?, int?>>> e5 =
					            Expression.Lambda<Func<int?, Func<int?, int?>>>(
					                Expression.Lambda<Func<int?, int?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<int?, Func<int?, int?>> f5 = e5.Compile();
					
					        int? f5Result = default(int?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?>>> e6 = Expression.Lambda<Func<Func<int?, int?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<int?, Func<int?, int?>>>(
					                    Expression.Lambda<Func<int?, int?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(int?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?, int?> f6 = e6.Compile()();
					
					        int? f6Result = default(int?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        int? csResult = default(int?);
					        Exception csEx = null;
					        try {
					            csResult = (int?) (val0 - val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_ulongq_Subtract() {
					        ulong?[] svals = new ulong?[] { 0, 1, ulong.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_ulongq_Subtract(svals[i], svals[j])) {
					                    Console.WriteLine("ulongq_Subtract failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulongq_Subtract(ulong? val0, ulong? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(ulong?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(ulong?), "p1");
					        Expression<Func<ulong?>> e1 = Expression.Lambda<Func<ulong?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(ulong?)),
					                    Expression.Constant(val1, typeof(ulong?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?> f1 = e1.Compile();
					
					        ulong? f1Result = default(ulong?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<ulong?, ulong?, Func<ulong?>>> e2 = Expression.Lambda<Func<ulong?, ulong?, Func<ulong?>>>(
					            Expression.Lambda<Func<ulong?>>(Expression.Subtract(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<ulong?, ulong?, Func<ulong?>> f2 = e2.Compile();
					
					        ulong? f2Result = default(ulong?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?, ulong?>>> e3 = Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					                    Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?, ulong?, ulong?> f3 = e3.Compile()();
					
					        ulong? f3Result = default(ulong?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?, ulong?>>> e4 = Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					            Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<ulong?, ulong?, ulong?>> f4 = e4.Compile();
					
					        ulong? f4Result = default(ulong?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<ulong?, Func<ulong?, ulong?>>> e5 =
					            Expression.Lambda<Func<ulong?, Func<ulong?, ulong?>>>(
					                Expression.Lambda<Func<ulong?, ulong?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<ulong?, Func<ulong?, ulong?>> f5 = e5.Compile();
					
					        ulong? f5Result = default(ulong?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?>>> e6 = Expression.Lambda<Func<Func<ulong?, ulong?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ulong?, Func<ulong?, ulong?>>>(
					                    Expression.Lambda<Func<ulong?, ulong?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(ulong?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?, ulong?> f6 = e6.Compile()();
					
					        ulong? f6Result = default(ulong?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        ulong? csResult = default(ulong?);
					        Exception csEx = null;
					        try {
					            csResult = (ulong?) (val0 - val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_longq_Subtract() {
					        long?[] svals = new long?[] { 0, 1, -1, long.MinValue, long.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_longq_Subtract(svals[i], svals[j])) {
					                    Console.WriteLine("longq_Subtract failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_longq_Subtract(long? val0, long? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(long?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(long?), "p1");
					        Expression<Func<long?>> e1 = Expression.Lambda<Func<long?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<long?, long?, long?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(long?)),
					                    Expression.Constant(val1, typeof(long?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?> f1 = e1.Compile();
					
					        long? f1Result = default(long?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<long?, long?, Func<long?>>> e2 = Expression.Lambda<Func<long?, long?, Func<long?>>>(
					            Expression.Lambda<Func<long?>>(Expression.Subtract(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<long?, long?, Func<long?>> f2 = e2.Compile();
					
					        long? f2Result = default(long?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?, long?>>> e3 = Expression.Lambda<Func<Func<long?, long?, long?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<long?, long?, long?>>>(
					                    Expression.Lambda<Func<long?, long?, long?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?, long?, long?> f3 = e3.Compile()();
					
					        long? f3Result = default(long?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?, long?>>> e4 = Expression.Lambda<Func<Func<long?, long?, long?>>>(
					            Expression.Lambda<Func<long?, long?, long?>>(
					                Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<long?, long?, long?>> f4 = e4.Compile();
					
					        long? f4Result = default(long?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<long?, Func<long?, long?>>> e5 =
					            Expression.Lambda<Func<long?, Func<long?, long?>>>(
					                Expression.Lambda<Func<long?, long?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<long?, Func<long?, long?>> f5 = e5.Compile();
					
					        long? f5Result = default(long?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?>>> e6 = Expression.Lambda<Func<Func<long?, long?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<long?, Func<long?, long?>>>(
					                    Expression.Lambda<Func<long?, long?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(long?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?, long?> f6 = e6.Compile()();
					
					        long? f6Result = default(long?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        long? csResult = default(long?);
					        Exception csEx = null;
					        try {
					            csResult = (long?) (val0 - val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_floatq_Subtract() {
					        float?[] svals = new float?[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_floatq_Subtract(svals[i], svals[j])) {
					                    Console.WriteLine("floatq_Subtract failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_floatq_Subtract(float? val0, float? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(float?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(float?), "p1");
					        Expression<Func<float?>> e1 = Expression.Lambda<Func<float?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<float?, float?, float?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(float?)),
					                    Expression.Constant(val1, typeof(float?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?> f1 = e1.Compile();
					
					        float? f1Result = default(float?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<float?, float?, Func<float?>>> e2 = Expression.Lambda<Func<float?, float?, Func<float?>>>(
					            Expression.Lambda<Func<float?>>(Expression.Subtract(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<float?, float?, Func<float?>> f2 = e2.Compile();
					
					        float? f2Result = default(float?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?, float?>>> e3 = Expression.Lambda<Func<Func<float?, float?, float?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<float?, float?, float?>>>(
					                    Expression.Lambda<Func<float?, float?, float?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?, float?, float?> f3 = e3.Compile()();
					
					        float? f3Result = default(float?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?, float?>>> e4 = Expression.Lambda<Func<Func<float?, float?, float?>>>(
					            Expression.Lambda<Func<float?, float?, float?>>(
					                Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<float?, float?, float?>> f4 = e4.Compile();
					
					        float? f4Result = default(float?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<float?, Func<float?, float?>>> e5 =
					            Expression.Lambda<Func<float?, Func<float?, float?>>>(
					                Expression.Lambda<Func<float?, float?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<float?, Func<float?, float?>> f5 = e5.Compile();
					
					        float? f5Result = default(float?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?>>> e6 = Expression.Lambda<Func<Func<float?, float?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<float?, Func<float?, float?>>>(
					                    Expression.Lambda<Func<float?, float?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(float?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?, float?> f6 = e6.Compile()();
					
					        float? f6Result = default(float?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        float? csResult = default(float?);
					        Exception csEx = null;
					        try {
					            csResult = (float?) (val0 - val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_doubleq_Subtract() {
					        double?[] svals = new double?[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_doubleq_Subtract(svals[i], svals[j])) {
					                    Console.WriteLine("doubleq_Subtract failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_doubleq_Subtract(double? val0, double? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(double?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(double?), "p1");
					        Expression<Func<double?>> e1 = Expression.Lambda<Func<double?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<double?, double?, double?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(double?)),
					                    Expression.Constant(val1, typeof(double?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?> f1 = e1.Compile();
					
					        double? f1Result = default(double?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<double?, double?, Func<double?>>> e2 = Expression.Lambda<Func<double?, double?, Func<double?>>>(
					            Expression.Lambda<Func<double?>>(Expression.Subtract(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<double?, double?, Func<double?>> f2 = e2.Compile();
					
					        double? f2Result = default(double?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?, double?>>> e3 = Expression.Lambda<Func<Func<double?, double?, double?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<double?, double?, double?>>>(
					                    Expression.Lambda<Func<double?, double?, double?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?, double?, double?> f3 = e3.Compile()();
					
					        double? f3Result = default(double?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?, double?>>> e4 = Expression.Lambda<Func<Func<double?, double?, double?>>>(
					            Expression.Lambda<Func<double?, double?, double?>>(
					                Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<double?, double?, double?>> f4 = e4.Compile();
					
					        double? f4Result = default(double?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<double?, Func<double?, double?>>> e5 =
					            Expression.Lambda<Func<double?, Func<double?, double?>>>(
					                Expression.Lambda<Func<double?, double?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<double?, Func<double?, double?>> f5 = e5.Compile();
					
					        double? f5Result = default(double?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?>>> e6 = Expression.Lambda<Func<Func<double?, double?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<double?, Func<double?, double?>>>(
					                    Expression.Lambda<Func<double?, double?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(double?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?, double?> f6 = e6.Compile()();
					
					        double? f6Result = default(double?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        double? csResult = default(double?);
					        Exception csEx = null;
					        try {
					            csResult = (double?) (val0 - val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_decimalq_Subtract() {
					        decimal?[] svals = new decimal?[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_decimalq_Subtract(svals[i], svals[j])) {
					                    Console.WriteLine("decimalq_Subtract failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_decimalq_Subtract(decimal? val0, decimal? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(decimal?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(decimal?), "p1");
					        Expression<Func<decimal?>> e1 = Expression.Lambda<Func<decimal?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(decimal?)),
					                    Expression.Constant(val1, typeof(decimal?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?> f1 = e1.Compile();
					
					        decimal? f1Result = default(decimal?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<decimal?, decimal?, Func<decimal?>>> e2 = Expression.Lambda<Func<decimal?, decimal?, Func<decimal?>>>(
					            Expression.Lambda<Func<decimal?>>(Expression.Subtract(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<decimal?, decimal?, Func<decimal?>> f2 = e2.Compile();
					
					        decimal? f2Result = default(decimal?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?, decimal?>>> e3 = Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					                    Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?, decimal?, decimal?> f3 = e3.Compile()();
					
					        decimal? f3Result = default(decimal?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?, decimal?>>> e4 = Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					            Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<decimal?, decimal?, decimal?>> f4 = e4.Compile();
					
					        decimal? f4Result = default(decimal?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<decimal?, Func<decimal?, decimal?>>> e5 =
					            Expression.Lambda<Func<decimal?, Func<decimal?, decimal?>>>(
					                Expression.Lambda<Func<decimal?, decimal?>>(
					                    Expression.Subtract(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<decimal?, Func<decimal?, decimal?>> f5 = e5.Compile();
					
					        decimal? f5Result = default(decimal?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?>>> e6 = Expression.Lambda<Func<Func<decimal?, decimal?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<decimal?, Func<decimal?, decimal?>>>(
					                    Expression.Lambda<Func<decimal?, decimal?>>(
					                        Expression.Subtract(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(decimal?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?, decimal?> f6 = e6.Compile()();
					
					        decimal? f6Result = default(decimal?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        decimal? csResult = default(decimal?);
					        Exception csEx = null;
					        try {
					            csResult = (decimal?) (val0 - val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static Type GetNonNullableType(Type type) {
				        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
				                type.GetGenericArguments()[0] :
				                type;
				    }
				}
				
			
			
			public  class Helper {
			    public static bool verifyException<T>(T fResult, Exception fEx, T csResult, Exception csEx) {
			        if (fEx != null || csEx != null) {
			            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
			        }
			        else {
			            return object.Equals(fResult, csResult);
			        }
			    }
			}
		
	}
				
				//-------- Scenario 383
				namespace Scenario383{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Multiply__2() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ushortq_Multiply() &
					                check_shortq_Multiply() &
					                check_uintq_Multiply() &
					                check_intq_Multiply() &
					                check_ulongq_Multiply() &
					                check_longq_Multiply() &
					                check_floatq_Multiply() &
					                check_doubleq_Multiply() &
					                check_decimalq_Multiply();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushortq_Multiply() {
					        ushort?[] svals = new ushort?[] { 0, 1, ushort.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_ushortq_Multiply(svals[i], svals[j])) {
					                    Console.WriteLine("ushortq_Multiply failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushortq_Multiply(ushort? val0, ushort? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(ushort?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(ushort?), "p1");
					        Expression<Func<ushort?>> e1 = Expression.Lambda<Func<ushort?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(ushort?)),
					                    Expression.Constant(val1, typeof(ushort?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?> f1 = e1.Compile();
					
					        ushort? f1Result = default(ushort?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<ushort?, ushort?, Func<ushort?>>> e2 = Expression.Lambda<Func<ushort?, ushort?, Func<ushort?>>>(
					            Expression.Lambda<Func<ushort?>>(Expression.Multiply(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<ushort?, ushort?, Func<ushort?>> f2 = e2.Compile();
					
					        ushort? f2Result = default(ushort?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?, ushort?>>> e3 = Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					                    Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?, ushort?, ushort?> f3 = e3.Compile()();
					
					        ushort? f3Result = default(ushort?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?, ushort?>>> e4 = Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					            Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<ushort?, ushort?, ushort?>> f4 = e4.Compile();
					
					        ushort? f4Result = default(ushort?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<ushort?, Func<ushort?, ushort?>>> e5 =
					            Expression.Lambda<Func<ushort?, Func<ushort?, ushort?>>>(
					                Expression.Lambda<Func<ushort?, ushort?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<ushort?, Func<ushort?, ushort?>> f5 = e5.Compile();
					
					        ushort? f5Result = default(ushort?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?>>> e6 = Expression.Lambda<Func<Func<ushort?, ushort?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ushort?, Func<ushort?, ushort?>>>(
					                    Expression.Lambda<Func<ushort?, ushort?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(ushort?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?, ushort?> f6 = e6.Compile()();
					
					        ushort? f6Result = default(ushort?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        ushort? csResult = default(ushort?);
					        Exception csEx = null;
					        try {
					            csResult = (ushort?) (val0 * val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_shortq_Multiply() {
					        short?[] svals = new short?[] { 0, 1, -1, short.MinValue, short.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_shortq_Multiply(svals[i], svals[j])) {
					                    Console.WriteLine("shortq_Multiply failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_shortq_Multiply(short? val0, short? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(short?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(short?), "p1");
					        Expression<Func<short?>> e1 = Expression.Lambda<Func<short?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<short?, short?, short?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(short?)),
					                    Expression.Constant(val1, typeof(short?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?> f1 = e1.Compile();
					
					        short? f1Result = default(short?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<short?, short?, Func<short?>>> e2 = Expression.Lambda<Func<short?, short?, Func<short?>>>(
					            Expression.Lambda<Func<short?>>(Expression.Multiply(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<short?, short?, Func<short?>> f2 = e2.Compile();
					
					        short? f2Result = default(short?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?, short?>>> e3 = Expression.Lambda<Func<Func<short?, short?, short?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<short?, short?, short?>>>(
					                    Expression.Lambda<Func<short?, short?, short?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?, short?, short?> f3 = e3.Compile()();
					
					        short? f3Result = default(short?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?, short?>>> e4 = Expression.Lambda<Func<Func<short?, short?, short?>>>(
					            Expression.Lambda<Func<short?, short?, short?>>(
					                Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<short?, short?, short?>> f4 = e4.Compile();
					
					        short? f4Result = default(short?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<short?, Func<short?, short?>>> e5 =
					            Expression.Lambda<Func<short?, Func<short?, short?>>>(
					                Expression.Lambda<Func<short?, short?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<short?, Func<short?, short?>> f5 = e5.Compile();
					
					        short? f5Result = default(short?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?>>> e6 = Expression.Lambda<Func<Func<short?, short?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<short?, Func<short?, short?>>>(
					                    Expression.Lambda<Func<short?, short?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(short?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?, short?> f6 = e6.Compile()();
					
					        short? f6Result = default(short?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        short? csResult = default(short?);
					        Exception csEx = null;
					        try {
					            csResult = (short?) (val0 * val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_uintq_Multiply() {
					        uint?[] svals = new uint?[] { 0, 1, uint.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_uintq_Multiply(svals[i], svals[j])) {
					                    Console.WriteLine("uintq_Multiply failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_uintq_Multiply(uint? val0, uint? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(uint?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(uint?), "p1");
					        Expression<Func<uint?>> e1 = Expression.Lambda<Func<uint?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<uint?, uint?, uint?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(uint?)),
					                    Expression.Constant(val1, typeof(uint?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?> f1 = e1.Compile();
					
					        uint? f1Result = default(uint?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<uint?, uint?, Func<uint?>>> e2 = Expression.Lambda<Func<uint?, uint?, Func<uint?>>>(
					            Expression.Lambda<Func<uint?>>(Expression.Multiply(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<uint?, uint?, Func<uint?>> f2 = e2.Compile();
					
					        uint? f2Result = default(uint?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?, uint?>>> e3 = Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					                    Expression.Lambda<Func<uint?, uint?, uint?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?, uint?, uint?> f3 = e3.Compile()();
					
					        uint? f3Result = default(uint?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?, uint?>>> e4 = Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					            Expression.Lambda<Func<uint?, uint?, uint?>>(
					                Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<uint?, uint?, uint?>> f4 = e4.Compile();
					
					        uint? f4Result = default(uint?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<uint?, Func<uint?, uint?>>> e5 =
					            Expression.Lambda<Func<uint?, Func<uint?, uint?>>>(
					                Expression.Lambda<Func<uint?, uint?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<uint?, Func<uint?, uint?>> f5 = e5.Compile();
					
					        uint? f5Result = default(uint?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?>>> e6 = Expression.Lambda<Func<Func<uint?, uint?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<uint?, Func<uint?, uint?>>>(
					                    Expression.Lambda<Func<uint?, uint?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(uint?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?, uint?> f6 = e6.Compile()();
					
					        uint? f6Result = default(uint?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        uint? csResult = default(uint?);
					        Exception csEx = null;
					        try {
					            csResult = (uint?) (val0 * val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_intq_Multiply() {
					        int?[] svals = new int?[] { 0, 1, -1, int.MinValue, int.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_intq_Multiply(svals[i], svals[j])) {
					                    Console.WriteLine("intq_Multiply failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_intq_Multiply(int? val0, int? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(int?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(int?), "p1");
					        Expression<Func<int?>> e1 = Expression.Lambda<Func<int?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<int?, int?, int?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(int?)),
					                    Expression.Constant(val1, typeof(int?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?> f1 = e1.Compile();
					
					        int? f1Result = default(int?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<int?, int?, Func<int?>>> e2 = Expression.Lambda<Func<int?, int?, Func<int?>>>(
					            Expression.Lambda<Func<int?>>(Expression.Multiply(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<int?, int?, Func<int?>> f2 = e2.Compile();
					
					        int? f2Result = default(int?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?, int?>>> e3 = Expression.Lambda<Func<Func<int?, int?, int?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<int?, int?, int?>>>(
					                    Expression.Lambda<Func<int?, int?, int?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?, int?, int?> f3 = e3.Compile()();
					
					        int? f3Result = default(int?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?, int?>>> e4 = Expression.Lambda<Func<Func<int?, int?, int?>>>(
					            Expression.Lambda<Func<int?, int?, int?>>(
					                Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<int?, int?, int?>> f4 = e4.Compile();
					
					        int? f4Result = default(int?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<int?, Func<int?, int?>>> e5 =
					            Expression.Lambda<Func<int?, Func<int?, int?>>>(
					                Expression.Lambda<Func<int?, int?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<int?, Func<int?, int?>> f5 = e5.Compile();
					
					        int? f5Result = default(int?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?>>> e6 = Expression.Lambda<Func<Func<int?, int?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<int?, Func<int?, int?>>>(
					                    Expression.Lambda<Func<int?, int?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(int?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?, int?> f6 = e6.Compile()();
					
					        int? f6Result = default(int?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        int? csResult = default(int?);
					        Exception csEx = null;
					        try {
					            csResult = (int?) (val0 * val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_ulongq_Multiply() {
					        ulong?[] svals = new ulong?[] { 0, 1, ulong.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_ulongq_Multiply(svals[i], svals[j])) {
					                    Console.WriteLine("ulongq_Multiply failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulongq_Multiply(ulong? val0, ulong? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(ulong?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(ulong?), "p1");
					        Expression<Func<ulong?>> e1 = Expression.Lambda<Func<ulong?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(ulong?)),
					                    Expression.Constant(val1, typeof(ulong?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?> f1 = e1.Compile();
					
					        ulong? f1Result = default(ulong?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<ulong?, ulong?, Func<ulong?>>> e2 = Expression.Lambda<Func<ulong?, ulong?, Func<ulong?>>>(
					            Expression.Lambda<Func<ulong?>>(Expression.Multiply(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<ulong?, ulong?, Func<ulong?>> f2 = e2.Compile();
					
					        ulong? f2Result = default(ulong?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?, ulong?>>> e3 = Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					                    Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?, ulong?, ulong?> f3 = e3.Compile()();
					
					        ulong? f3Result = default(ulong?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?, ulong?>>> e4 = Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					            Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<ulong?, ulong?, ulong?>> f4 = e4.Compile();
					
					        ulong? f4Result = default(ulong?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<ulong?, Func<ulong?, ulong?>>> e5 =
					            Expression.Lambda<Func<ulong?, Func<ulong?, ulong?>>>(
					                Expression.Lambda<Func<ulong?, ulong?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<ulong?, Func<ulong?, ulong?>> f5 = e5.Compile();
					
					        ulong? f5Result = default(ulong?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?>>> e6 = Expression.Lambda<Func<Func<ulong?, ulong?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ulong?, Func<ulong?, ulong?>>>(
					                    Expression.Lambda<Func<ulong?, ulong?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(ulong?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?, ulong?> f6 = e6.Compile()();
					
					        ulong? f6Result = default(ulong?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        ulong? csResult = default(ulong?);
					        Exception csEx = null;
					        try {
					            csResult = (ulong?) (val0 * val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_longq_Multiply() {
					        long?[] svals = new long?[] { 0, 1, -1, long.MinValue, long.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_longq_Multiply(svals[i], svals[j])) {
					                    Console.WriteLine("longq_Multiply failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_longq_Multiply(long? val0, long? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(long?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(long?), "p1");
					        Expression<Func<long?>> e1 = Expression.Lambda<Func<long?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<long?, long?, long?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(long?)),
					                    Expression.Constant(val1, typeof(long?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?> f1 = e1.Compile();
					
					        long? f1Result = default(long?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<long?, long?, Func<long?>>> e2 = Expression.Lambda<Func<long?, long?, Func<long?>>>(
					            Expression.Lambda<Func<long?>>(Expression.Multiply(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<long?, long?, Func<long?>> f2 = e2.Compile();
					
					        long? f2Result = default(long?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?, long?>>> e3 = Expression.Lambda<Func<Func<long?, long?, long?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<long?, long?, long?>>>(
					                    Expression.Lambda<Func<long?, long?, long?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?, long?, long?> f3 = e3.Compile()();
					
					        long? f3Result = default(long?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?, long?>>> e4 = Expression.Lambda<Func<Func<long?, long?, long?>>>(
					            Expression.Lambda<Func<long?, long?, long?>>(
					                Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<long?, long?, long?>> f4 = e4.Compile();
					
					        long? f4Result = default(long?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<long?, Func<long?, long?>>> e5 =
					            Expression.Lambda<Func<long?, Func<long?, long?>>>(
					                Expression.Lambda<Func<long?, long?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<long?, Func<long?, long?>> f5 = e5.Compile();
					
					        long? f5Result = default(long?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?>>> e6 = Expression.Lambda<Func<Func<long?, long?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<long?, Func<long?, long?>>>(
					                    Expression.Lambda<Func<long?, long?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(long?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?, long?> f6 = e6.Compile()();
					
					        long? f6Result = default(long?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        long? csResult = default(long?);
					        Exception csEx = null;
					        try {
					            csResult = (long?) (val0 * val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_floatq_Multiply() {
					        float?[] svals = new float?[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_floatq_Multiply(svals[i], svals[j])) {
					                    Console.WriteLine("floatq_Multiply failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_floatq_Multiply(float? val0, float? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(float?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(float?), "p1");
					        Expression<Func<float?>> e1 = Expression.Lambda<Func<float?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<float?, float?, float?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(float?)),
					                    Expression.Constant(val1, typeof(float?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?> f1 = e1.Compile();
					
					        float? f1Result = default(float?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<float?, float?, Func<float?>>> e2 = Expression.Lambda<Func<float?, float?, Func<float?>>>(
					            Expression.Lambda<Func<float?>>(Expression.Multiply(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<float?, float?, Func<float?>> f2 = e2.Compile();
					
					        float? f2Result = default(float?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?, float?>>> e3 = Expression.Lambda<Func<Func<float?, float?, float?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<float?, float?, float?>>>(
					                    Expression.Lambda<Func<float?, float?, float?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?, float?, float?> f3 = e3.Compile()();
					
					        float? f3Result = default(float?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?, float?>>> e4 = Expression.Lambda<Func<Func<float?, float?, float?>>>(
					            Expression.Lambda<Func<float?, float?, float?>>(
					                Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<float?, float?, float?>> f4 = e4.Compile();
					
					        float? f4Result = default(float?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<float?, Func<float?, float?>>> e5 =
					            Expression.Lambda<Func<float?, Func<float?, float?>>>(
					                Expression.Lambda<Func<float?, float?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<float?, Func<float?, float?>> f5 = e5.Compile();
					
					        float? f5Result = default(float?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?>>> e6 = Expression.Lambda<Func<Func<float?, float?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<float?, Func<float?, float?>>>(
					                    Expression.Lambda<Func<float?, float?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(float?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?, float?> f6 = e6.Compile()();
					
					        float? f6Result = default(float?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        float? csResult = default(float?);
					        Exception csEx = null;
					        try {
					            csResult = (float?) (val0 * val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_doubleq_Multiply() {
					        double?[] svals = new double?[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_doubleq_Multiply(svals[i], svals[j])) {
					                    Console.WriteLine("doubleq_Multiply failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_doubleq_Multiply(double? val0, double? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(double?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(double?), "p1");
					        Expression<Func<double?>> e1 = Expression.Lambda<Func<double?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<double?, double?, double?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(double?)),
					                    Expression.Constant(val1, typeof(double?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?> f1 = e1.Compile();
					
					        double? f1Result = default(double?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<double?, double?, Func<double?>>> e2 = Expression.Lambda<Func<double?, double?, Func<double?>>>(
					            Expression.Lambda<Func<double?>>(Expression.Multiply(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<double?, double?, Func<double?>> f2 = e2.Compile();
					
					        double? f2Result = default(double?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?, double?>>> e3 = Expression.Lambda<Func<Func<double?, double?, double?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<double?, double?, double?>>>(
					                    Expression.Lambda<Func<double?, double?, double?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?, double?, double?> f3 = e3.Compile()();
					
					        double? f3Result = default(double?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?, double?>>> e4 = Expression.Lambda<Func<Func<double?, double?, double?>>>(
					            Expression.Lambda<Func<double?, double?, double?>>(
					                Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<double?, double?, double?>> f4 = e4.Compile();
					
					        double? f4Result = default(double?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<double?, Func<double?, double?>>> e5 =
					            Expression.Lambda<Func<double?, Func<double?, double?>>>(
					                Expression.Lambda<Func<double?, double?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<double?, Func<double?, double?>> f5 = e5.Compile();
					
					        double? f5Result = default(double?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?>>> e6 = Expression.Lambda<Func<Func<double?, double?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<double?, Func<double?, double?>>>(
					                    Expression.Lambda<Func<double?, double?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(double?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?, double?> f6 = e6.Compile()();
					
					        double? f6Result = default(double?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        double? csResult = default(double?);
					        Exception csEx = null;
					        try {
					            csResult = (double?) (val0 * val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_decimalq_Multiply() {
					        decimal?[] svals = new decimal?[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_decimalq_Multiply(svals[i], svals[j])) {
					                    Console.WriteLine("decimalq_Multiply failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_decimalq_Multiply(decimal? val0, decimal? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(decimal?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(decimal?), "p1");
					        Expression<Func<decimal?>> e1 = Expression.Lambda<Func<decimal?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(decimal?)),
					                    Expression.Constant(val1, typeof(decimal?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?> f1 = e1.Compile();
					
					        decimal? f1Result = default(decimal?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<decimal?, decimal?, Func<decimal?>>> e2 = Expression.Lambda<Func<decimal?, decimal?, Func<decimal?>>>(
					            Expression.Lambda<Func<decimal?>>(Expression.Multiply(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<decimal?, decimal?, Func<decimal?>> f2 = e2.Compile();
					
					        decimal? f2Result = default(decimal?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?, decimal?>>> e3 = Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					                    Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?, decimal?, decimal?> f3 = e3.Compile()();
					
					        decimal? f3Result = default(decimal?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?, decimal?>>> e4 = Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					            Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<decimal?, decimal?, decimal?>> f4 = e4.Compile();
					
					        decimal? f4Result = default(decimal?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<decimal?, Func<decimal?, decimal?>>> e5 =
					            Expression.Lambda<Func<decimal?, Func<decimal?, decimal?>>>(
					                Expression.Lambda<Func<decimal?, decimal?>>(
					                    Expression.Multiply(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<decimal?, Func<decimal?, decimal?>> f5 = e5.Compile();
					
					        decimal? f5Result = default(decimal?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?>>> e6 = Expression.Lambda<Func<Func<decimal?, decimal?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<decimal?, Func<decimal?, decimal?>>>(
					                    Expression.Lambda<Func<decimal?, decimal?>>(
					                        Expression.Multiply(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(decimal?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?, decimal?> f6 = e6.Compile()();
					
					        decimal? f6Result = default(decimal?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        decimal? csResult = default(decimal?);
					        Exception csEx = null;
					        try {
					            csResult = (decimal?) (val0 * val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static Type GetNonNullableType(Type type) {
				        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
				                type.GetGenericArguments()[0] :
				                type;
				    }
				}
				
			
			
			public  class Helper {
			    public static bool verifyException<T>(T fResult, Exception fEx, T csResult, Exception csEx) {
			        if (fEx != null || csEx != null) {
			            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
			        }
			        else {
			            return object.Equals(fResult, csResult);
			        }
			    }
			}
		
	}
				
				//-------- Scenario 384
				namespace Scenario384{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Divide__2() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ushortq_Divide() &
					                check_shortq_Divide() &
					                check_uintq_Divide() &
					                check_intq_Divide() &
					                check_ulongq_Divide() &
					                check_longq_Divide() &
					                check_floatq_Divide() &
					                check_doubleq_Divide() &
					                check_decimalq_Divide();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushortq_Divide() {
					        ushort?[] svals = new ushort?[] { 0, 1, ushort.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_ushortq_Divide(svals[i], svals[j])) {
					                    Console.WriteLine("ushortq_Divide failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushortq_Divide(ushort? val0, ushort? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(ushort?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(ushort?), "p1");
					        Expression<Func<ushort?>> e1 = Expression.Lambda<Func<ushort?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(ushort?)),
					                    Expression.Constant(val1, typeof(ushort?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?> f1 = e1.Compile();
					
					        ushort? f1Result = default(ushort?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<ushort?, ushort?, Func<ushort?>>> e2 = Expression.Lambda<Func<ushort?, ushort?, Func<ushort?>>>(
					            Expression.Lambda<Func<ushort?>>(Expression.Divide(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<ushort?, ushort?, Func<ushort?>> f2 = e2.Compile();
					
					        ushort? f2Result = default(ushort?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?, ushort?>>> e3 = Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					                    Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?, ushort?, ushort?> f3 = e3.Compile()();
					
					        ushort? f3Result = default(ushort?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?, ushort?>>> e4 = Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					            Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<ushort?, ushort?, ushort?>> f4 = e4.Compile();
					
					        ushort? f4Result = default(ushort?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<ushort?, Func<ushort?, ushort?>>> e5 =
					            Expression.Lambda<Func<ushort?, Func<ushort?, ushort?>>>(
					                Expression.Lambda<Func<ushort?, ushort?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<ushort?, Func<ushort?, ushort?>> f5 = e5.Compile();
					
					        ushort? f5Result = default(ushort?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?>>> e6 = Expression.Lambda<Func<Func<ushort?, ushort?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ushort?, Func<ushort?, ushort?>>>(
					                    Expression.Lambda<Func<ushort?, ushort?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(ushort?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?, ushort?> f6 = e6.Compile()();
					
					        ushort? f6Result = default(ushort?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        ushort? csResult = default(ushort?);
					        Exception csEx = null;
					        try {
					            csResult = (ushort?) (val0 / val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_shortq_Divide() {
					        short?[] svals = new short?[] { 0, 1, -1, short.MinValue, short.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_shortq_Divide(svals[i], svals[j])) {
					                    Console.WriteLine("shortq_Divide failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_shortq_Divide(short? val0, short? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(short?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(short?), "p1");
					        Expression<Func<short?>> e1 = Expression.Lambda<Func<short?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<short?, short?, short?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(short?)),
					                    Expression.Constant(val1, typeof(short?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?> f1 = e1.Compile();
					
					        short? f1Result = default(short?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<short?, short?, Func<short?>>> e2 = Expression.Lambda<Func<short?, short?, Func<short?>>>(
					            Expression.Lambda<Func<short?>>(Expression.Divide(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<short?, short?, Func<short?>> f2 = e2.Compile();
					
					        short? f2Result = default(short?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?, short?>>> e3 = Expression.Lambda<Func<Func<short?, short?, short?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<short?, short?, short?>>>(
					                    Expression.Lambda<Func<short?, short?, short?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?, short?, short?> f3 = e3.Compile()();
					
					        short? f3Result = default(short?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?, short?>>> e4 = Expression.Lambda<Func<Func<short?, short?, short?>>>(
					            Expression.Lambda<Func<short?, short?, short?>>(
					                Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<short?, short?, short?>> f4 = e4.Compile();
					
					        short? f4Result = default(short?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<short?, Func<short?, short?>>> e5 =
					            Expression.Lambda<Func<short?, Func<short?, short?>>>(
					                Expression.Lambda<Func<short?, short?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<short?, Func<short?, short?>> f5 = e5.Compile();
					
					        short? f5Result = default(short?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?>>> e6 = Expression.Lambda<Func<Func<short?, short?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<short?, Func<short?, short?>>>(
					                    Expression.Lambda<Func<short?, short?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(short?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?, short?> f6 = e6.Compile()();
					
					        short? f6Result = default(short?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        short? csResult = default(short?);
					        Exception csEx = null;
					        try {
					            csResult = (short?) (val0 / val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_uintq_Divide() {
					        uint?[] svals = new uint?[] { 0, 1, uint.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_uintq_Divide(svals[i], svals[j])) {
					                    Console.WriteLine("uintq_Divide failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_uintq_Divide(uint? val0, uint? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(uint?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(uint?), "p1");
					        Expression<Func<uint?>> e1 = Expression.Lambda<Func<uint?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<uint?, uint?, uint?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(uint?)),
					                    Expression.Constant(val1, typeof(uint?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?> f1 = e1.Compile();
					
					        uint? f1Result = default(uint?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<uint?, uint?, Func<uint?>>> e2 = Expression.Lambda<Func<uint?, uint?, Func<uint?>>>(
					            Expression.Lambda<Func<uint?>>(Expression.Divide(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<uint?, uint?, Func<uint?>> f2 = e2.Compile();
					
					        uint? f2Result = default(uint?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?, uint?>>> e3 = Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					                    Expression.Lambda<Func<uint?, uint?, uint?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?, uint?, uint?> f3 = e3.Compile()();
					
					        uint? f3Result = default(uint?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?, uint?>>> e4 = Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					            Expression.Lambda<Func<uint?, uint?, uint?>>(
					                Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<uint?, uint?, uint?>> f4 = e4.Compile();
					
					        uint? f4Result = default(uint?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<uint?, Func<uint?, uint?>>> e5 =
					            Expression.Lambda<Func<uint?, Func<uint?, uint?>>>(
					                Expression.Lambda<Func<uint?, uint?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<uint?, Func<uint?, uint?>> f5 = e5.Compile();
					
					        uint? f5Result = default(uint?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?>>> e6 = Expression.Lambda<Func<Func<uint?, uint?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<uint?, Func<uint?, uint?>>>(
					                    Expression.Lambda<Func<uint?, uint?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(uint?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?, uint?> f6 = e6.Compile()();
					
					        uint? f6Result = default(uint?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        uint? csResult = default(uint?);
					        Exception csEx = null;
					        try {
					            csResult = (uint?) (val0 / val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_intq_Divide() {
					        int?[] svals = new int?[] { 0, 1, -1, int.MinValue, int.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_intq_Divide(svals[i], svals[j])) {
					                    Console.WriteLine("intq_Divide failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_intq_Divide(int? val0, int? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(int?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(int?), "p1");
					        Expression<Func<int?>> e1 = Expression.Lambda<Func<int?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<int?, int?, int?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(int?)),
					                    Expression.Constant(val1, typeof(int?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?> f1 = e1.Compile();
					
					        int? f1Result = default(int?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<int?, int?, Func<int?>>> e2 = Expression.Lambda<Func<int?, int?, Func<int?>>>(
					            Expression.Lambda<Func<int?>>(Expression.Divide(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<int?, int?, Func<int?>> f2 = e2.Compile();
					
					        int? f2Result = default(int?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?, int?>>> e3 = Expression.Lambda<Func<Func<int?, int?, int?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<int?, int?, int?>>>(
					                    Expression.Lambda<Func<int?, int?, int?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?, int?, int?> f3 = e3.Compile()();
					
					        int? f3Result = default(int?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?, int?>>> e4 = Expression.Lambda<Func<Func<int?, int?, int?>>>(
					            Expression.Lambda<Func<int?, int?, int?>>(
					                Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<int?, int?, int?>> f4 = e4.Compile();
					
					        int? f4Result = default(int?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<int?, Func<int?, int?>>> e5 =
					            Expression.Lambda<Func<int?, Func<int?, int?>>>(
					                Expression.Lambda<Func<int?, int?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<int?, Func<int?, int?>> f5 = e5.Compile();
					
					        int? f5Result = default(int?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?>>> e6 = Expression.Lambda<Func<Func<int?, int?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<int?, Func<int?, int?>>>(
					                    Expression.Lambda<Func<int?, int?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(int?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?, int?> f6 = e6.Compile()();
					
					        int? f6Result = default(int?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        int? csResult = default(int?);
					        Exception csEx = null;
					        try {
					            csResult = (int?) (val0 / val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_ulongq_Divide() {
					        ulong?[] svals = new ulong?[] { 0, 1, ulong.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_ulongq_Divide(svals[i], svals[j])) {
					                    Console.WriteLine("ulongq_Divide failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulongq_Divide(ulong? val0, ulong? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(ulong?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(ulong?), "p1");
					        Expression<Func<ulong?>> e1 = Expression.Lambda<Func<ulong?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(ulong?)),
					                    Expression.Constant(val1, typeof(ulong?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?> f1 = e1.Compile();
					
					        ulong? f1Result = default(ulong?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<ulong?, ulong?, Func<ulong?>>> e2 = Expression.Lambda<Func<ulong?, ulong?, Func<ulong?>>>(
					            Expression.Lambda<Func<ulong?>>(Expression.Divide(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<ulong?, ulong?, Func<ulong?>> f2 = e2.Compile();
					
					        ulong? f2Result = default(ulong?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?, ulong?>>> e3 = Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					                    Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?, ulong?, ulong?> f3 = e3.Compile()();
					
					        ulong? f3Result = default(ulong?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?, ulong?>>> e4 = Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					            Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<ulong?, ulong?, ulong?>> f4 = e4.Compile();
					
					        ulong? f4Result = default(ulong?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<ulong?, Func<ulong?, ulong?>>> e5 =
					            Expression.Lambda<Func<ulong?, Func<ulong?, ulong?>>>(
					                Expression.Lambda<Func<ulong?, ulong?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<ulong?, Func<ulong?, ulong?>> f5 = e5.Compile();
					
					        ulong? f5Result = default(ulong?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?>>> e6 = Expression.Lambda<Func<Func<ulong?, ulong?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ulong?, Func<ulong?, ulong?>>>(
					                    Expression.Lambda<Func<ulong?, ulong?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(ulong?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?, ulong?> f6 = e6.Compile()();
					
					        ulong? f6Result = default(ulong?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        ulong? csResult = default(ulong?);
					        Exception csEx = null;
					        try {
					            csResult = (ulong?) (val0 / val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_longq_Divide() {
					        long?[] svals = new long?[] { 0, 1, -1, long.MinValue, long.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_longq_Divide(svals[i], svals[j])) {
					                    Console.WriteLine("longq_Divide failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_longq_Divide(long? val0, long? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(long?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(long?), "p1");
					        Expression<Func<long?>> e1 = Expression.Lambda<Func<long?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<long?, long?, long?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(long?)),
					                    Expression.Constant(val1, typeof(long?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?> f1 = e1.Compile();
					
					        long? f1Result = default(long?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<long?, long?, Func<long?>>> e2 = Expression.Lambda<Func<long?, long?, Func<long?>>>(
					            Expression.Lambda<Func<long?>>(Expression.Divide(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<long?, long?, Func<long?>> f2 = e2.Compile();
					
					        long? f2Result = default(long?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?, long?>>> e3 = Expression.Lambda<Func<Func<long?, long?, long?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<long?, long?, long?>>>(
					                    Expression.Lambda<Func<long?, long?, long?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?, long?, long?> f3 = e3.Compile()();
					
					        long? f3Result = default(long?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?, long?>>> e4 = Expression.Lambda<Func<Func<long?, long?, long?>>>(
					            Expression.Lambda<Func<long?, long?, long?>>(
					                Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<long?, long?, long?>> f4 = e4.Compile();
					
					        long? f4Result = default(long?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<long?, Func<long?, long?>>> e5 =
					            Expression.Lambda<Func<long?, Func<long?, long?>>>(
					                Expression.Lambda<Func<long?, long?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<long?, Func<long?, long?>> f5 = e5.Compile();
					
					        long? f5Result = default(long?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?>>> e6 = Expression.Lambda<Func<Func<long?, long?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<long?, Func<long?, long?>>>(
					                    Expression.Lambda<Func<long?, long?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(long?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?, long?> f6 = e6.Compile()();
					
					        long? f6Result = default(long?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        long? csResult = default(long?);
					        Exception csEx = null;
					        try {
					            csResult = (long?) (val0 / val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_floatq_Divide() {
					        float?[] svals = new float?[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_floatq_Divide(svals[i], svals[j])) {
					                    Console.WriteLine("floatq_Divide failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_floatq_Divide(float? val0, float? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(float?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(float?), "p1");
					        Expression<Func<float?>> e1 = Expression.Lambda<Func<float?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<float?, float?, float?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(float?)),
					                    Expression.Constant(val1, typeof(float?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?> f1 = e1.Compile();
					
					        float? f1Result = default(float?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<float?, float?, Func<float?>>> e2 = Expression.Lambda<Func<float?, float?, Func<float?>>>(
					            Expression.Lambda<Func<float?>>(Expression.Divide(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<float?, float?, Func<float?>> f2 = e2.Compile();
					
					        float? f2Result = default(float?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?, float?>>> e3 = Expression.Lambda<Func<Func<float?, float?, float?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<float?, float?, float?>>>(
					                    Expression.Lambda<Func<float?, float?, float?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?, float?, float?> f3 = e3.Compile()();
					
					        float? f3Result = default(float?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?, float?>>> e4 = Expression.Lambda<Func<Func<float?, float?, float?>>>(
					            Expression.Lambda<Func<float?, float?, float?>>(
					                Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<float?, float?, float?>> f4 = e4.Compile();
					
					        float? f4Result = default(float?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<float?, Func<float?, float?>>> e5 =
					            Expression.Lambda<Func<float?, Func<float?, float?>>>(
					                Expression.Lambda<Func<float?, float?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<float?, Func<float?, float?>> f5 = e5.Compile();
					
					        float? f5Result = default(float?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?>>> e6 = Expression.Lambda<Func<Func<float?, float?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<float?, Func<float?, float?>>>(
					                    Expression.Lambda<Func<float?, float?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(float?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?, float?> f6 = e6.Compile()();
					
					        float? f6Result = default(float?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        float? csResult = default(float?);
					        Exception csEx = null;
					        try {
					            csResult = (float?) (val0 / val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_doubleq_Divide() {
					        double?[] svals = new double?[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_doubleq_Divide(svals[i], svals[j])) {
					                    Console.WriteLine("doubleq_Divide failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_doubleq_Divide(double? val0, double? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(double?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(double?), "p1");
					        Expression<Func<double?>> e1 = Expression.Lambda<Func<double?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<double?, double?, double?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(double?)),
					                    Expression.Constant(val1, typeof(double?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?> f1 = e1.Compile();
					
					        double? f1Result = default(double?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<double?, double?, Func<double?>>> e2 = Expression.Lambda<Func<double?, double?, Func<double?>>>(
					            Expression.Lambda<Func<double?>>(Expression.Divide(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<double?, double?, Func<double?>> f2 = e2.Compile();
					
					        double? f2Result = default(double?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?, double?>>> e3 = Expression.Lambda<Func<Func<double?, double?, double?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<double?, double?, double?>>>(
					                    Expression.Lambda<Func<double?, double?, double?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?, double?, double?> f3 = e3.Compile()();
					
					        double? f3Result = default(double?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?, double?>>> e4 = Expression.Lambda<Func<Func<double?, double?, double?>>>(
					            Expression.Lambda<Func<double?, double?, double?>>(
					                Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<double?, double?, double?>> f4 = e4.Compile();
					
					        double? f4Result = default(double?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<double?, Func<double?, double?>>> e5 =
					            Expression.Lambda<Func<double?, Func<double?, double?>>>(
					                Expression.Lambda<Func<double?, double?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<double?, Func<double?, double?>> f5 = e5.Compile();
					
					        double? f5Result = default(double?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?>>> e6 = Expression.Lambda<Func<Func<double?, double?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<double?, Func<double?, double?>>>(
					                    Expression.Lambda<Func<double?, double?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(double?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?, double?> f6 = e6.Compile()();
					
					        double? f6Result = default(double?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        double? csResult = default(double?);
					        Exception csEx = null;
					        try {
					            csResult = (double?) (val0 / val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_decimalq_Divide() {
					        decimal?[] svals = new decimal?[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_decimalq_Divide(svals[i], svals[j])) {
					                    Console.WriteLine("decimalq_Divide failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_decimalq_Divide(decimal? val0, decimal? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(decimal?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(decimal?), "p1");
					        Expression<Func<decimal?>> e1 = Expression.Lambda<Func<decimal?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(decimal?)),
					                    Expression.Constant(val1, typeof(decimal?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?> f1 = e1.Compile();
					
					        decimal? f1Result = default(decimal?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<decimal?, decimal?, Func<decimal?>>> e2 = Expression.Lambda<Func<decimal?, decimal?, Func<decimal?>>>(
					            Expression.Lambda<Func<decimal?>>(Expression.Divide(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<decimal?, decimal?, Func<decimal?>> f2 = e2.Compile();
					
					        decimal? f2Result = default(decimal?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?, decimal?>>> e3 = Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					                    Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?, decimal?, decimal?> f3 = e3.Compile()();
					
					        decimal? f3Result = default(decimal?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?, decimal?>>> e4 = Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					            Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<decimal?, decimal?, decimal?>> f4 = e4.Compile();
					
					        decimal? f4Result = default(decimal?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<decimal?, Func<decimal?, decimal?>>> e5 =
					            Expression.Lambda<Func<decimal?, Func<decimal?, decimal?>>>(
					                Expression.Lambda<Func<decimal?, decimal?>>(
					                    Expression.Divide(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<decimal?, Func<decimal?, decimal?>> f5 = e5.Compile();
					
					        decimal? f5Result = default(decimal?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?>>> e6 = Expression.Lambda<Func<Func<decimal?, decimal?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<decimal?, Func<decimal?, decimal?>>>(
					                    Expression.Lambda<Func<decimal?, decimal?>>(
					                        Expression.Divide(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(decimal?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?, decimal?> f6 = e6.Compile()();
					
					        decimal? f6Result = default(decimal?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        decimal? csResult = default(decimal?);
					        Exception csEx = null;
					        try {
					            csResult = (decimal?) (val0 / val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static Type GetNonNullableType(Type type) {
				        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
				                type.GetGenericArguments()[0] :
				                type;
				    }
				}
				
			
			
			public  class Helper {
			    public static bool verifyException<T>(T fResult, Exception fEx, T csResult, Exception csEx) {
			        if (fEx != null || csEx != null) {
			            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
			        }
			        else {
			            return object.Equals(fResult, csResult);
			        }
			    }
			}
		
	}
				
				//-------- Scenario 385
				namespace Scenario385{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Modulo__2() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ushortq_Modulo() &
					                check_shortq_Modulo() &
					                check_uintq_Modulo() &
					                check_intq_Modulo() &
					                check_ulongq_Modulo() &
					                check_longq_Modulo() &
					                check_floatq_Modulo() &
					                check_doubleq_Modulo() &
					                check_decimalq_Modulo();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushortq_Modulo() {
					        ushort?[] svals = new ushort?[] { 0, 1, ushort.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_ushortq_Modulo(svals[i], svals[j])) {
					                    Console.WriteLine("ushortq_Modulo failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushortq_Modulo(ushort? val0, ushort? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(ushort?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(ushort?), "p1");
					        Expression<Func<ushort?>> e1 = Expression.Lambda<Func<ushort?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(ushort?)),
					                    Expression.Constant(val1, typeof(ushort?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?> f1 = e1.Compile();
					
					        ushort? f1Result = default(ushort?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<ushort?, ushort?, Func<ushort?>>> e2 = Expression.Lambda<Func<ushort?, ushort?, Func<ushort?>>>(
					            Expression.Lambda<Func<ushort?>>(Expression.Modulo(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<ushort?, ushort?, Func<ushort?>> f2 = e2.Compile();
					
					        ushort? f2Result = default(ushort?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?, ushort?>>> e3 = Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					                    Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?, ushort?, ushort?> f3 = e3.Compile()();
					
					        ushort? f3Result = default(ushort?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?, ushort?>>> e4 = Expression.Lambda<Func<Func<ushort?, ushort?, ushort?>>>(
					            Expression.Lambda<Func<ushort?, ushort?, ushort?>>(
					                Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<ushort?, ushort?, ushort?>> f4 = e4.Compile();
					
					        ushort? f4Result = default(ushort?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<ushort?, Func<ushort?, ushort?>>> e5 =
					            Expression.Lambda<Func<ushort?, Func<ushort?, ushort?>>>(
					                Expression.Lambda<Func<ushort?, ushort?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<ushort?, Func<ushort?, ushort?>> f5 = e5.Compile();
					
					        ushort? f5Result = default(ushort?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<ushort?, ushort?>>> e6 = Expression.Lambda<Func<Func<ushort?, ushort?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ushort?, Func<ushort?, ushort?>>>(
					                    Expression.Lambda<Func<ushort?, ushort?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(ushort?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ushort?, ushort?> f6 = e6.Compile()();
					
					        ushort? f6Result = default(ushort?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        ushort? csResult = default(ushort?);
					        Exception csEx = null;
					        try {
					            csResult = (ushort?) (val0 % val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_shortq_Modulo() {
					        short?[] svals = new short?[] { 0, 1, -1, short.MinValue, short.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_shortq_Modulo(svals[i], svals[j])) {
					                    Console.WriteLine("shortq_Modulo failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_shortq_Modulo(short? val0, short? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(short?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(short?), "p1");
					        Expression<Func<short?>> e1 = Expression.Lambda<Func<short?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<short?, short?, short?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(short?)),
					                    Expression.Constant(val1, typeof(short?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?> f1 = e1.Compile();
					
					        short? f1Result = default(short?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<short?, short?, Func<short?>>> e2 = Expression.Lambda<Func<short?, short?, Func<short?>>>(
					            Expression.Lambda<Func<short?>>(Expression.Modulo(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<short?, short?, Func<short?>> f2 = e2.Compile();
					
					        short? f2Result = default(short?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?, short?>>> e3 = Expression.Lambda<Func<Func<short?, short?, short?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<short?, short?, short?>>>(
					                    Expression.Lambda<Func<short?, short?, short?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?, short?, short?> f3 = e3.Compile()();
					
					        short? f3Result = default(short?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?, short?>>> e4 = Expression.Lambda<Func<Func<short?, short?, short?>>>(
					            Expression.Lambda<Func<short?, short?, short?>>(
					                Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<short?, short?, short?>> f4 = e4.Compile();
					
					        short? f4Result = default(short?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<short?, Func<short?, short?>>> e5 =
					            Expression.Lambda<Func<short?, Func<short?, short?>>>(
					                Expression.Lambda<Func<short?, short?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<short?, Func<short?, short?>> f5 = e5.Compile();
					
					        short? f5Result = default(short?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<short?, short?>>> e6 = Expression.Lambda<Func<Func<short?, short?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<short?, Func<short?, short?>>>(
					                    Expression.Lambda<Func<short?, short?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(short?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<short?, short?> f6 = e6.Compile()();
					
					        short? f6Result = default(short?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        short? csResult = default(short?);
					        Exception csEx = null;
					        try {
					            csResult = (short?) (val0 % val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_uintq_Modulo() {
					        uint?[] svals = new uint?[] { 0, 1, uint.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_uintq_Modulo(svals[i], svals[j])) {
					                    Console.WriteLine("uintq_Modulo failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_uintq_Modulo(uint? val0, uint? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(uint?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(uint?), "p1");
					        Expression<Func<uint?>> e1 = Expression.Lambda<Func<uint?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<uint?, uint?, uint?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(uint?)),
					                    Expression.Constant(val1, typeof(uint?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?> f1 = e1.Compile();
					
					        uint? f1Result = default(uint?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<uint?, uint?, Func<uint?>>> e2 = Expression.Lambda<Func<uint?, uint?, Func<uint?>>>(
					            Expression.Lambda<Func<uint?>>(Expression.Modulo(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<uint?, uint?, Func<uint?>> f2 = e2.Compile();
					
					        uint? f2Result = default(uint?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?, uint?>>> e3 = Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					                    Expression.Lambda<Func<uint?, uint?, uint?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?, uint?, uint?> f3 = e3.Compile()();
					
					        uint? f3Result = default(uint?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?, uint?>>> e4 = Expression.Lambda<Func<Func<uint?, uint?, uint?>>>(
					            Expression.Lambda<Func<uint?, uint?, uint?>>(
					                Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<uint?, uint?, uint?>> f4 = e4.Compile();
					
					        uint? f4Result = default(uint?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<uint?, Func<uint?, uint?>>> e5 =
					            Expression.Lambda<Func<uint?, Func<uint?, uint?>>>(
					                Expression.Lambda<Func<uint?, uint?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<uint?, Func<uint?, uint?>> f5 = e5.Compile();
					
					        uint? f5Result = default(uint?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<uint?, uint?>>> e6 = Expression.Lambda<Func<Func<uint?, uint?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<uint?, Func<uint?, uint?>>>(
					                    Expression.Lambda<Func<uint?, uint?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(uint?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<uint?, uint?> f6 = e6.Compile()();
					
					        uint? f6Result = default(uint?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        uint? csResult = default(uint?);
					        Exception csEx = null;
					        try {
					            csResult = (uint?) (val0 % val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_intq_Modulo() {
					        int?[] svals = new int?[] { 0, 1, -1, int.MinValue, int.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_intq_Modulo(svals[i], svals[j])) {
					                    Console.WriteLine("intq_Modulo failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_intq_Modulo(int? val0, int? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(int?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(int?), "p1");
					        Expression<Func<int?>> e1 = Expression.Lambda<Func<int?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<int?, int?, int?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(int?)),
					                    Expression.Constant(val1, typeof(int?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?> f1 = e1.Compile();
					
					        int? f1Result = default(int?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<int?, int?, Func<int?>>> e2 = Expression.Lambda<Func<int?, int?, Func<int?>>>(
					            Expression.Lambda<Func<int?>>(Expression.Modulo(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<int?, int?, Func<int?>> f2 = e2.Compile();
					
					        int? f2Result = default(int?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?, int?>>> e3 = Expression.Lambda<Func<Func<int?, int?, int?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<int?, int?, int?>>>(
					                    Expression.Lambda<Func<int?, int?, int?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?, int?, int?> f3 = e3.Compile()();
					
					        int? f3Result = default(int?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?, int?>>> e4 = Expression.Lambda<Func<Func<int?, int?, int?>>>(
					            Expression.Lambda<Func<int?, int?, int?>>(
					                Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<int?, int?, int?>> f4 = e4.Compile();
					
					        int? f4Result = default(int?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<int?, Func<int?, int?>>> e5 =
					            Expression.Lambda<Func<int?, Func<int?, int?>>>(
					                Expression.Lambda<Func<int?, int?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<int?, Func<int?, int?>> f5 = e5.Compile();
					
					        int? f5Result = default(int?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<int?, int?>>> e6 = Expression.Lambda<Func<Func<int?, int?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<int?, Func<int?, int?>>>(
					                    Expression.Lambda<Func<int?, int?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(int?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<int?, int?> f6 = e6.Compile()();
					
					        int? f6Result = default(int?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        int? csResult = default(int?);
					        Exception csEx = null;
					        try {
					            csResult = (int?) (val0 % val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_ulongq_Modulo() {
					        ulong?[] svals = new ulong?[] { 0, 1, ulong.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_ulongq_Modulo(svals[i], svals[j])) {
					                    Console.WriteLine("ulongq_Modulo failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulongq_Modulo(ulong? val0, ulong? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(ulong?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(ulong?), "p1");
					        Expression<Func<ulong?>> e1 = Expression.Lambda<Func<ulong?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(ulong?)),
					                    Expression.Constant(val1, typeof(ulong?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?> f1 = e1.Compile();
					
					        ulong? f1Result = default(ulong?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<ulong?, ulong?, Func<ulong?>>> e2 = Expression.Lambda<Func<ulong?, ulong?, Func<ulong?>>>(
					            Expression.Lambda<Func<ulong?>>(Expression.Modulo(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<ulong?, ulong?, Func<ulong?>> f2 = e2.Compile();
					
					        ulong? f2Result = default(ulong?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?, ulong?>>> e3 = Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					                    Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?, ulong?, ulong?> f3 = e3.Compile()();
					
					        ulong? f3Result = default(ulong?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?, ulong?>>> e4 = Expression.Lambda<Func<Func<ulong?, ulong?, ulong?>>>(
					            Expression.Lambda<Func<ulong?, ulong?, ulong?>>(
					                Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<ulong?, ulong?, ulong?>> f4 = e4.Compile();
					
					        ulong? f4Result = default(ulong?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<ulong?, Func<ulong?, ulong?>>> e5 =
					            Expression.Lambda<Func<ulong?, Func<ulong?, ulong?>>>(
					                Expression.Lambda<Func<ulong?, ulong?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<ulong?, Func<ulong?, ulong?>> f5 = e5.Compile();
					
					        ulong? f5Result = default(ulong?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<ulong?, ulong?>>> e6 = Expression.Lambda<Func<Func<ulong?, ulong?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<ulong?, Func<ulong?, ulong?>>>(
					                    Expression.Lambda<Func<ulong?, ulong?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(ulong?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<ulong?, ulong?> f6 = e6.Compile()();
					
					        ulong? f6Result = default(ulong?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        ulong? csResult = default(ulong?);
					        Exception csEx = null;
					        try {
					            csResult = (ulong?) (val0 % val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_longq_Modulo() {
					        long?[] svals = new long?[] { 0, 1, -1, long.MinValue, long.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_longq_Modulo(svals[i], svals[j])) {
					                    Console.WriteLine("longq_Modulo failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_longq_Modulo(long? val0, long? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(long?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(long?), "p1");
					        Expression<Func<long?>> e1 = Expression.Lambda<Func<long?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<long?, long?, long?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(long?)),
					                    Expression.Constant(val1, typeof(long?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?> f1 = e1.Compile();
					
					        long? f1Result = default(long?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<long?, long?, Func<long?>>> e2 = Expression.Lambda<Func<long?, long?, Func<long?>>>(
					            Expression.Lambda<Func<long?>>(Expression.Modulo(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<long?, long?, Func<long?>> f2 = e2.Compile();
					
					        long? f2Result = default(long?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?, long?>>> e3 = Expression.Lambda<Func<Func<long?, long?, long?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<long?, long?, long?>>>(
					                    Expression.Lambda<Func<long?, long?, long?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?, long?, long?> f3 = e3.Compile()();
					
					        long? f3Result = default(long?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?, long?>>> e4 = Expression.Lambda<Func<Func<long?, long?, long?>>>(
					            Expression.Lambda<Func<long?, long?, long?>>(
					                Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<long?, long?, long?>> f4 = e4.Compile();
					
					        long? f4Result = default(long?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<long?, Func<long?, long?>>> e5 =
					            Expression.Lambda<Func<long?, Func<long?, long?>>>(
					                Expression.Lambda<Func<long?, long?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<long?, Func<long?, long?>> f5 = e5.Compile();
					
					        long? f5Result = default(long?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<long?, long?>>> e6 = Expression.Lambda<Func<Func<long?, long?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<long?, Func<long?, long?>>>(
					                    Expression.Lambda<Func<long?, long?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(long?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<long?, long?> f6 = e6.Compile()();
					
					        long? f6Result = default(long?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        long? csResult = default(long?);
					        Exception csEx = null;
					        try {
					            csResult = (long?) (val0 % val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_floatq_Modulo() {
					        float?[] svals = new float?[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_floatq_Modulo(svals[i], svals[j])) {
					                    Console.WriteLine("floatq_Modulo failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_floatq_Modulo(float? val0, float? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(float?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(float?), "p1");
					        Expression<Func<float?>> e1 = Expression.Lambda<Func<float?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<float?, float?, float?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(float?)),
					                    Expression.Constant(val1, typeof(float?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?> f1 = e1.Compile();
					
					        float? f1Result = default(float?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<float?, float?, Func<float?>>> e2 = Expression.Lambda<Func<float?, float?, Func<float?>>>(
					            Expression.Lambda<Func<float?>>(Expression.Modulo(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<float?, float?, Func<float?>> f2 = e2.Compile();
					
					        float? f2Result = default(float?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?, float?>>> e3 = Expression.Lambda<Func<Func<float?, float?, float?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<float?, float?, float?>>>(
					                    Expression.Lambda<Func<float?, float?, float?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?, float?, float?> f3 = e3.Compile()();
					
					        float? f3Result = default(float?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?, float?>>> e4 = Expression.Lambda<Func<Func<float?, float?, float?>>>(
					            Expression.Lambda<Func<float?, float?, float?>>(
					                Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<float?, float?, float?>> f4 = e4.Compile();
					
					        float? f4Result = default(float?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<float?, Func<float?, float?>>> e5 =
					            Expression.Lambda<Func<float?, Func<float?, float?>>>(
					                Expression.Lambda<Func<float?, float?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<float?, Func<float?, float?>> f5 = e5.Compile();
					
					        float? f5Result = default(float?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<float?, float?>>> e6 = Expression.Lambda<Func<Func<float?, float?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<float?, Func<float?, float?>>>(
					                    Expression.Lambda<Func<float?, float?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(float?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<float?, float?> f6 = e6.Compile()();
					
					        float? f6Result = default(float?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        float? csResult = default(float?);
					        Exception csEx = null;
					        try {
					            csResult = (float?) (val0 % val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_doubleq_Modulo() {
					        double?[] svals = new double?[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_doubleq_Modulo(svals[i], svals[j])) {
					                    Console.WriteLine("doubleq_Modulo failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_doubleq_Modulo(double? val0, double? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(double?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(double?), "p1");
					        Expression<Func<double?>> e1 = Expression.Lambda<Func<double?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<double?, double?, double?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(double?)),
					                    Expression.Constant(val1, typeof(double?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?> f1 = e1.Compile();
					
					        double? f1Result = default(double?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<double?, double?, Func<double?>>> e2 = Expression.Lambda<Func<double?, double?, Func<double?>>>(
					            Expression.Lambda<Func<double?>>(Expression.Modulo(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<double?, double?, Func<double?>> f2 = e2.Compile();
					
					        double? f2Result = default(double?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?, double?>>> e3 = Expression.Lambda<Func<Func<double?, double?, double?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<double?, double?, double?>>>(
					                    Expression.Lambda<Func<double?, double?, double?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?, double?, double?> f3 = e3.Compile()();
					
					        double? f3Result = default(double?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?, double?>>> e4 = Expression.Lambda<Func<Func<double?, double?, double?>>>(
					            Expression.Lambda<Func<double?, double?, double?>>(
					                Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<double?, double?, double?>> f4 = e4.Compile();
					
					        double? f4Result = default(double?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<double?, Func<double?, double?>>> e5 =
					            Expression.Lambda<Func<double?, Func<double?, double?>>>(
					                Expression.Lambda<Func<double?, double?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<double?, Func<double?, double?>> f5 = e5.Compile();
					
					        double? f5Result = default(double?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<double?, double?>>> e6 = Expression.Lambda<Func<Func<double?, double?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<double?, Func<double?, double?>>>(
					                    Expression.Lambda<Func<double?, double?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(double?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<double?, double?> f6 = e6.Compile()();
					
					        double? f6Result = default(double?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        double? csResult = default(double?);
					        Exception csEx = null;
					        try {
					            csResult = (double?) (val0 % val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					
					    static bool check_decimalq_Modulo() {
					        decimal?[] svals = new decimal?[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
					        for (int i = 0; i < svals.Length; i++) {
					            for (int j = 0; j < svals.Length; j++) {
					                if (!check_decimalq_Modulo(svals[i], svals[j])) {
					                    Console.WriteLine("decimalq_Modulo failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_decimalq_Modulo(decimal? val0, decimal? val1) {
					        ParameterExpression p0 = Expression.Parameter(typeof(decimal?), "p0");
					        ParameterExpression p1 = Expression.Parameter(typeof(decimal?), "p1");
					        Expression<Func<decimal?>> e1 = Expression.Lambda<Func<decimal?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					                new Expression[] {
					                    Expression.Constant(val0, typeof(decimal?)),
					                    Expression.Constant(val1, typeof(decimal?))
					                }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?> f1 = e1.Compile();
					
					        decimal? f1Result = default(decimal?);
					        Exception f1Ex = null;
					        try {
					            f1Result = f1();
					        }
					        catch (Exception ex) {
					            f1Ex = ex;
					        }
					
					        Expression<Func<decimal?, decimal?, Func<decimal?>>> e2 = Expression.Lambda<Func<decimal?, decimal?, Func<decimal?>>>(
					            Expression.Lambda<Func<decimal?>>(Expression.Modulo(p0, p1), new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p0, p1 });
					        Func<decimal?, decimal?, Func<decimal?>> f2 = e2.Compile();
					
					        decimal? f2Result = default(decimal?);
					        Exception f2Ex = null;
					        try {
					            f2Result = f2(val0, val1)();
					        }
					        catch (Exception ex) {
					            f2Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?, decimal?>>> e3 = Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					                    Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p0, p1 }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?, decimal?, decimal?> f3 = e3.Compile()();
					
					        decimal? f3Result = default(decimal?);
					        Exception f3Ex = null;
					        try {
					            f3Result = f3(val0, val1);
					        }
					        catch (Exception ex) {
					            f3Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?, decimal?>>> e4 = Expression.Lambda<Func<Func<decimal?, decimal?, decimal?>>>(
					            Expression.Lambda<Func<decimal?, decimal?, decimal?>>(
					                Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p0, p1 }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<decimal?, decimal?, decimal?>> f4 = e4.Compile();
					
					        decimal? f4Result = default(decimal?);
					        Exception f4Ex = null;
					        try {
					            f4Result = f4()(val0, val1);
					        }
					        catch (Exception ex) {
					            f4Ex = ex;
					        }
					
					        Expression<Func<decimal?, Func<decimal?, decimal?>>> e5 =
					            Expression.Lambda<Func<decimal?, Func<decimal?, decimal?>>>(
					                Expression.Lambda<Func<decimal?, decimal?>>(
					                    Expression.Modulo(p0, p1),
					                    new ParameterExpression[] { p1 }),
					                new ParameterExpression[] { p0 });
					        Func<decimal?, Func<decimal?, decimal?>> f5 = e5.Compile();
					
					        decimal? f5Result = default(decimal?);
					        Exception f5Ex = null;
					        try {
					            f5Result = f5(val0)(val1);
					        }
					        catch (Exception ex) {
					            f5Ex = ex;
					        }
					
					        Expression<Func<Func<decimal?, decimal?>>> e6 = Expression.Lambda<Func<Func<decimal?, decimal?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<decimal?, Func<decimal?, decimal?>>>(
					                    Expression.Lambda<Func<decimal?, decimal?>>(
					                        Expression.Modulo(p0, p1),
					                        new ParameterExpression[] { p1 }),
					                    new ParameterExpression[] { p0 }),
					                new Expression[] { Expression.Constant(val0, typeof(decimal?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<decimal?, decimal?> f6 = e6.Compile()();
					
					        decimal? f6Result = default(decimal?);
					        Exception f6Ex = null;
					        try {
					            f6Result = f6(val1);
					        }
					        catch (Exception ex) {
					            f6Ex = ex;
					        }
					
					        decimal? csResult = default(decimal?);
					        Exception csEx = null;
					        try {
					            csResult = (decimal?) (val0 % val1);
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        return Helper.verifyException(f1Result, f1Ex, csResult, csEx) &&
					            Helper.verifyException(f2Result, f2Ex, csResult, csEx) &&
					            Helper.verifyException(f3Result, f3Ex, csResult, csEx) &&
					            Helper.verifyException(f4Result, f4Ex, csResult, csEx);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static Type GetNonNullableType(Type type) {
				        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
				                type.GetGenericArguments()[0] :
				                type;
				    }
				}
				
			
			
			public  class Helper {
			    public static bool verifyException<T>(T fResult, Exception fEx, T csResult, Exception csEx) {
			        if (fEx != null || csEx != null) {
			            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
			        }
			        else {
			            return object.Equals(fResult, csResult);
			        }
			    }
			}
		
	}
	
}
