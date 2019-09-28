using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Data.Sqlite;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PInvoke.Common.Models;
using PInvoke.Common.Serialization;

namespace PInvoke.Server.Services
{
    public class DataService
    {
        public IEnumerable<Source> Sources { get; private set; }

        /*private LiteCollection<BsonDocument> methodsCollection;
        private LiteCollection<BsonDocument> enumerationsCollection;
        private LiteCollection<BsonDocument> structuresCollection;*/

        public DataService()
        {
            string dataDirectory = @"D:\Projets\C#\PInvoke\Data";
            string databasePath = Path.Combine(dataDirectory, "Output.db");

            // Setup database
            SqliteConnection sqliteConnection = new SqliteConnection($"Data Source={databasePath};Version=3;");
            sqliteConnection.Open();

            



            // Setup the database
            /*string databasePath = Path.Combine(dataDirectory, "Output.db");
            LiteDatabase liteDatabase = new LiteDatabase(databasePath);

            methodsCollection = liteDatabase.GetCollection("methods");
            methodsCollection.EnsureIndex("Source");
            methodsCollection.EnsureIndex("Library");
            methodsCollection.EnsureIndex("Name");

            enumerationsCollection = liteDatabase.GetCollection("enumerations");
            enumerationsCollection.EnsureIndex("Source");
            enumerationsCollection.EnsureIndex("Library");
            enumerationsCollection.EnsureIndex("Name");

            structuresCollection = liteDatabase.GetCollection("structures");
            structuresCollection.EnsureIndex("Source");
            structuresCollection.EnsureIndex("Library");
            structuresCollection.EnsureIndex("Name");*/



            /*methodsCollection.Query()
                .GroupBy(BsonExpression.Create("Source"))
                .Select(BsonExpression.Create("Source"))
                .ToEnumerable();*/




            string[] dataFiles = Directory.GetFiles(dataDirectory);
            List<Source> sources = new List<Source>();

            // Load all sources
            foreach (string dataFile in dataFiles)
            {
                Stream stream = new FileStream(dataFile, FileMode.Open, FileAccess.Read, FileShare.Read);

                TextReader textReader = new StreamReader(stream, Encoding.UTF8, true, 4096, true);
                JsonReader jsonReader = new JsonTextReader(textReader);

                //SourceInfo sourceInfo = new SourceInfo(stream, jsonReader);


                string json = File.ReadAllText(dataFile);
                JObject sourceObject = JObject.Parse(json);

                Source sourceValue = Serializer.DeserializeSource(sourceObject);
                sources.Add(sourceValue);
            }

            Sources = sources.Where(s => s.Libraries.Any());

        }

        public IEnumerable<Source> GetSources()
        {
            return null;// methodsCollection.
        }
    }
}
