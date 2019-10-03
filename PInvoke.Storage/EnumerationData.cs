using PInvoke.Common.Models;

namespace PInvoke.Storage
{
    public class EnumerationData
    {
        public string Source { get; set; }
        public string Library { get; set; }
        public string Name { get; set; }
        public Enumeration Content { get; set; }
    }
}
