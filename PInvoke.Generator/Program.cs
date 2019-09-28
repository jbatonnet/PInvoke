using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;

using PInvoke.Common.Generators;
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

                Source sourceValue = Serializer.DeserializeSource(sourceObject);
                sources.Add(sourceValue);
            }

            // Generate a method
            string methodName = "RegisterClassEx";

            Method method = sources
                .SelectMany(s => s.Libraries)
                .SelectMany(l => l.Methods)
                .FirstOrDefault(m => m.Name == methodName);

            Library library = sources
                .SelectMany(s => s.Libraries)
                .First(l => l.Methods.Contains(method));

            Source source = sources
                .First(s => s.Libraries.Contains(library));

            Console.WriteLine(method);
            Console.WriteLine();

            CSharpMethodGenerator methodGenerator = new CSharpMethodGenerator();
           /* GenerationResult<Method> methodGenerationResult = methodGenerator.Generate(source, library, method);

            foreach (Structure structure in methodGenerationResult.UsedStructures)
            {
                CSharpStructureGenerator structureGenerator = new CSharpStructureGenerator();
                GenerationResult<Structure> structureGenerationResult = structureGenerator.Generate(source, library, structure);

                Console.WriteLine(structureGenerationResult.Generated);
                Console.WriteLine();
            }

            Console.WriteLine(methodGenerationResult.Generated);*/

            Console.WriteLine();


            Console.ReadLine();
        }
    }
}
