using System.Linq;
using System.Text;

namespace PInvoke.Common.Generators.CSharp
{
    using Models;

    public class CSharpStructureGenerator : CSharpGenerator<Structure>
    {
        public CSharpStructureGenerator()
        {
        }
        public CSharpStructureGenerator(GenerationParameters generationParameters) : base(generationParameters)
        {
        }

        public override string Generate(Library library, Structure structure)
        {
            StringBuilder result = new StringBuilder();

            if (UseFullTypes)
                result.AppendLine("[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]");
            else
                result.AppendLine("[StructLayout(LayoutKind.Sequential)]");

            if (Modifier != CSharpModifier.None)
                result.Append($"{Modifier.ToString().ToLower()} ");

            result.AppendLine($"struct {structure.Name}");
            result.AppendLine("{");

            Field[] fields = structure.Fields.ToArray();
            foreach (Field field in fields)
            {
                string fieldType = GetType(field.Type);
                string fieldName = field.Name;

                result.AppendLine($"{GetSpacing()}{fieldType} {fieldName};");
            }

            result.AppendLine("}");

            return result.ToString().TrimEnd();
        }
    }
}