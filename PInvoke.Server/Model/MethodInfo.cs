using System.Collections.Generic;

using PInvoke.Common.Models;

namespace PInvoke.Server.Model
{
    public class MethodInfo : ObjectInfo
    {
        public ParsedType ReturnType { get; set; }
        public string Name { get; set; }
        public IEnumerable<Parameter> Parameters { get; set; }

        public IEnumerable<Method> Variants { get; set; }

        public MethodInfo(FastJsonStreamReader jsonReader, long jsonPosition) : base(jsonReader, jsonPosition)
        {
        }

        public override string ToString() => $"{ReturnType} {Name}({string.Join(", ", Parameters)})";
    }
}
