using System;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PInvoke.Common.Generators;
using PInvoke.Common.Generators.CSharp;
using PInvoke.Common.Models;
using PInvoke.Server.Services;

namespace PInvoke.Server.Controllers
{
    [Route("api/[action]")]
    public class ApiController : Controller
    {
        [Flags]
        public enum SearchType
        {
            Method = 1,
            Enumeration = 2,
            Structure = 4,

            All = Method | Enumeration | Structure
        }

        public enum GenerationLanguage
        {
            CSharp
        }

        private readonly DataService dataService;

        public ApiController(DataService dataService)
        {
            this.dataService = dataService;
        }

        /*[HttpGet]
        public IActionResult Search(string name, string source = null, SearchType type = SearchType.All)
        {
            name = name.Trim();

            Library[] libraries = dataService.Sources
                .SelectMany(s => s.Libraries)
                .ToArray();

            JObject result = new JObject();

            if (type.HasFlag(SearchType.Method))
            {
                Method[] methods = libraries
                    .SelectMany(l => l.Methods)
                    .Where(m => string.Equals(m.Name, name, StringComparison.InvariantCultureIgnoreCase) || m.Variants?.Any(v => string.Equals(v.Name, name, StringComparison.InvariantCultureIgnoreCase)) == true)
                    .ToArray();

                result["Methods"] = new JArray(methods.Select(Serializer.Serialize));
            }

            if (type.HasFlag(SearchType.Enumeration))
            {
                Enumeration[] enumerations = libraries
                    .SelectMany(l => l.Enumerations)
                    .Where(e => string.Equals(e.Name, name, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();

                result["Enumerations"] = new JArray(enumerations.Select(Serializer.Serialize));
            }

            if (type.HasFlag(SearchType.Enumeration))
            {
                Structure[] structures = libraries
                    .SelectMany(l => l.Structures)
                    .Where(s => string.Equals(s.Name, name, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();

                result["Structures"] = new JArray(structures.Select(Serializer.Serialize));
            }

            return new ObjectResult(result);
        }

        [HttpGet, HttpPost]
        public IActionResult Generate(string name, string source = null, string library = null, GenerationLanguage language = GenerationLanguage.CSharp)
        {
            name = name.Trim();

            Source[] sources = dataService.Sources
                .ToArray();

            if (!string.IsNullOrEmpty(source))
            {
                sources = sources
                    .Where(s => string.Equals(s.Name, source, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
            }

            Library[] libraries = dataService.Sources
                .SelectMany(s => s.Libraries)
                .ToArray();

            if (!string.IsNullOrEmpty(library))
            {
                libraries = libraries
                    .Where(l => string.Equals(l.Name, library, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
            }

            Method method = Enumerable
                .Concat
                (
                    libraries.SelectMany(l => l.Methods),
                    libraries.SelectMany(l => l.Methods).SelectMany(m => m.Variants)
                )
                .FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.InvariantCultureIgnoreCase));

            string generationResult = null;

            if (method != null)
            {
                Library _library = libraries.First(l => l.Methods.Contains(method));
                Source _source = dataService.Sources.First(s => s.Libraries.Contains(_library));

                generationResult = GenerateMethod(_source, _library, method, language);
            }

            return new ObjectResult(JObject.FromObject(generationResult));
        }

        private string GenerateMethod(Source source, Library library, Method method, GenerationLanguage language = GenerationLanguage.CSharp)
        {
            JObject generationParameters = new JObject();

            try
            {
                using (StreamReader streamReader = new StreamReader(Request.Body))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    generationParameters = JObject.Load(jsonReader);
                }
            }
            catch { }

            UsageInformation usageInformation = Generator.AnalyzeUsage(source, library, method);
            string generationResult = null;

            if (language == GenerationLanguage.CSharp)
            {
                CSharpMethodGenerator methodGenerator = new CSharpMethodGenerator(generationParameters);
                CSharpStructureGenerator structureGenerator = new CSharpStructureGenerator(generationParameters);
                CSharpEnumerationGenerator enumerationGenerator = new CSharpEnumerationGenerator(generationParameters);

                generationResult = methodGenerator.Generate(library, method);
            }
            
            return generationResult;
        }
        */

        [HttpPost]
        public IActionResult Generate()
        {
            return Ok();
        }
    }
}
