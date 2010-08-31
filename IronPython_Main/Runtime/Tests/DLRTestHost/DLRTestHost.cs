using System;
using System.Collections.Generic;
using System.IO;


using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Hosting;
using IronPython.Runtime;



namespace DLRTestHost
{

    class DLRTestHost
    {
        static StringWriter _writer = new StringWriter();
        static int language = -1;


        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "-h" || args[0] == "/?")
            {
                Usage();
                return;
            }


            //ScriptDomainManager.Options.LightweightScopes = true;

            StreamReader sr = new StreamReader(args[0]);
            String line;
            List<String> files = new List<string>();
            while ((line = sr.ReadLine()) != null)
                files.Add(line);

            if (args.Length > 1)
            {
                if (args[1] == "py") {
                    language = 0;
                } else {
                    Usage();
                    return;
                }
            }
            else
                language = 0;

            int count = 5;
            String testtype = "BenchUtil";
            String testunits = "Seconds";
            String invdir = "False";
            String testname = "sunspider";


            if (args.Length > 2)
                count = Int32.Parse(args[2]);
            if (args.Length > 3)
                testtype = args[3];
            if (args.Length > 4)
                testunits = args[4];
            if (args.Length > 5)
                invdir = args[5];
            if (args.Length > 6)
                testname = args[6];



            double grandtotal = 0.0;
            List<String> results = new List<string>();

            foreach (String file in files)
            {
                if (file == "")
                {
                    results.Add("0.000");
                    continue;
                }
                int total = 0;
                for (int i = 0; i < count; i++)
                    total += TestFile(file);
                double avg = (double)total / count;
                avg /= 1000; //convert to secs
                avg = Math.Round(avg, 3);
                grandtotal += avg;
                string name = Path.GetFileNameWithoutExtension(file);
                string savg = string.Format("{0:F3}", avg);
                results.Add(savg);
                //Console.WriteLine("{0} ::: {1}",name,savg);
            }

            string sgtotal = string.Format("{0:F3}", grandtotal);

            Console.WriteLine("<TestResults Type=\"{0}\">", testtype);
            Console.WriteLine("<TestResult Name=\"{0}\" Units=\"{1}\" InvertDirection=\"{2}\" Total=\"True\">{3}</TestResult>", testname, testunits, invdir, sgtotal);
            for (int i = 0; i < files.Count; i++)
            {
                if (files[i] == "") continue;
                Console.WriteLine("<TestResult Name=\"{0}\" Units=\"{1}\" InvertDirection=\"{2}\">{3}</TestResult>", Path.GetFileNameWithoutExtension(files[i]), testunits, invdir, results[i]);
            }
            Console.WriteLine("</TestResults>");

        }



        static string mb(long number)
        {
            return "" + Math.Round(number / (1024.0 * 1024.0)) + "mb";
        }

        static int TestFile(string file)
        {
            ScriptEngine _engine = null;

            if (language == 0)
            {
                _engine = IronPython.Hosting.Python.CreateEngine();               
               // _engine = (new ScriptRuntime(srsetup)).GetEngineByFileExtension("py");


                PythonContext context = (PythonContext)Microsoft.Scripting.Hosting.Providers.HostingHelpers.GetLanguageContext(_engine);

                string path = Environment.GetEnvironmentVariable("IRONPYTHONPATH");
                if (path != null && path.Length > 0)
                {
                    string[] paths = path.Split(Path.PathSeparator);
                    foreach (string p in paths)
                    {
                         //context.AddToPath(p);
                    }
                }
  
            }

            _engine.Runtime.IO.SetOutput(Console.OpenStandardError(), _writer);

            string name = Path.GetFileNameWithoutExtension(file);
            ScriptSource script = _engine.CreateScriptSourceFromFile(file);
            CompiledCode code = script.Compile();
            code.Execute();
            String time = _writer.GetStringBuilder().ToString().Trim();
            _writer.GetStringBuilder().Length = 0;

            // consider doing it twice to account better for compilation costs
            //Console.WriteLine(time);
            double ft = double.Parse(time);
            return (int)Math.Round(ft);
        }

        static void Usage()
        {
            Console.WriteLine("Usage: DLRTestHost <listfilename> <language> [count] [testtype] [units] [invert] [testname]");
        }
    }
}
