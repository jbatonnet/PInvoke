using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;

using PInvoke.Common.Generators.CSharp;
using PInvoke.Common.Models;
using PInvoke.Common.Serialization;

namespace PInvoke.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            string dataDirectory = @"D:\Projets\C#\PInvoke\Data";

            string[] dataFiles = Directory.GetFiles(dataDirectory);
            List<Source> sources = new List<Source>();

            // Load all sources
            foreach (string dataFile in dataFiles)
            {
                string json = File.ReadAllText(dataFile);
                JObject sourceObject = JObject.Parse(json);

                Source source = Serializer.Deserialize(sourceObject);
                sources.Add(source);
            }

            // Generate a method
            const string methodName = "RegisterClassEx";

            Library library = sources
                .SelectMany(s => s.Libraries)
                .FirstOrDefault(l => l.Methods.Any(m => m.Name == methodName));

            Method method = library.Methods
                .FirstOrDefault(m => m.Name == methodName);

            CSharpMethodGenerator generator = new CSharpMethodGenerator();
            string result = generator.Generate(library, method);

            Console.WriteLine(method);
            Console.WriteLine();
            Console.WriteLine(result);

            Console.ReadLine();
        }
    }
}
