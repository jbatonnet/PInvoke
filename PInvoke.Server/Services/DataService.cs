using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PInvoke.Common.Models;
using PInvoke.Common.Serialization;
using PInvoke.Server.Model;

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
                Stream stream = new FileStream(dataFile, FileMode.Open, FileAccess.Read, FileShare.Read);

                TextReader textReader = new StreamReader(stream, Encoding.UTF8, true, 4096, true);
                JsonReader jsonReader = new JsonTextReader(textReader);

                SourceInfo sourceInfo = new SourceInfo(stream, jsonReader);


                string json = File.ReadAllText(dataFile);
                JObject sourceObject = JObject.Parse(json);

                Source sourceValue = Serializer.DeserializeSource(sourceObject);
                sources.Add(sourceValue);
            }

            Sources = sources.Where(s => s.Libraries.Any());

        }
    }
}
