using System.Collections.Generic;

using PInvoke.Common.Models;

namespace PInvoke.Server.Model
{
    public class MethodInfo
    {
        public string Name { get; set; }
        public Method Method { get; set; }

        public override string ToString() => Method?.ToString() ?? Name;
    }
}
