using System.Collections.Generic;

namespace PInvoke.Common.Models
{
    public class Field
    {
        public ParsedType Type { get; set; }
        public string Name { get; set; }

        public override string ToString() => $"{Type} {Name}";
    }

    public class Structure
    {
        public string Name { get; set; }

        public IEnumerable<Field> Fields { get; set; }
    }
}