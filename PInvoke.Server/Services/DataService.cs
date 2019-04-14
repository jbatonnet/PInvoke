using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;

using PInvoke.Common.Models;
using PInvoke.Common.Serialization;

namespace PInvoke.Server.Services
{
    public class DataService
    {
        public IEnumerable<Source> Sources { get; private set; }

        public DataService()
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

            Sources = sources.Where(s => s.Libraries.Any());
        }
    }
}
