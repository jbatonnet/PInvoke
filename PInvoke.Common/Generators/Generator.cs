using PInvoke.Common.Models;

namespace PInvoke.Common.Generators
{
    public abstract class Generator<T>
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