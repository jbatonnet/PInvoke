using Newtonsoft.Json.Linq;

using PInvoke.Common.Models;

namespace PInvoke.Common.Serialization
{
    public static class Serializer
    {
        public static JObject Serialize(Source source)
        {
            /*JObject result = new JObject()
            {
                { "Name", source.Name },
                { "Libraries", new JArray(source.Libraries
                    .AsParallel()
                    .Select(l => new JObject()
                    {
                        { "Name", l.Name },
                        { "Enumerations", new JArray(l.Enumerations.Select(e => new JObject() {
                            { "Name", e.Name },
                            { "Values", new JArray(e.Values.Keys) }
                        }))},
                        { "Methods", new JArray(l.Methods.Select(m => new JObject() {
                            { "Signature", m.ToString() },
                            { "ReturnType", m.ReturnType.Name },
                            { "Name", m.Name },
                            { "Parameters", new JArray(m.Parameters.Select(p => new JObject() {
                                { "ParameterType", p.ParameterType.Name },
                                { "Name", p.Name }
                            }))}
                        }))}
                    })
                    .OrderBy(l => l["Name"].Value<string>())
                )}
            };*/

            return JObject.FromObject(source);
        }
        public static JObject Serialize(Method method)
        {
            return JObject.FromObject(method);
        }
        public static JObject Serialize(Enumeration enumeration)
        {
            return JObject.FromObject(enumeration);
        }
        public static JObject Serialize(Structure structure)
        {
            return JObject.FromObject(structure);
        }

        public static Source DeserializeSource(JObject sourceObject)
        {
            /*Source result = new Source()
            {
                Name = sourceObject["Name"].Value<string>(),
                Libraries = sourceObject["Libraries"].Values<JObject>().Select(libraryObject => new Library()
                {
                    Name = libraryObject["Name"].Value<string>(),
                    Enumerations = libraryObject["Enumerations"].Values<JObject>().Select(enumerationObject => new Enumeration()
                    {
                        Name = enumerationObject["Name"].Value<string>(),
                    })
                })
            };*/

            return sourceObject.ToObject<Source>();
        }
        public static Method DeserializeMethod(JObject methodObject)
        {
            return methodObject.ToObject<Method>();
        }
    }
}
