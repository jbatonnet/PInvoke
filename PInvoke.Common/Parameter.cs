using System;

namespace PInvoke.Common
{
    public class Parameter
    {
        public Type ParameterType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public override string ToString() => $"{ParameterType} {Name}";
    }
}