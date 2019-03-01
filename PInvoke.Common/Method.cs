using System;
using System.Collections.Generic;
using System.Linq;

namespace PInvoke.Common
{
    public class Method
    {
        public Type ReturnType { get; set; }
        public string Name { get; set; }
        public IEnumerable<Parameter> Parameters { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }

        public override string ToString() => $"{ReturnType} {Name}({string.Join(", ", Parameters)})";
    }
}