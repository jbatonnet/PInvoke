using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using PInvoke.Common.Models;

namespace PInvoke.Crunsher
{
    internal class ManCrunsher
    {
        public static Source Crunsh(string libraryDirectory)
        {
            ConcurrentQueue<string> manFiles = new ConcurrentQueue<string>(Directory.GetFiles(libraryDirectory, "*.*", SearchOption.AllDirectories));
            ConcurrentDictionary<string, Library> libraries = new ConcurrentDictionary<string, Library>(StringComparer.InvariantCultureIgnoreCase);

            int totalFileCount = manFiles.Count;
            int crunshedFileCount = 0;

            // Extract pages data
            void crunshingAction()
            {
                Regex methodRegex = new Regex(@"(?<ReturnType>.+)\s+(?<Name>[a-z0-9_\*\-\+\/=^\[\]~<>!\*&]+)\((?:\s*(?<ParameterType>[^,;\)]+\s+\**)(?<ParameterName>[^,\)\s]+)\s*[,\)])*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                while (manFiles.TryDequeue(out string manFile))
                {
                    string[] lines = File.ReadAllLines(manFile);

                    Thread.Sleep(10);

                    Interlocked.Increment(ref crunshedFileCount);
                }
            }

            Task[] crunshingTasks = Enumerable.Range(0, 1)//Math.Max(1, Environment.ProcessorCount / 2))
                .Select(i => Task.Run(new Action(crunshingAction)))
                .ToArray();

            Task allTasks = Task.WhenAll(crunshingTasks);

            Task reportingTask = Task.Run(async () =>
            {
                while (!allTasks.IsCompleted)
                {
                    await Task.Delay(750);

                    int methodCount = libraries.Sum(l => l.Value.Methods.Count());
                    int enumerationCount = libraries.Sum(l => l.Value.Enumerations.Count());

                    Console.WriteLine($"[man] {crunshedFileCount * 100 / totalFileCount}% - {libraries.Count} libraries, {methodCount} methods, {enumerationCount} enums");
                }
            });

            allTasks.Wait();

            // Dedup the data
            foreach (Library library in libraries.Values)
            {
                Method[] methods = library.Methods
                    .OrderBy(m => m.Name)
                    .GroupBy(m => m.ToString())
                    .Select(g => g.First())
                    .ToArray();

                library.Methods = new ConcurrentBag<Method>(methods);
            }

            return new Source()
            {
                Name = "man",
                Libraries = libraries.Values
            };
        }
    }
}
