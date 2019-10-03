using System;
using System.Collections.Generic;
using System.Linq;

using PInvoke.Common.Models;

namespace PInvoke.Common.Generators
{
    public abstract class Generator
    {
        public static UsageInformation AnalyzeUsage(Source source, Library library, Method method)
        {
            List<Enumeration> usedEnumerations = new List<Enumeration>();
            List<Method> usedMethods = new List<Method>();
            List<Constant> usedConstants = new List<Constant>();
            List<Structure> usedStructures = new List<Structure>();

            AnalyseUsage(source, library, method.ReturnType, usedEnumerations, usedMethods, usedConstants, usedStructures);

            foreach (Parameter parameter in method.Parameters)
                AnalyseUsage(source, library, parameter.ParameterType, usedEnumerations, usedMethods, usedConstants, usedStructures);

            return new UsageInformation()
            {
                UsedEnumerations = usedEnumerations,
                UsedMethods = usedMethods,
                UsedConstants = usedConstants,
                UsedStructures = usedStructures
            };
        }
        public static UsageInformation AnalyzeUsage(Source source, Library library, Structure structure)
        {
            List<Enumeration> usedEnumerations = new List<Enumeration>();
            List<Method> usedMethods = new List<Method>();
            List<Constant> usedConstants = new List<Constant>();
            List<Structure> usedStructures = new List<Structure>();

            foreach (Field field in structure.Fields)
                AnalyseUsage(source, library, field.Type, usedEnumerations, usedMethods, usedConstants, usedStructures);

            return new UsageInformation()
            {
                UsedEnumerations = usedEnumerations,
                UsedMethods = usedMethods,
                UsedConstants = usedConstants,
                UsedStructures = usedStructures
            };
        }

        private static void AnalyseUsage(Source source, Library library, ParsedType parsedType, List<Enumeration> usedEnumerations, List<Method> usedMethods, List<Constant> usedConstants, List<Structure> usedStructures)
        {
            UnknownType getBaseType(Models.Type type)
            {
                switch (type)
                {
                    case PointerType pointerType: return getBaseType(pointerType.Target);
                    case HandleType handleType: return getBaseType(handleType.Target);
                    case ConstType constType: return getBaseType(constType.Target);
                    case InType inType: return getBaseType(inType.Target);
                    case OutType outType: return getBaseType(outType.Target);
                    case InOutType inOutType: return getBaseType(inOutType.Target);
                    case BasicType basicType: return null;
                    case UnknownType unknownType: return unknownType;
                }

                return null;
            }

            UnknownType baseType = getBaseType(parsedType.Parsed);
            if (baseType == null)
                return;

            Structure structure = (source?.Libraries ?? new[] { library })
                .SelectMany(l => l.Structures)
                .FirstOrDefault(s => string.Equals(s.Name, baseType.Name, StringComparison.InvariantCultureIgnoreCase));

            if (structure != null)
            {
                usedStructures.Add(structure);

                foreach (Field field in structure.Fields)
                    AnalyseUsage(source, library, field.Type, usedEnumerations, usedMethods, usedConstants, usedStructures);

                return;
            }
        }
    }

    public abstract class Generator<T> : Generator
    {
        public bool UseSpaces { get; set; } = true;
        public int SpaceCount { get; set; } = 4;

        public abstract string Generate(Library library, T element);

        protected string GetSpacing(int level = 1)
        {
            if (UseSpaces)
                return new string(' ', level * SpaceCount);
            else
                return new string('\t', level);
        }
    }
}