using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PInvoke.Crunsher
{
    public class Program
    {
        public static Dictionary<string, string> Options { get; private set; }
        public static List<string> Parameters { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            Console.Title = "PInvoke.Crunsher";

            // Parse parameters
            Options = args.Where(a => a.StartsWith("/"))
                          .Select(a => a.TrimStart('/'))
                          .Select(a => new { Parameter = a.Trim(), Separator = a.Trim().IndexOf(':') })
                          .ToDictionary(a => a.Separator == -1 ? a.Parameter : a.Parameter.Substring(0, a.Separator).ToLower(), a => a.Separator == -1 ? null : a.Parameter.Substring(a.Separator + 1), StringComparer.InvariantCultureIgnoreCase);
            Parameters = args.Where(a => !a.StartsWith("/"))
                             .ToList();

            // Create output directory if needed
            string outputDirectory = "Output";
            if (Options.ContainsKey("output"))
                outputDirectory = Options["output"];

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            // MSDN library
            if (Options.ContainsKey("msdn"))
            {
                string msdnDirectory = Options["msdn"];
                MsdnCrunsher.DoWork(msdnDirectory, outputDirectory);
            }

            // Linux man pages
            if (Options.ContainsKey("man"))
            {
                string manDirectory = Options["man"];
                ManCrunsher.DoWork(manDirectory, outputDirectory);
            }
        }
    }
}
