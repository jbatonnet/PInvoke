using System.Collections.Generic;

namespace PInvoke.Common.Models
{
    public class Library
    {
        public string Name { get; set; }

        public IEnumerable<Method> Methods { get; set; }
        public IEnumerable<Enumeration> Enumerations { get; set; }

        public override string ToString() => Name;
    }
}