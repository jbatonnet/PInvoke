using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace PInvoke.Common.Models
{
    public abstract class Type
    {
    }

    public class PointerType : Type
    {
        public Type Target { get; set; }

        public override string ToString() => $"Pointer<{Target}>";
    }
    public class HandleType : Type
    {
        public Type Target { get; set; }

        public override string ToString() => $"Handle<{Target}>";
    }
    public class ConstType : Type
    {
        public Type Target { get; set; }

        public override string ToString() => $"Constant<{Target}>";
    }
    public class InType : Type
    {
        public Type Target { get; set; }

        public override string ToString() => $"In<{Target}>";
    }
    public class OutType : Type
    {
        public Type Target { get; set; }

        public override string ToString() => $"Out<{Target}>";
    }
    public class InOutType : Type
    {
        public Type Target { get; set; }

        public override string ToString() => $"InOut<{Target}>";
    }
    public class StringType : Type
    {
        public bool WideCharacters { get; set; }

        public override string ToString() => "String";
    }

    public enum BasicTypeValues
    {
        Void,
        Boolean,
        Byte,
        SByte,
        UShort,
        Short,
        UInteger,
        Integer,
        ULong,
        Long,
        Float,
        Double
    }
    public class BasicType : Type
    {
        public BasicTypeValues Type { get; set; }

        public override string ToString() => Type.ToString();
    }

    public class UnknownType : Type
    {
        public string Name { get; set; }

        public override string ToString() => Name;
    }

    public class ParsedType : Type
    {
        private static readonly Regex apiRegex = new Regex(@"^[a-z]api\s", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly ConcurrentDictionary<string, Type> parsedTypes = new ConcurrentDictionary<string, Type>();

        public string Raw
        {
            get => raw;
            set
            {
                raw = value;
                parsed = Parse(value);
            }
        }
        /*[JsonIgnore]*/ public Type Parsed => parsed;

        private string raw;
        private Type parsed;

        public override string ToString() => Raw;

        private static Type Parse(string name)
        {
            name = apiRegex.Replace(name.Trim(), "").Trim();
            string lowerName = name.ToLower();

            // Constants
            if (lowerName.StartsWith("const"))
                return new ConstType() { Target = Parse(name.Substring(5)) };

            // Pointers
            if (lowerName.EndsWith("*"))
                return new PointerType() { Target = Parse(name.Remove(name.Length - 1)) };
            if (lowerName.EndsWith("_ptr"))
                return new PointerType() { Target = Parse(name.Remove(name.Length - 4)) };
            if (lowerName.StartsWith("*"))
                return new PointerType() { Target = Parse(name.Substring(1)) };
            if (lowerName.StartsWith("lp"))
                return new PointerType() { Target = Parse(name.Substring(2)) };
            if (lowerName.StartsWith("p"))
                return new PointerType() { Target = Parse(name.Substring(1)) };

            // Qualifiers
            if (lowerName.StartsWith("in "))
                return new InType() { Target = Parse(name.Substring(3)) };
            if (lowerName.StartsWith("_in_ "))
                return new InType() { Target = Parse(name.Substring(3)) };
            if (lowerName.StartsWith("out "))
                return new OutType() { Target = Parse(name.Substring(4)) };
            if (lowerName.StartsWith("_out_ "))
                return new OutType() { Target = Parse(name.Substring(4)) };
            if (lowerName.StartsWith("inout "))
                return new InOutType() { Target = Parse(name.Substring(6)) };
            if (lowerName.StartsWith("_inout_ "))
                return new InOutType() { Target = Parse(name.Substring(6)) };

            // Handles
            if (lowerName.StartsWith("h"))
                return new HandleType() { Target = new UnknownType() { Name = name.Substring(1) } };

            // Strings
            if (lowerName == "cwstr")
                return new StringType() { WideCharacters = true };
            if (lowerName == "cstr")
                return new StringType() { WideCharacters = false };
            if (lowerName == "str")
                return new StringType() { WideCharacters = false };

            // Basic types
            switch (lowerName)
            {
                case "void":
                    return new BasicType() { Type = BasicTypeValues.Void };
                case "bool":
                case "bit":
                case "boolean":
                    return new BasicType() { Type = BasicTypeValues.Boolean };
                case "byte":
                case "char":
                    return new BasicType() { Type = BasicTypeValues.Byte };
                case "sbyte":
                    return new BasicType() { Type = BasicTypeValues.SByte };
                case "ushort":
                    return new BasicType() { Type = BasicTypeValues.UShort };
                case "atom":
                case "short":
                case "word":
                    return new BasicType() { Type = BasicTypeValues.Short };
                case "uint":
                case "uint32":
                    return new BasicType() { Type = BasicTypeValues.UInteger };
                case "int":
                case "dword":
                    return new BasicType() { Type = BasicTypeValues.Integer };
                case "ulong":
                    return new BasicType() { Type = BasicTypeValues.ULong };
                case "long":
                case "qword":
                    return new BasicType() { Type = BasicTypeValues.Long };
                case "single":
                case "float":
                    return new BasicType() { Type = BasicTypeValues.Float };
                case "double":
                    return new BasicType() { Type = BasicTypeValues.Double };
            }

            return new UnknownType() { Name = name };
        }
    }
}