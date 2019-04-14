using System.Collections.Generic;
using System.Linq;

namespace PInvoke.Common.Models
{
    public class Source
    {
        public string Name { get; set; }

        public virtual IEnumerable<Library> Libraries { get; set; }
    }

    public class OverridenSource : Source
    {
        public OverridenSource(Source underlyingSource, IEnumerable<Library> overridenLibraries)
        {
            Libraries = overridenLibraries.Concat(underlyingSource.Libraries);
        }
    }
}