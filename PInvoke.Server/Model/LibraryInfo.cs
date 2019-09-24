using System.Collections.Generic;

namespace PInvoke.Server.Model
{
    public class LibraryInfo : ObjectInfo
    {
        public string Name { get; set; }

        public IEnumerable<MethodInfo> Methods
        {
            get
            {
                jsonReader.Seek(jsonPosition);
                SkipToProperty("Methods");

                while (jsonReader.TokenType == JsonTokenType.StartObject)
                {
                    yield return new MethodInfo(jsonReader, jsonReader.Position);
                    jsonReader.Skip();
                }
            }
        }

        public LibraryInfo(FastJsonStreamReader jsonReader, long jsonPosition) : base(jsonReader, jsonPosition)
        {
        }

        public override string ToString() => Name;
    }
}
