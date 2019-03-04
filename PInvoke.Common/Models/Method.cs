using System.Collections.Generic;

namespace PInvoke.Common.Models
{
    public class Method
    {
        public ParsedType ReturnType { get; set; }
        public string Name { get; set; }
        public IEnumerable<Parameter> Parameters { get; set; }

        public IEnumerable<Method> Variants { get; set; }

        public override string ToString() => $"{ReturnType} {Name}({string.Join(", ", Parameters)})";
    }
}