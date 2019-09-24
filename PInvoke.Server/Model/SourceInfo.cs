using System.Collections.Generic;

namespace PInvoke.Server.Model
{
    public class SourceInfo : ObjectInfo
    {
        public string Name { get; }

        public IEnumerable<LibraryInfo> Libraries
        {
            get
            {
                jsonReader.Seek(jsonPosition);
                SkipToProperty("Libraries");

                while (jsonReader.TokenType == JsonTokenType.StartObject)
                {
                    yield return new LibraryInfo(jsonReader, jsonReader.Position);
                    jsonReader.Skip();
                }
            }
        }

        public SourceInfo(FastJsonStreamReader jsonReader, long jsonPosition) : base(jsonReader, jsonPosition)
        {
            SkipToProperty("Name");
            Name = jsonReader.ReadString();
        }

        public override string ToString() => Name;
    }
}
