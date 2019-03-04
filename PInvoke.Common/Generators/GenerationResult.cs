using System.Collections.Generic;

using PInvoke.Common.Models;

namespace PInvoke.Common.Generators
{
    public abstract class GenerationResult<T>
    {
        public T Original { get; set; }
        public string Generated { get; set; }

        public IEnumerable<Enumeration> UsedEnumerations { get; set; }
        public IEnumerable<Method> UsedMethods { get; set; }
        public IEnumerable<Constant> UsedConstants { get; set; }
        public IEnumerable<Structure> UsedStructures { get; set; }
    }
}