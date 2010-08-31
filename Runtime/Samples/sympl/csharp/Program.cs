using System;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;

namespace SymplSample
{
    class Program
    {
        static void Main(string[] args)
        {
            string dllPath = typeof(object).Assembly.Location;
            Assembly asm = Assembly.LoadFile(dllPath);

            string filename = @"..\..\Runtime\Samples\sympl\examples\test.sympl";
            var s = new Sympl(new Assembly[] { asm });
            var feo = s.ExecuteFile(filename);

            Console.WriteLine("ExecuteExpr ... ");
            s.ExecuteExpr("(print 5)", feo);

            if (args.Length > 0 && args[0] == "norepl") return;

            string input = null;
            string exprstr = "";
            Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
            Console.WriteLine("Enter expressions.  Enter blank line to abort input.");
            Console.WriteLine("Enter 'exit (the symbol) to exit.");
            Console.WriteLine();
            string prompt = ">>> ";
            while (true) {
                Console.Write(prompt);
                input = Console.ReadLine();
                if (input == "") {
                    exprstr = "";
                    prompt = ">>> ";
                    continue;
                } else {
                    exprstr = exprstr + " " + input;
                }
                // See if we have complete input.
                try {
                    var ast = new Parser().ParseExpr(new StringReader(exprstr));
                }
                catch (Exception) {
                    prompt = "... ";
                    continue;
                }
                // We do, so execute.
                try {
                    object res = s.ExecuteExpr(exprstr, feo);
                    exprstr = "";
                    prompt = ">>> ";
                    if (res == s.MakeSymbol("exit")) return;
                    Console.WriteLine(res);
                } catch (Exception e) {
                    exprstr = "";
                    prompt = ">>> ";
                    Console.Write("ERROR: ");
                    Console.WriteLine(e);
                }
            }
        }
    }
}
