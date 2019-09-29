using System.Collections.Generic;
using System.IO;
using System.Linq;

using PInvoke.Server.Model;
using PInvoke.Storage;

namespace PInvoke.Server.Services
{
    public class DataService
    {
        private SqliteStorage sqliteStorage;

        public DataService()
        {
            string dataDirectory = @"D:\Projets\C#\PInvoke\Data";
            string databasePath = Path.Combine(dataDirectory, "Output.db");

            sqliteStorage = new SqliteStorage(databasePath);
        }

        public SourceInfo GetSource(string source)
        {
            return new SourceInfo()
            {
                Name = source,
                Libraries = sqliteStorage.GetLibraries(source).Select(l => l.Name).ToArray()
            };
        }
        public IEnumerable<SourceInfo> GetSources()
        {
            LibraryData[] libraries = sqliteStorage.GetLibraries().ToArray();

            return libraries
                .GroupBy(l => l.Source)
                .Select(g => new SourceInfo()
                {
                    Name = g.Key,
                    Libraries = g.Select(l => l.Name).ToArray()
                });
        }

        public LibraryInfo GetLibrary(string source, string library)
        {
            return new LibraryInfo()
            {
                Name = library,
                Methods = sqliteStorage.GetMethods(source, library).Select(m => new MethodInfo() { Name = m.Name }).ToArray()
            };
        }
        public IEnumerable<LibraryInfo> GetLibraries(string source)
        {
            MethodData[] methods = sqliteStorage.GetMethods(source).ToArray();

            return methods
                .GroupBy(m => m.Library)
                .Select(g => new LibraryInfo()
                {
                    Name = g.Key,
                    Methods = g.Select(m => new MethodInfo() { Name = m.Name, Method = m.Content }).ToArray()
                });
        }

        public MethodInfo GetMethod(string source, string library, string method)
        {
            return new MethodInfo()
            {
                Name = method,
                Method = sqliteStorage.GetMethod(source, library, method).Content
            };
        }

    }
}
