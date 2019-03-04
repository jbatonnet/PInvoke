using System.Collections.Generic;

namespace PInvoke.Common.Models
{
    public class EnumerationValue
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Value))
                return $"{Name} = {Value}";
            else
                return Name;
        }
    }

    public class Enumeration
    {
        public string Name { get; set; }

        public IEnumerable<EnumerationValue> Values { get; set; }
    }
}