copy /y ..\..\*.dll
csc /debug+ /r:IronPython.dll MonthAtAGlance.cs App.cs MonthAtAGlance.Designer.cs
