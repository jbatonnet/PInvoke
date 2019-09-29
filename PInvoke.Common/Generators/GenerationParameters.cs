using System.Collections.Generic;

using PInvoke.Common.Models;

namespace PInvoke.Common.Generators
{
    public class GenerationParameters : Dictionary<string, object>
    {
        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (!TryGetValue(key, out object result))
                return defaultValue;

            return (T)result;
        }
    }
}