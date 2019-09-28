using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;

using Newtonsoft.Json.Linq;

using PInvoke.Common.Models;
using PInvoke.Common.Serialization;

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

            SqliteConnection sqliteConnection = new SqliteConnection($"Data Source={databasePath}");
            sqliteConnection.Open();

            void executeQuery(string query)
            {
                using (SqliteCommand sqliteCommand = sqliteConnection.CreateCommand())
                {
                    sqliteCommand.CommandText = query;
                    sqliteCommand.ExecuteNonQuery();
                }
            }

            foreach (string table in new[] { "methods", "structures", "enumerations" })
            {
                executeQuery($"CREATE TABLE {table} (source VARCHAR NOT NULL, library VARCHAR NOT NULL, name VARCHAR NOT NULL, content VARCHAR);");
                executeQuery($"CREATE INDEX idx_{table}_source ON {table} (source);");
                executeQuery($"CREATE INDEX idx_{table}_source_library ON {table} (source, library);");
                executeQuery($"CREATE INDEX idx_{table}_source_library_name ON {table} (source, library, name);");
            }

            int insertedMethods = 0;

            Task insertionTask = Task.Run(() =>
            {
                List<string> methodQueries = new List<string>(100);

                void flushMethods()
                {
                    if (methodQueries.Count == 0)
                        return;

                    string bulkQuery = "INSERT INTO methods VALUES " + string.Join(", ", methodQueries);
                    executeQuery(bulkQuery);

                    insertedMethods += methodQueries.Count;
                    methodQueries.Clear();
                }

                foreach (Source source in sources)
                {
                    foreach (Library library in source.Libraries)
                    {
                        foreach (Method method in library.Methods)
                        {
                            JObject methodObject = Serializer.Serialize(method);
                            string methodJson = methodObject.ToString();
                            byte[] methodBytes = Encoding.UTF8.GetBytes(methodJson);

                            methodQueries.Add($"('{source.Name}', '{library.Name}', '{method.Name}', '{Convert.ToBase64String(methodBytes)}')");

                            if (methodQueries.Count == methodQueries.Capacity)
                                flushMethods();
                        }
                    }
                }

                flushMethods();
            });

            Task.Run(async () =>
            {
                int methodCount = sources
                    .SelectMany(s => s.Libraries)
                    .SelectMany(l => l.Methods)
                    .Count();

                while (!insertionTask.IsCompleted)
                {
                    await Task.Delay(750);

                    if (insertionTask.IsCompleted)
                        break;

                    Console.WriteLine($"- {insertedMethods * 100 / methodCount}% ({insertedMethods} methods)");
                }
            });

            insertionTask.Wait();
        }
    }
}
