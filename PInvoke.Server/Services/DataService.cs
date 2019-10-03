using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PInvoke.Common.Models;
using PInvoke.Server.Models;
using PInvoke.Storage;

namespace PInvoke.Server.Services
{
    public class DataService
    {
        private SqliteStorage storage;

        private SourceInfo[] sources;

        public DataService()
        {
            storage = new SqliteStorage("Output.db");

            // Preload source
            sources = storage.GetLibraries()
                .GroupBy(l => l.Source)
                .Select(g => new SourceInfo()
                {
                    Name = g.Key,
                    Libraries = g.Select(l => l.Name).ToArray()
                })
                .ToArray();
        }

        public SourceInfo GetSource(string source) => sources.FirstOrDefault(s => s.Name.Equals(source, StringComparison.InvariantCultureIgnoreCase));
        public IEnumerable<SourceInfo> GetSources() => sources;

        public Library GetLibrary(string source, string library)
        {
            return new Library()
            {
                Name = library,
                Methods = storage.GetMethods(source, library).Select(m => m.Content).ToArray(),
                Enumerations = Enumerable.Empty<Enumeration>(),
                Structures = Enumerable.Empty<Structure>()
            };
        }
        public IEnumerable<Library> GetLibraries(string source)
        {
            MethodData[] methods = storage.GetMethods(source).ToArray();

            return methods
                .GroupBy(m => m.Library)
                .Select(g => new Library()
                {
                    Name = g.Key,
                    Methods = g.Select(m => m.Content).ToArray(),
                    Enumerations = Enumerable.Empty<Enumeration>(),
                    Structures = Enumerable.Empty<Structure>()
                });
        }
    }
}
