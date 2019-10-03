using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using PInvoke.Common.Models;
using PInvoke.Storage;

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

            Console.WriteLine();

            List<Source> sources = new List<Source>();

            // MSDN library
            if (Options.ContainsKey("msdn"))
            {
                string msdnDirectory = Options["msdn"];
                Console.WriteLine($"Crunshing MSDN documentation at {msdnDirectory} ...");

                sources.Add(MsdnCrunsher.Crunsh(msdnDirectory));
                Console.WriteLine();
            }

            // Linux man pages
            if (Options.ContainsKey("man"))
            {
                string manDirectory = Options["man"];
                Console.WriteLine($"Crunshing man pages at {manDirectory} ...");

                sources.Add(ManCrunsher.Crunsh(manDirectory));
                Console.WriteLine();
            }

            // Dump result on disk
            Console.WriteLine("Writing results to disk ...");
            
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            // Setup the database
            string databasePath = Path.Combine(outputDirectory, "Output.db");

            if (File.Exists(databasePath))
                File.Delete(databasePath);

            SqliteStorage sqliteStorage = new SqliteStorage(databasePath);

            int insertedMethods = 0;
            int insertedEnumerations = 0;
            int insertedStructures = 0;

            Task insertionTask = Task.Run(() =>
            {
                IEnumerable<MethodData> methods = from s in sources
                                                  from l in s.Libraries
                                                  from m in l.Methods
                                                  select new MethodData() { Source = s.Name, Library = l.Name, Name = m.Name, Content = m };

                IEnumerable<EnumerationData> enumerations = from s in sources
                                                            from l in s.Libraries
                                                            from e in l.Enumerations
                                                            select new EnumerationData() { Source = s.Name, Library = l.Name, Name = e.Name, Content = e };

                IEnumerable<StructureData> structures = from s in sources
                                                        from l in s.Libraries
                                                        from c in l.Structures
                                                        select new StructureData() { Source = s.Name, Library = l.Name, Name = c.Name, Content = c };

                IEnumerable<int> insertionProgress = sqliteStorage.Insert(methods);
                foreach (int progress in insertionProgress)
                    insertedMethods = progress;

                insertionProgress = sqliteStorage.Insert(enumerations);
                foreach (int progress in insertionProgress)
                    insertedEnumerations = progress;

                insertionProgress = sqliteStorage.Insert(structures);
                foreach (int progress in insertionProgress)
                    insertedStructures = progress;
            });

            Task.Run(async () =>
            {
                int methodCount = sources
                    .SelectMany(s => s.Libraries)
                    .SelectMany(l => l.Methods)
                    .Count();

                int enumerationCount = sources
                    .SelectMany(s => s.Libraries)
                    .SelectMany(l => l.Enumerations)
                    .Count();

                int structureCount = sources
                    .SelectMany(s => s.Libraries)
                    .SelectMany(l => l.Structures)
                    .Count();

                int totalCount = methodCount + enumerationCount + structureCount;

                while (!insertionTask.IsCompleted)
                {
                    await Task.Delay(750);

                    if (insertionTask.IsCompleted)
                        break;

                    int insertedCount = insertedMethods + insertedEnumerations + insertedStructures;
                    Console.WriteLine($"- {insertedCount * 100 / totalCount}% ({insertedMethods} methods, {insertedEnumerations} enumerations, {insertedStructures} structures)");
                }
            });

            insertionTask.Wait();
        }
    }
}
