using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PInvoke.Common.Models
{
    public class Source
    {
        public string Name { get; set; }

        public IEnumerable<Library> Libraries { get; set; }
    }
}