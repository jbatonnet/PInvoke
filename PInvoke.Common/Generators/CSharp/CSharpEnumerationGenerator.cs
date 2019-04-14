using System.Text;

using Newtonsoft.Json.Linq;

namespace PInvoke.Common.Generators.CSharp
{
    using Models;

    public class CSharpEnumerationGenerator : CSharpGenerator<Enumeration>
    {
        public bool NewLineBeforeBracket { get; set; } = true;

        public CSharpEnumerationGenerator()
        {
        }
        public CSharpEnumerationGenerator(JObject generationParameters) : base(generationParameters)
        {
            NewLineBeforeBracket = generationParameters["NewLineBeforeBracket"]?.Value<bool>() ?? NewLineBeforeBracket;
        }

        public override string Generate(Library library, Enumeration enumeration)
        {
            StringBuilder result = new StringBuilder();

            if (Modifier != CSharpModifier.None)
                result.Append($"{Modifier.ToString().ToLower()} ");

            result.Append($"enum {enumeration.Name}");

            if (NewLineBeforeBracket)
                result.AppendLine();

            result.AppendLine("{");

            foreach (var value in enumeration.Values)
            {
                result.Append($"{GetSpacing()}{value.Name}");
                result.AppendLine(",");
            }

            result.AppendLine("}");

            return null;
        }
    }
}