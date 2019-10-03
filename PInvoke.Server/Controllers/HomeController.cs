using System;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

using PInvoke.Common.Generators;
using PInvoke.Common.Generators.CSharp;
using PInvoke.Common.Models;
using PInvoke.Server.Models;
using PInvoke.Server.Services;

namespace PInvoke.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataService dataService;

        public HomeController(DataService dataService)
        {
            this.dataService = dataService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("browse")]
        public IActionResult Browse(string source = null, string library = null)
        {
            ViewData["Sources"] = dataService.GetSources().ToArray();

            SourceInfo selectedSource = null;
            if (source != null)
            {
                selectedSource = dataService.GetSource(source);
                ViewData["Source"] = selectedSource;
            }

            Library selectedLibrary;
            if (selectedSource != null && library != null)
            {
                selectedLibrary = dataService.GetLibrary(source, library);
                ViewData["Library"] = selectedLibrary;
            }

            return View();
        }

        [HttpGet("search")]
        public IActionResult Search(string name = null, string source = null)
        {
            /*ViewData["Name"] = name;
            ViewData["Sources"] = dataService.Sources.ToArray();

            if (name != null)
            {
                name = name.Trim();

                Library[] libraries = dataService.Sources
                    .SelectMany(s => s.Libraries)
                    .ToArray();

                {
                    Method[] methods = libraries
                        .SelectMany(l => l.Methods)
                        .Where(m => string.Equals(m.Name, name, StringComparison.InvariantCultureIgnoreCase) || m.Variants?.Any(v => string.Equals(v.Name, name, StringComparison.InvariantCultureIgnoreCase)) == true)
                        .ToArray();

                    ViewData["Methods"] = methods;
                }

                {
                    Enumeration[] enumerations = libraries
                        .SelectMany(l => l.Enumerations)
                        .Where(e => string.Equals(e.Name, name, StringComparison.InvariantCultureIgnoreCase))
                        .ToArray();

                    ViewData["Enumerations"] = enumerations;
                }

                {
                    Structure[] structures = libraries
                        .SelectMany(l => l.Structures)
                        .Where(s => string.Equals(s.Name, name, StringComparison.InvariantCultureIgnoreCase))
                        .ToArray();

                    ViewData["Structures"] = structures;
                }
            }*/

            return View();
        }

        [HttpGet("generate")]
        public IActionResult Generate(string source = null, string library = null, string element = null)
        {
            SourceInfo selectedSource = dataService.GetSource(source);

            if (selectedSource == null)
                return Redirect("/");

            Library selectedLibrary = dataService.GetLibrary(source, library);

            if (selectedLibrary == null)
                return Redirect("/");

            Method selectedMethod = selectedLibrary.Methods.FirstOrDefault(m => m.Name.Equals(element, StringComparison.InvariantCultureIgnoreCase));

            if (selectedMethod == null)
                return Redirect("/");

            ViewData["Source"] = selectedSource;
            ViewData["Library"] = selectedLibrary;
            ViewData["Method"] = selectedMethod;

            UsageInformation usageInformation = Generator.AnalyzeUsage(null, selectedLibrary, selectedMethod);

            CSharpMethodGenerator methodGenerator = new CSharpMethodGenerator();
            string generationResult = methodGenerator.Generate(selectedLibrary, selectedMethod);

            ViewData["Result"] = generationResult;

            return View();
        }
    }
}
