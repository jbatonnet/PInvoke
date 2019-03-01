using System.Collections.Concurrent;

namespace PInvoke.Common
{
    public class Library
    {
        public string Name { get; set; }

        public ConcurrentBag<Method> Methods { get; set; } = new ConcurrentBag<Method>();
        public ConcurrentBag<Enum> Enums { get; set; } = new ConcurrentBag<Enum>();
    }
}