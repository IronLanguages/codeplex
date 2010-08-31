extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Diagnostics;

class Test
{
    static string Run(string cmd, string args)
    {
        string result = "";
        ProcessStartInfo si = new ProcessStartInfo();
        si.FileName = cmd;
        si.Arguments = args;
        si.UseShellExecute = false;
        Process p1 = Process.Start(si);
        p1.WaitForExit(10000);
        if (!p1.HasExited) result = "Could assemble FunkyDelegate0.il: timeout";
        if (p1.ExitCode != 0) result = "Could assemble FunkyDelegate0.il: " + p1.StandardOutput.ReadToEnd();
        return result;
    }

    static int Main(string[] args)
    {
        try
        {
            string result = Run("ilasm.exe", "FunkyDelegate0.il /output=FunkyDelegate.dll /dll");
            if (result != "") throw new Exception(result);
            result = Run("csc", "User.cs /r:FunkyDelegate.dll " + System.Environment.GetEnvironmentVariable("APTCAFUNKYFLAGS"));
            if (result != "") throw new Exception(result);
            result = Run("ilasm.exe", "FunkyDelegate.il /output=FunkyDelegate.dll /dll");
            if (result != "") throw new Exception(result);
            result = Run("User.exe", "");
            if (result != "") throw new Exception(result);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: ");
            Console.WriteLine(ex.ToString());
            return 1;
        }
    }
}
