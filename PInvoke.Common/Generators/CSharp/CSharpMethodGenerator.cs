using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using PInvoke.Common.Models;

namespace PInvoke.Common.Generators.CSharp
{
    public class CSharpMethodGenerator : CSharpGenerator<Method>
    {
        public bool UseLowerCaseLibrary { get; set; } = true;
        public bool UseLibraryExtension { get; set; } = true;
        public bool SetLastError { get; set; } = true;

        public CSharpMethodGenerator()
        {
        }
        public CSharpMethodGenerator(GenerationParameters generationParameters) : base(generationParameters)
        {
            UseLowerCaseLibrary = generationParameters.GetValue<bool>("UseLowerCaseLibrary", UseLowerCaseLibrary);
            UseLibraryExtension = generationParameters.GetValue<bool>("UseLibraryExtension", UseLibraryExtension);
            SetLastError = generationParameters.GetValue<bool>("SetLastError", SetLastError);
        }

        public override string Generate(Library library, Method method)
        {
            StringBuilder result = new StringBuilder();

            string libraryName = library.Name;
            if (UseLowerCaseLibrary)
                libraryName = libraryName.ToLower();
            if (!UseLibraryExtension)
                libraryName = Path.GetFileNameWithoutExtension(libraryName);

            result.Append($"[{(UseFullTypes ? "System.Runtime.InteropServices." : "")}DllImport(\"{libraryName}\"");

            if (SetLastError)
                result.Append(", SetLastError = true");

            result.AppendLine(")]");

            string methodName = method.Name;

            if (Modifier != CSharpModifier.None)
                result.Append($"{Modifier.ToString().ToLower()} ");

            result.Append($"static extern {GetType(method.ReturnType)} {method.Name}(");

            Parameter[] parameters = method.Parameters.ToArray();
            List<string> parameterNames = new List<string>();

            for (int i = 0; i < parameters.Length; i++)
            {
                Parameter parameter = parameters[i];

                string parameterType = GetType(parameter.ParameterType);
                string parameterName = parameter.Name;

                if (string.IsNullOrWhiteSpace(parameterName))
                {
                    parameterName = new string(GetFinalType(parameter.ParameterType).Where(c => char.IsLetter(c)).ToArray());

                    if (parameterName.All(c => char.IsUpper(c)))
                        parameterName = parameterName.ToLower();
                    else
                        parameterName = parameterName.Remove(1).ToLower() + parameterName.Substring(1);
                }

                if (parameterNames.Contains(parameterName))
                {
                    int counter = 2;

                    while (parameterNames.Contains(parameterName + counter))
                        counter++;

                    parameterName = parameterName + counter;
                }

                parameterNames.Add(parameterName);

                result.Append($"{parameterType} {parameterName}");

                if (i < parameters.Length - 1)
                    result.Append(", ");
            }

            result.AppendLine(");");

            return result.ToString().TrimEnd();
        }
    }
}