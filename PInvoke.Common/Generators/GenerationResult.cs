using System.Collections.Generic;

using PInvoke.Common.Models;

namespace PInvoke.Common.Generators
{
    public class GenerationResult
    {
        public string Generated { get; set; }
        public UsageInformation UsageInformation { get; set; }
    }

    public class GenerationResult<T> : GenerationResult
    {
        public T Original { get; set; }
    }
}