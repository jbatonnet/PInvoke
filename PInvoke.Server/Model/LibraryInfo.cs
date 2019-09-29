using System.Collections.Generic;

namespace PInvoke.Server.Model
{
    public class LibraryInfo
    {
        public string Name { get; set; }
        public IEnumerable<MethodInfo> Methods { get; set; }

        public override string ToString() => Name;
    }
}
