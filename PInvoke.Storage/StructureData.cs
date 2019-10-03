using PInvoke.Common.Models;

namespace PInvoke.Storage
{
    public class StructureData
    {
        public string Source { get; set; }
        public string Library { get; set; }
        public string Name { get; set; }
        public Structure Content { get; set; }
    }
}
