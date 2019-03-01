using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Newtonsoft.Json.Linq;

using PInvoke.Common;

namespace PInvoke.Crunsher
{
    using Type = Common.Type;
    using Enum = Common.Enum;

    internal class MsdnCrunsher
    {
        public static void DoWork()
        {
            string libraryDirectory = @"D:\Temp\MSDN\EN-US";
            string[] libraryFiles = Directory.GetFiles(libraryDirectory, "*.mshc").Take(1).ToArray();

            int totalFileCount = 0;
            int crunshedFileCount = 0;

            foreach (string libraryFile in libraryFiles)
            {
                using (FileStream libraryFileStream = new FileStream(libraryFile, FileMode.Open))
                using (ZipArchive libraryFileArchive = new ZipArchive(libraryFileStream, ZipArchiveMode.Read))
                    totalFileCount += libraryFileArchive.Entries.Count;
            }

            ConcurrentQueue<string> fileContents = new ConcurrentQueue<string>();
            ConcurrentDictionary<string, Library> libraries = new ConcurrentDictionary<string, Library>(StringComparer.InvariantCultureIgnoreCase);

            // Extract MSDN library pages
            Task loadingTask = Task.Run(() =>
            {
                foreach (string libraryFile in libraryFiles)
                {
                    using (FileStream libraryFileStream = new FileStream(libraryFile, FileMode.Open))
                    using (ZipArchive libraryFileArchive = new ZipArchive(libraryFileStream, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry libraryFileEntry in libraryFileArchive.Entries)
                        {
                            if (Path.GetExtension(libraryFileEntry.Name) != ".html")
                                continue;

                            using (StreamReader libraryFileEntryReader = new StreamReader(libraryFileEntry.Open()))
                            {
                                string libraryFileEntryContent = libraryFileEntryReader.ReadToEnd();
                                fileContents.Enqueue(libraryFileEntryContent);
                            }
                        }
                    }
                }
            });

            // Extract pages data
            async Task crunshingAction()
            {
                XNamespace w3 = XNamespace.Get("http://www.w3.org/1999/xhtml");

                Regex methodRegex = new Regex(@"(?<ReturnType>.+)\s+(?<Name>[a-z0-9_\*\-\+\/=^\[\]~<>!\*&]+)\((?:\s*(?<ParameterType>[^,;\)]+\s+\**)(?<ParameterName>[^,\)\s]+)\s*[,\)])*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                Regex enumRegex = new Regex(@"(?<AlternativeType>[a-z0-9_]+)\s*\{(?:\s*(?<Name>[a-z0-9_]+)(?:\s*=\s*(?<Value>[^,\}]+))?\s*[,\}])+\s+(?<Type>[^;]+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                Regex tagRegex = new Regex(@"<[^>]+>", RegexOptions.Compiled);
                Regex parenthesisRegex = new Regex(@"\([^\)]+\)", RegexOptions.Compiled);
                Regex spaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

                while (!loadingTask.IsCompleted)
                {
                    while (fileContents.TryDequeue(out string fileContent))
                    {
                        Interlocked.Increment(ref crunshedFileCount);

                        XDocument document = XDocument.Parse(fileContent);

                        XElement contentElement = document.Root
                            .Element(w3 + "body")
                            .Descendants(w3 + "div")
                            .FirstOrDefault(e => e.Attribute("id")?.Value == "mainBody")
                            .Element("div");

                        XNode[] contentNodes = contentElement.Nodes().ToArray();
                        int nodeIndex = 0;

                        XNode currentNode = null;

                        // Extract data
                        List<XNode> descriptionNodes = new List<XNode>();
                        List<XNode> syntaxNodes = new List<XNode>();
                        List<XNode> parametersNodes = new List<XNode>();
                        List<XNode> returnValueNodes = new List<XNode>();
                        List<XNode> remarksNodes = new List<XNode>();
                        List<XNode> requirementsNodes = new List<XNode>();
                        List<XNode> relationsNodes = new List<XNode>();

                        for (; nodeIndex < contentNodes.Length; nodeIndex++)
                        {
                            currentNode = contentNodes[nodeIndex];

                            if (currentNode is XElement messageElement && messageElement.Attribute("class")?.Value == "CCE_Message")
                            {
                                nodeIndex++;
                                continue;
                            }

                            if (currentNode is XElement titleElement && titleElement.Name == "h2")
                                break;

                            descriptionNodes.Add(currentNode);
                        }

                        while (nodeIndex < contentNodes.Length)
                        {
                            string currentId = (currentNode as XElement)?.Attribute("id")?.Value;

                            // Extract syntax
                            if (currentId == "syntax")
                            {
                                for (nodeIndex++; nodeIndex < contentNodes.Length; nodeIndex++)
                                {
                                    currentNode = contentNodes[nodeIndex];

                                    if (currentNode is XElement titleElement && titleElement.Name == "h2")
                                        break;

                                    syntaxNodes.Add(currentNode);
                                }
                            }

                            // Extract parameters
                            else if (currentId == "parameters")
                            {
                                for (nodeIndex++; nodeIndex < contentNodes.Length; nodeIndex++)
                                {
                                    currentNode = contentNodes[nodeIndex];

                                    if (currentNode is XElement titleElement && titleElement.Name == "h2")
                                        break;

                                    parametersNodes.Add(currentNode);
                                }
                            }

                            // Extract return value
                            else if (currentId == "return-value")
                            {
                                for (nodeIndex++; nodeIndex < contentNodes.Length; nodeIndex++)
                                {
                                    currentNode = contentNodes[nodeIndex];

                                    if (currentNode is XElement titleElement && titleElement.Name == "h2")
                                        break;

                                    returnValueNodes.Add(currentNode);
                                }
                            }

                            // Extract remarks
                            else if (currentId == "remarks")
                            {
                                for (nodeIndex++; nodeIndex < contentNodes.Length; nodeIndex++)
                                {
                                    currentNode = contentNodes[nodeIndex];

                                    if (currentNode is XElement titleElement && titleElement.Name == "h2")
                                        break;

                                    remarksNodes.Add(currentNode);
                                }
                            }

                            // Extract requirements
                            else if (currentId == "requirements")
                            {
                                for (nodeIndex++; nodeIndex < contentNodes.Length; nodeIndex++)
                                {
                                    currentNode = contentNodes[nodeIndex];

                                    if (currentNode is XElement titleElement && titleElement.Name == "h2")
                                        break;

                                    requirementsNodes.Add(currentNode);
                                }
                            }

                            // Extract relations
                            else if (currentId == "see-also")
                            {
                                for (nodeIndex++; nodeIndex < contentNodes.Length; nodeIndex++)
                                {
                                    currentNode = contentNodes[nodeIndex];

                                    if (currentNode is XElement titleElement && titleElement.Name == "h2")
                                        break;

                                    relationsNodes.Add(currentNode);
                                }
                            }

                            // Move to the next section
                            else
                            {
                                for (nodeIndex++; nodeIndex < contentNodes.Length; nodeIndex++)
                                {
                                    currentNode = contentNodes[nodeIndex];

                                    if (currentNode is XElement titleElement && titleElement.Name == "h2")
                                        break;
                                }
                            }
                        }

                        // Process requirements
                        Dictionary<string, string> requirements = requirementsNodes
                            .OfType<XElement>()
                            .SelectMany(n => n.Descendants("tr"))
                            .Select(r => r.Descendants("td"))
                            .Where(r => r.Any())
                            .ToDictionary
                            (
                                g => tagRegex.Replace(g.First().ToString(), "").Trim(),
                                g => tagRegex.Replace(string.Join(", ", g.Skip(1)), "").Trim(),
                                StringComparer.InvariantCultureIgnoreCase
                            );

                        string libraryName = requirements.GetValueOrDefault("DLL") ?? requirements.GetValueOrDefault("Header");
                        if (libraryName == null)
                            continue;

                        libraryName = parenthesisRegex.Replace(libraryName, "");

                        Library library = libraries.GetOrAdd(libraryName, x => new Library() { Name = libraryName });

                        // Process syntax
                        string syntax = string.Join(Environment.NewLine, syntaxNodes.OfType<XElement>().Select(n => n.Value));
                        if (string.IsNullOrEmpty(syntax))
                            continue;

                        if (syntax.StartsWith("typedef enum ") || syntax.StartsWith("enum "))
                        {
                            Match enumMatch = enumRegex.Match(syntax);
                            if (!enumMatch.Success)
                                enumMatch.ToString();

                            string enumName = enumMatch.Groups["Type"].Value;
                            if (string.IsNullOrEmpty(enumName))
                                enumName = enumMatch.Groups["AlternativeType"].Value;

                            Dictionary<string, string> values = enumMatch.Groups["Name"].Captures
                                .Select((n, i) => new { Name = n.Value, Value = i < enumMatch.Groups["Value"].Captures.Count ? enumMatch.Groups["Value"].Captures[i].Value : null })
                                .ToDictionary(n => n.Name, n => n.Value);

                            Enum enumeration = new Enum()
                            {
                                Name = enumName,
                                Values = values
                            };

                            library.Enums.Add(enumeration);
                        }
                        else if (syntax.StartsWith("typedef struct ") || syntax.StartsWith("struct "))
                        {
                        }
                        else if (syntax.StartsWith("typedef union ") || syntax.StartsWith("union "))
                        {
                        }
                        else
                        {
                            Match methodMatch = methodRegex.Match(syntax);
                            if (!methodMatch.Success)
                                methodMatch.ToString();

                            Parameter[] parameters = methodMatch.Groups["ParameterName"].Captures
                                .Select((p, i) => new Parameter()
                                {
                                    ParameterType = new Type() { Name = methodMatch.Groups["ParameterType"].Captures[i].Value.Trim() },
                                    Name = p.Value
                                })
                                .ToArray();

                            Method method = new Method()
                            {
                                ReturnType = new Type() { Name = methodMatch.Groups["ReturnType"].Value },
                                Name = methodMatch.Groups["Name"].Value,
                                Parameters = parameters
                            };

                            library.Methods.Add(method);

                            // FIXME: Debug test
                            string test1 = spaceRegex.Replace(syntax, "").TrimEnd(';');
                            string test2 = method.ToString().Replace(" ", "");
                            if (!test1.Contains(";") && !test1.Contains("(Mem)") && !test1.Contains("__out_data_source") && !test1.Contains("(*)") && test1 != test2)
                                method.ToString();
                        }
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                }
            }

            Task[] crunshingTasks = Enumerable.Range(0, 1)//Math.Max(1, Environment.ProcessorCount / 2))
                .Select(i => crunshingAction())
                .ToArray();

            Task allTasks = Task.WhenAll(crunshingTasks);

            Task reportingTask = Task.Run(async () =>
            {
                while (!allTasks.IsCompleted)
                {
                    await Task.Delay(750);
                    Console.WriteLine($"[MSDN] {crunshedFileCount * 100 / totalFileCount}% - {libraries.Count} libraries, {libraries.Sum(l => l.Value.Methods.Count)} methods, {libraries.Sum(l => l.Value.Enums.Count)} enums");
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

            // Dump the data
            JObject[] objects = libraries.Values
                .AsParallel()
                .Select(l => new JObject()
                {
                    { "Name", l.Name },
                    { "Enums", new JArray(l.Enums.Select(e => new JObject() {
                        { "Name", e.Name },
                        { "Values", new JArray(e.Values.Keys) }
                    }))},
                    { "Methods", new JArray(l.Methods.Select(m => new JObject() {
                        { "Signature", m.ToString() },
                        { "ReturnType", m.ReturnType.Name },
                        { "Name", m.Name },
                        { "Parameters", new JArray(m.Parameters.Select(p => new JObject() {
                            { "ParameterType", p.ParameterType.Name },
                            { "Name", p.Name }
                        }))}
                    }))}
                })
                .OrderBy(l => l["Name"].Value<string>())
                .ToArray();

            string json = new JArray(objects).ToString();
            File.WriteAllText("Output.json", json);
        }
    }
}
