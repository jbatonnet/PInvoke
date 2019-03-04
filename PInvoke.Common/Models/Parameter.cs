using System;

namespace PInvoke.Common.Models
{
    public class Parameter
    {
        public ParsedType ParameterType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public override string ToString() => $"{ParameterType} {Name}";
    }
}