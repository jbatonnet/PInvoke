using PInvoke.Common.Models;

namespace PInvoke.Storage
{
    public class MethodData
    {
        public string Source { get; set; }
        public string Library { get; set; }
        public string Name { get; set; }
        public Method Content { get; set; }
    }
}
