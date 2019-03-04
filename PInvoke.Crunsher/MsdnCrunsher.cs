using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Newtonsoft.Json.Linq;

using PInvoke.Common.Models;
using PInvoke.Common.Serialization;

namespace PInvoke.Crunsher
{
    using Type = Common.Models.Type;

    internal class MsdnCrunsher
    {
        public static void DoWork(string libraryDirectory, string outputDirectory)
        {
            string[] libraryFiles = Directory.GetFiles(libraryDirectory, "*.mshc").ToArray();

            if (Debugger.IsAttached)
                libraryFiles = libraryFiles.Take(1).ToArray();

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

                Regex methodRegex = new Regex(@"(?<Type>.+)\s+(?<Name>[a-zA-Z0-9_\*\-\+\/=^\[\]~<>!\*&]+)\((?:\s*(?<Parameter>[^,;]+)\s*[,\)])*", RegexOptions.Compiled);
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

                        XElement headElement = document.Root
                            .Descendants(w3 + "Title")
                            .FirstOrDefault();

                        if (headElement.Value.EndsWith(" macro"))
                            continue;

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

                        Library library = libraries.GetOrAdd(libraryName, x => new Library()
                        {
                            Name = libraryName,
                            Enumerations = new ConcurrentBag<Enumeration>(),
                            Methods = new ConcurrentBag<Method>()
                        });

                        ConcurrentBag<Enumeration> enumerations = library.Enumerations as ConcurrentBag<Enumeration>;
                        ConcurrentBag<Method> methods = library.Methods as ConcurrentBag<Method>;

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

                            EnumerationValue[] values = enumMatch.Groups["Name"].Captures
                                .Select((n, i) => new EnumerationValue()
                                {
                                    Name = n.Value,
                                    Value = i < enumMatch.Groups["Value"].Captures.Count ? enumMatch.Groups["Value"].Captures[i].Value : null
                                })
                                .ToArray();

                            Enumeration enumeration = new Enumeration()
                            {
                                Name = enumName,
                                Values = values
                            };

                            enumerations.Add(enumeration);
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

                            Parameter[] parameters = methodMatch.Groups["Parameter"].Captures
                                .Select((p, i) =>
                                {
                                    string parameter = p.Value.Trim();
                                    if (string.IsNullOrWhiteSpace(parameter))
                                        return null;

                                    if (parameter.EndsWith("OPTIONAL", StringComparison.InvariantCultureIgnoreCase))
                                        parameter = parameter.Remove(parameter.Length - 8).Trim();

                                    int lastSeparator = parameter.LastIndexOfAny(new[] { ' ', '\t', '*' });

                                    string parameterType = lastSeparator == -1 ? parameter : parameter.Remove(lastSeparator);
                                    string parameterName = lastSeparator == -1 ? "" : parameter.Substring(lastSeparator).Trim();

                                    if (parameterName.Length > 2 && !parameterName.Any(c => char.IsLower(c)))
                                    {
                                        parameterType += parameterName;
                                        parameterName = "";
                                    }

                                    return new Parameter()
                                    {
                                        ParameterType = new ParsedType() { Raw = parameterType.Trim() },
                                        Name = parameterName == "" ? null : parameterName
                                    };
                                })
                                .Where(p => p != null)
                                .ToArray();

                            Method method = new Method()
                            {
                                ReturnType = new ParsedType() { Raw = methodMatch.Groups["Type"].Value },
                                Name = methodMatch.Groups["Name"].Value,
                                Parameters = parameters
                            };

                            methods.Add(method);

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

            int parrallelTaskCount = 1;
            if (!Debugger.IsAttached)
                parrallelTaskCount = Math.Max(1, Environment.ProcessorCount / 2);

            Task[] crunshingTasks = Enumerable.Range(0, parrallelTaskCount)
                .Select(i => crunshingAction())
                .ToArray();

            Task allTasks = Task.WhenAll(crunshingTasks);

            Task reportingTask = Task.Run(async () =>
            {
                while (!allTasks.IsCompleted)
                {
                    await Task.Delay(750);

                    int methodCount = libraries.Sum(l => l.Value.Methods.Count());
                    int enumerationCount = libraries.Sum(l => l.Value.Enumerations.Count());

                    Console.WriteLine($"[MSDN] {crunshedFileCount * 100 / totalFileCount}% - {libraries.Count} libraries, {methodCount} methods, {enumerationCount} enums");
                }
            });

            allTasks.Wait();

            // Dedup methods and group variants
            foreach (Library library in libraries.Values)
            {
                Method[] methods = library.Methods
                    .OrderBy(m => m.Name)
                    .GroupBy(m => m.ToString())
                    .Select(g => g.First())
                    .ToArray();

                methods = methods
                    .GroupBy(m => m.Name.TrimEnd('W').TrimEnd('A'))
                    .Select(g =>
                    {
                        Method[] variantMethods = g
                            .OrderByDescending(m => m.Name)
                            .ToArray();

                        if (g.Key != variantMethods[0].Name)
                        {
                            return new Method()
                            {
                                ReturnType = variantMethods[0].ReturnType,
                                Name = g.Key,
                                Parameters = variantMethods[0].Parameters,
                                Variants = variantMethods
                            };
                        }
                        else
                            return variantMethods[0];
                    })
                    .ToArray();

                library.Methods = new ConcurrentBag<Method>(methods);
            }

            // Dump the data
            Source source = new Source()
            {
                Name = "MSDN",
                Libraries = libraries.Values
            };

            JObject result = Serializer.Serialize(source);
            string json = result.ToString();

            string outputPath = Path.Combine(outputDirectory, "Output_MSDN.json");
            File.WriteAllText(outputPath, json);
        }
    }
}
