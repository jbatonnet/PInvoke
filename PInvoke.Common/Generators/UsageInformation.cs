using System.Collections.Generic;

using PInvoke.Common.Models;

namespace PInvoke.Common.Generators
{
    public class UsageInformation
    {
        public IEnumerable<Enumeration> UsedEnumerations { get; set; }
        public IEnumerable<Method> UsedMethods { get; set; }
        public IEnumerable<Constant> UsedConstants { get; set; }
        public IEnumerable<Structure> UsedStructures { get; set; }
    }
}